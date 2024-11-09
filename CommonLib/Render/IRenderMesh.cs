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
//                 кодировка : 25.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// ОО: IMesh - трехузловая сетка для работы с графикой
    /// </summary>
    public interface IRenderMesh 
    {
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        TriElement[] GetAreaElems();
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        TwoElement[] GetBoundElems();
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        int[] GetBoundKnots();
        /// <summary>
        /// Координаты X для узловых точек 
        /// </summary>
        double[] GetCoords(int dim);
        #region Граничные элементы и Граничные узлы
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        int[] GetBElementsBCMark();
        ///// <summary>
        ///// Получить массив типов границы для граничных элементов 
        ///// </summary>
        //TypeBoundCond[] GetBoundElementsType();
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        int[] GetBoundKnotsMark();
        ///// <summary>
        ///// Массив типов границы  для граничных узловых точек
        ///// </summary>
        //TypeBoundCond[] GetBKnotsBCType();
        #endregion 
        /// <summary>
        /// Количество элементов
        /// </summary>
        int CountElements { get; }
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        int CountBoundElements { get; }
        /// <summary>
        /// Количество узлов
        /// </summary>
        int CountKnots { get; }
        /// <summary>
        /// Количество граничных узлов
        /// </summary>
        int CountBoundKnots { get; }
        /// <summary>
        /// Диапазон координат для узлов сетки
        /// </summary>
        void MinMax(int dim, ref double min, ref double max);
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>
        void ElemValues(double[] Values, uint i, ref double[] elementValue);
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        /// <param name="element">номер конечного элемента</param>
        /// <returns></returns>
        double ElemSquare(uint element);
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        double ElemSquare(TriElement element);
        /// <summary>
        /// Получить тип граничных условий для граничного элемента
        /// </summary>
        /// <param name="elem">граничный элемент</param>
        /// <returns>ID типа граничных условий</returns>
        int GetBoundElementMarker(uint elem);
    }
}