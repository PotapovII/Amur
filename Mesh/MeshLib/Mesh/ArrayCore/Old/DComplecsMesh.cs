////---------------------------------------------------------------------------
////                          ПРОЕКТ  "H?V"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 15.10.2000 Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 15.03.2001 Потапов И.И. (с++)
////                         ПРОЕКТ  "DISER"
////                 правка  :   9.03.2002 Потапов И.И. (с++)
////                         ПРОЕКТ  "RiverLib"
////                 правка  :   06.12.2020 Потапов И.И. (с++ => c#)
////                       HBaseMesh => ComplecsMesh
////---------------------------------------------------------------------------
////                          ПРОЕКТ  "МКЭ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
//namespace MeshLib
//{
//    using CommonLib;
//    using MemLogLib;
//    using System;
//    using System.Linq;
//    /// <summary>
//    /// ОО: Произвольная базисная сетка задачи с массивной концепцией хранения данных
//    /// </summary>
//    [Serializable]
//    public class ArrayMesh : IMesh
//    {
//        /// <summary>
//        /// Количество узлов на основном КЭ 
//        /// </summary>
//        public TypeFunForm First { get => first; }
//        public TypeFunForm first;
//        /// <summary>
//        /// Количество узлов на вспомогательном КЭ 
//        /// </summary>
//        public TypeFunForm Second { get => second; }
//        public TypeFunForm second;
//        /// <summary>
//        /// Порядок сетки на которой работает функция формы
//        /// </summary>
//        public TypeRangeMesh tRangeMesh;
//        public TypeRangeMesh typeRangeMesh { get => tRangeMesh; }
//        /// <summary>
//        /// Тип КЭ сетки в 1D и 2D
//        /// </summary>
//        public TypeMesh tMesh;
//        public TypeMesh typeMesh { get => tMesh; }
//        /// <summary>
//        /// Вектор конечных элементов в области
//        /// </summary>
//        public uint[][] AreaElems;
//        /// <summary>
//        /// Тип конечных элементов в области
//        /// </summary>
//        public TypeFunForm[] AreaElemsFFType;
//        /// <summary>
//        /// Вектор конечных элементов на границе
//        /// </summary>
//        public uint[][] BoundElems;
//        /// <summary>
//        /// Получить массив маркеров для граничных элементов 
//        /// </summary>
//        public int[] BoundElementsMark;
//        /// <summary>
//        /// Получить массив типов границы для граничных элементов 
//        /// </summary>
//        public TypeBoundCond[] BoundElementsType;
//        /// <summary>
//        /// тип конечных элементов на границе
//        /// </summary>
//        public TypeFunForm[] BoundType;
//        /// <summary>
//        /// Вектор граничных узловых точек
//        /// </summary>
//        public int[] BoundKnots;
//        public int[] BoundKnotsMark;
//        /// <summary>
//        /// Координаты узловых точек и параметров определенных в них
//        /// </summary>
//        public double[] CoordsX;
//        public double[] CoordsY;
//        public ComplecsMesh()
//        {
//            this.tRangeMesh = TypeRangeMesh.mRange1;
//            this.tMesh = TypeMesh.MixMesh;
//        }
//        public ComplecsMesh(ComplecsMesh m)
//        {
//            this.tRangeMesh = m.tRangeMesh;
//            this.tMesh = m.tMesh;
//            MEM.MemCopy(ref CoordsX, m.CoordsX);
//            MEM.MemCopy(ref CoordsY, m.CoordsY);
//            MEM.MemCopy(ref AreaElems, m.AreaElems);
//            MEM.MemCopy(ref BoundElems, m.BoundElems);
//            MEM.MemCopy(ref BoundElementsMark, m.BoundElementsMark);
//            MEM.MemCopy(ref BoundElementsType, m.BoundElementsType);
//            MEM.MemCopy(ref BoundType, m.BoundType);
//            MEM.MemCopy(ref BoundKnots, m.BoundKnots);
//            MEM.MemCopy(ref BoundKnotsMark, m.BoundKnotsMark);
//        }
//        /// <summary>
//        /// Клонирование объекта сетки
//        /// </summary>
//        public IMesh Clone()
//        {
//            return new ComplecsMesh(this);
//        }
//        public int CountElements
//        {
//            get { return AreaElems == null ? 0 : AreaElems.Length; }
//        }
//        /// <summary>
//        /// Количество граничных элементов
//        /// </summary>
//        public int CountBoundElements
//        {
//            get { return BoundElems == null ? 0 : BoundElems.Length; }
//        }
//        public int CountKnots
//        {
//            get { return CoordsX == null ? 0 : CoordsX.Length; }
//        }
//        public int CountBoundKnots
//        {
//            get { return BoundKnots == null ? 0 : BoundKnots.Length; }
//        }
//        /// <summary>
//        /// Диапазон координат для узлов сетки
//        /// </summary>
//        public void MinMax(int dim, ref double min, ref double max)
//        {
//            var mas = GetCoords(dim);
//            max = mas == null ? double.MaxValue : mas.Max();
//            min = mas == null ? double.MinValue : mas.Min();
//        }

