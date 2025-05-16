//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?V"
//                         проектировщик:
//                           Потапов И.И. 
//                кодировка : 6.10.2000 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "MixSolver"
//            правка  :   06.12.2006 Потапов И.И. (c++=> c#)
//---------------------------------------------------------------------------
//                       ПРОЕКТ  "RiverLib"
//                рефакторинг : 25.12.2020 Потапов И.И. 
//---------------------------------------------------------------------------
//                           Потапов И.И.
//          рефакторинг (обобщение) : 03.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;

    using MeshLib;
    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public class HStripMeshGeneratorTri : AStripMeshGenerator
    {
        /// <summary>
        /// Создаваемая сетка
        /// </summary>
        TriMesh mesh = null;
        /// <summary>
        /// КЭ сетки
        /// </summary>
        TriElement[] AreaElems;
        /// <summary>
        /// граничные КЭ сетки
        /// </summary>
        TwoElement[] BoundElems;
        /// <summary>
        /// Тип граничных условий для граничного элемента
        /// </summary>
        public int[] BoundElementsMark;
        /// <summary>
        /// координаты узлов сетки
        /// </summary>
        double[] CoordsX;
        double[] CoordsY;
        /// <summary>
        /// граничные узлы сетки
        /// </summary>
        int[] BoundKnots;
        /// <summary>
        /// флаг граничных узлов сетки
        /// </summary>
        int[] BoundKnotsMark;
        /// <summary>
        /// количество КЭ сетки
        /// </summary>
        int CountElems = 0;
        /// <summary>
        /// количество узлов сетки
        /// </summary>
        int CountKnots = 0;
        /// <summary>
        /// количество граничных узлов сетки
        /// </summary>
        int CountBoundKnots = 0;
        /// <summary>
        /// количество граничных КЭ сетки
        /// </summary>
        int BoundElementsCount = 0;
        double dx = 0;
        int bottom = 0;
        int vertRight = 1;
        int waterLevel = 2;
        int vertLeft = 3;

        public HStripMeshGeneratorTri():this(new CrossStripMeshOption()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="MAXElem">максимальное количество КЭ сетки</param>
        /// <param name="MAXKnot">максимальное количество узлов сетки</param>
        public HStripMeshGeneratorTri(CrossStripMeshOption Option, int MAXElem = 1000000, int MAXKnot = 1000000)
            : base(Option)
        {
            AreaElems = new TriElement[MAXElem];
            BoundElems = new TwoElement[MAXElem];
            BoundElementsMark = new int[MAXElem];
            CoordsX = new double[MAXKnot];
            CoordsY = new double[MAXKnot];
            BoundKnots = new int[MAXKnot];
            BoundKnotsMark = new int[MAXKnot];
        }
        public override IMesh CreateMesh(ref double GR, ref int[][] riverGates, double WaterLevel, double[] xx, double[] yy, int Count = 0)
        {
            return CreateTriMesh(ref GR, ref riverGates, WaterLevel, xx, yy, Count);
        }
        /// <summary>
        /// Создает сетку в области
        /// </summary>
        /// <param name="WetBed">смоченный периметр</param>
        /// <param name="WaterLevel">уровень свободной поверхности</param>
        /// <param name="xx">координаты дна по Х</param>
        /// <param name="yy">координаты дна по Y</param>
        /// <param name="Count">Количество узлов по дну</param>
        /// <returns></returns>
        public TriMesh CreateTriMesh(ref double WetBed, ref int[][] riverGates, double WaterLevel, double[] xx, double[] yy, int Count = 0)
        {
            try
            {
                if (xx == null || yy == null)
                {
                    throw new Exception("Геометрия задачи не загружена");
                }
                CountElems = 0;
                CountKnots = 0;
                CountBoundKnots = 0;
                BoundElementsCount = 0;

                CalkBedFunction(ref WetBed, WaterLevel, xx, yy);

                this.mesh = new TriMesh();
                if (Count == 0)
                    Count = CountBed;
                // шаг сетки по створу
                dx = width / (Count);
                // создание первого элемента сетки 1 левый треугольник
                // и добавление его в меsh
                double x, y;
                // lt   rt
                // *----*
                //  \   |   
                //    \ |
                //      * rb
                //
                HNumbKnot lb, lt, rb, rt;
                lb = new HNumbKnot(left.x, left.y, bottom, CountKnots++);
                lt = lb;
                BoundKnots[CountBoundKnots] = lb.ID;
                BoundKnotsMark[CountBoundKnots++] = lb.marker;

                x = left.x + dx;
                y = spline.Value(x);
                rb = new HNumbKnot(x, y, bottom, CountKnots++);
                BoundKnots[CountBoundKnots] = rb.ID;
                BoundKnotsMark[CountBoundKnots++] = rb.marker;

                y = left.y;
                rt = new HNumbKnot(x, y, waterLevel, CountKnots++);
                BoundKnots[CountBoundKnots] = rt.ID;
                BoundKnotsMark[CountBoundKnots++] = rt.marker;

                AreaElems[CountElems].Vertex1 = (uint)lb.ID;
                AreaElems[CountElems].Vertex2 = (uint)rb.ID;
                AreaElems[CountElems].Vertex3 = (uint)rt.ID;
                CoordsX[lb.ID] = lb.x;
                CoordsY[lb.ID] = lb.y;
                CoordsX[rt.ID] = rt.x;
                CoordsY[rt.ID] = rt.y;
                CoordsX[rb.ID] = rb.x;
                CoordsY[rb.ID] = rb.y;
                ++CountElems;

                BoundElementsMark[BoundElementsCount] = waterLevel;
                BoundElems[BoundElementsCount].Vertex1 = (uint)rt.ID;
                BoundElems[BoundElementsCount++].Vertex2 = (uint)lt.ID;

                BoundElementsMark[BoundElementsCount] = bottom;
                BoundElems[BoundElementsCount].Vertex1 = (uint)lb.ID;
                BoundElems[BoundElementsCount++].Vertex2 = (uint)rb.ID;
                // создание левой границы сетки после 1 КЭ
                HNumbKnot[] knotsTmp = new HNumbKnot[2];
                knotsTmp[0] = new HNumbKnot(rb);
                knotsTmp[1] = new HNumbKnot(rt);
                riverGates = new int[Count][];
                riverGates[0] = new int[1];
                riverGates[0][0] = lb.ID;
                // герерация сетки в затопленной части
                int ii = 1;
                try
                {
                    for (ii = 1; ii < Count; ii++)
                    {
                        riverGates[ii] = new int[knotsTmp.Length];
                        for (int j = 0; j < knotsTmp.Length; j++)
                            riverGates[ii][j] = (int)knotsTmp[j].ID;
                        knotsTmp = DevideStripe(knotsTmp, ii, Count);
                    }
                }
                catch(Exception eez) 
                {
                    Logger.Instance.Exception(eez);
                    Logger.Instance.Info("ОШИБКА: На "+ii.ToString()+" шаге генерации сетки");
                }
                // замыкание
                if(dryRight != true)
                {
                    // формирование граничных узлов на затопленной правой границе 
                    for (int i = 0; i < knotsTmp.Length - 1; i++)
                    {
                        if (i != knotsTmp.Length - 2)
                        {
                            BoundKnots[CountBoundKnots] = knotsTmp[i + 1].ID;
                            if(Option.AxisOfSymmetry == true)
                                BoundKnotsMark[CountBoundKnots] = vertRight;
                            else
                                BoundKnotsMark[CountBoundKnots] = bottom;
                            CountBoundKnots++;
                        }
                        if (Option.AxisOfSymmetry == true)
                            BoundElementsMark[BoundElementsCount] = vertRight;
                        else
                            BoundKnotsMark[CountBoundKnots] = bottom;
                        BoundElems[BoundElementsCount].Vertex1 = (uint)knotsTmp[i + 1].ID;
                        BoundElems[BoundElementsCount].Vertex2 = (uint)knotsTmp[i].ID;
                        BoundElementsCount++;
                    }
                }

                #region Запись результирующей сетки
                mesh.AreaElems = new TriElement[CountElems];
                for (int i = 0; i < CountElems; i++)
                    mesh.AreaElems[i] = AreaElems[i];

                mesh.CoordsX = new double[CountKnots];
                mesh.CoordsY = new double[CountKnots];
                for (int i = 0; i < CountKnots; i++)
                {
                    mesh.CoordsX[i] = CoordsX[i];
                    mesh.CoordsY[i] = CoordsY[i];
                }

                mesh.BoundKnots = new int[CountBoundKnots];
                mesh.BoundKnotsMark = new int[CountBoundKnots];
                for (int i = 0; i < CountBoundKnots; i++)
                {
                    mesh.BoundKnotsMark[i] = BoundKnotsMark[i];
                    mesh.BoundKnots[i] = BoundKnots[i];
                }

                mesh.BoundElems = new TwoElement[BoundElementsCount];
                mesh.BoundElementsMark = new int[BoundElementsCount];
                for (int i = 0; i < BoundElementsCount; i++)
                {
                    mesh.BoundElems[i] = BoundElems[i];
                    mesh.BoundElementsMark[i] = BoundElementsMark[i];
                }
                #endregion
            }
            catch (Exception exx)
            {
                Logger.Instance.Exception(exx);
                Logger.Instance.Info("ОШИБКА: Проверте согласованность данных по геометрии створа");
            }
            return mesh;
        }
        /// <summary>
        /// делает разбиение сетки на одном шаге по Х фронта
        /// </summary>
        /// <param name="leftKnots">узлы левой стороны</param>
        /// <returns></returns>
        private HNumbKnot[] DevideStripe(HNumbKnot[] leftKnots, int idx, int Count)
        {
            // узлы правой стороны
            HNumbKnot[] rightKnots;
            double x, y;
            double YB, WL;
            // находим уровень свободной поверхности правой стороны
            WL = left.y;
            // ищем координату левой стороны (она не согласована с сеткой дна!!!)
            x = leftKnots[0].x + dx;
            // если координата вышла за ость симметирии ставим ее на координату ости
            //if (x > width)
            //    x = width;
            // вычисляем уровень дна
            YB = spline.Value(x);
            // строим точку дна правой стороны
            HPoint botom = new HPoint(x, YB);
            // строим свободной поверхности правой стороны
            HPoint top = new HPoint(x, WL);
            // находим глубину правой стороны
            double dd = WL - YB;
            // находим предварительное значение количества
            // узлов на правой стороне
            uint n = Convert.ToUInt16(Math.Abs(dd / dx));
            // находим интервал глубины на правой стороне
            double realD = dd / (n + 1);
            // находим окончательное значение количества
            // узлов на правой стороне 
            int step = 0; // step - количество внутренних узлов грани
            if (n > (leftKnots.Length - 2))
                step = leftKnots.Length;
            else
                if (n == (leftKnots.Length - 2))
                step = leftKnots.Length - 1;
            else
                    if (n < (leftKnots.Length - 2))
                step = leftKnots.Length - 2;
            if (dryRight == true)
            {
                if (idx == Count - 2)
                    step = 1;
                if (idx == Count - 1)
                    step = 0;
            }
            // создаем массив узлов правой стороны
            rightKnots = new HNumbKnot[step + 1];
            // создаем первый донный узем правой стороны
            rightKnots[0] = new HNumbKnot(botom.x, botom.y, bottom, CountKnots);
            // увеличиваем счетчик узлов
            CountKnots++;
            // запоминаем созданный донный узел в массиве граничных узлов
            BoundKnots[CountBoundKnots] = rightKnots[0].ID;
            // устанавливаем флаг данного узла
            BoundKnotsMark[CountBoundKnots++] = rightKnots[0].marker;

            // создаем граничный элемент 
            BoundElementsMark[BoundElementsCount] = bottom;
            BoundElems[BoundElementsCount].Vertex1 = (uint)leftKnots[0].ID;
            BoundElems[BoundElementsCount++].Vertex2 = (uint)rightKnots[0].ID;
            // создаем узлы правой стороны
            int k = 1; // k - счетчик узлов на правой стороне
            y = YB;
            for (; k < step; k++)
            {
                y += realD;
                rightKnots[k] = new HNumbKnot(x, y, 0, CountKnots++);
            }
            if (step != 0)
            {
                rightKnots[k] = new HNumbKnot(top.x, top.y, waterLevel, CountKnots++);
                // создаем граничный узел и флаг на свободной поверхности
                BoundKnots[CountBoundKnots] = rightKnots[k].ID;
                BoundKnotsMark[CountBoundKnots++] = rightKnots[k].marker;
                // создаем ГЭ на свободной поверхности
                BoundElementsMark[BoundElementsCount] = waterLevel;
                BoundElems[BoundElementsCount].Vertex1 = (uint)rightKnots[k].ID;
                BoundElems[BoundElementsCount++].Vertex2 = (uint)leftKnots[leftKnots.Length - 1].ID;
            }
            else  // правый угол
            {
                // создаем ГЭ на свободной поверхности
                BoundElementsMark[BoundElementsCount] = waterLevel;
                BoundElems[BoundElementsCount].Vertex1 = (uint)rightKnots[0].ID;
                BoundElems[BoundElementsCount++].Vertex2 = (uint)leftKnots[leftKnots.Length - 1].ID;
            }
            // Строим матрицу обхода КЭ - в и вычисляем координаты узлов 
            HNumbKnot A, B, C, D;
            if (rightKnots.Length == leftKnots.Length)
            {
                // Правый берег
                for (int i = 0; i < rightKnots.Length - 1; i++)
                {
                    // узлы КЭ - в против часовой стрелки
                    //  B----C
                    //  | \1 |
                    //  |2 \ | 
                    //  A----D
                    A = leftKnots[i];
                    B = leftKnots[i + 1];
                    C = rightKnots[i + 1];
                    D = rightKnots[i];

                    CoordsX[A.ID] = A.x;
                    CoordsY[A.ID] = A.y;
                    CoordsX[B.ID] = B.x;
                    CoordsY[B.ID] = B.y;
                    CoordsX[C.ID] = C.x;
                    CoordsY[C.ID] = C.y;
                    CoordsX[D.ID] = D.x;
                    CoordsY[D.ID] = D.y;

                    double dBD = HNumbKnot.Length(B, D);
                    double dAC = HNumbKnot.Length(A, C);

                    if (dBD <= dAC)
                    {
                        //  B----C
                        //  | \1 |
                        //  |2 \ | 
                        //  A----D
                        //   формируем
                        AreaElems[CountElems].Vertex1 = (uint)A.ID;
                        AreaElems[CountElems].Vertex2 = (uint)D.ID;
                        AreaElems[CountElems].Vertex3 = (uint)B.ID;
                        CountElems++;

                        AreaElems[CountElems].Vertex1 = (uint)C.ID;
                        AreaElems[CountElems].Vertex2 = (uint)B.ID;
                        AreaElems[CountElems].Vertex3 = (uint)D.ID;
                        CountElems++;
                    }
                    else
                    {
                        //  B----C
                        //  |1 / |
                        //  |/ 2 | 
                        //  A----D
                        //   формируем
                        AreaElems[CountElems].Vertex1 = (uint)B.ID;
                        AreaElems[CountElems].Vertex2 = (uint)A.ID;
                        AreaElems[CountElems].Vertex3 = (uint)C.ID;
                        CountElems++;

                        AreaElems[CountElems].Vertex1 = (uint)A.ID;
                        AreaElems[CountElems].Vertex2 = (uint)D.ID;
                        AreaElems[CountElems].Vertex3 = (uint)C.ID;
                        CountElems++;
                    }
                }
            }
            else
            {
                if (rightKnots.Length < leftKnots.Length)
                {
                    // Правый берег
                    for (int i = 0; i < rightKnots.Length - 1; i++)
                    {
                        // узлы КЭ - в против часовой стрелки
                        //  B----C
                        //  | \ 1|
                        //  | 2 \| 
                        //  A----D
                        A = leftKnots[i];
                        B = leftKnots[i + 1];
                        C = rightKnots[i + 1];
                        D = rightKnots[i];

                        CoordsX[A.ID] = A.x;
                        CoordsY[A.ID] = A.y;
                        CoordsX[B.ID] = B.x;
                        CoordsY[B.ID] = B.y;
                        CoordsX[C.ID] = C.x;
                        CoordsY[C.ID] = C.y;
                        CoordsX[D.ID] = D.x;
                        CoordsY[D.ID] = D.y;

                        // формируем
                        AreaElems[CountElems].Vertex1 = (uint)A.ID;
                        AreaElems[CountElems].Vertex2 = (uint)D.ID;
                        AreaElems[CountElems].Vertex3 = (uint)B.ID;
                        CountElems++;

                        AreaElems[CountElems].Vertex1 = (uint)C.ID;
                        AreaElems[CountElems].Vertex2 = (uint)B.ID;
                        AreaElems[CountElems].Vertex3 = (uint)D.ID;
                        CountElems++;
                        //}
                    }
                    int dknot = leftKnots.Length - rightKnots.Length;
                    // если количество узлов справо больше (может только на 1)
                    // узлы КЭ - в против часовой стрелки
                    //  A
                    //  | \
                    //  |  \
                    //  B----D
                    //  | 1 /
                    //  | /  
                    //  C 
                    B = leftKnots[leftKnots.Length - 2];
                    A = leftKnots[leftKnots.Length - 1];
                    D = rightKnots[rightKnots.Length - 1];
                    CoordsX[D.ID] = D.x;
                    CoordsY[D.ID] = D.y;
                    // ПРОВЕРИТЬ ПРАВИЛЬНОСТЬ ОБХОДА УЗЛОВ !!!!!!!!!
                    AreaElems[CountElems].Vertex1 = (uint)A.ID;
                    AreaElems[CountElems].Vertex2 = (uint)B.ID;
                    AreaElems[CountElems].Vertex3 = (uint)D.ID;
                    ++CountElems;
                    if (dknot == 2)
                    {
                        C = leftKnots[leftKnots.Length - 3];
                        // ПРОВЕРИТЬ ПРАВИЛЬНОСТЬ ОБХОДА УЗЛОВ !!!!!!!!!
                        AreaElems[CountElems].Vertex1 = (uint)C.ID;
                        AreaElems[CountElems].Vertex2 = (uint)D.ID;
                        AreaElems[CountElems].Vertex3 = (uint)B.ID;
                        ++CountElems;
                    }
                }
                else
                {
                    // левый берег
                    for (int i = 0; i < leftKnots.Length - 1; i++)
                    {
                        // узлы КЭ - в против часовой стрелки
                        //  B----C
                        //  | \ 2|
                        //  | 1\ | 
                        //  A----D
                        A = leftKnots[i];
                        B = leftKnots[i + 1];
                        C = rightKnots[i + 1];
                        D = rightKnots[i];
                        CoordsX[C.ID] = C.x;
                        CoordsY[C.ID] = C.y;
                        CoordsX[D.ID] = D.x;
                        CoordsY[D.ID] = D.y;
                        //double Lf = B.y - A.y;
                        //double Rf = C.y - D.y;
                        // формируем 
                        AreaElems[CountElems].Vertex1 = (uint)A.ID;
                        AreaElems[CountElems].Vertex2 = (uint)C.ID;
                        AreaElems[CountElems].Vertex3 = (uint)B.ID;
                        CountElems++;

                        AreaElems[CountElems].Vertex1 = (uint)C.ID;
                        AreaElems[CountElems].Vertex2 = (uint)A.ID;
                        AreaElems[CountElems].Vertex3 = (uint)D.ID;
                        CountElems++;
                    }
                    // если количество узлов справо больше (может только на 1)
                    if (rightKnots.Length > leftKnots.Length)
                    {
                        A = leftKnots[leftKnots.Length - 1];
                        B = rightKnots[rightKnots.Length - 1];
                        C = rightKnots[leftKnots.Length - 1];

                        CoordsX[B.ID] = B.x;
                        CoordsY[B.ID] = B.y;
                        CoordsX[C.ID] = C.x;
                        CoordsY[C.ID] = C.y;

                        AreaElems[CountElems].Vertex1 = (uint)A.ID;
                        AreaElems[CountElems].Vertex2 = (uint)C.ID;
                        AreaElems[CountElems].Vertex3 = (uint)B.ID;
                        ++CountElems;
                    }
                }
            }
            return rightKnots;
        }
    }
}

