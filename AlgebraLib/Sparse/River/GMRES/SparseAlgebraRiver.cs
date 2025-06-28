//namespace AlgebraLib.Sparse.River
//{
//    using System;
//    using CommonLib;
//    using MemLogLib;
    
//    public class SparseAlgebraRiver : SparseMatrix, IAlgebra
//    {
//        AlgebraResult result = new AlgebraResult();
//        /// <summary>
//        /// Информация о полученном результате
//        /// </summary>
//        public IAlgebraResult Result { get => result; }
//        /// <summary>
//        /// Название метода
//        /// </summary>
//        public string Name { get { return "Sparse Algebra River"; } }
//        /// <summary>
//        /// глобальная правая часть (ГПЧ)
//        /// </summary>
//        public double[] Fp = null;

//        public SparseAlgebraRiver(int numRows, int rowSize):base(numRows, rowSize) 
//        {
//            MEM.Alloc0((uint)FN, ref Fp, "Fp");
//        }
//        // Copy constructor
//        public SparseAlgebraRiver(SparseAlgebraRiver matrix, int numRows):base(matrix, numRows) 
//        {
//            MEM.Copy(ref Fp, matrix.Fp);
//        }
//        public SparseAlgebraRiver(SparseMatrix matrix) :base(matrix)
//        {
//            MEM.Alloc0((uint)FN, ref Fp, "Fp");
//        }
//        public SparseAlgebraRiver(SparseAlgebraRiver algebra) : base(algebra)
//        {
//            MEM.Copy(ref Fp, algebra.Fp);
//        }

//        #region IAlgebra
//        /// <summary>
//        /// <summary>
//        /// Очистка матрицы и правой части
//        /// </summary>
//        public override void Clear()
//        { 
//            base.Clear();
//            Fp = null;
//        }
//        /// <summary>
//        /// Сборка ГМЖ
//        /// </summary>
//        public void AddToMatrix(double[][] LMartix, uint[] Adress)
//        {
//            SparseRow newRow = new SparseRow((int)Adress[0], Adress.Length, FN);
//            for (int i = 0; i < Adress.Length; i++)
//            {
//                newRow.SetZeroValue((int)Adress[i]);
//            }
//            for (int i = 0; i < Adress.Length; i++)
//            {
//                newRow.RowNumber = (int)Adress[i];
//                for (int j = 0; j < Adress.Length; j++)
//                    newRow.SetValue(j, (int)Adress[j], LMartix[i][j]);
//                rows[Adress[i] - 1].AddRange(1.0, newRow, ref indexArray);
//            }
//        }

//        /// <summary>
//        /// // Сборка ГПЧ
//        /// </summary>
//        public void AddToRight(double[] LRight, uint[] Adress)
//        {

//        }
//        /// <summary>
//        /// Сборка САУ по строкам (не для всех решателей)
//        /// </summary>
//        /// <param name="ColElems">Коэффициенты строки системы</param>
//        /// <param name="ColAdress">Адреса коэффицентов</param>
//        /// <param name="IndexRow">Индекс формируемой строки системы</param>
//        /// <param name="Right">Значение правой части</param>
//        public void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double BetaE1)
//        {

//        }
//        /// <summary>
//        /// Получить строку (не для всех решателей)
//        /// </summary>
//        /// <param name="IndexRow">Индекс получемой строки системы</param>
//        /// <param name="ColElems">Коэффициенты строки системы</param>
//        /// <param name="BetaE1">Значение правой части</param>
//        public void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double BetaE1)
//        {

//        }
//        /// <summary>
//        /// Добавление в правую часть
//        /// </summary>
//        public void CopyRight(double[] CRight)
//        {

//        }
//        /// <summary>
//        /// Получение правой части СЛАУ
//        /// </summary>
//        /// <param name="CRight"></param>
//        public void GetRight(ref double[] CRight)
//        {

//        }
//        /// <summary>
//        /// Удовлетворение ГУ
//        /// </summary>
//        public void BoundConditions(double[] Conditions, uint[] Adress)
//        {

//        }
//        /// <summary>
//        /// Выполнение ГУ
//        /// </summary>
//        public void BoundConditions(double Conditions, uint[] Adress)
//        {

//        }
//        /// <summary>
//        /// Операция определения невязки BetaE1 = Matrix X - Right
//        /// </summary>
//        /// <param name="BetaE1">результат</param>
//        /// <param name="X">умножаемый вектор</param>
//        /// <param name="IsRight">знак операции = +/- 1</param>
//        public void GetResidual(ref double[] BetaE1, double[] X, int IsRight = 1)
//        {

