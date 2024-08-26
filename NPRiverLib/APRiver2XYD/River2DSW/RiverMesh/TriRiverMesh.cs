//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 02.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD.River2DSW
{
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Geometry;

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using GeometryLib;

    //---------------------------------------------------------------------------
    //  ОО: TriRiverMesh - базистная техузловая конечно-элементная сетка 
    //---------------------------------------------------------------------------
    [Serializable]
    public class TriRiverMesh : IRiverMesh
    {
        #region Свойства сетки
        /// <summary>
        /// Количество узлов на основном КЭ 
        /// </summary>
        public TypeFunForm First { get => TypeFunForm.Form_2D_Triangle_L1_River; }
        /// <summary>                                 
        /// Количество узлов на вспомогательном КЭ 
        /// </summary>
        public TypeFunForm Second { get => TypeFunForm.Form_2D_Triangle_L1_River; }
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        public TypeRangeMesh typeRangeMesh { get => TypeRangeMesh.mRange1; }
        /// <summary>
        /// Тип КЭ сетки: 2D
        /// </summary>
        public TypeMesh typeMesh { get => TypeMesh.Triangle; }
        #endregion 
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TriElementRiver[] AreaElems;
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public BoundElementRiver[] BoundElems;
        public IRiverNode[] Nodes { get => nodes; }
        /// <summary>
        /// Узлы КЭ сетки
        /// </summary>
        public RiverNode[] nodes;
        /// <summary>
        /// Активные сегменеты контура границы в расчетной области
        /// </summary>
        public BoundSegmentRiver[] boundSegment;
        /// <summary>
        public TriRiverMesh() { }
        public TriRiverMesh(TriRiverMesh m)
        {
            AreaElems = new TriElementRiver[m.AreaElems.Length];
            for (int i = 0; i < AreaElems.Length; i++)
                AreaElems[i] = new TriElementRiver(m.AreaElems[i]);
            BoundElems = new BoundElementRiver[m.BoundElems.Length];
            for (int i = 0; i < BoundElems.Length; i++)
                BoundElems[i] = new BoundElementRiver(m.BoundElems[i]);
            nodes = new RiverNode[m.nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = new RiverNode(m.nodes[i]);
            boundSegment = new BoundSegmentRiver[m.boundSegment.Length];
            for (int i = 0; i < boundSegment.Length; i++)
                boundSegment[i] = new BoundSegmentRiver(m.boundSegment[i]);
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public IMesh Clone()
        {
            return new TriRiverMesh(this);
        }
        /// <summary>
        /// Внутренняя перенумерация 
        /// </summary>
        public void RenumberingID()
        {
            int maxID = nodes.Max(ii => ii.n);

            uint[] newID = new uint[maxID + 1];
            for (uint i = 0; i < nodes.Length; i++)
            {
                newID[nodes[i].n] = i;
                nodes[i].n = (int)i;
            }
            foreach (var e in AreaElems)
            {
                e.Vertex1 = newID[e.Vertex1];
                e.Vertex2 = newID[e.Vertex2];
                e.Vertex3 = newID[e.Vertex3];
            }
            foreach (var e in BoundElems)
            {
                e.Vertex1 = newID[e.Vertex1];
                e.Vertex2 = newID[e.Vertex2];
            }
            foreach (var e in boundSegment)
            {
                e.startnode = (int)newID[e.startnode];
                e.endnode = (int)newID[e.endnode];
            }
        }

        /// <summary>
        /// Количество элементов
        /// </summary>
        public int CountElements
        {
            get { return AreaElems == null ? 0 : AreaElems.Length; }
        }
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        public int CountBoundElements
        {
            get { return BoundElems == null ? 0 : BoundElems.Length; }
        }
        /// <summary>
        /// Количество узлов
        /// </summary>
        public int CountKnots
        {
            get { return nodes == null ? 0 : nodes.Length; }
        }
        /// <summary>
        /// Количество активных сегменетов контура границы в расчетной области
        /// </summary>
        public int CountSegment
        {
            get { return boundSegment == null ? 0 : boundSegment.Length; }
        }
        /// <summary>
        /// Количество граничных узлов
        /// </summary>
        public int CountBoundKnots
        {
            get
            {
                List<int> BoundKnots = new List<int>();
                for (int i = 0; i < nodes.Length; i++)
                    if (nodes[i].segmentID != -1)
                        BoundKnots.Add(i);
                return BoundKnots.Count;
            }
        }
        /// <summary>
        /// Диапазон координат для узлов сетки
        /// </summary>
        public void MinMax(int dim, ref double min, ref double max)
        {
            var mas = GetCoords(dim);
            max = mas == null ? double.MaxValue : mas.Max();
            min = mas == null ? double.MinValue : mas.Min();
        }
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TriElement[] GetAreaElems()
        {
            TriElement[] te = new TriElement[CountElements];
            for (int i = 0; i < CountElements; i++)
                te[i] = AreaElems[i].nodes;
            return te;
        }
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoElement[] GetBoundElems()
        {
            TwoElement[] te = new TwoElement[CountBoundElements];
            for (int i = 0; i < CountBoundElements; i++)
                te[i] = BoundElems[i].nodes;
            return te;
        }
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public int[] GetBElementsBCMark()
        {
            int[] te = new int[CountElements];
            for (int i = 0; i < CountElements; i++)
                te[i] = BoundElems[i].segmentID;
            return te;
        }
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public TypeBoundCond[] GetBoundElementsType()
        {
            TypeBoundCond[] te = null;
            for (int i = 0; i < CountElements; i++)
                te[i] =  TypeBoundCond.Neumann;
            return te;
        }
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public int[] GetBoundKnots()
        {
            List<int> BoundKnots = new List<int>();
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].segmentID != -1)
                    BoundKnots.Add(i);
            return BoundKnots.ToArray();
        }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] GetBoundKnotsMark()
        {
            List<int> BoundKnotsMark = new List<int>();
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].segmentID != -1)
                    BoundKnotsMark.Add(nodes[i].segmentID);
            return BoundKnotsMark.ToArray();
        }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public TypeBoundCond[] GetBKnotsBCType() { return null; }

        /// <summary>
        /// Координаты X или Y для узловых точек 
        /// </summary>
        public double[] GetCoords(int dim)
        {
            double[] Coords = new double[nodes.Length];
            if (dim == 0)
            {
                for (int i = 0; i < Coords.Length; i++)
                    Coords[i] = nodes[i].X;
            }
            else
            {
                for (int i = 0; i < Coords.Length; i++)
                    Coords[i] = nodes[i].Y;
            }
            return Coords;
        }
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TriElement Element(uint i)
        {
            return AreaElems[i].nodes;
        }
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            knots[0] = AreaElems[i].Vertex1;
            knots[1] = AreaElems[i].Vertex2;
            knots[2] = AreaElems[i].Vertex3;
            return TypeFunForm.Form_2D_Triangle_L1;
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            bknot[0] = BoundElems[i].Vertex1;
            bknot[1] = BoundElems[i].Vertex2;
            return TypeFunForm.Form_1D_L1;
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>

        public void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            X[0] = nodes[AreaElems[i].Vertex1].X;
            X[1] = nodes[AreaElems[i].Vertex2].X;
            X[2] = nodes[AreaElems[i].Vertex3].X;

            Y[0] = nodes[AreaElems[i].Vertex1].Y;
            Y[1] = nodes[AreaElems[i].Vertex2].Y;
            Y[2] = nodes[AreaElems[i].Vertex3].Y;
        }
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>
        public void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            elementValue[0] = Values[AreaElems[i].Vertex1];
            elementValue[1] = Values[AreaElems[i].Vertex2];
            elementValue[2] = Values[AreaElems[i].Vertex3];
        }
        public uint GetMaxKnotDecrementForElement(uint i)
        {
            uint min = AreaElems[i].Vertex1;
            uint max = AreaElems[i].Vertex1;
            min = Math.Min(Math.Min(min, AreaElems[i].Vertex1),
                  Math.Min(AreaElems[i].Vertex2, AreaElems[i].Vertex3));

            max = Math.Max(Math.Max(max, AreaElems[i].Vertex1),
                Math.Max(AreaElems[i].Vertex2, AreaElems[i].Vertex3));
            return (max - min);
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public double ElemSquare(uint elem)
        {
            TriElement knot = Element(elem);
            double S = (nodes[knot.Vertex1].X * (nodes[knot.Vertex2].Y - nodes[knot.Vertex3].Y) +
                         nodes[knot.Vertex2].X * (nodes[knot.Vertex3].Y - nodes[knot.Vertex1].Y) +
                         nodes[knot.Vertex3].X * (nodes[knot.Vertex1].Y - nodes[knot.Vertex2].Y)) / 2.0;
            return S;
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public double ElemSquare(TriElement knot)
        {
            double S = (nodes[knot.Vertex1].X * (nodes[knot.Vertex2].Y - nodes[knot.Vertex3].Y) +
                        nodes[knot.Vertex2].X * (nodes[knot.Vertex3].Y - nodes[knot.Vertex1].Y) +
                        nodes[knot.Vertex3].X * (nodes[knot.Vertex1].Y - nodes[knot.Vertex2].Y)) / 2.0;
            return S;
        }
        /// <summary>
        /// Вычисление длины граничного КЭ
        /// </summary>
        /// <param name="belement">номер граничного конечного элемента</param>
        /// <returns></returns>
        public double GetBoundElemLength(uint belement)
        {
            TwoElement knot = BoundElems[belement].nodes;
            double a = nodes[knot.Vertex1].X - nodes[knot.Vertex2].X;
            double b = nodes[knot.Vertex1].Y - nodes[knot.Vertex2].Y;
            double Length = Math.Sqrt(a * a + b * b);
            return Length;
        }
        /// <summary>
        /// Вычисление уровня свободной поверхности на граничном  конечном элементе
        /// </summary>
        /// <param name="be">гкэ</param>
        /// <returns></returns>
        public double FreeFlowSurfaceElement(uint be)
        {
            TwoElement knot = BoundElems[be].nodes;
            double wse = (nodes[knot.Vertex1].zeta +  // дно
                          nodes[knot.Vertex1].h +     // глубина  
                          nodes[knot.Vertex2].zeta +
                          nodes[knot.Vertex2].h) * 0.5;
            return wse;
        }
        /// <summary>
        /// Получить выборку граничных узлов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        public uint[] GetBoundKnotsByMarker(int idx)
        {
            List<uint> BoundKnotsMark = new List<uint>();
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i].segmentID != idx)
                    BoundKnotsMark.Add((uint)nodes[i].segmentID);
            uint[] mass = BoundKnotsMark.ToArray();
            Array.Sort(mass);
            return mass;
        }
        /// <summary>
        /// Получить выборку граничных элементов по типу ГУ
        /// </summary>
        /// <param name="id">тип ГУ</param>
        /// <returns>массив ГЭ</returns>
        public uint[][] GetBoundElementsByMarker(int id)
        {
            int count = 0;
            for (int k = 0; k < CountBoundElements; k++)
            {
                if (BoundElems[k].segmentID == id)
                    ++count;
            }
            uint[][] mass = new uint[count][];
            int j = 0;
            for (int k = 0; k < CountBoundElements; k++)
            {
                if (BoundElems[k].segmentID == id)
                {
                    TwoElement el = BoundElems[k].nodes;
                    mass[j] = new uint[2];
                    mass[j][0] = el.Vertex1;
                    mass[j][1] = el.Vertex2;
                    j++;
                }
            }
            return mass;
        }
        /// <summary>
        /// Получить тип граничных условий для граничного элемента
        /// </summary>
        /// <param name="elem">граничный элемент</param>
        /// <returns>ID типа граничных условий</returns>
        public int GetBoundElementMarker(uint elem)
        {
            return BoundElems[elem].segmentID;
        }
        /// <summary>
        /// Ширина ленты в глобальной матрице жнсткости
        /// </summary>
        /// <returns></returns>
        public uint GetWidthMatrix()
        {
            uint max = GetMaxKnotDecrementForElement(0);
            for (uint i = 1; i < AreaElems.Length; i++)
            {
                uint tmp = GetMaxKnotDecrementForElement(i);
                if (max < tmp)
                    max = tmp;
            }
            return max + 1;
        }
        #region Утилиты
        /// <summary>
        /// Определение индексов границ для граничных КЭ по сегменту
        /// </summary>
        /// <param name="segmentID"></param>
        /// <param name="sID"></param>
        /// <param name="eID"></param>
        public void GetBConditionID(int segmentID, ref int sID, ref int eID)
        {
            if (boundSegment[segmentID].startnode != boundSegment[segmentID].endnode)
            {
                // корректное определение начала и конца ГЭ для сенмента
                for (int i = 0; i < BoundElems.Length; i++)
                    if (boundSegment[segmentID].startnode == BoundElems[i].Vertex1)
                    {
                        int ii = (BoundElems.Length + i - 1) % BoundElems.Length;
                        if (BoundElems[ii].boundCondType == 1)
                            sID = ii;
                        else
                            sID = i; 
                        break;
                    }
                for (int i = 0; i < BoundElems.Length; i++)
                    if (boundSegment[segmentID].endnode == BoundElems[i].Vertex2)
                    {
                        eID = i; 

                        break;
                    }
                //if(sID > eID)
                //{
                //    int tmp = sID; sID = eID; eID = tmp; 
                //}
            }
            else // костыль связанный с ошибкой в ранних версиях river2D при добавлении узла
                 // и не корректной перенумерацией номерров 
            {
                int idx = 0;
                for (int i = 1; i < BoundElems.Length; i++)
                    if (BoundElems[i].boundCondType != BoundElems[i-1].boundCondType)
                    {
                        if (idx == 0)
                        {
                            if (BoundElems[i - 1].boundCondType == 1)
                            {
                                eID = (int)BoundElems[i - 1].Vertex2;
                                boundSegment[segmentID].endnode = eID;
                            }
                            else
                            {
                                sID = (int)BoundElems[i].Vertex1;
                                boundSegment[segmentID].startnode = sID;
                            }
                            idx = 1;
                        }
                        if (idx == 1)
                        {
                            if (BoundElems[i - 1].boundCondType == 1)
                            {
                                eID = (int)BoundElems[i - 1].Vertex2;
                                boundSegment[segmentID].endnode = eID;
                            }
                                
                            else
                            {
                                sID = (int)BoundElems[i].Vertex1;
                                boundSegment[segmentID].startnode = sID;
                            }
                            break;
                        }
                    }

            }

        }


        #endregion

        public void Print()
        {
            Console.WriteLine();
            Console.WriteLine("CoordsX CoordsY");
            for (int i = 0; i < nodes.Length; i++)
            {
                Console.WriteLine(" id {0} x {1:F4} y {2:F4}", i, nodes[i].X, nodes[i].Y);
            }
            Console.WriteLine();
            Console.WriteLine("BoundKnots");
            int[] BoundKnots = GetBoundKnotsMark();
            for (int i = 0; i < BoundKnots.Length; i++)
            {
                Console.WriteLine(" id {0} ", BoundKnots[i]);
            }
            Console.WriteLine();
            Console.WriteLine("FE");
            for (int i = 0; i < AreaElems.Length; i++)
            {
                int ID = i;
                uint n0 = AreaElems[i].Vertex1;
                uint n1 = AreaElems[i].Vertex2;
                uint n2 = AreaElems[i].Vertex3;
                Console.WriteLine(" id {0}: {1} {2} {3}", ID, n0, n1, n2);
            }
            Console.WriteLine();
            Console.WriteLine("BFE");
            for (int i = 0; i < BoundElems.Length; i++)
            {
                uint n0 = BoundElems[i].Vertex1;
                uint n1 = BoundElems[i].Vertex2;
                int fl = BoundElems[i].segmentID;
                Console.WriteLine(" id {0}: {1} {2} fl {3}", i, n0, n1, fl);
            }
        }
    }
}