//        public TypeFunForm ElementKnots(uint i, ref uint[] knots)
//        {
//            knots = AreaElems[i];
//            return AreaElemsFFType[i];
//        }
//        /// <summary>
//        /// Получить узлы граничного элемента
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        public TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
//        {
//            bknot = BoundElems[i];
//            return (TypeFunForm)(bknot.Length - 1);
//        }
//        /// <summary>
//        /// Адаптер для отрисовки. 
//        /// Вектор конечных элементов приведенный к трехузловым КЭ
//        /// </summary>
//        public TriElement[] GetAreaElems()
//        {
//            // Здесь заплатка поскольку на каждый КЭ общего вида необходим адаптер к TriElement
//            // Для полной реализации необходима библиотека функций форм КЭ (пока не востановлена)
//            // Пока сделано для 3 и 4 узловых КЭ - в
//            uint CountTriFE = 0;
//            if (AreaElems[0].Length == 3 || AreaElems[0].Length == 4)
//            {
//                for (int i = 0; i < AreaElems.Length; i++)
//                {
//                    CountTriFE++;
//                    if (AreaElems[i].Length == 4)
//                        CountTriFE++;
//                }
//                TriElement[] triElements = new TriElement[CountTriFE];
//                CountTriFE = 0;
//                for (int i = 0; i < AreaElems.Length; i++)
//                {
//                    triElements[CountTriFE++] = new TriElement(AreaElems[i][0], AreaElems[i][1], AreaElems[i][2]);
//                    if (AreaElems[i].Length == 4)
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][0], AreaElems[i][2], AreaElems[i][3]);
//                }
//                return triElements;
//            }
//            if (AreaElems[0].Length == 6 || AreaElems[0].Length == 9)
//            {
//                //  *
//                //  | \
//                //  |  \
//                //  * ..* 
//                //  | . .\
//                //  |  .. \
//                //  * --*--*
//                for (int i = 0; i < AreaElems.Length; i++)
//                {
//                    CountTriFE += 4;
//                    if (AreaElems[i].Length == 9)
//                        CountTriFE += 8;
//                }
//                TriElement[] triElements = new TriElement[CountTriFE];
//                CountTriFE = 0;
//                for (int i = 0; i < AreaElems.Length; i++)
//                {
//                    if (AreaElems[i].Length == 6)
//                    {
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][0], AreaElems[i][1], AreaElems[i][5]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][3], AreaElems[i][5]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][2], AreaElems[i][3]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][5], AreaElems[i][3], AreaElems[i][4]);
//                    }
//                    if (AreaElems[i].Length == 9)
//                    {
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][0], AreaElems[i][1], AreaElems[i][7]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][8], AreaElems[i][7]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][2], AreaElems[i][3]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][3], AreaElems[i][8], AreaElems[i][1]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][8], AreaElems[i][3], AreaElems[i][5]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][3], AreaElems[i][4], AreaElems[i][5]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][7], AreaElems[i][8], AreaElems[i][5]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][7], AreaElems[i][5], AreaElems[i][6]);
//                    }
//                }
//                return triElements;
//            }
//            if (AreaElems[0].Length == 10 || AreaElems[0].Length == 16)
//            {
//                for (int i = 0; i < AreaElems.Length; i++)
//                {
//                    CountTriFE += 9;
//                    if (AreaElems[i].Length == 16)
//                        CountTriFE += 18;
//                }
//                TriElement[] triElements = new TriElement[CountTriFE];
//                CountTriFE = 0;
//                for (int i = 0; i < AreaElems.Length; i++)
//                {
//                    if (AreaElems[i].Length == 10)
//                    {
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][0], AreaElems[i][1], AreaElems[i][8]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][9], AreaElems[i][8]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][2], AreaElems[i][9]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][2], AreaElems[i][4], AreaElems[i][9]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][2], AreaElems[i][3], AreaElems[i][4]);

