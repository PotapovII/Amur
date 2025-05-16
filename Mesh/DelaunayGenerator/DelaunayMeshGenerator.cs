////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 19.08.2024 Потапов И.И.
////---------------------------------------------------------------------------
//namespace DelaunayGenerator
//{
//    using System;
//    using System.Linq;
//    using CommonLib;
//    using CommonLib.Geometry;
//    using MeshLib;
//    using MemLogLib;
//    /// <summary>
//    /// ОО: Делоне генератор выпуклой триангуляции
//    /// </summary>
//    public class DelaunayMeshGenerator
//    {
//        /// <summary>
//        /// Размер стека для перестройки треугольников по Делоне
//        /// </summary>
//        private readonly int[] EDGE_STACK = new int[1024];
//        /// <summary>
//        ///  Массив индексов вершин треугольника (каждая группа 
//        ///  из трех чисел образует треугольник). 
//        ///  Обход вершин всех треугольников направлен против часовой стрелки.        
//        /// </summary>
//        public int[] Triangles;
//        /// <summary>
//        /// Ссылки индексов ребер треугольника на ребра сопряженных треугольников
//        // (или -1 для ребер на выпуклой оболочке).
//        /// </summary>
//        public int[] HalfEdges;
//        /// <summary>
//        /// Массив координат входных точек 
//        /// </summary>
//        public IHPoint[] Points;
//        /// <summary>
//        /// Количество узлов в оболочке
//        /// </summary>
//        private int hashSize;
//        /// <summary>
//        /// Массив индексов выпуклой оболочки данных против часовой стрелки
//        /// вычисляется по hullNext по окончанию треангуляции
//        /// </summary>
//        public int[] Hull;
//        /// <summary>
//        /// Массив индексов выпуклой оболочки данных 
//        /// по направлению движения часовой стрелке
//        /// </summary>
//        private int[] hullPrev;
//        /// <summary>
//        /// Массив индексов выпуклой оболочки данных  
//        /// направлению движения против часовой стрелки
//        /// </summary>
//        private int[] hullNext;
//        /// <summary>
//        /// Массив индексов выпуклой оболочки данных против часовой стрелки
//        /// </summary>
//        private int[] hullTri;
//        /// <summary>
//        /// хэш-таблица для узлов выпулой оболочки, позволяет "быстро" по псевдо углу 
//        /// добовляемого узла определять узел на ближайшей видимой грани оболочки, 
//        /// необходимый для добавления в оболочку новых треугольников.
//        /// </summary>
//        private int[] hullHash;
//        /// <summary>
//        /// индексы точек отсортированных по растоянию от центра
//        /// </summary>
//        private int[] ids;
//        /// <summary>
//        /// Координаты центра триангуляции
//        /// </summary>
//        private double cx;
//        private double cy;

