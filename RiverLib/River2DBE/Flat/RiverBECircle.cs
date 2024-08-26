////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////            перенесено с правкой : 26.04.2021 Потапов И.И. 
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using AlgebraLib;
//    using CommonLib;
//    using CommonLib.IO;
//    using CommonLib.Math;
//    using FEMTasksLib;
//    using MemLogLib;
//    using MeshLib;
//    using System;
//    using System.Linq;
//    [Serializable]
//    public class RiverBECircle : RiverBECircleParams, IRiver
//    {
//        /// <summary>
//        /// Имена файлов с данными для задачи гидродинамики
//        /// </summary>
//        public TaskFileNames taskFileNemes()
//        {
//            TaskFileNames fn = new TaskFileNames();
//            fn.NameCPParams = "NameCPParams.txt";
//            fn.NameBLParams = "NameBLParams.txt";
//            fn.NameRSParams = "NameBERSParams.txt";
//            fn.NameRData = "NameRData.txt";
//            fn.NameEXT = "(*.tsk)|*.tsk|";
//            return fn;
//        }
//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public IRiver Clone()
//        {
//            return new RiverBECircle(new RiverBECircleParams());
//        }
//        /// <summary>
//        /// Наименование задачи
//        /// </summary>
//        public string Name { get => "гидрадинамика потока под трубой (МГЭ)"; }
//        /// <summary       
//        /// Тип задачи используется для выбора совместимых подзадач
//        /// </summary>
//        public TypeTask typeTask { get => TypeTask.streamX1D; }
//        /// <summary>
//        /// версия дата последних изменений интерфейса задачи
//        /// </summary>
//        public string VersionData() => "RiverBECircle 01.09.2021/14.02.2022"; 
//        /// <summary>
//        /// Текущее время
//        /// </summary>
//        public double time { get; set; }
//        /// <summary>
//        /// Текущий шаг по времени
//        /// </summary>
//        public double dtime { get; set; }
//        /// <summary>
//        /// Параметрическая геометрия контура 
//        /// </summary>
//        public double[] fxW = null;
//        public double[] fyW = null;
//        /// <summary>
//        /// Параметрическая геометрия контура 
//        /// </summary>
//        public double[] fxB = null;
//        public double[] fyB = null;
//        /// <summary>
//        /// Параметрическое задание геометрии для Х координат эллипса
//        /// </summary>
//        public PSpline GeometriaXW = null;
//        /// <summary>
//        /// Параметрическое задание геометрии для Y координат эллипса
//        /// </summary>
//        public PSpline GeometriaYW = null;
//        /// <summary>
//        /// Параметрическое задание геометрии для Х координат эллипса
//        /// </summary>
//        public PSpline GeometriaXB = null;
//        /// <summary>
//        /// Параметрическое задание геометрии для Y координат эллипса
//        /// </summary>
//        public PSpline GeometriaYB = null;
//        /// <summary>
//        /// Коэффициенты ядра для вычисления функции Грина
//        /// </summary>
//        protected double[] BettaW = null;
//        protected double[] dXdZetaW = null;
//        protected double[] dYdZetaW = null;
//        protected double[] BettaB = null;
//        protected double[] dXdZetaB = null;
//        protected double[] dYdZetaB = null;
//        /// <summary>
//        /// Матрица задачи
//        /// </summary>
//        protected double[][] Matrix = null;
//        protected double[] R = null;
//        protected double[] FF = null;
//        protected uint[] knots = null;
//        protected uint N;
//        [NonSerialized]
//        IAlgebra algebra = null;
//        /// <summary>
//        /// Блоки матрицы задачи
//        /// </summary>
//        protected double[,] Aww, Awb, Abw, Abb = null;
//        protected double[,] Bww, Bwb, Bbw, Bbb = null;
//        /// <summary>
//        /// приращение вдоль кривой контура (натуральная координата контура)
//        /// </summary>
//        protected double[] detJW = null;
//        protected double[] detJB = null;
//        /// <summary>
//        /// Количество узлов в контуре
//        /// </summary>
//        protected int NNw = 0;
//        protected int NNb = 0;
//        protected int Count = 0;
//        //public TwoMesh meshW = null;
//        //protected TwoMesh meshB = null;
//        /// <summary>
//        /// Период по дну
//        /// </summary>
//        protected double h;
//        /// <summary>
//        /// Циркуляция
//        /// </summary>
//        protected double Gamma = 0;
//        /// <summary>
//        /// волновое число для дна
//        /// </summary>
//        protected double lambda;
//        protected double lambda2;
//        protected double lambda4L2;