//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][8], AreaElems[i][9], AreaElems[i][7]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][9], AreaElems[i][5], AreaElems[i][7]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][9], AreaElems[i][4], AreaElems[i][5]);

//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][7], AreaElems[i][5], AreaElems[i][6]);
//                    }
//                    if (AreaElems[i].Length == 16)
//                    {
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][0], AreaElems[i][1], AreaElems[i][11]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][12], AreaElems[i][11]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][1], AreaElems[i][2], AreaElems[i][12]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][2], AreaElems[i][13], AreaElems[i][12]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][2], AreaElems[i][3], AreaElems[i][13]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][3], AreaElems[i][4], AreaElems[i][13]);

//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][11], AreaElems[i][12], AreaElems[i][10]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][12], AreaElems[i][15], AreaElems[i][10]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][12], AreaElems[i][13], AreaElems[i][14]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][12], AreaElems[i][14], AreaElems[i][15]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][13], AreaElems[i][4], AreaElems[i][14]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][4], AreaElems[i][5], AreaElems[i][14]);

//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][10], AreaElems[i][15], AreaElems[i][9]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][15], AreaElems[i][8], AreaElems[i][9]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][15], AreaElems[i][14], AreaElems[i][8]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][14], AreaElems[i][7], AreaElems[i][8]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][14], AreaElems[i][5], AreaElems[i][7]);
//                        triElements[CountTriFE++] = new TriElement(AreaElems[i][5], AreaElems[i][6], AreaElems[i][7]);
//                    }
//                }
//                return triElements;
//            }
//            return null;
//        }
//        /// <summary>
//        /// Вектор конечных элементов на границе
//        /// </summary>
//        public TwoElement[] GetBoundElems()
//        {
//            // Здесь заплатка поскольку на каждый КЭ общего вида необходим адаптер к TwoElement
//            // Пока сделано для 2 - х узловых граничных КЭ - в
//            if (BoundElems[0].Length == 2)
//            {
//                TwoElement[] BElements = new TwoElement[BoundElems.Length];
//                for (int i = 0; i < BoundElems.Length; i++)
//                    BElements[i] = new TwoElement(BoundElems[i][0], BoundElems[i][1]);
//                return BElements;
//            }
//            if (BoundElems[0].Length == 3)
//            {
//                uint CountBFE = 0;
//                TwoElement[] BElements = new TwoElement[2 * BoundElems.Length];
//                for (int i = 0; i < BoundElems.Length; i++)
//                {
//                    BElements[CountBFE++] = new TwoElement(BoundElems[i][0], BoundElems[i][1]);
//                    BElements[CountBFE++] = new TwoElement(BoundElems[i][1], BoundElems[i][2]);
//                }
//                return BElements;
//            }
//            if (BoundElems[0].Length == 4)
//            {
//                uint CountBFE = 0;
//                TwoElement[] BElements = new TwoElement[3 * BoundElems.Length];
//                for (int i = 0; i < BoundElems.Length; i++)
//                {
//                    BElements[CountBFE++] = new TwoElement(BoundElems[i][0], BoundElems[i][1]);
//                    BElements[CountBFE++] = new TwoElement(BoundElems[i][1], BoundElems[i][2]);
//                    BElements[CountBFE++] = new TwoElement(BoundElems[i][2], BoundElems[i][3]);
//                }
//                return BElements;
//            }
//            return null;
//        }
//        /// <summary>
//        /// Получить массив маркеров для граничных элементов 
//        /// </summary>
//        public int[] GetBElementsBCMark()
//        {
//            return BoundElementsMark;
//        }
//        /// <summary>
//        /// Получить массив маркеров для граничных элементов 
//        /// </summary>
//        public TypeBoundCond[] GetBoundElementsType()
//        {
//            return BoundElementsType;
//        }
//        /// <summary>
//        /// Массив граничных узловых точек
//        /// </summary>
//        public int[] GetBoundKnots() { return BoundKnots; }
//        /// <summary>
//        /// Массив меток  для граничных узловых точек
//        /// </summary>
//        public int[] GetBoundKnotsMark() { return BoundKnotsMark; }

