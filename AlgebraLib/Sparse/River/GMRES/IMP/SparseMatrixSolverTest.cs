//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                    разработка: Потапов И.И.
//                      03.06.2025
//---------------------------------------------------------------------------
namespace AlgebraLib.Sparse.River
{
    using System;
    using CommonLib;
    using MemLogLib;
    public class SparseMatrixSolverTest : ASparseMatrixSolver, IAlgebra
    {
        /// <summary>
        /// Основная матрица предобуславливателя
        /// </summary>
        protected SparseMatrix ILUMatrix = null;
        /// <summary>
        /// Флаг работы с матрицами предобуславливания
        /// </summary>
        protected int isPrecond = 1;
        /// <summary>
        /// Прстранство Крылова
        /// </summary>
        protected int M = 20;
        /// <summary>
        /// Максимальное количество итераций
        /// </summary>
        protected int MaxIters = 1000;
        
        public SparseMatrixSolverTest(uint numRows, int rowSize, int isPrecond, 
            int M, int MaxIters, double Error = MEM.Error9) : base(numRows, rowSize)
        {
            this.isPrecond = isPrecond;
            Set(M, MaxIters, Error);
            name = "GMRES (CSR matrix)";
            result.Name = name;
        }
        // Copy constructor
        public SparseMatrixSolverTest(SparseMatrixSolverTest algebra) : base(algebra) 
        { 
            isPrecond= algebra.isPrecond;
            MaxIters = algebra.MaxIters;
            M = algebra.M;
            name = "GMRES (CSR matrix)";
            result.Name = name;
            EPS = MEM.Error9;
        }
        public SparseMatrixSolverTest() : base(1,20) 
        { 
            name = "GMRES (CSR matrix)";
            EPS = MEM.Error9;
        }
        /// <summary>
        /// Установка параметров метода
        /// </summary>
        /// <param name="M"></param>
        /// <param name="MaxIters"></param>
        /// <param name="Error"></param>
        public void Set(int M, int MaxIters, double Error = MEM.Error9)
        {
            this.M = M;
            this.MaxIters = MaxIters;
            EPS = MEM.Error9;
        }
        #region IAlgebra
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        protected override void solve(ref double[] X)
        {
            if (isPrecond == 0)
                PCILU_0_GMRES(ref X);
            else
                PCILU_1_GMRES(ref X);
        }
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            ILUMatrix?.Clear();
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <param name="N">порядок системы</param>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new SparseMatrixSolverTest(this);
        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        {
            SMatrix.Print(flag);
            LOG.Print("Right", Right);
        }
        #endregion
        #region Старая реализация
        public void preconditionedCGs(double[] X, double[] Right)
        {

            double[] R, RN, BetaE1, Min, S;
            int i, imax, j;
            double deltaNew = 0, deltaOld = 0, delta0, err, err2delta0, alpha, beta, dTq;

            R = RN = BetaE1 = S = Min = null;

            R = new double[FN];
            RN = new double[FN];
            BetaE1 = new double[FN];
            Min = new double[FN];
            S = new double[FN];

            //initializing variables for the iterative solution

            i = 0;
            imax = 2 * (int)FN;   //theortically this method should converge in n iterations or less 
            err = 0.000000001;
            SMatrix.MatrixCRS_Mult(ref R, X);                                        // {R} = [A]{X}
            for (j = 0; j < FN; j++) Min[j] = 1.0 / SMatrix.Value(j + 1, j + 1); // Preconditioner M[i] = K[i,i]
            for (j = 0; j < FN; j++)
            {
                BetaE1[j] = Right[j] - R[j];
                RN[j] = Min[j] * BetaE1[j];
                deltaNew += BetaE1[j] * RN[j];
            }
            delta0 = deltaNew;
            //	err2delta0 = err*err*delta0;
            err2delta0 = 0.0000001;

            //the iterative solution

            while ((i < imax) && (deltaNew > err2delta0))
            {
                SMatrix.MatrixCRS_Mult(ref R, RN);                                    // {R} = [A]{RN}
                dTq = 0;                                            // sum of {RN[i]}*{R[i]}		
                for (j = 0; j < FN; j++) dTq = dTq + RN[j] * R[j];
                alpha = deltaNew / dTq;
                for (j = 0; j < FN; j++) X[j] += alpha * RN[j];           // {X} = {X} + alpha*{RN}
                if ((i % 50) == 0)                                  // if divisable by 50
                {
                    SMatrix.MatrixCRS_Mult(ref R, X);                                // {R} = [A]{X}
                    for (j = 0; j < FN; j++) BetaE1[j] = Right[j] - R[j];     // {BetaE1} = {Right} - {R}
                }
                else
                    for (j = 0; j < FN; j++) BetaE1[j] -= alpha * R[j];       // {BetaE1} = {BetaE1} - alpha*{R}
                for (j = 0; j < FN; j++) S[j] = Min[j] * BetaE1[j];           // {S} = [Min]{BetaE1}
                deltaOld = deltaNew;
                deltaNew = 0;
                for (j = 0; j < FN; j++) deltaNew += BetaE1[j] * S[j];
                beta = deltaNew / deltaOld;
                for (j = 0; j < FN; j++) RN[j] = S[j] + beta * RN[j];   // {RN} = {S} + beta*{RN}
                i++;
            }
        }
        public void conjugateGradients(double[] X, double[] Right)
        {
            // the conjugate gradients solution code
            double[] R, RN, BetaE1;
            int i, imax, j;
            double deltaNew = 0, deltaOld = 0, delta0, err, err2delta0, alpha, beta, dTq;


            R = RN = BetaE1 = null;

            R = new double[FN];
            RN = new double[FN];
            BetaE1 = new double[FN];

            //initializing variables for the iterative solution
            i = 0;
            imax = 2 * (int)FN;       //theortically this method should converge in n iterations or less 
            err = 0.0000001;
            SMatrix.MatrixCRS_Mult(ref R, X); // {R} = [A]{X}
            for (j = 0; j < FN; j++)
            {
                BetaE1[j] = Right[j] - R[j];
                RN[j] = BetaE1[j];
                deltaNew += BetaE1[j] * BetaE1[j];
            }
            delta0 = deltaNew;
            //	err2delta0 = err*err*delta0;
            err2delta0 = 0.0000001;

            //the iterative solution

            while ((i < imax) && (deltaNew > err2delta0))
            {
                SMatrix.MatrixCRS_Mult(ref R, RN);                                // {R} = [A]{RN}
                dTq = 0;                                        // sum of {RN[i]}*{R[i]}		
                for (j = 0; j < FN; j++) dTq = dTq + RN[j] * R[j];
                alpha = deltaNew / dTq;
                for (j = 0; j < FN; j++) X[j] += alpha * RN[j];       // {X} = {X} + alpha*{RN}
                if ((i % 50) == 0)                              // if divisable by 50
                {
                    SMatrix.MatrixCRS_Mult(ref R, X);                        // {R} = [A]{X}
                    for (j = 0; j < FN; j++) BetaE1[j] = Right[j] - R[j];  // {BetaE1} = {Right} - {R}
                }
                else
                    for (j = 0; j < FN; j++) BetaE1[j] -= alpha * R[j];  // {BetaE1} = {BetaE1} - alpha*{R}
                deltaOld = deltaNew;
                deltaNew = 0;
                for (j = 0; j < FN; j++) deltaNew += BetaE1[j] * BetaE1[j];
                beta = deltaNew / deltaOld;
                for (j = 0; j < FN; j++) RN[j] = BetaE1[j] + beta * RN[j]; // {RN} = {BetaE1} + beta*{RN}
                i++;
            }
        }
        //public void gmres(double[] X, double[] Right, int M, int MaxIters, double EPS)
        //{
        //    double[] R, RN, BetaE1, V, H;
        //    int i, j, MP, i0, im, iters, ii;
        //    double tem = 1, beta = 0, Cos, Sin, beta0, resCheck;

