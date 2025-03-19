//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                  Расчет речного потока в створе русла
//---------------------------------------------------------------------------
//               кодировка  : 19.05.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using MeshLib;
    using GeometryLib.Vector;
    using GeometryLib;
    using MeshLib.Mesh.RecMesh;
    using CommonLib.ChannelProcess;
    using CommonLib.Function;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CommonLib.Physics;

    /// <summary>
    ///  ОО: Определение класса RiverSectionaVarQuad - для расчета поля скорости
    ///  в створе речного потока
    /// </summary>
    [Serializable]
    public class RiverSectionaVarQuad : RiverStreamQuadParams, IRiver
    {
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;

        ///// <summary>
        ///// Граничные условия для поля скорости
        ///// </summary>
        //TypeBoundCond[] BCType = {     TypeBoundCond.Dirichlet,
        //                               TypeBoundCond.Neumann,
        //                               TypeBoundCond.Neumann,
        //                               TypeBoundCond.Neumann };
        ///// <summary>
        ///// Граничные условия для поля вязкости
        ///// </summary>
        //TypeBoundCond[] BCTypeMu = {   TypeBoundCond.Dirichlet,
        //                               TypeBoundCond.Dirichlet,
        //                               TypeBoundCond.Dirichlet,
        //                               TypeBoundCond.Dirichlet };
        /// <summary>
        /// Граничные условия для поля скорости
        /// </summary>
        TypeBoundCond[] BCType = {     TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Neumann };
        /// <summary>
        /// Граничные условия для поля вязкости
        /// </summary>
        TypeBoundCond[] BCTypeMu = {   TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Dirichlet};

        /// <summary>
        /// однородные кравевые условия
        /// </summary>
        BoundaryConditionsQuad bc = new BoundaryConditionsQuad();
        /// <summary>
        /// однородные кравевые условия
        /// </summary>
        BoundaryConditionsQuad bcMu = new BoundaryConditionsQuad();
        /// <summary>
        /// Решатель для поля скорости
        /// </summary>
        AlgebraQuadSolver algebra;
        /// <summary>
        /// Решатель для поля вязкости
        /// </summary>
        AlgebraQuadSolver algebraMu;

        /// <summary>
        /// ускорение с.п.
        /// </summary>
        protected double g = SPhysics.GRAV;
        /// <summary>
        /// Плотность воды
        /// </summary>
        protected double rho_w = SPhysics.rho_w;
        /// <summary>
        /// Плотность песка
        /// </summary>
        protected double rho_s = SPhysics.PHYS.rho_s;
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        protected double Mu0 = SPhysics.mu;
        /// <summary>
        /// коэффициент Кармана
        /// </summary>
        protected double kappa = SPhysics.kappa_w;
        /// <summary>
        /// Вязкость потока
        /// </summary>
        protected double Mu_w = SPhysics.mu;
        /// <summary>
        /// Пористость песка
        /// </summary>
        protected double epsilon = SPhysics.PHYS.epsilon;
        /// <summary>
        /// Вязкость песка приведенная
        /// </summary>
        protected double MuS = 10000;


        protected double Rcoef = 1;
        /// <summary>
        /// текущий уровень свободной поверхности
        /// </summary>
        protected double waterLevel;
        /// <summary>
        /// FlagStartMesh - первая генерация сетки
        /// </summary>
        protected bool FlagStartMesh = false;
        /// <summary>
        /// текущий расход потока
        /// </summary>
        protected double riverFlowRate;
        /// <summary>
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get; set; }
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        protected double[] bottom_x;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        protected double[] Zeta;
        protected double[] Depth;
        protected double[] U_star;
        protected double[] MuMax;
        /// <summary>
        /// точка правого уреза воды
        /// </summary>
        protected HKnot right;
        /// <summary>
        /// точка левого уреза воды
        /// </summary>
        protected HKnot left;
        /// <summary>
        ///  начальная геометрия русла
        /// </summary>
        protected IDigFunction Geometry;
        /// <summary>
        /// уровни(нь) свободной поверхности потока
        /// </summary>
        protected IDigFunction WaterLevels;
        /// <summary>
        /// расход потока
        /// </summary>
        protected IDigFunction flowRate;
        /// <summary>
        /// Параметры задачи зависимые от времени
        /// </summary>
        protected List<TaskEvolution> evolution = new List<TaskEvolution>();
        #region Поля для отрисовки
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[] U;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[] Mu;
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[] TauY;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] TauX;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[] tau;
        /// <summary>
        /// Касательное придонное напряжение
        /// </summary>
        public double[] bNormalTau;
        /// <summary>
        /// Касательное придонное гидростатическое напряжение
        /// </summary>
        public double[] bNormalTauGS;
        /// <summary>
        /// Придонное давление
        /// </summary>
        public double[] bPress;
        #endregion
        #region Поля для расчета
        /// <summary>
        /// Симплекс сетка области для отрисовки поляй задачи
        /// </summary>
        protected ChannelRectangleMesh mesh = null;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[][] mU;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMu;
        /// <summary>
        /// Поле плотности 
        /// </summary>
        public double[][] mRho;
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[][] mTauY;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[][] mTau;
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[][] mTauY1;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX1;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX2;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauY2;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauX3;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[][] mTauY3;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[][] mTau1;
        /// <summary>
        /// Источник для расчета вязкости
        /// </summary>
        double Fmu = 0;

        #endregion
        /// <summary>
        /// Начальный интекс донной поверхности
        /// </summary>
        protected int [] bottonIndex;
        /// <summary>
        /// Маркер границы донной поверхности
        /// </summary>
        protected int[] bottonMarkers;
        protected double[][] aP, aE, aW, aN, aS, aQ;
        protected double uBuff, bap, fe, fw, fn, fs, Ap, An, Ae, As, Aw, MaxU, Dis;
        protected double dx;
        protected double dy;
        public RiverSectionaVarQuad(RiverStreamQuadParams p) : base(p)
        {
            Init();
        }
        /// <summary>
        /// Инициализация 
        /// </summary>
        protected void Init()
        {
            // геометрия дна
            Geometry = new DigFunction();
            // свободная поверхность
            WaterLevels = new DigFunction();
            // расход потока
            flowRate = new DigFunction();
            evolution.Clear();
        }

        #region IRiver
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

        /// <summary>
        /// Наименование задачи
        /// </summary>
        public virtual string Name { get => "Расчет потока в створе русла"; }
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamY1D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "River2D 19.05.2025"; 
        /// <summary>
        /// граничные условия для поля скорости
        /// </summary>
        protected IBoundaryConditions BoundConditionQuadU = null;
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => BoundConditionQuadU;
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики при загрузке
        /// по умолчанию
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames();
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRivCrossQuadParams.txt";
            //fn.NameRData = "NameCrossRData.txt";
            fn.NameRData = "NameCrossRDataTest1.txt";

            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }

        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh() { return new TwoMesh(bottom_x, Zeta); }
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh() => mesh.Clone();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Cкорость потока", null, TypeFunForm.Form_2D_Rectangle_L1) };
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {
            int flagErr = 0;
            try
            {
                // Параметры расчетной области
                CreateCalkArea();
                flagErr++;
                // Расчет вязкости и плотности
                SetMuAndRho();                     
                flagErr++;
                // Расчет скоростей
                VelocityCalculation();             
                flagErr++;
                //// Расчет напряжений в узлах МЦР                                     
                CalkTau();
                flagErr++;
                //// Расчет гидростатических скоростей
                // CalkHydroStaticVelosity();
                time += dtime;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName)
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
            // геометрия дна
            Geometry.Load(file);
            // свободная поверхность
            WaterLevels.Load(file);
            // расход потока
            flowRate.Load(file);
            // инициализация задачи
            InitTask();
        }
        /// <summary>
        /// Установка объектоа КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public void Set(IMesh mesh, IAlgebra algebra = null)
        {

        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            Zeta = zeta;
            // FlagStartMesh - первая генерация сетки
            // flagBLoad - генерация сетки при размывах дна
            if (FlagStartMesh == false)// || flagBLoad == true)
            {
                SetDataForRiverStream(ref right, ref left);
                FlagStartMesh = true;
            }
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            zeta = Zeta;

        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public virtual IRiver Clone()
        {
            return new RiverSectionaVarQuad(this);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public virtual void AddMeshPolesForGraphics(ISavePoint sp)
        {
            mesh.GetValueTo1D(mU, ref U);
            sp.Add("U", U);
            mesh.GetValueTo1D(mMu, ref Mu);
            sp.Add("Mu", Mu);
            mesh.GetValueTo1D(mMu, ref Mu);
            sp.Add("Mu0", Mu);

            mesh.GetValueTo1D(mTauX, ref TauX);
            sp.Add("TauX", TauX);
            mesh.GetValueTo1D(mTauY, ref TauY);
            sp.Add("TauY", TauY);
            // векторное поле на сетке
            sp.Add("Tau", TauX, TauY);


            mesh.GetValueTo1D(mTauX1, ref TauX);
            sp.Add("TauX1", TauX);
            mesh.GetValueTo1D(mTauY1, ref TauY);
            sp.Add("TauY1", TauY);
            // векторное поле на сетке
            sp.Add("Tau1", TauX, TauY);

            mesh.GetValueTo1D(mTauX2, ref TauX);
            sp.Add("TauX2", TauX);
            mesh.GetValueTo1D(mTauY2, ref TauY);
            sp.Add("TauY2", TauY);
            // векторное поле на сетке
            sp.Add("Tau2", TauX, TauY);

            mesh.GetValueTo1D(mRho, ref TauY);
            sp.Add("mRho", TauY);

            mesh.GetValueTo1D(mTauX3, ref TauX);
            sp.Add("TauX3", TauX);
            mesh.GetValueTo1D(mTauY3, ref TauY);
            sp.Add("TauY3", TauY);
            // векторное поле на сетке
            sp.Add("Tau3", TauX, TauY);
            // фильтрация напряжений - только вода
            mesh.GetValueTo1D(mTauX, ref TauX);
            sp.Add("sTauX", TauX);
            mesh.GetValueTo1D(mTauY, ref TauY);
            sp.Add("sTauY", TauY);
            // векторное поле на сетке
            sp.Add("sTau", TauX, TauY);

            GetBTau();
            sp.AddCurve("Касательные придонные напряжения", mesh.X[0], bNormalTau);
            sp.AddCurve("Касательные напряжения гидростатика", mesh.X[0], bNormalTauGS);
            sp.AddCurve("Придонное давление", mesh.X[0], bPress);
            sp.AddCurve("Донных профиль", mesh.X[0], Zeta);
        }

        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            MEM.Alloc(mesh.CountKnots, ref tauX);
            MEM.Alloc(mesh.CountKnots, ref P);
            tauY = null;
            GetBTau();
            for (uint j = 0; j < Ny; j++)
            {
                tauX[j] = bNormalTau[j];
                P[j] = bPress[j];
            }
        }
        #endregion
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            Set((RiverStreamQuadParams)p);
            InitTask();
        }

        /// <summary>
        /// Инициализация задачи
        /// </summary>
        public void InitTask()
        {
            // получение отметок дна
            Geometry.GetFunctionData(ref bottom_x, ref Zeta, Nx);
            // начальный уровень свободной поверхности
            waterLevel = WaterLevels.FunctionValue(0);
            // начальный расход потока
            riverFlowRate = flowRate.FunctionValue(0);
            // генерация сетки в области
            SetDataForRiverStream(ref right, ref left);
            FlagStartMesh = true;
        }

        /// <summary>
        /// Установка параметров задачи
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="mesh"></param>
        public virtual void SetDataForRiverStream(ref HKnot right, ref HKnot left)
        {
            

            MEM.Alloc(Nx, Ny, ref mTauX, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTau, "Tau");

            MEM.Alloc(Nx, Ny, ref mTauX1, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY1, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTauX2, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY2, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTauX3, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY3, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTau1, "Tau");


            MEM.Alloc(Nx, Ny, ref mRho, "mRho");
            MEM.Alloc(Nx, Ny, ref mMu, "mMu");
            MEM.Alloc(Nx, Ny, ref mU, "mU");

            MEM.Alloc(Nx, Ny, ref aP, "aP");
            MEM.Alloc(Nx, Ny, ref aW, "aW");
            MEM.Alloc(Nx, Ny, ref aE, "aE");
            MEM.Alloc(Nx, Ny, ref aS, "aS");
            MEM.Alloc(Nx, Ny, ref aN, "aN");
            MEM.Alloc(Nx, Ny, ref aQ, "aQ");

            MEM.Alloc(Nx, ref Depth, "Depth");
            MEM.Alloc(Nx, ref U_star, "U_star");
            MEM.Alloc(Nx, ref MuMax, "MuMax");
            MEM.Alloc(Nx, ref bNormalTau, "bNormalTau");
            MEM.Alloc(Nx, ref bNormalTauGS, "bNormalTauGS");
            MEM.Alloc(Nx, ref bPress, "bPress");
            MEM.Alloc(Nx, ref bottonIndex, "bottonIndex");
            MEM.Alloc(Nx, ref bottonMarkers, "bottonMarkers");

            CreateCalkArea();
        }
        #region Вспомогательные функции
        /// <summary>
        /// Вычисление границ расчетной области и создание 
        /// сетки для рендеренга данных
        /// </summary>
        protected void CreateCalkArea()
        {
            // максимальная глубина - самая глубокая отметка
            double maxDepth = waterLevel - Zeta.Min();   // #2
            //нижняя координата расчетной области
            double Ymin = waterLevel - maxDepth;
            double Ly = maxDepth;
            // шаг сетки по вертикали
            dy = Ly / (Ny - 1);
            double Lx = Geometry.Length;
            double Xmin = Geometry.Xmin;
            // шаг сетки по горизонтали
            dx = Lx / (Nx - 1);
            //   *------------ 4 ------------*
            //   |                           |
            //   5                           | 
            //   |                           |
            //   *---- 0 ----*               3
            //                \              |
            //                 1             | 
            //                  \            |
            //                   *---- 2 ----*
            int[] BCMark = new int[] { 0, 1, 2, 3, 4, 5 };
            TypeBoundCond[] BCType = new TypeBoundCond[]
            {
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann,      
                TypeBoundCond.Dirichlet     
            };



            //mesh = new RectangleMesh(Lx, Ly, Nx, Ny, BCMark, BCType, Xmin, Ymin);
            mesh = new ChannelRectangleMesh(Nx, Ny, Lx, Ly);
            mesh.CreateMesh(BCMark);

            BoundConditionQuadU = new BoundaryConditionsQuad();


            // получение отметок дна
            Geometry.GetFunctionData(ref bottom_x, ref Zeta, Nx);
            
            Geometry.GetMark(bottom_x, ref bottonMarkers);
        }
        /// <summary>
        /// Определяем значения вязкости и плотности в узлах расчетной области, 
        /// исходя из расположения донных отметок   
        /// </summary>
        public virtual void SetMuAndRho() // #3
        {
            for (int i = 0; i < Zeta.Length; i++)
            {
                Depth[i] = waterLevel - Zeta[i];
                if (Depth[i] > 0)
                    U_star[i] = Math.Sqrt(g * J * Depth[i]);
                else
                    U_star[i] = 0;
            }
            switch (ViscosityModel)
            {
                case TurbulentViscosityQuadModelOld.ViscosityConst:
                    {
                        for (int i = 0; i < Zeta.Length; i++)
                            MuMax[i] = 0;
                        Mu_w = 0.21185;
                        CalkMuAndRho();
                    }
                    break;
                case TurbulentViscosityQuadModelOld.ViscosityWolfgangRodi:
                    {
                        for (int i = 0; i < Zeta.Length; i++)
                            MuMax[i] = rho_w * Depth[i] * U_star[i] * Rcoef;
                        CalkMuAndRho();
                    }
                    break;
                case TurbulentViscosityQuadModelOld.Viscosity2DXY:
                    {
                        for (int i = 0; i < Zeta.Length; i++)
                            MuMax[i] = 0;
                        CalkMuAndRho();
                        double Qwater = GetQwater();
                        double Fmu = CalkFmu(Qwater);
                        SolverMu(Fmu, ref mMu);
                    }
                    break;
            }
        }
        /// <summary>
        /// Полный расход по сечению
        /// </summary>
        /// <returns></returns>
        public double GetQwater()
        {
            int i, j;
            double Qwater = 0;
            double dV = mesh.dx * mesh.dy;
            for (i = 0; i < Nx - 1; i++)
                for (j = bottonIndex[i]; j < Ny - 1; j++)
                {
                    Qwater += 0.25 * (mU[i][j] + mU[i + 1][j] + mU[i][j + 1] + mU[i + 1][j + 1]) * dV;
                }
            return Qwater;
        }
        /// <summary>
        /// Расчет правой части
        /// </summary>
        /// <param name="Qwater"></param>
        /// <returns></returns>
        public double CalkFmu(double Qwater)
        {
            double alphaQ = 0.32828;
            double kappa = 0.41;
            double DepthMax = Depth.Max();
            Fmu = 2 * alphaQ * rho_w * kappa * Math.Sqrt(g * J / DepthMax);
            if (Flag_Q_Mu == true)
            {
                double dQ = (Qwater - Qwater0) / Qwater0;
                Fmu = Fmu * (1 + dtime * dQ);
            }
            return Fmu;
        }
        public void SolverMu(double Fmu, ref double[][] mMu)
        {
            algebraMu = new AlgebraQuadSolver(mesh, BCTypeMu, bcMu);

            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    if (mesh.Y[i][j] < Zeta[i])
                        mMu[i][j] = MuS;
                    else
                        mMu[i][j] = 1;

            algebraMu.CalkAk(mMu, 1, 1);
            algebraMu.CalkRight(Fmu);
            algebraMu.SystemSolver(ref mMu);

            //for (uint i = 0; i < Nx; i++)
            //{
            //    double xm = mesh.Xmin + i * dx;
            //    for (int j = 0; j < Ny; j++)
            //    {
            //        double ym = mesh.Ymin + dy * j;
            //        if (ym + dy / 2 < Zeta[i])
            //            mMu[i][j] = MuS;
            //    }
            //}
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    if (mesh.Y[i][j] < Zeta[i])
                        mMu[i][j] = MuS;
        }

        public void CalkMuAndRho()
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    double ym = mesh.Ymin + dy * j;
                    if (ym < Zeta[i])
                        bottonIndex[i] = j;
                }

            for (uint i = 0; i < Nx; i++)
            {
                double Depth2 = Depth[i] * Depth[i] / 4;
                for (int j = bottonIndex[i]; j < Ny; j++)
                {
                    double y = mesh.Ymin + dy * j - Zeta[i]; ;
                    mMu[i][j] = (Mu_w + MuMax[i] * ((Depth[i] - y) * y) / Depth2);
                    mRho[i][j] = rho_w;
                }
            }
        }

        /// <summary>
        /// Рассчитываем значения скоростей во внутренних узлах расчетной области методом Зейделя
        /// #5 Определяем граничные значения скоростей из граничных условий задачи
        /// #6 Находим максимальную невязку (разницу значений на текущей и предыдущей итерации в одном узле) во внутренних узлах расчетной сетки
        /// #7 Находим относительную невязку, частное модулей максимальной невязки и максимального значения в узлах
        /// #8 Если относительная невязка меньше значения погрешности - решение найдено, заканчиваем вычисление. Если больше – возврат к пункту 4
        /// </summary>
        
        public virtual void VelocityCalculation() 
        {
            algebra = new AlgebraQuadSolver(mesh, BCType, bc);
            algebra.CalkAk(mMu, 1, 1);
            algebra.CalkRight(mRho, g * J);
            algebra.SystemSolver(ref mU);
        }
        
        /// <summary>
        /// Расчет напряжений в узлах МЦР
        /// </summary>
        public void CalkTau()
        {
            int i, j;

            for (i = 1; i < Nx; i++)
                for (j = 1; j < Ny; j++)
                {
                    mTauX[i][j] = mMu[i][j] * (mU[i][j] - mU[i - 1][j]) / dx;
                    mTauY[i][j] = mMu[i][j] * (mU[i][j] - mU[i][j - 1]) / dy;
                }

            for (i = 1; i < Nx - 1; i++)
                for (j = 1; j < Ny - 1; j++)
                {
                    mTauX1[i][j] = mMu[i][j] * ((mU[i + 1][j] - mU[i - 1][j]) / (dx * 2));
                    mTauY1[i][j] = mMu[i][j] * ((mU[i][j + 1] - mU[i][j - 1]) / (dy * 2));
                }

            for (i = 1; i < Nx; i++)
                for (j = 1; j < Ny; j++)
                {
                    mTauX2[i][j] = 0.5 * (mMu[i][j] + mMu[i - 1][j]) * ((mU[i][j] - mU[i - 1][j]) / dx);
                    mTauY2[i][j] = 0.5 * (mMu[i][j] + mMu[i][j - 1]) * ((mU[i][j] - mU[i][j - 1]) / dy);
                }

            for (i = 1; i < Nx - 1; i++)
                for (j = 1; j < Ny - 1; j++)
                {
                    //mTauX3[i][j] = (mMu[i + 1][j] + mMu[i][j] + mMu[i - 1][j]) / 3.0 * (mU[i + 1][j] - mU[i - 1][j]) / dx;
                    if (MEM.Equals(mMu[i][j], mMu[i - 1][j]) == false)
                        mTauX3[i][j] = mMu[i][j] * (mU[i][j] - mU[i - 1][j]) / dx;
                    else
                        mTauX3[i][j] = 0.5 * (mMu[i][j] + mMu[i - 1][j]) * ((mU[i][j] - mU[i - 1][j]) / dx);
                    mTauY3[i][j] = 0.5 * (mMu[i][j] + mMu[i][j - 1]) * ((mU[i][j] - mU[i][j - 1]) / dy);
                }


            // 
            //for (i = NxMin - 2; i < Nx - 4; i++)
            //{
            //    for (j = 4; j < Ny - 4; j++)
            //    {
            //        if (MEM.Equals(mMu[i][j], mMu[i + 1][j]) == false)
            //        {
            //            double dT = (mTauX2[i + 4][j] - mTauX2[i - 3][j]) / 7;
            //            mTauX2[i - 2][j] = mTauX2[i - 3][j] + dT;
            //            mTauX2[i - 1][j] = mTauX2[i - 3][j] + 2 * dT;
            //            mTauX2[i + 0][j] = mTauX2[i - 3][j] + 3 * dT;
            //            mTauX2[i + 1][j] = mTauX2[i - 3][j] + 4 * dT;
            //            mTauX2[i + 2][j] = mTauX2[i - 3][j] + 5 * dT;
            //            mTauX2[i + 3][j] = mTauX2[i - 3][j] + 6 * dT;
            //        }
            //        if (MEM.Equals(mMu[i][j], mMu[i][j + 1]) == false)
            //        {
            //            double dT = (mTauX2[i][j + 4] - mTauX2[i][j - 3]) / 7;
            //            mTauX2[i][j - 2] = mTauX2[i][j - 3] + dT;
            //            mTauX2[i][j - 1] = mTauX2[i][j - 3] + 2 * dT;
            //            mTauX2[i][j - 0] = mTauX2[i][j - 3] + 3 * dT;
            //            mTauX2[i][j + 1] = mTauX2[i][j - 3] + 4 * dT;
            //            mTauX2[i][j + 2] = mTauX2[i][j - 3] + 5 * dT;
            //            mTauX2[i][j + 3] = mTauX2[i][j - 3] + 6 * dT;
            //        }
            //    }
            //}

            //// КЭ 1 порядка
            //for (i = NxMin; i < Nx-1; i++)
            //{
            //    for (j = 1; j < Ny-1; j++)
            //    {
            //        double mu0 = mMu[i-1][j-1] + mMu[i][j-1] + mMu[i][j] + mMu[i-1][j];
            //        double mu1 = mMu[i][j-1] + mMu[i+1][j - 1] + mMu[i+1][j] + mMu[i][j];
            //        double mu2 = mMu[i][j] + mMu[i+1][j] + mMu[i+1][j+1] + mMu[i][j+1];
            //        double mu3 = mMu[i - 1][j] + mMu[i][j] + mMu[i][j+1] + mMu[i - 1][j+1];
            //        //double u0 = mU[i - 1][j - 1];
            //        double u1 = mU[i][j - 1];
            //        double u2 = mU[i][j];
            //        double u3 = mU[i - 1][j];
            //        //double u4 = mU[i + 1][j - 1];
            //        double u5 = mU[i + 1][j];
            //        //double u6 = mU[i + 1][j + 1];
            //        double u7 = mU[i][j + 1];
            //        //double u8 = mU[i - 1][j + 1];

            //        mTauX3[i][j] = 0.25 * ((mu0 - mu1 - mu2 + mu3) * u2 + (-mu0 * u3 + mu1 * u5 + mu2 * u5 - mu3 * u3)) / dx;
            //        mTauY3[i][j] = 0.25 * ((mu0 + mu1 - mu2 - mu3) * u2 + (-mu0 * u1 - mu1 * u1 + mu2 * u7 + mu3 * u7)) / dy;
            //    }
            //}

        }
        /// <summary>
        /// #9 Выполняем расчет придонных касательных напряжений на e - х интервалах
        /// </summary>
        /// <param name="MainTau"></param>
        public void GetBTau()
        {
            double dy = mesh.dy;
            double dx = mesh.dx;
            for (int i = 1; i < Nx; i++)
            {
                //eta = waterLevel;
                double xC = dx * (i + 0.5f);
                double zetaC = 0.5 * (Zeta[i] + Zeta[i-1]);
                double dzeta = Zeta[i] - Zeta[i - 1];
                if (zetaC + dy/2  < waterLevel)
                {
                    float sigma =(float)Math.Min(dx, dy);
                    // вектор касательный к дну
                    Vector2 S = new Vector2(dx, dzeta);
                    // вектор нормальный к дну
                    Vector2 Ns = new Vector2(-S.Y, S.X);
                    // вектор - единичная нормаль к дну
                    Vector2 Normals = Vector2.Normalize(Ns);
                    // радиус вектор центра интервала дна
                    Vector2 C = new Vector2(xC, (float)zetaC);
                    // радиус вектор центра придонной точки
                    Vector2 P = C + sigma * Normals;
                    // Скорость в центре донного интервала C
                    double Ucc = mesh.CalkCeilValue(ref C, mU);
                    // Скорость в придонной точке P
                    double Upp = mesh.CalkCeilValue(ref P, mU);
                    // Приведенная вязкость
                    double Mu_p = mesh.CalkCeilValue(ref P, mMu);
                    // Приведенная вязкость
                    double rho_p = mesh.CalkCeilValue(ref P, mMu);
                    // Нормальное придонное касательное напряжение
                    //bNormalTau[i] = Mu_p * (Upp - Ucc) / sigma;
                    bNormalTau[i] = Mu_w * (Upp - Ucc) / sigma;
                    bPress[i] = g * rho_p * (Depth[i] + Depth[i-1]) / 2;
                    // гидростатическое придонное касательное напряжение
                    bNormalTauGS[i] = bPress[i] * J;
                }
                else
                {
                    bNormalTau[i] = 0;
                    bPress[i] = 0;
                    bNormalTauGS[i] = 0;
                }
           }
        }
        #endregion
    }
}
