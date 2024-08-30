namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Решение симметричной положительно определенной системы A x = b 
    /// с помощью разложения Холецкого
    /// </summary>
    public class AlgebraCholetsky : AFullAlgebra
    {
        public AlgebraCholetsky(uint NN) : base(new AlgebraResultDirect(), NN)
        {
            name = "Метод Холецкого для полной матрицы";
            result.Name = name;
        }
        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        protected override void solve(ref double[] X)
        {
            int res = CholeskyDiccomposition();
            if (res != 1)
            {
                result.Message = " Матрица не является положительно определенной." +
                                 " Решение СЛАУ методом Холецкого невозможно";
                result.X = X;
                result.errorType = ErrorType.methodCannotSolveSuchSLAEs;
                return;
            }
            CholeskySolve();
            MEM.MemCopy(ref X, Right);
            Result.X = X;
        }
        /// <summary>
        /// LL разложение Холецкого
        /// </summary>
        /// <returns></returns>
        protected int CholeskyDiccomposition()
        {
            double diag, sum;
            int i, j, k;
            if ((diag = Matrix[0][0]) <= 0)
                return -1;
            Matrix[0][0] = Math.Sqrt(diag);

            for (i = 1; i < FN; i++)
            {
                diag = Matrix[i][i];
                for (j = 0; j < i; j++)
                {
                    sum = 0;
                    for (k = 0; k < j; k++)
                        sum += Matrix[i][k] * Matrix[j][k];
                    Matrix[i][j] = (Matrix[i][j] - sum) / Matrix[j][j];
                }
                for (k = 0; k < i; k++)
                {
                    diag -= Matrix[i][k] * Matrix[i][k];
                }
                if (diag <= 0)
                    return -i - 1;
                Matrix[i][i] = Math.Sqrt(diag);
            }
            return 1;
        }
        protected void CholeskySolve()
        {
            int i, j;
            double t;
            if (FN > 1)
            {
                for (i = 0; i < FN; i++)
                {
                    t = Right[i] /= Matrix[i][i];
                    for (j = i + 1; j < FN; j++)
                        Right[j] -= Matrix[j][i] * t;
                }
                for (i = (int)FN - 1; i >= 1; i--)
                {
                    Right[i] /= Matrix[i][i];
                    t = -Right[i];
                    for (j = 0; j < i; j++)
                        Right[j] += Matrix[i][j] * t;
                }
            }
            Right[0] /= Matrix[0][0];
        }
        public override IAlgebra Clone()
        {
            return new AlgebraCholetsky(N);
        }
    }
}