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
    using CommonLib;
    using CommonLib.Geometry;
    using GeometryLib;
    /// <summary>
    /// Класс ребро КО (геометрические параметры ребра и ссылки)
    /// </summary>
    [Serializable]
    public class FVFacet : IFVFacet
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
        public IFVElement Owner { get; set; }
        /// <summary>
        /// Совладелец
        /// </summary>
        public IFVElement NBElem { get; set; }
        /// <summary>
        /// Владелец
        /// </summary>
        public int OwnerElem { get=> Owner.Id; set=>Owner.Id=value; }
        /// <summary>
        /// Совладелец
        /// </summary>
        public int NbElem { get=> NBElem.Id; set=> NBElem.Id=value; }
        /// <summary>
        /// Координаты грани
        /// </summary>
        public HPoint[] Vertex { get; set; }
        /// <summary>
        /// Вектор грани
        /// </summary>
        public HPoint FVertex { get; set; }
        /// <summary>
        /// центр грани
        /// </summary>
        public HPoint Centroid { get; set; }
        /// <summary>
        /// нормаль к грани
        /// </summary>
        public HPoint Normal { get; set; }
        /// <summary>
        /// Вектор нормальный к грани
        /// </summary>
        public HPoint FaceNormal { get; set; }
        /// <summary>
        /// Коэффициент интерполяции для сопряженных на грань узлов
        /// </summary>
        public double Alpha { get; set; }
        /// <summary>
        /// длина грани
        /// </summary>
        public double Length { get; set; }
        /// <summary>
        /// метка границы
        /// </summary>
        public int BoundaryFacetsMark { get; set; }

        public void Print()
        {
            Console.WriteLine(" Id ={0},  nodA = {1},  nodB = {2} Owner = {3} NBElem = {4} ",
                Id, Pointid1, Pointid2, Owner!=null, NBElem!=null );
        }
    
        public TwoElement Nods()
        {
            TwoElement e = new TwoElement();
            e.Vertex1 = (uint)Pointid1;
            e.Vertex2 = (uint)Pointid2;
            return e;
        }
        public FVFacet(HPoint v1, HPoint v2)
        {
            Vertex = new HPoint[2];
            Vertex[0] = v1;
            Vertex[1] = v2;
            BoundaryFacetsMark = -1;
        }
        /// <summary>
        /// Вычисление Хеша ребра для словаря
        /// </summary>
        /// <param name="p1index"></param>
        /// <param name="p2index"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Вычисление характеристик ребра
        /// </summary>
        public void InitFacet()
        {
            FVertex = Vertex[1] - Vertex[0];
            Length = FVertex.Length();
            Centroid = 0.5 * (Vertex[0] + Vertex[1]);
            Normal = GEO.GetNormalOrt(Vertex[0], Vertex[1]);
            FaceNormal = GEO.GetNormal(Vertex[0], Vertex[1]);
            
            if (NBElem != null)
            {
                // сильно упрощенный коэффицент интерполирования
                Alpha = Owner.Volume / (Owner.Volume + NBElem.Volume);
                // расчет коэффициента температурапроводности на грани
                //k = (Owner.k * NBElem.k) / ((1 - Alpha) * Owner.k + Alpha * NBElem.k);
            }
            else
            {
                Alpha = 0;
               // k = Owner.k;
            }
        }
    }
}