        //    R = RN = BetaE1 = V = H = null;

        //    R = new double[FN];
        //    RN = new double[FN];
        //    BetaE1 = new double[M + 1];
        //    V = new double[FN * M];
        //    H = new double[M * (M + 1)];

        //    SMatrix.MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}

        //    for (ii = 0; ii < FN; ii++)
        //    {
        //        R[ii] = Right[ii] - R[ii];
        //        beta += R[ii] * R[ii];
        //    }

        //    beta = Math.Sqrt(beta);
        //    beta0 = beta;

        //    //the iterative solution

        //    for (iters = 0; iters < MaxIters; iters++)
        //    {
        //        MP = M;

        //        //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
        //        for (ii = 0; ii < FN; ii++) R[ii] /= beta;
        //        for (ii = 0; ii <= MP; ii++) BetaE1[ii] = 0;
        //        BetaE1[0] = beta;

        //        for (j = 0; j < M; j++)
        //        {
        //            for (ii = 0; ii < FN; ii++) V[ii * M + j] = R[ii];
        //            SMatrix.MatrixCRS_Mult(ref RN, R);
        //            for (ii = 0; ii < FN; ii++) R[ii] = RN[ii];

        //            // Зачем
        //            double sum1 = 0.0;
        //            for (ii = 0; ii < FN; ii++) sum1 += R[ii];

