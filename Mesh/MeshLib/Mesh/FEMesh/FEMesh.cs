//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//---------------------------------------------------------------------------
//  кодировка HBaseMesh (последняя правка): 15.10.2000 Потапов И.И. (c++)
//  перенос  HBaseMesh => FEMesh :   23.01.2022 Потапов И.И. (с++ => c#)
//---------------------------------------------------------------------------
//      Убраны массивы ГУ из всей иерархи сеток : 01.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: Произвольная базисная сетка задачи с поэлементной 
    /// концепцией хранения данных о связности
    /// </summary>
    [Serializable]
    public class FEMesh : IFEMesh
    {
        #region Поля и методы IFEMesh
        /// <summary>
        /// id пообласти 
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Конечные элементы сетки
        /// </summary>
        public IFElement[] AreaElems { get; set; }
        /// <summary>
        /// граничные элементы сетки
        /// </summary>
        public IFElement[] BoundElems { get; set; }
        /// <summary>
        /// граничные узлы секи
        /// </summary>
        public IFENods[] BNods { get; set; }
        /// <summary>
        /// Координаты узловых точек и параметров определенных в них
        /// </summary>
        public double[][] Params { get; set; }
        /// <summary>
        /// Координаты узлов по Х
        /// </summary>
        public double[] coordsX;
        /// <summary>
        /// Координаты узлов по Н
        /// </summary>
        public double[] coordsY;
        /// <summary>
        /// Получить параметры сетки (координаты и поля)
        /// </summary>
        /// <returns></returns>
        public double[][] GetParams() { return Params; }
        /// <summary>
        /// добавить новую сетку к текущей
        /// </summary>
        /// <param name="mesh"></param>
        public void Add(IFEMesh mesh)
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
                        if ( MEM.Equals(x[BNods[i].ID], mx[mesh.BNods[j].ID],MEM.Error8) &&
                             MEM.Equals(y[BNods[i].ID], my[mesh.BNods[j].ID], MEM.Error8) )
                        //if ((Point[BNods[i].ID]) == (mesh.Point[mesh.BNods[j].ID]))
                        {                  
                            ConformID[mesh.BNods[j].ID] = BNods[i].ID;
                            Check[mesh.BNods[j].ID] = false; dKnot++;
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
                List<IFElement> ListAElement = new List<IFElement>(CountElements + mesh.CountElements);
                ListAElement.AddRange(AreaElems);
                // перебор по всем КЭ второй сетки
                int CountTwoFE = mesh.AreaElems.Length;
                for (uint i = 0; i < CountTwoFE; i++)
                {
                    IFElement Elem = mesh.AreaElems[i];
                    for (int j = 0; j < Elem.Length; j++)
                        Elem[j].ID = ConformID[Elem[j].ID];
                    ListAElement.Add(new FElement(Elem));
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
                    for (uint j = 0; j < mesh.BoundElems[i].Length; j++)
                        mesh.BoundElems[i].Nods[j].ID = ConformID[mesh.BoundElems[i].Nods[j].ID];
                }
                // Пометка совпадающих ГКЭ
                for (uint i = 0; i < CountBoundElements; i++) // перебор по ГКЭ 2 подобласти
                {
                    for (uint j = 0; j < mesh.CountBoundElements; j++)
                    {
                        if(BoundElems[i].Equals(mesh.BoundElems[j]) == true)
                        {
                            SensBFE_1[i] = 0;
                            SensBFE_2[j] = 0;
                        }
                    }
                }
                // Формирование окончательного буферного массива
                List<IFElement> ListBElement = new List<IFElement>(CountBoundElements + mesh.CountBoundElements);
                //HVectorFE TmpBoundElems;
                for (uint i = 0; i < CountBoundElements; i++) // перебор по ГКЭ 1 подобласти
                    if (SensBFE_1[i] > 0)
                        ListBElement.Add(BoundElems[i]);
                //
                for (uint i = 0; i < mesh.CountBoundElements; i++) // перебор по ГКЭ 2 подобласти
                    if (SensBFE_2[i] > 0)
                        ListBElement.Add(mesh.BoundElems[i]);
                // создание массива ГКЭ
                BoundElems = ListBElement.ToArray();
                //*********  Массив координат и параметров сетки ************
                List<double> listX = new List<double>();
                List<double> listY = new List<double>();
                listX.AddRange(coordsX);
                listY.AddRange(coordsY);
                for (uint i = 0; i < mesh.CountKnots; i++)
                {
                    if (Check[i] == true)
                    {
                        listX.Add(mesh.CoordsX[i]);
                        listY.Add(mesh.CoordsY[i]);
                    }
                }
                coordsX = listX.ToArray();
                coordsY = listY.ToArray();

                List<double>[] ListParams = new List<double>[Params.Length];
                for (int p = 0; p < Params.Length; p++)
                {
                    ListParams[p] = new List<double>();
                    ListParams[p].AddRange(Params[p]);
                }
                double[][] mParams = mesh.GetParams();
                for (uint i = 0; i < mesh.CountKnots; i++)
                    if (Check[i] == true)
                    {
                        for (int p = 0; p < Params.Length; p++)
                            ListParams[p].Add(mParams[p][i]);
                    }
                for (int p = 0; p < Params.Length; p++)
                    Params[p] = ListParams[p].ToArray();
                //**************** Список граничных узлов ******************
                //        созданный без циклической сортировки !!!
                //     временный расширенный массив граничных узлов
                IFENods[] BKnots = null;
                MEM.Alloc(CountKnots, ref BKnots);
                //  запись всех узлов принадлежащих граничным элементам сформированной сетки
                for (int i = 0; i < CountBoundElements; i++)
                    for (k = 0; k < BoundElems[i].Length; k++)  // перебор по КЭ
                    {
                        // метод многократной прописки в буффер
                        // if(BKnots[BoundElems[i].Nods[k].ID]==null)
                        BKnots[BoundElems[i].Nods[k].ID] =  BoundElems[i].Nods[k];
                        BKnots[BoundElems[i].Nods[k].ID].MarkBC = BoundElems[i].MarkBC;
                    }
                List<IFENods> BoundKnots = new List<IFENods>(CountBoundKnots + mesh.CountBoundKnots);
                // количество граничных узлов в рез. сетке
                for (uint i = 0; i < CountKnots; i++)
                    if (BKnots[i] != null)
                        BoundKnots.Add(BKnots[i]);
                BNods = BoundKnots.ToArray();
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
            }
        }

        /// <summary>
        /// взять сетку для подобласти
        /// </summary>
        /// <param name="mesh"></param>
        public IFEMesh Get(int index)
        {
            return this;
        }
        /// <summary>
        /// взять полную сетку с размерностью d - 1 для участка границы 
        /// по флагам граничных элементов, с сохранением расширенного массива параметров
        /// </summary>
        /// <param name="mesh"></param>
        public IFEMesh GetBoundaryMesh(int MarkBC)
        {
            FEMesh bmesh = new FEMesh();
            int i = 0;
            int countBE = BoundElems.Count(x => x.MarkBC == MarkBC);
            bmesh.AreaElems = new IFElement[countBE];
            // выделяем граничные КЭ создавая из них КЭ одномерной сетки
            int[] bknotID = new int[CountBoundKnots];
            IFENods[] nods = new IFENods[CountBoundKnots];
            foreach (var belem in BoundElems)
            {
                if (belem.MarkBC == MarkBC)
                {
                    bmesh.AreaElems[i++] = new FElement(belem);
                    foreach (var nod in belem.Nods)
                    {
                        bknotID[nod.ID] += 1;
                        nods[nod.ID] = new FENods(nod);
                    }
                }
            }
            int countBKnot = bknotID.Count(x => x == 1);
            bmesh.BoundElems = new IFElement[countBKnot];
            bmesh.BNods =  new IFENods[countBKnot];
            i = 0;
            for(int ID=0; ID< bknotID.Length; ID++)
            {
                if (bknotID[ID]==1)
                {
                    int[] be = { nods[ID].ID };
                    bmesh.BoundElems[i] = new FElement(be, nods[ID].ID, MarkBC, TypeFunForm.Form_1D_L0);
                    bmesh.BNods[i++] = new FENods(nods[ID]); 
                }
            }
            int countParam = bknotID.Count(x => x > 0);
            // копирование параметров
            bmesh.Params = MEM.NewMass2D<double>(Params.Length, countParam);
            i = 0;
            for (int ID = 0; ID < bknotID.Length; ID++)
            {
                if (bknotID[ID] > 0)
                {
                    for (int ip = 0; ip < Params.Length; ip++)
                        bmesh.Params[ip][i] = Params[ip][ID];
                    i++;
                }
            }
            return bmesh;
        }
        /// <summary>
        /// взять координатную сетку с размерностью d - 1 
        /// для участка границы по флагам граничных узлов
        /// </summary>
        /// <param name="mesh"></param>
        public IMesh GetBoundaryTwoMesh(int index)
        {
            List<double> xx = new List<double>();
            List<double> yy = new List<double>();
            foreach (IFENods knot in BNods)
            {
                if (knot.MarkBC == index)
                {
                    xx.Add(Params[0][knot.ID]);
                    yy.Add(Params[1][knot.ID]);
                }
            }
            IMesh mesh = new TwoMesh(xx.ToArray(), yy.ToArray());
            return mesh;
        }
        #endregion
        #region Поля и методы IMesh
        /// <summary>
        /// Функция формы для текущего КЭ в области
        /// </summary>
        public TypeFunForm GetTypeFunFormAreaElems(int elem)
        {
            return AreaElems[elem].TFunForm;
        }
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
        public double[] CoordsX { get => coordsX; set => coordsX = value; }
        public double[] CoordsY { get => coordsY; set => coordsY = value; }
        public FEMesh(int CountParams = 0, TypeRangeMesh Range = TypeRangeMesh.mRange1)
        {
            Params = new double[CountParams][];
            this.tRangeMesh = Range;
            this.tMesh = TypeMesh.MixMesh;
        }
        public FEMesh(IFEMesh m)
        {
            Set(m);
        }
        public FEMesh(IMesh m)
        {
            Set(m);
        }
        /// <summary>
        /// Установка новой сетки в текущую
        /// </summary>
        /// <param name="m"></param>
        public void Set(IFEMesh m)
        {
            try
            {
                tRangeMesh = m.typeRangeMesh;
                tMesh = m.typeMesh;
                ID = m.ID;
                if (m.AreaElems == null) return;

                AreaElems = new IFElement[m.AreaElems.Length];
                for (int i = 0; i < AreaElems.Length; i++)
                    AreaElems[i] = new FElement(m.AreaElems[i]);

                BoundElems = new IFElement[m.BoundElems.Length];
                for (int i = 0; i < BoundElems.Length; i++)
                    BoundElems[i] = new FElement(m.BoundElems[i]);

                BNods = new IFENods[m.BNods.Length];
                for (int i = 0; i < BNods.Length; i++)
                    BNods[i] = new FENods(m.BNods[i]);

                double[][] p = m.GetParams();
                if (p != null)
                {
                    if (p.Length > 0)
                        Params = MEM.Copy(Params, p);
                    else
                        Params = p;
                }
                MEM.MemCopy(ref coordsX, m.CoordsX);
                MEM.MemCopy(ref coordsY, m.CoordsY);
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
            }
        }
        /// <summary>
        /// Установка новой сетки в текущую
        /// </summary>
        /// <param name="m"></param>
        public void Set(IMesh m)
        {
            try
            {
                tRangeMesh = m.typeRangeMesh;
                tMesh = m.typeMesh;
                ID = 0;
                AreaElems = new IFElement[m.CountElements];

                TriElement[] elems = m.GetAreaElems();
                for (int elem = 0; elem < AreaElems.Length; elem++)
                    AreaElems[elem] = new FElement(elems[elem], elem, 0);
                BoundElems = new IFElement[m.CountBoundElements];

                TwoElement[] belems = m.GetBoundElems();
                int[] marker = m.GetBElementsBCMark();
                
                for (int i = 0; i < BoundElems.Length; i++)
                    BoundElems[i] = new FElement(belems[i], i, marker[i]);

                BNods = new IFENods[m.CountBoundKnots];
                marker = m.GetBoundKnotsMark();
                int[] knots = m.GetBoundKnots();
                for (int i = 0; i < BNods.Length; i++)
                    BNods[i] = new FENods(knots[i], marker[i]);

                MEM.MemCopy(ref coordsX, m.GetCoords(0));
                MEM.MemCopy(ref coordsY, m.GetCoords(1));

            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
            }
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public IMesh Clone()
        {
            return new FEMesh(this);
        }
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
            get { return BNods == null ? 0 : BNods.Length; }
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
        public TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            IFENods[] Nods = AreaElems[i].Nods;
            MEM.Alloc(Nods.Length, ref knots);
            for (int j = 0; j < Nods.Length; j++)
                knots[j] = (uint)Nods[j].ID;
            return AreaElems[i].TFunForm;
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            IFENods[] Nods = BoundElems[i].Nods;
            MEM.Alloc(Nods.Length, ref bknot);
            for (int j = 0; j < Nods.Length; j++)
                bknot[j] = (uint)Nods[j].ID;
            return BoundElems[i].TFunForm;
        }
        /// <summary>
        /// Адаптер для отрисовки. 
        /// Вектор конечных элементов приведенный к трехузловым КЭ
        /// </summary>
        public TriElement[] GetAreaElems()
        {
            uint[] Knots = null;
            TriElement[] triElements = null;
            int CountTriFE = 0;
            for (uint i = 0; i < CountElements; i++)
            {
                TypeFunForm ff = AreaElems[i].TFunForm;
                CountTriFE += FunFormHelp.GetCountTriElement(ff);
            }
            MEM.Alloc(CountTriFE, ref triElements);
            int e = 0;
            TriElement[] els = null;
            for (int el = 0; el < CountElements; el++)
            {
                Knots = AreaElems[el].Nods.Select(x => (uint)x.ID).ToArray();
                TypeFunForm ff = AreaElems[el].TFunForm;
                FunFormHelp.GetTriElem(ff, Knots, ref els);
                for (int j = 0; j < els.Length; j++)
                    triElements[e++] = els[j];
            }
            return triElements;
        }
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoElement[] GetBoundElems()
        {
            TwoElement[] bElements = null;
            uint[] Knots = null;
            List< TwoElement > listTwoElems = new List< TwoElement >();
            for (uint el = 0; el < CountBoundElements; el++)
            {
                Knots = BoundElems[el].Nods.Select(x => (uint)x.ID).ToArray();
                var ffb = BoundElems[el].TFunForm;
                FunFormHelp.GetBoundTwoElems(ffb, Knots, ref bElements);
                for (int j = 0; j < bElements.Length; j++)
                    listTwoElems.Add(bElements[j]);
            }
            //return BElements;
            return listTwoElems.ToArray();
        }
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public int[] GetBElementsBCMark()
        {
            return BoundElems.Select(x => x.MarkBC).ToArray();
        }
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public int[] GetBoundKnots() { return BNods.Select(x => x.ID).ToArray(); }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] GetBoundKnotsMark() { return BNods.Select(x => x.MarkBC).ToArray();  }
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
        /// Получить координаты узлов КЭ
        /// </summary>
        /// <param name="knots"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void ElemXY(uint[] knots, ref double[] X, ref double[] Y)
        {
            for (int n = 0; n < knots.Length; n++)
            {
                X[n] = coordsX[knots[n]];
                Y[n] = coordsX[knots[n]];
            }
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>

        public void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            MEM.Alloc<double>(AreaElems[i].Length, ref X, "X");
            MEM.Alloc<double>(AreaElems[i].Length, ref Y, "Y");
            for (int n = 0; n < AreaElems[i].Length; n++)
            {
                X[n] = coordsX[AreaElems[i][n].ID];
                Y[n] = coordsY[AreaElems[i][n].ID];
            }
        }
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>

        public void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            for (int j = 0; j < AreaElems[i].Length; j++)
                elementValue[j] = Values[AreaElems[i][j].ID];
        }
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        public uint GetMaxKnotDecrementForElement(uint i)
        {
            int min = AreaElems[i][0].ID;
            int max = AreaElems[i][0].ID;
            for (int j = 1; j < AreaElems[i].Length; j++)
            {
                if (min > AreaElems[i][j].ID)
                    min = AreaElems[i][j].ID;

                if (max < AreaElems[i][j].ID)
                    max = AreaElems[i][j].ID;
            }
            return (uint)(max - min);
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public double ElemSquare(TriElement element)
        {
            return (coordsX[element.Vertex1] * (coordsY[element.Vertex2] - coordsY[element.Vertex3]) +
                    coordsX[element.Vertex2] * (coordsY[element.Vertex3] - coordsY[element.Vertex1]) +
                    coordsX[element.Vertex3] * (coordsY[element.Vertex1] - coordsY[element.Vertex2])) / 2.0;
        }
        /// <summary>
        ///  Вычисление площади КЭ первого порядка
        /// </summary>
        public double ElemSquare(uint elem)
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
        /// Вычисление длины граничного КЭ
        /// </summary>
        /// <param name="belement">номер граничного конечного элемента</param>
        /// <returns></returns>
        public double GetBoundElemLength(uint belement)
        {
            int[] knot = { BoundElems[belement][0].ID, BoundElems[belement][1].ID };
            if (BoundElems[belement].Nods.Length > 2)
                throw new Exception("Длину граничных КЭ высокого порядка необходимо вычислять численно!");
            double a = coordsX[knot[0]] - coordsX[knot[1]];
            double b = coordsY[knot[0]] - coordsY[knot[1]];
            double Length = Math.Sqrt(a * a + b * b);
            return Length;
        }
        /// <summary>
        /// Получить выборку граничных узлов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        public uint[] GetBoundKnotsByMarker(int i)
        {
            uint[] mass = BNods.Where(s => s.MarkBC == i).Select(x => (uint)x.ID).ToArray();
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
            int j = 0;
            IFElement[] bElement = BoundElems.Where(s => s.MarkBC == id).ToArray();
            uint[][] mass = new uint[bElement.Length][];
            for (int e = 0; e < bElement.Length; e++)
            {
                mass[j] = new uint[bElement[e].Length];
                for (int i = 0; i < bElement[e].Length; i++)
                    mass[j][i] = (uint)bElement[e][i].ID;
                j++;
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
            return BoundElems[elem].MarkBC;
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
        /// <summary>
        /// Приведение произвольной КЭ сетки к симплекс сетки
        /// </summary>
        /// <param name="a"></param>
        public static explicit operator TriMesh(FEMesh a)
        {
            TriMesh mesh = new TriMesh();
            mesh.AreaElems = a.GetAreaElems();
            mesh.BoundElems = a.GetBoundElems();
            mesh.BoundKnots = a.GetBoundKnots();
            mesh.BoundKnotsMark = a.GetBoundKnotsMark();
            mesh.CoordsX = a.GetCoords(0);
            mesh.CoordsY = a.GetCoords(1);
            return mesh;
        }
        #endregion
        public void Print()
        {
            Console.WriteLine();
            Console.WriteLine("CoordsX CoordsY");
            for (int i = 0; i < CoordsY.Length; i++)
                Console.WriteLine(" id {0} x {1} y {2}", i, CoordsX[i], CoordsY[i]);
            Console.WriteLine();
            Console.WriteLine("BoundKnots");
            for (int i = 0; i < BNods.Length; i++)
                Console.WriteLine(BNods[i].ToString());
            Console.WriteLine();
            Console.WriteLine("FE");
            for (int i = 0; i < AreaElems.Length; i++)
                Console.WriteLine(AreaElems[i].ToString());
            Console.WriteLine();
            Console.WriteLine("BFE");
            for (int i = 0; i < BoundElems.Length; i++)
                Console.WriteLine(BoundElems[i].ToString());
        }
    }
}