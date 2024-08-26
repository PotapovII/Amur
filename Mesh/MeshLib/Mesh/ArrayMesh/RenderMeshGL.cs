//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//  !!!  Нет флагов для граничных элементов !!! ++ 
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: IMesh - трехузловая сетка для работы с графикой
    /// </summary>
    public class RenderMeshGL: IRenderMeshGL
    {
        /// <summary>
        /// Размерность массива цвета
        /// </summary>
        public int CountColors { get; }
        /// <summary>
        /// Количество вершин
        /// </summary>
        public int CountKnots { get; }

        protected uint cu = 3;
        protected uint bu = 2;
        
        protected IRenderMesh mesh = null;
        public RenderMeshGL(IRenderMesh mesh)
        {
            this.mesh = mesh;
            CountColors = 4*mesh.CountKnots;
            CountKnots = mesh.CountKnots;
        }
        public RenderMeshGL(RenderMeshGL mesh)
        {
            this.mesh = mesh.mesh;
            CountColors = mesh.CountKnots;
            CountKnots = mesh.CountKnots;
        }
        /// <summary>
        /// Вектор 3-x конечных элементов в области
        /// </summary>
        public uint[] GetAreaElems()
        {
            if (mesh == null) return null;
            uint[] elems = new uint[cu * mesh.CountElements];
            TriElement[] es = mesh.GetAreaElems();
            uint k = 0;
            for(int i=0; i<mesh.CountElements; i++)
            {
                elems[k] = es[i].Vertex1;
                elems[k+1] = es[i].Vertex2;
                elems[k+2] = es[i].Vertex3;
                k += cu;
            }
            return elems;
        }
        /// <summary>
        /// Вектор 2-x конечных элементов на границе
        /// </summary>
        public uint[] GetBoundElems()
        {
            if (mesh == null) return null;
            uint[] elems = new uint[bu * mesh.CountBoundElements];
            TwoElement[] es = mesh.GetBoundElems();
            uint k = 0;
            for (int i = 0; i < mesh.CountBoundElements; i++)
            {
                elems[k] = es[k].Vertex1;
                elems[k + 1] = es[k].Vertex2;
                k += bu;
            }
            return elems;
        }
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public uint[] GetBoundKnots()
        {
            if (mesh == null) return null;
            uint[] Vertex = new uint[mesh.CountBoundKnots];
            int[] es = mesh.GetBoundKnots();
            for (int i = 0; i < mesh.CountElements; i++)
                Vertex[i] = (uint)es[i];
            return Vertex;
        }
        /// <summary>
        /// Координаты X,Y для узловых точек 
        /// </summary>
        public float[] GetCoords()
        {
            if (mesh == null) return null;
            float[] coords = new float[cu * mesh.CountKnots];
            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);
            uint k = 0;
            for (int i = 0; i < mesh.CountKnots; i++)
            {
                coords[k] = (float)x[i];
                coords[k + 1] = (float)y[i];
                coords[k + 2] = 0;
                k += cu;
            }
            return coords;
        }
        #region Граничные элементы и Граничные узлы
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public  uint[] GetBElementsBCMark()
        {
            return MEM.IntToUint(mesh.GetBElementsBCMark());
        }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public uint[] GetBoundKnotsMark()
        {
            return MEM.IntToUint(mesh.GetBoundKnotsMark());
        }
        #endregion
    }
}