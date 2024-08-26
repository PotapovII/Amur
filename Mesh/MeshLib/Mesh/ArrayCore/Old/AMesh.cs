////---------------------------------------------------------------------------
////                          ПРОЕКТ  "МКЭ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 15.07.2022 Потапов И.И.
////---------------------------------------------------------------------------
//namespace MeshLib
//{
//    using CommonLib;
//    using MemLogLib;
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    /// <summary>
//    /// ОО: Абстрактный класс базисной конечно-элементной сетки общего назначений
//    /// </summary>
//    [Serializable]
//    public abstract class AMesh : IMesh
//    {
//        public int Dimention => dimention;
//        protected int dimention = 0;
//        /// <summary>
//        /// Количество узлов на основном КЭ 
//        /// </summary>
//        public TypeFunForm First => mFirst;
//        protected TypeFunForm mFirst = TypeFunForm.Form_1D_L1;
//        /// <summary>
//        /// Количество узлов на вспомогательном КЭ 
//        /// </summary>
//        public TypeFunForm Second => mSecond;
//        protected TypeFunForm mSecond = TypeFunForm.Form_1D_L1;
//        /// <summary>
//        /// Порядок сетки на которой работает функция формы
//        /// </summary>
//        public TypeRangeMesh typeRangeMesh => mTypeRangeMesh;
//        protected TypeRangeMesh mTypeRangeMesh = TypeRangeMesh.mRange1;
//        /// <summary>
//        /// Тип КЭ сетки в 1D и 2D
//        /// </summary>
//        public TypeMesh typeMesh => mTypeMesh;
//        protected TypeMesh mTypeMesh = TypeMesh.Line;
//        #region базисные массывы данных
//        /// <summary>
//        /// Координаты узлов
//        /// </summary>
//        protected double[][] points = null;
//        /// <summary>
//        /// Массив связности (узлы конечно - элементной сетки)
//        /// 1 строка (1 элемент столбца) это индекс типа - функции формы 
//        /// следующие элементы столбца - номера узлов 
//        /// хвост столбца (-1)  если сетка смешанная
//        /// </summary>
//        protected int[][] elems = null;
//        /// <summary>
//        /// Массив границ (количество строк == количеству узлов на граничном элементе)
//        /// </summary>
//        public int[][] boundary = null;
//        /// <summary>
//        /// Массив маркеров границ 
//        /// 1 строка - метка границы
//        /// 2 строка - индекс типа граничного условя
//        /// </summary>
//        public int[][] boundaryMark = null;
//        /// <summary>
//        /// Количество узлв на граничном элементе
//        /// </summary>
//        protected int BEKnotsCount;
//        /// <summary>
//        /// Тип граничных функций формы
//        /// </summary>
//        TypeFunForm typeBoundFunForm = TypeFunForm.Form_1D_L1;
//        /// <summary>
//        /// Координаты X, Y или Z для узловых точек 
//        /// </summary>
//        public double[] GetCoords(int dim) { return points[dim]; }
//        /// <summary>
//        /// Коррлинаты Х
//        /// </summary>
//        public double[] CoordsX { get => points[0]; set => points[0] = value; }
//        /// <summary>
//        /// Коррлинаты Y
//        /// </summary>
//        public double[] CoordsY { get => points[1]; set => points[1] = value; }
//        /// <summary>
//        /// Коррлинаты Z
//        /// </summary>
//        public double[] CoordsZ { get => points[2]; set => points[2] = value; }

