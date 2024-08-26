////---------------------------------------------------------------------------
////                          ПРОЕКТ  "МКЭ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 03.07.2022 Потапов И.И.
////---------------------------------------------------------------------------
//namespace MeshLib
//{
//    using System;
//    using GeometryLib;
//    using CommonLib;
//    using System.Collections.Generic;
//    /// <summary>
//    /// КО сетка задачи
//    /// </summary>
//    [Serializable]
//    public class FVMTriМesh
//    {
//        #region базисные массывы данных
//        /// <summary>
//        /// Координаты узлов
//        /// </summary>
//        public double[][] points = null;
//        /// <summary>
//        /// Массив связности
//        /// </summary>
//        public double[][] polys = null;
//        /// <summary>
//        /// Массив границ
//        /// </summary>
//        public double[][] bound = null;
//        #endregion
//        /// <summary>
//        /// Количество элементов
//        /// </summary>
//        public int CountElements;
//        /// <summary>
//        ///  Контрольные объемы
//        /// </summary>
//        public FVElement[] AreaElems;
//        /// <summary>
//        /// Грани КО
//        /// </summary>
//        public FVFacet[] Facets;
//        /// <summary>
//        /// Граничные грани КО
//        /// </summary>
//        public FVFacet[] BoundaryFacets;
//        public FVMTriМesh() { }
//        public FVMTriМesh(double[][] points, double[][] polys, double[][] bound)
//        {
//            SetFVМesh(points, polys, bound);
//        }
//        /// <summary>
//        /// формирование сетки задачи
//        /// </summary>
//        /// <param name="points"></param>
//        /// <param name="polys"></param>
//        /// <param name="bound"></param>
//        public void SetFVМesh(double[][] points, double[][] polys, double[][] bound)
//        {
//            Dictionary<string, FVFacet> dictionary = new Dictionary<string, FVFacet>();
//            this.points = points;
//            this.polys = polys;
//            this.bound = bound;
//            CountElements = polys[0].Length;
//            int cu = polys.Length - 1;
//            AreaElems = new FVElement[CountElements];
//            int faceid = 0;
//            for (int eID = 0; eID < CountElements; eID++)
//            {
//                FVElement elem;
//                AreaElems[eID] = new FVElement(cu);
//                elem = AreaElems[eID];
//                elem.id = eID;
//                //elem.k = 1;
//                HPoint[] vertex = new HPoint[cu];
//                for (int vID = 0; vID < cu; vID++)
//                {
//                    int nodeA = (int)polys[vID][eID];
//                    int nodeB;
//                    elem.nodes[vID] = nodeA;
//                    vertex[vID] = new HPoint(points[0][nodeA], points[1][nodeA]);
//                    if (vID < cu - 1)
//                        nodeB = (int)polys[vID + 1][eID];
//                    else
//                        nodeB = (int)polys[0][eID];
//                    string faceеHash = FVFacet.GetFaceHash(nodeA, nodeB);
//                    if (!dictionary.ContainsKey(faceеHash))
//                    {
//                        FVFacet facet = new FVFacet(new HPoint(points[0][nodeA], points[1][nodeA]),
//                            new HPoint(points[0][nodeB], points[1][nodeB]));
//                        facet.id = faceid;
//                        facet.pointid1 = nodeB;
//                        facet.pointid2 = nodeB;
//                        facet.owner = elem;
//                        faceid++;
//                        dictionary.Add(faceеHash, facet);
//                        elem.Facets[vID] = facet;
//                    }
//                    else
//                    {
//                        FVFacet facet = dictionary[faceеHash];
//                        facet.nbelem = elem;
//                        elem.Facets[vID] = facet;
//                        FVElement owner = facet.owner;
//                        elem.NearestElements[vID] = owner;
//                        owner.NearestElements[owner.FaceLocalId(facet)] = elem;
//                    }
//                }
//                elem.vertex = vertex;
//                elem.PreCalc();
//            }
//            Facets = new FVFacet[dictionary.Count];
//            BoundaryFacets = new FVFacet[bound[0].Length];
//            foreach (var pair in dictionary)
//            {
//                FVFacet facet = pair.Value;
//                Facets[facet.id] = facet;
//            }
//            for (int eID = 0; eID < CountElements; eID++)
//                AreaElems[eID].InitElement();

//            for (int f = 0; f < bound[0].Length; f++)
//            {
//                string facestr = FVFacet.GetFaceHash((int)bound[0][f], (int)bound[1][f]);
//                FVFacet facet = dictionary[facestr];
//                facet.boundaryFacetsMark = (int)bound[2][f];
                
//                //facet.isboundary = true;
//                facet.boundaryType = TypeBoundCond.Dirichlet;
//                //facet.boundaryType = (BoundaryType)bound[4][f];
//                //facet.bndgroup = (int)bound[4][ f];

//                BoundaryFacets[f] = facet;
//            }
//        }
//    }
//}
