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
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.08.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using CommonLib.Geometry;
    /// <summary>
    /// Класс ребро КО (геометрические параметры ребра и ссылки)
    /// </summary>
    public interface IFVFacet : IAFacet
    {
        /// <summary>
        /// Владелец
        /// </summary>
        IFVElement Owner { get; set; }
        /// <summary>
        /// Совладелец
        /// </summary>
        IFVElement NBElem { get; set; }
        /// <summary>
        /// Координаты грани
        /// </summary>
        HPoint[] Vertex { get; set; }
        /// <summary>
        /// Вектор грани
        /// </summary>
        HPoint FVertex { get; set; }
        /// <summary>
        /// центр грани
        /// </summary>
        HPoint Centroid { get; set; }
        /// <summary>
        /// нормаль к грани
        /// </summary>
        HPoint Normal { get; set; }
        /// <summary>
        /// Вектор нормальный к грани
        /// </summary>
        HPoint FaceNormal { get; set; }
        /// <summary>
        /// Коэффициент интерполяции для сопряженных на грань узлов
        /// </summary>
        double Alpha { get; set; }
        /// <summary>
        /// длина грани
        /// </summary>
        double Length { get; set; }
        /// <summary>
        /// Узлы грани
        /// </summary>
        /// <returns></returns>
        TwoElement Nods();
        /// <summary>
        /// Вычисление характеристик ребра
        /// </summary>
        void InitFacet();
    }
}