//        int i0 = 0;
//        int i1 = 0;
//        int i2 = 0;
//        /// <summary>
//        /// счетчик треугольников
//        /// </summary>
//        private int trianglesLen;
//        /// <summary>
//        /// Массив входных координат в форме [x0, y0, x1, y1, ....] типа, 
//        /// указанного в конструкторе
//        /// </summary>
//        private double[] coordsX;
//        private double[] coordsY;
//        /// <summary>
//        /// условно нулевой узел входа в оболочку
//        /// </summary>
//        private int hullStart;
//        /// <summary>
//        /// Количество узлов в оболочке
//        /// </summary>
//        private int hullSize;
//        /// <summary>
//        /// ОО: Делоне генератор выпуклой триангуляции
//        /// </summary>
//        public DelaunayMeshGenerator(){}
//        /// <summary>
//        /// Создание выпуклой Сетки
//        /// </summary>
//        /// <param name="DEGUG"></param>
//        /// <returns></returns>
//        public IMesh CreateMesh(bool DEGUG = false) 
//        {
//            TriMesh mesh = new TriMesh();
//            int CountElems = Triangles.Length / 3;
//            MEM.Alloc(CountElems, ref mesh.AreaElems);
//            for (int i = 0; i < CountElems; i++)
//            {
//                mesh.AreaElems[i].Vertex1 = (uint)(Triangles[3 * i]);
//                mesh.AreaElems[i].Vertex2 = (uint)(Triangles[3 * i + 1]);
//                mesh.AreaElems[i].Vertex3 = (uint)(Triangles[3 * i + 2]);
//            }
//            MEM.Alloc(Points.Length, ref mesh.CoordsX);
//            MEM.Alloc(Points.Length, ref mesh.CoordsY);
//            for (int i = 0; i < Points.Length; i++)
//            {
//                mesh.CoordsX[i] = Points[i].X;
//                mesh.CoordsY[i] = Points[i].Y;
//            }
//            MEM.Alloc(Hull.Length, ref mesh.BoundElems);
//            MEM.Alloc(Hull.Length, ref mesh.BoundElementsMark);
//            for (int i = 0; i < Hull.Length; i++)
//            {
//                mesh.BoundElems[i].Vertex1 = (uint)Hull[i];
//                mesh.BoundElems[i].Vertex2 = (uint)Hull[(i+1)% Hull.Length];
//                mesh.BoundElementsMark[i] = 0;
//            }
//            MEM.Alloc(Hull.Length, ref mesh.BoundKnots);
//            MEM.Alloc(Hull.Length, ref mesh.BoundKnotsMark);
//            for (int i = 0; i < Hull.Length; i++)
//            {
//                mesh.BoundKnots[i] = Hull[i];
//                mesh.BoundKnotsMark[i] = 0;
//            }
//            if(DEGUG == true)
//                mesh.Print();
//            return mesh; 
//        }
//        /// <summary>
//        /// Генерация
//        /// </summary>
//        /// <param name="points"></param>
//        /// <exception cref="ArgumentOutOfRangeException"></exception>
//        /// <exception cref="Exception"></exception>
//        public void Generator(IHPoint[] points)
//        {
//            if (points.Length < 3)
//                throw new ArgumentOutOfRangeException("Нужно как минимум 3 вершины");
//            Points = points;
//            //var n = points.Length;
//            hashSize = (int)Math.Ceiling(Math.Sqrt(points.Length));
//            var maxTriangles = 2 * points.Length - 5;
//            //MEM.Alloc(Points.Length * 2, ref coords);
//            MEM.Alloc(Points.Length, ref coordsX);
//            MEM.Alloc(Points.Length, ref coordsY);
//            MEM.Alloc(maxTriangles * 3, ref Triangles);
//            MEM.Alloc(maxTriangles * 3, ref HalfEdges);
//            MEM.Alloc(points.Length, ref hullPrev);
//            MEM.Alloc(points.Length, ref hullNext);
//            MEM.Alloc(points.Length, ref hullTri);
//            MEM.Alloc(points.Length, ref ids);

//            MEM.Alloc(hashSize, ref hullHash);


//            for (var i = 0; i < Points.Length; i++)
//            {
//                var p = Points[i];
//                coordsX[i] = p.X;
//                coordsY[i] = p.Y;
   
//            }

//            #region поиск начального треугольника
//            cx = points.Sum(x => x.X) / (points.Length);
//            cy = points.Sum(x => x.Y) / (points.Length);

//            for (int i = 0; i < points.Length; i++)
//                ids[i] = i;

//            var minDist = double.PositiveInfinity;
//            // выбираем начальную точку ближе к центру
//            for (int i = 0; i < points.Length; i++)
//            {
//                double d = Dist(i);
//                if (d < minDist)
//                {
//                    i0 = i;
//                    minDist = d;
//                }
//            }
//            minDist = double.PositiveInfinity;
//            // найдите точку, ближайшую к наачльной
//            for (int i = 0; i < points.Length; i++)
//            {
//                if (i == i0) continue;
//                double d = Dist(i0, i);
//                if (d < minDist && d > 0)
//                {
//                    i1 = i;
//                    minDist = d;
//                }
//            }
//            double minRadius = double.PositiveInfinity;
//            // найдите третью точку, которая образует наименьшую окружность с первыми двумя
//            for (int i = 0; i < points.Length; i++)
//            {
//                if (i == i0 || i == i1) continue;
//                double r = Circumradius(i);
//                if (r < minRadius)
//                {
//                    i2 = i;
//                    minRadius = r;
//                }
//            }


//            if (minRadius == double.PositiveInfinity)
//            {
//                // Если три точки не найдены! То...
//                throw new Exception("Для этих входных данных не существует триангуляции Делоне!");
//            }
//            #endregion
            
