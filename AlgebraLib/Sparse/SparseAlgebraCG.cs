//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//          Решение САУ методом сопряженных градиентов C++
//                        Потапов И.И.
//                         30.05.02
//---------------------------------------------------------------------------
//                        Потапов И.И.
//          HConjuageGradient (C++) ==> SparseAlgebraCG (C#)
//                       18.04.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using CommonLib;
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Класс решатель САУ методом сопряженных градиентов
    /// с хранением матрицы в формате CRS
    /// и поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public class SparseAlgebraCG : SparseAlgebra
    {
        protected double[] R;           // вектор ошибки
        protected double[] P;           // сопряженный вектор
        protected double[] AP;          // произведение сопряженного вектора на матрицу
        public SparseAlgebraCG(uint FN, bool isPrecond = false) : base(FN, isPrecond)
        {
            name = "Метод сопряженных градиентов для матрицы в формате CRS";
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
                MEM.AllocClear((int)FN, ref P);
                MEM.AllocClear((int)FN, ref AP);
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
        protected override void solve(ref double[] X)
        {
            double sR = Right.Sum(x => Math.Abs(x));
            if (sR < MEM.Error10)
            {
                MEM.AllocClear((int)FN, ref X);
                Logger.Instance.Info("правая часть системы вырождена");
                Logger.Instance.Info("the right side of the system is degenerate");
                return;
            }
            double Alpha, Betta, Rho = 0, pAp;
            // начальное приближение
            MEM.AllocClear((int)FN, ref R);
            MEM.AllocClear((int)FN, ref P);
            MEM.AllocClear((int)FN, ref AP);
            getResidual(ref R, X);  // R = A*X0
            // R0 = A*X0 - F             // вектор ошибки
            for (uint i = 0; i < Right.Length; i++)
                R[i] -= Right[i];
            // начальное приближение
            for (uint i = 0; i < N; i++)
                P[i] = R[i];             // P0 = R0
            // цикл по сходимости
            int iters = 0;
            uint Maxiters = 2 * FN;
            for (iters = 0; iters < Maxiters; iters++)
            {
                getResidual(ref AP, P);  // AP = A * P
                                         // граничные условия
                                         // AP && BCond
                for (int i = 0; i < N; i++)
                    if (Matrix[i].Count == 1)
                        AP[i] = 0;
                // минимизация ошибки решения по текущему направлению
                Rho = MultyVector(R, R);
                pAp = MultyVector(P, AP);

                ((AlgebraResultIterative)result).Error0_L2 = Rho;

                if (pAp <= Rho && Rho < MEM.Error12)
                    break;
                Alpha = -Rho / pAp;
                // Модификация вектора решения и вектора ошибки
                // Проверка сходимости
                if (CalcX(ref X, Alpha) == true)
                    break;
                // поправка ошибки
                for (uint i = 0; i < R.Length; i++)
                    R[i] += Alpha * AP[i];
                // расчет нового сопряженного направления
                Betta = MultyVector(R, R) / Rho;
                // новый вектор направлений                
                for (int i = 0; i < N; i++)
                    P[i] = R[i] + Betta * P[i];
            }
            Result.X = X;
            ((AlgebraResultIterative)result).Iterations = iters;
            if (iters > Maxiters - 2)
            {
                result.errorType = ErrorType.convergenceStagnation;
                result.Message = " Решение СЛАУ не получено";
                ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(Rho);
            }
        }
        protected bool CalcX(ref double[] X, double Al)
        {
            double ck = 0;
            double maxX = 0;
            double error;
            for (int i = 0; i < N; i++)
            {
                X[i] += Al * P[i];
                if (ck < Math.Abs(Al * P[i]))
                    ck = Math.Abs(Al * P[i]);
                if (maxX < Math.Abs(X[i]))
                    maxX = Math.Abs(X[i]);
            }
            error = ck / (maxX + MEM.Error10);
            ((AlgebraResultIterative)result).ratioError = error;
            ((AlgebraResultIterative)result).normA = maxX;
            if (maxX > 0)
            {
                if (error < EPS)
                    return true;
            }
            else
               if (ck < EPS)
                return true;
            return false;
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new SparseAlgebraCG(this.N);
        }
        public static void Test1()
        {
            double[] m0 = { 1, -1 };
            double[] m1 = { -1, 1 };
            double[][] m = new double[2][];
            m[0] = m0;
            m[1] = m1;
            double[] B = { 0, 0 };
            double[] BC = { 0, 4 };

            uint[] ad = { 0, 0 };
            uint[] adb = { 0, 0 };
            uint N = 500;
            uint NE = N - 1;
            BC[1] = NE;
            double[] X = new double[N];
            SparseAlgebraCG algebra = new SparseAlgebraCG(N);

            for (uint i = 0; i < NE; i++)
            {
                ad[0] = i; ad[1] = i + 1;
                algebra.AddToMatrix(m, ad);
                algebra.AddToRight(B, ad);
                // algebra.Print();
            }

            adb[0] = 0; adb[1] = NE;
            uint[] adb0 = { 0 };
            uint[] adb1 = { NE };
            double[] BC0 = { 0 };
            double[] BC1 = { NE };
            algebra.BoundConditions(BC0, adb0);
            // algebra.Print();
            algebra.BoundConditions(BC1, adb1);
            //algebra.Print();
            algebra.Solve(ref X);
            for (int j = 0; j < N; j++)
                Console.Write(" " + X[j].ToString("F4"));
            Console.Read();
        }
        //public static void Main()
        //{
        //    Test1();
        //}
    }
}
