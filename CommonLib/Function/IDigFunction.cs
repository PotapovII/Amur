#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

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
//                 кодировка : 17.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.Function
{
    using System;
    using System.IO;
    using System.ComponentModel;
    /// <summary>
    /// Гладкость функции
    /// </summary>
    [Serializable]
    public enum SmoothnessFunction
    {
        /// <summary>
        /// Линейная интерполяция
        /// </summary>
        [Description("Линейная интерполяция")]
        linear = 0,
        /// <summary>
        /// Сплайн интерполяция
        /// </summary>
        [Description("Сплайн интерполяция")]
        spline,
        /// <summary>
        /// Полином 1 порядка
        /// </summary>
        [Description("Сплайн интерполяция")]
        polynom1,
        /// <summary>
        /// Полином 2 порядка
        /// </summary>
        [Description("Сплайн интерполяция")]
        polynom2,
        /// <summary>
        /// Полином 3 порядка
        /// </summary>
        [Description("Сплайн интерполяция")]
        polynom3
    }
    /// <summary>
    /// ОО: Данные для функциональных зависимостей
    /// Цифровая функция
    /// </summary>
    public interface IDigFunction : IFunction1D
    {
        /// <summary>
        /// Гладкость функции
        /// </summary>
        SmoothnessFunction Smoothness { get; set; }
        /// <summary>
        /// Функция заданна одной точкой
        /// </summary>
        bool isConstant { get; }
        /// <summary>
        /// Функция является параметрической
        /// </summary>
        bool isParametric { get; set; }
        /// <summary>
        /// Минимальное значение аргумента
        /// </summary>
        double Xmin { get; }
        /// <summary>
        /// Минимальное значение функции
        /// </summary>
        double Ymin { get; }
        /// <summary>
        /// Максимальное значение аргумента
        /// </summary>
        double Xmax { get; }
        /// <summary>
        /// Максимальное значение функции
        /// </summary>
        double Ymax { get; }
        /// <summary>
        /// Длина области определения
        /// </summary>
        double Length { get; }
        /// <summary>
        /// Высота области определения
        /// </summary>
        double Height { get; }
        /// <summary>
        /// Загрузка данных из файлов
        /// </summary>
        /// <param name="file"></param>
        void Load(StreamReader file);
        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        /// <param name="file"></param>
        void Save(StreamWriter file);
        /// <summary>
        /// Формирование данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        void SetFunctionData(double[] x, double[] y, string name = "без названия");
        /// <summary>
        /// Получить данных основы
        /// </summary>
        /// <param name="x">аргумент</param>
        /// <param name="y">функция</param>
        /// <param name="value">контекстный параметр</param>
        void GetFunctionData(ref string name, ref double[] x,ref double[] y);
        /// <summary>
        /// Добавление точки
        /// </summary>
        void Add(double x, double y);
        /// <summary>
        /// Получение маркера границы для каждого узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="Mark"></param>
        void GetMark(double[] x, ref int[] Mark);
        /// <summary>
        /// Очистка
        /// </summary>
        void Clear();
        /// <summary>
        /// Получить базу функции
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="N">количество</param>
        void GetBase(ref double[][] fun, int N = 0);
    }
}
