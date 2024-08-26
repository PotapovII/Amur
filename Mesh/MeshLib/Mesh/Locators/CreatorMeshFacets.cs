//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 19.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.Mesh.Locators
{
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// создатель массива ребер сетки 
    /// </summary>
    public class CreatorMeshFacets
    {
        /// <summary>
        /// массива ребер сетки 
        /// </summary>
        public ISFacet[] facets = null;
        /// <summary>
        /// сетка
        /// </summary>
        protected IRenderMesh mesh;
        public CreatorMeshFacets(IRenderMesh mesh)
        {
            this.mesh = mesh;
            CreatorFacets();
        }
        /// <summary>
        /// формирование массива ребер сетки 
        /// </summary>
        protected void CreatorFacets()
        {
            try
            {
                Dictionary<string, SFacet> dictionary = new Dictionary<string, SFacet>();
                TriElement[] elems = mesh.GetAreaElems();
                int cu = 3;
                for (int elem = 0; elem < elems.Length; elem++)
                {
                    for (int i = 0; i < cu; i++)
                    {
                        SFacet facet = new SFacet((int)elems[elem][i], (int)elems[elem][(i + 1) % cu]);
                        string faceеHash = facet.Hash();
                        if (dictionary.ContainsKey(faceеHash) == false)
                        {
                            facet.BoundaryFacetsMark = 1;
                            dictionary.Add(faceеHash, facet);
                        }
                        else
                        {
                            SFacet facetA = dictionary[faceеHash];
                            facetA.BoundaryFacetsMark = 0;
                        }
                    }
                }
                MEM.Alloc(dictionary.Count, ref facets);
                int faceid = 0;
                foreach (var pair in dictionary)
                {
                    ISFacet facet = pair.Value;
                    facets[faceid++] = facet;
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Error(ex.Message, "CreatorFacets");
            }
        }
    }
}
