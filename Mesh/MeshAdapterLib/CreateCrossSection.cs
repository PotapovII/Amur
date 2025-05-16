namespace MeshAdapterLib
{
    using System.Collections.Generic;

    using TriangleNet;
    using TriangleNet.Meshing;
    using TriangleNet.Geometry;
    using TriangleNet.Smoothing;

    using MeshLib;
    using MemLogLib;
    using GeometryLib;
    using CommonLib.Function;
    using MeshGeneratorsLib.StripGenerator;

    public class CreateCrossSection
    {
        /// <summary>
        /// Общий метод для работы с Tri Net генератором
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static MeshNet Create(IPolygon polygon, QualityMeshNetOptions op)
        {
            ConstraintOptions options = op.options;
            QualityOptions quality = op.quality;
            MeshNet mesh = null;
            if (op.sweepLine == null)
                mesh = (MeshNet)(polygon.Triangulate(options, quality));
            else
                mesh = (MeshNet)polygon.Triangulate(options, quality, op.sweepLine);
            for (int i = 0; i < op.CountSmooth; i++)
            {
                var smoother = new SimpleSmoother();
                smoother.Smooth(mesh);
            }
            if (op.Refine == true)
            {
                mesh.Refine(quality, true);
            }
            return mesh;
        }
        /// <summary>
        /// Создание сетки с придонным слоем для створа канала
        /// </summary>
        /// <param name="mesh">сеетка слоя</param>
        /// <param name="Contur">контур области над слоем</param>
        public static void GetMesh(QualityMeshNetOptions op, ref TriMesh smesh,
                                   ref IDigFunction GeometryWet,
                                   ref IDigFunction GeometryIn, 
                                   HKnot[] Contur, int SegmentSplitting = 1)
        {
            // создать контур
            IPolygon polygon = new Polygon();
            List<Vertex> Points = new List<Vertex>();
            List<int> Markers = new List<int>();
            for (int i = 0; i < Contur.Length; i++)
            {
                Points.Add(new Vertex(Contur[i].X, Contur[i].Y));
                Markers.Add(Contur[i].marker);
            }
            polygon.Add(new Contour(Points, Markers));
            // генерация сетки контуру
            MeshNet meshDel1 = Create(polygon, op);
            TriMesh mesh_cs = MeshAdapter.ConvertMeshNetToTriMesh(meshDel1);
            smesh.Add(mesh_cs);
        }

        /// <summary>
        /// Создание сетки для створа канала с придонным слоем
        /// </summary>
        /// <param name="Geometry"></param>
        /// <param name="WaterLevel"></param>
        /// <param name="CountBed"></param>
        /// <returns></returns>
        public static TriMesh GetMeshCS(QualityMeshNetOptions op, IDigFunction Geometry, 
                                      double WaterLevel, int CountBed, 
                                      ref IDigFunction GeometryWet,
                                      ref IDigFunction GeometryIn,
                                      bool renumber = true)
        {
            SMeshGenerator sg = new SMeshGenerator();
            HKnot[] Contur = null;
            // сетка придонного слоя
            TriMesh smesh = sg.CreateMeshTupe(WaterLevel, Geometry, CountBed, ref Contur);
            GetMesh(op, ref smesh, ref GeometryWet,ref GeometryIn, Contur);
            GeometryWet = new DigFunction(sg.bx, sg.by, "Дно мокрое");
            GeometryIn = new DigFunction(sg.sx, sg.sy, "Штамп дна ");
            double[] Y = smesh.GetCoords(1);
            for (int be = 0; be < smesh.BoundElems.Length; be++)
            {
                double ybe = 0.5*(Y[smesh.BoundElems[be].Vertex1] + Y[smesh.BoundElems[be].Vertex2]);
                if (MEM.Equals(ybe, WaterLevel) == true)
                    smesh.BoundElementsMark[be] = 2;
            }
            return smesh;
        }
    }
}
