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
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    /// <summary>
    /// ОО: Класс решатель САУ методом GMRES
    /// с хранением матрицы в формате CRS
    /// и поддержкой интерфейса алгебры для работы с КЭ задачами
    /// </summary>
    public class SparseAlgebraGMRES_P : SparseAlgebra
    {
        /// <summary>
        /// Максимальное количество итераций
        /// </summary>
        protected uint MaxIters;
        
        #region Рабочии переменные
        protected double[][] H = null;
        protected double[][] W = null;
        protected double[][] V = null;
        protected double[] R = null;
        protected double[] BetaE1 = null;
        //protected double[] Tmp = null;
        protected double[] XN = null;
        protected double[] RN = null;
        protected uint M;
        #endregion
        public SparseAlgebraGMRES_P(uint FN, uint M = 10, bool isPrecond = false, uint MaxIters = 1000) : base(FN, isPrecond)
        {
            this.M = M;
            name = "Параллельный метод GMRES в формате CRS";
            SetAlgebra(result, FN);
            result.Name = name;
            this.MaxIters = MaxIters;
        }
        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                // Выделение памяти
                int Mk = (int)M;
                MEM.Alloc2D(Mk + 1, Mk + 1, ref H);
                MEM.Alloc2D(Mk + 1, (int)FN, ref V);
                MEM.Alloc2D(Mk, (int)FN, ref W);
                MEM.Alloc(FN, ref XN);
                MEM.Alloc(FN, ref RN);
                MEM.Alloc(FN, ref R);
                // ошибка 
                MEM.Alloc(Mk + 1, ref BetaE1);
                //MEM.Alloc(Mk + 1, ref Tmp);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }
        #region Старая реализация
        ///// <summary>
        ///// Решение СЛУ GMRES методом
        ///// </summary>
        //protected void solve1(ref double[] X)
        //        {
        //            double beta = 0;
        //            double Value;
        //            // Вычисление начальной ошибки
        //            double[] _X = X;
        //            // 1. Вычисляем остаток r = b - Ax
        //            Calk_R_Norm(ref R, ref beta, _X);
        //            double beta0 = beta;
        //            double resCheck = 0;
        //            int iters = 0;

        //            uint MP;
        //            // цикл по решению
        //            for (iters = 0; iters < MaxIters; iters++)
        //            {
        //                MP = M;
        //                Parallel.ForEach(OrdPart, (range, loopState) =>
        //                {
        //                    for (int l = range.Item1; l < range.Item2; l++)
        //                    {
        //                        // 4. обнуляем вводим V и W
        //                        for (uint k = 0; k < M; k++)
        //                        {
        //                            V[k][l] = 0;
        //                            W[k][l] = 0;
        //                        }
        //                        // нормировка вектора ошибки
        //                        V[0][l] = R[l] / beta;
        //                    }
        //                });
        //                //5.  инициализируем матрицу H
        //                for (uint i = 0; i < M + 1; i++)
        //                    for (uint k = 0; k < M; k++)
        //                        H[i][k] = 0;
        //                //6. рассчет Hij
        //                // основной внешний цикл
        //                for (uint j = 0; j < M; j++)
        //                {
        //                    // 6.1 рассчитываем Wj
        //                    // получение вектора столбца направления
        //                    // Вычисляем R = M^-1 * (B - A*X)
        //                    if (isPrecond == true)
        //                    {
        //                        Parallel.ForEach(OrdPart, (range, loopState) =>
        //                        {
        //                            for (int ii = range.Item1; ii < range.Item2; ii++)
        //                            {
        //                                int i = (int)ii;
        //                                XN[i] = 0;
        //                                for (int idx = 0; idx < Matrix[i].Count; idx++)
        //                                    XN[i] += Matrix[i].Row[idx].Value * V[j][Matrix[i].Row[idx].IndexColumn];
        //                            }
        //                        });
        //                        CalkErrorPrecond(XN, ref RN);
        //                        // LOG.Print("RN", RN);
        //                        for (int i = 0; i < FN; i++)
        //                            W[j][i] = RN[i];
        //                    }
        //                    else
        //                    {
        //                        Parallel.ForEach(OrdPart, (range, loopState) =>
        //                        {
        //                            for (int ii = range.Item1; ii < range.Item2; ii++)
        //                            {
        //                                int i = (int)ii;
        //                                W[j][i] = 0;
        //                                for (int idx = 0; idx < Matrix[i].Count; idx++)
        //                                    W[j][i] += Matrix[i].Row[idx].Value * V[j][Matrix[i].Row[idx].IndexColumn];
        //                            }
        //                        });
        //                    }
        //                    // W = M^-1 * W
        //                    // 6.2 Цикл
        //                    // цикл по подсистемам
        //                    for (uint i = 0; i < j + 1; i++)
        //                    {
        //                        // проекция вектора ошибки Крылова на вектор сопряженной ошибки
        //                        Value = 0;
        //                        for (uint k = 0; k < FN; k++)
        //                            Value += W[j][k] * V[i][k];
        //                        // Коэффициент ортогонализации по Шмидту
        //                        H[i][j] = Value;
        //                        Parallel.ForEach(OrdPart, (range, loopState) =>
        //                        {
        //                            for (int k = range.Item1; k < range.Item2; k++)
        //                            {
        //                                W[j][k] -= Value * V[i][k];
        //                            }
        //                        });
        //                        // ортогонализация по Шмидту
        //                        //for (uint k = 0; k < FN; k++)
        //                        //    W[j][k] -= Value * V[i][k];
        //                    }
        //                    // 6.3
        //                    // вычисление нормы W(j)
        //                    double h = 0;
        //                    for (uint k = 0; k < FN; k++)
        //                        h += W[j][k] * W[j][k];
        //                    h = Math.Sqrt(h);
        //                    // сохранение нормы остаточной ошибки
        //                    H[j + 1][j] = h;
        //                    //6.4
        //                    // условие выхода
        //                    if (h < EPS)
        //                    {
        //                        MP = j + 1; break;
        //                    }
        //                    // 6.5
        //                    // получение нового вектора направления
        //                    Parallel.ForEach(OrdPart, (range, loopState) =>
        //                    {
        //                        for (int ii = range.Item1; ii < range.Item2; ii++)
        //                        {
        //                            V[j + 1][ii] = W[j][ii] / h;
        //                        }
        //                    });
        //                }
        //                //7. y->min
        //                // подготовка правой части
        //                for (uint k = 0; k < MP + 1; k++)
        //                    BetaE1[k] = beta;
        //                // минимизация системы //beta*e1-H(i,j)Y //
        //                // Решение малой системы H* y = beta * e1,
        //                Calk_Hy_beta(H, ref BetaE1, MP);
        //                // поправляем решение задачи
        //                _X = X;
        //                Parallel.ForEach(OrdPart, (range, loopState) =>
        //                {
        //                    for (int ii = range.Item1; ii < range.Item2; ii++)
        //                    {
        //                        for (uint j = 0; j < MP; j++)
        //                            _X[ii] += V[j][ii] * BetaE1[j];
        //                    }
        //                });

        //                Parallel.ForEach(OrdPart, (range, loopState) =>
        //                {
        //                    for (int i = range.Item1; i < range.Item2; i++)
        //                    {
        //                        R[i] = 0;
        //                        for (int idx = 0; idx < Matrix[i].Count; idx++)
        //                            R[i] -= Matrix[i].Row[idx].Value * _X[Matrix[i].Row[idx].IndexColumn];
        //                        R[i] += Right[i];
        //                    }
        //                });

        //                X = _X;
        //                // Вычисляем R = M^-1 * (B - A*X)
        //                if (isPrecond == true)
        //                    CalkErrorPrecond(R, ref R);
        //                // Вычисление нормы вектора ошибки
        //                beta = 0;
        //                for (uint i = 0; i < FN; i++)
        //                    beta += R[i] * R[i];

        //                beta = Math.Sqrt(beta);
        //                if (beta < MEM.Error9 && beta0 < MEM.Error9)
        //                    break;
        //                resCheck = beta / beta0;
        //                // если невязка мала то решение найдено
        //                if (resCheck < MEM.Error9)
        //                    break;
        //                if (double.IsNaN(beta) == true)
        //                {
        //                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
        //                    result.errorType = ErrorType.divisionByZero;
        //                    result.Message = " Решение СЛАУ не получено, деление на 0";
        //                    break;
        //                }
        //                if (double.IsInfinity(beta) == true)
        //                {
        //                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
        //                    result.errorType = ErrorType.divisionByZero;
        //                    result.Message = " Решение СЛАУ не получено, переполнение связанное с расходимостью";
        //                    break;
        //                }
        //                if (iters > MaxIters-1)
        //                {
        //                    ((AlgebraResultIterative)result).Error_L2 = Math.Sqrt(beta);
        //                    result.errorType = ErrorType.convergenceStagnation;
        //                    result.Message = " Решение СЛАУ не получено, достигнуто ограничение по итерациям";
        //                    break;
        //                }
        //            }
        //            ((AlgebraResultIterative)result).Iterations = iters;
        //            ((AlgebraResultIterative)result).ratioError = resCheck;
        //            ((AlgebraResultIterative)result).Error_L2 = beta;
        //            ((AlgebraResultIterative)result).Error0_L2 = beta0;
        //            result.Message = " M = " + M.ToString();
        //        }
        #endregion
        /// <summary>
        /// Решение СЛУ GMRES методом
        /// </summary>
        protected override void solve(ref double[] X)
        {
            try
            {
                OrderablePartitioner<Tuple<int, int>> OrdPart = Partitioner.Create(0, (int)FN);
                EPS = MEM.Error6;
                double beta = 0;
                double Value;
                // Вычисление начальной ошибки
                double[] _X = X;
                // 1. Вычисляем остаток r = b - Ax
                Parallel.ForEach(OrdPart, (range, loopState) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        R[i] = 0;
                        for (int idx = 0; idx < Matrix[i].Count; idx++)
                            R[i] -= Matrix[i].Row[idx].Value * _X[Matrix[i].Row[idx].IndexColumn];
                        R[i] += Right[i];
                    }
                });
                // Вычисляем R = M^-1 * (B - A*X)
                if (isPrecond == true)
                    CalkErrorPrecond(R, ref R);
                // 2. Находим норму вектора ошибки 
                beta = 0;
                for (uint i = 0; i < FN; i++)
                    beta += R[i] * R[i];
                beta = Math.Sqrt(beta);
                double beta0 = beta;
                double resCheck = 0;
                int iters = 0;
                uint MP;
                // цикл по решению
                for (iters = 0; iters < MaxIters; iters++)
                {
                    MP = M;
                    Parallel.ForEach(OrdPart, (range, loopState) =>
                    {
                        for (int l = range.Item1; l < range.Item2; l++)
                        {
                            // 4. обнуляем вводим V и W
                            for (uint k = 0; k < M; k++)
                            {
                                V[k][l] = 0;
                                W[k][l] = 0;
                            }
                            // нормировка вектора ошибки
                            V[0][l] = R[l] / beta;
                        }
                    });
                    //5.  инициализируем матрицу H
                    for (uint i = 0; i < M + 1; i++)
                        for (uint k = 0; k < M; k++)
                            H[i][k] = 0;
                    //6. рассчет Hij основной внешний цикл
                    for (uint j = 0; j < M; j++)
                    {
                        // 6.1 рассчитываем Wj
                        // получение вектора столбца направления
                        // Вычисляем R = M^-1 * (B - A*X)
                        if (isPrecond == true)
                        {
                            Parallel.ForEach(OrdPart, (range, loopState) =>
                            {
                                for (int ii = range.Item1; ii < range.Item2; ii++)
                                {
                                    int i = (int)ii;
                                    XN[i] = 0;
                                    for (int idx = 0; idx < Matrix[i].Count; idx++)
                                        XN[i] += Matrix[i].Row[idx].Value * V[j][Matrix[i].Row[idx].IndexColumn];
                                }
                            });
                            CalkErrorPrecond(XN, ref RN);
                            Parallel.ForEach(OrdPart, (range, loopState) =>
                            {
                                for (int ii = range.Item1; ii < range.Item2; ii++)
                                    W[j][ii] = RN[ii];
                            });
                        }
                        else
                        {
                            Parallel.ForEach(OrdPart, (range, loopState) =>
                            {
                                for (int ii = range.Item1; ii < range.Item2; ii++)
                                {
                                    int i = (int)ii;
                                    W[j][i] = 0;
                                    for (int idx = 0; idx < Matrix[i].Count; idx++)
                                        W[j][i] += Matrix[i].Row[idx].Value * V[j][Matrix[i].Row[idx].IndexColumn];
                                }
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
                                Value += W[j][k] * V[i][k];
                            // Коэффициент ортогонализации по Шмидту
                            H[i][j] = Value;
                            Parallel.ForEach(OrdPart, (range, loopState) =>
                            {
                                for (int k = range.Item1; k < range.Item2; k++)
                                {
                                    W[j][k] -= Value * V[i][k];
                                }
                            });
                        }
                        // 6.3
                        // вычисление нормы W(j)
                        double h = 0;
                        for (uint k = 0; k < FN; k++)
                            h += W[j][k] * W[j][k];
                        h = Math.Sqrt(h);
                        // сохранение нормы остаточной ошибки
                        H[j + 1][j] = h;
                        //6.4
                        // условие выхода
                        if (h < EPS)
                        {
                            MP = j + 1; break;
                        }
                        // 6.5
                        // получение нового вектора направления
                        Parallel.ForEach(OrdPart, (range, loopState) =>
                        {
                            for (int ii = range.Item1; ii < range.Item2; ii++)
                                V[j + 1][ii] = W[j][ii] / h;
                        });
                    }
                    //7. y->min
                    // подготовка правой части
                    for (uint k = 0; k < MP + 1; k++)
                        BetaE1[k] = beta;

                    #region Нахождение BetaE1
                    Calk_Hy_beta(H, ref BetaE1, MP);
                    // Решение малой системы H * y = beta * e1, методом вращений Гивенса
                    // Синус и косинус поворота
                    //double Cos, Sin, Hij;
                    //// исключение элементов из нижней трехугольной матрицы
                    //for (int i = 0; i < MP; i++)
                    //{
                    //    double D = Math.Sqrt(H[i][i] * H[i][i] + H[i + 1][i] * H[i + 1][i]);
                    //    Cos = H[i][i] / D;
                    //    Sin = H[i + 1][i] / D;
                    //    for (int j = i; j < MP; j++)
                    //    {
                    //        Hij = H[i][j];
                    //        H[i][j]     = Cos * Hij + Sin * H[i + 1][j];
                    //        H[i + 1][j] = Cos * H[i + 1][j] - Sin * Hij;
                    //    }
                    //    BetaE1[i + 1] = -BetaE1[i] * Sin;
                    //    BetaE1[i] = BetaE1[i] * Cos;
                    //}
                    //// выполняем обратный ход
                    //double c;
                    //for (int i = (int)MP - 1; i > -1; i--)
                    //{
                    //    c = BetaE1[i];
                    //    for (int j = i + 1; j < MP; j++)
                    //        c = c - H[i][j] * BetaE1[j];
                    //    BetaE1[i] = c / H[i][i];
                    //}
                    #endregion
                    // поправляем решение задачи
                    _X = X;
                    Parallel.ForEach(OrdPart, (range, loopState) =>
                    {
                        for (int ii = range.Item1; ii < range.Item2; ii++)
                        {
                            for (uint j = 0; j < MP; j++)
                                _X[ii] += V[j][ii] * BetaE1[j];
                        }
                    });
                    Parallel.ForEach(OrdPart, (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            R[i] = 0;
                            for (int idx = 0; idx < Matrix[i].Count; idx++)
                                R[i] -= Matrix[i].Row[idx].Value * _X[Matrix[i].Row[idx].IndexColumn];
                            R[i] += Right[i];
                        }
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

                    if (beta < EPS && beta0 < EPS)
                        break;
                    resCheck = beta / beta0;
                    // если невязка мала то решение найдено
                    if (resCheck < EPS)
                        break;
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
                //Console.WriteLine("Стоп");
                result.Print();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());                
            }
        }

        /// <summary>
        // Решение малой системы — решаем систему H* y = beta * e1, методом поворотов
        // приводя H к верхней треугольной матрицы, а beta — норма невязки.
        // 4. Решаем малую систему H * y = beta * e1
        /// </summary>
        /// <param name="H">верхняя треугольная матрица</param>
        /// <param name="Beta1">норма остатка</param>
        /// <param name="MP"></param>
        public void Calk_Hy_beta(double[][] H, ref double[] Beta1, uint MP)
        {
            // Синус и косинус поворота
            double Cos, Sin, Hij;
            // исключение элементов из нижней трехугольной матрицы ;)
            // только одна кодиагональ
            for (int i = 0; i < MP; i++)
            {
                double D = Math.Sqrt(H[i][i] * H[i][i] + H[i + 1][i] * H[i + 1][i]);
                Cos = H[i][i] / D;
                Sin = H[i + 1][i] / D;
                for (int j = i; j < MP; j++)
                {
                    Hij = H[i][j];
                    H[i][j] = Cos * Hij + Sin * H[i + 1][j];
                    H[i + 1][j] = Cos * H[i + 1][j] - Sin * Hij;
                }
                BetaE1[i + 1] = -BetaE1[i] * Sin;
                BetaE1[i] = BetaE1[i] * Cos;
            }
            // выполняем обратный ход
            double c;
            for (int i = (int)MP - 1; i > -1; i--)
            {
                c = BetaE1[i];
                for (int j = i + 1; j < MP; j++)
                    c = c - H[i][j] * BetaE1[j];
                BetaE1[i] = c / H[i][i];
            }
        }
        /// <summary>
        /// Вычисление ошибки, с учетом предобуславливания и ее нормы
        /// </summary>
        /// <param name="Res"></param>
        /// <param name="beta"></param>
        /// <param name="_X"></param>
        //public void Calk_R_Norm(ref double[] Res, ref double beta, double[] _X)
        //{
        //    double[] R = Res;
        //    // 1. Вычисляем остаток r = b - Ax
        //    Parallel.ForEach(OrdPart, (range, loopState) =>
        //    {
        //        for (int i = range.Item1; i < range.Item2; i++)
        //        {
        //            R[i] = 0;
        //            for (int idx = 0; idx < Matrix[i].Count; idx++)
        //                R[i] -= Matrix[i].Row[idx].Value * _X[Matrix[i].Row[idx].IndexColumn];
        //            R[i] += Right[i];
        //        }
        //    });
        //    //  LOG.Print("R", R);
        //    // Вычисляем R = M^-1 * (B - A*X)
        //    if (isPrecond == true)
        //        CalkErrorPrecond(R, ref R);
        //    // 2. Нормируем остаток вектора ошибки
        //    beta = Norm(R);
        //    Res = R;
        //}
        #region Методы для вычисления матрицы предобуславливания
        /// <summary>
        /// Декомпозиция матрицы на A => ILU
        /// </summary>
        public override void DecompositionPrecondMatrix()
        {
            //int i = 0, ii, j, e, row_i, row_i0, j0;
            try
            {
                OrderablePartitioner<Tuple<int, int>> OrdPart = Partitioner.Create(0, (int)FN);
                // копируем матрицу Matrix в ILU
                for (int i = 0; i < FN; i++)
                    ILU.Add(new SparseRow(Matrix[i]));
                if (FN < 1000)
                {

                    for (int i = 0; i < FN; i++)
                    {
                        // получаем строку ILU
                        SparseRow a = ILU[i];
                        // получаем индекс диагонального элемента матрицы
                        // копируем все элементы строки от диагонали ко конца строки
                        // в столбец
                        for (int j = diagonalRow[i]; j < a.Count; j++)
                        {
                            SparseColIndex b = MatrixColIndex[a.Row[j].IndexColumn];
                            double sum = MultMatrix(a, b, i, i);
                            ILU[i].Row[j].Value = Matrix[i].Row[j].Value - sum;
                        }
                        //  индексы i столбца
                        SparseColIndex ci = MatrixColIndex[i];
                        int ii = diagonalCol[i];
                        int row_i0 = ci[ii].IndexColumn;
                        int j0 = ci[ii].j;
                        double aii = 1 / ILU[row_i0].Row[j0].Value;
                        for (int e = diagonalCol[i] + 1; e < ci.Count; e++)
                        {
                            int row_i = ci[e].IndexColumn;
                            int j = ci[e].j;
                            SparseRow aj = ILU[row_i];
                            double sum = MultMatrix(aj, ci, i, i);
                            ILU[row_i].Row[j].Value = aii * (Matrix[row_i].Row[j].Value - sum);
                        }
                    }
                }
                else
                {
                    Parallel.ForEach(OrdPart, (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            // получаем строку ILU
                            SparseRow a = ILU[i];
                            // получаем индекс диагонального элемента матрицы
                            // копируем все элементы строки от диагонали ко конца строки
                            // в столбец
                            for (int j = diagonalRow[i]; j < a.Count; j++)
                            {
                                SparseColIndex b = MatrixColIndex[a.Row[j].IndexColumn];
                                double sum = MultMatrix(a, b, i, i);
                                ILU[i].Row[j].Value = Matrix[i].Row[j].Value - sum;
                            }
                            //  индексы i столбца
                            SparseColIndex ci = MatrixColIndex[i];
                            int ii = diagonalCol[i];
                            int row_i0 = ci[ii].IndexColumn;
                            int j0 = ci[ii].j;
                            double aii = 1 / ILU[row_i0].Row[j0].Value;
                            for (int e = diagonalCol[i] + 1; e < ci.Count; e++)
                            {
                                int row_i = ci[e].IndexColumn;
                                int j = ci[e].j;
                                SparseRow aj = ILU[row_i];
                                double sum = MultMatrix(aj, ci, i, i);
                                ILU[row_i].Row[j].Value = aii * (Matrix[row_i].Row[j].Value - sum);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        double Bar = 1000000;
        //double bmaxM = 1;
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization()
        {
            List<uint> BCAdress = new List<uint>();
            List<double> BCValue = new List<double>();
            for (int i = 0; i < FN; i++)
            {
                if (Math.Abs(Matrix[i].MaxRow()) > Bar)
                {
                    for (int j = 0; j < Matrix[i].Row.Count; j++)
                        if (Matrix[i].Row[j].IndexColumn == i)
                        {
                            double bValue = Right[i] / Matrix[i].Row[j].Value;
                            BCValue.Add(bValue);
                            BCAdress.Add((uint)i);
                            break;
                        }
                }
            }
            if (BCAdress.Count > 0)
                BoundConditions(BCValue.ToArray(), BCAdress.ToArray());
            base.SystemNormalization();
        }
        /// <summary>
        /// Установка граничных условий
        /// </summary>
        /// <param name="Value"></param>
        //protected override void SetBCondition(ref double[] Value)
        //{
        //    // максимальное значение в правой части
        //    double maxR = Value.Max(x => Math.Abs(x));
        //    if (maxR > Bar)
        //    {
        //        for (int i = 0; i < N; i++)
        //        {
        //            if (Math.Abs(Value[i]) > Bar)
        //                Value[i] *= bmaxM;
        //            if (Matrix[i].Count == 1)
        //            {
        //                Value[i] = Right[i] / Matrix[i].Row[0].Value;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        base.SetBCondition(ref Value);
        //    }
        //}
        #endregion

        
        /// <summary>
        /// клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new SparseAlgebraGMRES_P(this.N, this.M, this.isPrecond);
        }
    }
}
