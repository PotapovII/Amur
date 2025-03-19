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
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using MeshLib;
    using MemLogLib;
    using GeometryLib;
    using GeometryLib.Vector;
    using MeshLib.Mesh.RecMesh;

    /// <summary>
    ///  ОО: Определение класса RiverSectionalQuadSM - для расчета поля скорости
    ///  в створе речного потока
    ///  
    /// </summary>
    [Serializable]
    public class RiverSectionalQuadSM : IRiver
    {

        #region Параметры задачи
        /// <summary>
        /// Параметры задачи
        /// </summary>
        protected RiverStreamQuadParams Params = new RiverStreamQuadParams();
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            RiverStreamQuadParams pp = p as RiverStreamQuadParams;
            SetParams(pp);
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(RiverStreamQuadParams p)
        {
            Params = new RiverStreamQuadParams(p);
            InitTask();
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
            Params = new RiverStreamQuadParams();
            Params.Load(file);
        }
        #endregion


        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;
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
                                       TypeBoundCond.Dirichlet,
                                       TypeBoundCond.Dirichlet };
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;

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
        protected double MuS = 1000;
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
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMu1;
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
        /// Источник для расчета вязкости
        /// </summary>
        double Fmu = 0;
        /// <summary>
        /// Текущий расход потока
        /// </summary>
        double Qwater;

        #endregion
        public double[][] aP, aE, aW, aN, aS, aQ;
        protected double uBuff, bap, fe, fw, fn, fs, Ap, An, Ae, As, Aw, MaxU, Dis;
        protected int NxMin = 1, NyMin = 1;
        protected double dx;
        protected double dy;
        public RiverSectionalQuadSM(RiverStreamQuadParams p) 
        {
            SetParams(p);
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
        /// Поиск вязкости по объему
        /// </summary>
        bool FlagLookMu = true;
        /// <summary>
        /// Корректор расхода
        /// </summary>
        double alphaQ = 1;
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
                if (FlagLookMu == true)
                {
                    // Расчет вязкости и плотности
                    SetMuAndRho();
                    flagErr++;
                    // Расчет скоростей
                    VelocityCalculation();
                    FlagLookMu = false;
                }
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
            return new RiverSectionalQuadSM(this.Params);
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
            int Nx = Params.Nx;
            int Ny = Params.Ny;

            mesh.GetValueTo1D(mU, ref U);
            sp.Add("U", U);
            mesh.GetValueTo1D(mMu, ref Mu);
            sp.Add("Mu", Mu);
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    if (mesh.Y[i][j] < Zeta[i])
                        mMu[i][j] = Mu_w;
            mesh.GetValueTo1D(mMu, ref Mu);
            sp.Add("Mu0", Mu);

            mesh.GetValueTo1D(mMu1, ref Mu);
            sp.Add("Mu1", Mu);
            

            mesh.GetValueTo1D(mTauX, ref TauX);
            sp.Add("TauX", TauX);
            mesh.GetValueTo1D(mTauY, ref TauY);
            sp.Add("TauY", TauY);
            // векторное поле на сетке
            sp.Add("Tau", TauX, TauY);

            mesh.GetValueTo1D(mTauX1, ref TauX);
            sp.Add("TauX1 цр", TauX);
            mesh.GetValueTo1D(mTauY1, ref TauY);
            sp.Add("TauY1 цр", TauY);
            // векторное поле на сетке
            sp.Add("Tau1 цр", TauX, TauY);

            mesh.GetValueTo1D(mTauX2, ref TauX);
            sp.Add("TauX2 осреднение mu", TauX);
            mesh.GetValueTo1D(mTauY2, ref TauY);
            sp.Add("TauY2 осреднение mu", TauY);
            // векторное поле на сетке
            sp.Add("Tau2 осреднение mu", TauX, TauY);

            mesh.GetValueTo1D(mRho, ref TauY);
            sp.Add("mRho", TauY);

            mesh.GetValueTo1D(mTauX3, ref TauX);
            sp.Add("TauX3 цр и оср вязк", TauX);
            mesh.GetValueTo1D(mTauY3, ref TauY);
            sp.Add("TauY3 цр и оср вязк", TauY);
            // векторное поле на сетке
            sp.Add("Tau3 цр и оср вязк", TauX, TauY);
                // фильтрация напряжений - только вода
                for (uint i = 0; i < Nx; i++)
                    for (int j = 0; j < Ny; j++)
                        if (mesh.Y[i][j] < Zeta[i])
                        {
                            mTauX[i][j] = 0;
                            mTauY[i][j] = 0;
                    }
                mesh.GetValueTo1D(mTauX, ref TauX);
                sp.Add("sTauX", TauX);
                mesh.GetValueTo1D(mTauY, ref TauY);
                sp.Add("sTauY", TauY);
                // векторное поле на сетке
                sp.Add("sTau", TauX, TauY);

            GetBTau();

            sp.AddCurve("", mesh.X[0], bNormalTau);

            sp.AddCurve("Касательные придонные напряжения", mesh.X[0], bNormalTau);
            sp.AddCurve("Касательные напряжения гидростатика", mesh.X[0], bNormalTauGS);
            sp.AddCurve("Придонное давление", mesh.X[0], bPress);
            sp.AddCurve("Донных профиль", mesh.X[0], Zeta);

            double[] sMu = null, sTauX = null, sTauY = null, sArg = null, sTauV = null;
            double[] sX = null, sY = null;
            GetTauContur(ref sMu, ref sTauX, ref sTauY, ref sArg, ref sX,ref sY);
            sp.AddCurve("Касательные придонные напряжения по Х", sArg, sTauX);
            sp.AddCurve("Касательные придонные напряжения по Y", sArg, sTauY);
            sp.AddCurve("Донных профиль C", sX, sY);
            MEM.Alloc(sMu.Length, ref sTauV, "sTauV");

            for (int i = 0; i < sTauX.Length; i++)
                sTauV[i] = Math.Sqrt(sTauX[i] * sTauX[i] + sTauY[i] * sTauY[i]);
            sp.AddCurve("Касательные придонные напряжения", sArg, sTauV);
            sp.AddCurve("Донная вязкость", sArg, sMu);
        }

        public void GetTauContur(ref double[] sMu, ref double[] sTauX, ref double[] sTauY, ref double[] sArg,
            ref double[] sX, ref double[] sY)
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;

            List<double> LTauX = new List<double>();
            List<double> LTauY = new List<double>();
            List<double> LsMu = new List<double>();
            List<double> LsTau = new List<double>();
            List<double> LsX = new List<double>();
            List<double> LsY= new List<double>();

            // Поиск индекса левой границы
            uint ileft = 0;
            for (uint i = 0; i < Nx; i++)
                if (Zeta[i] > waterLevel && Zeta[i + 1] < waterLevel)
                {
                    ileft = i + 3; break;
                }
            // Запись напряжения с левой границы
            double stau = 0;
            for (int j = Ny - 1; j > 0; j--)
                if (mesh.Y[ileft][j] > Zeta[ileft])
                    stau += dy;
                else
                    break;
            // сдвиг графика для синхронизации с геоментрией
            stau = mesh.X[ileft][0] - stau;

            for (int j = Ny - 1; j > 0; j--)
                if (mesh.Y[ileft][j] > Zeta[ileft])
                {
                    stau += dy;
                    LTauX.Add(mTauX[ileft][j]);
                    LTauY.Add(mTauY[ileft][j]);
                    LsMu.Add(mMu[ileft][j]);
                    LsTau.Add(stau);
                    LsX.Add(mesh.X[ileft][j]);
                    LsY.Add(mesh.Y[ileft][j]);
                }
                else
                    break;
            stau = mesh.X[ileft + 1][0] - dx;
            // Запись напряжения с дна
            for (uint i = ileft + 1; i < Nx; i++)
            {
                stau += dx;
                for (int j = 0; j < Ny; j++)
                    if (mesh.Y[i][j] > Zeta[i])
                    {
                        LTauX.Add(mTauX[i][j + 1]);
                        LTauY.Add(mTauY[i][j + 1]);
                        LsMu.Add(mMu[i][j]);
                        LsTau.Add(stau);
                        LsX.Add(mesh.X[i][j]);
                        LsY.Add(mesh.Y[i][j]);
                        break;
                    }
            }
            double ll = stau - mesh.X[Nx - 1][0];
            // Запись напряжения с правой границы
            for (int j = 0; j < Ny; j++)
                if (mesh.Y[Nx - 1][j] > Zeta[Nx - 1])
                {
                    stau += dy;
                    LTauX.Add(mTauX[Nx - 2][j]);
                    LTauY.Add(mTauY[Nx - 2][j]);
                    LsMu.Add(mMu[Nx - 1][j]);
                    LsTau.Add(stau);
                    LsX.Add(mesh.X[Nx - 1][j]);
                    LsY.Add(mesh.Y[Nx - 1][j]);
                }
            sMu = LsMu.ToArray();
            sTauX = LTauX.ToArray();
            sTauY = LTauY.ToArray();
            sArg = LsTau.ToArray();
            sX = LsX.ToArray();
            sY = LsY.ToArray();
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
            for (uint j = 0; j < Params.Ny; j++)
            {
                tauX[j] = bNormalTau[j];
                P[j] = bPress[j];
            }
        }
        #endregion
        /// <summary>
        /// Инициализация задачи
        /// </summary>
        public void InitTask()
        {
            // получение отметок дна
            Geometry.GetFunctionData(ref bottom_x, ref Zeta, Params.Nx);
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
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            CreateCalkArea();

            MEM.Alloc(Nx, Ny, ref mTauX, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTau, "Tau");

            MEM.Alloc(Nx, Ny, ref mTauX1, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY1, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTau1, "Tau");
            MEM.Alloc(Nx, Ny, ref mTauX2, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY2, "mTauY");
            MEM.Alloc(Nx, Ny, ref mTauX3, "mTauX");
            MEM.Alloc(Nx, Ny, ref mTauY3, "mTauY");



            MEM.Alloc(Nx, Ny, ref mRho, "mRho");
            MEM.Alloc(Nx, Ny, ref mMu, "mMu");
            MEM.Alloc(Nx, Ny, ref mMu1, "mMu1");
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
            double Ymin = waterLevel - (1 + Params.KsDepth) * maxDepth;
            double Ly = waterLevel - Ymin;
            // шаг сетки по вертикали
            dy = Ly / (Params.Ny - 1);
            double Lx = Geometry.Length;
            double Xmin = Geometry.Xmin;
            // шаг сетки по горизонтали
            dx = Lx / (Params.Nx - 1);
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
            mesh = new ChannelRectangleMesh(Params.Nx, Params.Ny, Lx, Ly);
            mesh.CreateMesh(BCMark);


            BoundConditionQuadU = new BoundaryConditionsQuad();

            NxMin = 0;
            for (int j = 0; j < Params.Nx - 1; j++)
                if (Zeta[j] > waterLevel && Zeta[j + 1] <= waterLevel)
                {
                    NxMin = j;
                    break;
                }
            NxMin -= 6;


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
                    U_star[i] = Math.Sqrt(g * Params.J * Depth[i]);
                else
                    U_star[i] = 0;
            }
            switch (Params.ViscosityModel)
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
                        
                        double Fmu = CalkFmu();
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
            for (i = NxMin; i < Params.Nx - 1; i++)
                for (j = 1; j < Params.Ny - 1; j++)
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
        public double CalkFmu()
        {
            Qwater = GetQwater();
            if (FlagLookMu == true)
                alphaQ = 1;
            else
                alphaQ = Qwater / Params.Qwater0;
            double kappa = 0.41;
            double DepthMax = Depth.Max();
            Fmu = 2 * alphaQ * rho_w * kappa * Math.Sqrt(g * Params.J / DepthMax);
            //if (Flag_Q_Mu == true)
            //{
            //    double dQ = (Qwater - Qwater0) / Qwater0;
            //    Fmu = Fmu * (1 + dtime * dQ);
            //}
            return Fmu;
        }
        public void SolverMu(double Fmu, ref double[][] mMu)
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            //double d80 = 0.02;
            //double d50 = 0.003;
            //double d10 = 0.0005;
            double d80 = 0.005;
            double d50 = 0.001;
            double d10 = 0.001;
            double DepthMax = Depth.Max();
            double u0 = Math.Sqrt(g * Params.J * DepthMax);
            double kappa = 0.41;
            double mu10 = rho_w * kappa * u0 * d10;
            double mu50 = rho_w * kappa * u0 * d50;
            double mu80 = rho_w * kappa * u0 * d80;

            bcMu.ValueDir = new double[4] { mu10, mu10, mu10, mu10 };
                             
            algebraMu = new AlgebraQuadSolver(mesh, BCTypeMu, bcMu);

            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    if (mesh.Y[i][j] < Zeta[i])
                        mMu[i][j] = MuS;
                    else
                        mMu[i][j] = 1;

            algebraMu.CalkAk(mMu, NxMin, NyMin);
            algebraMu.CalkRight(Fmu);
            AlgebraQuadSolver.Zeta = Zeta;
            AlgebraQuadSolver.bcValues = new double[5]
            {
                 mu10, mu10, mu80, mu50, mu50
            };
            //   0.0,  0.11, 0.43, 0.67, 1.32
            algebraMu.SystemSolver(ref mMu, algebraMu.VarBoundCondition);
            AlgebraQuadSolver.Zeta = null;

            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    if(mMu[i][j]<MuS/2)
                        mMu1[i][j] = mMu[i][j];
                    else
                        mMu1[i][j] = Mu_w;

            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    if (mesh.Y[i][j] < Zeta[i])
                        mMu[i][j] = MuS;
        }

        public void CalkMuAndRho()
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;
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
                    }
                    else
                    {
                        mMu[i][j] = MuS;
                        mRho[i][j] = epsilon * rho_w + (1 - epsilon) * rho_s;
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
            algebra.CalkRight(mRho, g * Params.J);
            algebra.SystemSolver(ref mU);
        }
        
        /// <summary>
        /// Расчет напряжений в узлах МЦР
        /// </summary>
        public void CalkTau()
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            int i, j;

            for (i = NxMin; i < Nx; i++)
                for (j = 1; j < Ny; j++)
                {
                    mTauX[i][j] = mMu[i][j] * (mU[i][j] - mU[i - 1][j]) / dx;
                    mTauY[i][j] = mMu[i][j] * (mU[i][j] - mU[i][j - 1]) / dy;
                }

            for (i = NxMin; i < Nx - 1; i++)
                for (j = 1; j < Ny - 1; j++)
                {
                    mTauX1[i][j] = mMu[i][j] * ((mU[i + 1][j] - mU[i - 1][j]) / (dx * 2));
                    mTauY1[i][j] = mMu[i][j] * ((mU[i][j + 1] - mU[i][j - 1]) / (dy * 2));
                }

            for (i = NxMin; i < Nx; i++)
                for (j = 1; j < Ny; j++)
                {
                    mTauX2[i][j] = 0.5 * (mMu[i][j] + mMu[i - 1][j]) * ((mU[i][j] - mU[i - 1][j]) / dx);
                    mTauY2[i][j] = 0.5 * (mMu[i][j] + mMu[i][j - 1]) * ((mU[i][j] - mU[i][j - 1]) / dy);
                }

            //for (i = NxMin; i < Nx - 1; i++)
            //    for (j = 1; j < Ny - 1; j++)
            //    {
            //        mTauX3[i][j] = 1 / 3.0 * (mMu[i + 1][j] + 2 * mMu[i][j]) * ((mU[i + 1][j] - mU[i - 1][j]) / (dx * 2));
            //        mTauY3[i][j] = 1 / 3.0 * (mMu[i][j + 1] + 2 * mMu[i][j]) * ((mU[i][j + 1] - mU[i][j - 1]) / (dy * 2));
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
            for (int j = NxMin; j < Params.Nx; j++)
            {
                //eta = waterLevel;
                double xC = dx * (j + 0.5f);
                double zetaC = 0.5 * (Zeta[j] + Zeta[j-1]);
                double dzeta = Zeta[j] - Zeta[j - 1];
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
                    //bNormalTau[j] = Mu_p * (Upp - Ucc) / sigma;
                    bNormalTau[j] = Mu_w * (Upp - Ucc) / sigma;
                    bPress[j] = g * rho_p * (Depth[j] + Depth[j-1]) / 2;
                    // гидростатическое придонное касательное напряжение
                    bNormalTauGS[j] = bPress[j] * Params.J;
                }
                else
                {
                    bNormalTau[j] = 0;
                    bPress[j] = 0;
                    bNormalTauGS[j] = 0;
                }
           }
        }
        #endregion
    }
}
