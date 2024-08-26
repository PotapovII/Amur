namespace ViewerGL
{
    //---------------------------------------------------------------------------
    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
    //                         проектировщик:
    //                           Потапов И.И.
    //---------------------------------------------------------------------------
    //                 кодировка : 21.12.2020 Потапов И.И.
    //---------------------------------------------------------------------------
    using System.Drawing;
    /// <summary>
    /// ОО: Управляет преобразованием мировых координат в экраные (2D)
    /// </summary>
    public class WorldScaler
    {
        /// <summary>
        /// Экранный прямоугольник
        /// </summary>
        Rectangle Screen { get; set; }
        //Rectangle oldScreen = new Rectangle();
        /// <summary>
        /// Текущий видовой экран (видимое избражение). 
        /// - новое окно просмотра
        /// </summary>
        RectangleF viewport;
        public RectangleF Viewport
        {
            get => viewport;
            set
            {
                RectangleF old = viewport;
                viewport = value;
                if (float.IsNaN(viewport.X) == true)
                    viewport.X = old.X;
                if (float.IsNaN(viewport.Y) == true)
                    viewport.Y = old.Y;
            }
        }
        /// <summary>
        /// Прямоугольник для мировых координат
        /// </summary>
        RectangleF World { get; set; }
        /// <summary>
        /// Добавление полей сдвига рисунка от стенок (5%)
        /// </summary>
        float shift = 0.10f;
        /// <summary>
        /// Возвращает уровень масштабирования.
        /// </summary>
        public int Level { get; private set; }
        /// <summary>
        /// Инверсия:
        /// Координаты экрана Windows по оси Y перевернуты, 
        /// поэтому для параметра InverY необходимо установить 
        /// значение true.
        /// </summary>
        bool invertY = false;
        public WorldScaler() : this(false) { }
        /// <summary>
        /// Инверсия вертикальной координаты
        /// </summary>
        /// <param name="invertY"></param>
        public WorldScaler(bool invertY)
        {
            Level = -1;
            this.invertY = invertY;
        }
        /// <summary>
        /// Инициализация экраном
        /// </summary>
        /// <param name="screen"></param>
        public void Initialize(Rectangle screen)
        {
            this.Level = 1;
            this.Screen = screen;
            this.Viewport = screen;
            this.World = screen;
        }
        /// <summary>
        /// Инициализируйте проекцию.
        /// </summary>
        /// <param name="screen">Экранный прямоугольник</param>
        /// <param name="world">Мир, который нужно преобразовать в экранные координаты.</param>
        public void Initialize(Rectangle screen, RectangleWorld world)
        {
            this.Screen = screen;
            // Добавление полей сдвига рисунка от стенок (5%)
            float worldMargin = (world.Width < world.Height) ? world.Height * shift : world.Width * shift;
            #region Получите окно просмотра (изображение объекта отрисовки по центру экрана)
            // отношение сторон экрана
            float screenRatio = screen.Width / (float)screen.Height;
            // отношение сторон мира
            float worldRatio = world.Width / world.Height;
            float scale; // масштаб заивист от отношений сторон экрана и сторон мира
            if (screenRatio > worldRatio)
                scale = (world.Height + worldMargin) / screen.Height;
            else
                scale = (world.Width + worldMargin) / screen.Width;
            // центр мира  
            float centerX = world.Left + world.Width / 2;
            float centerY = world.Bottom + world.Height / 2;
            /// <summary>
            /// расчет текущего видового экрана (видимое избражение) для мира.
            /// </summary>
            this.Viewport = new RectangleF(centerX - screen.Width * scale / 2,
                                           centerY - screen.Height * scale / 2,
                                           screen.Width * scale,
                                           screen.Height * scale);
            #endregion
            this.World = this.Viewport;
            this.Level = 1;
        }

        /// <summary>
        /// Увеличивайте или уменьшайте масштаб области просмотра.
        /// </summary>
        /// <param name="amount">Величина увеличения</param>
        /// <param name="focusX">Относительное положение точки x</param>
        /// <param name="focusY">Относительное положение точки y</param>
        public bool ZoomUpdate(int amount, float focusX, float focusY)
        {
            float width, height;
            if (invertY)
                focusY = 1 - focusY;
            if (amount > 0) // Приблизить
            {
                this.Level++;
                if (this.Level > 50)
                {
                    this.Level = 50;
                    return false;
                }
                width = Viewport.Width / 1.1f;
                height = Viewport.Height / 1.1f;
            }
            else
            {
                this.Level--;
                if (this.Level < 1)
                {
                    this.Level = 1;
                    this.Viewport = this.World;
                    return false;
                }
                width = Viewport.Width * 1.1f;
                height = Viewport.Height * 1.1f;
            }
            // Текущий фокус на области просмотра
            float x = Viewport.X + Viewport.Width * focusX;
            float y = Viewport.Y + Viewport.Height * focusY;
            // Новые позиции слева и сверху
            x = x - width * focusX;
            y = y - height * focusY;
            // Проверьте, если за пределами мира
            if (x < World.X)
            {
                x = World.X;
            }
            else if (x + width > World.Right)
            {
                x = World.Right - width;
            }
            if (y < World.Y)
            {
                y = World.Y;
            }
            else if (y + height > World.Bottom)
            {
                y = World.Bottom - height;
            }
            // Установить новое окно просмотра
            this.Viewport = new RectangleF(x, y, width, height);
            return true;
        }
        public bool ZoomMax(float focusX, float focusY)
        {
            if (invertY)
                focusY = 1 - focusY;
            this.Level = 50;
            float width = Viewport.Width / 50f;
            float height = Viewport.Height / 50f;
            // Текущий фокус на области просмотра
            float x = Viewport.X + Viewport.Width * focusX;
            float y = Viewport.Y + Viewport.Height * focusY;
            // Новые позиции слева и сверху
            x = x - width * focusX;
            y = y - height * focusY;
            // Проверьте, если за пределами мира
            if (x < World.X)
                x = World.X;
            else
                if (x + width > World.Right)
                x = World.Right - width;

            if (y < World.Y)
                y = World.Y;
            else
                if (y + height > World.Bottom)
                y = World.Bottom - height;
            // Установить новое окно просмотра
            this.Viewport = new RectangleF(x, y, width, height);
            return true;
        }
        public void ZoomReset()
        {
            this.Viewport = this.World;
            this.Level = 1;
        }
        public bool ViewportContains(float x, float y)
        {
            return (x > Viewport.X && x < Viewport.Right
                && y > Viewport.Y && y < Viewport.Bottom);
        }
        /// <summary>
        /// Репер системы координат
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="px"></param>
        /// <param name="py"></param>
        public void WorldReper(ref PointF p0, ref PointF px, ref PointF py)
        {
            if (invertY == true)
            {
                p0 = new PointF(10, Screen.Height - 10);
                px = new PointF(Screen.Width - 10, Screen.Height - 10);
                py = new PointF(10, 10);
            }
            else
            {
                p0 = new PointF(10, 10);
                px = new PointF(Screen.Width - 10, 10);
                py = new PointF(10, Screen.Height - 10);
            }
        }

        /// <summary>
        /// Конвертация точки из мировых координат в экранные
        /// </summary>
        /// <param name="pt">точка</param>
        public void WorldToScreen(ref PointF pt)
        {
            pt.X = (pt.X - Viewport.X) / Viewport.Width * Screen.Width;
            pt.Y = (1 - (pt.Y - Viewport.Y) / Viewport.Height) * Screen.Height;
        }
        /// <summary>
        /// Конвертация точки из координат экранных в мировые
        /// </summary>
        /// <param name="pt"></param>
        public void ScreenToWorld(ref PointF pt)
        {
            pt.X = Viewport.X + Viewport.Width * pt.X / Screen.Width;
            pt.Y = Viewport.Y + Viewport.Height * (1 - pt.Y / Screen.Height);
        }

    }
}
