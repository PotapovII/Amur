namespace RiverLib
{
    //---------------------------------------------------------------------------
    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
    //                  создано  :   9.09.2022 Потапов И.И.
    //---------------------------------------------------------------------------
    using System;
    using System.Linq;
    using MemLogLib;
    using CommonLib;
    using GeometryLib;
    using CommonLib.Physics;

    /// <summary>
    ///  ОО: Определение класса RiverStreamTask - расчет полей скорости 
    ///             и напряжений в живом сечении потока
    /// </summary>
    [Serializable]
    public class TriSroSecRiverTask_1Y : ASectionalRiverTask, IRiver
    {
        #region Эксперимент Джон Питлик и К
        double[] expX0 = null;
        double[] expY0 = null;
        static double[] x0 = { 0.0, 0.1, 0.34, 0.48,  0.7 };
        static double[] y0 = { 0.09, 0.076, 0.076, 0.006, 0.006 };
        DigFunction Geometry0 = new DigFunction(x0, y0, "Начальное дно");

        double[] expX1 = null;
        double[] expY1 = null;
        static double[] x1 = { 0.0, 0.1, 0.25, 0.36, 0.4, 0.7 };
        static double[] y1 = { 0.09, 0.076, 0.076, 0.022, 0.012, 0.012 };
        DigFunction Geometry1 = new DigFunction(x1, y1, "Дно в 225 мин");

        double[] expX2 = null;
        double[] expY2 = null;
        static double[] x2 = { 0.0, 0.1, 0.14, 0.22, 0.3, 0.7 };
        static double[] y2 = { 0.09, 0.076, 0.076, 0.04, 0.022, 0.022 };
        DigFunction Geometry2 = new DigFunction(x2, y2, "Дно в 650 мин");
        #endregion

        #region Свойства
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public new string VersionData() => "GStaticSroSecRiverTask 06.09.2022"; 
        #endregion
        public TriSroSecRiverTask_1Y(RiverStreamParams p) : base(p)
        {
            name = "гидрадинамика створа (гидростатика)";
            this.cu = 3;
            Init();
            MEM.Alloc<double>(cu, ref elem_U, "elem_U");

            Geometry0.GetFunctionData(ref expX0, ref expY0, Params.CountBLKnots);
            Geometry1.GetFunctionData(ref expX1, ref expY1, Params.CountBLKnots);
            Geometry2.GetFunctionData(ref expX2, ref expY2, Params.CountBLKnots);
        }
        /// <summary>
        /// Нахождение поля скоростей
        /// </summary>
        public override void SolveU()
        {

        }

        /// <summary>
        /// расчет параметров потока, скоростей и глубины потока
        /// </summary>
        public override void SolveVelosity()
        {
            FlagStartMu = true;
        }

        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public override void SolveTaus()
        {

        }

        /// <summary>
        /// Установка параметров задачи
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="mesh"></param>
        public override void SetDataForRiverStream(double waterLevel, double[] fx, double[] fy, ref HKnot right, ref HKnot left)
        {
            this.bottom_x = fx;
            this.bottom_y = fy;
            int[][] riverGates = null;
            // генерация сетки
            mesh = sg.CreateMesh(ref GR, ref riverGates, waterLevel, bottom_x, bottom_y);
            right = sg.Right();
            left = sg.Left();
            if (mu == null)
            {
                MEM.Alloc(mesh.CountKnots, ref mu, "mu");
                MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
                MEM.Alloc(mesh.CountKnots, ref U, "U");
                MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                FlagStartMu = false;
                FlagStartRoughness = false;
            }

        }
        /// <summary>
        /// Расчет уровня свободной поверхности реки
        /// </summary>
        protected override void SolveWaterLevel()
        {
            if (Params.taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = flowRate.FunctionValue(time);
                if (time > 0)
                {
                    double Ck = 0.1;
                    double Umax = 5;
                    riverFlowRateCalk = RiverFlowRate();
                    double deltaH = (riverFlowRateCalk - riverFlowRate) / riverFlowRate;
                    double W = bottom_x[bottom_x.Length - 1] - bottom_x[0];
                    double H = riverFlowRateCalk / (W * Umax) * deltaH;
                    double dH = Ck * dtime * H;
                    if (Math.Abs(dH) > 0.005)
                        dH = Math.Sign(dH) * 0.005;
                    //   waterLevel = waterLevel - dH;
                }
                else
                {
                    riverFlowRateCalk = riverFlowRate;
                    FlagStartMesh = true;
                }
            }
            else
            {
                waterLevel = WaterLevels.FunctionValue(time);
            }
        }

        /// <summary>
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        public override double[] TausToVols(double[] xv, double[] yv)
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;

            // массив касательных напряжений
            MEM.Alloc(xv.Length - 1, ref tau);
            double dx = xv[1] - xv[0];
            Area = 0;
            for (int i = 0; i < tau.Length; i++)
            {
                double H = waterLevel - (yv[i] + yv[i + 1]) / 2;
                if (H > 0)
                {
                    tau[i] = rho_w * g * Params.J * H;
                    Area += H * dx;
                }
                else
                    tau[i] = 0;
            }
            double[] X = mesh.GetCoords(0);
            //double[] Y = mesh.GetCoords(1);
            MEM.Alloc(mesh.CountKnots, ref mu, "mu");
            MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
            MEM.Alloc(mesh.CountKnots, ref U, "U");
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            double Lambda = 0.01;
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                double x = X[i];
                TauY[i] = 0;
                TauZ[i] = 0;
                if (x < xv[0] || x > xv[xv.Length - 1])
                {
                    mu[i] = 0;
                    nu0[i] = 0;
                    U[i] = 0;
                }
                else
                {
                    int ix = (int)(x / dx);
                    ix = ix < 0 ? 0 : ix;
                    ix = ix > U.Length - 1 ? U.Length - 1 : ix;
                    int ixt = ix > tau.Length - 1 ? tau.Length - 1 : ix;
                    TauZ[i] = tau[ixt];
                    mu[i] = 0.1;
                    nu0[i] = 1e-6;
                    U[i] = Math.Sqrt(2 * tau[ixt] / ( rho_w * Lambda) );
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tau.Max();
            /// Сдвиговые напряжения средние
            tauMid = tau.Sum() / tau.Length;
            return tau;
        }

        public override double RiverFlowRate()
        {
            Area = 0;
            double su, S;
            riverFlowRateCalk = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                mesh.ElemValues(U, i, ref elem_U);
                su = (elem_U[0] + elem_U[1] + elem_U[2]) / cu;
                S = mesh.ElemSquare(i);
                // расход по живому сечению
                riverFlowRateCalk += S * su;
                Area += S;
            }
            //if (double.IsNaN(riverFlowRateCalk) == true)
            //    throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSroSecRiverTask_1Y(new RiverStreamParams());
        }
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            // поля на сетке
            sp.Add("U", U);
            sp.Add("Mu", mu);
            sp.Add("TauY", TauY);
            sp.Add("TauZ", TauZ);
            // векторное поле на сетке
            sp.Add("Tau", TauY, TauZ);

            
            sp.AddCurve("Отметки дна", bottom_x, bottom_y, TypeGraphicsCurve.FuncionCurve);
            sp.AddCurve(Geometry0.Name, expX0, expY0, TypeGraphicsCurve.FuncionCurve);
            sp.AddCurve(Geometry1.Name, expX1, expY1, TypeGraphicsCurve.FuncionCurve);
            sp.AddCurve(Geometry2.Name, expX2, expY2, TypeGraphicsCurve.FuncionCurve);

            // кривые 
            // дно - берег
            //sp.AddCurve("Русловой профиль", bottom_x, bottom_y);
            //double[] xwl = { left.x, right.x };
            //double[] ywl = { left.y, right.y };
            //// свободная поверхность
            //// sp.AddCurve("Свободная поверхность", xwl, ywl, TypeGraphicsCurve.FuncionCurve);
            //Scan();
            //if (evolution.Count > 1)
            //{
            //    double[] times = (from arg in evolution select arg.time).ToArray();
            //    double[] wls = (from arg in evolution select arg.waterLevel).ToArray();
            //    sp.AddCurve("Эв.св.поверхности", times, wls, TypeGraphicsCurve.TimeCurve);
            //    double[] mus = (from arg in evolution select arg.Mu).ToArray();
            //    sp.AddCurve("Вязкость", times, mus, TypeGraphicsCurve.TimeCurve);
            //    double[] tm = (from arg in evolution select arg.tauMax).ToArray();
            //    sp.AddCurve("Tau максимум", times, tm, TypeGraphicsCurve.TimeCurve);
            //    tm = (from arg in evolution select arg.tauMid).ToArray();
            //    sp.AddCurve("Tau средние", times, tm, TypeGraphicsCurve.TimeCurve);
            //    double[] gr = (from arg in evolution select arg.GR).ToArray();
            //    sp.AddCurve("Гидравл. радиус", times, gr, TypeGraphicsCurve.TimeCurve);
            //    double[] ar = (from arg in evolution select arg.Area).ToArray();
            //    sp.AddCurve("Площадь сечения", times, ar, TypeGraphicsCurve.TimeCurve);
            //    double[] rfr = (from arg in evolution select arg.riverFlowRate).ToArray();
            //    sp.AddCurve("Расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
            //    rfr = (from arg in evolution select arg.riverFlowRateCalk).ToArray();
            //    sp.AddCurve("Текущий расчетный расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
            //}
        }

    }        
}
