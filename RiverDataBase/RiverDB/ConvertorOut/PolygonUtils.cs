//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.12.2023 Потапов И.И.
//---------------------------------------------------------------------------
#define USE_ATTRIBS
namespace RiverDB.ConvertorOut
{
    using GeometryLib;
    using GeometryLib.Areas;
    using RiverDB.Convertors;
    using TriangleNet.Geometry;

    using System;
    using System.IO;
    using System.Data;
    using System.Collections.Generic;
    using TriangleNet;
    using TriangleNet.Meshing;
    using TriangleNet.Topology;
    using GeometryLib.Locators;
    using CommonLib.Areas;
    using MeshLib;
    using MeshAdapterLib;
    using CommonLib;
    using CommonLib.Mesh;
    using TriangleNet.Tools;
    using System.Linq;
    using MemLogLib;

    /// <summary>
    /// Класс для вывода полигона в фломате *.poly
    /// </summary>
    public class PolygonUtils
    {
        /// <summary>
        /// Количество атрибутов
        /// </summary>
        public static int CountAttributes = 5;
        /// <summary>
        /// Имена атрибутов
        /// </summary>
        public static string[] ArtNames =
            {
                "Глубина",
                "Срез.Глубина",
                "Температура",
                "Скорость",
                "Курс"
            };
        /// <summary>
        /// Сохранить полигон
        /// </summary>
        /// <param name="pointsTable"></param>
        /// <param name="FileName"></param>
        public static void Save(DataTable pointsTable, string FileName)
        {
            using (StreamWriter sw = new StreamWriter(FileName))
            {
                int marker = 1;
                int buf = pointsTable.Rows.Count;
                string FileEXT = Path.GetExtension(FileName);
                if (FileEXT == ".node")
                {
                    sw.WriteLine("{0} 2 1 0", buf);
                    buf = 0;
                    foreach (DataRow dr in pointsTable.Rows)
                    {
                        double xV = Convert.ToDouble(dr["knot_longitude"]);
                        double yV = Convert.ToDouble(dr["knot_latitude"]);
                        double H = Convert.ToDouble(dr["knot_fulldepth"]);
                        double tV = Convert.ToDouble(dr["knot_temperature"]);
                        DateTime DTk = Convert.ToDateTime(dr["knot_datetime"]);
                        double Hg = Convert.ToDouble(dr["experiment_waterlevel"]) / 100.0;
                        double Ho = H - Hg;
                        buf++;
                        sw.WriteLine("{0}  {1}  {2}  {3}  {4}", buf, xV, yV, Ho, marker);
                    }
                }
                if (FileEXT == ".bed")
                {
                    buf = 0;

                    double kd = 0.1;
                    foreach (DataRow dr in pointsTable.Rows)
                    {
                        double xV = Convert.ToDouble(dr["knot_longitude"]);
                        double yV = Convert.ToDouble(dr["knot_latitude"]);
                        double H = Convert.ToDouble(dr["knot_fulldepth"]);
                        double tV = Convert.ToDouble(dr["knot_temperature"]);
                        DateTime DTk = Convert.ToDateTime(dr["knot_datetime"]);
                        double Hg = Convert.ToDouble(dr["experiment_waterlevel"]) / 100.0;
                        double Ho = H - Hg;
                        double LatX = Convertor_SK42_to_WGS84.SK42BTOX(xV, yV, 10);
                        double LonY = Convertor_SK42_to_WGS84.SK42LTOY(xV, yV, 10);
                        buf++;
                        sw.WriteLine("{0}  {1}  {2}  {3} {4}", buf, LatX, LonY, Ho, kd);
                    }
                    sw.WriteLine();
                    sw.WriteLine("no more nodes.");
                    sw.WriteLine();
                    sw.WriteLine("no more breakline segments.");
                }
                sw.Close();
            }

        }