//        }
//        /// <summary>
//        /// Решение СЛУ
//        /// </summary>
//        public void Solve(ref double[] X)
//        {
//           // PCILU_gmres(ref X, int itnum, int varnum, int elcode, int M, int MaxIters, double rec/*, FILE* fp*/);
//        }
//        /// <summary>
//        /// Клонирование объекта
//        /// </summary>
//        /// <param name="N">порядок системы</param>
//        /// <returns></returns>
//        public IAlgebra Clone()
//        {
//            return new SparseAlgebraRiver(this);
//        }
//        /// <summary>
//        /// Вывод САУ на КОНСОЛЬ
//        /// </summary>
//        /// <param name="flag">количество знаков мантисы</param>
//        /// <param name="color">длина цветового блока</param>
//        public void Print(int flag = 0, int color = 1)
//        {

//        }

//        #endregion
//        public void preconditionedCGs(double[] X, double[] B)
//        {

//            double[] R, R0, BetaE1, Min, S;
//            int i, imax, j;
//            double deltaNew = 0, deltaOld = 0, delta0, err, err2delta0, alpha, beta, dTq;

//            R = R0 = BetaE1 = S = Min = null;

//            R = new double[FN];
//            R0 = new double[FN];
//            BetaE1 = new double[FN];
//            Min = new double[FN];
//            S = new double[FN];

//            //initializing variables for the iterative solution

//            i = 0;
//            imax = 2 * FN;   //theortically this method should converge in n iterations or less 
//            err = 0.000000001;
//            MatrixCRS_Mult(ref R, X);                                        // {R} = [A]{X}
//            for (j = 0; j < FN; j++) Min[j] = 1.0 / Value(j + 1, j + 1); // Preconditioner M[i] = K[i,i]
//            for (j = 0; j < FN; j++)
//            {
//                BetaE1[j] = B[j] - R[j];
//                R0[j] = Min[j] * BetaE1[j];
//                deltaNew += BetaE1[j] * R0[j];
//            }
//            delta0 = deltaNew;
//            //	err2delta0 = err*err*delta0;
//            err2delta0 = 0.0000001;

//            //the iterative solution

//            while ((i < imax) && (deltaNew > err2delta0))
//            {
//                MatrixCRS_Mult(ref R, R0);                                    // {R} = [A]{R0}
//                dTq = 0;                                            // sum of {R0[i]}*{R[i]}		
//                for (j = 0; j < FN; j++) dTq = dTq + R0[j] * R[j];
//                alpha = deltaNew / dTq;
//                for (j = 0; j < FN; j++) X[j] += alpha * R0[j];           // {X} = {X} + alpha*{R0}
//                if ((i % 50) == 0)                                  // if divisable by 50
//                {
//                    MatrixCRS_Mult(ref R, X);                                // {R} = [A]{X}
//                    for (j = 0; j < FN; j++) BetaE1[j] = B[j] - R[j];     // {BetaE1} = {B} - {R}
//                }
//                else
//                    for (j = 0; j < FN; j++) BetaE1[j] -= alpha * R[j];       // {BetaE1} = {BetaE1} - alpha*{R}
//                for (j = 0; j < FN; j++) S[j] = Min[j] * BetaE1[j];           // {S} = [Min]{BetaE1}
//                deltaOld = deltaNew;
//                deltaNew = 0;
//                for (j = 0; j < FN; j++) deltaNew += BetaE1[j] * S[j];
//                beta = deltaNew / deltaOld;
//                for (j = 0; j < FN; j++) R0[j] = S[j] + beta * R0[j];   // {R0} = {S} + beta*{R0}
//                i++;
//            }
//        }
//        public void conjugateGradients(double[] X, double[] B)
//        {
//            // the conjugate gradients solution code
//            double[] R, R0, BetaE1;
//            int i, imax, j;
//            double deltaNew = 0, deltaOld = 0, delta0, err, err2delta0, alpha, beta, dTq;


//            R = R0 = BetaE1 = null;

//            R = new double[FN];
//            R0 = new double[FN];
//            BetaE1 = new double[FN];

//            //initializing variables for the iterative solution

//            i = 0;
//            imax = 2 * FN;       //theortically this method should converge in n iterations or less 
//            err = 0.0000001;
//            MatrixCRS_Mult(ref R, X); // {R} = [A]{X}
//            for (j = 0; j < FN; j++)
//            {
//                BetaE1[j] = B[j] - R[j];
//                R0[j] = BetaE1[j];
//                deltaNew += BetaE1[j] * BetaE1[j];
//            }
//            delta0 = deltaNew;
//            //	err2delta0 = err*err*delta0;
//            err2delta0 = 0.0000001;

//            //the iterative solution

