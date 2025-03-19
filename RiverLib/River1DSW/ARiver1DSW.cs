//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                          23.02.2021 
//                          Потапов И.И. 
//---------------------------------------------------------------------------
namespace RiverLib
{
    using CommonLib;
    using CommonLib.IO;
    using MemLogLib;
    using MeshLib;
    using System;
    using System.IO;
    using GeometryLib;
    using CommonLib.EConverter;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;
    using CommonLib.Function;

    /// <summary>
    /// ОО: Стандарт на 1D решатели мелкой воды
    /// </summary>
    [Serializable]
    public abstract class ARiver1DSW : IRiver
    {

        #region Параметры задачи
        /// <summary>
        /// Параметры задачи
        /// </summary>
        protected River1DSWParams Params = new River1DSWParams();
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            River1DSWParams pp = p as River1DSWParams;
            SetParams(pp);
        }
        public void SetParams(River1DSWParams p)
        {
            Params = new River1DSWParams(p);
        }
        public object GetParams()
        {
            return Params;
        }
        public void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            WR.LoadParams(Load, message, fileName);
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            Params = new River1DSWParams();
            Params.Load(file);
        }
        #endregion

        #region Свойства

        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => null;
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;

        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            TaskFileNames fn = new TaskFileNames();
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "SWENameRSParams.txt";
            fn.NameRData = "SWENameRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get => name; }
        protected string name;
        /// <summary       
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamX1D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "ARiver1DSW 01.09.2021"; 
        /// <summary>
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get; set; }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh() { return new TwoMesh(x, zeta); }
        /// <summary>
        ///  Сетка для расчета задачи гидродинамики
        /// </summary>
        public IMesh Mesh() => mesh; 
        IMesh mesh = null;

        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Средняя скорость потока", null, TypeFunForm.Form_1D_L1),
                                          new Unknown("Средняя глубина потока", null, TypeFunForm.Form_1D_L1) };
        #endregion
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
        /// алгебра
        /// </summary>
        protected IAlgebra algebra = null;

        public ARiver1DSW(River1DSWParams p, IMesh mesh = null, IAlgebra algebra = null) 
        {
            SetParams(p);
            Set(mesh, algebra);
        }
        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public void Set(IMesh mesh, IAlgebra algebra = null)
        {
            if (algebra != null)
                this.algebra = algebra;
            this.mesh = mesh as TwoMesh;
            if (this.mesh != null)
            {
                x = mesh.GetCoords(0);
                zeta = mesh.GetCoords(1);
                Params.CountKnots = x.Length;
            }
            else
                return;
            //else
            //    throw new Exception("Сетка задачи не определена");

            MEM.Alloc<double>(Params.CountKnots, ref U);
            MEM.Alloc<double>(Params.CountKnots, ref H);
            MEM.Alloc<double>(Params.CountKnots, ref Ks);
            for (int i = 0; i < U.Length; i++)
            {
                U[i] = Params.U0;
                H[i] = Params.H0;
            }
        }
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            this.zeta = zeta;
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            zeta = this.zeta;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public abstract void SolverStep();
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IRiver Clone();
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public abstract void AddMeshPolesForGraphics(ISavePoint sp);
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            CalkTau(ref tauX);
            if((Erosion == EBedErosion.SedimentErosion || Erosion == EBedErosion.BedLoadAndSedimentErosion))
            {

            }
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
        //public void Set(double[] U, double[] H)
        //{
        //    // Начальные поля
        //    for (int i = 0; i < CountKnots; i++)
        //    {
        //        this.U[i] = U[i];
        //        this.H[i] = H[i];
        //    }
        //}
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public virtual bool LoadData(string fileName)
        {
            string message = "Файл данных задачи не обнаружен";
            return WR.LoadParams(LoadData, message, fileName);
        }
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void LoadData(StreamReader file)
        {
            MEM.Alloc<double>(Params.CountKnots, ref x);
            MEM.Alloc<double>(Params.CountKnots, ref zeta);
            /// <summary>
            /// уровни(нь) свободной поверхности потока
            /// </summary>
            IDigFunction Geometry = new DigFunction();
            Geometry.Load(file);
            Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
            IMesh mesh = new TwoMesh(x, zeta);
            Set(mesh, null);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }
    }
}
