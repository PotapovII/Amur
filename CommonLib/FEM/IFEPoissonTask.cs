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

namespace CommonLib
{
    using System;
    /// <summary>
    /// Простые КЭ задачи
    /// Задача Пуассона
    /// </summary>
    public interface IFEPoissonTask : IBaseFEMTask
    {
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        void InitLocal(int cu);
        /// <summary>
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ
        /// </summary>
        void FEPoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double Q);
        /// <summary>
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с переменной правой частью
        /// </summary>
        void FEPoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double[] Q);
        /// <summary>
        /// Интерполяция поля МКЭ
        /// </summary>
        void Interpolation(ref double[] TauZ, double[] tmpTauZ);
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        /// <param name="U">функции</param>
        /// <param name="Area">площадь расчетной области </param>
        /// <returns>интеграла по площади расчетной области для функции U</returns>
        /// <exception cref="Exception"></exception>
        double RiverFlowRate(double[] U, ref double Area);
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        double SimpleRiverFlowRate(double[] U, ref double Area);
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        void CalkTauInterpolation(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity);
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        void SolveTaus(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity);
    }
}
