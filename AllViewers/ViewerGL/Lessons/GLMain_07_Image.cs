namespace ViewerGL
{
    using System;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок Работа с текстурами в 2D
    /// </summary>
    public class GLMain_07_Image : GameWindow
    {
        Image2D ImgMap;
        FImage2D ImgHero;
        int x = 100, y = 100;
        KeyboardState keyState;
        public GLMain_07_Image(int WinSizeX = 1024,
                      int WinSizeY = 512,
                      string WinTitle = "Титул") :
                base(WinSizeX, WinSizeY,
                GraphicsMode.Default, WinTitle, 
                GameWindowFlags.FixedWindow)
        {
            // Включает возможность использование текстур
            GL.Enable(EnableCap.Texture2D);
            ImgMap = new Image2D(Width, Height,
                        @"Texture\Images\bkg.jpg", new Vector2(0, 0));
            ImgHero = new FImage2D(20, 20,
                        @"Texture\Images\vac.png", new Vector2(100, 100));
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // отобажение видимой области игры
            GL.Viewport(0, 0, Width, Height);
            // применение последующих матричных операций
            // к стеку проекционных матриц
            GL.MatrixMode(MatrixMode.Projection);
            // Определяем размер мировой области в масштабе экранной области
            GL.Ortho(0, Width, Height, 0, -1, 1);
            // применение последующих матричных операций
            // к стеку матриц modelview
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            ImgMap.Dispose();
            ImgHero.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // определяет работу с клавиатурой
            keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
            if (keyState.IsKeyDown(Key.Left) == true)
                x-=10;
            if (keyState.IsKeyDown(Key.Right) == true)
                x+=10;
            if (keyState.IsKeyDown(Key.Down) == true)
                y += 10;
            if (keyState.IsKeyDown(Key.Up) == true)
                y -= 10;
            ImgHero.Move(new Vector2(x, y));
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit
                   | ClearBufferMask.DepthBufferBit);
            ImgMap.Draw(); 
            ImgHero.Draw();
            SwapBuffers();
        }
    }
}