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
//                    06.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace CommonLib.Mesh
{
    using CommonLib;
    using CommonLib.Geometry;
    using System.Linq;
    using System;

    /// <summary>
    /// Обертка для сетки
    /// </summary>
    public interface IMeshWrapper
    {
        /// <summary>
        /// Сетка расчетной области
        /// </summary>
        /// <returns></returns>
        IMesh GetMesh();
        /// <summary>
        /// массив площадей КЭ
        /// </summary>
        double[] GetS();
        /// <summary>
        /// массив длин граничных элементов
        /// </summary>
        double[] GetLb();
        /// <summary>
        /// массив нормалей к граничным элементам
        /// </summary>
        HPoint[] GetNormals();
        /// <summary>
        /// массив касательфынх к граничным элементам
        /// </summary>
        HPoint[] GetTau();
        /// <summary>
        /// массив площадей КО
        /// </summary>
        double[] GetElemS();
        /// <summary>
        /// Размер КЭ по х
        /// </summary>
        double[] GetHx();
        /// <summary>
        /// Размер КЭ по y
        /// </summary>
        double[] GetHy();
        /// <summary>
        /// массив функций формы
        /// </summary>
        double[][] GetN();
        /// <summary>
        /// массив 
        /// </summary>
        double[][] GetAN();
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        double[][] GetdNdx();
        /// <summary>
        /// массив производных для функций формы
        /// </summary>
        double[][] GetdNdy();
        /// <summary>
        ///  вычисление значений функций формы в точке
        /// </summary>
        /// <param name="elem">номер элемента</param>
        /// <param name="x">координата х</param>
        /// <param name="y">координата Y</param>
        /// <param name="N">массив функций формы</param>
        void CalkForm(int elem, double x, double y, ref double[] N);
        /// <summary>
        ///  вычисление значений функций формы в точке
        /// </summary>
        /// <param name="elem">номер элемента</param>
        /// <param name="p">точка</param>
        /// <param name="N">массив функций формы</param>
        void CalkForm(int elem, IHPoint p, ref double[] N);
        /// <summary>
        /// Установка КЭ сетки
        /// </summary>
        /// <param name="mesh"></param>
        void SetMesh(IMesh mesh);
        /// <summary>
        /// Перенос значений с КЭ  в узлы КЭ
        /// </summary>
        /// <param name="Y">узловые значения</param>
        /// <param name="X">значения на КЭ</param>
        void ConvertField(ref double[] Y, double[] X);
        #region Дополнительная ленивая функциональность
        /// <summary>
        /// Вычисление минимального растояния от узла до стенки
        /// В плпнпх - хеширование узлов на масштабе глубины
        /// </summary>
        void CalkDistance(ref double[] distance, ref double[] Hp);
        /// <summary>
        /// Площадь сечения
        /// </summary>
        /// <returns></returns>
        double GetArea();
        /// <summary>
        /// Смоченный периметр живого сечения
        /// </summary>
        /// <returns></returns>
        double GetBottom();
        /// <summary>
        /// Ширина живого сечения
        /// </summary>
        /// <returns></returns>
        double GetWidth();
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        /// <param name="U">функции</param>
        /// <param name="Area">площадь расчетной области </param>
        /// <returns>интеграла по площади расчетной области для функции U</returns>
        /// <exception cref="Exception"></exception>  
        double RiverFlowRate(double[] U, ref double Area);
        #endregion
    }
}