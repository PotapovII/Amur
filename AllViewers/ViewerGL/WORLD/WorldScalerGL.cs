//namespace ViewerGL
//{
//    //---------------------------------------------------------------------------
//    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//    //                         проектировщик:
//    //                           Потапов И.И.
//    //---------------------------------------------------------------------------
//    //                 кодировка : 21.12.2020 Потапов И.И.
//    //---------------------------------------------------------------------------
//    using System.Drawing;
//    using OpenTK.Graphics.OpenGL;
//    /// <summary>
//    /// ОО: Управляет преобразованием мировых координат в экраные (2D)
//    /// </summary>
//    public class WorldScalerGL
//    {
//        /// <summary>
//        /// Экранный прямоугольник
//        /// </summary>
//        WorldRectangle Screen;
//        /// <summary>
//        /// Текущий видовой экран (видимое избражение). 
//        /// - новое окно просмотра
//        /// </summary>
//        WorldRectangle viewport { get; set; } = new WorldRectangle(0,0,1,1);
//        public WorldRectangle Viewport
//        {
//            get => viewport;
//            set
//            {
//                //WorldRectangle old = viewport;
//                viewport = value;
//                //if (float.IsNaN(viewport.WorldLeft) == true)
//                //    viewport.WorldLeft = old.WorldLeft;
//                //if (float.IsNaN(viewport.WorldBottom) == true)
//                //    viewport.WorldBottom = old.Y;
//            }
//        }
//        /// <summary>
//        /// Прямоугольник для мировых координат
//        /// </summary>
//        WorldRectangle World { get; set; }
//        /// <summary>
//        /// Добавление полей сдвига рисунка от стенок (5%)
//        /// </summary>
//        float shift = 0.10f;
//        /// <summary>
//        /// Возвращает уровень масштабирования.
//        /// </summary>
//        public int Level { get; private set; }
//        /// <summary>
//        /// Инверсия:
//        /// Координаты экрана Windows по оси Y перевернуты, 
//        /// поэтому для параметра InverY необходимо установить 
//        /// значение true.
//        /// </summary>
//        bool invertY = false;
//        public WorldScalerGL() : this(false) { }
//        /// <summary>
//        /// Инверсия вертикальной координаты
//        /// </summary>
//        /// <param name="invertY"></param>
//        public WorldScalerGL(bool invertY)
//        {
//            Level = -1;
//            this.invertY = invertY;
//        }
//        /// <summary>
//        /// Инициализация экраном
//        /// </summary>
//        /// <param name="screen"></param>
//        public void Initialize(WorldRectangle screen)
//        {
//            this.Level = 1;
//            this.Screen = screen;
//            this.Viewport = screen;
//            this.World = screen;
//        }
//        /// <summary>
//        /// Инициализируйте проекцию.
//        /// </summary>
//        /// <param name="screen">Экранный прямоугольник</param>
//        /// <param name="world">Мир, который нужно преобразовать в экранные координаты.</param>
//        public void Initialize(WorldRectangle screen, WorldRectangle world)
//        {
//            float WorldWidth = world.WorldRight - world.WorldLeft;
//            float WorldHeight = world.WorldTop - world.WorldBottom;
//            //// Добавление полей сдвига рисунка от стенок (5%)
//            float shift = 0.1f;
//            float worldMargin = (WorldWidth < WorldHeight) ? WorldHeight * shift : WorldWidth * shift;
//            //// отношение сторон экрана
//            float screenRatio = screen.Width / screen.Height; 
//            //// отношение сторон мира
//            float worldRatio = WorldWidth / WorldHeight;
//            float scale; // масштаб заивист от отношений сторон экрана и сторон мира
//            if (screenRatio > worldRatio)
//                scale = (WorldHeight + worldMargin) / screen.Height;
//            else
//                scale = (WorldWidth + worldMargin) / screen.Width;
//            // центр мира  
//            float centerX = world.WorldLeft + WorldWidth / 2;
//            float centerY = world.WorldBottom + WorldHeight / 2;
//            // расчет текущего видового экрана (видимое избражение) для мира.
//            float ViewportLeft = centerX - screen.Width * scale / 2;
//            float ViewportRight = centerY + screen.Width * scale / 2;
//            float ViewportBottom = centerY - screen.Height * scale / 2;
//            float ViewportTop = centerY + screen.Height * scale / 2;
//            float near = 0.0f, far = 5.0f;
//            GL.Ortho(ViewportLeft, ViewportRight, ViewportBottom, ViewportTop, near, far);
//            this.World = this.Viewport;
//            this.Level = 1;
//        }

