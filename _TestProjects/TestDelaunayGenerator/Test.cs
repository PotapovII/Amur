using CommonLib;
using CommonLib.Geometry;
using DelaunayGenerator;
using GeometryLib.Aalgorithms;
using GeometryLib.Vector;
using MeshLib;
using RenderLib;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestDelaunayGenerator
{
    public class Test
    {
        IHPoint[] points = null;
        public Test() { }
        public void CreateRestArea(int idx)
        {
            const int N = 450;
            double h = 1.0 / (N - 1);
            switch (idx)
            {
                case 0:
                    points = new IHPoint[5]
                    {
                        new HPoint(0, 0),
                        new HPoint(1, 0),
                        new HPoint(1, 1),
                        new HPoint(0, 1),
                        new HPoint(0.5, 0.5)
                    };
                    break;
                case 1:
                    points = new IHPoint[N * N];
                    for (int i = 0; i < N; i++)
                        for (int j = 0; j < N; j++)
                            points[i * N + j] = new HPoint(h * i, h * j);
                    break;
                case 2:
                    points = new IHPoint[N * N];
                    for (int i = 0; i < N; i++)
                    {
                        double hx = h - (h / 3 * i) / N;
                        for (int j = 0; j < N; j++)
                            points[i * N + j] = new HPoint(h * i, hx * j);
                    }
                    break;
                case 3:
                    var width = 100;
                    var height = 100;
                    List<Vector2> samples = CircleDelaunayGenerator.SampleCircle(new Vector2(width / 2, height / 3), 220, 5);
                    points = new IHPoint[samples.Count];
                    for (int i = 0; i < samples.Count; i++)
                        points[i] = new HPoint(samples[i].X, samples[i].Y);


                    break;
            }
        }
        public void Run()
        {

            DelaunayMeshGenerator delaunator = new DelaunayMeshGenerator();
            delaunator.Generator(points);
            IMesh mesh = delaunator.CreateMesh();

            IConvexHull ch = new ConvexHull();
           // ch.FindHull(points, )
            ShowMesh(mesh);
        }

        protected void ShowMesh(IMesh mesh)
        {
            if (mesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
                Form form = new ViForm(data);
                form.ShowDialog();
            }
        }

    }
}
