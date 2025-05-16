//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          01.09.21
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using MemLogLib;
    using CommonLib.Delegate;
    
    public class DMath
    {
        public static double F(double x)
        {
            return x - 2;
            //return x*x*x - 2;
            //return Math.Sqrt(x) - 2;
            //return Math.Exp(x) - 2;
            //return Math.Log(2+x) - 2;
        }
        public static void Main()
        {
            double distance = Distance(0, 1, 1, 0, 1, 1);
            Console.WriteLine("Distance = {0}", distance);
            distance = Distance(0, 0, 1, 1, 1, 0);
            Console.WriteLine("Distance = {0}", distance);
            //double root = RootBisectionMethod(F, 0, 10, MEM.Error8);
            //Console.WriteLine("root = {0}", root);
            //root = RootСhordMethod(F, 0, 10, MEM.Error8);
            //Console.WriteLine("root = {0}", root);
        }
        /// <summary>
        /// Метод вычисления корня уравнения fun(x)=0 методом деления отрезка пополам
        /// </summary>
        public static double RootBisectionMethod(Function<double,double> fun, double a, double b, double Error = 10e-6)
        {
            int k;
            double root = 0;
            double funRoot;
            double funA = fun(a);
            double funB = fun(b);
            if (funA * funB > 0)
            {
                Logger.Instance.Info("ОШИБКА корень уравнения fun(x)=0 отсутствует в искомом интервале a =" + a.ToString() + "  b= " + b.ToString());
                return (a + b) / 2;
            }
            for (k = 0; k < 1000; k++)
            {
                root = (a + b) / 2;
                funRoot = fun(root);
                if (funA * funRoot < 0)
                    b = root;
                else
                {
                    a = root;
                    funA = funRoot;
                }
                //  k++;
                if (Math.Abs((b - a) / (Math.Abs(a) + Math.Abs(b))) <= Error)
                    break;
            }
            Console.WriteLine("intrs = {0}", k);
            return root;
        }
        /// <summary>
        /// Метод вычтсления корня уравнения fun(x)=0 методом хорд (хорош для нахождения корней линейных уравнений)
        /// </summary>
        public static double RootСhordMethod(Function<double, double> fun, double a, double b, double Error = 10e-6)
        {
            int k;
            double rootOld;
            double root = a;
            double funRoot;
            double funA = fun(a);
            double funB = fun(b);
            if (funA * funB > 0)
            {
                Logger.Instance.Info("ОШИБКА корень уравнения fun(x)=0 отсутствует в искомом интервале a = " + a.ToString() + "  b= " + b.ToString());
                return (a + b) / 2;
            }
            for (k = 0; k < 100000; k++)
            {
                rootOld = root;
                root = a - (b - a) * funA / (funB - funA);
                funRoot = fun(root);
                if (funA * funRoot < 0)
                {
                    b = root;
                    funB = funRoot;
                }
                else
                {
                    a = root;
                    funA = funRoot;
                }
                if (Math.Abs((root - rootOld) / (Math.Abs(root) + Math.Abs(rootOld))) <= Error)
                    break;
            }
            Console.WriteLine("intrs = {0}", k);
            return root;
        }
        /// <summary>
        /// Метод трапеций для выисления интеграла функции f(x) для нерегулярнрй сетке x[]
        /// </summary>
        public static double Integtal(double[] x, double[] f)
        {
            double sum = 0;
            for (int i = 0; i < x.Length - 1; i++)
                sum += (f[i] + f[i + 1]) * Math.Abs(x[i + 1] - x[i]);
            return 0.5 * sum;
        }
        /// <summary>
        /// Метод трапеций для выисления интеграла функции | f(x) | для нерегулярнрй сетке x[]
        /// </summary>
        public static double IntegtalAbs(double[] x, double[] f)
        {
            double sum = 0;
            for (int i = 0; i < x.Length - 1; i++)
                sum += (Math.Abs(f[i]) + Math.Abs(f[i + 1])) * Math.Abs(x[i + 1] - x[i]);
            return 0.5 * sum;
        }

        /// <summary>
        /// Метод вычисления длины кривой (по отрезкам)
        /// </summary>
        public static double LengthCurve(double[] x, double[] y)
        {
            double sum = 0;
            for (int i = 0; i < x.Length - 1; i++)
                sum += Math.Sqrt((y[i] - y[i + 1]) * (y[i] - y[i + 1]) + (x[i + 1] - x[i]) * (x[i + 1] - x[i]));
            return sum;
        }

        /// <summary>
        /// Метод вычисления длины кривой (по отрезкам) до узла idx
        /// </summary>
        public static double LengthCurve(double[] x, double[] y, int idx)
        {
            double sum = 0;
            for (int i = 1; i < idx; i++)
                sum += Math.Sqrt((y[i] - y[i - 1]) * (y[i] - y[i - 1]) + (x[i - 1] - x[i]) * (x[i - 1] - x[i]));
            return sum;
        }
        /// <summary>
        /// Вычисление максимума  функции заданной массивом значений y на интервале Lmin <= x <= Lmax
        /// </summary>
        public static (double xMax, double yMax) Max(double[] y, double Lmax, double Lmin = 0)
        {
            double[] x = new double[y.Length];
            double dx = (Lmax - Lmin) / (y.Length - 1);
            for (int i = 0; i < x.Length; i++)
                x[i] = Lmin + i * dx;
            return Max(x, y);
        }
        /// <summary>
        /// Вычисление максимума функции заданной массивами x и y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static ( double xMax, double yMax ) Max(double[] x, double[] y)
        {
            double xMax = x[0];
            double yMax = y[0];
            int imax = 0;
            for (int i = 1; i < x.Length; i++)
            {
                if(y[i]> yMax)
                {
                    yMax = y[i];
                    xMax = x[i];
                    imax = i;
                }
            }
            if(imax==0 || imax == x.Length-1)
                return (xMax, yMax);
            else
            {
                xMax = RootExtremum(x[imax - 1], x[imax], x[imax + 1], y[imax - 1], y[imax], y[imax + 1]);
                yMax = FunCubuc(x[imax - 1], x[imax], x[imax + 1], y[imax - 1], y[imax], y[imax + 1], xMax);
            }
            return ( xMax, yMax );
        }
        /// <summary>
        /// Вычисление минимума функции заданной массивом значений y на интервале Lmin <= x <= Lmax
        /// </summary>
        /// <returns></returns>
        public static (double xMin, double yMin) Min(double[] y, double Lmax, double Lmin = 0)
        {
            double[] x = new double[y.Length];
            double dx = (Lmax-Lmin) / (y.Length - 1);
            for (int i = 0; i < x.Length; i++)
                x[i] = Lmin + i * dx;
            return Min(x, y);
        }
        /// <summary>
        /// Вычисление минимума функции заданной массивами x и y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static (double xMin, double yMin) Min(double[] x, double[] y)
        {
            double xMin = x[0];
            double yMin = y[0];
            int imax = 0;
            for (int i = 1; i < x.Length; i++)
            {
                if (y[i] < yMin)
                {
                    yMin = y[i];
                    xMin = x[i];
                    imax = i;
                }
            }
            if (imax == 0 || imax == x.Length - 1)
                return (xMin, yMin);
            else
            {
                xMin = RootExtremum(x[imax - 1], x[imax], x[imax + 1], y[imax - 1], y[imax], y[imax + 1]);
                yMin = FunCubuc(x[imax - 1], x[imax], x[imax + 1], y[imax - 1], y[imax], y[imax + 1], xMin);
            }
            return (xMin, yMin);
        }
        /// <summary>
        /// Поиск корня в табличной функции
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static (double xRoot, double yRoot) RootFun(double[] x, double[] y)
        {
            double xRoot = 0;
            double yRoot = 0;
            double yL = y[0];
            int imax = 0;
            int i;
            for (i = 1; i < x.Length; i++)
            {
                if (y[i]*yL < 0)
                {
                    imax = i;
                    break;
                }
            }
            if(i == x.Length)
            {
                Logger.Instance.Info("Корень отсутсвует");
                return (xRoot, yRoot);
            }
            if(imax == x.Length-1)
            {
                xRoot = RootFunCubuc(x[imax - 2], x[imax-1], x[imax], y[imax - 2], y[imax - 1], y[imax]);
                yRoot = FunCubuc(x[imax - 2], x[imax - 1], x[imax], y[imax - 2], y[imax - 1], y[imax], xRoot);
            }
            else
            {
                if (imax == 0)
                {
                    xRoot = RootFunCubuc(x[imax], x[imax + 1], x[imax + 2], y[imax], y[imax + 1], y[imax + 2]);
                    yRoot = FunCubuc(x[imax], x[imax + 1], x[imax + 2], y[imax], y[imax + 1], y[imax + 2], xRoot);
                }
                else
                {
                    xRoot = RootFunCubuc(x[imax - 1], x[imax], x[imax + 1], y[imax - 1], y[imax], y[imax + 1]);
                    yRoot = FunCubuc(x[imax - 1], x[imax], x[imax + 1], y[imax - 1], y[imax], y[imax + 1], xRoot);
                }
            }
            return (xRoot, yRoot);
        }
        /// <summary>
        /// квадратичная интерполяция
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double FunCubuc(double x0, double x1, double x2, double U0, double U1, double U2, double x)
        {
            double U = U0 * (x - x1) * (x - x2) / (x0 - x1) / (x0 - x2) + U1 * (x - x0) * (x - x2) / (x1 - x0) / (x1 - x2) + U2 * (x - x0) * (x - x1) / (x2 - x0) / (x2 - x1);
            return U;
        }
        /// <summary>
        /// определяет точку экстремума при квадратичной интерполяции функции
        /// </summary>
        public static double RootExtremum(double x0, double x1, double x2, double U0, double U1, double U2)
        {
            double root;
            double Z = -U1 * x0 + U0 * x1 - U0 * x2 + U1 * x2 + U2 * x0 - U2 * x1;
            if( Math.Abs(Z)<MEM.Error10)
            {
                if(U2 > U0)
                    root = x2;
                else
                    root = x0;
            }
            else
                root = 0.5 * (U1 * x2 * x2 - U0 * x2 * x2 + U0 * x1 * x1 - U2 * x1 * x1 + U2 * x0 * x0 - U1 * x0 * x0) / Z;
            return root;
        }
        /// <summary>
        /// определяет корень при квадратичной интерполяции функции
        /// полагается, что конень 1
        /// </summary>
        public static double RootFunCubuc(double x0, double x1, double x2, double U0, double U1, double U2)
        {
            double root = 0;
            try
            {
                double a = U0 / (x0 - x1) / (x0 - x2) + U1 / (x1 - x0) / (x1 - x2) + U2 / (x2 - x0) / (x2 - x1);
                double b = (-U0 * x1 - U0 * x2) / (x0 - x1) / (x0 - x2) + (-U1 * x0 - U1 * x2) / (x1 - x0) / (x1 - x2) + (-U2 * x0 - U2 * x1) / (x2 - x0) / (x2 - x1);
                double c = U0 * x1 * x2 / (x0 - x1) / (x0 - x2) + U1 * x0 * x2 / (x1 - x0) / (x1 - x2) + U2 * x0 * x1 / (x2 - x0) / (x2 - x1);
                if (Math.Abs(a) < MEM.Error10)
                {
                    // линейная интерполяция
                    if (Math.Abs(b) < MEM.Error10)
                    {
                        Logger.Instance.Info("Корень отсутсвует");
                        return 0;
                    }
                    root = -c / b;
                }
                else
                {
                    double det = b * b - 4 * a * c;
                    if (det < 0)
                    {
                        Logger.Instance.Info("Действительный корень отсутсвует");
                        return 0;
                    }
                    double detS = Math.Sqrt(det);
                    double rp = ( detS - b) / (2 * a);
                    double rm = (-detS - b) / (2 * a);
                    if (Math.Min(x0, x2) <= rp && rp <= Math.Max(x0, x2))
                        root = rp;
                    else
                        root = rm;
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Info("Корень отсутсвует");
                Logger.Instance.Exception(ex);
            }
            return root;
        }
        /// <summary>
        /// Расcтояние от точки xp, yp до линии (x0, y0), (x1, y1)
        /// </summary>
        public static double Distance(double x0, double y0, double x1, double y1, double xp, double yp)
        {
            double A = y1 - y0;
            double B = x1 - x0;
            double C = - y0 * B + x0 * A;
            double L = Math.Sqrt(A * A + B * B);
            return Math.Abs(A * xp + B * yp + C) / L;
        }
        /// <summary>
        /// точка пересечения (x y) нормали опущенной на линию A*x+B*y+C=0 (x0, y0, x1, y1) из точки (xp yp)
        /// </summary>
        public static void NormalCrossPoint(ref  double x,ref double y, double x0, double y0, double x1, double y1, double xp, double yp)
        {
            double A = y1 - y0;
            double B = x1 - x0;
            double C = -y0 * B + x0 * A;
            double L2 = A * A + B * B;
            x =  - (B * A * yp - xp * B * B + C * A) / L2;
            y = (A *A * yp - C * B - B * A * xp) / L2;
        }
    }
}
