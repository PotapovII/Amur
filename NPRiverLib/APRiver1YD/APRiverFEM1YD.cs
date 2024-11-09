//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using MeshGeneratorsLib.StripGenerator;
    using NPRiverLib.APRiver1YD.Params;

    using System;
    using System.Linq;
    using CommonLib.Geometry;
    using GeometryLib;

    /// <summary>
    ///  ОО: Определение класса APRiverFEM1YD - расчет полей скорости, 
    ///  вязкости и напряжений в живом сечении потока методом КЭ 
    /// </summary>    
    [Serializable]
    public abstract class APRiverFEM1YD : APRiver1YD<RSCrossParams>, IRiver
    {
        #region Локальные переменные
        /// <summary>
        /// Задача Пуассона КЭ реализация
        /// </summary>
        [NonSerialized]
        protected IFEPoissonTask taskPoisson = null;
        /// <summary>
        /// Длина смоченного периметра
        /// </summary>
        protected double WetBed = 0;
        /// <summary>
        /// площадь сечения канала
        /// </summary>
        public double Area = 0;
        /// <summary>
        /// правая часть уравнения
        /// </summary>
        protected double Q;
        /// <summary>
        /// Сдвиговые напряжения максимум
        /// </summary>
        public double tauMax = 0;
        /// <summary>
        /// Сдвиговые напряжения средние
        /// </summary>
        public double tauMid = 0;
        /// <summary>
        /// FlagStartMu - флаг вычисления расчет вязкости
        /// </summary>
        protected bool FlagStartMu = false;
        /// <summary>
        /// FlagStartMu - флаг вычисления расчет вязкости
        /// </summary>
        protected bool FlagStartRoughness = false;
        /// <summary>
        /// Шероховатость дна
        /// </summary>
        protected double roughness = 0.001;
        protected double[] tauY = null;
        protected double[] tauZ = null;
        protected double[] Coord = null;

        /// <summary>
        /// Генератор КЭ сетки в ствое задачи
        /// </summary>
        protected IStripMeshGenerator meshGenerator = null;
        /// <summary>
        /// Обертка для работы с КЭ сеткой и вычисления алгебраической турбулентной вязкости
        /// </summary>
        protected IMWCross wMesh = null;

        #endregion
        public APRiverFEM1YD(RSCrossParams p) : base(p) { }

        #region Локальные методы

        /// <summary>
        /// Инициализация задачи
        /// </summary>
        protected override void InitTask()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            Q = rho_w * g * Params.J;
            // получение отметок дна
            Geometry.GetFunctionData(ref bottom_x, ref bottom_y, Params.CountBLKnots);
            // начальный уровень свободной поверхности
            waterLevel = WaterLevels.FunctionValue(0);
            // начальный расход потока
            riverFlowRate = FlowRate.FunctionValue(0);
        }
        public virtual void Scan()
        {
            TaskEvolution te = new ExTaskEvolution(time, eddyViscosityConst,
                                waterLevel, tauMax, tauMid, WetBed, Area,
                                riverFlowRate, riverFlowRateCalk, 0, 0);
            evolution.Add(te);
        }
        /// <summary>
        /// Расчет уровня свободной поверхности реки, 
        /// </summary>
        /// <returns>true - изменение, false - изменений небыло</returns>
        protected override bool CalkWaterLevel()
        {
            double waterLevel0 = waterLevel;
            if (Params.taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = FlowRate.FunctionValue(time);
                if (time > 0)
                {
                    double Ck = 0.1;
                    double Umax = 5;
                    riverFlowRateCalk = taskPoisson.SimpleRiverFlowRate(Ux, ref Area);
                    double deltaH = (riverFlowRateCalk - riverFlowRate) / riverFlowRate;
                    double W = bottom_x[bottom_x.Length - 1] - bottom_x[0];
                    double H = riverFlowRateCalk / (W * Umax) * deltaH;
                    double dH = Ck * dtime * H;
                    if (Math.Abs(dH) > 0.005)
                        dH = Math.Sign(dH) * 0.005;
                    waterLevel = waterLevel - dH;
                }
                else
                {
                    riverFlowRateCalk = riverFlowRate;
                }
            }
            else
            {
                waterLevel = WaterLevels.FunctionValue(time);
            }
            if (MEM.Equals(waterLevel0, waterLevel, 0.001) != true)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        protected override double[] TausToVols(in double[] xv, in double[] yv)
        {
            // расчет напряжений Txy  Txz
            taskPoisson.SolveTaus(ref TauY, ref TauZ, Ux, eddyViscosity);
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(0);
            // количество узлов на нижней границе области
            TSpline tauSplineZ = new TSpline();
            TSpline tauSplineY = new TSpline();
            MEM.Alloc(bounds.Length, ref tauY);
            MEM.Alloc(bounds.Length, ref tauZ);
            MEM.Alloc(bounds.Length, ref Coord);
            // пробегаем по граничным узлам и записываем для них Ty, Tz T
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < bounds.Length - 1; i++)
            {
                tauZ[i] = 0.5 * (TauZ[bounds[i]] + TauZ[bounds[i + 1]]);
                tauY[i] = 0.5 * (TauY[bounds[i]] + TauY[bounds[i + 1]]);
                Coord[i] = 0.5 * (xx[bounds[i]] + xx[bounds[i + 1]]);
            }
            double left = xx[bounds[0]];
            double right = xx[bounds[bounds.Length - 1]];
            // формируем сплайны напряжений в натуральной координате
            tauSplineZ.Set(tauZ, Coord, (uint)bounds.Length);
            tauSplineY.Set(tauY, Coord, (uint)bounds.Length);
            // массив касательных напряжений
            MEM.Alloc(xv.Length - 1, ref tau);
            for (int i = 0; i < tau.Length; i++)
            {
                double xtau = 0.5 * (xv[i] + xv[i + 1]);
                if (xtau < left || right < xtau)
                    tau[i] = 0;
                else
                {
                    double L = HPoint.Length(xv[i + 1], yv[i + 1], xv[i], yv[i]);
                    double CosG = (xv[i + 1] - xv[i]) / L;
                    double SinG = (yv[i + 1] - yv[i]) / L;
                    tau[i] = tauSplineZ.Value(xtau) * CosG +
                             tauSplineY.Value(xtau) * SinG;
                    if (double.IsNaN(tau[i]) == true)
                        throw new Exception("Mesh for RiverStreamTask");
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tau.Max();
            /// Сдвиговые напряжения средние
            tauMid = tau.Sum() / tau.Length;
            return tau;
        }

        #endregion 

        #region Вычисление вязкости
        /// <summary>
        /// расчет параметров потока, скоростей и глубины потока
        /// </summary>
        //protected override void SolveVelosity()
        //{
        //    SPhysics.PHYS.turbViscType = ETurbViscType.Leo_C_van_Rijn1984_C;
        //    SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, wMesh, Ux, Params.J);
        //    uint[] bc = mesh.GetBoundKnotsByMarker(0);
        //    // вычисление скорости
        //    task.FEPoissonTask(ref Ux, eddyViscosity, bc, null, Q);

        //    FlagStartMu = true;
        //}
        #endregion

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            base.AddMeshPolesForGraphics(sp);
            // кривые 
            // дно - берег
            sp.AddCurve("Русловой профиль", bottom_x, bottom_y);
            //if (left != null && right != null)
            //{
            //    double[] xwl = { left.x, right.x };
            //    double[] ywl = { left.y, right.y };
            //    // свободная поверхность
            //    sp.AddCurve("Свободная поверхность", xwl, ywl);
            //    Scan();
            //}
            if (evolution.Count > 1)
            {
                double[] times = (from arg in evolution select arg.time).ToArray();
                double[] wls = (from arg in evolution select arg.waterLevel).ToArray();
                sp.AddCurve("Эв.св.поверхности", times, wls, TypeGraphicsCurve.TimeCurve);
                double[] mus = (from arg in evolution select arg.eddyViscosityConst).ToArray();
                sp.AddCurve("Вязкость", times, mus, TypeGraphicsCurve.TimeCurve);
                double[] tm = (from arg in evolution select arg.tauMax).ToArray();
                sp.AddCurve("Tau максимум", times, tm, TypeGraphicsCurve.TimeCurve);
                tm = (from arg in evolution select arg.tauMid).ToArray();
                sp.AddCurve("Tau средние", times, tm, TypeGraphicsCurve.TimeCurve);
                double[] gr = (from arg in evolution select arg.WetBed).ToArray();
                sp.AddCurve("Гидравл. радиус", times, gr, TypeGraphicsCurve.TimeCurve);
                double[] ar = (from arg in evolution select arg.Area).ToArray();
                sp.AddCurve("Площадь сечения", times, ar, TypeGraphicsCurve.TimeCurve);
                double[] rfr = (from arg in evolution select arg.riverFlowRate).ToArray();
                sp.AddCurve("Расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
                rfr = (from arg in evolution select arg.riverFlowRateCalk).ToArray();
                sp.AddCurve("Текущий расчетный расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
            }
        }
    }
}
