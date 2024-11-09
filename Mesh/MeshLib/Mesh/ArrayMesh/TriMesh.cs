//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                  Иерархия : 17.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
//      Убраны массивы ГУ из всей иерархи сеток : 01.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using System;
    using System.Linq;
    using MemLogLib;
    //---------------------------------------------------------------------------
    //  ОО: TriMesh - базистная техузловая конечно-элементная сетка 
    //---------------------------------------------------------------------------
    [Serializable]
    public class TriMesh : ArrayMesh
    {
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TriElement[] AreaElems;
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoElement[] BoundElems;
        public TriMesh():base() { }
        public TriMesh(TriMesh m):base(m)
        {
            MEM.MemCopy(ref AreaElems, m.AreaElems);
            MEM.MemCopy(ref BoundElems, m.BoundElems);
        }
        public TriMesh(RenderMesh m) 
        {
            this.tRangeMesh = m.tRangeMesh;
            this.tMesh = TypeMesh.Triangle;
            MEM.MemCopy(ref CoordsX, m.CoordsX);
            MEM.MemCopy(ref CoordsY, m.CoordsY);
            MEM.MemCopy(ref AreaElems, m.AreaElems);
            MEM.MemCopy(ref BoundElems, m.BoundElems);
            MEM.MemCopy(ref BoundKnots, m.BoundKnots);
            MEM.MemCopy(ref BoundElementsMark, m.BoundElementsMark);
            MEM.MemCopy(ref BoundKnotsMark, m.BoundKnotsMark);
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public override IMesh Clone()
        {
            return new TriMesh(this);
        }
        /// <summary>
        /// Количество элементов
        /// </summary>
        public override int CountElements
        {
            get { return AreaElems == null ? 0 : AreaElems.Length; }
        }
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        public override int CountBoundElements
        {
            get { return BoundElems == null ? 0 : BoundElems.Length; }
        }
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public override TriElement[] GetAreaElems() { return AreaElems; }
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public override TwoElement[] GetBoundElems() { return BoundElems; }
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public override TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            if(knots == null)
                knots = new uint[3] { AreaElems[i].Vertex1, AreaElems[i].Vertex2, AreaElems[i].Vertex3 };
            knots[0] = AreaElems[i].Vertex1;
            knots[1] = AreaElems[i].Vertex2;
            knots[2] = AreaElems[i].Vertex3;
            
            return TypeFunForm.Form_2D_Triangle_L1;
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public override TypeFunForm ElementBoundKnots(uint i, ref uint[] bknots)
        {
            if (bknots == null)
                bknots = new uint[2] { AreaElems[i].Vertex1, AreaElems[i].Vertex2 };
            bknots[0] = BoundElems[i].Vertex1;
            bknots[1] = BoundElems[i].Vertex2;
            return TypeFunForm.Form_1D_L1;
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>

        public override void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            MEM.Alloc<double>(3, ref X, "X");
            MEM.Alloc<double>(3, ref Y, "Y");
            X[0] = CoordsX[AreaElems[i].Vertex1];
            X[1] = CoordsX[AreaElems[i].Vertex2];
            X[2] = CoordsX[AreaElems[i].Vertex3];
            Y[0] = CoordsY[AreaElems[i].Vertex1];
            Y[1] = CoordsY[AreaElems[i].Vertex2];
            Y[2] = CoordsY[AreaElems[i].Vertex3];
        }
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>
        public override void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            elementValue[0] = Values[AreaElems[i].Vertex1];
            elementValue[1] = Values[AreaElems[i].Vertex2];
            elementValue[2] = Values[AreaElems[i].Vertex3];
        }
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        public override uint GetMaxKnotDecrementForElement(uint i)
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
        public override double ElemSquare(uint elem)
        {
            TriElement knot = AreaElems[elem];
            double S = (CoordsX[knot.Vertex1] * (CoordsY[knot.Vertex2] - CoordsY[knot.Vertex3]) +
                        CoordsX[knot.Vertex2] * (CoordsY[knot.Vertex3] - CoordsY[knot.Vertex1]) +
                        CoordsX[knot.Vertex3] * (CoordsY[knot.Vertex1] - CoordsY[knot.Vertex2])) / 2.0;
            return S;
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public override double ElemSquare(TriElement element)
        {
            return (CoordsX[element.Vertex1] * (CoordsY[element.Vertex2] - CoordsY[element.Vertex3]) +
                    CoordsX[element.Vertex2] * (CoordsY[element.Vertex3] - CoordsY[element.Vertex1]) +
                    CoordsX[element.Vertex3] * (CoordsY[element.Vertex1] - CoordsY[element.Vertex2])) / 2.0;
        }
        /// <summary>
        /// Вычисление длины граничного КЭ
        /// </summary>
        /// <param name="belement">номер граничного конечного элемента</param>
        /// <returns></returns>
        public override double GetBoundElemLength(uint belement)
        {
            TwoElement knot = BoundElems[belement];
            double a = CoordsX[knot.Vertex1] - CoordsX[knot.Vertex2];
            double b = CoordsY[knot.Vertex1] - CoordsY[knot.Vertex2];
            double Length = Math.Sqrt(a * a + b * b);
            return Length;
        }
        /// <summary>
        /// Ширина ленты в глобальной матрице жнсткости
        /// </summary>
        /// <returns></returns>
        public override uint GetWidthMatrix()
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
        public override void Print()
        {
            base.Print();
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
                int fl = BoundElementsMark[i];
                Console.WriteLine(" id {0}: {1} {2} fl {3}", i, n0, n1, fl);
            }
        }
    }
}