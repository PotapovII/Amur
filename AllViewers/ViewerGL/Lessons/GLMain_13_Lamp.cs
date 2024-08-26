namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок N  13  Отрисовка трехмерного объекта по
    /// опорным точкам с расчетом нормалей поверхности
    /// и освещения объекта
    /// </summary>
    public class GLMain_13_Lamp : GameWindow
    {
        private float rot_l, rot_2;
        private const int Iter = 64;
        private double[,] GeometricArray = new double[Iter, 3];
        private double[,,] ResaultGeometric = new double[Iter, Iter, 3];
        private int count_elements = 0;
        private double Angle = 2 * Math.PI / 64;
        private int SelectedIndex = 0;
        private float Value = 0;

        public GLMain_13_Lamp(int WinSizeX = 812, 
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
            GL.ClearColor(Color.Yellow);
            // очистка буферов цвета и глубины
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // установка порта вывода в соответствии с размерами элемента АпТ
            GL.Viewport(0, 0, Width, Height);
            // настройка проекции
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Frustum(-0.1 * Width / Height,
                0.1 * Width / Height, -0.1, 0.1, 0.1, 200);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            // настройка параметров OpenGL для визуализации
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            // количество элементов последовательности геометрии, на основе 
            // которых будет строиться тело вращения
            count_elements = 8;
            // непосредственное заполнение точек
            GeometricArray[0, 0] = 0;
            GeometricArray[0, 1] = 0;
            GeometricArray[0, 2] = 0;

            GeometricArray[1, 0] = 0.7;
            GeometricArray[1, 1] = 0;
            GeometricArray[1, 2] = 1;
            GeometricArray[2, 0] = 1.3;
            GeometricArray[2, 1] = 0;
            GeometricArray[2, 2] = 2;
            GeometricArray[3, 0] = 1.0;
            GeometricArray[3, 1] = 0;
            GeometricArray[3, 2] = 3;
            GeometricArray[4, 0] = 0.5;
            GeometricArray[4, 1] = 0;
            GeometricArray[4, 2] = 4;
            GeometricArray[5, 0] = 3;
            GeometricArray[5, 1] = 0;
            GeometricArray[5, 2] = 6;
            GeometricArray[6, 0] = 1;
            GeometricArray[6, 1] = 0;
            GeometricArray[6, 2] = 7;
            GeometricArray[7, 0] = 0;
            GeometricArray[7, 1] = 0;
            GeometricArray[7, 2] = 7.2f;

            // по умолчанию мы будем отрисовывать фигуру в режиме GL.POINTS
            SelectedIndex = 0;
            // построение геометрии тела вращения
            // принцип сводится к двум циклам: на основе первого перебираются
            // вершины в геометрической последовательности,
            // а второй использует параметр Iter и производит поворот последней линии
            // геометрии вокруг центра тела вращения
            // при этом используется заранее определенный угол angle, который
            // определяется как 2 * Рi / количество медиан объекта
            // за счет выполнения этого алгоритма получается набор вершин, описывающих
            // оболочку тела вращения.
            // остается только соединить эти точки в режиме рисования примитивов 
            // для получения визуализированного объекта
            // цикл по последовательности точек кривой, на основе которой будет
            // построено тело вращения
            for (int ax = 0; ax < count_elements; ax++)
            {
                // цикла по медианам объекта, заранее определенным в программе
                for (int bx = 0; bx < Iter; bx++)
                {
                    // для всех (bx > 0) элементов алгоритма используется предыдущая
                    // построенная последовательность для ее поворота на установленный угол
                    if (bx > 0)
                    {
                        double new_x = ResaultGeometric[ax, bx - 1, 0] * Math.Cos(Angle)
                        - ResaultGeometric[ax, bx - 1, 1] * Math.Sin(Angle);
                        double new_y = ResaultGeometric[ax, bx - 1, 0] * Math.Sin(Angle)
                        + ResaultGeometric[ax, bx - 1, 1] * Math.Cos(Angle);
                        ResaultGeometric[ax, bx, 0] = new_x;
                        ResaultGeometric[ax, bx, 1] = new_y;
                        ResaultGeometric[ax, bx, 2] = GeometricArray[ax, 2];
                    }
                    else
                    {
                        // для построения первой медианы мы используем начальную кривую,
                        // описывая ее нулевым значением угла поворота
                        double a = 0;
                        double new_x = GeometricArray[ax, 0] * Math.Cos(a) -
                        GeometricArray[ax, 1] * Math.Sin(a);
                        double new_y = GeometricArray[ax, 1] * Math.Sin(a) +
                        GeometricArray[ax, 1] * Math.Cos(a);
                        ResaultGeometric[ax, bx, 0] = new_x;
                        ResaultGeometric[ax, bx, 1] = new_y;
                        ResaultGeometric[ax, bx, 2] = GeometricArray[ax, 2];
                    }
                }
            }
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

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Key.Escape))
                Exit();
            
            rot_l++; 
            rot_2++;
            if (rot_l > 360) rot_l = 0;
            if (rot_2 > 360) rot_2 = 0;
            
            int NN = 7;
            if (keyState.IsKeyDown(Key.Left) == true)
            {
                Value -= 0.05f;
                Value = Value < 0 ? 0 : Value;
            }

            if (keyState.IsKeyDown(Key.Right) == true)
            {
                Value += 0.05f;
                Value = Value > NN ? NN : Value;
            }

            if (keyState.IsKeyDown(Key.Q) == true)
                SelectedIndex = 0;
            if (keyState.IsKeyDown(Key.W) == true)
                SelectedIndex = 1;
            if (keyState.IsKeyDown(Key.E) == true)
                SelectedIndex = 2;

        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            // два параметра, которые мы будем использовать для 
            // непрерывного вращения сцены вокруг 2 координатных осей
           
            // очистка буфера цвета и буфера глубины
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.White);
            // очищение текущей матрицы
            GL.LoadIdentity();
            // установка положения камеры (наблюдателя). Как видно из кода,
            // дополнительно на положение наблюдателя по оси Z влияет значение,
            // установленное в ползунке, доступном для пользователя.
            // таким образом, при перемещении ползунка, наблюдатель будет отдаляться или
            // приближаться к объекту наблюдения
            GL.Translate(0, 0, -7 - Value);
            // 2 поворота (углы rot_l и rot_2)
            GL.Rotate(rot_l, 1, 0, 0);
            GL.Rotate(rot_2, 0, 1, 0);
            // устанавливаем размер точек, равный 5
            GL.PointSize(5.0f);
            // условие switch определяет установленный режим отображения, 
            // на основе выбранного пункта элемента
            // listBox1, установленного в форме программы
            switch (SelectedIndex)
            {
                // отображение в виде точек
                case 0:
                    {
                        // режим вывода геометрии - точки
                        GL.Begin(PrimitiveType.Points);
                        // выводим всю ранее просчитанную геометрию объекта
                        for (int ax = 0; ax < count_elements; ax++)
                        {
                            for (int bx = 0; bx < Iter; bx++)
                            {
                                // отрисовка точки
                                GL.Vertex3(ResaultGeometric[ax, bx, 0],
                                           ResaultGeometric[ax, bx, 1],
                                           ResaultGeometric[ax, bx, 2]);
                            }
                        }
                        // завершаем режим рисования
                        GL.End();
                        break;
                    }
                // отображение объекта в сеточном режиме, используя режим GL_LINES_STRIP
                case 1:
                    {
                        // устанавливаем режим отрисовки линиями (последовательность линий)
                        GL.Begin(PrimitiveType.LineStrip);
                        for (int ax = 0; ax < count_elements; ax++)
                        {
                            for (int bx = 0; bx < Iter; bx++)
                            {
                                GL.Vertex3(ResaultGeometric[ax, bx, 0],
                                            ResaultGeometric[ax, bx, 1],
                                            ResaultGeometric[ax, bx, 2]);
                                GL.Vertex3(ResaultGeometric[ax + 1, bx, 0],
                                            ResaultGeometric[ax + 1, bx, 1],
                                            ResaultGeometric[ax + 1, bx, 2]);
                                if (bx + 1 < Iter - 1)
                                {
                                    GL.Vertex3(ResaultGeometric[ax + 1, bx + 1, 0],
                                                ResaultGeometric[ax + 1, bx + 1, 1],
                                                ResaultGeometric[ax + 1, bx + 1, 2]);
                                }
                                else
                                {
                                    GL.Vertex3(ResaultGeometric[ax + 1, 0, 0],
                                                ResaultGeometric[ax + 1, 0, 1],
                                                ResaultGeometric[ax + 1, 0, 2]);
                                }
                            }
                        }
                        GL.End();
                        break;
                    }
                // отрисовка оболочки с расчетом нормалей для корректного затенения
                // граней объекта
                case 2:
                    {
                        GL.Begin(PrimitiveType.Quads);
                        // режим отрисовки полигонов состоящих из 4 вершин
                        for (int ax = 0; ax < count_elements; ax++)
                        {
                            for (int bx = 0; bx < Iter; bx++)
                            {
                                // вспомогательные переменные для более наглядного
                                // использования кода при расчете нормалей
                                double x1 = 0, x2 = 0, x3 = 0, x4 = 0,
                                       y1 = 0, y2 = 0, y3 = 0, y4 = 0,
                                       z1 = 0, z2 = 0, z3 = 0, z4 = 0;
                                // первая вершина
                                x1 = ResaultGeometric[ax, bx, 0];
                                y1 = ResaultGeometric[ax, bx, 1];
                                z1 = ResaultGeometric[ax, bx, 2];
                                // если текущий ax не последний
                                if (ax + 1 < count_elements)
                                {
                                    // берем следующую точку последовательности
                                    x2 = ResaultGeometric[ax + 1, bx, 0];
                                    y2 = ResaultGeometric[ax + 1, bx, 1];
                                    z2 = ResaultGeometric[ax + 1, bx, 2];
                                    // если текущий bx не последний
                                    if (bx + 1 < Iter - 1)
                                    {
                                        //	берем следующую точку последовательности и следующий медиан
                                        x3 = ResaultGeometric[ax + 1, bx + 1, 0];
                                        y3 = ResaultGeometric[ax + 1, bx + 1, 1];
                                        z3 = ResaultGeometric[ax + 1, bx + 1, 2];


                                        // точка, соответствующая по номеру, 
                                        // только на соседнем медиане
                                        x4 = ResaultGeometric[ax, bx + 1, 0];
                                        y4 = ResaultGeometric[ax, bx + 1, 1];
                                        z4 = ResaultGeometric[ax, bx + 1, 2];
                                    }
                                    else
                                    {
                                        // если это последний медиан, то в качестве след.
                                        // мы берем начальный(замыкаем геометрию фигуры)
                                        x3 = ResaultGeometric[ax + 1, 0, 0];
                                        y3 = ResaultGeometric[ax + 1, 0, 1];
                                        z3 = ResaultGeometric[ax + 1, 0, 2];
                                        x4 = ResaultGeometric[ax, 0, 0];
                                        y4 = ResaultGeometric[ax, 0, 1];
                                        z4 = ResaultGeometric[ax, 0, 2];
                                    }
                                }
                                else
                                {
                                    // данный элемент ах последний, следовательно, мы будем
                                    // использовать начальный(нулевой) вместо данного ах

                                    // следующей точкой будет нулевая ах
                                    x2 = ResaultGeometric[0, bx, 0];
                                    y2 = ResaultGeometric[0, bx, 1];
                                    z2 = ResaultGeometric[0, bx, 2];
                                    if (bx + 1 < Iter - 1)
                                    {
                                        x3 = ResaultGeometric[0, bx + 1, 0];
                                        y3 = ResaultGeometric[0, bx + 1, 1];
                                        z3 = ResaultGeometric[0, bx + 1, 2];
                                        x4 = ResaultGeometric[ax, bx + 1, 0];
                                        y4 = ResaultGeometric[ax, bx + 1, 1];
                                        z4 = ResaultGeometric[ax, bx + 1, 2];
                                    }
                                    else
                                    {
                                        x3 = ResaultGeometric[0, 0, 0];
                                        y3 = ResaultGeometric[0, 0, 1];
                                        z3 = ResaultGeometric[0, 0, 2];
                                        x4 = ResaultGeometric[ax, 0, 0];
                                        y4 = ResaultGeometric[ax, 0, 1];
                                        z4 = ResaultGeometric[ax, 0, 2];
                                    }
                                }
                                // переменные для расчета нормали
                                double n1 = 0, n2 = 0, n3 = 0;
                                // нормаль будем рассчитывать как векторное произведение граней полигона
                                // для нулевого элемента нормаль мы будем считать немного по - другому.
                                // на самом деле разница в расчете нормали актуальна
                                // только для первого и последнего полигона на медиане
                                if (ax == 0)
                                {
                                    // при расчете нормали для ах мы будем использовать точки 1,2,3
                                    n1 = (y2 - y1) * (z3 - z1) - (y3 - y1) * (z2 - z1);
                                    n2 = (z2 - z1) * (x3 - x1) - (z3 - z1) * (x2 - x1);
                                    n3 = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
                                }
                                else
                                {
                                    // для остальных - 1,3,4
                                    n1 = (y4 - y3) * (z1 - z3) - (y1 - y3) * (z4 - z3);
                                    n2 = (z4 - z3) * (x1 - x3) - (z1 - z3) * (x4 - x3);
                                    n3 = (x4 - x3) * (y1 - y3) - (x1 - x3) * (y4 - y3);
                                }
                                // если не включен режим IMORMILIZE, то мы должны в
                                // обязательном порядке 
                                // произвести нормализацию вектора нормали перед тем как
                                // передать информацию о нормали
                                double n5 = (double)Math.Sqrt(n1 * n1 + n2 * n2 + n3 * n3);
                                n1 /= (n5 + 0.01);
                                n2 /= (n5 + 0.01);
                                n3 /= (n5 + 0.01);
                                // передаем информацию о нормали
                                GL.Normal3(-n1, -n2, -n3);
                                // передаем 4 вершины для отрисовки полигона
                                GL.Vertex3(x1, y1, z1);
                                GL.Vertex3(x2, y2, z2);
                                GL.Vertex3(x3, y3, z3);
                                GL.Vertex3(x4, y4, z4);
                            }
                        }
                        // завершаем выбранный режим рисования полигонов
                        GL.End();
                        break;
                    }

            }
            SwapBuffers();
        }
    }
}