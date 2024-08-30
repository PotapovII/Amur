//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 18.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using GeometryLib;
    using MeshLib;
    using MemLogLib;
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using MeshLib.SaveData;

    /// <summary>
    ///ОО: Компонент визуализации данных (сетки, полей, кривых) 
    /// </summary>
    public partial class GDI_ControlClouds : UserControl
    {
        /// <summary>
        /// Точки фильтра
        /// </summary>
        public PointF[] FilterPoints { get => proxyRendererControl.Points; set => proxyRendererControl.Points = value; }


        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        public List<CloudKnot> Conturs
        {
            get => proxyRendererControl.Conturs;
            set => proxyRendererControl.Conturs = value;
        }
        /// <summary>
        /// Менеджер отрисовки данных
        /// </summary>
        ProxyRendererControlClouds proxyRendererControl;
        /// <summary>
        /// Данные для отрисовки
        /// </summary>
        IClouds clouds;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        RenderOptionsFields renderOptions;
        /// <summary>
        /// Настройка цветов для объектов рендеренга
        /// </summary>
        ColorSchemeClouds colorScheme;
        ///// <summary>
        ///// Эволюционная точка сохранения 
        ///// </summary>
        //ISaveCloud sps = null;
        public GDI_ControlClouds()
        {
            InitializeComponent();
            saveFileDialog1.Filter = "Облако данных (*.node)|*.node|" +
                                     "All files (*.*)|*.*";
            renderOptions = new RenderOptionsFields();
            colorScheme = new ColorSchemeClouds();
            proxyRendererControl = new ProxyRendererControlClouds();
            proxyRendererControl.sendPintData = this.SetCloudKnot;
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
        }


        /// <summary>
        /// передача данных 
        /// </summary>
        /// <param name="point"></param>
        protected void SetCloudKnot(CloudKnot point)
        {
            textBoxX.Text = point.x.ToString();
            textBoxY.Text = point.y.ToString();
            tb_H.Text = point.Attributes[0].ToString();
            tb_T.Text = point.Attributes[1].ToString();
            tb_V.Text = point.Attributes[2].ToString();
            tb_C.Text = point.Attributes[3].ToString();
        }

        private void btUpDate_Click(object sender, EventArgs e)
        {
            int mark = 1;
            CloudKnot point = new CloudKnot(0, 0, new double[] { 0, 0, 0, 0 }, mark);
            point.x = LOG.Double(textBoxX.Text);
            point.y = LOG.Double(textBoxY.Text);
            point.Attributes[0] = LOG.Double(tb_H.Text);
            point.Attributes[1] = LOG.Double(tb_T.Text);
            point.Attributes[2] = LOG.Double(tb_V.Text);
            point.Attributes[3] = LOG.Double(tb_C.Text);
            proxyRendererControl.UpCloudKnot(point);
        }

        private void btDelNodes_Click(object sender, EventArgs e)
        {
            proxyRendererControl.DelCloudKnot();
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
            colorScheme.formatTextValues = (uint)nUD_formatText.Value;

            renderOptions.showFilter = tabControlState.SelectedIndex;
            renderOptions.indexValues = listBoxPoles.SelectedIndex;
            renderOptions.showBoudaryKnots = cb_showBoudaryKnots.Checked;
            renderOptions.opValuesKnot = cb_opValuesKnot.Checked;
            renderOptions.opVectorValues = cb_opVectorValues.Checked;
            renderOptions.coordReper = cb_coordReper.Checked;
            renderOptions.opTargetLine = cb_TargetLine.Checked;
            renderOptions.a.X = float.Parse(tbAX.Text, MEM.formatter); 
            renderOptions.a.Y = float.Parse(tbAY.Text, MEM.formatter);
            renderOptions.b.X = float.Parse(tbBX.Text, MEM.formatter);
            renderOptions.b.Y = (float) double.Parse(tbBY.Text, MEM.formatter);

            proxyRendererControl.colorScheme = colorScheme;
            proxyRendererControl.renderOptions = renderOptions;

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
            nUD_formatText.Value = (uint)colorScheme.formatTextValues;
        }
        public void SetRenderOptions()
        {
            cb_showBoudaryKnots.Checked = renderOptions.showBoudaryKnots;
            cb_opValuesKnot.Checked = renderOptions.opValuesKnot;
            cb_opVectorValues.Checked = renderOptions.opVectorValues;
            cb_coordReper.Checked = renderOptions.coordReper;
            cb_TargetLine.Checked = renderOptions.opTargetLine;
            tbAX.Text = renderOptions.a.X.ToString("F5");
            tbAY.Text = renderOptions.a.Y.ToString("F5");
            tbBX.Text = renderOptions.b.X.ToString("F5");
            tbBY.Text = renderOptions.b.Y.ToString("F5");
        }

        #region Работа со шрифтами
        private void SetColorFont()
        {
            button6.Font = colorScheme.FontReper;
            button7.Font = colorScheme.FontNodesCloud;
            button8.Font = colorScheme.FontNodesCloud;
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
            colorScheme.FontNodesCloud = GetFont(colorScheme.FontNodesCloud, sender);
            textBox2.Font = colorScheme.FontNodesCloud;
        }
        private void button8_Click(object sender, EventArgs e)
        {
            colorScheme.FontNodesCloud = GetFont(colorScheme.FontNodesCloud, sender);
            textBox3.Font = colorScheme.FontNodesCloud;
        }
        #endregion

        #region Работа с кистями
        private void SetColorBrush()
        {
            button1.BackColor = colorScheme.BrushNodes.Color;
            button4.BackColor = colorScheme.BrushTextNodes.Color;
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
            colorScheme.BrushNodes = GetBraush(colorScheme.BrushNodes, sender);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextNodes = GetBraush(colorScheme.BrushTextNodes, sender);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextValues = GetBraush(colorScheme.BrushTextValues, sender);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextValues = GetBraush(colorScheme.BrushTextValues, sender);
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
                proxyRendererControl.SetData(clouds);
                SendOption();
                // отрисовка в статус бар
                tSSL_Nods.Text = clouds.CountKnots.ToString();
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            PointF[] Points = proxyRendererControl.Points;
            tbAX.Text = Points[0].X.ToString("F5");
            tbAY.Text = Points[0].Y.ToString("F5");
            tbBX.Text = Points[1].X.ToString("F5");
            tbBY.Text = Points[1].Y.ToString("F5");
        }

        private void btSaveTargetLine_Click(object sender, EventArgs e)
        {
            using (StreamWriter file = new StreamWriter("bufferCrossLines.bcl"))
            {
                file.WriteLine(tbAX.Text);
                file.WriteLine(tbAY.Text);
                file.WriteLine(tbBX.Text);
                file.WriteLine(tbBY.Text);
                file.Close();
            }
        }

        private void btLoadTargetLine_Click(object sender, EventArgs e)
        {
            string fileName = "bufferCrossLines.bcl";
            if ( File.Exists(fileName) == true )
            {
                using (StreamReader file = new StreamReader(fileName))
                {
                    tbAX.Text = file.ReadLine();
                    tbAY.Text = file.ReadLine();
                    tbBX.Text = file.ReadLine();
                    tbBY.Text = file.ReadLine();
                    file.Close();
                }
            }
            else
                Logger.Instance.Info("Буффер файл створа - отсутствует");
        }

        private void buttonSetReg_Click(object sender, EventArgs e)
        {
            try
            {
                PointF a = new PointF(float.Parse(textBoxXmin.Text, LOG.formatter),
                                      float.Parse(textBoxYmin.Text, LOG.formatter));
                PointF b = new PointF(float.Parse(textBoxXmax.Text, LOG.formatter),
                                      float.Parse(textBoxYmax.Text, LOG.formatter));
                proxyRendererControl.SetRegion(a, b);
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
            }
        }

        private void tabControlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendOption();
        }

        private void tabPage5_Click(object sender, EventArgs e)
        {

        }

        private void tabControlOption_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
