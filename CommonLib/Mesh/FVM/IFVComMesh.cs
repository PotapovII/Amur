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
    using CommonLib;
    /// <summary>
    /// Контрольно объемная (КО) сетка задачи для 3 и 4 узловых КО
    /// для однородных и смешанных сеток 1 порядка
    /// </summary>
    public interface IFVComMesh : IMesh
    {
        #region Поля и методы FV
        /// <summary>
        ///  Контрольные объемы
        /// </summary>
        IFVElement[] AreaElems { get; set; }
        /// <summary>
        /// Грани КО
        /// </summary>
        IFVFacet[] Facets { get; set; }
        /// <summary>
        /// Граничные грани КО
        /// </summary>
        IFVFacet[] BoundaryFacets { get; set; }
        #endregion
        /// <summary>
        /// Интерполяция Функции с центров КО в узлы КЭ сетки
        /// </summary>
        /// <param name="U_elems"></param>
        /// <param name="FlagBCCorrection">поправка на границах</param>
        /// <returns></returns>
        void ConvertElementsToKnots(double[] Zeta_e, ref double[] Zeta_n, double[] BoundaryValueDomain = null, bool FlagBCCorrection = false);
        /// <summary>
        /// Получить среднее на контрольном объеме значение функции по значениям в узлах
        /// </summary>
        /// <param name="Zeta_n"></param>
        /// <param name="Zeta_e"></param>
        void ConvertKnotsToElements(double[] Zeta_n, ref double[] Zeta_e);
    }
}
