//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace ViewerGL
{
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    /// <summary>
    /// Метод отрисовки
    /// </summary>
    public enum RenderMethod
    {
        ArrayVertex,
        ArrayBufferVBO,
        ArrayBufferVAO
    }
    /// <summary>
    /// ОО: IMesh - трехузловая сетка для работы с графикой
    /// Объекты вершинных массивов (VAO)
    /// </summary>
    public class RenderMesh2D
    {
        //public RenderMethod renderMethod = RenderMethod.ArrayBufferVBO;
        //public RenderMethod renderMethod = RenderMethod.ArrayBufferVAO;
        public RenderMethod renderMethod = RenderMethod.ArrayVertex;
        /// <summary>
        /// Сетка
        /// </summary>
        protected IRenderMeshGL mesh = null;
        /// <summary>
        /// инддексы вершин в КЭ сетке
        /// </summary>
        protected uint[] indexes = null;    
        /// <summary>
        /// id индексов
        /// </summary>
        public int eboIndexID;
        /// <summary>
        /// id координат
        /// </summary>
        public int vboCoordID;
        /// <summary>
        /// id координат
        /// </summary>
        public int vboColorID;
        /// <summary>
        /// id массива объектов
        /// </summary>
        public int vaoID;
        /// <summary>
        /// Координаты вершин в КЭ сетке
        /// </summary>
        protected float[] vertexes = null;
        /// <summary>
        /// Координаты вершин в КЭ сетке
        /// </summary>
        protected float[] colors = null;
        public RenderMesh2D(ISavePoint sp)
        {
            mesh = new RenderMeshGL((IRenderMesh)sp.mesh);
            Init();
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh"></param>
        public RenderMesh2D(IRenderMeshGL mesh)
        {
            this.mesh = mesh;
            Init();
        }
        protected void Init()
        {
            vertexes = mesh.GetCoords();
            indexes = mesh.GetAreaElems();
            MEM.Alloc(mesh.CountColors, ref colors);
            GLB.SetColor4(colors);
            switch (renderMethod)
            {
                case RenderMethod.ArrayVertex:
                    {
                    }
                    break;
                case RenderMethod.ArrayBufferVBO:
                    {
                        GLB.CreateVBO_IVC(indexes, vertexes, colors,
                        ref eboIndexID, ref vboCoordID, ref vboColorID);
                    }
                    break;
                case RenderMethod.ArrayBufferVAO:
                    {
                        vaoID = GLB.CreateVAO_IVC(indexes, vertexes, colors,
                        ref eboIndexID, ref vboCoordID, ref vboColorID);
                    }
                    break;
            }
        }
        public void DelleteVBO()
        {
            switch (renderMethod)
            {
                case RenderMethod.ArrayVertex:
                    {
                    }
                    break;
                case RenderMethod.ArrayBufferVBO:
                    {
                        GLB.DelleteVBO_IVC(eboIndexID, vboCoordID, vboColorID);
                    }
                    break;
                case RenderMethod.ArrayBufferVAO:
                    {
                        GLB.DelleteVAO_IVC(vaoID, eboIndexID, vboCoordID, vboColorID);
                    }
                    break;
            }
        }
        /// <summary>
        /// Отрисовка данных через вершинные массивы
        /// </summary>
        public void DrawVBO(float[] colors = null)
        {
            switch (renderMethod)
            {
                case RenderMethod.ArrayVertex:
                    {
                        GLB.DrawTriangles(indexes, vertexes, colors);
                    }
                    break;
                case RenderMethod.ArrayBufferVBO:
                    {
                        GLB.DrawVBO_IVC(eboIndexID, vboCoordID, vboColorID, indexes.Length, colors);
                    }
                    break;
                case RenderMethod.ArrayBufferVAO:
                    {
                        GLB.BindVAO(vaoID);
                        GLB.DrawVAO_IVC(indexes.Length, vboColorID, colors);
                        GLB.UnBindVAO();
                    }
                    break;
            }
        }
    }
}