//            while ((i < imax) && (deltaNew > err2delta0))
//            {
//                MatrixCRS_Mult(ref R, R0);                                // {R} = [A]{R0}
//                dTq = 0;                                        // sum of {R0[i]}*{R[i]}		
//                for (j = 0; j < FN; j++) dTq = dTq + R0[j] * R[j];
//                alpha = deltaNew / dTq;
//                for (j = 0; j < FN; j++) X[j] += alpha * R0[j];       // {X} = {X} + alpha*{R0}
//                if ((i % 50) == 0)                              // if divisable by 50
//                {
//                    MatrixCRS_Mult(ref R, X);                        // {R} = [A]{X}
//                    for (j = 0; j < FN; j++) BetaE1[j] = B[j] - R[j];  // {BetaE1} = {B} - {R}
//                }
//                else
//                    for (j = 0; j < FN; j++) BetaE1[j] -= alpha * R[j];  // {BetaE1} = {BetaE1} - alpha*{R}
//                deltaOld = deltaNew;
//                deltaNew = 0;
//                for (j = 0; j < FN; j++) deltaNew += BetaE1[j] * BetaE1[j];
//                beta = deltaNew / deltaOld;
//                for (j = 0; j < FN; j++) R0[j] = BetaE1[j] + beta * R0[j]; // {R0} = {BetaE1} + beta*{R0}
//                i++;
//            }
//        }
//        public void gmres(double[] X, double[] B, int M, int MaxIters, double rec)
//        {
//            double[] R, R0, BetaE1, V, H;
//            int i, j, MP, i0, im, iters, ii;
//            double tem = 1, beta = 0, Cos, Sin, res0, resCheck;

//            R = R0 = BetaE1 = V = H = null;

//            R = new double[FN];
//            R0 = new double[FN];
//            BetaE1 = new double[M + 1];
//            V = new double[FN * M];
//            H = new double[M * (M + 1)];

//            MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}

//            for (ii = 0; ii < FN; ii++)
//            {
//                R[ii] = B[ii] - R[ii];
//                beta += R[ii] * R[ii];
//            }

//            beta = Math.Sqrt(beta);
//            res0 = beta;

//            //the iterative solution

//            for (iters = 0; iters < MaxIters; iters++)
//            {
//                MP = M;

//                //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
//                for (ii = 0; ii < FN; ii++) R[ii] /= beta;
//                for (ii = 0; ii <= MP; ii++) BetaE1[ii] = 0;
//                BetaE1[0] = beta;

//                for (j = 0; j < M; j++)
//                {
//                    for (ii = 0; ii < FN; ii++) V[ii * M + j] = R[ii];
//                    MatrixCRS_Mult(ref R0, R);
//                    for (ii = 0; ii < FN; ii++) R[ii] = R0[ii];

//                    // Зачем
//                    double sum1 = 0.0;
//                    for (ii = 0; ii < FN; ii++) sum1 += R[ii];

//                    for (i = 0; i <= j; i++)
//                    {
//                        H[i * M + j] = 0;
//                        for (ii = 0; ii < FN; ii++) H[i * M + j] += R[ii] * V[ii * M + i];
//                        for (ii = 0; ii < FN; ii++) R[ii] -= H[i * M + j] * V[ii * M + i];
//                    }
//                    tem = 0;
//                    for (ii = 0; ii < FN; ii++) tem += R[ii] * R[ii];
//                    tem = Math.Sqrt(tem);
//                    H[(j + 1) * M + j] = tem;
//                    if (tem < rec)
//                    {
//                        MP = j + 1;
//                        goto l5;
//                    }
//                    for (ii = 0; ii < FN; ii++) R[ii] /= tem;
//                }
//            //triangularization
//            l5: for (i = 0; i < MP; i++)
//                {
//                    im = i + 1;
//                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
//                    Cos = H[i * M + i] * tem;
//                    Sin = -H[im * M + i] * tem;
//                    for (j = i; j < MP; j++)
//                    {
//                        tem = H[i * M + j];
//                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
//                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
//                    }
//                    BetaE1[im] = Sin * BetaE1[i];
//                    BetaE1[i] *= Cos;
//                }
//                double sum = 0.0;
//                for (ii = 0; ii < (M * (M + 1)); ii++) sum += H[ii];
//                sum = 0.0;
//                for (ii = 0; ii < (M + 1); ii++) sum += BetaE1[ii];
//                //solution of linear system
//                for (i = (MP - 1); i >= 0; i--)
//                {
//                    BetaE1[i] /= H[i * M + i];
//                    for (i0 = (i - 1); i0 >= 0; i0--) BetaE1[i0] -= H[i0 * M + i] * BetaE1[i];
//                }
//                sum = 0.0;
//                for (ii = 0; ii < (M + 1); ii++) sum += BetaE1[ii];
//                for (i = 0; i < MP; i++)
//                {
//                    for (ii = 0; ii < FN; ii++) X[ii] += BetaE1[i] * V[ii * M + i];
//                }

//                //new residual and stopping tests
//                MatrixCRS_Mult(ref R, X);
//                beta = 0;
//                for (ii = 0; ii < FN; ii++)
//                {
//                    R[ii] = B[ii] - R[ii];
//                    beta += R[ii] * R[ii];
//                }
//                beta = Math.Sqrt(beta);
//                resCheck = beta / res0;
//                //	if (beta < rec) break;
//                if (resCheck < rec) break;