//            // Выберем оринтацию вершин начального треугольника
//            if (Orient(i0, i1, i2) == true)
//            {
//                int i = i1;
//                i1 = i2;
//                i2 = i;
//            }
//            /// Центр окружности проведенной по трем вершинами с координатами ...
//            Circumcenter();
//            // Расчет растояний от центра окружности 1 треугольника до точек триангуляции
//            var dists = new double[points.Length];
//            for (var i = 0; i < points.Length; i++)
//            {
//                dists[i] = Dist(i);
//            }
//            // быстрая сортировка точек по расстоянию от центра окружности исходного треугольника
//            Quicksort(ids, dists, 0, points.Length - 1);
           
//            // установите начальный треугольник в качестве начальной оболочки
//            hullStart = i0;
//            hullSize = 3;

//            hullNext[i0] = i1;
//            hullNext[i1] = i2;
//            hullNext[i2] = i0;

//            hullPrev[i2] = i1;
//            hullPrev[i0] = i2;
//            hullPrev[i1] = i0;

//            hullTri[i0] = 0;
//            hullTri[i1] = 1;
//            hullTri[i2] = 2;

//            hullHash[HashKey(i0)] = i0;
//            hullHash[HashKey(i1)] = i1;
//            hullHash[HashKey(i2)] = i2;
//            // счетчик треугольников
//            trianglesLen = 0;
//            // Добавление 1 треугольника в список треугольников
//            AddTriangle(i0, i1, i2, -1, -1, -1);

//            #region Поиск выпуклой оболочки и триангуляция
//            // Поиск выпуклой оболочки и триангуляция
//            for (var k = 0; k < ids.Length; k++)
//            {
//                // добавление текущего k - го узла
//                int i = ids[k];

//                // игнорировать  начальные точки треугольника
//                if (i == i0 || i == i1 || i == i2) 
//                    continue;

//                // поиск  края видимой выпуклой оболочки, используя хэш ребра
//                int start = 0;
                
//                // поиск близких по хешированному псевдо углу узлов
//                // на выпуклой оболочке 
//                for (int j = 0; j < hashSize; j++)
//                {
//                    int key = HashKey(i);
//                    start = hullHash[(key + j) % hashSize];
//                    if (start != -1 && start != hullNext[start]) 
//                        break;
//                }
//                start = hullPrev[start];
//                int e = start;
//                int q = hullNext[e];
//                // проверка видимости найденного стартового узла и возможности
//                // построения новых треугольников на оболочке
//                while (Orient(i, e, q) == false)
//                {
//                    e = q;
//                    if (e == start)
//                    {
//                        // плохой узел 
//                        e = int.MaxValue;
//                        break;
//                    }
//                    q = hullNext[e];
//                }
//                // скорее всего, это почти повторяющаяся точка; пропустите ее
//                if (e == int.MaxValue) 
//                    continue;
//                //  добавьте первый треугольник от точки
//                int t = AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);
//                // рекурсивная перестройки треугольников от точки к точке,
//                // пока они не удовлетворят условию Делоне
//                hullTri[i] = Legalize(t + 2);
//                // добавление треугольника в оболочку
//                hullTri[e] = t;
//                hullSize++;
//                // пройдите вперед по оболочке,
//                // добавляя треугольники и переворачивая их рекурсивно
//                int nextW = hullNext[e];
//                int nextE = hullNext[nextW];
//                /// проверка видимой грани (nextW,nextE) оболочки из i точки
//                /// при движении вперед по контуру 
//                while (Orient(i, nextW, nextE) == true)
//                {
//                    // добавить треугольник 
//                    t = AddTriangle(nextW, i, nextE, hullTri[i], -1, hullTri[nextW]);
//                    //  проверка и перестройка по Делоне
//                    hullTri[i] = Legalize(t + 2);
//                    // пометить как удаленный узел ущедщий из оболочки
//                    hullNext[nextW] = nextW; 
//                    hullSize--;
//                    // следующее ребро оболочки
//                    nextW = nextE;
//                    nextE = hullNext[nextW];
//                }
//                // пройдите назад с другой стороны,
              
