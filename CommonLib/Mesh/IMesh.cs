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
//      Убраны массивы ГУ из всей иерархи сеток : 01.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// ОО: IMesh - базистная конечно-элементная сетка с 
    /// </summary>
    public interface IMesh : IRenderMesh
    {
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        TypeRangeMesh typeRangeMesh { get; }
        /// <summary>
        /// Тип КЭ сетки в 1D и 2D
        /// </summary>
        TypeMesh typeMesh { get; }
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        TypeFunForm ElementKnots(uint i, ref uint[] knots);
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot);
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>
        void GetElemCoords(uint i, ref double[] X, ref double[] Y);
        /// <summary>
        /// Получить координаты Y вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Y вершин КЭ</returns>
        /// void ElemY(uint i, ref double[] Y);
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        uint GetMaxKnotDecrementForElement(uint i);
        /// <summary>
        /// Вычисление длины граничного КЭ
        /// </summary>
        /// <param name="belement">номер граничного конечного элемента</param>
        /// <returns></returns>
        double GetBoundElemLength(uint belement);
        /// <summary>
        /// Получить выборку граничных узлов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        uint[] GetBoundKnotsByMarker(int id);
        /// <summary>
        /// Получить выборку граничных элементов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        uint[][] GetBoundElementsByMarker(int id);
        /// <summary>
        /// Ширина ленты в глобальной матрице жнсткости
        /// </summary>
        /// <returns></returns>
        uint GetWidthMatrix();
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        IMesh Clone();
        /// <summary>
        /// Тестовая печать КЭ сетки в консоль
        /// </summary>
        void Print();
    }
}