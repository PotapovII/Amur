////---------------------------------------------------------------------------
////                          ПРОЕКТ  "DISER"
////                  создано  :   9.03.2007 Потапов И.И.
////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////            перенесено с правкой : 06.12.2020 Потапов И.И. 
////            создание родителя : 21.02.2022 Потапов И.И. 
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using AlgebraLib;
//    using CommonLib;
//    using CommonLib.IO;
//    using MemLogLib;
//    using GeometryLib;
//    using MeshLib;
//    using System;
//    using System.IO;
//    /// <summary>
//    ///  ОО: Определение класса SrossSectionalRiverTask - расчет полей скорости 
//    ///     и напряжений в живом сечении потока МКЭ на произвольной сетке
//    /// </summary>
//    [Serializable]
//    public abstract class ARiverTask : IRiver
//    {
//        #region Свойства параметров IPropertyTask
//        /// <summary>
//        /// Переопределяется в наследнике для конкретных свойств
//        /// </summary>
//        public abstract IPropertyTask PropertyTask { get; set; }
//        /// <summary>
//        /// свойств задачи
//        /// </summary>
//        /// <param name="p"></param>
//        public abstract object GetParams();
//        /// <summary>
//        /// Установка свойств задачи
//        /// </summary>
//        /// <param name="p"></param>
//        public abstract void SetParams(object obj);
//        /// <summary>
//        /// Чтение параметров задачи из файла
//        /// </summary>
//        /// <param name="file"></param>
//        public abstract void LoadParams(string fileName);
//        #endregion

//        #region Свойства IRiver
//        /// <summary>
//        /// Получить граничные условия для задачи донных деформаций
//        /// </summary>
//        /// <returns></returns>
//        public IBoundaryConditions GetBConditions()
//        {
//            return null;
//        }

//        /// <summary>
//        /// Имена файлов с данными для задачи гидродинамики
//        /// </summary>
//        public virtual ITaskFileNames taskFileNemes()
//        {
//            ITaskFileNames fn = new TaskFileNames();
//            fn.NameCPParams = "NameCPParams.txt";
//            fn.NameBLParams = "NameBLParams.txt";
//            fn.NameRSParams = "NameRSParams.txt";
//            fn.NameRData = "NameCrossRData.txt";
//            fn.NameEXT = "(*.tsk)|*.tsk|";
//            return fn;
//        }
//        /// <summary>
//        /// Наименование задачи
//        /// </summary>
//        public string Name => name;
//        protected string name = "гидрадинамика створа";
//        /// <summary       
//        /// Тип задачи используется для выбора совместимых подзадач
//        /// </summary>
//        public virtual TypeTask typeTask { get => TypeTask.streamY1D; }
//        /// <summary>
//        /// версия дата последних изменений интерфейса задачи
//        /// </summary>
//        public virtual string VersionData() => "ARiverTask 11.03.2022"; 
//        /// <summary>
//        /// Текущее время
//        /// </summary>
//        public double time { get; set; }
//        /// <summary>
//        /// Текущий шаг по времени
//        /// </summary>
//        public double dtime { get; set; }
//        #endregion

