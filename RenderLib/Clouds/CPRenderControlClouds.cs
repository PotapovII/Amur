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

    using MeshLib;
    using CommonLib;
    using GeometryLib;
    using MeshLib.SaveData;
    using GeometryLib.Areas;
    using CommonLib.Areas;
    using GeometryLib.World;
    using CommonLib.Delegate;

    /// <summary>
    /// Визуализирует сетку с помощью GDI.
    /// </summary>
    public class CPRenderControlClouds : Control, IProxyRendererClouds
    {
        /// <summary>
        /// Точки фильтра
        /// </summary>
        public PointF[] Points { get; set; } = new PointF[2];
        /// <summary>
        /// индекс точки фильтра
        /// </summary>
        protected int pIndex { get; set; }
        /// <summary>
        /// Установка фильтра
        /// </summary>
        /// <param name="points"></param>
        public void SetTargetLine(PointF[] points)
        {
            оptions.a = points[0];
            оptions.b = points[1];
        }

        /// <summary>
        /// индекс точки фильтра
        /// </summary>
        protected bool setOne { get; set; } = false;
        PointF pointA = new PointF(0, 0);
        PointF pointB = new PointF(0, 0);

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
        /// Загрузка данных в вершине
        /// </summary>
        public SendData<CloudKnot> sendPintData { get; set; } = null;
                            
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
        public CPRenderControlClouds()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.BackColor = colors.Background;

            this.zoom = new WorldScaler(true);
            this.context = new BufferedGraphicsContext();
            this.clouds = new CloudRiverNods();
            this.pIndex = 0;


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
                pIndex = 0;
                Points[0] = оptions.a;
                Points[1] = оptions.b;
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
            if (!String.IsNullOrEmpty(coordinate))
            {
                Graphics g = pe.Graphics;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.DrawString(coordinate, colors.FontReper, colors.BrushTextReper, this.Width / 2, 10);
            }
            if (оptions.showFilter == 0) // работа с окном фильтрации
            {
                if (setOne == true || setOne == false && pointA != pointB)
                {
                    Graphics g = pe.Graphics;
                    PointF pa = pointA;
                    PointF pb = pointB;
                    zoom.WorldToScreen(ref pa);
                    zoom.WorldToScreen(ref pb);
                    g.FillEllipse(colorScheme.BrushNodeContur, pa.X - 1.5f, pa.Y - 1.5f, 3, 3);
                    g.FillEllipse(colorScheme.BrushNodeContur, pb.X - 1.5f, pb.Y - 1.5f, 3, 3);
                    PointF pet = new PointF(pb.X, pa.Y);
                    PointF pwb = new PointF(pa.X, pb.Y);

                    g.DrawLine(colorScheme.PenCounturLine, pa, pet);
                    g.DrawLine(colorScheme.PenCounturLine, pet, pb);
                    g.DrawLine(colorScheme.PenCounturLine, pb, pwb);
                    g.DrawLine(colorScheme.PenCounturLine, pwb, pa);
                }
            }
            if (оptions.showFilter == 1) // работа с контуром границы
            {
                Graphics g = pe.Graphics;
                for(int i=0; i<conturs.Count; i++)
                {
                    var pt = new PointF((float)conturs[i].x, (float)conturs[i].y);
                    zoom.WorldToScreen(ref pt);
                    g.FillEllipse(colorScheme.BrushNodeContur, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
                    if (i>0)
                    {
                        var pb = new PointF((float)conturs[i-1].x, (float)conturs[i-1].y);
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
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (оptions.showFilter == 0) // работа с динамическим фильтром
            {
                if (setOne == true)
                {
                    timer.Stop();
                    PointF c = new PointF((float)e.X, (float)e.Y);
                    zoom.ScreenToWorld(ref c);
                    pointB = c;
                    this.Invalidate();
                    timer.Start();
                }
            }
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
                    Points[pIndex] = c;
                    this.pIndex = (pIndex + 1) % 2;
                    // работа с динамическим фильтром
                    if (оptions.showFilter == 0)
                    {
                        if (setOne == false)
                        {
                            pointA = c;
                            pointB = c;
                        }
                        else
                        {
                            pointB = c;
                        }
                        setOne = !setOne;
                    }
                    if (оptions.showFilter == 1)
                    {
                        int mark = 1;
                        CloudKnot p = new CloudKnot(c.X, c.Y, new double[] { 0, 0, 0, 0 }, mark);
                        conturs.Add(p);
                        if (sendPintData != null)
                            sendPintData(p);
                     }
                    // строка для отрисовки в OnPaint
                    coordinate = String.Format("X:{0} Y:{1}", c.X.ToString(nfi), c.Y.ToString(nfi));
                    // координаты для определения окна
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
        public void UpCloudKnot(CloudKnot p)
        {
            if (conturs.Count > 0)
            {
                conturs[conturs.Count - 1] = p;
                Invalidate();
            }
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
    }
}

