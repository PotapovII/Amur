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
    using MemLogLib;
    using CommonLib;
    using MeshGeneratorsLib.StripGenerator;

    using System;
    using System.Linq;
    using CommonLib.Geometry;
    using GeometryLib;
    using NPRiverLib.APRiver_1XD;

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
        /// <summary>
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        protected override double[] TausToVols(in double[] xv,in double[] yv)
        {
            // массив касательных напряжений
            MEM.Alloc(xv.Length - 1, ref tau);
            // расчет напряжений Txy  Txz
            SolveTaus();
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(0);
            // количество узлов на нижней границе области
            TSpline tauSplineZ = new TSpline();
            TSpline tauSplineY = new TSpline();
            MEM.Alloc(bounds.Length, ref tau_xy);
            MEM.Alloc(bounds.Length, ref tau_xz);
            MEM.Alloc(bounds.Length, ref Coord);
            // пробегаем по граничным узлам и записываем для них Ty, Tz 
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < bounds.Length - 1; i++)
            {
                tau_xz[i] = 0.5 * (TauZ[bounds[i]] + TauZ[bounds[i + 1]]);
                tau_xy[i] = 0.5 * (TauY[bounds[i]] + TauY[bounds[i + 1]]);
                Coord[i] = 0.5 * (xx[bounds[i]] + xx[bounds[i + 1]]);
            }
            double left = xx[bounds[0]];
            double right = xx[bounds[bounds.Length - 1]];
            // формируем сплайны напряжений в натуральной координате
            tauSplineZ.Set(tau_xz, Coord, (uint)bounds.Length);
            tauSplineY.Set(tau_xy, Coord, (uint)bounds.Length);

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
                    {
                        tau[i] = 0;
                        //throw new Exception("Mesh for RiverStreamTask");
                    }
                        
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tau.Max();
            /// Сдвиговые напряжения средние
            tauMid = tau.Sum() / tau.Length;
            return tau;
        }

        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        protected  void SolveTaus()
        {
            try
            {
                double[] x = { 0, 0, 0 };
                double[] y = { 0, 0, 0 };
                double[] u = { 0, 0, 0 };
                double S;
                uint[] knots = { 0, 0, 0 };
                double[] Selem = new double[mesh.CountElements];
                double[] tmpTausZ = new double[mesh.CountElements];
                double[] tmpTausY = new double[mesh.CountElements];
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] = 0;
                    TauZ[i] = 0;
                }
                for (uint i = 0; i < mesh.CountElements; i++)
                {
                    mesh.ElementKnots(i, ref knots);
                    mesh.GetElemCoords(i, ref x, ref y);
                    // получить среднюю вязкость на КЭ
                    double Mu = (eddyViscosity[knots[0]] + eddyViscosity[knots[1]] + eddyViscosity[knots[2]]) / 3;
                    //Mu = mesh.ElementMidleValues(eddyViscosity, i);
                    // площадь КЭ
                    S = mesh.ElemSquare(i);
                    // деформации
                    double dN0dz = (x[2] - x[1]) / (2 * S);
                    double dN1dz = (x[0] - x[2]) / (2 * S);
                    double dN2dz = (x[1] - x[0]) / (2 * S);

                    double dN0dy = (y[1] - y[2]) / (2 * S);
                    double dN1dy = (y[2] - y[0]) / (2 * S);
                    double dN2dy = (y[0] - y[1]) / (2 * S);

                    double Ez = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0]));
                    double Ey = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1]));
                    // 
                    tmpTausZ[i] = Mu * Ez / (2 * S);
                    tmpTausY[i] = Mu * Ey / (2 * S);

                    Selem[(int)knots[0]] += S / 3;
                    Selem[(int)knots[1]] += S / 3;
                    Selem[(int)knots[2]] += S / 3;

                    TauZ[(int)knots[0]] += S * tmpTausZ[i] / 3;
                    TauZ[(int)knots[1]] += S * tmpTausZ[i] / 3;
                    TauZ[(int)knots[2]] += S * tmpTausZ[i] / 3;

                    TauY[(int)knots[0]] += S * tmpTausY[i] / 3;
                    TauY[(int)knots[1]] += S * tmpTausY[i] / 3;
                    TauY[(int)knots[2]] += S * tmpTausY[i] / 3;
                }
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] /= Selem[i];
                    TauZ[i] /= Selem[i];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
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
            double[] xx = null;
            MEM.Alloc(tau.Length, ref xx);
            for (int i = 0; i < xx.Length; i++)
                xx[i] = 0.5 * (bottom_x[i] + bottom_x[i + 1]);
            sp.AddCurve("Придонные напряжения", xx, tau);
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
