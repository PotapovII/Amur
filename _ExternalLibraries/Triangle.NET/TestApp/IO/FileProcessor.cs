// -----------------------------------------------------------------------
// <copyright file="FileProcessor.cs" company="">
// Christian Woltering, Triangle.NET, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace MeshExplorer.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using MeshExplorer.IO.Formats;

    using TriangleNet;
    using TriangleNet.Geometry;

    /// <summary>
    /// Provides static methods to read and write mesh files.
    /// Предоставляет статические методы для чтения и записи файлов сетки.
    /// </summary>
    public static class FileProcessor
    {
        static Dictionary<string, IMeshFile> container = new Dictionary<string, IMeshFile>();

        public static bool CanHandleFile(string path)
        {
            if (File.Exists(path))
            {
                var provider = GetProviderInstance(path);

                if (provider != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true, if the given file contains mesh information.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ContainsMeshData(string path)
        {
            IMeshFile provider = GetProviderInstance(path);

            return provider.ContainsMeshData(path);
        }

        /// <summary>
        /// Read an input geometry from given file.
        /// </summary>
        public static IPolygon Read(string path)
        {
            var provider = GetProviderInstance(path);

            return provider.Read(path);
        }

        /// <summary>
        /// Read a mesh from given file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MeshNet Import(string path)
        {
            var provider = GetProviderInstance(path);
            return (MeshNet)provider.Import(path);
        }

        /// <summary>
        /// Save the current mesh to given file.
        /// </summary>
        public static void Save(string path, MeshNet mesh)
        {
            IMeshFile provider = GetProviderInstance(path);

            provider.Write(mesh, path);
        }

        private static IMeshFile GetProviderInstance(string path)
        {
            string ext = Path.GetExtension(path);

            IMeshFile provider = null;

            if (container.ContainsKey(ext))
            {
                provider = container[ext];
            }
            else
            {
                provider = CreateProviderInstance(ext);
            }

            return provider;
        }

        private static IMeshFile CreateProviderInstance(string ext)
        {
            // TODO: automate by using IBaseFormater's Extensions property.

            IMeshFile provider = null;

            if (ext == ".node" || ext == ".poly" || ext == ".ele")
            {
                provider = new TriangleFile();
            }
            else if (ext == ".json")
            {
                provider = new JsonFile();
            }

            if (provider == null)
            {
                throw new NotImplementedException("File format not implemented.");
            }

            container.Add(ext, provider);

            return provider;
        }
    }
}
