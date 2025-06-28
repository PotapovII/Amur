namespace ShTriMeshGeneratorLib
{
    using System;

    using MemLogLib;
    using CommonLib.Geometry;
    using GeometryLib.World;
    using GeometryLib.Geometry;
    /// <summary>
    /// Ребро границы. Может содержать целое множество точек, а не только свои вершины.
    /// По сути является ребром, построенном на паре опорных вершин границы
    /// Реализовано в рамках делонатора
    /// </summary>
    public struct HillEdge : IHillEdge
    {
        DRectangle rect;
        public HillEdge(int ID, IHPoint A, IHPoint B, int mark = 0, int count = 0)
            : this()
        {
            this.ID = ID;
            this.A = A;
            this.B = B;
            this.mark = mark;
            this.Count = count;
            rect = new DRectangle(A, B);
        }
        public HillEdge(HEdge edge, int mark = 0, int count = 0)
            : this()
        {
            ID = edge.ID;
            A = edge.A;
            B = edge.B;
            this.mark = mark;
            this.Count = count;
            rect = new DRectangle(A, B);
        }
        public int mark { get; set; }
        public int Count { get; set; }
        public int ID { get; set; }
        public IHPoint A { get; set; }
        public IHPoint B { get; set; }
        /// <summary>
        /// Получить массив точек границ / интерполяция по свойствам
        /// </summary>
        /// <param name="inCount"></param>
        /// <returns></returns>
        public IHPoint[] GetPoints(int inCount = 0)
        {
            IHPoint[] p = new IHPoint[inCount+2];
            double s = 1.0 / (inCount + 1);
            p[0] = A.IClone();
            for (int i = 1; i < p.Length - 1; i++)
            {
                double N1 = 1 - s * i;
                double N2 = s * i;
                IHPoint V = A.IClone();
                V.X = N1 * A.X + N2 * B.X;
                V.Y = N1 * A.Y + N2 * B.Y;
                p[i] = V;
            }
            p[p.Length-1] = B.IClone();
            return p;
        }
        /// <summary>
        /// Принадлежность описанному ректанглу
        /// </summary>
        public bool RectContains(IHPoint p)
        {
            return rect.Contains(p);
        }
        /// <summary>
        /// Принадлежность линии
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(IHPoint p)
        {
            if(rect.Contains(p) == true)
            {
                double dx_test = Math.Abs(p.X - rect.Xmin);
                double dy_test = Math.Abs(p.Y - rect.Ymin);
                if (Math.Abs(rect.Height * dx_test - rect.Width * dy_test) < MEM.Error10)
                    return true;
                else
                    return false;
            }
            return false;
        }
    }
}