//                int prewE = e;
//                if (prewE == start)
//                {
//                   int prewW = hullPrev[prewE];
//                    while (Orient(i, prewW, prewE) == true)
//                    {
//                        // добавить треугольник 
//                        t = AddTriangle(prewW, i, prewE, -1, hullTri[prewE], hullTri[prewW]);
//                        //  проверка и перестройка по Делоне
//                        Legalize(t + 2);
//                        hullTri[prewW] = t;
//                        // пометить как удаленный узел ущедщий из оболочки
//                        hullNext[prewE] = prewE; 
//                        hullSize--;
//                        // следующее ребро оболочки
//                        prewE = prewW;
//                        prewW = hullPrev[prewE];
//                    }
//                }
//                // пометить как удаленный
//                hullStart = hullPrev[i] = prewE;
//                hullNext[prewE] = hullPrev[nextW] = i;
//                hullNext[i] = nextW;
//                // сохраните два новых ребра в хэш-таблице
//                hullHash[HashKey(i)] = i;
//                hullHash[HashKey(prewE)] = prewE;
//            }
//            // Создаем массив граничных узлов оболочки
//            Hull = new int[hullSize];
//            int s = hullStart;
//            for (int i = 0; i < hullSize; i++)
//            {
//                Hull[i] = s;
//                s = hullNext[s];
//            }
//            #endregion
//            // удаляем временные массивы
//            hullPrev = hullNext = hullTri = null; 
//            // обрезка треангуляционных массивов
//            // Элементы
//            Triangles = Triangles.Take(trianglesLen).ToArray();
//            // Связи
//            HalfEdges = HalfEdges.Take(trianglesLen).ToArray();
//        }

//        #region CreationLogic
//        /// <summary>
//        /// знак верктоного произведения построенного на касательных к двум граням
//        /// </summary>
//        /// <param name="i"></param>
//        /// <param name="q"></param>
//        /// <param name="r"></param>
//        /// <returns></returns>
//        bool Orient(int i, int q, int r)
//        {
//            return (coordsY[q] - coordsY[i]) * (coordsX[r] - coordsX[q]) 
//                 - (coordsX[q] - coordsX[i]) * (coordsY[r] - coordsY[q]) < 0;
//        }
//        /// <summary>
//        /// рекурсивная перестройки треугольников от точки к точке,
//        /// пока они не удовлетворят условию Делоне 
//        /// </summary>
//        /// <param name="a"></param>
//        /// <returns></returns>
//        private int Legalize(int EdgeA_ID)
//        {
//            var i = 0;
//            int ar;

//            // recursion eliminated with EdgeA_ID fixed-size stack
//            // рекурсия устранена с помощью стека фиксированного размера
//            while (true)
//            {
//                var EdgeB_ID = HalfEdges[EdgeA_ID];

//                // Если пара треугольников не удовлетворяет условию Делоне
//                // (p1 находится внутри описанной окружности [p0, pl, pr]),
//                // переверните их против часовой стрелки.
//                // Выполните ту же проверку рекурсивно для новой пары
//                // треугольников
//                // 
//                //            pl                       pl
//                //           /||\                     /  \
//                //        al/ || \bl               al/    \EdgeA_ID
//                //         /  ||  \                 /      \
//                //    EdgeA_ID|| EdgeB_ID  flip    /___ar___\
//                //      p0\   ||   /p1      =>   p0\---bl---/p1
//                //         \  ||  /                 \      /
//                //        ar\ || /br         EdgeB_ID\    /br
//                //           \||/                     \  /
//                //            pr                       pr
//                //
                
//                // адрес - смешение для 1 треугольника
//                int triA_ID = EdgeA_ID - EdgeA_ID % 3;
//                ar = triA_ID + (EdgeA_ID + 2) % 3;
                
//                if (EdgeB_ID == -1)
//                {
//                    // граница выпуклой оболочки 
//                    if (i == 0)
//                        break;
//                    EdgeA_ID = EDGE_STACK[--i];
//                    continue;
//                }

//                int al = triA_ID + (EdgeA_ID + 1) % 3;
//                // адрес - смешение для 2 треугольника
//                int triB_ID = EdgeB_ID - EdgeB_ID % 3;
//                int bl = triB_ID + (EdgeB_ID + 2) % 3;

//                int p0 = Triangles[ar];
//                int pr = Triangles[EdgeA_ID];
//                int pl = Triangles[al];
//                int p1 = Triangles[bl];

