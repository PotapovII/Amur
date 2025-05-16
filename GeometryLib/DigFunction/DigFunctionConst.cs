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
    using MemLogLib;
    using System;
    using System.IO;

    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class DigFunctionConst : IDigFunction
    {
        /// <summary>
        /// Значение
        /// </summary>
        protected double Value;
        /// <summary>
        /// Гладкость функции
        /// </summary>
        public SmoothnessFunction Smoothness { get; set; } 
             = SmoothnessFunction.linear;
        /// <summary>
        /// Функция заданна одной точкой
        /// </summary>
        public bool isConstant { get; } = true;
        /// <summary>
        /// Функция является параметрической
        /// </summary>
        public bool isParametric { get; set; } = true;
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get { return name; } }
        protected string name;
        /// <summary>
        /// Минимальное значение аргумента
        /// </summary>
        public double Xmin { get => double.MinValue; } 
        /// <summary>
        /// Минимальное значение функции
        /// </summary>
        public double Ymin { get=>Value; }
        /// <summary>
        /// Максимальное значение аргумента
        /// </summary>
        public double Xmax { get=> double.MaxValue; }
        /// <summary>
        /// Максимальное значение функции
        /// </summary>
        public double Ymax { get => Value; }
        /// <summary>
        /// Длина области определения
        /// </summary>
        public double Length { get => 1; }
        /// <summary>
        /// Высота области определения
        /// </summary>
        public double Height { get => Value; }
        /// <summary>
        /// загрузка начальной геометрии из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            string line = file.ReadLine();
            string name = line;
            for (line = file.ReadLine(); line != null; line = file.ReadLine())
            {
                if (line.Trim() == "#")
                    break;
                string[] sline = line.Split(' ');
                Value = double.Parse(sline[0].Trim(), MEM.formatter);
            }
        }
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        public void Save(StreamWriter file)
        {
            file.WriteLine(name);
            file.WriteLine("{0}", Value);
            file.WriteLine("#");
        }

        /// <summary>
        /// Формирование данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void SetFunctionData(double[] x, double[] y, string name = "без названия")
        {
            Value = y[0];
            this.name = name;
        }
        /// <summary>
        /// Получить данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void GetFunctionData(ref string name, ref double[] x, ref double[] y)
        {
            name = this.name;
            x = new double[1] { 0 };
            y = new double[1] { Value };
        }
        /// <summary>
        /// Добавление точки
        /// </summary>
        public void Add(double x, double y) 
        { 
            throw new NotImplementedException();
        }
        /// <summary>
        /// Получение маркера границы для каждого узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="Mark"></param>
        public void GetMark(double[] x, ref int[] Mark)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Очистка
        /// </summary>
        public void Clear()
        {
            Value = 0;
        }
        /// <summary>
        /// Получить базу функции
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="N">количество</param>
        public void GetBase(ref double[][] fun, int N = 0)
        {
            throw new NotImplementedException();
        }

        public double FunctionValue(double x)
        {
            return Value;
        }
        public void GetFunctionData(ref double[] x, ref double[] y, int Count = 10, bool revers = false)
        {
            Count = 2;
            MEM.Alloc(Count, ref x);
            MEM.Alloc(Count, ref y);
            x[0] = double.MinValue;
            y[0] = Value;
            x[1] = double.MaxValue;
            y[1] = Value;
        }

        public DigFunctionConst(double Value, string name = "")
        {
            this.Value = Value;
            if (name == "")
                name = "Константа";
        }
        public DigFunctionConst(DigFunction f)
        {
            Value = f.FunctionValue(0);
            name = f.Name;
        }
    }
}