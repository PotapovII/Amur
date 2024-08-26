namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок 14  Текстурный вывод текста
    /// </summary>
    public class GLMain_14_Text2 : GameWindow
    {
        Image2D textPrinter;
        
        Font serif = new Font(FontFamily.GenericSerif, 24);
        Font sans = new Font(FontFamily.GenericSansSerif, 24);
        Font mono = new Font(FontFamily.GenericMonospace, 24);
        public GLMain_14_Text2(int WinSizeX = 812, 
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

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            
            GL.Enable(EnableCap.Texture2D);

            textPrinter = new Image2D(Width, Height, new Vector2(0,0) );

            PointF position = PointF.Empty;
            textPrinter.Clear(Color.SaddleBrown);
            // Текст белым цветом и разными шрифтами
            // GenericSerif
            textPrinter.DrawString("За рекой гремит гроза", 
                serif, Brushes.White, position);
            position.Y += serif.Height;
            // GenericSansSerif
            textPrinter.DrawString("За рекой гремит гроза", 
                sans, Brushes.White, position);
            position.Y += sans.Height;
            // GenericMonospace
            textPrinter.DrawString("За рекой гремит гроза", 
                mono, Brushes.White, position);
            // Позиция для следующей строки текста, если такая появится
            position.Y += mono.Height;
            textPrinter.UpdateTextPrinter();
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            textPrinter.Dispose();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            GL.LoadIdentity();
            GL.Ortho(-2.0, 2.0, -2.0, 2.0, 0.0, 4.0);
        }
        KeyboardState keyState;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit
                  | ClearBufferMask.DepthBufferBit);

            GL.Begin(PrimitiveType.TriangleFan);
            GL.Vertex2(-1.5f, -1.5f);
            GL.Vertex2(1.5f, -1.5f);
            GL.Vertex2(1.5f, 1.5f);
            GL.Vertex2(-1.5f, 1.5f);
            GL.End();

            textPrinter.Draw();
            SwapBuffers();
        }
    }
}