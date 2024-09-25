//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    /// <summary>
    /// Шапка cdg файла дань C++ (хранит размерности массивов и 
    /// про.. хрень как дань совместимости)
    /// </summary>
    public class RVControl
    {
        public int trans;
        /// <summary>
        /// Тип сетки
        /// </summary>
        public int meshtype;
        /// <summary>
        /// Размерность
        /// </summary>
        public int dims;
        /// <summary>
        /// Количество переменных
        /// </summary>
        public int CountVars;
        /// <summary>
        /// Количество параметров
        /// </summary>
        public int knotParams;
        public int bparams;
        public int[] Keqns = new int[12];
        /// <summary>
        /// Количество узлов
        /// </summary>
        public int CountNodes;
        /// <summary>
        /// Количество конечных элементов
        /// </summary>
        public int CountElements;
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        public int CountBoundElements;
        /// <summary>
        /// Количество сегментов
        /// </summary>
        public int CountBoundSegs;
    }
}
