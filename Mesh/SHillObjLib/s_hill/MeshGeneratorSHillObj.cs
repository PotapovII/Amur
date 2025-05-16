namespace SHillObjLib
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using CommonLib.Geometry;
    /// <summary>
    /// Меш генератор по s-hell алгоритму 
    /// </summary>
    public class MeshGeneratorSHillObj
    {
        private HPoint[] points;

        public double fraction = 0.3f;

        public MeshGeneratorSHillObj(){}

        private void Analyse(HPoint[] suppliedPoints, 
                        Hull hull, List<Triad> triads, 
                        bool rejectDuplicatePoints, bool hullOnly)
        {
            if (suppliedPoints.Length < 3)
                throw new ArgumentException("Количество предоставленных точек должно быть >= 3");

            this.points = suppliedPoints;
            int nump = points.Length;

            double[] distance2ToCentre = new double[nump];
            int[] sortedIndices = new int[nump];

            // Выберите первую точку в качестве начальной
            for (int k = 0; k < nump; k++)
            {
                distance2ToCentre[k] = points[0].Length2(points[k]);
                sortedIndices[k] = k;
            }

            // Сортировка по расстоянию до начальной точки
            Array.Sort(distance2ToCentre, sortedIndices);

            // Теперь, когда мы отсортировали вершины,
            // дубликаты отбраковываются более эффективно
            if (rejectDuplicatePoints)
            {
                // Выполняйте поиск в обратном направлении,
                // чтобы каждое удаление было независимым от
                // любого другого
                for (int k = nump - 2; k >= 0; k--)
                {
                    // Если точки идентичны, то расстояния между ними будут нулевое,
                    // таким образом, в отсортированном списке они будут соседними
                    if (HPoint.Equals(points[sortedIndices[k]],points[sortedIndices[k + 1]].x)== true)
                    {
                        // Ожидается, что дубликаты будут редкими, так что это не особенно эффективно
                        Array.Copy(sortedIndices, k + 2, sortedIndices, k + 1, nump - k - 2);
                        Array.Copy(distance2ToCentre, k + 2, distance2ToCentre, k + 1, nump - k - 2);
                        nump--;
                    }
                }
            }

            Debug.WriteLine((points.Length - nump).ToString() + " повторяющиеся точки удалены");

            if (nump < 3)
                throw new ArgumentException("Количество предоставленных уникальных точек должно быть >= 3");

            #region Подготовка к s-hill триангуляции
            int mid = -1;
            double romin2 = double.MaxValue, circumCentreX = 0, circumCentreY = 0;

            // Найдите точку, которая вместе с первыми двумя точками
            // образует треугольник с наименьшей окружностью
            Triad tri = new Triad(sortedIndices[0], sortedIndices[1], 2);
            for (int kc = 2; kc < nump; kc++)
            {
                tri.c = sortedIndices[kc];
                if (tri.FindCircumcirclePrecisely(points) && tri.circumcircleR2 < romin2)
                {
                    mid = kc;
                    // Центр окружности первого треугольника
                    romin2 = tri.circumcircleR2;
                    circumCentreX = tri.circumcircleX;
                    circumCentreY = tri.circumcircleY;
                }
                else if (romin2 * 4 < distance2ToCentre[kc])
                    break;
            }

            // При необходимости измените индексы, чтобы 2 - я
            // точка образовывала наименьшую окружность с 0 - й и 1 - й
            if (mid != 2)
            {
                int indexMid = sortedIndices[mid];
                double distance2Mid = distance2ToCentre[mid];

                Array.Copy(sortedIndices, 2, sortedIndices, 3, mid - 2);
                Array.Copy(distance2ToCentre, 2, distance2ToCentre, 3, mid - 2);
                sortedIndices[2] = indexMid;
                distance2ToCentre[2] = distance2Mid;
            }

            // Эти три точки и есть наш начальный треугольник
            tri.c = sortedIndices[2];
            tri.MakeClockwise(points);
            tri.FindCircumcirclePrecisely(points);

            // Добавьте треугольник в качестве первой триады и три точки
            // к выпуклой оболочке
            triads.Add(tri);
            hull.Add(new HullVertex(points, tri.a));
            hull.Add(new HullVertex(points, tri.b));
            hull.Add(new HullVertex(points, tri.c));

            // Отсортируйте остальные точки в соответствии с их расстоянием от центра тяжести
            // Повторно измерьте расстояния точек от центра описанной окружности
            HPoint centre = new HPoint(circumCentreX, circumCentreY);
            for (int k = 3; k < nump; k++)
                distance2ToCentre[k] = points[sortedIndices[k]].Length2(centre);
            // Отсортируйте _другие_ точки в порядке расстояния до центра окружности
            Array.Sort(distance2ToCentre, sortedIndices, 3, nump - 3);
            #endregion 

            // Добавляем новые точки в в оболочку (удаляя из цепочки скрытые)
            // и создаем треугольники....
            int numt = 0;
            #region Собственно триангуляция
            for (int k = 3; k < nump; k++)
            {
                int pointsIndex = sortedIndices[k];

                HullVertex ptx = new HullVertex(points, pointsIndex);
                // направлен наружу от корпуса [0] к северо-востоку.
                double dx = ptx.x - hull[0].x;
                double dy = ptx.y - hull[0].y;

                int numh = hull.Count;
                int numh_old = numh;

                List<int> pidx = new List<int>(), tridx = new List<int>();
                // новое местоположение точки оболочки внутри оболочки .....
                int hidx;  

                if (hull.EdgeVisibleFrom(0, dx, dy) == true)
                {
                    // начиная с видимой грани оболочки !!!
                    int e2 = numh;
                    hidx = 0;
                    // проверьте, виден ли также сегмент numh
                    if (hull.EdgeVisibleFrom(numh - 1, dx, dy) == true)
                    {
                        // видимый.
                        pidx.Add(hull[numh - 1].pointsIndex);
                        tridx.Add(hull[numh - 1].triadIndex);
                        for (int h = 0; h < numh - 1; h++)
                        {
                            // если сегмент h виден, удалите его
                            pidx.Add(hull[h].pointsIndex);
                            tridx.Add(hull[h].triadIndex);
                            if (hull.EdgeVisibleFrom(h, ptx) == true)
                            {
                                hull.RemoveAt(h);
                                h--;
                                numh--;
                            }
                            else
                            {
                                // выйти из невидимости
                                hull.Insert(0, ptx);
                                numh++;
                                break;
                            }
                        }
                        
                        // посмотрите назад, сквозь конструкцию оболочки
                        for (int h = numh - 2; h > 0; h--)
                        {
                            // если виден сегмент h, удалите h + 1
                            if (hull.EdgeVisibleFrom(h, ptx) == true)
                            {
                                pidx.Insert(0, hull[h].pointsIndex);
                                tridx.Insert(0, hull[h].triadIndex);
                                // стереть конец цепочки
                                hull.RemoveAt(h + 1);  
                            }
                            else
                                break; // выйти из режима невидимости
                        }
                    }
                    else
                    {
                        hidx = 1;  // keep pt hull[0]
                                   // сохранить pt в оболочку [0]
                        tridx.Add(hull[0].triadIndex);
                        pidx.Add(hull[0].pointsIndex);

                        for (int h = 1; h < numh; h++)
                        {
                            // if segment h is visible delete h  
                            // если сегмент h виден, удалите его
                            pidx.Add(hull[h].pointsIndex);
                            tridx.Add(hull[h].triadIndex);
                            if (hull.EdgeVisibleFrom(h, ptx) == true)
                            {                     // видимые
                                hull.RemoveAt(h);
                                h--;
                                numh--;
                            }
                            else
                            {
                                // quit on invisibility
                                // выйти из режима невидимости
                                hull.Insert(h, ptx);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    int e1 = -1, e2 = numh;
                    for (int h = 1; h < numh; h++)
                    {
                        if (hull.EdgeVisibleFrom(h, ptx) == true)
                        {
                            if (e1 < 0)
                                e1 = h;  // first visible
                                         // первый видимый
                        }
                        else
                        {
                            if (e1 > 0)
                            {
                                // first invisible segment.
                                // первый невидимый сегмент.
                                e2 = h;
                                break;
                            }
                        }
                    }

                    // triangle pidx starts at e1 and ends at e2 (inclusive).	
                    // треугольник pidx начинается с e1 и заканчивается на e2 (включительно).
                    if (e2 < numh)
                    {
                        for (int e = e1; e <= e2; e++)
                        {
                            pidx.Add(hull[e].pointsIndex);
                            tridx.Add(hull[e].triadIndex);
                        }
                    }
                    else
                    {
                        for (int e = e1; e < e2; e++)
                        {
                            pidx.Add(hull[e].pointsIndex);
                            tridx.Add(hull[e].triadIndex);   // there are only n-1 triangles from n hull pts.
                            // есть только n-1 треугольников из n элементов оболочки.
                        }
                        pidx.Add(hull[0].pointsIndex);
                    }

                    // erase elements e1+1 : e2-1 inclusive.
                    // стереть элементы e1+1 : e2-1 включительно.
                    if (e1 < e2 - 1)
                        hull.RemoveRange(e1 + 1, e2 - e1 - 1);

                    // insert ptx at location e1+1.
                    // вставьте ptx в положение e1+1.
                    hull.Insert(e1 + 1, ptx);
                    hidx = e1 + 1;
                }

                // Если мы вычисляем только оболочку, то на этом мы закончили
                if (hullOnly)
                    continue;

                int a = pointsIndex;
                int npx = pidx.Count - 1;
                numt = triads.Count;
                int triadCounts = numt;

                for (int p = 0; p < npx; p++)
                {
                    Triad trx = new Triad(a, pidx[p], pidx[p + 1]);
                    trx.FindCircumcirclePrecisely(points);

                    trx.bc = tridx[p];
                    if (p > 0)
                        trx.ab = numt - 1;
                    trx.ac = numt + 1;

                    // проиндексируйте обратно в триады.
                    Triad txx = triads[tridx[p]];
                    if ((trx.b == txx.a && trx.c == txx.b) | 
                        (trx.b == txx.b && trx.c == txx.a))
                        txx.ab = numt;
                    else if ((trx.b == txx.a && trx.c == txx.c) | 
                             (trx.b == txx.c && trx.c == txx.a))
                                txx.ac = numt;
                    else if ((trx.b == txx.b && trx.c == txx.c) | 
                             (trx.b == txx.c && trx.c == txx.b))
                                txx.bc = numt;

                    triads.Add(trx);
                    numt++;
                }
                
                // Последний край находится снаружи
                triads[numt - 1].ac = -1;

                hull[hidx].triadIndex = numt - 1;
                if (hidx > 0)
                    hull[hidx - 1].triadIndex = triadCounts;
                else
                {
                    numh = hull.Count;
                    hull[numh - 1].triadIndex = triadCounts;
                }
            }
            #endregion
        }
        /// <summary>
        /// Верните выпуклую оболочку предоставленных точек,
        /// При необходимости проверьте, нет ли повторяющихся точек
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <param name="rejectDuplicatePoints">Следует ли опускать дублирующиеся пункты</param>
        /// <returns></returns>
        public List<HPoint> ConvexHull(HPoint[] points, bool rejectDuplicatePoints)
        {
            Hull hull = new Hull();
            List<Triad> triads = new List<Triad>();

            Analyse(points, hull, triads, rejectDuplicatePoints, true);

            List<HPoint> hullVertices = new List<HPoint>();
            foreach (HullVertex hv in hull)
                hullVertices.Add(new HPoint(hv.x, hv.y));

            return hullVertices;
        }
        /// <summary>
        /// Верните триангуляцию Делоне для указанных точек
        /// Не проверяйте наличие повторяющихся точек
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <returns>Триады, задающие триангуляцию</returns>
        public List<Triad> Triangulation(HPoint[] points, ref Hull hull)
        {
            return Triangulation(points, false, ref hull);
        }

        /// <summary>
        /// Возвращает триангуляцию Делоне для указанных точек
        /// При необходимости проверьте, нет ли повторяющихся точек
        /// 
        /// Return the Delaunay triangulation of the supplied points
        /// Optionally check for duplicate points
        /// </summary>
        /// <param name="points">List of 2D vertices</param>
        /// <param name="rejectDuplicatePoints">Whether to omit duplicated points</param>
        /// <returns></returns>
        public List<Triad> Triangulation(HPoint[] points, bool rejectDuplicatePoints, ref Hull hull)
        {
            List<Triad> triads = new List<Triad>();
            hull = new Hull();
            Analyse(points, hull, triads, rejectDuplicatePoints, false);
            // Теперь нужно перевернуть все пары соседних треугольников,
            // которые не удовлетворяют критерию Делоне
            int numt = triads.Count;
            bool[] idsA = new bool[numt];
            bool[] idsB = new bool[numt];
            // Мы сохраняем "список" треугольников, которые мы перевернули, чтобы отслеживать
            // любые последующие изменения
            // Когда количество изменений велико, его лучше всего сохранять в виде вектора значений bools

            // Когда число становится небольшим, его лучше поддерживать в виде набора
            // Мы переключаемся между этими режимами по мере уменьшения числа переключений
            // ограничение цикла итераций включено, чтобы предотвратить "колебание" вырожденных случаев
            // и невозможность остановки алгоритма.
            int flipped = FlipTriangles(triads, idsA);
            int iterations = 1;
            while (flipped > (int)(fraction * (double)numt) && iterations<1000)
            {
                //int flag = iterations & 1;
                int flag = iterations % 2;
                if (flag == 1)
                    flipped = FlipTriangles(triads, idsA, idsB);
                else
                    flipped = FlipTriangles(triads, idsB, idsA);
                iterations++;
            }

            Set<int> idSetA = new Set<int>();
            Set<int> idSetB = new Set<int>();
            //int flag = iterations & 1;
            int flagIter = iterations % 2;
            bool[] idsAB = (flagIter == 1) ? idsA : idsB;
            //flipped = FlipTriangles(triads, (flagIter == 1) ? idsA : idsB, idSetA);
            flipped = FlipTriangles(triads, idsAB, idSetA);
            iterations = 1;
            while (flipped > 0 && iterations < 2000)
            {
                //int flag = iterations & 1;
                int flag = iterations % 2;
                if (flag == 1)
                    flipped = FlipTriangles(triads, idSetA, idSetB);
                else
                    flipped = FlipTriangles(triads, idSetB, idSetA);
                iterations++;
            }
            return triads;
        }



        /// <summary>
        /// Сравните триаду с ее тремя соседями и поменяйте местами 
        /// с любым соседом, чья противоположная точка находится внутри 
        /// окружности триады
        /// </summary>
        /// <param name="triads">Триады</param>
        /// <param name="triadIndexToTest">Индекс тестируемой триады</param>
        /// <param name="triadIndexFlipped">Индекс соседнего треугольника, 
        /// с помощью которого он был перевернут (если таковой имеется)</param>
        /// <returns>это верно, если триада поменялась местами с кем-либо из ее соседей</returns>
        private bool FlipTriangle(List<Triad> triads, int triadIndexToTest, out int triadIndexFlipped)
        {
            int oppositeVertex = 0, edge1, edge2, edge3 = 0, edge4 = 0;
            triadIndexFlipped = 0;

            Triad triA = triads[triadIndexToTest];
            // протестируйте все 3 соседа triA
            // грань bc
            if (triA.bc >= 0)
            {
                triadIndexFlipped = triA.bc;
                Triad triB = triads[triadIndexFlipped];
                // найдите общую грань у соседа
                triB.FindAdjacency(triA.b, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (triA.InsideCircumcircle(points[oppositeVertex]) == true)
                {
                    // не выполнено условие Делоне.
                    edge1 = triA.ab;
                    edge2 = triA.ac;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = triA.a, 
                            trib = triA.b, 
                            tric = triA.c;
                        triA.Initialize(tria, trib, oppositeVertex, 
                                        edge1, edge3, triadIndexFlipped, points);
                        triB.Initialize(tria, tric, oppositeVertex, 
                                        edge2, edge4, triadIndexToTest, points);

                        // измените рисунок на треугольных надписях.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }
            // грань ab
            if (triA.ab >= 0)
            {
                triadIndexFlipped = triA.ab;
                Triad triB = triads[triadIndexFlipped];
                // найдите общую грань у соседа
                triB.FindAdjacency(triA.a, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (triA.InsideCircumcircle(points[oppositeVertex]))
                {
                    // не выполнено условие Делоне.
                    edge1 = triA.ac;
                    edge2 = triA.bc;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = triA.a, trib = triA.b, tric = triA.c;
                        triA.Initialize(tric, tria, oppositeVertex, 
                            edge1, edge3, triadIndexFlipped, points);
                        triB.Initialize(tric, trib, oppositeVertex, 
                            edge2, edge4, triadIndexToTest, points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }
            // грань ac
            if (triA.ac >= 0)
            {
                triadIndexFlipped = triA.ac;
                Triad triB = triads[triadIndexFlipped];
                // найдите общую грань у соседа
                triB.FindAdjacency(triA.a, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
                if (triA.InsideCircumcircle(points[oppositeVertex]))
                {
                    // не выполнено условие Делоне.
                    edge1 = triA.ab;   
                    edge2 = triA.bc;
                    if (edge1 != edge3 && edge2 != edge4)
                    {
                        int tria = triA.a, trib = triA.b, tric = triA.c;
                        triA.Initialize(trib, tria, oppositeVertex, 
                            edge1, edge3, triadIndexFlipped, points);
                        triB.Initialize(trib, tric, oppositeVertex, 
                            edge2, edge4, triadIndexToTest, points);

                        // change knock on triangle labels.
                        if (edge3 >= 0)
                            triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
                        if (edge2 >= 0)
                            triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Переверните треугольники, которые не удовлетворяют условию Делоне
        /// </summary>
        private int FlipTriangles(List<Triad> triads, bool[] idsFlipped)
        {
            int numt = (int)triads.Count;
            Array.Clear(idsFlipped, 0, numt);

            int flipped = 0;
            for (int t = 0; t < numt; t++)
            {
                int t2;
                if (FlipTriangle(triads, t, out t2) == true)
                {
                    flipped += 2;
                    idsFlipped[t] = true;
                    idsFlipped[t2] = true;

                }
            }

            return flipped;
        }
        /// <summary>
        /// Переверните треугольники, которые не удовлетворяют условию Делоне
        /// </summary>
        private int FlipTriangles(List<Triad> triads, bool[] idsToTest, bool[] idsFlipped)
        {
            int numt = (int)triads.Count;
            Array.Clear(idsFlipped, 0, numt);

            int flipped = 0;
            for (int t = 0; t < numt; t++)
            {
                if (idsToTest[t] == true)
                {
                    int t2;
                    if (FlipTriangle(triads, t, out t2) == true)
                    {
                        flipped += 2;
                        idsFlipped[t] = true;
                        idsFlipped[t2] = true;
                    }
                }
            }

            return flipped;
        }
        /// <summary>
        /// Переверните треугольники, которые не удовлетворяют условию Делоне
        /// </summary>
        private int FlipTriangles(List<Triad> triads, bool[] idsToTest, Set<int> idsFlipped)
        {
            int numt = (int)triads.Count;
            idsFlipped.Clear();

            int flipped = 0;
            for (int t = 0; t < numt; t++)
            {
                if (idsToTest[t] == true)
                {
                    int t2;
                    if (FlipTriangle(triads, t, out t2) == true)
                    {
                        flipped += 2;
                        idsFlipped.Add(t);
                        idsFlipped.Add(t2);
                    }
                }
            }

            return flipped;
        }
        /// <summary>
        /// Переверните треугольники, которые не удовлетворяют условию Делоне
        /// </summary>
        private int FlipTriangles(List<Triad> triads, Set<int> idsToTest, Set<int> idsFlipped)
        {
            int flipped = 0;
            idsFlipped.Clear();

            foreach (int t in idsToTest)
            {
                int t2;
                if (FlipTriangle(triads, t, out t2) == true)
                {
                    flipped += 2;
                    idsFlipped.Add(t);
                    idsFlipped.Add(t2);
                }
            }

            return flipped;
        }

//        #region Debug verification routines: verify that triad adjacency and indeces are set correctly
//#if DEBUG
//        private void VerifyHullContains(Hull hull, int idA, int idB)
//        {
//            if (
//                ((hull[0].pointsIndex == idA) && (hull[hull.Count - 1].pointsIndex == idB)) ||
//                ((hull[0].pointsIndex == idB) && (hull[hull.Count - 1].pointsIndex == idA)))
//                return;

//            for (int h = 0; h < hull.Count - 1; h++)
//            {
//                if (hull[h].pointsIndex == idA)
//                {
//                    Debug.Assert(hull[h + 1].pointsIndex == idB);
//                    return;
//                }
//                else if (hull[h].pointsIndex == idB)
//                {
//                    Debug.Assert(hull[h + 1].pointsIndex == idA);
//                    return;
//                }
//            }

//        }

//        private void VerifyTriadContains(Triad tri, int nbourTriad, int idA, int idB)
//        {
//            if (tri.ab == nbourTriad)
//            {
//                Debug.Assert(
//                    ((tri.a == idA) && (tri.b == idB)) ||
//                    ((tri.b == idA) && (tri.a == idB)));
//            }
//            else if (tri.ac == nbourTriad)
//            {
//                Debug.Assert(
//                    ((tri.a == idA) && (tri.c == idB)) ||
//                    ((tri.c == idA) && (tri.a == idB)));
//            }
//            else if (tri.bc == nbourTriad)
//            {
//                Debug.Assert(
//                    ((tri.c == idA) && (tri.b == idB)) ||
//                    ((tri.b == idA) && (tri.c == idB)));
//            }
//            else
//                Debug.Assert(false);
//        }

//        private void VerifyTriads(List<Triad> triads, Hull hull)
//        {
//            for (int t = 0; t < triads.Count; t++)
//            {
//                if (t == 17840)
//                    t = t + 0;

//                Triad tri = triads[t];
//                if (tri.ac == -1)
//                    VerifyHullContains(hull, tri.a, tri.c);
//                else
//                    VerifyTriadContains(triads[tri.ac], t, tri.a, tri.c);

//                if (tri.ab == -1)
//                    VerifyHullContains(hull, tri.a, tri.b);
//                else
//                    VerifyTriadContains(triads[tri.ab], t, tri.a, tri.b);

//                if (tri.bc == -1)
//                    VerifyHullContains(hull, tri.b, tri.c);
//                else
//                    VerifyTriadContains(triads[tri.bc], t, tri.b, tri.c);

//            }
//        }

//        private void WriteTriangles(List<Triad> triangles, string name)
//        {
//            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(name + ".dtt"))
//            {
//                writer.WriteLine(triangles.Count.ToString());
//                for (int i = 0; i < triangles.Count; i++)
//                {
//                    Triad t = triangles[i];
//                    writer.WriteLine(string.Format("{0}: {1} {2} {3} - {4} {5} {6}",
//                        i + 1,
//                        t.a, t.b, t.c,
//                        t.ab + 1, t.bc + 1, t.ac + 1));
//                }
//            }
//        }

//#endif

//        #endregion

    }

}
