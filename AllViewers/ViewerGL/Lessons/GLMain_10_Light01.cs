namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок 11  Работа с источником света, нормали, материалы 
    /// </summary>
    public class GLMain_10_Light01 : GameWindow
    {
        float AngleX = 0;
        float AngleY = 0;
        float AngleZ = 0;
        int pro = 1;

        const float AngleDl = 5;
        public GLMain_10_Light01(int WinSizeX = 812,
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
            SetupViewport();
            GL.ClearColor(1f, 1f, 1f, 1f); // цвет фона
            GL.Enable(EnableCap.DepthTest);
            AngleX = 30;
            AngleY = 30;
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetupViewport();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();

            if (keyState.IsKeyDown(Key.Keypad1) == true ||
                keyState.IsKeyDown(Key.Z) == true)
            {
                pro = 1;
                SetupViewport();
            }
            if (keyState.IsKeyDown(Key.Keypad2) == true ||
                keyState.IsKeyDown(Key.X) == true)
            {
                pro = 2;
                SetupViewport();
            }
            if (keyState.IsKeyDown(Key.W) == true)
            {
                AngleX -= AngleDl;
            }
            if (keyState.IsKeyDown(Key.S) == true)
            {
                AngleX += AngleDl;
            }
            if (keyState.IsKeyDown(Key.D) == true)
            {
                AngleY += AngleDl;
            }
            if (keyState.IsKeyDown(Key.A) == true)
            {
                AngleY -= AngleDl;
            }
            if (keyState.IsKeyDown(Key.E) == true)
            {
                AngleZ += AngleDl;
            }
            if (keyState.IsKeyDown(Key.Q) == true)
            {
                AngleZ -= AngleDl;
            }
         }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
           
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // поворот изображения
            GL.LoadIdentity();
            GL.Rotate(AngleX, 1.0, 0.0, 0.0);
            GL.Rotate(AngleY, 0.0, 1.0, 0.0);
            GL.Rotate(AngleZ, 0.0, 0.0, 1.0);

        
            GL.Color3(1f, 0f, 0f);
            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(0f, 0f, 1f);
            GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.Vertex3(-0.5f, 0.5f, 0.5f);
            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.End();

            GL.Color3(0f, 1f, 0f);
            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(0f, 0f, -1f);
            GL.Vertex3(0.5f, 0.5f, -0.5f);
            GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.End();

            GL.Color3(0f, 0f, 1f);
            GL.Begin(PrimitiveType.Quads);
            GL.Normal3(-1f, 0f, 0f);
            GL.Vertex3(-0.5f, 0.5f, 0.5f);
            GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.End();

            ShowQvad();

            SwapBuffers();
        }
        private void SetupViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            if(pro==1)
                GL.Ortho(-1f, 1f, -1f, 1f, -1f, 1f);
            else
                GL.Frustum(-1.1f, 1.1f, -1.1f, 1.1f, 0.0f, 1000f);

            GL.MatrixMode(MatrixMode.Modelview);

            GL.Viewport(0, 0, Width, Height);
            
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
        }

        public void ShowQvad()
        {
            float a = 0.3f;
            float[] vertex = { -a, -a, 0, a, -a, 0, a, a, 0, -a, a, 0 };
            GL.Normal3(0f, 0f, 1f);
            GL.Color3(Color.Blue);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertex);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            GL.DisableClientState(ArrayCap.VertexArray);
        }
    }
}