//        #endregion
//        public AMesh() { }
//        public AMesh(AMesh mesh)
//        {
//            Set(mesh);
//        }
//        public AMesh(IMesh mesh)
//        {
//            Set(mesh);
//        }
//        public AMesh(double[][] _points, int[][] _elems, int[][] _boundary, int[][] _boundaryMark)
//        {
//            Set(_points, _elems, _boundary, _boundaryMark);
//        }
//        public void Set(double[][] _points, int[][] _elems, int[][] _boundary, int[][] _boundaryMark)
//        {
//            dimention = _points.Length;
//            MEM.MemCopy(ref points, _points);
//            MEM.MemCopy(ref elems, _elems);
//            MEM.MemCopy(ref boundary, _boundary);
//            MEM.MemCopy(ref boundaryMark, _boundaryMark);
//            int[] type = elems[0].Distinct().ToArray();
//            mFirst = (TypeFunForm)type[0];
//            mSecond = (TypeFunForm)(type.Length == 2 ? type[1] : type[0]);
//            int EKnotsCount;
//            FunFormHelp.GetFunFormInfo(mFirst, out EKnotsCount, out BEKnotsCount, out mTypeRangeMesh, out mTypeMesh);
//        }
//        public void Set(IMesh imesh)
//        {
//            AMesh mesh = imesh as AMesh;
//            Set(mesh.points, mesh.elems, mesh.boundary, mesh.boundaryMark);
//        }
//        /// <summary>
//        /// Клонирование объекта сетки
//        /// </summary>
//        public abstract IMesh Clone();
//        /// <summary>
//        /// Количество элементов
//        /// </summary>
//        public int CountElements
//        {
//            get { return elems[0] == null ? 0 : elems[0].Length; }
//        }
//        /// <summary>
//        /// Количество граничных элементов
//        /// </summary>
//        public int CountBoundElements
//        {
//            get { return boundary[0] == null ? 0 : boundary[0].Length; }
//        }
//        /// <summary>
//        /// Количество узлов
//        /// </summary>
//        public int CountKnots
//        {
//            get { return points[0] == null ? 0 : points[0].Length; }
//        }
//        /// <summary>
//        /// Количество граничных узлов
//        /// </summary>
//        public int CountBoundKnots
//        {
//            get { return boundary == null ? 0 : boundary.Length * (int)mTypeRangeMesh; }
//        }
//        /// <summary>
//        /// Диапазон координат для узлов сетки
//        /// </summary>
//        public void MinMax(int dim, ref double min, ref double max)
//        {
//            max = points[dim] == null ? double.MaxValue : points[dim].Max();
//            min = points[dim] == null ? double.MinValue : points[dim].Min();
//        }
//        /// <summary>
//        /// Получить массив уникальных маркеров границы
//        /// </summary>
//        /// <returns></returns>
//        public virtual int[] GetBoundMarkersList()
//        {
//            return boundaryMark[0].Distinct().ToArray();
//        }
//        /// <summary>
//        /// Получить выборку граничных элементов по типу ГУ
//        /// </summary>
//        /// <param name="id">тип ГУ</param>
//        /// <returns>массив ГЭ</returns>
//        public virtual uint[][] GetBoundElementsByMarker(int id)
//        {
//            uint[][] mass = null;
//            int count = boundaryMark[0].Count(x => x == id);
//            MEM.Alloc2D(count, BEKnotsCount, ref mass);
//            int j = 0;
//            for (int k = 0; k < CountBoundElements; k++)
//            {
//                if (boundaryMark[0][k] == id)
//                    for (int i = 0; i < BEKnotsCount; i++)
//                        mass[j][i] = (uint)boundary[i][k];
//            }
//            return mass;
//        }
//        /// <summary>
//        /// Получить узлы конечного элемента
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        public virtual TypeFunForm ElementKnots(uint i, ref uint[] knots)
//        {
//            TypeFunForm ff = (TypeFunForm)elems[0][i];
//            // TO DO заменть на массивы
//            int EKnotsCount = FunFormHelp.GetFEKnots(ff);
//            MEM.Alloc(EKnotsCount, ref knots);
//            for (int n = 0; n < EKnotsCount; n++)
//                knots[n] = (uint)elems[n + 1][i];
//            return ff;
//        }
//        /// <summary>
//        /// Получить узлы граничного элемента
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        public virtual TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
//        {
//            MEM.Alloc(BEKnotsCount, ref bknot);
//            for (int n = 0; n < BEKnotsCount; n++)
//                bknot[n] = (uint)boundary[n][i];
//            return typeBoundFunForm;
//        }
//        /// <summary>
//        /// Вычисление площади КЭ
//        /// </summary>
//        /// <param name="element">номер конечного элемента</param>
//        /// <returns></returns>
//        public double ElemSquare(TriElement element)
//        {
//            return (points[0][element.Vertex1] * (points[1][element.Vertex2] - points[1][element.Vertex3]) +
//                    points[0][element.Vertex2] * (points[1][element.Vertex3] - points[1][element.Vertex1]) +
//                    points[0][element.Vertex3] * (points[1][element.Vertex1] - points[1][element.Vertex2])) / 2.0;
//        }
//        /// <summary>
//        /// Получить тип граничных условий для граничного элемента
//        /// </summary>
//        /// <param name="elem">граничный элемент</param>
//        /// <returns>ID типа граничных условий</returns>
//        public int GetBoundElementMarker(uint elem)
//        {
//            return boundaryMark[0][elem];
//        }
//        /// <summary>
//        /// Ширина ленты в глобальной матрице жнсткости
//        /// </summary>
//        /// <returns></returns>
//        public uint GetWidthMatrix()
//        {
//            uint max = GetMaxKnotDecrementForElement(0);
//            for (uint i = 1; i < CountElements; i++)
//            {
//                uint tmp = GetMaxKnotDecrementForElement(i);
//                if (max < tmp)
//                    max = tmp;
//            }
//            return max + 1;
//        }

