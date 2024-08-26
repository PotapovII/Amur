namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Platform;
    using OpenTK.Input;
    /// <summary>
    /// Урок 3 вершинные массивы (Vertex Array)
    /// </summary>
    public class GLMain_03_VertexArrayIndex : GameWindow
    {
        const float b = 0.7f;
        /// <summary>
        /// сдвиг от начала массива
        /// </summary>
        int shift = 0;
        /// <summary>
        /// размерность пространства точек
        /// </summary>
        int DimentionVertex = 3;
        /// <summary>
        /// размерность пространства цвета
        /// </summary>
        int DimentionColor = 4;
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
        /// массив индексов вершин
        /// </summary>
        uint[] indexes = new uint[]
        {
            0,1,2,
            2,1,3
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
        public GLMain_03_VertexArrayIndex(int WinSizeX = 812, 
                      int WinSizeY = 512, 
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY, 
                GraphicsMode.Default, WinTitle)
        {
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);
            // размер точек (квадрат) в пикселях
            GL.PointSize(15.0f);
            GL.LineWidth(1.0f);

            // включение отброски отрисовки задних граней
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            //GL.PolygonMode(MaterialFace.Front,
            //PolygonMode.Line);
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
            //отрисовка вершинного массива
            DrawIndexVertexAndColorsArray();
            //GL.PushMatrix();
            SwapBuffers();
        }
        /// <summary>
        /// Отрисовка вершинных масивов координат и цветов с помошью
        /// вершинных индексов
        /// </summary>
        public void DrawIndexVertexAndColorsArray()
        {
            // Начинаем работу с вершинным массивом координат
            GL.EnableClientState(ArrayCap.VertexArray);
            // Начинаем работу с вершинным массивом цветов вершин
            GL.EnableClientState(ArrayCap.ColorArray);
            // Читаем данные из массива координат
            GL.VertexPointer(DimentionVertex,
                VertexPointerType.Float, 0, vertexes);
            // Читаем данные из массива цветов
            GL.ColorPointer(DimentionColor,
                ColorPointerType.Float, 0, vertexColors);
            // ОТРИСОВКА ПО ЭЛЕМЕНТАМ
            GL.DrawElements(PrimitiveType.Triangles, indexes.Length, 
                DrawElementsType.UnsignedInt, indexes);
            // Выключаем работу с вершинным массивом цветов
            GL.DisableClientState(ArrayCap.ColorArray);
            // Выключаем работу с вершинным массивом координат
            GL.DisableClientState(ArrayCap.VertexArray);
        }
    }
}