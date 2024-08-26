//----------------------------W-----------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    using GeometryLib;
    using System.Linq;
    using System.Collections.Generic;
    using CommonLib.Geometry;

    /// <summary>
    /// Контрольно объемная (КО) сетка задачи для 3 и 4 узловых КО
    /// для однородных и смешанных сеток 1 порядка
    /// </summary>
    [Serializable]
    public class FVComMesh : IFVComMesh, IMesh
    {
        #region Поля и методы FV
        /// <summary>
        /// Координаты узловых точек и параметров определенных в них
        /// </summary>
        public double[] CoordsX;
        public double[] CoordsY;
        /// <summary>
        ///  Контрольные объемы
        /// </summary>
        public IFVElement[] AreaElems { get => areaElems; set => areaElems =value; }
        /// <summary>
        ///  Контрольные объемы
        /// </summary>
        public IFVElement[] areaElems;
        /// <summary>
        /// Грани КО
        /// </summary>
        public IFVFacet[] Facets { get => facets; set => facets = value; }
        /// <summary>
        /// Грани КО
        /// </summary>
        public IFVFacet[] facets;
        /// <summary>
        /// Граничные грани КО
        /// </summary>
        public IFVFacet[] BoundaryFacets { get => boundaryFacets; set => boundaryFacets = value; }
        /// <summary>
        /// Граничные грани КО
        /// </summary>
        public IFVFacet[] boundaryFacets;
        #endregion

        #region IMesh
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        public TypeRangeMesh typeRangeMesh => TypeRangeMesh.mRange1;
        /// <summary>
        /// Тип КЭ сетки в 1D и 2D
        /// </summary>
        public TypeMesh typeMesh => tMesh;
        TypeMesh tMesh = TypeMesh.MixMesh;
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TriElement[] GetAreaElems()
        {
            List<TriElement> list = new List<TriElement>();
            for (int i = 0; i < areaElems.Length; i++)
            {
                TriElement[] e = areaElems[i].Nods();
                list.Add(e[0]);
                if (e.Length == 2)
                    list.Add(e[1]);
            }
            return list.ToArray();
        }
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoElement[] GetBoundElems()
        {
            TwoElement[] elems = new TwoElement[boundaryFacets.Length];
            for (int i = 0; i < boundaryFacets.Length; i++)
                elems[i] = boundaryFacets[i].Nods();
            return elems;
        }
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public int[] GetBElementsBCMark()
        {
            int[] elems = null;
            MEM.Alloc(boundaryFacets.Length, ref elems);
            for (int i = 0; i < boundaryFacets.Length; i++)
                elems[i] = (int)boundaryFacets[i].BoundaryFacetsMark;
            return elems;
        }
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public int[] GetBoundKnots()
        {
            int[] elems = new int[boundaryFacets.Length];
            for (int i = 0; i < boundaryFacets.Length; i++)
                elems[i] = (int)boundaryFacets[i].Pointid1;
            return elems;
        }
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] GetBoundKnotsMark()
        {
            return GetBElementsBCMark();
        }
        /// <summary>
        /// Координаты X для узловых точек 
        /// </summary>
        public double[] GetCoords(int dim)
        {
            if (dim == 0)
                return CoordsX;
            else
                return CoordsY;
        }
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int CountElements
        {
            get { return areaElems == null ? 0 : areaElems.Length; }
        }
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        public int CountBoundElements
        {
            get { return boundaryFacets == null ? 0 : boundaryFacets.Length; }
        }
        /// <summary>
        /// Количество узлов
        /// </summary>
        public int CountKnots
        {
            get { return CoordsX == null ? 0 : CoordsX.Length; }
        }
        /// <summary>
        /// Количество граничных узлов
        /// </summary>
        public int CountBoundKnots
        {
            get { return boundaryFacets == null ? 0 : boundaryFacets.Length; }
        }
        /// <summary>
        /// Диапазон координат для узлов сетки
        /// </summary>
        public void MinMax(int dim, ref double min, ref double max)
        {
            if (dim == 0)
            {
                max = CoordsX == null ? double.MaxValue : CoordsX.Max();
                min = CoordsX == null ? double.MinValue : CoordsX.Min();
            }
            else
            {
                max = CoordsY == null ? double.MaxValue : CoordsY.Max();
                min = CoordsY == null ? double.MinValue : CoordsY.Min();
            }
        }
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            int[] nods = areaElems[i].Nodes;
            MEM.Alloc(nods.Length, ref knots);
            for (int n = 0; n < nods.Length; n++)
                knots[n] = (uint)nods[n];
            if (nods.Length == 3)
                return TypeFunForm.Form_2D_Triangle_L1;
            else
                return TypeFunForm.Form_2D_Rectangle_L1;
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            TwoElement nods = boundaryFacets[i].Nods();
            bknot[0] = nods.Vertex1;
            bknot[1] = nods.Vertex2;
            return TypeFunForm.Form_1D_L1;
        }
        /// <summary>
        /// Получить координаты Х, Y для вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>

        public void GetElemCoords(uint i, ref double[] X, ref double[] Y)
        {
            int[] nods = areaElems[i].Nodes;
            MEM.Alloc<double>(nods.Length, ref X, "X");
            MEM.Alloc<double>(nods.Length, ref Y, "Y");
            for (int n = 0; n < nods.Length; n++)
            {
                X[n] = CoordsX[nods[n]];
                Y[n] = CoordsY[nods[n]];
            }
        }
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>
        public void ElemValues(double[] Values, uint i, ref double[] elementValue)
        {
            int[] nods = areaElems[i].Nodes;
            for (int n = 0; n < nods.Length; n++)
                elementValue[0] = Values[nods[n]];
        }
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        public uint GetMaxKnotDecrementForElement(uint i)
        {
            int[] nods = areaElems[i].Nodes;
            int min = nods.Min();
            int max = nods.Max();
            return (uint)(max - min);
        }
        /// <summary>
        ///  Вычисление площади КЭ
        /// </summary>
        /// <param name="x">массив координат елемента Х</param>
        /// <param name="y">массив координат елемента Y</param>
        /// <returns></returns>
        public double ElemSquare(double[] x, double[] y)
        {
            return GEO.TriangleArea(x, y);
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        /// <param name="element">номер конечного элемента</param>
        /// <returns></returns>
        public double ElemSquare(uint element)
        {
            double S;
            int[] nods = areaElems[element].Nodes;
            double[] X = CoordsX;
            double[] Y = CoordsY;
            if (nods.Length > 4)
                throw new Exception("Площадь КЭ высокого порядка необходимо вычислять численно!");
            if (nods.Length == 3)
            {
                S = (X[nods[0]] * (Y[nods[1]] - Y[nods[2]]) +
                     X[nods[1]] * (Y[nods[2]] - Y[nods[0]]) +
                     X[nods[2]] * (Y[nods[0]] - Y[nods[1]])) / 2.0;
            }
            else
            {
                S = (X[nods[0]] * (Y[nods[1]] - Y[nods[2]]) +
                     X[nods[1]] * (Y[nods[2]] - Y[nods[0]]) +
                     X[nods[2]] * (Y[nods[0]] - Y[nods[1]])) / 2.0;

                S += (X[nods[0]] * (Y[nods[2]] - Y[nods[3]]) +
                      X[nods[2]] * (Y[nods[3]] - Y[nods[0]]) +
                      X[nods[3]] * (Y[nods[0]] - Y[nods[2]])) / 2.0;
            }
            return S;
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public double ElemSquare(TriElement element)
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
        public double GetBoundElemLength(uint belement)
        {
            TwoElement nods = boundaryFacets[belement].Nods();
            double a = CoordsX[nods.Vertex1] - CoordsX[nods.Vertex2];
            double b = CoordsY[nods.Vertex1] - CoordsY[nods.Vertex2];
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
            int count = 0;
            for (int k = 0; k < boundaryFacets.Length; k++)
            {
                if (boundaryFacets[k].BoundaryFacetsMark == i)
                    ++count;
            }
            uint[] mass = new uint[count];
            int j = 0;
            // КОСЯКУ TO DO
            for (int k = 0; k < boundaryFacets.Length; k++)
            {
                if (boundaryFacets[k].BoundaryFacetsMark == i)
                    mass[j++] = (uint)boundaryFacets[k].BoundaryFacetsMark;
            }
            Array.Sort(mass);
            return mass;
        }
        /// <summary>
        /// Получить выборку граничных элементов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        public uint[][] GetBoundElementsByMarker(int id)
        {
            {
                int count = 0;
                for (int k = 0; k < CountBoundElements; k++)
                {
                    if (boundaryFacets[k].BoundaryFacetsMark == id)
                        ++count;
                }
                uint[][] mass = new uint[count][];
                int j = 0;
                for (int k = 0; k < CountBoundElements; k++)
                {
                    if (boundaryFacets[k].BoundaryFacetsMark == id)
                    {
                        TwoElement el = boundaryFacets[k].Nods();
                        mass[j] = new uint[2];
                        mass[j][0] = el.Vertex1;
                        mass[j][1] = el.Vertex2;
                        j++;
                    }
                }
                return mass;
            }
        }
        /// <summary>
        /// Получить тип граничных условий для граничного элемента
        /// </summary>
        /// <param name="elem">граничный элемент</param>
        /// <returns>ID типа граничных условий</returns>
        public int GetBoundElementMarker(uint elem)
        {
            return (int)boundaryFacets[elem].BoundaryFacetsMark;
        }
        /// <summary>
        /// Ширина ленты в глобальной матрице жнсткости
        /// </summary>
        /// <returns></returns>
        public uint GetWidthMatrix()
        {
            uint max = GetMaxKnotDecrementForElement(0);
            for (uint i = 1; i < areaElems.Length; i++)
            {
                uint tmp = GetMaxKnotDecrementForElement(i);
                if (max < tmp)
                    max = tmp;
            }
            return max + 1;
        }
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public IMesh Clone()
        {
            return new FVComMesh(this);
        }
        /// <summary>
        /// Тестовая печать КЭ сетки в консоль
        /// </summary>
        public void Print()
        {
            Console.WriteLine();
            Console.WriteLine("CoordsX CoordsY");
            for (int i = 0; i < CoordsX.Length; i++)
            {
                Console.WriteLine(" id {0} x {1:F4} y {2:F4}", i, CoordsX[i], CoordsY[i]);
            }
            Console.WriteLine();
            Console.WriteLine("BoundKnots");
            for (int i = 0; i < boundaryFacets.Length; i++)
            {
                TwoElement elem = boundaryFacets[i].Nods();
                uint n0 = elem.Vertex1;
                Console.WriteLine(" id {0} ", n0);
            }
            Console.WriteLine();
            Console.WriteLine("FE");
            for (int i = 0; i < areaElems.Length; i++)
            {
                int ID = i;
                Console.WriteLine(" id {0} ");
                int[] nodes = areaElems[i].Nodes;
                for (int n = 0; n < nodes.Length; n++)
                    Console.Write(" {0} ", nodes[n]);
            }
            Console.WriteLine();
            Console.WriteLine("BFE");
            for (int i = 0; i < boundaryFacets.Length; i++)
            {
                TwoElement elem = boundaryFacets[i].Nods();
                uint n0 = elem.Vertex1;
                uint n1 = elem.Vertex2;
                int fl = (int)facets[i].BoundaryFacetsMark;
                Console.WriteLine(" id {0}: {1} {2} fl {3}", i, n0, n1, fl);
            }
        }

        #endregion

        /// <summary>
        /// Плащади связанные с узлами
        /// </summary>
        double[] S = null;

        public FVComMesh() { }
        public FVComMesh(MeshCore cmesh)
        {
            SetFVМesh(cmesh);
        }
        public FVComMesh(FVComMesh m)
        {
            MeshCore cmesh = new MeshCore(m);
            SetFVМesh(cmesh);
        }
        public FVComMesh(IMesh m)
        {
            MeshCore cmesh = new MeshCore(m);
            SetFVМesh(cmesh);
        }
        /// <summary>
        /// формирование сетки задачи
        /// </summary>
        /// <param name="points">координаты узлов</param>
        /// <param name="polys">узлы опорной сетки КО</param>
        /// <param name="bound">граничных узлов</param>
        public void SetFVМesh(MeshCore cmesh)
        {
            MEM.MemCopy(ref CoordsX, cmesh.points[0]);
            MEM.MemCopy(ref CoordsY, cmesh.points[1]);

            Dictionary<string, FVFacet> dictionary = new Dictionary<string, FVFacet>();
            
            int cu;
            areaElems = new FVElement[cmesh.elems[0].Length];
            int faceid = 0;
            for (int eID = 0; eID < cmesh.elems[0].Length; eID++)
            {
                TypeFunForm ff = (TypeFunForm) cmesh.fform[eID];
                cu = ff == TypeFunForm.Form_2D_Triangle_L1 ? 3 : 4;
                areaElems[eID] = new FVElement(cu);
                IFVElement elem = areaElems[eID];
                elem.Id = eID;
                HPoint[] vertex = new HPoint[cu];
                for (int vID = 0; vID < cu; vID++)
                {
                    int nodeA = (int)cmesh.elems[vID][eID];
                    int nodeB = (int)cmesh.elems[(vID + 1) % cu][eID];
                    elem.Nodes[vID] = nodeA;
                    vertex[vID] = new HPoint(CoordsX[nodeA], CoordsY[nodeA]);
                    string faceеHash = FVFacet.GetFaceHash(nodeA, nodeB);

                    if (dictionary.ContainsKey(faceеHash) == false)
                    {
                        FVFacet facet = new FVFacet(new HPoint(CoordsX[nodeA], CoordsY[nodeA]),
                                                    new HPoint(CoordsX[nodeB], CoordsY[nodeB]));
                        facet.BoundaryFacetsMark = 0;
                        facet.Id = faceid;
                        facet.Pointid1 = nodeA;
                        facet.Pointid2 = nodeB;
                        facet.Owner = elem;
                        faceid++;
                        dictionary.Add(faceеHash, facet);
                        elem.Facets[vID] = facet;
                    }
                    else
                    {
                        FVFacet facet = dictionary[faceеHash];
                        facet.BoundaryFacetsMark = -1;
                        facet.NBElem = elem;
                        elem.Facets[vID] = facet;
                        IFVElement owner = facet.Owner;
                        elem.NearestElements[vID] = owner;
                        owner.NearestElements[owner.FaceLocalId(facet)] = elem;
                    }
                }
                elem.Vertex = vertex;
                //elem.PreCalc();
            }
            facets = new FVFacet[dictionary.Count];
            foreach (var pair in dictionary)
            {
                FVFacet facet = pair.Value;
                facets[facet.Id] = facet;
            }
            for (int eID = 0; eID < CountElements; eID++)
                areaElems[eID].InitElement();

            string facestr;
            boundaryFacets = new FVFacet[cmesh.boundary[0].Length];
            for (int f = 0; f < boundaryFacets.Length; f++)
            {
                facestr = FVFacet.GetFaceHash((int)cmesh.boundary[0][f], (int)cmesh.boundary[1][f]);
                FVFacet facet = dictionary[facestr];
                facet.BoundaryFacetsMark = cmesh.boundaryMark[0][f];
                boundaryFacets[f] = facet;
            }
        }

        /// <summary>
        /// Интерполяция Функции с центров КО в узлы КЭ сетки
        /// </summary>
        /// <param name="U_elems"></param>
        /// <param name="FlagBCCorrection">поправка на границах</param>
        /// <returns></returns>
        public void ConvertElementsToKnots(double[] Zeta_e, ref double[] Zeta_n, double[] BoundaryValueDomain = null, bool FlagBCCorrection = false)
        {
            int nodecount = CoordsX.Length;
            MEM.VAlloc(nodecount, 0, ref Zeta_n);
            MEM.VAlloc(nodecount, 0, ref S);
            for (int eID = 0; eID < CountElements; eID++)
            {
                int cu = areaElems[eID].Nodes.Length;
                for (int nod = 0; nod < cu; nod++)
                {
                    int i = areaElems[eID].Nodes[nod];
                    Zeta_n[i] += Zeta_e[eID] * areaElems[eID].Volume / cu;
                    S[i] += areaElems[eID].Volume / cu;
                }
            }
            if (FlagBCCorrection == true && BoundaryValueDomain != null)
            {
                int idx;
                double dL;
                for (int i = 0; i < boundaryFacets.Length; i++)
                {
                    IFVFacet facet = boundaryFacets[i];
                    if (facet.BoundaryFacetsMark == 0)
                    {
                        idx = facet.Pointid1;
                        Zeta_n[idx] = 0;
                        S[idx] = 0;
                        idx = facet.Pointid1;
                        Zeta_n[idx] = 0;
                        S[idx] = 0;
                    }
                }
                for (int i = 0; i < boundaryFacets.Length; i++)
                {
                    IFVFacet facet = boundaryFacets[i];
                    if (facet.BoundaryFacetsMark == 0)
                    {
                        dL = facet.Length / 2;
                        idx = facet.Pointid1;
                        Zeta_n[idx] += BoundaryValueDomain[facet.BoundaryFacetsMark] * dL;
                        S[idx] += dL;
                        idx = facet.Pointid2;
                        Zeta_n[idx] += BoundaryValueDomain[facet.BoundaryFacetsMark] * dL;
                        S[idx] += dL;
                    }
                }
            }
            for (int i = 0; i < Zeta_n.Length; i++)
                Zeta_n[i] /= S[i];
        }
        /// <summary>
        /// Получить среднее на контрольном объеме значение функции по значениям в узлах
        /// </summary>
        /// <param name="Zeta_n"></param>
        /// <param name="Zeta_e"></param>
        public void ConvertKnotsToElements(double[] Zeta_n, ref double[] Zeta_e)
        {
            MEM.Alloc(CountElements, ref Zeta_e);
            for (int eID = 0; eID < CountElements; eID++)
            {
                int cu = areaElems[eID].Nodes.Length;
                Zeta_e[eID] = 0;
                for (int nod = 0; nod < cu; nod++)
                    Zeta_e[eID] += Zeta_n[areaElems[eID].Nodes[nod]];
                Zeta_e[eID] /= cu;
            }
        }
    }
}
