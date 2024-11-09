//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?V"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.10.2000 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.03.2001 Потапов И.И. (с++)
//                         ПРОЕКТ  "DISER"
//                 правка  :   9.03.2002 Потапов И.И. (с++)
//                         ПРОЕКТ  "RiverLib"
//                 правка  :   06.12.2020 Потапов И.И. (с++ => c#)
//                       HBaseMesh => ComplecsMesh
//---------------------------------------------------------------------------
//      Убраны массивы ГУ из всей иерархи сеток : 01.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Произвольная базисная сетка задачи с массивной концепцией хранения данных
    /// </summary>
    [Serializable]
    public abstract class ArrayMesh : IMesh
    {
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        public TypeRangeMesh tRangeMesh;
        public TypeRangeMesh typeRangeMesh { get => tRangeMesh; }
        /// <summary>
        /// Тип КЭ сетки в 1D и 2D
        /// </summary>
        public TypeMesh tMesh;
        public TypeMesh typeMesh { get => tMesh; }
       
        #region Граничные элементы
        /// <summary>
        /// Метки границы
        /// </summary>
        public int[] BoundElementsMark;
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public int[] GetBElementsBCMark()
        {
            return BoundElementsMark;
        }
        #endregion
        #region Граничные узлы
        /// <summary>
        /// Вектор граничных узловых точек
        /// </summary>
        public int[] BoundKnots;
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] BoundKnotsMark;
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public int[] GetBoundKnots() { return BoundKnots; }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] GetBoundKnotsMark() { return BoundKnotsMark; }
        #endregion
        /// <summary>
        /// Координаты узловых точек и параметров определенных в них
        /// </summary>
        public double[] CoordsX;
        public double[] CoordsY;
        public ArrayMesh()
        {
            this.tRangeMesh = TypeRangeMesh.mRange1;
            this.tMesh = TypeMesh.MixMesh;
        }
        public ArrayMesh(ArrayMesh m)
        {
            this.tRangeMesh = m.tRangeMesh;
            this.tMesh = m.tMesh;

            MEM.MemCopy(ref CoordsX, m.CoordsX);
            MEM.MemCopy(ref CoordsY, m.CoordsY);

            MEM.MemCopy(ref BoundKnots, m.BoundKnots);

            MEM.MemCopy(ref BoundElementsMark, m.BoundElementsMark);
            
            MEM.MemCopy(ref BoundKnotsMark, m.BoundKnotsMark);
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public abstract IMesh Clone();
        abstract public int CountElements { get; }
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        abstract public int CountBoundElements { get; }

        public int CountKnots
        {
            get { return CoordsX == null ? 0 : CoordsX.Length; }
        }
        public int CountBoundKnots
        {
            get { return BoundKnots == null ? 0 : BoundKnots.Length; }
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

        abstract public TypeFunForm ElementKnots(uint i, ref uint[] knots);

        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        abstract public TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot);

        /// <summary>
        /// Адаптер для отрисовки. 
        /// Массив конечных элементов приводится массиву трехузловых конечных элементов 
        /// </summary>
        public virtual TriElement[] GetAreaElems()
        {
            TriElement[] triElements = null;
            try
            {
                uint[] Knots = null;
            
                int CountTriFE = 0;
                for (uint i = 0; i < CountElements; i++)
                {
                    TypeFunForm ff = ElementKnots(i, ref Knots);
                    CountTriFE += FunFormHelp.GetCountTriElement(ff);
                }
                MEM.Alloc(CountTriFE, ref triElements);
                int e = 0;
                TriElement[] els = null;
                for (int el = 0; el < CountElements; el++)
                {
                    TypeFunForm ff = ElementKnots((uint)el, ref Knots);
                    FunFormHelp.GetTriElem(ff, Knots, ref els);
                    for (int j = 0; j < els.Length; j++)
                        triElements[e++] = els[j];
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("ОШИБКА: конвертации в методе ФккфнЬуыр.GetAreaElems");
            }
            return triElements;
        }
        /// <summary>
        /// Массив конечных элементов на границе приводится массиву двухузловых конечных элементов 
        /// </summary>
        public virtual TwoElement[] GetBoundElems()
        {
            TwoElement[] BElements = null;
            TwoElement[] bElements = null;
            uint[] Knots = null;
            TypeFunForm ffe = ElementKnots(0, ref Knots);
            TypeFunForm ffb = FunFormHelp.GetBoundFunFormForGeometry(ffe);
            int Count = FunFormHelp.GetBoundFEKnots(ffb);
            int CountTwoElems = FunFormHelp.GetBoundFEKnots(ffb) * CountBoundElements;
            MEM.Alloc(CountTwoElems, ref BElements);
            int idx = 0;
            for (uint el = 0; el < CountBoundElements; el++)
            {
                ffb = ElementBoundKnots(el, ref Knots);
                FunFormHelp.GetBoundTwoElems(ffb, Knots, ref bElements);
                for (int j = 0; j < Count; j++)
                    BElements[idx++] = bElements[j];
            }
            return BElements;
        }
       
        /// <summary>
        /// Координаты X или Y для узловых точек 
        /// </summary>
        public double[] GetCoords(int dim)
        {
            if (dim == 0)
                return CoordsX;
            else
                return CoordsY;
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>
        public virtual void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            uint[] Knots = null;
            TypeFunForm ff = ElementKnots((uint)i, ref Knots);
            MEM.Alloc<double>(Knots.Length, ref X, "X");
            MEM.Alloc<double>(Knots.Length, ref Y, "Y");
            for (int n = 0; n < Knots.Length; n++)
            {
                X[n] = CoordsX[Knots[n]];
                Y[n] = CoordsY[Knots[n]];
            }
        }
        public virtual void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            uint[] Knots = null;
            TypeFunForm ff = ElementKnots((uint)i, ref Knots);
            for (int j = 0; j < Knots.Length; j++)
                elementValue[j] = Values[Knots[j]];
        }
        public virtual uint GetMaxKnotDecrementForElement(uint i)
        {
            uint[] Knots = null;
            TypeFunForm ff = ElementKnots((uint)i, ref Knots);
            uint min = Knots[0];
            uint max = Knots[0];
            for (int j = 1; j < Knots.Length; j++)
            {
                if (min > Knots[j])
                    min = Knots[j];

                if (max < Knots[j])
                    max = Knots[j];
            }
            return max - min;
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public virtual double ElemSquare(TriElement element)
        {
            return (CoordsX[element.Vertex1] * (CoordsY[element.Vertex2] - CoordsY[element.Vertex3]) +
                    CoordsX[element.Vertex2] * (CoordsY[element.Vertex3] - CoordsY[element.Vertex1]) +
                    CoordsX[element.Vertex3] * (CoordsY[element.Vertex1] - CoordsY[element.Vertex2])) / 2.0;
        }
        /// <summary>
        ///  Вычисление площади КЭ первого порядка
        /// </summary>
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        /// <param name="element">номер конечного элемента</param>
        /// <returns></returns>
        public virtual double ElemSquare(uint elem)
        {
            uint[] Knots = null;
            TriElement[] triElements = null;
            TypeFunForm ff = ElementKnots(0, ref Knots);
            FunFormHelp.GetTriElem(ff, Knots, ref triElements);
            double S = 0;
            for (int i = 0; i < triElements.Length; i++)
                S += ElemSquare(triElements[i]);
            return S;
        }
        /// <summary>
        /// Вычисление длины кривой граничного КЭ
        /// </summary>
        /// <param name="belement">номер граничного конечного элемента</param>
        /// <returns></returns>
        public virtual double GetBoundElemLength(uint belement)
        {
            double Length = 0;
            uint[] Knots = null;
            TypeFunForm ffe = ElementBoundKnots(belement, ref Knots);
            for (int i = 1; i < Knots.Length; i++)
            {
                uint ia = Knots[i];
                uint ib = Knots[i - 1];
                double dx = CoordsX[ia] - CoordsX[ib];
                double dy = CoordsY[ia] - CoordsY[ib];
                Length += Math.Sqrt(dx * dx + dy * dy);
            }
            return Length;
        }
        /// <summary>
        /// Получить выборку граничных узлов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        public uint[] GetBoundKnotsByMarker(int i)
        {
            int count = 0;
            for (int k = 0; k < BoundKnots.Length; k++)
            {
                if (BoundKnotsMark[k] == i)
                    ++count;
            }
            uint[] mass = new uint[count];

            int j = 0;
            for (int k = 0; k < BoundKnots.Length; k++)
            {
                if (BoundKnotsMark[k] == i)
                    mass[j++] = (uint)BoundKnots[k];
            }
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
            for (int k = 0; k < BoundElementsMark.Length; k++)
            {
                if (BoundElementsMark[k] == id)
                    ++count;
            }
            uint[][] mass = new uint[count][];
            int j = 0;
            for (uint k = 0; k < CountBoundElements; k++)
            {
                if (BoundElementsMark[k] == id)
                {
                    uint[] Knots = null;
                    TypeFunForm ffe = ElementBoundKnots(k, ref Knots);
                    mass[j] = new uint[Knots.Length];
                    for (int i = 0; i < Knots.Length; i++)
                        mass[j][i] = Knots[i];
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
            return BoundElementsMark[elem];
        }
        /// <summary>
        /// Ширина ленты в глобальной матрице жнсткости
        /// </summary>
        /// <returns></returns>
        abstract public uint GetWidthMatrix();



        public virtual void Print()
        {
            Console.WriteLine();
            Console.WriteLine("CoordsX CoordsY");
            for (int i = 0; i < CoordsY.Length; i++)
            {
                Console.WriteLine(" id {0} x {1} y {2}", i, CoordsX[i], CoordsY[i]);
            }
            Console.WriteLine();
            Console.WriteLine("BoundKnots");
            for (int i = 0; i < BoundKnots.Length; i++)
            {
                Console.WriteLine(" id {0} ", BoundKnots[i]);
            }
            Console.WriteLine();
            // Console.WriteLine("FE");
            //for (int i = 0; i < AreaElems.Length; i++)
            //{
            //    int ID = i;
            //    uint n0 = AreaElems[i][0];
            //    uint n1 = AreaElems[i][1];
            //    uint n2 = AreaElems[i][2];
            //    Console.WriteLine(" id {0}: {1} {2} {3}", ID, n0, n1, n2);
            //}
            //Console.WriteLine();
            //Console.WriteLine("BFE");
            //for (int i = 0; i < AreaElems.Length; i++)
            //{
            //    uint n0 = BoundElems[i][0];
            //    uint n1 = BoundElems[i][1];
            //    Console.WriteLine(" id {0}: {1} {2}", i, n0, n1);
            //}
        }
    }
}