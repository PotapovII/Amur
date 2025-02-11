namespace RenderLib.GDI
{
    using MeshLib;
    using CommonLib;
    using System.Windows.Forms;
    using System.Threading;

    /// <summary>
    /// Класс для тестирования сетки
    /// </summary>
    public static class TestMesh
    {
        public static void Show(IMesh mesh)
        {
            if (mesh != null)
            {
                SavePoint sp = new SavePoint();
                sp.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                sp.Add("Координата Х", xx);
                sp.Add("Координата Y", yy);
                sp.Add("Координаты ХY", xx, yy);
                Form form = new ViForm(sp);
                form.Show();
            }
        }
    }
}
