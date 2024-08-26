#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                              Потапов И.И.
//                        - () Copyright 2014 -
//                          ALL RIGHT RESERVED
//                              07.10.14
//---------------------------------------------------------------------------
//     Реализация для библиотеки решающей задачу внешнего обтекания тел 
//                     методом граничных элементов
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
    using CommonLib;
    using System;
    /// <summary>
    /// ОО: Класс для решения задачи обтекания плоских тел МГЭ
    /// </summary>
    [Serializable]
    public class FlatBETask : APeriodicBEMTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public FlatBETask(BEMStreamParams p, IMesh mesh, IAlgebra algebra) : base(p, mesh, algebra)
        {
            C = new uint[Count / 2];
            for (uint i = 0; i < C.Length; i++)
                C[i] = i;
            line = new double[C.Length];
            // Вычисление бета вектора
            Betta = CalkBetta(NS);
        }
        /// <summary>
        /// вычисление скоростей на контуре для безвихривого потока
        /// </summary>
        /// <param name="result">касательная скорость обтекания контура</param>
        public override void SolveTask(ref double[] localV)
        {
            // вычисление матрицы системы
            Matrix = CalkMatrix(Count / 2, Count, Betta);
            // Формирование правых частей
            double[] B1 = new double[Count / 2];
            for (int i = 0; i < B1.Length; i++)
                B1[i] = 2 * Math.PI * fy[i];
            // объекты СЛАУ
            algebra.Clear();
            // формируем систему
            for (uint i = 0; i < B1.Length; i++)
            {
                for (int j = 0; j < B1.Length; j++)
                    line[j] = Matrix[i, j] - Matrix[i, B1.Length + j];
                algebra.AddStringSystem(line, C, i, B1[i]);
            }
            // Вектора расширенного решения
            double[] VSIM = new double[B1.Length];
            // решаем системы для определения соростей V1 V2 V3
            algebra.Solve(ref VSIM);

            V = new double[Count];
            int k = 0;
            for (int i = 0; i < B1.Length; i++)
                V[k++] = VSIM[i];
            for (int i = 0; i < B1.Length; i++)
                V[k++] = -VSIM[i];
            if (localV == null)
                localV = new double[Count];
            for (int i = 0; i < Count; i++)
                localV[i] = V[i];
        }
        /// <summary>
        /// Вычисление функции тока в координатах rm, zm
        /// </summary>
        /// <param name="x">осевая кордината</param>
        /// <param name="y">радиальная кордината</param>
        /// <returns>функция тока</returns>
        public override double CalkPhi(double x, double y)
        {
            double phi = 0;
            for (int i = 0; i < V.Length; i++)
                phi += V[i] * detJ[i] * FGreen(i, x, y);
            phi /= 2 * Math.PI * Count;
            return y * Vx0 + phi;
        }
        /// <summary>
        /// Вычисление расширенной матрицы системы для решения 
        ///            внешней осесимметричной задачи
        /// </summary>
        /// <param name="N">Порядок системы</param>
        /// <param name="Betta">Коэффициенты матрицы Betta</param>
        /// <returns>H</returns>
        public override double[,] CalkMatrix(int Ni, int N, double[] Betta)
        {
            double[,] A = new double[Ni, N];
            double dx, dy;
            double Gij, r2;
            for (int i = 0; i < Ni; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    // Вычисление проекций по Х
                    dx = fx[i] - fx[j];
                    // Вычисление проекций по Y
                    dy = fy[i] - fy[j];
                    // Вычисление квадрата расстояния между узлами
                    r2 = dx * dx + dy * dy;
                    if (i == j)
                        Gij = Math.Log(detJ[i] / Math.PI);
                    else
                        Gij = 0.5 * Math.Log(r2);
                    int idx = Math.Abs(i - j);
                    A[i, j] = -detJ[j] / N * (Betta[idx] + Gij);
                }
            }
            return A;
        }
        /// <summary>
        /// Расчет значения функции Грина от источника с узлом i в точке xj, yj
        /// </summary>
        /// <param name="i">узел контура</param>
        /// <param name="xj">кордината x точки наблюдения</param>
        /// <param name="yj">кордината y точки наблюдения</param>
        /// <returns>значение функции Грина</returns>
        protected double FGreen(int i, double xj, double yj)
        {
            // результат
            double Gij = 0;
            // Вычисление проекций по Х
            double dx = fx[i] - xj;
            // Вычисление проекций по Y
            double dy = fy[i] - yj;
            // Вычисление квадрата расстояния между узлами
            double r2 = dx * dx + dy * dy;
            if (r2 < 0.0000001)
                Gij = Math.Log(detJ[i] / Math.PI);
            else
                Gij = 0.5 * Math.Log(r2);
            return Gij;
        }
    }
}
