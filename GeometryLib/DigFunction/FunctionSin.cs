//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class FunctionSin : AbDigFunction
    {
        double H;
        double k;
        /// <summary>
        /// Получаем начальную геометрию русла для заданной дискретизации дна
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public override void GetFunctionData(ref double[] x, ref double[] y,
             int Count = 10, bool revers = false)
        {
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            double dx = Length / (Count - 1);
            if (isConstant == true)
            {
                for (int i = 0; i < Count; i++)
                {
                    double xi = x0[0] + i * dx;
                    x[i] = xi;
                    y[i] = y0[0];
                }
            }
            else
            {
                for (int i = 0; i < Count; i++)
                {
                    double xi = x0[0] + i * dx;
                    x[i] = xi;
                    y[i] = FunctionValue(xi);
                }
            }
            if (revers == true)
                MEM.Reverse(ref y);
        }
        /// <summary>
        /// Гауссовский холм
        /// </summary>
        public override double FunctionValue(double s)
        {
            double y = H * Math.Sin(k * s);
            return y;
        }
        public FunctionSin()
        {
            double[] xx = { 0, 1 };
            double[] yy = { 0, 1 };
            SetFunctionData(xx, yy, "Функция Sin");
            Init(0.1, 0.05);
        }
        public FunctionSin(double amplitude, double NN, double[] xx, double[] yy)
        {
            SetFunctionData(xx, yy, "Функция Sin");
            Init(amplitude, NN);
        }
        public void Init(double amplitude, double NN)
        {
            H = Height * amplitude;
            k = 2 * Math.PI * NN / Length;
        }
        /// <summary>
        /// Получить базу функции
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="N">количество</param>
        public override void GetBase(ref double[][] fun, int N = 0)
        {
            MEM.Alloc(2, N, ref fun);
            GetFunctionData(ref fun[0], ref fun[1], N);
        }
    }
}
