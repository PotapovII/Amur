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
namespace NPRiverLib.APRiver_1YD
{
    using CommonLib;
    using CommonLib.Physics;
    using NPRiverLib.APRiver_1YD.Params;

    using System;
    using System.Linq;
    

    /// <summary>
    ///  ОО: Определение класса APRiverFEM_1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ 
    /// </summary>    
    [Serializable]
    public abstract class APRiverFEM_1YD : APRiver_1YD<RSCrossParams>, IRiver
    {
        #region Локальные переменные
        /// <summary>
        /// Метод вычисления турбулентной вязкости
        /// </summary>
        //[NonSerialized]
        //GetTurbulentViscosity fun = null;
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
        #endregion
        public APRiverFEM_1YD(RSCrossParams p) : base(p) { }

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
        /// Расчет уровня свободной поверхности реки
        /// </summary>
        protected override void SolveWaterLevel()
        {
            if (Params.taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = FlowRate.FunctionValue(time);
                if (time > 0)
                {
                    double Ck = 0.1;
                    double Umax = 5;
                    riverFlowRateCalk = taskU.SimpleRiverFlowRate(U, ref Area);
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
        }
        #endregion 

        #region Вычисление вязкости
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        //public void CalkRoughness()
        //{
        //    Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
        //    // вычисление приведенной вязкости и согласованной скорости
        //    roughness = DMath.RootBisectionMethod(FunCalkRoughness, 0.00001, 0.01);
        //    Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
        //    FlagStartRoughness = true;
        //}
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для eddyViscosity = const</param>
        /// <returns></returns>
        //public double FunCalkRoughness(double Roughness)
        //{
        //    roughness = Roughness;
        //    // вычисление выихревой вязкости
        //    //fun(ref eddyViscosity, in eddyViscosity0, in U);
        //    uint[] bc = mesh.GetBoundKnotsByMarker(1);
        //    // вычисление скорости
        //    taskU.FEPoissonTask(ref U, eddyViscosity, bc, null, Q);
        //    double QRiver = taskU.SimpleRiverFlowRate(U, ref Area);
        //    double errQ = (QRiver - riverFlowRate) / riverFlowRate;
        //    return errQ;
        //}
        /// <summary>
        /// расчет параметров потока, скоростей и глубины потока
        /// </summary>
        protected override void SolveVelosity()
        {
            //vmm.Set(WetBed, waterLevel, roughness, Params, meshGenerator);
            //vmm.GetModel(Params.ViscosityModel, ref fun);
            // расчет скоростей
            // CalkRoughness();
            SPhysics.PHYS.turbViscType = TurbViscType.Leo_C_van_Rijn1984_C;
            SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, wMesh, U, Params.J);
            uint[] bc = mesh.GetBoundKnotsByMarker(0);
            // вычисление скорости
            taskU.FEPoissonTask(ref U, eddyViscosity, bc, null, Q);
            
            FlagStartMu = true;
        }
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
