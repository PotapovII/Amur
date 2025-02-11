namespace NPRiverLib.APRiver2XYD.River2DFST
{
    using System;
    using System.IO;

    using MeshLib;
    using NPRiverLib.IO;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.ChannelProcess;
    using CommonLib.Delegate;
    using CommonLib.Function;

    /// <summary>
    /// Флаг ГУ задачи
    /// </summary>
    [Flags]
    [Serializable]
    public enum NodeType
    {
        None = 0b_0000_0000,
        Bordary = 0b_0000_0001,
        Input = 0b_0000_0010,
        Output = 0b_0000_0100,
        InputBordary = Input | Bordary,
        OutputBordary = Output | Bordary,
        Wet = 0b_0001_0000,
        Dry = 0b_0010_0000,
        Solve = 0b_1000_0000,
        SolveWet = Solve | Wet,
        SolveDry = Solve | Dry,
        BordaryWet = Bordary | Wet,
        BordaryDry = Bordary | Dry,
    }
    /// <summary>
    ///  ОО: Определение класса RiverSWE_FCT_2XYD - для расчета полей расходов, 
    ///  скорости, глубин и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public abstract class ARiverSWE_FCT_2XYD : APRiver2XYD<SWE_FCTParams_2XYD>, IRiver
    {
        #region поля задачи
        /// <summary>
        /// Длина области
        /// </summary>
        protected double Lx;
        /// <summary>
        /// Ширина области
        /// </summary>
        protected double Ly;
        /// <summary>
        /// Количество узлов по Х
        /// </summary>
        protected int Nx;
        /// <summary>
        /// Количество узлов по У
        /// </summary>
        protected int Ny;
        /// <summary>
        /// шаг сетки по х и у
        /// </summary>
        protected double dx, dy;

        protected FCTTaskIndex TaskIndex;
        protected double CourantNumber;
        /// <summary>
        /// Метод установки НУ задачи
        /// </summary>
        public SimpleProcedure initialCondition { get; set; } = null;
        /// <summary>
        /// Метод установки ГУ задачи
        /// </summary>
        public SimpleProcedure borderCondition { get; set; } = null;
        /// <summary>
        /// Скорость по оси X
        /// </summary>
        protected double[,] u;
        /// <summary>
        /// Скорость по оси Y
        /// </summary>
        protected double[,] v;
        /// <summary>
        /// Параметр расхода воды
        /// </summary>
        protected const double Qh = 1.01f;
        /// <summary>
        /// Область решения
        /// </summary>
        protected bool[,] regionSolver;
        /// <summary>
		/// Область входной границы
		/// </summary>
        protected bool[,] inputBoundary;
        /// <summary>
        /// Параметры сглаживания разрыва в полях задачи
        /// </summary>
        protected const double ETA = 1.0f / 6;
        protected const double ET1 = 1.0f / 3;
        protected const double ET2 = -1.0f / 6;

        protected double[] b_u = null, b_v = null;
        #endregion 
        ///// <summary>
        ///// Симплекс сетка области для отрисовки поляй задачи
        ///// </summary>
        protected RectangleMeshTri rMesh = null;

        public ARiverSWE_FCT_2XYD(SWE_FCTParams_2XYD p) : base(p)
        {
            Lx = p.Lx;
            Ly = p.Ly;
            Nx = p.Nx;
            Ny = p.Ny;
            CourantNumber = p.CourantNumber;
            TaskIndex = p.TaskIndex;
        }
        #region IRiver
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики при загрузке
        /// по умолчанию
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh()
        {
            return null;
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReader2XYD_FCT();
        }

        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName)
        {
            return false;
        }
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions = null) { }
        #region методы предстартовой подготовки задачи
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(StreamReader file)
        {
            //MEM.Alloc<double>(Params.CountKnots, ref x);
            //MEM.Alloc<double>(Params.CountKnots, ref zeta);
            ///// <summary>
            ///// уровни(нь) свободной поверхности потока
            ///// </summary>
            //IDigFunction Geometry = new DigFunction();
            //Geometry.Load(file);
            //Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
            //IMesh mesh = new TwoMesh(x, zeta);
            //Set(mesh, null);
        }

        #endregion 
        /// <summary>
        /// Установка объектоа КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {

        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {

        }
        #endregion
    }
}