//        /// <summary>
//        /// Координаты X или Y для узловых точек 
//        /// </summary>
//        public double[] GetCoords(int dim)
//        {
//            if (dim == 0)
//                return CoordsX;
//            else
//                return CoordsY;
//        }
//        public void ElemX(uint i, ref double[] X)
//        {
//            MEM.Alloc<double>(AreaElems[i].Length, ref X, "X");
//            for (int n = 0; n < AreaElems[i].Length; n++)
//                X[n] = CoordsX[AreaElems[i][n]];
//        }
//        public void ElemY(uint i, ref double[] Y)
//        {
//            MEM.Alloc<double>(AreaElems[i].Length, ref Y, "Y");
//            for (int n = 0; n < AreaElems[i].Length; n++)
//                Y[n] = CoordsY[AreaElems[i][n]];
//        }
//        public void ElemValues(double[] Values, uint i, ref double[] elementValue)
//        {
//            for (int j = 0; j < AreaElems[i].Length; j++)
//                elementValue[j] = Values[AreaElems[i][j]];
//        }
//        public uint GetMaxKnotDecrementForElement(uint i)
//        {
//            uint min = AreaElems[i][0];
//            uint max = AreaElems[i][0];
//            for (int j = 1; j < AreaElems[i].Length; j++)
//            {
//                if (min > AreaElems[i][j])
//                    min = AreaElems[i][j];

//                if (max < AreaElems[i][j])
//                    max = AreaElems[i][j];
//            }
//            return max - min;
//        }
//        /// <summary>
//        /// Вычисление площади КЭ
//        /// </summary>
//        public double ElemSquare(TriElement element)
//        {
//            return (CoordsX[element.Vertex1] * (CoordsY[element.Vertex2] - CoordsY[element.Vertex3]) +
//                    CoordsX[element.Vertex2] * (CoordsY[element.Vertex3] - CoordsY[element.Vertex1]) +
//                    CoordsX[element.Vertex3] * (CoordsY[element.Vertex1] - CoordsY[element.Vertex2])) / 2.0;
//        }
//        /// <summary>
//        ///  Вычисление площади КЭ первого порядка
//        /// </summary>
//        public double ElemSquare(uint elem)
//        {
//            uint[] knot = { 0, 0, 0, 0 };
//            ElementKnots(elem, ref knot);
//            double S;
//            if (knot.Length > 4)
//                throw new Exception("Площадь КЭ высокого порядка необходимо вычислять численно!");
//            if (knot.Length == 3)
//            {
//                S = (CoordsX[knot[0]] * (CoordsY[knot[1]] - CoordsY[knot[2]]) +
//                     CoordsX[knot[1]] * (CoordsY[knot[2]] - CoordsY[knot[0]]) +
//                     CoordsX[knot[2]] * (CoordsY[knot[0]] - CoordsY[knot[1]])) / 2.0;
//            }
//            else
//            {
//                S = (CoordsX[knot[0]] * (CoordsY[knot[1]] - CoordsY[knot[2]]) +
//                     CoordsX[knot[1]] * (CoordsY[knot[2]] - CoordsY[knot[0]]) +
//                     CoordsX[knot[2]] * (CoordsY[knot[0]] - CoordsY[knot[1]])) / 2.0;

//                S += (CoordsX[knot[0]] * (CoordsY[knot[2]] - CoordsY[knot[3]]) +
//                      CoordsX[knot[2]] * (CoordsY[knot[3]] - CoordsY[knot[0]]) +
//                      CoordsX[knot[3]] * (CoordsY[knot[0]] - CoordsY[knot[2]])) / 2.0;
//            }
//            return S;
//        }
//        /// <summary>
//        /// Вычисление длины граничного КЭ
//        /// </summary>
//        /// <param name="belement">номер граничного конечного элемента</param>
//        /// <returns></returns>
//        public double GetBoundElemLength(uint belement)
//        {
//            uint[] knot = BoundElems[belement];
//            if (knot.Length > 2)
//                throw new Exception("Длину граничных КЭ высокого порядка необходимо вычислять численно!");
//            double a = CoordsX[knot[0]] - CoordsX[knot[1]];
//            double b = CoordsY[knot[0]] - CoordsY[knot[1]];
//            double Length = Math.Sqrt(a * a + b * b);
//            return Length;
//        }
//        /// <summary>
//        /// Получить выборку граничных узлов по типу ГУ
//        /// </summary>
//        /// <param name="i">тип ГУ</param>
//        /// <returns></returns>
//        public uint[] GetBoundKnotsByMarker(int i)
//        {
//            int count = 0;
//            for (int k = 0; k < BoundKnots.Length; k++)
//            {
//                if (BoundKnotsMark[k] == i)
//                    ++count;
//            }
//            uint[] mass = new uint[count];

