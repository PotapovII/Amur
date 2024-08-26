namespace TriMeshGeneratorLib
{
    using CommonLib;
    using RenderLib;
    using GeometryLib;
    using System;
    using System.IO;
    using System.Windows.Forms;
    using MeshLib;
    using System.Collections.Generic;

    public class Client
    {
        [STAThread]
        public static void Main()
        {
            Run();
            //RunMesh();
        }

        public static void RunMesh()
        {
            RVHabitatPhysics physics = new RVHabitatPhysics();
            string Name0 = "Soni-steady.cdg";
            Console.WriteLine("Введите 0 1-2 или болле символов");
            //string str = Console.ReadLine();
            //if (str != "")
            //{
            //    if (str.Length < 3)
            //        Name0 = "..\\CFG\\amurCDG.cdg";
            //    else
            //        Name0 = "..\\CFG\\SpinTW01.cdg";
            //}
            Name0 = "..\\Debug\\CFG\\SpinTW01.cdg";
            //string name = Path.GetFileNameWithoutExtension(Name0) + ".sdg";
            RVMeshRiver hmesh = new RVMeshRiver(physics);
            using (StreamReader file = new StreamReader(Name0))
            {
                hmesh.ReadLMesh_sdg(file);
                hmesh.GeneratingTriangularMesh();
                hmesh.GeneratingEdges();
            }
            double[][] values = null;
            IMesh  mesh1 = RVMeshAdapter.MeshFrontRenumberation(hmesh, ref values);
            ShowMesh(mesh1);

            hmesh.SmoothMesh(1, hmesh, 0);
            IMesh mesh2 = RVMeshAdapter.MeshFrontRenumberation(hmesh, ref values);
            ShowMesh(mesh2);
        }
        public static void Run()
        {
            RVHabitatPhysics physics = new RVHabitatPhysics();
            //string Name0 = "Soni-steady.cdg";
            //Console.WriteLine("Введите 0 1-2 или болле символов");
            //string str = Console.ReadLine();
            //if (str != "")
            //{
            //    if (str.Length < 3)
            //        Name0 = "amurCDG.cdg";
            //    else
            //        Name0 = "SpinTW01.cdg";
            //}
            string Name0 = "..\\Debug\\CFG\\SpinTW01.cdg";
            //Name0 = "..\\Debug\\CFG\\amurCDG.cdg";
            RVMeshRiver riverMesh = new RVMeshRiver(physics);
            using (StreamReader file = new StreamReader(Name0))
            {
                RVCdgIOut cdgP = new RVCdgIOut(riverMesh);
                cdgP.ReadFileCDG(file);
                // генерация сетки
                riverMesh.GeneratingTriangularMesh();
                // генерация граней
                riverMesh.GeneratingEdges();
            }
            double[][] values = null;
            // Отрисовка
            IMesh mesh = RVMeshAdapter.MeshFrontRenumberation(riverMesh, ref values);
            ShowMesh(mesh);

            //string name = Path.GetFileNameWithoutExtension(Name0) + ".sdg";
            //using (StreamWriter fsave = new StreamWriter(name))
            //{
            //    riverMesh.writeLMesh_sdg(fsave);
            //    fsave.Close();
            //}
            string Name1 = Path.GetFileNameWithoutExtension(Name0) + "1.cdg";

            using (StreamWriter fsave = new StreamWriter(Name1))
            {
                RVCdgIOut cdgP = new RVCdgIOut(riverMesh);
                cdgP.WriteFileCDG(fsave);
                fsave.Close();

            }
            
            using (StreamReader file = new StreamReader(Name1))
            {
                RVCdgIOut cdgP = new RVCdgIOut(riverMesh);
                cdgP.ReadFileCDG(file);
                // генерация сетки
                riverMesh.GeneratingTriangularMesh();
                // генерация граней
                riverMesh.GeneratingEdges();
            }
            // Отрисовка
            mesh = RVMeshAdapter.MeshFrontRenumberation(riverMesh, ref values);
            ShowMesh(mesh);
        }
        public static void ShowMesh(IMesh mesh)
        {

            if (mesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
                Form form = new ViForm(data);
                form.ShowDialog();
            }

        }

    }
}
