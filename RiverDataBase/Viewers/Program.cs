namespace Viewers
{
    using MeshLib;
    using RenderLib;

    using System;
    using System.Windows.Forms;
    internal class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SavePoint data = new SavePoint();
            Form form = new ViForm(data);
            form.ShowDialog();
        }
    }
}
