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
        public override void GetFunctionData(ref double[] x, ref double[] y,
            int Count = 10, bool revers = false)
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
            if (revers == true)
                MEM.Reverse(ref y);
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
    /// задает геометрию параболой
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
        public override void GetFunctionData(ref double[] x, ref double[] y,
                                        int Count = 10, bool revers = false)
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
            if (revers == true)
                MEM.Reverse(ref y);
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

    /// <summary>
    /// ОО: Класс - для определения начальной параболической геометрии канала Розовского 
    /// задает геометрию уступа
    /// </summary>
    [Serializable]
    public class FunctionСhannelStep : AbDigFunction
    {
        /// <summary>
        /// Максимальнаыя глубина канала
        /// </summary>
        double H;
        /// <summary>
        /// Максимальнаыя глубина канала в мелкой части
        /// </summary>
        double H1;
        /// <summary>
        /// Длина канала по свободной поверхности
        /// </summary>
        double L;
        /// <summary>
        /// Длина канала в узкой части 
        /// </summary>
        double L1;
        /// <summary>
        /// Длина канала в переходной части 
        /// </summary>
        double L2;
        double Lk;
        double Hk;
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
            double dx = L / (Count - 1);
            for (int i = 0; i < Count; i++)
            {
                double xi = i * dx;
                x[i] = xi;
                y[i] = FunctionValue(xi);
            }
            if (revers == true)
                MEM.Reverse(ref y);
        }
        /// <summary>
        /// Гауссовский холм
        /// </summary>
        public override double FunctionValue(double x)
        {
            if (x <= L1)
                return H1;
            else if (x >= Lk)
                return H;
            else
            {
                double F = (1 + Math.Cos((x - Lk) / L2 * Math.PI)) / 2;
                return H1 + Hk * F;
            }
            //if (x <= L1)
            //    return - H1;
            //else if(x >= Lk)
            //    return - H;
            //else
            //{
            //    double F = (1 + Math.Cos((x - Lk) / L2 * Math.PI)) / 2;
            //    return - H1 - Hk * F;
            //}
        }
        public FunctionСhannelStep(int CountCR, double L, double L1, double L2, double H, double H1) : base()
        {
            this.L = L;
            this.L1 = L1;
            this.L2 = L2;
            this.H = H;
            this.H1 = H1;
            this.Lk = L2 + L1;
            this.Hk = H - H1;
            double dx = L / (CountCR - 1);
            for (int i = 0; i < CountCR; i++)
            {
                double xi = i * dx;
                x0.Add(xi);
                y0.Add(FunctionValue(xi));
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

