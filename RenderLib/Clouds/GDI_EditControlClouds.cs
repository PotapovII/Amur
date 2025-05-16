//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 28.11.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using GeometryLib;
    using MemLogLib;
    
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using CommonLib.Areas;
    using CommonLib.Geometry;
    /// <summary>
    ///ОО: Компонент визуализации облачных данных и котруров границ 
    /// </summary>
    public partial class GDI_EditControlClouds : UserControl
    {
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        public List<CloudKnot> Conturs
        {
            get => control.Conturs;
            set => control.Conturs = value;
        }
        /// <summary>
        /// Контурная область используется как фильтр границ при генерации КЭ сетки
        /// </summary>
        public IMArea Area 
        {
            get => control.Area;
        }
        /// <summary>
        /// Менеджер отрисовки данных
        /// </summary>
        ProxyRendererEditControl control;
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
        /// <summary>
        /// Установка опций состояний редактора 
        /// </summary>
        EditRenderOptions editOptions;
        /// <summary>
        /// 
        /// </summary>
        int oldidx = -1;

        public GDI_EditControlClouds()
        {
            InitializeComponent();
            saveFileDialog1.Filter = "Облако данных (*.node)|*.node|" +
                                     "All files (*.*)|*.*";
            renderOptions = new RenderOptionsFields();
            colorScheme = new ColorSchemeClouds();
            editOptions = new EditRenderOptions();
            control = new ProxyRendererEditControl();
            control.sendPintData = this.SetCloudKnot;
            control.sendSListData = this.SetSLines;

            Control mControl = control.RenderControl;
            control.RenderControl.BackColor = Color.White;
            if (mControl != null)
            {
                // привязка менеджера отрисовки к панели
                this.panel1.Controls.Add(mControl);
                mControl.BackColor = Color.White;
                mControl.Dock = DockStyle.Fill;
                mControl.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                mControl.Location = new Point(0, 0);
                mControl.Name = "renderControl1";
                mControl.TabIndex = 0;
                mControl.Text = "";
                mControl.Visible = true;

                control.Initialize();
            }
            else
            {
                Text = "Eeee ..., не удалось инициализировать средство визуализации.";
            }
            SetRenderOptions();
            SetColorManager();
        }
        #region Работа с контуром
        public void SetEditState(EditState editState)
        {
            control.editRenderOptions.editState = editState;
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

        protected CloudKnot GetCloudKnot()
        {
            int mark = 1;
            CloudKnot point = new CloudKnot(0, 0, new double[] { 0, 0, 0, 0, 0 }, mark);
            point.x = LOG.Double(textBoxX.Text);
            point.y = LOG.Double(textBoxY.Text);
            point.Attributes[0] = LOG.Double(tb_H.Text);
            point.Attributes[1] = LOG.Double(tb_T.Text);
            point.Attributes[2] = LOG.Double(tb_V.Text);
            point.Attributes[3] = LOG.Double(tb_C.Text);
            return point;
        }
        private void btUpDate_Click(object sender, EventArgs e)
        {
            CloudKnot point = GetCloudKnot();
            control.UpCloudKnot(point);
        }
        private void bt_SegmentKnots_Click(object sender, EventArgs e)
        {
            UpDateFigure(listBoxPoints.SelectedIndex,
                         listBoxSegments.SelectedIndex,
                         listBoxFig.SelectedIndex);
        }

        private void bt_AllKnots_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxFig.SelectedIndex > -1)
                {
                    int CountKnots = LOG.Int(tbSegCountKnots.Text);
                    control.UpDateFigure(listBoxFig.SelectedIndex, CountKnots);
                }
                else
                    Logger.Instance.Info("Фигура не определена !");
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                tbSegCountKnots.Focus();
            }
        }
        private void btClear_Click(object sender, EventArgs e)
        {
            ClearContur();
        }
        private void btDelKnot_Click(object sender, EventArgs e)
        {
            DelLastKnotCountur();
        }
        #endregion
        #region Работа с интерфейсом
        ///// <summary>
        ///// Установка региона
        ///// </summary>
        ///// <param name="a">левый верхний угол региона</param>
        ///// <param name="b">правый нижний угол региона</param>
        //void SetRegion(PointF a, PointF b);
        /// <summary>
        /// Удалить последний узел контура
        /// </summary>
        public void DelLastKnotCountur()
        {
            control.DelCloudKnot();
        }
        /// <summary>
        /// Удалить контур
        /// </summary>
        public void ClearContur()
        {
            control.ClearCurContur();
        }
        /// <summary>
        ///  Удаление последней линий сглаживания
        /// </summary>
        /// <param name="p"></param>
        public void DelSmLines()
        {
            tb_LLS.Text = "";
            control.DelSmLines();
        }
        /// <summary>
        ///  удаление линии створа
        /// </summary>
        /// <param name="p"></param>
        public void DelCrossLine()
        {
            control.DelCrossLine();
        }
        /// <summary>
        /// Удаление линий сглаживания
        /// </summary>
        public void ClearSmLines()
        {
            tb_LLS.Text = "";
            control.ClearSmLines();
        }
        /// <summary>
        /// Программно добавить точку контура
        /// </summary>
        public void AddCloudKnotToContur(CloudKnot knot)
        {
            control.AddCloudKnotToContur(knot);
            SetCloudKnot(knot);
        }
        /// <summary>
        /// Сохранить текущий контур как фигуру
        /// </summary>
        public void SaveFigura()
        {
            List<string> lNames = control.AddFigs();
            if (lNames != null)
            {
                string[] names = lNames.ToArray();
                listBoxFig.Items.Clear();
                listBoxFig.Items.AddRange(names);
                listBoxFig.SelectedIndex = 0;
                ChangeRB();
            }
        }
        /// <summary>
        /// Сохранить текущий контур как фигуру
        /// </summary>
        public void AddFigs(IMFigura fig)
        {
            List<string> lNames = control.AddFigs(fig);
            if (lNames != null)
            {
                string[] names = lNames.ToArray();
                listBoxFig.Items.Clear();
                listBoxFig.Items.AddRange(names);
                listBoxFig.SelectedIndex = 0;
                ChangeRB();
            }
        }
        /// <summary>
        /// Удалить  фигуру находящуюся в фокусе
        /// </summary>
        public void DelCurFigura()
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
        }
        /// <summary>
        /// Удалить все фигуры области
        /// </summary>
        public void ClearArea()
        {
            control.ClearFigs();
            listBoxPoints.Items.Clear();
            listBoxSegments.Items.Clear();
            listBoxFig.Items.Clear();
            lbBoundary.Items.Clear();
        }
        #endregion
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
                control.ZoomWheel(((float)pt.X) / container.Width,
                    ((float)pt.Y) / container.Height, e.Delta);
            }
            base.OnMouseWheel(e);
        }

        #region Обработчики события изменения размера
        /// <summary>
        /// Свернуть окно и развернуть
        /// </summary>
        private void ResizeHandler(object sender, EventArgs e)
        {
            control.HandleResize();
        }
        /// <summary>
        /// Обновление опций
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btShow_Click(object sender, EventArgs e)
        {
            SendOption();
        }
        /// <summary>
        /// Обновить опции компонента
        /// </summary>
        public void SendOption()
        {

            renderOptions.showFilter = tabControlState.SelectedIndex;
            renderOptions.indexValues = listBoxPoles.SelectedIndex;
            renderOptions.showBoudaryKnots = cb_ColoBrushrKnots.Checked;
            renderOptions.opValuesKnot = cb_opValuesKnot.Checked;
            renderOptions.showKnotNamber = cb_opKnot.Checked;
            renderOptions.opVectorValues = cb_opVectorValues.Checked;
            renderOptions.coordReper = cb_coordReper.Checked;
                                       //

            colorScheme.formatTextReper = (uint)nUD_formatReper.Value;
            colorScheme.formatTextValues = (uint)nUD_formatText.Value;
            
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
            nUD_formatText.Value =  colorScheme.formatTextValues;
            nUD_formatReper.Value = colorScheme.formatTextReper;
        }
        public void SetRenderOptions()
        {
            cb_ColoBrushrKnots.Checked = renderOptions.showBoudaryKnots;
            cb_opKnot.Checked = renderOptions.opValuesKnot;
            cb_opVectorValues.Checked = renderOptions.opVectorValues;
            cb_coordReper.Checked = renderOptions.coordReper;
        }
        #endregion

        #region Работа со шрифтами
        private void SetColorFont()
        {
            bt_FontReper.Font = colorScheme.FontReper;
            tb_FontReper.Text = colorScheme.FontReper.ToString();
            bt_FontCloud.Font = colorScheme.FontNodesCloud;
            tb_FontCloud.Text = colorScheme.FontNodesCloud.ToString();

            bt_Reper.BackColor = colorScheme.PenReper.Color;
            bt_Area.BackColor = colorScheme.PenAreaLine.Color;
            bt_AreaFocus.BackColor = colorScheme.PenAreaFocusLine.Color;
            bt_Segment.BackColor = colorScheme.PenSegmentLine.Color;
            bt_SegmentFocus.BackColor = colorScheme.PenSegmentFocusLine.Color;
            bt_Countor.BackColor = colorScheme.PenCounturLine.Color;
            bt_IsoLine.BackColor = colorScheme.PenVectorLine.Color;
            bt_InArea.BackColor = colorScheme.PenInSegmentLine.Color;
            bt_OutArea.BackColor = colorScheme.PenOutSegmentLine.Color;
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

        private void bt_FontReper_Click(object sender, EventArgs e)
        {
            colorScheme.FontReper = GetFont(colorScheme.FontReper, sender);
            bt_FontReper.Font = colorScheme.FontReper;
            tb_FontReper.Text = colorScheme.FontReper.ToString();
        }
        private void bt_FontCloud_Click(object sender, EventArgs e)
        {
            colorScheme.FontNodesCloud = GetFont(colorScheme.FontNodesCloud, sender);
            bt_FontCloud.Font = colorScheme.FontNodesCloud;
            tb_FontCloud.Text = colorScheme.FontNodesCloud.ToString();
        }
        #endregion

        #region Работа с кистями
        private void SetColorBrush()
        {
            bt_NCoord.BackColor = colorScheme.BrushReper.Color;
            bt_Coords.BackColor = colorScheme.BrushTextReper.Color;

            bt_NunCoord.BackColor = colorScheme.BrushNodes.Color;
            bt_NunCloud.BackColor = colorScheme.BrushTextNodes.Color;
            bt_ValueCloud.BackColor = colorScheme.BrushTextValues.Color;
            bt_NCounturFoc.BackColor = colorScheme.BrushNodeFocusContur.Color;
            bt_NCountur.BackColor = colorScheme.BrushNodeContur.Color;
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

        #region Репер - базисный класс
        private void bt_NCoord_Click(object sender, EventArgs e)
        {
            colorScheme.BrushReper = GetBraush(colorScheme.BrushReper, sender);
        }

        private void bt_Coords_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextReper = GetBraush(colorScheme.BrushTextReper, sender);
        }
        #endregion 

        private void bt_NunCoord_Click(object sender, EventArgs e)
        {
            colorScheme.BrushNodes = GetBraush(colorScheme.BrushNodes, sender);
        }

        private void bt_NunCloud_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextNodes = GetBraush(colorScheme.BrushTextNodes, sender);
        }

        private void bt_ValueCloud_Click(object sender, EventArgs e)
        {
            colorScheme.BrushTextValues = GetBraush(colorScheme.BrushTextValues, sender);
        }

        private void bt_NCounturFoc_Click(object sender, EventArgs e)
        {
            colorScheme.BrushNodeFocusContur = GetBraush(colorScheme.BrushNodeFocusContur, sender);
        }

        private void bt_NCountur_Click(object sender, EventArgs e)
        {
            colorScheme.BrushNodeContur = GetBraush(colorScheme.BrushNodeContur, sender);
        }



        #endregion

        #region Работа с перьями
        private Pen GetPen(Pen b, int d, object sender)
        {
            colorDialog1.Color = b.Color;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                b = new Pen(colorDialog1.Color, d);
                Button bt = sender as Button;
                if (bt != null)
                    bt.BackColor = colorDialog1.Color;
            }
            return b;
        }
        private void bt_Area_Click(object sender, EventArgs e)
        {
            colorScheme.PenAreaLine = GetPen(colorScheme.PenAreaLine,
                            (int)nUD_penAreaLine.Value, sender);
        }

        private void bt_AreaFocus_Click(object sender, EventArgs e)
        {
            colorScheme.PenAreaFocusLine = GetPen(colorScheme.PenAreaFocusLine,
                            (int)nUD_penAreaLineFocus.Value, sender);
        }

        private void bt_Segment_Click(object sender, EventArgs e)
        {
            colorScheme.PenSegmentLine = GetPen(colorScheme.PenSegmentLine,
                            (int)nUD_penSegmentLine.Value, sender);
        }

        private void bt_SegmentFocus_Click(object sender, EventArgs e)
        {
            colorScheme.PenSegmentFocusLine = GetPen(colorScheme.PenSegmentFocusLine,
                            (int)nUD_penSegmentLineFocus.Value, sender);
        }

        private void bt_Reper_Click(object sender, EventArgs e)
        {
            colorScheme.PenReper = GetPen(colorScheme.PenReper,
                          (int)nUD_penReperLine.Value, sender);
        }

        private void bt_Countor_Click(object sender, EventArgs e)
        {
            colorScheme.PenCounturLine = GetPen(colorScheme.PenCounturLine,
                        (int)nUD_penCounturLine.Value, sender);
        }

        private void bt_IsoLine_Click(object sender, EventArgs e)
        {
            colorScheme.PenVectorLine = GetPen(colorScheme.PenVectorLine,
                   (int)nUD_penVectorLine.Value, sender);
        }

        private void bt_InArea_Click(object sender, EventArgs e)
        {
            colorScheme.PenInSegmentLine = GetPen(colorScheme.PenInSegmentLine,
                                     (int)nUD_penInArea.Value, sender);
        }

        private void bt_OutArea_Click(object sender, EventArgs e)
        {
            colorScheme.PenOutSegmentLine = GetPen(colorScheme.PenOutSegmentLine,
                                     (int)nUD_penOutArea.Value, sender);
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
                control.SetData(clouds);
                SendOption();
                // отрисовка в статус бар
                tSSL_Nods.Text = clouds.CountKnots.ToString();
            }
        }

        private void tabControlState_SelectedIndexChanged(object sender, EventArgs e)
        {
            SendOption();
        }

        #region Работа с контурами/областью
        /// <summary>
        /// Записать контур
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSaveFig_Click(object sender, EventArgs e)
        {
            SaveFigura();
        }
        /// <summary>
        /// Удалить фигуру
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDelFig_Click(object sender, EventArgs e)
        {
            DelCurFigura();
        }
        /// <summary>
        /// Удалить фигуры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDelFigs_Click(object sender, EventArgs e)
        {
            ClearArea();
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
                        rbHole.Checked = true;
                        rbSubArea.Checked = false;
                    }
                    else
                    {
                        rbCountur.Checked = false;
                        rbHole.Checked = false;
                        rbSubArea.Checked = true;
                    }
                }
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
        private void listBoxFig_SelectedIndexChanged(object sender, EventArgs e)
        {
            Fig_SelectedIndexChanged();
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
        private void btUpDateFig_Click(object sender, EventArgs e)
        {
            // если не контур области
            if (listBoxFig.SelectedIndex > 0)
            {
                FigureType ft = control.GetFType(listBoxFig.SelectedIndex);
                if(rbHole.Checked == true && ft == FigureType.FigureHole)
                    return;
                // Установка типа контура
                if (rbHole.Checked == true)
                    control.SetFType(listBoxFig.SelectedIndex, FigureType.FigureHole);
                if (rbSubArea.Checked == true)
                {
                    control.SetFType(listBoxFig.SelectedIndex, FigureType.FigureSubArea);
                    double ice = double.Parse(tbIce.Text, MEM.formatter);
                    double ks = double.Parse(tbKs.Text, MEM.formatter);
                    control.SetAtributes(listBoxFig.SelectedIndex, ice, ks);
                }
            }
        }
        /// <summary>
        /// Изменить фигуру
        /// </summary>
        private void Fig_SelectedIndexChanged()
        {
            if (listBoxFig.SelectedIndex != -1)
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

        /// <summary>
        /// Выбор границы
        /// </summary>
        private void BoundarySelectedIndexChanged()
        {
            if (oldidx > -1 && oldidx == lbBoundary.SelectedIndex) return;
            if (lbBoundary.SelectedIndex != -1)
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
        /// <summary>
        /// Выбор сегмента
        /// </summary>
        /// <param name="SegmentSelectedIndex"></param>
        /// <param name="FigSelectedIndex"></param>
        private void SelectedSegment(int SegmentSelectedIndex, int FigSelectedIndex)
        {
            if (SegmentSelectedIndex != -1 && FigSelectedIndex != -1)
            {
                IMFigura fig = control.GetFig(FigSelectedIndex);
                IMSegment sf = fig.Segments[SegmentSelectedIndex];
                tbSegCountKnots.Text = sf.CountKnots.ToString();
                cbSegMark.SelectedIndex = sf.Marker - 1;
                control.SelectSegment(FigSelectedIndex, SegmentSelectedIndex);
            }
        }
        /// <summary>
        /// Обновление информации о сегменте
        /// </summary>
        private void UpDateFigure(int PointsSelectedIndex, int SegmentSelectedIndex, int FigSelectedIndex)
        {
            if (PointsSelectedIndex != -1 && FigSelectedIndex != -1 && SegmentSelectedIndex != -1)
            {
                int CountKnots = LOG.Int(tbSegCountKnots.Text);
                int Marker = cbSegMark.SelectedIndex + 1;
                control.UpDateFigSegment(FigSelectedIndex, SegmentSelectedIndex, CountKnots, Marker);
            }
        }
        /// <summary>
        /// Обновление информации о точке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPointFig_Click(object sender, EventArgs e)
        {
            int PointsSelectedIndex = listBoxPoints.SelectedIndex;
            int FigSelectedIndex = listBoxFig.SelectedIndex;
            CloudKnot p = GetCloudKnot();
            control.UpDateFigPoint(FigSelectedIndex, PointsSelectedIndex, p);
        }
        /// <summary>
        /// Загрузить информацию об точке
        /// </summary>
        private void GetPointInfo(int PointsSelectedIndex, int FigSelectedIndex)
        {
            if (PointsSelectedIndex != -1 && FigSelectedIndex != -1)
            {
                IMFigura fig = control.GetFig(FigSelectedIndex);
                IMPoint pf = fig.Points[PointsSelectedIndex];
                textBoxX.Text = pf.Point.X.ToString("F5");
                textBoxY.Text = pf.Point.Y.ToString("F5");
                CloudKnot ck = pf as CloudKnot;
                if (ck != null)
                {
                    tb_H.Text = ck.Attributes[0].ToString("F5");
                    // 1 - срезанная глубина
                    tb_T.Text = ck.Attributes[2].ToString("F5");
                    tb_V.Text = ck.Attributes[3].ToString("F5");
                    tb_C.Text = ck.Attributes[4].ToString("F5");
                }
                control.SelectKnot(FigSelectedIndex, PointsSelectedIndex);
            }
        }


        #endregion

        private void listBoxPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetPointInfo(listBoxPoints.SelectedIndex, listBoxFig.SelectedIndex);
        }
        //private void listBoxPoints_DoubleClick(object sender, EventArgs e)
        //{
        //    PointsUpDate(listBoxPoints.SelectedIndex, listBoxFig.SelectedIndex);
        //}
        private void lbBoundary_SelectedIndexChanged(object sender, EventArgs e)
        {
            BoundarySelectedIndexChanged();
        }

        private void listBoxSegments_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedSegment(listBoxSegments.SelectedIndex, listBoxFig.SelectedIndex);
        }

        private void сиSegMark_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbSegMark_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbSegMark.SelectedIndex > 0)
            {
                int CountKnots = LOG.Int(tbSegCountKnots.Text);
                if(CountKnots == 2)
                    tbSegCountKnots.Text = "20";
            }
            UpDateFigure(listBoxPoints.SelectedIndex,
                         listBoxSegments.SelectedIndex,
                         listBoxFig.SelectedIndex);
        }

        #region Работа с линиями сглаживания
        /// <summary>
        /// Загрузка линиями сглаживания в listBoxSLine
        /// </summary>
        /// <param name="SLines"></param>
        protected void SetSLines(List<IHSmLine> SLines)
        {
            listBoxSLine.Items.Clear();
            listBoxSLine.SelectedIndex = -1;
            for (int i = 0; i < SLines.Count; i++)
            {
                listBoxSLine.Items.Add("Линия " + i.ToString());
                if (SLines[i].Selected == 1)
                    listBoxSLine.SelectedIndex = i;
            }
            if (listBoxSLine.SelectedIndex == -1 && SLines.Count > 0)
            {
                listBoxSLine.SelectedIndex = listBoxSLine.Items.Count-1;
                SLines[listBoxSLine.SelectedIndex].Selected = 1;
                tb_SmLineCount.Text = SLines[listBoxSLine.SelectedIndex].Count.ToString();
            }
        }
        /// <summary>
        /// Смена фокуса в listBoxSLine
        /// </summary>
        private void listBoxSLine_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSLine.SelectedIndex != -1)
            {
                int Count = 0;
                double L = control.SelectSLines(listBoxSLine.SelectedIndex, ref Count);
                tb_LLS.Text = L.ToString();
                tb_SmLineCount.Text = Count.ToString();
            }
            else
            {
                tb_LLS.Text = "";
            }
        }
        /// <summary>
        /// Установить новое количество внутренних вершин линии сгласживания для всех линий
        /// </summary>
        private void btAllSmLines_Click(object sender, EventArgs e)
        {
            if (listBoxSLine.SelectedIndex != -1)
            {
                try
                {
                    int Count = int.Parse(tb_SmLineCount.Text);
                    for (int i = 0; i < listBoxSLine.Items.Count; i++)
                    {
                        control.UpDateSLines(i, Count);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception(ex);
                    MessageBox.Show(ex.Message);
                    tb_SmLineCount.Focus();
                }
            }
            else
            {
                Logger.Instance.Info("Линии сглаживания не определены !");
                tb_LLS.Text = "";
            }
        }
        /// <summary>
        /// Получить список линий сглаживания
        /// </summary>
        /// <returns></returns>
        public List<IHSmLine> GetSLines()
        {
            return control.SLines;
        }
        /// <summary>
        /// Получить линию створа
        /// </summary>
        /// <returns></returns>
        public IHLine GetCrossLine()
        {
            return control.crossLine;
        }

        /// <summary>
        ///  установка линии створа
        /// </summary>
        /// <param name="p"></param>
        public void SetCrossLine(IHLine crossLine)
        {
            control.crossLine = crossLine;
        }

        /// <summary>
        /// Установить новое количество внутренних вершин линии сгласживания 
        /// </summary>
        private void bt_bt_OpDateSmLine_Click(object sender, EventArgs e)
        {
            if (listBoxSLine.SelectedIndex != -1)
            {
                try
                {
                    int Count = int.Parse(tb_SmLineCount.Text);
                    control.UpDateSLines(listBoxSLine.SelectedIndex, Count);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception(ex);
                    MessageBox.Show(ex.Message);
                    tb_SmLineCount.Focus();
                }
            }
            else
            {
                Logger.Instance.Info("Линии сглаживания не определены !");
                tb_LLS.Text = "";
            }
        }
        /// <summary>
        /// Загрузка линий сглаживания
        /// </summary>
        /// <param name="sl"></param>
        public void LoadSmLines(List<IHSmLine> sl)
        {
            control.LoadSmLines(sl);
        }
        #endregion

        private void cbIce_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbKs_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