        //            for (i = 0; i <= j; i++)
        //            {
        //                H[i * M + j] = 0;
        //                for (ii = 0; ii < FN; ii++) H[i * M + j] += R[ii] * V[ii * M + i];
        //                for (ii = 0; ii < FN; ii++) R[ii] -= H[i * M + j] * V[ii * M + i];
        //            }
        //            tem = 0;
        //            for (ii = 0; ii < FN; ii++) tem += R[ii] * R[ii];
        //            tem = Math.Sqrt(tem);
        //            H[(j + 1) * M + j] = tem;
        //            if (tem < EPS)
        //            {
        //                MP = j + 1;
        //                goto l5;
        //            }
        //            for (ii = 0; ii < FN; ii++) R[ii] /= tem;
        //        }
        //    //triangularization
        //    l5: for (i = 0; i < MP; i++)
        //        {
        //            im = i + 1;
        //            tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
        //            Cos = H[i * M + i] * tem;
        //            Sin = -H[im * M + i] * tem;
        //            for (j = i; j < MP; j++)
        //            {
        //                tem = H[i * M + j];
        //                H[i * M + j] = Cos * tem - Sin * H[im * M + j];
        //                H[im * M + j] = Sin * tem + Cos * H[im * M + j];
        //            }
        //            BetaE1[im] = Sin * BetaE1[i];
        //            BetaE1[i] *= Cos;
        //        }
        //        double sum = 0.0;
        //        for (ii = 0; ii < (M * (M + 1)); ii++) sum += H[ii];
        //        sum = 0.0;
        //        for (ii = 0; ii < (M + 1); ii++) sum += BetaE1[ii];
        //        //solution of linear system
        //        for (i = (MP - 1); i >= 0; i--)
        //        {
        //            BetaE1[i] /= H[i * M + i];
        //            for (i0 = (i - 1); i0 >= 0; i0--) BetaE1[i0] -= H[i0 * M + i] * BetaE1[i];
        //        }
        //        sum = 0.0;
        //        for (ii = 0; ii < (M + 1); ii++) sum += BetaE1[ii];
        //        for (i = 0; i < MP; i++)
        //        {
        //            for (ii = 0; ii < FN; ii++) X[ii] += BetaE1[i] * V[ii * M + i];
        //        }

        //        //new residual and stopping tests
        //        SMatrix.MatrixCRS_Mult(ref R, X);
        //        beta = 0;
        //        for (ii = 0; ii < FN; ii++)
        //        {
        //            R[ii] = Right[ii] - R[ii];
        //            beta += R[ii] * R[ii];
        //        }
        //        beta = Math.Sqrt(beta);
        //        resCheck = beta / beta0;
        //        //	if (beta < EPS) break;
        //        if (resCheck < EPS) break;

