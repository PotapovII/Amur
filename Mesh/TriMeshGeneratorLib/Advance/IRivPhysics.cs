//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    using System.IO;
    /// <summary>
    /// Физика - орпеделение задачи
    /// </summary>
    public interface IRivPhysics
    {
        /// <summary>
        /// Создать новый узел
        /// </summary>
        RivNode CreateNewNode(int n = 1, double x = 100.0, double y = 100.0, double z = 100.0);
        /// <summary>
        /// Создать новый сегмент
        /// </summary>
        RivEdge CreateNewSegment(int n = 1, RivNode nP1 = null, RivNode nP2 = null, RivEdge segP = null);
        /// <summary>
        /// Считать узел
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        RivNode ReadNode(StreamReader file);
        /// <summary>
        /// Считать  сегмент
        /// </summary>
        RivEdge ReadSegment(int n, RivNode nP1, RivNode nP2, StreamReader file);
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
