namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок N 12 работа с мышью и камерой
    /// отрисовка через вершинные массивы
    /// </summary>
    public class GLMain_12_Frustum_and_Mouse : GameWindow
    {
        const float b = 1.0f;
        /// <summary>
        /// сдвиг в буффере от начала массива
        /// </summary>
        int shift = 0;
        /// <summary>
        /// Размер "мира" по осям x и y
        /// </summary>
        int N = 10;
        Vector2 pos = new Vector2(0, 0);
        /// <summary>
        /// Поворот камеры по вертикали
        /// </summary>
        float xAlpha = 20;
        /// <summary>
        /// Поворот камеры по горизонтали
        /// </summary>
        float zAlpha = 0;
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
        Camera camera = new Camera();
        public GLMain_12_Frustum_and_Mouse(int WinSizeX = 1024,
                      int WinSizeY = 1024,
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY,
                GraphicsMode.Default, WinTitle)
        {
        }
        /// <summary>
        /// Отрисовка плоского мира
        /// </summary>
        protected void ShowWorld()
        {
            // Начинаем работу с вершинным массивом координат
            GL.EnableClientState(ArrayCap.VertexArray);
            // Читаем данные из массива координат
            GL.VertexPointer(DimentionVertex, 
                VertexPointerType.Float, shift, vertexes);
            // ОТРИСОВКА мира
            for (int i = -N; i < N; i++)
                for (int j = -N; j < N; j++)
                {
                    GL.PushMatrix();
                    if((i+j)%2==0) 
                        GL.Color3(Color.Green);
                    else
                        GL.Color3(Color. Gray);
                    GL.Translate(i * 2, j * 2, 0);
                    GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
                    GL.PopMatrix();
                }
            // Выключаем работу с вершинным массивом координат
            GL.DisableClientState(ArrayCap.VertexArray);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.DeepSkyBlue);
            // размер точек (квадрат) в пикселях
            GL.PointSize(15.0f);
            GL.LineWidth(1.0f);

            // включение буфера глубины
            GL.Enable(EnableCap.DepthTest);
            // включение отброски отрисовки задних граней
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            //GL.PolygonMode(MaterialFace.Back,
            //PolygonMode.Fill);
            // проекция мировых координат на
            // плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            //GL.Ortho(0, Width, Height, 0, -1, 1);
            // установка перспективной проекции
            GL.Frustum(-1, 1, -1, 1, 2, 80);
            
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
            //CursorVisible = false;
            camera.CalkMoveCamera(keyState);
           // camera.PlayerMove();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | 
                     ClearBufferMask.DepthBufferBit);
            GL.PushMatrix();
                camera.CameraApply();
                ShowWorld();
            GL.PopMatrix();
            SwapBuffers();
        }

        #region Переменные и методы убраные в класс камера
        //protected void MoveCamera()
        //{
        //    GL.Rotate(-xAlpha, 1, 0, 0);
        //    GL.Rotate(-zAlpha, 0, 0, 1);
        //    // камера смотрит на точку положения игрока
        //    GL.Translate(-pos.X, -pos.Y, -3);
        //}
        ///// <summary>
        ///// Управление камерой с клавиатуры
        ///// </summary>
        ///// <param name="keyState"></param>
        //protected void CalkMoveCamera(KeyboardState keyState)
        //{
        //    // Повороты камеры
        //    if (keyState.IsKeyDown(Key.Up) == true)
        //        xAlpha = ++xAlpha > 180 ? 180 : xAlpha;
        //    if (keyState.IsKeyDown(Key.Down) == true)
        //        xAlpha = --xAlpha < 0 ? 0 : xAlpha;
        //    if (keyState.IsKeyDown(Key.Left) == true)
        //        zAlpha++;
        //    if (keyState.IsKeyDown(Key.Right) == true)
        //        zAlpha--;
        //    // Движение камеры 
        //    float speed = 0;
        //    float ugol = -zAlpha / 180f * (float)Math.PI;
        //    if (keyState.IsKeyDown(Key.W) == true)
        //        speed = 0.1f;
        //    if (keyState.IsKeyDown(Key.S) == true)
        //        speed = -0.1f;
        //    if (keyState.IsKeyDown(Key.A) == true)
        //    {
        //        speed = 0.1f;
        //        ugol -= (float)Math.PI / 2;
        //    }
        //    if (keyState.IsKeyDown(Key.D) == true)
        //    {
        //        speed = 0.1f;
        //        ugol += (float)Math.PI / 2;
        //    }

        //    if (speed != 0)
        //    {
        //        pos.X += (float)Math.Sin(ugol) * speed;
        //        pos.Y += (float)Math.Cos(ugol) * speed;
        //    }
        //}

        #endregion
    }
}