        //    }
        //}
        //public int PCILU_gmres(ref double[] X, int itnum, int varnum, int elcode, int M, int MaxIters, double EPS/*, FILE* fp*/)
        //{
        //    int i, j, MP, i0, im, iters, ii;
        //    double tem = 1, beta, beta0, resCheck, zero, Cos, Sin, convergence;
        //    double[] Dp = null, Bp = null, BetaE1 = null, Yp = null, Rp = null;
        //    double[,] Hp = null;
        //    double[,] Vp = null;
        //    MEM.Alloc0(N, ref Bp);
        //    MEM.Alloc0(N, ref Dp);
        //    MEM.Alloc0(N, ref BetaE1);
        //    MEM.Alloc0(N, ref Yp);
        //    MEM.Alloc0(N, ref Rp);
        //    MEM.Alloc(M, M, ref Vp);
        //    MEM.Alloc(M, M + 1, ref Hp);




        //    zero = EPS;
        //    // вызывает конструктор копирования
        //    ILUMatrix = new SparseMatrix(SMatrix, (int)FN);
        //    ILUMatrix.LUDecompose();

        //    for (ii = 0; ii < FN; ii++)
        //        Right[ii] = 0.0;
        //    // получаем первый вектор ошибки
        //    // Вычисляем BetaE1 = M^-1 * (B - A*X)
        //    SMatrix.Mult_MatrixCRS(ref BetaE1, Right);
        //    beta = 0;
        //    for (ii = 0; ii < FN; ii++)
        //    {
        //        Dp[ii] = Bp[ii] - BetaE1[ii]; // residual vector
        //        BetaE1[ii] = Dp[ii];
        //        beta += BetaE1[ii] * BetaE1[ii];
        //    }
        //    beta = Math.Sqrt(beta);
        //    beta0 = beta;
        //    //loop that governs number of reinitializations MaxIters
        //    //could eventually be a while statement...
        //    for (iters = 0; iters < MaxIters; iters++)
        //    {
        //        MP = M;
        //        //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
        //        for (ii = 0; ii < FN; ii++) BetaE1[ii] /= beta;
        //        for (ii = 0; ii < FN; ii++) Yp[ii] = 0;

        //        for (ii = 0; ii <= MP; ii++) Rp[ii] = 0;
        //        Rp[0] = beta;

        //        for (j = 0; j < M; j++)
        //        {
        //            for (ii = 0; ii < FN; ii++) Vp[ii, j] = BetaE1[ii];
        //            ILUMatrix.Precondition(BetaE1);
        //            SMatrix.Mult_MatrixCRS(ref Dp, BetaE1);

        //            for (ii = 0; ii < FN; ii++)
        //                BetaE1[ii] = Dp[ii];

        //            for (i = 0; i <= j; i++)
        //            {
        //                Hp[i, j] = 0;
        //                for (ii = 0; ii < FN; ii++)
        //                    Hp[i, j] += BetaE1[ii] * Vp[ii, i];
        //                for (ii = 0; ii < FN; ii++)
        //                    BetaE1[ii] -= Hp[i, j] * Vp[ii, i];
        //            }
        //            tem = 0;
        //            for (ii = 0; ii < FN; ii++)
        //                tem += BetaE1[ii] * BetaE1[ii];

