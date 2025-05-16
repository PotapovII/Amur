//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.Areas
{
    using System.Linq;
    using System.Drawing;
    using System.Collections.Generic;
    using CommonLib.Areas;

    public class MArea : IMArea
    {
        public MArea()
        {
        }
        /// <summary>
        /// установить статус фигур
        /// </summary>
        /// <param name="status"></param>
        public void SetFigureStatus(FigureStatus status)
        {
            foreach (var f in figures)
                f.Status = status;
        }
        /// <summary>
        /// установить типы фигур
        /// </summary>
        public void SetFigureTypes(FigureType type)
        {
            foreach (var f in figures)
                f.FType = type;
        }
        /// <summary>
        /// Список фигур
        /// </summary>
        List<IMFigura> figures = new List<IMFigura>();
        /// <summary>
        /// Количество фигур
        /// </summary>
        public int Count => figures.Count;
        /// <summary>
        /// Количество подобластей
        /// </summary>
        public int CountSubAreas => figures.Count(x => x.FType == FigureType.FigureSubArea);
        /// <summary>
        /// Количество дырок
        /// </summary>
        public int CountHoles => figures.Count(x => x.FType == FigureType.FigureHole);
        /// <summary>
        /// Список фигур
        /// </summary>
        public List<IMFigura> Figures => figures;
        /// <summary>
        /// Взять фигуру
        /// </summary>
        public IMFigura Get(int index) => figures[(int)index];
        /// <summary>
        /// Взять/дать фигуру 
        /// </summary>
        public IMFigura this[int index]
        {
            get => figures[(int)index];
            set => figures[(int)index] = value;
        }
        /// <summary>
        /// Регион
        /// </summary>
        RectangleWorld world = new RectangleWorld();
        /// <summary>
        /// Список фигур
        /// </summary>
        public List<string> Names
        {
            get
            {
                List<string> names = new List<string>();
                foreach (var f in figures)
                    names.Add(f.Name);
                return names;
            }
        }
        /// <summary>
        /// Добавить фигуру
        /// </summary>
        /// <param name="point"></param>
        public void Add(IMFigura fig)
        {
            if (figures.Count == 0)
                fig.FType = FigureType.FigureContur;
            //else
            //    fig.FType = FigureType.FigureHole;
            figures.Add(fig);
            int[] idx = fig.GetSegmentIndexs();
            int count = boundMark.Count;
            IMBoundary bm = new MBoundary(fig, count);
            boundMark.Add(bm);
        }
        /// <summary>
        /// Метки границ
        /// </summary>
        public List<IMBoundary> BoundMark => boundMark;
        List<IMBoundary> boundMark = new List<IMBoundary>();
        /// <summary>
        /// текущий интекс метки границ
        /// </summary>
        public int SelectIndexBoundMark { get; set; }
        /// <summary>
        /// Создание новой метки границ для текущей фигуры
        /// </summary>
        /// <param name="FID"></param>
        /// <param name="idx"></param>
        public void AddBoundMark(int FID, int[] idx)
        {
            string FiguraName = figures[FID].Name;
            int count = boundMark.Count(x => x.FiguraName == FiguraName);
            string mbName = FiguraName + "_" + "BM" + count.ToString();
            for (int i = 0; i < boundMark.Count; i++)
            {
                if (boundMark[i].FiguraName == FiguraName)
                {
                    foreach (var b in idx)
                        boundMark[i].SegmentIndex.Remove(b);
                }
            }
            int Allcount = boundMark.Count;
            IMBoundary bm = new MBoundary(mbName, FiguraName, Allcount, FID, idx);
            boundMark.Add(bm);
            SelectIndexBoundMark = boundMark.Count - 1;
        }
        /// <summary>
        /// удаление меток при удалении фигуры
        /// </summary>
        public void RemoveBoundMark(string FiguraName)
        {

            List<IMBoundary> bm = new List<IMBoundary>();
            for (int i = 0; i < boundMark.Count; i++)
                if (boundMark[i].FiguraName != FiguraName)
                    bm.Add(boundMark[i]);
            boundMark = bm;
            SelectIndexBoundMark = boundMark.Count - 1;
        }
        /// <summary>
        /// Получить индекс фигуры по имени метки
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetFigIndexByMarkName(string name)
        {
            for (int i = 0; i < Figures.Count; i++)
                if (Figures[i].Name == name)
                    return i;
            return -1;
        }
        /// <summary>
        /// Получить метку по имени метки
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IMBoundary GetMBoundaryByMarkName(string name)
        {
            SelectIndexBoundMark = -1;
            for (int i = 0; i < boundMark.Count; i++)
            {
                SelectIndexBoundMark = i;
                if (boundMark[i].Name == name)
                    return boundMark[i];
            }
            return null;
        }
        /// <summary>
        ///  Взять активную метку
        /// </summary>
        /// <returns></returns>
        public IMBoundary GetSelection()
        {
            if (boundMark.Count == 0)
                return null;
            else
            {
                if (boundMark.Count > 0 && SelectIndexBoundMark == -1)
                    SelectIndexBoundMark = boundMark.Count - 1;
                return boundMark[SelectIndexBoundMark];
            }

        }
        /// <summary>
        /// Установка индекса активной метки
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectIndex(int index)
        {
            SelectIndexBoundMark = index;
        }

        /// <summary>
        /// Убрать фигуру
        /// </summary>
        /// <param name="point"></param>
        public void Remove(IMFigura fig) => figures.Remove(fig);
        /// <summary>
        /// Очистить контур
        /// </summary>
        public void Clear()
        { 
            figures.Clear();
            boundMark.Clear();
            //AreaLinks.Clear();
            //BoundaryLinks.Clear();
        }
        /// <summary>
        /// Регион в котором расположены объекты
        /// </summary>
        /// <returns></returns>
        public RectangleWorld GetRegion()
        {
            foreach (var f in figures)
                foreach (var point in f.Points)
                    world.Extension(point.Point);
            return world;
        }
        /// <summary>
        /// Установить регион
        /// </summary>
        /// <param name="a">левая нижняя точка</param>
        /// <param name="b">правая верхняя точка</param>
        public void SetRegion(PointF a, PointF b) => world = new RectangleWorld(a, b);


        /// <summary>
        /// Контур области определен true, неопределен false
        /// </summary>
        /// <returns></returns>
        public bool Ready()
        {
            bool flag = false;
            foreach (var f in figures)
            {
                if(f.FType == FigureType.FigureContur)
                    flag = true; break;
            }
            return flag;
        }

        ///// <summary>
        ///// Связи м/д сеткой и полями для выполнения начальных условий
        ///// </summary>
        //public List<ILink> AreaLinks { get; set; }
        ///// <summary>
        ///// добавить связь /изменить значение связи
        ///// </summary>
        //public void AddAreaLinks(ILink link)
        //{
        //    AddLinks(AreaLinks, link);
        //}
        //public void DelAreaLinks(ILink link)
        //{
        //    DelAreaLinks(AreaLinks, link);
        //}
        //public void AddLinks<T>(List<T> Links, T link) where T : ILink
        //{
        //    bool flag = true;
        //    foreach (var al in Links)
        //        if (al.indexField == link.indexField && al.indexLink == link.indexLink)
        //        {
        //            if (al.ValueField != link.ValueField)
        //                al.ValueField = link.ValueField;
        //            flag = false; break;
        //        }
        //    if (flag == true)
        //        Links.Add(link);
        //}

        ///// <summary>
        ///// удалить связь 
        ///// </summary>
        //public void DelAreaLinks<T>(List<T> Links, T link) where T : ILink
        //{
        //    foreach (var al in Links)
        //        if (al.indexField == link.indexField && al.indexLink == link.indexLink)
        //        {
        //            Links.Remove(al);
        //            break;
        //        }
        //}
        ///// <summary>
        ///// Связи м/д сеткой и полями для выполнения граничных условий
        ///// </summary>
        //public List<IBoundaryLink> BoundaryLinks { get; set; }
        ///// <summary>
        ///// добавить связь /изменить значение связи
        ///// </summary>
        ///// <param name="link"></param>
        //public void AddBoundaryLinks(IBoundaryLink link)
        //{
        //    AddLinks(BoundaryLinks, link);
        //}
        ///// <summary>
        ///// удалить связь 
        ///// </summary>
        //public void DelBoundaryLinks(IBoundaryLink link)
        //{
        //    DelAreaLinks(BoundaryLinks, link);
        //}
    }
}