//        //public void OrthoViewport(float near = 0, float far = 5)
//        //{
//        //    GL.Ortho(Viewport.WorldLeft, Viewport.WorldRight, Viewport.WorldBottom, Viewport.WorldTop, near, far);
//        //}

//        /// <summary>
//        /// Увеличивайте или уменьшайте масштаб области просмотра.
//        /// </summary>
//        /// <param name="amount">Величина увеличения</param>
//        /// <param name="focusX">Относительное положение точки x</param>
//        /// <param name="focusY">Относительное положение точки y</param>
//        public bool ZoomUpdate(int amount, float focusX, float focusY)
//        {
//            float width, height;
//            if (invertY)
//                focusY = 1 - focusY;
//            if (amount > 0) // Приблизить
//            {
//                this.Level++;
//                if (this.Level > 50)
//                {
//                    this.Level = 50;
//                    return false;
//                }
//                width = Viewport.Width / 1.1f;
//                height = Viewport.Height / 1.1f;
//            }
//            else
//            {
//                this.Level--;
//                if (this.Level < 1)
//                {
//                    this.Level = 1;
//                    this.Viewport = this.World;
//                    return false;
//                }
//                width = Viewport.Width * 1.1f;
//                height = Viewport.Height * 1.1f;
//            }
//            // Текущий фокус на области просмотра
//            float x = Viewport.X + Viewport.Width * focusX;
//            float y = Viewport.Y + Viewport.Height * focusY;
//            // Новые позиции слева и сверху
//            x = x - width * focusX;
//            y = y - height * focusY;
//            // Проверьте, если за пределами мира
//            World.ToContains(ref x, ref y);
//            // Установить новое окно просмотра
//            this.Viewport = new WorldRectangle(x, y, x + width,y + height);
//            return true;
//        }
//        public bool ZoomMax(float focusX, float focusY)
//        {
//            if (invertY)
//                focusY = 1 - focusY;
//            this.Level = 50;
//            float width = Viewport.Width / 50f;
//            float height = Viewport.Height / 50f;
//            // Текущий фокус на области просмотра
//            float x = Viewport.X + Viewport.Width * focusX;
//            float y = Viewport.Y + Viewport.Height * focusY;
//            // Новые позиции слева и сверху
//            x = x - width * focusX;
//            y = y - height * focusY;
//            // Проверьте, если за пределами мира
//            World.ToContains(ref x, ref y);
//            // Установить новое окно просмотра
//            this.Viewport = new WorldRectangle(x, y, width, height);
//            return true;
//        }
//        public void ZoomReset()
//        {
//            this.Viewport = this.World;
//            this.Level = 1;
//        }
//        public bool ViewportContains(float x, float y)
//        {
//            return Viewport.ViewportContains(x, y);
//        }
//        /// <summary>
//        /// Репер системы координат
//        /// </summary>
//        /// <param name="p0"></param>
//        /// <param name="px"></param>
//        /// <param name="py"></param>
//        public void WorldReper(ref PointF p0, ref PointF px, ref PointF py)
//        {
//            if (invertY == true)
//            {
//                p0 = new PointF(10, Screen.Height - 10);
//                px = new PointF(Screen.Width - 10, Screen.Height - 10);
//                py = new PointF(10, 10);
//            }
//            else
//            {
//                p0 = new PointF(10, 10);
//                px = new PointF(Screen.Width - 10, 10);
//                py = new PointF(10, Screen.Height - 10);
//            }
//        }

//        /// <summary>
//        /// Конвертация точки из мировых координат в экранные
//        /// </summary>
//        /// <param name="pt">точка</param>
//        public void WorldToScreen(ref PointF pt)
//        {
//            pt.X = (pt.X - Viewport.X) / Viewport.Width * Screen.Width;
//            pt.Y = (1 - (pt.Y - Viewport.Y) / Viewport.Height) * Screen.Height;
//        }
//        /// <summary>
//        /// Конвертация точки из координат экранных в мировые
//        /// </summary>
//        /// <param name="pt"></param>
//        public void ScreenToWorld(ref PointF pt)
//        {
//            pt.X = Viewport.X + Viewport.Width * pt.X / Screen.Width;
//            pt.Y = Viewport.Y + Viewport.Height * (1 - pt.Y / Screen.Height);
//        }
//    }
//}