//                bool illegal = InCircle(p0, pr, pl, p1);
//                if (illegal)
//                {
//                    Triangles[EdgeA_ID] = p1;
//                    Triangles[EdgeB_ID] = p0;
//                    int hbl = HalfEdges[bl];
//                    // ребро поменяно местами на другой стороне оболочки (редко);
//                    // исправить ссылку ребра смежного треугольника
//                    if (hbl == -1)
//                    {
//                        int e = hullStart;
//                        do
//                        {
//                            if (hullTri[e] == bl)
//                            {
//                                hullTri[e] = EdgeA_ID;
//                                break;
//                            }
//                            e = hullPrev[e];
//                        } 
//                        while (e != hullStart);
//                    }
//                    Link(EdgeA_ID, hbl);
//                    Link(EdgeB_ID, HalfEdges[ar]);
//                    Link(ar, bl);
//                    // не беспокойтесь о достижении предела: это может
//                    // произойти только при крайне вырожденном вводе
//                    if (i < EDGE_STACK.Length)
//                    {
//                        EDGE_STACK[i++] = triB_ID + (EdgeB_ID + 1) % 3;
//                    }
//                    else
//                    {
//                        Console.WriteLine("Переполнение стека при проверке Делоне" +
//                            " для добавленных треугольников!");
//                        break;
//                    }
//                }
//                else
//                {
//                    if (i == 0) 
//                        break;
//                    EdgeA_ID = EDGE_STACK[--i];
//                }
//            }
//            return ar;
//        }
//        /// <summary>
//        /// принадлежность узла кругу проведенному через три точки
//        /// </summary>
//        /// <param name="i"></param>
//        /// <param name="j"></param>
//        /// <param name="k"></param>
//        /// <param name="n"></param>
//        /// <returns></returns>
//        private bool InCircle(int i, int j, int k, int n)
//        {
//            var dx = coordsX[i] - coordsX[n];
//            var dy = coordsY[i] - coordsY[n];
//            var ex = coordsX[j] - coordsX[n];
//            var ey = coordsY[j] - coordsY[n];
//            var fx = coordsX[k] - coordsX[n];
//            var fy = coordsY[k] - coordsY[n];

//            var ap = dx * dx + dy * dy;
//            var bp = ex * ex + ey * ey;
//            var cp = fx * fx + fy * fy;

//            return dx * (ey * cp - bp * fy) -
//                   dy * (ex * cp - bp * fx) +
//                   ap * (ex * fy - ey * fx) < 0;
//        }
//        /// <summary>
//        /// Добавление треугольника в список треугольников
//        /// </summary>
//        /// <param name="i0">индекс вершины 0</param>
//        /// <param name="i1">индекс вершины 1</param>
//        /// <param name="i2">индекс вершины 2</param>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <param name="c"></param>
//        /// <returns>возвращает адрес смещения для нового треугольника</returns>
//        private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
//        {
//            var triangleID = trianglesLen;
//            Triangles[triangleID] = i0;
//            Triangles[triangleID + 1] = i1;
//            Triangles[triangleID + 2] = i2;

//            Link(triangleID, a);
//            Link(triangleID + 1, b);
//            Link(triangleID + 2, c);

//            trianglesLen += 3;
//            return triangleID;
//        }
//        private void Link(int EdgesID, int b)
//        {
//            HalfEdges[EdgesID] = b;
//            if (b != -1) 
//                HalfEdges[b] = EdgesID;
//        }
//        /// <summary>
//        /// Получение хеш индекса через псевдо угол точки относительно 
//        /// начального центра триангуляции
//        /// </summary>
//        /// <param name="idx"></param>
//        /// <returns></returns>
//        private int HashKey(int idx)
//        {
//            return ((int)(PseudoAngle(coordsX[idx] - cx,
//                coordsY[idx] - cy) * hashSize) % hashSize);
//        }
//        /// <summary>
//        /// определение радиуса окружности проходящую через 3 точки
//        /// </summary>
//        /// <param name="i"></param>
//        /// <returns></returns>
//        private double Circumradius(int i)
//        {
//            double dx = coordsX[i1] - coordsX[i0];
//            double dy = coordsY[i1] - coordsY[i0];
//            double ex = coordsX[i] - coordsX[i0];
//            double ey = coordsY[i] - coordsY[i0];
//            double bl = dx * dx + dy * dy;
//            double cl = ex * ex + ey * ey;
//            double d = 0.5 / (dx * ey - dy * ex);
//            double x = (ey * bl - dy * cl) * d;
//            double y = (dx * cl - ex * bl) * d;
//            return x * x + y * y;
//        }
//        /// <summary>
//        /// Центр окружности проведенной по трем вершинам
//        /// </summary>
//        private void Circumcenter()
//        {
//            double ax = coordsX[i0];
//            double ay = coordsY[i0];
//            double dx = coordsX[i1] - coordsX[i0];
//            double dy = coordsY[i1] - coordsY[i0];
//            double ex = coordsX[i2] - coordsX[i0];
//            double ey = coordsY[i2] - coordsY[i0];

