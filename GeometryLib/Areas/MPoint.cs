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
    using GeometryLib;
    using MemLogLib;
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Узел фигуры
    /// </summary>
    public class MPoint : IMPoint
    {
        /// <summary>
        /// Координата по х
        /// </summary>
        public double X { get => Point.X; set => Point.X = value; }
        /// <summary>
        /// Координата по y
        /// </summary>
        public double Y { get => Point.Y; set => Point.Y = value; }
        /// <summary>
        /// Наименование точек
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Статус фигуры
        /// </summary>
        public FigureStatus Status { get; set; }
        /// <summary>
        /// Координаты точки
        /// </summary>
        public IHPoint Point { get; set; }

        /// <summary>
        /// Связь с фигурами
        /// </summary>
        public List<IMFigura> FigLink => figs;
        protected List<IMFigura> figs = new List<IMFigura>();
        public MPoint()
        {
            Name = "no name point";
            Point = new HPoint(0, 0);
        }
        public MPoint(IMPoint p)
        {
            this.Point = new HPoint( p.Point );
            this.Name = p.Name;
            this.Status = p.Status;
            figs.AddRange(p.FigLink);
        }
        public MPoint(string Name, IHPoint p, IMFigura fig = null)
        {
            this.Point = new HPoint(p);
            this.Name = Name;
            this.Status = FigureStatus.UnselectedShape;
            figs.Add(fig);
        }
        public MPoint(string Name, HPoint p, IMFigura fig = null)
        {
            this.Point = new HPoint( p );
            this.Name = Name;
            this.Status = FigureStatus.UnselectedShape;
            figs.Add(fig);
        }
        public MPoint(string Name, CloudKnot p, IMFigura fig = null)
        {
            this.Point = new CloudKnot( p );
            this.Name = Name;
            this.Status = FigureStatus.UnselectedShape;
            figs.Add(fig);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public IHPoint IClone() => new MPoint(this);

        public int CompareTo(object obj)
        {
            MPoint a = obj as MPoint;
            if (a != null)
            {
                if (X < a.X)
                    return -1;
                if (X > a.X)
                    return 1;
                return 0;
            }
            else
                throw new Exception("ошибка приведения типа");
        }

        //public string ToString(string Filter = "F15")
        //{
        //    string str = Name;
        //    //if (Point as CloudKnot != null)
        //    str += " " + ((CloudKnot)Point).ToString();
        //    //else
        //    //    str += " " + ((HPoint)Point).ToString();
        //    str += " " + ((int)Status).ToString();
        //    return str;
        //}

        //public static MPoint Parse(string line, IMFigura fig = null)
        //{
        //    string[] mas = (line.Trim()).Split(' ');
        //    string Name = mas[0];
        //    int FType = int.Parse(mas[1]);
        //    int Count = int.Parse(mas[2]);
        //    CloudKnot point = CloudKnot.Parse(mas[4 + i]);
        //    MPoint p = new MPoint(Name, point, fig);
        //    return p;
        //}
    }
}
