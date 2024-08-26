namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;

    /// <summary>
    /// Вершина сетки
    /// </summary>

    struct Vertex
    {
        /// <summary>
        /// точка
        /// </summary>
        public float x, y, z;
        /// <summary>
        /// нормаль
        /// </summary>
        public float nx, ny, nz;
    }

    /// <summary>
    /// Урок N 10 работа с камерой и светом
    /// + дисплайный список
    /// </summary>
    public class GLMain_10_Light0 : GameWindow
    {
        /// <summary>
        /// угол поворота камеры
        /// </summary>
        float rotationAngle = 0;

        float trackBarValue = 0;
        /// <summary>
        /// индекс отрисовки
        /// </summary>
        int SelectedIndex = 0;
        /// <summary>
        /// время для эволюции поверхности
        /// </summary>
        double time = 0;
        /// <summary>
        /// id дисплейного списка
        /// </summary>
        int displayList_id = 0;

        public GLMain_10_Light0(int WinSizeX = 812,
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
            // настройка параметров openGL для визуализации
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            // создание поверхности и копирование ее данных в видеопамять
            CreateDL();
        }
        protected override void OnUnload(EventArgs e)
        {
            int namberDisplayL1 = 1;
            GL.DeleteLists(displayList_id, namberDisplayL1);
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
            int NN = 7;
            if (keyState.IsKeyDown(Key.Left) == true)
            {
                trackBarValue -= 0.05f;
                trackBarValue = trackBarValue < 0 ? 0 : trackBarValue;
            }
                
            if (keyState.IsKeyDown(Key.Right) == true)
            {
                trackBarValue += 0.05f;
                trackBarValue = trackBarValue > NN ? NN : trackBarValue;
            }
               
            time++;
            if (time > 120) time = 0;

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
            SetupLight(); // настройка параметров освещения
            SetupMaterial(); // настройка свойств материала
            SetupCamera(); // настройка камеры
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawSurface(time);
            SwapBuffers();
        }
        /// <summary>
        /// управления свойствами материала
        /// </summary>
        private void SetupMaterial()
        {
            // задаем характеристики материала
            float[] m_diffuse = new float[] { 0.0f, 0.5f, 0.0f, 1 };
            // коэффициент диффузного отражения
            float[] m_ambient = new float[] { 0.0f, 0.2f, 0.0f, 1 };
            // коэффициент фонового отражения
            float[] m_specular = new float[] { 0.3f, 0.2f, 0.3f, 1 };
            // коэффициент зеркального отражения
            float m_shininess = 1; // степень зеркального отражения (для модели Фонга)
            // устанавливаем характеристики материала
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, m_diffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, m_ambient);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, m_specular);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, m_shininess);
        }
        /// <summary>
        /// управление источником света
        /// </summary>
        private void SetupLight()
        {
            // задаем характеристики источника света
            float[] lightDirection = new float[] { 2, 2, 4, 0 };
            float[] l_diffuse = new float[] { 0.5f, 0.5f, 0.5f, 1 };
            float[] l_ambient = new float[] { 0.5f, 0.5f, 0.5f, 1 };
            float[] l_specular = new float[] { 0.5f, 0.5f, 0.5f, 1 };
            // устанавливаем характеристики источника света
            GL.Light(LightName.Light0, LightParameter.Position, lightDirection);
            //GL.Light(LightName.Light0, LightParameter.Diffuse, l_diffuse);
            //GL.Light(LightName.Light0, LightParameter.Ambient, l_ambient);
            //GL.Light(LightName.Light0, LightParameter.Specular, l_specular);
            // включаем освещение
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
        }
        /// <summary>
        /// Установка камеры
        /// </summary>
        private void SetupCamera()
        {
            // скорость вращения
            float rotationSpeed = 0.25f;
            GL.LoadIdentity();
            // задаем позицию камеры
            float cameraPositionX = 10;
            float cameraPositionY = 4;// 12;
            float cameraPositionZ =  8;
            // вычисляем угол вращения
            rotationAngle = (rotationAngle + rotationSpeed) % 360;
            // устанавливаем камеру
            Matrix4 lookat = Matrix4.LookAt(cameraPositionX, cameraPositionY,
            cameraPositionZ, 0, 0, 0, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            GL.Rotate(rotationAngle, 0, 0, 1);
            GL.Translate(0, 0, 5 - trackBarValue);
        }
        /// <summary>
        /// Отрисовка поверхности
        /// </summary>
        /// <param name="time"></param>
        private void DrawSurface(double time = 0)
        {
            // переменные для определения поверхности
            float ScaleKof = 0.8f;
            const float ZNEAR = 1f;
            const float ZFAR = 40;
            const float FIELD_OF_VIEW = 60;
            float aspect = (float)Width / (float)Height;

            // очистка окна
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0, 0, 0, 1);
    
            // настройка проекции
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
                Matrix4 perspectiveMatrix =
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FIELD_OF_VIEW),
            aspect, ZNEAR, ZFAR);
            GL.LoadMatrix(ref perspectiveMatrix);

            // в модели Modelview рисуются все объекты 
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
                GL.LoadIdentity();
                 SetupLight();
            GL.PopMatrix();

            GL.Scale(ScaleKof, ScaleKof, ScaleKof);
                // при первом обращении к данной функции запишем команды
                // рисования поверхности в дисплейный список
            GL.CallList(displayList_id);
           
        }
        /// <summary>
        /// Создание поверхности
        /// </summary>
        void CreateDL()
        {
            int s_columns = 50;
            int s_rows = 50;
            float s_xMin = -10;
            float s_xMax = 10;
            float s_yMin = -10;
            float s_yMax = 10;
            
            CSinSurface pv = new CSinSurface();
            displayList_id = GL.GenLists(1);
            GL.NewList(displayList_id, ListMode.Compile);
            GL.Color3(255, 0, 0);
            // вычисляем шаг узлов сетки
            float dy = (s_yMax - s_yMin) / (s_rows - 1);
            float dx = (s_xMax - s_xMin) / (s_columns - 1);
            float у = s_yMin;
            // пробегаем по строкам сетки
            for (int row = 0; row < s_rows - 1; ++row, у += dy)
            {
                // кажждой строке будет соответствовать своя лента из треугольников
                GL.Begin(PrimitiveType.TriangleStrip);
                float x = s_xMin;
                // пробегаем по столбцам текущей строки
                for (int column = 0; column < s_columns - 1; ++column, x += dx)
                {
                    // вычисляем параметры вершины в узлах пары соседних вершин
                    // ленты из треугольников
                    Vertex v0 = pv.CalculateVertex(x, у + dy, time);
                    Vertex v1 = pv.CalculateVertex(x, у, time);
                    // задаем нормаль и координаты вершины на четной позиции
                    GL.Normal3(v0.nx, v0.ny, v0.nz);
                    GL.Vertex3(v0.x, v0.y, v0.z);
                    // задаем нормаль и координаты вершины на нечетной позиции
                    GL.Normal3(v1.nx, v1.ny, v1.nz);
                    GL.Vertex3(v1.x, v1.y, v1.z);
                }
                GL.End();
            }
            GL.EndList();
        }

    }
    /// <summary>
    ///  Создает поверхность из функции sin(x)/x
    /// </summary>
    class CSinSurface
    {
        double time = 0;
        // функция sinc=sin(x)/x
        public double Sinc(double x)
        {
            return (Math.Abs(x) < 1e-7) ? 1 : Math.Sin(x) / x;
        }
        // представление поверхности в виде функции F(x,y,z)=f(х,у) -z
        public double F(double x, double y, double z)
        {
            double r = Math.Sqrt(x * x + y * y);
            double f = Sinc(r) * Math.Cos(2 * Math.PI * time / 60);
            return f - z;
        }
        /// <summary>
        /// Вычисление отметки z для координат x y в момент времени time
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Vertex CalculateVertex(double x, double y, double time = 0)
        {
            this.time = time;
            Vertex resault = new Vertex();
            // вычисляем значение координаты z
            double r = Math.Sqrt(x * x + y * y);
            double z = Sinc(r);
            // "бесконечно малое" приращение аргумента
            // для численного дифференцирования
            double delta = 1e-6;
            // вычисляем значение функции в точке x,y,z
            float f = (float)F(x, y, z);
            // вычисляем приблизительные частные производные функции F по dx, dy, dz
            // их значения приблизительно равны координатам вектора
            // нормали к поверхности в точке (х, у, z)
            double dfdx = -(F(x + delta, y, z) - f) / delta;
            double dfdy = -(F(x, y + delta, z) - f) / delta;
            double dfdz = 1;
            // величина обратная длине ветора антиградиента
            double invLen = 1 / Math.Sqrt(dfdx * dfdx + dfdy * dfdy + dfdz * dfdz);
            // координаты вершины
            resault.x = (float)x; resault.y = (float)y; resault.z = (float)z;
            // приводим вектор нормали к единичной длине
            resault.nx = (float)(dfdx * invLen);
            resault.ny = (float)(dfdy * invLen);
            resault.nz = (float)(dfdz * invLen);
            return resault;
        }
    }
}