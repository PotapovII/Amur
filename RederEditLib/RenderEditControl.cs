//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 16.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderEditLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;

    using RenderLib;
    using GeometryLib.Areas;
    using CommonLib;
    using CommonLib.Geometry;
    using CommonLib.Areas;
    using GeometryLib.World;

    /// <summary>
    /// Визуализирует сетку с помощью GDI.
    /// </summary>
    public class RenderEditControl : Control
    {
        RectangleWorld World;
        /// <summary>
        /// данные по сетке и полям
        /// </summary>
        SavePointData data;
        /// <summary>
        /// Класс методов для отрисовки данных полей
        /// </summary>
        TaskRendererFields taskRendererFields;
        /// <summary>
        /// данные облака
        /// </summary>
        IClouds clouds;
        /// <summary>
        /// Класс методов для отрисовки данных облака
        /// </summary>
        TaskRendererClouds taskRendererClouds = null;
        ///// <summary>
        ///// Контуры фигур.
        ///// Сохраняет данные задачи в удобной для рендеринга структуре данных.
        ///// </summary>
        //protected MArea mArea = new MArea();
        //public MArea Area => mArea; 
        /// <summary>
        /// Список вводимых буфферных точеск
        /// </summary>
        private List<HPoint> points = new List<HPoint>();
        /// <summary>
        /// Опции герерации сетки в контуре
        /// </summary>
        private OptionsGenMesh optionsGenMesh = new OptionsGenMesh();
        /// <summary>
        /// Настройка цветовой схемы изображения
        /// </summary>
        private ColorScheme colors = new ColorScheme();
        /// <summary>
        /// Класс для работы в контрола режиме графического редактора
        /// </summary>
        private OptionsEdit options = new OptionsEdit();
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
        private WorldScaler zoom;
        /// <summary>
        /// Флаг установки данных в контрол и вызова метода Render при true
        /// </summary>
        private bool initialized = false;
        /// <summary>
        /// строка для координат
        /// </summary>
        private string coordinate = String.Empty;
        /// <summary>
        /// Таймер для автообновления
        /// </summary>
        Timer timer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref = "RenderControl" />.
        /// </summary>
        public RenderEditControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.BackColor = colors.Background;

            this.zoom = new WorldScaler(true);
            this.context = new BufferedGraphicsContext();
            //this.mArea = new MArea();

            this.timer = new Timer();
            this.timer.Interval = 3000;
            this.timer.Tick += (sender, e) =>
            {
                timer.Stop();
                coordinate = String.Empty;
                this.Invalidate();
            };
        }
        /// <summary>
        /// Определение размера "мира"
        /// </summary>
        public void WorldRef()
        {
            RectangleWorld worldD;
            if (data == null)
                worldD = new RectangleWorld();
            else
                worldD = data.World;
            RectangleWorld worldC;
            if (clouds == null)
                worldC = new RectangleWorld();
            else
                worldC = CloudsUtils.GetWorld(clouds);
            World = RectangleWorld.Extension(ref worldD, ref worldC);
            bool flagUpdate = true;
            zoom.Initialize(this.ClientRectangle, World, flagUpdate);
            initialized = true;
        }
        /// <summary>
        /// Обновляет отображаемые входные данные.
        /// </summary>
        public void SetData(IClouds clouds)
        {
            this.clouds = clouds;
            taskRendererClouds = new TaskRendererClouds(clouds);
            WorldRef();
        }
        /// <summary>
        /// Обновляет отображаемые входные данные.
        /// </summary>
        /// <summary>
        /// Обновляет отображаемые входные данные.
        /// </summary>
        public void SetData(SavePointData data)
        {
            this.data = data;
            taskRendererFields = new TaskRendererFields(data);
            WorldRef();
        }
        /// <summary>
        /// Класс для работы в контрола режиме графического редактора
        /// </summary>
        public OptionsEdit Options
        {
            get => options;
            set
            {
                options = value;
                this.Render();
            }
        }
        public OptionsGenMesh optionsMesh
        {
            get => optionsGenMesh;
            set => optionsGenMesh = value;
        }

        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        public RenderOptionsFields renderOptions
        {
            get
            {
                return taskRendererClouds.renderOptions;
            }
            set
            {
                if (taskRendererClouds != null)
                    taskRendererClouds.renderOptions = value;
                this.Render();
            }
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        /// 
        public ColorScheme colorScheme
        {
            get => colors;
            set
            {
                colors = value;
                this.Render();
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

        #region Работа с контурами фигур и гнерация сетки
        //public IMFigura GetFig(int idx)
        //{
        //    return mArea[idx];
        //}
        //public void SetStatusFig(int idx, FigureStatus status = FigureStatus.SelectedShape)
        //{
        //    mArea[idx].Status = status;
        //}
        //public void SetStatusFig(int idxFig, int idxPoint, FigureStatus status = FigureStatus.SelectedShape)
        //{
        //    IMPoint p = mArea[idxFig].GetPoint(idxPoint);
        //    p.Status = status;
        //}
        //public void AddBoundMark(int FID,int[] idx)
        //{
        //    mArea.AddBoundMark(FID, idx);
        //}

        ///// <summary>
        ///// Установка индекса активной метки
        ///// </summary>
        ///// <param name="index"></param>
        //public void SetSelectIndex(int index)
        //{
        //    mArea.SetSelectIndex(index);
        //    Render();
        //}


        ///// <summary>
        ///// Добавить фигуру
        ///// </summary>
        ///// <returns></returns>
        //public List<string> AddFigs()
        //{
        //    if (points.Count > 2)
        //    {
        //        int FID = mArea.Count;
        //        string name = "контур" + FID.ToString();
        //        IMFigura fig = new Figura(name, FID, points);
        //        mArea.Add(fig);
        //        mArea.SetFigureStatus(FigureStatus.UnselectedShape);
        //        List<string> names = mArea.Names;
        //        points.Clear();
        //        this.Render();
        //        return names;
        //    }
        //    return null;
        //}

        //public List<string> RemoveFig(int index)
        //{
        //    List<string> names = new List<string>();
        //    if (index > -1 && index < mArea.Count)
        //    {
        //        IMFigura fig = mArea[index];
        //        if(fig.FType == FigureType.FigureContur)
        //        {
        //            mArea.Remove(fig);
        //            if (mArea.Count>0)
        //                mArea[0].FType = FigureType.FigureContur;
        //        }
        //        else
        //            mArea.Remove(fig);
        //        mArea.RemoveBoundMark(fig.Name);
        //        names = mArea.Names;
        //        //points.Clear();
        //        this.Render();
        //    }
        //    return names;
        //}

        ///// <summary>
        ///// Возвращение типа контура
        ///// </summary>
        ///// <param name="index"></param>
        ///// <returns></returns>
        //public FigureType GetFType(int index)
        //{
        //    IMFigura fig = mArea[index];
        //    return fig.FType;
        //}
        ///// <summary>
        ///// Установка типа контура
        ///// </summary>
        //public void SetFType(int index, FigureType ft)
        //{
        //    IMFigura fig = mArea[index];
        //    fig.FType = ft;
        //}
        ///// <summary>
        ///// Удалить фигуры
        ///// </summary>
        //public void ClearFigs()
        //{
        //    mArea.Clear();
        //    Invalidate();
        //    this.Render();
        //}
        ///// <summary>
        ///// Обновление узла
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="indexFig"></param>
        ///// <param name="indexPoint"></param>
        //public void UpDatePoint(Vector2 coord, Vector2 atrib, int indexFig, int indexPoint)
        //{
        //    IMFigura fig = mArea[indexFig];
        //    for (int i = 0; i < fig.Count; i++)
        //    {
        //        IMPoint point = fig.GetPoint(i);
        //        point.Status = FigureStatus.UnselectedShape;
        //        fig.SetPoint(i, point);
        //    }
        //    IMPoint pp = fig.GetPoint(indexPoint);
        //    pp.Point = new CloudKnot();
        //    //if (pp.Status == FigureStatus.SelectedShape)
        //    //    pp.Status = FigureStatus.UnselectedShape;
        //    //else
        //        pp.Status = FigureStatus.SelectedShape;
        //    // отрисовка отображения
        //    HandleResize();
        //}
        public void ClearNewFig()
        {
            points.Clear();
            // отрисовка отображения
            this.Render();
        }
        //public void SetRegion(PointF a, PointF b)
        //{
        //    mArea.SetRegion(a, b);
        //    HandleResize();
        //}
        //public RectangleWorld GetRegion()
        //{
        //    return mArea.GetRegion();
        //}
        #endregion

        #region Работа масштабом и буфером отрисовки
        /// <summary>
        /// Обновите графический буфер и увеличьте масштаб после изменения размера.
        /// </summary>
        public void HandleResize()
        {
            //if (mArea.Count == 0)
            //    return;
        //    zoom.Initialize(this.ClientRectangle, mArea.GetRegion());
            InitializeBuffer();
            initialized = true;
            this.Invalidate();
        }
        /// <summary>
        /// Обновляет отображаемые входные данные.
        /// </summary>
        public void SetData(MArea mArea)
        {
            RectangleWorld World;
          //  this.mArea = mArea;
            // Установить масштаб для новых данных
            if (mArea == null)
                World = new RectangleWorld();
            else
                World = mArea.GetRegion();
            zoom.Initialize(this.ClientRectangle, World, options.ckScaleUpdate);
            initialized = true;
            this.Render();
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
        /// Изменение масштаба при выбоне новой группы кривых 
        /// </summary>
        public void ReScale()
        {
            if (initialized == false)
                return;
            // Получение нового масштаба при выбоне новой группы кривых
            //RectangleWorld World = mArea.GetRegion();
            //// this.Render();                             
            //zoom.Initialize(this.ClientRectangle, World, options.ckScaleUpdate);
            InitializeBuffer();
        }
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
        #endregion

        #region Переопределения управляющих событий
        /// <summary>
        /// Перегрузка события отрисовки изображения
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (!initialized)
            {
                base.OnPaint(pe);
                return;
            }
            // отрисовка буфера
            buffer.Render();
            // отрисовка данных
            this.Render();
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
                    var nfi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
                    PointF c = new PointF((float)e.X, (float)e.Y);
                    zoom.ScreenToWorld(ref c);
                    // строка для отрисовки в OnPaint
                    coordinate = String.Format("X:{0} Y:{1}", c.X.ToString(nfi), c.Y.ToString(nfi));
                    // координаты для определения 
                    points.Add(c);
                    this.Invalidate();
                    timer.Start();
                }
            }
            base.OnMouseClick(e);
        }

        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        public void DelLastKnot()
        {
            if (points.Count > 0)
            {
                timer.Stop();
                points.Remove(points[points.Count - 1]);
                //if (sendPintData != null && points.Count > 1)
                //    sendPintData(points[conturs.Count - 1]);
                Invalidate();
                this.Invalidate();
                timer.Start();
            }
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
        // узлы
        Pen penNodSel = new Pen(Brushes.Red, 3);
        Pen penNodOrd = new Pen(Brushes.Green, 3);
        Size s = new Size(4, 4);
        Size sp = new Size(6, 6);

        /// <summary>
        /// Отрисовка объектов
        /// </summary>
        private void Render()
        {
            Graphics g = buffer.Graphics;
            if (taskRendererClouds != null)
            {
                taskRendererClouds.Render(g, zoom);
            }
            if (buffer == null) return;
            g.Clear(this.BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (taskRendererClouds != null)
                taskRendererClouds.RenderCoordReper(zoom, g, colorScheme);
            // отрисовка координат мыши
            if (!String.IsNullOrEmpty(coordinate))
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.DrawString(coordinate, colors.FontReper, colors.BrushPoint, this.Width / 2, 10);
                //coordinate = String.Empty;
            }
            //if (!initialized || mArea == null) return;
            
            //// ----------------------------------------------------
            ////  Render
            //// ----------------------------------------------------
            //Pen p = new Pen(Brushes.Blue, 3);
            //// контура
            //Pen penOrd = new Pen(Brushes.Green, 3);
            //Pen penSel = new Pen(Brushes.Red, 3);
            //Pen penHide = new Pen(Brushes.Gray, 3);
            //Pen penMark = new Pen(Brushes.DeepPink, 3);
            //// сегменты выделение
            //Pen penSegSel = new Pen(Brushes.DodgerBlue, 3);
            //SolidBrush brush = new SolidBrush(Color.Red);
            //if (points.Count == 1)
            //{
            //    PointF pt = points[0];
            //    zoom.WorldToScreen(ref pt);
            //    pt.X -= 2; pt.Y -= 2;
            //    g.DrawEllipse(p, new RectangleF(pt,s));
            //}
            //if (points.Count==2)
            //{
            //    PointF pt = points[0];
            //    PointF ps = points[1];
            //    zoom.WorldToScreen(ref pt);
            //    zoom.WorldToScreen(ref ps);
            //    g.DrawLine(p, pt, ps);
            //    pt.X -= 2; pt.Y -= 2;
            //    g.DrawEllipse(p, new RectangleF(pt, s));
            //    ps.X -= 2; ps.Y -= 2;
            //    g.DrawEllipse(p, new RectangleF(ps, s));
            //}
            //if (points.Count >2)
            //{
            //    for(int i=0; i<points.Count; i++)
            //    {
            //        PointF pt = points[i];
            //        PointF ps = points[(i+1)%points.Count];
            //        zoom.WorldToScreen(ref pt);
            //        zoom.WorldToScreen(ref ps);
            //        g.DrawLine(p, pt, ps);
            //        pt.X -= 2; pt.Y -= 2;
            //        g.DrawEllipse(p, new RectangleF(pt, s));
            //        ps.X -= 2; ps.Y -= 2;
            //        g.DrawEllipse(p, new RectangleF(ps, s));
            //    }
            //}
            //// Отрисовка фигур
            //foreach (var f in mArea.Figures)
            //{
            //    List<IMPoint> points = f.Points;

            //    Pen pg = penHide;
            //    if (f.Status == FigureStatus.UnselectedShape)
            //        pg = penOrd;
            //    if (f.Status == FigureStatus.SelectedShape)
            //        pg = penSel;
            //    for (int i = 0; i < points.Count; i++)
            //    {
            //        //DrawPoint(g, points[i], ref pt);
            //        //DrawPoint(g, points[(i + 1) % points.Count], ref ps);
            //        // 1 узел
            //       PointF pt = (HPoint) points[i].Point;
            //        zoom.WorldToScreen(ref pt);
            //        // 2 узел
            //       PointF ps = (HPoint) points[(i + 1) % points.Count].Point;
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

            //IMBoundary bm = mArea.GetSelection();
            //// Отрисовка выделенных границ
            //foreach (var f in mArea.Figures)
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

            //        //DrawPoint(g, points[i], ref pt);
            //        //DrawPoint(g, points[(i + 1) % points.Count], ref ps);
            //        // 1 узел
            //        PointF pt = (HPoint)points[segment.nodeA].Point;
            //        zoom.WorldToScreen(ref pt);
            //        // 2 узел
            //        PointF ps = (HPoint) points[segment.nodeB].Point;
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
            //// ----------------------------------------------------
            this.Invalidate();
        }

        
        protected void DrawPoint(Graphics g, IMPoint Nod, ref PointF pt)
        {
            pt = (HPoint)Nod.Point;
            zoom.WorldToScreen(ref pt);
            
            pt.X -= 3; pt.Y -= 3;
            if (Nod.Status == FigureStatus.UnselectedShape)
                g.DrawEllipse(penNodOrd, new RectangleF(pt, sp));
            else
                g.DrawEllipse(penNodSel, new RectangleF(pt, sp));
        }
    }
}