//        double[] VCC = null;
//        /// <summary>
//        /// локальная скорость на цилиндрее
//        /// </summary>
//        double[] VCircle = null;

//        double[] VC;
//        double[] SV;
//        double[] tauX;
//        /// <summary>
//        /// Смещение точки сгущения
//        /// </summary>
//        double a0 = 0;
//        /// <summary>
//        /// Установка свойств задачи
//        /// </summary>
//        /// <param name="p"></param>
//        public void SetParams(object p)
//        {
//            base.SetParams((RiverBECircleParams)p);
//            InitTask();
//        }
//        /// <summary>
//        /// Конструктор
//        /// </summary>
//        public RiverBECircle(RiverBECircleParams p) : base(p)
//        {
//            InitTask();
//        }
//        /// <summary>
//        /// Метод выделения ресурсов задачи после установки свойств задачи
//        /// </summary>
//        public void InitTask()
//        {
//            NNw = NW; // meshW.Count;
//            NNb = NB; // meshB.Count;
//            // Начальная информация о форме дна
//            MEM.Alloc(NNw, ref fxW, "fxW");
//            MEM.Alloc(NNw, ref fyW, "fyW");
//            // Начальная информация о форме тела (цилиндра)
//            MEM.Alloc(NNb, ref fxB, "fxB");
//            MEM.Alloc(NNb, ref fyB, "fyB");
//            MEM.Alloc(NNw, ref dXdZetaW, "dXdZetaW");
//            MEM.Alloc(NNw, ref detJW, "detJW");
//            MEM.Alloc(NNb, ref detJB, "detJB");
//            MEM.Alloc(NNb, ref VCircle, "VCircle");
//            // равномерная сетка
//            if (MeshType == true)
//            {
//                double dx = LW * 1.0 / NNw;
//                for (uint i = 0; i < NW; i++)
//                {
//                    double x = dx * (i + 1);
//                    fxW[i] = x;
//                    fyW[i] = 0;
//                }
//                dx = 1.0 / NNb;
//                for (uint i = 0; i < NB; i++)
//                {
//                    double x = dx * (i + 1);
//                    fxB[i] = XC + RC * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
//                    fyB[i] = YC + RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
//                }
//            }
//            else
//            {
//                //double dx = LW * 1.0 / NNw;
//                //Console.Clear();
//                for (uint i = 0; i < NW; i++)
//                {
//                    double xi = 1.0 * i / (NNw - 1);
//                    fxW[i] = LW * FS(xi);
//                    fyW[i] = 0;
//                   // Console.WriteLine(" {0}  {1}", LW * xi, fxW[i]);
//                }

//                //for (uint i = 0; i < NB; i++)
//                //{
//                //    double a = FSin(1.0 / NB);
//                //    fxB[i] = XC + RC * Math.Cos(a);
//                //    fyB[i] = YC + RC * Math.Sin(a);
//                //}
//                double   dx = 1.0 / NNb;
//                for (uint i = 0; i < NB; i++)
//                {
//                    double x = dx * (i + 1);
//                    fxB[i] = XC + RC * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
//                    fyB[i] = YC + RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
//                }
//            }
//            Count = NNw + NNb;
//            initW(fxW, fyW);
//            initB(fxB, fyB);
//            // h - период дна, при проходе по контуру координата по х вдоль дна вырастет на это значение
//            h = fxW.Max() - fxW.Min();
//            lambda = 2.0 * Math.PI / h;
//            lambda2 = lambda / 2.0;
//            lambda4L2 = 4.0 / (lambda * lambda);
//            N = (uint)(Count + 2);
//            if (algebra == null)
//                algebra = new AlgebraGauss(N);

