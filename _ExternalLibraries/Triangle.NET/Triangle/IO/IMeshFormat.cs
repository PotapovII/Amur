﻿// -----------------------------------------------------------------------
// <copyright file="IBaseFormater.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet.IO
{
    using System.IO;
    using TriangleNet.Meshing;

    /// <summary>
    /// Interface for mesh I/O.
    /// </summary>
    public interface IBaseFormater : IFileFormat
    {
        /// <summary>
        /// Read a file containing a mesh.
        /// </summary>
        /// <param name="filename">The path of the file to read.</param>
        /// <returns>An instance of the <see cref="IMeshNet" /> interface.</returns>
        IMeshNet Import(string filename);

        /// <summary>
        /// Save a mesh to disk.
        /// </summary>
        /// <param name="mesh">An instance of the <see cref="IMeshNet" /> interface.</param>
        /// <param name="filename">The path of the file to save.</param>
        void Write(IMeshNet mesh, string filename);

        /// <summary>
        /// Save a mesh to a <see cref="Stream" />.
        /// </summary>
        /// <param name="mesh">An instance of the <see cref="IMeshNet" /> interface.</param>
        /// <param name="stream">The stream to save to.</param>
        void Write(IMeshNet mesh, Stream stream);
    }
}
