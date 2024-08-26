using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK.Input;
namespace ViewerGL
{
    /// <summary>
    /// Урок 1 Отрисовка простых фигур
    /// через вершины 
    /// </summary>
    public class GLView_MouseScale : GameWindow
    {
        int Level = 1;
        WorldRectangle world;
        WorldRectangle screen;
        WorldRectangle viewport;
        // прототип сетки
        int[][] m = new int[2][]
        {
            new int[3]{ 0, 1, 2 },
            new int[3] { 0, 2, 3 }
        };
        float[] x = { 0, 0, 2, 4 };
        float[] y = { 2, 0, 0, 3 };

        public GLView_MouseScale(int WinSizeX = 812, 
                      int WinSizeY = 512, 
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY, 
                GraphicsMode.Default, WinTitle)
        {
            //ws = new WorldScalerGL();
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

            world = new WorldRectangle(x,y);
            viewport = world;
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
            base.OnUpdateFrame(e);
            MouseState mouse = Mouse.GetState();
            // обработка колеса мыши
            Mouse_WheelMove();

            KeyboardState keyState = Keyboard.GetState();

            if (mouse[MouseButton.Right])
                ZoomReset();
            //a += da * s;
            //if (a > 1 || a < -1)
            //    s = -s;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // проекция мировых координат на плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            // установка единичной матрицы
            GL.LoadIdentity();
            //GL.Rotate(0.5f, 0, 0, 1);

            Initialize(screen, viewport);

            //GL.Rotate((1 - a) * 180, 0, 1, 0);
            //GL.Rotate(0.5f, 0, 0, 1);
            //GL.Translate(0, 0.1, 0);

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
            //float a = 0.5f;
            //GL.PushMatrix();
            //    GL.LoadIdentity();
            //        GL.Begin(PrimitiveType.Quads);
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
        int amountOld = 0;
        /// <summary>
        ///  Handle mouse wheel
        /// </summary>
        private void Mouse_WheelMove()
        {
            MouseState mouse = Mouse.GetState();
            if (screen.ViewportContains(mouse.X, mouse.Y) == true)
            {
                int MaxScale = 50;
                float focusX = ((float)mouse.X) / Width;
                float focusY = 1f - ((float)mouse.Y) / Height;
                //int amount = Mouse.WheelDelta* mouse.Wheel;
                int amount = mouse.ScrollWheelValue * mouse.Wheel;
                if (amountOld != amount)
                    amountOld = amount;
                else
                    return;
                float width, height;
                float scale = 1.05f;
                if (amount > 0) // Приблизить
                {
                    this.Level++;
                    if (this.Level > MaxScale)
                    {
                        this.Level = MaxScale;
                        return;
                    }
                    width = viewport.Width / scale;
                    height = viewport.Height / scale;
                }
                else
                {
                    this.Level--;
                    if (this.Level < 1)
                    {
                        this.Level = 1;
                        this.viewport = world;
                        return ;
                    }
                    width = viewport.Width * scale;
                    height = viewport.Height * scale;
                }
                // Текущий фокус на области просмотра
                // Новые позиции слева и снизу
                float x = viewport.X + (viewport.Width - width) * focusX;
                float y = viewport.Y + (viewport.Height - height) * focusY;
                if (x < world.X)
                    x = world.X;
                else 
                    if (x + width > world.WorldRight)
                        x = world.WorldRight - width;
                if (y < world.Y)
                    y = world.Y;
                else 
                    if (y + height > world.WorldTop)
                        y = world.WorldTop - height;
                // Установить новое окно просмотра
                viewport = new WorldRectangle(x, y, x + width, y + height);
                return ;
            }
        }
        public void Initialize(WorldRectangle screen, WorldRectangle world)
        {
            float WorldWidth = world.WorldRight - world.WorldLeft;
            float WorldHeight = world.WorldTop - world.WorldBottom;
            //// Добавление полей сдвига рисунка от стенок (5%)
            float shift = 0.05f;
            float worldMargin = (WorldWidth < WorldHeight) ? WorldHeight * shift : WorldWidth * shift;
            //// отношение сторон экрана
            float screenRatio = screen.Width / screen.Height;
            //// отношение сторон мира
            float worldRatio = WorldWidth / WorldHeight;
            float scale; // масштаб заивист от отношений сторон экрана и сторон мира
            if (screenRatio > worldRatio)
                scale = (WorldHeight + worldMargin) / screen.Height;
            else
                scale = (WorldWidth + worldMargin) / screen.Width;
            // центр мира  
            float centerX = world.CenterX;
            float centerY = world.CenterY;
            // расчет текущего видового экрана (видимое избражение) для мира.
            float ViewportLeft = centerX - screen.Width * scale / 2;
            float ViewportRight = centerX + screen.Width * scale / 2;
            float ViewportBottom = centerY - screen.Height * scale / 2;
            float ViewportTop = centerY + screen.Height * scale / 2;
            float near = 0.0f, far = 5.0f;
            GL.Ortho(ViewportLeft, ViewportRight, ViewportBottom, ViewportTop, near, far);
        }
        public void ZoomReset()
        {
            viewport = world;
            Level = 1;
        }
    }
}