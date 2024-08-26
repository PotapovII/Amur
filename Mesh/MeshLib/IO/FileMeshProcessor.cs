//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 22.04.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using System;
    using System.Collections.Generic;
    using MemLogLib;
    using System.IO;
    using CommonLib.IO;

    /// <summary>
    /// ОО: Менеджер по конфертации файлов в форматы проекта
    /// </summary>
    public static class FileMeshProcessor
    {
        /// <summary>
        /// Список форатов
        /// </summary>
        static List<IBaseFormater<IMesh>> formats;

        static FileMeshProcessor()
        {
            formats = new List<IBaseFormater<IMesh>>();
            // По умолчанию добавим формат для трехузловых сеток.
            formats.Add(new TriMeshFormat());
            formats.Add(new FVМeshFormat());
        }

        public static void Add(IBaseFormater<IMesh> format)
        {
            formats.Add(format);
        }

        public static bool IsSupported(string file)
        {
            foreach (var format in formats)
                if (format.IsSupported(file))
                    return true;
            return false;
        }

        #region Mesh чтение / запись

        /// <summary>
        /// Прочитать файл, содержащий сетку.
        /// </summary>
        /// <param name = "filename"> Путь к файлу для чтения. </param>
        /// <returns> Экземпляр интерфейса <see cref = "IMesh" />. </returns>
        public static void Read(string filename, ref IMesh mesh)
        {
            foreach (IBaseFormater<IMesh> format in formats)
            {
                if (format != null && format.IsSupported(filename))
                {
                    format.Read(filename, ref mesh);
                    return;
                }
            }
            throw new Exception("Формат файла не поддерживается.");
        }

        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name = "mesh"> Экземпляр интерфейса <see cref = "IMesh" />. </param>
        /// <param name = "filename"> Путь к файлу для сохранения. </param>
        public static void Write(IMesh mesh, string filename)
        {
            foreach (IBaseFormater<IMesh> format in formats)
            {
                if (format != null && format.IsSupported(filename))
                {
                    format.Write(mesh, filename);
                    return;
                }
            }

            throw new Exception("Формат файла не поддерживается.");
        }
        #endregion
    }
}