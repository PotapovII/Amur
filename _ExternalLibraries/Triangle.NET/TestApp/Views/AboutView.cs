using System;
using System.Diagnostics;
using System.Windows.Forms;
using TriangleNet;
using TriangleNet.Geometry;

namespace MeshExplorer.Views
{
    public partial class AboutView : UserControl, IView
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void lbCodeplex_Clicked(object sender, EventArgs e)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("http://triangle.codeplex.com/");
                Process.Start(info);
            }
            catch (Exception)
            { }
        }

        #region IView

        public void HandleNewInput(IPolygon geometry)
        {
        }

        public void HandleMeshImport(IPolygon geometry, MeshNet mesh)
        {
        }

        public void HandleMeshUpdate(MeshNet mesh)
        {
        }

        public void HandleMeshChange(MeshNet mesh)
        {
        }

        #endregion
    }
}