        /// <summary>
        /// Сохранить полигон
        /// </summary>
        /// <param name="pointsTable"></param>
        /// <param name="FileName"></param>
        public static void SaveVertexFromMesh(MeshNet meshRiver, string FileName)
        {
            IFEMesh bmesh = null;
            double[][] values = null;
            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, meshRiver, Direction.toRight);
            //int shift = 1;
            int shift = 0;
            double[] x = bmesh.GetCoords(0);
            double[] y = bmesh.GetCoords(1);
            double minX = 0, maxX = 0, minY = 0, maxY = 0;
            bmesh.MinMax(0, ref minX, ref maxX);
            bmesh.MinMax(1, ref minY, ref maxY);
            double midleX = 0.5 * (minX + maxX);
            double midleY = 0.5 * (minY + maxY);
            int Label = 0;
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < x.Length; i++)
            {
                Vertex vertex = new Vertex(x[i] - midleX, y[i] - midleY, Label);
                vertex.ID = i;
                vertices.Add(vertex);
            }
            TwoElement[] elems = bmesh.GetBoundElems();
            for (int i = 0; i < elems.Length; i++)
            {
                vertices[(int)elems[i].Vertex1].Label = 1;
                vertices[(int)elems[i].Vertex2].Label = 1;
            }
            using (StreamWriter sw = new StreamWriter(FileName))
            {
                string FileEXT = Path.GetExtension(FileName);
                if (FileEXT == ".node")
                {
                    sw.WriteLine("{0} 2 {1} 0", bmesh.CountKnots, 0);
                    for (int i = 0; i < x.Length; i++)
                    {
                        string line = (vertices[i].ID + shift).ToString();
                        line += " " + vertices[i].X.ToString();
                        line += " " + vertices[i].Y.ToString();
                        //for (int i = 0; i < attributesLength; i++)
                        //    line += " " + vertex.attributes[i].ToString();
                        line += " " + vertices[i].Label.ToString();
                        sw.WriteLine(line);
                    }
                    TwoElement[] belems = bmesh.GetBoundElems();
                    sw.WriteLine("{0} 1", belems.Length);
                    for (int i = 0; i < elems.Length; i++)
                    {
                        string line = (i + shift).ToString();
                        line += " " + (elems[i].Vertex1 + shift).ToString();
                        line += " " + (elems[i].Vertex2 + shift).ToString();
                        line += " 1";
                        sw.WriteLine(line);
                    }
                    sw.WriteLine("0");
                }
                sw.Close();
            }

        }



        /// <summary>
        /// Сохранить полигон
        /// </summary>
        /// <param name="pointsTable"></param>
        /// <param name="FileName"></param>
        public static void Save(IPolygon polygon, string FileName)
        {
            using (StreamWriter sw = new StreamWriter(FileName))
            {
                int buf = polygon.Points.Count;
                int attributesLength = 0;
                if (polygon.Points[0].attributes != null)
                    attributesLength = polygon.Points[0].attributes.Length;
                string FileEXT = Path.GetExtension(FileName);
                if (FileEXT == ".node")
                {
                    sw.WriteLine("{0} 2 {1} 0", buf, attributesLength);
                    buf = 0;
                    foreach (Vertex vertex in polygon.Points)
                    {
                        string line = buf.ToString();
                        line += " " + vertex.X.ToString();
                        line += " " + vertex.Y.ToString();
                        for (int i = 0; i < attributesLength; i++)
                            line += " " + vertex.attributes[i].ToString();
                        line += " " + vertex.Label.ToString();
                        sw.WriteLine(line);
                        buf++;
                    }
                }
                if (FileEXT == ".bed")
                {
                    buf = 0;
                    string sks = "0.01";
                    if (attributesLength > 0)
                    {
                        foreach (Vertex vertex in polygon.Points)
                        {
                            string line = buf.ToString();
                            line += " " + vertex.X.ToString();
                            line += " " + vertex.Y.ToString();
                            line += " " + vertex.attributes[0].ToString(); // H
                            line += " " + sks; // ks
                            buf++;
                            sw.WriteLine(line);
                        }
                        sw.WriteLine();
                        sw.WriteLine("no more nodes.");
                        sw.WriteLine();
                        sw.WriteLine("no more breakline segments.");
                    }
                }
                sw.Close();
            }
        }


        public static void AddBoundarySimpleCountur(IMArea Area, ref IPolygon cloudPoints)
        {
            int pointsCount = cloudPoints.Points.Count;
            // генерация сетки по облаку точек
            int mark = 1;
            int pointsCurCount = pointsCount;
            // Проход по контурам фигур
            for (int i = 0; i < Area.Count; i++)
            {
                IMFigura fig = Area.Figures[i];
                List<IMPoint> figPoints = fig.Points;
                for (int k = 0; k < figPoints.Count; k++)
                {
                    IMPoint p = figPoints[k];
                    CloudKnot knot = p.Point as CloudKnot;
                    var v = PolygonUtils.ConvertCloudKnotToVertex(pointsCurCount++, mark + i, knot);
                    cloudPoints.Add(v);
                }
            }
            pointsCurCount = pointsCount;
            var points = cloudPoints.Points;
            for (int i = 0; i < Area.Count; i++)
            {
                IMFigura fig = Area.Figures[i];
                List<IMSegment> segment = fig.Segments;
                mark = 1;
                for (int s = 0; s < segment.Count; s++)
                {
                    int knotA = pointsCurCount + s;
                    int knotB = pointsCurCount + (s + 1) % segment.Count;
                    Segment seg = new Segment(points[knotA], points[knotB], mark);
                    cloudPoints.Add(seg);
                }
                pointsCurCount += segment.Count;
            }
        }
        public static IPolygon CreatePolygonFromPolygon(IPolygon cloud)
        {
            for (int s = 0; s < cloud.Segments.Count; s++)
            {
                ISegment segment = cloud.Segments[s];
                Vertex vA = segment.GetVertex(0);
                Vertex vB = segment.GetVertex(1);
                cloud.Points[vA.ID].Label = 1;
                cloud.Points[vB.ID].Label = 1;
            }

            IPolygon polygon = new Polygon();
            foreach (Vertex v in cloud.Points)
            {
                Vertex vertex = new Vertex(v.X, v.Y, v.Label);
                polygon.Add(vertex);
            }

            for (int s = 0; s < cloud.Segments.Count; s++)
            {
                Vertex vA = cloud.Segments[s].GetVertex(0);
                Vertex vB = cloud.Segments[s].GetVertex(1);
                int Label = cloud.Segments[s].Label;
                Vertex vertexA = new Vertex(vA.X, vA.Y, vA.Label);
                Vertex vertexB = new Vertex(vB.X, vB.Y, vB.Label);
                Segment seg = new Segment(vertexA, vertexB, Label);
                polygon.Add(seg);
            }
            return polygon;
        }

        public static IPolygon CreatePolygonFromPolygon(MeshNet meshRiver)
        {
            IFEMesh bmesh = null;
            double[][] values = null;
            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, meshRiver, Direction.toRight);

            double[] x = bmesh.GetCoords(0);
            double[] y = bmesh.GetCoords(1);
            int Label = 0;

            IPolygon polygon = new Polygon();
            for (int i = 0; i < x.Length; i++)
            {
                Vertex vertex = new Vertex(x[i], y[i], Label);
                vertex.ID = i;
                polygon.Add(vertex);
            }
            TwoElement[] elems = bmesh.GetBoundElems();
            for (int i = 0; i < elems.Length; i++)
            {
                Vertex vA = polygon.Points[(int)elems[i].Vertex1];
                Vertex vB = polygon.Points[(int)elems[i].Vertex2];
                vA.Label = 1;
                vB.Label = 1;
                Vertex vertexA = new Vertex(vA.X, vA.Y, vA.Label);
                Vertex vertexB = new Vertex(vB.X, vB.Y, vB.Label);
                Segment seg = new Segment(vertexA, vertexB, Label);
                polygon.Add(seg);
            }
            return polygon;
        }

        public static void AddBoundaryBaseCountur(IMArea Area, ref IPolygon cloudPoints)
        {

            // количество узлов облака
            int pointsCount = cloudPoints.Points.Count;
            // количество узлов облака перед добавления в контур
            int pointsCurCount = pointsCount;
            #region Узлы контура
            // генерация сетки по облаку точек
            int mark = 1;
            // Проход по контурам фигур
            for (int i = 0; i < Area.Count; i++)
            {
                IMFigura fig = Area.Figures[i];
                List<IMPoint> figPoints = fig.Points;
                for (int k = 0; k < figPoints.Count; k++)
                {
                    IMPoint p = figPoints[k];
                    CloudKnot knot = p.Point as CloudKnot;
                    cloudPoints.Add(ConvertCloudKnotToVertex(pointsCurCount++, mark + i, knot));
                }
            }
            #endregion
            #region Сегменты контура
            pointsCurCount = pointsCount;
            var points = cloudPoints.Points;
            for (int i = 0; i < Area.Count; i++)
            {
                IMFigura fig = Area.Figures[i];
                List<IMSegment> segment = fig.Segments;
                for (int s = 0; s < segment.Count; s++)
                {
                    mark = segment[s].Marker;
                    int knotA = pointsCurCount + s;
                    int knotB = pointsCurCount + (s + 1) % segment.Count;
                    Segment seg = new Segment(points[knotA], points[knotB], mark);
                    cloudPoints.Add(seg);
                }
                pointsCurCount += segment.Count;
            }
            #endregion
        }

        /// <summary>
        /// Интерполяция атрибутов без хеширования
        /// </summary>
        /// <param name="sMmesh">сетка источник</param>
        /// <param name="meshRiver">расчетная сетка источник</param>
        public static void CalkAttributesOld(MeshNet sMmesh, ref MeshNet meshRiver)
        {
            try
            {
                double[] N = null, DN_x = null, DN_y = null;
                // Подготовка контейнера
                foreach (var vKnot in meshRiver.Vertices)
                {
                    // FE КЭ сетки
                    foreach (Triangle elem in sMmesh.Triangles)
                    {
                        CloudKnot[] clp =
                        {
                                            ConvertVertexToCloudKnot(elem.GetVertex(0)),
                                            ConvertVertexToCloudKnot(elem.GetVertex(1)),
                                            ConvertVertexToCloudKnot(elem.GetVertex(2))
                        };
                        CloudKnot knot = ConvertVertexToCloudKnot(vKnot);
                        if (TriLocator.Locator(clp, ref knot) == true)
                        {
                            TriLocator.CalkFF(clp, knot, ref N, ref DN_x, ref DN_y);
                            for (int ki = 0; ki < vKnot.Attributes.Length; ki++)
                            {
                                vKnot.Attributes[ki] = 0;
                                for (int kk = 0; kk < N.Length; kk++)
                                {
                                    vKnot.Attributes[ki] += N[kk] * clp[kk].Attributes[ki];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Интерполяция атрибутов с поиском КЭ по R-дереву описанных квадратов
        /// </summary>
        /// <param name="sMmesh">сетка источник</param>
        /// <param name="meshRiver">расчетная сетка источник</param>
        public static int CalkAttributes(MeshNet sMmesh, MeshNet meshBase, ref MeshNet meshRiver)
        {
            int artCaout = 0;
            try
            {
                ITriangle elem = null;
                double[] N = null, DN_x = null, DN_y = null;
                // строим R-дерево для тереугольников сетки свойств
                TriangleQuadTree tree = new TriangleQuadTree(sMmesh, 5, 2);
                Console.WriteLine("Создано хеш дерево для фильтрованной сетки источника");
                TriangleQuadTree treeBase = null; 
                // Подготовка контейнера
                foreach (var vKnot in meshRiver.Vertices)
                {
                    CloudKnot knot = ConvertVertexToCloudKnot(vKnot);
                    elem = tree.Query(vKnot.X, vKnot.Y);
                    if (elem != null)
                    {
                        CloudKnot[] clp =
                        {
                               ConvertVertexToCloudKnot(elem.GetVertex(0)),
                               ConvertVertexToCloudKnot(elem.GetVertex(1)),
                               ConvertVertexToCloudKnot(elem.GetVertex(2))
                        };
                        if (TriLocator.Locator(clp, ref knot) == true)
                        {
                            TriLocator.CalkFF(clp, knot, ref N, ref DN_x, ref DN_y);
                            for (int ki = 0; ki < vKnot.Attributes.Length; ki++)
                            {
                                vKnot.Attributes[ki] = 0;
                                for (int kk = 0; kk < N.Length; kk++)
                                {
                                    vKnot.Attributes[ki] += N[kk] * clp[kk].Attributes[ki];
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Ошибка найденный КЭ не содержит точку!!!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Точка с координатами " + knot.ToString() + " не найдена в трехугольниках хеш дерева");
                        if (treeBase == null)
                        {
                            treeBase = new TriangleQuadTree(meshBase, 5, 2);
                            Console.WriteLine("Создано хеш дерево для расширеной сетки источника");
                        }
                        artCaout++;
                        foreach (var pelem in meshBase.Triangles)
                        //var pelem = treeBase.Query(knot.X, knot.Y);
                        //if (pelem != null)
                        {
                            CloudKnot[] clp =
                            {
                               ConvertVertexToCloudKnot(pelem.GetVertex(0)),
                               ConvertVertexToCloudKnot(pelem.GetVertex(1)),
                               ConvertVertexToCloudKnot(pelem.GetVertex(2))
                            };
                            if (TriLocator.Locator(clp, ref knot) == true)
                            {
                                artCaout--;
                                TriLocator.CalkFF(clp, knot, ref N, ref DN_x, ref DN_y);
                                for (int ki = 0; ki < vKnot.Attributes.Length; ki++)
                                {
                                    vKnot.Attributes[ki] = 0;
                                    for (int kk = 0; kk < N.Length; kk++)
                                        vKnot.Attributes[ki] += N[kk] * clp[kk].Attributes[ki];
                                }
                                Console.WriteLine("Точка с координатами " + knot.ToString() + " найдена в сетке");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                artCaout = -1;
            }
            return artCaout;
        }
        
        /// <summary>
        /// Добавиь граничный контур
        /// </summary>
        /// <param name="cloudPoints"></param>
        public static bool AddBoundaryCountur(IMArea Area, IPolygon BCloudPoints,
                                                ref IPolygon cloudPoints, ref List<SegmentInfo> segInfo,
                                                ConstraintOptions options, QualityOptions quality)
        {
            try
            {
                Vertex V = cloudPoints.Points[0];
                double depth = V.attributes[0];
                double sDepth = V.attributes[1];
                // глубина срезки
                double Hs = V.attributes[0] - V.attributes[1];
                //V.attributes[1] = V.attributes[0] - Hs;
                MeshNet uMmesh = null;
                ICollection<Triangle> Triangles;
                double[] N = null, DN_x = null, DN_y = null;
                // количество узлов облака
                int pointsCount = cloudPoints.Points.Count;
                // количество узлов облака перед добавления в контур
                int ID = pointsCount + 1;
                // генерация сетки по облаку точек
                //  List<CloudKnot> knots = gdI_EditControlClouds1.Conturs;
                int mark = 1;
                segInfo.Clear();
                // Проход по контурам фигур
                for (int area = 0; area < Area.Count; area++)
                {
                    // Список для узлов контура для текущей фигуры
                    List<Vertex> newPoints = new List<Vertex>();
                    // Список для типов границ для каждой созданной точки контра
                    List<int> segs = new List<int>();
                    // текущая фигура
                    IMFigura fig = Area.Figures[area];
                    #region вершины контура
                    // Сегменты фигуры
                    List<IMSegment> segments = fig.Segments;
                    for (int i = 0; i < segments.Count; i++)
                    {
                        int ii = (i + 1) % segments.Count;
                        int mi = (segments.Count + i - 1) % segments.Count;
                        // маркер границы сегмента
                        int Marker = segments[i].Marker;
                        CloudKnot knot = null;
                        if (segments[i].CountKnots == 2)
                        {
                            knot = (CloudKnot)segments[i].pointA.Point;
                            Vertex av = ConvertCloudKnotToVertex(ID, Marker, knot);
                            av.attributes[AtrCK.idx_Hs] = av.attributes[AtrCK.idx_H] - Hs;
                            av.attributes[AtrCK.idx_Ice] = fig.Ice;
                            av.attributes[AtrCK.idx_ks] = fig.ks;
                            newPoints.Add(av); 
                            ID++;
                            segs.Add(Marker);
                        }
                        else
                        {
                            CloudKnot pA = (CloudKnot)segments[i].pointA.Point;
                            CloudKnot pB = (CloudKnot)segments[i].pointB.Point;
                            pA.Attributes[AtrCK.idx_Ice] = fig.Ice;
                            pA.Attributes[AtrCK.idx_ks] = fig.ks;
                            pB.Attributes[AtrCK.idx_Ice] = fig.Ice;
                            pB.Attributes[AtrCK.idx_ks] = fig.ks;
                            int ida = area == 0 ? 1 : -1;
                            // Информация о сегменте 
                            SegmentInfo si = new SegmentInfo(ida, Marker, pA, pB);
                            double[] Attributes = new double[pA.Attributes.Length];
                            double ds = 1.0 / (segments[i].CountKnots - 1);
                            int startPoint = 0;
                            int segmentsCountKnots;

                            if (segments[i].Marker > 1)
                            {
                                segmentsCountKnots = segments[i].CountKnots - 1;
                                startPoint = 1;
                            }
                            else
                            {
                                if (segments[i].Marker == 1 && segments[ii].Marker > 1)
                                    segmentsCountKnots = segments[i].CountKnots;
                                else
                                {
                                    segmentsCountKnots = segments[i].CountKnots - 1;
                                }
                            }
                            for (int j = startPoint; j < segmentsCountKnots; j++)
                            {
                                // интерполяция по берегу
                                knot = CloudKnot.Interpolation(pA, pB, j * ds, Marker);
                                if (Marker > 1 || fig.FType == FigureType.FigureSubArea)
                                // if (Marker > 1)
                                {
                                    // интерполяция атрибутов в области
                                    if (uMmesh == null)
                                    {
                                        uMmesh = (MeshNet)BCloudPoints.Triangulate(options, quality);
                                    }
                                    else
                                    {
                                        // FE КЭ сетки 
                                        Triangles = uMmesh.Triangles;
                                        // заменить алгоритм на хешь поиск
                                        foreach (Triangle elem in Triangles)
                                        {
                                            CloudKnot[] clp =
                                            {
                                                ConvertVertexToCloudKnot(elem.GetVertex(0)),
                                                ConvertVertexToCloudKnot(elem.GetVertex(1)),
                                                ConvertVertexToCloudKnot(elem.GetVertex(2))
                                            };
                                            if (TriLocator.Locator(clp, ref knot) == true)
                                            {
                                                TriLocator.CalkFF(clp, knot, ref N, ref DN_x, ref DN_y);
                                                for (int ki = 0; ki < knot.Attributes.Length; ki++)
                                                {
                                                    knot.Attributes[ki] = 0;
                                                    for (int kk = 0; kk < N.Length; kk++)
                                                    {
                                                        knot.Attributes[ki] += N[kk] * clp[kk].Attributes[ki];
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                Vertex vertex = ConvertCloudKnotToVertex(ID, Marker, knot);
                                vertex.attributes[1] = vertex.attributes[0] - Hs;
                                newPoints.Add(vertex); ID++;
                                segs.Add(Marker);
                            }
                            segInfo.Add(si);
                        }
                    }
                    // добовляем вершины контура в полигон
                    foreach (var v in newPoints)
                        cloudPoints.Add(v);
                    // Если контур фигуоы дырка находим точку в фигуре
                    // и маркируем контур в полигоне
                    if (fig.FType == FigureType.FigureHole)
                    {
                        int limit = 5;
                        double eps = 2e-5;
                        Point p = Contour.FindPointInPolygon(newPoints, limit, eps);
                        cloudPoints.Holes.Add(p);
                    }
                    #endregion
                    #region Сегменты контура
                    // пишем контур в полигон
                    for (int s = 0; s < newPoints.Count; s++)
                    {
                        mark = segs[s];
                        Vertex a = newPoints[s];
                        Vertex b = newPoints[(s + 1) % newPoints.Count];
                        Segment seg = new Segment(a, b, mark);
                        cloudPoints.Add(seg);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            try
            {
                // Проход по контурам фигур
                for (int area = 0; area < Area.Count; area++)
                {
                    IMFigura fig = Area.Figures[area];
                    if (fig.FType == FigureType.FigureSubArea)
                    {
                        List<Vertex> Points = cloudPoints.Points;
                        foreach (var v in Points)
                        {
                            if (fig.Contains(v.X, v.Y) == true)
                            {
                                v.Attributes[AtrCK.idx_ks] = fig.ks;
                                v.Attributes[AtrCK.idx_Ice] = fig.Ice;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            int inOut = 0;
            foreach (SegmentInfo seg in segInfo)
                if (seg.type > 1) inOut++;
            if (inOut < 2)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Добавиь граничный контур
        /// </summary>
        /// <param name="cloudPoints"></param>
        public static void CreateBoundaryCountur(IMArea Area, ref IPolygon cloudPoints, ref List<SegmentInfo> segInfo)
        {
            try
            {
                // количество узлов облака
                int pointsCount = cloudPoints.Points.Count;
                // количество узлов облака перед добавления в контур
                int ID = pointsCount + 1;
                // генерация сетки по облаку точек
                //  List<CloudKnot> knots = gdI_EditControlClouds1.Conturs;
                int mark = 1;
                segInfo.Clear();
                // Проход по контурам фигур
                for (int area = 0; area < Area.Count; area++)
                {
                    // Запоминаем узлы контура для текущей фигуры
                    List<Vertex> newPoints = new List<Vertex>();
                    // Запоминаем типы границ
                    List<int> segs = new List<int>();
                    // текущая фигура
                    IMFigura fig = Area.Figures[area];
                    #region вершины контура
                    //  List<IMSegment> bsegments = new List<IMSegment>();
                    // Сегменты фигуры
                    List<IMSegment> segments = fig.Segments;
                    for (int i = 0; i < segments.Count; i++)
                    {
                        // маркер границы сегмента
                        int Marker = segments[i].Marker;
                        CloudKnot knot = null;
                        if (segments[i].CountKnots == 2)
                        {
                            knot = (CloudKnot)segments[i].pointA.Point;
                            newPoints.Add(ConvertCloudKnotToVertex(ID, Marker, knot)); ID++;
                            segs.Add(Marker);
                        }
                        else
                        {
                            CloudKnot pA = (CloudKnot)segments[i].pointA.Point;
                            CloudKnot pB = (CloudKnot)segments[i].pointB.Point;
                            int ida = area == 0 ? 1 : -1;
                            // Информация о сегменте 
                            SegmentInfo si = new SegmentInfo(ida, Marker, pA, pB);
                            double[] Attributes = new double[pA.Attributes.Length];
                            double ds = 1.0 / (segments[i].CountKnots - 1);
                            for (int j = 0; j < segments[i].CountKnots - 1; j++)
                            {
                                // интерполяция по берегу
                                double N1 = 1 - j * ds;
                                double N2 = j * ds;
                                double x = pA.x * N1 + pB.x * N2;
                                double y = pA.y * N1 + pB.y * N2;
                                for (int k = 0; k < Attributes.Length; k++)
                                    Attributes[k] = pA.Attributes[k] * N1 + pB.Attributes[k] * N2;
                                
                                knot = new CloudKnot(x, y, Attributes, Marker);
                                Vertex vertex = ConvertCloudKnotToVertex(ID, Marker, knot);
                                newPoints.Add(vertex); ID++;
                                segs.Add(Marker);
                            }
                            segInfo.Add(si);
                        }
                    }
                    // добовляем вершины контура в полигон
                    foreach (var v in newPoints)
                        cloudPoints.Add(v);

                    // Если контур фигуоы дырка находим точку в фигуре
                    // и маркируем контур в полигоне
                    if (fig.FType == FigureType.FigureHole)
                    {
                        int limit = 5;
                        double eps = 2e-5;
                        Point p = Contour.FindPointInPolygon(newPoints, limit, eps);
                        cloudPoints.Holes.Add(p);
                    }
                    #endregion
                    #region Сегменты контура
                    // пишем контур в полигон
                    for (int s = 0; s < newPoints.Count; s++)
                    {
                        mark = segs[s];
                        Vertex a = newPoints[s];
                        Vertex b = newPoints[(s + 1) % newPoints.Count];
                        Segment seg = new Segment(a, b, mark);
                        cloudPoints.Add(seg);
                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Конвертер CloudKnot к Vertex 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="mark"></param>
        /// <param name="knot"></param>
        /// <param name="v"></param>
        public static Vertex ConvertCloudKnotToVertex(int ID, int Marker, CloudKnot knot)
        {
            Vertex v = new Vertex(knot.x, knot.y, Marker);
            v.ID = ID;
            MEM.Copy(ref v.attributes, knot.Attributes);
            return v;
        }
        /// <summary>
        /// Конвертер CloudKnot к Vertex 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="mark"></param>
        /// <param name="knot"></param>
        /// <param name="v"></param>
        public static CloudKnot ConvertVertexToCloudKnot(Vertex vertex)
        {
            CloudKnot knot = new CloudKnot(vertex.X, vertex.Y, vertex.Attributes, vertex.ID);
            return knot;
        }
    }
}
