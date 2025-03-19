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
    using System.Threading.Tasks;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using MeshLib;
    using RiverLib.IO;
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
    ///  ОО: Определение класса RiverSectionalQuad_SV - для расчета поля скорости
    ///  в створе речного потока
    ///  
    /// </summary>
    [Serializable]
    public class RiverSectionalQuad_SV : RiverStreamQuadParams, IRiver
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

        /// <summary>
        /// Граничные условия для поля скорости по оси Х
        /// </summary>
        TypeBoundCond[] BCType = {     TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Neumann };
        /// <summary>
        /// Граничные условия для поля функции тока
        /// </summary>
        TypeBoundCond[] BCTypeStream = {     TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Neumann };
        /// <summary>
        /// Граничные условия для поля вихря
        /// </summary>
        TypeBoundCond[] BCTypeVortex = {   TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann };

        /// <summary>
        /// Граничные условия для поля вязкости
        /// </summary>
        TypeBoundCond[] BCTypeMu = {   TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann,
                                       TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Neumann };
        /// <summary>
        /// кравевые условия для поля функции тока
        /// </summary>
        BoundaryConditionsQuad bcStream = new BoundaryConditionsQuad();
        /// <summary>
        /// кравевые условия для поля вихря
        /// </summary>
        BoundaryConditionsQuad bcVortex = new BoundaryConditionsQuad();
        /// <summary>
        /// однородные кравевые для поля вязкости
        /// </summary>
        BoundaryConditionsQuad bc = new BoundaryConditionsQuad();
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
        //protected double MuW = 1;

        /// <summary>
        /// правая часть уравнения
        /// </summary>
        protected double Q;
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
        /// Поле скоростей
        /// </summary>
        public double[] V;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[] W;
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
        /// массив для функции вихря
        /// </summary>
        public double[][] mWater;
        /// <summary>
        /// массив для функции вихря
        /// </summary>
        public double[][] mVortex;
        /// <summary>
        /// массив для функции тока
        /// </summary>
        public double[][] mPhi;
        /// <summary>
        /// массив для горизонтальной скорости
        /// </summary>
        public double[][] mU;
        /// <summary>
        /// массив для горизонтальной скорости
        /// </summary>
        public double[][] mW;
        /// <summary>
        /// массив для вертикальной скорости
        /// </summary>
        public double[][] mV;
        /// <summary>
        /// Буфферный массив для релаксации функции тока
        /// </summary>
        public double[][] mRelaxPhi;
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
        /// Поле напряжений - модуль
        /// </summary>
        public double[][] mTau1;
        /// <summary>
        /// Источник для расчета вязкости
        /// </summary>
        double Fmu = 0;

        #endregion
        public double[][] aP, aE, aW, aN, aS, aQ;
        protected double uBuff, bap, fe, fw, fn, fs, Ap, An, Ae, As, Aw, MaxU, Dis;
        protected int NxMin = 1, NyMin = 1;
        protected double dx;
        protected double dy;
        public RiverSectionalQuad_SV(RiverStreamQuadParams p) : base(p)
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
        public virtual string Name { get => "Расчет полного потока в створе русла"; }
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
            fn.NameRData = "NameCrossRData.txt";
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
                // Расчет скоростей в створе
                VelocityCalculation_YZ();
                // Расчет продольной скорости
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
            return new RiverSectionalQuad_SV(this);
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
            mesh.GetValueTo1D(mV, ref U);
            sp.Add("V", U);
            mesh.GetValueTo1D(mW, ref U);
            sp.Add("W", U);

            mesh.GetValueTo1D(mPhi, ref U);
            sp.Add("mPhi", U);
            mesh.GetValueTo1D(mVortex, ref U);
            sp.Add("mVortex", U);

            mesh.GetValueTo1D(mMu, ref Mu);
            sp.Add("Mu", Mu);

            mesh.GetValueTo1D(mWater, ref Mu);
            sp.Add("Water", Mu);


            mesh.GetValueTo1D(mTauX, ref TauX);
            sp.Add("TauX", TauX);
            mesh.GetValueTo1D(mTauY, ref TauY);
            sp.Add("TauY", TauY);
            // векторное поле на сетке
            sp.Add("Tau", TauX, TauY);
            mesh.GetValueTo1D(mRho, ref TauY);
            sp.Add("mRho", TauY);
            // фильтрация напряжений - только вода
            for (uint i = 0; i < Nx; i++)
                for (int ix = 0; ix < Ny; ix++)
                    if (mesh.Y[i][ix] < Zeta[i])
                    {
                        mTauX[i][ix] = 0;
                        mTauY[i][ix] = 0;
                    }
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
            for (uint ix = 0; ix < Ny; ix++)
            {
                tauX[ix] = bNormalTau[ix];
                P[ix] = bPress[ix];
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
            Q = rho_w * g * J;
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
            CreateCalkArea();

            MEM.Alloc(Nx, Ny, ref mTauX, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTau, "Tau");

            MEM.Alloc(Nx, Ny, ref mTauX1, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY1, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTau1, "Tau");

            

            MEM.Alloc(Nx, Ny, ref mRho, "mRho");
            MEM.Alloc(Nx, Ny, ref mMu, "mMu");
            MEM.Alloc(Nx, Ny, ref mU, "mU");
            MEM.Alloc(Nx, Ny, ref mV, "mV");
            MEM.Alloc(Nx, Ny, ref mW, "mW");
            MEM.Alloc(Nx, Ny, ref mPhi, "mPhi");
            MEM.Alloc(Nx, Ny, ref mVortex, "mVortex");
            MEM.Alloc(Nx, Ny, ref mWater, "mWater");
            
            MEM.Alloc(Nx, Ny, ref mRelaxPhi, "mRelaxPhi");

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
            double Ymin = waterLevel - (1 + KsDepth) * maxDepth;
            double Ly = waterLevel - Ymin;
            // шаг сетки по вертикали
            dy = Ly / (Ny - 1);
            double Lx = Geometry.Length;
            double Xmin = Geometry.Xmin;
            // шаг сетки по горизонтали
            dx = Lx / (Nx - 1);
            //   *---- 2 ----*
            //   |           |
            //   3           1 
            //   |           |
            //   *---- 0 ----*
            int[] BCMark = new int[] { 0, 1, 2, 3 };
            TypeBoundCond[] BCType = new TypeBoundCond[]
            {
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann,
                TypeBoundCond.Neumann,
                TypeBoundCond.Dirichlet
            };
            //mesh = new RectangleMesh(Lx, Ly, Nx, Ny, BCMark, BCType, Xmin, Ymin);
            mesh = new ChannelRectangleMesh(Nx, Ny, Lx, Ly);
            mesh.CreateMesh(BCMark);

            BoundConditionQuadU = new BoundaryConditionsQuad();
            NxMin = 0;
            for (int ix = 0; ix < Nx - 1; ix++)
                if (Zeta[ix] > waterLevel && Zeta[ix + 1] <= waterLevel)
                {
                    NxMin = ix;
                    break;
                }
            NxMin -= 4;
            NxMin = NxMin < 1 ? 1 : NxMin;
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
            for (i = NxMin; i < Nx - 1; i++)
                for (j = 1; j < Ny - 1; j++)
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

            double DepthMax = Depth.Max();
            Fmu = 2 * rho_w * Math.Sqrt(g * J / DepthMax);
            if (Flag_Q_Mu == true)
            {
                double dQ = (Qwater - Qwater0) / Qwater0;
                Fmu = Fmu * (1 + dtime * dQ);
            }
            return Fmu;
        }
        public void SolverMu(double Fmu, ref double[][] mMu)
        {
            algebraMu = new AlgebraQuadSolver(mesh, BCTypeMu, bc);
            algebraMu.CalkAk(mMu, NxMin, NyMin);
            algebraMu.CalkRight(Fmu);
            algebraMu.SystemSolver(ref mMu);
            for (uint i = 0; i < Nx; i++)
            {
                double xm = mesh.Xmin + i * dx;
                for (int ix = 0; ix < Ny; ix++)
                {
                    double ym = mesh.Ymin + dy * ix;
                    if (ym + dy / 2 < Zeta[i])
                        mMu[i][ix] = MuS;
                }
            }
        }

        public void CalkMuAndRho()
        {
            for (uint i = 0; i < Nx; i++)
            {
                double xm = mesh.Xmin + i * dx;
                double Depth2 = Depth[i] * Depth[i] / 4;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = mesh.Ymin + dy * j;
                    if (ym + dy / 2 >= Zeta[i])
                    {
                        double y = ym - Zeta[i];
                        mMu[i][j] = (Mu_w + MuMax[i] * ((Depth[i] - y) * y) / Depth2);
                        mRho[i][j] = rho_w;
                        mWater[i][j] = mMu[i][j];
                    }
                    else
                    {
                        mMu[i][j] = MuS;
                        mRho[i][j] = epsilon * rho_w + (1 - epsilon) * rho_s;
                        mWater[i][j] = Mu_w;
                    }
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
            algebra.CalkAk(mMu, NxMin, NyMin);
            algebra.CalkRight(mRho, g * J);
            algebra.SystemSolver(ref mU);
        }
        /// <summary>
        /// Решение задачи гидрадинамики в постановке вихрь - функция тока
        /// </summary>
        public virtual int VelocityCalculation_YZ()
        {
            bool Stoks = false;
            /// <summary>
            /// максимальная текущая относительная ошибка
            /// </summary>
            double maxerr = 0;

            double errorPhi = 0;
            double errorVertex = 0;
            /// <summary>
            /// максимальное по модулю значение поля
            /// </summary>
            double maxValue = 0;
            double err = 0.00001;
            double error = 0;
            double Tau = 1;
            double Mu = 0.1;
            /// <summary>
            /// Параметр релаксации потока
            /// </summary>
            double alfa = 0.025;
            //algebra = new AlgebraQuadSolver(mesh, BCType, bc);
            //algebra.CalkAk(mMu, NxMin, NyMin);
            //algebra.CalkRight(mRho, g * J);
            //algebra.SystemSolver(ref mU);
            //bool flagFor;
            //верхняя грань
            //for (int i = 0; i < LengthX; i++)
            //mVortex[0][i] = - Tau / Mu;
            for (int i = NxMin; i < Nx; i++)
                mVortex[i][Ny - 1] = -Tau / Mu;

            double dx2 = dx * dx;
            double dx4 = dx2 * dx2;
            double dy2 = dy * dy;
            double dy4 = dy2 * dy2;
            double dxy4 = dx2 * dy2;
            double dx2_dy2 = 2 * (dx2 + dy2);

            //int Counts = 1000000;
            int Index = 0;
            //double oerr = 0;
            //int taskID = 0;
            // Цикл по сходимости
            for (Index = 0; Index < 500000; Index++)
            {
                if (Index > 0)
                {
                    #region  расчет функции тока Фи по центрально-разностной схеме
                    for (int CountPhi = 0; CountPhi < 100000; CountPhi++)
                    {
                        maxValue = 0;
                        maxerr = 0;
                        for (int i = NxMin; i < Nx - 1; i++)
                        {
                            for (int j = 1; j < Ny - 1; j++)
                            {
                                double fn = mPhi[i][j + 1];
                                double fs = mPhi[i][j - 1];
                                double fe = mPhi[i + 1][j];
                                double fw = mPhi[i - 1][j];
                                double S = mPhi[i][j];
                                mPhi[i][j] = ((fe + fw) * dy2 + (fn + fs) * dx2 + mVortex[i][j] * dx2 * dy2) / dx2_dy2;
                                error = Math.Abs(mPhi[i][j] - S);
                                if (maxValue < Math.Abs(mPhi[i][j])) 
                                    maxValue = Math.Abs(mPhi[i][j]);
                                if (error > maxerr) 
                                    maxerr = error;
                            }
                        }
                        errorPhi = maxerr / (maxValue + err);
                        if (errorPhi < MEM.Error4)
                            break;
                    }
                    #endregion
                    #region Релаксация функции тока, расчет полей скорости и определение вихря на границах области

                    maxerr = 0;
                    int im = 0, jm = 0;
                    for (int i = NxMin; i < Nx; i++)
                        for (int j = 0; j < Ny; j++)
                        {
                            error = Math.Abs(mPhi[i][j] - mRelaxPhi[i][j]);
                            if (error > maxerr)
                            {
                                maxerr = error; im = i; jm = j;
                            }
                            mPhi[i][j] = (1 - alfa) * mRelaxPhi[i][j] + alfa * mPhi[i][j];
                            mRelaxPhi[i][j] = mPhi[i][j];
                        }
                    // Процесс сошелся
                    errorPhi = maxerr / (maxValue + err);
                    if (errorPhi < MEM.Error4 && (Index > 2))
                        return Index;
                    // расчет скоростей 
                    CalkVelocity();
                    // учет граничных условий
                    for (int j = 1; j < Ny; j++)
                    {
                        mVortex[NxMin-1][j] = (mW[NxMin][j] - mW[NxMin-1][j]) / dx;  // левая
                        mVortex[Nx - 1][j] = (mW[Nx - 1][j] - mW[Nx - 2][j]) / dx; // правая
                        //mVortex[Nx - 1][j] = 0; // правая
                    }
                    for (int i = NxMin; i < Nx; i++)
                    {
                        // mVortex[0, i] = - Tau / Mu;  //верхняя
                        mVortex[i][Ny - 1] = - (mV[i][Ny - 1] - mV[i][Ny - 2]) / dy;  //нижняя
                    }
                    #endregion
                }
                #region расчет функции вихря в области
                for (;;)
                {
                    maxerr = 0;
                    maxValue = 0;

                    for (int i = NxMin; i < Nx - 1; i++)
                    {
                        for (int j = 1; j < Ny - 1; j++)
                        {
                            double D_e = 0.5 * (mWater[i + 1][j] + mWater[i][j]) * dy / dx;
                            double D_w = 0.5 * (mWater[i - 1][j] + mWater[i][j]) * dy / dx;
                            double D_n = 0.5 * (mWater[i][j + 1] + mWater[i][j]) * dx / dy;
                            double D_s = 0.5 * (mWater[i][j - 1] + mWater[i][j]) * dx / dy;

                            double u_P = mV[i][j];
                            double u_E = mV[i + 1][j];
                            double u_W = mV[i - 1][j];

                            double v_P = mW[i][j];
                            double v_N = mW[i][j + 1];
                            double v_S = mW[i][j - 1];

                            double u_w = 0.5 * (u_W + u_P);
                            double u_e = 0.5 * (u_E + u_P);
                            double v_n = 0.5 * (v_N + v_P);
                            double v_s = 0.5 * (v_S + v_P);

                            double F_e = 0.5 * (mRho[i + 1][j] + mRho[i][j]) * u_e * dy;
                            double F_w = 0.5 * (mRho[i - 1][j] + mRho[i][j]) * u_w * dy;
                            double F_n = 0.5 * (mRho[i][j + 1] + mRho[i][j]) * v_n * dx;
                            double F_s = 0.5 * (mRho[i][j - 1] + mRho[i][j]) * v_s * dx;

                            double P_e = F_e / D_e;
                            double P_w = F_w / D_w;
                            double P_n = F_n / D_n;
                            double P_s = F_s / D_s;

                            double a_E = D_e * A(P_e) + Math.Max(-F_e, 0);
                            double a_W = D_w * A(P_w) + Math.Max(F_w, 0);
                            double a_N = D_n * A(P_n) + Math.Max(-F_n, 0);
                            double a_S = D_s * A(P_s) + Math.Max(F_s, 0);

                            double a_P = a_E + a_W + a_N + a_S;

                            double W0 = mVortex[i][j];

                            mVortex[i][j] = (a_E * mVortex[i + 1][j] +
                                             a_W * mVortex[i - 1][j] +
                                             a_N * mVortex[i][j + 1] +
                                             a_S * mVortex[i][j - 1]) / a_P;
                            error = Math.Abs(mVortex[i][j] - W0);
                            if (maxValue < Math.Abs(mVortex[i][j]))
                                maxValue = Math.Abs(mVortex[i][j]);
                            if (Math.Abs(error) > maxerr)
                                maxerr = Math.Abs(error);
                        }
                    }
                    errorVertex = maxerr / (maxValue + err);
                    if (errorVertex < MEM.Error4)
                        break;
                }
                #endregion
            }
            return Index;
        }
        /// <summary>
        /// Степенная КО схема
        /// </summary>
        double A(double Vortex)
        {
            double D = 1 - 0.1 * Math.Abs(Vortex);
            double D5 = D * D * D * D * D;
            return Math.Max(0, D5);
        }
        /// <summary>
        /// расчет скоростей 
        /// </summary>
        public void CalkVelocity()
        {
            //расчет горизонтальных скоростей U
            for (int i = NxMin-1; i < Nx; i++)
                for (int j = 0; j < Ny - 1; j++)
                    mV[i][j + 1] = (mPhi[i][j+1] - mPhi[i][j]) / dy;
            //U[i + 1, j] = (Phi[i, j] - Phi[i + 1, j]) / dy;

            for (int i = NxMin; i < Nx - 1; i++)
            {
                // из граничного условия для вихря на поверхности
                mV[i][0] = 2 * mV[i][1] - mV[i][2];
                // Первый порядок для ГУ на дне
                mV[i][Ny - 1] = - mV[i][Ny - 2];
            }
            //расчет вертикальных скоростей
            for (int i = NxMin; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    mW[i][j] = - (mPhi[i][j] - mPhi[i-1][j]) / dx;
            // Первый порядок для ГУ на стенках
            for (int j = 0; j < Ny; j++)
            {
                mW[NxMin-1][j] = - mW[NxMin][j];
                mW[Nx-1][j] = -mW[Nx-2][j];
            }
        }
        /// <summary>
        /// Расчет напряжений в узлах МЦР
        /// </summary>
        public void CalkTau()
        {
            int i, ix;
            for (i = NxMin; i < Nx - 1; i++)
                for (ix = 1; ix < Ny - 1; ix++)
                {
                    mTauX1[i][ix] = mMu[i][ix] * ((mU[i + 1][ix] - mU[i - 1][ix]) / (dx * 2));
                    mTauY1[i][ix] = mMu[i][ix] * ((mU[i][ix + 1] - mU[i][ix - 1]) / (dy * 2));
                }

            for (i = NxMin; i < Nx; i++)
                for (ix = 1; ix < Ny; ix++)
                {
                    mTauX[i][ix] = mMu[i][ix] * (mU[i][ix] - mU[i - 1][ix]) / dx;
                    mTauY[i][ix] = mMu[i][ix] * (mU[i][ix] - mU[i][ix - 1]) / dy;
                }

            //for (i = NxMin; i < Nx; i++)
            //    for (ix = 1; ix < Ny; ix++)
            //    {
            //        mTauX[i][ix] = 0.5*( mMu[i][ix] + mMu[i-1][ix]) * ((mU[i][ix] - mU[i - 1][ix]) / (dx));
            //        mTauY[i][ix] = 0.5 * (mMu[i][ix] + mMu[i][ix-1]) * ((mU[i][ix] - mU[i][ix - 1]) / (dy));
            //    }
        }
        /// <summary>
        /// #9 Выполняем расчет придонных касательных напряжений на e - х интервалах
        /// </summary>
        /// <param name="MainTau"></param>
        public void GetBTau()
        {
            double dy = mesh.dy;
            double dx = mesh.dx;
            for (int ix = NxMin; ix < Nx; ix++)
            {
                //eta = waterLevel;
                double xC = dx * (ix + 0.5f);
                double zetaC = 0.5 * (Zeta[ix] + Zeta[ix - 1]);
                double dzeta = Zeta[ix] - Zeta[ix - 1];
                if (zetaC + dy / 2 < waterLevel)
                {
                    float sigma = (float)Math.Min(dx, dy);
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
                    //bNormalTau[ix] = Mu_p * (Upp - Ucc) / sigma;
                    bNormalTau[ix] = Mu_w * (Upp - Ucc) / sigma;
                    bPress[ix] = g * rho_p * (Depth[ix] + Depth[ix - 1]) / 2;
                    // гидростатическое придонное касательное напряжение
                    bNormalTauGS[ix] = bPress[ix] * J;
                }
                else
                {
                    bNormalTau[ix] = 0;
                    bPress[ix] = 0;
                    bNormalTauGS[ix] = 0;
                }
            }
        }
        #endregion
    }
}
