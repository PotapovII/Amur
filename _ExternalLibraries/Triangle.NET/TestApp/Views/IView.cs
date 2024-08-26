// -----------------------------------------------------------------------
// <copyright file="IView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace MeshExplorer.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TriangleNet;
    using TriangleNet.Geometry;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IView
    {
        void HandleNewInput(IPolygon geometry);
        void HandleMeshImport(IPolygon geometry, MeshNet mesh);
        void HandleMeshUpdate(MeshNet mesh);
        void HandleMeshChange(MeshNet mesh);
    }
}
