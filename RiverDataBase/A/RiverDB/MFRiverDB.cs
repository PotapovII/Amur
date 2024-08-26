using RiverDB.Convertors;
using RiverDB.FormsDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RiverDB
{
    public partial class MFRiverDB : Form
    {
        FConnectPath fConnectPath = new FConnectPath();
        public MFRiverDB()
        {
            InitializeComponent();
        }

        private void узлыToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Knot list = new Knot("Knot");
            list.ShowDialog();
        }

        private void экспериментыToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            LevelsRiver list = new LevelsRiver("Experiment");
            list.ShowDialog();
        }

        private void гидропостыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Form catalogue = new Catalogue("River");
            //catalogue.ShowDialog();
            Place list = new Place("Place");
            list.ShowDialog();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fConnectPath.ShowDialog();
        }

        private void gpxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConvertForm form = new ConvertForm();
            form.ShowDialog();
        }
    }
}