        //            tem = Math.Sqrt(tem);
        //            Hp[j + 1, j] = tem;
        //            if (tem < EPS)
        //            {
        //                MP = j + 1;
        //                goto l5;
        //            }
        //            for (ii = 0; ii < FN; ii++)
        //                BetaE1[ii] /= tem;
        //        }
        //    // приведение H к тругольному виду методом вращений
        //    l5: for (i = 0; i < MP; i++)
        //        {
        //            im = i + 1;
        //            tem = (1.0) / (Math.Sqrt(Hp[i, i] * Hp[i, i] + Hp[im, i] * Hp[im, i]));
        //            Cos = Hp[i, i] * tem;
        //            Sin = -Hp[im, i] * tem;
        //            for (j = i; j < MP; j++)
        //            {
        //                tem = Hp[i, j];
        //                Hp[i, j] = Cos * tem - Sin * Hp[im, j];
        //                Hp[im, j] = Sin * tem + Cos * Hp[im, j];
        //            }
        //            Rp[im] = Sin * Rp[i];
        //            Rp[i] *= Cos;
        //        }
        //        // solution of linear system
        //        for (i = (MP - 1); i >= 0; i--)
        //        {
        //            Rp[i] /= Hp[i, i];
        //            for (i0 = i - 1; i0 >= 0; i0--)
        //                Rp[i0] -= Hp[i0, i] * Rp[i];
        //        }
        //        // for (ii =0; ii < FN; ii++) BetaE1[ii]=0;
        //        for (i = 0; i < MP; i++)
        //        {
        //            for (ii = 0; ii < FN; ii++) Yp[ii] += Rp[i] * Vp[ii, i];
        //        }
        //        ILUMatrix.Precondition(Yp);
        //        for (ii = 0; ii < FN; ii++)
        //            Right[ii] += Yp[ii];

        //        //new residual and stopping tests
        //        SMatrix.Mult_MatrixCRS(ref BetaE1, Right);
        //        //matrixVectorMult(Right, BetaE1, itnum, varnum, -1);
        //        beta = 0;
        //        for (ii = 0; ii < FN; ii++)
        //        {
        //            Dp[ii] = Bp[ii] - BetaE1[ii];
        //            BetaE1[ii] = Dp[ii];
        //            beta += BetaE1[ii] * BetaE1[ii];
        //        }
        //        beta = Math.Sqrt(beta);
        //        resCheck = beta / beta0;
        //        convergence = 1.0 - (beta / beta0);

        //        if (resCheck < EPS)
        //            break;
        //        else // Stopped before convergence 
        //            if (iters == (MaxIters - 1)) ;
        //    }
        //    MEM.Copy(ref X, Right);
        //    ILUMatrix.Clear();
        //    return (iters + 1);
        //}
        #endregion

