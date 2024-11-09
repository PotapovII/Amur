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
    /// ОО: Произвольная базисная сетка задачи с массивной концепцией хранения данных
    /// </summary>
    [Serializable]
    public class ComplecsMesh : ArrayMesh
    {
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public uint[][] AreaElems;
        /// <summary>
        /// Тип конечных элементов в области
        /// </summary>
        public TypeFunForm[] AreaElemsFFType;
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public uint[][] BoundElems;

        public ComplecsMesh():base(){}
        public ComplecsMesh(ComplecsMesh m):base(m)
        {
            MEM.MemCopy(ref AreaElems, m.AreaElems);
            MEM.MemCopy(ref AreaElemsFFType, m.AreaElemsFFType);
            MEM.MemCopy(ref BoundElems, m.BoundElems);
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public override IMesh Clone()
        {
            return new ComplecsMesh(this);
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
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public override TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            knots = AreaElems[i];
            return AreaElemsFFType[i];
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public override TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            bknot = BoundElems[i];
            return (TypeFunForm)(bknot.Length - 1);
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>

        public override void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            MEM.Alloc<double>(AreaElems[i].Length, ref X, "X");
            MEM.Alloc<double>(AreaElems[i].Length, ref Y, "Y");
            for (int n = 0; n < AreaElems[i].Length; n++)
            {
                X[n] = CoordsX[AreaElems[i][n]];
                Y[n] = CoordsY[AreaElems[i][n]];
            }
        }
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>

        public override void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            for (int j = 0; j < AreaElems[i].Length; j++)
                elementValue[j] = Values[AreaElems[i][j]];
        }
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        public override uint GetMaxKnotDecrementForElement(uint i)
        {
            uint min = AreaElems[i][0];
            uint max = AreaElems[i][0];
            for (int j = 1; j < AreaElems[i].Length; j++)
            {
                if (min > AreaElems[i][j])
                    min = AreaElems[i][j];

                if (max < AreaElems[i][j])
                    max = AreaElems[i][j];
            }
            return max - min;
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

        public static explicit operator TriMesh(ComplecsMesh a)
        {
            TriMesh mesh = new TriMesh();
            mesh.AreaElems = a.GetAreaElems();
            mesh.BoundElems = a.GetBoundElems();
            mesh.BoundKnots = a.BoundKnots;
            mesh.BoundKnotsMark = a.BoundKnotsMark;
            mesh.BoundElementsMark = a.BoundElementsMark;
            mesh.CoordsX = a.CoordsX;
            mesh.CoordsY = a.CoordsY;
            return mesh;
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
    }
}