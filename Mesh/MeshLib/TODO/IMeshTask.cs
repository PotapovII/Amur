//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//             кодировка : 04.02.22 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using CommonLib.IO;
    /// <summary>
    /// ОО: интерфейс для задач построения расчетной сетки в области
    /// </summary>
    interface IMeshTask : ITask
    {
        /// <summary>
        ///  Сетка для расчета задачи
        /// </summary>
        IFEMesh BuildIFEMesh();
        /// <summary>
        ///  Сетка для расчета задачи
        /// </summary>
        IMesh BuildIMesh();
        /// <summary>
        /// Установка объектоа алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        void Set(IAlgebra algebra = null);
        /// <summary>
        /// Установка новых координат
        /// </summary>
        void Set(double[] x, double[] y);
        /// <summary>
        /// Установка новых координат
        /// </summary>
        void Set(double[][] x, double[][] y);
        /// <summary>
        /// Установка новых координат по адресам
        /// </summary>
        void Set(int[] adress, double[] x, double[] y);
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        IMeshTask Clone();
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        IOFormater<IMeshTask> GetFormater();
    }
}
