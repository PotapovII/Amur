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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          21.02.24
//---------------------------------------------------------------------------
namespace CommonLib.Tasks
{
    /// <summary>
    /// Значение параметров для выполнения ГУ
    /// </summary>
    public interface IBCValues
    {
        /// <summary>
        /// Значение параметров для выполнения ГУ
        /// </summary>
        double[] Values { get; }
        /// <summary>
        /// Умножение параметров на число
        /// </summary>
        /// <param name="scal"></param>
        void Mult(double scal);
        void Sum(IBCValues bv);
    }


    /// <summary>
    /// Расчетная область
    /// </summary>
    public interface IBaseCalculationDomain
    {
        /// <summary>
        /// Сетка расчетной области
        /// </summary>
        /// <returns></returns>
        IMesh GetMesh();
        /// <summary>
        /// Количество неизвестных в расчетной области
        /// </summary>
        /// <returns></returns>
        uint CountUnknowns();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на 
        /// сетке расчетной области краевые условия задачи
        /// </summary>
        IUnknown[] Unknowns();
        /// <summary>
        /// Значения величин для выполнения граничных условий в узлах 
        /// для каждой неизвестной
        /// </summary>
        /// <param name="unknown">номер неизвестной</param>
        /// <returns></returns>
        void GetBoundKnotsValues(uint unknown, ref double[] values);
        /// <summary>
        /// Значения величин для выполнения граничных условий в узлах 
        /// для каждой неизвестной
        /// </summary>
        /// <param name="unknown">номер неизвестной</param>
        /// <returns></returns>
        void GetBoundKnotsIndex(uint unknown, ref int[] indexs);
        /// <summary>
        /// Начальные условия
        /// </summary>
        void InitValues();
        /// <summary>
        /// Вычисление зависимых граничных условий
        /// </summary>
        /// <param name="unknown"></param>
        void CalcBoundary(uint unknown);
    }

    /// <summary>
    /// Расчетная область
    /// </summary>
    public interface ICalculationDomain : IBaseCalculationDomain
    {
        /// <summary>
        /// Значения величин для выполнения граничных на ГКЭ 
        /// расчетной области для каждой неизвестной
        /// </summary>
        /// <param name="unknown"></param>
        /// <param name="bcv"></param>
        void GetBoundElementsValues(uint unknown, ref IBCValues[] bcv);
    }
}
