//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//                      01.08.2021 Потапов И.И. 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using CommonLib;
    using System;
    /// <summary>
    /// ОО: Решение системы A x = b с помощью LU разложения 
    /// </summary>
    public class AlgebraLUMax : AFullAlgebra
    {
        double[] work;
        int[] index;
        public AlgebraLUMax(uint NN) : base(new AlgebraResult(), NN)
        {
            name = "Метод LU разложения с выбором ведущего элемента для полной матрицы";
            SetAlgebra(result, NN);
            result.Name = name;
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                MEM.AllocClear(FN, ref work);
                MEM.Alloc<int>((int)FN, ref index);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        protected override void solve(ref double[] X)
        {
            int sgn = 0;
            Result.conditionality = LU_Diccomposition(ref sgn);
            LU_Solve(Matrix, ref Right);
            MEM.MemCopy(ref X, Right);
            Result.X = X;
        }
        protected double LU_Diccomposition(ref int sgn)
        {
            double cond = 0;
            sgn = 1;
            if (FN != 1)
            {
                int i, j, k, ip;
                int y, Pivot;
                double norm_A = 0.0;
                double Elem, Pr, t;
                double[] vy = work;
                double ek, ynorm, znorm;

                // Вычисление нормы матрицы Matrix
                for (j = 0; j < FN; j++)
                {
                    t = 0.0;
                    for (i = 0; i < FN; i++)
                        t += Math.Abs(Matrix[i][j]);
                    if (norm_A < t)
                        norm_A = t;
                }

                for (y = 0; y < FN - 1; y++)
                {
                    // поиск максимального элемента в столбце y
                    double Max = Math.Abs(Matrix[y][y]);
                    Pivot = y;
                    for (i = y + 1; i < FN; i++)
                    {
                        Elem = Math.Abs(Matrix[i][y]);
                        if (Elem >= Max)
                        {
                            Max = Elem;
                            Pivot = i;
                        }
                    }
                    if (y != Pivot)
                    {
                        // перестановка строк Pivot и y
                        // cblas_dswap(CblasRowMajor, Matrix + pivot*FN, 1, Matrix + y*FN, 1)
                        for (j = 0; j < FN; j++)
                        {
                            LU_SWAPD(ref Matrix[Pivot][j], ref Matrix[y][j]);
                        }
                        sgn = -sgn;
                    }
                    index[y] = Pivot;
                    if (Max > 0)
                    {
                        Pr = 1.0 / Matrix[y][y];
                        for (j = y + 1; j < FN; j++)
                        {
                            Matrix[j][y] *= Pr;
                            t = Matrix[j][y];
                            for (i = y + 1; i < FN; i++)
                                Matrix[j][i] -= t * Matrix[y][i];
                        }
                    };
                }

                // решение системы Matrix^t*y = E
                for (k = 0; k < FN; k++)
                {
                    t = 0.0;
                    for (i = 0; i < k; i++)
                        t += Matrix[i][k] * vy[i];
                    ek = (t >= 0.0) ? 1.0 : -1.0;
                    vy[k] = -(ek + t) / Matrix[k][k];
                }
                for (k = (int)FN - 1; ; k--)
                {
                    t = 0.0;
                    for (i = k + 1; i < FN; i++)
                        t += Matrix[i][k] * vy[i];
                    vy[k] -= t;
                    if (k == 0) break;
                }
                for (i = 0; i < FN - 1; i++)
                {
                    ip = index[i];
                    if (ip != i)
                        LU_SWAPD(ref vy[ip], ref vy[i]);
                }

                ynorm = 0.0;
                for (i = 0; i < FN; i++) ynorm += Math.Abs(vy[i]);

                // решение системы Matrix*z = y
                LU_Solve(Matrix, ref vy);

                znorm = 0.0;
                for (i = 0; i < FN; i++) znorm += Math.Abs(vy[i]);

                cond = norm_A * znorm / ynorm;
                if (cond < 1.0) cond = 1.0;

            }
            index[FN - 1] = 0;
            return cond;
        }
        protected void LU_SWAPD(ref double a, ref double b)
        {
            double temp = a;
            a = b;
            b = temp;
        }
        protected void LU_Solve(double[][] LU, ref double[] right)
        {
            int i, ip, j;
            double t;

            if (FN > 1)
            {
                for (i = 0; i < FN - 1; i++)
                {
                    ip = index[i];
                    LU_SWAPD(ref right[ip], ref right[i]);
                }

                for (i = 0; i < FN - 1; i++)
                {
                    t = right[i];
                    for (j = i + 1; j < FN; j++)
                        right[j] -= LU[j][i] * t;
                }
                for (i = (int)FN - 1; i >= 1; i--)
                {
                    right[i] /= LU[i][i];
                    t = -right[i];
                    for (j = 0; j < i; j++)
                        right[j] += LU[j][i] * t;
                };
            }
            right[0] /= LU[0][0];
        }

        public override IAlgebra Clone()
        {
            return new AlgebraLUMax(N);
        }
    }
}