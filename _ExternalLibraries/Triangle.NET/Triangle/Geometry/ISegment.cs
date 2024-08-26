// -----------------------------------------------------------------------
// <copyright file="ISegment.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Geometry
{
    /// <summary>
    /// Interface for segment geometry.
    /// </summary>
    public interface ISegment : IEdge
    {
        /// <summary>
        /// Gets the vertex at given index.
        /// Получает вершину по заданному индексу.
        /// </summary>
        /// <param name="index">The local index (0 or 1).</param>
        Vertex GetVertex(int index);

        /// <summary>
        /// Gets an adjoining triangle.
        /// Получает смежный треугольник.
        /// </summary>
        /// <param name="index">The triangle index (0 or 1).</param>
        ITriangle GetTriangle(int index);
    }
}
