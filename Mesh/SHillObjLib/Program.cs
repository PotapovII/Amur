namespace SHillObjLib
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections.Generic;
    using CommonLib.Geometry;
    class Program
    {
        static void Main(string[] args)
        {
            Random randy = new Random(138);

            List<HPoint> points = new List<HPoint>();

            if (args.Length == 0)
            {
                // Generate random points.
                SortedDictionary<int, Point> ps = new SortedDictionary<int, Point>();
                for (int i = 0; i < 100000; i++)
                {
                    int x = randy.Next(100000);
                    int y = randy.Next(100000);
                    points.Add(new HPoint(x, y));
                }
            }
            else
            {
                // Считайте файл точек, используемый программой Delaunay Triangulation Tester DTT
                // (See http://gemma.uni-mb.si/dtt/)
                using (StreamReader reader = new StreamReader(args[0]))
                {
                    int count = int.Parse(reader.ReadLine());
                    for (int i = 0; i < count; i++)
                    {
                        string line = reader.ReadLine();
                        string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        points.Add(new HPoint(double.Parse(split[0]), double.Parse(split[1])));
                    }
                }
            }

            // Запишите пункты в формате, подходящем для DTT
            using (StreamWriter writer = new StreamWriter("Triangulation c#.pnt"))
            {
                writer.WriteLine(points.Count);
                for (int i = 0; i < points.Count; i++)
                    writer.WriteLine(String.Format("{0},{1}", points[i].x, points[i].y));
            }
            // Запись набора данных, который для триангуляции
            MeshGeneratorSHillObj angulator = new MeshGeneratorSHillObj();

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Hull hull = null;
            List<Triad> triangles = angulator.Triangulation(points.ToArray(), true, ref hull);
            watch.Stop();

            Debug.WriteLine(watch.ElapsedMilliseconds + " ms");

            // Запишите результаты триангуляции в формате, подходящем для DTT
            using (StreamWriter writer = new StreamWriter("Triangulation c#.dtt"))
            {
                writer.WriteLine(triangles.Count.ToString());
                for (int i = 0; i < triangles.Count; i++)
                {
                    Triad t = triangles[i];
                    writer.WriteLine(string.Format("{0}: {1} {2} {3} - {4} {5} {6}",
                        i + 1,
                        t.a, t.b, t.c,
                        t.ab + 1, t.bc + 1, t.ac + 1));
                }
            }
        }
    }
}
