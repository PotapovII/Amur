#region License
/*
Copyright (c) 2019 - 2025  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.07.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.DigTimeFunction
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib.Function;
    /// <summary>
    /// ОО: Данные для функциональных зависимостей
    /// временная функция
    /// </summary>
    public class DigTimeFunction: IDigTimeFunction
    {
        List<(DateTime,double)> values = new List<(DateTime,double)> ();
        /// <summary>
        /// Имя функции/данных
        /// </summary>
        public string Name { get => name; }
        protected string name;
        public DigTimeFunction(string name)
        {
            this.name = name;
        }
        public DigTimeFunction(DigTimeFunction dtf)
        {
            this.name = dtf.Name;
            values.AddRange(dtf.values);
        }
        public DigTimeFunction(string Name, DateTime[] a, double[] b)
        {
            this.name = Name;
            for(int i = 0; i < a.Length; i++) 
                values.Add((a[i], b[i]));
        }
        /// <summary>
        /// загрузка начальной геометрии из файла
        /// </summary>
        /// <param name="file"></param>
        public void Load(StreamReader file)
        {
            values.Clear();
            string line = file.ReadLine();
            this.name = line;
            for (line = file.ReadLine(); line != null; line = file.ReadLine())
            {
                if (line.Trim() == "#")
                    break;
                string[] sline = line.Split('\"', ' ', '\'');
                DateTime x = DateTime.Parse(sline[0].Trim());
                double y = double.Parse(sline[1].Trim(), MEM.formatter);
                values.Add((x, y));
            }
        }
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        public void Save(StreamWriter file)
        {
            file.WriteLine(name);
            for (int i = 0; i < values.Count; i++)
                file.WriteLine("{0} {1}", values[i].Item1, values[i].Item2);
            file.WriteLine("#");
        }
        /// <summary>
        /// Получение интерполированного (линейно) не сглаженного значения данных 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double FunctionValue(DateTime x)
        {
            // Находим ближайшие даты в массиве
            int lowerIndex = -1;
            int upperIndex = -1;
            for (int i = 0; i < values.Count - 1; i++)
            {
                if (values[i].Item1 <= x && values[i + 1].Item1 >= x)
                {
                    lowerIndex = i;
                    upperIndex = i + 1;
                    break; // Нашли подходящий интервал, выходим из цикла.
                }
            }
            // Если дата вне диапазона массивов
            if (lowerIndex == -1)
            {
                // Можно вернуть значение крайнего элемента или выбросить исключение
                if (x < values[0].Item1)
                {
                    return values[0].Item2;
                }
                else
                {
                    // Возвращаем последний элемент массива.  
                    return values[values.Count - 1].Item2;
                }
            }
            // Преобразуем DateTime в числовой формат (дни с 0001-01-01)
            double x1 = values[lowerIndex].Item1.ToOADate(); 
            double x2 = values[upperIndex].Item1.ToOADate();
            double y1 = values[lowerIndex].Item2;
            double y2 = values[upperIndex].Item2;
            double targetX = x.ToOADate();
            // Формула линейной интерполяции:  
            double Y = y1 + (targetX - x1) * (y2 - y1) / (x2 - x1);
            return Y;
        }
        /// <summary>
        /// Создание цифровой функции с аргументом 
        /// в секундах flag = 0,  минутах flag = 1, часах flag = 2, днях flag = 3
        /// </summary>
        /// <returns></returns>
        /// <param name="flag"></param>
        /// <returns></returns>
        public IDigFunction CreateDigFunction(int flag = 0)
        {
            double[] arg = (values.Select(x => x.Item1.ToOADate())).ToArray();
            double[] vals = (values.Select(x => x.Item2)).ToArray();
            IDigFunction fun = new DigFunction(arg, vals, name);
            return fun;
        }
    }
}
