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
    public class GLMain_14_TextFont : GameWindow
    {
        /// <summary>
        /// создание bitmap 1х1 пиксель
        /// </summary>
        protected Bitmap bmp = new Bitmap(1, 1);
        int H, W, textureID;
        float[] charWidth;

    public GLMain_14_TextFont(int WinSizeX = 812, 
                      int WinSizeY = 512, 
                      string WinTitle = "Титул") :
                        base(WinSizeX, WinSizeY, 
                        GraphicsMode.Default, WinTitle)
        {
            //LoadTexture2D("Verdana.png");
            LoadTexture2D("Verdana_B_alpha.png");
            
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Yellow);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, -1.0, 1.0);

            GL.Enable(EnableCap.Texture2D);
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

            GL.PushMatrix();
            float scale = 0.15f;
            GL.Translate(-0.1f, 0f, 0f);
            GL.Scale(0.1f, 0.1f, 1f);
            string Coding = "1251";
            DrawText("Hello world!?", textureID, Coding, scale);
            GL.Translate(0f, -1f, 0f);
            GL.Color3(Color.Green);
            DrawText("Хрень какая!", textureID, Coding, scale);
            GL.PopMatrix();
            SwapBuffers();
        }


        public void LoadTexture2D(string fileName)
        {
            if (File.Exists(fileName))
            {
                // загрузка
                bmp = (Bitmap)Image.FromFile(fileName);
                // Поворачивает, зеркально отражает, либо поворачивает и зеркально отражает объект
                // bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            else
            {
                Console.WriteLine("Ошибка! Файл текстуры " + fileName + "не найден.");
                return;
            }
            W = bmp.Width;
            H = bmp.Height;
            
            CalkWidthSimbol(bmp);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.99f);

            Rectangle rectangle = new Rectangle(0, 0, W, H);
            // блокировка растрового изображения в системной памяти,
            // для его изменения программным способом
            BitmapData data = bmp.LockBits(rectangle,
                              ImageLockMode.ReadOnly,
                              SDI.PixelFormat.Format32bppArgb);
            // Создание текстурного буфера
            // генерация ID текстурного буффера
            textureID = GL.GenTexture();
            // привязка ID к типу текстуры
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            // определяем параметры текстуры при ее отображении
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (uint)TextureMinFilter.Linear);
            // копируем данные о текстуре в память видеокарты
            // создает текстуру одного определенного уровня детализации
            // и воспринимает только изображения,
            // размер которых кратен степени двойки 512x512
            GL.TexImage2D(TextureTarget.Texture2D, // target
                0, // lavel - уровень детализации.
                   // Нам нужно исходное изображение,
                   // поэтому уровень детализации - ноль
                PixelInternalFormat.Rgba, // components
                data.Width,
                data.Height,
                0, // border - гарницы не будет,
                   // поэтому значение этого параметра - ноль
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,  // type
                data.Scan0); // pixels
                             // завершаем работу с буффером текстуры
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // останавливаем блокировку БМП date
            bmp.UnlockBits(data);
        }

        public void DrawText(string text, int textureID, string Coding = "1251", float scale = 0.15f)
        {
            float[] CoordinateRectangle = new[]
               {
                    0f, 0f, // левая нижняя координата
                    1f, 0f, // правая нижняя координата
                    1f, 1f, // правая верхняя координата
                    0f, 1f  // левая верхняя координата
                };
            /// <summary>
            /// Если мы не переворачиваем текстуру, 
            /// то необходимо перевернуть текстурные координаты
            /// </summary>
            float[] CoordinateTexture = new[]
                    {
                    0f, 1f, // левая верхняя координата
                    1f, 1f, // правая верхняя координата
                    1f, 0f, // правая нижняя координата
                    0f, 0f // левая нижняя координата
                };

            GL.Enable(EnableCap.Texture2D);
            // монтируем текстуру
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.PushMatrix();

            // Разрешаем работу с вершинными массивами
            // для методов DrawArrays/ DrawElements 
            GL.EnableClientState(ArrayCap.VertexArray);
            // Разрешаем работу массивами текстурных координат
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.VertexPointer(2, VertexPointerType.Float, 0, CoordinateRectangle);
            // описываем формат массива текстурных координат.
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, CoordinateTexture);

            //string text = "Ура печатаем текст !";
            //string text = "Антоха кодит!";
            //string text = "The shit! 0123456789";

            byte[] bytes = Encoding.GetEncoding(1251).GetBytes(text);
            
            // Вычисление текстурных координат 1 клетки
            for (int i = 0; i < bytes.Length; i++)
            {
                float cWidth = charWidth[bytes[i]];
                byte c = bytes[i];
                float charSize = 1 / 16.0f;
                int y = c >> 4;
                int y1 = c / 16;
                int x = c & 0b1111;
                int x1 = c % 16;
                float left, right, top, bottom;
                left = x * charSize;
                right = left + charSize * cWidth;
                top = y * charSize;
                bottom = top + charSize;
                CoordinateTexture[0] = CoordinateTexture[6] = left;
                CoordinateTexture[2] = CoordinateTexture[4] = right;
                CoordinateTexture[1] = CoordinateTexture[3] = bottom;
                CoordinateTexture[5] = CoordinateTexture[7] = top;

                CoordinateRectangle[2] = CoordinateRectangle[4] = cWidth;


                GL.DrawArrays(PrimitiveType.Quads, 0, 4);
                GL.Translate(cWidth, 0f, 0f);
            }

            // Выключает работу с DrawArrays
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);

            GL.PopMatrix();
            // отключаем буффер текстуры
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // отключаем буффер координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>
        /// Вычисление ширины символов в раскладке для квадратной 
        /// текстуры с шириной кратной степени двойки (например 512x512)
        /// </summary>
        /// <param name="data">массив текстуры 
        /// {data[0],data[1],data[2],data[3]} - один пиксел 
        ///     R      G       B      alpha  
        /// </param>
        /// <param name="width">ширина текстуры в пикселях</param>
        /// <param name="cnt">количество байт на 1 пиксел</param>
        /// <param name="chekByte">тестовый байт в символе</param>
        public void CalkWidthSimbol(Bitmap data)
        {
            int width = data.Width;
            charWidth = new float[256];
            int pixelSize = width / 16;
            for (int k = 0; k < charWidth.Length; k++)
            {
                int x = (k % 16) * pixelSize;
                int y = (k / 16) * pixelSize;
                int i=0;
                uint alpha = 0;
                for (i = x + pixelSize - 1; i>x; i--)
                {
                    for (int j = y + pixelSize - 1; j > y; j--)
                    {
                        //alpha = data[(j * width + i) * cnt + chekByte];
                        Color color = data.GetPixel(i, j);
                        alpha = color.R;
                        if (alpha > 0)
                            break;
                    }
                    if (alpha > 0)
                        break;
                }
                i+=pixelSize/10;
                if (i > x + pixelSize - 1)
                    i = x + pixelSize - 1;
                if(k==32 ) i = x + pixelSize/2;

                charWidth[k] = (i-x)/(float)pixelSize;
            }
        }

    }
}