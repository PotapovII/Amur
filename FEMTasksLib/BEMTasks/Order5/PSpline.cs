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
//                        - (C) Copyright 2012 -
//                          ALL RIGHT RESERVED
//                               21.06.12
//---------------------------------------------------------------------------
//     Реализация для библиотеки решающих задачу обтекания тел 
//                     метода граничных элементов
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using System;
    /// <summary>
    /// ОО: периодический сплайн Петрова А.Г. используется 
    /// для аппроксимации контура области в методах ГЭ
    /// </summary>
    [Serializable]
    public class PSpline
    {
        public string ErrMessage = "Все хорошо!";
        // коэффициенты сплайна
        double[] Coeff = null;
        // коэффициенты функции прототипа
        double[] f = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="f">порождающая функция</param>
        public PSpline(double[] f)
        {
            // леточная симметричная матрица
            //algebra = new AlgebraGauss();
            // вычисление коэффициентов сплайна
            //CalkCoeffSpline(f);
            CoefSpline(f);
        }
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public PSpline()
        {
        }
        /// <summary>
        /// Вычисления коэффициентов по методу прогонки
        /// </summary>
        /// <param name="_f"></param>
        /// <returns></returns>
        public double[] CoefSpline(double[] _f)
        {
            f = _f;
            Coeff = new double[f.Length];
            double[] F = new double[f.Length];

            for (int i = 1; i < F.Length - 1; i++)
                F[i] = 6 * (f[i + 1] - 2 * f[i] + f[i - 1]);

            F[0] = 6 * (f[1] - 2 * f[0] + f[F.Length - 1]);

            F[F.Length - 1] = 6 * (f[0] - 2 * f[F.Length - 1] + f[F.Length - 2]);

            double[] A = new double[F.Length];
            double[] B = new double[F.Length];
            double[] d = new double[F.Length];
            double[] v = new double[f.Length];

            double a = 2 + Math.Sqrt(3);

            int N = f.Length;

            A[0] = 0; B[0] = 0;

            for (int i = 1; i < A.Length; i++)
            {
                d[i] = A[i - 1] + 4;
                A[i] = -1 / d[i];
                B[i] = (F[i - 1] - B[i - 1]) / d[i];
            }

            for (int i = 0; i < v.Length - 1; i++)
                v[i] = Math.Pow(-1, i + 1) * (Math.Pow(a, i + 1 - N) + Math.Pow(a, -(i + 1))) / (1 - Math.Pow(a, -N));
            v[v.Length - 1] = 1;

            double[] u = new double[f.Length];
            u[N - 1] = 0;

            for (int i = N - 1; i > 0; i--)
                u[i - 1] = A[i] * u[i] + B[i];

            double dNdy = ((1 + Math.Pow(a, -N)) / (1 - Math.Pow(a, -N))) * ((F[N - 1] - u[0] - u[N - 2]) / (a - 1 / a));

            for (int i = 0; i < N; i++)
                Coeff[i] = u[i] + dNdy * v[i];
            return Coeff;

        }

        /// <summary>
        /// Вычисление массива значений в узлах : для циклической функции
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        public double[] SplineFunction()
        {
            double[] S = new double[Coeff.Length];
            double dx = 1.0 / (Coeff.Length - 1);
            int N = S.Length;
            for (int i = 0; i < S.Length; i++)
            {
                double x = 1;//  dx* i; // тождество ((
                double x2 = x * x;
                double q1 = 1 - x;
                double q2 = x;
                double q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
                double q4 = 1.0 / 6 * (x2 * x - x);
                if (i == 0)
                {
                    S[i] = f[0] + Coeff[N - 1] * q3 + Coeff[0] * q4;
                }
                else
                    S[i] = f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4;
            }
            return S;
        }

        /// <summary>
        /// Вычисление массива значений в узлах : для циклической функции
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        public double SplineValue(double value)
        {

            int N = Coeff.Length;
            double S = 0;

            double x = 1;
            int i = ((int)value);
            if (i < 0)
            {
                i = N + i;
                x = 1 + Math.Abs((int)value) + value;
            }
            else
            {
                if (value < 0)
                    x = Math.Abs((int)value) + value;
                else
                    x = value - Math.Abs((int)value);
            }
            double x2 = x * x;
            double q1 = 1 - x;
            double q2 = x;
            double q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
            double q4 = 1.0 / 6 * (x2 * x - x);
            if (i == 0)
            {
                S = f[N - 1] * q1 + f[0] * q2 + Coeff[N - 1] * q3 + Coeff[0] * q4;
            }
            else
                S = f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4;

            return S;
        }

        /// <summary>
        /// Вычисление массива значений в дробных узлах : для циклической функции
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        public double Value(double value)
        {
            int N = Coeff.Length;
            double xx = N + value;
            int i = ((int)xx);
            double x = xx - i;

            double x2 = x * x;
            double q1 = 1 - x;
            double q2 = x;
            double q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
            double q4 = 1.0 / 6 * (x2 * x - x);

            double S = f[i % N] * q1 + f[(i + 1) % N] * q2 + Coeff[i % N] * q3 + Coeff[(i + 1) % N] * q4;

            return S;
        }

        /// <summary>
        /// Получение призводной в дробных узлах : для циклической функции
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double DiffValue(double value)
        {
            int N = Coeff.Length;
            double xx = N + value;
            int i = ((int)xx);
            double x = xx - i;

            double x2 = x * x;
            double q1 = -1;
            double q2 = 1;
            double q3 = -1.0 / 6.0 * (3 * x2 - 6 * x + 2);
            double q4 = 1.0 / 6.0 * (3 * x2 - 1);

            double S = f[i % N] * q1 + f[(i + 1) % N] * q2 + Coeff[i % N] * q3 + Coeff[(i + 1) % N] * q4;
            return S;
        }





        public double[] BeSplineFunction()
        {
            double[] S = new double[2 * Coeff.Length];
            double dx = 1.0 / (Coeff.Length - 1);
            int N = Coeff.Length;
            int k = 0;
            double x, x2, q1, q2, q3, q4;
            for (int i = 0; i < Coeff.Length; i++)
            {
                // промежуточное значение
                x = 0.5; //  dx* i; // тождество ((
                x2 = x * x;
                q1 = 1 - x;
                q2 = x;
                q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
                q4 = 1.0 / 6 * (x2 * x - x);
                if (i == 0)
                    S[k++] = f[0] + Coeff[N - 1] * q3 + Coeff[0] * q4;
                else
                    S[k++] = f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4;
                // начальное значение
                x = 1;//  dx* i; // тождество ((
                x2 = x * x;
                q1 = 1 - x;
                q2 = x;
                q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
                q4 = 1.0 / 6 * (x2 * x - x);
                if (i == 0)
                    S[k++] = f[0] + Coeff[N - 1] * q3 + Coeff[0] * q4;
                else
                    S[k++] = f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4;
            }
            return S;
        }
        /// <summary>
        /// Вычисление производных массива значений в узлах : для циклической функции
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        public double[] BeDiffSplineFunction()
        {
            double[] S = new double[2 * Coeff.Length];
            double dx = 1.0 / (Coeff.Length + 1);
            int N = Coeff.Length;
            int k = 0;
            double x, x2, q1, q2, q3, q4;
            for (int i = 0; i < Coeff.Length; i++)
            {
                x = 0.5; // dx* i; // по Петрову
                x2 = x * x;
                q1 = -1;
                q2 = 1;
                q3 = -1.0 / 6.0 * (3 * x2 - 6 * x + 2);
                q4 = 1.0 / 6.0 * (3 * x2 - 1);
                if (i == 0)
                    S[k++] = N * (f[N - 1] * q1 + f[0] * q2 + Coeff[N - 1] * q3 + Coeff[0] * q4);
                else
                    S[k++] = N * (f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4);
                //
                x = 1; // dx* i; // по Петрову
                x2 = x * x;
                q1 = -1;
                q2 = 1;
                q3 = -1.0 / 6.0 * (3 * x2 - 6 * x + 2);
                q4 = 1.0 / 6.0 * (3 * x2 - 1);
                if (i == 0)
                    S[k++] = N * (f[N - 1] * q1 + f[0] * q2 + Coeff[N - 1] * q3 + Coeff[0] * q4);
                else
                    S[k++] = N * (f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4);
            }
            return S;
        }


        public double[] DiffSplineFunction()
        {
            double[] S = new double[Coeff.Length];
            double dx = 1.0 / (Coeff.Length + 1);
            int N = S.Length;
            for (int i = 0; i < S.Length; i++)
            {
                double x = 1; // dx* i; // по Петрову
                double x2 = x * x;
                double q1 = -1;
                double q2 = 1;
                double q3 = -1.0 / 6.0 * (3 * x2 - 6 * x + 2);
                double q4 = 1.0 / 6.0 * (3 * x2 - 1);

                if (i == 0)
                    S[0] = N * (f[N - 1] * q1 + f[0] * q2 + Coeff[N - 1] * q3 + Coeff[0] * q4);
                else
                    S[i] = N * (f[i - 1] * q1 + f[i] * q2 + Coeff[i - 1] * q3 + Coeff[i] * q4);

            }
            return S;
        }
        /// <summary>
        /// Дифференцирование кривой в узлах сплайна без насыщения
        /// </summary>
        /// <returns></returns>
        public double[] DiffSplineFunctionWhith()
        {
            double[] DSn = new double[f.Length];
            double[,] dij = new double[f.Length, f.Length];
            int Count = f.Length / 2;
            double A = 4 * Math.PI / f.Length;
            // определение матрицы
            for (int i = 0; i < f.Length; i++)
                for (int j = 0; j < f.Length; j++)
                {
                    double S = 0;
                    for (int k = 1; k <= Count; k++)
                        S += k * Math.Sin(2 * Math.PI * k * (j - i) / f.Length);
                    dij[i, j] = A * S;
                }

            for (int i = 0; i < f.Length; i++)
                for (int j = 0; j < f.Length; j++)
                    DSn[i] += dij[i, j] * f[j];
            return DSn;
        }

        /// <summary>
        /// Интегрирование кривой аппроксимируемой сплайном
        /// </summary>
        /// <returns></returns>
        public double[] IntegralSplineFunction()
        {
            double[] IntFun = new double[f.Length];
            // Расчет в узлах
            for (int i = 1; i < f.Length; i++)
            {
                double S = 0;
                for (int j = 1; j < i; j++)
                {
                    S += 0.5 * (f[j - 1] + f[j]) - 1 / 24.0 * (Coeff[j - 1] + Coeff[j]);
                }
                IntFun[i] = S / f.Length;
            }
            return IntFun;
        }
        /// <summary>
        /// Фильтрация
        /// </summary>
        /// <param name="F"></param>
        /// <returns></returns>
        public double[] FilterSpline(double[] F)
        {
            double[] filter = new double[F.Length];
            int NN = filter.Length - 1;
            for (int j = 1; j < NN; j++)
                filter[j] = (F[j - 1] + 4 * F[j] + F[j + 1]) / 6.0;
            filter[0] = (F[NN] + 4 * F[0] + F[1]) / 6.0;
            filter[NN] = (F[NN - 1] + 4 * F[NN] + F[0]) / 6.0;
            return filter;
        }
        /// <summary>
        /// Поиск индекса первого положительного числа массива
        /// </summary>
        /// <param name="a">массив</param>
        /// <returns>Индекс первого положительного числа массива</returns>
        static public int NumFirstPos(double[] a)
        {
            int i = 0;
            for (int k = 0; k < a.Length; k++)
                if (a[k] > 0) return k;
            return i;
        }
        /// <summary>
        /// Поиск индекса последнего положительного числа массива
        /// </summary>
        /// <param name="a">массив</param>
        /// <returns>индекс последнего положительного числа массива</returns>
        static public int NumLastPos(double[] a)
        {
            int i = 0;
            for (int k = a.Length - 1; k > -1; k--)
                if (a[k] > 0) return k;
            return i;
        }

        #region Методы по обслуживанию вычислений
        // ---------------------------------------------------------------------------------------------------------------
        // дополнение
        // ---------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Вычисление значения функции сплайна в точке 0 ~ х ~ 1 между idx  и idx-1 узлами
        /// </summary>
        /// <param name="idx">индек ведущего узла</param>
        /// <param name="x">локальная переменая</param>
        /// <returns></returns>
        public double FunValue(int idx, double x)
        {
            int N = Coeff.Length;
            double u1, u2, c1, c2;
            double q1, q2, q3, q4;
            if (idx == 0)
            {
                u1 = f[N - 1];
                u2 = f[0];
                c1 = Coeff[N - 1];
                c2 = Coeff[0];
            }
            else
            {
                u1 = f[idx - 1];
                u2 = f[idx];
                c1 = Coeff[idx - 1];
                c2 = Coeff[idx];
            }
            double x2 = x * x;
            q1 = 1 - x;
            q2 = x;
            q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
            q4 = 1.0 / 6 * (x2 * x - x);
            // Сплайн аппроксимация
            double u = u1 * q1 + u2 * q2 + q3 * c1 + q4 * c2;
            // Линейная апроксимация
            double us = u1 * q1 + u2 * q2;
            return u;
        }



        /// <summary>
        /// Вычисление корня функции между idx  и idx-1 узлами
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double RootValue(int idx)
        {
            int N = Coeff.Length;
            double u1, u2, c1, c2;
            double q1, q2, q3, q4;
            double a, dNdx;
            double err = 0.0000001;
            if (idx == 0)
            {
                u1 = f[N - 1];
                u2 = f[0];
                c1 = Coeff[N - 1];
                c2 = Coeff[0];
            }
            else
            {
                u1 = f[idx - 1];
                u2 = f[idx];
                c1 = Coeff[idx - 1];
                c2 = Coeff[idx];
            }
            double X = 0;
            // Метод половинного деления
            a = 0;
            dNdx = 1;
            double x, x2;
            double u;
            double uL = u1;
            double M = Math.Abs(u1) + Math.Abs(u2);
            for (int i = 0; i < 36; i++)
            {
                x = (a + dNdx) / 2.0;
                x2 = x * x;
                q1 = 1 - x;
                q2 = x;
                q3 = -1.0 / 6 * (x2 * x - 3 * x2 + 2 * x);
                q4 = 1.0 / 6 * (x2 * x - x);
                u = u1 * q1 + u2 * q2 + q3 * c1 + q4 * c2;
                if (Math.Abs(u / M) < err)
                {
                    X = x; break;
                }
                if (uL * u > 0)
                    a = x;
                else
                    dNdx = x;
                x = (a + dNdx) / 2.0;
                X = x;
                if (dNdx - a < err)
                    break;
            }
            return X;
        }
        #endregion

        public bool TestMethod(double[] tcoeff, double[] x, bool Flag)
        {
            double m = 0;
            double e = 0;
            for (int i = 0; i < x.Length; i++)
            {
                e += Math.Abs(tcoeff[i] - x[i]);
                m += Math.Abs(x[i]);
            }
            if (e / m < 0.0001) Flag = Flag & true;
            else Flag = Flag & false;
            return Flag;
        }
        /// <summary>
        /// Тест метод
        /// </summary>
        public bool TestSpline()
        {
            // N = 32
            bool Flag = true;
            // тест метода: Вычисление коэффициентов сплайна
            // коэффициенты кривой
            double[] fx = {  0.400345, 0.575908, 0.729339, 0.854742, 0.947298, 1.00345, 1.02104,
                             0.999391, 0.939336, 0.843184, 0.714628, 0.55861, 0.381125, 0.188993,
                            -0.010402, -0.209397, -0.400345, -0.575908, -0.729339, -0.854742,
                            -0.947298, -1.00345, -1.02104, -0.999391, -0.939336, -0.843184,
                            -0.714628, -0.55861, -0.381125, -0.188993, 0.010402, 0.209397};
            // тестовые коэффициенты сплайна 
            double[] tcoeff = {-0.0154842, -0.0222745, -0.0282088, -0.033059, -0.0366388,
                            -0.0388106, -0.0394909, -0.0386536, -0.0363309, -0.032612,
                            -0.0276398, -0.0216055, -0.0147408, -0.0073097, 0.00040232,
                            0.00809888, 0.0154842, 0.0222745, 0.0282088, 0.033059, 0.0366388,
                            0.0388106, 0.0394909, 0.0386536, 0.0363309, 0.032612, 0.0276398,
                            0.0216055, 0.0147408, 0.0073097, -0.00040232, -0.00809888};

            double[] coeff1 = CoefSpline(fx);
            Flag = TestMethod(tcoeff, coeff1, Flag);

            // тест метода: Дифференцирование кривой в узлах сплайна без насыщения
            // тестовые значения производной
            double[] tDSn =  { 5.90203, 5.29788, 4.49014, 3.50985, 2.39468, 1.18748, -0.0653577,
                            -1.31568, -2.51544, -3.61854, -4.58257, -5.3705, -5.95205, -6.30486,
                            -6.41538, -6.27936, -5.90203, -5.29788, -4.49014, -3.50985, -2.39468,
                            -1.18748, 0.0653577, 1.31568, 2.51544, 3.61854, 4.58257, 5.3705,
                            5.95205, 6.30486, 6.41538, 6.27936};
            double[] DSn = DiffSplineFunctionWhith();
            Flag = TestMethod(tDSn, DSn, Flag);

            double[] tY = {0.400345, 0.575908, 0.729339, 0.854742, 0.947298, 1.00345, 1.02104,
                           0.999391, 0.939336, 0.843184, 0.714628, 0.55861, 0.381125, 0.188993,
                          -0.010402, -0.209397, -0.400345, -0.575908, -0.729339, -0.854742,
                          -0.947298, -1.00345, -1.02104, -0.999391, -0.939336, -0.843184,
                          -0.714628, -0.55861, -0.381125, -0.188993, 0.010402, 0.209397};

            double[] Y = SplineFunction();
            Flag = TestMethod(tY, Y, Flag);

            double[] tDS = {5.90198, 5.29784, 4.49011, 3.50982, 2.39466, 1.18747, -0.0653571,
                            -1.31567, -2.51542, -3.61851, -4.58253, -5.37046, -5.952, -6.30481,
                            -6.41532, -6.27931, -5.90198, -5.29784, -4.49011, -3.50982, -2.39466,
                            -1.18747, 0.0653571, 1.31567, 2.51542, 3.61851, 4.58253, 5.37046,
                            5.952, 6.30481, 6.41532, 6.27931};
            double[] DS = DiffSplineFunction();
            Flag = TestMethod(tDS, DS, Flag);

            double[] f1x = { 1, 2, 3, 6, 10, 15, 20, 25, 30, 32.72, 30, 25, 20, 15, 10, 5, 3, 2, 1, 0 };
            double[] f1y = {-2.85, -0.55, 1.17, 4.62, 6.91, 7.66, 6.36, 2.35, -5.32, -11.44, -9.63, -5.58, -3.45, -2.65, -3.03, -4.67,
                     -5.75, -6.39, -7.04, -6.27};
            double[] tfilterX = {1, 2, 10.0/3, 37.0/6, 61.0/6, 15, 20, 25, 29.62, 31.8133, 29.62, 25, 20,
                        15, 10, 11.0/2, 19.0/6, 2, 1, 1.0/3};

            double[] filterX = FilterSpline(f1x);
            Flag = TestMethod(tfilterX, filterX, Flag);

            int idx = NumFirstPos(f1y);
            Flag = Flag && (idx == 2);
            idx = NumLastPos(f1y);
            Flag = Flag && (idx == 7);
            return Flag;
        }
    }
}
