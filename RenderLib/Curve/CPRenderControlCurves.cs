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
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using CommonLib.Areas;
    using GeometryLib.World;
    using MeshLib;
    /// <summary>
    /// Визуализирует сетку с помощью GDI.
    /// </summary>
    public class CPRenderControlCurves : Control, IProxyRendererCurves
    {
        RectangleWorld World = new RectangleWorld();
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
        private WorldScaler zoom;
        /// <summary>
        /// Сохраняет данные задачи в удобной для рендеринга структуре данных.
        /// </summary>
        private GraphicsData data;
        /// <summary>
        /// Класс методов для отрисовки данных
        /// </summary>
        private TaskRendererCurves taskRenderer;
        /// <summary>
        /// Настройка цветовой схемы изображения
        /// </summary>
        private ColorScheme colors = new ColorScheme();
        /// <summary>
        /// Опции отрисовки 
        /// </summary>
        private RenderOptionsCurves оptions = new RenderOptionsCurves();
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
        public CPRenderControlCurves()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.BackColor = colors.Background;

            this.zoom = new WorldScaler(true);
            this.context = new BufferedGraphicsContext();
            this.data = new GraphicsData();

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
        public RenderOptionsCurves renderOptions
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
                this.Render();
            }
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        /// 
        public ColorScheme colorScheme
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
        public void SetData(GraphicsData data)
        {
            this.data = data;
            if (taskRenderer == null)
                taskRenderer = new TaskRendererCurves(data);
            else
                taskRenderer.SetTaskRendererCurves(data);
            // Установить масштаб для новых данных
            World = data.GetRegion(оptions.coordInv);
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
        public void SetRegion(PointF a, PointF b)
        {
            World = new RectangleWorld(a, b);
            InitializeBuffer();
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
            if (data == null)
                return;
            zoom.Initialize(this.ClientRectangle, World);
            InitializeBuffer();
        }
        /// <summary>
        /// Изменение масштаба при выбоне новой группы кривых 
        /// </summary>
        public void ReScale()
        {
            if (initialized == false)
                return;
            // Получение нового масштаба при выбоне новой группы кривых
            RectangleWorld World = data.GetRegion();
            //this.Render();
            zoom.Initialize(this.ClientRectangle, World, оptions.ckScaleUpdate);
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
            if (!initialized || data == null)
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
                g.DrawString(coordinate, colors.FontReper, colors.BrushPoint, this.Width / 2, 10);
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