//        #region КЭ
//        /// <summary>
//        /// Узлы КЭ
//        /// </summary>
//        protected uint[] knots = { 0, 0, 0 };
//        /// <summary>
//        /// Квадратурные точки для численного интегрирования
//        /// </summary>
//        protected NumInegrationPoints pIntegration;
//        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
//        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
//        /// <summary>
//        /// функции формы для КЭ
//        /// </summary>
//        protected AbFunForm ff = null;
//        /// <summary>
//        /// локальная матрица часть СЛАУ
//        /// </summary>
//        protected double[][] LaplMatrix = null;
//        /// <summary>
//        /// локальная правая часть СЛАУ
//        /// </summary>
//        protected double[] LocalRight = null;
//        /// <summary>
//        /// адреса ГУ
//        /// </summary>
//        protected uint[] adressBound = null;
//        /// <summary>
//        /// координаты узлов КЭ 
//        /// </summary>
//        protected double[] elem_x = null;
//        protected double[] elem_y = null;
//        /// <summary>
//        /// Скорсть в узлах
//        /// </summary>
//        protected double[] elem_U = null;
//        protected double[] elem_V = null;
//        /// <summary>
//        /// вязкость в узлах КЭ 
//        /// </summary>
//        protected double[] elem_mu = null;
//        #endregion
//        /// <summary>
//        /// Количество узлов на КЭ
//        /// </summary>
//        protected private int cu;
//        /// <summary>
//        /// КЭ сетка векторая для отрисовки
//        /// </summary>
//        public IMesh Mesh => mesh;
//        /// <summary>
//        /// КЭ сетка КЭ для расчетов
//        /// </summary>
//        protected IFEMesh mesh = null;
//        /// <summary>
//        /// Поле скоростей
//        /// </summary>
//        public double[] U;
//        /// <summary>
//        /// Поле скоростей
//        /// </summary>
//        public double[] V;
//        /// <summary>
//        /// Алгебра для КЭ задачи
//        /// </summary>
//        [NonSerialized]
//        protected IAlgebra algebra = null;
//        /// <summary>
//        /// Длина смоченного периметра
//        /// </summary>
//        protected double GR = 0;
//        /// <summary>
//        /// площадь сечения канала
//        /// </summary>
//        public double Area = 0;
//        /// <summary>
//        /// правая часть уравнения
//        /// </summary>
//        protected double Q;
//        /// <summary>
//        ///  Сетка для расчета донных деформаций
//        /// </summary>
//        public virtual IMesh BedMesh() { return new TwoMesh(bottom_x, bottom_y); }
//        /// <summary>
//        /// координаты дна по оси х
//        /// </summary>
//        protected double[] bottom_x;
//        /// <summary>
//        /// координаты дна по оси у
//        /// </summary>
//        protected double[] bottom_y;
//        /// <summary>
//        /// точка правого уреза воды
//        /// </summary>
//        protected HKnot right;
//        /// <summary>
//        /// точка левого уреза воды
//        /// </summary>
//        protected HKnot left;
//        /// <summary>
//        /// текущий расход потока
//        /// </summary>
//        protected double riverFlowRate;
//        /// <summary>
//        /// текущий расчетный расход потока 
//        /// </summary>
//        protected double riverFlowRateCalk;
//        /// <summary>
//        /// текущий уровень свободной поверхности
//        /// </summary>
//        protected double waterLevel;
//        /// <summary>
//        /// Сдвиговые напряжения максимум
//        /// </summary>
//        public double tauMax = 0;
//        /// <summary>
//        /// Сдвиговые напряжения средние
//        /// </summary>
//        public double tauMid = 0;
//        /// <summary>
//        ///  начальная геометрия русла
//        /// </summary>
//        protected IDigFunction Geometry;
//        /// <summary>
//        /// уровни(нь) свободной поверхности потока
//        /// </summary>
//        protected IDigFunction WaterLevels;
//        /// <summary>
//        /// расход потока
//        /// </summary>
//        protected IDigFunction flowRate;
//        /// <summary>
//        /// FlagStartMesh - первая генерация сетки
//        /// </summary>
//        protected bool FlagStartMesh = false;
//        /// <summary>
//        /// FlagStartMu - флаг вычисления расчет вязкости
//        /// </summary>
//        protected bool FlagStartMu = false;
//        /// <summary>
//        /// FlagStartMu - флаг вычисления расчет вязкости
//        /// </summary>
//        protected bool FlagStartRoughness = false;
//        /// <summary>
//        /// Шероховатость дна
//        /// </summary>
//        protected double roughness = 0.001;
//        protected double[] tauY = null;
//        protected double[] tauZ = null;
//        protected double[] Coord = null;
//        /// <summary>
//        /// Краевые условия задачи
//        /// </summary>
//        public IBoundaryCondition BoundaryCondition = null;
//        public ARiverTask(IPropertyTask p)
//        {
//            PropertyTask = p;
//            Init();
//        }
//        /// <summary>
//        /// Инициализация 
//        /// </summary>
//        protected void Init()
//        {
//            // геометрия дна
//            Geometry = new DigFunction();
//            // свободная поверхность
//            WaterLevels = new DigFunction();
//            // расход потока
//            flowRate = new DigFunction();
//        }
//        /// <summary>
//        /// создание/очистка ЛМЖ и ЛПЧ ...
//        /// </summary>
//        /// <param name="cu">количество неизвестных</param>
//        public void InitLocal(int cu)
//        {
//            MEM.Alloc<double>(cu, ref elem_x, "elem_x");
//            MEM.Alloc<double>(cu, ref elem_y, "elem_y");
//            MEM.Alloc<double>(cu, ref elem_U, "elem_U");
//            MEM.Alloc<double>(cu, ref elem_mu, "elem_mu");
//            // с учетом степеней свободы
//            MEM.AllocClear(cu, ref LocalRight);
//            MEM.Alloc2DClear(cu, ref LaplMatrix);
//        }
//        /// <summary>
//        /// Чтение данных задачи из файла
//        /// </summary>
//        /// <param name="file"></param>
//        public void LoadData(StreamReader file)
//        {
//            // геометрия дна
//            Geometry.Load(file);
//            // свободная поверхность
//            WaterLevels.Load(file);
//            // расход потока
//            flowRate.Load(file);
//            // инициализация задачи
//            InitTask();
//        }
//        /// <summary>
//        /// Выполнение скалярных граничных условий 1 рода (функция на границе)
//        /// </summary>
//        /// <param name="algebra">алгебра</param>
//        /// <param name="FName">имя поля</param>
//        /// <param name="typeBC">тип гр</param>
//        public void BordersCondition(IAlgebra algebra, string FName)
//        {
//            double[] bcPhi = null;
//            uint[] bcIndex = null;
//            int[] bcLabels = BoundaryCondition.BordersLabels(TypeBoundaryCondition.Dirichlet);
//            for (int ibc = 0; ibc < bcLabels.Length; ibc++)
//            {
//                BoundaryCondition.BordersCondition(FName, bcLabels[ibc], ref bcPhi, ref bcIndex);
//                algebra.BoundConditions(bcPhi, bcIndex);
//            }
//        }

       
//        /// <summary>
//        /// Инициализация задачи
//        /// </summary>
//        public abstract void InitTask();

