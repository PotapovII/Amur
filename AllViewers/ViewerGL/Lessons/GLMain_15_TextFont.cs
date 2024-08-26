namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;

    using SDI = System.Drawing.Imaging;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;



    /// <summary>
    /// Урок 14  Текстурный вывод текста
    /// </summary>
    public class GLMain_15_TextFont : GameWindow
    {
        /// <summary>
        /// создание bitmap 1х1 пиксель
        /// </summary>
        protected Bitmap bmp = new Bitmap(1, 1);
        int H, W, textureID;
        float[] charWidth;
        static string CharSheet = "абвгдеёжзийклмнопрстуфхцчшщьыъэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯ";
        public GLMain_15_TextFont(int WinSizeX = 812, 
                      int WinSizeY = 512, 
                      string WinTitle = "Титул") :
                        base(WinSizeX, WinSizeY, 
                        GraphicsMode.Default, WinTitle)
        {
            bmp = new Bitmap("шрифт6.png");
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Yellow);

            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);

            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, 0, Height, -1, 1);
            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            GL.Enable(EnableCap.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);
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
            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit
                  | ClearBufferMask.DepthBufferBit);
            //DrawTexture2DElem();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            string word = "ПРИВЕТ";
            int[] text2 = new int[word.Length];
            for (int i = 0; i < text2.Length; i++)
            {
                text2[i] = CharSheet.IndexOf(word[i]);
            }

            Color color = Color.Red;

            GL.Enable(EnableCap.Blend); // включает прозрачность
            GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.OneMinusSrcAlpha);
            int symbol_w = 7;
            int symbol_h = 13;
            
            //GL.Color3(color);//делает цвет фона, а хочется текста
            GL.Color3(1f, 1f, 1f);//делает фон прозрачным
            float w1 = (1f / bmp.Width) * symbol_w;
            float h1 = 1;
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            int x = 50;
            int y = 50;
            for (int j = 0; j < text2.Length; j++)
            {
                float xa = x + j * symbol_w;
                float xb = x + (j + 1) * symbol_w;
                float yt = y + symbol_h;

                GL.Begin(BeginMode.Quads);
                    GL.TexCoord2(text2[j] * w1, h1);                GL.Vertex3(xa, y, 0.0f);            
                    GL.TexCoord2(text2[j] * w1, 0.0);               GL.Vertex3(xa, yt, 0.0f); 
                    GL.TexCoord2((float)(text2[j] + 1) * w1, 0.0);  GL.Vertex3(xb, yt, 0.0f); 
                    GL.TexCoord2((float)(text2[j] + 1) * w1, h1);   GL.Vertex3(xb, y, 0.0f);
                GL.End();
            }
            GL.Disable(EnableCap.Blend);// выключает прозрачность            

            //GL.Flush();
            //GL.Finish();

            SwapBuffers();
        }
    }
}