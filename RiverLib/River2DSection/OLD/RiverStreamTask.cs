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
    using MemLogLib;
    using GeometryLib;
    using MeshLib;
    using MeshGeneratorsLib;
    using System;
    using System.IO;
    using System.Linq;
    using CommonLib.EConverter;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;
    using CommonLib.Geometry;
    using CommonLib.Function;
    using MeshGeneratorsLib.StripGenerator;


    /// <summary>
    ///  ОО: Определение класса RiverStreamTask - расчет полей скорости 
    ///             и напряжений в живом сечении потока
    /// </summary>
    [Serializable]
    public class RiverStreamTask : RiverStreamParams, IRiver
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
        public IBoundaryConditions BoundCondition() => null;

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
        public string Name { get => "гидрадинамика створа в прямой пойме TriMesh"; }
        /// <summary       
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamY1D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string VersionData() => "RiverStreamTask 01.09.2021"; 
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
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] LaplMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        private double[] LocalRight = { 0, 0, 0 };
        /// <summary>
        /// Генератор КЭ сетки в ствое задачи
        /// </summary>
        HStripMeshGeneratorTri sg = new HStripMeshGeneratorTri();
        /// <summary>
        /// Длина смоченного периметра
        /// </summary>
        double GR = 0;
        /// <summary>
        /// правая часть уравнения
        /// </summary>
        private double Q;
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh() { return new TwoMesh(x, y); }
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        double[] x;
        /// <summary>
        /// координаты дна по оси у
        /// </summary>
        double[] y;
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
        /// текущий уровень свободной поверхности
        /// </summary>
        double WaterLevel;
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
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            Set((RiverStreamParams)p);
            InitTask();
        }
        public RiverStreamTask(RiverStreamParams p) : base(p)
        {
            Init();
        }
        /// <summary>
        /// Инициализация 
        /// </summary>
        void Init()
        {
            this.cu = 3;
            LaplMatrix = new double[cu][];
            for (int i = 0; i < cu; i++)
                LaplMatrix[i] = new double[cu];
            // геометрия дна
            Geometry = new DigFunction();
            // свободная поверхность
            WaterLevels = new DigFunction();
            // расход потока
            flowRate = new DigFunction();
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
        public void InitTask()
        {
            double rho_w = SPhysics.rho_w;
            double g = SPhysics.GRAV;

            Q = g * rho_w * J;
            // получение отметок дна
            Geometry.GetFunctionData(ref x, ref y, CountBLKnots);
            // начальный уровень свободной поверхности
            WaterLevel = WaterLevels.FunctionValue(0);
            // начальный расход потока
            riverFlowRate = flowRate.FunctionValue(0); 
            // память под напряжения в области
            MEM.Alloc<double>(CountKnots, ref tau, "tau");
            // генерация сетки в области
            SetDatForRiverStream(WaterLevel, x, y, ref right, ref left);
            FlagStartMesh = true;
            Logger.Instance.Info("вязкость потока в файле: " + Mu.ToString("F6"));
            // вычисление приведенной вязкости
            Mu = DMath.RootBisectionMethod(DFRate, 0.0001, 10);
            Logger.Instance.Info("вязкость потока ");
            Logger.Instance.Info("согласованная вязкость потока: " + Mu.ToString("F6"));
        }
        /// <summary>
        /// Функция вычисления вязкости для заданного расхода, уровня свободной поверхности и неометрии
        /// </summary>
        /// <param name="mu"></param>
        /// <returns></returns>
        public double DFRate(double mu)
        {
            Mu = mu;
            SolveVelosity();
            return (RiverFlowRate() - riverFlowRate)/ riverFlowRate;
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
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public IRiver Clone()
        {
            return new RiverStreamTask(new RiverStreamParams());
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
            y = zeta;
            // FlagStartMesh - первая генерация сетки
            // bedErosion - генерация сетки при размывах дна
            if (FlagStartMesh == false || bedErosion != EBedErosion.NoBedErosion)
            {
                SetDatForRiverStream(WaterLevel, x, y, ref right, ref left);
                FlagStartMesh = true;
            }
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {
            zeta = y;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {
            // расчет урvовня свободной поверхности реки
            SolveWaterLevel();
            // определение расчетной области потока и построение КЭ сетки
            SetDatForRiverStream(WaterLevel, x, y, ref right, ref left);
            // расчет гидрадинамики  (скоростей потока)
            SolveVelosity();
            // расчет  придонных касательных напряжений на дне
            tau = TausToVols(x, y);
            time += dtime;
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {
            // поля на сетке
            sp.Add("U", U);
            sp.Add("TauY", TauY);
            sp.Add("TauZ", TauZ);
            // векторное поле на сетке
            sp.Add("Tau", TauY, TauZ);
            // кривые 
            // дно - берег
            sp.AddCurve("Русловой профиль", x, y);
            double[] xwl = { left.x, right.x };
            double[] ywl = { left.y, right.y };
            // свободная поверхность
            sp.AddCurve("Свободная поверхность", xwl, ywl);
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
            tauX = tau;
        }
        /// <summary>
        /// Расчет уровня свободной поверхности реки
        /// </summary>
        private void SolveWaterLevel()
        {
            if (taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = flowRate.FunctionValue(time);
                double Ck = 0.1;
                double Umax = 5;
                double rfRate = RiverFlowRate();
                double deltaH = (rfRate - riverFlowRate) / riverFlowRate;
                double W = x[x.Length - 1] - x[0];
                double H = rfRate / (W * Umax) * deltaH;
                double dH = Ck * dtime * H;
                if (Math.Abs(dH) > 0.005)
                    dH = Math.Sign(dH) * 0.005;
                WaterLevel = WaterLevel - dH;
            }
            else
            {
                WaterLevel = WaterLevels.FunctionValue(time);
            }
        }
        /// <summary>
        /// Установка параметров задачи
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="mesh"></param>
        public void SetDatForRiverStream(double WaterLevel, double[] x, double[] y, ref HKnot right, ref HKnot left)
        {
            this.x = x;
            this.y = y;
            // генерация сетки
            mesh = sg.CreateMesh(ref GR,WaterLevel, x, y);
            right = sg.Right();
            left = sg.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);
            U = new double[mesh.CountKnots];
            TauZ = new double[mesh.CountKnots];
            TauY = new double[mesh.CountKnots];
        }
        /// <summary>
        /// Нахождение поля скоростей
        /// </summary>
        public double[] SolveVelosity()
        {
            algebra.Clear();
            uint[] knots = { 0, 0, 0 };
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                // узлы
                mesh.ElementKnots(i, ref knots);
                //Координаты и площадь
                mesh.GetElemCoords(i, ref x, ref y);
                //Площадь
                double S = mesh.ElemSquare(i);

                // Градиенты от функций форм
                double[] a = new double[cu];
                double[] b = new double[cu];
                a[0] = (y[1] - y[2]);
                b[0] = (x[2] - x[1]);
                a[1] = (y[2] - y[0]);
                b[1] = (x[0] - x[2]);
                a[2] = (y[0] - y[1]);
                b[2] = (x[1] - x[0]);

                // Вычисление ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                        LaplMatrix[ai][aj] = Mu * (a[ai] * a[aj] + b[ai] * b[aj]) / (4 * S);

                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);

                // Вычисление ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = Q * S / 3;

                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            //Удовлетворение ГУ
            uint[] bound = mesh.GetBoundKnotsByMarker(1);
            algebra.BoundConditions(0.0, bound);

            algebra.Solve(ref U);

            foreach (var ee in U)
                if (double.IsNaN(ee) == true)
                    throw new Exception("SolveVelosity >> algebra");

            return U;
        }
        /// <summary>
        /// Отчетная информация о речном процессе
        /// </summary>
        /// <returns></returns>
        public ReportDataRiver GetRiverReportData()
        {
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            ReportDataRiver rrd = new ReportDataRiver();
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                //Координаты и площадь
                mesh.GetElemCoords(i, ref x, ref y);
                mesh.ElemValues(U, i, ref u);

                double su = u.Sum() / 3;
                //Площадь
                double S = mesh.ElemSquare(i);
                rrd.area += S;
                //private double Ekin, Epot;
                //
                double xc = x.Sum() / 3;
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
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            double su, S;
            double riverFlowRate = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                //Координаты и площадь
                //mesh.ElemX(i, ref x);
                //mesh.ElemY(i, ref y);
                mesh.ElemValues(U, i, ref u);
                //u = GetFieldElem(U, i);
                // средняя скорость на КЭ
                su = u.Sum() / 3;
                // площадь КЭ
                S = mesh.ElemSquare(i);
                riverFlowRate += S * su;
            }
            if (double.IsNaN(riverFlowRate) == true)
                throw new Exception("riverFlowRate NaN");
            return riverFlowRate;
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void SolveTaus()
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
                //u = GetFieldElem(U, i);
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
            double[] tau = new double[xv.Length - 1];
            // расчет напряжений Txy  Txz
            SolveTaus();
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(1);
            // количество узлов на нижней границе области
            TSpline tauSplineZ = new TSpline();
            TSpline tauSplineY = new TSpline();
            double[] tauZ = new double[bounds.Length];
            double[] tauY = new double[bounds.Length];
            double[] Coord = new double[bounds.Length];
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
            return tau;
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
