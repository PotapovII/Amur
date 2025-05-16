//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 29.11.2023 Потапов И.И. + работа с CloudKnot
//---------------------------------------------------------------------------
namespace GeometryLib.Areas
{
    using CommonLib.Areas;
    using CommonLib.Geometry;
    using System.Collections.Generic;
    using System.IO;
    using TestsUtils;

    /// <summary>
    /// Контурная фигура
    /// </summary>
    public class Figura : IMFigura
    {
        /// <summary>
        /// Номер фигуры
        /// </summary>
        public int FID { get; set; }
        /// <summary>
        /// Количество точек
        /// </summary>
        public int Count => points.Count;
        /// <summary>
        /// Наоменование точек
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public FigureStatus Status { get; set; }
        /// <summary>
        /// Тип фигуры
        /// </summary>
        public FigureType FType { get; set; }
        #region Атрибуты фигуры
        /// <summary>
        /// Свойства в узлах (состояние контексное) от задачи
        /// </summary>
        // double[] Attributes { get; set; }
        /// <summary>
        /// Толщина льда
        /// </summary>
        public double Ice { get; set; }
        /// <summary>
        /// шероховатость дна
        /// </summary>
        public double ks { get; set; }
        #endregion
        /// <summary>
        /// Точки контура
        /// </summary>
        public List<IMPoint> Points => points;
        /// <summary>
        /// Точки контура
        /// </summary>
        protected List<IMPoint> points = new List<IMPoint>();
        /// <summary>
        /// Список сегментов фигуры
        /// </summary>
        public List<IMSegment> Segments
        {
            get
            {
                if (segmentStatus == false)
                    CreateSegments();
                return segments;
            }
        }
        /// <summary>
        /// Список сегментов фигуры
        /// </summary>
        protected List<IMSegment> segments = new List<IMSegment>();
        /// <summary>
        /// Флаг отложенной инициализации
        /// </summary>
        protected bool segmentStatus = false;
        /// <summary>
        /// Изменение координат
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="p"></param>
        public void SetPoint(int idx, IMPoint p)
        {
            points[idx % points.Count] = p;
        }
        /// <summary>
        /// Получить узел
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public IMPoint GetPoint(int idx)
        {
            return points[idx % points.Count];
        }
        /// <summary>
        /// Получить сегмент
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public IMSegment GetSegment(int idx)
        {
            if (segmentStatus == false)
                CreateSegments();
            return segments[idx % segments.Count];
        }
        public Figura(string Name,int FID, List<HPoint> ps, 
            FigureType ft = FigureType.FigureContur,
            double ks = 0.1, double Ice = 0)
        {
            this.FID = FID;
            this.Name = Name;
            this.Status = FigureStatus.UnselectedShape;
            this.FType = ft;
            this.ks = ks;
            this.Ice = Ice;
            for (int i = 0; i < ps.Count; i++)
            {
                string name = "point" + i.ToString();
                IMPoint pp = new MPoint(name, ps[i], this);
                this.points.Add(pp);
            }
        }
        public Figura(string Name, int FID, List<CloudKnot> ps, 
                    FigureType ft = FigureType.FigureContur,
                    double ks = 0.1, double Ice = 0)
        {
            this.FID = FID;
            this.Name = Name;
            this.Status = FigureStatus.UnselectedShape;
            this.FType = ft;
            this.ks = ks;
            this.Ice = Ice;
            for (int i = 0; i < ps.Count; i++)
            {
                string name = "point" + i.ToString();
                IMPoint pp = new MPoint(name, ps[i], this);
                this.points.Add(pp);
            }
        }
        public Figura(Figura fig, bool ext = false) 
        {
            this.FID = fig.FID;
            this.Name = fig.Name;
            this.Status = fig.Status;
            this.FType = fig.FType;
            this.Ice = fig.Ice;
            this.ks = fig.ks;
            if (ext == false)
            {
                this.points.AddRange(fig.points);
            }
            else
            {
                List<IMSegment> segments = fig.Segments;
                List <IMPoint> newPoints = new List<IMPoint>();
                int k = 0;
                for (int i = 0; i < segments.Count; i++)
                {
                    if (segments[i].CountKnots == 2)
                    {
                        string name = "point" + k.ToString();
                        k++;
                        IMPoint pp = new MPoint(name, segments[i].pointA.Point, this);
                        newPoints.Add(pp);
                        segments.Add(new MSegment(this, i));
                    }
                }
                this.points.AddRange(newPoints);
            }
        }

