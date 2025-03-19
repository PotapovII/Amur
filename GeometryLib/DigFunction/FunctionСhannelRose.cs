//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 23.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: Класс - для определения начальной параболической геометрии канала Розовского 
    /// </summary>
    [Serializable]
    public class FunctionСhannelRose : AbDigFunction
    {
        /// <summary>
        /// Максимальнаыя глубина канала
        /// </summary>
        double H;
        /// <summary>
        /// Ширина канала по свободной поверхности
        /// </summary>
        double W, W2;
        /// <summary>
        /// Доля ширины для берега канала
        /// </summary>
        double RiverBank;
        /// <summary>
        /// Получаем начальную геометрию русла для заданной дискретизации дна
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public override void GetFunctionData(ref double[] x, ref double[] y, int Count)
        {
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            double L = W * (1 + 2 * RiverBank * W);
            double left = - RiverBank * W;
            double dx = L / (Count - 1);
            for (int i = 0; i < Count; i++)
            {
                double xi = left + i * dx;
                x[i] = xi;
                y[i] = FunctionValue(xi);
            }
        }
        /// <summary>
        /// Гауссовский холм
        /// </summary>
        public override double FunctionValue(double y)
        {
            double Z = H * (1 - 4.2 * (W - y) * y / W2);
            return Z;
        }
        public FunctionСhannelRose(int CountCR = 11, double RiverBank = 0.1, double H = 0.14, double W = 1.6) : base()
        {
            this.H = H;
            this.W = W; 
            this.RiverBank = RiverBank;
            W2 = W * W;
            double L = W * (1 + 2 * RiverBank * W);
            double left = - RiverBank * W;
            double dy = L / (CountCR-1);
            for (int i = 0; i < CountCR; i++)
            {
                double yi = left + i * dy;
                x0.Add(yi);
                y0.Add(FunctionValue(yi));
            }
            name = "Канал розовского";
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

    /// <summary>
    /// ОО: Класс - для определения начальной параболической геометрии канала Розовского 
    /// </summary>
    [Serializable]
    public class FunctionСhannel : AbDigFunction
    {
        /// <summary>
        /// Максимальнаыя глубина канала
        /// </summary>
        double H;
        /// <summary>
        /// Ширина канала по свободной поверхности
        /// </summary>
        double W, W2;
        
        double N;
        /// <summary>
        /// Доля ширины для берега канала
        /// </summary>
        double RiverBank;
        /// <summary>
        /// Получаем начальную геометрию русла для заданной дискретизации дна
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public override void GetFunctionData(ref double[] x, ref double[] y, int Count)
        {
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            double L = W * (1 + 2 * RiverBank);
            double left = - RiverBank * W;
            double dx = L / (Count - 1);
            for (int i = 0; i < Count; i++)
            {
                double xi = left + i * dx;
                x[i] = xi;
                y[i] = FunctionValue(xi);
            }
        }
        /// <summary>
        /// Гауссовский холм
        /// </summary>
        public override double FunctionValue(double y)
        {
            double Z = H * (1 - (W - y) * y / N);
            return Z;
        }
        public FunctionСhannel(int CountCR = 11, double H = 0.14, double W = 1.6) : base()
        {
            this.H = H;
            this.W = W;
            double yc = W / 2;
            N = (W - yc) * yc;
            RiverBank = 0.1 * W;
            double L = W * (1 + 2 * RiverBank);
            double left = - RiverBank;
            double dy = L / (CountCR - 1);
            for (int i = 0; i < CountCR; i++)
            {
                double yi = left + i * dy;
                x0.Add(yi);
                y0.Add(FunctionValue(yi));
            }
            name = "Канал";
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

