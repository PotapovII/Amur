namespace RenderLib
{
    using System;
    using System.Windows.Forms;
    using MeshLib;
    public partial class LoaderSavePoints : Form
    {
        SavePoint sp = new SavePoint();
        public LoaderSavePoints()
        {
            InitializeComponent();
        }
        private void btShow_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sp = (SavePoint)sp.LoadSavePoint(openFileDialog1.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sp != null)
            {
                SavePointData spData = new SavePointData();
                spData.SetSavePoint(sp);
                Form form = new ViewForm(spData);
                form.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
