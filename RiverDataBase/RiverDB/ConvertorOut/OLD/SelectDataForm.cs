//
#define USE_ATTRIBS
//
namespace RiverDB.ConvertorOut
{
    using CommonLib;
    using CommonLib.Mesh;
    using ConnectLib;
    using GeometryLib;
    using MemLogLib;
    using MeshAdapterLib;
    using MeshLib;
    using MeshLib.SaveData;
    using RenderLib;
    using RiverDB.ConvertorIn;
    using RiverDB.Convertors;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;
    using TriangleNet;
    using TriangleNet.Geometry;
    using TriangleNet.Meshing;
    using TriangleNet.Smoothing;
    public partial class SelectDataForm : Form
    {
        ConstraintOptions options = new ConstraintOptions();
        QualityOptions quality = new QualityOptions();
        Convert_WGS84_To_CityMetrs Con = new Convert_WGS84_To_CityMetrs();
        /// <summary>
        /// Циклов сглаживания сетки
        /// </summary>
        int CountSmooth;

        bool SmoothChecked;

        int attributes = 5;
        string[] ArtNames = { "Глубина", "Срез.Глубина", "Температура", "Скорость", "Курс" };

        double K = 2 * Math.PI / 360;
        /// <summary>
        /// Эволюция кривых
        /// </summary>
        public IClouds sc = null;
        /// <summary>
        /// Ссылка на метод синхронизации (ф.о.в.)
        /// </summary>
        [NonSerialized]
        public SendParamCloud sendParam = null;
        /// <summary>
        /// Таблица точек 
        /// </summary>
        string TName = "knot";
        DataTable pointsTable;


        double fxmin;
        double fxmax;
        double fymin;
        double fymax;


        public SelectDataForm()
        {
            InitializeComponent();

            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            saveFileDialog1.Filter = "N, координаты, ном. глубина, срез. глубина, время, дата, скорости (*.bed)|*.bed|" +
                                     "Формат для TrianleNet (.node)|.node|" +
                                     "Оба формата (*.*)|*.*";
        }

        private void btLoadData_Click(object sender, EventArgs e)
        {
            string strSelect = "SELECT CAST(knot.knot_datetime AS DATE) as mydate FROM knot"
                + " GROUP BY CAST(knot.knot_datetime AS DATE) ORDER BY mydate";

            DataTable mapTable = ConnectDB.GetDataTable(strSelect, TName);

            cListBoxDates.Items.Clear();
            foreach (DataRow dr in mapTable.Rows)
                cListBoxDates.Items.Add(dr["mydate"]);

            for (int i = 0; i < cListBoxDates.Items.Count; i++)
                cListBoxDates.SetItemChecked(i, cbAllData.Checked);

            //Connect.GetMinMaxCoords();
            btSelectData.Enabled = true;
        }

