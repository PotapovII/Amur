//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 правка  :   06.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshAdapterLib
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using GeometryLib;
    using MeshLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TriangleNet.Geometry;
    using TriangleNet.Meshing;
    using TriangleNet;
    using TriangleNet.Topology;
    using TriangleNet.IO;
    using TriangleNet.Smoothing;
    using CommonLib.Geometry;
    // Библиотека с триангулятором Делоне.
    // Мощная но медленная по сравнению с триангуляторами повторителями
    // и проекторами

    /// <summary>
    /// ОО: Конвертер формата TriangleNet.Mesh в формат TriMesh
    ///         с фронтальной перенумерацией узлов
    /// </summary>
    public class MeshAdapter
    {
        /// <summary>
        /// Создает контур области
        /// </summary>
        /// <param name="label">The boundary label.</param>
        /// <returns>A circular contour.</returns>
        public static List<Contour> CreateContour(double WaterLevel, double[] x, double[] y, int strech = 1)
        {
            List<Contour> contours = new List<Contour>();
            int N = x.Length - 1;
            int beginLeft;
            int beginRight;
            HPoint left = null;
            HPoint right = null;
            bool dryLeft = true;
            bool dryRight = true;
            //int index = 0;
            if (y[0] < WaterLevel) // левый берег затоплен
            {
                left = new HPoint(x[0], y[0]);
                dryLeft = false;
                beginLeft = 0;
            }
            else // левый берег сухой
            {
                beginLeft = 0;
                //-- нахождение "левой" точки берега-------------
                for (int i = 0; i < x.Length - 1; i++)
                {
                    if (((y[i] > WaterLevel) && (y[i + 1] <= WaterLevel)))// левый берег
                    {
                        double newX = (WaterLevel - y[i]) / (y[i + 1] - y[i]) * (x[i + 1] - x[i]) + x[i];
                        left = new HPoint(newX, WaterLevel);
                        beginLeft = i;
                        dryLeft = true;
                        break;
                    }
                }
            }
            if (y[N] < WaterLevel) // правый берег затоплен
            {
                right = new HPoint(x[N], y[N]);
                dryRight = false;
                beginRight = N;
            }
            else
            {
                beginRight = N;
                //-- нахождение "правой" точки берега-------------
                for (int i = N; i > 0; i--)
                {
                    if (((y[i] > WaterLevel) && (y[i - 1] <= WaterLevel)))// правый берег
                    {
                        double newX = (WaterLevel - y[i - 1]) / (y[i] - y[i - 1]) * (x[i] - x[i - 1]) + x[i - 1];
                        right = new HPoint(newX, WaterLevel);
                        beginRight = i;
                        break;
                    }
                }
            }
            int CountBed = beginRight - beginLeft + 1;
            // дно
            var pointsBed = new List<Vertex>();
            int label = 1;
            Vertex v0 = new Vertex(left.x, left.y, label);
            pointsBed.Add(v0);
            for (int i = beginLeft + 1; i < beginRight; i++)
            {
                pointsBed.Add(new Vertex(x[i], y[i], label));
            }
            Vertex vN = new Vertex(right.x, right.y, label);
            pointsBed.Add(vN);
            Contour contour = new Contour(pointsBed, label, true);
            contours.Add(contour);
            // симметрия справа
            if (dryRight == false)
            {
                label = 3;
                var pointsRB = new List<Vertex>();
                // глубина
                double H = WaterLevel - y[N];
                double NN = H / (x[N] - x[N - 1]);
                int CountH = (int)(NN);
                if (CountH < 5)
                    CountH = 5;
                double dy = H / CountH;
                for (int i = 1; i <= CountH; i++)
                {
                    double yi = right.y + i * dy;
                    pointsRB.Add(new Vertex(right.x, yi, label));
                }
                Contour contourRB = new Contour(pointsRB, label, true);
                contours.Add(contourRB);
            }
            // водная поверхность
            label = 2;
            var pointsWL = new List<Vertex>();
            int CountWL = (int)(CountBed / strech);
            if (CountWL < 5)
                CountWL = 5;
            double dx = (right.x - left.x) / (CountWL + 1);
            int begin = 1;
            int end = CountWL + 1;

            for (int i = begin; i < end; i++)
            {
                double xi = right.x - i * dx;
                pointsWL.Add(new Vertex(xi, WaterLevel, label));
            }
            Contour contourWL = new Contour(pointsWL, label, true);
            contours.Add(contourWL);
            // симметрия с лева
            if (dryLeft == false)
            {
                label = 4;
                var pointsLB = new List<Vertex>();
                // глубина
                double H = WaterLevel - y[0];
                double NN = H / (x[1] - x[0]);
                int CountH = (int)(NN);
                if (CountH < 5)
                    CountH = 5;
                double dy = H / CountH;
                for (int i = 0; i < CountH; i++)
                {
                    double yi = WaterLevel - i * dy;
                    pointsLB.Add(new Vertex(left.x, yi, label));
                }
                Contour contourLB = new Contour(pointsLB, label, true);
                contours.Add(contourLB);
            }
            return contours;
        }
        /// <summary>
        /// Создает полигон области
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static IPolygon CreatePoligon(double WaterLevel, double[] xx, double[] yy, int strech = 1)
        {
            IPolygon polygon = new Polygon();
            List<Contour> contours = CreateContour(WaterLevel, xx, yy, strech);
            return CreateArea.CreatePolygon(contours);
        }
        /// <summary>
        /// Генерация сетки
        /// </summary>
        /// <param name="WaterLevel"></param>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="quality"></param>
        /// <param name="options"></param>
        /// <param name="auto"></param>
        /// <returns></returns>
        public static TriMesh CreateMesh(double WaterLevel, double[] xx, double[] yy,
                        QualityOptions quality, ConstraintOptions options, bool auto = false)
        {
            TriMesh BedMesh = null;
            try
            {
                // получить контур живого сечения
                IPolygon polygon = CreatePoligon(WaterLevel, xx, yy);
                int Count = polygon.Points.Count;
                foreach (var e in polygon.Points)
                {
                    Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
                }
                MeshNet mesh = null;
                if (auto == true)
                {
                    double dx = xx[1] - xx[0];
                    double newLargestArea = 0.3 * dx * dx;
                    quality.MaximumArea = newLargestArea;
                    mesh = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
                    //var smoother1 = new SimpleSmoother();
                    //smoother1.Smooth(mesh);
                    //smoother1.Smooth(mesh);
                }
                else
                {
                    mesh = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
                }
                Direction direction = Direction.toRight;
                BedMesh = ConvertMeshNetToTriMesh(mesh, direction);
            }
            catch (Exception e)
            {
                string str = e.Message;
            }
            return BedMesh;
        }


        public static void CorrectorTriMesh(ref TriMesh mesh)
        {
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            int[] flagIdx = new int[X.Length];
            HKnot [] points = new HKnot[X.Length];
            int[] newIdx = new int[X.Length];
            for (int ii = 0; ii < X.Length; ii++)
            {
                points[ii] = new HKnot(X[ii], Y[ii], ii);
                newIdx[ii] = ii;
            }
            // с помощью оператора orderby
            var sortedVertex = from p in points
                               orderby p.X, p.Y
                               select p;
            HKnot old = null;
            int i = 0;
            List<(int, int)> list = new List<(int, int)>();
            foreach (var v in sortedVertex)
            {
                if (i > 0)
                {
                    if (MEM.Equals(v.X, old.X) == true &&
                        MEM.Equals(v.Y, old.Y) == true)
                    {
                        if (v.marker < old.marker)
                            list.Add((v.marker, old.marker));
                        else
                            list.Add((old.marker,v.marker));
                        flagIdx[v.marker] = -1;
                    }
                }
                Console.WriteLine("X = {0} Y = {1} ID = {2}", v.X, v.Y, v.marker);
                if (flagIdx[v.marker] != -1)
                    old = v;
                i++;
            }

            foreach (var v in list)
                newIdx[v.Item2] = newIdx[v.Item1];



            //for (int e = 0; e < flagIdx.Length; e++)
            //    flagIdx[e] = 0;
            //foreach (var v in list)
            //    flagIdx[v.Item2] = -1;
            //foreach (var v in list)
            //{
            //    Console.WriteLine("ID = {0} ID = {1}", v.Item1, v.Item2);
            //    newIdx[v.Item2] = newIdx[v.Item1];
            //    for (int e = v.Item2; e < newIdx.Length; e++)
            //        if (flagIdx[e] != -1)
            //            newIdx[e]--;
            //}

            //// координаты
            //mesh.CoordsX = new double[newIdx.Length - list.Count];
            //mesh.CoordsY = new double[newIdx.Length - list.Count];
            //for (int e = 0; e < mesh.CoordsY.Length; e++)
            //{
            //    mesh.CoordsX[e] = X[newIdx[e]];
            //    mesh.CoordsY[e] = Y[newIdx[e]];
            //}
            for (int e = 0; e < mesh.CountElements; e++)
            {
                mesh.AreaElems[e].Vertex1 = (uint)newIdx[mesh.AreaElems[e].Vertex1];
                mesh.AreaElems[e].Vertex2 = (uint)newIdx[mesh.AreaElems[e].Vertex2];
                mesh.AreaElems[e].Vertex3 = (uint)newIdx[mesh.AreaElems[e].Vertex3];
                Console.WriteLine("{3}: V1 = {0} V2 = {1}  V3 = {2}", 
                    mesh.AreaElems[e].Vertex1, 
                    mesh.AreaElems[e].Vertex2, 
                    mesh.AreaElems[e].Vertex3, e);
            }
            for (int e = 0; e < mesh.CountBoundElements; e++)
            {
                mesh.BoundElems[e].Vertex1 = (uint)newIdx[mesh.BoundElems[e].Vertex1];
                mesh.BoundElems[e].Vertex2 = (uint)newIdx[mesh.BoundElems[e].Vertex2];
            }
            for (int e = 0; e < mesh.CountBoundKnots; e++)
            {
                mesh.BoundKnots[e] = newIdx[mesh.BoundKnots[e]];
            }
        }
        /// <summary>
        /// Фронтальный перенумератор сетки
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static TriMesh ConvertMeshNetToTriMesh(IMeshNet mesh, Direction direction = Direction.toRight)
        {
            

            TriMesh BedMesh = new TriMesh();
            int ix, iy, jy;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;

            int CountKnots = (int)(Math.Sqrt(Vertices.Count)) + 2;
            List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
            for (int i = 0; i < CountKnots; i++)
                for (int j = 0; j < CountKnots; j++)
                    XMap[i, j] = new List<int>();

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            int MaxID = 0;
            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                if (e.X > MaxX) MaxX = e.X;
                if (e.X < MinX) MinX = e.X;
                if (e.Y > MaxY) MaxY = e.Y;
                if (e.Y < MinY) MinY = e.Y;
                if (MaxID < e.ID) MaxID = e.ID;
                if (e.Label > 0)
                    CountBoundKnots++;
            }
            BedMesh.BoundKnots = new int[CountBoundKnots];
            BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            // шаги хеширования
            double dx = (MaxX - MinX) / (CountKnots - 1);
            double dy = (MaxY - MinY) / (CountKnots - 1);
            // хеширование узлов
            foreach (var e in Vertices)
            {
                ix = (int)((e.X - MinX) / dx);
                iy = (int)((e.Y - MinY) / dy);
                XMap[ix, iy].Add(e.ID);
            }
            // Новые нумера узлов
            int[] NewNumber = new int[MaxID + 1];
            for (uint i = 0; i < CountKnots; i++)
                NewNumber[i] = -1;
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toLeft:
                    {
                        // Получение новый номеров узлов
                        int iix;
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            iix = CountKnots - ix - 1;
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[iix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toDown:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toUp:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            jy = CountKnots - iy - 1;
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, jy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
            }
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[Vertices.Count];
            BedMesh.CoordsY = new double[Vertices.Count];
            
            int idx = 0;
            foreach (var e in Vertices)
            {
                int OldKnot = e.ID;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                BedMesh.CoordsX[NewKnot] = e.X;
                BedMesh.CoordsY[NewKnot] = e.Y;
                // граничные узлы
                if (e.Label > 0)
                {
                    BedMesh.BoundKnots[idx] = NewKnot;
                    BedMesh.BoundKnotsMark[idx] = e.Label;
                    idx++;
                }
                // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new TriElement[Triangles.Count];
            //for (int i = 0; i < BedMesh.AreaElems.Length; i++)
            //    BedMesh.AreaElems[i] = new int[3];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int OldKnot = e.GetVertexID(0);
                int NewKnot = NewNumber[OldKnot];
                BedMesh.AreaElems[fe].Vertex1 = (uint)NewKnot;

                OldKnot = e.GetVertexID(1);
                NewKnot = NewNumber[OldKnot];
                BedMesh.AreaElems[fe].Vertex2 = (uint)NewKnot;

                OldKnot = e.GetVertexID(2);
                NewKnot = NewNumber[OldKnot];
                BedMesh.AreaElems[fe].Vertex3 = (uint)NewKnot;
                //for (int j = 0; j < 3; j++)
                //{
                //    int OldKnot = e.GetVertexID(j);
                //    int NewKnot = NewNumber[OldKnot];
                //    BedMesh.AreaElems[fe][j] = NewKnot;
                //}
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            BedMesh.BoundElems = new TwoElement[Edges.Count()];
            BedMesh.BoundElementsMark = new int[Edges.Count()];
            //for (int i = 0; i < BedMesh.BoundElems.Length; i++)
            //    BedMesh.BoundElems[i] = new int[2];
            var _Edges = Edges.ToArray();
            int be = 0;
            foreach (var e in Edges)
            {
                int OldKnot = e.P0;
                int NewKnot0 = NewNumber[OldKnot];
                OldKnot = e.P1;
                int NewKnot1 = NewNumber[OldKnot];
                BedMesh.BoundElems[be].Vertex1 = (uint)NewKnot0;
                BedMesh.BoundElems[be].Vertex2 = (uint)NewKnot1;
                BedMesh.BoundElementsMark[be] = e.Label;
                be++;
            }

            // + 20 03 2025
            {
                int[] marks = null;
                MEM.Alloc(BedMesh.CountKnots, ref marks);
                TwoElement[] belems = BedMesh.GetBoundElems();
                int[] bEMark = BedMesh.GetBElementsBCMark();
                int countBE = 0;
                for (be = 0; be < BedMesh.CountBoundElements; be++)
                {
                    if (bEMark[be] > 0)
                    {
                        marks[belems[be].Vertex1] = bEMark[be];
                        marks[belems[be].Vertex2] = bEMark[be];
                        countBE++;
                    }
                }
                for (uint bk = 0; bk < BedMesh.CountBoundKnots; bk++)
                    BedMesh.BoundKnotsMark[bk] = marks[BedMesh.BoundKnots[bk]]-1;
                TwoElement[] newbelems = null;
                int[] newbEMark = null;
                uint nbe = 0;
                MEM.Alloc(countBE, ref newbelems);
                MEM.Alloc(countBE, ref newbEMark);
                for (be = 0; be < BedMesh.CountBoundElements; be++)
                {
                    if (bEMark[be] > 0)
                    {
                        newbEMark[nbe] = bEMark[be] - 1;
                        newbelems[nbe++] = belems[be];
                    }
                }
                BedMesh.BoundElems = newbelems;
                BedMesh.BoundElementsMark = newbEMark;
            }

         //   CorrectorTriMesh(ref BedMesh);
            return BedMesh;
        }

        /// <summary>
        /// Преобразователь типа треугольной сетки 
        /// от типа TriangleNet.MeshNet mesh к типу CommonLib.IMesh
        /// с фронтальной перенумерацией сетки 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static void ConvertMeshNetToTriMesh(ref IMesh bmesh, MeshNet mesh, Direction direction = Direction.toRight)
        {
            TriMesh BedMesh = new TriMesh();
            int ix, iy, jy;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;

            int CountKnots = (int)(Math.Sqrt(Vertices.Count)) + 2;
            // хеш таблица
            List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
            for (int i = 0; i < CountKnots; i++)
                for (int j = 0; j < CountKnots; j++)
                    XMap[i, j] = new List<int>();

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            int MaxID = 0;
            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                if (e.X > MaxX) MaxX = e.X;
                if (e.X < MinX) MinX = e.X;
                if (e.Y > MaxY) MaxY = e.Y;
                if (e.Y < MinY) MinY = e.Y;
                if (MaxID < e.ID) MaxID = e.ID;
                if (e.Label > 0)
                    CountBoundKnots++;
            }
            BedMesh.BoundKnots = new int[CountBoundKnots];
            BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            // шаги хеширования
            double dx = (MaxX - MinX) / (CountKnots - 1);
            double dy = (MaxY - MinY) / (CountKnots - 1);
            // хеширование узлов
            foreach (var e in Vertices)
            {
                ix = (int)((e.X - MinX) / dx);
                iy = (int)((e.Y - MinY) / dy);
                XMap[ix, iy].Add(e.ID);
            }
            // Новые нумера узлов
            int[] NewNumber = new int[MaxID + 1];
            for (uint i = 0; i < CountKnots; i++)
                NewNumber[i] = -1;
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toLeft:
                    {
                        // Получение новый номеров узлов
                        int iix;
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            iix = CountKnots - ix - 1;
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[iix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toDown:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toUp:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            jy = CountKnots - iy - 1;
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, jy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
            }
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[Vertices.Count];
            BedMesh.CoordsY = new double[Vertices.Count];
            int idx = 0;
            foreach (var e in Vertices)
            {
                int OldKnot = e.ID;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                BedMesh.CoordsX[NewKnot] = e.X;
                BedMesh.CoordsY[NewKnot] = e.Y;
                // граничные узлы
                if (e.Label > 0)
                {
                    BedMesh.BoundKnots[idx] = NewKnot;
                    BedMesh.BoundKnotsMark[idx] = e.Label;
                    idx++;
                }
                // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new TriElement[Triangles.Count];
            //for (int i = 0; i < BedMesh.AreaElems.Length; i++)
            //    BedMesh.AreaElems[i] = new int[3];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int OldKnot = e.GetVertexID(0);
                int NewKnot = NewNumber[OldKnot];
                BedMesh.AreaElems[fe].Vertex1 = (uint)NewKnot;

                OldKnot = e.GetVertexID(1);
                NewKnot = NewNumber[OldKnot];
                BedMesh.AreaElems[fe].Vertex2 = (uint)NewKnot;

                OldKnot = e.GetVertexID(2);
                NewKnot = NewNumber[OldKnot];
                BedMesh.AreaElems[fe].Vertex3 = (uint)NewKnot;
                //for (int j = 0; j < 3; j++)
                //{
                //    int OldKnot = e.GetVertexID(j);
                //    int NewKnot = NewNumber[OldKnot];
                //    BedMesh.AreaElems[fe][j] = NewKnot;
                //}
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            List<TwoElement> boundElems = new List<TwoElement>();
            List<int> BoundElementsMark = new List<int>();
            List<TypeBoundCond> BCType = new List<TypeBoundCond>();
            foreach (var e in Edges)
            {
                if (e.Label > 0)
                {
                    int OldKnot = e.P0;
                    int NewKnot0 = NewNumber[OldKnot];
                    OldKnot = e.P1;
                    int NewKnot1 = NewNumber[OldKnot];
                    TwoElement elem = new TwoElement();
                    elem.Vertex1 = (uint)NewKnot0;
                    elem.Vertex2 = (uint)NewKnot1;
                    boundElems.Add(elem);
                    BoundElementsMark.Add(e.Label);
                    BCType.Add(TypeBoundCond.Dirichlet);
                }
            }
            BedMesh.BoundElems = boundElems.ToArray();
            BedMesh.BoundElementsMark = BoundElementsMark.ToArray();
            bmesh = BedMesh;
        }


        /// <summary>
        /// Преобразователь типа треугольной сетки 
        /// от типа TriangleNet.MeshNet mesh к типу CommonLib.IMesh
        /// с фронтальной перенумерацией сетки 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static void ConvertFrontRenumberation(ref IFEMesh bmesh, MeshNet mesh, Direction direction = Direction.toRight, int FigID=0)
        {
            IFEMesh BedMesh = new FEMesh();
            int ix, iy, jy;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;

            int CountKnots = (int)(Math.Sqrt(Vertices.Count)) + 2;
            // хеш таблица
            List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
            for (int i = 0; i < CountKnots; i++)
                for (int j = 0; j < CountKnots; j++)
                    XMap[i, j] = new List<int>();

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            int MaxID = 0;
            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                if (e.X > MaxX) MaxX = e.X;
                if (e.X < MinX) MinX = e.X;
                if (e.Y > MaxY) MaxY = e.Y;
                if (e.Y < MinY) MinY = e.Y;
                if (MaxID < e.ID) MaxID = e.ID;
                if (e.Label > 0)
                    CountBoundKnots++;
            }
            
            // шаги хеширования
            double dx = (MaxX - MinX) / (CountKnots - 1);
            double dy = (MaxY - MinY) / (CountKnots - 1);
            // хеширование узлов
            foreach (var e in Vertices)
            {
                ix = (int)((e.X - MinX) / dx);
                iy = (int)((e.Y - MinY) / dy);
                XMap[ix, iy].Add(e.ID);
            }
            // Новые нумера узлов
            int[] NewNumber = new int[MaxID + 1];
            for (uint i = 0; i < CountKnots; i++)
                NewNumber[i] = -1;
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toLeft:
                    {
                        // Получение новый номеров узлов
                        int iix;
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            iix = CountKnots - ix - 1;
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[iix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toDown:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toUp:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            jy = CountKnots - iy - 1;
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, jy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
            }
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[Vertices.Count];
            BedMesh.CoordsY = new double[Vertices.Count];
            BedMesh.BNods = new IFENods[CountBoundKnots];
            //BoundKnots = new int[CountBoundKnots];
            //BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            int idx = 0;
            foreach (var e in Vertices)
            {
                int OldKnot = e.ID;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                BedMesh.CoordsX[NewKnot] = e.X;
                BedMesh.CoordsY[NewKnot] = e.Y;
                // граничные узлы
                if (e.Label > 0)
                {
                    BedMesh.BNods[idx] = new FENods(NewKnot, e.Label);
                    //BedMesh.BoundKnots[idx] = NewKnot;
                    //BedMesh.BoundKnotsMark[idx] = e.Label;
                    idx++;
                }
                // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new  IFElement[Triangles.Count];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int Vertex1 = NewNumber[e.GetVertexID(0)];
                int Vertex2 = NewNumber[e.GetVertexID(1)];
                int Vertex3 = NewNumber[e.GetVertexID(2)];
                BedMesh.AreaElems[fe] = new FElement(Vertex1, Vertex2, Vertex3, fe, FigID);
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            //BedMesh.BoundElems = new IFElement[Triangles.Count];
            fe = 0;
            List<IFElement> boundElems = new List<IFElement>();
            //List<int> BoundElementsMark = new List<int>();
            foreach (var e in Edges)
            {
                if (e.Label > 0)
                {
                    int OldKnot = e.P0;
                    int Vertex1 = NewNumber[OldKnot];
                        OldKnot = e.P1;
                    int Vertex2 = NewNumber[OldKnot];
                    boundElems.Add(new FElement(Vertex1, Vertex2, fe, e.Label));
                    fe++;
                }
            }
            BedMesh.BoundElems = boundElems.ToArray();
            bmesh = BedMesh;
        }

        /// <summary>
        /// Преобразователь типа треугольной сетки 
        /// от типа TriangleNet.MeshNet mesh к типу CommonLib.IMesh
        /// с фронтальной перенумерацией сетки 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static void ConvertFrontRenumberation(ref IFEMesh bmesh, ref double[][] values, MeshNet mesh, Direction direction = Direction.toRight)
        {
            int FigID = 0;
            IFEMesh BedMesh = new FEMesh();
            int ix, iy, jy;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;

            int CountKnots = (int)(Math.Sqrt(Vertices.Count)) + 2;
            // хеш таблица
            List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
            for (int i = 0; i < CountKnots; i++)
                for (int j = 0; j < CountKnots; j++)
                    XMap[i, j] = new List<int>();

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            int MaxID = 0;


            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                if (e.X > MaxX) MaxX = e.X;
                if (e.X < MinX) MinX = e.X;
                if (e.Y > MaxY) MaxY = e.Y;
                if (e.Y < MinY) MinY = e.Y;
                if (MaxID < e.ID) MaxID = e.ID;
                if (e.Label > 0)
                    CountBoundKnots++;
            }

            int ArtCount = -1;
            foreach (var e in Vertices)
            {
                if (e.Attributes != null)
                {
                    ArtCount = e.Attributes.Length;
                    MEM.Alloc2D(ArtCount, Vertices.Count, ref values);
                }
                break;
            }

            // шаги хеширования
            double dx = (MaxX - MinX) / (CountKnots - 1);
            double dy = (MaxY - MinY) / (CountKnots - 1);
            // хеширование узлов
            foreach (var e in Vertices)
            {
                ix = (int)((e.X - MinX) / dx);
                iy = (int)((e.Y - MinY) / dy);
                XMap[ix, iy].Add(e.ID);
            }
            // Новые нумера узлов
            int[] NewNumber = new int[MaxID + 1];
            for (uint i = 0; i < CountKnots; i++)
                NewNumber[i] = -1;
            int NewIndex = 0;
            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toLeft:
                    {
                        // Получение новый номеров узлов
                        int iix;
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            iix = CountKnots - ix - 1;
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[iix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toDown:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toUp:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            jy = CountKnots - iy - 1;
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, jy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
            }
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[Vertices.Count];
            BedMesh.CoordsY = new double[Vertices.Count];
            BedMesh.BNods = new IFENods[CountBoundKnots];
            //BoundKnots = new int[CountBoundKnots];
            //BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            int idx = 0;
            foreach (var e in Vertices)
            {
                int OldKnot = e.ID;
                int NewKnot = NewNumber[OldKnot];
                // координаты
                BedMesh.CoordsX[NewKnot] = e.X;
                BedMesh.CoordsY[NewKnot] = e.Y;
                // граничные узлы
                if (e.Label > 0)
                {
                    BedMesh.BNods[idx] = new FENods(NewKnot, e.Label);
                    //BedMesh.BoundKnots[idx] = NewKnot;
                    //BedMesh.BoundKnotsMark[idx] = e.Label;
                    idx++;
                }
                if(e.Attributes != null)
                {
                    for(int ai=0; ai< ArtCount; ai++)
                    {
                        values[ai][NewKnot] = e.Attributes[ai];
                    }
                }
                // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new IFElement[Triangles.Count];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int Vertex1 = NewNumber[e.GetVertexID(0)];
                int Vertex2 = NewNumber[e.GetVertexID(1)];
                int Vertex3 = NewNumber[e.GetVertexID(2)];
                BedMesh.AreaElems[fe] = new FElement(Vertex1, Vertex2, Vertex3, fe, FigID);
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            //BedMesh.BoundElems = new IFElement[Triangles.Count];
            fe = 0;
            List<IFElement> boundElems = new List<IFElement>();
            //List<int> BoundElementsMark = new List<int>();
            foreach (var e in Edges)
            {
                if (e.Label > 0)
                {
                    int OldKnot = e.P0;
                    int Vertex1 = NewNumber[OldKnot];
                    OldKnot = e.P1;
                    int Vertex2 = NewNumber[OldKnot];
                    boundElems.Add(new FElement(Vertex1, Vertex2, fe, e.Label));
                    fe++;
                }
            }
            BedMesh.BoundElems = boundElems.ToArray();
            bmesh = BedMesh;
        }


        /// <summary>
        /// Преобразователь типа треугольной сетки с отсечением узлов
        /// </summary>
        /// <param name="bmesh">Результат</param>
        /// <param name="values">Атрибуты узлов</param>
        /// <param name="mesh">Источник</param>
        /// <param name="direction">Фронт перенумерации</param>
        public static void ConvertFrontRenumberationAndCutting(ref IFEMesh bmesh, ref double[][] values, MeshNet mesh, Direction direction = Direction.toRight)
        {
            int FigID = 0;
            IFEMesh BedMesh = new FEMesh();
            int ix, iy, jy;
            if (mesh == null) return;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;
            int MaxID = 0;
            foreach (var e in Vertices)
               if (MaxID < e.ID) MaxID = e.ID;
            // активные узлы сетки
            int[] meshKnots = null;
            MEM.Alloc(MaxID + 1, ref meshKnots, 0);
            foreach (var e in Triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    int knot = e.GetVertexID(i);
                    meshKnots[knot] = 1;
                }
            }
 

            // количество активных узлов сетки
            int NewCountKnots = meshKnots.Sum();
            int CountKnots = (int)(Math.Sqrt(Vertices.Count)) + 2;
            // хеш таблица
            List<int>[,] XMap = new List<int>[CountKnots, CountKnots];
            for (int i = 0; i < CountKnots; i++)
                for (int j = 0; j < CountKnots; j++)
                    XMap[i, j] = new List<int>();

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;


            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                int knot = meshKnots[e.ID];
                if (knot > 0)
                {
                    if (e.X > MaxX) MaxX = e.X;
                    if (e.X < MinX) MinX = e.X;
                    if (e.Y > MaxY) MaxY = e.Y;
                    if (e.Y < MinY) MinY = e.Y;
                    if (MaxID < e.ID) MaxID = e.ID;
                    if (e.Label > 0)
                        CountBoundKnots++;
                }
            }

            int ArtCount = -1;
            foreach (var e in Vertices)
            {
                if (e.Attributes != null)
                {
                    ArtCount = e.Attributes.Length;
                    MEM.Alloc2D(ArtCount, NewCountKnots, ref values);
                }
                break;
            }

            // шаги хеширования
            double dx = (MaxX - MinX) / (CountKnots - 1);
            double dy = (MaxY - MinY) / (CountKnots - 1);
            // хеширование узлов
            int CountTestNew = 0;
            foreach (var e in Vertices)
            {
                int knot = meshKnots[e.ID];
                if (knot > 0)
                {
                    ix = (int)((e.X - MinX) / dx);
                    iy = (int)((e.Y - MinY) / dy);
                    XMap[ix, iy].Add(e.ID);
                    CountTestNew++;
                }
            }
            // Новые нумера узлов
            int[] NewNumber = null;
            MEM.Alloc(MaxID+1, ref NewNumber, -1);
            int NewIndex = 0;

            switch (direction)
            {
                case Direction.toRight:
                    {
                        // Получение новый номеров узлов
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toLeft:
                    {
                        // Получение новый номеров узлов
                        int iix;
                        for (ix = 0; ix < CountKnots; ix++) // по Х
                        {
                            iix = CountKnots - ix - 1;
                            for (iy = 0; iy < CountKnots; iy++) // по Y
                            {
                                int CountX = XMap[iix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по узлам в хеше
                                {
                                    int Old = XMap[iix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toDown:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, iy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, iy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
                case Direction.toUp:
                    {
                        for (iy = 0; iy < CountKnots; iy++) // по Y
                        {
                            jy = CountKnots - iy - 1;
                            for (ix = 0; ix < CountKnots; ix++) // по Х
                            {
                                int CountX = XMap[ix, jy].Count();
                                for (int i = 0; i < CountX; i++) // по Y
                                {
                                    int Old = XMap[ix, jy][i];
                                    NewNumber[Old] = NewIndex;
                                    NewIndex++;
                                }
                            }
                        }
                        break;
                    }
            }
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[NewCountKnots];
            BedMesh.CoordsY = new double[NewCountKnots];
            BedMesh.BNods = new IFENods[CountBoundKnots];
            //BoundKnots = new int[CountBoundKnots];
            //BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            int idx = 0;
            foreach (var e in Vertices)
            {
                int knot = meshKnots[e.ID];
                if (knot > 0)
                {
                    int OldKnot = e.ID;
                    int NewKnot = NewNumber[OldKnot];
                    // координаты
                    BedMesh.CoordsX[NewKnot] = e.X;
                    BedMesh.CoordsY[NewKnot] = e.Y;
                    // граничные узлы
                    if (e.Label > 0)
                    {
                        BedMesh.BNods[idx] = new FENods(NewKnot, e.Label);
                        //BedMesh.BoundKnots[idx] = NewKnot;
                        //BedMesh.BoundKnotsMark[idx] = e.Label;
                        idx++;
                    }
                    if (e.Attributes != null)
                    {
                        for (int ai = 0; ai < ArtCount; ai++)
                        {
                            values[ai][NewKnot] = e.Attributes[ai];
                        }
                      //  Console.WriteLine("ID {0} Vx {1} Vy {2}", e.ID, e.attributes[3], e.attributes[4]);
                    }
                    // Console.WriteLine(" id {0} x {1} y {2} label {3}", e.ID, e.X, e.Y, e.Label);
                }
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new IFElement[Triangles.Count];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int Vertex1 = NewNumber[e.GetVertexID(0)];
                int Vertex2 = NewNumber[e.GetVertexID(1)];
                int Vertex3 = NewNumber[e.GetVertexID(2)];
                BedMesh.AreaElems[fe] = new FElement(Vertex1, Vertex2, Vertex3, fe, FigID);
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            //BedMesh.BoundElems = new IFElement[Triangles.Count];
            fe = 0;
            List<IFElement> boundElems = new List<IFElement>();
            //List<int> BoundElementsMark = new List<int>();
            foreach (var e in Edges)
            {
                if (e.Label > 0)
                {
                    int OldKnot = e.P0;
                    int Vertex1 = NewNumber[OldKnot];
                    OldKnot = e.P1;
                    int Vertex2 = NewNumber[OldKnot];
                    boundElems.Add(new FElement(Vertex1, Vertex2, fe, e.Label));
                    fe++;
                }
            }
            BedMesh.BoundElems = boundElems.ToArray();
            bmesh = BedMesh;
        }

        /// <summary>
        /// Адаптация MeshNet к TriMesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static void Adapter(ref CommonLib.IMesh bmesh, MeshNet mesh)
        {
            TriMesh BedMesh = new TriMesh();
            //int ix, iy, jy;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            int MaxID = 0;
            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                if (e.X > MaxX) MaxX = e.X;
                if (e.X < MinX) MinX = e.X;
                if (e.Y > MaxY) MaxY = e.Y;
                if (e.Y < MinY) MinY = e.Y;
                if (MaxID < e.ID) MaxID = e.ID;
                if (e.Label > 0)
                    CountBoundKnots++;
            }
            BedMesh.BoundKnots = new int[CountBoundKnots];
            BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[Vertices.Count];
            BedMesh.CoordsY = new double[Vertices.Count];
            int idx = 0;
            int idxX = 0;
            foreach (var e in Vertices)
            {
                // координаты
                BedMesh.CoordsX[idxX] = e.X;
                BedMesh.CoordsY[idxX] = e.Y;
                idxX++;
                // граничные узлы
                if (e.Label > 0)
                {
                    BedMesh.BoundKnots[idx] = idxX;
                    BedMesh.BoundKnotsMark[idx] = e.Label;
                    idx++;
                }
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new TriElement[Triangles.Count];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int OldKnot = e.GetVertexID(0);
                BedMesh.AreaElems[fe].Vertex1 = (uint)OldKnot;
                OldKnot = e.GetVertexID(1);
                BedMesh.AreaElems[fe].Vertex2 = (uint)OldKnot;
                OldKnot = e.GetVertexID(2);
                BedMesh.AreaElems[fe].Vertex3 = (uint)OldKnot;
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            BedMesh.BoundElems = new TwoElement[Edges.Count()];
            BedMesh.BoundElementsMark = new int[Edges.Count()];
            //for (int i = 0; i < BedMesh.BoundElems.Length; i++)
            //    BedMesh.BoundElems[i] = new int[2];
            int be = 0;
            foreach (var e in Edges)
            {
                BedMesh.BoundElems[be].Vertex1 = (uint)e.P0;
                BedMesh.BoundElems[be].Vertex2 = (uint)e.P1;
                BedMesh.BoundElementsMark[be] = e.Label;
                be++;
            }
            bmesh = BedMesh;
        }


        /// <summary>
        /// Адаптация MeshNet к TriMesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static TriMesh Adapter(MeshNet mesh)
        {
            TriMesh BedMesh = new TriMesh();
            //int ix, iy, jy;
            // узлы КЭ сетки
            ICollection<Vertex> Vertices = mesh.Vertices;
            // FE
            ICollection<Triangle> Triangles = mesh.Triangles;
            // BFE
            IEnumerable<Edge> Edges = mesh.Edges;

            double MaxX = double.MinValue;
            double MinX = double.MaxValue;
            double MaxY = double.MinValue;
            double MinY = double.MaxValue;
            int CountBoundKnots = 0;
            int MaxID = 0;
            // Подготовка контейнера
            foreach (var e in Vertices)
            {
                if (e.X > MaxX) MaxX = e.X;
                if (e.X < MinX) MinX = e.X;
                if (e.Y > MaxY) MaxY = e.Y;
                if (e.Y < MinY) MinY = e.Y;
                if (MaxID < e.ID) MaxID = e.ID;
                if (e.Label > 0)
                    CountBoundKnots++;
            }
            BedMesh.BoundKnots = new int[CountBoundKnots];
            BedMesh.BoundKnotsMark = new int[CountBoundKnots];
            // **************** Создание нового массива координат ******************
            BedMesh.CoordsX = new double[Vertices.Count];
            BedMesh.CoordsY = new double[Vertices.Count];
            int idx = 0;
            int idxX = 0;
            foreach (var e in Vertices)
            {
                // координаты
                BedMesh.CoordsX[idxX] = e.X;
                BedMesh.CoordsY[idxX] = e.Y;
                idxX++;
                // граничные узлы
                if (e.Label > 0)
                {
                    BedMesh.BoundKnots[idx] = idxX;
                    BedMesh.BoundKnotsMark[idx] = e.Label;
                    idx++;
                }
            }
            // **************** Создание нового массива обхода ******************
            BedMesh.AreaElems = new TriElement[Triangles.Count];
            // перезапись треугольников
            int fe = 0;
            foreach (var e in Triangles)
            {
                int OldKnot = e.GetVertexID(0);
                BedMesh.AreaElems[fe].Vertex1 = (uint)OldKnot;
                OldKnot = e.GetVertexID(1);
                BedMesh.AreaElems[fe].Vertex2 = (uint)OldKnot;
                OldKnot = e.GetVertexID(2);
                BedMesh.AreaElems[fe].Vertex3 = (uint)OldKnot;
                fe++;
            }
            // **************** Создание нового массива граничных элементов ******************
            BedMesh.BoundElems = new TwoElement[Edges.Count()];
            BedMesh.BoundElementsMark = new int[Edges.Count()];
            //for (int i = 0; i < BedMesh.BoundElems.Length; i++)
            //    BedMesh.BoundElems[i] = new int[2];
            int be = 0;
            foreach (var e in Edges)
            {
                BedMesh.BoundElems[be].Vertex1 = (uint)e.P0;
                BedMesh.BoundElems[be].Vertex2 = (uint)e.P1;
                BedMesh.BoundElementsMark[be] = e.Label;
                be++;
            }
            return BedMesh;
        }

        /// <summary>
        /// Создание сетки в задачах граничных элементов для вывода полей тока....
        /// </summary>
        /// <param name="xb">координаты дна</param>
        /// <param name="yb">координаты дна</param>
        /// <param name="xc">координаты цилиндра (дырка в сетке)</param>
        /// <param name="yc">координаты цилиндра (дырка в сетке)</param>
        /// <param name="H">высота области</param>
        /// <param name="L">длина области</param>
        /// <returns></returns>
        public static IFEMesh CreateFEMesh(double[] xb, double[] yb, double[] xc, double[] yc, double H, double L)
        {
            IFEMesh femesh = null;
            /// <summary>
            /// Опции герерации сетки в контуре
            /// </summary>
            IPolygon polygon = new Polygon();
            int label = 1;
            double X0 = xb[0];
            double XL = xb[xb.Length - 1];
            double XC = 0.5 * (X0 + XL);
            double XC0 = XC - L / 2;
            double XCL = XC + L / 2;
            int i0 = 0;
            int iL = 0;
            for (int i = 0; i < xb.Length; i++)
                if(xb[i]>XC0)
                {
                    i0 = i; break;
                }
            for (int i = i0+1; i < xb.Length; i++)
                if (xb[i] > XCL)
                {
                    iL = i; break;
                }
            // область
            List<Vertex> vertexs = new List<Vertex>();
            for (int i = i0; i < iL; i++)
            {
                vertexs.Add(new Vertex(xb[i], yb[i], 0));
            }
            vertexs.Add(new Vertex(xb[iL-1], H, 0));
            vertexs.Add(new Vertex(xb[i0], H, 0));
            //for (int i = 0; i < xb.Length; i++)
            //    vertexs.Add(new Vertex(xb[xb.Length - i - 1], H, 0));
            Contour contour = new Contour(vertexs, label);
            polygon.Add(contour);

            label = 2;
            //дырка - цилиндр
            List<Vertex> vertexsHole = new List<Vertex>();
            for (int i = 0; i < xc.Length; i++)
                vertexsHole.Add(new Vertex(xc[i], yc[i], label));
            double x0 = xc.Sum() / xc.Length;
            double y0 = yc.Sum() / yc.Length;
            Point center = new Point(x0, y0);
            Contour contour1 = new Contour(vertexsHole, label);
            polygon.Add(contour1, center);


            ConstraintOptions options = new ConstraintOptions();
            // создать контур
            options.Convex = false;
            // создать трианг. Делоне
            options.ConformingDelaunay = true;
            // создавать узлы на границах ?
            options.SegmentSplitting = 0;

            QualityOptions quality = new QualityOptions();
            quality.MinimumAngle = 25;
            quality.MaximumAngle = 120;
            double dx = (XCL-XC0) / 100;
            double Area = 0.3 * dx * dx;
            quality.MaximumArea = Area;
            int CountSmooth = 10;
            
            
            // Triangulate the polygon
            // генерация сетки контуру
            MeshNet meshDel1 = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);

            FileProcessor.Write(meshDel1, "booble.node");
            for (int i = 0; i < CountSmooth; i++)
            {
                var smoother = new SimpleSmoother();
                smoother.Smooth(meshDel1);
            }
            CommonLib.IMesh tmesh = null;
            //MeshAdapter.Adapter(ref tmesh, meshDel1);
            MeshAdapter.ConvertMeshNetToTriMesh(ref tmesh, meshDel1, Direction.toRight);

            femesh = new FEMesh(tmesh);
            return femesh;
        }
    }
}