//            int j = 0;
//            for (int k = 0; k < BoundKnots.Length; k++)
//            {
//                if (BoundKnotsMark[k] == i)
//                    mass[j++] = (uint)BoundKnots[k];
//            }
//            Array.Sort(mass);
//            return mass;
//        }
//        /// <summary>
//        /// Получить выборку граничных элементов по типу ГУ
//        /// </summary>
//        /// <param name="id">тип ГУ</param>
//        /// <returns>массив ГЭ</returns>
//        public uint[][] GetBoundElementsByMarker(int id)
//        {
//            int count = 0;
//            for (int k = 0; k < BoundElementsMark.Length; k++)
//            {
//                if (BoundElementsMark[k] == id)
//                    ++count;
//            }
//            uint[][] mass = new uint[count][];
//            int j = 0;
//            for (int k = 0; k < CountBoundElements; k++)
//            {
//                if (BoundElementsMark[k] == id)
//                {
//                    uint[] el = BoundElems[k];
//                    mass[j] = new uint[el.Length];
//                    for (int i = 0; i < el.Length; i++)
//                        mass[j][i] = el[i];
//                    j++;
//                }
//            }
//            return mass;
//        }
//        /// <summary>
//        /// Получить тип граничных условий для граничного элемента
//        /// </summary>
//        /// <param name="elem">граничный элемент</param>
//        /// <returns>ID типа граничных условий</returns>
//        public int GetBoundElementMarker(uint elem)
//        {
//            return BoundElementsMark[elem];
//        }
//        /// <summary>
//        /// Ширина ленты в глобальной матрице жнсткости
//        /// </summary>
//        /// <returns></returns>
//        public uint GetWidthMatrix()
//        {
//            uint max = GetMaxKnotDecrementForElement(0);
//            for (uint i = 1; i < AreaElems.Length; i++)
//            {
//                uint tmp = GetMaxKnotDecrementForElement(i);
//                if (max < tmp)
//                    max = tmp;
//            }
//            return max + 1;
//        }

//        public static explicit operator TriMesh(ComplecsMesh a)
//        {
//            TriMesh mesh = new TriMesh();
//            mesh.AreaElems = a.GetAreaElems();
//            mesh.BoundElems = a.GetBoundElems();
//            mesh.BoundKnots = a.BoundKnots;
//            mesh.BoundKnotsMark = a.BoundKnotsMark;
//            mesh.CoordsX = a.CoordsX;
//            mesh.CoordsY = a.CoordsY;
//            return mesh;
//        }
//        public void Print()
//        {
//            Console.WriteLine();
//            Console.WriteLine("CoordsX CoordsY");
//            for (int i = 0; i < CoordsY.Length; i++)
//            {
//                Console.WriteLine(" id {0} x {1} y {2}", i, CoordsX[i], CoordsY[i]);
//            }
//            Console.WriteLine();
//            Console.WriteLine("BoundKnots");
//            for (int i = 0; i < BoundKnots.Length; i++)
//            {
//                Console.WriteLine(" id {0} ", BoundKnots[i]);
//            }
//            Console.WriteLine();
//            Console.WriteLine("FE");
//            for (int i = 0; i < AreaElems.Length; i++)
//            {
//                int ID = i;
//                uint n0 = AreaElems[i][0];
//                uint n1 = AreaElems[i][1];
//                uint n2 = AreaElems[i][2];
//                Console.WriteLine(" id {0}: {1} {2} {3}", ID, n0, n1, n2);
//            }
//            Console.WriteLine();
//            Console.WriteLine("BFE");
//            for (int i = 0; i < AreaElems.Length; i++)
//            {
//                uint n0 = BoundElems[i][0];
//                uint n1 = BoundElems[i][1];
//                Console.WriteLine(" id {0}: {1} {2}", i, n0, n1);
//            }
//        }
//    }
//}