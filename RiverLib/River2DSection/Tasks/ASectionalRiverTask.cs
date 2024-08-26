namespace RiverLib
{
    //---------------------------------------------------------------------------
    //                          ПРОЕКТ  "DISER"
    //                  создано  :   9.03.2007 Потапов И.И.
    //---------------------------------------------------------------------------
    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
    //            перенесено с правкой : 06.12.2020 Потапов И.И. 
    //            создание родителя : 21.02.2022 Потапов И.И. 
    //---------------------------------------------------------------------------
    using AlgebraLib;
    using CommonLib;
    using CommonLib.IO;
    
    using MemLogLib;
    using GeometryLib;
    using MeshLib;
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using MeshGeneratorsLib;
    using CommonLib.EConverter;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;
    using CommonLib.Geometry;
    using CommonLib.Function;
    using RiverLib.IO;
    using MeshGeneratorsLib.StripGenerator;

    /// <summary>
    ///  ОО: Определение класса SrossSectionalRiverTask - расчет полей скорости 
    ///     и напряжений в живом сечении потока МКЭ на произвольной сетке
    /// </summary>
    [Serializable]
    public abstract class ASectionalRiverTask : IRiver
    {
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => taskReady;
        /// <summary>
        /// готовность задачи к работе
        /// </summary>
        public bool taskReady = false;

        #region Параметры задачи
        /// <summary>
        /// Параметры задачи
        /// </summary>
        protected RiverStreamParams Params = new RiverStreamParams();
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            RiverStreamParams pp = p as RiverStreamParams;
            SetParams(pp);
        }

        public void SetParams(RiverStreamParams p)
        {
            Params = new RiverStreamParams(p);
        }

        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>

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
            Params = new RiverStreamParams();
            Params.Load(file);
        }


        #endregion


        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        double Mu = 1.1;
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


        #region Свойства
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameCrossRData.txt";
            return fn;
        }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name => name;
        protected string name = "гидрадинамика створа";
        /// <summary       
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamY1D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "ASectionalRiverTask 21.02.2022"; 
        /// <summary>
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get; set; }
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Cкорость потока", null, TypeFunForm.Form_2D_Rectangle_L1) };

        #endregion
        /// <summary>
        /// Параметры задачи зависимые от времени
        /// </summary>
        protected List<TaskEvolution> evolution = new List<TaskEvolution>();
        #region КЭ
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = { 0, 0, 0 };
        /// <summary>
        /// Квадратурные точки для численного интегрирования
        /// </summary>
        protected NumInegrationPoints pIntegration;
        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
        /// <summary>
        /// функции формы для КЭ
        /// </summary>
        protected AbFunForm ff = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] LaplMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] LocalRight = null;
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected uint[] adressBound = null;
        /// <summary>
        /// координаты узлов КЭ 
        /// </summary>
        protected double[] elem_x = null;
        protected double[] elem_y = null;
        /// <summary>
        /// Скорсть в узлах
        /// </summary>
        protected double[] elem_U = null;
        /// <summary>
        /// вязкость в узлах КЭ 
        /// </summary>
        protected double[] elem_mu = null;
        #endregion
        /// <summary>
        /// кинематическая вязкость воды
        /// </summary>
        protected double Mu0 = 1e-3;
        /// <summary>
        /// коэффициент Кармана
        /// </summary>
        protected double kappa = 0.41;
        /// <summary>
        /// коэффициент Прандтля для вязкости
        /// </summary>
        protected double sigma = 1;
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        protected private int cu;
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh() => mesh;
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh mesh = null;
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[] U;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[] mu, nu0;
        /// <summary>
        /// Поле напряжений dz
        /// </summary>
        public double[] TauZ;
        /// <summary>
        /// Поле напряжений dy
        /// </summary>
        public double[] TauY;
        /// <summary>
        /// Поле напряжений - модуль
        /// </summary>
        public double[] tau;
        /// <summary>
        /// Алгебра для КЭ задачи
        /// </summary>
        [NonSerialized]
        protected IAlgebra algebra = null;
        /// <summary>
        /// Генератор КЭ сетки в ствое задачи
        /// </summary>
        protected HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
        /// <summary>
        /// Длина смоченного периметра
        /// </summary>
        protected double GR = 0;
        /// <summary>
        /// площадь сечения канала
        /// </summary>
        public double Area = 0;
        /// <summary>
        /// правая часть уравнения
        /// </summary>
        protected double Q;
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh() { return new TwoMesh(bottom_x, bottom_y); }
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        protected double[] bottom_x;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        protected double[] bottom_y;
        /// <summary>
        /// точка правого уреза воды
        /// </summary>
        protected HKnot right;
        /// <summary>
        /// точка левого уреза воды
        /// </summary>
        protected HKnot left;
        /// <summary>
        /// текущий расход потока
        /// </summary>
        protected double riverFlowRate;
        /// <summary>
        /// текущий расчетный расход потока 
        /// </summary>
        protected double riverFlowRateCalk;
        /// <summary>
        /// текущий уровень свободной поверхности
        /// </summary>
        protected double waterLevel;
        /// <summary>
        /// Сдвиговые напряжения максимум
        /// </summary>
        public double tauMax = 0;
        /// <summary>
        /// Сдвиговые напряжения средние
        /// </summary>
        public double tauMid = 0;
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
        /// FlagStartMesh - первая генерация сетки
        /// </summary>
        protected bool FlagStartMesh = true;
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
        protected double[] tauY = null;
        protected double[] tauZ = null;
        protected double[] Coord = null;

        public ASectionalRiverTask(RiverStreamParams p) 
        {
            SetParams(p);
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
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public void InitLocal(int cu)
        {
            MEM.Alloc<double>(cu, ref elem_x, "elem_x");
            MEM.Alloc<double>(cu, ref elem_y, "elem_y");
            MEM.Alloc<double>(cu, ref elem_U, "elem_U");
            MEM.Alloc<double>(cu, ref elem_mu, "elem_mu");
            // с учетом степеней свободы
            MEM.AllocClear(cu, ref LocalRight);
            MEM.Alloc2DClear(cu, ref LaplMatrix);
        }

        /// <summary>
        /// Инициализация задачи
        /// </summary>
        public void InitTask()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            Q = rho_w * g * Params.J;
            // получение отметок дна
            Geometry.GetFunctionData(ref bottom_x, ref bottom_y, Params.CountBLKnots);
            // начальный уровень свободной поверхности
            waterLevel = WaterLevels.FunctionValue(0);
            // начальный расход потока
            riverFlowRate = flowRate.FunctionValue(0);
            // память под напряжения в области
            MEM.Alloc(Params.CountKnots, ref tau, "tau");
            // генерация сетки в области
            SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
            FlagStartMesh = false;
        }

        #region Расчет вязкости потока
        /// <summary>
        /// вычисление приведенной вязкости методом деления пополам
        /// </summary>
        public void ReCalkMu()
        {
            Logger.Instance.Info("вязкость потока в файле: " + Mu.ToString("F6"));
            // вычисление приведенной вязкости и согласованной скорости
            Mu = DMath.RootBisectionMethod(DFRateMu, 0.001, 10);
            SetMu();
            Logger.Instance.Info("вязкость потока ");
            Logger.Instance.Info("согласованная вязкость потока: " + Mu.ToString("F6"));
            FlagStartMu = true;
        }
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        public void ReCalkRoughness()
        {
            Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
            // вычисление приведенной вязкости и согласованной скорости
            roughness = DMath.RootBisectionMethod(DFRateRoughness, 0.00001, 0.01);
            Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
            FlagStartRoughness = true;
        }
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        public void ReCalkRoughnessWRodi()
        {
            Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
            // вычисление приведенной вязкости и согласованной скорости
            roughness = DMath.RootBisectionMethod(DFRateRoughnessWRodi, 0.000001, 0.01);
            Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
            FlagStartRoughness = true;
        }
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        public void ReCalkRoughnessDiff()
        {
            Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
            /// </summary>
            SetMu();
            SolveU();
            // вычисление приведенной вязкости и согласованной скорости
            roughness = DMath.RootBisectionMethod(DFRateRoughnessDiff, 0.5, 5);
            Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
            FlagStartRoughness = true;
        }
        /// <summary>
        /// Функция вычисления вязкости для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateMu(double root_mu)
        {
            Mu = root_mu;
            MEM.MemSet(mu, root_mu);
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateRoughness(double Roughness)
        {
            roughness = Roughness;
            SetMuRoughness();
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateRoughnessWRodi(double Roughness)
        {
            roughness = Roughness;
            SetMuRoughnessWRodi();
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateRoughnessDiff(double Roughness)
        {
            roughness = Roughness;
            SetMuDiff();
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Установка вязкости при mu = const
        /// </summary>
        public void SetMu()
        {
            MEM.MemSet(mu, Mu);
        }
        /// <summary>
        /// Установка вязкости при mu = mu(roughness)
        /// </summary>
        public void SetMuRoughness()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            double mu0 = SPhysics.mu;
            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);
            double Area = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
                Area += mesh.ElemSquare(i);
            double H0 = Area / GR;
            double uStar = Math.Sqrt(g * H0 * Params.J);
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double zeta = sg.spline.Value(xx[node]);
                double H = waterLevel - zeta;
                double z = (yy[node] - zeta);
                double xi = z / H;
                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
                    mu[node] = mu0;
                else
                    mu[node] = rho_w * kappa * uStar * (1 - xi) * (roughness + z) + mu0;
            }
        }
        /// <summary>
        /// Установка вязкости при mu = mu(roughness)
        /// </summary>
        public void SetMuRoughnessWRodi()
        {
            double nu0 = SPhysics.nu;
            double mu0 = SPhysics.mu;
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);
            
            double Area = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
                Area += mesh.ElemSquare(i);
            double H0 = Area/GR;
            double uStar = Math.Sqrt(g * H0 * Params.J);
            double Re = uStar * H0 / nu0;
            double Pa = 0.2;
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double zeta = sg.spline.Value(xx[node]);
                double H = waterLevel - zeta;
                double z = yy[node] - zeta;
                double xi = z / H;
                double zplus = z * uStar / nu0;
                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
                {
                    mu[node] = mu0;
                }
                else
                {
                    //if (zplus < 30)
                    //{
                    //    mu[node] = mu0 * (1 - xi);
                    //}
                    //if (zplus > 30 && zplus <= 0.2 * Re)
                    if (zplus <= 0.2 * Re)
                    {
                        mu[node] = rho_w * kappa * uStar * (1 - xi) * (roughness + z) + mu0;
                    }
                    if (zplus > 0.2 * Re)
                    {
                        mu[node] = rho_w * kappa * uStar * (1 - xi) * (roughness + z) * H /
                        (H + 2 * Math.PI * Math.PI * Pa * (roughness + z) * Math.Sin(Math.PI * xi)) + mu0;
                    }
                }
            }
        }
        /// <summary>
        /// Установка вязкости при  
        /// </summary>
        public void SetMuDiff()
        {
            double mu0 = SPhysics.mu;
            double rho_w = SPhysics.rho_w;

            for (int node = 0; node < mesh.CountKnots; node++)
                mu[node] = mu0;
            SolveU();
            double S;
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            uint[] knots = { 0, 0, 0 };
            double[] dU = new double[mesh.CountKnots];
            double[] Area = new double[mesh.CountKnots];

            double FArea = 0;
            MEM.Alloc<double>(cu, ref elem_mu, "elem_mu");
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                mesh.GetElemCoords(elem, ref x, ref y);

                mesh.ElemValues(U, elem, ref u);
                // получит вязкость в узлах
                mesh.ElemValues(mu, elem, ref elem_mu);
                //Площадь
                S = mesh.ElemSquare(elem);
                FArea += S;
                double dudx = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0])) / (2 * S);
                double dudy =  (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1])) / (2 * S);
                
                double du =  Math.Sqrt(dudx * dudx + dudy * dudy);
                
                dU[knots[0]] += du * S / 3;
                dU[knots[1]] += du * S / 3;
                dU[knots[2]] += du * S / 3;

                Area[knots[0]] += S / 3;
                Area[knots[1]] += S / 3;
                Area[knots[2]] += S / 3;
            }
            for (uint i = 0; i < mesh.CountKnots; i++)
                dU[i] /= Area[i];

            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                double zeta = sg.spline.Value(xx[node]);
                double H = waterLevel - zeta;
                double z = yy[node] - zeta;
                double xi = z / H;
                double lm = (kappa * z) * Math.Exp(- roughness*xi);
                if (H <= MEM.Error10)
                    mu[node] = mu0;
                else
                    mu[node] = rho_w * lm * lm * Math.Sqrt(dU[node]) + mu0;
            }
        }
    

        #endregion
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public bool LoadData(string fileName)
        {
            string message = "Файл данных задачи не обнаружен";
            Init();
            return WR.LoadParams(LoadData, message, fileName);
        }

        public virtual void LoadData(StreamReader file) 
        {
            LoadTaskData(file);
        }
        /// <summary>
        /// Чтение данных задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void LoadTaskData(StreamReader file)
        {
            //if (taskReady == false)
            {
                // геометрия дна
                Geometry.Load(file);
                // свободная поверхность
                WaterLevels.Load(file);
                // расход потока
                flowRate.Load(file);
                // инициализация задачи
                InitTask();
                // Задача готова к расчету
                taskReady = true;
            }
        }

        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh">сетка</param>
        /// <param name="algebra">решатель</param>
        public void Set(IMesh mesh, IAlgebra algebra = null)
        {
            this.mesh = mesh;
            this.algebra = algebra;
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta">отметки дна</param>
        /// <param name="bedErosion">флаг генерация сетки при размывах дна</param>
        public void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            if(taskReady == false)
            {

            }

            Erosion = bedErosion;
            if(Erosion != EBedErosion.NoBedErosion)
            {
                bottom_y = zeta;
            }
            // FlagStartMesh - первая генерация сетки
            // bedErosion - генерация сетки при размывах дна
            if (FlagStartMesh == true)// || bedErosion == true)
            {
                SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
                FlagStartMesh = false;
            }
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            zeta = bottom_y;
        }
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {
            //if (FlagStartMesh == false) return;
            int flagErr = 0;
            try
            {
                // расчет уровня свободной поверхности реки
                SolveWaterLevel();
                flagErr++;
                // определение расчетной области потока и построение КЭ сетки
                SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
                flagErr++;
                // расчет гидрадинамики  (скоростей потока)
                SolveVelosity();
                flagErr++;
                // расчет  придонных касательных напряжений на дне
                tau = TausToVols(bottom_x, bottom_y);
                flagErr++;
                // сохранение данных в начальный момент времени
                flagErr++;
                time += dtime;
            }
            catch (Exception ex)
            {
                string ErrorName = RiverError.ErrorName(flagErr);
                Logger.Instance.Error(ErrorName, "ASectionalRiverTask");
                Logger.Instance.Exception(ex);
            }
        }
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public virtual void Scan()
        {
            TaskEvolution te = new ExTaskEvolution(time, Mu,
                                waterLevel, tauMax, tauMid, GR, Area, 
                                riverFlowRate, riverFlowRateCalk, 0, 0);
            evolution.Add(te);
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public virtual void AddMeshPolesForGraphics(ISavePoint sp)
        {
            // поля на сетке
            sp.Add("U", U);
            sp.Add("Mu", mu);
            sp.Add("TauY", TauY);
            sp.Add("TauZ", TauZ);
            // векторное поле на сетке
            sp.Add("Tau", TauY, TauZ);
            // кривые 
            // дно - берег
            sp.AddCurve("Русловой профиль", bottom_x, bottom_y);
            double[] xwl = { left.x, right.x };
            double[] ywl = { left.y, right.y };
            // свободная поверхность
            sp.AddCurve("Свободная поверхность", xwl, ywl);

            Scan();
            if (evolution.Count > 1)
            {
                double[] times = (from arg in evolution select arg.time).ToArray();
                double[] wls = (from arg in evolution select arg.waterLevel).ToArray();
                sp.AddCurve("Эв.св.поверхности", times, wls, TypeGraphicsCurve.TimeCurve);
                double[] mus = (from arg in evolution select arg.Mu).ToArray();
                sp.AddCurve("Вязкость", times, mus, TypeGraphicsCurve.TimeCurve);
                double[] tm = (from arg in evolution select arg.tauMax).ToArray();
                sp.AddCurve("Tau максимум", times, tm, TypeGraphicsCurve.TimeCurve);
                tm = (from arg in evolution select arg.tauMid).ToArray();
                sp.AddCurve("Tau средние", times, tm, TypeGraphicsCurve.TimeCurve);
                double[] gr = (from arg in evolution select arg.GR).ToArray();
                sp.AddCurve("Гидравл. радиус", times, gr, TypeGraphicsCurve.TimeCurve);
                double[] ar = (from arg in evolution select arg.Area).ToArray();
                sp.AddCurve("Площадь сечения", times, ar, TypeGraphicsCurve.TimeCurve);
                double[] rfr = (from arg in evolution select arg.riverFlowRate).ToArray();
                sp.AddCurve("Расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
                rfr = (from arg in evolution select arg.riverFlowRateCalk).ToArray();
                sp.AddCurve("Текущий расчетный расход потока", times, rfr, TypeGraphicsCurve.TimeCurve);
            }
        }

        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public virtual void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            _tauX = tau;
        }
        /// <summary>
        /// Расчет уровня свободной поверхности реки
        /// </summary>
        protected virtual void SolveWaterLevel()
        {
            if (Params.taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = flowRate.FunctionValue(time);
                if (time > 0)
                {
                    double Ck = 0.1;
                    double Umax = 5;
                    riverFlowRateCalk = RiverFlowRate();
                    double deltaH = (riverFlowRateCalk - riverFlowRate) / riverFlowRate;
                    double W = bottom_x[bottom_x.Length - 1] - bottom_x[0];
                    double H = riverFlowRateCalk / (W * Umax) * deltaH;
                    double dH = Ck * dtime * H;
                    if (Math.Abs(dH) > 0.005)
                        dH = Math.Sign(dH) * 0.005;
                    waterLevel = waterLevel - dH;
                }
                else
                {
                    riverFlowRateCalk = riverFlowRate;
                    FlagStartMesh = true;
                }
            }
            else
            {
                waterLevel = WaterLevels.FunctionValue(time);
            }
        }
        /// <summary>
        /// Установка параметров задачи
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="mesh"></param>
        public virtual void SetDataForRiverStream(double waterLevel, double[] fx, double[] fy, ref HKnot right, ref HKnot left)
        {
            this.bottom_x = fx;
            this.bottom_y = fy;
            // генерация сетки
            mesh = sg.CreateMesh(ref GR, waterLevel, bottom_x, bottom_y);
            IPointsA.SetInt(1 + (int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_L1);
            IPointsB.SetInt(1 + (int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_L1);
            right = sg.Right();
            left = sg.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);
            if (mu == null)
            {
                MEM.Alloc(mesh.CountKnots, ref mu, "mu");
                MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
                MEM.Alloc(mesh.CountKnots, ref U, "U");
                MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                FlagStartMu = false;
                FlagStartRoughness = false;
            }
            if (mesh.CountKnots != mu.Length)
            {
                double mMu = mu.Sum() / mu.Length;
                MEM.Alloc(mesh.CountKnots, ref mu, "mu");
                MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
                MEM.Alloc(mesh.CountKnots, ref U, "U");
                MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                switch (Params.ViscosityModel)
                {
                    case TurbulentViscosityModel.ViscosityConst:
                        {
                            MEM.MemSet(mu, mMu);
                            break;
                        }
                    case TurbulentViscosityModel.ViscosityBlasius:
                        {
                            SetMuRoughness();
                            break;
                        }
                    case TurbulentViscosityModel.ViscosityWolfgangRodi:
                        {
                            SetMuRoughnessWRodi();
                            break;
                        }
                    case TurbulentViscosityModel.ViscosityPrandtlKarman:
                        {
                            SetMuDiff();
                            break;
                        }
                    default:
                        {
                            MEM.MemSet(mu, mMu);
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        public virtual double[] TausToVols(double[] xv, double[] yv)
        {
            // массив касательных напряжений
            MEM.Alloc(xv.Length - 1, ref tau);
            // расчет напряжений Txy  Txz
            SolveTaus();
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(1);
            // количество узлов на нижней границе области
            TSpline tauSplineZ = new TSpline();
            TSpline tauSplineY = new TSpline();
            MEM.Alloc(bounds.Length, ref tauY);
            MEM.Alloc(bounds.Length, ref tauZ);
            MEM.Alloc(bounds.Length, ref Coord);
            // пробегаем по граничным узлам и записываем для них Ty, Tz T
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < bounds.Length - 1; i++)
            {
                tauZ[i] = 0.5 * (TauZ[bounds[i]] + TauZ[bounds[i + 1]]);
                tauY[i] = 0.5 * (TauY[bounds[i]] + TauY[bounds[i + 1]]);
                Coord[i] = 0.5 * (xx[bounds[i]] + xx[bounds[i + 1]]);
            }
            double left = xx[bounds[0]];
            double right = xx[bounds[bounds.Length - 1]];
            // формируем сплайны напряжений в натуральной координате
            tauSplineZ.Set(tauZ, Coord, (uint)bounds.Length);
            tauSplineY.Set(tauY, Coord, (uint)bounds.Length);

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
                        throw new Exception("Mesh for RiverStreamTask");
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tau.Max();
            /// Сдвиговые напряжения средние
            tauMid = tau.Sum()/tau.Length;
            return tau;
        }
        
        /// <summary>
        /// Отчетная информация о речном процессе
        /// </summary>
        /// <returns></returns>
        public ReportDataRiver GetRiverReportData()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            ReportDataRiver rrd = new ReportDataRiver();
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                // получить тип и узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(i, ref knots);
                // выбрать по типу квадратурную схему для интегрирования
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                // определить количество узлов на КЭ
                int cu = knots.Length;
                // выделить память под локальные массивы
                InitLocal(cu);
                //Координаты и площадь
                mesh.GetElemCoords(i, ref elem_x, ref elem_y);

                mesh.ElemValues(U, i, ref elem_U);

                double su = elem_U.Sum() / elem_U.Length;
                //Площадь
                double S = mesh.ElemSquare(i);
                rrd.area += S;
                //private double Ekin, Epot;
                //
                double xc = elem_y.Sum() / elem_y.Length;
                // потонциальная энергия потока на КЭ
                rrd.Epot += S * xc;
                // кинетическая энергия потока на КЭ
                rrd.Ekin += S * su * su / 2;
                // расход потока на КЭ
                rrd.riverRate += S * su;
            }
            // потонциальная энергия потока
            rrd.Epot *= g * rho_w;
            // кинетическая энергия потока
            rrd.Ekin *= rho_w;
            // полная энергия потока
            rrd.EnergyFlow = rrd.Ekin + rrd.Epot;
            // средняя скорость
            rrd.midleVelocity = rrd.riverRate / rrd.area;
            // массовый расход
            rrd.Width = 0;
            rrd.Depth = 0;// area / Width;
            return rrd;
        }
        /// <summary>
        /// Расчет расхода воды
        /// </summary>
        /// <returns>объемный расход воды</returns>
        public virtual double RiverFlowRate()
        {
            Area = 0;
            double su, S;
            riverFlowRateCalk = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                // получить тип и узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(i, ref knots);
                // выбрать по типу квадратурную схему для интегрирования
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                // определить количество узлов на КЭ
                int cu = knots.Length;
                // выделить память под локальные массивы
                InitLocal(cu);
                //Координаты и площадь
                mesh.GetElemCoords(i, ref elem_x, ref elem_y);

                mesh.ElemValues(U, i, ref elem_U);
                su = elem_U.Sum() / elem_U.Length;
                //Площадь
                S = mesh.ElemSquare(i);
                // расход по живому сечению
                riverFlowRateCalk += S * su;
                Area += S;
            }
            if (double.IsNaN(riverFlowRateCalk) == true)
                throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public virtual IOFormater<IRiver> GetFormater()
        {
            return new RiverFormatReaderSrossSection_RvY();
        }

        /// <summary>
        /// расчет параметров потока, скоростей и глубины потока
        /// </summary>
        public virtual void SolveVelosity()
        {
            switch (Params.ViscosityModel)
            {
                case TurbulentViscosityModel.ViscosityConst:
                    {
                        if (FlagStartMu == false)
                            ReCalkMu();
                        else
                            SolveU();
                        break;
                    }
               case TurbulentViscosityModel.ViscosityBlasius:
                    {
                        if (FlagStartRoughness == false)
                            ReCalkRoughness();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.ViscosityWolfgangRodi:
                    {
                        if(FlagStartRoughness == false)
                            ReCalkRoughnessWRodi();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.ViscosityPrandtlKarman:
                    {
                        if (FlagStartRoughness == false)
                           ReCalkRoughnessDiff();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.Viscosity2DXY:
                    {
                        //double Pv, Dv;
                        //double dUx, dUy, dUUV2;
                        //double[] xx = mesh.GetCoordsX();
                        //double[] yy = mesh.GetCoordsY();
                        //uint elem = 0;
                        //// буфферезация вязкости
                        //MEM.MemCpy(ref nu0, mu);
                        //// цикл по нелинейности
                        //for (int n = 0; n < 100; n++)
                        //{
                        //    double SPv = 0, SDv = 0, gU = 0;
                        //    try
                        //    {
                        //        double[] elem_H = null;
                        //        double[] elem_Xi = null;
                        //        algebra.Clear();
                        //        for (elem = 0; elem < mesh.CountElements; elem++)
                        //        {
                        //            // получить тип и узлы КЭ
                        //            TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                        //            // выбрать по типу квадратурную схему для интегрирования
                        //            if (mesh.First == typeff)
                        //                pIntegration = IPointsA;
                        //            else
                        //                pIntegration = IPointsB;
                        //            // определить количество узлов на КЭ
                        //            cu = knots.Length;
                        //            // выделить память под локальные массивы
                        //            InitLocal(cu);
                        //            MEM.Alloc(cu, ref elem_H, "elem_H");
                        //            MEM.Alloc(cu, ref elem_Xi, "elem_Xi");
                        //            for (int node = 0; node < cu; node++)
                        //            {
                        //                double zeta = sg.spline.Value(xx[knots[node]]);
                        //                elem_H[node] = waterLevel - zeta;
                        //                if (MEM.Equals(elem_H[node], 0))
                        //                {
                        //                    elem_Xi[node] = 0;
                        //                    elem_H[node] = 0.001;
                        //                }
                        //                else
                        //                    elem_Xi[node] = (yy[node] - zeta) / elem_H[node];
                        //            }
                        //            // получить координаты узлов
                        //            mesh.ElemX(elem, ref elem_x);
                        //            mesh.ElemY(elem, ref elem_y);
                        //            // получит вязкость в узлах
                        //            mesh.ElemValues(mu, elem, ref elem_mu);
                        //            mesh.ElemValues(U, elem, ref elem_U);
                        //            // получить функции формы КЭ
                        //            ff = FunFormsManager.CreateKernel(typeff);
                        //            // передать координат узлов в функции формы
                        //            ff.SetGeoCoords(elem_x, elem_y);
                        //            // цикл по точкам интегрирования
                        //            for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                        //            {
                        //                ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        //                // вычислить глобальыне производные в точках интегрирования
                        //                ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        //                Mu = 0;
                        //                dUx = 0;
                        //                dUy = 0;
                        //                for (int ai = 0; ai < cu; ai++)
                        //                {
                        //                    // вычислить вязкость в точке интегрирования
                        //                    Mu += elem_mu[ai] * ff.N[ai];
                        //                    // >>>> горизональная производная 
                        //                    dUx += elem_U[ai] * ff.DN_x[ai];
                        //                    dUy += elem_U[ai] * ff.DN_y[ai];
                        //                }
                        //                dUUV2 = Mu * Math.Sqrt(dUx * dUx + dUy * dUy);
                        //                Pv = 0;
                        //                Dv = 0;
                        //                for (int ai = 0; ai < cu; ai++)
                        //                {
                        //                    Pv += (5 * kappa / sigma * rho_w + 2 * Mu0 / (elem_H[ai] * Math.Sqrt(g * elem_H[ai] * J))) * kappa * g * elem_H[ai] * J;
                        //                    Dv += 6 * kappa * kappa / sigma * (dUUV2 + elem_Xi[ai] * elem_Xi[ai] * elem_H[ai] * rho_w * g * J);
                        //                }
                        //                // вычислить глобальыне производные в точках интегрирования
                        //                ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        //                // получть произведение якобиана и веса для точки интегрирования
                        //                double DWJ = ff.DetJ * pIntegration.weight[pi];
                        //                // локальная матрица жесткости                    
                        //                for (int ai = 0; ai < cu; ai++)
                        //                    for (int aj = 0; aj < cu; aj++)
                        //                        LaplMatrix[ai][aj] += (Mu0 + Mu) * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                        //                // Вычисление ЛПЧ

                        //                for (int ai = 0; ai < cu; ai++)
                        //                {
                        //                    LocalRight[ai] += rho_w * (Pv - Dv) * ff.N[ai] * DWJ;
                        //                }
                        //                SPv += Pv; SDv += Dv; gU += dUUV2;
                        //            }
                        //            // добавление вновь сформированной ЛЖМ в ГМЖ
                        //            algebra.AddToMatrix(LaplMatrix, knots);
                        //            // добавление вновь сформированной ЛПЧ в ГПЧ
                        //            algebra.AddToRight(LocalRight, knots);
                        //        }
                        //        Console.WriteLine(" SPv  = {0}  SDv = {1} gU = {2}", SPv, SDv, gU);
                        //        //Удовлетворение ГУ
                        //        uint[] bound = mesh.GetBoundKnotsByMarker(1);
                        //        algebra.BoundConditions(Mu0, bound);
                        //        bound = mesh.GetBoundKnotsByMarker(2);
                        //        algebra.BoundConditions(Mu0, bound);

                        //        algebra.Solve(ref mu);

                        //        foreach (var ee in mu)
                        //            if (double.IsNaN(ee) == true)
                        //                throw new Exception("SolveVelosity >> algebra");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Logger.Instance.Exception(ex);
                        //        Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                        //        Logger.Instance.Info("Элементов обработано :" + elem.ToString());
                        //    }
                        //    // расчет скорости
                        //    SolveU();
                        //}
                        break;
                    }

            }
            FlagStartMu = true;
        }

        #region абстрактные методы
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public abstract IRiver Clone();
        
        /// <summary>
        /// расчет гидрадинамики  (скоростей потока)
        /// </summary>
        public abstract void SolveU();
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public abstract void SolveTaus();
        #endregion
    }
}
