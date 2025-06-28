namespace ShTriMeshGeneratorLib
{
    using System;
    using MemLogLib;
    using CommonLib.Geometry;
    using GeometryLib.Geometry;
    /// <summary>
    /// TO DO Временный тип для создания границ области изменить при совмешении с редактором области
    /// Граница области
    /// </summary>
    public class BorderTmp
    {
        /// <summary>
        /// Счетчик для уникальности границ
        /// </summary>
        protected static int uniqueIdCounter = 0;
        /// <summary>
        /// уникальный идентификатор границы
        /// </summary>
        public readonly int ID;
        /// <summary>
        /// Вершины, обращующие форму границы (опорные вершины)
        /// </summary>
        public IHPoint[] BaseVertexes;
        /// <summary>
        /// Все множество точек, принадлежащее границе, включая опорные вершины
        /// <see cref="BaseVertexes"/>
        /// </summary>
        public IHPoint[] Points;
        /// <summary>
        /// Граничные ребра, формирующие оболочку.
        /// Построены на опорных вершинах границы <see cref="BaseVertexes"/>
        /// </summary>
        public IHillEdge[] BaseBoundaryEdges;
        /// <summary>
        /// Граничные ребра.
        /// Построены на всем множестве точек границы <see cref="Points"/>.
        /// Если между опорными вершинами границы больше нет точек,
        /// тогда дублирует <see cref="BaseBoundaryEdges"/>
        /// </summary>
        public IHEdge[] BoundaryEdges;
        /// <summary>
        /// Прямоугольник, описанный около текущей ограниченной области
        /// </summary>
        public IHPoint[] OutRect;
        /// <summary>
        /// Инициализация границы
        /// </summary>
        /// <param name="BaseVertexes">опорные вершины, образующие форму границы</param>
        /// <param name="generator">правила генерации точек на ребрах границы, между опорными вершинами</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BorderTmp(IHPoint[] BaseVertexes, IGeneratorBase generator)
        {
            this.ID = BorderTmp.uniqueIdCounter;
            //наращиваем счетчик для индексации границ
            BorderTmp.uniqueIdCounter++;
            //проверка на null
            if (BaseVertexes is null || BaseVertexes.Length == 0)
                throw new ArgumentNullException($"{nameof(BaseVertexes)} null или пуст");
            if (generator is null)
                throw new ArgumentNullException($"{nameof(generator)} не может быть null");

            this.BaseVertexes = BaseVertexes;
            //генерация точек
            this.Points = generator.Generate(this);
            //инициализация описанного прямоугольника
            this.InitilizeRect();
            //сохраняем индексы вершин, образующих область
            //  InitializeVertexIds();
            // Инициализация граничных ребер
            InitializeBoundaryEdges();
        }
        
        #region Инициализация свойств границы
        /// <summary>
        /// Инициализация граничных ребер <see cref="BaseBoundaryEdges"/>, <see cref="BoundaryEdges"/>
        /// </summary>
        protected void InitializeBoundaryEdges()
        {
            //выделение памяти для массива опорных ребер
            MEM.Alloc(BaseVertexes.Length, ref BaseBoundaryEdges);
            //выделение памяти для массива всех ребер
            MEM.Alloc(Points.Length, ref BoundaryEdges);
            //индекс текущего опорного ребра
            int baseEdgeId = -1;
            //заполняем оба массива ребер
            for (int i = 0; i < Points.Length; i++)
            {
                //если достигнута следующая опорная вершина
                //делаем инкремент для индекса опорных вершин
                //и создаем новое опорное ребро с началом в baseEdgeId + 1
                IHPoint v1 = Points[i];
                IHPoint v2 = BaseVertexes[(baseEdgeId + 1) % BaseVertexes.Length];
                if (Math.Abs(v1.X - v2.X) < 1e-15 && Math.Abs(v1.Y - v2.Y) < 1e-15)
                //if (Points[i] == BaseVertexes[(baseEdgeId + 1) % BaseVertexes.Length])
                {
                    baseEdgeId += 1;
                    BaseBoundaryEdges[baseEdgeId] =
                        new HillEdge(
                            baseEdgeId,
                            BaseVertexes[baseEdgeId % BaseVertexes.Length],
                            BaseVertexes[(baseEdgeId + 1) % BaseVertexes.Length]
                        );
                }
                //создаем обычное ребро, входящее в состав опорного ребра
                BoundaryEdges[i] = new HEdge(i, Points[i % Points.Length], Points[(i + 1) % Points.Length]);
                //наращиваем счетчик ребер у текущего опорного ребра
                //т.к. опорное ребро состоит из множества таких ребер
                BaseBoundaryEdges[baseEdgeId].Count += 1;
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
            foreach (var vertex in this.BaseVertexes)
            {
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
            IHPoint[] rectangle = new IHPoint[4];
            rectangle[0] = new HPoint(minX, minY);
            rectangle[1] = new HPoint(minX, maxY);
            rectangle[2] = new HPoint(maxX, maxY);
            rectangle[3] = new HPoint(maxX, minY);
            this.OutRect = rectangle;
        }

        //protected int[] VertexesIds;
        ////TODO убрать бы...
        ///// <summary>
        ///// Инициализация индексов вершин границы
        ///// </summary>
        //protected void InitializeVertexIds()
        //{
        //    VertexesIds = new int[this.BaseVertexes.Length];
        //    int currentVertexId = 0;
        //    for (int i = 0; i < Points.Length; i++)
        //    {
        //        if (BaseVertexes[currentVertexId].X == Points[i].X &&
        //            BaseVertexes[currentVertexId].Y == Points[i].Y)
        //        {
        //            VertexesIds[currentVertexId] = i;
        //            currentVertexId++;
        //            if (currentVertexId == VertexesIds.Length)
        //                break;
        //        }
        //    }
        //}
        #endregion

    }
}
