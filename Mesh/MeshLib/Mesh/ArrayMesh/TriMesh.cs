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
    using System.Collections.Generic;

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
        /// <summary>
        /// Установка новой сетки в текущую
        /// </summary>
        /// <param name="m"></param>
        public void Set(TriMesh m)
        {
            try
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
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
            }
        }
        /// <summary>
        /// добавить новую сетку к текущей
        /// </summary>
        /// <param name="mesh"></param>
        public void Add(TriMesh mesh)
        {
            // переопределенный +=
            try
            {
                // Если первая сетка пустая то просто скопировать вторую
                if (CountKnots == 0)
                {
                    this.Set(mesh);
                    return;
                }
                // Если прибавляемая пустая то ничего не прибавлять
                if (mesh.CountKnots == 0) return;
                // ==========================================================
                int k;
                int numKnotOne = CountKnots;     // кол-во узлов в 1 подобласти
                int numKnotTwo = mesh.CountKnots;// кол-во узлов в 2 подобласти
                                                 // создание временного массива для хранения
                int[] ConformID = null;
                bool[] Check = null;
                MEM.Alloc(numKnotTwo, ref ConformID);
                MEM.Alloc(numKnotTwo, ref Check);
                for (int i = 0; i < numKnotTwo; i++)
                {
                    Check[i] = true;
                    ConformID[i] = i;
                }
                // координаты 1 сетки
                double[] x = GetCoords(0);
                double[] y = GetCoords(1);
                // координаты 2 сетки
                double[] mx = mesh.GetCoords(0);
                double[] my = mesh.GetCoords(1);

                int dKnot = 0;              // счетчик числа совпадающих узлов
                for (uint i = 0; i < CountBoundKnots; i++)   // перебор по 1 сетке
                {
                    for (uint j = 0; j < mesh.CountBoundKnots; j++) // перебор по 2 сетке
                    {
                        // если координаты граничных точек совпадают
                        if (MEM.Equals(x[this.BoundKnots[i]], mx[mesh.BoundKnots[j]], MEM.Error8) &&
                            MEM.Equals(y[this.BoundKnots[i]], my[mesh.BoundKnots[j]], MEM.Error8))
                        //if ((Point[BNods[i].ID]) == (mesh.Point[mesh.BNods[j].ID]))
                        {
                            ConformID[mesh.BoundKnots[j]] = this.BoundKnots[i];
                            Check[mesh.BoundKnots[j]] = false; dKnot++;
                            break;
                        }
                    }
                }
                // Перенумерация узлов во 2 -й подобласти
                k = numKnotOne;
                for (uint i = 0; i < numKnotTwo; i++)
                    if (Check[i] == true) { ConformID[i] = k; k++; }
                //
                // **************** Создание нового массива обхода ******************
                List<TriElement> ListAElement = new List<TriElement>(CountElements + mesh.CountElements);
                ListAElement.AddRange(AreaElems);
                // перебор по всем КЭ второй сетки
                int CountTwoFE = mesh.AreaElems.Length;
                for (uint i = 0; i < CountTwoFE; i++)
                {
                    TriElement Elem = mesh.AreaElems[i];
                    Elem.Vertex1 = (uint)ConformID[Elem.Vertex1];
                    Elem.Vertex2 = (uint)ConformID[Elem.Vertex2];
                    Elem.Vertex3 = (uint)ConformID[Elem.Vertex3];
                    ListAElement.Add(Elem);
                }
                AreaElems = ListAElement.ToArray();
                //***************  Массив обхода граничных КЭ *************
                int[] SensBFE_1 = null;
                int[] SensBFE_2 = null;
                MEM.Alloc(CountBoundElements, ref SensBFE_1, 1);
                MEM.Alloc(mesh.CountBoundElements, ref SensBFE_2, 1);
                // перенумерация узлов в ГКЭ второго сегмента
                for (uint i = 0; i < mesh.CountBoundElements; i++)
                {
                    // перенумерация узлов в ГЭ второй сетки
                    mesh.BoundElems[i].Vertex1 = (uint)ConformID[mesh.BoundElems[i].Vertex1];
                    mesh.BoundElems[i].Vertex2 = (uint)ConformID[mesh.BoundElems[i].Vertex2];
                }
                // Пометка совпадающих ГКЭ
                for (uint i = 0; i < CountBoundElements; i++) // перебор по ГКЭ 2 подобласти
                {
                    for (uint j = 0; j < mesh.CountBoundElements; j++)
                    {
                        if (BoundElems[i].Equals(mesh.BoundElems[j]) == true)
                        {
                            SensBFE_1[i] = 0;
                            SensBFE_2[j] = 0;
                        }
                    }
                }
                // Формирование окончательного буферного массива
                List<TwoElement> ListBElement = new List<TwoElement>(CountBoundElements + mesh.CountBoundElements);
                List<int> ListBoundElementsMark = new List<int>(CountBoundElements + mesh.CountBoundElements);
                //HVectorFE TmpBoundElems;
                for (uint i = 0; i < CountBoundElements; i++) // перебор по ГКЭ 1 подобласти
                    if (SensBFE_1[i] > 0)
                    {
                        ListBElement.Add(BoundElems[i]);
                        ListBoundElementsMark.Add(BoundElementsMark[i]);
                    }
                for (uint i = 0; i < mesh.CountBoundElements; i++) // перебор по ГКЭ 2 подобласти
                    if (SensBFE_2[i] > 0)
                    {
                        ListBElement.Add(mesh.BoundElems[i]);
                        ListBoundElementsMark.Add(mesh.BoundElementsMark[i]);
                    }
                // создание массива ГКЭ
                BoundElems = ListBElement.ToArray();
                BoundElementsMark = ListBoundElementsMark.ToArray();

                //*********  Массив координат и параметров сетки ************
                List<double> listX = new List<double>();
                List<double> listY = new List<double>();
                listX.AddRange(CoordsX);
                listY.AddRange(CoordsY);
                for (uint i = 0; i < mesh.CountKnots; i++)
                {
                    if (Check[i] == true)
                    {
                        listX.Add(mesh.CoordsX[i]);
                        listY.Add(mesh.CoordsY[i]);
                    }
                }
                CoordsX = listX.ToArray();
                CoordsY = listY.ToArray();
                //**************** Список граничных узлов ******************
                //        созданный без циклической сортировки !!!
                //     временный расширенный массив граничных узлов
                int[] tmpBoundKnots = null;
                int[] tmpBoundKnotsMark = null;
                MEM.Alloc(CountKnots + mesh.CountKnots, ref tmpBoundKnots, -1);
                MEM.Alloc(CountKnots + mesh.CountKnots, ref tmpBoundKnotsMark);
                //  запись всех узлов принадлежащих граничным элементам сформированной сетки
                for (int i = 0; i < CountBoundElements; i++)
                {
                    tmpBoundKnots[BoundElems[i].Vertex1] = (int)BoundElems[i].Vertex1;
                    tmpBoundKnots[BoundElems[i].Vertex2] = (int)BoundElems[i].Vertex2;
                    tmpBoundKnotsMark[BoundElems[i].Vertex1] = BoundElementsMark[i];
                    tmpBoundKnotsMark[BoundElems[i].Vertex2] = BoundElementsMark[i];
                }
                List<int> LBoundKnots = new List<int>(CountBoundKnots + mesh.CountBoundKnots);
                List<int> LBoundKnotsMark = new List<int>(CountBoundKnots + mesh.CountBoundKnots);
                // количество граничных узлов в рез. сетке
                for (uint i = 0; i < CountKnots; i++)
                    if (tmpBoundKnots[i] != -1)
                    {
                        LBoundKnots.Add(tmpBoundKnots[i]);
                        LBoundKnotsMark.Add(tmpBoundKnotsMark[i]);
                    }
                BoundKnots = LBoundKnots.ToArray();
                BoundKnotsMark = LBoundKnotsMark.ToArray();
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
            }
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