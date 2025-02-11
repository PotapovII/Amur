
namespace RiverDB
{
    using System;
    using System.Windows.Forms;

    using RiverDB.Report;
    using RiverDB.FormsDB;
    using RiverDB.Convertors;
    using RiverDB.ConvertorOut;

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
            form.Show();
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
            list.Show();
        }

        private void tsm_Knots_Click(object sender, EventArgs e)
        {
            Knot list = new Knot("Knot");
            list.Show();
        }

        private void tsm_GPost_Click(object sender, EventArgs e)
        {
            Place list = new Place("Place");
            list.Show();
        }

        private void работаСВычислительнойСеткойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateMeshForm form = new CreateMeshForm();
            form.Show();
        }

        private void tsmWaterLevel_Click(object sender, EventArgs e)
        {
            FormReportWL florm = new FormReportWL();
            florm.Show();
        }
    }
}