//        /// <summary>
//        /// Получить массив маркеров для граничных элементов 
//        /// </summary>
//        public int[] GetBElementsBCMark()
//        {
//            return boundaryMark[0];
//        }

//        #region VIRTUAL METH
//        /// <summary>
//        /// Получить массив типов границы для граничных элементов 
//        /// </summary>
//        public virtual TypeBoundCond[] GetBoundElementsType()
//        {
//            TypeBoundCond[] bc = null;
//            MEM.Alloc(boundaryMark.Length, ref bc);
//            for (int n = 0; n < boundaryMark.Length; n++)
//                bc[n] = (TypeBoundCond)boundaryMark[1][n];
//            return bc;
//        }
//        /// <summary>
//        /// Массив граничных узловых точек
//        /// </summary>
//        public virtual int[] GetBoundKnots()
//        {
//            int[] mas = null;
//            MEM.Alloc(boundary.Length * boundary[0].Length, ref mas);
//            int k = 0;
//            for (int i = 0; i < boundary.Length; i++)
//                for (int j = 0; j < boundary[0].Length; j++)
//                    mas[k++] = boundary[i][j];
//            int[] bks = mas.Distinct().ToArray();
//            return bks;
//        }
//        /// <summary>
//        /// Массив меток для граничных узловых точек
//        /// </summary>
//        public virtual int[] GetBoundKnotsMark()
//        {
//            List<int> bkList = new List<int>();
//            List<int> markList = new List<int>();
//            for (int i = 0; i < CountBoundElements; i++)
//            {
//                int mark = boundary[0][i];
//                for (int j = 0; j < BEKnotsCount; j++)
//                {
//                    int knot = boundary[j][i];
//                    if (bkList.Contains(knot) == false)
//                    {
//                        bkList.Add(knot);
//                        markList.Add(mark);
//                    }
//                }
//            }
//            return markList.ToArray();
//        }
//        /// <summary>
//        /// Массив типов граничных условий для граничных узловых точек
//        /// </summary>
//        public virtual TypeBoundCond[] GetBoundKnotsTypeBoundCond()
//        {
//            List<int> bkList = new List<int>();
//            List<TypeBoundCond> bcList = new List<TypeBoundCond>();
//            for (int i = 0; i < CountBoundElements; i++)
//            {
//                TypeBoundCond mark = (TypeBoundCond)boundary[1][i];
//                for (int j = 0; j < BEKnotsCount; j++)
//                {
//                    int knot = boundary[j][i];
//                    if (bkList.Contains(knot) == false)
//                    {
//                        bkList.Add(knot);
//                        bcList.Add(mark);
//                    }
//                }
//            }
//            return bcList.ToArray();
//        }
//        /// <summary>
//        /// Получить значения функции связанной с сеткой в вершинах КЭ
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>
//        public virtual void ElemValues(double[] Values, uint i, ref double[] elementValue)
//        {
//            TypeFunForm ff = (TypeFunForm)elems[0][i];
//            int EKnotsCount = FunFormHelp.GetFEKnots(ff);
//            MEM.Alloc(EKnotsCount, ref elementValue);
//            for (int n = 0; n < EKnotsCount; n++)
//                elementValue[n] = Values[elems[n + 1][i]];
//        }
//        /// <summary>
//        /// Получить максимальную разницу м/д номерами узнов на КЭ
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
//        public virtual uint GetMaxKnotDecrementForElement(uint i)
//        {
//            TypeFunForm ff = (TypeFunForm)elems[0][i];
//            int EKnotsCount = FunFormHelp.GetFEKnots(ff);
//            int min = elems[1][i];
//            int max = elems[1][i];
//            for (int n = 1; n < EKnotsCount; n++)
//            {
//                int m = elems[1 + n][i];
//                if (min > m)
//                    min = m;

