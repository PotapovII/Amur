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
        public int meshtype;
        public int dims;
        public int CountVars;
        public int knotParams;
        public int bparams;
        public int[] Keqns = new int[12];
        public int CountNodes;
        public int CountElements;
        public int CountBoundElements;
        public int CountBoundSegs;
    }
}
