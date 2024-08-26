namespace ViewerGL
{
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    using OpenTK.Input;
    /// <summary>
    /// Урок 2 - дисплейные списки
    /// </summary>
    public class GLMain_02_DisplayList : GameWindow
    {
        /// <summary>
        /// код дисплейного списка
        /// </summary>
        int displayList_ID = -1;
        public GLMain_02_DisplayList(int WinSizeX = 812, 
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

            // включение отброски отрисовки задних граней
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            //GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);

            // создание дисплейного списка
            displayList_ID = CreateDisplayList();

            // проекция мировых координат на плоскость экрана
            GL.MatrixMode(MatrixMode.Projection);
            // установка единичной матрицы
            GL.LoadIdentity();
        }
        protected override void OnUnload(EventArgs e)
        {
            DellDiaplayList();
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

        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
     
            // вращение объектов отрисовки
            GL.Rotate(1.5f, 0, 0, 1);
            GL.PushMatrix();
            // отрисовка дисплейного списка
            GL.CallList(displayList_ID);
            GL.PushMatrix();
            SwapBuffers();
        }
        /// <summary>
        /// Удаление дисплейного списка по его
        /// id из памяти видеокарты
        /// </summary>
        /// <param name="namberDisplayList">
        /// номер дисплейного списка</param>
        public void DellDiaplayList(int namberDisplayList = 1)
        {
            GL.DeleteLists(displayList_ID, namberDisplayList);
        }
        /// <summary>
        /// Создаем дисплейный список и 
        /// отправляем его в память видеокарты
        /// </summary>
        /// <returns></returns>
        public int CreateDisplayList()
        {
            float b = 0.5f;
            // создаем id списка
            int id = GL.GenLists(1);
            GL.Color3(Color.Red);
            // создаем дисплейный список
            GL.NewList(id, ListMode.Compile);
            
            // создаем данные для дисплейного списока
            GL.Begin(PrimitiveType.Triangles);
                    GL.Vertex2(-b, -b);
                    GL.Vertex2(b, -b);
                    GL.Vertex2(-b, b);

                    GL.Vertex2(-b, b);
                    GL.Vertex2(b, -b);
                    GL.Vertex2(b, b);
                GL.End();
            GL.EndList();
            return id;
        }
    }
}