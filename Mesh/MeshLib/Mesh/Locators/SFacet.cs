namespace MeshLib.Mesh.Locators
{
    using CommonLib;
    /// <summary>
    /// Класс постое ребро
    /// </summary>
    internal class SFacet : ISFacet
    {
        /// <summary>
        /// 1 узел
        /// </summary>
        public int Pointid1 { get; set; }
        /// <summary>
        /// второй узел
        /// </summary>
        public int Pointid2 { get; set; }
        /// <summary>
        /// Метка границы 
        /// </summary>
        public int BoundaryFacetsMark { get; set; }
        /// <summary>
        /// Вычисление Хеша ребра для словаря
        /// </summary>
        /// <param name="p1index"></param>
        /// <param name="p2index"></param>
        /// <returns></returns>
        public string Hash()
        {
            if (Pointid1 > Pointid2)
                return Pointid2.ToString() + "_" + Pointid1.ToString();
            else
                return Pointid1.ToString() + "_" + Pointid2.ToString();
        }
        public SFacet(ISFacet p) 
        {
            Pointid1 = p.Pointid1;
            Pointid2 = p.Pointid2;
            BoundaryFacetsMark = p.BoundaryFacetsMark;
        }
        public SFacet(int pointid1, int pointid2,int markBC = 0)
        {
            Pointid1 = pointid1;
            Pointid2 = pointid2;
            BoundaryFacetsMark = markBC;
        }
        public SFacet()
        {
            Pointid1 = -1;
            Pointid2 = -1;
            BoundaryFacetsMark = 0;
        }
    }
}