//                if (max < m)
//                    max = m;
//            }
//            return (uint)(max - min);
//        }
//        /// <summary>
//        /// Вычисление длины кривой граничного КЭ
//        /// </summary>
//        /// <param name="belement">номер граничного конечного элемента</param>
//        /// <returns></returns>
//        public virtual double GetBoundElemLength(uint belement)
//        {
//            double Length = 0;
//            for (int i = 1; i < BEKnotsCount; i++)
//            {
//                int ia = boundary[i][belement];
//                int ib = boundary[i - 1][belement];
//                double dx = points[0][ia] - points[0][ib];
//                double dy = points[1][ia] - points[1][ib];
//                Length += Math.Sqrt(dx * dx + dy * dy);
//            }
//            return Length;
//        }
//        /// <summary>
//        /// Вычисление площади КЭ
//        /// </summary>
//        /// <param name="element">номер конечного элемента</param>
//        /// <returns></returns>
//        public virtual double ElemSquare(uint elem)
//        {
//            TriElement[] triElements = null;
//            GetTriElem((int)elem, ref triElements);
//            double S = 0;
//            for (int i = 0; i < triElements.Length; i++)
//                S += ElemSquare(triElements[i]);
//            return S;
//        }

//        /// <summary>
//        /// Тестовая печать КЭ сетки в консоль
//        /// </summary>
//        public virtual void Print()
//        {
//            LOG.Print("#points", points, 6);
//            LOG.Print("#elems", elems);
//            LOG.Print("#boundary", boundary);
//            LOG.Print("#boundaryMark", boundaryMark);
//        }
//        /// <summary>
//        /// Адаптер для отрисовки. 
//        /// Массив конечных элементов приводится массиву трехузловых конечных элементов 
//        /// </summary>
//        public virtual TriElement[] GetAreaElems()
//        {
//            TriElement[] triElements = null;
//            int CountTriFE = 0;
//            for (int i = 0; i < CountElements; i++)
//            {
//                TypeFunForm ff = (TypeFunForm)elems[0][i];
//                CountTriFE += FunFormHelp.GetCountTriElement(ff);
//            }
//            MEM.Alloc(CountTriFE, ref triElements);
//            int e = 0;
//            TriElement[] els = null;
//            for (int el = 0; el < CountElements; el++)
//            {
//                GetTriElem(el, ref els);
//                for (int j = 0; j < els.Length; j++)
//                    triElements[e++] = els[j];
//            }
//            return triElements;
//        }
//        /// <summary>
//        /// Массив конечных элементов на границе приводится массиву двухузловых конечных элементов 
//        /// </summary>
//        public virtual TwoElement[] GetBoundElems()
//        {
//            TwoElement[] BElements = null;
//            TwoElement[] bElements = null;
//            TypeFunForm ffe = (TypeFunForm)elems[0][0];
//            TypeFunForm ffb = FunFormHelp.GetBoundFunFormForGeometry(ffe);
//            int Count = FunFormHelp.GetBoundFEKnots(ffb);
//            int CountTriFE = FunFormHelp.GetBoundFEKnots(ffb) * CountBoundElements;
//            MEM.Alloc(CountTriFE, ref BElements);
//            int idx = 0;
//            for (int el = 0; el < CountBoundElements; el++)
//            {
//                GetBoundTwoElems(ffb, el, ref bElements);
//                for (int j = 0; j < Count; j++)
//                    BElements[idx++] = bElements[j];
//            }
//            return BElements;
//        }
//        /// <summary>
//        /// Адаптер для отрисовки. 
//        /// Массив конечных элементов приводится массиву трехузловых конечных элементов 
//        /// </summary>
//        protected void GetTriElem(int el, ref TriElement[] triElements)
//        {
//            int[] Knots = null;
//            TypeFunForm ff = (TypeFunForm)elems[0][el];
//            int CountTriFE = FunFormHelp.GetCountTriElement(ff);
//            int Count = FunFormHelp.GetFEKnots(ff);
//            MEM.Alloc(CountTriFE, ref triElements);
//            MEM.Alloc(Count, ref Knots);
//            for (int j = 0; j < Count; j++)
//                Knots[j] = elems[j + 1][el];
//            switch (ff)
//            {
//                case TypeFunForm.Form_2D_TriangleAnalitic_L1:
//                case TypeFunForm.Form_2D_Triangle_L0:
//                case TypeFunForm.Form_2D_Triangle_L1:
//                case TypeFunForm.Form_2D_Triangle_L1_River:
//                case TypeFunForm.Form_2D_Triangle_CR:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[2]);
//                    break;
//                case TypeFunForm.Form_2D_Triangle_L2:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[3], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[3]);
//                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[3], Knots[4]);
//                    break;
//                case TypeFunForm.Form_2D_Triangle_L3:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[8]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[9], Knots[8]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[9]);
//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[4], Knots[9]);
//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[3], Knots[4]);

