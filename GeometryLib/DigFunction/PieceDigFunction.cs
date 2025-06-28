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
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// ОО: Класс - для загрузки начальной геометрии русла в створе реки
    /// </summary>
    [Serializable]
    public class PieceDigFunction : IDigFunction
    {
        IDigFunction[] digFunctions = null;
        /// <summary>
        /// загрузка начальной геометрии из файла
        /// </summary>
        /// <param name="file"></param>
        public  void Load(StreamReader file)
        {
            List<IDigFunction> list = new List<IDigFunction>();
            IDigFunction fun = null;
            do
            {
                fun = new DigFunction();
                fun.Load(file);
                if (fun.Name != "")
                    list.Add(fun);
            }
            while (fun.Name != "");
            if(list.Count > 0)
                digFunctions = list.ToArray();
        }
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        public  void Save(StreamWriter file)
        {
            if(digFunctions!=null)
            {
                for (int i = 0; i < digFunctions.Length; i++)
                    digFunctions[i].Save(file);
            }
        }
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get { return name; } }
        protected string name;

        public PieceDigFunction(string name = "кусочная функция")
        { }
        public PieceDigFunction(IDigFunction[] digFunctions)
        {
            this.digFunctions = digFunctions;
        }
        public PieceDigFunction(PieceDigFunction p) 
        {
            this.digFunctions = new IDigFunction[p.digFunctions.Length]; ;
            for (int i = 0; i < p.digFunctions.Length; i++)
                digFunctions[i] = p.digFunctions[i];
        }

        /// <summary>
        /// Гладкость функции
        /// </summary>
        public SmoothnessFunction Smoothness
        {
            get => SmoothnessFunction.Piece;
            set => value = SmoothnessFunction.Piece;
        }
        /// <summary>
        /// Функция заданна одной точкой
        /// </summary>
        public bool isConstant { get => false; set => value = false; }
        /// <summary>
        /// Функция является параметрической
        /// </summary>
        public bool isParametric { get => false; set => value = false; }
        /// <summary>
        /// Минимальное значение аргумента
        /// </summary>
        public double Xmin => digFunctions.Min(x=>x.Xmin);
        /// <summary>
        /// Минимальное значение функции
        /// </summary>
        public double Ymin => digFunctions.Min(x => x.Ymin);
        /// <summary>
        /// Максимальное значение аргумента
        /// </summary>
        public double Xmax => digFunctions.Max(x => x.Xmax);
        /// <summary>
        /// Максимальное значение функции
        /// </summary>
        public double Ymax => digFunctions.Max(x => x.Ymax);
        /// <summary>
        /// Длина области определения
        /// </summary>
        public double Length => digFunctions.Sum(x=>x.Length);
        /// <summary>
        /// Высота области определения
        /// </summary>
        public double Height => digFunctions.Sum(x => x.Height);
        /// <summary>
        /// Формирование данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void SetFunctionData(double[] x, double[] y, string name = "без названия")
        {
            throw new NotImplementedException("Не поддерживается");
        }
        /// <summary>
        /// Получить данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        public void GetFunctionData(ref string name, ref double[] x, ref double[] y)
        {
            throw new NotImplementedException("Не поддерживается");
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
            double dx = Length / (Count - 1);
            double x0 = digFunctions[0].Xmin;
            for (int i = 0; i < Count; i++)
            {
                double xi = x0 + i * dx;
                x[i] = xi;
                y[i] = FunctionValue(xi);
            }
        }
        /// <summary>
        /// Добавление точки
        /// </summary>
        public void Add(double x, double y)
        {
            throw new NotImplementedException("Не поддерживается");
        }

        /// <summary>
        /// Получение маркера границы для каждого узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="Mark"></param>
        public void GetMark(double[] x, ref int[] Mark)
        {
            throw new NotImplementedException("Не поддерживается");
        }
        /// <summary>
        /// Очистка
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < digFunctions.Length; i++)
                digFunctions[i].Clear();
        }
        /// <summary>
        /// Получить базу функции
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="N">количество</param>
        public void GetBase(ref double[][] fun, int N = 0)
        {
            throw new NotImplementedException("Не поддерживается");
        }
        /// <summary>
        /// Получение не сглаженного значения данных по линейной интерполяции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double FunctionValue(double xx)
        {
            double f = 0;
            if(digFunctions != null)
            {
                for (int i = 0; i < digFunctions.Length; i++)
                {
                    if (digFunctions[i].Contains(xx) == true)
                    {
                        if (xx > 0.2)
                            xx = xx;
                        f = digFunctions[i].FunctionValue(xx);
                        return f;
                    }
                }
                digFunctions[digFunctions.Length-1].FunctionValue(xx);
            }
            return f;
        
        }
        /// <summary>
        /// Аргумент хх содержится в строгом диапазоне функции
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool Contains(double x) => true;
    }
}