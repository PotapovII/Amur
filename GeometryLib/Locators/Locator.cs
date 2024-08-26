//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 18.06.2021 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using System.Linq;
    using MemLogLib;
    /// <summary>
    /// Класс для поиска размерности регулярной сетки для проекции на 
    /// нее области описанной с помощью нерегулярной сетки
    /// </summary>
    public class Locator
    {
        /// <summary>
        /// Поиск количества рекомендательных узлов по оси Х
        /// </summary>
        /// <returns></returns>
        public static void FindStep(double[] X, ref int NMin, ref int Nmax)
        {
            double[] xx = null;
            MEM.Copy(xx, X);
            Array.Sort(xx);
            int k = 0;
            double Right = X.Max();
            double Left = X.Min();
            double L = Right - Left;
            double dxMin = Right - Left;
            double dxMax = 0;
            double dxx;
            for (int i = 1; i < xx.Length; i++)
            {
                if (MEM.Equals(xx[k] - xx[i], 0, MEM.Error5) != true)
                {
                    dxx = Math.Abs(xx[k] - xx[i]);
                    dxMin = Math.Min(dxMin, dxx);
                    dxMax = Math.Max(dxMax, dxx);
                    k = i;
                }
            }
            NMin = (int)(1.00001 * L / dxMin) + 1;
            Nmax = (int)(1.00001 * L / dxMax) + 1;
        }
        /// <summary>
        /// Поиск количества рекомендательных узлов по оси Х
        /// </summary>
        /// <returns></returns>
        public static void FindMinStep(double[] X, ref int N, ref double L)
        {
            double[] xx = null;
            MEM.MemCopy(ref xx, X);
            Array.Sort(xx);
            int k = 0;
            L = X.Max() - X.Min();
            double dxMin = L;
            double dxx;
            for (int i = 1; i < xx.Length; i++)
            {
                if (MEM.Equals(xx[k] - xx[i], 0, MEM.Error5) != true)
                {
                    dxx = Math.Abs(xx[k] - xx[i]);
                    dxMin = Math.Min(dxMin, dxx);
                    k = i;
                }
            }
            N = (int)(1.00001 * L / dxMin) + 1;
        }
        /// <summary>
        /// Поиск количества рекомендательных узлов по оси Х
        /// </summary>
        /// <returns></returns>
        public static void FindMinStep(double[] X, ref int N, ref double xa, ref double xb)
        {
            double[] xx = null;
            MEM.MemCopy(ref xx, X);
            Array.Sort(xx);
            int k = 0;
            xa = X.Min();
            xb = X.Max();
            double L = xb - xa;
            double dxMin = L;
            double dxx;
            for (int i = 1; i < xx.Length; i++)
            {
                if (MEM.Equals(xx[k] - xx[i], 0, MEM.Error5) != true)
                {
                    dxx = Math.Abs(xx[k] - xx[i]);
                    dxMin = Math.Min(dxMin, dxx);
                    k = i;
                }
            }
            N = (int)(1.00001 * L / dxMin) + 1;
        }
    }
}
