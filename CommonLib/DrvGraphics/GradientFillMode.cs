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
//                 кодировка : 21.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib.DrvGraphics
{
    /// <summary>
    /// ОО: Задает режим градиентной заливки 
    /// </summary>
    public enum GradientFillMode : uint
    {
        /// <summary>
        /// В этом режиме две конечные точки описывают прямоугольник. Прямоугольник определяется
        /// иметь постоянный цвет (заданный структурой TriVertex) для
        /// левый и правый края. GDI интерполирует цвет слева направо
        /// кромка и заполняет интерьер
        /// </summary>
        GRADIENT_FILL_RECT_H = 0,
        /// <summary>
        /// В этом режиме две конечные точки описывают прямоугольник. Прямоугольник
        /// определено, чтобы иметь постоянный цвет (заданный структурой TriVertex)
        /// для верхнего и нижнего краев. GDI интерполирует цвет сверху
        /// к нижнему краю и заполняет интерьер
        /// </summary>
        GRADIENT_FILL_RECT_V = 1,
        /// <summary>
        /// В этом режиме массив структур TriVertex передается в GDI
        /// вместе со списком индексов массива, описывающих отдельные треугольники.
        /// GDI выполняет линейную интерполяцию между вершинами треугольника и заливками
        /// интерьер. Отрисовка выполняется напрямую в режимах 24 и 32 бит на пиксель.
        /// Дизеринг выполняется в режимах 16-, 8-, 4- и 1-bpp
        /// </summary>
        GRADIENT_FILL_TRIANGLE = 2,
        OP_FLAG = 0xff
    }
}
