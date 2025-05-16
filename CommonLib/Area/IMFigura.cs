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
//                 кодировка : 25.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.Areas
{
    using CommonLib.Geometry;
    using GeometryLib;
    using System.Collections.Generic;
    /// <summary>
    /// Тип фигуры
    /// </summary>
    public enum FigureType
    {
        /// <summary>
        /// основной контур
        /// </summary>
        FigureContur = 0,
        /// <summary>
        /// дырка в основном контуре
        /// </summary>
        FigureHole = 1,
        /// <summary>
        /// подобласть в основном контуре
        /// </summary>
        FigureSubArea = 2
    }
    /// <summary>
    /// Фигура - контур
    /// </summary>
    public interface IMFigura
    { 
        /// <summary>
        /// Номер фигуры
        /// </summary>
        int FID { get; set; }
        /// <summary>
        /// Количество точек
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Наименование точек
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Статус фигуры
        /// </summary>
        FigureStatus Status { get; set; }
        /// <summary>
        /// Тип фигуры
        /// </summary>
        FigureType FType { get; set; }
        #region Атрибуты фигуры
        /// <summary>
        /// Свойства в узлах (состояние контексное) от задачи
        /// </summary>
        // double[] Attributes { get; set; }
        /// <summary>
        /// Толщина льда
        /// </summary>
        double Ice { get; set; }
        /// <summary>
        /// шероховатость дна
        /// </summary>
        double ks { get; set; }
        #endregion
        /// <summary>
        /// Точки контура
        /// </summary>
        List<IMPoint> Points { get; }
        /// <summary>
        /// Список сегментов фигуры
        /// </summary>
        List<IMSegment> Segments { get; }
        /// <summary>
        /// Изменение координат
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="p"></param>
        void SetPoint(int idx, IMPoint p);
        /// <summary>
        /// Получить узел
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        IMPoint GetPoint(int idx);
        /// <summary>
        /// Получить сегмент
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        IMSegment GetSegment(int idx);
        /// <summary>
        /// Получить список индексов сегментов
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        int[] GetSegmentIndexs();
        /// Очистить контур
        /// </summary>
        void Clear();
        /// <summary>
        /// Добавить точку в контур
        /// </summary>
        /// <param name="point"></param>
        void Add(IHPoint point);
        /// <summary>
        /// Добавить точку в контур
        /// </summary>
        /// <param name="point"></param>
        void Add(IMPoint point);
        /// <summary>
        /// Убрать точку из контур
        /// </summary>
        /// <param name="point"></param>
        void Remove(IMPoint point);
        /// <summary>
        /// Принадлежит ли точка полигону фигуры
        /// </summary>
        bool Contains(IHPoint point);
        /// <summary>
        /// Принадлежит ли точка полигону фигуры
        /// </summary>
        bool Contains(double x, double y);
    }
}
