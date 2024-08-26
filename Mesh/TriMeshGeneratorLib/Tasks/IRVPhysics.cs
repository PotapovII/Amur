//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using System.IO;
    /// <summary>
    /// Физика - орпеделение задачи
    /// </summary>
    public interface IRVPhysics
    {
        /// <summary>
        /// Создать новый узел
        /// </summary>
        RVNode CreateNewNode(int n = 1, double x = 100.0, double y = 100.0, double z = 100.0);
        /// <summary>
        /// Создать новый сегмент
        /// </summary>
        RVSegment CreateNewSegment(int n = 1, RVNode nP1 = null, RVNode nP2 = null, RVSegment segP = null);
        /// <summary>
        /// Считать узел
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        RVNode ReadNode(StreamReader file);
        /// <summary>
        /// Считать  сегмент
        /// </summary>
        RVSegment ReadSegment(int n, RVNode nP1, RVNode nP2, StreamReader file);
        /// <summary>
        /// неизвестных в задаче
        /// </summary>
        /// <returns></returns>
        int CountVars { get; }
        /// <summary>
        /// параметров в задаче
        /// </summary>
        /// <returns></returns>
        int CountParams { get; }
        /// <summary>
        /// название задачи
        /// </summary>
        /// <returns></returns>
        string NameType { get; }
    }
}
