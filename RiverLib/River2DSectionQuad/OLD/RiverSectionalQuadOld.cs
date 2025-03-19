//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                  Расчет речного потока в створе русла
//---------------------------------------------------------------------------
//               кодировка  : 19.05.2023 Потапов И.И.
//---------------------------------------------------------------------------

namespace RiverLib.River2DSectionQuad.OLD
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
    using CommonLib.Delegate;



    /// <summary>
    ///  ОО: Определение класса RiverSectionalQuadOld - для расчета поля скорости
    ///  в створе речного потока
    ///  
    /// </summary>
    [Serializable]
    public class RiverSectionalQuadOld : RiverStreamQuadParams, IRiver
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
        protected int NxMin = 0;
        protected double dx;
        protected double dy;
        public RiverSectionalQuadOld(RiverStreamQuadParams p) : base(p)
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
        /// граничные условия
        /// </summary>
        protected IBoundaryConditions BoundCondition1D = null;
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => BoundCondition1D;
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
            return new RiverSectionalQuadOld(this);
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
            //this.bottom_x = fx;
            //this.Zeta = fy;
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

            //mesh = sg.CreateMesh(ref GR, waterLevel, bottom_x, Zeta);
            //IPointsA.SetInt(1 + (int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_L1);
            //IPointsB.SetInt(1 + (int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_L1);
            //right = sg.right;
            //left = sg.left;
            //// получение ширины ленты для алгебры
            //int WidthMatrix = (int)mesh.GetWidthMatrix();
            //// TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            //algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);
            //if (mu == null)
            //{
            //    MEM.Alloc(mesh.CountKnots, ref mu, "mu");
            //    MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
            //    MEM.Alloc(mesh.CountKnots, ref U, "U");
            //    MEM.Alloc(mesh.CountKnots, ref TauX, "TauX");
            //    MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            //    FlagStartMu = false;
            //    FlagStartRoughness = false;
            //}
            //if (mesh.CountKnots != mu.Length)
            //{
            //    double mMu = mu.Sum() / mu.Length;
            //    MEM.Alloc(mesh.CountKnots, ref mu, "mu");
            //    MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
            //    MEM.Alloc(mesh.CountKnots, ref U, "U");
            //    MEM.Alloc(mesh.CountKnots, ref TauX, "TauX");
            //    MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            //    switch (ViscosityModel)
            //    {
            //        case TurbulentViscosityModel.ViscosityConst:
            //            {
            //                MEM.MemSet(mu, mMu);
            //                break;
            //            }
            //        case TurbulentViscosityModel.ViscosityBlasius:
            //            {
            //                SetMuRoughness();
            //                break;
            //            }
            //        case TurbulentViscosityModel.ViscosityWolfgangRodi:
            //            {
            //                SetMuRoughnessWRodi();
            //                break;
            //            }
            //        case TurbulentViscosityModel.ViscosityPrandtlKarman:
            //            {
            //                SetMuDiff();
            //                break;
            //            }
            //        default:
            //            {
            //                MEM.MemSet(mu, mMu);
            //                break;
            //            }
            //    }
            //}
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
            if (waterLevel < Zeta[0])
                BCType[3] = TypeBoundCond.Neumann;
            else
                BCType[3] = TypeBoundCond.Dirichlet;
            if (waterLevel < Zeta[Zeta.Length - 1])
                BCType[1] = TypeBoundCond.Neumann;
            else
                BCType[1] = TypeBoundCond.Dirichlet;

            //mesh = new RectangleMesh(Lx, Ly, Nx, Ny, BCMark, BCType, Xmin, Ymin);
            mesh = new ChannelRectangleMesh(Nx, Ny, Lx, Ly);
            mesh.CreateMesh(BCMark);

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
            // установка штрафной функции 
            for (uint i = 0; i < Nx; i++)
            {
                double xm = mesh.Xmin + i * dx;
                double Depth2 = Depth[i] * Depth[i] / 4;
                for (int ix = 0; ix < Ny; ix++)
                {
                    double ym = mesh.Ymin + dy * ix;
                    if (ym + dy / 2 >= Zeta[i])
                    {
                        double y = ym - Zeta[i];
                        mMu[i][ix] = Mu_w;
                    }
                    else
                    {
                        mMu[i][ix] = 1000;
                    }
                }
            }
            SystemCalculation(ref mMu, WCoundaryCondition, WCalkCoeff);
            for (uint i = 0; i < Nx; i++)
            {
                double xm = mesh.Xmin + i * dx;
                double Depth2 = Depth[i] * Depth[i] / 4;
                for (int ix = 0; ix < Ny; ix++)
                {
                    double ym = mesh.Ymin + dy * ix;
                    if (ym + dy / 2 >= Zeta[i])
                    {
                        double y = ym - Zeta[i];
                        mRho[i][ix] = rho_w;
                    }
                    else
                    {
                        mMu[i][ix] = 1000;
                        mRho[i][ix] = epsilon * rho_w + (1 - epsilon) * rho_s;
                    }
                }
            }
        }
        public void WCalkCoeff()
        {
            // расчет коэффициентов КР
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = 1; j < Ny - 1; j++)
                {
                    Ap = mMu[i][j];
                    Ae = mMu[i + 1][j];
                    Aw = mMu[i - 1][j];
                    An = mMu[i][j + 1];
                    As = mMu[i][j - 1];
                    aE[i][j] = (Ae + Ap) / 2.0 / (dx * dx);
                    aW[i][j] = (Aw + Ap) / 2.0 / (dx * dx);
                    aS[i][j] = (As + Ap) / 2.0 / (dy * dy);
                    aN[i][j] = (An + Ap) / 2.0 / (dy * dy);
                    aP[i][j] = aE[i][j] + aW[i][j] + aN[i][j] + aS[i][j];
                    aQ[i][j] = Fmu;
                }
        }

        /// <summary>
        /// #5 Определяем граничные значения скоростей из граничных условий задачи   
        /// </summary>
        public void WCoundaryCondition(ref double[][] X) // #5
        {
            for (int ix = 0; ix < Nx; ix++)
            {
                // На  дне
                //if (mesh.BCType[0] == TypeBoundCond.Dirichlet)
                X[ix][0] = 0.001; // Дирихле 
                                  //else
                                  //    mU[ix][0] = mU[ix][1]; // Нейман и ...
                                  // На свободной поверхности потока
                                  //if (mesh.BCType[2] == TypeBoundCond.Dirichlet)
                                  //    mU[ix][Ny-1] = 0; // Дирихле 
                                  //else
                                  //mMu[ix][Ny - 1] = mMu[ix][Ny - 2];  // Нейман и ...
                X[ix][Ny - 1] = 0.001;  // Нейман и ...
            }
            for (int iy = 0; iy < Ny; iy++) // Нейман по бокам        
            {
                X[NxMin - 1][iy] = X[NxMin][iy];
                X[Nx - 1][iy] = X[Nx - 2][iy];
            }
        }



        public void CalkMuAndRho()
        {
            for (uint i = 0; i < Nx; i++)
            {
                double xm = mesh.Xmin + i * dx;
                double Depth2 = Depth[i] * Depth[i] / 4;
                for (int ix = 0; ix < Ny; ix++)
                {
                    double ym = mesh.Ymin + dy * ix;
                    if (ym + dy / 2 >= Zeta[i])
                    {
                        double y = ym - Zeta[i];
                        mMu[i][ix] = Mu_w + MuMax[i] * ((Depth[i] - y) * y) / Depth2;
                        mRho[i][ix] = rho_w;
                    }
                    else
                    {
                        mMu[i][ix] = MuS;
                        mRho[i][ix] = epsilon * rho_w + (1 - epsilon) * rho_s;
                    }
                }
            }
        }

        protected virtual void CalkAk()
        {
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = 1; j < Ny - 1; j++)
                {
                    Ap = mMu[i][j];
                    Ae = mMu[i + 1][j];
                    Aw = mMu[i - 1][j];
                    An = mMu[i][j + 1];
                    As = mMu[i][j - 1];
                    aE[i][j] = (Ae + Ap) / 2.0 / (dx * dx);
                    aW[i][j] = (Aw + Ap) / 2.0 / (dx * dx);
                    aS[i][j] = (As + Ap) / 2.0 / (dy * dy);
                    aN[i][j] = (An + Ap) / 2.0 / (dy * dy);
                    aP[i][j] = aE[i][j] + aW[i][j] + aN[i][j] + aS[i][j];
                    aQ[i][j] = mRho[i][j] * g * J;
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
            CalkAk();
            double RelDis = 1;
            int steps = 0;
            for (int index = 0; index < 100000; index++)
            {
                double MaxDis = 0;
                MaxU = 0;
                for (int i = NxMin; i < Nx - 1; i++) // -> X
                    for (int j = 1; j < Ny - 1; j++) // -> Y
                    {
                        uBuff = mU[i][j];
                        bap = aP[i][j];
                        fe = aE[i][j] * mU[i + 1][j];
                        fw = aW[i][j] * mU[i - 1][j];
                        fn = aN[i][j] * mU[i][j + 1];
                        fs = aS[i][j] * mU[i][j - 1];
                        mU[i][j] = (fe + fw + fn + fs + aQ[i][j]) / bap;
                        if (MaxDis < Math.Abs(uBuff - mU[i][j]))     // #6
                            MaxDis = Math.Abs(uBuff - mU[i][j]);
                        if (MaxU < Math.Abs(mU[i][j]))
                            MaxU = Math.Abs(mU[i][j]);
                    }
                // Определяем граничные значения скоростей 
                UBorders();                 // #5
                // Находим относительную невязку
                RelDis = MaxDis / MaxU;     // #7
                // Если относительная невязка
                // меньше заданного значения погрешности
                if (RelDis < MEM.Error8)         // #8
                {
                    //Console.Write("Условие достигнуто на {0} итерации", steps);
                    //Console.WriteLine();
                    break;
                }
                steps++;
            }
        }

        /// <summary>
        /// Решение Системы линейных уравнений
        /// </summary>
        /// <param name="X"></param>
        /// <param name="CoundaryCondition"></param>
        public virtual void SystemCalculation(ref double[][] X, BProc CoundaryCondition, SimpleProcedure CalkCoeff)
        {
            CalkCoeff();
            double RelDis = 1;
            int steps = 0;
            for (int index = 0; index < 100000; index++)
            {
                double MaxDis = 0;
                MaxU = 0;
                for (int i = NxMin; i < Nx - 1; i++) // -> X
                    for (int j = 1; j < Ny - 1; j++) // -> Y
                    {
                        uBuff = X[i][j];
                        bap = aP[i][j];
                        fe = aE[i][j] * X[i + 1][j];
                        fw = aW[i][j] * X[i - 1][j];
                        fn = aN[i][j] * X[i][j + 1];
                        fs = aS[i][j] * X[i][j - 1];
                        X[i][j] = (fe + fw + fn + fs + aQ[i][j]) / bap;
                        if (MaxDis < Math.Abs(uBuff - X[i][j]))     // #6
                            MaxDis = Math.Abs(uBuff - X[i][j]);
                        if (MaxU < Math.Abs(X[i][j]))
                            MaxU = Math.Abs(X[i][j]);
                    }
                // Определяем граничные значения скоростей 
                CoundaryCondition(ref X);                 // #5
                // Находим относительную невязку
                RelDis = MaxDis / MaxU;     // #7
                // Если относительная невязка
                // меньше заданного значения погрешности
                if (RelDis < MEM.Error8)         // #8
                {
                    //Console.Write("Условие достигнуто на {0} итерации", steps);
                    //Console.WriteLine();
                    break;
                }
                steps++;
            }
        }

        /// <summary>
        /// #5 Определяем граничные значения скоростей из граничных условий задачи   
        /// </summary>
        public void UBorders() // #5
        {
            for (int ix = 0; ix < Nx; ix++)
            {
                // На  дне
                //if (mesh.BCType[0] == TypeBoundCond.Dirichlet)
                mU[ix][0] = 0; // Дирихле 
                               //else
                               //    mU[ix][0] = mU[ix][1]; // Нейман и ...
                               // На свободной поверхности потока
                               //if (mesh.BCType[2] == TypeBoundCond.Dirichlet)
                               //    mU[ix][Ny-1] = 0; // Дирихле 
                               //else
                mU[ix][Ny - 1] = mU[ix][Ny - 2];  // Нейман и ...
            }
            for (int iy = 0; iy < Ny; iy++) // Нейман по бокам        
            {
                mU[NxMin - 1][iy] = mU[NxMin][iy];
                mU[Nx - 1][iy] = mU[Nx - 2][iy];
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
            //double zetaC, yC;
            //int ind = 0;
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