//                    triElements[CountTriFE++] = new TriElement(Knots[8], Knots[9], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[9], Knots[5], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[9], Knots[4], Knots[5]);

//                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[5], Knots[6]);
//                    break;
//                case TypeFunForm.Form_2D_Rectangle_L0:
//                case TypeFunForm.Form_2D_Rectangle_L1:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[2]);
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[2], Knots[3]);
//                    break;
//                case TypeFunForm.Form_2D_Rectangle_S2:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[3]);
//                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[4], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[6], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[1], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[3], Knots[5]);
//                    break;
//                case TypeFunForm.Form_2D_Rectangle_S3:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[11]);
//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[3], Knots[4]);
//                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[6], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[8], Knots[9], Knots[10]);

//                    triElements[CountTriFE++] = new TriElement(Knots[11], Knots[1], Knots[10]);
//                    triElements[CountTriFE++] = new TriElement(Knots[10], Knots[1], Knots[8]);

//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[7], Knots[8]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[7]);

//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[4], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[5], Knots[7]);
//                    break;
//                case TypeFunForm.Form_2D_Rectangle_L2:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[8], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[3]);
//                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[8], Knots[1]);
//                    triElements[CountTriFE++] = new TriElement(Knots[8], Knots[3], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[4], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[8], Knots[5]);
//                    triElements[CountTriFE++] = new TriElement(Knots[7], Knots[5], Knots[6]);
//                    break;
//                case TypeFunForm.Form_2D_Rectangle_L3:
//                    triElements[CountTriFE++] = new TriElement(Knots[0], Knots[1], Knots[11]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[12], Knots[11]);
//                    triElements[CountTriFE++] = new TriElement(Knots[1], Knots[2], Knots[12]);
//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[13], Knots[12]);
//                    triElements[CountTriFE++] = new TriElement(Knots[2], Knots[3], Knots[13]);
//                    triElements[CountTriFE++] = new TriElement(Knots[3], Knots[4], Knots[13]);