        #region Реализация
        /// <summary>
        /// Предобуславливание ILU(0)
        /// </summary>
        /// <param name="X">решение</param>
        /// <returns> количество итераций </returns>
        public void PCILU_0_GMRES(ref double[] X)
        {
            double[] R, RN, BetaE1, Min, V, H;
            double tem = 1, beta = 0, beta0, Cos, Sin, resCheck = 0;
            int ii, iters, i, j, MP, i0, im;

            R = RN = BetaE1 = Min = V = H = null;

            R = new double[FN];
            RN = new double[FN];
            BetaE1 = new double[M + 1];
            Min = new double[FN];
            V = new double[FN * M];
            H = new double[M * (M + 1)];

            //initializing variables for the iterative solution

            SMatrix.MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
            for (j = 0; j < FN; j++) 
                Min[j] = 1.0 / SMatrix.Value(j, j); // Preconditioner M[i] = K[i,i]

            for (ii = 0; ii < FN; ii++)
            {
                RN[ii] = Right[ii] - R[ii];
                R[ii] = Min[ii] * RN[ii];
                beta += R[ii] * R[ii];
            }

            beta = Math.Sqrt(beta);
            beta0 = beta;

            //the iterative solution

            for (iters = 0; iters < MaxIters; iters++)
            {
                MP = M;

                //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
                for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / beta;
                for (ii = 0; ii <= MP; ii++) BetaE1[ii] = 0;
                BetaE1[0] = beta;

                for (j = 0; j < M; j++)
                {
                    for (ii = 0; ii < FN; ii++) V[ii * M + j] = R[ii];
                    SMatrix.MatrixCRS_Mult(ref RN, R);
                    for (ii = 0; ii < FN; ii++) R[ii] = Min[ii] * RN[ii];

                    for (i = 0; i <= j; i++)
                    {
                        H[i * M + j] = 0;
                        for (ii = 0; ii < FN; ii++) H[i * M + j] += R[ii] * V[ii * M + i];
                        for (ii = 0; ii < FN; ii++) R[ii] -= H[i * M + j] * V[ii * M + i];
                    }
                    tem = 0;
                    for (ii = 0; ii < FN; ii++) tem += R[ii] * R[ii];
                    tem = Math.Sqrt(tem);
                    H[(j + 1) * M + j] = tem;
                    if (tem < EPS)
                    {
                        MP = j + 1;
                        goto l5;
                    }
                    for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / tem;
                }

            //triangularization
            l5: for (i = 0; i < MP; i++)
                {
                    im = i + 1;
                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
                    Cos = H[i * M + i] * tem;
                    Sin = -H[im * M + i] * tem;
                    for (j = i; j < MP; j++)
                    {
                        tem = H[i * M + j];
                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
                    }
                    BetaE1[im] = Sin * BetaE1[i];
                    BetaE1[i] = BetaE1[i] * Cos;
                }
                //solution of linear system
                for (i = (MP - 1); i >= 0; i--)
                {
                    BetaE1[i] = BetaE1[i] / H[i * M + i];
                    for (i0 = i - 1; i0 >= 0; i0--) BetaE1[i0] = BetaE1[i0] - H[i0 * M + i] * BetaE1[i];
                }
                for (i = 0; i < MP; i++)
                {
                    for (ii = 0; ii < FN; ii++)
                        X[ii] += BetaE1[i] * V[ii * M + i];
                }
                //new residual and stopping tests
                SMatrix.MatrixCRS_Mult(ref R, X);                                // {R} = [A]{X}
                beta = 0;
                for (ii = 0; ii < FN; ii++)
                {
                    RN[ii] = Right[ii] - R[ii];
                    R[ii] = Min[ii] * RN[ii];
                    beta += R[ii] * R[ii];
                }
                beta = Math.Sqrt(beta);
                resCheck = beta / beta0;
                if (resCheck < EPS) break;
                if (double.IsNaN(beta) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, деление на 0";
                    break;
                }
                if (double.IsInfinity(beta) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, переполнение связанное с расходимостью";
                    break;
                }
                if (iters > MaxIters - 1)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.convergenceStagnation;
                    result.Message = " Решение СЛАУ не получено, достигнуто ограничение по итерациям";
                    break;
                }
            }
            ((AlgebraResultIterative)result).Iterations = iters;
            ((AlgebraResultIterative)result).ratioError = resCheck;
            ((AlgebraResultIterative)result).Error_L2 = beta;
            ((AlgebraResultIterative)result).Error0_L2 = beta0;
            result.Message = " M = " + M.ToString();
            LOG.Print("X", X);
        }
        /// <summary>
        /// Предобуславливание ILU(0)
        /// </summary>
        /// <param name="X">решение</param>
        /// <returns> количество итераций </returns>
        public void PCILU_1_GMRES(ref double[] X)
        {
            double tem = 1, beta = 0, beta0, Cos, Sin, resCheck = 0;
            int ii, iters, i, j, MP, i0, im;
            double[] R, RN, BetaE1, Y, V, H;
            R = RN = BetaE1 = Y = V = H = null;
            MEM.Alloc(FN, ref R);
            MEM.Alloc(FN, ref RN);
            MEM.Alloc(FN, ref Y);
            MEM.Alloc((int)(FN * M), ref V);
            MEM.Alloc(M + 1, ref BetaE1);
            MEM.Alloc((M + 1) * M, ref H);
            ILUMatrix = new SparseMatrix(SMatrix, (int)FN);
            //ILUMatrix.Print(0, "SMatrix");
            ILUMatrix.LUDecompose();
            //ILUMatrix.Print(0, "ILUMatrix");
            LOG.Print("R", R);
            SMatrix.MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
            LOG.Print("pR", R);
            for (ii = 0; ii < FN; ii++)
            {
                RN[ii] = Right[ii] - R[ii];
                R[ii] = RN[ii];
                beta += R[ii] * R[ii];
            }
            ILUMatrix.Precondition(R);

            LOG.Print("pR", R);
            beta = Math.Sqrt(beta);
            LOG.Print("Right", Right);
            beta0 = beta;

            // основной цикл
            for (iters = 0; iters < MaxIters; iters++)
            {
                MP = M;
                // Метод Арнольди (который использует метод
                // ортогонализации Грама-Шмидта)
                for (ii = 0; ii < FN; ii++)
                    R[ii] = R[ii] / beta;
                for (ii = 0; ii < FN; ii++)
                    Y[ii] = 0;
                for (ii = 0; ii <= MP; ii++)
                    BetaE1[ii] = 0;
                BetaE1[0] = beta;

                for (j = 0; j < M; j++)
                {

                    for (ii = 0; ii < FN; ii++)
                        V[ii * M + j] = R[ii];
                    ILUMatrix.Precondition(R);
                    SMatrix.MatrixCRS_Mult(ref RN, R);
                    for (ii = 0; ii < FN; ii++)
                        R[ii] = RN[ii];

                    for (i = 0; i <= j; i++)
                    {
                        H[i * M + j] = 0;
                        for (ii = 0; ii < FN; ii++)
                            H[i * M + j] += R[ii] * V[ii * M + i];
                        for (ii = 0; ii < FN; ii++)
                            R[ii] -= H[i * M + j] * V[ii * M + i];
                    }

                    tem = 0;
                    for (ii = 0; ii < FN; ii++)
                        tem += R[ii] * R[ii];
                    tem = Math.Sqrt(tem);
                    H[(j + 1) * M + j] = tem;
                    if (tem < EPS)
                    {
                        MP = j + 1;
                        goto l5;
                    }
                    for (ii = 0; ii < FN; ii++)
                        R[ii] = R[ii] / tem;
                }

            //triangularization
            l5: for (i = 0; i < MP; i++)
                {
                    im = i + 1;
                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] +
                                             H[im * M + i] * H[im * M + i]));
                    Cos = H[i * M + i] * tem;
                    Sin = -H[im * M + i] * tem;
                    for (j = i; j < MP; j++)
                    {
                        tem = H[i * M + j];
                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
                    }
                    BetaE1[im] = Sin * BetaE1[i];
                    BetaE1[i] = BetaE1[i] * Cos;
                }
                // решение линейной системы
                // выполняем обратный ход
                for (i = (MP - 1); i >= 0; i--)
                {
                    BetaE1[i] = BetaE1[i] / H[i * M + i];
                    for (i0 = (i - 1); i0 >= 0; i0--)
                        BetaE1[i0] = BetaE1[i0] - H[i0 * M + i] * BetaE1[i];
                }
                // находим вектор поправок
                for (i = 0; i < MP; i++)
                {
                    for (ii = 0; ii < FN; ii++)
                        Y[ii] += BetaE1[i] * V[ii * M + i];
                }
                // предобуславливаем вектор поправок
                ILUMatrix.Precondition(Y);
                // корректируем решение 
                for (ii = 0; ii < FN; ii++)
                    X[ii] += Y[ii];
                // произведение нового решения на матрицу
                SMatrix.MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
                // определение новой невязки R
                beta = 0;
                for (ii = 0; ii < FN; ii++)
                {
                    RN[ii] = Right[ii] - R[ii];
                    R[ii] = RN[ii];
                    beta += R[ii] * R[ii];
                }
                // нормы невязки
                beta = Math.Sqrt(beta);
                resCheck = beta / beta0;
                // проверка на сходимость и остановку итерационного процесса
                if (resCheck < EPS) break;
                if (double.IsNaN(beta) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, деление на 0";
                    break;
                }
                if (double.IsInfinity(beta) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, переполнение связанное с расходимостью";
                    break;
                }
                if (iters > MaxIters - 1)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.convergenceStagnation;
                    result.Message = " Решение СЛАУ не получено, достигнуто ограничение по итерациям";
                    break;
                }
            }
            ((AlgebraResultIterative)result).Iterations = iters;
            ((AlgebraResultIterative)result).ratioError = resCheck;
            ((AlgebraResultIterative)result).Error_L2 = beta;
            ((AlgebraResultIterative)result).Error0_L2 = beta0;
            result.Message = " M = " + M.ToString();
            LOG.Print("X" , X);
        }
        #endregion

    }
}
