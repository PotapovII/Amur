namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using SDI = System.Drawing.Imaging;
    using SDT = System.Drawing.Text;


    using System.Drawing.Imaging;
    using System.IO;
    /// <summary>
    /// Класс текстура используется для создания спрайтов
    /// и (или) печати текста
    /// </summary>
    class Texture2D : IDisposable
    {
        /// <summary>
        /// создание bitmap 1х1 пиксель
        /// </summary>
        protected  Bitmap bmp = new Bitmap(1, 1);
        /// <summary>
        /// id текстуры
        /// </summary>
        protected int ID;
        /// <summary>
        /// ширина текстуры
        /// </summary>
        public int W { get; }
        /// <summary>
        /// высота текстуры
        /// </summary>
        public int H { get; }
        /// <summary>
        /// id буффера текстуры
        /// </summary>
        protected int CoordinateID { get; set; }
        /// <summary>
        /// содержит текстурные координаты
        /// </summary>
        protected float[] Coordinate { get; set; }
        /// <summary>
        /// Область bmp
        /// </summary>
        protected Rectangle rectangle;
        #region Поля для работы с текстом
        /// <summary>
        /// Контекст для рисования в bmp
        /// </summary>
        protected Graphics graphics = null;
        #endregion 
        /// <summary>
        /// если существует файл, то загружаем его в объект bitmap
        /// </summary>
        /// <param name="fileName"></param>
        public Texture2D(string fileName)
        {
            if (File.Exists(fileName))
            {
                // загрузка
                bmp = (Bitmap)Image.FromFile(fileName);
                // Поворачивает, зеркально отражает, либо поворачивает и зеркально отражает объект
                // bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            else
            {
                Console.WriteLine("Ошибка! Файл текстуры " + fileName + "не найден.");
                return;
            }
            W = bmp.Width;
            H = bmp.Height;
            rectangle = new Rectangle(0, 0, W, H);
            graphics = null;
            // блокировка растрового изображения в системной памяти,
            // для его изменения программным способом
            BitmapData data = bmp.LockBits(rectangle,
                ImageLockMode.ReadOnly, SDI.PixelFormat.Format32bppArgb);
            // cоздание текстурного буфера
            CreateTexture2D(data);
            // останавливаем блокировку БМП date
            bmp.UnlockBits(data);
        }

        public Texture2D(int width, int height)
        {
            if (GraphicsContext.CurrentContext == null)
                throw new InvalidOperationException("GraphicsContext не обнаружен");
            bmp = new Bitmap(width, height, SDI.PixelFormat.Format32bppArgb);
            W = bmp.Width;
            H = bmp.Height;
            rectangle = new Rectangle(0, 0, W, H);
            graphics = Graphics.FromImage(bmp);
            BitmapData data = bmp.LockBits(rectangle,
                ImageLockMode.ReadOnly, SDI.PixelFormat.Format32bppArgb);
            // cоздание текстурного буфера
            CreateTexture2D(data);
            // останавливаем блокировку БМП date
            bmp.UnlockBits(data);
        }
        /// <summary>
        /// Создание текстурного буффера
        /// </summary>
        /// <param name="data"></param>
        public void CreateTexture2D(BitmapData data)
        {
            #region Создание текстурного буфера
            // генерация ID текстурного буффера
            ID = GL.GenTexture();
            // привязка ID к типу текстуры
            GL.BindTexture(TextureTarget.Texture2D, ID);
            // определяем параметры текстуры при ее отображении
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (uint)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (uint)TextureMinFilter.Linear);
            IntPtr intPtr = IntPtr.Zero;
            if (data != null)
                intPtr = data.Scan0;
            // копируем данные о текстуре в память видеокарты
            // создает текстуру одного определенного уровня детализации
            // и воспринимает только изображения,
            // размер которых кратен степени двойки 512x512
            GL.TexImage2D(TextureTarget.Texture2D, // target
                0, // lavel - уровень детализации.
                   // Нам нужно исходное изображение,
                   // поэтому уровень детализации - ноль
                PixelInternalFormat.Rgba, // components
                data.Width,
                data.Height,
                0, // border - гарницы не будет,
                   // поэтому значение этого параметра - ноль
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,  // type
                intPtr); // pixels
            // завершаем работу с буффером текстуры
            GL.BindTexture(TextureTarget.Texture2D, 0);
            
            #endregion 
            #region Создание вершинного буфера vbo для координат текстуры
            // указываем текстурные координаты
            Coordinate = new[]
            {
                0f, 1f, // левая верхняя координата
                1f, 1f, // правая верхняя координата
                1f, 0f, // правая нижняя координата
                0f, 0f // левая нижняя координата
            };
            // генерируем BufferID vbo для массива координат текстуры
            CoordinateID = GL.GenBuffer();
            // монтруем BufferID как массив
            GL.BindBuffer(BufferTarget.ArrayBuffer, CoordinateID);
            // копируем буферезируемый массив в видеопамять
            GL.BufferData(BufferTarget.ArrayBuffer,
                Coordinate.Length * sizeof(float),
                Coordinate, BufferUsageHint.StaticDraw);
            // отключаем работу с vbo буфером
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            #endregion
        }
        
        /// <summary>
        /// Метод Bind указывает на то, что мы хотим 
        /// использовать текстуру с ID 
        /// </summary>
        public void Bind()
        {
            // монтируем текстуру
            GL.BindTexture(TextureTarget.Texture2D, ID);
            // монтируем vbo текстурных координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, CoordinateID);
            // описываем формат массива текстурных координат.
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);
        }
        /// <summary>
        /// Функция UnBind биндит текущую текстуру с 
        /// нулевым параметром - отключает ее
        /// </summary>
        public void Unbind()
        {
            // отключаем буффер текстуры
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // отключаем буффер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        /// <summary>
        /// очищаем память
        /// </summary>
        public void Dispose()
        {
            // удаляем буффер текстур
            GL.DeleteBuffer(CoordinateID);
            // удаляем текстуру
            GL.DeleteTexture(ID);
            if (bmp != null)
                bmp.Dispose();
            if(graphics!=null)
                graphics.Dispose();
        }
        #region Методы для работы с текстом
        /// <summary>
        /// Заливка образа цветом color
        /// </summary>
        /// <param name="color"></param>
        public void Clear(Color color)
        {
            graphics.Clear(color);
        }
        /// <summary>
        /// Выводит строку текта text в точке point 
        /// растрового образе, используя фонт font и 
        /// цвета brush. Начало координат растрового 
        /// образа находится в его левом верхнем углу 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="point"></param>
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            graphics.DrawString(text, font, brush, point);
        }
        public void UpdateTextPrinter()
        {
            // создаем данные для текстуры на основе растровых данных,
            // содержащихся в bmp
            BitmapData data = bmp.LockBits(rectangle,
                SDI.ImageLockMode.ReadOnly,
                SDI.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, ID);
            // копируем данные в видеопамять
            GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                rectangle.X, rectangle.Y, rectangle.Width, 
                rectangle.Height,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
                PixelType.UnsignedByte, data.Scan0);
            // Освобождаем память, занимаемую data
            bmp.UnlockBits(data);
            rectangle = Rectangle.Empty;
        }
        #endregion
    }
}
