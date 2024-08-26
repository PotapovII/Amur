namespace MeshGeneratorsLib
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    /// <summary>
    /// Определяет контур подобласти
    /// </summary>
    [Serializable]
    public class HMapSubArea 
    {
        /// <summary>
        /// id пообласти 
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// id пообласти 
        /// </summary>
        public int SubType { get; set; }

        /// <summary>
        /// Данные для контура
        /// </summary>
        public List<HMapFacet> Facets = new List<HMapFacet>();
        public HMapSubArea(int ID = 0, int subType =0)
        {
            this.ID = ID;
            this.SubType = subType;
        }
        public HMapSubArea(HMapSubArea msa)
        {
            if (msa == null)
                return;
            ID = msa.ID;
            SubType = msa.SubType;
            foreach (HMapFacet s in msa.Facets)
                Facets.Add(new HMapFacet(s));
        }
        public void Add(HMapFacet facet)
        {
            Facets.Add(facet);
        }
        public void Clear()
        {
            Facets.Clear();
        }
    }
}