//            }
//        }
//        public int PCJAC_GMRES(double[] X, double[] B, int M, int MaxIters, double rec)
//        {

//            double[] R, R0, BetaE1, Min, V, H;
//            double tem = 1, beta = 0, res0, Cos, Sin, resCheck, convergence;
//            int ii, iters, i, j, MP, i0, im;

//            R = R0 = BetaE1 = Min = V = H = null;

//            R = new double[FN];
//            R0 = new double[FN];
//            BetaE1 = new double[M + 1];
//            Min = new double[FN];
//            V = new double[FN * M];
//            H = new double[M * (M + 1)];

//            //initializing variables for the iterative solution

//            MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
//            for (j = 0; j < FN; j++) Min[j] = 1.0 / Value(j + 1, j + 1); // Preconditioner M[i] = K[i,i]

//            for (ii = 0; ii < FN; ii++)
//            {
//                R0[ii] = B[ii] - R[ii];
//                R[ii] = Min[ii] * R0[ii];
//                beta += R[ii] * R[ii];
//            }

//            beta = Math.Sqrt(beta);
//            res0 = beta;

//            //the iterative solution

//            for (iters = 0; iters < MaxIters; iters++)
//            {
//                MP = M;

//                //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
//                for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / beta;
//                for (ii = 0; ii <= MP; ii++) BetaE1[ii] = 0;
//                BetaE1[0] = beta;

//                for (j = 0; j < M; j++)
//                {
//                    for (ii = 0; ii < FN; ii++) V[ii * M + j] = R[ii];
//                    MatrixCRS_Mult(ref R0, R);
//                    for (ii = 0; ii < FN; ii++) R[ii] = Min[ii] * R0[ii];

//                    for (i = 0; i <= j; i++)
//                    {
//                        H[i * M + j] = 0;
//                        for (ii = 0; ii < FN; ii++) H[i * M + j] += R[ii] * V[ii * M + i];
//                        for (ii = 0; ii < FN; ii++) R[ii] -= H[i * M + j] * V[ii * M + i];
//                    }
//                    tem = 0;
//                    for (ii = 0; ii < FN; ii++) tem += R[ii] * R[ii];
//                    tem = Math.Sqrt(tem);
//                    H[(j + 1) * M + j] = tem;
//                    if (tem < rec)
//                    {
//                        MP = j + 1;
//                        goto l5;
//                    }
//                    for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / tem;
//                }

//            //triangularization
//            l5: for (i = 0; i < MP; i++)
//                {
//                    im = i + 1;
//                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
//                    Cos = H[i * M + i] * tem;
//                    Sin = -H[im * M + i] * tem;
//                    for (j = i; j < MP; j++)
//                    {
//                        tem = H[i * M + j];
//                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
//                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
//                    }
//                    BetaE1[im] = Sin * BetaE1[i];
//                    BetaE1[i] = BetaE1[i] * Cos;
//                }
//                //solution of linear system
//                for (i = (MP - 1); i >= 0; i--)
//                {
//                    BetaE1[i] = BetaE1[i] / H[i * M + i];
//                    for (i0 = i - 1; i0 >= 0; i0--) BetaE1[i0] = BetaE1[i0] - H[i0 * M + i] * BetaE1[i];
//                }
//                for (i = 0; i < MP; i++)
//                {
//                    for (ii = 0; ii < FN; ii++)
//                        X[ii] += BetaE1[i] * V[ii * M + i];
//                }
//                //new residual and stopping tests
//                MatrixCRS_Mult(ref R, X);                                // {R} = [A]{X}
//                beta = 0;
//                for (ii = 0; ii < FN; ii++)
//                {
//                    R0[ii] = B[ii] - R[ii];
//                    R[ii] = Min[ii] * R0[ii];
//                    beta += R[ii] * R[ii];
//                }
//                beta = Math.Sqrt(beta);
//                resCheck = beta / res0;

//                if (resCheck < rec) break;
//                else if (iters == (MaxIters - 1)) ;

//            }
//            return (iters + 1);

//        }
//        public int PCJAC_GMRES(double[] B, int M, int MaxIters, double rec)
//        {

//            double[] X, R, R0, BetaE1, Min, V, H;
//            double tem = 1, beta = 0, res0, Cos, Sin, resCheck, convergence;
//            int ii, iters, i, j, MP, i0, im;

//            X = R = R0 = BetaE1 = Min = V = H = null;

//            X = new double[FN];
//            for (ii = 0; ii < FN; ii++) X[ii] = 0.0;
//            R = new double[FN];
//            R0 = new double[FN];
//            BetaE1 = new double[M + 1];
//            Min = new double[FN];
//            V = new double[FN * M];
//            H = new double[M * (M + 1)];

