namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок 1 вершинная отрисовка простых фигур
    /// через вершины 
    /// </summary>
    public class GLMain_00 : GameWindow
    {
        float b = 0.8f;
        float a, da;
        int s;
        public GLMain_00(int WinSizeX = 812, 
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
            a = 0f;
            da = 0.005f;
            s = 1;
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            // включение отброски отрисовки задних граней
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            //GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
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
            a += da * s;
            if (a > 1 || a < -1)
                s = -s;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // проекция мировых координат на плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            // установка единичной матрицы
            GL.LoadIdentity();
            // область отрисовки
            //double left = -2.0, right = 2.0,
            //bot = -2.0, top = 2.0, near = 0.0, far = 4.0;
            //GL.Ortho(left, right, bot, top, near, far);

            //GL.Rotate((1 - a) * 180, 0, 1, 0);
             GL.Rotate(0.5f, 0, 1, 0);
             GL.Translate(0, a, 0);

            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            //GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);

            GL.Begin(PrimitiveType.Triangles);
                GL.Color3(Color.Red);
                GL.Vertex2(-b, -b);
                GL.Color3(Color.Green);
                GL.Vertex2(b, -b);
                GL.Color3(Color.Blue);
                GL.Vertex2(-b, b);

                GL.Color3(Color.Yellow);
                GL.Vertex2(-b, b);
                GL.Color3(Color.Blue);
                GL.Vertex2(b, b);
                GL.Color3(Color.Red);
                GL.Vertex2(b, -b);
            GL.End();

            GL.PushMatrix();
                GL.LoadIdentity();
                    GL.Begin(PrimitiveType.Quads);
                        GL.Color3(Color.Red);
                        GL.Vertex2(-a, -a);
                        GL.Color3(Color.Green);
                        GL.Vertex2(a, -a);
                        GL.Color3(Color.Blue);
                        GL.Vertex2(a, a);
                        GL.Color3(Color.Yellow);
                        GL.Vertex2(-a, a);
                    GL.End();
            GL.PopMatrix();

            SwapBuffers();
        }
    }
}