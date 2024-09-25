//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 09.08.2023 Потапов И.И.
//---------------------------------------------------------------------------
#define USE_ATTRIBS
namespace RiverDB.ConvertorOut
{
    using CommonLib;
    using CommonLib.Mesh;
    using ConnectLib;
    using GeometryLib;
    using GeometryLib.Areas;
    using MeshLib;
    using MeshLib.SaveData;
    using MeshAdapterLib;
    using RiverDB.ConvertorIn;
    using RenderLib;

    using System;
    using System.Data;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using TriangleNet;
    using TriangleNet.Geometry;
    using TriangleNet.Meshing;
    using TriangleNet.Smoothing;
    using RiverDB.Properties;
    using TriangleNet.IO;
    using CommonLib.Areas;
    using System.IO;
    using CommonLib.Geometry;
    using MemLogLib;
    using GeometryLib.Geometry;
    using System.Linq;

    public partial class FCreateCloudMesh : Form
    {
        ConstraintOptions options = new ConstraintOptions();
        QualityOptions quality = new QualityOptions();
        Convert_WGS84_To_CityMetrs Con = new Convert_WGS84_To_CityMetrs();
        // список сегментов
        List<SegmentInfo> segInfo = new List<SegmentInfo>();
        /// <summary>
        /// Сетка результат для построения cdg файла
        /// </summary>
        MeshNet rMesh = null;
        /// <summary>
        /// Сетка источник
        /// </summary>
        MeshNet sMesh = null;
        SimpleSmoother smoother = null;
        ViForm baseform = null;
        /// <summary>
        /// Циклов сглаживания сетки
        /// </summary>
        int CountSmooth;

        bool SmoothChecked;

        const int CountAttributes = 5;
        string[] ArtNames = { "Глубина", "Срез.Глубина", "Температура", "Скорость", "Курс" };

        double K = 2 * Math.PI / 360;
        /// <summary>
        /// Границы расчетной области
        /// </summary>
        public IMArea Area = null;
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
        int placeID = 1; // Хабаровск
        public FCreateCloudMesh()
        {
            InitializeComponent();
            tsb_Contur.Checked = false;
            tsb_BeLine.Checked = false;
        }