//            //initializing variables for the iterative solution

//            MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
//            for (j = 0; j < FN; j++) Min[j] = 1.0 / Value(j + 1, j + 1); // Preconditioner M[i] = K[i,i]

//            for (ii = 0; ii < FN; ii++)
//            {
//                R0[ii] = B[ii] - R[ii];
//                R[ii] = Min[ii] * R0[ii];
//                beta += R[ii] * R[ii];
//            }

//            beta = Math.Sqrt(beta);
//            res0 = beta;

//            //the iterative solution

//            for (iters = 0; iters < MaxIters; iters++)
//            {
//                MP = M;

//                //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
//                for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / beta;
//                for (ii = 0; ii <= MP; ii++) BetaE1[ii] = 0;
//                BetaE1[0] = beta;

//                for (j = 0; j < M; j++)
//                {
//                    for (ii = 0; ii < FN; ii++) V[ii * M + j] = R[ii];
//                    MatrixCRS_Mult(ref R0, R);
//                    for (ii = 0; ii < FN; ii++) R[ii] = Min[ii] * R0[ii];

//                    for (i = 0; i <= j; i++)
//                    {
//                        H[i * M + j] = 0;
//                        for (ii = 0; ii < FN; ii++) H[i * M + j] += R[ii] * V[ii * M + i];
//                        for (ii = 0; ii < FN; ii++) R[ii] -= H[i * M + j] * V[ii * M + i];
//                    }
//                    tem = 0;
//                    for (ii = 0; ii < FN; ii++) tem += R[ii] * R[ii];
//                    tem = Math.Sqrt(tem);
//                    H[(j + 1) * M + j] = tem;
//                    if (tem < rec)
//                    {
//                        MP = j + 1;
//                        goto l5;
//                    }
//                    for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / tem;
//                }

//            //triangularization
//            l5: for (i = 0; i < MP; i++)
//                {
//                    im = i + 1;
//                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
//                    Cos = H[i * M + i] * tem;
//                    Sin = -H[im * M + i] * tem;
//                    for (j = i; j < MP; j++)
//                    {
//                        tem = H[i * M + j];
//                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
//                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
//                    }
//                    BetaE1[im] = Sin * BetaE1[i];
//                    BetaE1[i] = BetaE1[i] * Cos;
//                }
//                //solution of linear system
//                for (i = (MP - 1); i >= 0; i--)
//                {
//                    BetaE1[i] = BetaE1[i] / H[i * M + i];
//                    for (i0 = i - 1; i0 >= 0; i0--) BetaE1[i0] = BetaE1[i0] - H[i0 * M + i] * BetaE1[i];
//                }
//                for (i = 0; i < MP; i++)
//                {
//                    for (ii = 0; ii < FN; ii++)
//                        X[ii] += BetaE1[i] * V[ii * M + i];
//                }
//                //new residual and stopping tests
//                MatrixCRS_Mult(ref R, X);                                // {R} = [A]{X}
//                beta = 0;
//                for (ii = 0; ii < FN; ii++)
//                {
//                    R0[ii] = B[ii] - R[ii];
//                    R[ii] = Min[ii] * R0[ii];
//                    beta += R[ii] * R[ii];
//                }
//                beta = Math.Sqrt(beta);
//                resCheck = beta / res0;

//                if (resCheck < rec) break;
//                else if (iters == (MaxIters - 1)) ;

//            }

//            for (ii = 0; ii < FN; ii++) B[ii] = X[ii];
//            return (iters + 1);
//        }
//        public int PCILU_GMRES(double[] X, double[] B, int M, int MaxIters, double rec)
//        {
//            double tem = 1, beta = 0, res0, Cos, Sin, resCheck, convergence;
//            int ii, iters, i, j, MP, i0, im;
            
//            double[] R, R0, BetaE1, Y, V, H;
//            R = R0 = BetaE1 = Y = V = H = null;
//            MEM.Alloc(FN, ref R);
//            MEM.Alloc(FN, ref R0);
//            MEM.Alloc(FN, ref Y);
//            MEM.Alloc(FN * M, ref V);
//            MEM.Alloc(M + 1, ref BetaE1);
//            MEM.Alloc((M + 1) * M, ref H);

//            SparseMatrix ILUmat = new SparseMatrix(this, FN);
//            ILUmat.LUDecompose();
            

//            MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
//            for (ii = 0; ii < FN; ii++)
//            {
//                R0[ii] = B[ii] - R[ii];
//                R[ii] = R0[ii];
//                beta += R[ii] * R[ii];
//            }
//            beta = Math.Sqrt(beta);
//            res0 = beta;

