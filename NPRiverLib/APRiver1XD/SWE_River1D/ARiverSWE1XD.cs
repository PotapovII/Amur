//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                          23.02.2021 
//                          Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 23.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1XD.River1DSW
{
    using CommonLib;
    using CommonLib.IO;
    using MemLogLib;
    using MeshLib;
    using System;
    using System.IO;
    using GeometryLib;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;
    using CommonLib.Function;
    using MeshGeneratorsLib.TapeGenerator;
    using NPRiverLib.ATask;
    using System.Collections.Generic;

    /// <summary>
    /// ОО: Стандарт на 1D решатели мелкой воды
    /// </summary>
    [Serializable]
    public abstract class ARiverSWE1XD : APRiver1XD<RiverSWEParams1XD>, IRiver
    {
        /// <summary>
        /// Массив для вывода результатов
        /// </summary>
        protected double[] rezult = null;
        #region Параметры задачи
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            Params = new RiverSWEParams1XD();
            Params.Load(file);
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "SWEParams_1XD.txt";
            fn.NameRData = "SWENameRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh() { return new TwoMesh(x, zeta); }
        ///// <summary>
        ///// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        ///// </summary>
        //public IUnknown[] Unknowns() => unknowns;
        //protected IUnknown[] unknowns = { new Unknown("Средняя скорость потока", null, TypeFunForm.Form_1D_L1),
        //                                  new Unknown("Средняя глубина потока", null, TypeFunForm.Form_1D_L1) };
        #endregion
        /// <summary>
        /// свободная поверхность потока
        /// </summary>
        public double[] Eta;
        /// <summary>
        /// средняя скорость
        /// </summary>
        public double[] U;
        /// <summary>
        /// средняя глубина потока
        /// </summary>
        public double[] H;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double[] Ks;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double[] x;
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double[] zeta;
        /// <summary>
        ///  начальная геометрия русла
        /// </summary>
        protected IDigFunction Geometry;
        /// <summary>
        /// КЭ сетка для отрисовки
        /// </summary>
        public ARiverSWE1XD(RiverSWEParams1XD p):base(p) 
        {
        }
        /// <summary>
        /// Задачи по умолчанию
        /// </summary>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            double L = 10;
            switch (testTaskID)
            {
                case 0:
                    {
                        Geometry = new DigFunction(L, 0);
                    }
                    break;
                case 1:
                    {
                        double[] x = { 0, 2, 2.5, 4.5, 5, L };
                        double[] y = { 0, 0, -0.1, -0.1, 0, 0 };
                        Geometry = new DigFunction(x, y);
                        Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
                    }
                    break;
                case 2:
                    {
                        double[] x = { 0, 2, 2.5, 4.5, 5, L };
                        double[] y = { 0, 0, 0.1, 0.1, 0, 0 };
                        Geometry = new DigFunction(x, y);
                        Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
                    }
                    break;
            }
            Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
            Set(mesh, null);
        }
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public override void LoadData(StreamReader file)
        {
            Geometry = new DigFunction();
            Geometry.Load(file);
            Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
            Set(mesh, null);
        }
        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            eTaskReady = ETaskStatus.LoadAreaData;
            MEM.Alloc<double>(Params.CountKnots, ref Eta);
            MEM.Alloc<double>(Params.CountKnots, ref U);
            MEM.Alloc<double>(Params.CountKnots, ref H);
            MEM.Alloc<double>(Params.CountKnots, ref Ks);
            for (int i = 0; i < U.Length; i++)
            {
                U[i] = Params.U0;
                Eta[i] = Params.H0;
                H[i] = Params.H0 - zeta[i];
            }
            mesh = TapeMeshGenerator.CreateMesh(x, zeta, Eta);
            base.Set(mesh, algebra);
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            if (zeta != null)
            {
                this.zeta = zeta;
                mesh = TapeMeshGenerator.CreateMesh(x, zeta, H);
            }
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            zeta = this.zeta;
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            CalkTau(ref tauX);
            if((Erosion == EBedErosion.SedimentErosion || 
                Erosion == EBedErosion.BedLoadAndSedimentErosion)){}
            tauY = null;
            P = null;
            CS = null;
        }

        /// <summary>
        /// Расчет придонного касательного напряжения
        /// </summary>
        /// <param name="U">Скорость потока</param>
        /// <param name="H">Глубина потока</param>
        /// <returns>Придонное напряжение</returns>
        public double CalkTau(int i)
        {
            double rho_w = SPhysics.rho_w;
            double g = SPhysics.GRAV;
            double u = U[i];
            double h = H[i];
            double tau = 0;
            switch (Params.typeTau)
            {
                case TypeSWETau.Darcy: // расчет придонных напряжений по формуле Дарси
                    tau = rho_w * Params.Lambda * u * u / 2;
                    break;
                case TypeSWETau.Maning: // расчет придонных напряжений по формуле Шези
                    {
                        double H13 = Math.Pow(h, 1.0 / 3.0);
                        tau = rho_w * g * Params.Maning * Params.Maning * u * u / H13;
                    }
                    break;
                case TypeSWETau.Analytics: // расчет придонных напряжений по уклону
                    tau = rho_w * g * Params.J * h;
                    break;
                case TypeSWETau.Shezi_1: // расчет придонных напряжений по шероховатость дна
                    {
                        double e2 = 7.3890561; // Math.Exp(2);
                        double ks = Ks[i] + 0.0000000001;
                        double Ch;
                        if (H[i] / ks < e2 / 12)
                            Ch = 2.5 + 30 / e2 * h / ks;  // Для малых глубин
                        else
                            Ch = 5.75 * Math.Log10(12 * h / ks);
                        tau = rho_w * u * u / (Ch * Ch); // g сократились
                    }
                    break;
            }
            return tau;
        }
        /// <summary>
        /// Расчет придонного касательного напряжения
        /// </summary>
        /// <param name="Tau"></param>
        /// <param name="typeTau"></param>
        public void CalkTau(ref double[] Tau)
        {
            double rho_w = SPhysics.rho_w;
            double g = SPhysics.GRAV;
            MEM.Alloc<double>(Params.CountKnots, ref Tau);
            switch (Params.typeTau)
            {
                case TypeSWETau.Darcy: // расчет придонных напряжений по формуле Дарси
                    for (int i = 0; i < U.Length; i++)
                        Tau[i] = rho_w * Params.Lambda * U[i] * U[i] / 2;
                    break;
                case TypeSWETau.Maning: // расчет придонных напряжений по формуле Шези
                    double H13;
                    for (int i = 0; i < U.Length; i++)
                    {
                        H13 = Math.Pow(H[i], 1.0 / 3.0);
                        Tau[i] = rho_w * g * Params.Maning * Params.Maning * U[i] * U[i] / H13;
                        if (double.IsNaN(Tau[i]) == true)
                            Tau[i] = Tau[i];
                    }
                    break;
                case TypeSWETau.Shezi_1: // расчет придонных напряжений по уклону
                    {
                        double e2 = 7.3890561; // Math.Exp(2);
                        for (int i = 0; i < U.Length; i++)
                        {
                            double ks = Ks[i] + 0.0000000001;
                            double Ch;
                            if (H[i] / ks < e2 / 12)
                                Ch = 2.5 + 30 / e2 * H[i] / ks;  // Для малых глубин
                            else
                                Ch = 5.75 * Math.Log10(12 * H[i] / ks);
                            Tau[i] = rho_w * U[i] * U[i] / (Ch * Ch); // g сократились
                        }
                    }
                    break;
                case TypeSWETau.Analytics: // расчет придонных напряжений по уклону
                    for (int i = 0; i < U.Length; i++)
                        Tau[i] = rho_w * g * Params.J * H[i];
                    break;
            }
        }

        /// <summary>
        /// Расчет придонного касательного напряжения
        /// </summary>
        /// <param name="U">Скорость потока</param>
        /// <param name="H">Глубина потока</param>
        /// <returns>Придонное напряжение</returns>
        public double CalkTau(double _U, double _H = 0, int taypeTau = 0)
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            double H13 = Math.Pow(_H, 1.0 / 3.0);
            double tau = 0;
            switch (taypeTau)
            {
                case 0: // расчет придонных напряжений по формуле Дарси
                    tau = rho_w * Params.Lambda * _U * _U / 2;
                    break;
                case 1: // расчет придонных напряжений по формуле Шези
                    tau = rho_w * g * Params.Maning * Params.Maning * _U * _U / H13;
                    break;
                case 2: // расчет придонных напряжений по уклону
                    tau = rho_w * g * Params.J * _H;
                    break;
            }
            return tau;
        }

        double e2 = 7.3890561; // Math.Exp(2);
        double ks, Ch;
        protected double GetLambda(int i)
        {
            ks = Ks[i] + 0.0000000001;
            if (H[i] / ks < e2 / 12)
                Ch = 2.5 + 30 / e2 * H[i] / ks;  // Для малых глубин
            else
                Ch = 5.75 * Math.Log10(12 * H[i] / ks);
            return 2.0 / (Ch * Ch); // g сократились
        }

        

        ///// <summary>
        ///// Создает экземпляр класса конвертера
        ///// </summary>
        ///// <returns></returns>
        //public override IOFormater<IRiver> GetFormater()
        //{
        //    return new ProxyTaskFormat<IRiver>();
        //}
    }
}
