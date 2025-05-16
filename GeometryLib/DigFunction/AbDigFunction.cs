//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 17.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib.Function;
    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public abstract class AbDigFunction : IDigFunction
    {
        /// <summary>
        /// Гладкость функции
        /// </summary>
        public SmoothnessFunction Smoothness { get; set; }
        /// <summary>
        /// Функция заданна одной точкой
        /// </summary>
        public bool isConstant => false;
        /// <summary>
        /// Функция является параметрической
        /// </summary>
        public bool isParametric
        {
            get
            {
                // ленивое определение, если не заданна дерективно 
                if (parametricOk == false)
                {
                    parametric = false;
                    for (int i = 1; i < x0.Count; i++)
                        if (Math.Abs(x0[i] - x0[i - 1]) < MEM.Error8)
                        {
                            parametric = true; return parametric;
                        }
                    parametricOk = true;
                    parametric = false;
                    return parametric;
                }
                else
                    return parametric;
            }
            set
            {
                parametric = value;
                if (parametric == true)
                    parametricOk = true;
                else
                    parametricOk = false;
            }
        }
        protected bool parametric = false;
        /// <summary>
        /// флаг ленивого определения parametric
        /// </summary>
        protected bool parametricOk;
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get { return name; } }
        protected string name;
        /// <summary>
        /// Начальное значение аргумента
        /// </summary>
        public double Xmin { get { return x0.ToArray().Min(); } }
        /// <summary>
        /// Начальное значение функции
        /// </summary>
        public double Ymin { get { return y0.ToArray().Min(); } }
        /// <summary>
        /// Начальное значение аргумента
        /// </summary>
        public double Xmax { get { return x0.ToArray().Max(); } }
        /// <summary>
        /// Начальное значение функции
        /// </summary>
        public double Ymax { get { return y0.ToArray().Max(); } }
        /// <summary>
        /// Длина области определения
        /// </summary>
        public double Length { get { return Xmax - Xmin; } }
        /// <summary>
        /// Высота области определения
        /// </summary>
        public double Height { get { return Ymax - Ymin; } }
        /// <summary>
        /// параметры функции
        /// </summary>
        public List<double> x0 = null;
        public List<double> y0 = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="sn"></param>
        public AbDigFunction(SmoothnessFunction sn = SmoothnessFunction.linear, bool parametricOk = false)
        {
            Smoothness = sn;
            this.parametricOk = parametricOk;
            x0 = new List<double>();
            y0 = new List<double>();
        }
        /// <summary>
        /// Добавление точки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Add(double x, double y)
        {
            x0.Add(x);
            y0.Add(y);
        }
        /// <summary>
        /// Очистка
        /// </summary>
        public void Clear()
        {
            x0.Clear();
            y0.Clear();
            parametricOk = false;
        }
        /// <summary>
        /// Получаем начальную геометрию русла для заданной дискретизации дна
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public virtual void GetFunctionData(ref double[] x, ref double[] y,
                            int Count = 10, bool revers = false)
        {
            MEM.Alloc<double>(Count, ref x);
            MEM.Alloc<double>(Count, ref y);
            if (isConstant == true && isParametric == false || x0.Count == 1)
            {
                x[0] = x0[0];
                y[0] = y0[0];
                return;
            }
            if (isParametric == false)
            {
                if (Count < 21) Count = 21;
                double L = x0[x0.Count - 1] - x0[0];
                double dx = L / (Count - 1);
                if (Smoothness == SmoothnessFunction.spline)
                {
                    TSpline spline = new TSpline();
                    spline.Set(y0.ToArray(), x0.ToArray());
                    for (int i = 0; i < Count; i++)
                    {
                        double xi = x0[0] + i * dx;
                        x[i] = xi;
                        y[i] = spline.Value(xi);
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
            }
            else
            {
                double L = DMath.LengthCurve(x0.ToArray(), y0.ToArray());
                double dx = L / (x0.Count - 1);
                double ds = L / (Count - 1);
                if (Smoothness == SmoothnessFunction.spline)
                {
                    double[] s = new double[x0.Count];
                    for (int i = 0; i < s.Length; i++)
                        s[i] = i * dx;
                    TSpline splineX = new TSpline();
                    TSpline splineY = new TSpline();
                    double[] xx0 = x0.ToArray();
                    double[] yy0 = y0.ToArray();
                    splineX.Set(xx0, s);
                    splineY.Set(yy0, s);
                    for (int i = 0; i < Count; i++)
                    {
                        x[i] = splineX.Value(i * ds);
                        y[i] = splineY.Value(i * ds);
                    }
                }
                else
                {
                    double[] LS = new double[x0.Count - 1];
                    for (int k = 0; k < x0.Count-1; k++)
                    {
                        double dsx = x0[k + 1] - x0[k];
                        double dsy = y0[k + 1] - y0[k];
                        LS[k] = Math.Sqrt(dsx * dsx + dsy * dsy);
                    }
                    int i;
                    for (int k = 0; k < Count; k++)
                    {
                        double xx = k * ds;
                        double Li = 0;
                        double sum = 0;
                        for (i = 0; i < x0.Count - 1; i++)
                        {
                            Li = LS[i];
                            sum += Li;
                            if (sum >= xx)
                                break;
                        }
                        if (i < x0.Count - 1)
                        {
                            double s0 = sum - Li;
                            double N1 = (xx - s0) / Li;
                            double N0 = 1 - N1;
                            x[k] = N0 * x0[i] + N1 * x0[i + 1];
                            y[k] = N0 * y0[i] + N1 * y0[i + 1];
                        }
                        else
                        {
                            x[k] = x0[i];
                            y[k] = y0[i];
                        }
                    }
                }
            }
            if (revers == true)
                MEM.Reverse(ref y);
        }
        /// <summary>
        /// Получить данные основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void GetFunctionData(ref string name, ref double[] x, ref double[] y)
        {
            name = this.name;
            x = x0.ToArray();
            y = y0.ToArray();
        }
        /// <summary>
        /// Получение маркера границы для каждого узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="Mark"></param>
        public void GetMark(double[] x, ref int[] Mark)
        {
            MEM.Alloc(x.Length, ref Mark);
            for (int i = 0; i < x.Length; i++)
                for (int i0 = 1; i0 < x0.Count; i0++)
                    if (x0[i0] >= x[i])
                    {
                        Mark[i] = i0;
                        break;
                    }
        }

        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public virtual double FunctionValue(double xx)
        {
            if (isConstant == true)
                return y0[0];
            else
            {
                if(xx<x0[0])
                    return y0[0];
                if (xx > x0[x0.Count-1])
                    return y0[x0.Count - 1];
                int i;
                for (i = 1; i < x0.Count; i++)
                    if (x0[i] >= xx)
                        break;
                double N1 = (xx - x0[i - 1]) / (x0[i] - x0[i - 1]);
                double N0 = 1 - N1;
                double y = N0 * y0[i - 1] + N1 * y0[i];
                return y;
            }
        }
        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public virtual void GetFunctionParametricData(double xx, ref double x, ref double y)
        {

        }
        /// <summary>
        /// загрузка начальной геометрии из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file) { }
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        public virtual void Save(StreamWriter file) { }
        /// <summary>
        /// внешняя загрузка начальной геометрии
        /// например из точки сохранения
        /// </summary>
        /// <param name="p"></param>
        /// <param name="WaterLevel"></param>
        public virtual void SetFunctionData(double[] x, double[] y, string name = "без названия")
        {
            this.name = name;
            Clear();
            for (int i = 0; i < x.Length; i++)
                Add(x[i], y[i]);
        }
        /// <summary>
        /// Получить базу функции
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="N">количество</param>
        public virtual void GetBase(ref double[][] fun, int N = 0)
        {
            MEM.Alloc(2, x0.Count, ref fun);
            for (int i = 0; i < x0.Count; i++)
            {
                fun[0][i] = x0[i];
                fun[1][i] = y0[i];
            }
        }
    }
}
