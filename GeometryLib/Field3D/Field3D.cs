//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                    создание поля Field1D
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                      создание иерархии полей
//                 кодировка : 13.03.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Поле название поля и массив(ы) данных поля в узлах сетки
    /// </summary>
    [Serializable]
    public class Field3D
    {
        
        /// <summary>
        /// Название поля
        /// </summary>
        public string Name { get; set; }
        public int CountX { get; protected set; }
        public int CountY { get; protected set; }
        /// <summary>
        /// значения поля в узлах сетки
        /// </summary>
        public double[][] Values;
        /// <summary>
        /// координаты узлов по х
        /// </summary>
        public double[] x;
        /// <summary>
        /// координаты узлов по y
        /// </summary>
        public double[] y;
        public Field3D(string Name, double[][] VX,double[] x, double[] y)
        {
            this.Name = Name;
            CountX = VX.Length;
            CountY = VX[0].Length;
            this.x = MEM.Copy<double>(this.x, x);
            this.y = MEM.Copy<double>(this.y, y);
            MEM.MemCopy(ref Values, VX);
        }
        public Field3D(Field3D p)
        {
            Name = p.Name;
            CountX = p.CountX;
            CountY = p.CountY;
            this.x = MEM.Copy<double>(this.x, p.x);
            this.y = MEM.Copy<double>(this.y, p.y);
            MEM.MemCopy(ref Values, p.Values);
        }
        /// <summary>
        /// Вычисление интерполяционного значения поля в точке (ax,ay)
        /// </summary>
        public double GetValue(double ax, double ay)
        {
            double V = 0;
            try
            {
                int i = 0;
                int j = 0;
                // TODO подумать над хешем
                if (x[0] > ax)
                {
                    ax = x[0];
                    i = 1;
                }
                else
                {
                    if (x[x.Length - 1] < ax)
                    {
                        ax = x[x.Length - 1];
                        i = x.Length - 1;
                    }
                    else
                    {
                        for (i = 1; i < x.Length; i++)
                            if (ax <= x[i]) break;
                    }
                }
                if (y[0] > ay)
                {
                    ay = y[0];
                    j = 1;
                }
                else
                {
                    if (y[y.Length - 1] < ay)
                    {
                        ay = y[y.Length - 1];
                        j = y.Length - 1;
                    }
                    else
                    {
                        for (j = 1; j < y.Length; j++)
                            if (ay <= y[j]) break;
                    }
                }
                double x0 = x[i - 1];
                double L = x[i] - x0;
                double y0 = y[j - 1];
                double H = y[j] - y0;
                double NX2 = (ax - x0) / L;
                double NX1 = 1 - NX2;
                double NY2 = (ay - y0) / H;
                double NY1 = 1 - NY2;
                double N0 = NX1 * NY2;
                double N1 = NX2 * NY2;
                double N2 = NX2 * NY1;
                double N3 = NX1 * NY1;
                double V0 = Values[j][i - 1];
                double V1 = Values[j][i];
                double V2 = Values[j - 1][i];
                double V3 = Values[j - 1][i - 1];
                V = N0 * V0 + N1 * V1 + N2 * V2 + N3 * V3;
                if (double.IsNaN(V) == true)
                    throw new Exception("IsNaN");
                if (double.IsInfinity(V) == true)
                    throw new Exception("IsInfinity");
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Error("Нештатное завершение процесса расчета", "Field3D.GetValue()");
            }
            return V;
        }
    }

}
