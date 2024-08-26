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
//                 кодировка : 01.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.DrvGraphics
{
    using System;
    using System.Drawing;
    using CommonLib.Geometry;
    /// <summary>
    /// Абстракция дравера отрисовки геметрических примитивив и текста 
    /// для реализации алгоритмов построения заливки, изолиний, векторных полей, КЭ сетки и т.д.
    /// </summary>
    public interface IDrvGraphic
    {
        /// <summary>
        /// Отрисовка линии
        /// </summary>
        void DrawLine(Pen pen, IFPoint pa, IFPoint pb);
        void DrawLine(Pen pen, PointF pa, PointF pb);
        /// <summary>
        /// Отрисовка эллипса
        /// </summary>
        void FillEllipse(SolidBrush brash, RectangleF rec);
        void FillEllipse(SolidBrush brash, float x, float y, float w, float h);
        /// <summary>
        /// Отрисовка строки
        /// </summary>
        void DrawString(String text, Font font, SolidBrush brushText, PointF point);
        void DrawString(String text, Font font, SolidBrush brushText, IFPoint point);
        /// <резюме>
        /// Функция GradientFill заполняет прямоугольные и треугольные структуры
        /// </summary>
        /// <param name = "pVertex"> Массив структур TriVertex, каждая из которых определяет вершину треугольника </param>
        /// <param name = "pMesh"> Массив структур TriElement в режиме треугольника </param>
        /// <param name = "ulMode"> Определяет режим градиентной заливки </param>
        /// <returns> Если функция завершается успешно, возвращается значение true, false </returns>
        void GradientFill(TriVertex[] pVertex, TriElement[] pMesh, GradientFillMode ulMode);
        /// <summary>
        /// Отрисовка полигона по точкам
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="points"></param>
        void DrawPolygon(Pen pen, PointF[] points);
        /// <summary>
        /// Отрисовка треугольника по точкам
        /// </summary>
        void DrawTriangle(Pen pen, PointF[] points);
        void DrawTriangle(Pen pen, IFPoint[] points);
    }

}
