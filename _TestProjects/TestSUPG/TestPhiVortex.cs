namespace TestSUPG
{
    using MeshLib;
    using CommonLib;
    using MemLogLib;
    using MeshLib.TaskArea;

    using RenderLib;

    using System;
    using System.Windows.Forms;
    using FDMTaskLib;
    using FEMTasksLib;

    /// <summary>
    /// Тест для решение задачи вынужденной конвекции в 
    /// постановке вихрь функция тока для задачи Стокса
    /// в прямоугольной области
    /// </summary>
    public class TestPhiVortex 
    {
        PhiVortex task = null;
        public TestPhiVortex(int Nx, int Ny, double Lx, double Ly, double V) 
        {
            task = new PhiVortex(Nx, Ny, Lx, Ly, V);
            task.Solver();
        }
        public void ShowMesh()
        {
            IMesh mesh = task.CreateMesh();
            if (mesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] Result1 = null;
                double[] Result2 = null;
                task.Convertor(ref Result1, task.mV);
                task.Convertor(ref Result2, task.mW);
                data.Add("Скорость по Х", Result1);
                data.Add("Скорость по Y", Result2);
                data.Add("Скорость", Result1, Result2);
                task.Convertor(ref Result1, task.Phi);
                task.Convertor(ref Result2, task.Vortex);
                data.Add("Phi", Result1);
                data.Add("Vortex", Result2);
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
