namespace ShTriMeshGeneratorLib
{
    /// <summary>
    /// Отношение принадлежности точки области
    /// </summary>
    public enum PointStatus
    {
        /// <summary>
        /// Внешний
        /// </summary>
        External,
        /// <summary>
        /// Входит в область
        /// </summary>
        Internal,
        /// <summary>
        /// Принадлежит границе
        /// </summary>
        Boundary
    }
}
