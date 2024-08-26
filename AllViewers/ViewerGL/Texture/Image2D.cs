using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
namespace ViewerGL
{
    /// <summary>
    /// Работа с движением текстуры в 2D плоскости
    /// </summary>
    class Image2D : IDisposable
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
        /// хранение текстуры
        /// </summary>
        private Texture2D texture;
        /// <summary>
        /// хранене позиций 
        /// </summary>
        public Vector2 Position { get; set; }

        public int Width, Height;
        /// <summary>
        /// конструктор картинки (ширина, высота, имя файла, его позиция)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="filename"></param>
        /// <param name="position"></param>
        public Image2D(int width, int height, string filename, Vector2 position)
        {
            // объявляем текстуру.
            // Передаём в конструктор имя файла filename
            texture = new Texture2D(filename);
            Position = position;
            Width = width;
            Height = height;
            // генерируем буффер  для метода SetBuffer()
            vertexBufferId = GL.GenBuffer();
            SetPositionToVBOBuffer();
        }
        /// <summary>
        /// конструктор картинки (ширина, высота, его позиция)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="position"></param>
        public Image2D(int width, int height, Vector2 position)
        {
            // объявляем текстуру.
            // Передаём в конструктор имя файла filename
            texture = new Texture2D(width, height);
            Position = position;
            Width = width;
            Height = height;
            // генерируем буффер  для метода SetBuffer()
            vertexBufferId = GL.GenBuffer();
            SetPositionToVBOBuffer();
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
            texture.Bind();
            // монтируем вершинный массив координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.VertexPointer(3, VertexPointerType.Float,0, 0);
            // отрисовка квадра на которого натянута текстура  
            GL.DrawArrays(PrimitiveType.Quads, 0, vertexData.Length);
            // отклчаем/освобождаем буффер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // отклчаем тестуру
            texture.Unbind();
            // Выключает работу с DrawArrays
            GL.DisableClientState(ArrayCap.VertexArray); 
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }
        /// <summary>
        ///  очищаем видео память 
        /// </summary>
        public void Dispose() 
        {
            texture?.Dispose();
            GL.DeleteBuffer(vertexBufferId); 
        }


        #region Методы для работы с текстом
        /// <summary>
        /// Заливка образа цветом color
        /// </summary>
        /// <param name="color"></param>
        public void Clear(Color color)
        {
            texture.Clear(color);
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
            texture.DrawString(text, font, brush, point);
        }
        public void UpdateTextPrinter()
        {
            texture.UpdateTextPrinter();
        }
        #endregion
    }
}
