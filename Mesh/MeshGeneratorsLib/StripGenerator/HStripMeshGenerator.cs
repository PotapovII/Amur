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
//                           Потапов И.И. (массивы в списки)
//          рефакторинг (обобщение) : 14.05.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;

    using MeshLib;
    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.Geometry;
    using System.Collections.Generic;

    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public class HStripMeshGenerator : AStripMeshGenerator
    {
        /// <summary>
        /// Создаваемая сетка
        /// </summary>
        TriMesh mesh = null;
        /// <summary>
        /// КЭ сетки
        /// </summary>
        List<TriElement> AreaElems = new List<TriElement>();
        /// <summary>
        /// граничные КЭ сетки
        /// </summary>
        List<TwoElement> BoundElems = new List<TwoElement>();
        /// <summary>
        /// Тип граничных условий для граничного элемента
        /// </summary>
        List<int> BoundElementsMark = new List<int>();
        /// <summary>
        /// координаты узлов сетки
        /// </summary>
        double[] CoordsX;
        double[] CoordsY;
        /// <summary>
        /// граничные узлы сетки
        /// </summary>
        List<int>  BoundKnots = new List<int>();
        /// <summary>
        /// флаг граничных узлов сетки
        /// </summary>
        List<int> BoundKnotsMark = new List<int>();
        /// <summary>
        /// количество узлов сетки
        /// </summary>
        int CountKnots = 0;
        /// <summary>
        /// локальыне переменные
        /// </summary>
        TwoElement belem;
        /// <summary>
        /// локальыне переменные
        /// </summary>
        TriElement elem;
        double dx = 0;
        int bottom = 0;
        int vertRight = 1;
        int waterLevel = 2;
        int vertLeft = 3;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="MAXElem">максимальное количество КЭ сетки</param>
        /// <param name="MAXKnot">максимальное количество узлов сетки</param>
        public HStripMeshGenerator(bool axisOfSymmetry = false, int MAXKnot = 1000000) 
            : base(axisOfSymmetry)
        {
            CoordsX = new double[MAXKnot];
            CoordsY = new double[MAXKnot];
        }
        public override IMesh CreateMesh(ref double GR, double WaterLevel, 
                                  double[] xx, double[] yy, int Count = 0)
        {
            return CreateTriMesh(ref GR, WaterLevel, xx, yy, Count);
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
        public TriMesh CreateTriMesh(ref double WetBed, double WaterLevel, double[] xx, double[] yy, int Count = 0)
        {
            try
            {
                AreaElems.Clear();
                BoundElems.Clear();
                BoundElementsMark.Clear();
                BoundKnots.Clear();
                BoundKnotsMark.Clear();
                if (xx == null || yy == null)
                {
                    throw new Exception("Геометрия задачи не загружена");
                }
                CountKnots = 0;
                // Поиск береговых точек створа
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
                HNumbKnot lb, lt, rb, rt;
                lb = new HNumbKnot(left.x, left.y, bottom, CountKnots++);
                lt = lb;

                BoundKnots.Add(lb.number);
                BoundKnotsMark.Add(lb.type);

                x = left.x + dx;
                y = spline.Value(x);
                rb = new HNumbKnot(x, y, bottom, CountKnots++);

                BoundKnots.Add(rb.number);
                BoundKnotsMark.Add(rb.type);

                y = left.y;
                rt = new HNumbKnot(x, y, waterLevel, CountKnots++);

                BoundKnots.Add(rt.number);
                BoundKnotsMark.Add(rt.type);

                elem = new TriElement((uint)lb.number, (uint)rb.number, (uint)rt.number);

                AreaElems.Add(elem);

                BoundElementsMark.Add(waterLevel);
                belem = new TwoElement(rt.number, lt.number);
                BoundElems.Add(belem);

                BoundElementsMark.Add(bottom);
                belem = new TwoElement(lb.number, rb.number);
                BoundElems.Add(belem);

                CoordsX[lb.number] = lb.x;
                CoordsY[lb.number] = lb.y;
                CoordsX[rt.number] = rt.x;
                CoordsY[rt.number] = rt.y;
                CoordsX[rb.number] = rb.x;
                CoordsY[rb.number] = rb.y;


                // создание левой границы сетки после 1 КЭ
                HNumbKnot[] knotsTmp = new HNumbKnot[2];
                knotsTmp[0] = new HNumbKnot(rb);
                knotsTmp[1] = new HNumbKnot(rt);
                // герерация сетки в затопленной части
                
                for (int i = 1; i < Count; i++)
                    knotsTmp = DevideStripe(knotsTmp, i, Count);

                // замыкание
                if (dryRight != true)
                {
                    // формирование граничных узлов на затопленной правой границе 
                    for (int i = 0; i < knotsTmp.Length - 1; i++)
                    {
                        if (i != knotsTmp.Length - 2)
                        {
                            BoundKnots.Add(knotsTmp[i + 1].number);
                            if (AxisOfSymmetry == true)
                                BoundKnotsMark.Add(vertRight);
                            else
                                BoundKnotsMark.Add(bottom);
                        }
                        if (AxisOfSymmetry == true)
                            BoundElementsMark.Add(vertRight);
                        else
                            BoundKnotsMark.Add(bottom);
                        belem = new TwoElement(knotsTmp[i + 1].number, knotsTmp[i].number);
                        BoundElems.Add(belem);
                    }
                }
                #region Запись результирующей сетки
                mesh.AreaElems = AreaElems.ToArray();

                mesh.BoundKnots = BoundKnots.ToArray();
                mesh.BoundKnotsMark = BoundKnotsMark.ToArray();

                mesh.BoundElems = BoundElems.ToArray();
                mesh.BoundElementsMark = BoundElementsMark.ToArray();

                mesh.CoordsX = new double[CountKnots];
                mesh.CoordsY = new double[CountKnots];
                for (int i = 0; i < CountKnots; i++)
                {
                    mesh.CoordsX[i] = CoordsX[i];
                    mesh.CoordsY[i] = CoordsY[i];
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
        /// делает разбиение сетки на одном шаге фронта по Х 
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
            // ищем координату правой стороны (она не согласована с сеткой дна!!!)
            x = leftKnots[0].x + dx;
            // если координата вышла за ость симметирии ставим ее на координату оси
            //if (x > width)
            //    x = width;
            // вычисляем уровень дна
            YB = spline.Value(x);
            // строим точку дна правой стороны
            HPoint botom = new HPoint(x, YB);
            // строим точку свободной поверхности для правой стороны
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
            BoundKnots.Add(rightKnots[0].number);
            // устанавливаем флаг данного узла
            BoundKnotsMark.Add(rightKnots[0].type);
            // создаем граничный элемент 
            BoundElementsMark.Add(bottom);
            belem = new TwoElement(leftKnots[0].number, rightKnots[0].number);
            BoundElems.Add(belem);
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
                BoundKnots.Add(rightKnots[k].number);
                BoundKnotsMark.Add(rightKnots[k].type);
                // создаем ГЭ на свободной поверхности
                BoundElementsMark.Add(waterLevel);
                belem = new TwoElement(rightKnots[k].number, leftKnots[leftKnots.Length - 1].number);
                BoundElems.Add(belem);
            }
            else  // правый угол
            {
                belem = new TwoElement(rightKnots[0].number, leftKnots[leftKnots.Length - 1].number);
                // создаем ГЭ на свободной поверхности
                BoundElementsMark.Add(waterLevel);
                BoundElems.Add(belem);
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

                    SetElems(A, B, C, D);
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
                        SetElems(A, B, C, D);
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
                    CoordsX[D.number] = D.x;
                    CoordsY[D.number] = D.y;
                    // формируем
                    elem = new TriElement(A.number, B.number, D.number);
                    AreaElems.Add(elem);

                    if (dknot == 2)
                    {
                        C = leftKnots[leftKnots.Length - 3];
                        elem = new TriElement(C.number, D.number, B.number);
                        AreaElems.Add(elem);
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
                        SetElems(A, B, C, D);
                    }
                    // если количество узлов справо больше (может только на 1)
                    if (rightKnots.Length > leftKnots.Length)
                    {
                        A = leftKnots[leftKnots.Length - 1];
                        B = rightKnots[rightKnots.Length - 1];
                        C = rightKnots[leftKnots.Length - 1];

                        CoordsX[B.number] = B.x;
                        CoordsY[B.number] = B.y;
                        CoordsX[C.number] = C.x;
                        CoordsY[C.number] = C.y;

                        elem = new TriElement(A.number, C.number, B.number);
                        AreaElems.Add(elem);
                    }
                }
            }
            return rightKnots;
        }

        protected void SetElems(HNumbKnot A, HNumbKnot B, HNumbKnot C, HNumbKnot D)
        {
            CoordsX[A.number] = A.x;
            CoordsY[A.number] = A.y;
            CoordsX[B.number] = B.x;
            CoordsY[B.number] = B.y;
            CoordsX[C.number] = C.x;
            CoordsY[C.number] = C.y;
            CoordsX[D.number] = D.x;
            CoordsY[D.number] = D.y;

            double dBD = HNumbKnot.Length(B, D);
            double dAC = HNumbKnot.Length(A, C);

            if (dBD <= dAC)
            {
                //  B----C
                //  | \1 |
                //  |2 \ | 
                //  A----D
                //   формируем
                elem = new TriElement(A.number, D.number, B.number);
                AreaElems.Add(elem);
                elem = new TriElement(C.number, B.number, D.number);
                AreaElems.Add(elem);
            }
            else
            {
                //  B----C
                //  |1 / |
                //  |/ 2 | 
                //  A----D
                //   формируем
                elem = new TriElement(B.number, A.number, C.number);
                AreaElems.Add(elem);
                elem = new TriElement(A.number, D.number, C.number);
                AreaElems.Add(elem);
            }
        }
    }
}

