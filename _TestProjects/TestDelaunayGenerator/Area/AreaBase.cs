namespace TestDelaunayGenerator.Areas
{
    using CommonLib.Geometry;
    using TestDelaunayGenerator.Boundary;
    /// <summary>
    /// Базовый абстрактный метод генерации множества точек/узлов области для будущей триангуляции
    /// </summary>
    public abstract class AreaBase
    {
        /// <summary>
        /// Название полигона, области
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Узлы/точки триангуляции
        /// </summary>
        public IHPoint[] Points;
        /// <summary>
        /// контейнер, содержащий в себе множество ограниченных областей
        /// </summary>
        public BoundaryContainer BoundaryContainer = null;

        /// <summary>
        /// Генератор границы
        /// </summary>
        public IGeneratorBase BoundaryGenerator = new GeneratorFixed(0);
        /// <summary>
        /// Количество точек триангуляции
        /// </summary>
        protected int Count;
        /// <summary>
        /// Базовый абстрактный метод генерации множества точек/узлов области для будущей триангуляции
        /// </summary>
        protected AreaBase(string Name, int count = 10_000)
        {
            this.Name = Name;
            this.Count = count;
            // Initialize();
        }
        /// <summary>
        /// Инициализация области, вызывается в конструкторе
        /// </summary>
        public abstract void Initialize();

        public void AddBoundary(IHPoint[] boundary)
        {
            if (BoundaryContainer == null)
                BoundaryContainer = new BoundaryContainer(BoundaryGenerator);
            BoundaryContainer.Add(boundary);
        }
    }
}
