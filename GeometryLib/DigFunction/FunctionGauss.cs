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
    public class FunctionGauss : AbDigFunction
    {
        double H;
        double s0;
        double sigma2;
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
            double y = H * Math.Exp(-(s - s0) * (s - s0) / sigma2);
            return y;
        }
        public FunctionGauss()
        {
            double[] xx = { 0, 1 };
            double[] yy = { 0, 1 };
            Init(0.1, 0.05, xx, yy);
        }
        public FunctionGauss(double amplitude, double sigma, double[] xx, double[] yy)
        {
            Init(amplitude, sigma, xx, yy);
        }
        public void Init(double amplitude, double sigma, double[] xx, double[] yy)
        {
            name = "Функция Гаусса";
            x0.Add(xx[0]); x0.Add(xx[1]);
            y0.Add(yy[0]); y0.Add(yy[1]);
            // среднеквадратичное отклонение
            H = Height * amplitude;
            sigma2 = 2 * sigma * sigma * (Length * Length);
            s0 = Length / 2;
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
