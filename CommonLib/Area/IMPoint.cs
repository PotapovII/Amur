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
    using System.Collections.Generic;
    /// <summary>
    /// Статус фигуры
    /// </summary>
    public enum FigureStatus
    {
        /// <summary>
        /// выделенная фигура
        /// </summary>
        SelectedShape,
        /// <summary>
        /// не выделенная фигура
        /// </summary>
        UnselectedShape,
        /// <summary>
        /// спрятанная фигура
        /// </summary>
        HiddenFigure
    }
    /// <summary>
    /// узел фигуры
    /// </summary>
    public interface IMPoint : IHPoint
    {
        /// <summary>
        /// Наоменование точек
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Статус фигуры
        /// </summary>
        FigureStatus Status { get; set; }
        /// <summary>
        /// Координаты точки
        /// </summary>
        IHPoint Point { get; set; }
        /// <summary>
        /// Связь с фигурами
        /// </summary>
        List<IMFigura> FigLink { get; }
    }
}
