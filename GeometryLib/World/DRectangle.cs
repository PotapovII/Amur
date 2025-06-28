namespace GeometryLib.World
{
    using System;
    using CommonLib.Geometry;
    /// <summary>
    /// Содержит набор из четырех целых чисел, 
    /// определяющих расположение и размер прямоугольника.
    /// </summary>
    public struct DRectangle
    {
        public double Xmin;
        public double Ymin;
        public double Xmax;
        public double Ymax;
        /// <summary>
        /// Ширина
        /// </summary>
        public double Width => Xmax - Xmin;
        /// <summary>
        /// Высота
        /// </summary>
        public double Height => Ymax - Ymin;
        public DRectangle(DRectangle r)
        {
            Xmin = r.Xmin;
            Ymin = r.Ymin;
            Xmax = r.Xmax;
            Ymax = r.Ymax;
        }
        public DRectangle(IHPoint a, IHPoint b)
        {
            Xmin = Math.Min(a.X, b.X);
            Ymin = Math.Min(a.Y, b.Y);
            Xmax = Math.Max(a.X, b.X);
            Ymax = Math.Max(a.Y, b.Y);
        }
        /// <summary>
        /// Принадлежность 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(IHPoint p)
        {
            return ((Xmin <= p.X) && (p.X <= Xmax)) &&
                   ((Ymin <= p.Y) && (p.Y <= Ymax));
        }

        /// <summary>
        /// Проверьте, находится ли данная точка внутри прямоугольника.
        /// </summary>
        public bool Contains(double x, double y)
        {
            return ((x >= Xmin) && (x <= Xmax) && (y >= Ymin) && (y <= Ymax));
        }
        /// <summary>
        /// Проверьте, содержит ли прямоугольник другой прямоугольник.
        /// </summary>
        public bool Contains(DRectangle other)
        {
            return (Xmin <= other.Xmin && other.Xmax <= Xmax
                 && Ymin <= other.Ymin && other.Ymax <= Ymax);
        }
        /// <summary>
        /// Измените прямоугольник, чтобы включить в 
        /// него заданный прямоугольник.
        /// </summary>
        public void Expand(DRectangle other)
        {
            Xmin = Math.Min(Xmin, other.Xmin);
            Ymin = Math.Min(Ymin, other.Ymin);
            Xmax = Math.Max(Xmax, other.Xmax);
            Ymax = Math.Max(Ymax, other.Ymax);
        }
    }
}
