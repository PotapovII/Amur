using System;
// для тестов
using MeshLib;
using CommonLib;
using MemLogLib;

namespace ViewerGL.Mesh
{
    class ClassTest
    {
        int[] Flag = { 0, 1, 2, 3 };
        TriMesh triMesh = null;
        float[] colors = null;
        /// <summary>
        /// Сетка
        /// </summary>
        protected IRenderMeshGL mesh = null;

        protected RenderMesh2DArray renderMesh2DArray = null;
        protected RenderMesh2D renderMesh2D = null;
        public ClassTest(int N=10, double L =1)
        {
            TriMeshCreate.GetTetrangleMesh(ref triMesh, N, L, Flag);
            mesh = new RenderMeshGL(triMesh);
            renderMesh2DArray = new RenderMesh2DArray(mesh);
            renderMesh2D = new RenderMesh2D(mesh);
            int CountColors = mesh.CountColors;

            MEM.Alloc(mesh.CountColors, ref colors);
            GLB.SetColor4(colors);
        }
        public void DrawRenderMesh2DArray()
        {
            GLB.SetColor4(colors,1,0,0,1);
            renderMesh2DArray.DrawArray(colors);
        }

        int k=0;
        public void DrawRenderMesh2D()
        {
            if(k==0)
                GLB.SetColor4(colors, 0, 1, 0, 1);
            if (k == 20)
                GLB.SetColor4(colors, 1, 1, 0, 1);
            if (k == 40)
                GLB.SetColor4(colors, 0, 1, 1, 1);
            k=++k<41?k:0;
            renderMesh2D.DrawVBO(colors);
        }
        public void Dellete()
        {
            renderMesh2D.DelleteVBO();
        }
    }
}
