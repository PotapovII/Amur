#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                    17.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
using CommonLib;
using CommonLib.Geometry;
using GeometryLib.Vector;
using MemLogLib;
using System;
using System.Collections.Generic;

namespace MeshLib.Wrappers
{
    /// <summary>
    /// Позволяет получить список граничных элементов и их связь с КЭ
    /// </summary>
    [Serializable]
    public class MWDLinkTri
    {
        /// <summary>
        /// Словарь граней КЭ сетки
        /// </summary>
        public Dictionary<string, IAFacet> dictionary = new Dictionary<string, IAFacet>();
        /// <summary>
        /// граничные грани со связями
        /// </summary>
        public IAFacet[] boundaryFacets = null;
        /// <summary>
        /// Длина граничных элемнгов (граний со связями на КЭ)
        /// </summary>
        public double[] FacetsLen = null;
        /// <summary>
        /// Минимальное растояние от центра КЭ сопряженного с граничным элементом 
        /// </summary>
        public double[] Distance = null;
        /// <summary>
        /// Индес принадлежности КЭ границе по Distance
        /// </summary>
        public int[] BoundElementIndex = null;
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh mesh;
        TwoElement[] belems = null;
        TriElement[] elems = null;
        double[] X = null;
        double[] Y = null;
        public IMesh GetMesh() => mesh;
        public MWDLinkTri(IMesh mesh)
        {
            this.mesh = mesh;
            MEM.VAlloc(mesh.CountKnots, -1, ref BoundElementIndex);
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            belems = mesh.GetBoundElems();
            elems = mesh.GetAreaElems();
            CreateAFacetMesh(mesh);
        }

        /// <summary>
        /// Формирование словаря граней КЭ сетки cmesh
        /// </summary>
        public void CreateAFacetMesh(IMesh cmesh)
        {
            try
            {
                int cu = 3;
                TriElement[] triElements = mesh.GetAreaElems();
                int faceid = 0;
                for (int eID = 0; eID < triElements.Length; eID++)
                {
                    TriElement elem = triElements[eID];
                    for (int vID = 0; vID < cu; vID++)
                    {
                        int nodeA = (int)elem[vID];
                        int nodeB = (int)elem[(vID + 1) % cu];
                        string faceеHash = AFacet.GetFaceHash(nodeA, nodeB);
                        if (dictionary.ContainsKey(faceеHash) == false)
                        {
                            IAFacet facet = new AFacet(faceid, nodeA, nodeB, eID, 0);
                            faceid++;
                            dictionary.Add(faceеHash, facet);
                        }
                        else
                        {
                            IAFacet facet = dictionary[faceеHash];
                            facet.BoundaryFacetsMark = -1;
                            facet.NbElem = eID;
                        }
                    }
                }
                string facestr;
                
                int[] marks = mesh.GetBElementsBCMark();
                boundaryFacets = new AFacet[belems.Length];
                FacetsLen = new double[belems.Length];
      
                for (int f = 0; f < boundaryFacets.Length; f++)
                {
                    int i0 = (int)belems[f].Vertex1;
                    int i1 = (int)belems[f].Vertex2;
                    facestr = AFacet.GetFaceHash(i0, i1);
                    IAFacet facet = dictionary[facestr];
                    facet.BoundaryFacetsMark = marks[f];
                    boundaryFacets[f] = facet;
                    FacetsLen[f] = Math.Sqrt((X[i0] - X[i1]) * (X[i0] - X[i1]) + (Y[i0] - Y[i1]) * (Y[i0] - Y[i1]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Расчет минимального растояния от центра КЭ сопряженного с граничным элементом 
        /// </summary>
        public void CalkDistance()
        {
            MEM.Alloc(boundaryFacets.Length, ref Distance);
            for (int f = 0; f < boundaryFacets.Length; f++)
            {
                int elemID = boundaryFacets[f].OwnerElem;
                int i0 = (int)belems[f].Vertex1;
                int i1 = (int)belems[f].Vertex2;

                int e0 = (int)elems[elemID].Vertex1;
                int e1 = (int)elems[elemID].Vertex2;
                int e2 = (int)elems[elemID].Vertex2;

                HPoint pBE = new HPoint((X[i0] + X[i1]) / 2, (Y[i0] + Y[i1]) / 2);
                HPoint pFE = new HPoint((X[e0] + X[e1] + X[e2]) / 3, (Y[e0] + Y[e1] + Y[e2]) / 3);
                double dis = (pFE - pBE).Length();
                if (BoundElementIndex[elemID] == -1)
                {
                    Distance[f] = dis;
                    BoundElementIndex[elemID] = f;
                }
                else
                {
                    if (Distance[f] > dis)
                    {
                        Distance[f] = dis;
                        BoundElementIndex[elemID] = f;
                    }
                }
            }
        }
    }
}