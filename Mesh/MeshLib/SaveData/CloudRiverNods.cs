namespace MeshLib.SaveData
{
    using CommonLib;
    using System.Collections.Generic;
    using System.Linq;
    using GeometryLib;
    using MemLogLib;
    using System;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Cloud - множество маркированных точек 
    /// </summary>
    public class CloudRiverNods : IClouds
    {
        /// <summary>
        /// имена атрибутов
        /// </summary>
        public string[] AttributNames { get; }

        public List<CloudKnot> CloudKnots = new List<CloudKnot>();
        public CloudRiverNods()
        {
            AttributNames = new string[6]
            { "Глубина", "Срез.глубина", "Тепература","Скорость","Курс","Вектор скорости" };
        }

        public CloudRiverNods(CloudRiverNods m)
        {
            AttributNames = m.AttributNames;
            foreach (CloudKnot knot in m.CloudKnots)
                CloudKnots.Add(knot);
        }
        public CloudRiverNods(IClouds m)
        {
            AttributNames = ((CloudRiverNods)m).AttributNames;
            IHPoint[] knots = m.GetKnots();
            foreach (CloudKnot knot in knots)
                CloudKnots.Add(knot);
        }
        /// <summary>
        /// Получить облако узлов
        /// </summary>
        /// <returns></returns>
        public IHPoint[] GetKnots()
        {
            return CloudKnots.ToArray();
        }
        /// <summary>
        /// Добавить узел в облако
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mark"></param>
        /// <param name="Atributs"></param>
        public void AddNode(double x, double y, int mark, double[] Attributes = null)
        {
            CloudKnots.Add(new CloudKnot(x, y, Attributes, mark));
        }
        /// <summary>
        /// изменить маркер узла
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mark"></param>
        /// <param name="Atributs"></param>
        public void ReMarkNode(double x, double y, int mark)
        {
            HPoint p = new HPoint(x, y);
            for (int i = 0; i < CloudKnots.Count; i++)
                if (p == CloudKnots[i])
                {
                    CloudKnots[i].mark = mark;
                    break;
                }
        }
        /// <summary>
        /// Количество узлов
        /// </summary>
        public int CountKnots => CloudKnots.Count;
        /// <summary>
        /// Координаты X для узловых точек 
        /// </summary>
        /// <param name="dim">номер размерности</param>
        /// <returns></returns>
        /// <summary>
        /// Координаты X или Y для узловых точек 
        /// </summary>
        public double[] GetCoords(int dim)
        {
            if (dim == 0)
                return CloudKnots.Select(p => p.x).ToArray();
            else
                return CloudKnots.Select(p => p.y).ToArray();
        }
        /// <summary>
        /// Диапазон координат для узлов сетки
        /// </summary>
        public void MinMax(int dim, ref double min, ref double max)
        {
            var mas = GetCoords(dim);
            if (mas.Length == 0)
            {
                max = 0; min = 0;
            }
            else
            {
                max = mas == null ? double.MaxValue : mas.Max();
                min = mas == null ? double.MinValue : mas.Min();
            }
        }
        /// <summary>
        /// Массив маркеров вершин 
        /// </summary>
        public int[] GetMarkKnots()
        {
            return CloudKnots.Select(p => p.mark).ToArray();
        }
        /// <summary>
        /// Получить поле атрибута
        /// </summary>
        /// <param name="indexAttribut"></param>
        /// <returns></returns>
        public IField GetPole(int indexAttribut)
        {
            //{ "Глубина", "Тепература","Скорость","Курс","Вектор скорости" };
            IField Field = null;
            if (indexAttribut < 4)
            {
                double[] m = CloudKnots.Select(p => p.Attributes[indexAttribut]).ToArray();
                Field = new Field1D(AttributNames[indexAttribut], m);
            }
            else
            {
                double[] V = CloudKnots.Select(p => p.Attributes[2]).ToArray();
                double[] C = CloudKnots.Select(p => p.Attributes[3]).ToArray();
                double[] Vx = null;
                double[] Vy = null;
                MEM.Alloc(CountKnots, ref Vx, "Vx");
                MEM.Alloc(CountKnots, ref Vy, "Vy");
                int k = 0;
                double K = 2 * Math.PI / 360;
                for (int i = 0; i < CountKnots; i++)
                {
                    Vx[k] = V[k] * Math.Sin(C[k] * K);
                    Vy[k] = V[k] * Math.Cos(C[k] * K);
                }
                Field = new Field2D(AttributNames[indexAttribut], Vx, Vy);
            }
            return Field;
        }
    
    }

    ///// <summary>
    ///// ОО: Cloud - множество маркированных точек 
    ///// </summary>
    //public class Cloud : ICloud
    //{
    //    /// <summary>
    //    /// Координаты узловых точек и параметров определенных в них
    //    /// </summary>
    //    public double[] CoordsX;
    //    public double[] CoordsY;
    //    /// <summary>
    //    /// Массив меток  для граничных узловых точек
    //    /// </summary>
    //    public int[] KnotsMark;

    //    public Cloud(double[] coordsX, double[] coordsY, int[] knotsMark)
    //    {
    //        MEM.MemCopy(ref CoordsX, coordsX);
    //        MEM.MemCopy(ref CoordsY, coordsY);
    //        MEM.MemCopy(ref KnotsMark, knotsMark);
    //    }

    //    public Cloud(Cloud m)
    //    {
    //        MEM.MemCopy(ref CoordsX, m.CoordsX);
    //        MEM.MemCopy(ref CoordsY, m.CoordsY);
    //        MEM.MemCopy(ref KnotsMark, m.KnotsMark);
    //    }
    //    public Cloud(ICloud m)
    //    {
    //        MEM.MemCopy(ref CoordsX, m.GetCoords(0));
    //        MEM.MemCopy(ref CoordsY, m.GetCoords(1));
    //        MEM.MemCopy(ref KnotsMark, m.GetMarkKnots());
    //    }

    //    /// <summary>
    //    /// Количество узлов
    //    /// </summary>
    //    public int CountKnots
    //    {
    //        get { return CoordsX == null ? 0 : CoordsX.Length; }
    //    }
    //    /// <summary>
    //    /// Координаты X для узловых точек 
    //    /// </summary>
    //    /// <param name="dim">номер размерности</param>
    //    /// <returns></returns>
    //    /// <summary>
    //    /// Координаты X или Y для узловых точек 
    //    /// </summary>
    //    public double[] GetCoords(int dim)
    //    {
    //        if (dim == 0)
    //            return CoordsX;
    //        else
    //            return CoordsY;
    //    }
    //    /// <summary>
    //    /// Диапазон координат для узлов сетки
    //    /// </summary>
    //    public void MinMax(int dim, ref double min, ref double max)
    //    {
    //        var mas = GetCoords(dim);
    //        max = mas == null ? double.MaxValue : mas.Max();
    //        min = mas == null ? double.MinValue : mas.Min();
    //    }
    //    /// <summary>
    //    /// Массив маркеров вершин 
    //    /// </summary>
    //    public int[] GetMarkKnots() { return KnotsMark; }
    //}

}
