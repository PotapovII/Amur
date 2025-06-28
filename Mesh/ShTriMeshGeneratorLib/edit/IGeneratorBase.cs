namespace ShTriMeshGeneratorLib
{
    using CommonLib.Geometry;

    public interface IGeneratorBase
    {
        /// <summary>
        /// Множество точек
        /// </summary>
        IHPoint[] Points { get; }

        IHPoint[] Generate(BorderTmp boundary);

        /// <summary>
        /// Индексы опорных вершин внутри <see cref="Points"/>
        /// </summary>
        int[] BaseVertexIds();
    }
}
