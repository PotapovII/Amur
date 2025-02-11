//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 04.10.2023 Потапов И.И. 
//--------------------------------------------------------------------------
namespace RiverDB.ConvertorOut
{
    using System;
    using System.Windows.Forms;

    using MeshLib;
    using CommonLib;
    using CommonLib.Mesh;

    using GeometryLib;
    using MeshAdapterLib;
    using MeshExplorer.IO;

    using TriangleNet;
    using TriangleNet.Tools;
    using TriangleNet.Meshing;
    using TriangleNet.Geometry;
    using TriangleNet.Smoothing;
    using TriangleNet.Meshing.Algorithm;

    public partial class CreateMeshForm : Form
    {
        enum STA 
        {
            Triangulate = 0,
            Refine = 1,
            Smooth = 2,
            Error = 3,
            LoadMesh = 4,
            LoadPolygon = 5
        }

        string[] messages = { "Триангуляция выполнена",
                         "Сгущение сетки выполнено",
                         "Сглаживание сетки выполнено",
                         "Ошибка : ",
                         "Сетка загружена",
                         "Полигон загружен"
        };        


        public bool RefineMode = false;
        public bool ExceptionThrown = false;
        /// <summary>
        /// Параметры качества 
        /// </summary>

        ConstraintOptions options = new ConstraintOptions();
        QualityOptions quality = new QualityOptions();
        Statistic statistic = new Statistic();
        int CountSmooth = 0;
        IPolygon input;
        MeshNet meshRiver;
        const int CountAttributes = 5;
        string[] ArtNames = { "Глубина", "Срез.Глубина", "Температура", "Скорость", "Курс" };
        

