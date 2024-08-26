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
    using System.Linq;
    using System.IO;
    using MemLogLib;
    using CommonLib.IO;
    using System.Collections.Generic;

    class FVМeshFormat : IBaseFormater<IMesh>
    {
        public bool IsSupported(string file)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".mesh" || ext == ".fmsh" /*|| ext == ".node" || ext == ".poly" || ext == ".ele"*/)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Прочитать файл, содержащий сетку.
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Read(string filename, ref IMesh mesh, uint testID = 0)
        {
            string ext = Path.GetExtension(filename);

            if (ext == ".mesh")
            {
                TriMeshFormat tmf = new TriMeshFormat();
                tmf.Read(filename, ref mesh);
                FVComMesh fvmesh = new FVComMesh(mesh);
                mesh = fvmesh;
                return;
            }
            if (ext == ".fmsh")
            {
                MeshCore cmesh = new MeshCore();
                var lines = File.ReadAllText(filename).Split('\n');
                if (lines[0].Trim() != "##Point")
                    throw new NotSupportedException("Could not load '" + filename + "' file.");
                int row = 1;
                // Point d2
                cmesh.points = WR.LoadDoubleMatrix(lines, ref row);
                row++;
                // Triangle d4 
                cmesh.elems = WR.LoadIntMatrix(lines, ref row);
                row++;
                // TypeFForms d1
                cmesh.fform = WR.LoadIntMatrix(lines, ref row)[0];
                row++;
                // Boundary d2
                cmesh.boundary = WR.LoadIntMatrix(lines, ref row);
                row++;
                // BoundaryMark d2
                cmesh.boundaryMark = WR.LoadIntMatrix(lines, ref row);
                row++;
                // Params d2
                cmesh.Params = WR.LoadDoubleMatrix(lines, ref row);
                row++;
                FVComMesh fvmesh = new FVComMesh(cmesh);
                mesh = fvmesh;
                return;
            }
            //if (ext == ".node")
            //{
            //return;
            //}
            //if (ext == ".poly")
            //{
            //return;
            //}
            throw new NotSupportedException("Could not load '" + filename + "' file.");
        }
        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public void Write(IMesh Mesh, string filename)
        {
            string ext = Path.GetExtension(filename);
            if (ext == ".mesh")
            {
                FVComMesh mesh = Mesh as FVComMesh;
                if (mesh == null)
                    throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
                TriMeshFormat tmf = new TriMeshFormat();
                tmf.Write(Mesh, filename);
            }
            if (ext == ".fmsh")
            {
                MeshCore cmesh = new MeshCore(Mesh);
                if (cmesh != null)
                {
                    using (StreamWriter file = new StreamWriter(filename))
                    {
                        file.WriteLine("##Point");
                        WR.WriteDoubleMassive(file, cmesh.points[0]);
                        WR.WriteDoubleMassive(file, cmesh.points[1]);
                        file.WriteLine("##Triangle");
                        WR.WriteIntMassive(file, cmesh.elems[0]);
                        WR.WriteIntMassive(file, cmesh.elems[1]);
                        WR.WriteIntMassive(file, cmesh.elems[2]);
                        WR.WriteIntMassive(file, cmesh.elems[3]);
                        file.WriteLine("##TypeFForms");
                        WR.WriteIntMassive(file, cmesh.fform);
                        file.WriteLine("##Boundary");
                        WR.WriteIntMassive(file, cmesh.boundary[0]);
                        WR.WriteIntMassive(file, cmesh.boundary[1]);
                        file.WriteLine("##BoundaryMark");
                        WR.WriteIntMassive(file, cmesh.boundaryMark[0]);
                        WR.WriteIntMassive(file, cmesh.boundaryMark[1]);
                        file.WriteLine("##Params");
                        if (cmesh.Params != null)
                            for (int i = 0; i < cmesh.Params.Length; i++)
                                WR.WriteDoubleMassive(file, cmesh.Params[i]);
                        file.Close();
                    }
                    return;
                }
            }

            //if (ext == ".node")
            //{

            //}
            //if (ext == ".poly")
            //{

            //}
            throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public List<string> GetTestsName()
        {
            List<string> strings = new List<string>();
            strings.Add("Поток в канале с плоским дном");
            return strings;
        }

    }
}
