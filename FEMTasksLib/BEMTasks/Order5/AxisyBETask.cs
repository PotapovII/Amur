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
    /// ОО: Класс для решения задачи обтекания тел вращения МГЭ
    /// </summary>
    [Serializable]
    public class AxisyBETask : APeriodicBEMTask
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public AxisyBETask(BEMStreamParams p, IMesh mesh, IAlgebra algebra) : base(p, mesh, algebra)
        {
            if (Count % 2 == 1) Count++;
            NS = Count / 2;
            C = new uint[NS];
            for (uint i = 0; i < C.Length; i++) C[i] = i;
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
            Matrix = CalkMatrix(NS, NS, Betta);
            // Формирование правых частей
            double[] B1 = new double[NS];
            for (int i = 0; i < B1.Length; i++)
                B1[i] = 2 * Math.PI * fy[i] * fy[i];  //2 * Math.PI * fy[i];
            algebra.Clear();
            // формируем систему
            for (uint i = 0; i < B1.Length; i++)
            {
                for (int j = 0; j < B1.Length; j++)
                    line[j] = Matrix[i, j];
                algebra.AddStringSystem(line, C, i, B1[i]);
            }
            // Вектора расширенного решения
            double[] VSIM = new double[NS];
            // решаем системы для определения соростей V1 V2 V3
            algebra.Solve(ref VSIM);
            ////////////////////////////////////////
            // отбрасываем слот расширения
            int k = 0;
            for (int i = 0; i < NS; i++)
                V1[k++] = VSIM[i];
            for (int i = 0; i < NS; i++)
                V1[k++] = -VSIM[i];
            // скорость во всей области
            V = new double[Count];
            if (localV == null)
                localV = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                V[i] = V1[i];// *Vx0;
                localV[i] = V[i];
            }
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
            for (int i = 0; i < NS; i++)
                phi += 0.5 * V[i] * detJ[i] * CalkGreen(fy[i], fx[i], y, x);
            phi /= 2 * Math.PI * Count;
            return 0.5 * y * y + phi;
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
            for (int i = 0; i < Ni; i++)
            {
                if (Math.Abs(fy[i]) < 0.0000001)
                {
                    A[i, i] = 1;
                }
                else
                {
                    for (int j = 0; j < N; j++)
                    {
                        int ij = Math.Abs(i - j);
                        A[i, j] = -detJ[j] * (fy[i] * Betta[ij] + 0.5 * CalkGreen(i, j)) / N;
                    }
                }
            }
            return A;
        }

        #region Методы для для получения дискретного аналога гранично-элементной задачи
        /// <summary>
        /// Расчет значения функции Грина для двух узлов контура
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        protected double CalkGreen(int i, int j)
        {
            double G = 0;
            if (i == j)
            {
                G = 2 * fy[i] * (2.0 + Math.Log(0.5 * detJ[i] / (8.0 * Math.PI * fy[i])));
                return G;
            }
            else
            {
                G = CalkGreen(fy[i], fx[i], fy[j], fx[j]);
                return G;
            }
        }
        /// <summary>
        /// Расчет значения функции Грина от источника с координатами ri,zi в точке rj,zj
        /// </summary>
        /// <param name="ri">радиальная кордината источника</param>
        /// <param name="zi">осевая кордината источника</param>
        /// <param name="rj">радиальная кордината точки наблюдения</param>
        /// <param name="zj">осевая кордината точки наблюдения</param>
        /// <returns>значение функции Грина</returns>
        protected double CalkGreen(double ri, double zi, double rj, double zj)
        {
            // результат
            double G = 0;
            // инткрименты координат для вычисления элеептического интеграла по Петрову
            double dr = ri - rj;
            double drp = ri + rj;
            double dz = zi - zj;
            double r1 = Math.Sqrt(dr * dr + dz * dz);
            double r2 = Math.Sqrt(drp * drp + dz * dz);
            double kk1 = 4 * r1 * r2 / ((r2 + r1) * (r2 + r1));
            double kk = 1 - kk1;
            int n = 1;
            double sum = 0;
            double delta = 100;
            if (kk < 0.5)
            {
                while (delta > 0.00000001)
                {
                    delta = Calk_A(n) * Math.Pow(kk, n);
                    sum += delta;
                    n++;
                }
                sum = sum * Math.PI / 2.0;
            }
            else
            {
                double sum1 = 1;
                double sum2 = -1;
                while (delta > 0.00000001)
                {
                    // проверить почему не Math.Pow(kk1, 2*n);
                    delta = Calk_B(n) * Math.Pow(kk1, n);
                    sum1 -= delta;
                    sum2 += Calk_C(n) * Math.Pow(kk1, n);
                    n++;
                }
                sum = 0.5 * sum1 * Math.Log(16.0 / kk1) + sum2;
            }
            G = -2 * sum * (r1 + r2);
            return G;
        }
        /// <summary>
        /// Коэффициент A для вычисления ядра грина в осесимметричной задачи
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected double Calk_A(int m)
        {
            double a = 0.5;
            if (m > 1)
            {
                for (int n = 1; n < m; n++)
                    a *= ((2.0 * n - 1.0) * (2.0 * n + 1.0) / (2.0 * n * (2.0 * n + 2.0)));
            }
            return a;
        }
        /// <summary>
        /// Коэффициент В для вычисления ядра грина 
        /// </summary>
        protected double Calk_B(int n)
        {
            return Calk_A(n) / (2.0 * n);
        }
        /// <summary>
        /// Коэффициент С для вычисления ядра грина
        /// </summary>
        protected double Calk_C(int n)
        {
            return Calk_A(n) / (2.0 * n) * (2.0 * Lambda(n) - 1.0 / (2.0 * n - 1.0));
        }
        /// <summary>
        /// Коэффициент Lambda для вычисления ядра грина
        /// </summary>
        protected double Lambda(int m)
        {
            double s = 0;
            for (int n = 1; n <= m; n++)
                s += 1.0 / ((2.0 * n - 1.0) * 2.0 * n);
            return s;
        }
        #endregion
    }
}