//            Matrix = new double[N][];
//            for (int i = 0; i < N; i++)
//                Matrix[i] = new double[N];
//            R = new double[N];
//            FF = new double[N];
//            knots = new uint[N];
//            for (uint j = 0; j < N; j++)
//                knots[j] = j;
//        }
//        public double FS(double a)
//        {
//            return a + AmpBottom/(2 * Math.PI) * Math.Sin(2 * Math.PI * (a - a0));
//            //return a + AmpBottom / (2 * Math.PI) * Math.Cos(Math.PI * (a - a0));
//        }
//        //public double FSin(double a)
//        //{
//        //    return - 2 * Math.PI * ( a + AmpCircle * Math.Sin(2 * Math.PI * a) ) - Math.PI / 2;
//        //}
//        /// <summary>
//        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
//        /// </summary>
//        /// <param name="sp">контейнер данных</param>
//        public void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            sp.AddCurve("Касательная скорость потока дне", fxW, SV);
//            sp.AddCurve("Цилиндр", fxB, fyB);
//            sp.AddCurve("Дно", fxW, fyW);
//        }
//        /// <summary>
//        ///  Сетка для расчета донных деформаций
//        /// </summary>
//        public IMesh BedMesh() => new TwoMesh(fxW, fyW);
//        public IMesh Mesh => new TwoMesh(fxW, fyW);
//        /// <summary>
//        /// Установка адаптеров для КЭ сетки и алгебры
//        /// </summary>
//        /// <param name="_mesh"></param>
//        /// <param name="algebra"></param>
//        public void Set(IMesh mesh, IAlgebra algebra = null)
//        {
//            if (algebra != null)
//                this.algebra = algebra;
//            TwoMesh m = null;
//            if (mesh != null)
//                m = mesh as TwoMesh;
//            if (m != null)
//            {
//                if (m != null)
//                {
//                    fxW = m.GetCoordsX();
//                    fyW = m.GetCoordsY();
//                    NW = fxW.Length;
//                }
//            }
//        }
//        /// <summary>
//        /// Установка новых отметок дна
//        /// </summary>
//        /// <param name="zeta"></param>
//        public void SetZeta(double[] zeta, bool bedErosion)
//        {
//            Erosion = bedErosion;
//            fyW = zeta;
//        }
//        /// <summary>
//        /// Получить отметки дна
//        /// </summary>
//        /// <param name="zeta"></param>
//        public void GetZeta(ref double[] zeta)
//        {
//            zeta = fyW;
//        }
//        /// <summary>
//        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
//        /// усредненных на конечных элементах
//        /// </summary>
//        /// <param name="tauX">придонное касательное напряжение по х</param>
//        /// <param name="tauY">придонное касательное напряжение по у</param>
//        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
//        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, StressesFlag sf = StressesFlag.Nod)
//        {
//            tauX = this.tauX;
//            tauY = null;
//            P = null;
//        }
//        /// <summary>
//        /// Установка геометрии дна
//        /// </summary>
//        /// <param name="x"></param>
//        /// <param name="y"></param>
//        public void Set(double[] x, double[] y)
//        {
//            initW(x, y);
//        }
//        /// <summary>
//        /// Расчет полей глубины и скоростей на текущем шаге по времени
//        /// </summary>
//        public void SolverStep()
//        {
//            try
//            {
//                bool flagGamma = true;
//                if (flagGamma == true)
//                {
//                    int NDU = 10;
//                    double GammaMin = -5;
//                    double GammaMax = 1;
//                    double dGamma = Math.Abs(GammaMax - GammaMin)/(NDU-1);
//                    double[] dG = new double[NDU];
//                    double[] dU = new double[NDU];
//                    Gamma = GammaMin;
//                    for (int ig = 0; ig < NDU; ig++)
//                    {
//                        SolverStepForGamma(Gamma);
//                        // скорости на цилиндре
//                        for (int j = 0; j < NNb; j++)
//                            VCircle[j] = FF[NNw + j];
//                         // VCircle[j] = U_inf * dXdZetaB[j] / detJB[j] - FF[NNw + j];
//                        dG[ig] = Gamma;
//                        double VCircleMax = DMath.Max(VCircle, 2 * Math.PI).yMax;
//                        double VCircleMin = DMath.Min(VCircle, 2 * Math.PI).yMin;
//                        dU[ig] = VCircleMax + VCircleMin;
//                        //Console.WriteLine("Gamma = {2}  Ua = {0}, Ub = {1}", VCircle[NNb / 2], VCircle[NNb -1], Gamma);
//                        //dU[ig] = Math.Abs(FF[NNw + NNb / 2]) + Math.Abs(FF[NNw + NNb]);
//                        //Console.WriteLine("Ua = {0}, Ub = {1}", FF[NNw + NNb / 2], FF[NNw + NNb]);
//                        Gamma += dGamma;
//                    }
//                    // найденная циркуляция
//                    Gamma =  DMath.RootFun(dG, dU).xRoot;
//                    Console.WriteLine("Gamma = {2}  Ua = {0}, Ub = {1}", VCircle[NNb / 2], VCircle[NNb - 1], Gamma);
//                }
//                else
//                {
//                    Gamma = -0.5;
//                }
//                // решение задачи для заданной циркуляции
//                SolverStepForGamma(Gamma);
//                for (int j = 0; j < NNb; j++)
//                    VCircle[j] = FF[NNw + j];
//                //  скорости на дне
//                for (int j = 0; j < NNw; j++)
//                    VC[j] = U_inf * dXdZetaW[j] / detJW[j] - FF[j];