        protected void CreateSegments()
        {
            segments.Clear();
            for (int i = 0; i < points.Count; i++)
                segments.Add(new MSegment(this, i));
            segmentStatus = true;
        }
        /// <summary>
        /// Получить список индексов сегментов
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public int[] GetSegmentIndexs()
        {
            if (points.Count > 0 && segments.Count == 0)
                CreateSegments();
            int[] idx = new int[points.Count];
            for (int i = 0; i < points.Count; i++)
                idx[i] = segments[i].index;
            return idx;
        }
        /// <summary>
        /// Взять/дать узел в фигуре
        /// </summary>
        public IMPoint this[int index]
        {
            get => points[(int)index];
            set => points[(int)index] = value;
        }
        /// <summary>
        /// Добавить точку в контур
        /// </summary>
        /// <param name="point"></param>
        public void Add(IHPoint point)
        {
            string name = "Point" + points.Count.ToString();
            IMPoint p = new MPoint(name, point, this);
            points.Add(p);
            segmentStatus = false;
        }
        public void Add(CloudKnot point)
        {
            string name = "Point" + points.Count.ToString();
            IMPoint p = new MPoint(name, point, this);
            points.Add(p);
            segmentStatus = false;
        }
        /// <summary>
        /// Добавить точку в контур
        /// </summary>
        /// <param name="point"></param>
        public void Add(IMPoint point)
        {
            points.Add(new MPoint(point));
            segmentStatus = false;
        }
        /// <summary>
        /// Убрать точку из контур
        /// </summary>
        public void Remove(IMPoint point)
        {
            points.Remove(point);
            segmentStatus = false;
        }
        /// <summary>
        /// Очистить контур
        /// </summary>
        public void Clear()
        {
            points.Clear();
            segments.Clear();
            segmentStatus = false;
        }

        /// <summary>
        /// Принадлежит ли точка полигону фигуры
        /// </summary>
        public bool Contains(IHPoint point)
        {
            return LocPolygon2D.Contains(point, Points);
        }
        /// <summary>
        /// Принадлежит ли точка полигону фигуры
        /// </summary>
        public bool Contains(double x, double y)
        {
            IHPoint point = new HPoint(x, y);
            return LocPolygon2D.Contains(point, Points);
        }
        ///// <summary>
        ///// Сохранение области
        ///// </summary>
        ///// <param name="Filter"></param>
        ///// <returns></returns>
        //public string ToString(string Filter = "F15")
        //{
        //    string str = FID.ToString();
        //    str += " " + Name;
        //    str += " " + ((int)FType).ToString();
        //    str += " " + (points.Count).ToString();
        //    str += " " + Ice.ToString("F6");
        //    str += " " + ks.ToString("F6");
        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        IMPoint iknot = points[i];
        //        CloudKnot knot = iknot as CloudKnot;
        //        if (knot != null)
        //        {
        //            str += " " + points[i].ToString();
        //        }
        //        else 
        //        {
        //            MPoint ph = iknot as MPoint;
        //            if (ph != null)
        //                str += " " + ph.ToString();
        //        }
        //    }
        //    for (int i = 0; i < segments.Count; i++)
        //    {
        //        string ds = ((MSegment)segments[i]).ToString();
        //        str += " " + ds;
        //    }
        //    return str;
        //}
        //public static IMFigura Parse(string line)
        //{
        //    List<CloudKnot> ps = new List <CloudKnot>();
        //    string[] mas = (line.Trim()).Split(' ');
        //    int FID = int.Parse(mas[0]);
        //    string Name = mas[1];
        //    int FType = int.Parse(mas[2]);
        //    int Count = int.Parse(mas[3]);
        //    double Ice = double.Parse(mas[4]);
        //    double ks = double.Parse(mas[5]);
        //    for (int i = 0; i < Count; i++)
        //    {
        //        CloudKnot point = CloudKnot.Parse(mas[6 + i]);
        //        ps.Add(point);
        //    }
        //    Figura fig = new Figura(Name, FID, ps, (FigureType)FType);
        //    fig.CreateSegments();
        //    for (int i = 0; i < Count; i++)
        //    {
        //        fig.segments[i] = MSegment.Parse(mas[7 + Count + i], fig);
        //    }
        //    return fig;
        //}
        /// <summary>
        /// Запсь фигуры
        /// </summary>
        /// <param name="file"></param>
        public void WriteCloud(StreamWriter file)
        {
            string str = Name;
            str += " " + FID.ToString();
            str += " " + ((int)FType).ToString();
            str += " " + (points.Count).ToString();
            str += " " + Ice.ToString("F6");
            str += " " + ks.ToString("F6");
            file.WriteLine(str);
            for (int i = 0; i < points.Count; i++)
            {
                IMPoint iknot = points[i];
                str = ((CloudKnot)(iknot).Point).ToString();
                file.WriteLine(str);
            }
            for (int i = 0; i < segments.Count; i++)
            {
                str = ((MSegment)segments[i]).ToString();
                file.WriteLine(str);
            }
        }
        /// <summary>
        /// Чтение фигуры из потока
        /// </summary>
        /// <param name="file"></param>
        public static IMFigura Read(StreamReader file)
        {
            List<CloudKnot> ps = new List<CloudKnot>();
            string line = file.ReadLine();
            string[] mas = (line.Trim()).Split(' ');
            
            string Name = mas[0];
            int FID = int.Parse(mas[1]);
            int FType = int.Parse(mas[2]);
            int Count = int.Parse(mas[3]);
            double Ice = double.Parse(mas[4]);
            double ks = double.Parse(mas[5]);

            for (int i = 0; i < Count; i++)
            {
                line = file.ReadLine();
                CloudKnot point = CloudKnot.Parse(line);
                ps.Add(point);
            }
            Figura fig = new Figura(Name, FID, ps, (FigureType)FType, ks, Ice);
            fig.CreateSegments();
            for (int i = 0; i < Count; i++)
            {
                line = file.ReadLine();
                fig.segments[i] = MSegment.Parse(line, fig);
            }
            return fig;
        }
    }
}
