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
namespace NPRiverLib.APRiver_1XD
{
    using System;

    using CommonLib;
    using MemLogLib;
    using MeshGeneratorsLib.StripGenerator;
    /// <summary>
    ///  ОО: Определение класса APRiverFEM1YD - расчет полей скорости, 
    ///  вязкости и напряжений в живом сечении потока методом КЭ 
    /// </summary>    
    [Serializable]
    public abstract class APRiverFEM1XD : AFEMRiver1XD<FEMParams_1XD>, IRiver
    {
        #region Локальные переменные
        /// <summary>
        /// Задача Пуассона КЭ реализация
        /// </summary>
        [NonSerialized]
        protected IFEPoissonTask taskPoisson = null;
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
        /// <summary>
        /// Придонныое касательыое напряжение tau_xy
        /// </summary>
        protected double[] tau_xy = null;
        /// <summary>
        /// Придонныое касательыое напряжение tau_xz
        /// </summary>
        protected double[] tau_xz = null;
        /// <summary>
        /// Аргумент для придонных касательынх напряжений
        /// </summary>
        protected double[] Coord = null;

        /// <summary>
        /// Генератор КЭ сетки в ствое задачи
        /// </summary>
        protected IStripMeshGenerator meshGenerator = null;


        #endregion
        public APRiverFEM1XD(FEMParams_1XD p) : base(p) 
        {
        }

        #region Локальные методы
        ///// <summary>
        ///// Расчет касательных напряжений на дне канала
        ///// </summary>
        ///// <param name="mesh">сетка</param>
        ///// <param name="TauZ">напражения на сетке</param>
        ///// <param name="TauY">напражения на сетке</param>
        ///// <param name="xv">координаты Х узлов КО</param>
        ///// <param name="yv">координаты У узлов КО</param>
        //protected override double[] TausToVols(in double[] xv,in double[] yv, bool spline = false)
        //{
        //    // массив касательных напряжений
        //    MEM.Alloc(xv.Length - 1, ref tau);
        //    // расчет напряжений Txy  Txz
        //    SolveTaus();
        //    // граничные узлы на нижней границе области
        //    uint[] bounds = mesh.GetBoundKnotsByMarker(0);
        //    // количество узлов на нижней границе области
        //    MEM.Alloc(bounds.Length - 1, ref tau_xy);
        //    MEM.Alloc(bounds.Length - 1, ref tau_xz);
        //    MEM.Alloc(bounds.Length - 1, ref Coord);
        //    // пробегаем по граничным узлам и записываем для них Ty, Tz 
        //    double[] xx = mesh.GetCoords(0);
        //    for (int i = 0; i < bounds.Length - 1; i++)
        //    {
        //        tau_xz[i] = 0.5 * (TauZ[bounds[i]] + TauZ[bounds[i + 1]]);
        //        tau_xy[i] = 0.5 * (TauY[bounds[i]] + TauY[bounds[i + 1]]);
        //        Coord[i] = 0.5 * (xx[bounds[i]] + xx[bounds[i + 1]]);
        //    }
        //    double left = xx[bounds[0]];
        //    double right = xx[bounds[bounds.Length - 1]];
        //    // формируем сплайны напряжений в натуральной координате

        //    if (spline == true)
        //    {
        //        TSpline tauSplineZ = new TSpline();
        //        TSpline tauSplineY = new TSpline();
        //        tauSplineZ.Set(tau_xz, Coord, (uint)bounds.Length);
        //        tauSplineY.Set(tau_xy, Coord, (uint)bounds.Length);
        //        for (int i = 0; i < tau.Length; i++)
        //        {
        //            double xtau = 0.5 * (xv[i] + xv[i + 1]);
        //            if (xtau < left || right < xtau)
        //                tau[i] = 0;
        //            else
        //            {
        //                double L = HPoint.Length(xv[i + 1], yv[i + 1], xv[i], yv[i]);
        //                double CosG = (xv[i + 1] - xv[i]) / L;
        //                double SinG = (yv[i + 1] - yv[i]) / L;
        //                tau[i] = tauSplineZ.Value(xtau) * CosG +
        //                         tauSplineY.Value(xtau) * SinG;
        //                if (double.IsNaN(tau[i]) == true)
        //                {
        //                    tau[i] = 0;
        //                    //throw new Exception("Mesh for RiverStreamTask");
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        IDigFunction tauZ = new DigFunction(Coord, tau_xz);
        //        IDigFunction tauY = new DigFunction(Coord, tau_xy);
        //        for (int i = 0; i < tau.Length; i++)
        //        {
        //            double xtau = 0.5 * (xv[i] + xv[i + 1]);
        //            if (xtau < left || right < xtau)
        //                tau[i] = 0;
        //            else
        //            {
        //                double L = HPoint.Length(xv[i + 1], yv[i + 1], xv[i], yv[i]);
        //                double CosG = (xv[i + 1] - xv[i]) / L;
        //                double SinG = (yv[i + 1] - yv[i]) / L;
        //                tau[i] = tauZ.FunctionValue(xtau) * CosG +
        //                         tauY.FunctionValue(xtau) * SinG;
        //                if (double.IsNaN(tau[i]) == true)
        //                {
        //                    tau[i] = 0;
        //                    //throw new Exception("Mesh for RiverStreamTask");
        //                }
        //            }
        //        }

        //    }
        //    // Сдвиговые напряжения максимум
        //    tauMax = tau.Max();
        //    /// Сдвиговые напряжения средние
        //    tauMid = tau.Sum() / tau.Length;
        //    return tau;
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
            // дно - берег узлы
            sp.AddCurve("Русловой профиль (узлы)", bottom_x, bottom_y);
            sp.AddCurve("Придонные напряжения", xtau, tau);
        }
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

    }
}
