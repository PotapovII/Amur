
namespace TriangleNet.Meshing
{
    using TriangleNet.Geometry;

    /// <summary>
    /// Interface for polygon triangulation.
    /// </summary>
    public interface IConstraintMesher
    {
        /// <summary>
        /// Triangulates a polygon.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <returns>MeshNet</returns>
        IMeshNet Triangulate(IPolygon polygon);

        /// <summary>
        /// Triangulates a polygon, applying constraint options.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="options">Constraint options.</param>
        /// <returns>MeshNet</returns>
        IMeshNet Triangulate(IPolygon polygon, ConstraintOptions options);
    }
}
