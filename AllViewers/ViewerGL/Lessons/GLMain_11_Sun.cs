namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок N 11 расширенная работа с светом
    /// вершинные массивы массивы  + массывы нормалей, 
    /// нормализация массивов нормалей
    /// </summary>
    public class GLMain_11_Sun : GameWindow
    {
        float[] normals = null;
        float[] normalsConst = { 0f,0f,1f, 
                                0f, 0f, 1f, 
                                0f, 0f, 1f, 
                                0f, 0f, 1f };
        float[] normalsVar = { -1f, -1f, 3f,  
                                1f, -1f, 3f, 
                                1f, 1f, 3f, 
                               -1f, 1f, 3f };
        public GLMain_11_Sun(int WinSizeX = 812,
                      int WinSizeY = 512,
                      string WinTitle = "Титул") :
                        base(WinSizeX, WinSizeY,
                        GraphicsMode.Default, WinTitle)
        {
            normals = normalsConst;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            // установка матрицы проекции
            GL.Frustum(-0.1, 0.1, -0.1, 0.1, 0.2, 1000);
            
            // задается по умолчанию
            // в модели Modelview рисуются все объекты 
            // и устанавливается освещение
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Enable(EnableCap.DepthTest);
            //  Light.SetLight();

            GL.Viewport(0, 0, Width, Height);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Normalize);
        }
        protected override void OnUnload(EventArgs e)
        {
             base.OnUnload(e);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            theta += 0.5F;
            theta = theta < 360 ? theta : 0;
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
            if (keyState.IsKeyDown(Key.Z) == true)
            {
                // освещение всей поверхности однородное
                normals = normalsConst;
            }
            if (keyState.IsKeyDown(Key.X) == true)
            {
                // более плавное освещение всей поверхности 
                normals = normalsVar;
            }
        }
        float theta = 0;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit |
                ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0, 0, 0, 1);

            GL.PushMatrix();
                GL.Rotate(-70, 1, 0, 0);
                GL.Rotate(35, 0, 0, 1);
                GL.Translate(2, 3, -2);
                GL.PushMatrix();
                    GL.Rotate(theta, 0, 1, 0);
                    float[] pos = { 0, 0, 1, 0 };
                    GL.Light(LightName.Light0, 
                        LightParameter.Position, pos);

                    GL.Translate(0, 0, 1);
                    float scale = 0.4f;
                    GL.Scale(scale, scale, scale);
                    GL.Color3(Color.White);
                    DrawQuadL();

                GL.PopMatrix();
            GL.Color3(Color.Green);

            DrawQuadL();

            ShowQvad();

            GL.PopMatrix();

            SwapBuffers();
        }
                
        /// <summary>
        /// Орисовка прямоугольника классика
        /// </summary>
        void DrawQuadL()
        {
            float b = 0.3f;
            // задаем нормаль и координаты вершины на четной позиции
            GL.Normal3(0f, 0f, 1f);
            GL.Begin(PrimitiveType.Triangles);
                GL.Vertex2(-b, -b);
                GL.Vertex2(b, -b);
                GL.Vertex2(-b, b);
                GL.Vertex2(-b, b);
                GL.Vertex2(b, b);
                GL.Vertex2(b, -b);
            GL.End();
        }
        /// <summary>
        /// Орисовка прямоугольника через вершинный массыв и массыв нормалей
        /// </summary>
        public void ShowQvad()
        {
            float a = 0.8f;
            float[] vertex = { -a, -a, 0, 
                                a, -a, 0,  
                                a, a, 0,  
                               -a, a, 0 };
            float[] colors = { 0f, 0f, 1f, 1f,   
                               1f, 0f, 0f, 1f, 
                               1f, 0f, 1f, 1f,   
                               0f, 1f, 1f, 1f };

            GL.Normal3(0f, 0f, 1f);
            GL.Color3(Color.Blue);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertex);
            GL.NormalPointer(NormalPointerType.Float, 0, normals);
            GL.ColorPointer(4, ColorPointerType.Float, 0, colors);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }
    }

    class Light
    {
        public static void SetLight()
        {
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);

            Vector4 position = new Vector4(0.0f, 200.0f, 300.0f, 1.0f);
            Vector4 ambient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
            Vector4 diffuse = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
            Vector4 specular = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            GL.Light(LightName.Light0, LightParameter.Position, position);
            GL.Light(LightName.Light0, LightParameter.Ambient, ambient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, specular);

        }

        public static void SetMaterial()
        {
            GL.Color4(1.0f, 1.0f, 1.0f, 0.5f);

            Vector4 ambient = new Vector4(0.3f, 0.3f, 0.3f, 0.5f);
            Vector4 diffuse = new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
            Vector4 specular = new Vector4(0.0f, 0.0f, 0.0f, 0.5f);

            GL.Material(MaterialFace.FrontAndBack, 
                MaterialParameter.Ambient, ambient);
            GL.Material(MaterialFace.FrontAndBack, 
                MaterialParameter.Diffuse, diffuse);
            GL.Material(MaterialFace.FrontAndBack, 
                MaterialParameter.Specular, specular);
            GL.Material(MaterialFace.FrontAndBack, 
                MaterialParameter.Shininess, 1.0f);
        }
    }

}