        private void btLoadData_Click(object sender, EventArgs e)
        {
            GetDataFilter();
            
            
        }
        #region Работа с БД
        /// <summary>
        /// Получить списаок дат работы экспедиций
        /// </summary>
        protected void GetDataFilter()
        {
            string[] states =
                {
                " Старт соединения с БД ",
                " Данные получены ",
                " Сформирован список дат наблюдений ",
                " Доступ к БД отсутствует проверте строку соединения !"
                };
            try
            {
                LocalLog(states[0]);
                string strSelect = "SELECT CAST(knot.knot_datetime AS DATE) as mydate FROM knot"
                + " GROUP BY CAST(knot.knot_datetime AS DATE) ORDER BY mydate";

                DataTable mapTable = ConnectDB.GetDataTable(strSelect, TName);
                LocalLog(states[1]);

                cListBoxDates.Items.Clear();
                foreach (DataRow dr in mapTable.Rows)
                    cListBoxDates.Items.Add(dr["mydate"]);

                for (int i = 0; i < cListBoxDates.Items.Count; i++)
                    cListBoxDates.SetItemChecked(i, cbAllData.Checked);
                LocalLog(states[2]);
                btSelectData.Enabled = true;
            }
            catch (Exception ex)
            {
                LocalLog(states[3]);
                LocalLog(ex.Message);
            }
        }
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
        public DataTable GetSelectDataTable(string filter, int Place_id = 1)
        {
            // код хабаровска 
            string place_id = Place_id.ToString();
            string strSelect = "SELECT knot_id, knot_latitude, knot_longitude, knot_fulldepth, knot_depth, knot_temperature," +
                            " knot_speed, knot_course, knot_datetime, knot_marker, CAST(knot.knot_datetime AS DATE) as DTime," +
                            " experiment_waterlevel" +
                            " FROM knot, experiment where  experiment.place_id = " + place_id + " and " +
                            " CAST(knot.knot_datetime AS DATE) = CAST(experiment_datetime AS DATE) " +
                            " and CAST(knot.knot_datetime AS DATE) IN " + filter;
            DataTable pTable = ConnectDB.GetDataTable(strSelect, TName);
            return pTable;
        }
        /// <summary>
        /// Загрузка данных
        /// </summary>
        protected void LoadDataNodes()
        {
            string[] states =
            {
                " Фильтр данных пуст! Выберете набор...",
                " Облако данных сформировано",
            };
            if (cListBoxDates.Items.Count <= 0)
            {
                MessageBox.Show(states[0], "Замечание!",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information,
                   MessageBoxDefaultButton.Button1,
                   MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string filter = GetSqlFilter();
            if (filter == " (  '2100.01.01' ) ")
            {
                LocalLog(states[0]);
                return;
            }
            pointsTable = GetSelectDataTable(filter);
            sc = new SavePointRiverNods();

            List<CloudKnot> points = GetCloudKnots();
            foreach (CloudKnot p in points)
            {
                sc.AddNode(p);
            }
            LocalLog(states[1]);
            SetSaveCloud(sc);
            if (filter != " ( ")
            {
                string Text = "Набор из " + pointsTable.Rows.Count.ToString() + " вершин загружен";
                LocalLog(Text);
                return;
            }
        }
        /// <summary>
        /// Получить вершину CloudKnot из DataRow строки 
        /// </summary>
        /// <param name="dr">данные DataRow</param>
        /// <param name="k">индекс строки</param>
        /// <param name="v">вершина</param>
        /// <returns></returns>
        public bool GetCloudKnot(DataRow dr, ref CloudKnot p)
        {
            p = null;
            int mark = 0;
            double FiltrVelocity = (double)nud_Velocity.Value;
            double V = (double)dr["knot_speed"];
            // Если определен фильтр по скорости то выбираем только узлы
            // со скоростью меньше филтруемой скорости
            if (cbFilterVelocity.Checked == true)
            {
                if (V > FiltrVelocity) return false;
            }
            if (cb_MarkerNods.Checked == true)
            {
                int knot_marker = (int)dr["knot_marker"];
                if (knot_marker == 0) return false; 
            }
            double x = (double)dr["knot_longitude"];
            double y = (double)dr["knot_latitude"];
            if (rbGrad.Checked == false)
                Con.WGS84_To_LocalCity(ref x, ref y);
            double H = (double)dr["knot_fulldepth"];
            double sH = (double)dr["knot_depth"];
            double T = (double)dr["knot_temperature"];
            double C = (double)dr["knot_course"];
            int knot_id = (int)dr["knot_id"];
            var attribs = new double[CountAttributes];
            attribs[0] = H;
            attribs[1] = sH;
            attribs[2] = T;
            attribs[3] = V;// 3.6;
            attribs[4] = C;
            p = new CloudKnot(x, y, attribs, mark, knot_id);
            p.time = (DateTime)dr["knot_datetime"];
            return true;
        }

        public bool CalkNode(DataRow dr, ref CloudKnot a, double Length)
        {
            double error = Length * 0.05;
            CloudKnot v = null;
            if( GetCloudKnot(dr, ref v) == true )
            {
                if (MEM.Equals(a.X, v.x, error) == true &&
                    MEM.Equals(a.Y, v.y, error) == true)
                {
                    a = v;
                    return true;
                }
            }
            return false;
            //double x = (double)dr["knot_longitude"];
            //double y = (double)dr["knot_latitude"];
            //if (rbGrad.Checked == false)
            //    Con.WGS84_To_LocalCity(ref x, ref y);
            //if (MEM.Equals(a.X, x, error) == true &&
            //    MEM.Equals(a.Y, y, error) == true)
            //{
            //    double H = (double)dr["knot_fulldepth"];
            //    double sH = (double)dr["knot_depth"];
            //    double T = (double)dr["knot_temperature"];
            //    double V = (double)dr["knot_speed"];
            //    double C = (double)dr["knot_course"];
            //    a.X = x;
            //    a.Y = y;
            //    a.Attributes[0] = H;
            //    a.Attributes[1] = sH;
            //    a.Attributes[2] = T;
            //    a.Attributes[3] = V;
            //    a.Attributes[4] = C;
            //    a.ID = (int)dr["knot_id"];
            //    a.time = (DateTime)dr["knot_datetime"];
            //    return true;
            //}
        }

        /// <summary>
        /// Получить облако данных
        /// </summary>
        /// <param name="invertices"></param>
        /// <param name="flagFilter"></param>
        /// <returns></returns>
        protected List<CloudKnot> GetCloudKnots()
        {
            int ID = 0;
            List<CloudKnot> points = new List<CloudKnot>(pointsTable.Rows.Count);
            if (cbTreckVelocity.Checked == false)
            {
                CloudKnot ck = null;
                foreach (DataRow dr in pointsTable.Rows)
                {
                    if (GetCloudKnot(dr, ref ck) == true)
                        points.Add(ck);
                }
            }
            else
            {
                double FiltrVelocity = (double)nud_Velocity.Value;
                CloudKnot node = null;
                foreach (DataRow dr in pointsTable.Rows)
                {
                    if (GetCloudKnot(dr, ref node) == true)
                        points.Add(node);
                }
                // Сортировка по времени
                points.Sort((x, y) => x.time.CompareTo(y.time));
                // 
                double timeBreck = (double)nud_TimeBrack.Value;
                
                points[0].timeGroupID = ID;
                Console.Clear();
                HPoint velOld = null;
                Console.Clear();
                // Определение скоростных групп 
                for (int i = 1; i < points.Count; i++)
                {
                    CloudKnot p0 = points[i - 1];
                    CloudKnot p1 = points[i];
                    HPoint vel = p1 - p0;
                    var V = p0.Attributes[3];
                    double time = (p1.time - p0.time).TotalSeconds;
                    if (time > timeBreck)
                    {
                        ID++;
                        if (velOld != null)
                        {
                            points[i - 1].Attributes[3] = V * velOld.x;
                            points[i - 1].Attributes[4] = V * velOld.y;
                            velOld = null;
                        }
                        else
                        {
                            points[i - 1].Attributes[3] = 0;
                            points[i - 1].Attributes[4] = 0;
                        }
                        points[i - 1].timeGroupID = ID;
                    }
                    else
                    {
                        vel.Normalize();
                        double LL = vel.Length();
                        if (LL > 1.01 || V > FiltrVelocity)
                        {
                            vel.X = 0; vel.Y = 0;
                        }
                        points[i - 1].Attributes[3] = V * vel.x;
                        points[i - 1].Attributes[4] = V * vel.y;
                        points[i - 1].timeGroupID = ID;
                        Console.Write(points[i - 1].ID.ToString() + " " + p0.time.ToString() + "  " +
                        points[i - 1].timeGroupID.ToString() + "  " + V.ToString("F4") + " " + vel.ToString() + " ");
                        Console.WriteLine(p0.ToString());
                        velOld = vel;
                    }
                }
                var Ve = points[points.Count - 1].Attributes[3];
                points[points.Count - 1].Attributes[3] = Ve * velOld.x;
                points[points.Count - 1].Attributes[4] = Ve * velOld.y;
                points[points.Count - 1].timeGroupID = ID;
                Console.WriteLine(points[points.Count - 1].ToString());
            }
            return points;
        }

        /// <summary>
        /// Получить облако данных в фломате IPolygon
        /// </summary>
        /// <param name="invertices"></param>
        /// <param name="flagFilter"></param>
        /// <returns></returns>
        protected IPolygon GetCloudPoints()
        {
            List<CloudKnot> points = GetCloudKnots();
            IPolygon cloudPoints = new Polygon(points.Count);
            int ID = 1;
            int Marker = 0;
            foreach (var p in points)
            {
                Vertex v = PolygonUtils.ConvertCloudKnotToVertex(ID++, Marker, p);
                cloudPoints.Add(v);
            }
            return cloudPoints;
        }

        /// <summary>
        /// Сопряжение линий сглаживания 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsb_LinkSMLines_Click(object sender, EventArgs e)
        {
            if (cListBoxDates.Items.Count <= 0) return;
            /// Получить список линий сглаживания
            List<IHSmLine> sLine = gdI_EditControlClouds1.GetSLines();
            if (sLine.Count <= 0) return;

            string filter = GetSqlFilter();
            pointsTable = GetSelectDataTable(filter);
            double[] Length = new double[sLine.Count];
            for (int i = 0; i < sLine.Count; i++)
                Length[i] = sLine[i].Length();
            // линковка на основном облаке
            foreach (DataRow dr in pointsTable.Rows)
            {
                for (int i = 0; i < sLine.Count; i++)
                {
                    if (sLine[i].LinkA == false)
                    {
                        CloudKnot a = (CloudKnot)sLine[i].A;
                        sLine[i].LinkA = CalkNode(dr, ref a, Length[i]);
                        if (sLine[i].LinkA == true)
                            sLine[i].A = a;

                    }
                    if (sLine[i].LinkB == false)
                    {
                        CloudKnot a = (CloudKnot)sLine[i].B;
                        sLine[i].LinkB = CalkNode(dr, ref a, Length[i]);
                        if (sLine[i].LinkB == true)
                            sLine[i].B = a;
                    }
                }
            }
            int flagLinks = 0;
            for (int i = 0; i < sLine.Count; i++)
            {
                if (sLine[i].Link == true)
                    flagLinks++;
            }
            if (flagLinks == sLine.Count)
                return;
            else
            {
                for (int link = 0; link < sLine.Count; link++)
                {
                    for (int ii = 0; ii < sLine.Count; ii++)
                    {
                        if (sLine[ii].Link == true)
                        {
                            for (int i = 0; i < sLine.Count; i++)
                            {
                                if (sLine[i].LinkA == false)
                                {
                                    CloudKnot a = (CloudKnot)sLine[i].A;
                                    sLine[i].LinkA = CalkCloudKnotNode(sLine[ii], ref a, Length[i]);
                                    if (sLine[i].LinkA == true)
                                        sLine[i].A = a;

                                }
                                if (sLine[i].LinkB == false)
                                {
                                    CloudKnot a = (CloudKnot)sLine[i].B;
                                    sLine[i].LinkB = CalkCloudKnotNode(sLine[ii], ref a, Length[i]);
                                    if (sLine[i].LinkB == true)
                                        sLine[i].B = a;
                                }
                            }
                        }
                    }
                    flagLinks = 0;
                    for (int i = 0; i < sLine.Count; i++)
                    {
                        if (sLine[i].Link == true)
                            flagLinks++;
                    }
                    if (flagLinks == sLine.Count)
                        break;
                }
            }
            gdI_EditControlClouds1.SendOption();
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
                SendParamCloud d = new SendParamCloud(gdI_EditControlClouds1.SendSaveCloud);
                this.Invoke(d, new object[] { sc });
            }
            else
                gdI_EditControlClouds1.SendSaveCloud(sc);
        }

        #endregion

        #region Работа с Сеткой
        /// <summary>
        /// Отобразить сетку в отдельном окне
        /// </summary>
        /// <param name="meshRiver"></param>
        public void ShowMesh(MeshNet meshRiver)
        {
            //VelW(meshRiver.Vertices);
            IFEMesh bmesh = null;
            double[][] values = null;
            MeshAdapter.ConvertFrontRenumberationAndCutting(ref bmesh, ref values, meshRiver, Direction.toRight);
            if (bmesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, bmesh);
                double[] x = bmesh.GetCoords(0);
                double[] y = bmesh.GetCoords(1);
                data.Add("Координата Х", x);
                data.Add("Координата Y", y);
                // Ноль графика - отметка репера по Балтийской системе
                double hr = ConnectDB.WaterLevelGP(placeID);
                if (hr > 0)
                {
                    List<double> list = new List<double>();
                    list.AddRange(values[1]);
                    for (int i = 0; i < list.Count; i++)
                        list[i] = hr - list[i];
                    data.Add("Отметки дна", list.ToArray());
                }
                if(cbFVelocity.Checked == true)
                {
                    if (cbTreckVelocity.Checked == false)
                    {
                        double[] Vx = null;
                        double[] Vy = null;
                        MEM.Alloc(x.Length, ref Vx);
                        MEM.Alloc(x.Length, ref Vy);
                        for (int i = 0; i < Vx.Length; i++)
                        {
                            double phi = values[4][i] * Math.PI / 180;
                            Vx[i] = values[3][i] * Math.Cos(phi + Math.PI / 4);
                            Vy[i] = values[3][i] * Math.Sin(phi + Math.PI / 4);
                        }
                        data.Add("Скорость по X", Vx);
                        data.Add("Скорость по Y", Vy);
                        data.Add("Скорость", Vx, Vy);
                    }
                    else
                    {
                        double fv = (double)nud_Velocity.Value;
                        double[] Vx = null;
                        double[] Vy = null;
                        MEM.Alloc(x.Length, ref Vx);
                        MEM.Alloc(x.Length, ref Vy);
                        for (int i = 0; i < Vx.Length; i++)
                        {
                            if(Math.Abs(values[3][i]) < fv)
                                Vx[i] = values[3][i];
                            else
                                Vx[i] = 0;
                            if (Math.Abs(values[4][i]) < fv)
                                Vy[i] = values[4][i];
                            else
                                Vy[i] = 0;
                        }
                        double mX1 = Vx.Max();
                        double mX2 = Vx.Min();
                        double mY1 = Vy.Max();
                        double mY2 = Vy.Min();
                        data.Add("Скорость по X", Vx);
                        data.Add("Скорость по Y", Vy);
                        data.Add("Скорость", Vx, Vy);
                    }
                }

                if (values != null)
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
        /// <summary>
        /// Добавление данных с линий сглаживания в облако натурных узлов
        /// </summary>
        /// <param name="cloudPoints"></param>
        public void AddSmLinesData(ref IPolygon cloudPoints)
        {
            List<IHSmLine> sLine = gdI_EditControlClouds1.GetSLines();
            int k = cloudPoints.Points.Count;
            for (int i = 0; i < sLine.Count; i++)
            {
                if (sLine[i].Link == true)
                {
                    /// Выгрузка интерполяционных данных о линии сглаживания
                    CloudKnot[] cloudKnots = ((HSmLine)sLine[i]).GetInterpolationData();
                    if (cloudKnots != null) // если линия сглаживания слинкована обработаем ее
                    {
                        for (int p = 0; p < cloudKnots.Length; p++)
                        {
                            CloudKnot nod = cloudKnots[p];
                            var v = new Vertex(nod.X, nod.Y, 0);
                            // Read a vertex marker.
                            v.Label = 0;
                            v.ID = k++;
                            double[] attribs = new double[CountAttributes];
                            MEM.MemCopy(ref attribs, nod.Attributes);
                            v.attributes = attribs;
                            cloudPoints.Add(v);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Создание КЭ сетки с контурным фильтром
        /// </summary>
        private void CreateCountursMesh()
        {
            // генерация сетки по облаку точек
            List<CloudKnot> knots = gdI_EditControlClouds1.Conturs;
            int conturPointsCount = knots.Count;

            if (conturPointsCount < 2)
            {
                if (MessageBox.Show(
                    "Продолжить генерацию?",
                    "Контур фильтра не задан!",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
                {
                    return;
                }
            }
            if (cListBoxDates.Items.Count <= 0) return;
            string filter = GetSqlFilter();
            pointsTable = GetSelectDataTable(filter);
            IPolygon cloudPoints = GetCloudPoints();
            int pointsCount = cloudPoints.Points.Count;
            if (conturPointsCount > 2)
            {
                int mark = 1;
                for (int k = 0; k < conturPointsCount; k++)
                {
                    CloudKnot knot = knots[k];
                    var v = PolygonUtils.ConvertCloudKnotToVertex(pointsCount + k, mark, knot);
                    cloudPoints.Add(v);
                }
                var points = cloudPoints.Points;
                for (int k = 0; k < conturPointsCount; k++)
                {
                    int knotA = pointsCount + k;
                    int knotB = pointsCount + (k + 1) % conturPointsCount;
                    Segment seg = new Segment(points[knotA], points[knotB], mark);
                    cloudPoints.Add(seg);
                }
            }
            // тестовый вывод 
            SetCreateMeshOptions();
            MeshNet meshRiver = (MeshNet)cloudPoints.Triangulate(options, quality);
            // SmoothMeshNet(ref meshRiver);
            ShowMesh(meshRiver);
        }
        /// <summary>
        /// Создать полигон для генерации сетки
        /// </summary>
        /// <param name="getCloud"></param>
        /// <returns></returns>
        public IPolygon CraetePolygon(bool getCloud = true)
        {
            if (cListBoxDates.Items.Count <= 0) return null;
            Area = gdI_EditControlClouds1.Area;
            if (Area?.Ready() == false)
            {
                tbMessage.Text = "Контуры области не определены";
                return null;
            }
            SetCreateMeshOptions();
            // фильтр для облака точек наблюдения
            string filter = GetSqlFilter();
            // список облака точек наблюдения
            pointsTable = GetSelectDataTable(filter);
            // облака данных для генератора сетки
            IPolygon cloudPoints = GetCloudPoints();

            IPolygon BCloudPoints = new Polygon(cloudPoints);
            segInfo.Clear();
            PolygonUtils.AddBoundaryCountur(Area, BCloudPoints, ref cloudPoints, ref segInfo, options, quality);

            return cloudPoints;
        }

        /// <summary>
        /// Создание КЭ сетки без фильтров в области
        /// </summary>
        private void CreateMesh()
        {
            if (cListBoxDates.Items.Count <= 0) return;
            // параметры сетки
            SetCreateMeshOptions();
            // фильтр для облака точек наблюдения
            string filter = GetSqlFilter();
            pointsTable = GetSelectDataTable(filter);
            // облака данных для генератора сетки
            IPolygon cloudPoints = GetCloudPoints();

            AddSmLinesData(ref cloudPoints);
            VelW(cloudPoints);
            MeshNet meshRiver = (MeshNet)cloudPoints.Triangulate(options, quality);
            //SmoothMeshNet(ref meshRiver);
            ShowMesh(meshRiver);
        }
        public void VelW(IPolygon cloudPoints)
        {
            foreach (var e in cloudPoints.Points)
            {
                Console.WriteLine("ID {0} Vx {1} Vy {2}", e.ID, e.attributes[3], e.attributes[4]);
            }
        }
        /// <summary>
        /// Создание КЭ сетки с фильтром области
        /// </summary>
        private void CreateAreaMesh()
        {
            IPolygon cloudPoints = null;
            // Создание полигона для генерации сетки но облаку натурных узлов
            if (GetCloudPointsForMesh(ref cloudPoints) == true)
            {
                // добавление контура области
                PolygonUtils.AddBoundarySimpleCountur(Area, ref cloudPoints);
                // создание сетки
                MeshNet meshRiver = (MeshNet)cloudPoints.Triangulate(options, quality);
                // отрисовка
                ShowMesh(meshRiver);
            }
        }

        /// <summary>
        /// Создание КЭ сетки с фильтром области и границами
        /// </summary>
        public void CreateBoundaryAreaMesh()
        {
            if (cListBoxDates.Items.Count <= 0) return;
            Area = gdI_EditControlClouds1.Area;
            if (Area?.Ready() == false)
            {
                tbMessage.Text = "Контуры области не определены";
                return;
            }
            SetCreateMeshOptions();
            // фильтр для облака точек наблюдения
            string filter = GetSqlFilter();
            // список облака точек наблюдения
            pointsTable = GetSelectDataTable(filter);
            // облака данных для генератора сетки
            IPolygon cloudPoints = GetCloudPoints();

            PolygonUtils.AddBoundaryBaseCountur(Area, ref cloudPoints);

            MeshNet meshRiver = (MeshNet)cloudPoints.Triangulate(options, quality);

            ShowMesh(meshRiver);
        }

        /// <summary>
        /// Создание полигона для генерации сетки но облаку натурных узлов
        /// </summary>
        /// <param name="cloudPoints"></param>
        /// <returns></returns>
        protected bool GetCloudPointsForMesh(ref IPolygon cloudPoints)
        {
            if (cListBoxDates.Items.Count <= 0) return false;
            Area = gdI_EditControlClouds1.Area;
            if (Area?.Ready() == false)
            {
                tbMessage.Text = "Контуры области не определены";
                return false;
            }
            SetCreateMeshOptions();
            // фильтр для облака точек наблюдения
            string filter = GetSqlFilter();
            // список облака точек наблюдения
            pointsTable = GetSelectDataTable(filter);
            // облака данных для генератора сетки
            cloudPoints = GetCloudPoints();
            if (cloudPoints != null)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Генерация сетки по контуру с добавлением к облачным данным узлов линий сглаживания
        /// </summary>
        protected MeshNet GetBaseMesh()
        {
            IPolygon polygon = null;
            // Создание полигона для генерации сетки но облаку натурных узлов
            if (GetCloudPointsForMesh(ref polygon) == true)
            {
                // добавления узлов из линий сглаживания
                AddSmLinesData(ref polygon);
                // создание полигона для аппроксимации глубин на границах области
                IPolygon BCloudPoints = new Polygon(polygon);
                // добавления узлов из линий сглаживания
                //AddSmLinesData(ref BCloudPoints);
                segInfo.Clear();
                if (PolygonUtils.AddBoundaryCountur(Area, BCloudPoints, ref polygon, ref segInfo, options, quality) == false)
                {
                    if (MessageBox.Show(
                        "Продолжить генерацию?",
                        "Граничные сегменты в области недоопределены",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
                    {
                        return null;
                    }
                }
                MeshNet meshRiver = null;
                try
                {
                    meshRiver = (MeshNet)polygon.Triangulate(options, quality);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    LocalLog("Генератор не может обработать выбранное множество точек!");
                }
                return meshRiver;
            }
            return null;
        }
        /// <summary>
        /// Генерация сетки по контуру без внутренних узлов
        /// </summary>
        /// <returns></returns>
        protected MeshNet GetCalkMesh()
        {
            if (cListBoxDates.Items.Count <= 0) return null;
            Area = gdI_EditControlClouds1.Area;
            if (Area?.Ready() == false)
            {
                tbMessage.Text = "Контуры области не определены";
                return null;
            }
            SetCreateMeshOptions();
            IPolygon polygon = new Polygon();
            PolygonUtils.CreateBoundaryCountur(Area, ref polygon, ref segInfo);
            int inOut = 0;
            foreach (SegmentInfo seg in segInfo)
                if (seg.type > 1) inOut++;
            if (inOut < 2)
            {
                if (MessageBox.Show(
                    "Продолжить генерацию?",
                    "Граничные сегменты в области недоопределены",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly) == DialogResult.No)
                {
                    return null;
                }
            }
            quality.MinimumAngle = (float)nUDMinimumAngle.Value;
            quality.MaximumAngle = (float)nUDMaximumAngle.Value;
            MeshNet meshRiver = (MeshNet)polygon.Triangulate(options, quality);
            return meshRiver;
        }

        /// <summary>
        /// Создание КЭ сетки с фильтром области и границами
        /// </summary>
        public void CreateBoundaryCountsAreaMesh()
        {
            IPolygon polygon = CraetePolygon();
            if (polygon != null)
            {
                MeshNet meshRiver = (MeshNet)polygon.Triangulate(options, quality);
                if (meshRiver != null)
                    ShowMesh(meshRiver);
            }
        }

        public void CreateFileTask()
        {
            if (rMesh == null)
            {
                if (sMesh != null)
                    rMesh = sMesh;
                else
                {
                    IPolygon polygon = CraetePolygon();
                    if (polygon != null)
                        rMesh = (MeshNet)polygon.Triangulate(options, quality);
                }
            }
            if (rMesh != null)
            {
                List<SegmentInfo> segInfoF = new List<SegmentInfo>();
                foreach (SegmentInfo seg in segInfo)
                    if (seg.type > 1)
                        segInfoF.Add(seg);

                //foreach (SegmentInfo seg in segInfoF)
                //{
                //    Vertex a = new Vertex(seg.pA.X, seg.pA.Y);

                //    Vertex b = new Vertex(seg.pB.X, seg.pB.Y);

                //}


                ExportMRF form = new ExportMRF(rMesh, segInfoF, placeID);
                form.Show();
            }
        }



        #endregion

        private void cbAllData_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cListBoxDates.Items.Count; i++)
                cListBoxDates.SetItemChecked(i, cbAllData.Checked);
        }
        private void btSelectData_Click(object sender, EventArgs e)
        {
            LoadDataNodes();
        }
        private void btCreateMesh_Click(object sender, EventArgs e)
        {
            CreateMesh();
        }

        private void btCreateCountursMesh_Click(object sender, EventArgs e)
        {
            CreateCountursMesh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateAreaMesh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CreateBoundaryAreaMesh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CreateBoundaryCountsAreaMesh();
        }
        private void btMrf_Click(object sender, EventArgs e)
        {
            CreateFileTask();
        }

        private void btSaveCloud_Click(object sender, EventArgs e)
        {
            SaveNodes();
        }

        protected void SaveNodes()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string ext = ".node";
            string ext1 = ".bed";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += "(*" + ext1 + ")|*" + ext1 + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            IPolygon polygon = CraetePolygon();
            if (pointsTable != null && polygon != null)
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    PolygonUtils.Save(pointsTable, sfd.FileName);
                }
            }
            else
            {
                LocalLog("Область выборки не задана!");
            }
        }

        protected void SaveContur()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string ext = ".сntr";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            List<CloudKnot> knots = gdI_EditControlClouds1.Conturs;
            if (knots.Count > 2)
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter file = new StreamWriter(sfd.FileName))
                        {
                            foreach (CloudKnot s in knots)
                                file.WriteLine(s.ToString());
                            file.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LocalLog(ex.Message);
                    }
                }
                else
                {
                    LocalLog("Контур не задан!");
                }
            }
        }
        protected void LoadContur()
        {
            OpenFileDialog sfd = new OpenFileDialog();
            string ext = ".сntr";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    gdI_EditControlClouds1.ClearContur();
                    using (StreamReader file = new StreamReader(sfd.FileName))
                    {
                        for (string line = file.ReadLine(); line != null; line = file.ReadLine())
                        {
                            CloudKnot knot = CloudKnot.Parse(line);
                            gdI_EditControlClouds1.AddCloudKnotToContur(knot);
                        }

                    }
                }
                catch (Exception ex)
                {
                    LocalLog(ex.Message);
                }
            }
            else
            {
                LocalLog("Контур не задан!");
            }
        }
        /// <summary>
        /// Сохранение расчетной области
        /// </summary>
        public void SaveArea()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string ext = ".area";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            IMArea Area = gdI_EditControlClouds1.Area;
            if (Area.Count > 0)
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter file = new StreamWriter(sfd.FileName))
                        {
                            int idxScale = rbMetrix.Checked == true ? 1 : 0;
                            file.WriteLine(idxScale + " # Размерность узлов 1 - метры, 0 - градусы");
                            // сохраняем состояние выборки данных
                            file.WriteLine(cListBoxDates.Items.Count.ToString() + " # Cохраняем состояние выборки данных");
                            for (int i = 0; i < cListBoxDates.Items.Count; i++)
                            {
                                CheckState flag = cListBoxDates.GetItemCheckState(i);
                                if (flag == CheckState.Checked)
                                    file.Write("1 ");
                                else
                                    file.Write("0 ");
                            }
                            file.WriteLine();
                            WriteCB(file, "Фильтры узлов ", cbFilterVelocity);
                            WriteCB(file, "Фильтры узлов ", cb_MarkerNods);
                            WriteCB(file, "Фильтры узлов ", cbTreckVelocity);
                            WriteCB(file, "Фильтры узлов ", cbFVelocity);
                            file.WriteLine(nud_Velocity.Value.ToString() + " # Максимальная скорость в узле");
                            file.WriteLine(nud_TimeBrack.Value.ToString() + " # Время разрыва трека в (с)");
                            file.WriteLine("# Расчетная область");
                            // сохраняем состояние фигур
                            Area = gdI_EditControlClouds1.Area;
                            file.WriteLine(Area.Figures.Count.ToString() + " # Количество фигур");
                            if (Area?.Ready() == true)
                            {
                                foreach (IMFigura s in Area.Figures)
                                {
                                    Figura fig = (Figura)s;
                                    fig.WriteCloud(file);
                                }
                            }
                            /// сохраняем  список линий сглаживания
                            List<IHSmLine> sLine = gdI_EditControlClouds1.GetSLines();
                            file.WriteLine(sLine.Count.ToString() + " # Линии сглаживания");
                            if (sLine.Count > 0)
                            {
                                foreach (IHSmLine s in sLine)
                                {
                                    HSmLine line = (HSmLine)s;
                                    string str = line.ToString();
                                    file.WriteLine(str);
                                }
                            }
                            // сохраняем состояние настроек генерации сетки
                            //SetCreateMeshOptions();
                            // список облака точек наблюдения
                            file.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LocalLog(ex.Message);
                    }
                }
                else
                {
                    LocalLog("Область не определена!");
                }
            }
        }
        public void WriteCB(StreamWriter file, string name, CheckBox cb)
        {
            int idxScale = cb.Checked == true ? 1 : 0;
            file.WriteLine(idxScale + " # "+ name + " " + cb.Text);
        }
        public void ReadCB(StreamReader file,ref CheckBox cb)
        {
            string line = file.ReadLine();
            string[] mas = (line.Trim()).Split(' ');
            int idx = int.Parse(mas[0]);
            if (idx > 0)
                cb.Checked = true;
            else
                cb.Checked = false;
        }
        /// <summary>
        /// Загрузка расчетной области
        /// </summary>
        public void LoadArea()
        {
            OpenFileDialog sfd = new OpenFileDialog();
            string ext = ".area";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    gdI_EditControlClouds1.ClearContur();
                    using (StreamReader file = new StreamReader(sfd.FileName))
                    {
                        string line = file.ReadLine();
                        string[] mas = (line.Trim()).Split(' ');
                        int idxScale = int.Parse(mas[0]);
                        if (idxScale > 0)
                            rbMetrix.Checked = true;
                        else
                            rbGrad.Checked = true;
                        if (tsb_BeLine.Checked == true)
                            tsb_Contur.Checked = false;

                        if (cListBoxDates.Items.Count < 1)
                            GetDataFilter();
                        line = file.ReadLine();
                        Console.WriteLine(line);
                        line = file.ReadLine();
                        mas = (line.Trim()).Split(' ');
                        for (int i = 0; i < mas.Length; i++)
                        {
                            if (mas[i] == "0")
                                cListBoxDates.SetItemChecked(i, false);
                            else
                                cListBoxDates.SetItemChecked(i, true);
                        }
                        ReadCB(file, ref cbFilterVelocity);
                        ReadCB(file, ref cb_MarkerNods);
                        ReadCB(file, ref cbTreckVelocity);
                        ReadCB(file, ref cbFVelocity);
                        line = file.ReadLine();
                        mas = (line.Trim()).Split(' ');
                        nud_Velocity.Value = int.Parse(mas[0]);
                        line = file.ReadLine();
                        mas = (line.Trim()).Split(' ');
                        nud_TimeBrack.Value = int.Parse(mas[0]);
                        LoadDataNodes();
                        string str1 = file.ReadLine(); // # Расчетная область
                        line = file.ReadLine();
                        mas = (line.Trim()).Split(' ');
                        int CountFigs = int.Parse(mas[0]);
                        gdI_EditControlClouds1.Area.Figures.Clear();
                        for (int i = 0; i < CountFigs; i++)
                        {
                            IMFigura fig = Figura.Read(file);
                            gdI_EditControlClouds1.AddFigs(fig);
                        }
                        string rLine = file.ReadLine(); //  # Линии сглаживания
                        string[] rLines = rLine.Split(' ');
                        int Count = int.Parse(rLines[0]);
                        if (Count > 0)
                        {
                            List<IHSmLine> sLine = new List<IHSmLine>();
                            for (int i = 0; i < Count; i++)
                            {
                                line = file.ReadLine();
                                HSmLine ss = HSmLine.Parse(line);
                                sLine.Add(ss);
                                gdI_EditControlClouds1.LoadSmLines(sLine);
                            }
                            SetEditState();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LocalLog(ex.Message);
                }
            }
            else
            {
                LocalLog("Контур не задан!");
            }
        }
        #region Меню
        private void sm_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void sm_ConnectFiltrDS_Click(object sender, EventArgs e)
        {
            GetDataFilter();
        }

        private void sm_LoadCloadData_Click(object sender, EventArgs e)
        {
            LoadDataNodes();
        }

        private void sm_ClearCountur_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.ClearContur();
        }

        private void sm_DelModsCountur_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.DelLastKnotCountur();
        }

        private void sm_AddFigToArea_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.SaveFigura();
        }

        private void sm_DelFigToArea_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.DelCurFigura();
        }

        private void sm_ClearArea_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.ClearArea();
        }

        private void sm_GenerMeshFroNods_Click(object sender, EventArgs e)
        {
            CreateMesh();
        }

        private void sm_GenerMeshFroCountur_Click(object sender, EventArgs e)
        {
            CreateCountursMesh();
        }

        private void sm_GenerMeshFroArea_Click(object sender, EventArgs e)
        {
            CreateBoundaryCountsAreaMesh();
        }

        private void sm_GenerWorkFile_Click(object sender, EventArgs e)
        {
            CreateFileTask();
        }
        private void sm_SsaveContur_Click(object sender, EventArgs e)
        {
            SaveContur();
        }



        #region Меню TSB
        private void tsb_ConnectFiltrDS_Click(object sender, EventArgs e)
        {
            GetDataFilter();
        }

        private void tsb_LoadCloadData_Click(object sender, EventArgs e)
        {
            LoadDataNodes();
        }

        private void tsb_ClearCountur_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.ClearContur();
        }
        private void tsb_DelNod_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.DelLastKnotCountur();
        }
        private void tsb_SaveFig_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.SaveFigura();
        }
        private void tsb_DelFig_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.DelCurFigura();
        }
        private void tsb_ClearArea_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.ClearArea();
        }
        private void tsb_GenerMeshFroNods_Click(object sender, EventArgs e)
        {
            CreateMesh();
        }
        private void tsb_CreateCountursMesh_Click(object sender, EventArgs e)
        {
            CreateCountursMesh();
        }

        private void tsb_CreateBoundaryCountsAreaMesh_Click(object sender, EventArgs e)
        {
            sMesh = GetBaseMesh();
            if (sMesh != null)
                ShowMesh(sMesh);
        }


        private void tsb_GenerWorkFile_Click(object sender, EventArgs e)
        {
            CreateFileTask();
        }

        #endregion Меню TSB

        #endregion Меню
        /// <summary>
        /// Улучшение сетки
        /// </summary>
        /// <param name="meshRiver"></param>
        protected void SmoothMeshNet(ref MeshNet meshRiver)
        {
            for (int i = 0; i < CountSmooth; i++)
            {
                var smoother = new SimpleSmoother();
                smoother.Smooth(meshRiver);
            }
        }
        private void LocalLog(string mes)
        {
            tbMessage.Text = mes; Console.WriteLine(tbMessage.Text);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            string[] states =
                {
                    " Старт локального улучшения сетки ",
                    " Локальное улучшение сетки выполнено! ",
                    " Критическая ошибка при локальном улучшении сетки"
                };

            if (rMesh == null)
                rMesh = GetCalkMesh();
            try
            {
                LocalLog(states[0]);
                rMesh.Refine(quality, true);
                LocalLog(states[1]);
            }
            catch (Exception ex)
            {
                LocalLog(states[2]);
                MessageBox.Show("Exception - Smooth", ex.Message, MessageBoxButtons.OK);
            }
            if (rMesh != null)
                ShowBaseMesh(rMesh);
        }

        private void tsCreateMesh_Click(object sender, EventArgs e)
        {
            string[] states =
            {
                " Старт построения сетки ",
                " Сетка создана! ",
                " Критическая ошибка при построении сетки"
            };
            try
            {
                LocalLog(states[0]);
                rMesh = GetCalkMesh();
                ShowBaseMesh(rMesh);
                LocalLog(states[1]);
            }
            catch (Exception ex)
            {
                LocalLog(states[2]);
                MessageBox.Show("Exception - Smooth", ex.Message, MessageBoxButtons.OK);
            }

        }

        public void ShowBaseMesh(MeshNet meshRiver)
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
                sp.Add(new Field1D("Координата Х", x));
                sp.Add(new Field1D("Координата Y", y));
                // Ноль графика - отметка репера по Балтийской системе
                double hr = ConnectDB.WaterLevelGP(placeID);
                if (hr > 0)
                {
                    List<double> list = new List<double>();
                    list.AddRange(values[1]);
                    for (int i = 0; i < list.Count; i++)
                        list[i] = hr - list[i];
                    Field1D zeta = new Field1D("Отметки дна", list.ToArray());
                    sp.Add(zeta);
                }
                if (values != null)
                    for (int i = 0; i < values.Length; i++)
                    {
                        sp.Add(new Field1D(ArtNames[i], values[i]));
                    }
                double[][] p = bmesh.GetParams();
                for (int i = 0; i < p.Length; i++)
                    sp.Add(new Field1D("Фу" + i.ToString(), p[i]));
                if (baseform == null)
                {
                    baseform = new ViForm(sp);
                    baseform.Show();
                }
                else
                {
                    if (baseform.CloseDO == true)
                    {
                        baseform = new ViForm(sp);
                        baseform.Show();
                    }
                    baseform.SetSavePoint(sp);
                }

            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            string[] states =
                {
                    " Старт процесса сглаживания сетки ",
                    " Сетка сглажена! ",
                    " Критическая ошибка при сглаживании "
                };

            if (rMesh == null)
                rMesh = GetCalkMesh();
            if (smoother == null)
                smoother = new SimpleSmoother();
            try
            {
                CountSmooth = (int)nUDCountSmooth.Value;
                LocalLog(states[0]);
                for (int i = 0; i < CountSmooth; i++)
                {
                    smoother.Smooth(rMesh, 10);
                    LocalLog(states[1]);
                }
            }
            catch (Exception ex)
            {
                LocalLog(states[2]);
                MessageBox.Show("Exception - Smooth", ex.Message, MessageBoxButtons.OK);
            }
            if (rMesh != null)
                ShowBaseMesh(rMesh);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            try
            {
                string[] states =
                {
                    " Построение сетки источника ",
                    " Расчет атрибутов начат ",
                    " Атрибуты рассчитаны успешно ",
                    " Атрибуты рассчитаны с артефактами сетки : ",
                    " Критическая ошибка при расчете атрибутов: "
                };
                LocalLog(states[0]);
                sMesh = GetBaseMesh();

                #region Полная сетка по натуральным вершинам без контура
                string filterBase = GetSqlFilter();
                var pointsTableBase = GetSelectDataTable(filterBase);
                IPolygon cloudPointsBase = GetCloudPoints();
                // генерация сетки по облаку точек
                MeshNet meshBase = (MeshNet)cloudPointsBase.Triangulate(options, quality);
                #endregion
                LocalLog(states[1]);

                int artCaout = PolygonUtils.CalkAttributes(sMesh, meshBase, ref rMesh);
                if (artCaout == -1)
                    LocalLog(states[4]);
                else
                    if (artCaout == 0)
                    LocalLog(states[2]);
                else
                {
                    string state = states[3] + " " + artCaout.ToString();
                    LocalLog(state);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception - Smooth", ex.Message, MessageBoxButtons.OK);
            }
            if (rMesh != null)
                ShowBaseMesh(rMesh);
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                toolStripButton4_Click(sender, e);
                toolStripButton2_Click(sender, e);
            }
        }
        private void tsb_SsaveContur_Click(object sender, EventArgs e)
        {
            SaveContur();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            LoadContur();
        }

        private void tsm_SaveCloudNodes_Click(object sender, EventArgs e)
        {
            SaveNodes();
        }

        private void tsm_LoadArea_Click(object sender, EventArgs e)
        {
            LoadArea();
        }

        private void tsm_SaveArea_Click(object sender, EventArgs e)
        {
            SaveArea();
        }
        private void sm_SsaveArea_Click(object sender, EventArgs e)
        {
            SaveArea();
        }

        private void tsb_Contur_Click(object sender, EventArgs e)
        {
            if (tsb_Contur.Checked == true)
                tsb_BeLine.Checked = false;
            SetEditState();
        }
        private void tsb_BeLine_Click(object sender, EventArgs e)
        {
            if (tsb_BeLine.Checked == true)
                tsb_Contur.Checked = false;
            SetEditState();
        }
        protected void SetEditState()
        {
            if (tsb_BeLine.Checked == true)
                gdI_EditControlClouds1.SetEditState(EditState.BeLine);
            else if (tsb_Contur.Checked == true)
                gdI_EditControlClouds1.SetEditState(EditState.Contur);
            else
                gdI_EditControlClouds1.SetEditState(EditState.NoState);
            gdI_EditControlClouds1.SendOption();
        }
        /// <summary>
        /// Удаление последней линий сглаживания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsb_delBeLine_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.DelSmLines();
        }
        /// <summary>
        /// Очистка всех линий сглаживания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsb_delAllLine_Click(object sender, EventArgs e)
        {
            gdI_EditControlClouds1.ClearSmLines();
        }
        
        /// <summary>
        /// Связь между линией сглаживания и узлом другой линией сглаживания 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="a"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public bool CalkCloudKnotNode(IHSmLine source, ref CloudKnot a, double Length)
        {
            double error = Length * 0.025;
            if (MEM.Equals(a.X, source.A.X, error) == true &&
                MEM.Equals(a.Y, source.A.Y, error) == true)
            {
                if (source.LinkA == true)
                    a = new CloudKnot((CloudKnot)source.A);
                else
                    a = new CloudKnot(source.A.X, source.A.Y, ((CloudKnot)source.B).Attributes);
                return true;
            }
            if (MEM.Equals(a.X, source.B.X, error) == true &&
                MEM.Equals(a.Y, source.B.Y, error) == true)
            {
                if (source.LinkB == true)
                    a = new CloudKnot((CloudKnot)source.B);
                else
                    a = new CloudKnot(source.B.X, source.B.Y, ((CloudKnot)source.A).Attributes);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Создание сетки с границами области
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_CreateBoundaryMesh_Click(object sender, EventArgs e)
        {
            tsb_CreateBoundaryCountsAreaMesh_Click(sender, e);
        }
    }
}