//                // скорости на цилиндре
//                for (int j = 0; j < NNb; j++)
//                    VC[NNw + j] = U_inf * dXdZetaB[j] / detJB[j] - FF[NNw + j];

//                for (int j = 0; j < NNb; j++)
//                    VCircle[j] = U_inf * dXdZetaB[j] / detJB[j] - FF[NNw + j];

//                // Фильтрация скоростей
//                alglib.spline1dinterpolant c = new alglib.spline1dinterpolant();
//                alglib.spline1dfitreport rep = new alglib.spline1dfitreport();
//                double[] arg = fxW;
//                MEM.AllocClear(NW, ref SV);
//                MEM.AllocClear(NW, ref VCC);
//                for (int j = 0; j < NW; j++)
//                    VCC[j] = VC[j];
//                int j0 = NW / 10 + 1;
//                for (int j = 0; j < j0; j++)
//                    VCC[j] = VC[j0];
//                j0 = NW - j0;
//                for (int j = j0; j < NW; j++)
//                    VCC[j] = VC[j0];

//                int info = 0;
//                //аппроксимируем V и сглаживаем по кубическому сплайну
//                alglib.spline1dfitpenalized(arg, VCC, arg.Length, Flexibility, Hardness, out info, out c, out rep);
//                for (int i = 0; i < NW; i++)
//                    SV[i] = (float)alglib.spline1dcalc(c, arg[i]);

//                for (int j = 0; j < NW; j++)
//                    tauX[j] = Rho * Lambda * SV[j] * SV[j] / 2.0;
//            }
//            catch(Exception ex)
//            {
//                Logger.Instance.Exception(ex);
//            }
//        }

//        /// <summary>
//        /// Расчет полей глубины и скоростей на текущем шаге по времени
//        /// </summary>
//        public void SolverStepForGamma(double Gamma)
//        {
//            MEM.Alloc<double>(Count, ref VC);
//            MEM.Alloc<double>(Count, ref SV);
//            MEM.Alloc<double>(Count, ref tauX);

//            // вычисление матрицы системы
//            Aww = CalkMatrixWW();
//            Awb = CalkMatrixWB();
//            Abw = CalkMatrixBW();
//            Abb = CalkMatrixBB();

//            Bww = CalkMatrixF_WW();
//            Bwb = CalkMatrixF_WB();
//            Bbw = CalkMatrixF_BW();
//            Bbb = CalkMatrixF_BB();

//            // Сборка Matrix и R
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNw; j++)
//                    Matrix[i][j] = Aww[i, j];
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNb; j++)
//                    Matrix[i][j + NNw] = Awb[i, j];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNw; j++)
//                    Matrix[i + NNw][j] = Abw[i, j];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNb; j++)
//                    Matrix[i + NNw][j + NNw] = Abb[i, j];
//            for (int i = 0; i < NNw; i++)
//            {
//                double sum = 0;
//                for (int j = 0; j < NNb; j++)
//                    sum += Bwb[i, j];
//                Matrix[i][NNw + NNb] = -Math.PI - sum;
//                Matrix[i][NNw + NNb + 1] = sum;
//            }
//            for (int i = 0; i < NNw; i++)
//            {
//                double sumW = 0;
//                for (int j = 0; j < NNw; j++)
//                    sumW += Bww[i, j] * fyW[j];
//                double sumB = 0;
//                for (int j = 0; j < NNb; j++)
//                    sumB += Bwb[i, j] * fyB[j];
//                R[i] = U_inf * (-Math.PI * fyW[i] + sumW + sumB);
//            }
//            // Уравнения : точка на теле
//            for (int i = 0; i < NNb; i++)
//            {
//                double sum = 0;
//                for (int j = 0; j < NNw; j++)
//                    sum += Bbw[i, j];
//                Matrix[NNw + i][NNw + NNb] = sum;
//                Matrix[NNw + i][NNw + NNb + 1] = -Math.PI - sum;
//            }
//            for (int i = 0; i < NNb; i++)
//            {
//                double sumW = 0;
//                for (int j = 0; j < NNw; j++)
//                    sumW += Bbw[i, j] * fyW[j];
//                double sumB = 0;
//                for (int j = 0; j < NNb; j++)
//                    sumB += Bbb[i, j] * fyB[j];
//                R[NNw + i] = U_inf * (-Math.PI * fyB[i] + sumW + sumB);
//            }
//            // Предпоследнее уравнение : интеграл d Psi /d n по дну должен дать 0
//            for (int j = 0; j < NNw; j++)
//                Matrix[NNw + NNb][j] = detJW[j] / NNw;

