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
//                    31.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
//    08.06.2024 + интерфейс для определения придонной динамической скорости
//---------------------------------------------------------------------------
using CommonLib.Function;
using CommonLib.Geometry;
namespace CommonLib.Mesh
{
    /// <summary>
    /// Обертка для сетки для задач CFG в створе, выполняет предварительные расчеты задачи
    /// связанные с сеткой
    /// </summary>
    public interface IMeshWrapperСhannelSectionCFG : IMeshWrapperCrossCFG
    {
        /// <summary>
        /// индексы приповерхностных конечных элементов в которые попадает точка наблюдения
        /// </summary>
        int[] GetBedElems();
        /// <summary>
        /// координаты точки отложенной по нормали к дну в расчетную область 
        /// на величину delta * step
        /// </summary>
        /// <returns></returns>
        IHPoint[] GetBedPoint();
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        uint[] GetBoundaryBedAdress();
        /// <summary>
        /// Шаг дискретизации по дну
        /// </summary>
        /// <returns></returns>
        double[] GetpLengthBed();
        /// <summary>
        /// координаты точки отложенной по нормали к дну в расчетную область 
        /// на величину delta * step
        /// </summary>
        /// <returns></returns>
        IHPoint[] GetMuBedPoint();
        /// <summary>
        /// растояние от донного узла до точки наблюдения 0.05 * H
        /// </summary>
        /// <returns></returns>
        double[] GetMuLengthBed();
        /// <summary>
        /// индексы придонных конечных элементов в которые попадает точка наблюдения
        /// при расчете динамической скорости на растоянии от дна 0.05 * H
        /// </summary>
        int[] GetMuBedElems();

        /// <summary>
        /// индексы приповерхностных конечных элементов в которые попадает точка
        /// </summary>
        int[] GetWaterLevelElems();
        /// <summary>
        /// координаты точки отложенной по нормали к свободной поверхности потока 
        /// в расчетную область на величину delta * step
        /// </summary>
        /// <returns></returns>
        IHPoint[] GetWaterLevelPoint();
        /// <summary>
        /// адреса узлов на свободной поверхности потока
        /// </summary>
        uint[] GetBoundaryWaterLevelAdress();
        /// <summary>
        /// Шаг дискретизации по свободной поверхности
        /// </summary>
        /// <returns></returns>
        double[] GetpLengthWaterLevel();
        /// <summary>
        /// адреса узлов на свободной поверхности потока и дне
        /// </summary>
        uint[] GetBoundaryAdress();
        /// <summary>
        /// Расчет компонент поля скорости по функции тока на дне
        /// </summary>
        /// <param name="result">результат решения</param>
        void CalkBedVortex(double[] Phi, double[] Vortex, double w, ref double[] bcBedVortexValue);
        /// <summary>
        /// Расчет граничных условий для функции Ux
        /// </summary>
        /// <param name="result">результат решения</param>
        void CalkBoundaryUx(double[] wlVelosityX, ref double[] wl_Ux);
        /// <summary>
        /// Расчет компонент поля скорости по функции тока на свободной поверхности
        /// </summary>
        /// <param name="result">результат решения</param>
        void CalkWaterLevelVortex(double[] Phi, double[] Vortex, double[] wlVelosity, double w, ref double[] bcWLVortexValue);
        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        void CalkBoundaryVortex(double[] Phi, double[] Vortex, double[] wlVelosity, double w, ref double[] bcVortexValue, int VelocityG2);
        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        void CalkBoundaryVortex_Plane(double[] Phi, double[] Vortex, double[] wlVelosity, double w, ref double[] bcVortexValue);
        /// <summary>
        /// Вычисление функции динамической придонной скорости
        /// </summary>
        /// <param name="Ux"></param>
        /// <returns></returns>
        void CalkBoundary_U_star(double[] Ux, ref double[] Us);
        /// <summary>
        /// координаты донных узлов
        /// </summary>
        void BedCoords(ref double[] y, ref double[] z);
        /// <summary>
        /// Отметка свободной поверхности
        /// </summary>
        double WaterLevel { get; }
        /// <summary>
        /// Минимальный радиус - растояние от оси z до выпоклого берега чечения
        /// </summary>
        double GetR_midle();
        /// <summary>
        /// Флаг постановки 0 - плоская задача, 1 - осесимметричная задача
        /// </summary>
        int GetRing();
    }
}
