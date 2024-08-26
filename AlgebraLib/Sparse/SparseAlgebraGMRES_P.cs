//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                    разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                         класс HPackSPreGMRES
//                  для решения САУ GMRES - методом с
//              симметричным предобуславливанием системы
//              для матрицы плотной упаковки по строкам
//                           Data: 14.11.2002
//---------------------------------------------------------------------------
//                            Потапов И.И.
//          HConjuageGradient (C++) ==> SparseAlgebraCG (C#)
//                              19.04.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;
    using System.Threading.Tasks;

    /// <summary>
    /// ОО: Класс решатель САУ методом сопряженных градиентов
    /// с хранением матрицы в формате CRS
    /// и поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public class SparseAlgebraGMRES_P : SparseAlgebra
    {
        #region Рабочии переменные
        protected double[,] H = null;
        protected double[,] W = null;
        protected double[,] V = null;
        protected double[] R = null;
        protected double[] BetaE1 = null;
        protected double[] Tmp = null;
        protected uint M;
        #endregion
        public SparseAlgebraGMRES_P(uint FN, uint M = 10, bool isPrecond = false) : base(FN, isPrecond)
        {
            this.M = M;
            name = "Метод GMRES для матрицы в формате CRS";
            SetAlgebra(result, FN);
            result.Name = name;
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                // Выделение памяти
                H = new double[M + 1, M + 1];
                W = new double[M, FN];
                V = new double[M + 1, FN];
                // ошибка 
                R = new double[FN];
                BetaE1 = new double[M + 1];
                Tmp = new double[M + 1];
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        ///// <summary>
        ///// Установка граничных условий
        ///// </summary>
        ///// <param name="Value"></param>
        //protected void SetBCondition(ref double[] Value)
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        if (Matrix[i].Count == 1)
        //            Value[i] = Right[i] / Matrix[i].Row[0].Elem;
        //    }
        //}
        /// <summary>
        /// Решение СЛУ GMRES методом
        /// </summary>
        protected override void solve(ref double[] X)
        {
            double[] XX = new double[FN];
            double[] YY = new double[FN];
            double beta;
            double Value;
            // Вычисление начальной ошибки


            //for (int i = 0; i < FN; i++)
            //{
            //    R[i] = 0;
            //    for (int idx = 0; idx < Matrix[i].Count; idx++)
            //        R[i] -= Matrix[i].Row[idx].Elem * X[Matrix[i].Row[idx].Knot];
            //    R[i] += Right[i];
            //}

            double[] _X = X;
            Parallel.For(0, FN, ii =>
            {
                int i = (int)ii;
                R[i] = 0;
                for (int idx = 0; idx < Matrix[i].Count; idx++)
                    R[i] -= Matrix[i].Row[idx].Elem * _X[Matrix[i].Row[idx].Knot];
                R[i] += Right[i];
            });
            //  LOG.Print("R", R);
            // Вычисляем R = M^-1 * (B - A*X)
            if (isPrecond == true)
                CalkErrorPrecond(R, ref R);
            //  LOG.Print("R", R);
            // Вычисление нормы вектора ошибки
            beta = 0;
            for (uint i = 0; i < FN; i++)
                beta += R[i] * R[i];
            beta = Math.Sqrt(beta);
            double beta0 = beta;
            double resCheck = 0;

            int iters = 0;
            uint Maxiters = 2 * FN;
            uint MP;
            // цикл по решению
            for (iters = 0; iters < Maxiters; iters++)
            {
                MP = M;
                // 4. вводим V и W
                for (uint k = 0; k < M; k++)
                    for (uint l = 0; l < FN; l++)
                    {
                        V[k, l] = 0;
                        W[k, l] = 0;
                    }
                // нормировка вектора ошибки
                for (uint k = 0; k < FN; k++)
                    V[0, k] = R[k] / beta;
                //5.  инициализируем матрицу H
                for (uint i = 0; i < M + 1; i++)
                    for (uint k = 0; k < M; k++)
                        H[i, k] = 0;
                //6. рассчет Hij
                // основной внешний цикл
                for (uint j = 0; j < M; j++)
                {
                    // 6.1 рассчитываем Wj
                    // получение вектора столбца направления
                    // Вычисляем R = M^-1 * (B - A*X)
                    if (isPrecond == true)
                    {

                        //for (int i = 0; i < FN; i++)
                        //{
                        //    XX[i] = 0;
                        //    for (int idx = 0; idx < Matrix[i].Count; idx++)
                        //        XX[i] += Matrix[i].Row[idx].Elem * V[j, Matrix[i].Row[idx].Knot];
                        //}
                        Parallel.For(0, FN, ii =>
                        {
                            int i = (int)ii;
                            XX[i] = 0;
                            for (int idx = 0; idx < Matrix[i].Count; idx++)
                                XX[i] += Matrix[i].Row[idx].Elem * V[j, Matrix[i].Row[idx].Knot];
                        });

                        CalkErrorPrecond(XX, ref YY);
                        // LOG.Print("YY", YY);

                        for (int i = 0; i < FN; i++)
                            W[j, i] = YY[i];
                    }
                    else
                    {
                        //for (int i = 0; i < FN; i++)
                        //{
                        //    W[j, i] = 0;
                        //    for (int idx = 0; idx < Matrix[i].Count; idx++)
                        //        W[j, i] += Matrix[i].Row[idx].Elem * V[j, Matrix[i].Row[idx].Knot];
                        //}
                        Parallel.For(0, FN, ii =>
                        {
                            int i = (int)ii;
                            W[j, i] = 0;
                            for (int idx = 0; idx < Matrix[i].Count; idx++)
                                W[j, i] += Matrix[i].Row[idx].Elem * V[j, Matrix[i].Row[idx].Knot];
                        });
                    }
                    // W = M^-1 * W
                    // 6.2 Цикл
                    // цикл по подсистемам
                    for (uint i = 0; i < j + 1; i++)
                    {
                        // проекция вектора ошибки Крылова на вектор сопряженной ошибки
                        Value = 0;
                        for (uint k = 0; k < FN; k++)
                            Value += W[j, k] * V[i, k];
                        // Коэффициент ортогонализации по Шмидту
                        H[i, j] = Value;
                        // ортогонализация по Шмидту
                        for (uint k = 0; k < FN; k++)
                            W[j, k] -= Value * V[i, k];
                    }
                    // 6.3
                    // вычисление нормы W(j)
                    double h = 0;
                    for (uint k = 0; k < FN; k++)
                        h += W[j, k] * W[j, k];
                    // сохранение нормы остаточной ошибки
                    h = Math.Sqrt(h);
                    H[j + 1, j] = h;
                    //6.4
                    // условие выхода
                    if (h < EPS)
                    {
                        MP = j + 1; break;
                    }
                    // 6.5
                    // получение нового вектора направления
                    //for (uint k = 0; k < FN; k++)
                    //    V[j + 1, k] = W[j, k] / h;
                    Parallel.For(0, FN, ii =>
                    {
                        int k = (int)ii;
                        V[j + 1, k] = W[j, k] / h;
                    });
                }
                //7. y->min

                // подготовка правой части
                for (uint k = 0; k < MP + 1; k++)
                    BetaE1[k] = beta;
                // минимизация системы //beta*e1-H(i,j)Y //
                {
                    // Синус и косинус поворота
                    double C, S;
                    // исключение элементов из нижней трехугольной матрицы ;)
                    // только одна кодиагональ
                    for (int i = 0; i < MP; i++)
                    {
                        double D = Math.Sqrt(H[i, i] * H[i, i] + H[i + 1, i] * H[i + 1, i]);
                        C = H[i, i] / D;
                        S = H[i + 1, i] / D;
                        for (int j = i; j < MP; j++)
                            Tmp[j] = C * H[i, j] + S * H[i + 1, j];
                        for (int j = i; j < MP; j++)
                            H[i + 1, j] = C * H[i + 1, j] - S * H[i, j];
                        for (int j = i; j < MP; j++)
                            H[i, j] = Tmp[j];
                        BetaE1[i + 1] = -BetaE1[i] * S;
                        BetaE1[i] = BetaE1[i] * C;
                    }
                    // выполняем обратный ход
                    double c;
                    for (int i = (int)MP - 1; i > -1; i--)
                    {
                        c = BetaE1[i];
                        for (int j = i + 1; j < MP; j++)
                            c = c - H[i, j] * BetaE1[j];
                        BetaE1[i] = c / H[i, i];
                    }
                }
                // поправляем решение задачи
                _X = X;
                Parallel.For(0, FN, ii =>
                {
                    int k = (int)ii;
                    double sumValue = 0;
                    for (uint j = 0; j < MP; j++)
                        sumValue += V[j, k] * BetaE1[j];
                    // решение системы
                    _X[k] += sumValue;
                });

                //for (int i = 0; i < FN; i++)
                //{
                //    R[i] = 0;
                //    for (int idx = 0; idx < Matrix[i].Count; idx++)
                //        R[i] -= Matrix[i].Row[idx].Elem * X[Matrix[i].Row[idx].Knot];
                //    R[i] += Right[i];
                //}
                // Вычисление начальной ошибки
                Parallel.For(0, FN, ii =>
                {
                    int i = (int)ii;
                    R[i] = 0;
                    for (int idx = 0; idx < Matrix[i].Count; idx++)
                        R[i] -= Matrix[i].Row[idx].Elem * _X[Matrix[i].Row[idx].Knot];
                    R[i] += Right[i];
                });
                X = _X;

                // Вычисляем R = M^-1 * (B - A*X)
                if (isPrecond == true)
                    CalkErrorPrecond(R, ref R);
                // Вычисление нормы вектора ошибки
                beta = 0;
                for (uint i = 0; i < FN; i++)
                    beta += R[i] * R[i];
                beta = Math.Sqrt(beta);

                if (beta < MEM.Error9 && beta0 < MEM.Error9)
                    break;

                resCheck = beta / beta0;
                // если невязка мала то решение найдено
                if (resCheck < MEM.Error9)
                    break;
                if (double.IsNaN(beta) == true)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, деление на 0";
                    break;
                }
                if (double.IsInfinity(beta) == true || iters > 1000)
                {
                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
                    result.errorType = ErrorType.divisionByZero;
                    result.Message = " Решение СЛАУ не получено, переполнение связанное с расходимостью";
                    break;
                }
            }
            ((AlgebraResultIterative)result).Iterations = iters;
            ((AlgebraResultIterative)result).ratioError = resCheck;
            ((AlgebraResultIterative)result).Error_L2 = beta;
            ((AlgebraResultIterative)result).Error0_L2 = beta0;
            result.Message = " M = " + M.ToString();
            if (iters > Maxiters - 2)
            {
                result.errorType = ErrorType.convergenceStagnation;
                result.Message += " Решение СЛАУ не получено";
            }
        }
        /// <summary>
        /// клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new SparseAlgebraGMRES_P(this.N, this.M, this.isPrecond);
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
            uint N = 150;
            uint NE = N - 1;
            BC[1] = NE;
            double[] X = new double[N];
            SparseAlgebraGMRES_P algebra = new SparseAlgebraGMRES_P(N);

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
