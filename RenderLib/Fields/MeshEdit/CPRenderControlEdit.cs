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
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;

    using CommonLib.Areas;
    using GeometryLib.World;
    using CommonLib.Geometry;
    using CommonLib;

    /// <summary>
    /// Визуализирует сетку с помощью GDI.
    /// </summary>
    public class CPRenderControlEdit : Control, IProxyRendererEditMesh
    {
        /// <summary>
        /// Индекс состаяния работ
        /// </summary>
        public int IndexTask { get; set; } = 0;
        ///// <summary>
        ///// Точки створа
        ///// </summary>
        //public PointF[] Points { get => points; set => points = value; }
        ///// <summary>
        ///// Точки створа
        ///// </summary>
        //protected PointF[] points = new PointF[2];
        ///// <summary>
        ///// индекс точки стрвора
        ///// </summary>
        //protected int pIndex { get; set; }
        ///// <summary>
        ///// Установка створа
        ///// </summary>
        ///// <param name="points"></param>
        //public void SetTargetLine(PointF[] points)
        //{
        //    оptions.a = points[0];
        //    оptions.b = points[1];
        //}
        /// <summary>
        /// Список узлов добавленных в область
        /// </summary>
        public List<IHPoint> NewNodes { get => newNodes; }
        public List<IHPoint> newNodes = new List<IHPoint>();
        /// <summary>
        /// добавить узлед с координатами
        /// </summary>
        public void AddNod(IHPoint p)
        {
            NewNodes.Add(p);
        }
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
        SavePointData data = null;
        /// <summary>
        /// Облако узлов
        /// </summary>
        IClouds clouds = null;
        /// <summary>
        /// Класс методов для отрисовки данных облака
        /// </summary>
        TaskRendererClouds taskRendererClouds;
        /// <summary>
        /// Класс методов для отрисовки данных
        /// </summary>
        TaskRendererEdit taskRenderer;
        /// <summary>
        /// Настройка цветовой схемы изображения
        /// </summary>
        ColorSchemeEdit colors = new ColorSchemeEdit();
        /// <summary>
        /// Опции отрисовки 
        /// </summary>
        RenderOptionsEdit оptions = new RenderOptionsEdit();
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
        public CPRenderControlEdit()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);

            this.BackColor = colors.Background;

            this.zoom = new WorldScaler(true);
            this.context = new BufferedGraphicsContext();
            this.data = new SavePointData();
            //this.pIndex = 0;
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
        public RenderOptionsEdit renderOptions
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
                //points[0] = оptions.a;
                //points[1] = оptions.b;
                this.Render();
            }
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        /// 
        public ColorSchemeEdit colorScheme
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
        public void SetData(SavePointData data)
        {
            this.data = data;
            taskRenderer = new TaskRendererEdit(data);
            WorldRef();
            //RectangleWorld World;
            //this.data = data;
            //taskRenderer = new TaskRendererEdit(data);
            //// Установить масштаб для новых данных
            //if (data == null)
            //    World = new RectangleWorld();
            //else
            //    World = data.World;
            //bool flagUpdate = оptions.ckScaleUpdate;
            //// При первом запуске flagUpdate всегда true
            //if (startFlag == false)
            //{
            //    startFlag = true;
            //    flagUpdate = true;
            //}
            //zoom.Initialize(this.ClientRectangle, World, flagUpdate);
            //initialized = true;
        }


        /// <summary>
        /// Запись данных в списки компонента
        /// </summary>
        /// <param name="cloudData">Данные для отрисовки</param>
        public void SetData(IClouds clouds)
        {
            this.clouds = clouds;
            taskRendererClouds = new TaskRendererClouds(clouds);
            WorldRef();
        }

        /// <summary>
        /// Определение размера "мира"
        /// </summary>
        public RectangleWorld GetWorld()
        {
            if (data == null && clouds == null)
                return new RectangleWorld();
            else
            {
                if (data != null && clouds != null)
                {
                    RectangleWorld worldC = CloudsUtils.GetWorld(clouds);
                    return RectangleWorld.Extension(ref data.World, ref worldC);
                }
                if (clouds == null)
                    return data.World;
                else
                    return CloudsUtils.GetWorld(clouds);
            }
        }
        /// <summary>
        /// Определение размера "мира"
        /// </summary>
        public void WorldRef()
        {
            RectangleWorld World = GetWorld();
            bool flagUpdate = true;
            zoom.Initialize(this.ClientRectangle, World, flagUpdate);
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
            if (data == null) return;
            zoom.Initialize(this.ClientRectangle, GetWorld());
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
            if (taskRendererClouds != null)
            {
                taskRendererClouds.Render(g, zoom);
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
                g.DrawString(coordinate, colors.FontReper, colors.BrushTextKnot, this.Width / 2, 10);
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
                    switch(IndexTask)
                    {
                        case 0: // закладка задачи
                            break;
                        case 1: // закладка работы со створами
                            //Points[pIndex] = c;
                            //this.pIndex = (pIndex + 1) % 2;
                            break;
                        case 2: // закладка работы с сеткой

                            break;
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

