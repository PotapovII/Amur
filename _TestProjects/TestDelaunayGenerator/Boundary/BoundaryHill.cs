namespace TestDelaunayGenerator.Boundary
{
    using System;
    using System.Linq;
    using CommonLib.Geometry;
    using GeometryLib;
    using MemLogLib;

    /// <summary>
    /// Класс для определения контура области, через грани оболочки и 
    /// </summary>
    public class CounturHill
    {
        /// <summary>
        /// Название контура
        /// </summary>
        public string Name;
        /// <summary>
        /// Точки, образующие границу, включая вершины области
        /// </summary>
        public HNumbKnot[] Points = null;
        /// <summary>
        /// Грани оболочки
        /// </summary>
        public IHillEdge[] hEdges = null;
        public CounturHill(string Name, IHillEdge[] hEdges)
        {
            this.Name = Name;
            this.hEdges = hEdges;
            Init();
        }
        /// <summary>
        /// Сгенерировать опорные точки границы и определить их маркер
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < hEdges.Length; i++)
                if (MEM.Equals(hEdges[i].B, hEdges[(i + 1) % hEdges.Length].A) == false)
                    throw new Exception("Контур ободочки не замкнут");

            int countPints = hEdges.Sum(x=>x.Count) - hEdges.Length;
            Points = new HNumbKnot[countPints];
            int ip = 0;
            foreach(var e in hEdges)
            {
                double dx = (e.A.X - e.B.X) / e.Count;
                double dy = (e.A.Y - e.B.Y) / e.Count;
                for (int p = 0; p < e.Count - 1; p++) 
                    Points[ip] = new HNumbKnot(e.A.X + dx * p, e.A.Y + dy * p, e.mark, ip++);
            }
        }
    }
    /// <summary>
    /// Контур для границы области
    /// </summary>
    public class BoundaryHill
    {
        /// <summary>
        /// Точки, образующие границу, включая вершины области
        /// </summary>
        public IHPoint[] Points = null;
        /// <summary>
        /// Вершины, задающие форму оболочки границы
        /// </summary>
        public IHPoint[] Vertexes = null;
        /// <summary>
        /// Общее количество граничных точек
        /// </summary>
        public int Length => Points.Length;
        /// <summary>
        /// Индексы вершин, образующих область.
        /// Первая вершина идет с нулевым индексом
        /// </summary>
        public int[] VertexesIds = null;
        /// <summary>
        /// Прямоугольник, описанный около текущей ограниченной области
        /// </summary>
        public IHPoint[] OutRect = null;
        /// <summary>
        /// Создать контур границы для области
        /// </summary>
        /// <param name="vertexes">множество точек, формирующее область объявляемой границы</param>
        /// <param name="basePoints">базовое множество точек, которое будет ограничено текущей границей</param>
        public BoundaryHill(IHPoint[] vertexes, IGeneratorBase generator)
        {
            this.Vertexes = vertexes;
            Initialize(generator);
        }
        /// <summary>
        /// Инициализация границы, генерация точек между вершинами, образующими оболочку границы
        /// </summary>
        /// <param name="generator">Способ генерации точек на границе</param>
        /// <returns>Сгенерированное множество точек границы, в т.ч. вершины, образующие область границы</returns>
        public void Initialize(IGeneratorBase generator)
        {
            //генерируем множество граничных узлов
            Points = generator.Generate(this);
            //сохраняем индексы вершин, образующих область
            VertexesIds = new int[this.Vertexes.Length];
            int currentVertexId = 0;
            for (int i = 0; i < Points.Length; i++)
            {
                if (Vertexes[currentVertexId].X == Points[i].X && 
                    Vertexes[currentVertexId].Y == Points[i].Y)
                {
                    VertexesIds[currentVertexId] = i;
                    currentVertexId++;
                    if (currentVertexId == VertexesIds.Length)
                        break;
                }
            }
            InitializeSquare();
        }
        /// <summary>
        /// Вычислить 4 вершины, которые образуют прямоугольник,
        /// в который можно вписать текущую ограниченную область
        /// </summary>
        protected void InitializeSquare()
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            //собираем края области
            foreach (var vertex in this.Vertexes)
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
            OutRect = rectangle;
        }
        ///// <summary>
        ///// Перед использованием, необходимо вызвать <see cref="BoundaryHill.Initialize(GeneratorBase)"/>
        ///// </summary>
        ///// <param name="index"></param>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //public IHPoint this[int index]
        //{
        //    get => Points[index];
        //    set => Points[index] = value;
        //}
    }
}
