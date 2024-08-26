using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using SDI = System.Drawing.Imaging;
using System.Drawing.Imaging;
using System.IO;

namespace ViewerGL
{
    /// <summary>
    /// Работа с движением текстуры в 2D плоскости
    /// </summary>
    class FImage2D : IDisposable
    {
        /// <summary>
        /// хранение ID буффера вершин
        /// </summary>
        private int vertexBufferId;
        /// <summary>
        /// хранение координат текстуры
        /// </summary>
        public float[] vertexData;
        /// <summary>
        /// создание bitmap 1х1 пиксель
        /// </summary>
        protected Bitmap bmp = new Bitmap(1, 1);
        /// <summary>
        /// id текстуры
        /// </summary>
        protected int ID;
        /// <summary>
        /// ширина текстуры
        /// </summary>
        protected int W;
        /// <summary>
        /// высота текстуры
        /// </summary>
        protected int H;
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
        /// хранене позиций 
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Width, Height;
        /// <summary>
        /// конструктор картинки (ширина, высота, имя файла, его позиция)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="filename"></param>
        /// <param name="position"></param>
        public FImage2D(int width, int height, string filename, Vector2 position)
        {
            // объявляем текстуру.
            // Передаём в конструктор имя файла filename
            LoadTexture2D(filename);
            Position = position;
            Width = width;
            Height = height;
            // генерируем буффер  для метода SetBuffer()
            vertexBufferId = GL.GenBuffer();
            SetPositionToVBOBuffer();
        }
        
        public void LoadTexture2D(string fileName)
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
            CreateTextureBuffer2D(data);
            // останавливаем блокировку БМП date
            bmp.UnlockBits(data);
        }
        public void CreateTextureFilter()
        {
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
        }
        /// <summary>
        /// Создание текстурного буффера
        /// </summary>
        /// <param name="data"></param>
        public void CreateTextureBuffer2D(BitmapData data)
        {
            #region Создание текстурного буфера
            // генерация ID текстурного буффера
            ID = GL.GenTexture();
            // привязка ID к типу текстуры
            GL.BindTexture(TextureTarget.Texture2D, ID);

            CreateTextureFilter();

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
        /// перемещение спрайта
        /// </summary>
        /// <param name="direction">принимает в себя координаты объекта </param>
        public void Move(Vector2 direction)
        {
            // изменяем координаты картинки,
            // переданные в метод Move()
            Position = direction;
            SetPositionToVBOBuffer(); // 
        }
        /// <summary>
        /// обновление массива координат находящихся
        /// в vbo буффере после их изменения для 
        /// текущего объекта
        /// </summary>
        private void SetPositionToVBOBuffer()
        {
            //  изменяем указывем координаты точек 
            //  квадрата на который натягиваем текстуру
            vertexData = new float[] // массив точек
            {
                // левый верхний
                Position.X,    0.0f+Position.Y,    0.0f, 
                // правый верхний
                Width + Position.X,   0.0f+Position.Y,    0.0f, 
                // правый нижний
                Width + Position.X,   Height+Position.Y,  0.0f,
                // левый нижний
                Position.X,    Height+Position.Y,  0.0f
            };
            // монтируем vbo буффер 
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            /// копируем координаты вершин спрайта в видеопамять
            GL.BufferData(BufferTarget.ArrayBuffer,
                vertexData.Length * sizeof(float),
                vertexData, BufferUsageHint.StaticDraw);
            // отключаем vbo буффер
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        /// <summary>
        /// отрисовка картинки
        /// </summary>
        public void Draw()
        {
            // Разрешаем работу с вершинными массивами
            // для методов DrawArrays/ DrawElements 
            GL.EnableClientState(ArrayCap.VertexArray);
            // Разрешаем работу массивами текстурных координат
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            // монтируем/подключаем текстуру...
            // монтируем текстуру
            GL.BindTexture(TextureTarget.Texture2D, ID);
            // монтируем vbo текстурных координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, CoordinateID);
            // описываем формат массива текстурных координат.
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0);
            // монтируем вершинный массив координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.VertexPointer(3, VertexPointerType.Float,0, 0);
            // отрисовка квадра на которого натянута текстура  
            GL.DrawArrays(PrimitiveType.Quads, 0, vertexData.Length);
            // отклчаем/освобождаем буффер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // отклчаем тестуру
            // отключаем буффер текстуры
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // отключаем буффер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Выключает работу с DrawArrays
            GL.DisableClientState(ArrayCap.VertexArray); 
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }
        /// <summary>
        ///  очищаем видео память 
        /// </summary>
        public void Dispose() 
        {
            // удаляем буффер текстур
            GL.DeleteBuffer(CoordinateID);
            // удаляем текстуру
            GL.DeleteTexture(ID);
            if (bmp != null)
                bmp.Dispose();
            if (graphics != null)
                graphics.Dispose();
            GL.DeleteBuffer(vertexBufferId); 
        }
    }
}
