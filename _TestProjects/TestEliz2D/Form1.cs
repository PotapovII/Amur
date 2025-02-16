using System;
using System.Windows.Forms;
using System.ComponentModel;

using MeshLib;
using RenderLib;
using CommonLib;
using NPRiverLib.APRiver1XD.KGD_River2D;

namespace TestEliz2D
{
    public partial class Form1 : Form
    {
        AreasProfile2D area2d = null;
        KsiMesh Mesh;
        RGDParameters1XD rGDParameters1XD = new RGDParameters1XD();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            ViewForm vf = new ViewForm(area2d);
            vf.Show();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            prpgrd_Hydro.SelectedObject = rGDParameters1XD;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rGDParameters1XD.H = areaProfileControl1.H;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ViewForm vf = new ViewForm(Mesh);
            vf.Show();
        }
        /// <summary>
        /// Сохранение расчетной области в файл с расширением .ar
        /// Структура файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dialog_SaveArea_FileOk(object sender, CancelEventArgs e)
        {

            /// структура файла .arp
            /// 
            /// L1 L2 L3 H topLayer BottomLayer Nx NyMiddle
            /// TypeBedParamIndex
            /// BedParam1 BedParam2 BedParam3 BedParam4
            /// (для каждого из подслоев)
            /// MeshIndex
            /// MainMeshParam1 MainMeshParam2  MainMeshParam3 
            /// AdditioanalMeshParam1--11
            //try
            //{
            //    using (StreamWriter outputFile = new StreamWriter(saveAreaFileDialog.FileName))
            //    {
            //        outputFile.WriteLine(XB.Length.ToString());
            //        outputFile.WriteLine(Yb[0] - YB[0]);
            //        outputFile.WriteLine(YT[0] - Yt[0]);
            //        outputFile.WriteLine("XB, YB, XT, YT");
            //        for (int i = 0; i < XB.Length; i++)
            //        {
            //            outputFile.Write(XB[i].ToString() + " " + YB[i].ToString() + " " + XT[i].ToString() + " " + YT[i].ToString());
            //            outputFile.WriteLine();

            //        }
            //        outputFile.Close();

            //    }
            //    statusStrip1.Text = "Ok!";
            //}
            //catch (Exception ex)
            //{
            //    statusStrip1.Text = ex.Message;
            //}
        }

        private void SaveAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveParamsDialog.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                area2d = areaProfileControl1.getArea();
                PointsGeneratorEliz[] generators = areaProfileControl1.getGenerators();
                MeshBuilderProfile2D mb = new MeshBuilderProfile2D(area2d, generators);
                Mesh = mb.GenerateMesh();
                //
                // Тест
                //
                //KsiWrapper ks = new KsiWrapper(Mesh);
                //ks.TriangleGeometryCalculation();
                //ks.MakeWallFuncStructure(rGDParameters1XD.surf_flag);


                //KGD_Eliz2024_1XD eliz = new KGD_Eliz2024_1XD();
                //eliz.SetParams(rGDParameters1XD);
                // поместить в бэкграунд? как использовать многопоточность ИИ?
                //eliz.SolverStep();
                //
                //TriMesh t = Mesh.ConvertToTriMesh();
                toolStripStatusLabel1.Text = "Ok!";
                
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message;
                area2d = null;
                Mesh = null;

            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveParamsDialog.ShowDialog();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openAreaParamsDialog.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            IMesh mesh = Mesh.ConvertToTriMesh();
            ISavePoint sp = new SavePoint();
            sp.SetSavePoint(0,mesh,null);
            var X = mesh.GetCoords(0);
            var Y = mesh.GetCoords(1);
            sp.Add(" Координаты X", X);
            sp.Add(" Координаты Y", Y);
            sp.Add(" Координаты", X, Y);
            ViForm vf = new ViForm(sp);
            vf.Show();
        }
    }
}





