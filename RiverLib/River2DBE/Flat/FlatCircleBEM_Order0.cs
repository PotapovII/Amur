////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////            перенесено с правкой : 26.04.2021 Потапов И.И. 
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using AlgebraLib;
//    using FEMTasksLib;
//    using System;
//    using System.Linq;
//    [Serializable]
//    public class FlatCircleBEM_Order0 : RiverCircleParams, IFlatCircle
//    {
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
//        /// волновое число для дна
//        /// </summary>
//        protected double lambda;
//        protected double lambda2;
//        protected double lambda4L2;
//        /// <summary>
//        /// Конструктор
//        /// </summary>
//        public FlatCircleBEM_Order0(RiverCircleParams p) : base(p)
//        {
//            NNw = NW; // meshW.Count;
//            NNb = NB; // meshB.Count;
//            // Начальная информация о форме дна
//            fxW = new double[NNw];
//            fyW = new double[NNw];
//            fxB = new double[NNb];
//            fyB = new double[NNb];
//            dXdZetaW = new double[NNw];
//            detJW = new double[NNw];
//            detJB = new double[NNb];
//            double dx = LW * 1.0 / NNw;
//            for (uint i = 0; i < NW; i++)
//            {
//                double x = dx * (i + 1);
//                fxW[i] = x;
//                fyW[i] = 0;
//            }
//            dx = 1.0 / NNb;
//            for (uint i = 0; i < NB; i++)
//            {
//                double x = dx * (i + 1);
//                fxB[i] = XC + RC * Math.Cos(-2 * Math.PI * x - Math.PI / 2);
//                fyB[i] = YC + RC * Math.Sin(-2 * Math.PI * x - Math.PI / 2);
//            }
//            Count = NNw + NNb;
//            initW(fxW, fyW);
//            initB(fxB, fyB);
//#if DEBUG
//            //LOG.Print("fxW", fxW, 4);
//            //LOG.Print("fyW", fyW, 4);
//            //LOG.Print("detJW", detJW, 4);
//            //LOG.Print("dXdZetaW", dXdZetaW, 4);
//            //LOG.Print("dYdZetaW", dYdZetaW, 4);
//            //LOG.Print("BettaW", BettaW, 4);

//            //LOG.Print("fxB", fxB, 4);
//            //LOG.Print("fyB", fyB, 4);
//            //LOG.Print("dXdZetaB", dXdZetaB, 4);
//            //LOG.Print("dYdZetaB", dYdZetaB, 4);
//            //LOG.Print("detJB", detJB, 4);
//            //LOG.Print("BettaB", BettaB, 4);
//#endif
//            // приращение вдоль кривой
//            h = fxW.Max() - fxW.Min();
//            lambda = 2.0 * Math.PI / h;
//            lambda2 = lambda / 2.0;
//            lambda4L2 = 4.0 / (lambda * lambda);
//            N = (uint)(Count + 2);
//            Matrix = new double[N][];
//            for (int i = 0; i < N; i++)
//                Matrix[i] = new double[N];
//            R = new double[N];
//            FF = new double[N];
//            knots = new uint[N];
//            for (uint j = 0; j < N; j++)
//                knots[j] = j;
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
//        /// вычисление скоростей на контуре для безвихривого потока
//        /// </summary>
//        /// <param name="VC">касательная скорость на дне</param>
//        /// <param name="tauX">касательные напряжения на дне</param>
//        public void SolverTask(ref double[] VC, ref double[] SV, ref double[] tauX)
//        {
//            if (VC == null)
//                VC = new double[Count];
//            if (VC.Length != Count)
//                VC = new double[Count];
//            if (tauX == null)
//                tauX = new double[Count];
//            if (tauX.Length != Count)
//                tauX = new double[Count];
//            // вычисление матрицы системы
//            Aww = CalkMatrixWW();
//            Awb = CalkMatrixWB();
//            Abw = CalkMatrixBW();
//            Abb = CalkMatrixBB();

//            //LOG.Print("Aww", Aww, 4);
//            //LOG.Print("Awb", Awb, 4);
//            //LOG.Print("Abw", Abw, 4);
//            //LOG.Print("Abb", Abb, 4);

//            Bww = CalkMatrixF_WW();
//            Bwb = CalkMatrixF_WB();
//            Bbw = CalkMatrixF_BW();
//            Bbb = CalkMatrixF_BB();

//            //LOG.Print("Bww", Bww, 4);
//            //LOG.Print("Bwb", Bwb, 4);
//            //LOG.Print("Bbw", Bbw, 4);
//            //LOG.Print("Bbb", Bbb, 4);

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
//            for (int j = 0; j < NNw; j++)
//                Matrix[NNw + NNb][j] = detJW[j] / NNw;

//            for (int j = 0; j < NNb; j++)
//                Matrix[NNw + NNb + 1][NNw + j] = detJB[j] / NNb;

//            //LOG.Print("Matrix", Matrix, 4);
//            //LOG.Print("R", R, 4);


//            IAlgebra algebra = new AlgebraGauss(N);
//            algebra.AddToMatrix(Matrix, knots);
//            algebra.AddToRight(R, knots);
//            algebra.Solve(ref FF);

//            for (int j = 0; j < NNw; j++)
//                VC[j] = U_inf * dXdZetaW[j] / detJW[j] - FF[j];
//            for (int j = 0; j < NNb; j++)
//                VC[NNw + j] = U_inf * dXdZetaB[j] / detJB[j] - FF[j];

//            for (int j = 0; j < Count; j++)
//                tauX[j] = Rho * Lambda * VC[j] * VC[j] / 2.0;
//        }

//        #region реализации метода граничных элементов высокого порядка
//        protected void initW(double[] fxW, double[] fyW)
//        {
//            // Формирование правых частей
//            //GeometriaXW = new PSpline(fxW);
//            for (uint i = 0; i < NW; i++)
//                dXdZetaW[i] = LW;
//            GeometriaYW = new PSpline(fyW);
//            // LOG.Print("SfxW", GeometriaXW.SplineFunction(), 4);
//            //LOG.Print("SfyW", GeometriaYW.SplineFunction(), 4);
//            //dXdZetaW = GeometriaXW.DiffSplineFunction();
//            dYdZetaW = GeometriaYW.DiffSplineFunction();
//            //LOG.Print("dXdZetaW", dXdZetaW, 4);
//            //LOG.Print("dYdZetaW", dYdZetaW, 4);
//            //dXdZetaW = GeometriaXW.DiffSplineFunctionWhith();
//            //dYdZetaW = GeometriaYW.DiffSplineFunctionWhith();
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
//                    A[i, j] = -0.5 * detJB[j] / NNb * GreenWB(i, j);
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
//            double Gij = Math.Log(lambda4L2 * (a * a + b * b));
//            return Gij;
//        }
//        protected double[,] CalkMatrixBW()
//        {
//            double[,] A = new double[NNb, NNw];
//            for (int i = 0; i < NNb; i++)
//                for (int j = 0; j < NNw; j++)
//                {
//                    A[i, j] = -0.5 * detJW[j] / NNw * GreenBW(i, j);
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
//            double Gij = Math.Log(lambda4L2 * (a * a + b * b));
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
//    }
//}
