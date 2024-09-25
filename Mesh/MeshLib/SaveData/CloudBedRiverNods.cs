//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.10.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.SaveData
{
    using CommonLib;
    using System.Collections.Generic;
    using System.Linq;
    using GeometryLib;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Cloud - множество маркированных точек 
    /// </summary>
    public class CloudBedRiverNods : IClouds
    {
        /// <summary>
        /// имена атрибутов
        /// </summary>
        public string[] AttributNames { get; }

        public List<IBedRiverNode> CloudKnots = new List<IBedRiverNode>();
        public CloudBedRiverNods()
        {
            AttributNames = new string[2] { "Отметки дна", "Шероховатость" };
        }
        public CloudBedRiverNods(CloudBedRiverNods m)
        {
            AttributNames = m.AttributNames;
            foreach (IBedRiverNode knot in m.CloudKnots)
                CloudKnots.Add(knot);
        }
        public CloudBedRiverNods(IClouds m)
        {
            AttributNames = ((SavePointRiverNods)m).AttributNames;
            IHPoint[] knots = m.GetKnots();
            foreach (IBedRiverNode knot in knots)
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
        public void AddNode(BedRiverNode node)
        {
            CloudKnots.Add(new BedRiverNode(node));
        }
        /// <summary>
        /// Добавить узел в облако
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mark"></param>
        /// <param name="Atributs"></param>
        public void AddNode(double x, double y, double zeta, double ks)
        {
            CloudKnots.Add(new BedRiverNode(x, y, zeta, ks));
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
            if(Attributes == null)
                CloudKnots.Add(new BedRiverNode(x, y, 1, 0.1));
            else
                CloudKnots.Add(new BedRiverNode(x, y, Attributes[0], Attributes[1]));
        }

        /// <summary>
        /// Добавить узел в облако
        /// </summary>
        public void AddNode(IHPoint node)
        {
            BedRiverNode n = node as BedRiverNode;
            if(n!=null)
                CloudKnots.Add(new BedRiverNode(n));
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
            //HPoint p = new HPoint(x, y);
            //for (int i = 0; i < CloudKnots.Count; i++)
            //    if (p == CloudKnots[i])
            //    {
            //        CloudKnots[i].mark = mark;
            //        break;
            //    }
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
                return CloudKnots.Select(p => p.X).ToArray();
            else
                return CloudKnots.Select(p => p.Y).ToArray();
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
            return new int[0] { };
        }
        /// <summary>
        /// Получить поле атрибута
        /// </summary>
        /// <param name="indexAttribut"></param>
        /// <returns></returns>
        public IField GetPole(int indexAttribut)
        {
            IField Field = null;
            if (indexAttribut == 0)
            {
                double[] m = CloudKnots.Select(p => p.zeta).ToArray();
                Field = new Field1D("Отметки дна", m);
            }
            if (indexAttribut == 1)
            {
                double[] m = CloudKnots.Select(p => p.ks).ToArray();
                Field = new Field1D("Шероховатость", m);
            }
            return Field;
        }
    
    }


}
