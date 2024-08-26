
namespace TriangleNet.Rendering.GDI.Native
{
    using System;

    /// <summary>
    /// Specifies gradient fill mode
    /// Задает режим градиентной заливки
    /// </summary>
    [Flags]
    internal enum GradientFillMode : uint
    {
        /// <summary>
        /// In this mode, two endpoints describe a rectangle. The rectangle is defined
        /// to have a constant color (specified by the TRIVERTEX structure) for the
        /// left and right edges. GDI interpolates the color from the left to right
        /// edge and fills the interior
        /// В этом режиме две конечные точки описывают прямоугольник. Прямоугольник определяется
        /// иметь постоянный цвет (заданный структурой TRIVERTEX) для
        /// левый и правый края. GDI интерполирует цвет слева направо
        /// кромка и заполняет интерьер
        /// </summary>
        GRADIENT_FILL_RECT_H = 0,
        /// <summary>
        /// In this mode, two endpoints describe a rectangle. The rectangle is
        /// defined to have a constant color (specified by the TRIVERTEX structure)
        /// for the top and bottom edges. GDI interpolates the color from the top
        /// to bottom edge and fills the interior
        /// В этом режиме две конечные точки описывают прямоугольник. Прямоугольник
        /// определено, чтобы иметь постоянный цвет (заданный структурой TRIVERTEX)
        /// для верхнего и нижнего краев. GDI интерполирует цвет сверху
        /// к нижнему краю и заполняет интерьер
        /// </summary>
        GRADIENT_FILL_RECT_V = 1,
        /// <summary>
        /// In this mode, an array of TRIVERTEX structures is passed to GDI
        /// along with a list of array indexes that describe separate triangles.
        /// GDI performs linear interpolation between triangle vertices and fills
        /// the interior. Drawing is done directly in 24- and 32-bpp modes.
        /// Dithering is performed in 16-, 8-, 4-, and 1-bpp mode
        /// В этом режиме массив структур TRIVERTEX передается в GDI
        /// вместе со списком индексов массива, описывающих отдельные треугольники.
        /// GDI выполняет линейную интерполяцию между вершинами треугольника и заливками
        /// интерьер. Отрисовка выполняется напрямую в режимах 24 и 32 бит на пиксель.
        /// Дизеринг выполняется в режимах 16-, 8-, 4- и 1-bpp
        /// </summary>
        GRADIENT_FILL_TRIANGLE = 2
    }
}
