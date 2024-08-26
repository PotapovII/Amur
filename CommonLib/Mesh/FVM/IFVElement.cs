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
    /// КО элемент
    /// </summary>
    public interface IFVElement
    {
        /// <summary>
        /// Тип функции формы основного КЭ 
        /// </summary>
        TypeFunForm TFunForm { get; set; }
        /// <summary>
        /// грани КО
        /// </summary>
        IFVFacet[] Facets { get; set; }
        /// <summary>
        /// ближайшие КО
        /// </summary>
        IFVElement[] NearestElements { get; set; }
        /// <summary>
        ///  Координаты вершин/от левый нижний против часовой
        /// </summary>
        HPoint[] Vertex { get; set; }
        /// <summary>
        ///  Узлы вершин треугольника
        /// </summary>
        int[] Nodes { get; set; }
        /// <summary>
        /// вектор  от центра КО до центров ближайших
        /// </summary>
        HPoint[] VecDistance { get; set; }
        /// <summary>
        /// Растояние от центра КО до центров ближайших
        /// </summary>
        double[] Distance { get; set; }
        /// <summary>
        /// вектор от центра до центра граней
        /// </summary>
        HPoint[] VecFacetsDistance { get; set; }
        /// <summary>
        /// вектор от центра соседа до центра граней
        /// </summary>
        HPoint[] VecNearFacetsDistance { get; set; }
        /// <summary>
        /// Длина вектор от центра соседа до центра граней
        /// </summary>
        double[] NearFacetsDistance { get; set; }
        /// <summary>
        ///  Коэффицент КО схемы 0-k грани для продольной диффузии
        /// -  длина k - го ребра / расстояние между центрам КО и центром i - го КО
        /// </summary>
        double[] Ak { get; set; }
        /// <summary>
        /// Сумма всех Ak
        /// </summary>
        double Ap { get; set; }
        /// <summary>
        ///  Коэффицент КО схемы 0-k грани для поперечной диффузии
        /// </summary>
        double[] ACrossk { get; set; }
        /// <summary>
        /// ценр КО
        /// </summary>
        HPoint Centroid { get; set; }
        /// <summary>
        /// Номер КО
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// Площадь КО
        /// </summary>
        double Volume { get; set; }
        /// <summary>
        /// Возвращает треугольные элементы в формате TriElement
        /// </summary>
        /// <returns></returns>
        TriElement[] Nods();
        /// <summary>
        /// Возврашает индекс грани в контрольном объеме по объекту грани
        /// и -1 если грань не пренадлежит КО
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        int GetFacetIndex(IFVFacet f);
        /// <summary>
        /// Получить индекс грани в контольном объеме
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        int FaceLocalId(IFVFacet facet);
        /// <summary>
        /// Расчет постоянных параметров контрольного объема
        /// </summary>
        void InitElement();
    }
}
