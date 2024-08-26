using System;
using System.Linq;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Input;
namespace ViewerGL
{
    public class GLViewOne : GameWindow
    {
        int Level = 1;
        // Ящик начальных мировых координат
        WorldRectangle world;
        // Ящик текущих видовых координат
        WorldRectangle viewport;
        // Ящик экранных координат
        WorldRectangle screen;
        // прототип сетки
        int[][] m = new int[2][]
        {
            new int[3]{ 0, 1, 2 },
            new int[3] { 0, 2, 3 }
        };
        float[] x = { 0, 0, 2, 4 };
        float[] y = { 2, 0, 0, 3 };

        public GLViewOne(int WinSizeX = 600, 
                      int WinSizeY = 600, 
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY, 
                GraphicsMode.Default, WinTitle)
        {
            //ws = new WorldScalerGL();
            world = new WorldRectangle(x, y);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Green);
            // размер точек (квадрат) в пикселях
            GL.PointSize(15.0f);
            GL.LineWidth(1.0f);
            // включение отброски отрисовки задних граней
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            //GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            world = new WorldRectangle(x, y);
            screen = new WorldRectangle(0, 0, Width, Height);

        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            screen = new WorldRectangle(0, 0, Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
            base.OnUpdateFrame(e);
            // обработка колеса мыши
            MouseState mouse = Mouse.GetState();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // проекция мировых координат на плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            // установка единичной матрицы
            GL.LoadIdentity();

            Initialize(screen, world);
            
            //GL.Rotate(0.5f, 0, 0, 1);
            //GL.Rotate((1 - a) * 180, 0, 1, 0);
            //GL.Rotate(0.5f, 0, 0, 1);
            //GL.PushMatrix(); 
            //GL.Translate(-1, -1, 0);

            // отрисовка сетки
            GL.Begin(PrimitiveType.Triangles);
                for (int i = 0; i < m.Length; i++)
                {
                    int ka = m[i][0];
                    int kb = m[i][1];
                    int kc = m[i][2];
                    GL.Color3(Color.Red);
                    GL.Vertex2(-x[ka], y[ka]);
                    GL.Color3(Color.GreenYellow);
                    GL.Vertex2(x[kb], y[kb]);
                    GL.Color3(Color.Blue);
                    GL.Vertex2(x[kc], y[kc]);
                }
            GL.End();
            //GL.PopMatrix();
            //float a = 0.5f;
            //GL.PushMatrix();
            //    GL.LoadIdentity();
            //        GL.Begin(BeginMode.Quads);
            //            GL.Color3(Color.Red);
            //            GL.Vertex2(-a, -a);
            //            GL.Color3(Color.Green);
            //            GL.Vertex2(a, -a);
            //            GL.Color3(Color.Blue);
            //            GL.Vertex2(a, a);
            //            GL.Color3(Color.Yellow);
            //            GL.Vertex2(-a, a);
            //        GL.End();
            //GL.PopMatrix();
            SwapBuffers();
        }
        /// <summary>
        /// Настройка экранной области на экранную область
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="world"></param>
        /// <param name="near"></param>
        /// <param name="far"></param>
        public void Initialize(WorldRectangle screen, WorldRectangle world, float near = 0.0f, float far = 5.0f)
        {
            // проекция мировых координат на плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            // установка единичной матрицы
            GL.LoadIdentity();
            //// Добавление полей сдвига рисунка от стенок (5%)
            float shift = 0.05f;
            float worldMargin = (world.Width < world.Height) ? world.Height * shift : world.Width * shift;
            //// отношение сторон экрана
            float screenRatio = screen.Width / screen.Height;
            //// отношение сторон мира
            float worldRatio = world.Width / world.Height;
            float scale; // масштаб заивист от отношений сторон экрана и сторон мира
            if (screenRatio > worldRatio)
                scale = (world.Height + worldMargin) / screen.Height;
            else
                scale = (world.Width + worldMargin) / screen.Width;
            // центр мира  
            float centerX = world.CenterX;
            float centerY = world.CenterY;
            // расчет текущего видового экрана (видимое избражение) для мира.
            float ViewportLeft = centerX - screen.Width * scale / 2;
            float ViewportRight = centerX + screen.Width * scale / 2;
            float ViewportBottom = centerY - screen.Height * scale / 2;
            float ViewportTop = centerY + screen.Height * scale / 2;
            GL.Ortho(ViewportLeft, ViewportRight, ViewportBottom, ViewportTop, near, far);
            GL.MatrixMode(MatrixMode.Modelview);
            // установка единичной матрицы
            GL.LoadIdentity();
        }
      
    }
}