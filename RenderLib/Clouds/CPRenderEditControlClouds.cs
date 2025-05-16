//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 16.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;

    using CommonLib;
    using GeometryLib;
    using MeshLib.SaveData;
    using GeometryLib.Areas;
    using GeometryLib.Vector;
    using CommonLib.Geometry;
    using CommonLib.Areas;
    using GeometryLib.World;
    using CommonLib.Delegate;
    using GeometryLib.Geometry;

    /// <summary>
    /// Визуализирует сетку с помощью GDI.
    /// </summary>
    public class CPRenderEditControlClouds : Control, IProxyRendererReaderClouds
    {
        /// <summary>
        /// индекс точки фильтра
        /// </summary>
        protected int pIndex { get; set; }
        /// <summary>
        /// Границы расчетной области
        /// </summary>
        public IMArea Area { get; }

        #region Работа с контуром границы
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        public List<CloudKnot> Conturs
        {
            get => conturs;
            set => conturs = value;
        }
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        List<CloudKnot> conturs = new List<CloudKnot>();
        /// <summary>
        /// Линии сглаживания
        /// </summary>
        public List<IHSmLine> SLines
        {
            get => sLines;
            set => sLines = value;
        }
        /// <summary>
        /// Линия створа
        /// </summary>
        public IHLine crossLine
        {
            get => _crossLine;
            set { _crossLine = value;
                if (sendCrossLine != null && _crossLine != null)
                    sendCrossLine(_crossLine);
            }
        }
        protected IHLine _crossLine;
        /// <summary>
        /// Линии сглаживания
        /// </summary>
        List<IHSmLine> sLines = new List<IHSmLine>();
        /// <summary>
        /// Количество заданных вершин сглаживания
        /// </summary>
        CloudKnot start = null;
        /// <summary>
        /// Загрузка данных в вершине
        /// </summary>
        public SendData<CloudKnot> sendPintData { get; set; } = null;
        /// <summary>
        /// Загрузка данных о линиях сглаживания
        /// </summary>
        public SendData<List<IHSmLine>> sendSListData { get; set; } = null;
        /// <summary>
        /// Загрузка данных о линии створа
        /// </summary>
        public SendData<IHLine> sendCrossLine { get; set; } = null;
        #endregion
        /// <summary>
        /// Флаг первой отрисовки при передаче данных
        /// </summary>
        private bool startFlag = false;
        /// <summary>
        /// Предоставляет графический буфер для двойной буферизации.
        /// </summary>
        private BufferedGraphics buffer;
        /// <summary>
        /// Предоставляет методы создания графических буферов, 
        /// которые могут использоваться для двойной буферизации.
        /// </summary>
        private BufferedGraphicsContext context;
        /// <summary>
        /// управляет преобразованием мировых координат в экраные (2D)
        /// </summary>
        WorldScaler zoom;
        /// <summary>
        /// Сохраняет данные задачи в удобной для рендеринга структуре данных.
        /// </summary>
        IClouds clouds;
        /// <summary>
        /// Класс методов для отрисовки данных
        /// </summary>
        TaskRendererClouds taskRenderer;
        /// <summary>
        /// Настройка цветовой схемы изображения
        /// </summary>
        ColorSchemeClouds colors = new ColorSchemeClouds();
        /// <summary>
        /// Опции отрисовки 
        /// </summary>
        RenderOptionsFields оptions = new RenderOptionsFields();
        /// <summary>
        /// Опции состояний редактора 
        /// </summary>
        EditRenderOptions editOptions = new EditRenderOptions();
        /// <summary>
        /// Флаг установки данных в контрол
        /// </summary>
        bool initialized = false;
        /// <summary>
        /// строка для координат
        /// </summary>
        string coordinate = String.Empty;
        /// <summary>
        /// Таймер для автообновления
        /// </summary>
        Timer timer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref = "RenderControl" />.
        /// </summary>
        public CPRenderEditControlClouds()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.BackColor = colors.Background;

            this.zoom = new WorldScaler(true);
            this.context = new BufferedGraphicsContext();
            this.clouds = new SavePointRiverNods();
            this.pIndex = 0;


            this.timer = new Timer();
            this.timer.Interval = 3000;
            this.timer.Tick += (sender, e) =>
            {
                timer.Stop();
                coordinate = String.Empty;
                this.Invalidate();
            };

            Area = new MArea();
        }

        /// <summary>
        /// Установка опций состояний редактора 
        /// </summary>
        public EditRenderOptions editRenderOptions
        {
            get
            {
                return taskRenderer == null ? editOptions : taskRenderer.editRenderOptions;
            }
            set
            {
                if (taskRenderer != null)
                    taskRenderer.editRenderOptions = value;
                editOptions = value;
            }
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        public RenderOptionsFields renderOptions
        {
            get
            {
                return taskRenderer == null ? оptions : taskRenderer.renderOptions;
            }
            set
            {
                if (taskRenderer != null)
                    taskRenderer.renderOptions = value;
                оptions = value;
                //pIndex = 0;
                //Points[0] = оptions.a;
                //Points[1] = оptions.b;
                this.Render();
            }
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        /// 
        public ColorSchemeClouds colorScheme
        {
            get
            {
                return taskRenderer == null ? colors : taskRenderer.colorScheme;
            }
            set
            {
                if (taskRenderer != null)
                    taskRenderer.colorScheme = value;
                colors = value;
            }
        }
        /// <summary>
        /// Инициализировать графический буфер (должен вызываться в событии загрузки форм).
        /// </summary>
        public void Initialize()
        {
            zoom.Initialize(this.ClientRectangle);
            InitializeBuffer();
            initialized = true;
            this.Invalidate();
        }
        /// <summary>
        /// Обновляет отображаемые входные данные.
        /// </summary>
        public void SetData(IClouds clouds)
        {
            RectangleWorld World;
            this.clouds = clouds;
            taskRenderer = new TaskRendererClouds(clouds);
            // Установить масштаб для новых данных
            World = CloudsUtils.GetWorld(clouds);

            bool flagUpdate = оptions.ckScaleUpdate;
            // При первом запуске flagUpdate всегда true
            if (startFlag == false)
            {
                startFlag = true;
                flagUpdate = true;
            }
            zoom.Initialize(this.ClientRectangle, World, flagUpdate);
            initialized = true;
        }

        /// <summary>
        /// Установить регион
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void SetRegion(PointF a, PointF b)
        {
            bool flagUpdate = оptions.ckScaleUpdate;
            // При первом запуске flagUpdate всегда true
            if (startFlag == false)
            {
                startFlag = true;
                flagUpdate = true;
            }
            RectangleWorld world = new RectangleWorld(a, b);
            zoom.Initialize(this.ClientRectangle, world, flagUpdate);
            initialized = true;
        }
        /// <summary>
        /// Увеличьте масштаб до указанного места.
        /// </summary>
        /// <param name="location">Фокус увеличения.</param>
        /// <param name="delta">Указывает, увеличивать или уменьшать масштаб.</param>
        public void ZoomWheel(float x, float y, int delta)
        {
            if (!initialized) return;
            if (zoom.ZoomUpdate(delta, x, y))
            {
                // Перерисовать
                this.Render();
            }
        }
        public void ZoomMax(float x, float y)
        {
            if (!initialized) return;
            if (zoom.ZoomMax(x, y))
            {
                // Перерисовать
                this.Render();
            }
        }
        /// <summary>
        /// Обновите графический буфер и увеличьте масштаб после изменения размера.
        /// </summary>
        public void HandleResize()
        {
            if (clouds == null) return;
            zoom.Initialize(this.ClientRectangle, CloudsUtils.GetWorld(clouds));
            InitializeBuffer();
        }
        /// <summary>
        /// Инициализация буффера
        /// </summary>
        private void InitializeBuffer()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                if (buffer != null)
                {
                    if (this.ClientRectangle == buffer.Graphics.VisibleClipBounds)
                    {
                        this.Invalidate();
                        // Границы не изменились. Вероятно, мы просто восстановили 
                        // окно из свернутого состояния.
                        return;
                    }
                    buffer.Dispose();
                }
                buffer = context.Allocate(Graphics.FromHwnd(this.Handle), this.ClientRectangle);
                if (initialized)
                {
                    this.Render();
                }
            }
        }
        private void Render()
        {
            coordinate = String.Empty;
            if (buffer == null)
            {
                return;
            }
            Graphics g = buffer.Graphics;
            g.Clear(this.BackColor);
            if (!initialized || clouds == null)
            {
                return;
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (taskRenderer != null)
            {
                taskRenderer.Render(g, zoom);
                AreaRender(g, zoom);
            }
            this.Invalidate();
        }
        #region Переопределения управляющих событий
        /// <summary>
        /// Перегрузка события отрисовки изображения
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (!initialized)
            {
                // отрисовка облака
                base.OnPaint(pe);
                return;
            }
            // отрисовка данных
            buffer.Render();

            // отрисовка координат мыши
            Graphics g = pe.Graphics;
            if (!String.IsNullOrEmpty(coordinate))
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.DrawString(coordinate, colors.FontReper, colors.BrushTextReper, this.Width / 2, 10);
            }

            switch (editRenderOptions.editState)
            {
                case EditState.NoState:
                    break;
                case EditState.Contur:
                    // отрисовка линий полигона
                    for (int i = 0; i < conturs.Count; i++)
                    {
                        var pt = new PointF((float)conturs[i].x, (float)conturs[i].y);
                        zoom.WorldToScreen(ref pt);
                        g.FillEllipse(colorScheme.BrushTextNodes, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
                        if (i > 0)
                        {
                            var pb = new PointF((float)conturs[i - 1].x, (float)conturs[i - 1].y);
                            zoom.WorldToScreen(ref pb);
                            g.DrawLine(colorScheme.PenCounturLine, pt, pb);
                        }
                        if (i > 1 && i == conturs.Count - 1)
                        {
                            var pa = new PointF((float)conturs[0].x, (float)conturs[0].y);
                            zoom.WorldToScreen(ref pa);
                            g.DrawLine(colorScheme.PenCounturLine, pt, pa);
                        }
                    }
                    break;
                case EditState.BeLine:
                    {
                        // отрисовка линий сглаживания
                        for (int i = 0; i < sLines.Count; i++)
                        {
                            var pA = new PointF((float)sLines[i].A.X, (float)sLines[i].A.Y);
                            var pB = new PointF((float)sLines[i].B.X, (float)sLines[i].B.Y);
                            zoom.WorldToScreen(ref pA);
                            zoom.WorldToScreen(ref pB);
                            g.FillEllipse(colorScheme.BrushTextNodes, pA.X - 1.5f, pA.Y - 1.5f, 3, 3);
                            g.FillEllipse(colorScheme.BrushTextNodes, pB.X - 1.5f, pB.Y - 1.5f, 3, 3);
                            Pen pen;
                            if (sLines[i].Link == false)
                            {
                                if (sLines[i].Selected == 1)
                                    pen = colorScheme.PenSelectCounturLine;
                                else
                                    pen = colorScheme.PenCounturLine;
                            }
                            else
                            {
                                if (sLines[i].Selected == 1)
                                    pen = colorScheme.PenSelectLinkCounturLine;
                                else
                                    pen = colorScheme.PenLinkCounturLine;
                            }
                            g.DrawLine(pen, pA, pB);
                        }
                        if (start != null)
                        {
                            var pA = new PointF((float)start.X, (float)start.Y);
                            zoom.WorldToScreen(ref pA);
                            g.FillEllipse(colorScheme.BrushTextNodes, pA.X - 1.5f, pA.Y - 1.5f, 3, 3);
                        }
                    }
                    break;
                case EditState.CrossLine:
                    {
                        // отрисовка линий сглаживания
                        if (_crossLine != null)
                        {
                            var pA = new PointF((float)_crossLine.A.X, (float)_crossLine.A.Y);
                            var pB = new PointF((float)_crossLine.B.X, (float)_crossLine.B.Y);
                            zoom.WorldToScreen(ref pA);
                            zoom.WorldToScreen(ref pB);
                            g.FillEllipse(colorScheme.BrushTextNodes, pA.X - 1.5f, pA.Y - 1.5f, 3, 3);
                            g.FillEllipse(colorScheme.BrushTextNodes, pB.X - 1.5f, pB.Y - 1.5f, 3, 3);
                            Pen pen = colorScheme.PenSelectCounturLine;
                            g.DrawLine(pen, pA, pB);
                        }
                        if (start != null)
                        {
                            var pA = new PointF((float)start.X, (float)start.Y);
                            zoom.WorldToScreen(ref pA);
                            g.FillEllipse(colorScheme.BrushTextNodes, pA.X - 1.5f, pA.Y - 1.5f, 3, 3);
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Отрисовка объектов
        /// </summary>
        private void AreaRender(Graphics g, WorldScaler zoom)
        {
            if (Area == null) return;
            // ----------------------------------------------------
            //  Render
            // ----------------------------------------------------
            //// контура
            Size s = new Size(6, 6);
            Size sp = new Size(10, 10);
            // Отрисовка фигур
            foreach (var f in Area.Figures)
            {
                List<IMPoint> points = f.Points;
                // Отричовка узлов
                for (int i = 0; i < points.Count; i++)
                {
                    PointF pt = (HPoint) points[i].Point;
                    zoom.WorldToScreen(ref pt);
                    
                    if (points[i].Status == FigureStatus.UnselectedShape)
                    {
                        pt.X -= s.Width/2; pt.Y -= s.Height / 2;
                        g.FillEllipse(colorScheme.BrushNodeContur, new RectangleF(pt, s));
                    }
                    else
                    {
                        pt.X -= sp.Width / 2; pt.Y -= sp.Height / 2;
                        g.FillEllipse(colorScheme.BrushNodeFocusContur, new RectangleF(pt, sp));
                    }
                }
                List<IMSegment> segs = f.Segments;
                for (int i = 0; i < segs.Count; i++)
                {
                    // 1 узел
                    PointF pa = (HPoint) segs[i].pointA.Point;
                    zoom.WorldToScreen(ref pa);
                    // 2 узел
                    PointF pb = (HPoint)segs[i].pointB.Point;
                    zoom.WorldToScreen(ref pb);
                    if (segs[i].Status == FigureStatus.UnselectedShape)
                        g.DrawLine(colorScheme.PenSegmentLine, pa, pb);
                    else
                        g.DrawLine(colorScheme.PenSegmentFocusLine, pa, pb);
                }
            }

            //IMBoundary bm = Area.GetSelection();
            //// Отрисовка выделенных границ
            //foreach (var f in Area.Figures)
            //{
            //    List<IMPoint> points = f.Points;
            //    List<IMSegment> segments = f.Segments;
            //    for (int i = 0; i < segments.Count; i++)
            //    {
            //        Pen pg = penOrd;
            //        IMSegment segment = segments[i];
            //        if (bm.SegmentIndex.Contains(i) == true && bm.FiguraName == f.Name)
            //        {
            //            pg = penMark;
            //        }
            //        // 1 узел
            //        PointF pt = points[segment.nodeA].Point;
            //        zoom.WorldToScreen(ref pt);
            //        // 2 узел
            //        PointF ps = points[segment.nodeB].Point;
            //        zoom.WorldToScreen(ref ps);
            //        // сегмент
            //        g.DrawLine(pg, pt, ps);
            //        pt.X -= 3; pt.Y -= 3;
            //        if (points[i].Status == FigureStatus.UnselectedShape)
            //            g.DrawEllipse(penNodOrd, new RectangleF(pt, sp));
            //        else
            //            g.DrawEllipse(penNodSel, new RectangleF(pt, sp));
            //        ps.X -= 3; ps.Y -= 3;
            //        if (points[(i + 1) % points.Count].Status == FigureStatus.UnselectedShape)
            //            g.DrawEllipse(penNodOrd, new RectangleF(ps, sp));
            //        else
            //            g.DrawEllipse(penNodSel, new RectangleF(ps, sp));
            //    }
            //}

            // ----------------------------------------------------
            this.Invalidate();
        }



        protected override void OnMouseMove(MouseEventArgs e)
        {
            //if (оptions.showFilter == 0) // работа с динамическим фильтром
            //{
            //    if (setOne == true)
            //    {
            //        timer.Stop();
            //        PointF c = new PointF((float)e.X, (float)e.Y);
            //        zoom.ScreenToWorld(ref c);
            //        pointB = c;
            //        this.Invalidate();
            //        timer.Start();
            //    }
            //}
        }

        /// <summary>
        /// Вывод мировых кординат в точке щелчка мышки
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (!initialized) return;
            if (e.Button == MouseButtons.Right)
            {
                // сброс изображения с начальному масштабу
                zoom.ZoomReset();
                // отрисовка отображения
                this.Render();
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    timer.Stop();
                    PointF c = new PointF((float)e.X, (float)e.Y);
                    zoom.ScreenToWorld(ref c);
                    CloudKnot p = null;
                    

                    switch (editRenderOptions.editState)
                    {
                        case EditState.NoState:
                            break;
                        case EditState.Contur:
                            p = new CloudKnot(c.X, c.Y, AtrCK.CreateZerro(), 1);
                            conturs.Add(p);
                            break;
                        case EditState.BeLine:
                            if (start == null)
                            {
                                start = new CloudKnot(c.X, c.Y, AtrCK.CreateZerro(), 1);
                                p = start;
                            }
                            else
                            {
                                CloudKnot end = new CloudKnot(c.X, c.Y, AtrCK.CreateZerro(), 1);
                                sLines.Add(new HSmLine(start, end));
                                p = end;
                                start = null;
                                if (sendSListData != null)
                                    sendSListData(sLines);
                            }
                            break;
                        case EditState.CrossLine:
                            if (start == null)
                            {
                                start = new CloudKnot(c.X, c.Y, AtrCK.CreateZerro(), 1);
                                p = start;
                            }
                            else
                            {
                                CloudKnot end = new CloudKnot(c.X, c.Y, AtrCK.CreateZerro(), 1);
                                _crossLine = new CloudKnotLine(start, end);

                                p = end;
                                start = null;
                            }
                            break;

                    }
                    if (sendPintData != null && p != null)
                        sendPintData(p);
                    // строка для отрисовки в OnPaint
                    var nfi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
                    coordinate = String.Format("X:{0} Y:{1}", c.X.ToString(nfi), c.Y.ToString(nfi));
                    // координаты для определения окна
                    this.Invalidate();
                    timer.Start();
                }
            }
            base.OnMouseClick(e);
        }
        /// <summary>
        /// Загрузка линий сглаживания
        /// </summary>
        /// <param name="sl"></param>
        public void LoadSmLines(List<IHSmLine> sl)
        {
            sLines.Clear();
            sLines.AddRange(sl);
            start = null;
            if (sendSListData != null)
                sendSListData(sLines);
        }
        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        public void UpCloudKnot(CloudKnot p)
        {
            if (conturs.Count > 0)
            {
                conturs[conturs.Count - 1] = p;
                Invalidate();
            }
        }
        /// <summary>
        /// Программно добавить точку контура
        /// </summary>
        public void AddCloudKnotToContur(CloudKnot knot)
        {
            conturs.Add((CloudKnot)knot);
        }
        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        public void DelCloudKnot()
        {
            if (conturs.Count > 0)
            {
                conturs.Remove(conturs[conturs.Count - 1]);
                if (sendPintData != null && conturs.Count > 1)
                    sendPintData(conturs[conturs.Count - 1]);
                Invalidate();
            }
        }
        public void ClearCurContur()
        {
            conturs.Clear();
            Invalidate();
        }
        /// <summary>
        ///  Удаление последней линий сглаживания
        /// </summary>
        /// <param name="p"></param>
        public void DelSmLines()
        {
            if (sLines.Count > 0)
            {
                sLines.Remove(sLines[sLines.Count - 1]);
                if (sendSListData != null && sLines.Count >-1)
                    sendSListData(sLines);
                Invalidate();
            }
        }
        /// <summary>
        ///  Удаление последней линий сглаживания
        /// </summary>
        /// <param name="p"></param>
        public void DelCrossLine()
        {
            if (crossLine != null)
            {
                crossLine = null;
                Invalidate();
            }
        }
        /// <summary>
        /// Удаление линий сглаживания
        /// </summary>
        public void ClearSmLines()
        {
            sLines.Clear();
            sendSListData(sLines);
            Invalidate();
        }
        /// <summary>
        /// Максимальное увеличение
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            ZoomMax(((float)e.X) / this.Width, ((float)e.Y) / this.Height);
            base.OnMouseDoubleClick(e);
        }
        /// <summary>
        /// Рисует фон элемента управления.
        /// </summary>
        /// <param name="pevent"></param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Ничего не делать
            if (!initialized)
            {
                base.OnPaintBackground(pevent);
            }
        }
        #endregion


        #region Работа с контурами фигур и гнерация сетки
        public IMFigura GetFig(int idx)
        {
            return Area[idx];
        }
        public void SetStatusFig(int idx, FigureStatus status = FigureStatus.SelectedShape)
        {
            Area[idx].Status = status;
        }
        public void SetStatusFig(int idxFig, int idxPoint, FigureStatus status = FigureStatus.SelectedShape)
        {
            IMPoint p = Area[idxFig].GetPoint(idxPoint);
            p.Status = status;
        }
        public void AddBoundMark(int FID, int[] idx)
        {
            Area.AddBoundMark(FID, idx);
        }
        /// <summary>
        /// Установка индекса активной метки
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectIndex(int index)
        {
            Area.SetSelectIndex(index);
            this.Render();
        }
        /// <summary>
        /// Добавить фигуру
        /// </summary>
        /// <returns></returns>
        public List<string> AddFigs()
        {
            if (conturs.Count > 2)
            {
                int FID = Area.Count;
                string name = "контур" + FID.ToString();
                IMFigura fig = new Figura(name, FID, conturs);
                Area.Add(fig);
                Area.SetFigureStatus(FigureStatus.UnselectedShape);
                List<string> names = Area.Names;
                conturs.Clear();
                this.Render();
                return names;
            }
            return null;
        }
        /// <summary>
        /// Добавить фигуру
        /// </summary>
        /// <returns></returns>
        public List<string> AddFigs(IMFigura fig)
        {
            Area.Add(fig);
            Area.SetFigureStatus(FigureStatus.UnselectedShape);
            List<string> names = Area.Names;
            conturs.Clear();
            this.Render();
            return names;
        }
        public List<string> RemoveFig(int index)
        {
            List<string> names = new List<string>();
            if (index > -1 && index < Area.Count)
            {
                IMFigura fig = Area[index];
                if (fig.FType == FigureType.FigureContur)
                {
                    Area.Remove(fig);
                    if (Area.Count > 0)
                        Area[0].FType = FigureType.FigureContur;
                }
                else
                    Area.Remove(fig);
                Area.RemoveBoundMark(fig.Name);
                names = Area.Names;
                //points.Clear();
                this.Render();
            }
            return names;
        }

        /// <summary>
        /// Возвращение типа контура
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FigureType GetFType(int index)
        {
            IMFigura fig = Area[index];
            return fig.FType;
        }
        /// <summary>
        /// Установка типа контура
        /// </summary>
        public void SetFType(int index, FigureType ft)
        {
            IMFigura fig = Area[index];
            fig.FType = ft;
        }
        /// <summary>
        /// Установка атрибутов контура
        /// </summary>
        public void SetAtributes(int index, double ice, double ks)
        {
            IMFigura fig = Area[index];
            fig.ks = ks;
            fig.Ice = ice;
        }
        /// <summary>
        /// Удалить фигуры
        /// </summary>
        public void ClearFigs()
        {
            Area.Clear();
            Invalidate();
            this.Render();
        }
        /// <summary>
        /// Обновление узла
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="atr"></param>
        /// <param name="indexFig"></param>
        /// <param name="indexPoint"></param>
        public void UpDatePoint(Vector2 coord, double[] atr, int indexFig, int indexPoint)
        {
            IMFigura fig = Area[indexFig];
            for (int i = 0; i < fig.Count; i++)
            {
                IMPoint point = fig.GetPoint(i);
                point.Status = FigureStatus.UnselectedShape;
                fig.SetPoint(i, point);
            }
            IMPoint pp = fig.GetPoint(indexPoint);
            pp.Point = new CloudKnot(coord.X, coord.Y, atr, 1);
            //if (pp.Status == FigureStatus.SelectedShape)
            //    pp.Status = FigureStatus.UnselectedShape;
            //else
            pp.Status = FigureStatus.SelectedShape;
            // отрисовка отображения
            HandleResize();
        }

        /// <summary>
        /// Обновление активных позиций фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="PointsSelectedIndex"></param>
        /// <param name="SegmentSelectedIndex"></param>
        /// <param name="CountKnots"></param>
        /// <param name="Mark"></param>
        /// <param name="p"></param>
        //public void UpDateFig(int FigSelectedIndex, int PointsSelectedIndex, int SegmentSelectedIndex, 
        //    int CountKnots, int Marker, CloudKnot p)
        //{
        //    IMFigura fig = Area[FigSelectedIndex];
        //    // Обновление точки
        //    IMPoint pp = fig.GetPoint(PointsSelectedIndex);
        //    pp.Point = p;
        //    for (int i = 0; i < fig.Count; i++)
        //    {
        //        fig.Points[i].Status = FigureStatus.UnselectedShape;
        //        fig.Segments[i].Status = FigureStatus.UnselectedShape;
        //    }
        //    pp.Status = FigureStatus.SelectedShape;
        //    // Обновление сегмента
        //    IMSegment sg = fig.GetSegment(SegmentSelectedIndex);
        //    sg.CountKnots = CountKnots;
        //    sg.Marker = Marker;
        //    sg.Status = FigureStatus.SelectedShape;
        //    Invalidate();
        //    this.Render();
        //}
        /// <summary>
        /// Обновление точек фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="PointsSelectedIndex"></param>
        /// <param name="p"></param>
        public void UpDateFigPoint(int FigSelectedIndex, int PointsSelectedIndex, CloudKnot p)
        {
            IMFigura fig = Area[FigSelectedIndex];
            // Обновление точки
            IMPoint pp = fig.GetPoint(PointsSelectedIndex);
            pp.Point = p;
            for (int i = 0; i < fig.Count; i++)
                fig.Points[i].Status = FigureStatus.UnselectedShape;
            pp.Status = FigureStatus.SelectedShape;
            Invalidate();
            this.Render();
        }
        /// <summary>
        /// Обновление фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="SegmentSelectedIndex"></param>
        /// <param name="CountKnots">Количество вершин на сегменте</param>
        /// <param name="Marker">Маркер границы</param>
        public void UpDateFigSegment(int FigSelectedIndex, int SegmentSelectedIndex,int CountKnots, int Marker)
        {
            IMFigura fig = Area[FigSelectedIndex];
            // Обновление сегмента
            IMSegment sg = fig.GetSegment(SegmentSelectedIndex);
            for (int i = 0; i < fig.Count; i++)
                fig.Segments[i].Status = FigureStatus.UnselectedShape;
            sg.CountKnots = CountKnots;
            sg.Marker = Marker;
            sg.Status = FigureStatus.SelectedShape;
            Invalidate();
            this.Render();
        }

        /// <summary>
        /// Групповое обновление количества узлов на сегментах
        /// </summary>
        public void UpDateFigure(int FigSelectedIndex, int CountKnots)
        {
            IMFigura fig = Area[FigSelectedIndex];
            for (int i = 0; i < fig.Count; i++)
            {
                IMSegment sg = fig.GetSegment(i);
                sg.CountKnots = CountKnots;
            }
        }
        /// <summary>
        /// Выделение узла при отрисовке
        /// </summary>
        /// <param name="FigSelectedIndex">номер фигуры</param>
        /// <param name="PointsSelectedIndex">номер узла</param>
        public void SelectKnot(int FigSelectedIndex, int PointsSelectedIndex)
        {
            IMFigura fig = Area[FigSelectedIndex];
            for (int i = 0; i < fig.Count; i++)
                fig.Points[i].Status = FigureStatus.UnselectedShape;
            IMPoint pf = fig.Points[PointsSelectedIndex];
            // Обновление точки
            pf.Status = FigureStatus.SelectedShape;
            Invalidate();
            this.Render();
        }
        /// <summary>
        /// Выделение сегмента при отрисовке
        /// </summary>
        /// <param name="FigSelectedIndex">номер фигуры</param>
        /// <param name="PointsSelectedIndex">номер сегмента</param>
        public void SelectSegment(int FigSelectedIndex, int SegmentSelectedIndex)
        {
            IMFigura fig = Area[FigSelectedIndex];
            for (int i = 0; i < fig.Count; i++)
                fig.Segments[i].Status = FigureStatus.UnselectedShape;
            IMSegment pf = fig.Segments[SegmentSelectedIndex];
            // Обновление точки
            pf.Status = FigureStatus.SelectedShape;
            Invalidate();
            this.Render();
        }
        /// <summary>
        /// Выбор активной линии сгласживания
        /// </summary>
        public double SelectSLines(int idx, ref int Count)
        {
            for (int i = 0; i < sLines.Count; i++)
                sLines[i].Selected = 0;
            sLines[idx].Selected = 1;
            Count = sLines[idx].Count;
            Invalidate();
            this.Render();
            return sLines[idx].Length();
        }
        public void UpDateSLines(int idx, int Count)
        {
            sLines[idx].Count = Count;
        }
        //public void ClearNewFig()
        //{
        //    conturs.Clear();
        //    // отрисовка отображения
        //    this.Render();
        //}
        //public void SetRegion(PointF a, PointF b)
        //{
        //    Area.SetRegion(a, b);
        //    HandleResize();
        //}
        //public RectangleWorld GetRegion()
        //{
        //    return Area.GetRegion();
        //}
        #endregion
    }
}

