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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2024 -
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                           29.01.24
//---------------------------------------------------------------------------
using MemLogLib;
using System;

namespace CommonLib.Function
{
    /// <summary>
    /// ОО: Одномерная функция
    /// </summary>
    public interface IFunction1D
    {
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Получение интерполированного (линейно) не сглаженного значения данных 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double FunctionValue(double x);
        /// <summary>
        /// Получение сглаженных данных 
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="Count">количество точек</param>
        void GetFunctionData(ref double[] x, ref double[] y, 
                             int Count = 10, bool revers = false);
    }

    //---------------------------------------------------------------------------
    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
    //                         проектировщик:
    //                           Потапов И.И.
    //---------------------------------------------------------------------------
    //                 кодировка : 22.11.2024 Потапов И.И.
    //---------------------------------------------------------------------------
    /// <summary>
    /// ОО: Табличная интерполяция
    /// </summary>
    [Serializable]
    public class DigFunction1D : IFunction1D
    {
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Аргумент
        /// </summary>
        public double[] arg = null;
        /// <summary>
        /// Функция
        /// </summary>
        public double[] fun = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="sn"></param>
        public DigFunction1D(string Name, double[] Arg, double[] Fun)
        {
            this.Name = Name;
            MEM.Copy(ref arg, Arg);
            MEM.Copy(ref fun, Fun);
        }
        /// <summary>
        /// Получаем линейную интерполяцию функции 
        /// в области определенной аргументом для Count точек
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public void GetFunctionData(ref double[] x, ref double[] y, 
                                    int Count = 10, bool revers = false)
        {
            MEM.Alloc(Count, ref x);
            MEM.Alloc(Count, ref y);
            double L = arg[arg.Length-1] - arg[0];
            int NN = Count - 1;
            x[0] = arg[0];
            y[0] = fun[0];
            if (Count == 1) return;
            x[NN] = arg[arg.Length-1];
            y[NN] = fun[arg.Length-1];
            if (Count == 2) return;
            double dx = L / NN;
            int i = 0;
            int start;
            for (int ii = 1; ii < NN; ii++)
            {
                double xx = ii * dx;
                start = i;
                for (i = start; i < arg.Length; i++)
                    if (arg[i] >= xx)
                        break;
                double N1 = (xx - arg[i - 1]) / (arg[i] - arg[i - 1]);
                x[ii] = xx;
                y[ii] = (1 - N1) * fun[i - 1] + N1 * fun[i];
            }
            if(revers == true)
                MEM.Reverse(ref y);
        }
        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double FunctionValue(double xx)
        {
            if (xx < arg[0])
                return fun[0];
            if (xx > arg[arg.Length - 1])
                return fun[arg.Length - 1];
            int i;
            for (i = 1; i < arg.Length; i++)
                if (arg[i] >= xx)
                    break;
            double N1 = (xx - arg[i - 1]) / (arg[i] - arg[i - 1]);
            double N0 = 1 - N1;
            double y = N0 * fun[i - 1] + N1 * fun[i];
            return y;
        }
        public void Test()
        {
            double[] d_50 = { 0.0001, 0.0002, 0.0003, 0.0004, 0.00050, 0.00075, 0.001, 0.0035 };
            double[] Ws_a = { 0.0057, 0.0170, 0.0321, 0.0461, 0.0567, 0.0858, 0.1115, 0.254 };

            IFunction1D WW = new DigFunction1D("A", d_50, Ws_a);

            double[] x = null;
            double[] y = null;
            int Count = 30;

            WW.GetFunctionData(ref x, ref y, Count);

            LOG.Print("d_50", d_50, 5);
            LOG.Print("Ws_a", Ws_a, 5);
            LOG.Print("x", x, 5);
            LOG.Print("x", y, 5);
        }
    }

}
