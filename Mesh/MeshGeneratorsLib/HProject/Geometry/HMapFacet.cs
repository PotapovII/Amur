namespace MeshGeneratorsLib
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using GeometryLib;
    using CommonLib;
    
    /// <summary>
    /// грань подобласти состоит из списка сегментов
    /// </summary>
    [Serializable]
    public class HMapFacet 
    {
        /// <summary>
        /// Вспомогательное поле
        /// </summary>
        public int ID;
        /// <summary>
        /// список сегментов грани
        /// </summary>
        public List<HMapSegment> Segments = new List<HMapSegment>();
        public HMapFacet(int ID=0) { this.ID = ID; }
        public HMapFacet(HMapSegment segment,int FID=0)
        {
            ID = FID;
            Segments.Add(new HMapSegment(segment));
        }
        //---------------------------------------------------------------------------
        public HMapFacet(int FID, List<VMapKnot> Knots, int ID = 0, int MarkBC = 0)
        {
            ID = FID;
            Add(Knots, ID, MarkBC);
        }
        //---------------------------------------------------------------------------
        public HMapFacet(HMapFacet a)
        {
            if (a == null)
                return;
            this.ID = a.ID;
            Segments.Clear();
            foreach (HMapSegment s in a.Segments)
                Segments.Add(new HMapSegment(s));
        }
        /// <summary>
        /// Ключ для словарей
        /// </summary>
        /// <returns></returns>
        public string GetHash()
        {
            string hesh = "";
            if(Segments.Count == 1)
                hesh = Segments[0].GetHash();
            else
                hesh = Segments[0].GetHash() + "_" + Segments[Segments.Count-1].GetHash();
            ////List<int> idts = new List<int>();
            ////for(int i = 0; i < Segments.Count; i++)
            ////    idts.Add(Segments[i].ID);
            ////idts.Sort();
            //int[] ids = Segments.Select(s => s.ID).ToArray();
            //Array.Sort(ids);
            //foreach (int s in ids)
            //    hesh+=s.ToString() + "_";
            return hesh;
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// Количество узлов в ребре
        /// </summary>
        /// <returns></returns>
        public int CountKnots()
        {
            int knots = 0;
            for (int sg = 0; sg < Segments.Count; sg++)
                knots += Segments[sg].Count;
            return (knots - Segments.Count + 1);
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// Добавление сигмента в грань
        /// </summary>
        /// <param name="segment"></param>
        public void Add(HMapSegment segment)
        {
            Segments.Add(segment);
        }
        //---------------------------------------------------------------------------
        /// <summary>
        /// Добавление сигмента в грань
        /// </summary>
        /// <param name="segment"></param>
        public void Add(List<VMapKnot> Knots, int ID = 0, int MarkBC = 0)
        {
            HMapSegment ms = new HMapSegment(Knots, ID, MarkBC);
            Segments.Add(ms);
        }
        //---------------------------------------------------------------------------
        ///// <summary>
        ///// Возвращаем суммарный сегмент грани
        ///// пологаем что сегменты грани записаны последовательно
        ///// </summary>
        ///// <returns></returns>
        //public HMapSegment GetAllSegment()
        //{
        //    HMapSegment ms = new HMapSegment(Segments[0]);
        //    for (int i = 1; i< Segments.Count; i++)
        //    {
        //        for(int n = 1; n< Segments[i].Count; n++)
        //            ms.Add(Segments[i].Knots[n]);
        //    }
        //    return ms;
        //}
        ////---------------------------------------------------------------------------
        //public VMapKnot Start()
        //{
        //    HMapSegment ms = new HMapSegment(Segments[0]);
        //    int Count = Segments[0].Count;
        //    return Segments[0].Knots[Count-1];
        //}
        ////---------------------------------------------------------------------------
        //public void Clear()
        //{
        //    Segments.Clear(); ID = 0; Type = 0;
        //}
        ////---------------------------------------------------------------------------
        //double Length(int Precision)
        //{
        //    double Length = 0;
        //    for (int i = 0; i < Segments.Count; i++)
        //        Length += Segments[i].Length(Precision);
        //    return Length;
        //}
    }
}
