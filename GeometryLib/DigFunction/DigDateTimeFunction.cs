//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 17.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using CommonLib.Function;
    using CommonLib.Geometry;
    using MemLogLib;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class DigDateTimeFunction : IDigFunction
    {
        /// <summary>
        /// Количество опорных точек функции
        /// </summary>
        public int Count { get => Values.Length; }
        /// <summary>
        /// Гладкость функции
        /// </summary>
        public SmoothnessFunction Smoothness
        { get => SmoothnessFunction.linear; set => sm = value; }
        protected SmoothnessFunction sm;
        /// <summary>
        /// Функция заданна одной точкой
        /// </summary>
        public bool isConstant => false;
        /// <summary>
        /// Функция является параметрической
        /// </summary>
        /// <summary>
        /// Функция является параметрической
        /// </summary>
        public bool isParametric { get => false; set => ip =value; }
        protected bool ip;
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get { return name; } }
        protected string name;
        /// <summary>
        /// Начальное значение аргумента
        /// </summary>
        public double Xmin { get { return 0; } }
        /// <summary>
        /// Начальное значение функции
        /// </summary>
        public double Ymin { get { return Values.ToArray().Min(); } }
        /// <summary>
        /// Начальное значение аргумента
        /// </summary>
        public double Xmax { get { return (Dates[Dates.Length-1] - Dates[0]).TotalSeconds; } }
        /// <summary>
        /// Начальное значение функции
        /// </summary>
        public double Ymax { get { return Values.ToArray().Max(); } }
        /// <summary>
        /// Длина области определения
        /// </summary>
        public double Length { get { return Xmax - Xmin; } }
        /// <summary>
        /// Высота области определения
        /// </summary>
        public double Height { get { return Ymax - Ymin; } }

        protected DateTime[] Dates = null;
        protected double[] Values = null;
        /// <summary>
        /// загрузка начальной геометрии из файла
        /// </summary>
        /// <param name="file"></param>
        public  void Load(StreamReader file)
        {
            string line = file.ReadLine();
            name = line;
            // Считываем количество дат
            int count = int.Parse(file.ReadLine());
            // Создаем массивы для хранения дат и значений
            Dates = new DateTime[count];
            Values = new double[count];
            // Считываем даты и значения из потока
            for (int i = 0; i < count; i++)
            {
                string[] parts = file.ReadLine().Split(' ');
                Dates[i] = DateTime.Parse(parts[0]);
                Values[i] = double.Parse(parts[1]);
            }
        }
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        public  void Save(StreamWriter file) 
        {
            file.WriteLine(name);
            // Записываем количество дат
            file.WriteLine(Dates.Length);
            // Записываем даты и значения в поток
            for (int i = 0; i < Dates.Length; i++)
                file.WriteLine($"{Dates[i]} {Values[i]}");
        }
        public DigDateTimeFunction(){}

        public DigDateTimeFunction(DateTime[] dates, double[] values, string name = "")
        {
            SetDateTimeFunction(dates, values, name); 
        }
        public void SetDateTimeFunction(DateTime[] dates, double[] values, string name) 
        {
            this.name = name;
            if (dates == null || values == null)
                throw new ArgumentNullException("Массивы дат и значений не могут быть null.");
            if (dates.Length != values.Length)
                throw new ArgumentException("Длина массивов дат и значений должна совпадать.");
            Dates = dates;
            Values = values;
        }

        public DigDateTimeFunction(DigDateTimeFunction p) 
        {
            name = p.Name;
            Dates = p.Dates;
            Values = p.Values;
        }
        public DigDateTimeFunction(DataRow t,double V, string name = "постоянная функция")
        {
            this.name = name;   
        }
        /// <summary>
        /// Формирование данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void SetFunctionData(double[] x, double[] y, string name = "без названия"){}

        /// <summary>
        /// Вычисляет значение функции в заданной секунде от начальной даты массива аргументов, используя линейную интерполяцию.
        /// </summary>
        /// <param name="seconds">Секунды от начальной даты.</param>
        /// <returns>Значение функции в заданной секунде.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если заданная секунда выходит за пределы диапазона дат.</exception>
        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double FunctionValue(double seconds)
        {
            if (Dates.Length == 0) 
                throw new ArgumentException("Массив дат пуст.");
            // Преобразуем секунды в DateTime
            DateTime targetDate = Dates[0].AddSeconds(seconds);
            // Находим ближайшие даты слева и справа от targetDate
            int leftIndex = -1;
            int rightIndex = -1;
            for (int i = 0; i < Dates.Length - 1; i++)
            {
                if (Dates[i] <= targetDate && Dates[i + 1] >= targetDate)
                {
                    leftIndex = i;
                    rightIndex = i + 1;
                    break;
                }
            }
            // Если targetDate меньше самой левой даты
            if (leftIndex == -1 && rightIndex == -1)
            {
                if (targetDate < Dates[0])
                {
                    // Возвращаем значение для первой точки
                    return Values[0]; 
                }
                else if (targetDate > Dates[Dates.Length-1])
                {
                    //Возвращаем значение для последней точки
                    return Values[Values.Length-1]; 
                }
            }
            // Если targetDate находится между двумя точками, выполняем интерполяцию
            if (leftIndex != -1 && rightIndex != -1)
            {
                double x = (targetDate - Dates[leftIndex]).TotalSeconds;
                double dx = (Dates[rightIndex] - Dates[leftIndex]).TotalSeconds;
                return Values[leftIndex] + (Values[rightIndex] - Values[leftIndex]) * (x / dx);
            }
            // Если targetDate совпадает с одной из точек, возвращаем соответствующее значение
            if (leftIndex != -1) 
                return Values[leftIndex];
            if (rightIndex != -1) 
                return Values[rightIndex];
            // Возвращаем 0 в случае ошибки (не должно произойти при корректных данных)
            throw new ArgumentException("Массив дат пуст или неотсортирован.");
        }
        /// <summary>
        /// Возвращает массив дней и соответствующих значений для заданного интервала дат.
        /// </summary>
        /// <param name="start">Начальная дата.</param>
        /// <param name="end">Конечная дата.</param>
        /// <returns>Кортеж, содержащий массивы дней (double) и значений (double).</returns>
        public (double[] Days, double[] Values) GetValuesForInterval(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentException("Начальная дата должна быть раньше конечной даты.");

            // Определяем количество дней в интервале
            int dayCount = (end - start).Days + 1;

            // Создаем массивы для хранения дней и значений
            double[] days = new double[dayCount];
            double[] values = new double[dayCount];

            // Заполняем массивы данными
            for (int i = 0; i < dayCount; i++)
            {
                DateTime currentDate = start.AddDays(i);
                days[i] = (currentDate - Dates[0]).TotalDays; // Дни от начальной даты исходного массива
                values[i] = FunctionValue((currentDate - Dates[0]).TotalSeconds);
            }
            return (days, values);
        }
        /// <summary>
        /// Получить данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void GetFunctionData(ref string name, ref double[] days, ref double[] values)
        {
            name = this.name;            
            int dayCount = (Dates[Dates.Length] - Dates[0]).Days + 1;
            MEM.Alloc(dayCount, ref days);
            MEM.Alloc(dayCount, ref values);
            // Заполняем массивы данными
            for (int i = 0; i < dayCount; i++)
            {
                DateTime currentDate = Dates[i];
                days[i] = (currentDate - Dates[0]).TotalDays; // Дни от начальной даты исходного массива
                values[i] = FunctionValue((currentDate - Dates[0]).TotalSeconds);
            }
        }
        /// <summary>
        /// Получаем линейную интерполяцию функции 
        /// в области определенной аргументом для Count точек
        /// </summary>
        /// <param name="x">координаты Х</param>
        /// <param name="y">координаты У</param>
        /// <param name="Count">количество узлов для русла</param>
        public void GetFunctionData(ref double[] x, ref double[] y,
                                    int Count = 10, bool revers = false)
        {
            //MEM.Alloc(Count, ref x);
            //MEM.Alloc(Count, ref y);
            //double L = arg[arg.Length - 1] - arg[0];
            //int NN = Count - 1;
            //x[0] = arg[0];
            //y[0] = fun[0];
            //if (Count == 1) return;
            //x[NN] = arg[arg.Length - 1];
            //y[NN] = fun[arg.Length - 1];
            //if (Count == 2) return;
            //double dx = L / NN;
            //int i = 0;
            //int start;
            //for (int ii = 1; ii < NN; ii++)
            //{
            //    double xx = ii * dx;
            //    start = i;
            //    for (i = start; i < arg.Length; i++)
            //        if (arg[i] >= xx)
            //            break;
            //    double N1 = (xx - arg[i - 1]) / (arg[i] - arg[i - 1]);
            //    x[ii] = xx;
            //    y[ii] = (1 - N1) * fun[i - 1] + N1 * fun[i];
            //}
            //if (revers == true)
            //    MEM.Reverse(ref y);
        }
        /// <summary>
        /// Добавление точки
        /// </summary>
        public void Add(double x, double y) { }
        /// <summary>
        /// Получение маркера границы для каждого узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="Mark"></param>
        public void GetMark(double[] x, ref int[] Mark) { }
        /// <summary>
        /// Очистка
        /// </summary>
        public void Clear() { }
        /// <summary>
        /// Получить базу функции
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="N">количество</param>
        public void GetBase(ref double[][] fun, int N = 0) { }
        /// <summary>
        /// Аргумент хх содержится в строгом диапазоне функции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool Contains(double seconds)
        {
            // Преобразуем секунды в DateTime
            DateTime targetDate = Dates[0].AddSeconds(seconds);
            if (Dates[0] <= targetDate && Dates[Dates.Length - 1] >= targetDate)
                return true;
            else 
                return false;
        }
    }
}