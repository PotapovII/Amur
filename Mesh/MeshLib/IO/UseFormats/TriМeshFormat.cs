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
    using CommonLib.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    class TriMeshFormat : IBaseFormater<IMesh>
    {
        public bool IsSupported(string file)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".mesh" || ext == ".node" || ext == ".poly" || ext == ".ele")
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
        public void Read(string filename, ref IMesh rmesh, uint testID = 0)
        {
            string ext = Path.GetExtension(filename);

            if (ext == ".mesh")
            {
                TriMesh mesh = new TriMesh();
                using (StreamReader file = new StreamReader(filename))
                {
                    string line = file.ReadLine();
                    int Count = int.Parse(line.Trim('\t'));
                    mesh.AreaElems = new TriElement[Count];
                    for (int i = 0; i < Count; i++)
                    {
                        line = file.ReadLine().Trim();
                        string[] slines = line.Split(',', '(', ')', ' ', '\t');
                        mesh.AreaElems[i].Vertex1 = uint.Parse(slines[0]);
                        mesh.AreaElems[i].Vertex2 = uint.Parse(slines[1]);
                        mesh.AreaElems[i].Vertex3 = uint.Parse(slines[2]);
                    }
                    line = file.ReadLine();
                    int CountCoord = int.Parse(line.Trim('\t'));
                    mesh.CoordsX = new double[CountCoord];
                    mesh.CoordsY = new double[CountCoord];
                    for (int i = 0; i < CountCoord; i++)
                    {
                        line = file.ReadLine().Trim('(', ')', '\t');
                        string[] slines = line.Split(' ', '\t');
                        mesh.CoordsX[i] = double.Parse(slines[0]);
                        mesh.CoordsY[i] = double.Parse(slines[1]);
                    }

                    line = file.ReadLine();
                    int CountBE = int.Parse(line.Trim('\t'));
                    mesh.BoundElems = new TwoElement[CountBE];
                    mesh.BoundElementsMark = new int[CountBE];
                    for (int i = 0; i < CountBE; i++)
                    {
                        line = file.ReadLine().Trim();
                        string[] slines = line.Split(',', '(', ')', ' ', '\t');
                        mesh.BoundElems[i].Vertex1 = uint.Parse(slines[0]);
                        mesh.BoundElems[i].Vertex2 = uint.Parse(slines[1]);
                        mesh.BoundElementsMark[i] = int.Parse(slines[2]);
                    }

                    line = file.ReadLine();
                    int CountBK = int.Parse(line.Trim('\t'));
                    mesh.BoundKnots = new int[CountBK];
                    mesh.BoundKnotsMark = new int[CountBK];
                    for (int i = 0; i < CountBK; i++)
                    {
                        line = file.ReadLine().Trim();
                        string[] slines = line.Split(',', '(', ')', ' ', '\t');
                        mesh.BoundKnots[i] = int.Parse(slines[0]);
                        mesh.BoundKnotsMark[i] = int.Parse(slines[1]);
                    }
                }
                rmesh = mesh;
                return;
            }
            if (ext == ".node")
            {
            }
            if (ext == ".poly")
            {
            }
            throw new NotSupportedException("Could not load '" + filename + "' file.");
        }
        /// <summary>
        /// Сохраняем сетку на диск.
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public void Write(IMesh Mesh, string filename)
        {
            TriMesh mesh = Mesh as TriMesh;
            if (mesh == null)
                throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
            string ext = Path.GetExtension(filename);
            if (ext == ".mesh")
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    file.WriteLine(mesh.CountElements);
                    for (int i = 0; i < mesh.CountElements; i++)
                    {
                        file.WriteLine(mesh.AreaElems[i].Vertex1.ToString() + " " +
                                       mesh.AreaElems[i].Vertex2.ToString() + " " +
                                       mesh.AreaElems[i].Vertex3.ToString());
                    }
                    file.WriteLine(mesh.CountKnots);
                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        file.WriteLine("{0} {1}",
                        mesh.CoordsX[i],
                        mesh.CoordsY[i]);
                    }
                    file.WriteLine(mesh.CountBoundElements);
                    for (int i = 0; i < mesh.CountBoundElements; i++)
                    {
                        file.WriteLine(mesh.BoundElems[i].Vertex1.ToString() + " " +
                                       mesh.BoundElems[i].Vertex2.ToString() + " " +
                                       mesh.BoundElementsMark[i].ToString());
                    }
                    file.WriteLine(mesh.BoundKnots.Length);
                    for (int i = 0; i < mesh.BoundKnots.Length; i++)
                    {
                        file.WriteLine("{0} {1}",
                        mesh.BoundKnots[i],
                        mesh.BoundKnotsMark[i]);
                    }
                    file.Close();
                }
                return;
            }
            if (ext == ".node")
            {

            }
            if (ext == ".poly")
            {

            }
            throw new NotSupportedException("Не возможно сохранить выбранный формат сетки mesh == null");
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public List<string> GetTestsName()
        {
            List<string> strings = new List<string>();
            strings.Add("Основная задача - тестовая");
            return strings;
        }
    }
}
