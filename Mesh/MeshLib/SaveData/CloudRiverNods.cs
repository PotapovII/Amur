//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.10.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.SaveData
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using GeometryLib;
    using CommonLib.Geometry;
    /// <summary>
    /// ОО: Cloud - множество маркированных точек 
    /// </summary>
    public class SavePointRiverNods : IClouds
    {
        /// <summary>
        /// имена атрибутов
        /// </summary>
        public string[] AttributNames { get; }

        public List<CloudKnot> CloudKnots = new List<CloudKnot>();

        public SavePointRiverNods()
        {
            // при создании нового идет настройка на текущее состояние 
            // статического класса и его флагов
            AttributNames = AtrCK.AtrNames;
        }
        public SavePointRiverNods(SavePointRiverNods m)
        {
            AttributNames = m.AttributNames;
            foreach (CloudKnot knot in m.CloudKnots)
                CloudKnots.Add(knot);
        }
        public SavePointRiverNods(IClouds m)
        {
            AttributNames = ((SavePointRiverNods)m).AttributNames;
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
        /// Добавить узел в облако
        /// </summary>
        public void AddNode(IHPoint node)
        {
            CloudKnot cn =  node as CloudKnot;
            if(cn != null)
                CloudKnots.Add(new CloudKnot(cn));
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
            IField Field = null;
            if (indexAttribut < 5)
            {
                double[] m = CloudKnots.Select(p => p.Attributes[indexAttribut]).ToArray();
                Field = new Field1D(AttributNames[indexAttribut], m);
            }
            else
            if (indexAttribut == 5)
            {
                double[] Vx = CloudKnots.Select(p => p.Attributes[3]).ToArray();
                double[] Vy = CloudKnots.Select(p => p.Attributes[4]).ToArray();
                Field = new Field2D(AttributNames[indexAttribut], Vx, Vy);
            }
            else
            {
                double[] m = CloudKnots.Select(p => (double)p.ID).ToArray();
                Field = new Field1D(AttributNames[indexAttribut], m);
            }
            return Field;
        }
    
    }
}
