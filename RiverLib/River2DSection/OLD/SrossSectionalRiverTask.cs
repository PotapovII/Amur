//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//                  создано  :   9.03.2007 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 06.12.2020 Потапов И.И. 
//---------------------------------------------------------------------------
namespace RiverLib
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.IO;
    using MeshGeneratorsLib;
    using MemLogLib;
    using GeometryLib;
    using MeshLib;
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using CommonLib.EConverter;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;
    using CommonLib.Geometry;
    using CommonLib.Function;
    using MeshGeneratorsLib.StripGenerator;

    /// <summary>
    ///  ОО: Определение класса SrossSectionalRiverTask - расчет полей скорости 
    ///     и напряжений в живом сечении потока МКЭ на произвольной сетке
    /// </summary>
    [Serializable]
    public class SrossSectionalRiverTask : RiverStreamParams, IRiver
    {
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        public bool TaskReady() => true;
        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        double Mu = SPhysics.mu;
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        public EBedErosion GetBedErosion() => Erosion;
        public EBedErosion Erosion;

        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public virtual IBoundaryConditions BoundCondition() => null;

        #region Свойства
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames();
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameCrossRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get => "турбулентная гидрадинамика створа в прямой пойме"; }
        /// <summary       
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamY1D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "SrossSectionalRiverTask 01.09.2021"; 
        /// <summary>
        /// Текущее время
        /// </summary>
        public double time { get; set; }
        /// <summary>
        /// Текущий шаг по времени
        /// </summary>
        public double dtime { get; set; }
        #endregion
        /// <summary>
        /// Параметры задачи зависимые от времени
        /// </summary>
        List<TaskEvolution> evolution = new List<TaskEvolution>();
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
        // функции формы для КЭ
        AbFunForm ff;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] LaplMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        private double[] LocalRight = null;
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
        double Mu0 = 1e-3;
        /// <summary>
        /// коэффициент Кармана
        /// </summary>
        double kappa = 0.41;
        /// <summary>
        /// коэффициент Прандтля для вязкости
        /// </summary>
        double sigma = 1;
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        private int cu;
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh() => mesh;
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh mesh = null;
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns() => unknowns;
        protected IUnknown[] unknowns = { new Unknown("Cкорость потока", null, TypeFunForm.Form_2D_Rectangle_L1) };
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
        private IAlgebra algebra = null;
        /// <summary>
        /// Генератор КЭ сетки в ствое задачи
        /// </summary>
        HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
        /// <summary>
        /// Длина смоченного периметра
        /// </summary>
        double GR = 0;
        /// <summary>
        /// площадь сечения канала
        /// </summary>
        public double Area = 0;
        /// <summary>
        /// правая часть уравнения
        /// </summary>
        private double Q;
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh() { return new TwoMesh(bottom_x, bottom_y); }
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        double[] bottom_x;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        double[] bottom_y;
        /// <summary>
        /// точка правого уреза воды
        /// </summary>
        HKnot right;
        /// <summary>
        /// точка левого уреза воды
        /// </summary>
        HKnot left;
        /// <summary>
        /// текущий расход потока
        /// </summary>
        double riverFlowRate;
        /// <summary>
        /// текущий расчетный расход потока 
        /// </summary>
        double riverFlowRateCalk;
        /// <summary>
        /// текущий уровень свободной поверхности
        /// </summary>
        double waterLevel;
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
        IDigFunction Geometry;
        /// <summary>
        /// уровни(нь) свободной поверхности потока
        /// </summary>
        IDigFunction WaterLevels;
        /// <summary>
        /// расход потока
        /// </summary>
        IDigFunction flowRate;
        /// <summary>
        /// FlagStartMesh - первая генерация сетки
        /// </summary>
        bool FlagStartMesh = false;
        /// <summary>
        /// FlagStartMu - флаг вычисления расчет вязкости
        /// </summary>
        bool FlagStartMu = false;
        /// <summary>
        /// Шероховатость дна
        /// </summary>
        double roughness = 0.001;
        double[] tauY = null;
        double[] tauZ = null;
        double[] Coord = null;
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            Set((RiverStreamParams)p);
            InitTask();
        }
        public SrossSectionalRiverTask(RiverStreamParams p) : base(p)
        {
            Init();
        }
        /// <summary>
        /// Инициализация 
        /// </summary>
        void Init()
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
        /// Чтение входных данных задачи из файла
        /// геометрия канала
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
            InitTask();
        }
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

        public void InitTask()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            Q = rho_w * g * J;
            // получение отметок дна
            Geometry.GetFunctionData(ref bottom_x, ref bottom_y, CountBLKnots);
            // начальный уровень свободной поверхности
            waterLevel = WaterLevels.FunctionValue(0);
            // начальный расход потока
            riverFlowRate = flowRate.FunctionValue(0);
            // память под напряжения в области
            MEM.Alloc(CountKnots, ref tau, "tau");
            // генерация сетки в области
            SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
            FlagStartMesh = true;
        }
        /// <summary>
        /// Функция вычисления вязкости для заданного расхода, уровня свободной поверхности и неометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRate(double root_mu)
        {
            Mu = root_mu;
            MEM.MemSet(mu, root_mu);
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public IRiver Clone()
        {
            return new SrossSectionalRiverTask(new RiverStreamParams());
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
            Erosion = bedErosion;
            bottom_y = zeta;
            // FlagStartMesh - первая генерация сетки
            // bedErosion - генерация сетки при размывах дна
            if (FlagStartMesh == false)// || bedErosion == true)
            {
                SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
                FlagStartMesh = true;
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
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {
            // расчет уровня свободной поверхности реки
            SolveWaterLevel();
            // определение расчетной области потока и построение КЭ сетки
            SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
            // расчет гидрадинамики  (скоростей потока)
            SolveVelosity();
            // расчет  придонных касательных напряжений на дне
            tau = TausToVols(bottom_x, bottom_y);
            // сохранение данных в начальный момент времени
            //if (MEM.Equals(time, 0))
                Scan();
            time += dtime;
        }

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
        public void AddMeshPolesForGraphics(ISavePoint sp)
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

            double[] times = (from arg in evolution select arg.time).ToArray();
            double[] wls = (from arg in evolution select arg.waterLevel).ToArray();
            sp.AddCurve("Эв.св.поверхности", times, wls);
            double[] mus = (from arg in evolution select arg.Mu).ToArray();
            sp.AddCurve("Вязкость", times, mus);
            double[] tm = (from arg in evolution select arg.tauMax).ToArray();
            sp.AddCurve("Tau максимум", times, tm);
            tm = (from arg in evolution select arg.tauMid).ToArray();
            sp.AddCurve("Tau средние", times, tm);
            double[] gr = (from arg in evolution select arg.GR).ToArray();
            sp.AddCurve("Гидравл. радиус", times, gr);
            double[] ar = (from arg in evolution select arg.Area).ToArray();
            sp.AddCurve("Площадь сечения", times, ar);
            double[] rfr = (from arg in evolution select arg.riverFlowRate).ToArray();
            sp.AddCurve("Расход потока", times, rfr);
            rfr = (from arg in evolution select arg.riverFlowRateCalk).ToArray();
            sp.AddCurve("Текущий расчетный расход потока", times, rfr);
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            _tauX = tau;
        }
        /// <summary>
        /// Расчет уровня свободной поверхности реки
        /// </summary>
        private void SolveWaterLevel()
        {
            if (taskVariant == TaskVariant.flowRateFun)
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
        public void SetDataForRiverStream(double waterLevel, double[] fx, double[] fy, ref HKnot right, ref HKnot left)
        {
            this.bottom_x  = fx;
            this.bottom_y  = fy;
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
            }
            if (mesh.CountKnots != mu.Length)
            {
                double mMu = mu.Sum() / mu.Length;
                MEM.Alloc(mesh.CountKnots, ref mu, "mu");
                MEM.Alloc(mesh.CountKnots, ref nu0, "nu0");
                MEM.Alloc(mesh.CountKnots, ref U, "U");
                MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                for (int i = 0; i < mu.Length; i++)
                    mu[i] = mMu;
            }
        }
        public void ReCalkMu()
        {
            Logger.Instance.Info("вязкость потока в файле: " + Mu.ToString("F6"));
            // вычисление приведенной вязкости
            // и согласованной скорости
            Mu = DMath.RootBisectionMethod(DFRate, 0.001, 10);
            //Mu= 0.1;
            MEM.MemSet(mu, Mu);
            Logger.Instance.Info("вязкость потока ");
            Logger.Instance.Info("согласованная вязкость потока: " + Mu.ToString("F6"));
            FlagStartMu = true;
        }
        public void SolveVelosity()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            switch (ViscosityModel)
            {
                case TurbulentViscosityModel.ViscosityConst:
                    {
                        if (FlagStartMu == false)
                        {
                            ReCalkMu();
                        }
                        else
                        {
                            // расчет скорости для текущего mu и текущего уровня свободной поверхности
                            SolveU();
                        }
                        break;
                    }   
                case TurbulentViscosityModel.ViscosityWolfgangRodi:
                    {

                        double[] xx = mesh.GetCoords(0);
                        double[] yy = mesh.GetCoords(1);

                        for (int node = 0; node < mesh.CountKnots; node++)
                        {
                            double zeta = sg.spline.Value(xx[node]);
                            double H = waterLevel - zeta;
                            double z = (yy[node] - zeta);
                            double xi = z / H;
                            if (H <= MEM.Error10 || MEM.Equals(xi, 1))
                                mu[node] = rho_w * 1e-6;
                            else
                                mu[node] = rho_w * kappa * Math.Sqrt(g * H * J) * (1 - xi)*(roughness + z) + rho_w * 1e-6;
                        }
                        // расчет скорости
                        SolveU();
                        break;
                    }
                case TurbulentViscosityModel.ViscosityPrandtlKarman:
                    {
                        double[] xx = mesh.GetCoords(0);
                        double[] yy = mesh.GetCoords(1);

                        for (int node = 0; node < mesh.CountKnots; node++)
                        {
                            double zeta = sg.spline.Value(xx[node]);
                            double H = waterLevel - zeta;
                            double z = (yy[node] - zeta);
                            double xi = z  / H;
                            if (H <= MEM.Error10 || MEM.Equals(xi, 0) || MEM.Equals(xi, 1))
                                mu[node] = rho_w * 1e-6;
                            else
                                mu[node] = rho_w * kappa * Math.Sqrt(g * H * J) * (1 - xi) * z;
                        }
                        // расчет скорости
                        SolveU();
                        break;
                    }
                case TurbulentViscosityModel.Viscosity2DXY:
                    {
                        double Pv, Dv;
                        double dUx, dUy, dUUV2;
                        double[] xx = mesh.GetCoords(0);
                        double[] yy = mesh.GetCoords(1);
                        uint elem = 0;
                        // буфферезация вязкости
                        MEM.MemCpy(ref nu0, mu);
                        // цикл по нелинейности
                        for (int n = 0; n < 100; n++)
                        {
                            double SPv = 0, SDv = 0, gU = 0;
                            try
                            {
                                double[] elem_H = null;
                                double[] elem_Xi = null;
                                algebra.Clear();
                                for (elem = 0; elem < mesh.CountElements; elem++)
                                {
                                    // получить тип и узлы КЭ
                                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                                    // выбрать по типу квадратурную схему для интегрирования
                                    if (FunFormHelp.CheckFF(typeff) == 0)
                                        pIntegration = IPointsA;
                                    else
                                        pIntegration = IPointsB;
                                    // определить количество узлов на КЭ
                                    cu = knots.Length;
                                    // выделить память под локальные массивы
                                    InitLocal(cu);
                                    MEM.Alloc(cu, ref elem_H, "elem_H");
                                    MEM.Alloc(cu, ref elem_Xi, "elem_Xi");
                                    for (int node = 0; node < cu; node++)
                                    {
                                        double zeta = sg.spline.Value(xx[knots[node]]);
                                        elem_H[node] = waterLevel - zeta;
                                        if (MEM.Equals(elem_H[node], 0))
                                        {
                                            elem_Xi[node] = 0;
                                            elem_H[node] = 0.001;
                                        }
                                        else
                                            elem_Xi[node] = (yy[node] - zeta) / elem_H[node];
                                    }
                                    // получить координаты узлов
                                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);

                                    // получит вязкость в узлах
                                    mesh.ElemValues(mu, elem, ref elem_mu);
                                    mesh.ElemValues(U, elem, ref elem_U);
                                    // получить функции формы КЭ
                                    ff = FunFormsManager.CreateKernel(typeff);
                                    // передать координат узлов в функции формы
                                    ff.SetGeoCoords(elem_x, elem_y);
                                    // цикл по точкам интегрирования
                                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                                    {
                                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                                        // вычислить глобальыне производные в точках интегрирования
                                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                                        Mu = 0;
                                        dUx = 0;
                                        dUy = 0;
                                        for (int ai = 0; ai < cu; ai++)
                                        {
                                            // вычислить вязкость в точке интегрирования
                                            Mu += elem_mu[ai] * ff.N[ai];
                                            // >>>> горизональная производная 
                                            dUx += elem_U[ai] * ff.DN_x[ai];
                                            dUy += elem_U[ai] * ff.DN_y[ai];
                                        }
                                        dUUV2 = Mu * Math.Sqrt(dUx * dUx + dUy * dUy);
                                        Pv = 0;
                                        Dv = 0;
                                        for (int ai = 0; ai < cu; ai++)
                                        {
                                            Pv += (5 * kappa / sigma * rho_w + 2 * Mu0 / (elem_H[ai] * Math.Sqrt(g * elem_H[ai] * J))) * kappa * g * elem_H[ai] * J;
                                            Dv += 6 * kappa * kappa / sigma * (dUUV2 + elem_Xi[ai] * elem_Xi[ai] * elem_H[ai] * rho_w * g * J);
                                        }
                                        // вычислить глобальыне производные в точках интегрирования
                                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                                        // получть произведение якобиана и веса для точки интегрирования
                                        double DWJ = ff.DetJ * pIntegration.weight[pi];
                                        // локальная матрица жесткости                    
                                        for (int ai = 0; ai < cu; ai++)
                                            for (int aj = 0; aj < cu; aj++)
                                                LaplMatrix[ai][aj] += (Mu0 + Mu) * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                                        // Вычисление ЛПЧ

                                        for (int ai = 0; ai < cu; ai++)
                                        {
                                            LocalRight[ai] += rho_w * (Pv - Dv) * ff.N[ai] * DWJ;
                                        }
                                        SPv += Pv; SDv += Dv; gU += dUUV2;
                                    }
                                    // добавление вновь сформированной ЛЖМ в ГМЖ
                                    algebra.AddToMatrix(LaplMatrix, knots);
                                    // добавление вновь сформированной ЛПЧ в ГПЧ
                                    algebra.AddToRight(LocalRight, knots);
                                }
                                Console.WriteLine(" SPv  = {0}  SDv = {1} gU = {2}", SPv, SDv, gU);
                                //Удовлетворение ГУ
                                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                                algebra.BoundConditions(Mu0, bound);
                                bound = mesh.GetBoundKnotsByMarker(2);
                                algebra.BoundConditions(Mu0, bound);

                                algebra.Solve(ref mu);

                                foreach (var ee in mu)
                                    if (double.IsNaN(ee) == true)
                                        throw new Exception("SolveVelosity >> algebra");
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.Exception(ex);
                                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
                            }
                            // расчет скорости
                            SolveU();
                        }
                        break;
                    }

            }
            FlagStartMu = true;
        }

        /// <summary>
        /// Нахождение поля скоростей
        /// </summary>
        public double[] SolveU()
        {
            uint elem = 0;
            try
            {
                algebra.Clear();
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить тип и узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // выбрать по типу квадратурную схему для интегрирования
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // определить количество узлов на КЭ
                    cu = knots.Length;
                    // выделить память под локальные массивы
                    InitLocal(cu);
                    // получить координаты узлов
                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    // получить функции формы КЭ
                    ff = FunFormsManager.CreateKernel(typeff);
                    // передать координат узлов в функции формы
                    ff.SetGeoCoords(elem_x, elem_y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // вычислить вязкость в точке интегрирования
                        Mu = 0;
                        for (int ai = 0; ai < cu; ai++)
                            Mu += elem_mu[ai] * ff.N[ai];
                        // вычислить глобальыне производные в точках интегрирования
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // получть произведение якобиана и веса для точки интегрирования
                        double DWJ = ff.DetJ * pIntegration.weight[pi];
                        // локальная матрица жесткости                    
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] +=  Q * ff.N[ai] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                algebra.BoundConditions(0.0, bound);
                if (AxisSymmetry == 0)
                {
                    bound = mesh.GetBoundKnotsByMarker(0);
                    algebra.BoundConditions(0.0, bound);
                }
                algebra.Solve(ref U);
                foreach (var ee in U)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("SolveVelosity >> algebra");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
            
            return U;
        }
        
        
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void SolveTaus1()
        {
            double DWJ;
            uint elem = 0;
            double Mu;
            double dUx;
            double dUy;
            double _Tau;
            try
            {
                algebra.Clear();
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить тип и узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // выбрать по типу квадратурную схему для интегрирования
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // определить количество узлов на КЭ
                    int cu = knots.Length;
                    // выделить память под локальные массивы
                    InitLocal(cu);
                    // получить координаты узлов
                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    mesh.ElemValues(U, elem, ref elem_U);
                    // получить функции формы КЭ
                    ff = FunFormsManager.CreateKernel(typeff);
                    // передать координат узлов в функции формы
                    ff.SetGeoCoords(elem_x, elem_y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // вычислить глобальыне производные в точках интегрирования
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        _Tau = 0;
                        // вычислить напрядене в узлах для точки интегрирования
                        //for (int ai = 0; ai < cu; ai++)
                        //    _Tau += elem_mu[ai] * ff.N[ai] * elem_U[ai] * ff.DN_x[ai];
                        Mu = 0;
                        dUx = 0;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            // вычислить вязкость в точке интегрирования
                            Mu += elem_mu[ai] * ff.N[ai];
                            // >>>> горизональная производная 
                            dUx += elem_U[ai] * ff.DN_x[ai];
                        }
                        _Tau = Mu * dUx;
                        // получть произведение якобиана и веса для точки интегрирования
                        DWJ = ff.DetJ * pIntegration.weight[pi];
                        // локальная матрица жесткости                    
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += ff.N[ai] * ff.N[aj] * DWJ;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += _Tau * ff.N[ai] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.Solve(ref TauY);
                //for (int i = 0; i < TauY.Length; i++)
                //    TauY[i] /= rho_w;

                algebra.Clear();
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить тип и узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // выбрать по типу квадратурную схему для интегрирования
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // определить количество узлов на КЭ
                    int cu = knots.Length;
                    // выделить память под локальные массивы
                    InitLocal(cu);

                    // получить координаты узлов
                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    mesh.ElemValues(U, elem, ref elem_U);
                    // получить функции формы КЭ
                    ff = FunFormsManager.CreateKernel(typeff);
                    // передать координат узлов в функции формы
                    ff.SetGeoCoords(elem_x, elem_y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        // вычисление фф в точках интегрирования
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // вычислить глобальыне производные в точках интегрирования
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // получть произведение якобиана и веса для точки интегрирования
                        DWJ = ff.DetJ * pIntegration.weight[pi];

                        _Tau = 0;
                        // вычислить напрядене в узлах для точки интегрирования
                        Mu = 0;
                        dUy = 0;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            // вычислить вязкость в точке интегрирования
                            Mu += elem_mu[ai] * ff.N[ai];
                            // >>>> горизональная производная 
                            dUy += elem_U[ai] * ff.DN_y[ai];
                        }
                        _Tau = Mu * dUy;
                        //{
                        //    _Tau = 0;
                        //    for (int ai = 0; ai < cu; ai++)
                        //        _Tau += elem_mu[ai] * ff.N[ai] * elem_U[ai] * ff.DN_y[ai];
                        //}
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += _Tau * ff.N[ai] * DWJ;
                        // локальная матрица жесткости                    
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += ff.N[ai] * ff.N[aj] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.Solve(ref TauZ);
                //for (int i = 0; i < TauZ.Length; i++)
                //    TauZ[i] /= rho_w;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void SolveTaus0()
        {
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            double S;

            double[] tmpTausZ = new double[mesh.CountElements];
            double[] tmpTausY = new double[mesh.CountElements];

            for (uint i = 0; i < mesh.CountElements; i++)
            {
                mesh.GetElemCoords(i, ref x, ref y);
                mesh.ElemValues(U, i, ref u);
                // получит вязкость в узлах
                mesh.ElemValues(mu, i, ref elem_mu);
                //u = GetFieldElem(U, i);
                Mu = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3.0;
                //Площадь
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
            }
            uint[] knots = { 0, 0, 0 };
            algebra.Clear();
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                mesh.ElementKnots(i, ref knots);
                // площадь КЭ
                S = mesh.ElemSquare(i);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                {
                    LocalRight[j] = tmpTausZ[i] * S / 3;
                }
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauZ);

            algebra.Clear();
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                mesh.ElementKnots(i, ref knots);
                //double[] a = new double[3];
                //double[] b = new double[3];
                // площадь КЭ
                S = mesh.ElemSquare(i);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }

                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                {
                    LocalRight[j] = tmpTausY[i] * S / 3;
                }
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauY);
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void SolveTaus()
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
                    mesh.ElemValues(U, i, ref u);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, i, ref elem_mu);
                    //u = GetFieldElem(U, i);
                    Mu = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3.0;
                    //Площадь
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
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
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
        public double[] TausToVols(double[] xv, double[] yv)
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
        public double RiverFlowRate()
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
        public IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }
    }
}
