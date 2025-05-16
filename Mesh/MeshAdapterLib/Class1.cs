namespace MeshAdapterLib
{

    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Geometry;

    using MeshLib;

    using SHillObjLib;

    public class MeshAdapterSHillObj
    {
        /// <summary>
        /// Генерация объекта симпл - сетки
        /// </summary>
        /// <param name="DEGUG"></param>
        /// <returns></returns>
        public static IMesh CreateMesh(Triad[] Triangles, HullVertex[] hull, HPoint[] Points)
        {
            TriMesh mesh = new TriMesh();
            int CountElems = Triangles.Length;
            MEM.Alloc(CountElems, ref mesh.AreaElems);
            List<TriElement> tri = new List<TriElement>();
            for (int i = 0; i < CountElems; i++)
            {
                int i0 = Triangles[i].a;
                int i1 = Triangles[i].b;
                int i2 = Triangles[i].c;
                tri.Add(new TriElement((uint)i0, (uint)i1, (uint)i2));
            }
            mesh.AreaElems = tri.ToArray();
            MEM.Alloc(Points.Length, ref mesh.CoordsX);
            MEM.Alloc(Points.Length, ref mesh.CoordsY);
            for (int i = 0; i < Points.Length; i++)
            {
                mesh.CoordsX[i] = Points[i].X;
                mesh.CoordsY[i] = Points[i].Y;
            }
            int CountHullKnots = hull.Length;
            MEM.Alloc(CountHullKnots, ref mesh.BoundElems);
            MEM.Alloc(CountHullKnots, ref mesh.BoundElementsMark);
            MEM.Alloc(CountHullKnots, ref mesh.BoundKnots);
            MEM.Alloc(CountHullKnots, ref mesh.BoundKnotsMark);
            for (int i = 0; i < CountHullKnots; i++)
            {
                mesh.BoundElems[i].Vertex1 = (uint)hull[i].pointsIndex;
                mesh.BoundElems[i].Vertex2 = (uint)hull[(i + 1) % CountHullKnots].pointsIndex;
                mesh.BoundElementsMark[i] = 0;
                mesh.BoundKnots[i] = hull[i].pointsIndex;
                mesh.BoundKnotsMark[i] = 0;
            }
            return mesh;
        }

    }
}
