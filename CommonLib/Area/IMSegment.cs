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
    /// <summary>
    /// Сегмент линия между двумя узлами - подчиненный объет в фигуре связанный с ней
    /// </summary>
    public interface IMSegment
    {
        /// <summary>
        /// Наоменование точек
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Статус фигуры
        /// </summary>
        FigureStatus Status { get; set; }
        /// <summary>
        /// Координаты точки начала сегмента
        /// </summary>
        IMPoint pointA { get; }
        /// <summary>
        /// Координаты точки конца сегмента
        /// </summary>
        IMPoint pointB { get; }
        /// <summary>
        /// номер узла A сегмента в фигуре
        /// </summary>
        int nodeA { get; }
        /// <summary>
        /// номер узла B сегмента в фигуре
        /// </summary>
        int nodeB { get; }
        /// <summary>
        /// индекс сегмента в фигуре
        /// </summary>
        int index { get; }
        /// <summary>
        /// Связь с фигурами
        /// </summary>
        IMFigura FiguraLink { get; }
        /// <summary>
        /// Количество узлов в полелинии
        /// </summary>
        int CountKnots { get; set; }
        /// <summary>
        /// Маркер сегмента
        /// </summary>
        int Marker { get; set; }
        /// <summary>
        /// Модификация сегмента, фигуры и вершин при приведение его в к состоянию split == 2
        /// </summary>
        void Split();
    }
}