//                    triElements[CountTriFE++] = new TriElement(Knots[11], Knots[12], Knots[10]);
//                    triElements[CountTriFE++] = new TriElement(Knots[12], Knots[15], Knots[10]);
//                    triElements[CountTriFE++] = new TriElement(Knots[12], Knots[13], Knots[14]);
//                    triElements[CountTriFE++] = new TriElement(Knots[12], Knots[14], Knots[15]);
//                    triElements[CountTriFE++] = new TriElement(Knots[13], Knots[4], Knots[14]);
//                    triElements[CountTriFE++] = new TriElement(Knots[4], Knots[5], Knots[14]);

//                    triElements[CountTriFE++] = new TriElement(Knots[10], Knots[15], Knots[9]);
//                    triElements[CountTriFE++] = new TriElement(Knots[15], Knots[8], Knots[9]);
//                    triElements[CountTriFE++] = new TriElement(Knots[15], Knots[14], Knots[8]);
//                    triElements[CountTriFE++] = new TriElement(Knots[14], Knots[7], Knots[8]);
//                    triElements[CountTriFE++] = new TriElement(Knots[14], Knots[5], Knots[7]);
//                    triElements[CountTriFE++] = new TriElement(Knots[5], Knots[6], Knots[7]);
//                    break;
//                default:
//                    throw new Exception("Не поддерживается!");
//            }
//        }
//        protected void GetBoundTwoElems(TypeFunForm ffb, int el, ref TwoElement[] BElements)
//        {
//            int Count = FunFormHelp.GetBoundFEKnots(ffb);
//            MEM.Alloc(Count, ref BElements);
//            switch (ffb)
//            {
//                case TypeFunForm.Form_1D_L1:
//                    {
//                        BElements[0] = new TwoElement((uint)boundary[0][el], (uint)boundary[1][el]);
//                        return;
//                    }
//                case TypeFunForm.Form_1D_L2:
//                    {
//                        BElements[0] = new TwoElement((uint)boundary[0][el], (uint)boundary[1][el]);
//                        BElements[1] = new TwoElement((uint)boundary[1][el], (uint)boundary[2][el]);
//                        return;
//                    }
//                case TypeFunForm.Form_1D_L3:
//                    {
//                        BElements[0] = new TwoElement((uint)boundary[0][el], (uint)boundary[1][el]);
//                        BElements[1] = new TwoElement((uint)boundary[1][el], (uint)boundary[2][el]);
//                        BElements[2] = new TwoElement((uint)boundary[2][el], (uint)boundary[3][el]);
//                        return;
//                    }
//                default:
//                    throw new Exception("Не поддерживается!");
//            }
//        }
       
//        #endregion
//        /// <summary>
//        /// Получить координаты Х вершин КЭ
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        /// <returns>Координаты Х вершин КЭ</returns>
//        public abstract void ElemX(uint i, ref double[] X);
//        /// <summary>
//        /// Получить координаты Y вершин КЭ
//        /// </summary>
//        /// <param name="i">номер элемента</param>
//        /// <returns>Координаты Y вершин КЭ</returns>
//        public abstract void ElemY(uint i, ref double[] Y);
//        /// <summary>
//        /// Получить выборку граничных узлов по типу ГУ
//        /// </summary>
//        /// <param name="i">тип ГУ</param>
//        /// <returns></returns>
//        public abstract uint[] GetBoundKnotsByMarker(int i);
//    }
//}