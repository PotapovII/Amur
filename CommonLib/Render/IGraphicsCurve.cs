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
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               перекодировка : 03 01 2021 Потапов И.И.
//---------------------------------------------------------------------------
//               перекодировка : 29 09 2022 Потапов И.И.
//               добавлено поле типа кривой TypeGraphicsCurve
//---------------------------------------------------------------------------
namespace CommonLib
{
    using CommonLib.IO;
    using System;
    /// <summary>
    /// Тип кривой
    /// </summary>
    [Serializable]
    public enum TypeGraphicsCurve
    {
        /// <summary>
        /// Все кривые
        /// </summary>
        AllCurve = 0,
        /// <summary>
        /// Пространственная - параметрическая кривая  x=x(xi), y=y(xi)
        /// </summary>
        AreaCurve = 1,
        /// <summary>
        /// Эволюционная кривая 0 <= t < inf   f=f(t)
        /// </summary>
        TimeCurve = 2,
        /// <summary>
        /// Функция  - inf < x < inf  y=y(x)
        /// </summary>
        FuncionCurve = 3
    }
    /// <summary>
    /// ОО: данные о кривой для отрисовки
    /// </summary>
    public interface IGraphicsCurve
    {
        /// <summary>
        /// Имя кривой
        /// </summary>
        string CurveName { get; set; }
        /// <summary>
        /// Тип кривой
        /// </summary>
        TypeGraphicsCurve TGraphicsCurve { get; set; }
        /// <summary>
        /// Масштаб графика по Х
        /// </summary>
        double ScaleX { get; set; }
        /// <summary>
        /// Масштаб графика по Y
        /// </summary>
        double ScaleY { get; set; }
        /// <summary>
        /// количество точек кривой
        /// </summary>
        int Count { get; }
        /// <summary>
        /// получить точку кривой в масштабе
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        void GetPoint(int index, ref double x, ref double y);
        /// <summary>
        /// добавить точку в кривую
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="y">координата Y</param>
        void Add(double x, double y);
        /// <summary>
        /// очистка точек кривой из списка
        /// </summary>
        void Clear();
        /// <summary>
        /// Сортировка точек по аргументу
        /// </summary>
        void SortX();
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        void SortY();
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        double MaxY();
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        double MinY();
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        double MaxX();
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        double MinX();
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        IOFormater<IGraphicsCurve> GetFormater();
    }
}