//            // основной цикл
//            for (iters = 0; iters < MaxIters; iters++)
//            {
//                MP = M;
//                // Метод Арнольди (который использует метод
//                // ортогонализации Грама-Шмидта)
//                for (ii = 0; ii < FN; ii++) 
//                    R[ii] = R[ii] / beta;
//                for (ii = 0; ii < FN; ii++) 
//                    Y[ii] = 0;
//                for (ii = 0; ii <= MP; ii++) 
//                    BetaE1[ii] = 0;
//                BetaE1[0] = beta;

//                for (j = 0; j < M; j++)
//                {
                    
//                    for (ii = 0; ii < FN; ii++) 
//                        V[ii * M + j] = R[ii];

//                    ILUmat.Precondition(R);
//                    MatrixCRS_Mult(ref R0, R);
                    
//                    for (ii = 0; ii < FN; ii++) 
//                        R[ii] = R0[ii];

//                    for (i = 0; i <= j; i++)
//                    {
//                        H[i * M + j] = 0;
//                        for (ii = 0; ii < FN; ii++) 
//                            H[i * M + j] += R[ii] * V[ii * M + i];
//                        for (ii = 0; ii < FN; ii++) 
//                            R[ii] -= H[i * M + j] * V[ii * M + i];
//                    }

//                    tem = 0;
//                    for (ii = 0; ii < FN; ii++) 
//                        tem += R[ii] * R[ii];
//                    tem = Math.Sqrt(tem);
//                    H[(j + 1) * M + j] = tem;
//                    if (tem < rec)
//                    {
//                        MP = j + 1;
//                        goto l5;
//                    }
//                    for (ii = 0; ii < FN; ii++) 
//                        R[ii] = R[ii] / tem;
//                }

//            //triangularization
//            l5: for (i = 0; i < MP; i++)
//                {
//                    im = i + 1;
//                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + 
//                                             H[im * M + i] * H[im * M + i]));
//                    Cos = H[i * M + i] * tem;
//                    Sin = -H[im * M + i] * tem;
//                    for (j = i; j < MP; j++)
//                    {
//                        tem = H[i * M + j];
//                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
//                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
//                    }
//                    BetaE1[im] = Sin * BetaE1[i];
//                    BetaE1[i] = BetaE1[i] * Cos;
//                }
//                // решение линейной системы
//                // выполняем обратный ход
//                for (i = (MP - 1); i >= 0; i--)
//                {
//                    BetaE1[i] = BetaE1[i] / H[i * M + i];
//                    for (i0 = (i - 1); i0 >= 0; i0--) 
//                        BetaE1[i0] = BetaE1[i0] - H[i0 * M + i] * BetaE1[i];
//                }
//                // находим вектор поправок
//                for (i = 0; i < MP; i++)
//                {
//                    for (ii = 0; ii < FN; ii++)
//                        Y[ii] += BetaE1[i] * V[ii * M + i];
//                }
//                // предобуславливаем вектор поправок
//                ILUmat.Precondition(Y);
//                // корректируем решение 
//                for (ii = 0; ii < FN; ii++) 
//                    X[ii] += Y[ii];
//                // произведение нового решения на матрицу
//                MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}
//                // определение новой невязки R
//                beta = 0;
//                for (ii = 0; ii < FN; ii++)
//                {
//                    R0[ii] = B[ii] - R[ii];
//                    R[ii] = R0[ii];
//                    beta += R[ii] * R[ii];
//                }
//                // нормы невязки
//                beta = Math.Sqrt(beta);
//                resCheck = beta / res0;
//                // проверка на сходимость и остановку итерационного процесса
//                if (resCheck < rec) break;
//                else if (iters == (MaxIters - 1)) ;
//            }
//            return (iters + 1);
//        }
//        public int PCILU_GMRES(double[] B, int M, int MaxIters, double rec)
//        {

//            double[] X, R, R0, BetaE1, Y, V, H;
//            double tem = 1, beta = 0, res0, Cos, Sin, resCheck, convergence;
//            int ii, iters, i, j, MP, i0, im;

//            X = R = R0 = BetaE1 = Y = V = H = null;

//            X = new double[FN];
//            for (ii = 0; ii < FN; ii++) 
//                X[ii] = 0.0;
//            R = new double[FN];
//            R0 = new double[FN];
//            BetaE1 = new double[M + 1];
//            Y = new double[FN];
//            V = new double[FN * M];
//            H = new double[M * (M + 1)];

//            SparseMatrix ILUmat = null;

//            ILUmat = new SparseMatrix(this, FN);

//            ILUmat.LUDecompose();

//            //initializing variables for the iterative solution

//            MatrixCRS_Mult(ref R, X);     // {R} = [A]{X}

//            for (ii = 0; ii < FN; ii++)
//            {
//                R0[ii] = B[ii] - R[ii];
//                R[ii] = R0[ii];
//                beta += R[ii] * R[ii];
//            }

//            beta = Math.Sqrt(beta);
//            res0 = beta;

//            //the iterative solution

//            for (iters = 0; iters < MaxIters; iters++)
//            {
//                MP = M;

