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
//                 кодировка : 10.10.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    using GeometryLib;
    using CommonLib.Geometry;
    /// <summary>
    /// ОО: Отсылка данных для отрисовки
    /// </summary>
    /// <param name="a"></param>
    [Serializable]
    public delegate void SendParamCloud(IClouds a);
    /// <summary>
    /// ОО: ICloud - множество точек которое может содержать контурные точки
    /// </summary>
    public interface IClouds
    {
        /// <summary>
        /// имена атрибутов
        /// </summary>
        string[] AttributNames { get; }
        /// <summary>
        /// Количество узлов
        /// </summary>
        int CountKnots { get; }
        /// <summary>
        /// Координаты X для узловых точек 
        /// </summary>
        /// <param name="dim">номер размерности</param>
        /// <returns></returns>
        double[] GetCoords(int dim);
        /// <summary>
        /// Получить облако узлов
        /// </summary>
        /// <returns></returns>
        IHPoint[] GetKnots();
        /// <summary>
        /// Получить поле атрибута
        /// </summary>
        /// <param name="indexAttribut"></param>
        /// <returns></returns>
        IField GetPole(int indexAttribut);
        /// <summary>
        /// Добавить узел в облако
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mark"></param>
        /// <param name="Atributs"></param>
        void AddNode(double x, double y, int mark, double[] Attributes = null);
        /// <summary>
        /// Добавить узел в облако
        /// </summary>
        void AddNode(IHPoint node);
        /// <summary>
        /// изменить маркер узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mark"></param>
        /// <param name="Atributs"></param>
        void ReMarkNode(double x, double y, int mark);
        /// <summary>
        /// Диапазон координат для узлов сетки
        /// </summary>
        void MinMax(int dim, ref double min, ref double max);
        /// <summary>
        /// Массив маркеров вершин 
        /// </summary>
        int[] GetMarkKnots();
    }
}
