namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Input;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    /// <summary>
    /// Урок 5 Объекты вершинных массивов (VAO)
    /// </summary>
    public class GLMain_05_VAO : GameWindow
    {
        const float b = 0.7f;
        /// <summary>
        /// сдвиг в буффере от начала массива
        /// </summary>
        int shift = 0;
        /// <summary>
        /// заглушка вместо массива 
        /// данных - все берем из буффера            
        /// </summary>
        int proxyArray = 0;
        /// <summary>
        /// размерность пространства точек
        /// </summary>
        int DimentionVertex = 3;
        /// <summary>
        /// размерность пространства цвета
        /// </summary>
        int DimentionColor = 4;
        /// <summary>
        /// ключевые индексы VBO
        /// </summary>
        int vboVertexID = -1, vboColorID = -1;
        /// <summary>
        /// ключевой индекс VAO
        /// </summary>
        int vaoID = -1;
        /// <summary>
        /// массив вершин 
        /// </summary>
        float[] vertexes = new float[]
        {
            -b, -b, 0f,
             b, -b, 0f,
            -b,  b, 0f,
             b,  b, 0f
        };
        /// <summary>
        /// массив цветов
        /// </summary>
        float[] vertexColors = new float[]
        {
            1.0f, 0.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
            0.5f, 0.0f, 0.5f, 1.0f
        };

        public GLMain_05_VAO(int WinSizeX = 812,
                      int WinSizeY = 512,
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY,
                GraphicsMode.Default, WinTitle)
        { }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);
            // размер точек (квадрат) в пикселях
            GL.PointSize(15.0f);
            GL.LineWidth(1.0f);

            // включение отброски отрисовки
            // задних граней
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            // сборка VAO
            vaoID = CreateVAO_noShader();
            // начало работы с VAO
            GL.BindVertexArray(vaoID);
            
            GL.PolygonMode(MaterialFace.Front,
            PolygonMode.Line);
            //GL.PolygonMode(MaterialFace.Back,
            //PolygonMode.Fill);
            // проекция мировых координат на
            // плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            // установка единичной матрицы
            GL.LoadIdentity();
        }
        protected override void OnUnload(EventArgs e)
        {
            DeleteVAO_noShader();
            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // вращение объектов отрисовки
            GL.Rotate(1.5f, 0, 0, 1);
            //GL.PushMatrix();
            
            // отрисовка из буффера VAO
            GL.DrawArrays(PrimitiveType.TriangleStrip, shift, 4);

            //GL.PushMatrix();
            SwapBuffers();
        }
        /// <summary>
        /// Создание VBO буффера
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int CreateVBO(float[] data)
        {
            // id vbo буфера
            int vbo = GL.GenBuffer();
            // указание на начала работы по буфферизации
            // данных для VBO буфера c id 
            // указание типа буфера:
            // BufferTarget.ArrayBuffer 
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                data.Length * sizeof(float), data,
                BufferUsageHint.StaticDraw);
            // завершение работы с буффером и отправка
            // данных в VBO в видеопамять
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            return vbo;
        }
        /// <summary>
        /// Удаление буфера VAO
        /// </summary>
        protected void DeleteVAO_noShader()
        {
            // конец работы с VAO
            GL.BindVertexArray(0);
            // удаление данных VAO из ВК
            GL.DeleteVertexArray(vaoID);
            // удаление данных VBO из ВК
            GL.DeleteBuffer(vboVertexID);
            GL.DeleteBuffer(vboColorID);
        }
        /// <summary>
        /// Создание буфера VAO
        /// </summary>
        /// <returns></returns>
        protected int CreateVAO_noShader()
        {
            // получить vao индекс
            int vaoID = GL.GenVertexArray();
            // начинаем работать с VAO
            GL.BindVertexArray(vaoID);
            vboVertexID = CreateVBO(vertexes);
            vboColorID = CreateVBO(vertexColors);
            
            // Включаем использование вершинного масивов
            GL.EnableClientState(ArrayCap.VertexArray);
            // Включаем использование вершинного
            // масивов цветов
            GL.EnableClientState(ArrayCap.ColorArray);
           
            // начинаем работать с буффером координат
            // !!!
            GL.BindBuffer(BufferTarget.ArrayBuffer,
                vboVertexID);
            // Читаем данные из буфферного массива
            GL.VertexPointer(DimentionVertex,
            VertexPointerType.Float, shift, proxyArray);
            // начинаем работать с буффером цвета
            // !!!
            GL.BindBuffer(BufferTarget.ArrayBuffer,
                vboColorID);
            // Читаем данные из буфферного массива цветов
            GL.ColorPointer(DimentionColor,
            ColorPointerType.Float, shift, proxyArray);


            // зевершаем работать с настройкой VAO
            GL.BindVertexArray(0);
            // завершение работы с буфферами VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Выключаем использование вершинного
            // массива координат
            GL.DisableClientState(ArrayCap.VertexArray);
            // Выключаем использование вершинного
            // массива цветов
            GL.DisableClientState(ArrayCap.ColorArray);
            return vaoID;
        }

    }
}