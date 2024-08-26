//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//---------------------------------------------------------------------------
//                 кодировка : 15.10.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Linq;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: ARenderMesh - класс адаптер конечно элементной сетки
    /// </summary>
    [Serializable]
    public class RenderMesh: IRenderMesh
    {
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TriElement[] AreaElems;
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoElement[] BoundElems;
        /// <summary>
        /// Метки границы
        /// </summary>
        public int[] BoundElementsMark;
        /// <summary>
        /// Вектор граничных узловых точек
        /// </summary>
        public int[] BoundKnots;
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] BoundKnotsMark;
        /// <summary>
        /// Координаты узловых точек и параметров определенных в них
        /// </summary>
        public double[] CoordsX;
        public double[] CoordsY;

        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        public TypeRangeMesh tRangeMesh => TypeRangeMesh.mRange1;
        
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public int[] GetBElementsBCMark()
        {
            return BoundElementsMark;
        }
        /// <summary>
        /// Получить тип граничных условий для граничного элемента
        /// </summary>
        /// <param name="elem">граничный элемент</param>
        /// <returns>ID типа граничных условий</returns>
        public virtual int GetBoundElementMarker(uint elem)
        {
            return BoundElementsMark[elem];
        }
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public int[] GetBoundKnots() { return BoundKnots; }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] GetBoundKnotsMark() { return BoundKnotsMark; }
        public RenderMesh(){}
        public RenderMesh(RenderMesh m)
        {
            MEM.MemCopy(ref AreaElems, m.AreaElems);
            MEM.MemCopy(ref BoundKnots, m.BoundKnots);
            MEM.MemCopy(ref BoundElems, m.BoundElems);

            MEM.MemCopy(ref BoundKnotsMark, m.BoundKnotsMark);
            MEM.MemCopy(ref BoundElementsMark, m.BoundElementsMark);

            MEM.MemCopy(ref CoordsX, m.CoordsX);
            MEM.MemCopy(ref CoordsY, m.CoordsY);
        }
        public RenderMesh(TriMesh m)
        {
            MEM.MemCopy(ref AreaElems, m.AreaElems);
            MEM.MemCopy(ref BoundKnots, m.BoundKnots);
            MEM.MemCopy(ref BoundElems, m.BoundElems);
            MEM.MemCopy(ref BoundKnotsMark, m.BoundKnotsMark);
            MEM.MemCopy(ref CoordsX, m.CoordsX);
            MEM.MemCopy(ref CoordsY, m.CoordsY);
        }
        public RenderMesh(IMesh m)
        {
            MEM.MemCopy(ref AreaElems, m.GetAreaElems());
            MEM.MemCopy(ref BoundKnots, m.GetBoundKnots());
            MEM.MemCopy(ref BoundElems, m.GetBoundElems());
            MEM.MemCopy(ref BoundKnotsMark, m.GetBoundKnotsMark());
            MEM.MemCopy(ref CoordsX, m.GetCoords(0));
            MEM.MemCopy(ref CoordsY, m.GetCoords(1));
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
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public virtual TypeFunForm ElementKnots(uint i, ref uint[] knots)
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
        public virtual TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            bknot[0] = BoundElems[i].Vertex1;
            bknot[1] = BoundElems[i].Vertex2;
            return TypeFunForm.Form_1D_L1;
        }
        /// <summary>
        /// Адаптер для отрисовки. 
        /// Массив конечных элементов приводится массиву трехузловых конечных элементов 
        /// </summary>
        public virtual TriElement[] GetAreaElems()
        {
            uint[] Knots = {0, 0, 0};
            TriElement[] triElements = null;
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
            return triElements;
        }
        /// <summary>
        /// Массив конечных элементов на границе приводится массиву двухузловых конечных элементов 
        /// </summary>
        public virtual TwoElement[] GetBoundElems()
        {
            TwoElement[] BElements = null;
            TwoElement[] bElements = null;
            uint[] Knots = {0, 0, 0};
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
        public virtual void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            uint[] Knots = null;
            ElementKnots((uint)i, ref Knots);
            for (int j = 0; j < Knots.Length; j++)
                elementValue[j] = Values[Knots[j]];
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
        /// Клонирование объекта сетки
        /// </summary>
        public IMesh Clone()
        {
            return new TriMesh(this);
        }
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
            Console.WriteLine("FE");
            for (int i = 0; i < AreaElems.Length; i++)
            {
                int ID = i;
                uint n0 = AreaElems[i][0];
                uint n1 = AreaElems[i][1];
                uint n2 = AreaElems[i][2];
                Console.WriteLine(" id {0}: {1} {2} {3}", ID, n0, n1, n2);
            }
            Console.WriteLine();
            Console.WriteLine("BFE");
            for (int i = 0; i < AreaElems.Length; i++)
            {
                uint n0 = BoundElems[i][0];
                uint n1 = BoundElems[i][1];
                Console.WriteLine(" id {0}: {1} {2}", i, n0, n1);
            }
        }
        /// <summary>
        /// Вычисление длины ГКЭ
        /// </summary>
        public virtual double ElemSquare(TwoElement element)
        {
            double dx = CoordsX[element.Vertex1] - CoordsX[element.Vertex2];
            double dy = CoordsY[element.Vertex1] - CoordsY[element.Vertex2];
            return Math.Sqrt(dx * dx + dy * dy);
        }
        /// <summary>
        /// Вычисление площади расчетной области 
        /// </summary>
        /// <param name="Area">площадь</param>
        public double GetSquareArea()
        {
            double Area = 0;
            TriElement[] elems = GetAreaElems();
            for (uint elem = 0; elem < elems.Length; elem++)
            {
                TriElement e = elems[elem];
                double S = Math.Abs(ElemSquare(e));
                Area += S;
            }
            return Area;
        }
        /// <summary>
        /// Вычисление cмоченного периметра
        /// </summary>
        /// <param name="waterLevelMark">маркер свободной поверхности</param>
        /// <returns>Смоченный периметр</returns>
        public double GetLengthWetBoundary(int waterLevelMark = -1)
        {
            double WetLength = 0;
            TwoElement[] elems = GetBoundElems();
            for (uint elem = 0; elem < elems.Length; elem++)
            {
                var mark = GetBoundElementMarker(elem);
                if (mark != waterLevelMark)
                {
                    TwoElement e = elems[elem];
                    double S = ElemSquare(e);
                    WetLength += S;
                }
            }
            return WetLength;
        }
        /// <summary>
        /// Вычисление площади расчетной области и интеграла поля по площади
        /// </summary>
        /// <param name="pole"></param>
        /// <param name="Area"></param>
        /// <param name="Sum"></param>
        public void GetAreaAndRate(double[] pole, ref double Area, ref double Sum)
        {
            Sum = 0;
            Area = 0;
            TriElement[] elems = GetAreaElems();
            for (uint elem = 0; elem < elems.Length; elem++)
            {
                TriElement e = elems[elem];
                double S = Math.Abs(ElemSquare(e));
                Area += S;
                Sum += (pole[e.Vertex1] + pole[e.Vertex2] + pole[e.Vertex3]) * S / 3;
            }
        }
    }
}
