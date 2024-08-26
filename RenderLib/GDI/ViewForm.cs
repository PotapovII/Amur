//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 18.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using MeshLib;
    public partial class ViewForm : Form
    {
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

        /// <param name="spData">Данные для отрисовки</param>
        public ViewForm(SavePointData spData = null)
        {
            InitializeComponent();
            // openFileDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            openFileDialog1.Filter = "файл - русловой процесс rpsp (*.rpsp)|*.rpsp|" +
                                     "All files (*.*)|*.*";

            renderOptions = new RenderOptionsFields();
            colorScheme = new ColorSchemeFields();
            SetData(spData);
        }
        /// <param name="spData">Данные для отрисовки</param>
        public void SetData(SavePointData spData)
        {
            this.spData = spData;
            List<string> Names = spData.PoleNames();
            listBoxPoles.Items.Clear();
            if (Names.Count > 0)
            {
                foreach (var name in Names)
                    listBoxPoles.Items.Add(name);
                listBoxPoles.SelectedIndex = 0;
            }
            else
                listBoxPoles.SelectedIndex = -1;
        }
        /// <summary>
        /// Настройка менеджера отрисовки данных и привязка его к панели
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewForm_Load(object sender, EventArgs e)
        {
            proxyRendererControl = new ProxyRendererControlFields();
            var control = proxyRendererControl.RenderControl;

            if (control != null)
            {
                // привязка менеджера отрисовки к панели
                this.panel1.Controls.Add(control);
                control.BackColor = Color.White;
                control.Dock = DockStyle.Fill;
                control.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                control.Location = new System.Drawing.Point(0, 0);
                control.Name = "renderControl1";
                control.Size = new Size(703, 612);
                control.TabIndex = 0;
                control.Text = "";
                control.Visible = true;
                proxyRendererControl.Initialize();
            }
            else
            {
                Text = "Eeee ..., не удалось инициализировать средство визуализации.";
            }
            proxyRendererControl.SetData(spData);
            SetRenderOptions();
            SetColorManager();
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
        bool isResizing = false;
        Size oldClientSize;
        /// <summary>
        /// Свернуть окно и развернуть
        /// </summary>
        private void ResizeHandler(object sender, EventArgs e)
        {
            if (!isResizing)
            {
                proxyRendererControl.HandleResize();
            }
        }
        /// <summary>
        /// Запоминаем окончание процесса изменения размера окна
        /// </summary>
        private void ResizeEndHandler(object sender, EventArgs e)
        {
            isResizing = false;
            if (this.ClientSize != this.oldClientSize)
            {
                this.oldClientSize = this.ClientSize;
                proxyRendererControl.HandleResize();
            }
        }
        /// <summary>
        /// Запоминаем начало процесса изменения размера окна
        /// </summary>
        private void ResizeBeginHandler(object sender, EventArgs e)
        {
            isResizing = true;
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
            colorScheme.MaxIsoLine = trackBarMax.Value;
            colorScheme.MinIsoLine = trackBarMin.Value;
            colorScheme.CountIsoLine = (int)nUD_CountIsoLine.Value;
            colorScheme.formatText = (uint)nUD_formatText.Value;

            renderOptions.indexValues = listBoxPoles.SelectedIndex;
            renderOptions.showBoudary = cb_showBoudary.Checked;
            renderOptions.showElementNamber = cb_showElementNamber.Checked;
            renderOptions.showMesh = cb_showMesh.Checked;
            renderOptions.showKnotNamber = cb_showKnotNamber.Checked;
            renderOptions.opFillValues = cb_opFillValues.Checked;
            renderOptions.opIsoLineValues = cb_opIsoLineValues.Checked;
            renderOptions.opValuesKnot = cb_opValuesKnot.Checked;
            renderOptions.opVectorValues = cb_opVectorValues.Checked;
            renderOptions.opGraphicCurve = cb_opGraphicCurve.Checked;
            renderOptions.coordReper = cb_coordReper.Checked;

            proxyRendererControl.colorScheme = colorScheme;
            proxyRendererControl.renderOptions = renderOptions;
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
        }
        public void SetRenderOptions()
        {
            cb_showBoudary.Checked = renderOptions.showBoudary;
            cb_showElementNamber.Checked = renderOptions.showElementNamber;
            cb_showMesh.Checked = renderOptions.showMesh;
            cb_showKnotNamber.Checked = renderOptions.showKnotNamber;
            cb_opFillValues.Checked = renderOptions.opFillValues;
            cb_opIsoLineValues.Checked = renderOptions.opIsoLineValues;
            cb_opValuesKnot.Checked = renderOptions.opValuesKnot;
            cb_opVectorValues.Checked = renderOptions.opVectorValues;
            cb_opGraphicCurve.Checked = renderOptions.opGraphicCurve;
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
            button1.BackColor = colorScheme.BrushPoint.Color;
            button3.BackColor = colorScheme.BrushBoundaryPoint.Color;
            button4.BackColor = colorScheme.BrushTextKnot.Color;
            button5.BackColor = colorScheme.BrushTextValues.Color;
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


        #endregion

        #region Работа с перьями
        public Pen GetPen(Pen b, int d, object sender)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b = new Pen(colorDialog1.Color, d);
                (sender as Button).BackColor = colorDialog1.Color;
            }
            return b;
        }
        private void SetColorPen()
        {
            button9.BackColor = colorScheme.PenMeshLine.Color;
            nUD_penMeshLine.Value = (int)colorScheme.PenMeshLine.Width;
            button2.BackColor = colorScheme.PenBoundaryLine.Color;
            nUD_penBoundaryLine.Value = (int)colorScheme.PenBoundaryLine.Width;
            button10.BackColor = colorScheme.PenIsoLine.Color;
            nUD_penBoundaryLine.Value = (int)colorScheme.PenIsoLine.Width;
            button11.BackColor = colorScheme.PenVectorLine.Color;
            nUD_penVectorLine.Value = (int)colorScheme.PenVectorLine.Width;
            button12.BackColor = colorScheme.PenGraphLine.Color;
            nUD_penGraphLine.Value = (int)colorScheme.PenGraphLine.Width;
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
                SavePoint sp = new SavePoint();
                sp = (SavePoint)sp.LoadSavePoint(openFileDialog1.FileName);
                if (sp != null)
                    SendSavePoint(sp);
            }
        }
        /// <summary>
        /// Установка данных
        /// </summary>
        /// <param name="sp"></param>
        public void SendSavePoint(SavePoint sp)
        {
            SavePointData spData = new SavePointData();
            spData.SetSavePoint(sp);
            SetData(spData);
            proxyRendererControl.SetData(spData);
            SendOption();
        }

    }
}
