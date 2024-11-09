//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 06.08.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    /// <summary>
    /// Класс ребро КО сетки 1 порядка (геометрические параметры ребра и ссылки)
    /// </summary>
    [Serializable]
    public class AFacet : IAFacet
    {
        /// <summary>
        /// Новер грани
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 1 узел
        /// </summary>
        public int Pointid1 { get; set; }
        /// <summary>
        /// второй узел
        /// </summary>
        public int Pointid2 { get; set; }
        /// <summary>
        /// Владелец
        /// </summary>
        public int OwnerElem { get; set; }
        /// <summary>
        /// Совладелец
        /// </summary>
        public int NbElem { get; set; }
        /// <summary>
        /// метка границы
        /// </summary>
        public int BoundaryFacetsMark { get; set; }
        public AFacet(int id, int pointid1, int pointid2, int owner,
                      int boundaryFacetsMark)
        {
            this.Id = id;
            this.Pointid1 = pointid1;
            this.Pointid2 = pointid2;
            this.OwnerElem = owner;
            this.NbElem = -1;
            this.BoundaryFacetsMark = boundaryFacetsMark;
        }
        public AFacet(AFacet p)
        {
            Id = p.Id;
            Pointid1 = p.Pointid1;
            Pointid2 = p.Pointid2;
            OwnerElem = p.OwnerElem;
            NbElem = p.NbElem;
            BoundaryFacetsMark = p.BoundaryFacetsMark;
        }

        ///// <summary>
        ///// Вычисление Хеша ребра для словаря
        ///// </summary>
        ///// <param name="p1index"></param>
        ///// <param name="p2index"></param>
        ///// <returns></returns>
        public static string GetFaceHash(int p1index, int p2index)
        {
            if (p1index > p2index)
                return p2index.ToString() + "_" + p1index.ToString();
            else
                return p1index.ToString() + "_" + p2index.ToString();
        }
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
    }
}
