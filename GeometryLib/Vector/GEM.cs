//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 27.11.2022 Потапов И.И. (c# => c#)
//---------------------------------------------------------------------------
namespace GeometryLib.Vector
{
    using System.Linq;
    public class GEM
    {
        /// <summary>
        /// Нормализация вектора по интервалу
        /// </summary>
        /// <param name="mas"></param>
        public static double NormMas(ref double[] mas, double M = 1)
        {
            double min = mas.Min();
            double max = mas.Max();
            double L = (max - min)/M;
            for (int i = 0; i < mas.Length; i++)
                mas[i] = (mas[i] - min) / L;
            return L;
        }
        /// <summary>
        /// Нормализация вектора по интервалу
        /// </summary>
        /// <param name="mas"></param>
        public static double NormMaxMas(ref double[] mas)
        {
            double max = mas.Max();
            for (int i = 0; i < mas.Length; i++)
                mas[i] = mas[i] / max;
            return max;
        }
    }
}
