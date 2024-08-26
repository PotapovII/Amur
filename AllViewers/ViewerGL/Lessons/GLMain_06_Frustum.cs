namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Input;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;

    /// <summary>
    /// Урок N 6 Перспективная и ортопроекции
    /// </summary>
    public class GLMain_06_Frustum : GameWindow
    {
        const float b = 1.0f;
        /// <summary>
        /// сдвиг в буффере от начала массива
        /// </summary>
        int shift = 0;
        /// <summary>
        /// размерность пространства точек
        /// </summary>
        int DimentionVertex = 3;
        /// <summary>
        /// массив вершин 
        /// </summary>
        float[] vertexes = new float[]
        {
            -b, -b, 0f,
             b, -b, 0f,
             b,  b, 0f,
            -b,  b, 0f
        };

        public GLMain_06_Frustum(int WinSizeX = 512,
                      int WinSizeY = 512,
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY,
                GraphicsMode.Default, WinTitle)
        { }

        /// <summary>
        /// Отрисовка мира
        /// </summary>
        protected void ShowWorld()
        {
            // Начинаем работу с вершинным массивом координат
            GL.EnableClientState(ArrayCap.VertexArray);
            // Читаем данные из массива координат
            GL.VertexPointer(DimentionVertex, 
                VertexPointerType.Float, shift, vertexes);
            // ОТРИСОВКА
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            // Выключаем работу с вершинным массивом координат
            GL.DisableClientState(ArrayCap.VertexArray);
        }
        protected void MoveCamera()
        {
            GL.Translate(0, 0, -2);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);
            // размер точек (квадрат) в пикселях
            GL.PointSize(15.0f);
            GL.LineWidth(1.0f);

            // включение буфера глубины
            GL.Enable(EnableCap.DepthTest);
            // включение отброски отрисовки задних граней
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.PolygonMode(MaterialFace.Back,
            PolygonMode.Fill);
            // проекция мировых координат на
            // плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            //GL.Ortho(0, Width, Height, 0, -1, 1);
            // установка перспективной проекции
            GL.Frustum(-1, 1, -1, 1, 2, 8);
            GL.Translate(0, 0, -2);
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
            GL.Clear(ClearBufferMask.ColorBufferBit | 
                     ClearBufferMask.DepthBufferBit);
            GL.Translate(0, 0, -0.001);
            GL.Color3(Color.Yellow);
            ShowWorld();
            SwapBuffers();
        }
    }
}