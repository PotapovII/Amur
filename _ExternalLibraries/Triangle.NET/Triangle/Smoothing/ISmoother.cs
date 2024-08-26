// -----------------------------------------------------------------------
// <copyright file="ISmoother.cs">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.Smoothing
{
    using TriangleNet.Meshing;

    /// <summary>
    /// Interface for mesh smoothers.
    /// </summary>
    public interface ISmoother
    {
        void Smooth(IMeshNet mesh);
        void Smooth(IMeshNet mesh, int limit);
    }
}