//                //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
//                for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / beta;
//                for (ii = 0; ii < FN; ii++) Y[ii] = 0;
//                for (ii = 0; ii <= MP; ii++) BetaE1[ii] = 0;
//                BetaE1[0] = beta;

//                for (j = 0; j < M; j++)
//                {
//                    for (ii = 0; ii < FN; ii++) V[ii * M + j] = R[ii];
//                    ILUmat.Precondition(R);
//                    //ILUmat.ForwardSweep(R);
//                    //ILUmat.BackSubstitute(R);
//                    MatrixCRS_Mult(ref R0, R);
//                    for (ii = 0; ii < FN; ii++) R[ii] = R0[ii];

//                    for (i = 0; i <= j; i++)
//                    {
//                        H[i * M + j] = 0;
//                        for (ii = 0; ii < FN; ii++) H[i * M + j] += R[ii] * V[ii * M + i];
//                        for (ii = 0; ii < FN; ii++) R[ii] -= H[i * M + j] * V[ii * M + i];
//                    }
//                    tem = 0;
//                    for (ii = 0; ii < FN; ii++) tem += R[ii] * R[ii];
//                    tem = Math.Sqrt(tem);
//                    H[(j + 1) * M + j] = tem;
//                    if (tem < rec)
//                    {
//                        MP = j + 1;
//                        goto l5;
//                    }
//                    for (ii = 0; ii < FN; ii++) R[ii] = R[ii] / tem;
//                }

//            //triangularization
//            l5: for (i = 0; i < MP; i++)
//                {
//                    im = i + 1;
//                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
//                    Cos = H[i * M + i] * tem;
//                    Sin = -H[im * M + i] * tem;
//                    for (j = i; j < MP; j++)
//                    {
//                        tem = H[i * M + j];
//                        H[i * M + j] = Cos * tem - Sin * H[im * M + j];
//                        H[im * M + j] = Sin * tem + Cos * H[im * M + j];
//                    }
//                    BetaE1[im] = Sin * BetaE1[i];
//                    BetaE1[i] = BetaE1[i] * Cos;
//                }
//                //solution of linear system
//                for (i = (MP - 1); i >= 0; i--)
//                {
//                    BetaE1[i] = BetaE1[i] / H[i * M + i];
//                    for (i0 = (i - 1); i0 >= 0; i0--) BetaE1[i0] = BetaE1[i0] - H[i0 * M + i] * BetaE1[i];
//                }
//                for (i = 0; i < MP; i++)
//                {
//                    for (ii = 0; ii < FN; ii++)
//                        Y[ii] += BetaE1[i] * V[ii * M + i];
//                }
//                //new residual and stopping tests
//                ILUmat.Precondition(Y);
//                //ILUmat.ForwardSweep(Y);
//                //ILUmat.BackSubstitute(Y);

//                for (ii = 0; ii < FN; ii++) X[ii] += Y[ii];
//                MatrixCRS_Mult(ref R, X);                                // {R} = [A]{X}
//                beta = 0;
//                for (ii = 0; ii < FN; ii++)
//                {
//                    R0[ii] = B[ii] - R[ii];
//                    R[ii] = R0[ii];
//                    beta += R[ii] * R[ii];
//                }
//                beta = Math.Sqrt(beta);
//                resCheck = beta / res0;

//                if (resCheck < rec) break;
//                else if (iters == (MaxIters - 1)) ;

//            }
//            for (ii = 0; ii < FN; ii++) B[ii] = X[ii];
//            return (iters + 1);
//        }
//        public int PCILU_gmres(ref double[] X, int itnum, int varnum, int elcode, int M, int MaxIters, double rec/*, FILE* fp*/)
//        {
//            int i, j, MP, i0, im, iters, ii;
//            double tem = 1, beta, res0, resCheck, zero, Cos, Sin, convergence;
            
//            double[] Dp = null, Bp = null,  BetaE1 = null, Yp = null, Rp = null;
            
//            double[,] Hp = null;
//            double[,] Vp = null;
//            MEM.Alloc0(N, ref Bp);
//            MEM.Alloc0(N, ref Dp);
//            MEM.Alloc0(N, ref BetaE1);
//            MEM.Alloc0(N, ref Yp);
//            MEM.Alloc0(N, ref Rp);
//            MEM.Alloc(M, M, ref Vp);
//            MEM.Alloc(M, M + 1, ref Hp);

//            SparseMatrix KPrime = this;
//            SparseMatrix ILUmat = null;

//            zero = rec;
//            // вызывает конструктор копирования
//            ILUmat = new SparseMatrix(KPrime, FN); 
//            ILUmat.LUDecompose();

