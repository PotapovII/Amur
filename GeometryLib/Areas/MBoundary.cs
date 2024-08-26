//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 22.12.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.Areas
{
    using CommonLib.Areas;
    using System.Collections.Generic;
    /// <summary>
    /// Метка границы для заданной фигуры (список сегментов)
    /// </summary>
    public class MBoundary : IMBoundary
    {
        /// <summary>
        /// Индекс метки границы
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// наименование фигуры
        /// </summary>
        public string FiguraName { get; set; }
        /// <summary>
        /// Индекс фигуры
        /// </summary>
        public int FID { get; set; }
        /// <summary>
        /// список индексов сегментов
        /// </summary>
        public List<int> SegmentIndex { get; set; }
        /// <summary>
        /// Имя метки границы
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Сортиновка по имени метки
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IMBoundary obj)
        {
            return this.Name.CompareTo(obj.Name);
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public MBoundary(string Name, string FiguraName, int ID, int FID, int[] mas)
        {
            this.Name = Name;
            this.FiguraName = FiguraName;
            this.ID = ID;
            this.FID = FID;
            SegmentIndex = new List<int>();
            SegmentIndex.AddRange(mas);
        }
        
        public MBoundary(IMFigura fig, int N)
        {
            this.Name = fig.Name + "_BM"+N.ToString();
            this.FiguraName = fig.Name;
            this.ID = N;
            this.FID = fig.FID;
            SegmentIndex = new List<int>();
            SegmentIndex.AddRange(fig.GetSegmentIndexs());
        }

        public MBoundary(MBoundary b)
        {
            this.Name = b.Name;
            this.FiguraName = b.FiguraName;
            this.ID = b.ID;
            this.FID = b.FID;
            SegmentIndex = new List<int>();
            SegmentIndex.AddRange(b.SegmentIndex.ToArray());
        }
    }
}