//            double bl = dx * dx + dy * dy;
//            double cl = ex * ex + ey * ey;
//            double d = 0.5 / (dx * ey - dy * ex);
//            cx = ax + (ey * bl - dy * cl) * d;
//            cy = ay + (dx * cl - ex * bl) * d;
//        }
//        /// <summary>
//        /// Вычисление псевдо угола точки 
//        /// </summary>
//        /// <param name="dx"></param>
//        /// <param name="dy"></param>
//        /// <returns></returns>
//        private static double PseudoAngle(double dx, double dy)
//        {
//            var p = dx / (Math.Abs(dx) + Math.Abs(dy));
//            return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
//        }
//        /// <summary>
//        /// быстрая сортировка точек по расстоянию от центра окружности исходного треугольника
//        /// </summary>
//        /// <param name="ids">индекс сортируемой точки</param>
//        /// <param name="dists">дистанции от центра до сортируемой точки</param>
//        /// <param name="left">начальный номер узла сортируемых массивов</param>
//        /// <param name="right">конечный номер узла сортируемых массивов</param></param>
//        private static void Quicksort(int[] ids, double[] dists, int left, int right)
//        {
//            if (right - left <= 20)
//            {
//                for (var i = left + 1; i <= right; i++)
//                {
//                    var temp = ids[i];
//                    var tempDist = dists[temp];
//                    var j = i - 1;
//                    while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
//                    ids[j + 1] = temp;
//                }
//            }
//            else
//            {
//                var median = (left + right) >> 1;
//                var i = left + 1;
//                var j = right;
//                Swap(ids, median, i);
//                if (dists[ids[left]] > dists[ids[right]]) Swap(ids, left, right);
//                if (dists[ids[i]] > dists[ids[right]]) Swap(ids, i, right);
//                if (dists[ids[left]] > dists[ids[i]]) Swap(ids, left, i);

//                var temp = ids[i];
//                var tempDist = dists[temp];
//                while (true)
//                {
//                    do i++; while (dists[ids[i]] < tempDist);
//                    do j--; while (dists[ids[j]] > tempDist);
//                    if (j < i) break;
//                    Swap(ids, i, j);
//                }
//                ids[left + 1] = ids[j];
//                ids[j] = temp;

//                if (right - i + 1 >= j - left)
//                {
//                    Quicksort(ids, dists, i, right);
//                    Quicksort(ids, dists, left, j - 1);
//                }
//                else
//                {
//                    Quicksort(ids, dists, left, j - 1);
//                    Quicksort(ids, dists, i, right);
//                }
//            }
//        }
//        private static void Swap(int[] arr, int i, int j)
//        {
//            var tmp = arr[i];
//            arr[i] = arr[j];
//            arr[j] = tmp;
//        }

//        //private static double Circumradius(double ax, double ay, double bx, double by, double cx, double cy)
//        //{
//        //    var dx = bx - ax;
//        //    var dy = by - ay;
//        //    var ex = cx - ax;
//        //    var ey = cy - ay;

//        //    var bl = dx * dx + dy * dy;
//        //    var cl = ex * ex + ey * ey;
//        //    var d = 0.5 / (dx * ey - dy * ex);
//        //    var x = (ey * bl - dy * cl) * d;
//        //    var y = (dx * cl - ex * bl) * d;
//        //    return x * x + y * y;
//        //}

//        private double Dist(int i, int j)
//        {
//            var dx = coordsX[i] - coordsX[j];
//            var dy = coordsY[i] - coordsY[j];
//            return dx * dx + dy * dy;
//        }
//        private double Dist(int j)
//        {
//            var dx = cx - coordsX[j];
//            var dy = cy - coordsY[j];
//            return dx * dx + dy * dy;
//        }


//        #endregion CreationLogic
//    }
//}