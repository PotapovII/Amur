namespace RiverDB
{
    using RiverDB.ConvertorOut;
    using RiverDB.Convertors;
    using RiverDB.FormsDB;
    using System;
    using System.Windows.Forms;

    public partial class MFRiverDB : Form
    {
        FConnectPath fConnectPath = new FConnectPath();
        public MFRiverDB()
        {
            InitializeComponent();
        }

        private void gpxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConvertForm form = new ConvertForm();
            form.ShowDialog();
        }

        private void ExittTSM_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsm_CreareCloudMesh_Click(object sender, EventArgs e)
        {
            FCreateCloudMesh form = new FCreateCloudMesh();
            form.Show();
        }

        private void tsmOptions_Click(object sender, EventArgs e)
        {
            fConnectPath.ShowDialog();
        }

        private void tsm_ExperimentData_Click(object sender, EventArgs e)
        {
            LevelsRiver list = new LevelsRiver("Experiment");
            list.ShowDialog();
        }

        private void tsm_Knots_Click(object sender, EventArgs e)
        {
            Knot list = new Knot("Knot");
            list.ShowDialog();
        }

        private void tsm_GPost_Click(object sender, EventArgs e)
        {
            Place list = new Place("Place");
            list.ShowDialog();
        }

        private void работаСВычислительнойСеткойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateMeshForm form = new CreateMeshForm();
            form.Show();
        }
    }
}
