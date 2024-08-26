namespace CommonLib.Geometry
{
    /// <summary>
    /// Ребро
    /// </summary>
    public interface IHEdge : IHLine
    {
        /// <summary>
        /// Индекс ребра
        /// </summary>
        int ID { get; set; }
    }
}
