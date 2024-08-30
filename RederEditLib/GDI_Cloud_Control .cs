namespace RederEditLib
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using RenderLib;
    using CommonLib;
    using MemLogLib;

    using TriangleNet;
    using TriangleNet.Geometry;
    using TriangleNet.IO;
    using TriangleNet.Meshing;
    using TriangleNet.Smoothing;
    using TriangleNet.Tools;

    using TriMeshGeneratorLib;
    using MeshAdapterLib;
    using GeometryLib;
    using MeshLib;
    using CommonLib.Mesh;
    using GeometryLib.Areas;
    using GeometryLib.Vector;
    using CommonLib.Areas;

    public partial class GDI_Cloud_Control : UserControl
    {
        /// <summary>
        /// Менеджер отрисовки данных
        /// </summary>
        RenderEditControl control;
        /// <summary>
        /// Данные для отрисовки
        /// </summary>
        IClouds clouds = null;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        OptionsEdit options;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        RenderOptionsFields renderOptions;
        /// <summary>
        /// Опции герерации сетки в контуре
        /// </summary>
        OptionsGenMesh optionsGenMesh = new OptionsGenMesh();
        /// <summary>
        /// Настройка цветов для объектов рендеренга
        /// </summary>
        ColorSchemeFields colorScheme;
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        public List<CloudKnot> Conturs
        {
            get => conturs;
        }
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        List<CloudKnot> conturs = new List<CloudKnot>();

        public GDI_Cloud_Control()
        {
            InitializeComponent();
            options = new OptionsEdit();
            renderOptions = new RenderOptionsFields();
            colorScheme = new ColorSchemeFields();
            control = new RenderEditControl();
            if (control != null)
            {
                // привязка менеджера отрисовки к панели
                this.panel1.Controls.Add(control);
                control.BackColor = Color.White;
                control.Dock = DockStyle.Fill;
                control.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                control.Location = new System.Drawing.Point(0, 0);
                control.Name = "renderControl1";
                control.TabIndex = 0;
                control.Text = "";
                control.Visible = true;
                control.Initialize();
            }
            else
            {
                Text = "Eeee ..., не удалось инициализировать средство визуализации.";
            }
            SetRenderOptions();
            SetColorManager();
        }

        #region Обработчики события изменения размера
        /// <summary>
        /// Перегруженный метод скрола мыши для масштабирования изображения
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var container = this.panel1.ClientRectangle;
            System.Drawing.Point pt = e.Location;
            int Delta = e.Delta;
            if (container.Contains(pt))
            {
                control.ZoomWheel(((float)pt.X) / container.Width,
                    ((float)pt.Y) / container.Height, Delta);
            }
            base.OnMouseWheel(e);
        }
        /// <summary>
        /// Свернуть окно и развернуть
        /// </summary>
        private void ResizeHandler(object sender, EventArgs e)
        {
            control.HandleResize();
        }
        #endregion


        public void CreatConturs()
        {
            conturs.Clear();
            var Area = control.Area;
            // Отрисовка фигур
            foreach (var fig in Area.Figures)
            {
                List<IMPoint> points = fig.Points;
                for (int i = 0; i < points.Count; i++)
                {
                    var point = (CloudKnot) points[i].Point;
                    conturs.Add(new CloudKnot(point));
                }
            }
        }

        /// <summary>
        /// Установка данных и опций в RendererControl
        /// отрисовка параметров сетки в статус бар
        /// </summary>
        /// <param name="sp"></param>
        public void SendSaveCloud(IClouds cloud)
        {
            if (cloud != null)
            {
                clouds = cloud;
                // Установка данных о сетке и расчетных полей на ней
                // определение мирового региона для отрисовки области и данных
                // AccountingСurves - флаг учета масштаба кривых при расчете области отрисовки</param>
                // cloudData.SetSavePoint(sp, renderOptions.ckAccountingСurves);
                // Запись данных в списки компонента
                SetData(clouds);
                // Передача данных в прокси/рендер контрол
                control.SetData(clouds);
                SendOption();
                // отрисовка в статус бар
                tSSL_Nods.Text = clouds.CountKnots.ToString();
            }
        }
        /// <summary>
        /// Запись данных в списки компонента
        /// </summary>
        /// <param name="cloudData">Данные для отрисовки</param>
        public void SetData(IClouds clouds)
        {
            this.clouds = clouds;
            int SelectedIndex = listBoxPoles.SelectedIndex;
            List<string> Names = new List<string>(clouds.AttributNames);
            listBoxPoles.Items.Clear();
            if (Names.Count > 0)
            {
                foreach (var name in Names)
                    listBoxPoles.Items.Add(name);
                if (Names.Count > SelectedIndex && SelectedIndex > -1)
                    listBoxPoles.SelectedIndex = SelectedIndex;
                else
                    listBoxPoles.SelectedIndex = 0;
            }
            else
                listBoxPoles.SelectedIndex = -1;
        }
        //public void SetColorManager()
        //{
        //}
        ///// <summary>
        ///// Настройка интерфейса по умолчанию
        ///// </summary>
        //public void SetRenderOptions()
        //{
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            control.ClearNewFig();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            control.ClearFigs();
            listBoxPoints.Items.Clear();
            listBoxSegments.Items.Clear();
            listBoxFig.Items.Clear();
            lbBoundary.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> lNames = control.AddFigs();
            if (lNames != null)
            {
                string[] names = lNames.ToArray();
                listBoxFig.Items.Clear();
                listBoxFig.Items.AddRange(names);
                listBoxFig.SelectedIndex = 0;
                ChangeRB();
                CreatConturs();
            }
        }

        private void ChangeRB()
        {
            if (listBoxFig.SelectedIndex == -1)
            {
                rbCountur.Checked = false;
                rbHole.Checked = false;
                rbSubArea.Checked = false;
            }
            else
            { 
                FigureType ft = control.GetFType(listBoxFig.SelectedIndex);
                if (ft == FigureType.FigureContur)
                {
                    rbCountur.Checked = true;
                    rbHole.Checked = false;
                    rbSubArea.Checked = false;
                }
                else
                {
                    if (ft == FigureType.FigureHole)
                    {
                        rbCountur.Checked = false;
                        rbSubArea.Checked = false;
                        rbHole.Checked = true;
                    }
                    else
                    {
                        rbCountur.Checked = false;
                        rbSubArea.Checked = true;
                        rbHole.Checked = false;
                    }
                }
            }
        }

        private void btRemoveCountur_Click(object sender, EventArgs e)
        {
            if (listBoxFig.SelectedIndex != -1)
            {
                string[] names = control.RemoveFig(listBoxFig.SelectedIndex).ToArray();
                listBoxFig.Items.Clear();
                if (names.Length > 0)
                {
                    listBoxFig.Items.AddRange(names);
                    listBoxFig.SelectedIndex = 0;
                }
                else
                    listBoxFig.SelectedIndex = -1;
                ChangeRB();
            }
            if(listBoxFig.Items.Count == 0)
            {
                listBoxPoints.Items.Clear();
                listBoxSegments.Items.Clear();
                lbBoundary.Items.Clear();
                textBoxX.Text = "";
                textBoxY.Text = "";
            }
        }
        private void listBoxFig_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBoxFig.SelectedIndex!=-1)
            {
                // получаем текущую фигуру
                IMFigura fig = control.GetFig(listBoxFig.SelectedIndex);
                // добавляем узлы
                listBoxPoints.Items.Clear();
                foreach (var p in fig.Points)
                    listBoxPoints.Items.Add(p.Name);

                listBoxPoints.SelectedIndex = 0;
                IMPoint pf = fig.Points[listBoxPoints.SelectedIndex];
                textBoxX.Text = pf.Point.X.ToString("F5");
                textBoxY.Text = pf.Point.Y.ToString("F5");

                // добавляем  сегменты
                listBoxSegments.Items.Clear();
                foreach (var s in fig.Segments)
                   listBoxSegments.Items.Add(s.Name);
                listBoxSegments.SelectedIndex = 0;
                // список меток
                AddBM();
            }
            ChangeRB();
        }
        private void AddBM()
        {
            lbBoundary.Items.Clear();
            List<IMBoundary> BoundMark = control.Area.BoundMark;
            BoundMark.Sort();
            foreach (var bm in BoundMark)
            {
                if (bm.SegmentIndex.Count > 0)
                    lbBoundary.Items.Add(bm.Name);
            }
        }
        private void btSelectBM_Click(object sender, EventArgs e)
        {
            int FID = listBoxFig.SelectedIndex;
            // список выделенных индексов
            var ss = listBoxSegments.SelectedIndices;
            int[] idx = new int[ss.Count];
            for (int i = 0; i < ss.Count; i++)
                idx[i] = ss[i];
            control.AddBoundMark(FID, idx);
            // список меток
            AddBM();
            lbBoundary.SelectedIndex = 0;
            ChangeRB();
        }

        int oldidx = -1;
        private void lbBoundary_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (oldidx > -1 && oldidx == lbBoundary.SelectedIndex) return;
            if(lbBoundary.SelectedIndex!=-1)
            {
                oldidx = lbBoundary.SelectedIndex;
                // имя метки
                string bmName = lbBoundary.Items[lbBoundary.SelectedIndex].ToString();
                // метка
                IMBoundary bm = control.Area.GetMBoundaryByMarkName(bmName);
                if (bm != null)
                {
                    // переключение на фигуру
                    listBoxFig.SelectedIndex = control.Area.GetFigIndexByMarkName(bm.FiguraName);
                    // выделение сегментов
                    listBoxSegments.ClearSelected();
                    // 
                    foreach (var idx in bm.SegmentIndex)
                        listBoxSegments.SetSelected(idx, true);
                }
                control.SetSelectIndex(oldidx);
                lbBoundary.SelectedIndex = oldidx;
            }
        }
        private void listBoxPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPoints.SelectedIndex != -1 && listBoxFig.SelectedIndex != -1)
            {
                IMFigura fig = control.GetFig(listBoxFig.SelectedIndex);
                IMPoint pf = fig.Points[listBoxPoints.SelectedIndex];
                textBoxX.Text = pf.Point.X.ToString("F5");
                textBoxY.Text = pf.Point.Y.ToString("F5");
                //PointF p = new PointF(float.Parse(textBoxX.Text), float.Parse(textBoxY.Text));
                //control.UpDatePoint(p, listBoxFig.SelectedIndex, listBoxPoints.SelectedIndex);
            }
        }

        private void GDI_Edit_Control_Load(object sender, EventArgs e)
        {
            SetRegion();
            tcEdit.Invalidate();
           // SetEnabled();
        }

        private void SetRegion()
        {
            try
            {
                //PointF a = new PointF(float.Parse(textBoxXmin.Text, LOG.formatter),
                //                      float.Parse(textBoxYmin.Text, LOG.formatter));
                //PointF b = new PointF(float.Parse(textBoxXmax.Text, LOG.formatter),
                //                      float.Parse(textBoxYmax.Text, LOG.formatter));
                PointF a = new PointF(0,0);
                PointF b = new PointF(1,1);
                control.SetRegion(a, b);
                control.HandleResize();
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
            }
        }

        private void listBoxFig_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxFig.SelectedIndex != -1)
            {
                // получаем текущую фигуру
                IMFigura fig = control.GetFig(listBoxFig.SelectedIndex);
                if (fig.Status == FigureStatus.SelectedShape)
                    control.SetStatusFig(listBoxFig.SelectedIndex, FigureStatus.UnselectedShape);
                else
                    control.SetStatusFig(listBoxFig.SelectedIndex, FigureStatus.SelectedShape);
            }
        }

        private void listBoxPoints_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxPoints.SelectedIndex != -1 && listBoxFig.SelectedIndex != -1)
            {
                Vector2 coord = new Vector2(float.Parse(textBoxX.Text, MEM.formatter), float.Parse(textBoxY.Text, MEM.formatter));
                Vector2 atrib = new Vector2(float.Parse(textBoxH.Text, MEM.formatter), float.Parse(textBoxT.Text, MEM.formatter));
                control.UpDatePoint(coord, atrib, listBoxFig.SelectedIndex, listBoxPoints.SelectedIndex);

            }
        }
        private void buttonCreateMesh_Click(object sender, EventArgs e)
        {
            if (listBoxFig.SelectedIndex > -1)
            {
                SetOptionsEdit();
                control.optionsMesh = optionsGenMesh;
                CreateMesh(listBoxFig.SelectedIndex, 1);
            }
        }
        private void btMeshCreate_Click(object sender, EventArgs e)
        {
            if (listBoxFig.SelectedIndex > -1)
            {
                SetOptionsEdit();
                control.optionsMesh = optionsGenMesh;
                CreateAllMesh();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SetOptionsEdit();
            control.optionsMesh = optionsGenMesh;
            IMesh mesh  = CreateMeshV(listBoxFig.SelectedIndex, 1);
            if (mesh != null)
            {
                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);

                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                data.Add("Координата Х", x);
                data.Add("Координата Y", y);
                Form form = new ViForm(data);
                form.Show();
            }
        }
        private void rbCountur_CheckedChanged(object sender, EventArgs e)
        {
            if (listBoxFig.SelectedIndex != -1)
            {
                if (rbCountur.Checked == true)
                    control.SetFType(listBoxFig.SelectedIndex,  FigureType.FigureContur);
                if (rbSubArea.Checked == true)
                    control.SetFType(listBoxFig.SelectedIndex, FigureType.FigureSubArea);
                if (rbHole.Checked == true)
                    control.SetFType(listBoxFig.SelectedIndex, FigureType.FigureHole);
            }
        }

        private void buttonUDPoint_Click(object sender, EventArgs e)
        {
            Vector2 coord = new Vector2(float.Parse(textBoxX.Text,MEM.formatter), float.Parse(textBoxY.Text, MEM.formatter));
            Vector2 atrib = new Vector2(float.Parse(textBoxH.Text, MEM.formatter), float.Parse(textBoxT.Text, MEM.formatter));
            control.UpDatePoint(coord, atrib, listBoxFig.SelectedIndex, listBoxPoints.SelectedIndex);
        }
        private void btDelKnot_Click(object sender, EventArgs e)
        {
            control.DelLastKnot();
        }
        private void btShow_Click(object sender, EventArgs e)
        {
            SendOption();
        }

        public void SendOption()
        {
            colorScheme.formatText = (uint)nUD_formatText.Value;

          //  renderOptions.showFilter = tabControlState.SelectedIndex;
            renderOptions.indexValues = listBoxPoles.SelectedIndex;
            renderOptions.showBoudaryKnots = cb_showBoudaryKnots.Checked;
            renderOptions.opValuesKnot = cb_opValuesKnot.Checked;
            renderOptions.opVectorValues = cb_opVectorValues.Checked;
            renderOptions.coordReper = cb_coordReper.Checked;

            control.colorScheme = colorScheme;
            control.renderOptions = renderOptions;

            int indexPole = renderOptions.indexValues;
            int Dim = 0;
            double MinV = 0;
            double MaxV = 0;
            double[] Values = null;
            double[] ValuesX = null;
            double[] ValuesY = null;
            if (clouds == null)
                return;
            IField pole = clouds.GetPole(renderOptions.indexValues);
            if (CloudsUtils.GetPoleMinMax(pole, ref MinV, ref MaxV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == false)
                return;
            tSS_Max.Text = MaxV.ToString("F4");
            tSS_Min.Text = MinV.ToString("F4");
        }
        public void SetColorManager()
        {
            SetColorBrush();
            SetColorFont();
            nUD_formatText.Value = (uint)colorScheme.formatText;
        }
        public void SetRenderOptions()
        {
            cb_showBoudaryKnots.Checked = renderOptions.showBoudaryKnots;
            cb_opValuesKnot.Checked = renderOptions.opValuesKnot;
            cb_opVectorValues.Checked = renderOptions.opVectorValues;
            cb_coordReper.Checked = renderOptions.coordReper;
        }

        #region Работа со шрифтами
        private void SetColorFont()
        {
            button6.Font = colorScheme.FontReper;
            button7.Font = colorScheme.FontKnot;
            button8.Font = colorScheme.FontValue;
        }
        public Font GetFont(Font b, object sender)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                b = fontDialog1.Font;
                (sender as Button).Font = fontDialog1.Font;
            }
            return b;
        }

        #endregion

        #region Работа с кистями

        public SolidBrush GetBraush(SolidBrush b, object sender)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b = new SolidBrush(colorDialog1.Color);
                (sender as Button).BackColor = colorDialog1.Color;
            }
            return b;
        }
      
        #endregion

        #region Работа с перьями
        private Pen GetPen(Pen b, int d, object sender)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b = new Pen(colorDialog1.Color, d);
                (sender as Button).BackColor = colorDialog1.Color;
            }
            return b;
        }
        #endregion

        #region Методы для генерации КЭ сетки ===============================================

        private void SetOptionsEdit()
        {
            optionsGenMesh.MinimumAngle = (int)nUDMinimumAngle.Value;
            optionsGenMesh.MaximumAngle = (int)nUDMaximumAngle.Value;
            int proc = (int)nUDLenghtMax.Value;
            RectangleWorld reg = control.GetRegion();
            double ds = reg.MaxScale / proc;
            optionsGenMesh.MaximumArea = 0.5 * ds * ds;
            optionsGenMesh.SegmentSplitting = cbSegmentSplitting.Checked == true ? 1 : 0;
            if (cbSegmentSplitting.Checked == true)
            {
                optionsGenMesh.MinimumAngle = (int)1;
                optionsGenMesh.MaximumAngle = (int)179;
            }
            optionsGenMesh.CountSmooth = (int)nUDCountSmooth.Value;
            optionsGenMesh.CreateContur = cbCreateContur.Checked;
            optionsGenMesh.ConformingDelaunay = cbConformingDelaunay.Checked;
            optionsGenMesh.DirectionRenumber = Direction.toRight;
            optionsGenMesh.RenumberMesh = checkBoxReMeshDel.Checked;
        }

        /// <summary>
        /// генерация сетки для выбранной фигуры
        /// </summary>
        /// <param name="indexFig"></param>
        /// <param name="typeMeshGenerator"></param>
        public void CreateMesh(int indexFig, int typeMeshGenerator)
        {
            if (typeMeshGenerator == 0)
                CreateMeshRiverTri(indexFig);
            if (typeMeshGenerator == 1)
                CreateMeshTriangle(indexFig);
        }

        public void CreateAllMesh()
        {
            MArea mArea = control.Area;
            ConstraintOptions options = new ConstraintOptions();
            QualityOptions quality = new QualityOptions();
            //Statistic statisticMesh = new Statistic();
            IPolygon polygon = new Polygon();
            
            for (int indexFig = 0; indexFig < mArea.Count; indexFig++)
            {
                IMFigura fig = mArea[indexFig];
                //int[] markers = new int[fig.Count];
                List<int> markers = new List<int>(new int[fig.Count]);
                //markers.AddRange(new int[fig.Count]);
                // ищем маркеры границы сегментов текущей фигуры
                foreach (var mark in mArea.BoundMark)
                {
                    if(mark.FID == fig.FID)
                    {
                        for (int i = 0; i < mark.SegmentIndex.Count; i++)
                            markers[mark.SegmentIndex[i]] = mark.ID+2;
                    }
                }
                //IBoundaryLink boundaryLink = mArea.BoundaryLinks[]
                double x = 0, y = 0;
                List<Vertex> vertexs = new List<Vertex>();
                //List<int> Markers = new List<int>();
                //Markers.AddRange(markers);
                // создаем 
                for (int i = 0; i < fig.Count; i++)
                {
                    IMPoint node = fig.GetPoint(i);
                    
                    x += node.Point.X;
                    y += node.Point.Y;
                    vertexs.Add(new Vertex(node.Point.X, node.Point.Y, indexFig + 1));
                }
                // Contour contour = new Contour(vertexs, indexFig + 1);
                // контур с маркерами
                Contour contour = new Contour(vertexs, markers);
                if (FigureType.FigureHole == fig.FType)
                {
                    x /= fig.Count;
                    y /= fig.Count;
                    TriangleNet.Geometry.Point center = new TriangleNet.Geometry.Point(x, y);
                    polygon.Add(contour, center);
                }
                else
                {
                    polygon.Add(contour);
                }
            }
            //double dx = 5.0 / 100;
            //double Area = 0.3 * dx * dx; 
            //optionsGenMesh.MaximumArea = Area;
            optionsGenMesh.GetOptionsGenMesh(ref options, ref quality);
            // Triangulate the polygon
            // генерация сетки контуру
            MeshNet meshDel1 = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
            //FileProcessor.Write(meshDel1, "booble.node");
            for (int i = 0; i < optionsGenMesh.CountSmooth; i++)
            {
                var smoother = new SimpleSmoother();
                smoother.Smooth(meshDel1);
            }

            ShowMesh(meshDel1);
        }

        public void ShowMesh(MeshNet meshDel1)
        {
            //CommonLib.IMesh bmesh = null;
            IFEMesh bmesh = null;
            //if (optionsGenMesh.RenumberMesh == false)
            //    MeshAdapter.Adapter(ref bmesh, meshDel1);
            //else
            //    MeshAdapter.FrontRenumberation(ref bmesh, meshDel1, Direction.toRight);
            MeshAdapter.ConvertFrontRenumberation( ref bmesh, meshDel1, Direction.toRight, 0);
            if (bmesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, bmesh);
                double[] x = bmesh.GetCoords(0);
                double[] y = bmesh.GetCoords(1);
                data.Add(new Field1D("Координата Х", x));
                data.Add(new Field1D("Координата Y", y));
                double[][] p = bmesh.GetParams();
                for (int i = 0; i < p.Length; i++)
                    data.Add(new Field1D("Фу"+i.ToString(), p[i]));
                Form form = new ViForm(data);
                form.Show();
            }
        }

        public CommonLib.IMesh CreateMeshV(int indexFig, int typeMeshGenerator)
        {
            ConstraintOptions options = new ConstraintOptions();
            QualityOptions quality = new QualityOptions();
            Statistic statisticMesh = new Statistic();
            IPolygon polygon = new Polygon();
            MArea mArea = control.Area;
            IMFigura fig = mArea[indexFig];
            List<Vertex> vertexs = new List<Vertex>();
            int label = 1;
            for (int i = 0; i < fig.Count; i++)
            {
                IMPoint node = fig.GetPoint(i);
                vertexs.Add(new Vertex(node.Point.X, node.Point.Y, label));
            }
            Contour contour = new Contour(vertexs, label);
            polygon.Add(contour);

            //double dx = 5.0 / 100;
            //double Area = 0.3 * dx * dx; 
            //optionsGenMesh.MaximumArea = Area;
            optionsGenMesh.GetOptionsGenMesh(ref options, ref quality);
            // Triangulate the polygon
            // генерация сетки контуру
            MeshNet meshDel1 = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
            FileProcessor.Write(meshDel1, "booble.node");
            for (int i = 0; i < optionsGenMesh.CountSmooth; i++)
            {
                var smoother = new SimpleSmoother();
                smoother.Smooth(meshDel1);
            }

            CommonLib.IMesh bmesh = null;
            if (optionsGenMesh.RenumberMesh == false)
                MeshAdapter.Adapter(ref bmesh, meshDel1);
            else
                MeshAdapter.FrontRenumberation(ref bmesh, meshDel1, Direction.toRight);
            return bmesh;
        }

        public void CreateMeshTriangle(int indexFig)
        {
            ConstraintOptions options = new ConstraintOptions();
            QualityOptions quality = new QualityOptions();
            Statistic statisticMesh = new Statistic();
            IPolygon polygon = new Polygon();
            MArea mArea = control.Area;
            IMFigura fig = mArea[indexFig];
            List<Vertex> vertexs = new List<Vertex>();
            int label = 1;
            for (int i = 0; i < fig.Count; i++)
            {
                IMPoint node = fig.GetPoint(i);
                vertexs.Add(new Vertex(node.Point.X, node.Point.Y, label));
            }
            Contour contour = new Contour(vertexs, label);
            polygon.Add(contour);

            //double dx = 5.0 / 100;
            //double Area = 0.3 * dx * dx; 
            //optionsGenMesh.MaximumArea = Area;
            optionsGenMesh.GetOptionsGenMesh(ref options, ref quality);
            // Triangulate the polygon
            // генерация сетки контуру
            MeshNet meshDel1 = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
            FileProcessor.Write(meshDel1, "booble.node");
            for (int i = 0; i < optionsGenMesh.CountSmooth; i++)
            {
                var smoother = new SimpleSmoother();
                smoother.Smooth(meshDel1);
            }
            ShowMesh(meshDel1);
        }

        public void CreateMeshRiverTri(int indexFig)
        {
            MArea mArea = control.Area;
            IMFigura fig = mArea[indexFig];
            RVHabitatPhysics physics = new RVHabitatPhysics();
            BuilderTIN builder = new BuilderTIN(fig);
            RVMeshRiver hmesh = builder.Create(physics);
            hmesh.GeneratingTriangularMesh();
            hmesh.GeneratingEdges();
            double[][] values = null;
            IMesh mesh1 = RVMeshAdapter.MeshFrontRenumberation(hmesh, ref values, Direction.toRight);
            if (mesh1 != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh1);
                double[] x = mesh1.GetCoords(0);
                double[] y = mesh1.GetCoords(1);
                data.Add(new Field1D("Координата Х", x));
                data.Add(new Field1D("Координата Y", y));
                SavePointData spData = new SavePointData();
                spData.SetSavePoint(data);
                Form form = new ViewForm(spData);
                form.ShowDialog();
            }
        }

        #endregion

        private void button10_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextKnot = GetBraush(colorScheme.BrushTextKnot, sender);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorScheme.BrushPoint = GetBraush(colorScheme.BrushPoint, sender);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextValues = GetBraush(colorScheme.BrushTextValues, sender);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            colorScheme.FontReper = GetFont(colorScheme.FontReper, sender);
            tbFontCoord.Font = colorScheme.FontReper;
        }
        private void SetColorBrush()
        {
            btField.BackColor = colorScheme.BrushPoint.Color;
            btTextKnot.BackColor = colorScheme.BrushTextKnot.Color;
            btMeshKnot.BackColor = colorScheme.BrushTextValues.Color;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            colorScheme.FontKnot = GetFont(colorScheme.FontKnot, sender);
            tbFontKnot.Font = colorScheme.FontKnot;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            colorScheme.FontValue = GetFont(colorScheme.FontValue, sender);
            tbFontValue.Font = colorScheme.FontValue;
        }
    }
}
