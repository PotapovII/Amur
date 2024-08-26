namespace MeshAdapterLib
{
    using System.IO;
    using TriangleNet.Geometry;
    public static class PolygonAdapter
    {
        /// <summary>
        /// Сохранить полигон в формате .poly
        /// </summary>
        /// <param name="fileName">имя файла</param>
        /// <param name="cloudPoints">полигон</param>
        public static void Save(IPolygon cloudPoints, string fileName = "test.poly")
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                // Точки
                var points = cloudPoints.Points;
                int CountAttributes = points[0].attributes.Length;
                file.WriteLine("{0} {1} {2} {3}", points.Count, 2, CountAttributes, 1);
                for (int i = 0; i < points.Count; i++)
                {
                    file.Write("{0} {1} {2} ", i + 1, points[i].X, points[i].Y);
                    for (int j = 0; j < CountAttributes; j++)
                        file.Write("{0} ", points[i].attributes[j]);
                    file.WriteLine(points[i].Label);
                }
                // Сегменты
                var segs = cloudPoints.Segments;
                file.WriteLine("{0} {1}", segs.Count, 1);
                for (int i = 0; i < segs.Count; i++)
                {
                    file.WriteLine("{0} {1} {2} {3} ", i + 1, segs[i].P0 + 1, segs[i].P1 + 1, segs[i].Label);
                }
                file.WriteLine("0");
                file.Close();
            }
        }
    }
}
