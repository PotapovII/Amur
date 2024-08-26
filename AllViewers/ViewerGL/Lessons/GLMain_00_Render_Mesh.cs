namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    using ViewerGL.Mesh;
    /// <summary>
    /// Отрисовка КЭ сетки 
    /// </summary>
    public class GLMain_00_Render_Mesh : GameWindow
    {
        int SelectedIndexMethods = 0;
        int SelectedIndex = 0;
        ClassTest test = null;
        public GLMain_00_Render_Mesh(int WinSizeX = 812, 
                      int WinSizeY = 512, 
                      string WinTitle = "Титул") :
                        base(WinSizeX, WinSizeY, 
                        GraphicsMode.Default, WinTitle)
        {
            test = new ClassTest(50,1);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            test.Dellete();
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

            if (keyState.IsKeyDown(Key.Z) == true)
                SelectedIndexMethods = 0;
            if (keyState.IsKeyDown(Key.X) == true)
                SelectedIndexMethods = 1;

            if (keyState.IsKeyDown(Key.Q) == true)
                SelectedIndex = 0;
            if (keyState.IsKeyDown(Key.W) == true)
                SelectedIndex = 1;
            if (keyState.IsKeyDown(Key.E) == true)
                SelectedIndex = 2;
            // задание типа примитивов, используемых для отображения поверхности
            switch (SelectedIndex)
            {
                case 1:
                    GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                    GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
                    break;
                case 2:
                    GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                    GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
                    break;
                case 0:
                    GL.PolygonMode(MaterialFace.Front, PolygonMode.Point);
                    GL.PolygonMode(MaterialFace.Back, PolygonMode.Point);
                    GL.PointSize(3);
                    break;
            }

        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.PushMatrix();
                GL.LoadIdentity();
                GL.Translate(-0.5, -0.5, 0);
                GL.Scale(1.3f, 1.3f, 1);
            if (SelectedIndexMethods == 0)
                    test.DrawRenderMesh2DArray();
            if (SelectedIndexMethods == 1)
                test.DrawRenderMesh2D();
            if (SelectedIndexMethods == 2)
                test.DrawRenderMesh2D();
            GL.PopMatrix();
            SwapBuffers();
        }
    }
}