        public CreateMeshForm()
        {
            InitializeComponent();
            
        }

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Open(ofd.FileName);
            }
        }

        private bool Open(string filename)
        {
            if (!FileProcessor.CanHandleFile(filename))
            {
                // TODO: show message.
            }
            else
            {
                if (FileProcessor.ContainsMeshData(filename))
                {
                    if (filename.EndsWith(".ele") || MessageBox.Show("Import meshRiver", 
                        "Do you want to import the meshRiver?", MessageBoxButtons.YesNo) == DialogResult.OK)
                    {
                        input = null;
                        try
                        {
                            meshRiver = FileProcessor.Import(filename);
                            stStatResultTest.Text = messages[(int)STA.LoadMesh];
                        }
                        catch (Exception e)
                        {
                            stStatResultTest.Text = messages[(int)STA.Error] + e.Message;
                            MessageBox.Show("Import meshRiver error", e.Message, MessageBoxButtons.OK);
                            return false;
                        }

                        if (meshRiver != null)
                        {
                            //  statisticView.UpdateStatistic(meshRiver);
                            HandleMeshImport();
                            btnSmooth.Enabled = true; // TODO: Remove
                        }
                        // else Message
                        return true;
                    }
                }
                try
                {
                    input = FileProcessor.Read(filename);
                    stStatResultTest.Text = messages[(int)STA.LoadPolygon];
                }
                catch (Exception e)
                {
                    stStatResultTest.Text = messages[(int)STA.Error] + e.Message;
                    MessageBox.Show("Import polygon error", e.Message, MessageBoxButtons.OK);
                    return false;
                }
            }
            if (input != null)
            {
                HandleNewInput();
            }
            return true;
        }
        private void HandleMeshImport()
        {
            // Render meshRiver
            ShowMesh(meshRiver);
            // Update window caption
            btnMesh.Enabled = true;
            btnMesh.Text = "Refine";
            RefineMode = true;
            // TODO: Should the Voronoi diagram automatically update?
            // Enable menu items
            menuFileSave.Enabled = true;
        }

        private void HandleNewInput()
        {
            // Reset meshRiver
            meshRiver = null;
            // Reset buttons
            btnMesh.Enabled = true;
            btnMesh.Text = "Triangulate";
            btnSmooth.Enabled = false;
            ShowMesh(meshRiver);
        }

        private void menuFileSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileProcessor.Save(sfd.FileName, meshRiver);
            }
        }

        private void menuFileQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMesh_Click(object sender, EventArgs e)
        {
            TriangulateOrRefine();
        }
        private void TriangulateOrRefine()
        {
            if ((input == null && RefineMode == false) || ExceptionThrown == true)
            {
                return;
            }

            if (RefineMode == false)
            {
                Triangulate();
                if (cbQuality.Checked == true && meshRiver != null)
                {
                    btnMesh.Text = "Refine";
                    btnSmooth.Enabled = meshRiver.IsPolygon;
                }
                RefineMode = true;
            }
            else
            {
                if (cbQuality.Checked == true)
                {
                    Refine();
                    HandleMeshUpdate();
                }
            }
        }

        private void Triangulate()
        {
            if (input == null) return;

            SetCreateMeshOptions();

            options.Convex = cbCreateContur.Checked;
            // создать трианг. Делоне
            options.ConformingDelaunay = cbConformingDelaunay.Checked;
            options.SegmentSplitting = cbSegmentSplitting.Checked == true ? 0 : 1;
            //quality.MinimumAngle = (float)nUDMinimumAngle.Value;
            //quality.MaximumAngle = (float)nUDMaximumAngle.Value;

            if (cbConformDel.Checked == true)
            {
                options.ConformingDelaunay = true;
            }

            if (cbQuality.Checked)
            {
                quality.MinimumAngle = (float)nUDMinimumAngle.Value;

                double maxAngle = (float)nUDMaximumAngle.Value;

                if (maxAngle < 180)
                {
                    quality.MaximumAngle = maxAngle;
                }
                // Ignore area constraints on initial triangulation.
            }

            //if (meshControlView.ParamConvexChecked)
            //{
            //    options.Convex = true;
            //}

            try
            {
                if (cbSweepline.Checked == true)
                {
                    meshRiver = (MeshNet)input.Triangulate(options, quality, new SweepLine());
                    
                }
                else
                {
                    meshRiver = (MeshNet)input.Triangulate(options, quality);
                }
                //statisticView.UpdateStatistic(meshRiver);
                HandleMeshUpdate();
                if (cbQuality.Checked == true)
                {
                    RefineMode = true;
                }
                stStatResultTest.Text = messages[(int)STA.Triangulate];
            }
            catch (Exception ex)
            {
                LockOnException();
                stStatResultTest.Text = messages[(int)STA.Error] + ex.Message;
                MessageBox.Show("Exception - Triangulate", ex.Message, MessageBoxButtons.OK);
            }
            //UpdateLog();
        }

        private void Refine()
        {

            if (meshRiver == null) return;

            //double area = ParamMaxAreaValue;

            //var quality = new QualityOptions();

            //if (area > 0 && area < 1)
            //{
            //    quality.MaximumArea = area * statisticView.Statistic.LargestArea;
            //}

            //quality.MinimumAngle = meshControlView.ParamMinAngleValue;

            //double maxAngle = ParamMaxAngleValue;

            //if (maxAngle < 180)
            //{
            //    quality.MaximumAngle = maxAngle;
            //}
            SetCreateMeshOptions();

            try
            {
                meshRiver.Refine(quality, cbConformDel.Checked);

                //statisticView.UpdateStatistic(meshRiver);

                HandleMeshChange();
            }
            catch (Exception ex)
            {
                //LockOnException();
                MessageBox.Show("Exception - Refine", ex.Message, MessageBoxButtons.OK);
            }

            //UpdateLog();
        }
        /// <summary>
        /// Установка параметров генерации сетки 
        /// </summary>
        private void SetCreateMeshOptions()
        {
            // создать выпуклый контур
            options.Convex = cbCreateContur.Checked;
            // создать трианг. Делоне
            options.ConformingDelaunay = cbConformingDelaunay.Checked;// true;
            options.SegmentSplitting = cbSegmentSplitting.Checked == true ? 0 : 1;
            quality.MinimumAngle = (float)nUDMinimumAngle.Value;
            quality.MaximumAngle = (float)nUDMaximumAngle.Value;
            CountSmooth = (int)nUDCountSmooth.Value;
            cbQuality.Checked = checkBoxSM.Checked;
        }
        private void btnSmooth_Click(object sender, EventArgs e)
        {
            Smooth();
        }
        private void Smooth()
        {
            if (meshRiver == null || ExceptionThrown) return;
            if (!meshRiver.IsPolygon)
            {
                return;
            }
            var smoother = new SimpleSmoother();
            try
            {
                smoother.Smooth(this.meshRiver);
                statistic.Update(meshRiver, 10);
                HandleMeshUpdate();
                stStatResultTest.Text = messages[(int)STA.Smooth];

            }
            catch (Exception ex)
            {
                LockOnException();
                stStatResultTest.Text = messages[(int)STA.Error] + ex.Message;
                MessageBox.Show("Exception - Smooth", ex.Message, MessageBoxButtons.OK);
            }
        }
        private void HandleMeshUpdate()
        {
            // Render meshRiver
            ShowMesh(meshRiver);
            HandleMeshUpdate(meshRiver);
            HandleMeshChange();
        }
        private void HandleMeshChange()
        {
            HandleMeshUpdate(meshRiver);
            // Enable menu items
            menuFileSave.Enabled = true;
        }
        private void LockOnException()
        {
            btnMesh.Enabled = false;
            btnSmooth.Enabled = false;
            ExceptionThrown = true;
        }

        public void HandleMeshUpdate(MeshNet meshRiver)
        {
            // Previous meshRiver stats
            if(meshRiver == null) return;
            lbNumVert.Text = meshRiver.Vertices.Count.ToString();
            lbNumSeg.Text = meshRiver.Segments.Count.ToString();
            lbNumTri.Text = meshRiver.Triangles.Count.ToString();
        }
        /// <summary>
        /// Отобразить сетку в отдельном окне
        /// </summary>
        /// <param name="meshRiver"></param>
        public void ShowMesh(MeshNet meshRiver)
        {
            IFEMesh bmesh = null;
            double[][] values = null;
            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, meshRiver, Direction.toRight);
            if (bmesh != null)
            {
                SavePoint sp = new SavePoint();
                sp.SetSavePoint(0, bmesh);
                double[] x = bmesh.GetCoords(0);
                double[] y = bmesh.GetCoords(1);

                string[] ArtNames = { "Глубина", "Срез.Глубина", "Температура", "Скорость", "Курс" };
                if (values.Length == 5)
                {
                    if (values != null)
                        for (int i = 0; i < values.Length; i++)
                        {
                            sp.Add(new Field1D(ArtNames[i], values[i]));
                        }
                    double[][] p = bmesh.GetParams();
                    for (int i = 0; i < p.Length; i++)
                        sp.Add(new Field1D("Фу" + i.ToString(), p[i]));
                }
                if (values.Length == 2)
                {
                    sp.Add("Скорость U", values[0]);
                    sp.Add("Скорость V", values[1]);
                }
                sp.Add(new Field1D("Координата Х", x));
                sp.Add(new Field1D("Координата Y", y));
                SetSavePoint(sp);
            }
        }
        /// <summary>
        /// Метод синхронизирует передачу объекта SavePoint между разными потоками
        /// (потока вычислений и потока контрола)
        /// </summary>
        /// <param name="sp"></param>
        private void SetSavePoint(ISavePoint sp)
        {

            if (this.InvokeRequired)
            {
                SendParam d = new SendParam(gdI_Control1.SendSavePoint);
                this.Invoke(d, new object[] { sp });
            }
            else
                gdI_Control1.SendSavePoint(sp);
        }
    }
}
