//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 30.09.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                  + 
//                 кодировка : 29.03.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace TestDelaunayGenerator
{
    using System;
    using System.Linq;
    using CommonLib;
    using CommonLib.Geometry;
    using MeshLib;
    using System.Collections.Generic;
    using TestDelaunayGenerator.Boundary;
    using System.Threading.Tasks;
    using MemLogLib;
    using GeometryLib.Locators;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Troika
    {
        public int flag;
        public int i;
        public int j;
        public int k;
        public int this[int index]
        {
            get => index == 0 ? i : index == 1 ? j : k;
            set { if (index == 0) i = value; else if (index == 1) j = value; else k = value; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int i, int j, int k) Get() => (i, j, k);
        public TriElement GetTri => new TriElement((uint)i, (uint)j, (uint)k);
    }

    /// <summary>
    /// ОО: Делоне генератор выпуклой триангуляции
    /// </summary>
    public class DelaunayMeshGenerator
    {
        /// <summary>
        /// Количество треугольников, которые стали "нулевыми" треугольниками в заражении
        /// </summary>
        protected int infectionInitiatorCount = 0;
        /// <summary>
        /// 0 - треугольник не принадлежит области
        /// 1 - треугольник принадлежит области
        /// 2 - треугольник еще не обработан
        /// </summary>
        protected int[] isIncluded = null;


        /// <summary>
        /// Размер стека для перестройки треугольников по Делоне
        /// </summary>
        private int[] EdgeStack;
        /// <summary>
        ///  Массив индексов вершин треугольника (каждая группа 
        ///  из трех чисел образует треугольник). 
        ///  Обход вершин всех треугольников направлен против часовой стрелки.        
        ///  (0, 1, 2),(.. ,.. , ..),(.. ,.. , ..)
        /// </summary>
        public Troika[] STriangles;
        /// <summary>
        /// Ссылки индексов ребер треугольника на ребра сопряженных треугольников
        // (или -1 для ребер на выпуклой оболочке). (Ребра диаграмы Вронского)
        /// </summary>
        public int[] HalfEdges;
        /// <summary>
        /// Массив координат входных точек 
        /// </summary>
        public IHPoint[] Points;
        /// <summary>
        /// Маркер узла (внутренний внешний)
        /// </summary>
        private bool[] mark;

        /// <summary>
        /// Размерность хеш пространства
        /// </summary>
        private int hashSize;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных против часовой стрелки
        /// вычисляется по hullNext по окончанию треангуляции
        /// </summary>
        public int[] Hull;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных 
        /// по направлению движения часовой стрелке
        /// </summary>
        private int[] hullPrev;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных  
        /// направлению движения против часовой стрелки
        /// </summary>
        private int[] hullNext;
        /// <summary>
        /// Массив индексов выпуклой оболочки данных против часовой стрелки
        /// </summary>
        private int[] hullTri;
        /// <summary>
        /// хэш-таблица для узлов выпулой оболочки, позволяет "быстро" по псевдо углу 
        /// добовляемого узла определять узел на ближайшей видимой грани оболочки, 
        /// необходимый для добавления в оболочку новых треугольников.
        /// </summary>
        private int[] hullHash;
        /// <summary>
        /// индексы точек отсортированных по растоянию от центра
        /// </summary>
        private int[] ids;
        /// <summary>
        /// Координаты центра триангуляции X
        /// </summary>
        private double cx;
        /// <summary>
        /// Координаты центра триангуляции Y
        /// </summary>
        private double cy;
        /// <summary>
        /// центр триангуляции
        /// </summary>
        protected HPoint pc;

        int i0 = 0;
        int i1 = 0;
        int i2 = 0;
        /// <summary>
        /// счетчик треугольников
        /// </summary>
        private int trianglesLen;
        /// <summary>
        /// Массив входных координат в форме [x0, y0, x1, y1, ....] типа, 
        /// указанного в конструкторе
        /// </summary>
        private double[] coordsX;
        private double[] coordsY;
        /// <summary>
        /// Квадрат расстояния от центра генерации до точки сетки
        /// </summary>
        private double[] dists;

        /// <summary>
        /// условно нулевой узел входа в оболочку
        /// </summary>
        private int hullStart;
        /// <summary>
        /// Количество узлов, образующих выпуклую оболочку
        /// </summary>
        private int CountHullKnots;

        /// <summary>
        /// Внешняя точка, не входящая в сетку
        /// </summary>
        IHPoint externalPoint;
        /// <summary>
        /// True - использовать предварительную фильтрацию точек, т.е.
        /// перед триангуляцией оставить только те точки, которые гарантированно попадут в триангуляцию
        /// </summary>
        protected bool usePointFilter = true;
        /// <summary>
        /// True - использовать предварительную фильтрацию точек, т.е.
        /// перед триангуляцией оставить только те точки, которые гарантированно попадут в триангуляцию
        /// </summary>
        public bool UsePointFilter => usePointFilter;

        /// <summary>
        /// Контейнер для ограниченных областей
        /// </summary>
        public BoundaryContainer boundaryContainer;
        int ii0, ii1, ii2;
        /// <summary>
        /// ОО: Делоне генератор выпуклой триангуляции
        /// </summary>
        /// <param name="points">Множество точек, из которых будет сформирована триангуляция Делоне.
        /// Если заданы граничные узлы, то они также должны входить в текущий массив.</param>
        /// <param name="boundaryContainer">контейнер для ограниченных областей, усекающих триангуляцию</param>
        /// <param name="usePointFilter">true - использовать предварительную фильтрацию точек,
        /// которые гарантированно не войдут в триангуляцию</param>
        public DelaunayMeshGenerator(
            IHPoint[] points, BoundaryContainer boundaryContainer = null, bool usePointFilter = true)
        {
            if (points is null || points.Length < 3)
                throw new ArgumentOutOfRangeException("Нужно как минимум 3 вершины");
            this.Points = points;
            this.boundaryContainer = boundaryContainer;
            this.usePointFilter = usePointFilter;
        }

        /// <summary>
        /// Генерация объекта симпл - сетки
        /// </summary>
        /// <param name="DEGUG"></param>
        /// <returns></returns>
        public IMesh CreateMesh(bool DEGUG = false)
        {
            int CountElems = STriangles.Length;

            TriMesh mesh = new TriMesh();
            MEM.Alloc(CountElems, ref mesh.AreaElems);
            MEM.Alloc(CountElems, ref isIncluded, (int)2);

            List<TriElement> tri = new List<TriElement>();
            for (int i = 0; i < CountElems; i++)
            {
                if (CheckIn(i) == true)
                    tri.Add(STriangles[i].GetTri);
            }
#if DEBUG
            Console.WriteLine($"Количество нулевых треугольников в заражении: {infectionInitiatorCount}.");
#endif
            //сохраняем все треугольники сетки в объект сетки
            mesh.AreaElems = tri.ToArray();
            mesh.CoordsX = this.coordsX;
            mesh.CoordsY = this.coordsY;
            #region формирование граничных точек и линий
            //определение количества точек границы
            int boundaryPointsAmount;
            if (boundaryContainer != null)
                boundaryPointsAmount = this.boundaryContainer.AllBoundaryKnots.Length + CountHullKnots;
            else
                boundaryPointsAmount = CountHullKnots;
            //выделение памяти
            MEM.Alloc(boundaryPointsAmount, ref mesh.BoundElems);
            MEM.Alloc(boundaryPointsAmount, ref mesh.BoundElementsMark);
            MEM.Alloc(boundaryPointsAmount, ref mesh.BoundKnots);
            MEM.Alloc(boundaryPointsAmount, ref mesh.BoundKnotsMark);
            int meshIndex = 0;
            //граничные точки и линии, сформированные на основе переданных точек границы (boundarySet)
            if (boundaryContainer != null)
            {
                //первый индекс граничной точки в массиве
                int notBoundaryOffset = this.Points.Length - boundaryContainer.AllBoundaryKnots.Length;
                //текущее смещение в общем массиве точек
                int currentOffset = notBoundaryOffset;
                foreach (BoundaryHill boundary in boundaryContainer)
                {
                    //индекс массива точек, обозначающий последнюю вершину текущей границы
                    int boundaryLastId = currentOffset + boundary.Length;
                    for (int i = currentOffset; i < boundaryLastId; i++)
                    {
                        int edgeStartId = i; //индекс точки из массива, являющейся началом ребра
                        int edgeEndId = i + 1; //конец ребра
                        //если выход за пределы массива,
                        //то конечной вершиной ребра будет первая граничная точка из текущей границы
                        if (edgeEndId == boundaryLastId)
                            edgeEndId = currentOffset;

                        mesh.BoundElems[meshIndex].Vertex1 = (uint)edgeStartId;
                        mesh.BoundElems[meshIndex].Vertex2 = (uint)edgeEndId;
                        mesh.BoundKnots[meshIndex] = edgeStartId;
                        meshIndex++;
                    }
                    currentOffset += boundary.Length;
                }
            }
            //TODO убрать вовсе или изменить логику
            //граничные точки и линии, сформированные на основе всего множества точек сетки (естественным образом)
            if (boundaryContainer == null || boundaryContainer.Count % 2 == 0)
                for (int i = 0; i < CountHullKnots; i++)
                {
                    mesh.BoundElems[meshIndex].Vertex1 = (uint)Hull[i];
                    mesh.BoundElems[meshIndex].Vertex2 = (uint)Hull[(i + 1) % CountHullKnots];
                    mesh.BoundElementsMark[meshIndex] = 0;
                    mesh.BoundKnots[meshIndex] = Hull[i];
                    mesh.BoundKnotsMark[meshIndex] = 0;
                    meshIndex++;
                }
            #endregion

            if (DEGUG == true)
                mesh.Print();
            return mesh;
        }

        /// <summary>
        /// Выполняет предварительную фильтрацию точек, оставляя лишь те, что гарантированно войдут в триагнуляцию.
        /// Имеет смысл, если <see cref="UsePointFilter"/> находится в True.
        /// </summary>
        /// <param name="parallel">True - распараллелить цикл в методе</param>
        public void PreFilterPoints(bool parallel = true)
        {
            if (boundaryContainer != null)
                //выделение памяти
                MEM.Alloc(Points.Length, ref mark, value: true);
            //если фильтр предварительной фильтрации выключен, то покидаем метод
            if (!usePointFilter || boundaryContainer is null)
                return;

            cy = Points.Sum(x => x.Y) / (Points.Length);
            var maxX = Points.Max(x => x.X);
            externalPoint = new HPoint(maxX * 1.1, cy);

            bool withHashSquare = true;
            //выполняем проверку точек вплоть до последней точки из НАЧАЛЬНОГО массива (Points),
            //т.к. массив точек ДОПОЛНЕН массивом граничных точек
            if (parallel == true)
                Parallel.For(
                    0, Points.Length - this.boundaryContainer.AllBoundaryKnots.Length, (range, loopState) =>
                    {
                        int i = range;
                        //for (var i = range; i < Points.Length - this.boundaryContainer.AllBoundaryKnots.Length; i++)
                        //{
                        // Проверяем, входит ли точка в сетку или же её необходимо исключить
                        mark[i] = InArea(i, withHashSquare);
                        //}
                    }
                );
            else
                for (var i = 0; i < Points.Length - this.boundaryContainer.AllBoundaryKnots.Length; i++)
                {
                    // Проверяем, входит ли точка в сетку или же её необходимо исключить
                    mark[i] = InArea(i, withHashSquare);
                }

            //очищаем массив от неиспользуемых точек, обрезаем до нужного размера
            int markedPointsAmount = mark.Count(x => x is true);
            //следующий индекс для перезаписи в массиве
            int curNewPointIndex = 0;
            //сужаем исходный массив до размера, необходимого лишь множеству точек из области построения
            for (int i = 0; i < mark.Length; i++)
            {
                if (mark[i])
                {
                    Points[curNewPointIndex] = //новое положение точки
                        Points[i]; //старое положение точки
                    curNewPointIndex++;
                }
            }

            //обрезаем исходный массив точек до необходимого размера
            Array.Resize(ref Points, markedPointsAmount);

            //TODO mark не нужен, ибо в нем все элементы True
            //перезаполняем mark
            MEM.Alloc<bool>(Points.Length, ref mark, value: true);
        }


        /// <summary>
        /// Генерация
        /// </summary>
        public void Generator()
        {
            //выделение памяти
            MEM.Alloc(Points.Length, ref mark, value: true);

            //TODO выяснить резонно ли искать центр тяжести области
            //находим центр тяжести области
            cx = Points.Sum(x => x.X) / (Points.Length);
            cy = Points.Sum(x => x.Y) / (Points.Length);
            pc = new HPoint(cx, cy);

            //внешняя точка
            //дублирование инициализации, т.к. метод PreFilterPoints() мб и не вызван
            if (!usePointFilter)
            {
                var maxX = Points.Max(x => x.X);
                externalPoint = new HPoint(maxX * 1.1, cy); //TODO: исправить формирование внешней точки
            }

            //выделяем память
            int maxTriangles = 2 * Points.Length - 5;
            MEM.Alloc(Points.Length, ref EdgeStack);
            MEM.Alloc(Points.Length, ref coordsX);
            MEM.Alloc(Points.Length, ref coordsY);
            //MEM.Alloc(maxTriangles * 3, ref Triangles);
            MEM.Alloc(maxTriangles * 3, ref HalfEdges);
            MEM.Alloc(maxTriangles, ref STriangles);
            MEM.Alloc(Points.Length, ref hullPrev);
            MEM.Alloc(Points.Length, ref hullNext);
            MEM.Alloc(Points.Length, ref hullTri);
            MEM.Alloc(Points.Length, ref ids);
            MEM.Alloc(Points.Length, ref dists);
            hashSize = (int)Math.Ceiling(Math.Sqrt(Points.Length)); //размер хэш-пространства
            MEM.Alloc(hashSize, ref hullHash);

            //заполняем массивы хранящие значения X, Y и метку отрисовки точки
            for (var i = 0; i < Points.Length; i++)
            {
                var p = Points[i];
                coordsX[i] = p.X;
                coordsY[i] = p.Y;
                mark[i] = true;
            }

            // Начальное состояние адресации вершин
            for (int i = 0; i < Points.Length; i++)
                ids[i] = i;

            #region поиск начального треугольника
            double minRadius = double.PositiveInfinity;
            var minDist = double.PositiveInfinity;
            #region Попытка упростить формирование начальной оболочки. Пока случаются проблемы
            //функция для получения строкового значения ряда координат по их индексам
            string ToStringCoords(IEnumerable<int> coordsItems)
            {
                IEnumerable<string> stringCoords = coordsItems.Select(
                i => string.Format("{0};{1}", Points[i].X, Points[i].Y));
                return string.Join("|", stringCoords);
            }

            ////TODO искать все 3 точки сразу, в одном цикле относительно центра тяжести области
            //// выбираем начальную точку ближе к центру
            //var curDists = new double[3] {
            //double.PositiveInfinity,
            //double.PositiveInfinity,
            //double.PositiveInfinity,
            //}; //0 - расстояние до i0, 2 - расстояние до i2
            //for (int i = 0; i < Points.Length; i++)
            //{
            //    //if (!mark[i]) continue;
            //    double curDist = Dist(i);
            //    //Console.WriteLine(curDist);
            //    //if (curDist < minDist)
            //    if (curDist < curDists[2])
            //    {
            //        i2 = i1;
            //        i1 = i0;
            //        i0 = i;
            //        curDists[2] = curDists[1];
            //        curDists[1] = curDists[0];
            //        curDists[0] = curDist;
            //        minDist = curDist;
            //    }
            //}

            //minRadius = Circumradius(i2);

            //int[] seedTriangleIds = { i0, i1, i2 };
            //Console.WriteLine($"Координаты центра: {cx};{cy}");
            //Console.WriteLine($"Новый способ: {string.Join(", ", seedTriangleIds)}");
            //Console.WriteLine($"Координаты: {ToStringCoords(seedTriangleIds)}");

            #endregion

            #region старый способ определение нач оболочки
            minDist = double.PositiveInfinity;
            for (int i = 0; i < Points.Length; i++)
            {
                //if (mark[i] == false) continue;
                double d = Dist(i);
                if (d < minDist)
                {
                    i0 = i;
                    minDist = d;
                }
            }
            minDist = double.PositiveInfinity;
            // найдите точку, ближайшую к начальной
            for (int i = 0; i < Points.Length; i++)
            {
                if (i == i0) continue;
                //if (mark[i] == false) continue;
                double d = Dist(i0, i);
                if (d < minDist && d > 0)
                {
                    i1 = i;
                    minDist = d;
                }
            }
            // найдите третью точку, которая образует
            // наименьшую окружность с первыми двумя точками
            for (int i = 0; i < Points.Length; i++)
            {
                if (i == i0 || i == i1) continue;
                //if (mark[i] == false) continue;
                double r = Circumradius(i);
                if (r < minRadius)
                {
                    i2 = i;
                    minRadius = r;
                }
            }

            //int[] oldTriangleIds = { i0, i1, i2 };
            //Console.WriteLine($"Старый: {string.Join(", ", oldTriangleIds)}");
            //Console.WriteLine($"Координаты: {ToStringCoords(oldTriangleIds)}");
            #endregion

            if (minRadius == double.PositiveInfinity)
            {
                // Если три точки не найдены! То...
                throw new Exception("Для этих входных данных не существует триангуляции Делоне!");
            }
            // Выберем оринтацию вершин начального треугольника
            if (Orient(i0, i1, i2) == true)
            {
                int i = i1;
                i1 = i2;
                i2 = i;
            }
            #endregion
            //пересчет центра области - центра описанной окружности около начального треугольника
            Circumcenter();
            //расчет расстояний от центра области до каждой из точек в области
            for (var i = 0; i < Points.Length; i++)
            {
                if (mark[i] == false) continue; //пропускает не входящие в область построения
                dists[i] = Dist(i);
            }
            // быстрая сортировка точек по расстоянию от
            // центра окружности исходного треугольника
            Quicksort(ids, dists, 0, Points.Length - 1);

            #region начальная оболочка из первого треугольника
            // стартовый условно нулевой узел входа в оболочку
            hullStart = i0;
            CountHullKnots = 3;
            hullNext[i0] = i1;
            hullNext[i1] = i2;
            hullNext[i2] = i0;

            hullPrev[i2] = i1;
            hullPrev[i0] = i2;
            hullPrev[i1] = i0;

            hullTri[i0] = 0;
            hullTri[i1] = 1;
            hullTri[i2] = 2;

            hullHash[HashKey(i0)] = i0;
            hullHash[HashKey(i1)] = i1;
            hullHash[HashKey(i2)] = i2;
            // счетчик треугольников
            trianglesLen = 0;
            // Добавление 1 треугольника в список треугольников
            AddTriangle(i0, i1, i2, -1, -1, -1);
            #endregion

            #region Поиск выпуклой оболочки и триангуляция
            //TODO можно игнорировать первые 3 узла, т.к. составляют начальную оболочку
            // Поиск выпуклой оболочки и триангуляция
            //узлы, составляющие начальную оболочку (первые 3) не учитываются
            for (var k = 3; k < ids.Length; k++)
            //for (var k = 0; k < ids.Length; k++)
            {
                // добавление текущего k - го узла
                int i = ids[k];

                // поиск  края видимой выпуклой оболочки, используя хэш ребра
                //ближайший узел к текущему на выпуклой оболочке
                int start = 0;
                // поиск близкого узла на выпуклой оболочке 
                // по псевдо углу хеширования
                for (int j = 0; j < hashSize; j++)
                {
                    int key = HashKey(i);
                    start = hullHash[(key + j) % hashSize];
                    if (start != -1 && start != hullNext[start])
                        break;
                }
                start = hullPrev[start];
                int e = start;
                int q = hullNext[e];
                // проверка видимости найденного стартового узла и возможности
                // построения новых треугольников на оболочке
                //true - грань видима для добавляемой точки
                while (Orient(i, e, q) == false)
                {
                    e = q;
                    if (e == start)
                    {
                        // плохой узел 
                        e = int.MaxValue;
                        break;
                    }
                    q = hullNext[e];
                }
                // скорее всего, это почти повторяющаяся точка; пропустите ее
                if (e == int.MaxValue)
                    continue;
                // если e - hullNext[e] - на видимой границе оболочки
                //  добавьте первый треугольник от точки i
                //    hullTri[e]
                //        |
                // -- e ---- hullNext[e] ---
                //     \       /
                //  -1  \     / -1
                //       \   /
                //         i

                //индекс первой вершины треугольника в массиве треугольников
                int t = AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);
                // рекурсивная перестройки треугольников от точки к точке,
                // пока они не удовлетворят условию Делоне
                hullTri[i] = Legalize(t + 2);
                // добавление треугольника в оболочку
                hullTri[e] = t;
                CountHullKnots++;
                // пройдите вперед по оболочке,
                // добавляя треугольники и переворачивая их рекурсивно
                int nextW = hullNext[e];
                int nextE = hullNext[nextW];
                /// проверка видимой грани (nextW,nextE) оболочки из i точки
                /// при движении вперед по контуру 
                while (Orient(i, nextW, nextE) == true)
                {
                    // если nextW - hullNext[nextW] - на видимой границе оболочки
                    //  добавьте первый треугольник от точки i
                    //
                    //                 hullTri[nextW]
                    //                     |
                    //       ---- nextW ----- hullNext[nextW] --->
                    //               \         /
                    //    hullTri[i]  \       / -1
                    //                 \     /
                    //                  \   /
                    //                    i    
                    // добавить треугольник 
                    t = AddTriangle(nextW, i, nextE, hullTri[i], -1, hullTri[nextW]);
                    //  проверка и перестройка по Делоне
                    hullTri[i] = Legalize(t + 2);
                    // пометить как удаленный узел ущедщий из оболочки
                    hullNext[nextW] = nextW;
                    CountHullKnots--;
                    // следующее ребро оболочки
                    nextW = nextE;
                    nextE = hullNext[nextW];
                }
                // пройдите назад с другой стороны,
                int prewE = e;
                if (prewE == start)
                {
                    int prewW = hullPrev[prewE];
                    while (Orient(i, prewW, prewE) == true)
                    {
                        //  если prewW  - prewE - на видимой границе оболочки
                        //  добавьте первый треугольник от точки i
                        //
                        //                 hullTri[prewW]
                        //                     |
                        //       ----  nextW -----  prewE ---
                        //               \         /
                        //            -1  \       / hullTri[prewE]
                        //                 \     /
                        //                  \   /
                        //                    i    
                        // добавить треугольник 
                        t = AddTriangle(prewW, i, prewE, -1, hullTri[prewE], hullTri[prewW]);
                        //  проверка и перестройка по Делоне
                        Legalize(t + 2);
                        hullTri[prewW] = t;
                        // пометить как удаленный узел ущедщий из оболочки
                        hullNext[prewE] = prewE;
                        CountHullKnots--;
                        // следующее ребро оболочки
                        prewE = prewW;
                        prewW = hullPrev[prewE];
                    }
                }
                // пометить как удаленный
                hullStart = hullPrev[i] = prewE;
                hullNext[prewE] = hullPrev[nextW] = i;
                hullNext[i] = nextW;
                // сохраните два новых ребра в хэш-таблице
                hullHash[HashKey(i)] = i;
                hullHash[HashKey(prewE)] = prewE;
            }
            // Создаем массив граничных узлов оболочки
            Hull = new int[CountHullKnots];
            int s = hullStart;
            for (int i = 0; i < CountHullKnots; i++)
            {
                Hull[i] = s;
                s = hullNext[s];
            }
            #endregion
            // удаляем временные массивы
            hullPrev = hullNext = hullTri = null;
            // обрезка треангуляционных массивов
            // узлы треугольных элементов
            //Triangles = Triangles.Take(trianglesLen).ToArray();
            // ребра диаграмы Вронского
            HalfEdges = HalfEdges.Take(trianglesLen).ToArray();
        }


        #region CreationLogic
        /// <summary>
        /// знак верктоного произведения построенного на касательных к двум граням. <br/>
        /// Используется для проверки угла на выпуклость, т.е. true - угол < 180
        /// </summary>
        /// <param name="i"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        bool Orient(int i, int q, int r)
        {
            return (coordsY[q] - coordsY[i]) * (coordsX[r] - coordsX[q])
                 - (coordsX[q] - coordsX[i]) * (coordsY[r] - coordsY[q]) < 0;
        }
        /// <summary>
        /// рекурсивная перестройки треугольников от точки к точке,
        /// пока они не удовлетворят условию Делоне 
        /// </summary>
        /// <param name="EdgeA_ID">индекс 3-ей вершины треугольника в массиве Triangles</param>
        /// <returns>индекс 2-ой (средней) вершины треугольника</returns>
        private int Legalize(int EdgeA_ID)
        {
            var i = 0;
            int ar;

            // recursion eliminated with EdgeA_ID fixed-size stack
            // рекурсия устранена с помощью стека фиксированного размера
            while (true)
            {
                var EdgeB_ID = HalfEdges[EdgeA_ID];
                

                // Если пара треугольников не удовлетворяет условию Делоне
                // (p1 находится внутри описанной окружности [p0, pl, pr]),
                // переверните их против часовой стрелки.
                // Выполните ту же проверку рекурсивно для новой пары
                // треугольников
                //                                    triA
                //            pl                       pl
                //           /||\                     /  \
                //        al/ || \bl               al/    \EdgeA_ID
                //         /  ||  \                 /      \
                //    EdgeA_ID|| EdgeB_ID  flip    /___ar___\
                //      p0\   ||   /p1      =>   p0\---bl---/p1
                //         \  ||  /                 \      /
                //        ar\ || /br         EdgeB_ID\    /br
                //           \||/                     \  /
                //            pr                       pr
                //                                    triB

                // адрес - смешение для 1 треугольника (1-ый индекс в треугольнике)
                int triA_ID = EdgeA_ID - EdgeA_ID % 3;
                ar = triA_ID + (EdgeA_ID + 2) % 3;

                //если смежный треугольник не был найден (т.е. -1), то достаем следующий из стека
                if (EdgeB_ID == -1)
                {
                    // граница выпуклой оболочки 
                    if (i == 0)
                        break;
                    EdgeA_ID = EdgeStack[--i];
                    continue;
                }

                int al = triA_ID + (EdgeA_ID + 1) % 3;
                // адрес - смешение для 2 треугольника
                int triB_ID = EdgeB_ID - EdgeB_ID % 3;
                int bl = triB_ID + (EdgeB_ID + 2) % 3;

                //индексы вершин двух смежных треугольников
                //int p0 = Triangles[ar];
                //int pr = Triangles[EdgeA_ID];
                //int pl = Triangles[al];
                //int p1 = Triangles[bl]; 
                int idxElemA = triA_ID / 3;
                int idxElemB = triB_ID / 3;
                int p0 = STriangles[idxElemA][(EdgeA_ID + 2) % 3];
                int pr = STriangles[idxElemA][(EdgeA_ID + 0) % 3];
                int pl = STriangles[idxElemA][(EdgeA_ID + 1) % 3];
                //вершина смежного треугольника
                int p1 = STriangles[idxElemB][(EdgeB_ID + 2) % 3];

                bool illegal = InCircle(p0, pr, pl, p1);
                if (illegal)
                {
                    //Triangles[EdgeA_ID] = p1;
                    //Triangles[EdgeB_ID] = p0;
                    //                                    triA
                    //            pl                       pl
                    //           /||\                     /  \
                    //        al/ || \bl               al/    \EdgeA_ID
                    //         /  ||  \                 /      \
                    //    EdgeA_ID|| EdgeB_ID  flip    /___ar___\
                    //      p0\   ||   /p1      =>   p0\---bl---/p1
                    //         \  ||  /                 \      /
                    //        ar\ || /br         EdgeB_ID\    /br
                    //           \||/                     \  /
                    //            pr                       pr
                    //                                    triB
                    STriangles[idxElemA][(EdgeA_ID + 0) % 3] = p1;
                    STriangles[idxElemB][(EdgeB_ID + 0) % 3] = p0;
                    int hbl = HalfEdges[bl];
                    // ребро поменяно местами на другой стороне оболочки (редко);
                    // исправить ссылку ребра смежного треугольника
                    if (hbl == -1)
                    {
                        int e = hullStart;
                        do
                        {
                            if (hullTri[e] == bl)
                            {
                                hullTri[e] = EdgeA_ID;
                                break;
                            }
                            e = hullPrev[e];
                        }
                        while (e != hullStart);
                    }
                    Link(EdgeA_ID, hbl);
                    Link(EdgeB_ID, HalfEdges[ar]);
                    Link(ar, bl);
                    // не беспокойтесь о достижении предела: это может
                    // произойти только при крайне вырожденном вводе
                    if (i < EdgeStack.Length)
                    {
                        //помещаем середину второго треугольника полученного при флипе
                        EdgeStack[i++] = triB_ID + (EdgeB_ID + 1) % 3;
                    }
                    else
                    {
                        Console.WriteLine("Переполнение стека при проверке Делоне" +
                            " для добавленных треугольников!");
                        break;
                    }
                }
                else
                {
                    if (i == 0)
                        break;
                    EdgeA_ID = EdgeStack[--i];
                }
            }
            return ar;
        }
        /// <summary>
        /// принадлежность узла кругу проведенному через три точки
        /// </summary>
        /// <param name="i">V1</param>
        /// <param name="j">V2</param>
        /// <param name="k">V3</param>
        /// <param name="n">проверяемый узел, не должен входить в окружность</param>
        /// <returns></returns>
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
        /// <returns>возвращает адрес смещения для нового треугольника</returns>
        
        private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
        {
            var triangleID = trianglesLen / 3;
            
            STriangles[triangleID].i = i0;
            STriangles[triangleID].j = i1;
            STriangles[triangleID].k = i2;

            triangleID = trianglesLen;
            
            //Triangles[triangleID] = i0;
            //Triangles[triangleID + 1] = i1;
            //Triangles[triangleID + 2] = i2;

            Link(triangleID, a);
            Link(triangleID + 1, b);
            Link(triangleID + 2, c);
            trianglesLen += 3;
            return triangleID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Link(int EdgesID, int b)
        {
            HalfEdges[EdgesID] = b;
            if (b != -1)
                HalfEdges[b] = EdgesID;
        }
        /// <summary>
        /// Получение хеш индекса через псевдо угол точки относительно 
        /// начального центра триангуляции
        /// </summary>
        /// <param name="idx">индекс точки в исходном массиве</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int HashKey(int idx)
        {
            //разность координат между текущей точкой и центром триангуляции требуется для того,
            //чтобы принять центр триангуляции за центр координат
            return (int)(PseudoAngle(coordsX[idx] - cx,
                coordsY[idx] - cy) * hashSize) % hashSize;
        }
        /// <summary>
        /// определение квадрата радиуса окружности проходящей через 3 точки
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private double Circumradius(int i)
        {
            double dx = coordsX[i1] - coordsX[i0];
            double dy = coordsY[i1] - coordsY[i0];
            double ex = coordsX[i] - coordsX[i0];
            double ey = coordsY[i] - coordsY[i0];
            double bl = dx * dx + dy * dy;
            double cl = ex * ex + ey * ey;
            double d = 0.5 / (dx * ey - dy * ex);
            double x = (ey * bl - dy * cl) * d;
            double y = (dx * cl - ex * bl) * d;
            return x * x + y * y;
        }
        /// <summary>
        /// Пересчет начальной точки области - центра описанной окружности около начальной оболочки,
        /// т.е. начально треугольника
        /// </summary>
        private void Circumcenter()
        {
            //координаты вершин начального треугольника
            double ax = coordsX[i0];
            double ay = coordsY[i0];
            double dx = coordsX[i1] - coordsX[i0];
            double dy = coordsY[i1] - coordsY[i0];
            double ex = coordsX[i2] - coordsX[i0];
            double ey = coordsY[i2] - coordsY[i0];

            //расчет центра описанной окружности
            double bl = dx * dx + dy * dy;
            double cl = ex * ex + ey * ey;
            double d = 0.5 / (dx * ey - dy * ex);
            cx = ax + (ey * bl - dy * cl) * d;
            cy = ay + (dx * cl - ex * bl) * d;
            pc = new HPoint(cx, cy);
        }
        /// <summary>
        /// Вычисление псевдо угола точки 
        /// </summary>
        /// <param name="dx">отклонение точки от центра координат по оси Х</param>
        /// <param name="dy">отклонение точки от центра координат по оси Y</param>
        /// <returns>псевно угол (упрощенная альтернатива полярному углу)</returns>
        private static double PseudoAngle(double dx, double dy)
        {
            var p = dx / (Math.Abs(dx) + Math.Abs(dy));
            return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
        }
        /// <summary>
        /// быстрая сортировка точек по расстоянию от центра окружности исходного треугольника
        /// </summary>
        /// <param name="ids">индексы сортируемых точек</param>
        /// <param name="dists">расстояния от центра до сортируемой точки</param>
        /// <param name="left">начальный номер узла сортируемых массивов</param>
        /// <param name="right">конечный номер узла сортируемых массивов</param>
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
        /// <summary>
        /// Поменять местами элементы в массиве (сделать свап, смену)
        /// </summary>
        /// <param name="arr">массив с элементами</param>
        /// <param name="i">индекс 1 элемента</param>
        /// <param name="j">индекс 2 элемента</param>
        private static void Swap(int[] arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        /// <summary>
        /// Квадрат расстояния между точками по указанным индексам
        /// </summary>
        /// <param name="i">индекс 1 точки</param>
        /// <param name="j">индекс 2 точки</param>
        private double Dist(int i, int j)
        {
            var dx = coordsX[i] - coordsX[j];
            var dy = coordsY[i] - coordsY[j];
            return dx * dx + dy * dy;
        }
        /// <summary>
        /// Квадрат расстояния от центра области до точки с указанным индексом
        /// </summary>
        /// <param name="j">индекс точки</param>
        private double Dist(int j)
        {
            var dx = cx - coordsX[j];
            var dy = cy - coordsY[j];
            return dx * dx + dy * dy;
        }
        /// <summary>
        /// Точка принадлежит области
        /// </summary>
        /// <param name="i">индекс точки в массиве</param>
        /// <returns>True - точка принадлежит области</returns>
        private bool InArea(int i, bool withSquare = false)
        {
            //граничные точки по умолчанию входят в триангуляцию
            int offsetPoints = this.Points.Length - this.boundaryContainer.AllBoundaryKnots.Length;
            if (i >= offsetPoints)
                return true;
            //передаем конкретную точку
            return InArea((HPoint)Points[i], withSquare);
        }
        private bool InArea(HPoint point, bool withSquare = false)
        {
            int crossCount = 0;
            //проверка на вхождение точки в описанный квадрат около ограниченной области
            if (withSquare == true)
            {
                foreach (BoundaryHill boundary in boundaryContainer)
                {
                    //если количество вершин меньше 5,
                    //то пропускаем проверку для текущей области
                    if (boundary.Vertexes.Length < 5)
                    {
                        if (crossCount % 2 == 0)
                            crossCount += 1;
                        else
                            crossCount += 2;
                        continue;
                    }
                    //выполняем вхождение точки в описанный квадрат
                    //только для областей как минимум с 5 вершинами
                    for (int i = 0; i < boundary.OutRect.Length; i++)
                    {
                        if (CrossLineUtils.IsCrossing(
                            (HPoint)boundary.OutRect[i],
                            (HPoint)boundary.OutRect[(i + 1) % boundary.OutRect.Length],
                            (HPoint)externalPoint,
                            point))
                            crossCount += 1;
                    }
                }

                //если точка не вошла в описанные квадраты, то далее нет смысла выполнять проверки
                if (crossCount % 2 == 0)
                    return false;

            }

            crossCount = 0;
            //количество пересечений с границей
            //метод - хелпер, помогающий отрисовать невыпуклый контур
            //в цикле подсчитывается количество пересечений с границей области
            foreach (BoundaryHill boundary in boundaryContainer)
                for (int k = 0; k < boundary.Vertexes.Length; k++)
                {
                    if (CrossLineUtils.IsCrossing(
                        (HPoint)boundary.Vertexes[k],
                        (HPoint)boundary.Vertexes[(k + 1) % boundary.Vertexes.Length],
                         (HPoint)externalPoint,
                         point) == true)
                        crossCount += 1;
                }
            return (crossCount % 2 == 1);
        }
        /// <summary>
        /// Принадлежит ли треугольник невыпуклой области <br/>
        /// помечает треугольники, которые принадлежат невыпуклой области <br/>
        /// args: индексы вершин треугольника
        /// </summary>
        /// <returns>True - точка принадлежит области</returns>
        private bool CheckIn(int triangleId)
        {
            //если граница не определена, то помечаем точку, как входящую в сетку
            if (boundaryContainer is null) return true;
            //вершины треугольника
            int i, j, k;
            (i, j, k) = STriangles[triangleId].Get();
            //смещение по количеству неграничных узлов
            int offsetKnots = Points.Length - boundaryContainer.AllBoundaryKnots.Length;
            //включена предварительная фильтрация узлов
            //хотя бы 1 вершина не является граничной
            if (this.usePointFilter)
                if (i < offsetKnots || j < offsetKnots || k < offsetKnots)
                    isIncluded[triangleId] = 1;
            //если ранее "статус" треугольника был определен
            if (isIncluded[triangleId] == 0)
                return false;
            else if (isIncluded[triangleId] == 1)
                return true;
            infectionInitiatorCount++;
            //вычисляем принадлежность треугольника области
            double ctx = (coordsX[i] + coordsX[j] + coordsX[k]) / 3;
            double cty = (coordsY[i] + coordsY[j] + coordsY[k]) / 3;
            HPoint ctri = new HPoint(ctx, cty);
            bool isInArea = InArea(ctri);
            //формируем значение
            int value = 0;
            if (isInArea == true)
                value = 1;
            isIncluded[triangleId] = value;
            //начинаем заражение
            (int, int)[] infectionStack = new (int, int)[this.STriangles.Length];
            //текущий свободный индекс в стеке
            int check = 0;
            for (int currentStackId = 1; currentStackId > 0; currentStackId --)
            {
                if (check == 0) 
                {
                    infectionStack[0] = (triangleId, value);
                    currentStackId = 0; 
                    check = 1; 
                }
                (triangleId, value) = infectionStack[currentStackId];
                int knot = 0;
                for(int vertex = triangleId * 3; vertex < triangleId * 3 + 3; )
                {
                    int halfEdge = HalfEdges[vertex];
                    int adjacentTriangleId = halfEdge / 3;
                    //пропускаем случаи, когда у ребра нет смежного треугольника
                    //или треугольник ранее был обработан
                    if (halfEdge == -1 || isIncluded[adjacentTriangleId] != 2)
                    {
                        vertex++;
                        knot++;
                        continue;
                    }
                    //помещаем текущий треугольник в стэк
                    infectionStack[currentStackId++] = (triangleId, value);
                    //проверка статуса треугольника на основе предыдущего заражения
                    //индексы вершин смежного ребра двух смежных треугольников
                    // первая вершина (предыдущий треугольник)
                    int lastAdjacentVertexId = STriangles[triangleId][knot];
                    // вторая вершина (текущий треугольник)
                    int currentAdjacentId = STriangles[halfEdge/3][(halfEdge + 0) % 3]; 
                    //в качестве текущего треугольника устанавливаем смежный
                    triangleId = adjacentTriangleId;
                    //вершины треугольника
                    (i, j, k) = STriangles[triangleId].Get();
                    //остановка цикла границ
                    bool breakFlag = false; 
                    //проверяем принадлежит ли смежное ребро границе
                    for (int boundId = 0; boundId < boundaryContainer.Count; boundId++)
                    {
                        //одна из вершин ребра не является граничным узлом
                        if (lastAdjacentVertexId < offsetKnots || 
                            currentAdjacentId < offsetKnots)
                            break;
                        //смещение для ТЕКУЩЕЙ ограниченной области в рамках контейнера граничных узлов
                        int offsetBoundary = boundaryContainer.GetBoundaryOffset(boundId);
                        //смещение для СЛЕДУЮЩЕЙ ограниченной области в рамках контейнера граничных узлов
                        int offsetNextBoundary = boundaryContainer.AllBoundaryKnots.Length;
                        if (boundId < boundaryContainer.Count - 1)
                            offsetNextBoundary = boundaryContainer.GetBoundaryOffset(boundId + 1);

                        BoundaryHill boundary = boundaryContainer[boundId];
                        //индексы вершин, между которыми расположены узлы, образующие ограниченную область
                        int[] boundaryKnotsIds = new int[boundary.Vertexes.Length + 1];
                        //заполняем вершинами, которые формируют ограниченную область
                        for (int innerKnotId = 0; innerKnotId < boundary.Vertexes.Length; innerKnotId++)
                        {
                            int globalKnotId = offsetKnots + offsetBoundary + boundary.VertexesIds[innerKnotId];
                            boundaryKnotsIds[innerKnotId] = globalKnotId;
                        }
                        //добавляем в конец точку, которая будет иметь максимальный индекс среди узлов, образующих границу,
                        //но при этом эта точка не является вершиной ограниченной области, хотя и является соседней для такой вершины
                        boundaryKnotsIds[boundaryKnotsIds.Length - 1] = offsetKnots + offsetNextBoundary - 1;
                        //внутренние индексы вершин, образующих смежное ребро, в массиве для текущей ограниченной области
                        //index < 0, если вершины не являются вершинами текущей ограниченной области
                        int innerLastAdjacentVertexId = Array.BinarySearch<int>(boundaryKnotsIds, lastAdjacentVertexId);
                        int innerCurrentAdjacentVertexId = Array.BinarySearch<int>(boundaryKnotsIds, currentAdjacentId);
                        //как минимум одна вершина треугольника является вершиной ограниченной области
                        //2-ая вершина принадлежит ребру, которое образует 1-ая вершина
                        if (IsBelongsBorderVertex(innerLastAdjacentVertexId, innerCurrentAdjacentVertexId, currentAdjacentId, boundaryKnotsIds) ||
                            IsBelongsBorderVertex(innerCurrentAdjacentVertexId, innerLastAdjacentVertexId, lastAdjacentVertexId, boundaryKnotsIds))
                        {
                            value = (int)((value + 1) % 2);
                            break;
                        }
                        int[] adjacentEdgeIds =
                        {
                            lastAdjacentVertexId,
                            currentAdjacentId
                        };
                        for (int knotId = 0; knotId < boundaryKnotsIds.Length; knotId++)
                        {
                            int edgeCounter = 0;
                            //являются ли вершины ребра граничными узлами
                            foreach (int edgeVertex in adjacentEdgeIds)
                                if (boundaryKnotsIds[knotId % boundaryKnotsIds.Length] < edgeVertex &&
                                edgeVertex < boundaryKnotsIds[(knotId + 1) % boundaryKnotsIds.Length])
                                    edgeCounter++;
                            //обе вершины принадлежит одному ребру
                            if (edgeCounter == 2)
                                value = (int)((value + 1) % 2);
                            //хотя бы 1 вершина принадлежит текущему ребру - прерываем цикл
                            if (edgeCounter >= 1)
                            {
                                breakFlag = true;
                                break;
                            }
                        }
                        if (breakFlag == true)
                            break;
                    }
                    //записываем флаг принадлежности треугольника области
                    isIncluded[triangleId] = value;
                    vertex = triangleId * 3;
                    knot = 0;
                }
            }
            return isInArea;
        }
        /// <summary>
        /// Принадлежит ли текущее ребро ограниченной области,
        /// при этом одна вершин ребра также должна быть вершиной ограниченной области
        /// </summary>
        /// <param name="checkedVertex">индекс проверяемой вершины в массиве boundaryKnotsIds. мб меньше 0 </param>
        /// <param name="innerIdOther">индекс второй вершины в массиве вершин ограниченной области</param>
        /// <param name="globalIdOther">глобальный индекс второй вершины относительно <see cref="ids"/></param>
        /// <param name="boundaryKnotsIds">массив вершин, образующих конкретную ограниченную область</param>
        /// <returns>True - ребро принадлежит ограниченной области <br/>
        /// False - ребро не принадлежит ограниченной области или checkedVertex не является вершиной ограниченной области</returns>
        bool IsBelongsBorderVertex(int checkedVertex, int innerIdOther, int globalIdOther, int[] boundaryKnotsIds)
        {
            //проверяемая вершина не является вершиной ограниченной области
            if (checkedVertex < 0)
                return false;
            //количество вершин ограниченной области
            int len = boundaryKnotsIds.Length;
            //обе вершины являются вершинами ограниченной области
            if (innerIdOther >= 0)
                //вершины образуют одну и ту же грань ограниченной области
                if (Math.Abs(checkedVertex - innerIdOther) == 1 || 
                    Math.Abs(checkedVertex - innerIdOther) == len - 1)
                    return true;
            // принадлежит ли 2-ая вершина ребру ограниченной области,
            // вершиной которого является проверяемая вершина
            // т.к. проверяемая вершина образует 2 ребра, проверяем каждое по очереди
            if (boundaryKnotsIds[Math.Abs(checkedVertex + len - 1) % len] < globalIdOther &&
                    globalIdOther < boundaryKnotsIds[checkedVertex])
                    return true;
            else 
                if (boundaryKnotsIds[checkedVertex] < globalIdOther &&
                    globalIdOther < boundaryKnotsIds[(checkedVertex + len + 1) % len])
                    return true;
            return false;
        }

        #endregion CreationLogic
    }
}