namespace CommonLib.Geometry
{
    /// <summary>
    /// Треугольник - интерфейс
    /// </summary>
    public interface ITwoElement
    {
        /// <summary>
        /// Первая точка треугольника
        /// </summary>
        uint Vertex1 { get; set; }
        /// <summary>
        /// Вторая точка треугольника
        /// </summary>
        uint Vertex2 { get; set; }
    }
    /// <summary>
    /// Треугольник - интерфейс
    /// </summary>
    public interface ITriElement : ITwoElement
    {
        /// <summary>
        /// Третья точка треугольника
        /// </summary>
        uint Vertex3 { get; set; }
    }

    /// <summary>
    /// Треугольник - интерфейс
    /// </summary>
    public interface IHTriangle : ITriElement
    {
        /// <summary>
        /// Возвращает или задает идентификатор треугольника.
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// Возвращает или устанавливает метку общего назначения,
        /// используется для получения информации о регионе.
        /// </remarks>
        int Marker { get; set; }
    }
    /// <summary>
    /// Треугольник - интерфейс
    /// </summary>
    public interface IDTriangle : ITriElement
    {
        /// <summary>
        /// Возвращает или задает идентификатор треугольника.
        /// </summary>
        int ID { get; set; }
        /// <summary>
        /// Возвращает или устанавливает метку общего назначения,
        /// используется для получения информации о регионе.
        /// </remarks>
        int Marker { get; set; }


    }
}
