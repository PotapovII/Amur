//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 03.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Linq;
    using MemLogLib;
    using CommonLib;
    using GeometryLib;
    using CommonLib.Geometry;

    /// <summary>
    /// КО элемент
    /// </summary>
    [Serializable]
    public class FVElement : IFVElement
    {
        /// <summary>
        /// Тип функции формы основного КЭ 
        /// </summary>
        public TypeFunForm TFunForm { get; set; }
        /// <summary>
        /// грани КО
        /// </summary>
        public IFVFacet[] Facets { get=>facets; set=> facets=value; }
        /// <summary>
        /// грани КО
        /// </summary>
        public IFVFacet[] facets;
        /// <summary>
        /// ближайшие КО
        /// </summary>
        public IFVElement[] NearestElements { get => nearestElements; set => nearestElements = value; }
        /// <summary>
        /// ближайшие КО
        /// </summary>
        public IFVElement[] nearestElements;
        /// <summary>
        ///  Координаты вершин/от левый нижний против часовой
        /// </summary>
        public HPoint[] Vertex { get=> vertex; set=> vertex=value; } 
        /// <summary>
        ///  Координаты вершин/от левый нижний против часовой
        /// </summary>
        public HPoint[] vertex;
        /// <summary>
        ///  Узлы вершин треугольника
        /// </summary>
        public int[] Nodes { get=> nodes; set=> nodes=value; }
        /// <summary>
        ///  Узлы вершин треугольника
        /// </summary>
        public int[] nodes;
        /// <summary>
        /// вектор  от центра КО до центров ближайших
        /// </summary>
        public HPoint[] VecDistance { get=> vecDistance; set=> vecDistance=value; }
        /// <summary>
        /// вектор  от центра КО до центров ближайших
        /// </summary>
        public HPoint[] vecDistance;
        /// <summary>
        /// Растояние от центра КО до центров ближайших КО
        /// </summary>
        public double[] Distance { get=> distance; set=> distance=value; }
        /// <summary>
        /// Растояние от центра КО до центров ближайших
        /// </summary>
        public double[] distance;
        /// <summary>
        /// вектор от центра до центра граней
        /// </summary>
        public HPoint[] VecFacetsDistance { get=> vecFacetsDistance; set=> vecFacetsDistance=value; }
        /// <summary>
        /// вектор от центра до центра граней
        /// </summary>
        public HPoint[] vecFacetsDistance;
        /// <summary>
        /// вектор от центра соседа до центра граней
        /// </summary>
        public HPoint[] VecNearFacetsDistance { get=> vecNearFacetsDistance; set=> vecNearFacetsDistance=value; }
        /// <summary>
        /// вектор от центра соседа до центра граней
        /// </summary>
        public HPoint[] vecNearFacetsDistance;
        /// <summary>
        /// Длина вектор от центра соседа до центра граней
        /// </summary>
        public double[] NearFacetsDistance { get=> nearFacetsDistance; set=> nearFacetsDistance=value; }
        /// <summary>
        /// Длина вектор от центра соседа до центра граней
        /// </summary>
        public double[] nearFacetsDistance;
        /// <summary>
        ///  Коэффицент КО схемы 0-k грани для продольной диффузии
        /// -  длина k - го ребра / расстояние между центрам КО и центром i - го КО
        /// </summary>
        public double[] Ak { get=>ak; set=> ak=value; }
        /// <summary>
        ///  Коэффицент КО схемы 0-k грани для продольной диффузии
        /// -  длина k - го ребра / расстояние между центрам КО и центром i - го КО
        /// </summary>
        public double[] ak;
        /// <summary>
        /// Сумма всех ak
        /// </summary>
        public double Ap { get; set; }
        /// <summary>
        ///  Коэффицент КО схемы 0-k грани для поперечной диффузии
        /// </summary>
        public double[] ACrossk { get=>aCrossk; set=> aCrossk=value; }
        /// <summary>
        ///  Коэффицент КО схемы 0-k грани для поперечной диффузии
        /// </summary>
        public double[] aCrossk;
        /// <summary>
        /// ценр КО
        /// </summary>
        public  HPoint Centroid { get; set; } 
        /// <summary>
        /// Номер КО
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Площадь КО
        /// </summary>
        public double Volume { get; set; }

        public FVElement(int cu = 3)
        {
            Centroid = new HPoint();
            TFunForm = TypeFunForm.Form_2D_Triangle_L1;
            MEM.Alloc(cu, ref facets);
            MEM.Alloc(cu, ref nearestElements);
            MEM.Alloc(cu, ref vertex);
            MEM.Alloc(cu, ref nodes);
            MEM.Alloc(cu, ref vecDistance);
            MEM.Alloc(cu, ref vecFacetsDistance);
            MEM.Alloc(cu, ref vecNearFacetsDistance);
            MEM.Alloc(cu, ref nearFacetsDistance);
            MEM.Alloc(cu, ref distance);
            MEM.Alloc(cu, ref ak);
            MEM.Alloc(cu, ref aCrossk);
            Id = -1;
        }


        /// <summary>
        /// Возвращает треугольные элементы в формате TriElement
        /// </summary>
        /// <returns></returns>
        public TriElement[] Nods()
        {
            TriElement[] e = null;
            if (vertex.Length == 3)
            {
                e = new TriElement[1];
                e[0].Vertex1 = (uint)nodes[0];
                e[0].Vertex2 = (uint)nodes[1];
                e[0].Vertex3 = (uint)nodes[2];
            }
            else
            {
                e = new TriElement[2];
                e[0].Vertex1 = (uint)nodes[0];
                e[0].Vertex2 = (uint)nodes[1];
                e[0].Vertex3 = (uint)nodes[2];

                e[1].Vertex1 = (uint)nodes[2];
                e[1].Vertex2 = (uint)nodes[3];
                e[1].Vertex3 = (uint)nodes[0];
            }
            return e;
        }
        /// <summary>
        /// Возврашает индекс грани в контрольном объеме по объекту грани
        /// и -1 если грань не пренадлежит КО
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public int GetFacetIndex(IFVFacet f)
        {
            int cu = nodes.Length;
            for (int i=0; i< cu;i++)
            {
                if ((nodes[i] == f.Pointid1 && nodes[(i + 1) % cu] == f.Pointid2) ||
                    (nodes[i] == f.Pointid2 && nodes[(i + 1) % cu] == f.Pointid1))
                    return i;
            }
            return -1;
        }

        public int FaceLocalId(IFVFacet facet)
        {
            for (int i = 0; i < facets.Length; i++)
                if (facets[i] == facet)
                    return i;
            return -1;
        }
        
        /// <summary>
        /// Расчет постоянных параметров контрольного объема
        /// </summary>
        public void InitElement()
        {
            Centroid = GEO.PolygonCentroid(vertex);
            Volume = GEO.PolygonArea(vertex);
            // цикл по граням
            for (int idx = 0; idx < vertex.Length; idx++)
            {
                if (facets[idx].Owner == this)
                    facets[idx].InitFacet();

                
                vecFacetsDistance[idx] = facets[idx].Centroid - Centroid;
                nearFacetsDistance[idx] = vecFacetsDistance[idx].Length();
                
                IFVElement nb = nearestElements[idx];
                if (nb != null)
                {
                    vecDistance[idx] = nb.Centroid - Centroid;
                    distance[idx] = vecDistance[idx].Length();
                    vecNearFacetsDistance[idx] = nb.Centroid - facets[idx].Centroid;
                }
                else
                {
                    vecDistance[idx] =  facets[idx].Centroid - Centroid;
                    distance[idx] = vecDistance[idx].Length();
                }

                IFVFacet facet = facets[idx];
                if (facet.BoundaryFacetsMark != -1)
                {
                    if (facet.BoundaryFacetsMark == 0)
                    {
                        ak[idx] = facet.Length / nearFacetsDistance[idx];
                        // вклад кросс - диффузии
                        HPoint normalDistance = (new HPoint(vecFacetsDistance[idx])).GetOrt();
                        aCrossk[idx] = facet.Normal * normalDistance;
                    }
                    else
                    if (facet.BoundaryFacetsMark == 1)
                    {
                        ak[idx] = 0;
                    }
                }
                else
                {
                    ak[idx] = facet.Length / distance[idx];
                    HPoint normalDistance = (new HPoint(vecDistance[idx])).GetOrt();
                    aCrossk[idx] = facet.Normal * normalDistance;
                }
            }
            Ap = ak.Sum();
        }

    }
}