        private void cbAllData_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cListBoxDates.Items.Count; i++)
                cListBoxDates.SetItemChecked(i, cbAllData.Checked);
        }
        /// <summary>
        /// Загрузка данных
        /// </summary>
        protected void LoadDataNodes()
        {
            if (cListBoxDates.Items.Count <= 0) return;
            string filter = GetSqlFilter();
            if (filter == " (  '2100.01.01' ) ")
            {
                tbMessage.Text = "Фильтр данных пуст! Выберете набор...";
                return;
            }
            pointsTable = GetSelectDataTable(filter);
            int k = 0;
            double K = 2 * Math.PI / 360;
            sc = new CloudRiverNods();
            int mark = 0;
            foreach (DataRow dr in pointsTable.Rows)
            {
                double x = (double)dr["knot_longitude"];
                double y = (double)dr["knot_latitude"];

                if(rbGrad.Checked == false)
                    Con.WGS84_To_LocalCity(ref x, ref y);

                double H = (double)dr["knot_fulldepth"];
                double sH = (double)dr["knot_depth"];
                double T = (double)dr["knot_temperature"];
                double V = (double)dr["knot_speed"];
                double C = (double)dr["knot_course"];
                double Vx = V * Math.Sin(C * K);
                double Vy = V * Math.Cos(C * K);
                sc.AddNode(x, y, mark, new double[5] { H, sH, T, V, C });
                k++;
            }
            SetSaveCloud(sc);
            if (filter != " ( ")
            {
                tbMessage.Text = "Набор из " + pointsTable.Rows.Count.ToString() + " вершин загружен";
                return;
            }
        }
        private void btSelectData_Click(object sender, EventArgs e)
        {
            LoadDataNodes();
        }
        private void loadDataNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDataNodes();
        }
        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            LoadDataNodes();
        }

        /// <summary>
        /// Метод синхронизирует передачу объекта SavePoint между разными потоками
        /// (потока вычислений и потока контрола)
        /// </summary>
        /// <param name="sp"></param>
        protected void SetSaveCloud(IClouds sc)
        {
            if (this.InvokeRequired)
            {
                SendParamCloud d = new SendParamCloud(gdI_ControlClouds1.SendSaveCloud);
                this.Invoke(d, new object[] { sc });
            }
            else
                gdI_ControlClouds1.SendSaveCloud(sc);
        }

        public bool GetFilter()
        {
            PointF[] FilterPoints = gdI_ControlClouds1.FilterPoints;
            if (FilterPoints != null)
            {
                double ax = FilterPoints[0].X;
                double ay = FilterPoints[0].Y;
                double bx = FilterPoints[1].X;
                double by = FilterPoints[1].Y;

                fxmin = Math.Min(ax, bx);
                fxmax = Math.Max(ax, bx);
                fymin = Math.Min(ay, by);
                fymax = Math.Max(ay, by);
            }
            return FilterPoints != null;
        }

        /// <summary>
        /// Сохранение обласка в файл с учетом фильтрации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSaveCloud_Click(object sender, EventArgs e)
        {
            bool flagFilter = GetFilter();
            if (flagFilter == false)
                throw new Exception("Не определен фильтр для узлов");

            double Hd = 0;//задаваемая на форме срезка 
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                int buf = 0;

                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    string FileName = saveFileDialog1.FileName;
                    string FileEXT = Path.GetExtension(FileName);
                    if (FileEXT == ".node")
                    {
                        foreach (DataRow dr in pointsTable.Rows)
                        {
                            double xV = Convert.ToDouble(dr["knot_longitude"]);
                            double yV = Convert.ToDouble(dr["knot_latitude"]);
                            if ((xV >= fxmin) && (xV <= fxmax) && (yV >= fymin) && (yV <= fymax))
                                buf++;
                        }
                        sw.WriteLine("{0} 2 1 0", buf);
                        buf = 0;
                        foreach (DataRow dr in pointsTable.Rows)
                        {
                            double xV = Convert.ToDouble(dr["knot_longitude"]);
                            double yV = Convert.ToDouble(dr["knot_latitude"]);
                            double H = Convert.ToDouble(dr["knot_fulldepth"]);
                            double tV = Convert.ToDouble(dr["knot_temperature"]);
                            DateTime DTk = Convert.ToDateTime(dr["knot_datetime"]);
                            double Hg = Convert.ToDouble(dr["experiment_waterlevel"]) / 100;
                            double Ho = H - Hg + Hd;
                            int marker = 1;
                            if ((xV >= fxmin) && (xV <= fxmax) && (yV >= fymin) && (yV <= fymax))
                            {
                                buf++;
                                sw.WriteLine("{0}  {1}  {2}  {3}  {4}", buf, xV, yV, Ho, marker);
                            }
                        }
                    }
                    if (FileEXT == ".bed")
                    {
                        buf = 0;
                        double kd = 0.01;
                        foreach (DataRow dr in pointsTable.Rows)
                        {
                            double xV = Convert.ToDouble(dr["knot_longitude"]);
                            double yV = Convert.ToDouble(dr["knot_latitude"]);
                            double H = Convert.ToDouble(dr["knot_fulldepth"]);
                            double tV = Convert.ToDouble(dr["knot_temperature"]);
                            DateTime DTk = Convert.ToDateTime(dr["knot_datetime"]);
                            double Hg = Convert.ToDouble(dr["experiment_waterlevel"]) / 100;
                            double Ho = H - Hg + Hd;

                            double LatX = Convertor_SK42_to_WGS84.SK42BTOX(xV, yV, 10);
                            double LonY = Convertor_SK42_to_WGS84.SK42LTOY(xV, yV, 10);

                            if ((xV >= fxmin) && (xV <= fxmax) && (yV >= fymin) && (yV <= fymax))
                            {
                                buf++;
                                sw.WriteLine("{0}  {1}  {2}  {3} {4}", buf, LatX, LonY, Ho, kd);
                            }
                        }
                        sw.WriteLine();
                        sw.WriteLine("no more nodes.");
                        sw.WriteLine();
                        sw.WriteLine("no more breakline segments.");
                    }
                    sw.Close();
                }

            }
        }

        public void ShowMesh(MeshNet mesh)
        {
            IFEMesh bmesh = null;
            //if (optionsGenMesh.RenumberMesh == false)
            //    MeshAdapter.Adapter(ref bmesh, meshDel1);
            //else
            //    MeshAdapter.FrontRenumberation(ref bmesh, meshDel1, Direction.toRight);
            //MeshAdapter.ConvertFrontRenumberation(ref bmesh, mesh, Direction.toRight, 0);

            double[][] values = null;

            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, mesh, Direction.toRight);

            if (bmesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, bmesh);
                double[] x = bmesh.GetCoords(0);
                double[] y = bmesh.GetCoords(1);
                data.Add(new Field1D("Координата Х", x));
                data.Add(new Field1D("Координата Y", y));

                for (int i = 0; i < values.Length; i++)
                {
                    data.Add(new Field1D(ArtNames[i], values[i]));
                }
                double[][] p = bmesh.GetParams();
                for (int i = 0; i < p.Length; i++)
                    data.Add(new Field1D("Фу" + i.ToString(), p[i]));

                Form form = new ViForm(data);
                form.Show();
            }
        }

        private void tsb_Filter_Click(object sender, EventArgs e)
        {
        }
        
        #region Вспомогательные методы

        protected string GetSqlFilter()
        {
            string filter = " ( ";
            for (int i = 0; i < cListBoxDates.Items.Count; i++)
            {
                CheckState flag = cListBoxDates.GetItemCheckState(i);
                if (flag == CheckState.Checked)
                {
                    string d = cListBoxDates.Items[i].ToString();
                    DateTime dataA = DateTime.Parse(d);
                    string FL = dataA.ToString("yyyy-MM-dd ");
                    filter += "'" + FL + "',";
                }
            }
            filter += " '2100.01.01' ) ";
            return filter;
        }

        /// <summary>
        /// Получить полную таблицу данных
        /// </summary>
        /// <returns></returns>
        public DataTable GetSelectDataTable(string filter)
        {
            // код хабаровска 
            string place_id = "1";
            string strSelect = "SELECT knot_latitude, knot_longitude, knot_fulldepth, knot_depth, knot_temperature," +
                            " knot_speed, knot_course, knot_datetime, CAST(knot.knot_datetime AS DATE) as DTime," +
                            " experiment_waterlevel" +
                            " FROM knot, experiment where  experiment.place_id = " + place_id + " and " +
                            " CAST(knot.knot_datetime AS DATE) = CAST(experiment_datetime AS DATE) " +
                            " and CAST(knot.knot_datetime AS DATE) IN " + filter;
            DataTable pTable = ConnectDB.GetDataTable(strSelect, TName);
            return pTable;
        }
        /// <summary>
        /// Получить облако данных
        /// </summary>
        /// <param name="invertices"></param>
        /// <param name="flagFilter"></param>
        /// <returns></returns>
        protected IPolygon GetCloudPoints(int invertices, bool flagFilter = false)
        {
            IPolygon cloudPoints = new Polygon(invertices);
            int k = 1;

            if (flagFilter == true)
            {
                foreach (DataRow dr in pointsTable.Rows)
                {
                    double x = (double)dr["knot_longitude"];
                    double y = (double)dr["knot_latitude"];
                    if (rbGrad.Checked == false)
                        Con.WGS84_To_LocalCity(ref x, ref y);

                    if (inArea(x, y, flagFilter))
                    {
                        double H = (double)dr["knot_fulldepth"];
                        double sH = (double)dr["knot_depth"];
                        double T = (double)dr["knot_temperature"];
                        double V = (double)dr["knot_speed"];
                        double C = (double)dr["knot_course"];

                        var v = new Vertex(x, y, 0);
                        // Read a vertex marker.
                        v.Label = 0;
                        v.ID = k;
#if USE_ATTRIBS
                        var attribs = new double[attributes];
                        attribs[0] = H;
                        attribs[1] = sH;
                        attribs[2] = T;
                        attribs[3] = V;
                        attribs[4] = C;
                        v.attributes = attribs;
#endif
                        cloudPoints.Add(v);
                    }
                }
            }
            else
            {
                foreach (DataRow dr in pointsTable.Rows)
                {
                    double x = (double)dr["knot_longitude"];
                    double y = (double)dr["knot_latitude"];
                                    if(rbGrad.Checked == false)
                    Con.WGS84_To_LocalCity(ref x, ref y);

                    double H = (double)dr["knot_fulldepth"];
                    double sH = (double)dr["knot_depth"];
                    double T = (double)dr["knot_temperature"];
                    double V = (double)dr["knot_speed"];
                    double C = (double)dr["knot_course"];
                    var v = new Vertex(x, y, 0);
                    // Read a vertex marker.
                    v.Label = 0;
                    v.ID = k;
#if USE_ATTRIBS
                    var attribs = new double[attributes];
                    attribs[0] = H;
                    attribs[1] = sH;
                    attribs[2] = T;
                    attribs[3] = V;
                    attribs[4] = C;
                    v.attributes = attribs;
#endif
                    cloudPoints.Add(v);
                }
            }
            return cloudPoints;
        }
        /// <summary>
        /// Фильтр - оно для данных
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="flagFilter"></param>
        /// <returns></returns>
        public bool inArea(double x, double y, bool flagFilter = false)
        {
            if (flagFilter == true)
                return (x >= fxmin) && (x <= fxmax) && (y >= fymin) && (y <= fymax);
            else
                return true;
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
            // создавать узлы на границах ?
            options.SegmentSplitting = cbSegmentSplitting.Checked == true ? 0 : 1;
            quality.MinimumAngle = (float)nUDMinimumAngle.Value;
            quality.MaximumAngle = (float)nUDMaximumAngle.Value;
            CountSmooth = (int)nUDCountSmooth.Value;
            SmoothChecked = checkBoxSM.Checked;
        }
        protected void SmoothMeshNet(ref MeshNet mesh)
        {
            if (SmoothChecked == true)
                for (int i = 0; i < CountSmooth; i++)
                {
                    var smoother = new SimpleSmoother();
                    smoother.Smooth(mesh);
                }
        }
    #endregion

    /// <summary>
    /// Создание сетки без фильтров
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btCreateMesh_Click(object sender, EventArgs e)
        {
            if (cListBoxDates.Items.Count <= 0) return;
            string filter = GetSqlFilter();
            pointsTable = GetSelectDataTable(filter);
            IPolygon cloudPoints = GetCloudPoints(pointsTable.Rows.Count);
            // генерация сетки по облаку точек
            SetCreateMeshOptions();
            MeshNet mesh = (MeshNet)cloudPoints.Triangulate(options, quality);
            SmoothMeshNet(ref mesh);
            ShowMesh(mesh);
        }

        private void btCreateFilterMesh_Click(object sender, EventArgs e)
        {
            if (cListBoxDates.Items.Count <= 0) return;
            bool flagFilter = GetFilter();
            if (flagFilter == false)
                throw new Exception("Не определен фильтр для узлов");
            string filter = GetSqlFilter();
            pointsTable = GetSelectDataTable(filter);
            IPolygon cloudPoints = GetCloudPoints(pointsTable.Rows.Count, flagFilter);
            // генерация сетки по облаку точек
            SetCreateMeshOptions(); 
            MeshNet mesh = (MeshNet)cloudPoints.Triangulate(options, quality);
            SmoothMeshNet(ref mesh);
            ShowMesh(mesh);
        }

        private void btCreateCountursMesh_Click(object sender, EventArgs e)
        {
            if (cListBoxDates.Items.Count <= 0) return;
            bool flagFilter = GetFilter();
            if (flagFilter == false)
                throw new Exception("Не определен фильтр для узлов");
            string filter = GetSqlFilter();
            pointsTable = GetSelectDataTable(filter);
            IPolygon cloudPoints = GetCloudPoints(pointsTable.Rows.Count, false);
            int pointsCount = cloudPoints.Points.Count;
            // генерация сетки по облаку точек
            List<CloudKnot> knots = gdI_ControlClouds1.Conturs;

            int conturPointsCount = knots.Count;

            if (conturPointsCount > 2)
            {
                int mark = 1;
                for (int k = 0; k < conturPointsCount; k++)
                {
                    CloudKnot knot = knots[k];
                    var v = new Vertex(knot.x, knot.y, 1);
                    // Read a vertex marker.
                    v.Label = mark;
                    v.ID = pointsCount + k;
#if USE_ATTRIBS
                    var attribs = new double[attributes];
                    attribs[0] = knot.Attributes[0];
                    attribs[1] = knot.Attributes[1];
                    attribs[2] = knot.Attributes[2];
                    attribs[3] = knot.Attributes[3];
                    attribs[4] = 0;
                    v.attributes = attribs;
#endif
                    cloudPoints.Add(v);
                }
                var points = cloudPoints.Points;
                
                for (int k=0; k< conturPointsCount; k++)
                {
                    int knotA = pointsCount + k;
                    int knotB = pointsCount + (k+1)% conturPointsCount;
                    Segment seg = new Segment(points[knotA], points[knotB], mark);
                    cloudPoints.Add(seg);
                }
            }
            PolygonAdapter.Save(cloudPoints, "test.poly");
            SetCreateMeshOptions();
            MeshNet mesh = (MeshNet)cloudPoints.Triangulate(options, quality);
            // SmoothMeshNet(ref mesh);
            ShowMesh(mesh);
        }

        private void SelectDataForm_Load(object sender, EventArgs e)
        {

        }

        private void gdI_ControlClouds1_Load(object sender, EventArgs e)
        {

        }
    }
}