//            for (int j = 0; j < NNb; j++)
//                Matrix[NNw + NNb + 1][NNw + j] = detJB[j] / NNb;

//            R[NNw + NNb] = - Gamma;
//            R[NNw + NNb + 1] =   Gamma;

//            if (algebra == null)
//                algebra = new AlgebraGauss(N);
//            else
//                algebra.Clear();
//            algebra.AddToMatrix(Matrix, knots);
//            algebra.AddToRight(R, knots);
//            algebra.Solve(ref FF);
//        }

//        #region реализации метода граничных элементов высокого порядка
//        protected void initW(double[] fxW, double[] fyW)
//        {
//            this.fxW = fxW;
//            this.fyW = fyW;
//            // Формирование правых частей
//            GeometriaXW = new PSpline(fxW);
//            GeometriaYW = new PSpline(fyW);
//            dXdZetaW = GeometriaXW.DiffSplineFunction();
//            dYdZetaW = GeometriaYW.DiffSplineFunction();
//            // приращение вдоль кривой
//            for (int i = 0; i < NNw; i++)
//                detJW[i] = Math.Sqrt(dXdZetaW[i] * dXdZetaW[i] + dYdZetaW[i] * dYdZetaW[i]);
//            BettaW = CalkBetta(NNw);
//        }
//        protected void initB(double[] fxB, double[] fyB)
//        {
//            this.fxB = fxB;
//            this.fyB = fyB;
//            // Формирование правых частей
//            GeometriaXB = new PSpline(fxB);
//            GeometriaYB = new PSpline(fyB);
//            dXdZetaB = GeometriaXB.DiffSplineFunction();
//            dYdZetaB = GeometriaYB.DiffSplineFunction();
//            // приращение вдоль кривой
//            for (int i = 0; i < NNb; i++)
//                detJB[i] = Math.Sqrt(dXdZetaB[i] * dXdZetaB[i] + dYdZetaB[i] * dYdZetaB[i]);
//            // Вычисление бета вектора
//            BettaB = CalkBetta(NNb);
//        }
//        protected double[,] CalkMatrixWW()
//        {
//            double[,] A = new double[NNw, NNw];
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNw; j++)
//                {
//                    int idx = Math.Abs(i - j);
//                    A[i, j] = -detJW[j] / NNw * (BettaW[idx] + GreenWW(i, j));
//                }
//            return A;
//        }
//        /// <summary>
//        /// Расчет значения функции Грина от источника с узлом i в узле j
//        /// </summary>
//        protected double GreenWW(int i, int j)
//        {
//            if (i == j)
//            {
//                return Math.Log(detJW[i] / Math.PI);
//            }
//            else
//            {
//                // Вычисление проекций по Х
//                double dx = fxW[j] - fxW[i];
//                // Вычисление проекций по Y
//                double dy = fyW[j] - fyW[i];
//                double a = Math.Sinh(lambda2 * dy);
//                double b = Math.Sin(lambda2 * dx);
//                double Gij = 0.5 * Math.Log(lambda4L2 * (a * a + b * b));
//                return Gij;
//            }
//        }
//        protected double[,] CalkMatrixWB()
//        {
//            double[,] A = new double[NNw, NNb];
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNb; j++)
//                {
//                    A[i, j] = - detJB[j] / NNb * GreenWB(i, j);
//                }
//            return A;
//        }
//        /// <summary>
//        /// Расчет значения функции Грина от источника с узлом i в узле j
//        /// </summary>
//        protected double GreenWB(int i, int j)
//        {
//            // Вычисление проекций по Х
//            double dx = fxB[j] - fxW[i];
//            // Вычисление проекций по Y
//            double dy = fyB[j] - fyW[i];
//            double a = Math.Sinh(lambda2 * dy);
//            double b = Math.Sin(lambda2 * dx);
//            double Gij = 0.5 * Math.Log(lambda4L2 * (a * a + b * b));
//            return Gij;
//        }
//        protected double[,] CalkMatrixBW()
//        {
//            double[,] A = new double[NNb, NNw];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNw; j++)
//                {
//                    A[i, j] = - detJW[j] / NNw * GreenBW(i, j);
//                }
//            return A;
//        }
//        /// <summary>
//        /// Расчет значения функции Грина от источника с узлом i в узле j
//        /// </summary>
//        protected double GreenBW(int i, int j)
//        {
//            // Вычисление проекций по Х
//            double dx = fxW[j] - fxB[i];
//            // Вычисление проекций по Y
//            double dy = fyW[j] - fyB[i];
//            double a = Math.Sinh(lambda2 * dy);
//            double b = Math.Sin(lambda2 * dx);
//            double Gij = 0.5 * Math.Log(lambda4L2 * (a * a + b * b));
//            return Gij;
//        }
//        protected double[,] CalkMatrixBB()
//        {
//            double[,] A = new double[NNb, NNb];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNb; j++)
//                {
//                    int idx = Math.Abs(i - j);
//                    A[i, j] = -detJB[j] / NNb * (BettaB[idx] + GreenBB(i, j));
//                }
//            return A;
//        }
//        /// <summary>
//        /// Расчет значения функции Грина от источника с узлом i в узле j
//        /// </summary>
//        protected double GreenBB(int i, int j)
//        {
//            double Gij;
//            if (i == j)
//            {
//                Gij = Math.Log(detJB[i] / Math.PI);
//            }
//            else
//            {
//                // Вычисление проекций по Х
//                double dx = fxB[j] - fxB[i];
//                // Вычисление проекций по Y
//                double dy = fyB[j] - fyB[i];
//                double a = Math.Sinh(lambda2 * dy);
//                double b = Math.Sin(lambda2 * dx);
//                Gij = 0.5 * Math.Log(lambda4L2 * (a * a + b * b));

