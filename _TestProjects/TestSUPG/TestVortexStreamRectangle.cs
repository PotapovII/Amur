#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                  - (C) Copyright 2024
//                        Потапов И.И.
//                         21.12.24
//---------------------------------------------------------------------------
// Тестовая задача Стокса в переменных Phi,Vortex
// две степени свободы в узле, с "точными" граничными условиями для Vortex
//---------------------------------------------------------------------------
namespace TestSUPG
{
    using MeshLib;
    using RenderLib;
    using CommonLib;
    using FEMTasksLib;
    using System.Windows.Forms;

    public class TestVortexStreamRectangle
    {
        VortexStreamRectangle task = null;
        public TestVortexStreamRectangle(double u = 1, double l = 1,
                            double h = 1, int Nx = 30, int Ny = 20)
        {
            task = new VortexStreamRectangle(u, l, h, Nx, Ny);
        }
        public TestVortexStreamRectangle(double u, int Nx = 30, int Ny = 20)
        {
            task = new VortexStreamRectangle(u, Nx, Ny);
        }
        public void Run()
        {
            double[] result = null;
            task.SolveTask(ref result);
            ShowMesh(task);
        }
        public void ShowMesh(VortexStreamRectangle task)
        {
            IMesh mesh = task.Mesh;
            if (mesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                data.Add("Скорость по Х", task.U);
                data.Add("Скорость по Y", task.V);
                data.Add("Скорость", task.U, task.V);
                data.Add("Phi", task.Phi);
                data.Add("Vortex", task.Vortex);
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
