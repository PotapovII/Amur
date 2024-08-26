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
//                          Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.02.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// ОО: Расширение базового интерфейса КЭ сетки до требований 
    /// метода контрольного объема
    /// </summary>
    public interface IFVMesh : IMesh
    {
        /// <summary>
        /// Получить узлы элементов ( 6 3 1 ) сопряженных с элементом ( 4 5 2 )
        /// если 1 грань треугольника с узлами (3 5) внешняя граница то 
        /// получим узлы сопряженных элементов ( -1 3 1 )  
        /// 1-----2----3    1-----2----3
        /// |    /|   /     |    /|   /
        /// |   3 |  /      |   3 |  /  
        /// |  /  2 /       |  /  2 /  
        /// | /   |/        | /   |/ 
        /// 4--1--5         4--1--5 
        /// |    /
        /// |   /
        /// |  /
        /// | /
        /// |/
        /// 6
        /// </summary>
        /// <param name="element)">номер элемента</param>
        int[] ConjugateElementKnots(uint element);
        /// <summary>
        /// Получить номера элементов ( c b a ) сопряженных с элементом ( k )
        /// если 1 грань треугольника с узлами (4 5) внешняя граница то 
        /// получим следующие номера элементов ( -1 b a ) сопряженных c элементом ( k )  
        /// 1-----2----3    1-----2----3
        /// |  a /| b /     | a  /| b /
        /// |   3 |  /      |   3 |  /  
        /// |  /  2 /       |  /  2 /  
        /// | / k |/        | /   |/ 
        /// 4--1--5         4--1--5 
        /// |    /
        /// | c /
        /// |  /
        /// | /
        /// |/
        /// 6
        /// </summary>
        /// <param name="element">номер элемента</param>
        int[] ConjugateElement(uint element);
        /// <summary>
        /// Взять координату Х для i - го узла
        /// </summary>
        double X(uint i);
        /// <summary>
        /// Взять координату Y для i - го узла
        /// </summary>
        double Y(uint i);
        /// <summary>
        /// Получить координаты Х для всех узлов сетки
        /// </summary>
        double[] GetX();
        /// <summary>
        /// Получить координаты Y для всех узлов сетки
        /// </summary>
        double[] GetY();
        /// <summary>
        /// Длина граней КЭ элемента 12 23 31
        /// </summary>
        double[] ElementLength(uint element);
        /// <summary>
        /// Производные для функций формы КЭ по x
        /// </summary>
        double[] Dx(uint element);
        /// <summary>
        /// Производные для функций формы КЭ по x
        /// </summary>
        double[] Dy(uint element);
        /// <summary>
        /// Площадь КЭ
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        double Area(uint element);
        /// <summary>
        /// Площадь КО
        /// </summary>
        /// <param name="узел КО"></param>
        double AreaFV(uint knot);
        /// <summary>
        /// Расчет расширений сетки
        /// </summary>
        void CalculationOfExtensions();
    }
}
