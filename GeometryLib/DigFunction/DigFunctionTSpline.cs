//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 23.03.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using MemLogLib;
    using CommonLib.Function;
    
    /// <summary>
    /// ОО: Класс - для раоты с однозначными сплайн функциями
    /// </summary>
    [Serializable]
    public class DigFunctionTSpline : DigFunction
    {
        TSpline spline = new TSpline();
        public DigFunctionTSpline(string name = "сплайн функция")
        {
            Smoothness = SmoothnessFunction.spline;
        }
        public DigFunctionTSpline(DigFunctionTSpline sp)
        {
            name = sp.name;
            Smoothness = sp.Smoothness;
            double[] x = sp.x0.ToArray();
            double[] y = sp.y0.ToArray();
            SetFunctionData(x, y, name);
            spline.Set(y, x);
        }
        public  DigFunctionTSpline(double[] x, double[] y, string name = "")
        {
            if (name == "")
                name = "сплайн по точкам";
            Smoothness = SmoothnessFunction.spline;
            SetFunctionData(x, y, name);
            spline.Set(y, x);
        }
        /// <summary>
        /// Получаем начальную геометрию русла для заданной дискретизации дна
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public override void GetFunctionData(ref double[] x, ref double[] y,
               int Count = 10, bool revers = false)
        {
            if (Count < 21) Count = 21;
            double L = x0[x0.Count - 1] - x0[0];
            double dx = L / (Count - 1);
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            for (int i = 0; i < Count; i++)
            {
                double xi = x0[0] + i * dx;
                x[i] = xi;
                y[i] = spline.Value(xi);
            }
            if (revers == true)
                MEM.Reverse(ref y);
        }
        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public override double FunctionValue(double arg)
        {
            return spline.Value(arg);
        }
    }
}