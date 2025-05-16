//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 18.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using GeometryLib;
    using GeometryLib.Vector;
    using RenderLib.Fields;

    using System;
    using System.IO;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;
    using CommonLib.Geometry;
    using MeshLib.Locators;
    using GeometryLib.Geometry;

    /// <summary>
    ///ОО: Компонент визуализации данных (сетки, и сеточных полей, кривых (устарело) ) 
    /// </summary>
    public partial class GDI_Control : UserControl
    {
        /// <summary>
        /// Форма владелец контрола
        /// </summary>
        Form owner = null;
        /// <summary>
        /// Владелец
        /// </summary>
        /// <param name="owner"></param>
        public void AddOwner(Form owner){ this.owner = owner; }
        /// <summary>
        /// Список наблюдаемых сечений
        /// </summary>
        List<CrossLine> ListCross = new List<CrossLine>();
        /// <summary>
        /// Менеджер отрисовки данных
        /// </summary>
        ProxyRendererControlFields proxyRendererControl;
        /// <summary>
        /// Данные для отрисовки
        /// </summary>
        SavePointData spData;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        RenderOptionsFields renderOptions;
        /// <summary>
        /// Настройка цветов для объектов рендеренга
        /// </summary>
        ColorSchemeFields colorScheme;
        /// <summary>
        /// Точка сохранения
        /// </summary>
        SavePoint sp = null;
        /// <summary>
        ///Класс для получения значений функции вдоль линии (створf) определенной над сеткой
        /// </summary>
        LocatorTriMeshFacet tLine = null;
        /// <summary>
        /// Эволюционная точка сохранения 
        /// </summary>
        ISavePoint sps = null;
        public GDI_Control()
        {
            InitializeComponent();
            openFileDialog1.Filter = "файл - точка сохранения rpsp (*.sp)|*.sp|" +
                                     "All files (*.*)|*.*";
            saveFileDialog1.Filter = "файл - точка сохранения rpsp (*.sp)|*.sp|" +
                                     "All files (*.*)|*.*";
            renderOptions = new RenderOptionsFields();
            colorScheme = new ColorSchemeFields();
            proxyRendererControl = new ProxyRendererControlFields();
            Control control = proxyRendererControl.RenderControl;
            if (control != null)
            {
                // привязка менеджера отрисовки к панели
                this.panel1.Controls.Add(control);
                control.BackColor = Color.White;
                control.Dock = DockStyle.Fill;
                control.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                control.Location = new System.Drawing.Point(0, 0);
                control.Name = "renderControl1";
                control.TabIndex = 0;
                control.Text = "";
                control.Visible = true;
                proxyRendererControl.Initialize();
            }
            else
            {
                Text = "Eeee ..., не удалось инициализировать средство визуализации.";
            }
            SetRenderOptions();
            SetColorManager();
            tbMin.Enabled = false;
            tbMax.Enabled = false;
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
            checkedListBoxCurve.Items.Clear();
            if (Names.Count > 0)
            {
                foreach (var name in Names)
                    checkedListBoxCurve.Items.Add(name);
                checkedListBoxCurve.SelectedIndex = 0;
            }
            else
                checkedListBoxCurve.SelectedIndex = -1;
        }
        /// <summary>
        /// Перегруженный метод скрола мыши для масштабирования изображения
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var container = this.panel1.ClientRectangle;
            Point pt = e.Location;
            if (container.Contains(pt))
            {
                proxyRendererControl.ZoomWheel(((float)pt.X) / container.Width,
                    ((float)pt.Y) / container.Height, e.Delta);
            }
            base.OnMouseWheel(e);
        }

        #region Обработчики события изменения размера
        //bool isResizing = false;
        //Size oldClientSize;
        /// <summary>
        /// Свернуть окно и развернуть
        /// </summary>
        private void ResizeHandler(object sender, EventArgs e)
        {
            proxyRendererControl.HandleResize();
        }
        #endregion

        /// <summary>
        /// Обновление опций
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btShow_Click(object sender, EventArgs e)
        {
            SendOption();
        }

        public void SendOption()
        {

            for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
            {
                if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Unchecked)
                    spData.graphicsData.curves[i].Check = false;
                else
                    spData.graphicsData.curves[i].Check = true;
            }

            colorScheme.MaxIsoLine = trackBarMax.Value;
            colorScheme.MinIsoLine = trackBarMin.Value;
            colorScheme.CountIsoLine = (int)nUD_CountIsoLine.Value;
            colorScheme.formatText      = (uint)nUD_formatText.Value;
            colorScheme.formatTextReper = (uint)nUD_formatReper.Value;
            // 15 07 24 изменяемый масштаб координатных осей
            colorScheme.scaleCoords = (int)nUD_formatCoordMan.Value;
   

            renderOptions.indexValues = listBoxPoles.SelectedIndex;
            renderOptions.showBoudary = cb_showBoudary.Checked;
            renderOptions.showBoudaryKnots = cb_showBoudaryKnots.Checked;
            renderOptions.showBoudaryElems  = cb_showBoudaryElems.Checked;
            renderOptions.showElementNamber = cb_showElementNamber.Checked;
            renderOptions.showMesh = cb_showMesh.Checked;
            renderOptions.showKnotNamber = cb_showKnotNamber.Checked;
            renderOptions.opFillValues = cb_opFillValues.Checked;
            renderOptions.opIsoLineValues = cb_opIsoLineValues.Checked;
            renderOptions.opIsoLineValues0 = cb_opIsoLineValues0.Checked;
            renderOptions.opValuesKnot = cb_opValuesKnot.Checked;
            renderOptions.opVectorValues = cb_opVectorValues.Checked;
            renderOptions.opGraphicCurve = cb_opGraphicCurve.Checked;
            renderOptions.coordReper = cb_coordReper.Checked;

            // 30 09 2021
            renderOptions.ckAccountingСurves = cb_AccountingСurves.Checked;
            renderOptions.ckScaleUpdate = cb_ckScaleUpdate.Checked;
            // 28 07 2022 работа со створами
            renderOptions.opTargetLine = cb_TargetLine.Checked;
            // 15 06 2024 шкала для градиентной заливки
            renderOptions.opGradScale = cb_GradScale.Checked;


            // 15 07 24 изменяемый масштаб полей
            renderOptions.scaleFields = (int)nUD_formatFieldScale.Value;
            renderOptions.opIsoLineValuesShow = cb_opIsoLineValuesShow.Checked;
            renderOptions.opIsoLineSelect = cb_opIsoLineSelect.Checked;

            CValue(ref renderOptions.opIsoLineSelectValue, tb_opIsoLineSelectValue);

            float aX =0, aY=0, bX=0, bY=0;
            getCrossLine(ref aX, ref aY, ref bX, ref bY);
            renderOptions.a.X = aX;
            renderOptions.a.Y = aY;
            renderOptions.b.X = bX;
            renderOptions.b.Y = bY;

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
            if (spData.GetPoleMinMax(indexPole, ref MinV, ref MaxV, ref SumV, ref SAreaV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == true)
            {
                tSS_Max.Text = MaxV.ToString("F4");
                tSS_Min.Text = MinV.ToString("F4");
                tSS_Sum.Text = SumV.ToString("F4");
                tSS_Area.Text = SAreaV.ToString("F4");
                tSS_Int.Text = (SumV * SAreaV).ToString("F5");
                // 30 08 2024 шкала для градиентной заливки с контролем пределов
                renderOptions.cb_GradScaleLimit = cb_GradScaleLimit.Checked;
                if (cb_GradScaleLimit.Checked == false)
                {
                    tbMax.Text = MaxV.ToString("F4");
                    tbMin.Text = MinV.ToString("F4");
                }

                CValue(ref renderOptions.MinValue, tbMin);
                CValue(ref renderOptions.MaxValue, tbMax);
            }
            proxyRendererControl.renderOptions = renderOptions;
            proxyRendererControl.colorScheme = colorScheme;
            
        }

        /// <summary>
        /// Чтение координат 
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="bX"></param>
        /// <param name="bY"></param>
        protected void getCrossLine(ref float aX, ref float aY, ref float bX, ref float bY)
        {
            int foc = 0;
            try
            {
                aX = float.Parse(tbAX.Text, MEM.formatter);
                foc = 1;
                aY = float.Parse(tbAY.Text, MEM.formatter);
                foc = 2;
                bX = float.Parse(tbBX.Text, MEM.formatter);
                foc = 3;
                bY = float.Parse(tbBY.Text, MEM.formatter);
            }
            catch (Exception ex)
            {
                switch (foc)
                {
                    case 0: tbAX.Focus(); break;
                    case 1: tbAY.Focus(); break;
                    case 2: tbBX.Focus(); break;
                    case 3: tbBY.Focus(); break;
                }
                MessageBox.Show(ex.Message + " Формат числа неверен, измените его!");
                Logger.Instance.Info("Формат числа неверен, измените его!");
            }
        }

        protected void CValue(ref float V, TextBox tb)
        {
            try
            {
                if (tb.Text == "")
                {
                    tb.Text = "1";
                    Logger.Instance.Info("текстовое поле пусто, измените его!");
                }
                V = float.Parse(tb.Text, MEM.formatter);
            }
            catch (Exception ex)
            {
                tb.Focus();
                MessageBox.Show(ex.Message + " Формат числа неверен, измените его!");
                Logger.Instance.Info("Формат числа неверен, измените его!");
            }
        }
        private void trackBarMinMax_Scroll(object sender, EventArgs e)
        {
            if (trackBarMin.Value > 95)
                trackBarMin.Value = 95;
            if (trackBarMax.Value < 5)
                trackBarMax.Value = 5;
            if (trackBarMin.Value > trackBarMax.Value)
                trackBarMax.Value = trackBarMin.Value + 5;
            else
                if (trackBarMax.Value < trackBarMin.Value)
                trackBarMin.Value = trackBarMax.Value - 5;
        }
        public void SetColorManager()
        {
            SetColorBrush();
            SetColorPen();
            SetColorFont();
            trackBarMax.Value = colorScheme.MaxIsoLine;
            trackBarMin.Value = colorScheme.MinIsoLine;
            nUD_CountIsoLine.Value = (int)colorScheme.CountIsoLine;
            nUD_formatText.Value = (uint)colorScheme.formatText;
            nUD_formatReper.Value = (uint)colorScheme.formatTextReper;
        }
        public void SetRenderOptions()
        {
            cb_showBoudary.Checked = renderOptions.showBoudary;
            cb_showBoudaryKnots.Checked = renderOptions.showBoudaryKnots;
            cb_showElementNamber.Checked = renderOptions.showElementNamber;
            cb_showMesh.Checked = renderOptions.showMesh;
            cb_showKnotNamber.Checked = renderOptions.showKnotNamber;
            cb_showBoudaryElems.Checked = renderOptions.showBoudaryElems;
            cb_opFillValues.Checked = renderOptions.opFillValues;
            cb_opIsoLineValues.Checked = renderOptions.opIsoLineValues;
            cb_opIsoLineValues0.Checked = renderOptions.opIsoLineValues0;
            cb_opValuesKnot.Checked = renderOptions.opValuesKnot;
            cb_opVectorValues.Checked = renderOptions.opVectorValues;
            cb_opGraphicCurve.Checked = renderOptions.opGraphicCurve;
            cb_coordReper.Checked = renderOptions.coordReper;
            // 30 09 2021
            cb_AccountingСurves.Checked = renderOptions.ckAccountingСurves;
            cb_ckScaleUpdate.Checked = renderOptions.ckScaleUpdate;
            // 28 07 2022 Створ
            cb_TargetLine.Checked = renderOptions.opTargetLine;
            tbAX.Text = renderOptions.a.X.ToString("F5");
            tbAY.Text = renderOptions.a.Y.ToString("F5");
            tbBX.Text = renderOptions.b.X.ToString("F5");
            tbBY.Text = renderOptions.b.Y.ToString("F5");
            // 15 06 2024 шкала для градиентной заливки
            cb_GradScale.Checked = renderOptions.opGradScale;
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
        private void button6_Click(object sender, EventArgs e)
        {
            colorScheme.FontReper = GetFont(colorScheme.FontReper, sender);
            textBox1.Font = colorScheme.FontReper;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            colorScheme.FontKnot = GetFont(colorScheme.FontKnot, sender);
            textBox2.Font = colorScheme.FontKnot;
        }
        private void button8_Click(object sender, EventArgs e)
        {
            colorScheme.FontValue = GetFont(colorScheme.FontValue, sender);
            textBox3.Font = colorScheme.FontValue;
        }
        #endregion

        #region Работа с кистями
        private void SetColorBrush()
        {
            button1.BackColor  = colorScheme.BrushPoint.Color;
            button3.BackColor  = colorScheme.BrushBoundaryPoint.Color;
            button4.BackColor  = colorScheme.BrushTextKnot.Color;
            button5.BackColor  = colorScheme.BrushTextValues.Color;
            button13.BackColor = colorScheme.BrushTextReper.Color;
        }
        public SolidBrush GetBraush(SolidBrush b, object sender)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b = new SolidBrush(colorDialog1.Color);
                (sender as Button).BackColor = colorDialog1.Color;
            }
            return b;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            colorScheme.BrushPoint = GetBraush(colorScheme.BrushPoint, sender);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            colorScheme.BrushBoundaryPoint = GetBraush(colorScheme.BrushBoundaryPoint, sender);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextKnot = GetBraush(colorScheme.BrushTextKnot, sender);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextValues = GetBraush(colorScheme.BrushTextValues, sender);
        }
        // 15 07 24 fix
        private void button13_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextReper = GetBraush(colorScheme.BrushTextReper, sender);
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

        bool flagstart = true;
        private void SetColorPen()
        {
            flagstart = false;
            button9.BackColor = colorScheme.PenMeshLine.Color;
            nUD_penMeshLine.Value = (int)colorScheme.PenMeshLine.Width;
            button2.BackColor = colorScheme.PenBoundaryLine.Color;
            nUD_penBoundaryLine.Value = (int)colorScheme.PenBoundaryLine.Width;
            button10.BackColor = colorScheme.PenIsoLine.Color;
            nUD_penIsoLine.Value = (int)colorScheme.PenIsoLine.Width;
            button11.BackColor = colorScheme.PenVectorLine.Color;
            nUD_penVectorLine.Value = (int)colorScheme.PenVectorLine.Width;
            button12.BackColor = colorScheme.PenGraphLine.Color;
            nUD_penGraphLine.Value = (int)colorScheme.PenGraphLine.Width;
            flagstart = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            colorScheme.PenMeshLine = GetPen(colorScheme.PenMeshLine,
                                        (int)nUD_penMeshLine.Value, sender);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            colorScheme.PenBoundaryLine = GetPen(colorScheme.PenBoundaryLine,
                                        (int)nUD_penBoundaryLine.Value, sender);
        }
        private void button10_Click(object sender, EventArgs e)
        {
            colorScheme.PenIsoLine = GetPen(colorScheme.PenIsoLine,
                                        (int)nUD_penIsoLine.Value, sender);
        }
        private void button11_Click(object sender, EventArgs e)
        {
            colorScheme.PenVectorLine = GetPen(colorScheme.PenVectorLine,
                                        (int)nUD_penVectorLine.Value, sender);
        }
        private void button12_Click(object sender, EventArgs e)
        {
            colorScheme.PenGraphLine = GetPen(colorScheme.PenGraphLine,
                                        (int)nUD_penGraphLine.Value, sender);
        }
        #endregion

        private void buttonLoadSP_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sp = new SavePoint();
                sp = (SavePoint)sp.LoadSavePoint(openFileDialog1.FileName);
                if (sp != null)
                {
                    SendSavePoint(sp);
                }
                cb_opGraphicCurve.Checked = false;
            }
        }
        private void buttonSaveSP_Click(object sender, EventArgs e)
        {
            if (sp != null)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // создаем объект BinaryFormatter
                        BinaryFormatter formatter = new BinaryFormatter();
                        // получаем поток, куда будем записывать сериализованный объект
                        using (FileStream fs = new FileStream(saveFileDialog1.FileName, 
                            FileMode.OpenOrCreate))
                        {
                            sp.Name = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                            formatter.Serialize(fs, sp);
                        }
                        Logger.Instance.Info("Сохранение успешно:");
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Info("Сохранение потрачено: " + ex.Message);
                        Logger.Instance.Error(ex.Message, "buttonSaveSP_Click");
                    }
                }
            }
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
                proxyRendererControl.SetData(spData);
                SendOption();
                // отрисовка в статус бар
                tSSL_Time.Text = sp.time.ToString("F4");
                if (sp.mesh != null)
                {
                    tSSL_Nods.Text = sp.mesh.CountKnots.ToString();
                    tss_TaskName.Text = sp.Name;
                    if (owner != null) owner.Text = sp.Name;
                    IRenderMesh tsp = sp.mesh as IRenderMesh;
                    if (tsp != null)
                    {
                        tSSL_Elems.Text = tsp.CountElements.ToString();
                    }
                    tLine = new LocatorTriMeshFacet(sp.mesh);
                }
            }
        }

        private void cb_showMesh_CheckedChanged(object sender, EventArgs e)
        {
            cb_showElementNamber.Enabled = cb_showMesh.Checked;
        }
        #region Работа с кривыми
        /// <summary>
        /// построение графиков 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCurves_Click(object sender, EventArgs e)
        {
            if (spData == null) return;
            BMShow form = new BMShow();
            /// <summary>
            /// Отрисовка графиков 
            /// </summary>
            BMShow bmshow = new BMShow();
            GData gd = new GData();
            for (int i = 0; i < spData.graphicsData.curves.Count; i++)
            {
                if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Checked)
                {
                    GCurve curve = spData.graphicsData.curves[i].GetGCurve();
                    gd.Add(curve);
                }
            }
            form.Text = tss_TaskName.Text;
            form.SetData(gd);
            form.Show();
        }
        bool Look = true;
        private void checkedListBoxCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool flag = false;
            for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
            {
                if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Checked)
                {
                    flag = true;
                    break;
                }
            }
            Look = false;
            cb_opGraphicCurve.Checked = flag;
            Look = true;
        }
        /// <summary>
        /// установка отмеченных кривых в списке кривых
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_opGraphicCurve_CheckedChanged(object sender, EventArgs e)
        {
            if (Look == true)
                for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
                    checkedListBoxCurve.SetItemChecked(i, cb_opGraphicCurve.Checked);
        }
        #endregion


        private void button14_Click(object sender, EventArgs e)
        {
            if (sp != null)
            {
                FVCurves form = new FVCurves(sp);
                form.Show();
            }
        }
        public void SetSavePointCurves(ISavePoint isp)
        {
            sps = isp; 
        }
        private void btCurves_Click(object sender, EventArgs e)
        {
            if (sps != null)
            {
                FVCurves form = new FVCurves(sps);
                form.Show();
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            PointF[] Points = proxyRendererControl.Points;
            tbAX.Text = Points[0].X.ToString("F5");
            tbAY.Text = Points[0].Y.ToString("F5");
            tbBX.Text = Points[1].X.ToString("F5");
            tbBY.Text = Points[1].Y.ToString("F5");
            SendOption();
        }

        private void btSaveTargetLine_Click(object sender, EventArgs e)
        {
            if (ListCross.Count > 0)
            {
                string path = "..\\..\\Result";
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (dirInfo.Exists == false)
                    dirInfo.Create();
                string FileName = path + "\\" + "bufferCrossLines.cld";
                if (cb_Save.Checked == true)
                {
                    string buf = saveFileDialog1.Filter;
                    string dir = saveFileDialog1.InitialDirectory;
                    saveFileDialog1.Filter = "файл - линии створов (*.cld)|*.cld|" +
                         "All files (*.*)|*.*";
                    saveFileDialog1.InitialDirectory = path;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        FileName = saveFileDialog1.FileName;
                    saveFileDialog1.Filter = buf;
                    saveFileDialog1.InitialDirectory = dir;
                }
                using (StreamWriter file = new StreamWriter(FileName))
                {
                    foreach (CrossLine cl in ListCross)
                        file.WriteLine(cl.ToString());
                    file.Close();
                }
            }
        }
        private void btLoadTargetLine_Click(object sender, EventArgs e)
        {
            try
            {
                string path = "..\\..\\Result";
                string FileName = path + "\\" + "bufferCrossLines.cld";
                if (sb_Load.Checked == true)
                {
                    string buf = openFileDialog1.Filter;
                    string dir = openFileDialog1.InitialDirectory;
                    openFileDialog1.Filter = "файл - сохранения линии створа(*.cld)|*.cld|" + "(*.ccl) | *.ccl|" +
                         "All files (*.*)|*.*";
                    openFileDialog1.InitialDirectory = path;
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        FileName = openFileDialog1.FileName;
                    openFileDialog1.Filter = buf;
                    openFileDialog1.InitialDirectory = dir;
                }

                string FileEXT = Path.GetExtension(FileName).ToLower();
                if (File.Exists(FileName) == true)
                {
                    ListCross.Clear();
                    cbCrossList.Items.Clear();
                    int idx = 0;
                    using (StreamReader file = new StreamReader(FileName))
                    {
                        for (string line = file.ReadLine(); line != null; line = file.ReadLine())
                        {
                            switch (FileEXT)
                            {
                                case ".cld":
                                    {
                                        CrossLine cl = CrossLine.Parse(line);
                                        ListCross.Add(cl);
                                        cbCrossList.Items.Add(cl.Name);
                                    }
                                    break;
                                case ".ccl":
                                    {
                                        string Name = "Створ" + idx.ToString();
                                        CloudKnotLine cine = CloudKnotLine.Parse(line);
                                        CrossLine cl = new CrossLine(Name, cine);
                                        ListCross.Add(cl);
                                        cbCrossList.Items.Add(cl.Name);
                                    }
                                    break;
                            }
                        }
                        cbCrossList.SelectedIndex = 0;
                        file.Close();
                    }
                }
                else
                    Logger.Instance.Info("Буффер файл створа - отсутствует");
            }
            catch(Exception ex)
            {
                Logger.Instance.Info("Буффер файл створа - отсутствует : " + ex.Message);
            }
        
        }


        private void listBoxPoles_SelectedIndexChanged(object sender, EventArgs e) 
        {
            SendOption();
        }

        private void btCreateCrossTask_Click(object sender, EventArgs e)
        {
            FTaskCross form = new FTaskCross();
            form.Show();
        }
        private void btSave_Click(object sender, EventArgs e)
        {
            string CrossName = "Створ " + (ListCross.Count + 1).ToString();
            cbCrossList.Items.Add(CrossName);
            float aX = 0, aY = 0, bX = 0, bY = 0;
            getCrossLine(ref aX, ref aY, ref bX, ref bY);
            CrossLine current = new CrossLine(CrossName, aX, aY, bX, bY);
            //if (ListCross.Contains(current) == false)
            {
                ListCross.Add(current);
                cbCrossList.SelectedIndex = ListCross.Count - 1;
            }
        }

        private void cbCrossList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbCrossList.SelectedIndex != -1)
            {
                CrossLine current = ListCross[cbCrossList.SelectedIndex];
                tbAX.Text = current.A.X.ToString("F5");
                tbAY.Text = current.A.Y.ToString("F5");
                tbBX.Text = current.B.X.ToString("F5");
                tbBY.Text = current.B.Y.ToString("F5");
                SendOption();
            }
        }

        private void bt_DelOne_Click(object sender, EventArgs e)
        {
            if (ListCross.Count == 1)
            {
                bt_DelAll_Click(sender, e);
                return;
            }
            else
            {
                if (cbCrossList.SelectedIndex != -1)
                {
                    int idx = cbCrossList.SelectedIndex;
                    CrossLine current = ListCross[idx];
                    ListCross.Remove(current);
                    cbCrossList.Items.RemoveAt(idx);

                    if (idx == 0)
                        cbCrossList.SelectedIndex = idx;
                    if (idx > 0)
                        cbCrossList.SelectedIndex = idx - 1;

                    cbCrossList_SelectedIndexChanged(sender, e);
                }
            }
        }

        private void bt_DelAll_Click(object sender, EventArgs e)
        {
            ListCross.Clear();
            cbCrossList.SelectedIndex = -1;
            cbCrossList.Items.Clear();
            tbAX.Text = "0";
            tbAY.Text = "0";
            tbBX.Text = "0";
            tbBY.Text = "0";
            SendOption();
        }
        private void nUD_penMeshLine_ValueChanged(object sender, EventArgs e)
        {
            if(flagstart == true)
                colorScheme.PenMeshLine =  
                        new Pen(colorScheme.PenMeshLine.Color, 
                        (int)nUD_penMeshLine.Value);
        }

        private void nUD_penBoundaryLine_ValueChanged(object sender, EventArgs e)
        {
            if (flagstart == true)
                colorScheme.PenBoundaryLine =
                new Pen(colorScheme.PenBoundaryLine.Color,
                        (int)nUD_penBoundaryLine.Value);
        }

        private void nUD_penIsoLine_ValueChanged(object sender, EventArgs e)
        {
            if (flagstart == true)
                colorScheme.PenIsoLine =
                new Pen(colorScheme.PenIsoLine.Color,
                        (int)nUD_penIsoLine.Value);
        }

        private void nUD_penVectorLine_ValueChanged(object sender, EventArgs e)
        {
            if (flagstart == true)
                colorScheme.PenIsoLine =
                new Pen(colorScheme.PenIsoLine.Color,
                        (int)nUD_penVectorLine.Value);
        }

        private void nUD_penGraphLine_ValueChanged(object sender, EventArgs e)
        {
            if (flagstart == true)
                colorScheme.PenGraphLine =
                  new Pen(colorScheme.PenGraphLine.Color,
                        (int)nUD_penGraphLine.Value);
        }

        private void button14_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string ext = ".bed"; 
            string ext1 = ".node";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += "(*" + ext1 + ")|*" + ext1 + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            if (sp.mesh != null)
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sp.ImportSPMesh(sfd.FileName);
                }
            }
            else
            {
                
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string ext = ".bed";
            string ext1 = ".node";
            string filter = "(*" + ext + ")|*" + ext + "| ";
            filter += "(*" + ext1 + ")|*" + ext1 + "| ";
            filter += " All files (*.*)|*.*";
            sfd.Filter = filter;
            if (sp.mesh != null)
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    sp.ImportSPMesh(sfd.FileName);
                }
            }
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sp = new SavePoint();
                sp = (SavePoint)sp.LoadSavePoint(openFileDialog1.FileName);
                if (sp != null)
                {
                    SendSavePoint(sp);
                }
                cb_opGraphicCurve.Checked = false;
            }

            string sf = openFileDialog1.Filter;
            try 
            {
                string ext = ".mesh";
                string ext1 = ".node";
                string ext2 = ".cdg";
                string filter = "(*" + ext + ")|*" + ext + "| ";
                filter += "(*" + ext1 + ")|*" + ext1 + "| ";
                filter += "(*" + ext2 + ")|*" + ext2 + "| ";
                filter += " All files (*.*)|*.*";
                openFileDialog1.Filter = filter;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                   sp.ImportSPMesh(openFileDialog1.FileName);
                }
            } 
            catch (Exception ex) 
            { 
                    
            }
            openFileDialog1.Filter = sf;

            
            
            
        }
        /// <summary>
        /// Выбор состояния обработки мышки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbc_Task_SelectedIndexChanged(object sender, EventArgs e)
        {
            proxyRendererControl.IndexTask = tbc_Task.SelectedIndex;
        }

        private void cb_GradScaleLimit_CheckedChanged(object sender, EventArgs e)
        {
            if(cb_GradScaleLimit.Checked == true)
            {
                tbMin.Enabled = true;
                tbMax.Enabled = true;
                trackBarMin.Enabled = false;
                trackBarMax.Enabled = false;
            }
            else
            {
                tbMin.Enabled = false;
                tbMax.Enabled = false;
                trackBarMin.Enabled = true;
                trackBarMax.Enabled = true;
            }
        }

        private void btShowTargetLine_Click(object sender, EventArgs e)
        {
            ISavePoint spLine = new SavePoint(sp.Name);
            IRenderMesh mesh = sp.mesh;
            string CName = "";
            IHPoint[] Points;
            int Count = ListCross.Count == 0 ? 1 : ListCross.Count;
            for (int idx = 0; idx < Count; idx++)
            {
                if (ListCross.Count == 0)
                {
                    PointF[] P = proxyRendererControl.Points;
                    IHPoint[] tPoints = { new HPoint(P[0].X, P[0].Y), new HPoint(P[1].X, P[1].Y) };
                    Points = tPoints;
                }
                else
                {
                    CrossLine current = ListCross[idx];
                    //IHPoint[] tPoints = { new HPoint(current.A.X, current.A.Y),
                    //                      new HPoint(current.B.X, current.B.Y) };
                    IHPoint[] tPoints = { new HPoint(current.A), new HPoint(current.B) };
                    Points = tPoints;
                    CName = "Ств" + idx.ToString() + ": ";
                }
                //LocatorTriMesh tLine = new LocatorTriMesh(mesh, Points);
                tLine.SetCrossLine(Points);
                //cbTauNormals
                List<string> Names = spData.PoleNames();

                for (uint indexPole = 0; indexPole < Names.Count; indexPole++)
                {
                    if (cb_AllFields.Checked == false)
                        if (listBoxPoles.SelectedIndex != indexPole)
                            continue;
                    IField pole = spData.GetPole(indexPole);
                    if (pole.Dimention == 1)
                    {
                        double[] s = null;
                        double[] V = null;
                        double[] mVx = ((Field1D)pole).Values;
                        tLine.GetCurve(mVx, ref s, ref V);
                        if (s != null && V != null)
                        {
                            spLine.AddCurve(CName + Names[(int)indexPole], s, V);
                            Console.WriteLine(CName + Names[(int)indexPole]);
                        }
                        else
                            Logger.Instance.Error("Створ не определен", "btShowTargetLine_Click");
                    }
                    else
                    {
                        double[] s = null;
                        double[] V = null;
                        double[] Vx = null;
                        double[] Vy = null;
                        double[] Vn = null;
                        double[] Vt = null;
                        double[] mVx = null, mVy = null;
                        Vector2[] val = ((Field2D)pole).Values;
                        MEM.Alloc(val.Length, ref mVx);
                        MEM.Alloc(val.Length, ref mVy);
                        for (int i = 0; i < val.Length; i++)
                        {
                            mVx[i] = val[i].X;
                            mVy[i] = val[i].Y;
                        }
                        tLine.GetCurve(mVx, mVy, ref s, ref Vx, ref Vy, ref Vn, ref Vt);
                        MEM.Alloc(s.Length, ref V);
                        for (int i = 0; i < V.Length; i++)
                            V[i] = Math.Sqrt(Vx[i] * Vx[i] + Vy[i] * Vy[i]);
                        if (s != null && Vx != null)
                            spLine.AddCurve(CName + Names[(int)indexPole], s, V);
                        if (s != null && Vn != null)
                            spLine.AddCurve(CName + Names[(int)indexPole] + "_n", s, Vn);
                        if (s != null && Vt != null)
                            spLine.AddCurve(CName + Names[(int)indexPole] + "_t", s, Vt);

                    }
                }
            }
            FVCurves form = new FVCurves(spLine);
            form.Show();
        }
        private void btGraph_Click(object sender, EventArgs e)
        {
            if (sp != null)
            {
                ISavePoint spv = new SavePoint();
                GraphicsData gdata =(GraphicsData)sp.graphicsData;
                for (int i = 0; i < spData.graphicsData.curves.Count; i++)
                {
                    if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Checked)
                    {
                        GraphicsCurve c = gdata.GetCurve((uint)i);
                        spv.AddCurve(c);
                    }
                }
                FVCurves form = new FVCurves(spv);
                form.Show();
            }
        }
        private void btEvalGraph_Click(object sender, EventArgs e)
        {
            //if (sps != null)
            //{
            //    FVCurves form = new FVCurves(sps);
            //    form.Show();
            //}
            if (sps != null)
            {
                ISavePoint spv = new SavePoint();
                GraphicsData gdata = (GraphicsData)sps.graphicsData;
                for (int idx = 0; idx < gdata.Count; idx++)
                {
                    int i = idx % checkedListBoxCurve.Items.Count;
                    {
                        if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Checked)
                        {
                            GraphicsCurve c = gdata.GetCurve((uint)idx);
                            spv.AddCurve(c);
                        }
                    }
                }
                FVCurves form = new FVCurves(spv);
                form.Show();
            }
        }

        private void btCrossSection_Click(object sender, EventArgs e)
        {

        }
    }
}
