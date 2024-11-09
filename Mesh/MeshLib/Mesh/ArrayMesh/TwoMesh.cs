//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 02.01.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                  Иерархия : 17.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
//      Убраны массивы ГУ из всей иерархи сеток : 01.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    /// <summary>
    /// ОО: двухузловая конечно-элементная сетка 
    /// </summary>
    [Serializable]
    public class TwoMesh : ArrayMesh
    {
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TwoElement[] AreaElems;
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoMesh(uint N, double L = 1) : base()
        {
            double dx = L / (N - 1);
            MEM.Alloc(N, ref CoordsX);
            MEM.Alloc(N, ref CoordsY);
            MEM.Alloc(N - 1, ref AreaElems);
            MEM.Alloc(2, ref BoundKnots);
            MEM.Alloc(2, ref BoundKnotsMark);
            for (int i = 0; i < N; i++)
            {
                CoordsX[i] = dx * i;
            }
            for (uint i = 0; i < N - 1; i++)
                AreaElems[i] = new TwoElement(i, i + 1);
            BoundKnots[0] = 0;
            BoundKnots[1] = (int)N - 1;
            BoundKnotsMark[0] = 0;
            BoundKnotsMark[1] = 1;
        }

        public TwoMesh(double[] x, double[] y = null) : base()
        {
            if (x == null) return;
            if (x.Length == 0) return;
            MEM.Alloc(x.Length, ref CoordsX);
            MEM.Alloc(x.Length, ref CoordsY);
            MEM.Alloc(x.Length - 1, ref AreaElems);
            MEM.Alloc(2, ref BoundKnots);
            MEM.Alloc(2, ref BoundKnotsMark);
            for (int i = 0; i < x.Length; i++)
                CoordsX[i] = x[i];
            if (y != null)
                for (int i = 0; i < x.Length; i++)
                    CoordsY[i] = y[i];
            for (uint i = 0; i < x.Length - 1; i++)
                AreaElems[i] = new TwoElement(i, i + 1);
            BoundKnots[0] = 0;
            BoundKnots[1] = x.Length - 1;
            BoundKnotsMark[0] = 0;
            BoundKnotsMark[1] = 1;
            BoundElementsMark = new int[2];
            BoundElementsMark[0] = 0;
            BoundElementsMark[1] = 1;
        }
        public TwoMesh(TwoMesh m):base(m)
        {
            MEM.MemCopy(ref AreaElems, m.AreaElems);
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public override IMesh Clone()
        {
            return new TwoMesh(this);
        }
        public override int CountElements
        {
            get { return AreaElems == null ? 0 : AreaElems.Length; }
        }
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        public override int CountBoundElements => 0;
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public override TriElement[] GetAreaElems() { return null; }
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public override TwoElement[] GetBoundElems() { return null; }

        public override TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            knots[0] = AreaElems[i].Vertex1;
            knots[1] = AreaElems[i].Vertex2;
            return TypeFunForm.Form_1D_L1;
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public override TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            bknot = null;
            return TypeFunForm.Form_1D_L0;
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>

        public override void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            MEM.Alloc<double>(2, ref X, "X");
            MEM.Alloc<double>(2, ref Y, "Y");
            X[0] = CoordsX[AreaElems[i].Vertex1];
            X[1] = CoordsX[AreaElems[i].Vertex2];
            Y[0] = CoordsY[AreaElems[i].Vertex1];
            Y[1] = CoordsY[AreaElems[i].Vertex2];
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
        }
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        public override uint GetMaxKnotDecrementForElement(uint i)
        {
            return (uint)Math.Abs(AreaElems[i].Vertex2 - AreaElems[i].Vertex1);
        }
        public override double ElemSquare(uint elem)
        {
            double a = CoordsX[AreaElems[elem].Vertex1] - CoordsX[AreaElems[elem].Vertex2];
            double b = CoordsY[AreaElems[elem].Vertex1] - CoordsY[AreaElems[elem].Vertex2];
            double S = Math.Sqrt(a * a + b * b);
            return S;
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
                uint n0 = AreaElems[i].Vertex1;
                uint n1 = AreaElems[i].Vertex2;
                Console.WriteLine(" id {0}: {1} {2}", ID, n0, n1);
            }
        }
    }
}