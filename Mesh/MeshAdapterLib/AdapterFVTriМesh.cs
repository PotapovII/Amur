//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2022 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 правка  :   04.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshAdapterLib
{
    using System.Collections.Generic;
    using MemLogLib;
    using CommonLib;
    using MeshLib;

    public static class AdapterFVTriМesh
    {
        /// <summary>
        /// Конвертация FVTriМesh в TriMesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static IMesh GetMesh(FVComMesh mesh)
        {
            TriMesh tmesh = new TriMesh();
            tmesh.AreaElems = new TriElement[mesh.CountElements];
            for (int i = 0; i < tmesh.AreaElems.Length; i++)
            {
                tmesh.AreaElems[i].Vertex1 = (uint)mesh.AreaElems[i].Nodes[0];
                tmesh.AreaElems[i].Vertex2 = (uint)mesh.AreaElems[i].Nodes[1];
                tmesh.AreaElems[i].Vertex3 = (uint)mesh.AreaElems[i].Nodes[2];
            }
            List<double> bk = new List<double>();
            tmesh.BoundElems = new TwoElement[mesh.BoundaryFacets.Length];
            tmesh.BoundElementsMark = new int[mesh.BoundaryFacets.Length];
            for (int i = 0; i < tmesh.BoundElems.Length; i++)
            {
                tmesh.BoundElems[i].Vertex1 = (uint)mesh.BoundaryFacets[i].Pointid1;
                bk.Add(tmesh.BoundElems[i].Vertex1);
                tmesh.BoundElems[i].Vertex2 = (uint)mesh.BoundaryFacets[i].Pointid2;
                bk.Add(tmesh.BoundElems[i].Vertex2);
                tmesh.BoundElementsMark[i] = (int)mesh.BoundaryFacets[i].BoundaryFacetsMark;
            }
            double[] ux = null;
            MEM.SelectUniqueElems(bk.ToArray(), ref ux);
            tmesh.BoundKnots = new int[ux.Length];
            tmesh.BoundKnotsMark = new int[ux.Length];
            for (int i = 0; i < tmesh.BoundKnots.Length; i++)
            {
                tmesh.BoundKnots[i] = (int)ux[i];
                tmesh.BoundKnotsMark[i] = 0;
            }
            int nodecount = mesh.CoordsX.Length;
            tmesh.CoordsX = new double[nodecount];
            tmesh.CoordsY = new double[nodecount];
            for (int i = 0; i < tmesh.CoordsX.Length; i++)
            {
                tmesh.CoordsX[i] = mesh.CoordsX[i];
                tmesh.CoordsY[i] = mesh.CoordsY[i];
            }
            return tmesh;
        }
    }
}
