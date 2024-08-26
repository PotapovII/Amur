namespace ViewerGL
{
    using System;
    using System.Drawing;
    
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок N 8 шейдеры, EBO и VAO
    /// координаты и цвета в одном VBO 
    /// </summary>
    public class GLMain_09_Shader_EBO : GameWindow
    {
        private float frameTime = 0.0f;
        private int fps = 0;
        const float b = 0.7f;
        /// <summary>
        /// размерность пространства точек
        /// </summary>
        const int DimentionVertex = 3;
        /// <summary>
        /// размерность пространства цвета
        /// </summary>
        const int DimentionColor = 4;
        /// <summary>
        /// общая размерность пространства 
        /// </summary>
        int DimentionAll = DimentionVertex + DimentionColor;
        /// <summary>
        /// сдвиг от начала массива
        /// </summary>
        int shift = DimentionVertex;
        /// <summary>
        /// заглушка 
        /// </summary>
        const int proxyArray = 0;
        /// <summary>
        /// массив цветов
        /// </summary>
        float[] vertexColors = new float[]
        {
           -b, -b, 0f,  1.0f, 0.0f, 0.0f, 1.0f,
            b, -b, 0f,  0.0f, 1.0f, 0.0f, 1.0f,
           -b,  b, 0f,  0.0f, 0.0f, 1.0f, 1.0f,
            b,  b, 0f,  0.5f, 0.0f, 0.5f, 1.0f
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
        /// id vbo буффера уоординат - цветов
        /// </summary>
        int vbo_id = -1;
        /// <summary>
        /// id ebo буффера - индексоа вершин треугольников
        /// </summary>
        int ebo_id = -1;

        /// <summary>
        /// id vao буффера 
        /// </summary>
        int vaoID = -1;
        /// <summary>
        /// Шейдер
        /// </summary>
        private Shader shader = null;

        public GLMain_09_Shader_EBO(int WinSizeX = 812, 
                      int WinSizeY = 512, 
                      string WinTitle = "Титул") :
                        base(WinSizeX, WinSizeY, 
                        GraphicsMode.Default, WinTitle)
        {
            Console.WriteLine(GL.GetString(StringName.Version));
            Console.WriteLine(GL.GetString(StringName.Vendor));
            Console.WriteLine(GL.GetString(StringName.Renderer));
            Console.WriteLine(GL.GetString(StringName.ShadingLanguageVersion));
            VSync = VSyncMode.On;
            CursorVisible = true;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            shader = new Shader(@"Shader\Bshader.ver", @"Shader\Bshader.fra");
            // создание vao с индексным буфером и щейдерами
            CreateVAOShader(ref vaoID, ref vbo_id, ref ebo_id);
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            DeleteVAOShaders();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
            frameTime += (float)e.Time;
            fps++;
            if (frameTime >= 1.0f)
            {
                Title = $"OpenTK : FPS - {fps}";
                frameTime = 0.0f;
                fps = 0;
            }
            base.OnUpdateFrame(e);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            DrawVAOShaders();

            SwapBuffers();
        }
        /// <summary>
        /// Создание буфера VAO
        /// </summary>
        /// <returns></returns>
        protected void CreateVAOShader(ref int vao, ref int vbo, ref int ebo)
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            vbo = GLB.CreateVBO(vertexColors);
            ebo = GLB.CreateEBO(indexes);

            int vertexArray = shader.GetAttribProgram("aPosition");
            int colorArray = shader.GetAttribProgram("aColor");

            GL.EnableVertexAttribArray(vertexArray);
            GL.EnableVertexAttribArray(colorArray);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            
            GL.VertexAttribPointer(vertexArray, DimentionVertex, 
                VertexAttribPointerType.Float, false, 
                DimentionAll * sizeof(float), proxyArray);
            
            GL.VertexAttribPointer(colorArray, DimentionColor, 
                VertexAttribPointerType.Float, false,
            DimentionAll*sizeof(float), shift * sizeof(float));

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.DisableVertexAttribArray(vertexArray);
            GL.DisableVertexAttribArray(colorArray);
        }
        /// <summary>
        /// Отрисовка
        /// </summary>
        private void DrawVAOShaders()
        {
            shader.ActiveProgram();
            GL.BindVertexArray(vaoID);
            GL.DrawElements(PrimitiveType.Triangles, indexes.Length, 
                            DrawElementsType.UnsignedInt, 0);
            shader.DeactiveProgram();
        }

        private void DeleteVAOShaders()
        {
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(vbo_id);
            GL.DeleteBuffer(ebo_id);
        }

    }
}