//            for (ii = 0; ii < FN; ii++)
//                Fp[ii] = 0.0;
//            // получаем первый вектор ошибки
//            // Вычисляем BetaE1 = M^-1 * (B - A*X)
//            KPrime.Mult_MatrixCRS(ref BetaE1, Fp);
//            beta = 0;
//            for (ii = 0; ii < FN; ii++)
//            {
//                Dp[ii] = Bp[ii] - BetaE1[ii]; // residual vector
//                BetaE1[ii] = Dp[ii];
//                beta += BetaE1[ii] * BetaE1[ii];
//            }
//            beta = Math.Sqrt(beta);
//            res0 = beta;
//            //loop that governs number of reinitializations MaxIters
//            //could eventually be a while statement...
//            for (iters = 0; iters < MaxIters; iters++)
//            {
//                MP = M;
//                //Arnoldi method (which uses the Gram-Schmidt orthogonalization method)
//                for (ii = 0; ii < FN; ii++) BetaE1[ii] /= beta;
//                for (ii = 0; ii < FN; ii++) Yp[ii] = 0;
                
//                for (ii = 0; ii <= MP; ii++) Rp[ii] = 0;
//                Rp[0] = beta;

//                for (j = 0; j < M; j++)
//                {
//                    for (ii = 0; ii < FN; ii++) Vp[ii, j] = BetaE1[ii];
//                    ILUmat.Precondition(BetaE1);
//                    //ILUmat.ForwardSweep(BetaE1);
//                    //ILUmat.BackSubstitute(BetaE1);
//                    KPrime.Mult_MatrixCRS(ref Dp, BetaE1);

//                    for (ii = 0; ii < FN; ii++)
//                        BetaE1[ii] = Dp[ii];

//                    for (i = 0; i <= j; i++)
//                    {
//                        Hp[i, j] = 0;
//                        for (ii = 0; ii < FN; ii++) 
//                            Hp[i, j] += BetaE1[ii] * Vp[ii, i];
//                        for (ii = 0; ii < FN; ii++) 
//                            BetaE1[ii] -= Hp[i, j] * Vp[ii, i];
//                    }
//                    tem = 0;
//                    for (ii = 0; ii < FN; ii++) 
//                        tem += BetaE1[ii] * BetaE1[ii];
                    
//                    tem = Math.Sqrt(tem);
//                    Hp[j + 1, j] = tem;
//                    if (tem < rec)
//                    {
//                        MP = j + 1;
//                        goto l5;
//                    }
//                    for (ii = 0; ii < FN; ii++) 
//                        BetaE1[ii] /= tem;
//                }
//            // приведение H к тругольному виду методом вращений
//            l5: for (i = 0; i < MP; i++)
//                {
//                    im = i + 1;
//                    tem = (1.0) / (Math.Sqrt( Hp[i, i] * Hp[i, i] + Hp[im, i] * Hp[im, i]));
//                    Cos = Hp[i, i] * tem;
//                    Sin = -Hp[im, i] * tem;
//                    for (j = i; j < MP; j++)
//                    {
//                        tem = Hp[i, j];
//                        Hp[i, j] = Cos * tem - Sin * Hp[im, j];
//                        Hp[im, j] = Sin * tem + Cos * Hp[im, j];
//                    }
//                    Rp[im] = Sin * Rp[i];
//                    Rp[i] *= Cos;
//                }
//                // solution of linear system
//                for (i = (MP - 1); i >= 0; i--)
//                {
//                    Rp[i] /= Hp[i, i];
//                    for (i0 = i - 1; i0 >= 0; i0--)
//                        Rp[i0] -= Hp[i0, i] * Rp[i];
//                }
//                // for (ii =0; ii < FN; ii++) BetaE1[ii]=0;
//                for (i = 0; i < MP; i++)
//                {
//                    for (ii = 0; ii < FN; ii++) Yp[ii] += Rp[i] * Vp[ii, i];
//                }
//                ILUmat.Precondition(Yp);
//                //ILUmat.ForwardSweep(Yp);
//                //ILUmat.BackSubstitute(Yp);
//                for (ii = 0; ii < FN; ii++) 
//                    Fp[ii] += Yp[ii];

//                //new residual and stopping tests
//                KPrime.Mult_MatrixCRS(ref BetaE1, Fp);
//                //matrixVectorMult(Fp, BetaE1, itnum, varnum, -1);
//                beta = 0;
//                for (ii = 0; ii < FN; ii++)
//                {
//                    Dp[ii] = Bp[ii] - BetaE1[ii];
//                    BetaE1[ii] = Dp[ii];
//                    beta += BetaE1[ii] * BetaE1[ii];
//                }
//                beta = Math.Sqrt(beta);
//                resCheck = beta / res0;
//                convergence = 1.0 - (beta / res0);
                
//                if (resCheck < rec) 
//                    break;
//                else // Stopped before convergence 
//                    if (iters == (MaxIters - 1)) ;
//            }
//            MEM.Copy(ref X, Fp);
//            ILUmat.Clear();
//            return (iters + 1);
//        }
//    }
//}
