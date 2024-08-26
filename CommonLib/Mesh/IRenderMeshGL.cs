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
//  !!!  Нет флагов для граничных элементов !!! ++ 
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// ОО: IMesh - трехузловая сетка для работы с графикой
    /// </summary>
    public interface IRenderMeshGL
    {
        /// <summary>
        /// Размерность массива цвета
        /// </summary>
        int CountColors { get; }
        /// <summary>
        /// Размерность массива цвета
        /// </summary>
        int CountKnots { get; }
        /// <summary>
        /// Вектор 3-x конечных элементов в области
        /// </summary>
        uint[] GetAreaElems();
        /// <summary>
        /// Вектор 2-x конечных элементов на границе
        /// </summary>
        uint[] GetBoundElems();
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        uint[] GetBoundKnots();
        /// <summary>
        /// Координаты X для узловых точек 
        /// </summary>
        float[] GetCoords();
        #region Граничные элементы и Граничные узлы
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        uint[] GetBElementsBCMark();
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        uint[] GetBoundKnotsMark();
        #endregion
    }
}