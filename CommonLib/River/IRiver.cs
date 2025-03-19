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
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      Прототип River2DM C++
//---------------------------------------------------------------------------
//             кодировка : 14.07.21 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    using CommonLib.IO;
    using CommonLib.EConverter;
    using System;
    using CommonLib.ChannelProcess;

    /// <summary>
    /// Флаг получаемых напряжений
    /// </summary>
    [Serializable]
    public enum StressesFlag
    {
        /// <summary>
        /// получить напряжения на КЭ
        /// </summary>
        Element = 0,
        /// <summary>
        /// получить напряжения в узлах КЭ
        /// </summary>
        Nod
    }
    /// <summary>
    /// ОО: интерфейс для задач расчета гидродинамического потока для русловых задач
    /// </summary>
    public interface IRiver : ITask
    {
        /// <summary>
        /// Флаг определяющий необходимость расчета взвешенных наносов
        /// </summary>
        EBedErosion GetBedErosion();
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        IBoundaryConditions BoundCondition();
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики при загрузке
        /// по умолчанию
        /// </summary>
        ITaskFileNames taskFileNemes();
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        IMesh BedMesh();
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        void SolverStep();
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        //bool LoadData(string fileName);
        /// <summary>
        /// определение расчетной области задачи
        /// </summary>
        /// <returns></returns>
        // bool DoWorkDomain();
        /// <summary>
        /// Установка объектоа КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        void Set(IMesh mesh, IAlgebra algebra = null);
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        void SetZeta(double[] zeta, EBedErosion bedErosion);
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        void GetZeta(ref double[] zeta);
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        void GetRoughness(ref double[] Roughness);
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        /// <param name="С">объемная концентрация наносов по фракциям проинтегрированная/осредненная по глубине потока</param>
        /// <param name="sf">флаг определяющий заданны поля в узлах (Nod) или в центрах элементов</param>
        void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod);
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        IRiver Clone();
        /// <summary>
        /// Создает экземпляр класса конвертера для 
        /// загрузки и сохранения задачи не в бинарном формате
        /// </summary>
        /// <returns></returns>
        IOFormater<IRiver> GetFormater();
    }
}
