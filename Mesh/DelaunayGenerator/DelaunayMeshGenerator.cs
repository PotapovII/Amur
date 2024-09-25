//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 19.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace DelaunayGenerator
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using CommonLib;
    using CommonLib.Geometry;
    using GeometryLib.Geometry;

    using MeshLib;
    using MemLogLib;
    /// <summary>
    /// ОО: Делоне генератор выпуклой триангуляции
    /// </summary>
    public class DelaunayMeshGenerator
    {
        private readonly double EPSILON = Math.Pow(2, -52);
        private readonly int[] EDGE_STACK = new int[1024];
        /// <summary>
        ///  Массив индексов вершин треугольника (каждая группа 
        ///  из трех чисел образует треугольник). 
        ///  Обход вершин всех треугольников направлен против часовой стрелки.        
        /// </summary>
        public int[] Triangles;
        /// <summary>
        /// Массив индексов половин ребер треугольника, который позволяет вам 
        /// выполнять триангуляцию. i-я половина ребра в массиве соответствует вершине, 
        /// из triangles[i] которой исходит половина ребра. 
        /// halfedges[i] это индекс двойной полуреберья в соседнем 
        /// треугольнике (или -1 для внешних полуребер на выпуклой оболочке).
        /// </summary>
        public int[] HalfEdges;
        /// <summary>
        /// Массив координат входных точек 
        /// </summary>
        public IHPoint[] Points;
        /// <summary>
        /// Размерность оболочки
        /// </summary>
        private int hashSize;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных против часовой стрелки
        /// </summary>
        public int[] Hull;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных против часовой стрелки
        /// </summary>
        private int[] hullPrev;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных против часовой стрелки
        /// </summary>
        private int[] hullNext;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных против часовой стрелки
        /// </summary>
        private int[] hullTri;
        /// <summary>
        /// хэш-таблица для ребер
        /// </summary>
        private int[] hullHash;
        /// <summary>
        /// индекс точки отсортьированной по растоянию от центра
        /// </summary>
        private int[] ids;
        /// <summary>
        /// Координаты центра окружности 
        /// </summary>
        private double cx;
        private double cy;
        /// <summary>
        /// счетчик треугольников
        /// </summary>
        private int trianglesLen;
        /// <summary>
        /// Массив входных координат в форме [x0, y0, x1, y1, ....] типа, 
        /// указанного в конструкторе
        /// </summary>
        //private double[] coords;
        private double[] coordsX;
        private double[] coordsY;
        private int hullStart;
        private int hullSize;
        /// <summary>
        /// ОО: Делоне генератор выпуклой триангуляции
        /// </summary>
        public DelaunayMeshGenerator(){}
        /// <summary>
        /// Создание выпуклой Сетки
        /// </summary>
        /// <param name="DEGUG"></param>
        /// <returns></returns>
        public IMesh CreateMesh(bool DEGUG = false) 
        {
            TriMesh mesh = new TriMesh();
            int CountElems = Triangles.Length / 3;
            mesh.AreaElems = new TriElement[CountElems];
            for (int i = 0; i < CountElems; i++)
            {
                mesh.AreaElems[i].Vertex1 = (uint)(Triangles[3 * i]);
                mesh.AreaElems[i].Vertex2 = (uint)(Triangles[3 * i + 1]);
                mesh.AreaElems[i].Vertex3 = (uint)(Triangles[3 * i + 2]);
            }
            mesh.CoordsX = new double[Points.Length];
            mesh.CoordsY = new double[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                mesh.CoordsX[i] = Points[i].X;
                mesh.CoordsY[i] = Points[i].Y;
            }

            int CountBE = Hull.Length;
            mesh.BoundElems = new TwoElement[CountBE];
            mesh.BoundElementsMark = new int[CountBE];
            for (int i = 0; i < CountBE; i++)
            {
                mesh.BoundElems[i].Vertex1 = (uint)Hull[i];
                mesh.BoundElems[i].Vertex2 = (uint)Hull[(i+1)% CountBE];
                mesh.BoundElementsMark[i] = 0;
            }
            int CountBK = Hull.Length;
            mesh.BoundKnots = new int[CountBK];
            mesh.BoundKnotsMark = new int[CountBK];

            for (int i = 0; i < CountBK; i++)
            {
                mesh.BoundKnots[i] = Hull[i];
                mesh.BoundKnotsMark[i] = 0;
            }
            if(DEGUG == true)
                mesh.Print();
            return mesh; 
        }
        /// <summary>
        /// Генерация
        /// </summary>
        /// <param name="points"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public void Generator(IHPoint[] points)
        {
            if (points.Length < 3)
                throw new ArgumentOutOfRangeException("Нужно как минимум 3 вершины");
            Points = points;
            //var n = points.Length;
            hashSize = (int)Math.Ceiling(Math.Sqrt(points.Length));
            var maxTriangles = 2 * points.Length - 5;
            //MEM.Alloc(Points.Length * 2, ref coords);
            MEM.Alloc(Points.Length, ref coordsX);
            MEM.Alloc(Points.Length, ref coordsY);
            MEM.Alloc(maxTriangles * 3, ref Triangles);
            MEM.Alloc(maxTriangles * 3, ref HalfEdges);
            MEM.Alloc(points.Length, ref hullPrev);
            MEM.Alloc(points.Length, ref hullNext);
            MEM.Alloc(points.Length, ref hullTri);
            MEM.Alloc(points.Length, ref ids);
            MEM.Alloc(hashSize, ref hullHash);

            for (var i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                coordsX[i] = p.X;
                coordsY[i] = p.Y;
            }

            #region поиска границ рамки с точками
            var minX = coordsX[0];
            var minY = coordsY[0];
            var maxX = coordsX[0];
            var maxY = coordsY[0];
            ids[0] = 0;

            for (var i = 1; i < points.Length; i++)
            {
                var x = coordsX[i];
                var y = coordsY[i];

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
                ids[i] = i;
            }
            #endregion

            #region поиск начального треугольника
            // поиск центра рамки 
            var cx = (minX + maxX) / 2;
            var cy = (minY + maxY) / 2;

            var minDist = double.PositiveInfinity;
            var i0 = 0;
            var i1 = 0;
            var i2 = 0;

            // pick a seed point close to the center
            // выбираем начальную точку ближе к центру
            for (int i = 0; i < points.Length; i++)
            {
                var d = Dist(cx, cy, i);
                if (d < minDist)
                {
                    i0 = i;
                    minDist = d;
                }
            }
            var i0x = coordsX[i0];
            var i0y = coordsY[i0];

            minDist = double.PositiveInfinity;

            // find the point closest to the seed
            // найдите точку, ближайшую к наачльной
            for (int i = 0; i < points.Length; i++)
            {
                if (i == i0) continue;
                var d = Dist(i0, i);
                if (d < minDist && d > 0)
                {
                    i1 = i;
                    minDist = d;
                }
            }

            var i1x = coordsX[i1];
            var i1y = coordsY[i1];
            var minRadius = double.PositiveInfinity;
            // найдите третью точку, которая образует наименьшую окружность с первыми двумя
            for (int i = 0; i < points.Length; i++)
            {
                if (i == i0 || i == i1) continue;
                var r = Circumradius(i0x, i0y, i1x, i1y, coordsX[i], coordsY[i]);
                if (r < minRadius)
                {
                    i2 = i;
                    minRadius = r;
                }
            }
            var i2x = coordsX[i2];
            var i2y = coordsY[i2];

            if (minRadius == double.PositiveInfinity)
            {
                // Если три точки не найдены! То...
                throw new Exception("Для этих входных данных не существует триангуляции Делоне!");
            }
            #endregion
            
            // Выберем оринтацию вершин начального треугольника
            if (Orient(i0, i1, i2) == true)
            {
                var i = i1;
                var x = i1x;
                var y = i1y;
                i1 = i2;
                i1x = i2x;
                i1y = i2y;
                i2 = i;
                i2x = x;
                i2y = y;
            }
            /// Центр окружности проведенной по трем вершинами с координатами ...
            var center = Circumcenter(i0x, i0y, i1x, i1y, i2x, i2y);
            this.cx = center.X;
            this.cy = center.Y;
            // Расчет растояний от центра окружности 1 треугольника до точек триангуляции
            var dists = new double[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                dists[i] = Dist(center.X, center.Y, i);
            }
            // быстрая сортировка точек по расстоянию от центра окружности исходного треугольника
            Quicksort(ids, dists, 0, points.Length - 1);

            
            // установите начальный треугольник в качестве начальной оболочки
            hullStart = i0;
            hullSize = 3;

            hullNext[i0] = hullPrev[i2] = i1;
            hullNext[i1] = hullPrev[i0] = i2;
            hullNext[i2] = hullPrev[i1] = i0;

            hullTri[i0] = 0;
            hullTri[i1] = 1;
            hullTri[i2] = 2;

            hullHash[HashKey(i0x, i0y)] = i0;
            hullHash[HashKey(i1x, i1y)] = i1;
            hullHash[HashKey(i2x, i2y)] = i2;
            // счетчик треугольников
            trianglesLen = 0;
            // Добавление 1 треугольника в список треугольников
            AddTriangle(i0, i1, i2, -1, -1, -1);

            #region Поиск выпуклой оболочки и триангуляция
            double xp = 0;
            double yp = 0;
            // Поиск выпуклой оболочки и триангуляция
            for (var k = 0; k < ids.Length; k++)
            {
                // обработка текущего k - го узла
                var i = ids[k];
                var x = coordsX[i];
                var y = coordsY[i];

                // игнорировать почти совпадающие точки
                if (k > 0 && Math.Abs(x - xp) <= EPSILON && Math.Abs(y - yp) <= EPSILON) continue;
                xp = x;
                yp = y;

                // игнорировать  начальные точки треугольника
                if (i == i0 || i == i1 || i == i2) 
                    continue;

                // поиск видимого края выпуклой оболочки, используя хэш ребра
                var start = 0;
                for (var j = 0; j < hashSize; j++)
                {
                    var key = HashKey(x, y);
                    start = hullHash[(key + j) % hashSize];
                    if (start != -1 && start != hullNext[start]) 
                        break;
                }

                start = hullPrev[start];
                var e = start;
                var q = hullNext[e];

                while (Orient(i, e, q) == false)
                {
                    e = q;
                    if (e == start)
                    {
                        e = int.MaxValue;
                        break;
                    }
                    q = hullNext[e];
                }
                // скорее всего, это почти повторяющаяся точка; пропустите ее
                if (e == int.MaxValue) 
                    continue;

                //  добавьте первый треугольник от точки
                var t = AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);

                // recursively flip triangles from the point until they satisfy the Delaunay condition
                // рекурсивно переворачивайте треугольники от точки к точке, пока они не удовлетворят условию Делоне
                hullTri[i] = Legalize(t + 2);
                // следите за граничными треугольниками на оболочке
                hullTri[e] = t; // keep track of boundary triangles on the hull
                hullSize++;

                // walk forward through the hull, adding more triangles and flipping recursively
                // пройдите вперед по оболочке, добавляя больше треугольников и переворачивая их рекурсивно
                var next = hullNext[e];
                q = hullNext[next];

                while (Orient(i, next, q) == true)
                {
                    t = AddTriangle(next, i, q, hullTri[i], -1, hullTri[next]);
                    hullTri[i] = Legalize(t + 2);
                    hullNext[next] = next; // mark as removed // пометить как удаленный
                    hullSize--;
                    next = q;

                    q = hullNext[next];
                }

                // walk backward from the other side, adding more triangles and flipping
                // пройдите назад с другой стороны, добавляя больше треугольников и переворачивая
                if (e == start)
                {
                    q = hullPrev[e];
                    while (Orient(i, q, e) == true)
                    {
                        t = AddTriangle(q, i, e, -1, hullTri[e], hullTri[q]);
                        Legalize(t + 2);
                        hullTri[q] = t;
                        hullNext[e] = e; // mark as removed
                        hullSize--;
                        e = q;
                        q = hullPrev[e];
                    }
                }

                
                // пометить как удаленный
                hullStart = hullPrev[i] = e;
                hullNext[e] = hullPrev[next] = i;
                hullNext[i] = next;
                                
                // сохраните два новых ребра в хэш-таблице
                hullHash[HashKey(x, y)] = i;
                hullHash[HashKey(coordsX[e], coordsY[e])] = e;
            }

            Hull = new int[hullSize];
            var s = hullStart;
            for (var i = 0; i < hullSize; i++)
            {
                Hull[i] = s;
                s = hullNext[s];
            }
            #endregion
            // get rid of temporary arrays
            // избавиться от временных массивов
            hullPrev = hullNext = hullTri = null; 
            // trim typed triangle mesh arrays
            // обрезка типизированных треугольных сетчатых массивов
            Triangles = Triangles.Take(trianglesLen).ToArray();
            HalfEdges = HalfEdges.Take(trianglesLen).ToArray();
        }

        #region CreationLogic
        /// <summary>
        /// знак верктоного произведения построенного на касательных к двум граням
        /// </summary>
        /// <param name="i"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        bool Orient(int i, int q, int r)
        {
            return (coordsY[q] - coordsY[i]) * (coordsX[r] - coordsX[q]) - (coordsX[q] - coordsX[i]) * (coordsY[r] - coordsY[q]) < 0;
        }

        private int Legalize(int a)
        {
            var i = 0;
            int ar;

            // recursion eliminated with a fixed-size stack
            // рекурсия устранена с помощью стека фиксированного размера
            while (true)
            {
                var b = HalfEdges[a];

                // если пара треугольников не удовлетворяет условию Делоне
                // (p1 находится внутри описанной окружности [p0, pl, pr]),
                // переверните их, затем выполните ту же проверку/переворот
                // рекурсивно для новой пары треугольников

                /* if the pair of triangles doesn't satisfy the Delaunay condition
                 * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
                 * then do the same check/flip recursively for the new pair of triangles
                 *
                 *           pl                    pl
                 *          /||\                  /  \
                 *       al/ || \bl            al/    \a
                 *        /  ||  \              /      \
                 *       /  a||b  \    flip    /___ar___\
                 *     p0\   ||   /p1   =>   p0\---bl---/p1
                 *        \  ||  /              \      /
                 *       ar\ || /br             b\    /br
                 *          \||/                  \  /
                 *           pr                    pr
                 */
                int a0 = a - a % 3;
                ar = a0 + (a + 2) % 3;

                if (b == -1)
                {
                    // выпуклый край корпуса
                    if (i == 0) 
                        break;
                    a = EDGE_STACK[--i];
                    continue;
                }

                var b0 = b - b % 3;
                var al = a0 + (a + 1) % 3;
                var bl = b0 + (b + 2) % 3;

                var p0 = Triangles[ar];
                var pr = Triangles[a];
                var pl = Triangles[al];
                var p1 = Triangles[bl];

                var illegal = InCircle(p0, pr, pl, p1);
                if (illegal)
                {
                    Triangles[a] = p1;
                    Triangles[b] = p0;

                    var hbl = HalfEdges[bl];

                    // edge swapped on the other side of the hull (rare); fix the halfedge reference
                    // ребро поменяно местами на другой стороне корпуса (редко);
                    // исправить ссылку на половину ребра
                    if (hbl == -1)
                    {
                        var e = hullStart;
                        do
                        {
                            if (hullTri[e] == bl)
                            {
                                hullTri[e] = a;
                                break;
                            }
                            e = hullPrev[e];
                        } while (e != hullStart);
                    }
                    Link(a, hbl);
                    Link(b, HalfEdges[ar]);
                    Link(ar, bl);

                    var br = b0 + (b + 1) % 3;

                    // don't worry about hitting the cap: it can only happen on extremely degenerate input
                    // не беспокойтесь о достижении предела: это может произойти только при крайне вырожденном вводе
                    if (i < EDGE_STACK.Length)
                    {
                        EDGE_STACK[i++] = br;
                    }
                }
                else
                {
                    if (i == 0) break;
                    a = EDGE_STACK[--i];
                }
            }
            return ar;
        }
        
        private bool InCircle(int i, int j, int k, int n)
        {
            var dx = coordsX[i] - coordsX[n];
            var dy = coordsY[i] - coordsY[n];
            var ex = coordsX[j] - coordsX[n];
            var ey = coordsY[j] - coordsY[n];
            var fx = coordsX[k] - coordsX[n];
            var fy = coordsY[k] - coordsY[n];

            var ap = dx * dx + dy * dy;
            var bp = ex * ex + ey * ey;
            var cp = fx * fx + fy * fy;

            return dx * (ey * cp - bp * fy) -
                   dy * (ex * cp - bp * fx) +
                   ap * (ex * fy - ey * fx) < 0;
        }
        /// <summary>
        /// Добавление треугольника в список треугольников
        /// </summary>
        /// <param name="i0">индекс вершины 0</param>
        /// <param name="i1">индекс вершины 1</param>
        /// <param name="i2">индекс вершины 2</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
        {
            var triangleID = trianglesLen;
            Triangles[triangleID] = i0;
            Triangles[triangleID + 1] = i1;
            Triangles[triangleID + 2] = i2;

            Link(triangleID, a);
            Link(triangleID + 1, b);
            Link(triangleID + 2, c);

            trianglesLen += 3;
            return triangleID;
        }
        private void Link(int triangleID, int b)
        {
            HalfEdges[triangleID] = b;
            if (b != -1) 
                HalfEdges[b] = triangleID;
        }
        private int HashKey(double x, double y) => (int)(Math.Floor(PseudoAngle(x - cx, y - cy) * hashSize) % hashSize);
        private static double PseudoAngle(double dx, double dy)
        {
            var p = dx / (Math.Abs(dx) + Math.Abs(dy));
            return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
        }
        /// <summary>
        /// быстрая сортировка точек по расстоянию от центра окружности исходного треугольника
        /// </summary>
        /// <param name="ids">индекс сортируемой точки</param>
        /// <param name="dists">дистанции от центра до сортируемой точки</param>
        /// <param name="left">начальный номер узла сортируемых массивов</param>
        /// <param name="right">конечный номер узла сортируемых массивов</param></param>
        private static void Quicksort(int[] ids, double[] dists, int left, int right)
        {
            if (right - left <= 20)
            {
                for (var i = left + 1; i <= right; i++)
                {
                    var temp = ids[i];
                    var tempDist = dists[temp];
                    var j = i - 1;
                    while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
                    ids[j + 1] = temp;
                }
            }
            else
            {
                var median = (left + right) >> 1;
                var i = left + 1;
                var j = right;
                Swap(ids, median, i);
                if (dists[ids[left]] > dists[ids[right]]) Swap(ids, left, right);
                if (dists[ids[i]] > dists[ids[right]]) Swap(ids, i, right);
                if (dists[ids[left]] > dists[ids[i]]) Swap(ids, left, i);

                var temp = ids[i];
                var tempDist = dists[temp];
                while (true)
                {
                    do i++; while (dists[ids[i]] < tempDist);
                    do j--; while (dists[ids[j]] > tempDist);
                    if (j < i) break;
                    Swap(ids, i, j);
                }
                ids[left + 1] = ids[j];
                ids[j] = temp;

                if (right - i + 1 >= j - left)
                {
                    Quicksort(ids, dists, i, right);
                    Quicksort(ids, dists, left, j - 1);
                }
                else
                {
                    Quicksort(ids, dists, left, j - 1);
                    Quicksort(ids, dists, i, right);
                }
            }
        }
        private static void Swap(int[] arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        private static double Circumradius(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var ex = cx - ax;
            var ey = cy - ay;

            var bl = dx * dx + dy * dy;
            var cl = ex * ex + ey * ey;
            var d = 0.5 / (dx * ey - dy * ex);
            var x = (ey * bl - dy * cl) * d;
            var y = (dx * cl - ex * bl) * d;
            return x * x + y * y;
        }
        /// <summary>
        /// Центр окружности проведенной по трем вершинами с координатами ...
        /// </summary>
        private static HPoint Circumcenter(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var ex = cx - ax;
            var ey = cy - ay;
            var bl = dx * dx + dy * dy;
            var cl = ex * ex + ey * ey;
            var d = 0.5 / (dx * ey - dy * ex);
            var x = ax + (ey * bl - dy * cl) * d;
            var y = ay + (dx * cl - ex * bl) * d;

            return new HPoint(x, y);
        }
        private double Dist(int i, int j)
        {
            var dx = coordsX[i] - coordsX[j];
            var dy = coordsY[i] - coordsY[j];
            return dx * dx + dy * dy;
        }
        private double Dist(double x, double y, int j)
        {
            var dx = x - coordsX[j];
            var dy = y - coordsY[j];
            return dx * dx + dy * dy;
        }
        #endregion CreationLogic

        #region GetMethods
        public IEnumerable<IHFigure> GetTriangles()
        {
            for (var t = 0; t < Triangles.Length / 3; t++)
            {
                yield return new HTriangle(t, GetTrianglePoints(t));
            }
        }
        public IEnumerable<IHEdge> GetEdges()
        {
            for (var e = 0; e < Triangles.Length; e++)
            {
                if (e > HalfEdges[e])
                {
                    var p = Points[Triangles[e]];
                    var q = Points[Triangles[NextHalfedge(e)]];
                    yield return new HEdge(e, p, q);
                }
            }
        }
        public IEnumerable<IHEdge> GetVoronoiEdges(Func<int, IHPoint> triangleVerticeSelector = null)
        {
            if (triangleVerticeSelector == null) triangleVerticeSelector = x => GetCentroid(x);
            for (var e = 0; e < Triangles.Length; e++)
            {
                if (e < HalfEdges[e])
                {
                    var p = triangleVerticeSelector(TriangleOfEdge(e));
                    var q = triangleVerticeSelector(TriangleOfEdge(HalfEdges[e]));
                    yield return new HEdge(e, p, q);
                }
            }
        }
        public IEnumerable<IHEdge> GetVoronoiEdgesBasedOnCircumCenter() => GetVoronoiEdges(GetTriangleCircumcenter);
        public IEnumerable<IHEdge> GetVoronoiEdgesBasedOnCentroids() => GetVoronoiEdges(GetCentroid);
        public IEnumerable<IHFigure> GetVoronoiCells(Func<int, IHPoint> triangleVerticeSelector = null)
        {
            if (triangleVerticeSelector == null) triangleVerticeSelector = x => GetCentroid(x);

            var seen = new HashSet<int>();
            // Держите это вне цикла, повторно используйте емкость, меньше изменяйте размеры.
            var vertices = new List<IHPoint>(10);    // Keep it outside the loop, reuse capacity, less resizes.

            for (var e = 0; e < Triangles.Length; e++)
            {
                var pointIndex = Triangles[NextHalfedge(e)];
                // True if element was added, If resize the set? O(n) : O(1)
                // True, если элемент был добавлен. Если изменить размер набора? O(n) : O(1)
                if (seen.Add(pointIndex))
                {
                    foreach (var edge in EdgesAroundPoint(e))
                    {
                        // triangleVerticeSelector cant be null, no need to check before invoke (?.).
                        // triangleVerticeSelector не может быть пустым, нет необходимости проверять перед вызовом (?.).
                        vertices.Add(triangleVerticeSelector.Invoke(TriangleOfEdge(edge)));
                    }
                    yield return new HVoronoiCell(pointIndex, vertices);
                    // Очистить элементы, сохранить емкость
                    vertices.Clear();   // Clear elements, keep capacity 
                }
            }
        }
        public IEnumerable<IHFigure> GetVoronoiCellsBasedOnCircumcenters() => GetVoronoiCells(GetTriangleCircumcenter);
        public IEnumerable<IHFigure> GetVoronoiCellsBasedOnCentroids() => GetVoronoiCells(GetCentroid);
        public IEnumerable<IHEdge> GetHullEdges() => CreateHull(GetHullPoints());
        public IHPoint[] GetHullPoints() => Array.ConvertAll<int, IHPoint>(Hull, (x) => Points[x]);
        public IHPoint[] GetTrianglePoints(int t)
        {
            var points = new List<IHPoint>();
            foreach (var p in PointsOfTriangle(t))
            {
                points.Add(Points[p]);
            }
            return points.ToArray();
        }
        public IHPoint[] GetRellaxedPoints()
        {
            var points = new List<IHPoint>();
            foreach (var cell in GetVoronoiCellsBasedOnCircumcenters())
            {
                points.Add(GetCentroid(cell.ID));
            }
            return points.ToArray();
        }
        public IEnumerable<IHEdge> GetEdgesOfTriangle(int t) => 
            CreateHull(EdgesOfTriangle(t).Select(p => Points[p]));
        public static IEnumerable<IHEdge> CreateHull(IEnumerable<IHPoint> points) => 
            points.Zip(points.Skip(1).Append(points.FirstOrDefault()), 
                (a, b) => new HEdge(0, a, b)).OfType<IHEdge>();
        public IHPoint GetTriangleCircumcenter(int t)
        {
            var vertices = GetTrianglePoints(t);
            return GetCircumcenter(vertices[0], vertices[1], vertices[2]);
        }
        public IHPoint GetCentroid(int t)
        {
            var vertices = GetTrianglePoints(t);
            return GetCentroid(vertices);
        }
        public static IHPoint GetCircumcenter(IHPoint a, IHPoint b, IHPoint c) => 
            Circumcenter(a.X, a.Y, b.X, b.Y, c.X, c.Y);
        public static IHPoint GetCentroid(IHPoint[] points)
        {
            double accumulatedArea = 0.0f;
            double centerX = 0.0f;
            double centerY = 0.0f;

            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                var temp = points[i].X * points[j].Y - points[j].X * points[i].Y;
                accumulatedArea += temp;
                centerX += (points[i].X + points[j].X) * temp;
                centerY += (points[i].Y + points[j].Y) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return new HPoint();

            accumulatedArea *= 3f;
            return new HPoint(centerX / accumulatedArea, centerY / accumulatedArea);
        }
        #endregion GetMethods

        #region ForEachMethods
        public void ForEachTriangle(Action<IHFigure> callback)
        {
            foreach (var triangle in GetTriangles())
            {
                callback?.Invoke(triangle);
            }
        }
        public void ForEachTriangleEdge(Action<IHEdge> callback)
        {
            foreach (var edge in GetEdges())
            {
                callback?.Invoke(edge);
            }
        }
        public void ForEachVoronoiEdge(Action<IHEdge> callback)
        {
            foreach (var edge in GetVoronoiEdges())
            {
                callback?.Invoke(edge);
            }
        }
        public void ForEachVoronoiCellBasedOnCentroids(Action<IHFigure> callback)
        {
            foreach (var cell in GetVoronoiCellsBasedOnCentroids())
            {
                callback?.Invoke(cell);
            }
        }
        public void ForEachVoronoiCellBasedOnCircumcenters(Action<IHFigure> callback)
        {
            foreach (var cell in GetVoronoiCellsBasedOnCircumcenters())
            {
                callback?.Invoke(cell);
            }
        }
        public void ForEachVoronoiCell(Action<IHFigure> callback,
                    Func<int, IHPoint> triangleVertexSelector = null)
        {
            foreach (var cell in GetVoronoiCells(triangleVertexSelector))
            {
                callback?.Invoke(cell);
            }
        }
        #endregion ForEachMethods

        #region Methods based on index
        /// <summary>
        /// Returns the half-edges that share a start point with the given half edge, in order.
        /// Возвращает полуребра, имеющие общую начальную точку с заданным полуребром, по порядку.
        /// </summary>
        public IEnumerable<int> EdgesAroundPoint(int start)
        {
            var incoming = start;
            do
            {
                yield return incoming;
                var outgoing = NextHalfedge(incoming);
                incoming = HalfEdges[outgoing];
            } while (incoming != -1 && incoming != start);
        }
        /// <summary>
        /// Returns the three point indices of a given triangle id.
        /// Возвращает три индекса точек заданного идентификатора треугольника.
        /// </summary>
        public IEnumerable<int> PointsOfTriangle(int t)
        {
            foreach (var edge in EdgesOfTriangle(t))
            {
                yield return Triangles[edge];
            }
        }
        /// <summary>
        /// Returns the triangle ids adjacent to the given triangle id.
        /// Will return up to three values.
        /// Возвращает треугольник, смежный с заданным треугольником. 
        /// Возвращает до трех значений.
        /// </summary>
        public IEnumerable<int> TrianglesAdjacentToTriangle(int t)
        {
            var adjacentTriangles = new List<int>();
            var triangleEdges = EdgesOfTriangle(t);
            foreach (var e in triangleEdges)
            {
                var opposite = HalfEdges[e];
                if (opposite >= 0)
                {
                    adjacentTriangles.Add(TriangleOfEdge(opposite));
                }
            }
            return adjacentTriangles;
        }
        public static int NextHalfedge(int e) => (e % 3 == 2) ? e - 2 : e + 1;
        public static int PreviousHalfedge(int e) => (e % 3 == 0) ? e + 2 : e - 1;
        /// <summary>
        /// Returns the three half-edges of a given triangle id.
        /// Возвращает три полуребра заданного идентификатора треугольника.
        /// </summary>
        public static int[] EdgesOfTriangle(int t) => new int[] { 3 * t, 3 * t + 1, 3 * t + 2 };
        /// <summary>
        /// Returns the triangle id of a given half-edge.
        /// Возвращает идентификатор треугольника заданного полуребра.
        /// </summary>
        public static int TriangleOfEdge(int e) { return e / 3; }
        #endregion Methods based on index
    }
}