//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//          Решение САУ методом бисопряженных градиентов C++
//                        Потапов И.И.
//                         30.05.02
//---------------------------------------------------------------------------
//                        Потапов И.И.
//          HBEConjuageGradient (C++) ==> SparseAlgebraBeCG (C#)
//                       10.05.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Класс решатель САУ методом сопряженных градиентов
    /// с хранением матрицы в формате CRS
    /// и поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public class SparseAlgebraBeCG : SparseAlgebra
    {
        double Omega, Rho0;
        double Alpha, Beta, Rho;
        /// <summary>
        /// вектор ошибки
        /// </summary>
        protected double[] R;
        /// <summary>
        /// начальный вектор ошибки
        /// </summary>
        protected double[] RB;
        /// <summary>
        /// сопряженный вектор направления
        /// </summary>
        protected double[] P;
        /// <summary>
        /// бисопряженный вектор  направления
        /// </summary>
        protected double[] S;
        /// <summary>
        /// произведение векторов на матрицу
        /// </summary>
        protected double[] Ap;
        protected double[] As;
        protected double[] Ax;
        public SparseAlgebraBeCG(uint FN, bool isPrecond = false) : base(FN, isPrecond)
        {
            name = "Метод би сопряженных градиентов для матрицы в формате CRS";
            SetAlgebra(result, FN);
            result.Name = name;
        }

        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                MEM.AllocClear((int)FN, ref R);
                MEM.AllocClear((int)FN, ref RB);
                MEM.AllocClear((int)FN, ref P);
                MEM.AllocClear((int)FN, ref S);
                MEM.AllocClear((int)FN, ref As);
                MEM.AllocClear((int)FN, ref Ap);
                MEM.AllocClear((int)FN, ref Ax);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        /// <param name="X">начальное приближение=>решение</param>
        protected override void solve(ref double[] X)
        {
            Rho = 1; Alpha = 1; Omega = 1; Rho0 = 1; Beta = 0;
            // начальное приближение
            MEM.AllocClear((int)FN, ref R);
            MEM.AllocClear((int)FN, ref RB);
            MEM.AllocClear((int)FN, ref P);
            MEM.AllocClear((int)FN, ref S);
            MEM.AllocClear((int)FN, ref As);
            MEM.AllocClear((int)FN, ref Ap);
            MEM.AllocClear((int)FN, ref Ax);
            getResidual(ref Ax, X);  // Ax = A*X
            // R = F - A*X              // вектор ошибки
            for (uint i = 0; i < Right.Length; i++)
                R[i] = Right[i] - Ax[i];
            // RB = R
            for (int j = 0; j < N; j++)
                RB[j] = R[j];
            // цикл по сходимости
            int iters = 0;
            uint Maxiters = 2 * FN;
            for (iters = 0; iters < Maxiters; iters++)
            {
                Rho = MultyVector(RB, R);
                Beta = Rho / Rho0 * Alpha / Omega;
                for (int j = 0; j < N; j++)
                    P[j] = R[j] + Beta * (P[j] - Omega * Ap[j]);
                getResidual(ref Ap, P);
                Alpha = Rho / MultyVector(RB, Ap);
                for (int j = 0; j < N; j++)
                    S[j] = R[j] - Alpha * Ap[j];
                getResidual(ref As, S);
                Omega = MultyVector(As, S) / MultyVector(As, As);
                for (int j = 0; j < N; j++)
                {
                    X[j] = X[j] + Omega * S[j] + Alpha * P[j];
                    R[j] = S[j] - Omega * As[j];
                }
                // встряска решения
                //if (iters % 1000 == 0)
                //{
                //    // R = A*X - F             // вектор ошибки
                //    getResidual(ref Ax, X);  // R = A*X
                //    for (uint k = 0; k < Right.Length; k++)
                //        R[k] = Right[k] - Ax[k];
                //}
                Rho0 = Rho;
                if (double.IsNaN(Beta) == true || double.IsNaN(Omega) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(Rho);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, деление на 0";
                    break;
                }
                if (double.IsInfinity(Beta) == true || double.IsInfinity(Omega) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(Rho);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, переполнение связанное с расходимостью";
                    break;
                }
                if (Check(X) < MEM.Error9)
                {
                    //if (Debug == true)
                    Console.WriteLine("Итераций {0}", iters);
                    break;
                }
            }
            result.X = X;
            ((AlgebraResultIterative)result).Iterations = iters;
            if (iters > Maxiters - 2)
            {
                result.errorType = ErrorType.convergenceStagnation;
                result.Message = " Решение СЛАУ не получено";
                ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(Rho);
            }
        }
        protected double Check(double[] X)
        {
            double ss, errS = 0;
            double pp, errP = 0;
            double xx, maxX = 0;
            for (int i = 0; i < N; i++)
            {
                ss = Math.Abs(S[i]);
                if (errS < ss)
                    errS = ss;
                pp = Math.Abs(P[i]);
                if (errP < pp)
                    errP = pp;
                xx = Math.Abs(X[i]);
                if (maxX > xx)
                    maxX = xx;
            }
            maxX = (maxX < MEM.Error9) ? maxX + MEM.Error9 : maxX;
            double errorX = errS * Omega + Alpha * errP;
            double error = Math.Abs(errorX / maxX);

            ((AlgebraResultIterative)result).Error_C = errorX;
            ((AlgebraResultIterative)result).ratioError = error;
            ((AlgebraResultIterative)result).normA = maxX;

            return error;
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new SparseAlgebraBeCG(this.N);
        }
    }
}