//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using MeshLib;
    using CommonLib.IO;
    using GeometryLib;
    using System.IO;
    using System.Xml.Linq;

    public partial class GDI_Curves_Control : UserControl
    {
        /// <summary>
        /// Менеджер отрисовки данных
        /// </summary>
        ProxyRendererControlCurves proxyRendererControl;
        /// <summary>
        /// Данные для отрисовки
        /// </summary>
        GraphicsData graphicsData;
        /// <summary>
        /// Настройка объектов рендеренга
        /// </summary>
        RenderOptionsCurves renderOptions;
        /// <summary>
        /// Настройка цветов для объектов рендеренга
        /// </summary>
        ColorScheme colorScheme;
        ColorScheme colorSchemeOld = null;
        /// <summary>
        /// Данные для обработки
        /// </summary>
        ISavePoint isp = null;
        /// <summary>
        /// тип кривых
        /// </summary>
        TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AllCurve;
        /// <summary>
        /// Фильтр групп
        /// </summary>
        List<string> GNames = new List<string>();
        /// <summary>
        /// Фильтр групп
        /// </summary>
        List<string> CurGNames = new List<string>();
        public GDI_Curves_Control()
        {
            InitializeComponent();
            cbRefreshCurves.SelectedIndex = 0;
            cbMeasure.SelectedIndex = 0;
            tbScaleX.Enabled = false;
            tbScaleY.Enabled = false;
            openFileDialog1.Filter = "файл - русловой процесс rpsp (*.sp)|*.sp|" +
                                     "файл - русловой процесс rpsp (*.rpsp)|*.rpsp|" +
                                     "All files (*.*)|*.*";

            saveFileDialog1.Title = "Сохранение графика функции";

            renderOptions = new RenderOptionsCurves();
            colorScheme = new ColorScheme();
            proxyRendererControl = new ProxyRendererControlCurves();
            var control = proxyRendererControl.RenderControl;
            if (control != null)
            {
                // привязка менеджера отрисовки к панели
                this.panel1.Controls.Add(control);
                control.BackColor = Color.White;
                control.Dock = DockStyle.Fill;
                control.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                control.Location = new Point(0, 0);
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
        public void SetColorManager()
        {
            SetColorBrush();
            SetColorPen();
            SetColorFont();
        }
        /// <summary>
        /// Настройка интерфейса по умолчанию
        /// </summary>
        public void SetRenderOptions()
        {
            cb_coordReper.Checked = renderOptions.coordReper;
            cb_coordInv.Checked = renderOptions.coordInv;
            cb_PrintOp.Checked = renderOptions.printOp;
            cbAutoScaleX.Checked = renderOptions.opAutoScaleX;
            cbAutoScaleY.Checked = renderOptions.opAutoScaleY;
            cb_AutoColorCurves.Checked = renderOptions.opAutoColorCurves;
            cb_showKnotNamber.Checked = renderOptions.showKnotNamber;
            cb_opValuesKnot.Checked = renderOptions.opValuesKnot;
            cb_showMesh.Checked = renderOptions.showMesh;
            SetRB(renderOptions.opCurveScale);
        }
        /// <summary>
        /// Определение radioButton ... по renderOptions.opCurveScale
        /// </summary>
        public void SetRB(int value)
        {
            switch (value)
            {
                case 0:
                    radioButton01.Checked = true;
                    break;
                case 1:
                    radioButton02.Checked = true;
                    break;
                case 2:
                    radioButton03.Checked = true;
                    break;
                case 3:
                    radioButton04.Checked = true;
                    break;
            }
        }
        /// <summary>
        /// Определение renderOptions.opCurveScale
        /// </summary>
        public void SetRB()
        {
            if (radioButton01.Checked == true)
                renderOptions.opCurveScale = 0;
            if (radioButton02.Checked == true)
                renderOptions.opCurveScale = 1;
            if (radioButton03.Checked == true)
                renderOptions.opCurveScale = 2;
            if (radioButton04.Checked == true)
                renderOptions.opCurveScale = 3;
        }

        private void radioButton02_CheckedChanged(object sender, EventArgs e)
        {
            SetRB();
            tbScaleX.Enabled = radioButton04.Checked;
            tbScaleY.Enabled = radioButton04.Checked;
        }

        private void btShow_Click(object sender, EventArgs e)
        {
            SendOption();
            ResizeHandler(sender, e);
        }
        protected void RefListNames()
        {
            bool flagChangeGroup = false;
            for (int i = 0; i < checkedListBoxGroup.Items.Count; i++)
                if (checkedListBoxGroup.GetItemCheckState(i) == CheckState.Checked)
                {
                    if (CurGNames.Contains(GNames[i]) == false)
                    {
                        CurGNames.Add(GNames[i]);
                        flagChangeGroup = true;
                    }
                }
                else
                {
                    if (CurGNames.Contains(GNames[i]) == true)
                    {
                        CurGNames.Remove(GNames[i]);
                        flagChangeGroup = true;
                    }
                }
            if (flagChangeGroup == true)
            {
                // при изменении списка групп новая загрузка кривых
                GraphicsData gd = isp.graphicsData.GetSubIGraphicsData(tGraphicsCurve, CurGNames) as GraphicsData;
                if (gd != null)
                {
                    // Запись данных в списки компонента
                    List<string> Names = graphicsData.GraphicNames();
                    checkedListBoxCurve.Items.Clear();
                    if (Names.Count > 0)
                    {
                        // Записать
                        foreach (var name in Names)
                        {
                            string cname = gd.GroupNameFilter(name);
                            if (CurGNames.Contains(cname) == true)
                                checkedListBoxCurve.Items.Add(name);
                        }
                        if (checkedListBoxCurve.Items.Count > 0)
                            checkedListBoxCurve.SelectedIndex = 0;

                        cb_opGraphicCurve.Checked = true;
                        for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
                            checkedListBoxCurve.SetItemChecked(i, cb_opGraphicCurve.Checked);
                    }
                    else
                        checkedListBoxCurve.SelectedIndex = -1;
                    tSSL_Time.Text = isp.time.ToString("F4");
                    tSSL_Curves.Text = gd.curves.Count.ToString();
                }
            }
            for (int i = 0; i < graphicsData.curves.Count; i++)
                graphicsData.curves[i].Check = false;

            for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
            {
                if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Checked)
                {
                    string cname = checkedListBoxCurve.Items[i].ToString();
                    for (int k = 0; k < graphicsData.curves.Count; k++)
                    {
                        if (cname == graphicsData.curves[k].Name)
                        {
                            graphicsData.curves[k].Check = true;
                            break;
                        }
                    }
                }
            }
        }
        public void SendOption()
        {
            RefListNames();

            colorScheme.formatText = (uint)nUD_formatText.Value;
            colorScheme.formatTextReper = (uint)nUD_formatReper.Value;

            renderOptions.opAutoColorCurves = cb_AutoColorCurves.Checked;
            renderOptions.indexValues = checkedListBoxCurve.SelectedIndex;
            renderOptions.coordReper = cb_coordReper.Checked;
            renderOptions.coordInv = cb_coordInv.Checked;
            renderOptions.printOp = cb_PrintOp.Checked;

            renderOptions.opAutoScaleX = cbAutoScaleX.Checked;
            renderOptions.opAutoScaleY = cbAutoScaleY.Checked;

            renderOptions.showMesh = cb_showMesh.Checked;
            renderOptions.showKnotNamber = cb_showKnotNamber.Checked;
            renderOptions.opValuesKnot = cb_opValuesKnot.Checked;
            try
            {
                if (renderOptions.printOp == true)
                {
                    if (colorSchemeOld == null)
                        colorSchemeOld = new ColorScheme(colorScheme);
                    colorScheme.BrushTextReper = new SolidBrush(Color.Black);
                    colorScheme.BrushReper = new SolidBrush(Color.Black);
                    colorScheme.PenReper = new Pen(Color.Black, 3);
                    colorScheme.FontReper = new Font("Times New Roman", 28);
                    colorScheme.PenGraphLine = new Pen(Color.Black, 2);
                    //colorScheme.formatTextReper = 2;
                    
                    btBrushCurve.BackColor = colorScheme.BrushTextReper.Color;
                    btBrushCurve.ForeColor = ExtensionsColor.GetContrast(colorScheme.BrushTextReper.Color, true);
                }
                else
                {
                    if (colorSchemeOld != null)
                    {
                        colorScheme.BrushTextReper = new SolidBrush(Color.Blue);
                        colorScheme.BrushReper = new SolidBrush(Color.Green);
                        colorScheme.PenReper = new Pen(Color.Blue, 1);
                        colorScheme.FontReper = new Font("Arial", 14);
                        colorScheme.PenGraphLine = new Pen(Color.Black); 

                        //colorScheme.formatTextReper = colorSchemeOld.formatTextReper;
                        btBrushCurve.BackColor = colorScheme.BrushTextReper.Color;
                        btBrushCurve.ForeColor = ExtensionsColor.GetContrast(colorScheme.BrushTextReper.Color, true);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
            // Определение renderOptions.opCurveScale
            SetRB();

            int indexPole = renderOptions.indexValues;
            if (indexPole > -1)
            {
                //double SumV = 0;
                tSS_Max.Text = graphicsData.curves[indexPole].MaxY().ToString("F4");
                tSS_Min.Text = graphicsData.curves[indexPole].MinY().ToString("F4");
                tSS_Integral.Text = graphicsData.curves[indexPole].IntegtalCurve().ToString("F4");
                tsLabel.Text = graphicsData.curves[indexPole].Name;
                // масштабирование
                for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
                {
                    string nameX = tbScaleX.Text;
                    string nameY = tbScaleY.Text;
                    double sx = MEM.ParserABS(ref nameX, 1);
                    double sy = MEM.ParserABS(ref nameY, 1);
                    tbScaleX.Text = nameX;
                    tbScaleY.Text = nameY;
                    graphicsData.curves[i].SetScale(renderOptions.opCurveScale,
                                                    renderOptions.opAutoScaleX, 
                                                    renderOptions.opAutoScaleY, sx, sy);
                }
            }
            else
                Logger.Instance.Info("Ошибка! индекс поля отрицательный, список графиков пуст");
            // отсылка опций для обновления контрола
            proxyRendererControl.SetData(graphicsData);
            proxyRendererControl.colorScheme = colorScheme;
            proxyRendererControl.renderOptions = renderOptions;
        }

        #region Работа со шрифтами
        private void SetColorFont()
        {
            btFontCoords.Font = colorScheme.FontReper;
            //button7.Font = colorScheme.FontKnot;
            //button8.Font = colorScheme.FontValue;
        }
        public Font GetFont(Font b)
        {
            try
            {
                fontDialog1.Font = b;
                if (fontDialog1.ShowDialog() == DialogResult.OK)
                {
                    b = fontDialog1.Font;
                    //(sender as Button).Font = fontDialog1.Font;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
            }
            return b;
        }
        #endregion

        #region Работа с кистями
        private void SetColorBrush()
        {
            btBrushCurve.BackColor = colorScheme.BrushPoint.Color;
            //btBrushCurve.ForeColor = ExtensionsColor.GetContrast(btBrushCurve.BackColor);
            btBrushCoords.BackColor = colorScheme.BrushTextReper.Color;
            //btBrushCoords.ForeColor = ExtensionsColor.GetContrast(btBrushCoords.BackColor);
            btBrushFields.BackColor = colorScheme.BrushTextValues.Color;
            //btBrushCoords.ForeColor = ExtensionsColor.GetContrast(btBrushCoords.BackColor);
        }
        public SolidBrush GetBraush(SolidBrush b, object sender)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b = new SolidBrush(colorDialog1.Color);
                (sender as Button).BackColor = colorDialog1.Color;
                (sender as Button).ForeColor = ExtensionsColor.GetContrast(colorDialog1.Color, true);
            }
            return b;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            colorScheme.BrushPoint = GetBraush(colorScheme.BrushPoint, sender);
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
        private void SetColorPen()
        {
           // btColorCoord.BackColor = colorScheme.PenGraphLine.Color;
            nUD_penGraphLine.Value = (int)colorScheme.PenGraphLine.Width;
        }
        #endregion

        #region Обработчики события изменения размера
        /// <summary>
        /// Перегруженный метод скрола мыши для масштабирования изображения
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var container = this.panel1.ClientRectangle;
            Point pt = e.Location;
            int Delta = e.Delta;
            if (container.Contains(pt))
            {
                proxyRendererControl.ZoomWheel(((float)pt.X) / container.Width,
                    ((float)pt.Y) / container.Height, Delta);
            }
            base.OnMouseWheel(e);
        }
        /// <summary>
        /// Свернуть окно и развернуть
        /// </summary>
        private void ResizeHandler(object sender, EventArgs e)
        {
            proxyRendererControl.HandleResize();
        }
        #endregion


        /// <summary>
        ///  Установка точки сохранения
        /// </summary>
        /// <param name="sp"></param>
        public void SetSavePoint(SavePoint sp)
        {
            if (sp != null)
            {
                SendSavePoint(sp);
                cb_opGraphicCurve.Checked = false;
                cb_opGraphicCurve.Checked = true;
            }
        }
        /// <summary>
        /// Установка данных и опций в RendererControl
        /// отрисовка параметров сетки в статус бар
        /// </summary>
        /// <param name="sp"></param>
        public void SendSavePoint(ISavePoint isp)
        {
            GNames = isp.graphicsData.GraphicGroupNames();
            CurGNames.Clear();
            CurGNames.AddRange(GNames.ToArray());
            Refrech(isp);
        }
        protected void Refrech(ISavePoint isp)
        {
            if (isp != null)
            {
                this.isp = isp;
                GraphicsData gd = isp.graphicsData.GetSubIGraphicsData(tGraphicsCurve, CurGNames) as GraphicsData;
                if (gd != null)
                {
                    // Запись данных в списки компонента
                    SetData(gd);
                    // Передача данных в прокси/рендер контрол
                   // proxyRendererControl.SetData(gd);
                    // отрисовка в статус бар
                    tSSL_Time.Text = isp.time.ToString("F4");
                    tSSL_Curves.Text = gd.curves.Count.ToString();
                    SendOption();
                }
            }
        }

        public string GNName(string Name)
        {
            string[] lines = Name.Split('#');
            if (lines.Length == 1)
                return Name;
            else
                return lines[1];
        }

        /// <summary>
        /// Запись данных в списки компонента
        /// </summary>
        /// <param name="spData">Данные для отрисовки</param>
        public void SetData(GraphicsData spData)
        {
            this.graphicsData = spData;
            List<string> Names = graphicsData.GraphicNames();
            checkedListBoxCurve.Items.Clear();
            if (Names.Count > 0)
            {
                foreach (var name in Names)
                    checkedListBoxCurve.Items.Add(name);

                checkedListBoxCurve.SelectedIndex = 0;

                for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
                {
                    if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Unchecked)
                        graphicsData.curves[i].Check = false;
                    else
                        graphicsData.curves[i].Check = true;
                }
            }
            else
                checkedListBoxCurve.SelectedIndex = -1;

            
            if (CurGNames.Count > 0)
            {
                bool flag = false;
                foreach (var name in CurGNames)
                    if(checkedListBoxGroup.Items.Contains(name) == false)
                    {
                        checkedListBoxGroup.Items.Add(name);
                        flag = true;
                    }
                if(flag == true)
                {
                    for (int i = 0; i < checkedListBoxGroup.Items.Count; i++)
                    {
                        checkedListBoxGroup.SetItemChecked(i, true);
                    }
                }
                checkedListBoxGroup.SelectedItem = 0;
            }
            else
                checkedListBoxGroup.SelectedItem = -1;

            cb_opGraphicCurve.Checked = true;
        }
  

        string GetName(string name)
        {
            string[] names = name.Split('#', ' ');
            int Count = name.Length - (names[0].Length + 1);
            Count = Count > 12 ? 12 : Count;
            string n = name.Substring(names[0].Length + 1, Count);
            return n;
        }

        #region Pen
        // кривые
        private void btColorCurve_Click(object sender, EventArgs e)
        {
            colorScheme.PenGraphLine = GetPen(colorScheme.PenGraphLine,
                                        (int)nUD_penGraphCurve.Value, sender);
            SetColorBrush();
        }
        private void nUD_penGraphCurve_ValueChanged(object sender, EventArgs e)
        {
            colorScheme.PenGraphLine = new Pen(colorScheme.PenGraphLine.Color,
              (int)nUD_penGraphCurve.Value);
        }
        // координаты 
        private void btColorCoord_Click(object sender, EventArgs e)
        {
            colorScheme.PenReper = GetPen(colorScheme.PenReper,
                            (int)nUD_penGraphLine.Value, sender);
        }
        private void nUD_penGraphLine_ChangeUICues(object sender, UICuesEventArgs e)
        {
            colorScheme.PenGraphLine = new Pen(colorScheme.PenGraphLine.Color,
                                       (int)nUD_penGraphLine.Value);
        }
        private void nUD_penGraphLine_ValueChanged(object sender, EventArgs e)
        {
            colorScheme.PenGraphLine = new Pen(colorScheme.PenGraphLine.Color,
                (int)nUD_penGraphLine.Value);
        }
        #endregion
        #region Brush
        
        /// <summary>
        /// координаты 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btBrushCurve_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextReper = GetBraush(colorScheme.BrushTextReper, sender);
        }
        /// <summary>
        /// номера узлов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btBrushCoords_Click(object sender, EventArgs e)
        {
            colorScheme.BrushPoint = GetBraush(colorScheme.BrushPoint, sender);
        }
        /// <summary>
        /// полей в узлах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btBrushFields_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextValues = GetBraush(colorScheme.BrushTextValues, sender);
        }

        #endregion
        #region Font
        // кривые
        private void btFontCurve_Click(object sender, EventArgs e)
        {
            colorScheme.FontValue = GetFont(colorScheme.FontValue);
            textBoxCurve.Font = colorScheme.FontValue;
        }
        // координаты 
        private void btFontCoords_Click(object sender, EventArgs e)
        {
            colorScheme.FontReper = GetFont(colorScheme.FontReper);
            textBoxCoords.Font = colorScheme.FontReper;
        }


        #endregion

        private void buttonSetReg_Click(object sender, EventArgs e)
        {
            SetRegion();
        }
        private void SetRegion()
        {
            try
            {
                PointF a = new PointF(float.Parse(textBoxXmin.Text, LOG.formatter),
                                      float.Parse(textBoxYmin.Text, LOG.formatter));
                PointF b = new PointF(float.Parse(textBoxXmax.Text, LOG.formatter),
                                      float.Parse(textBoxYmax.Text, LOG.formatter));
                proxyRendererControl.SetRegion(a, b);
                proxyRendererControl.HandleResize();
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
            }
        }
        /// <summary>
        /// Выборка кривых для рисования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbRefreshCurves_SelectedIndexChanged(object sender, EventArgs e)
        {
            tGraphicsCurve = (TypeGraphicsCurve)cbRefreshCurves.SelectedIndex;
            Refrech(isp);
        }

        private void cbAutoScaleX_CheckedChanged(object sender, EventArgs e)
        {
            SendOption();
            panel1.Update();
        }

        private void cbAutoScaleY_CheckedChanged(object sender, EventArgs e)
        {
            SendOption();
            panel1.Update();
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            IGraphicsCurve curve = new GraphicsCurve();
            IOFormater<IGraphicsCurve> loader = curve.GetFormater();
            openFileDialog1.Filter = loader.FilterLD;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string PathFileName = openFileDialog1.FileName;
                string ext = Path.GetExtension(PathFileName);
                if (ext == ".cvs")
                {
                    loader.Read(openFileDialog1.FileName, ref curve);
                    if (isp == null)
                        isp = new SavePoint();
                    isp.AddCurve(curve);
                    SendSavePoint(isp);
                }
                else
                {
                    // Получаем папку, в которой находится файл
                    string PathFile = Path.GetDirectoryName(PathFileName);
                    // Получаем список файлов с заданным расширением
                    string[] files = Directory.GetFiles(PathFile, "*.fun");
                    //Console.WriteLine("Всего файлов {0}.", files.Length);
                    foreach (string pFileName in files)
                    {
                        loader.Read(pFileName, ref curve);
                        if (isp == null)
                            isp = new SavePoint();
                        isp.AddCurve(curve);
                    }
                    Logger.Instance.Info("Всего файлов загружено " + (files.Length).ToString());
                    SendSavePoint(isp);
                }
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            try
            {
                int indexPole = checkedListBoxCurve.SelectedIndex;
                if (indexPole == -1) return;
                GraphicsCurve curve = graphicsData.curves[indexPole];
                string Name = ((GraphicsCurve)curve).Name;
                IOFormater<IGraphicsCurve> wraiter = curve.GetFormater();
                saveFileDialog1.Filter = wraiter.FilterSD;
                saveFileDialog1.FileName = Name;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
                    {
                        if (checkedListBoxCurve.GetItemCheckState(i) == CheckState.Unchecked)
                            graphicsData.curves[i].Check = false;
                        else
                            graphicsData.curves[i].Check = true;
                    }
                    for (int ci = 0; ci < graphicsData.curves.Count; ci++)
                    {
                        if (graphicsData.curves[ci].Check == true)
                        {
                            curve = graphicsData.curves[ci];
                            int Count = 0;
                            if (cbCompress.Checked == true && 
                                WR.Parse(btN.Text, ref Count) == true)
                            {
                                double[] x = null, y = null;
                                MEM.Alloc(curve.Count, ref x);
                                MEM.Alloc(curve.Count, ref y);
                                for (int i = 0; i < curve.Count; i++)
                                {
                                    x[i] = curve[i].x;
                                    y[i] = curve[i].y;
                                }
                                DigFunction fun = new DigFunction(x, y);
                                double[] xx = null, yy = null;
                                fun.GetFunctionData(ref xx, ref yy, Count);
                                GraphicsCurve tcurve = new GraphicsCurve(curve.Name, xx, yy);
                                curve = tcurve;
                            }
                            int T = (int)isp.time;
                            string ext = Path.GetExtension(saveFileDialog1.FileName);
                            string path = Path.GetDirectoryName(saveFileDialog1.FileName);
                            string name = Path.GetFileName(saveFileDialog1.FileName);
                            string filename = path +"\\"+ ci.ToString() + "_" + name;
                            wraiter.Write(curve, filename);
                            //wraiter.Write(curve, saveFileDialog1.FileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
            //int indexPole = checkedListBoxCurve.SelectedIndex;
            //if (indexPole > -1)
            //{
            //    GraphicsCurve curve = graphicsData.curves[indexPole];
            //    if(cbCompress.Checked == true)
            //    {
            //        int Count = int.Parse(btN.Text);
            //        double[] x = null, y = null;
            //        MEM.Alloc(curve.Count, ref x);
            //        MEM.Alloc(curve.Count, ref y);
            //        for (int i = 0; i < curve.Count; i++)
            //        {
            //            x[i] = curve[i].x;
            //            y[i] = curve[i].y;
            //        }
            //        DigFunction fun = new DigFunction(x, y);
            //        double[] xx = null, yy = null;
            //        fun.GetFunctionData(ref xx, ref yy, Count);
            //        GraphicsCurve tcurve = new GraphicsCurve(curve.Name, xx, yy);
            //        curve = tcurve;
            //    }
            //    string Name = ((GraphicsCurve)curve).Name;
            //    IOFormater<IGraphicsCurve> wraiter = curve.GetFormater();
            //    saveFileDialog1.Filter = wraiter.FilterSD;
            //    saveFileDialog1.FileName = Name;
            //    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //    {
            //        wraiter.Write(curve, saveFileDialog1.FileName);
            //    }
            //}
        }
        /// <summary>
        /// среднее арифметическое выборки
        /// оценка математического ожидания величины (выборочное среднее)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Average_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole > -1)
            {
                try
                {
                    double xa = double.Parse(tb_Xa.Text, MEM.formatter);
                    double xb = double.Parse(tb_Xb.Text, MEM.formatter);
                    if (MEM.Equals(xb - xa, MEM.Error10) == false)
                    {
                        tSS_Analys.Text = graphicsData.curves[indexPole].AverageCurve(xa, xb).ToString("F4");
                    }
                    else
                    {
                        tSS_Analys.Text = "Xa == Xb! Диапазон задан не верно!";
                    }
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }
        /// <summary>
        /// Оценка стандартного отклонения на основании смещённой оценки дисперсии
        /// (иногда называемой просто выборочной дисперсией)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_StandardDeviationCurve_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole > -1)
            {
                try
                {
                    double xa = double.Parse(tb_Xa.Text, MEM.formatter);
                    double xb = double.Parse(tb_Xb.Text, MEM.formatter);
                    tSS_Analys.Text = graphicsData.curves[indexPole].StandardDeviationCurve(xa, xb).ToString("F4");
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }
        /// <summary>
        /// Оценка стандартного отклонения на основании несмещённой оценки дисперсии 
        /// (подправленной выборочной дисперсии, 
        /// в ГОСТ Р 8.736-2011 — «среднее квадратическое отклонение»)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_StandardDeviation_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole > -1)
            {
                try
                {
                    double xa = double.Parse(tb_Xa.Text, MEM.formatter);
                    double xb = double.Parse(tb_Xb.Text, MEM.formatter);
                    tSS_Analys.Text = graphicsData.curves[indexPole].StandardDeviationCurve(xa, xb, 1).ToString("F4");
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }

        private void btCovern_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole > -1)
            {
                try
                {
                    double zetaMin = 0;
                    double xa = 0;
                    double xb = 1;
                    graphicsData.curves[indexPole].GetCovernInterval(ref xa, ref xb, ref zetaMin);
                    tb_Xa.Text = xa.ToString("F6");
                    tb_Xb.Text = xb.ToString("F6");
                    tb_Xc.Text = "0";
                    double L = xb - xa;
                    tb_L.Text = L.ToString("F6");
                    double q = Math.Abs(zetaMin/L);
                    tb_q.Text = q.ToString("F6");
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }

        private void btHill_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole > -1)
            {
                try
                {
                    double zetaMax = 0;
                    double xa = 0;
                    double xb = 1;
                    graphicsData.curves[indexPole].GetHillInterval(ref xa, ref xb, ref zetaMax);
                    tb_Xa.Text = "0";
                    tb_Xb.Text = xa.ToString("F6");
                    tb_Xc.Text = xb.ToString("F6");
                    double L = xb - xa;
                    tb_L.Text = L.ToString("F6");
                    double q = Math.Abs(zetaMax / L);
                    tb_q.Text = q.ToString("F6");
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }

        private void btWave_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole > -1)
            {
                try
                {
                    double zetaMin = 0;
                    double zetaMax = 0;
                    double xa = 0;
                    double xb = 1;
                    double xc = 1;
                    double xd = 1;
                    graphicsData.curves[indexPole].GetCovernInterval(ref xa, ref xb, ref zetaMin);
                    graphicsData.curves[indexPole].GetHillInterval(ref xc, ref xd, ref zetaMax);
                    double x = (xb + xc) / 2;
                    tb_Xa.Text = xa.ToString("F6");
                    tb_Xb.Text = xb.ToString("F6");
                    tb_Xc.Text = xd.ToString("F6");
                    double L = xd - xa;
                    tb_L.Text = L.ToString("F6");
                    double q = Math.Abs(zetaMax - zetaMin / L);
                    tb_q.Text = q.ToString("F6");
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }
        int oldX = 0, oldY = 0;
        private void nud_X_ValueChanged(object sender, EventArgs e)
        {
            GV(nud_X, tbScaleX,ref oldX);
        }

        private void nud_Y_ValueChanged(object sender, EventArgs e)
        {
            GV(nud_Y, tbScaleY,ref oldY);
        }

        /// <summary>
        /// Симафор для изменения cb_opGraphicCurve.Checked без вызова обработчика событий
        /// </summary>
        bool Look = true;
        /// <summary>
        /// Групповое выделение/сброс чекита кривых
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_opGraphicCurve_CheckedChanged(object sender, EventArgs e)
        {
            if (Look == true)
                for (int i = 0; i < checkedListBoxCurve.Items.Count; i++)
                {
                    checkedListBoxCurve.SetItemChecked(i, cb_opGraphicCurve.Checked);
                    graphicsData.curves[i].Check = cb_opGraphicCurve.Checked;
                }
        }

        private void checkedListBoxCurve_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool flag = false;
            int i = checkedListBoxCurve.SelectedIndex;
            if (i == -1) return;
            for (; i < checkedListBoxCurve.Items.Count; i++)
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
        /// Симафор для изменения cb_opGraphicCurve.Checked без вызова обработчика событий
        /// </summary>
        bool LookGroup = true;

        private void cbFNames_CheckedChanged(object sender, EventArgs e)
        {
            if (LookGroup == true)
            {
                for (int i = 0; i < checkedListBoxGroup.Items.Count; i++)
                    checkedListBoxGroup.SetItemChecked(i, cbFNames.Checked);
            }
        }

        private void checkedListBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool flag = false;
            int i = checkedListBoxGroup.SelectedIndex;
            if (i == -1) return;
            for (; i < checkedListBoxGroup.Items.Count; i++)
            {
                if (checkedListBoxGroup.GetItemCheckState(i) == CheckState.Checked)
                {
                    flag = true;
                    break;
                }
            }
            LookGroup = false;
            cbFNames.Checked = flag;
            LookGroup = true;
        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        int indexPoleA = -1;
        int indexPoleB = -1;

        private void tbCurveA_Click(object sender, EventArgs e)
        {
            if (checkedListBoxCurve.SelectedIndex != -1)
            {
                indexPoleA = checkedListBoxCurve.SelectedIndex;
                tbCurveA.Text = (string)checkedListBoxCurve.Items[indexPoleA];
            }
        }
        private void tbCurveB_Click(object sender, EventArgs e)
        {
            if (checkedListBoxCurve.SelectedIndex != -1)
            {
                indexPoleB = checkedListBoxCurve.SelectedIndex;
                tbCurveB.Text = (string)checkedListBoxCurve.Items[indexPoleB];
            }
        }

        private void btCalkMeasure_Click(object sender, EventArgs e)
        {
            if (indexPoleA > -1 && indexPoleB > -1 
                && cbMeasure.SelectedIndex > -1)
            {
                try
                {
                    GraphicsCurve curvesA = graphicsData.curves[indexPoleA];
                    GraphicsCurve curvesB = graphicsData.curves[indexPoleB];
                    double result = 0;
                    GraphicsCurve.CalkMeasure(curvesA, curvesB,
                    (MeasureType)cbMeasure.SelectedIndex, ref result);
                    tbError.Text = result.ToString("F6");
                }
                catch (Exception ee)
                {
                    tSS_Analys.Text = ee.Message;
                }
            }
        }
        /// <summary>
        /// Удалить кривую
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDel_Click(object sender, EventArgs e)
        {
            int indexPole = checkedListBoxCurve.SelectedIndex;
            if (indexPole == -1) return;
            GraphicsCurve curve = graphicsData.curves[indexPole];
            isp.RemoveCurve(curve.Name);
            SendSavePoint(isp);
        }

        private void GV(NumericUpDown nud, TextBox tb, ref int old)
        {
            int idx = (int)nud.Value;
            double v = double.Parse(tb.Text);
            v = v * Math.Pow(10, idx  - old);
            tb.Text = v.ToString();
            old = idx;
        }
    }
}
