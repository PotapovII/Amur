//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2024 -
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                           29.01.24
//---------------------------------------------------------------------------
namespace RiverLib.Patankar
{
    using System;
    using MemLogLib;
    using CommonLib.Function;
    /// <summary>
    /// Функция с донными волнами на 2 интервале дна
    /// </summary>
    public class BedSinLen2 : IFunction1D
    {
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Получение интерполированного (линейно) не сглаженного значения данных 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double FunctionValue(double x)
        {
            if (x < Len1 || x > Len1 + Len2)
                return 0;
            else
                return bottomWaveAmplitude * Math.Sin(K * (x - Len1));
        }
        /// <summary>
        /// Получение сглаженных данных 
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="Count">количество точек</param>
        public void GetFunctionData(ref double[] x, ref double[] y, int Count = 0)
        {
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            double L = Len1+ Len2 + Len3;
            double dx = L / (Count - 1);
            for (int i = 0; i < Count; i++)
            {
                double xi = i * dx;
                x[i] = xi;
                y[i] = FunctionValue(xi);
            }
        }
        double Len1, Len2, Len3, K, bottomWaveAmplitude;
        public BedSinLen2(double Len1, double Len2, double Len3, double bottomWaveAmplitude, 
            int wavePeriod, string Name = "")
        {
            this.Len1 = Len1;
            this.Len2 = Len2;
            this.Len3 = Len3;
            this.bottomWaveAmplitude = bottomWaveAmplitude;
            this.Name = Name;
            K = 2 * Math.PI * wavePeriod / Len2;
        }
    }
}
        
 