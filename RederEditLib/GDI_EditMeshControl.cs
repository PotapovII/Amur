namespace RenderEditLib
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using MeshLib;
    using RenderLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Areas;

    //using TriangleNet;
    //using TriangleNet.IO;
    //using TriangleNet.Tools;
    //using TriangleNet.Meshing;
    //using TriangleNet.Geometry;
    //using TriangleNet.Smoothing;

    using MeshAdapterLib;
    using TriMeshGeneratorLib;

    using GeometryLib;
    using GeometryLib.Areas;
    using System.IO;
    using MeshLib.SaveData;

    public partial class GDI_EditMeshControl : UserControl
    {
        /// <summary>
        /// Форма владелец контрола
        /// </summary>
        Form owner = null;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        RenderOptionsFields renderOptions;
        /// <summary>
        /// Менеджер отрисовки данных
        /// </summary>
        RenderEditControl control;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        OptionsEdit options;
        /// <summary>
        /// Настройка цветов для объектов рендеренга
        /// </summary>
        ColorScheme colorScheme;
        /// <summary>
        /// Опции герерации сетки в контуре
        /// </summary>
        OptionsGenMesh optionsGenMesh = new OptionsGenMesh();
        /// <summary>
        /// Точка сохранения
        /// </summary>
        SavePoint sp = null;
        /// <summary>
        /// Данные для отрисовки
        /// </summary>
        SavePointData spData;
        /// <summary>
        /// 
        /// </summary>
        IClouds clouds = null;
        public GDI_EditMeshControl()
        {
            InitializeComponent();
            options = new OptionsEdit();
            colorScheme = new ColorScheme();
            control = new RenderEditControl();
            renderOptions = new RenderOptionsFields();
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

        public void SetColorManager()
        {

        }
        /// <summary>
        /// Настройка интерфейса по умолчанию
        /// </summary>
        public void SetRenderOptions()
        {

        }
        private void listBoxPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (listBoxPoints.SelectedIndex != -1 && listBoxFig.SelectedIndex != -1)
            //{
            //    IMFigura fig = control.GetFig(listBoxFig.SelectedIndex);
            //    IMPoint pf = fig.Points[listBoxPoints.SelectedIndex];
            //    textBoxX.Text = pf.Point.X.ToString("F5");
            //    textBoxY.Text = pf.Point.Y.ToString("F5");
            //    //PointF p = new PointF(float.Parse(textBoxX.Text), float.Parse(textBoxY.Text));
            //    //control.UpDatePoint(p, listBoxFig.SelectedIndex, listBoxPoints.SelectedIndex);
            //}
        }
        private void GDI_Edit_Control_Load(object sender, EventArgs e)
        {
            tcEdit.Invalidate();
            SetEnabled();
        }

        private void listBoxPoints_DoubleClick(object sender, EventArgs e)
        {
            //if (listBoxPoints.SelectedIndex != -1 && listBoxFig.SelectedIndex != -1)
            //{
            //    Vector2 coord = new Vector2(float.Parse(textBoxX.Text, MEM.formatter), float.Parse(textBoxY.Text, MEM.formatter));
            //    Vector2 atrib = Vector2.Zero;
            //    control.UpDatePoint(coord, atrib, listBoxFig.SelectedIndex, listBoxPoints.SelectedIndex);
            //}
        }
        private void btMeshCreate_Click(object sender, EventArgs e)
        {
            //if (listBoxFig.SelectedIndex > -1)
            //{
            //    SetOptionsEdit();
            //    control.optionsMesh = optionsGenMesh;
            //    CreateAllMesh();
            //}
        }



        private void buttonUDPoint_Click(object sender, EventArgs e)
        {
            //Vector2 coord = new Vector2(float.Parse(textBoxX.Text, MEM.formatter), float.Parse(textBoxY.Text, MEM.formatter));
            //Vector2 atrib = Vector2.Zero;
            //control.UpDatePoint(coord, atrib, listBoxFig.SelectedIndex, listBoxPoints.SelectedIndex);
        }

        #region Работа с полями =============================================================
        //protected void ShowFieldLink()
        //{
        //    MArea mArea = control.Area;
        //    lbAreaFields.Items.Clear();
        //    foreach (var link in mArea.AreaLinks)
        //    {
        //        string FieldName = task.Fields[link.indexField].Name;
        //        FieldName += "A:" + link.indexLink + " :" + link.ValueField.ToString("F4");
        //        lbAreaFields.Items.Add(FieldName);
        //    }
        //}
        //protected void ShowBoundaryFieldLink()
        //{
        //    MArea mArea = control.Area;
        //    lbBoundCond.Items.Clear();
        //    foreach (var link in mArea.BoundaryLinks)
        //    {
        //        string FieldName = task.Fields[link.indexField].Name;
        //        FieldName += "B:" + link.indexLink + " :" + link.ValueField.ToString("F4");
        //        lbBoundCond.Items.Add(FieldName);
        //    }
        //}

        private void SetEnabled(bool fl = false)
        {

        }
        //private void btCreateSCond_Click(object sender, EventArgs e)
        //{
        //    int  figID = listBoxFig.SelectedIndex;
        //    if (figID > -1)
        //    {
        //        MArea mArea = control.Area;
        //        int iField = lbUnKnow.SelectedIndex;
        //        double vField = double.Parse(tbSValue.Text);
        //        mArea.AddAreaLinks(new FLink(figID, iField, vField));
        //        ShowFieldLink();
        //    }
        //    else
        //    {

        //    }
        //}
        //private void btDeleteSCond_Click(object sender, EventArgs e)
        //{
        //    if (lbAreaFields.SelectedIndex > -1)
        //    {
        //        MArea mArea = control.Area;
        //        ILink link = mArea.AreaLinks[lbAreaFields.SelectedIndex];
        //        mArea.DelAreaLinks(new FLink(link));
        //        ShowFieldLink();
        //    }
        //    else
        //    {

        //    }
        //}
        //private void btCreateBCond_Click(object sender, EventArgs e)
        //{
        //    int bMarkID = lbBoundary.SelectedIndex;
        //    int iField = lbUnKnow.SelectedIndex;
        //    IMBoundary selMark = null;
        //    List<IMBoundary> BoundMark = control.Area.BoundMark;
        //    BoundMark.Sort();
        //    int k = 0;
        //    for(int i=0; i<BoundMark.Count; i++)
        //    {
        //        if (k == bMarkID)
        //        {
        //            selMark = BoundMark[k];
        //            break;
        //        }
        //        if (BoundMark[i].SegmentIndex.Count > 0)
        //            k++;
        //    }
        //    if (selMark != null)
        //    {
        //        MArea mArea = control.Area;
        //        int startID = selMark.SegmentIndex[0];
        //        double vField = double.Parse(tbBValue.Text);
        //        mArea.AddBoundaryLinks(new FBoundaryLink(startID, bMarkID, iField, vField));
        //    }
        //    ShowBoundaryFieldLink();
        //}
        //private void btDeleteBCond_Click(object sender, EventArgs e)
        //{
        //    if (lbBoundCond.SelectedIndex > -1)
        //    {
        //        MArea mArea = control.Area;
        //        IBoundaryLink link = mArea.BoundaryLinks[lbBoundCond.SelectedIndex];
        //        mArea.DelBoundaryLinks(new FBoundaryLink(link));
        //        ShowBoundaryFieldLink();
        //    }
        //    else
        //    {

        //    }
        //}
        //private void lbBoundCond_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    MArea mArea = control.Area;
        //    IBoundaryLink link = mArea.BoundaryLinks[lbBoundCond.SelectedIndex];
        //    lbBoundary.SelectedIndex = link.indexLink;
        //    lbUnKnow.SelectedIndex = link.indexField;
        //}
        #endregion

        #region Методы для генерации КЭ сетки ===============================================

        private void SetOptionsEdit()
        {
            //optionsGenMesh.MinimumAngle = (int)nUDMinimumAngle.Value;
            //optionsGenMesh.MaximumAngle = (int)nUDMaximumAngle.Value;
            //int proc = (int)nUDLenghtMax.Value;
            //RectangleWorld reg = control.GetRegion();
            //double ds = reg.MaxScale / proc;
            //optionsGenMesh.MaximumArea = 0.5 * ds * ds;
            //optionsGenMesh.SegmentSplitting = cbSegmentSplitting.Checked == true ? 1 : 0;
            //if (cbSegmentSplitting.Checked == true)
            //{
            //    optionsGenMesh.MinimumAngle = (int)1;
            //    optionsGenMesh.MaximumAngle = (int)179;
            //}
            //optionsGenMesh.CountSmooth = (int)nUDCountSmooth.Value;
            //optionsGenMesh.CreateContur = cbCreateContur.Checked;
            //optionsGenMesh.ConformingDelaunay = cbConformingDelaunay.Checked;
            //optionsGenMesh.DirectionRenumber = Direction.toRight;
            //optionsGenMesh.RenumberMesh = checkBoxReMeshDel.Checked;
        }
        
        #endregion

        public void SendOption()
        {
            colorScheme.formatText = (uint)nUD_formatText.Value;
            colorScheme.formatTextReper = (uint)nUD_formatReper.Value;
            // 15 07 24 изменяемый масштаб координатных осей
            colorScheme.scaleCoords = (int)nUD_formatCoordMan.Value;


            renderOptions.indexValues = listBoxPoles.SelectedIndex;
            renderOptions.showBoudary = cb_showBoudary.Checked;
            renderOptions.showBoudaryKnots = cb_showBoudaryKnots.Checked;
            renderOptions.showBoudaryElems = cb_showBoudaryElems.Checked;
            renderOptions.showElementNamber = cb_showElementNamber.Checked;
            renderOptions.showMesh = cb_showMesh.Checked;
            renderOptions.showKnotNamber = cb_showKnotNamber.Checked;
            renderOptions.coordReper = cb_coordReper.Checked;

            // 30 09 2021
            renderOptions.ckAccountingСurves = cb_AccountingСurves.Checked;
            renderOptions.ckScaleUpdate = cb_ckScaleUpdate.Checked;
            // 15 07 24 изменяемый масштаб полей
            renderOptions.scaleFields = (int)nUD_formatFieldScale.Value;

            int indexPole = renderOptions.indexValues;
            int Dim = 0;
            double MinV = 0;
            double MaxV = 0;
            double SumV = 0;
            double SAreaV = 0;
            double[] Values = null;
            double[] ValuesX = null;
            double[] ValuesY = null;
            if (spData == null)
                return;
            if (spData.GetPoleMinMax(indexPole, ref MinV, ref MaxV, ref SumV, ref SAreaV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == false)
                return;
            tSS_Max.Text = MaxV.ToString("F4");
            tSS_Min.Text = MinV.ToString("F4");
            tSS_Area.Text = SAreaV.ToString("F4");
        }

        /// <summary>
        /// Установка данных и опций в RendererControl
        /// отрисовка параметров сетки в статус бар
        /// </summary>
        /// <param name="sp"></param>
        public void SendSavePoint(ISavePoint isp)
        {
            sp = isp as SavePoint;
            if (sp != null)
            {
                SavePointData spData = new SavePointData();
                // Установка данных о сетке и расчетных полей на ней
                // определение мирового региона для отрисовки области и данных
                // AccountingСurves - флаг учета масштаба кривых при расчете области отрисовки</param>
                spData.SetSavePoint(sp, renderOptions.ckAccountingСurves);
                // Запись данных в списки компонента
                SetData(spData);
                // Передача данных в прокси/рендер контрол
                control.SetData(spData);
                SendOption();
                // отрисовка в статус бар
                tSSL_Nods.Text = sp.mesh.CountKnots.ToString();
                tss_TaskName.Text = sp.Name;
                if (owner != null) 
                    owner.Text = sp.Name;
                IRenderMesh tsp = sp.mesh as IRenderMesh;
                if (tsp != null)
                {
                    tSSL_Elems.Text = tsp.CountElements.ToString();
                }
            }
        }
        /// <summary>
        /// Запись данных в списки компонента
        /// </summary>
        /// <param name="spData">Данные для отрисовки</param>
        public void SetData(SavePointData spData)
        {
            this.spData = spData;
            int SelectedIndex = listBoxPoles.SelectedIndex;
            List<string> Names = spData.PoleNames();
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
            Names = spData.graphicsData.GraphicNames();
        }
        private void tsb_LoadMesh_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string sf = ofd.Filter;
            try
            {
                string ext = ".bed";
                string ext1 = ".node";
                string ext2 = ".cdg";
                string ext3 = ".mesh";
                string filter = "(*" + ext + ")|*" + ext + "| ";
                filter += "(*" + ext1 + ")|*" + ext1 + "| ";
                filter += "(*" + ext2 + ")|*" + ext2 + "| ";
                filter += "(*" + ext3 + ")|*" + ext3 + "| ";
                filter += " All files (*.*)|*.*";
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ImportSPMesh(ofd.FileName, ref clouds, ref sp);
                    SendSaveCloud(clouds);
                    SendSavePoint(sp);
                }
            }
            catch (Exception ex)
            {

            }
            ofd.Filter = sf;
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
        /// <summary>
        /// Сохраняем облако глубин в файл 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="FileName"></param>
        /// <param name="shift"></param>
        public void ImportSPMesh(string FileName, ref IClouds clouds, ref SavePoint sp)
        {
            clouds = null;
            sp = null;
            using (StreamReader files = new StreamReader(FileName))
            {
                string FileEXT = Path.GetExtension(FileName);
                if (FileEXT == ".bed")
                {
                    CloudBedRiverNods cloud = new CloudBedRiverNods();
                    for (string line = files.ReadLine(); line != null; line = files.ReadLine())
                    {
                        string[] lines = line.Split(' ');
                        if (lines.Length == 5)
                        {
                            BedRiverNode nod = BedRiverNode.Parse(lines);
                            cloud.AddNode(nod);
                        }
                    }
                    clouds = cloud;
                }
                if (FileEXT == ".sp")
                {
                    sp = new SavePoint();
                    sp = (SavePoint)sp.LoadSavePoint(FileName);
                }
                files.Close();
            }
        }

        private void tsb_Save_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string ext = ".bed";
            string ext1 = ".node";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += "(*" + ext1 + ")|*" + ext1 + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            //if (sp.mesh != null)
            //{
            //    if (sfd.ShowDialog() == DialogResult.OK)
            //    {
            //        sp.ImportSPMesh(sfd.FileName);
            //    }
            //}
        }
    }
}
