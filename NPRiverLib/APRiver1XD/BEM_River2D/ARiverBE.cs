//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 26.04.2021 Потапов И.И. 
//---------------------------------------------------------------------------
//                      выделен 26.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver1XD.BEM_River2D
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    
    using MeshLib;
    using MemLogLib;
    using FEMTasksLib;
    using CommonLib;
    using AlgebraLib;
    using CommonLib.IO;
    using CommonLib.ChannelProcess;
    using GeometryLib;
    using NPRiverLib.APRiver_1XD;
    using MeshAdapterLib;

    [Serializable]
    public abstract class ARiverBEM1XD : APRiver1XD<RiverBEMParams1XD>, IRiver
    {

        #region Параметры задачи
        //public void LoadParams(string fileName = "")
        //{
        //    string message = "Файл парамеров задачи - доные деформации - не обнаружен";
        //    WR.LoadParams(Load, message, fileName);
        //}
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            Params = new RiverBEMParams1XD();
            Params.Load(file);
        }
        #endregion
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// </summary>
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames(GetFormater());
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameBEM_Params.txt";
            fn.NameRData = "NameBEM_Params.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            return fn;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh() => new TwoMesh(fxW, fyW);
        ///// <summary>
        ///// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        ///// </summary>
        //public IUnknown[] Unknowns() => unknowns;
        //protected IUnknown[] unknowns = { new Unknown("Функция тока на границе", null, TypeFunForm.Form_1D_L1),
        //                                  new Unknown("Скорость на границе", null, TypeFunForm.Form_1D_L1) };
        #region Рабочие массивы и поля
        /// <summary>
        /// Параметрическая геометрия контура 
        /// </summary>
        public double[] fxW = null;
        public double[] fyW = null;
        /// <summary>
        /// Параметрическая геометрия контура обтекаемого тела
        /// </summary>
        public double[] fxB = null;
        public double[] fyB = null;
        /// <summary>
        /// Параметрическая геометрия контура обтекаемого тела
        /// </summary>
        public double[] fxСircle = null;
        public double[] fyСircle = null;
        /// <summary>
        /// Параметрическое задание геометрии для Х координат эллипса
        /// </summary>
        public PSpline GeometriaXW = null;
        /// <summary>
        /// Параметрическое задание геометрии для Y координат эллипса
        /// </summary>
        public PSpline GeometriaYW = null;
        /// <summary>
        /// Параметрическое задание геометрии для Х координат эллипса
        /// </summary>
        public PSpline GeometriaXB = null;
        /// <summary>
        /// Параметрическое задание геометрии для Y координат эллипса
        /// </summary>
        public PSpline GeometriaYB = null;
        /// <summary>
        /// Коэффициенты ядра для вычисления функции Грина
        /// </summary>
        protected double[] BettaW = null;
        protected double[] dXdZetaW = null;
        protected double[] dYdZetaW = null;
        protected double[] BettaB = null;
        protected double[] dXdZetaB = null;
        protected double[] dYdZetaB = null;
        /// <summary>
        /// Матрица задачи
        /// </summary>
        protected double[][] Matrix = null;
        protected double[] R = null;
        protected double[] FF = null;
        protected uint[] knots = null;
        protected uint N;
        /// <summary>
        /// Блоки матрицы задачи
        /// </summary>
        protected double[,] Aww = null, Awb = null, Abw = null, Abb = null;
        protected double[,] Bww, Bwb, Bbw, Bbb = null;
        /// <summary>
        /// приращение вдоль кривой контура (натуральная координата контура)
        /// </summary>
        protected double[] detJW = null;
        protected double[] detJB = null;
        /// <summary>
        /// Количество узлов в контуре
        /// </summary>
        protected int NNw = 0;
        protected int NNb = 0;
        protected int Count = 0;
        //public TwoMesh meshW = null;
        //protected TwoMesh meshB = null;
        /// <summary>
        /// Период по дну
        /// </summary>
        protected double bottomPeriod;
        /// <summary>
        /// Циркуляция
        /// </summary>
        protected double Gamma = 0;

        protected Green green;
        /// <summary>
        /// волновое число для дна
        /// </summary>
        //protected double lambda;
        //protected double lambda2;
        //protected double lambda4L2;

        protected double[] VCC = null;
        /// <summary>
        /// локальная скорость на цилиндрее
        /// </summary>
        protected double[] VCircle = null;

        protected double[] VC;
        protected double[] SV;
        protected double[] tauX;

        protected List<double> gammaList = new List<double>();
        protected List<double> timeList = new List<double>();

        protected double TwoL, sAlpha, Log2;
        /// <summary>
        /// Смещение точки сгущения
        /// </summary>
        protected double a0 = 0;
        #endregion
        /// <summary>
        /// Конструктор
        /// </summary>
        public ARiverBEM1XD(RiverBEMParams1XD p) :base(p) 
        {
        }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            InitTask();
        }
        /// <summary>
        /// Загрузка задачи иp форматного файла
        /// </summary>
        /// <param name="file"></param>
        public override void LoadData(StreamReader file)
        {
            //Geometry = new DigFunction();
            //Geometry.Load(file);
            //Geometry.GetFunctionData(ref x, ref zeta, Params.CountKnots);
            //Set(mesh, null);
            InitTask();
        }

        public virtual bool LoadData(string fileName)
        {
            return false;
        }
        /// <summary>
        /// Метод выделения ресурсов задачи после установки свойств задачи
        /// </summary>
        public virtual void InitTask()
        {
            // выделение памяти
            InitAreaTask();
            // Определение начальной формы цилиндра и дна
            InitGeomenryTask();
            // Вычисляемые параметры геометрии
            ClakGeomenryParams();
        }
        /// <summary>
        /// выделение памяти
        /// </summary>
        public void InitAreaTask()
        {
            green = new Green();
            Log2 = Math.Log(2);

            NNw = Params.NW; 
            NNb = Params.NB; 
            // Начальная информация о форме дна
            MEM.Alloc(NNw, ref fxW, "fxW");
            MEM.Alloc(NNw, ref fyW, "fyW");
            // Начальная информация о форме тела (цилиндра)
            MEM.Alloc(NNb, ref fxB, "fxB");
            MEM.Alloc(NNb, ref fyB, "fyB");
            MEM.Alloc(NNb, ref fxСircle, "fxСircle");
            MEM.Alloc(NNb, ref fyСircle, "fyСircle");
            MEM.Alloc(NNw, ref dXdZetaW, "dXdZetaW");
            MEM.Alloc(NNw, ref detJW, "detJW");
            MEM.Alloc(NNb, ref detJB, "detJB");
            MEM.Alloc(NNb, ref VCircle, "VCircle");

            // вычисление матрицы системы
            MEM.Alloc2D(NNw, NNw, ref Aww);
            MEM.Alloc2D(NNw, NNb, ref Awb);
            MEM.Alloc2D(NNb, NNw, ref Abw);
            MEM.Alloc2D(NNb, NNb, ref Abb);

            Count = NNw + NNb;
            // вектора скоростей и напряжений
            MEM.Alloc<double>(Count, ref VC);
            MEM.Alloc<double>(Count, ref SV);
            MEM.Alloc<double>(Count, ref tauX);

            N = (uint)(Count + 2);
            if (algebra == null)
                algebra = new AlgebraGauss(N);
            MEM.Alloc2D((int)N, (int)N, ref Matrix);
            MEM.Alloc(N, ref R);
            MEM.Alloc(N, ref FF);
            MEM.Alloc(N, ref knots);
            for (uint j = 0; j < N; j++)
                knots[j] = j;

            double dx = 1.0 / NNb;
            for (uint i = 0; i < Params.NB; i++)
            {
                double x = dx * (i + 1);
                fxСircle[i] = Params.XC + Params.RC * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
                fyСircle[i] = Params.YC + Params.RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
            }
        }
        /// <summary>
        /// Определение начальной формы цилиндра и дна
        /// </summary>
        public virtual void InitGeomenryTask(double scale = 1)
        {
            InitBattomGeomenry();
            InitBodyGeomenry(scale);
        }

        /// <summary>
        ///  вычисление геометрии дна
        /// </summary>
        public virtual void InitBattomGeomenry()
        {
            double B1 = -0.1;
            double dx;
            // равномерная сетка
            if (Params.MeshType == true)
            {
                dx = Params.LW * 1.0 / NNw;
                for (uint i = 0; i < Params.NW; i++)
                {
                    double x = dx * (i + 1);
                    fxW[i] = x;
                    double V = (fxW[i] - (Params.XC + Params.RC * 0.1));
                    fyW[i] = B1 * Math.Exp(-V * V / 4);
                }

            }
            else // сетка со сгущением в точке 0
            {
                for (uint i = 0; i < Params.NW; i++)
                {
                    double xi = 1.0 * i / (NNw - 1);
                    fxW[i] = Params.LW * FS(xi);
                    double V = (fxW[i] - (Params.XC + Params.RC * 0.1));
                    fyW[i] = B1 * Math.Exp(-V * V / 4);
                }
            }
        }

        /// <summary>
        ///  вычисление геометрии обтекаемого тела
        /// </summary>
        public virtual void InitBodyGeomenry(double scale = 1)
        {
            double dx = 1.0 / NNb;
            for (uint i = 0; i < Params.NB; i++)
            {
                double x = dx * (i + 1);
                fxB[i] = Params.XC + Params.RC * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
                fyB[i] = Params.YC + Params.RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
            }
        }        
        /// <summary>
        /// Вычисляемые параметры геометрии
        /// </summary>
        public void ClakGeomenryParams()
        {
            initB(fxB, fyB);
            initW(fxW, fyW);
            // bottomPeriod - период дна, при проходе по контуру координата по х вдоль дна вырастет на это значение
            bottomPeriod = fxW.Max() - fxW.Min();
            TwoL = 2.0 / bottomPeriod;
            sAlpha = Math.PI / bottomPeriod;
        }

        /// <summary>
        /// Функция сжатия
        /// </summary>
        public double FS(double a)
        {
            return a + Params.AmpBottom / (2 * Math.PI) * Math.Sin(2 * Math.PI * (a - a0));
        }
        bool flagPrint = true;

        #region Эксеримент Mau
        double[] _x = { -0.98065, -0.96131, -0.94196, -0.92261, -0.90327, -0.88392, -0.86457, -0.84523, -0.82588, -0.80653, -0.78719,
            -0.76784, -0.74849, -0.72915, -0.70980, -0.69045, -0.67111, -0.65176, -0.63241, -0.61307, -0.59372, -0.57437, -0.55503, -0.53568,
                -0.51633, -0.49698, -0.47764, -0.45829, -0.43894, -0.41960, -0.40025, -0.38090, -0.36156, -0.34221, -0.32286, -0.30352, -0.28417,
                -0.26482, -0.24548, -0.22613, -0.20678, -0.18744, -0.16809, -0.14874, -0.12940, -0.11005, -0.09070, -0.07136, -0.05201, -0.03266,
                -0.01332, 0.00603, 0.02538, 0.04472, 0.06407, 0.08342, 0.10276, 0.12211, 0.14146, 0.16080, 0.18015, 0.19950, 0.21884, 0.23819,
                0.25754, 0.27688, 0.29623, 0.31558, 0.33492, 0.35427, 0.37362, 0.39296, 0.41231, 0.43166, 0.45101, 0.47035, 0.48970, 0.50905,
                0.52839, 0.54774, 0.56709, 0.58643, 0.60578, 0.62513, 0.64447, 0.66382, 0.68317, 0.70251, 0.72186, 0.74121, 0.76055, 0.77990,
                0.79925, 0.81859, 0.83794, 0.85729, 0.87663, 0.89598, 0.91533, 0.93467, 0.95402, 0.97337, 0.99271, 1.01206, 1.03141, 1.05075,
                1.07010, 1.08945, 1.10879, 1.12814, 1.14749, 1.16683, 1.18618, 1.20553, 1.22487, 1.24422, 1.26357, 1.28291, 1.30226, 1.32161,
                1.34095, 1.36030, 1.37965, 1.39899, 1.41834, 1.43769, 1.45704, 1.47638, 1.49573, 1.51508, 1.53442, 1.55377, 1.57312, 1.59246,
                1.61181, 1.63116, 1.65050, 1.66985, 1.68920, 1.70854, 1.72789, 1.74724, 1.76658, 1.78593, 1.80528, 1.82462, 1.84397, 1.86332,
                1.88266, 1.90201, 1.92136, 1.94070, 1.96005, 1.97940, 1.99874, 2.01809, 2.03744, 2.05678, 2.07613, 2.09548, 2.11482, 2.13417,
                2.15352, 2.17286, 2.19221, 2.21156, 2.23090, 2.25025, 2.26960, 2.28894, 2.30829, 2.32764, 2.34698, 2.36633, 2.38568, 2.40503,
                2.42437, 2.44372, 2.46307, 2.48241, 2.50176, 2.52111, 2.54045, 2.55980, 2.57915, 2.59849, 2.61784, 2.63719, 2.65653, 2.67588,
                2.69523, 2.71457, 2.73392, 2.75327, 2.77261, 2.79196, 2.81131, 2.83065, 2.85000 };

        double[] _z0 = { 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, -0.00620, -0.00947, -0.00986, -0.00791, -0.00218, 0.00407,
                0.00846, 0.00994, 0.00844, 0.00339, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000  };

        double[] _z90 = { 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, -0.00055, -0.00205, -0.00492, -0.01176, -0.01945, -0.02800, -0.03644, -0.04257, -0.04545, -0.04338, -0.03834,
                -0.02649, -0.01374, -0.00134, 0.01068, 0.02259, 0.03449, 0.04096, 0.03707, 0.02726, 0.01676, 0.00926, 0.00399, 0.00022, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000 };

        double[] _z150 = { 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, -0.00065, -0.00182, -0.00463, -0.00962, -0.01922, -0.02765, -0.03570, -0.04360, -0.04741, -0.04982, -0.04726, -0.04191,
                -0.03330, -0.02407, -0.01146, 0.00169, 0.01330, 0.02475, 0.03453, 0.04381, 0.05223, 0.05405, 0.05115, 0.03788, 0.02364, 0.01532,
                0.01056, 0.00628, 0.00246, 0.00135, 0.00036, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
                0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000 };


        double[] _z300 = { 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00002, 0.00007,
            0.00013, 0.00019, 0.00026, 0.00032, 0.00038, 0.00044, 0.00050, 0.00056, 0.00063, 0.00069, 0.00075, 0.00113, 0.00223, 0.00088,
            -0.00130, -0.00429, -0.00755, -0.01169, -0.02016, -0.02965, -0.03887, -0.04729, -0.05376, -0.05624, -0.05697, -0.05366, -0.04793,
            -0.04069, -0.03127, -0.02138, -0.00971, 0.00323, 0.01327, 0.02499, 0.03470, 0.04172, 0.04570, 0.04795, 0.04928, 0.04973, 0.04788,
            0.04232, 0.03436, 0.02510, 0.01532, 0.00628, 0.00125, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000, 0.00000,
            0.00000, 0.00000, 0.00000, 0.00000, 0.00000 };

        #endregion

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (gammaList.Count>2)
                sp.AddCurve("Циркуляция", timeList.ToArray(), gammaList.ToArray());
            if (flagPrint == true)
            {
                
                for (int i = 0; i < _x.Length; i++)
                {
                    _x[i] = _x[i]  + 1.5;
                }

                sp.AddCurve("Труба", fxB, fyB);
                sp.AddCurve("Дно - эксперимент 0 c", _x, _z0);
                sp.AddCurve("Дно - эксперимент 90 c", _x, _z90);
                sp.AddCurve("Дно - эксперимент 150 c", _x, _z150);
                sp.AddCurve("Дно - эксперимент 300 c", _x, _z300);
                flagPrint = false;
            }
            sp.AddCurve("Обтекаемая форма", fxB, fyB);
            sp.AddCurve("Касательная скорость потока дне", fxW, SV);
            sp.AddCurve("Дно", fxW, fyW);
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
       // public IMesh Mesh() => new TwoMesh(fxW, fyW);
        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            if (algebra != null)
                this.algebra = algebra;
            TwoMesh m = null;
            if (mesh != null)
                m = mesh as TwoMesh;
            if (m != null)
            {
                if (m != null)
                {
                    fxW = m.GetCoords(0);
                    fyW = m.GetCoords(1);
                    Params.NW = fxW.Length;
                }
            }
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            if (zeta != null)
            {
                Erosion = bedErosion;
                fyW = zeta;
                initW(fxW, fyW);
                // bottomPeriod - период дна, при проходе по контуру координата по х вдоль дна вырастет на это значение
                bottomPeriod = fxW.Max() - fxW.Min();
                TwoL = 2.0 / bottomPeriod;
                sAlpha = Math.PI / bottomPeriod;
                mesh = MeshAdapter.CreateFEMesh(fxW, fyW, fxB, fyB, Params.HPhi, Params.LPhi);
            }
        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetZeta(ref double[] zeta)
        {
            zeta = fyW;
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            tauX = this.tauX;
            tauY = null;
            P = null;
        }
        #region реализации метода граничных элементов высокого порядка
        /// <summary>
        /// Расчет геометрии донной поверхности
        /// </summary>
        /// <param name="fxW"></param>
        /// <param name="fyW"></param>
        protected void initW(double[] fxW, double[] fyW)
        {
            this.fxW = fxW;
            this.fyW = fyW;
            // Формирование правых частей
            GeometriaXW = new PSpline(fxW);
            GeometriaYW = new PSpline(fyW);
            dXdZetaW = GeometriaXW.DiffSplineFunction();
            dYdZetaW = GeometriaYW.DiffSplineFunction();
            // приращение вдоль кривой
            for (int i = 0; i < NNw; i++)
                detJW[i] = Math.Sqrt(dXdZetaW[i] * dXdZetaW[i] + dYdZetaW[i] * dYdZetaW[i]);
            BettaW = CalkBetta(NNw);
        }
        /// <summary>
        /// Расчет геометрии обтекаемого тела
        /// </summary>
        /// <param name="fxB"></param>
        /// <param name="fyB"></param>
        protected void initB(double[] fxB, double[] fyB)
        {
            this.fxB = fxB;
            this.fyB = fyB;
            // Формирование правых частей
            GeometriaXB = new PSpline(fxB);
            GeometriaYB = new PSpline(fyB);
            dXdZetaB = GeometriaXB.DiffSplineFunction();
            dYdZetaB = GeometriaYB.DiffSplineFunction();
            // приращение вдоль кривой
            for (int i = 0; i < NNb; i++)
                detJB[i] = Math.Sqrt(dXdZetaB[i] * dXdZetaB[i] + dYdZetaB[i] * dYdZetaB[i]);
            // Вычисление бета вектора
            BettaB = CalkBetta(NNb);
        }
        /// ======================================
        protected void CalkMatrixWW()
        {
            green.Set(fxW, fyW, fxW, fyW, detJW, bottomPeriod);

            Parallel.For(0, NNw, (i, state) =>
            {
                    //for (int i = 0; i < NNw; i++)
                    for (int j = 0; j < NNw; j++)
                {
                    int idx = Math.Abs(i - j);
                    Aww[i, j] = -detJW[j] / NNw * (BettaW[idx] + green.Get(i, j));
                }
            });
        }
        protected void CalkMatrixWB()
        {
            green.Set(fxB, fyB, fxW, fyW, detJW, bottomPeriod);
            Parallel.For(0, NNw, (i, state) =>
            {
                //for (int i = 0; i < NNw; i++)
                for (int j = 0; j < NNb; j++)
                {
                    Awb[i, j] = -detJB[j] / NNb * green.Get(i, j);
                }
            });
        }
        protected void CalkMatrixBW()
        {
            green.Set(fxW, fyW, fxB, fyB, detJB, bottomPeriod);
            //double[,] A = MEM.NewMassRec2D<double>(NNb, NNw);
            Parallel.For(0, NNb, (i, state) =>
            {
                //for (int i = 0; i < NNb; i++)
                for (int j = 0; j < NNw; j++)
                {
                    Abw[i, j] = -detJW[j] / NNw * green.Get(i, j);
                }
            });
        }
        protected void CalkMatrixBB()
        {
            green.Set(fxB, fyB, fxB, fyB, detJB, bottomPeriod);
            Parallel.For(0, NNb, (i, state) =>
            {
                //   for (int i = 0; i < NNb; i++)
                for (int j = 0; j < NNb; j++)
                {
                    int idx = Math.Abs(i - j);
                    Abb[i, j] = -detJB[j] / NNb * (BettaB[idx] + green.Get(i, j));
                }
            });
        }
        /// <summary>
        /// Вычисление коэффициентов квадратурных формул
        /// </summary>
        /// <param name="N">Количество узлов</param>
        /// <returns>коэффициентов квадратурных формул</returns>
        protected double[] CalkBetta(int N)
        {
            int M = (int)(N / 2) - 1;
            double[] Betta = new double[N];
            double Alpha = 0;
            double log2 = Math.Log(2.0);
            double ztak = -1.0;
            double Sum;

            for (int k = 0; k < N; k++)
            {
                ztak = -1 * ztak;
                Sum = 0;
                if (k == 0)
                {
                    for (int m = 1; m <= M; m++)
                        Sum += 1.0 / m;
                    Betta[0] += -(log2 + ztak / N + Sum);
                }
                else
                {
                    for (int m = 1; m <= M; m++)
                        Sum += Math.Cos(2.0 * Math.PI * k * m / N) / m;
                    Alpha = -(log2 + ztak / N + Sum);
                    double sn = Math.Sin(k * Math.PI / N);
                    double ln = Math.Log(Math.Abs(sn));
                    Betta[k] += -ln + Alpha;
                }
            }
            return Betta;
        }
        #endregion

        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public abstract void SolverStepForGamma(double Gamma);
        /// <summary>
        /// Поиск циркуляции
        /// </summary>
        /// <param name="Gamma"></param>
        public abstract double LookingGamma();
    }
}