//        /// <summary>
//        /// Чтение входных данных задачи из файла
//        /// геометрия канала
//        /// эволюция расходов/уровней
//        /// </summary>
//        /// <param name="p"></param>
//        public void LoadData(string fileName)
//        {
//            string message = "Файл данных задачи не обнаружен";
//            WR.LoadParams(LoadData, message, fileName);
//        }
//        /// <summary>
//        /// Установка адаптеров для КЭ сетки и алгебры
//        /// </summary>
//        /// <param name="_mesh">сетка</param>
//        /// <param name="algebra">решатель</param>
//        public void Set(IMesh mesh, IAlgebra algebra = null)
//        {
//            this.mesh = (IFEMesh) mesh;
//            this.algebra = algebra;
//        }
//        /// <summary>
//        /// Получить отметки дна
//        /// </summary>
//        /// <param name="zeta"></param>
//        public void GetZeta(ref double[] zeta)
//        {
//            zeta = bottom_y;
//        }
//        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//        /// <summary>
//        /// Создает экземпляр класса конвертера
//        /// </summary>
//        /// <returns></returns>
//        public virtual IOFormater<IRiver> GetFormater()
//        {
//            return new ProxyTaskFormat<IRiver>();
//        }
//        #region абстрактные методы
//        /// <summary>
//        /// Установка новых отметок дна
//        /// </summary>
//        /// <param name="zeta">отметки дна</param>
//        /// <param name="bedErosion">флаг генерация сетки при размывах дна</param>
//        public abstract void SetZeta(double[] zeta, bool bedErosion);
//        /// <summary>
//        /// Расчет полей глубины и скоростей на текущем шаге по времени
//        /// </summary>
//        public abstract void SolverStep();
//        /// <summary>
//        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
//        /// </summary>
//        /// <param name="sp">контейнер данных</param>
//        public abstract void AddMeshPolesForGraphics(ISavePoint sp);
//        /// <summary>
//        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
//        /// усредненных на конечных элементах
//        /// </summary>
//        /// <param name="tauX">придонное касательное напряжение по х</param>
//        /// <param name="tauY">придонное касательное напряжение по у</param>
//        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
//        public abstract void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P, StressesFlag sf = StressesFlag.Nod);
//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public abstract IRiver Clone();
//        #endregion
//    }
//}
