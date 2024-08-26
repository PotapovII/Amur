using System;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SDI = System.Drawing.Imaging;
using SDT = System.Drawing.Text;
namespace ViewerGL
{
    /// <summary>
    /// Класс вывода текста через текстуру
    /// </summary>
    public class TextPrinter : IDisposable
    {
        /// <summary>
        /// Носитель текста
        /// </summary>
        protected Bitmap bmp;
        /// <summary>
        /// Контекст для рисования в bmp
        /// </summary>
        protected Graphics graphics;
        /// <summary>
        /// id текстурного буффера
        /// </summary>
        protected int textureID;
        /// <summary>
        /// id буффера текстуры
        /// </summary>
        protected int CoordinateID { get; }
        /// <summary>
        /// содержит текстурные координаты
        /// </summary>
        protected float[] Coordinate { get; }
        /// <summary>
        /// Область bmp
        /// </summary>
        protected Rectangle rectGFX;
        /// <summary>
        /// Флаг
        /// </summary>
        protected bool disposed;
        /// <param name="width">ширина растрового образа</param>
        /// <param name="height">высота растрового образа</param>
        /// <exception cref="InvalidOperationException"></exception>
        public TextPrinter(int width, int height)
        {
            if (GraphicsContext.CurrentContext == null) 
                throw new InvalidOperationException("GraphicsContext не обнаружен");
            bmp = new Bitmap(width, height,
                SDI.PixelFormat.Format32bppArgb);
            graphics = Graphics.FromImage(bmp);
            // Используем сглаживание
            graphics.TextRenderingHint = SDT.TextRenderingHint.AntiAlias;
            // создаем id текстурного буфера
            textureID = GL.GenTexture();
            // монтируем текстурный буфер
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            // определяем свойства текстуры
            GL.TexParameter(TextureTarget.Texture2D, 
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, 
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // создаем пустую тектсуру, которую потом пополним растровыми данымми 
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
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
        /// Заливка образа цветом color
        /// </summary>
        /// <param name="color"></param>
        public void Clear(Color color)
        {
            graphics.Clear(color);
        }
        /// <summary>
        /// Выводит строку текта text в точке point растрового образе, 
        /// используя фонт font и цвета brush. Начало координат 
        /// растрового образа находится в его левом верхнем углу
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="point"></param>
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            graphics.DrawString(text, font, brush, point);
        }

        public void BindTextPrinter()
        {
            // монтируем текстурный буфер для вывода на экран
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            // монтируем vbo текстурных координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, CoordinateID);
            // описываем формат массива текстурных координат.
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);
        }
        public void UnBindTextPrinter()
        {
            // отключаем буффер текстуры
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // отключаем буффер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        /// <summary>
        /// отрисовка картинки
        /// </summary>
        public void Draw()
        {
            BindTextPrinter();
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f);
            GL.Vertex2(1f, -1f);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex2(1f, 1f);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex2(-1f, 1f);
            GL.End();
            UnBindTextPrinter();
        }
        // Получает обработчик textureID (System.Int32) текструры,
        // который связывается с TextureTarget.Texture2d
        // см.в OnRenderFrame: GL.BindTexture(TextureTarget.Texture2D, renderer.Texture)
        public int TextureID
        {
            get
            {
                UploadBitmap();
                return textureID;
            }
        }
        /// <summary>
        /// Выгружеат растровые данные в текстуру OpenGL
        /// </summary>
        void UploadBitmap()
        {
            if (rectGFX != RectangleF.Empty)
            {
                SDI.BitmapData data = bmp.LockBits(rectGFX,
                SDI.ImageLockMode.ReadOnly,
                SDI.PixelFormat.Format32bppArgb);
                GL.BindTexture(TextureTarget.Texture2D, textureID);
                // Текстура формируется на основе растровых данных, содержащихся в data
                GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                    rectGFX.X, rectGFX.Y, rectGFX.Width, rectGFX.Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                // Освобождаем память, занимаемую data
                bmp.UnlockBits(data);
                rectGFX = Rectangle.Empty;
            }
        }
        /// <summary>
        ///  Загрузка в буффер VBO
        /// </summary>
        public void UpdateTextPrinter()
        {
            rectGFX = new Rectangle(0, 0, bmp.Width, bmp.Height);
            // создаем данные для текстуры на основе растровых данных,
            // содержащихся в bmp
            SDI.BitmapData data = bmp.LockBits(rectGFX,
                SDI.ImageLockMode.ReadOnly,
                SDI.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            // копируем данные в видеопамять
            GL.TexSubImage2D(TextureTarget.Texture2D, 0,
                rectGFX.X, rectGFX.Y, rectGFX.Width, rectGFX.Height,
                PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            // Освобождаем память, занимаемую data
            bmp.UnlockBits(data);
            rectGFX = Rectangle.Empty;
        }
        void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    bmp.Dispose();
                    graphics.Dispose();
                    if (GraphicsContext.CurrentContext != null) GL.DeleteTexture(textureID);
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
