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
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Решение системы A x = b с помощью LU разложения (для тестов без оптимизации)
    /// </summary>
    public class AlgebraLU : AFullAlgebra
    {
        double sum = 0;
        double[,] lu;
        double[] y;
        public AlgebraLU(uint NN) : base(new AlgebraResult(), NN)
        {
            name = "Метод LU разложения для полной матрицы";
            SetAlgebra(result, NN);
            result.Name = name;
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                MEM.Alloc2DClear(FN, ref lu);
                MEM.AllocClear(FN, ref y);
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
            MEM.AllocClear(FN, ref X);
            // декомпозиция Мatrix
            DecompositionMatrix();
            BackSolve(ref X);
        }
        /// <summary>
        ///  Обратный ход
        /// </summary>
        /// <param name="X"></param>
        public void BackSolve(ref double[] X)
        {
            double t;
            MEM.AllocClear(FN, ref y);
            // find solution of Ly = b
            for (int i = 0; i < FN; i++)
            {
                sum = 0;
                for (int k = 0; k < i; k++)
                    sum += lu[i, k] * y[k];
                y[i] = Right[i] - sum;
            }
            // find solution of Ux = y
            for (int i = (int)FN - 1; i >= 0; i--)
            {
                t = 1.0 / lu[i, i];
                sum = 0;
                for (int k = i + 1; k < FN; k++)
                    sum += lu[i, k] * X[k];
                X[i] = t * (y[i] - sum);
            }
        }
        /// <summary>
        /// Декомпозиция матрицы на A => LU
        /// </summary>
        public void DecompositionMatrix()
        {
            for (int i = 0; i < FN; i++)
            {
                for (int j = i; j < FN; j++)
                {
                    sum = 0;
                    for (int k = 0; k < i; k++)
                        sum += lu[i, k] * lu[k, j];
                    lu[i, j] = Matrix[i][j] - sum;
                }
                for (int j = i + 1; j < FN; j++)
                {
                    sum = 0;
                    for (int k = 0; k < i; k++)
                        sum += lu[j, k] * lu[k, i];
                    lu[j, i] = (1 / lu[i, i]) * (Matrix[j][i] - sum);
                }
            }
        }
        public override IAlgebra Clone()
        {
            return new AlgebraLU(N);
        }
    }
}