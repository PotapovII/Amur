namespace ShTriMeshGeneratorLib
{
    using System;
    using MemLogLib;
    using CommonLib.Geometry;
    using System.Collections.Generic;
    using GeometryLib.Locators;

    /// <summary>
    /// TO DO Временный тип для создания границ области изменить при совмешении с редактором области
    /// Граница области
    /// </summary>
    public class Border
    {
        /// <summary>
        /// уникальный идентификатор границы
        /// </summary>
        public readonly int ID;
        /// <summary>
        /// Все множество точек, принадлежащее границе, включая опорные вершины
        /// </summary>
        public IHPoint[] Points;
        /// <summary>
        /// Граничные ребра, формирующие оболочку.
        /// Построены на опорных вершинах границы <see cref="BaseVertexes"/>
        /// </summary>
        public IHillEdge[] BoundaryEdges;
        /// <summary>
        /// Внешний контур
        /// </summary>
        IHPoint[] rectangle = new IHPoint[4];
        /// <summary>
        /// Инициализация границы
        /// </summary>
        /// <param name="BaseVertexes">опорные вершины, образующие форму границы</param>
        /// <param name="generator">правила генерации точек на ребрах границы, между опорными вершинами</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Border(IHillEdge[] BoundaryEdges, int ID)
        {
            this.ID = ID;
            //инициализация описанного прямоугольника
            this.InitilizeRect();
            // Инициализация граничных ребер
            InitializeBoundaryEdges();
        }
        
        #region Инициализация свойств границы
        /// <summary>
        /// Инициализация граничных точек
        /// </summary>
        protected void InitializeBoundaryEdges()
        {
            int Count = 0;
            for (int i = 0; i < BoundaryEdges.Length; i++)
                Count += BoundaryEdges[i].Count - 1;
            //выделение памяти для массива опорных ребер
            MEM.Alloc(Count, ref Points);
            //заполняем оба массива ребер
            int idx = 0;
            for (int i = 0; i < BoundaryEdges.Length; i++)
            {
                IHPoint[] p = BoundaryEdges[i].GetPoints();
                for (int j = 0; j < BoundaryEdges[i].Count - 1; j++)
                    Points[idx++]= p[j];
            }
        }
        /// <summary>
        /// Инициализация прямоугольника, описанного около границы
        /// </summary>
        protected void InitilizeRect()
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            //собираем края области
            for (int i = 0; i < BoundaryEdges.Length; i++)
            {
                var vertex = BoundaryEdges[i].A;
                if (vertex.X < minX)
                    minX = vertex.X;
                if (vertex.X > maxX)
                    maxX = vertex.X;
                if (vertex.Y < minY)
                    minY = vertex.Y;
                if (vertex.Y > maxY)
                    maxY = vertex.Y;
            }
            //формируем описанный прямоугольник
            rectangle[0] = new HPoint(minX, minY);
            rectangle[1] = new HPoint(minX, maxY);
            rectangle[2] = new HPoint(maxX, maxY);
            rectangle[3] = new HPoint(maxX, minY);
        }
        #endregion
    }

    /// <summary>
    /// Контейнер для границ области. 
    /// Хранит и управляет внешней и внутренними границами 
    /// </summary>
    public class ListBorder
    {
        /// <summary>
        /// Внутренние границы
        /// </summary>
        public List<Border> Boundaries = new List<Border>();
        public void Add(Border b)=> Boundaries.Add(b);
        public void Remove(Border b) => Boundaries.Remove(b);
        public void Clear() => Boundaries.Clear();
        /// <summary>
        /// Все граничные точки
        /// </summary>
        public IHPoint[] GetBoundaryPoints()
        {
            IHPoint[] BPoints = null;
            if(Boundaries.Count > 0)
            {
                // количество всех точек
                int Count = 0;
                for (int i = 0; i < Boundaries.Count; i++)
                    Count += Boundaries[i].Points.Length;
                MEM.Alloc(Count, ref BPoints);
                int idx = 0;
                for (int i = 0; i < Boundaries.Count; i++)
                {
                    IHPoint[] p = Boundaries[i].Points;
                    for (int j = 0; j < p.Length; j++)
                        BPoints[idx++] = p[j];
                }
            }
            return BPoints;
        }
        #region Добавление границ
        /// <summary>
        /// Проверяет, принадлежит ли ребро (start, end) какой-либо границе
        /// </summary>


        #endregion 
    }
}