//            }
//            return Gij;
//        }
//        /// ======================================
//        protected double[,] CalkMatrixF_WW()
//        {
//            double[,] B = new double[NNw, NNw];
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNw; j++)
//                {
//                    if (i == j)
//                    {
//                        double sum1 = 0;
//                        for (int k = 0; k < NNw; k++)
//                            sum1 += GreenFWW(i, k);
//                        double sum2 = 0;
//                        for (int k = 0; k < NNb; k++)
//                            sum2 += GreenFWB(i, k);
//                        B[i, j] = (sum1 / NNw - sum2);
//                    }
//                    else
//                        B[i, j] = GreenFWW(i, j) / NNw;
//                }
//            return B;
//        }
//        protected double[,] CalkMatrixF_WB()
//        {
//            double[,] B = new double[NNw, NNb];
//            for (int i = 0; i < NNw; i++)
//                for (int j = 0; j < NNb; j++)
//                    B[i, j] = GreenFWB(i, j);
//            return B;
//        }
//        protected double[,] CalkMatrixF_BW()
//        {
//            double[,] B = new double[NNb, NNw];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNw; j++)
//                    B[i, j] = GreenFBW(i, j);
//            return B;
//        }
//        protected double[,] CalkMatrixF_BB()
//        {
//            double[,] B = new double[NNb, NNb];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNb; j++)
//                {
//                    if (i == j)
//                    {
//                        double sum1 = 0;
//                        for (int k = 0; k < NNb; k++)
//                            sum1 += GreenFBB(i, k);
//                        sum1 /= NNb;
//                        double sum2 = 0;
//                        for (int k = 0; k < NNw; k++)
//                            sum2 += GreenFBW(i, k);

