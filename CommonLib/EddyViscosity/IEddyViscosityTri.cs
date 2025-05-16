//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
using CommonLib.Mesh;

namespace CommonLib.EddyViscosity
{
    /// <summary>
    /// Интерфейс для расчета вихревой вязкости
    /// </summary>
    public interface IEddyViscosityTri : ITask
    {
        /// <summary>
        /// Тип системы координат 0 - плоскя, 1 - цилиндрическая, 2 - осесимметричная 
        /// </summary>
        int Sigma { get; set; }
        /// <summary>
        /// Размерность узла задачи, зависит от модели т.в.
        /// </summary>
        int Cet_cs(); 
        /// <summary>
        /// Расчет вихревой вязкости 
        /// </summary>
        /// <param name="eddyViscosity">вихревая вязкость</param>
        /// <param name="Ux">осевая/окружная/осевая/скорость потока</param>
        /// <param name="Phi">функция тока</param>
        /// <param name="Vortex">вихрь</param>
        /// <param name="dt">шаг по времени</param>
        void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt);
        /// <summary>
        /// Установка данных для расчета задачи
        /// </summary>
        /// <param name="mesh">сетка задачи</param>
        /// <param name="algebra">решатель задачи</param>
        /// <param name="wMesh">врапер задачи</param>
        void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null);
        /// <summary>
        /// Тип модели
        /// </summary>
        ETurbViscType turbViscType { get; set; }
    }
}