//                        B[i, j] = (-sum1 - sum2);
//                    }
//                    else
//                        B[i, j] = GreenFBB(i, j) / NNb;
//                }
//            return B;
//        }
//        protected double GreenFWW(int i, int j)
//        {
//            if (i == j)
//            {
//                return 0;
//            }
//            else
//            {
//                // Вычисление проекций по Х
//                double dx = fxW[j] - fxW[i];
//                // Вычисление проекций по Y
//                double dy = fyW[j] - fyW[i];

//                double az = Math.Sinh(lambda2 * dy);
//                double bz = Math.Sin(lambda2 * dx);
//                double zn = 4.0 * (az * az + bz * bz);

//                double a = -dXdZetaW[j] * Math.Sinh(lambda * dy);
//                double b = dYdZetaW[j] * Math.Sin(lambda * dx);

//                double Gij = lambda * (b + a) / zn;
//                return Gij;
//            }
//        }
//        protected double GreenFWB(int i, int j)
//        {
//            // Вычисление проекций по Х
//            double dx = fxB[j] - fxW[i];
//            // Вычисление проекций по Y
//            double dy = fyB[j] - fyW[i];

//            double az = Math.Sinh(lambda2 * dy);
//            double bz = Math.Sin(lambda2 * dx);
//            double zn = 4.0 * (az * az + bz * bz);

//            double a = -dXdZetaB[j] * Math.Sinh(lambda * dy);
//            double b = dYdZetaB[j] * Math.Sin(lambda * dx);

//            double Gij = lambda * (b + a) / (NNb * zn);
//            return Gij;
//        }
//        protected double GreenFBW(int i, int j)
//        {
//            // Вычисление проекций по Х
//            double dx = fxW[j] - fxB[i];
//            // Вычисление проекций по Y
//            double dy = fyW[j] - fyB[i];

//            double az = Math.Sinh(lambda2 * dy);
//            double bz = Math.Sin(lambda2 * dx);
//            double zn = 4.0 * (az * az + bz * bz);

//            double a = -dXdZetaW[j] * Math.Sinh(lambda * dy);
//            double b = dYdZetaW[j] * Math.Sin(lambda * dx);

//            double Gij = lambda * (b + a) / (NNw * zn);
//            return Gij;
//        }
//        protected double GreenFBB(int i, int j)
//        {
//            if (i == j) return 0;
//            // Вычисление проекций по Х
//            double dx = fxB[j] - fxB[i];
//            // Вычисление проекций по Y
//            double dy = fyB[j] - fyB[i];

//            double az = Math.Sinh(lambda2 * dy);
//            double bz = Math.Sin(lambda2 * dx);
//            double zn = 4.0 * (az * az + bz * bz);

//            double a = -dXdZetaB[j] * Math.Sinh(lambda * dy);
//            double b = dYdZetaB[j] * Math.Sin(lambda * dx);

//            double Gij = lambda * (b + a) / zn;
//            return Gij;
//        }
//        /// <summary>
//        /// Вычисление коэффициентов квадратурных формул
//        /// </summary>
//        /// <param name="N">Количество узлов</param>
//        /// <returns>коэффициентов квадратурных формул</returns>
//        protected double[] CalkBetta(int N)
//        {
//            int M = (int)(N / 2) - 1;
//            double[] Betta = new double[N];
//            double Alpha = 0;
//            double log2 = Math.Log(2.0);
//            double ztak = -1.0;
//            double Sum;

//            for (int k = 0; k < N; k++)
//            {
//                ztak = -1 * ztak;
//                Sum = 0;
//                if (k == 0)
//                {
//                    for (int m = 1; m <= M; m++)
//                        Sum += 1.0 / m;
//                    Betta[0] += -(log2 + ztak / N + Sum);
//                }
//                else
//                {
//                    for (int m = 1; m <= M; m++)
//                        Sum += Math.Cos(2.0 * Math.PI * k * m / N) / m;
//                    Alpha = -(log2 + ztak / N + Sum);
//                    double sn = Math.Sin(k * Math.PI / N);
//                    double ln = Math.Log(Math.Abs(sn));
//                    Betta[k] += -ln + Alpha;
//                }
//            }
//            return Betta;
//        }
//        #endregion

//        public virtual void LoadData(string fileName)
//        {
//        }

//        /// <summary>
//        /// Создает экземпляр класса конвертера
//        /// </summary>
//        /// <returns></returns>
//        public IOFormater<IRiver> GetFormater()
//        {
//            return new ProxyTaskFormat<IRiver>();
//        }

//    }
//}
