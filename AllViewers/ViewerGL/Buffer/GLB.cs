//---------------------------------------------------------------------------
//                       ПРОЕКТ "GLViewer"
//                         проектировщик:
//                           Потапов И.И. 
//         кодировка : 11.12.2022 Потапов И.И. (c# & OpenTK)
//---------------------------------------------------------------------------
namespace ViewerGL
{
    using OpenTK.Graphics.OpenGL;
    /// <summary>
    /// Общие методы работы с буфферами для отрисовки треугольников
    /// и не только
    /// </summary>
    public static class GLB
    {
        public const int DimentionColor = 4;
        public const int DimentionVertex = 3;
        public const int shift = 0;
        public const int proxyArray = 0;

        #region Работа с VBO
        /// <summary>
        /// Создание VBO буфера
        /// </summary>
        /// <param name="data">массив копируемый 
        /// в буфер видеопамяти/param>
        /// <returns>id буфера</returns>
        public static int CreateVBO(float[] data)
        {
            // получение id буфера vbo 
            int vbo = GL.GenBuffer();
            // начало работы с id - м буфером VBO  
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // отправка данных в видеопамять
            GL.BufferData(BufferTarget.ArrayBuffer,
                data.Length * sizeof(float), data,
                BufferUsageHint.StaticDraw);
            // завершение работы с буфером 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            return vbo;
        }
        /// <summary>
        /// Создание EBO буфера
        /// </summary>
        /// <param name="data">массив копируемый 
        /// в буфер видеопамяти/param>
        /// <returns>id буфера</returns>
        public static int CreateEBO(uint[] data)
        {
            // получение id индексного буфера ebo 
            int ebo = GL.GenBuffer();
            // начало работы с id - м индексным буфером VBO  
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            // отправка данных в видеопамять
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                data.Length * sizeof(uint), data,
                BufferUsageHint.StaticDraw);
            // завершение работы с буфером 
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            return ebo;
        }
        /// <summary>
        /// Изменение всех данных в id - м  буфере VBO 
        /// </summary>
        /// <param name="vbo">id буфера</param>
        /// <param name="data">массив копируемый в 
        /// буфер видеопамяти</param>
        public static void ChangeData(int vbo, float[] data)
        {
            // начинаем работать с буфером 
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // отправка данных в видеопамять
            GL.BufferData(BufferTarget.ArrayBuffer,
                data.Length * sizeof(float), data,
                BufferUsageHint.DynamicDraw);
            // завершение работы с буфером 
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        /// <summary>
        /// Создание VBO буфферов для отрисовки 
        /// без учета нормалей
        /// </summary>
        /// <param name="indexes">вершинне индексы</param>
        /// <param name="vertexes">координаты вершин</param>
        /// <param name="colors">цвета вершин</param>
        /// <param name="eboIndexID">id вершинного буфера</param>
        /// <param name="vboCoordID">id буфера координат</param>
        /// <param name="vboColorID">id буфера цветов</param>
        public static void CreateVBO_IVC(uint[] indexes, float[] vertexes,
            float[] colors, ref int eboIndexID, ref int vboCoordID,
            ref int vboColorID)
        {
            eboIndexID = GLB.CreateEBO(indexes);
            vboCoordID = GLB.CreateVBO(vertexes);
            vboColorID = GLB.CreateVBO(colors);
        }
        /// <summary>
        /// Отрисовка данных из VBO буферов через вершинные массивы
        /// с возможной сменой цвета в VBO буфере 
        /// </summary>
        /// <param name="eboIndexID">id вершинного буфера</param>
        /// <param name="vboCoordID">id буфера координат</param>
        /// <param name="vboColorID">id буфера цветов</param>
        /// <param name="CountIndexes">Размерность индекса вершин</param>
        /// <param name="colors">цвета вершин</param>
        public static void DrawVBO_IVC(int eboIndexID,int vboCoordID, 
            int vboColorID, int CountIndexes, float[] colors = null)
        {
            if (colors != null)
                ChangeData(vboColorID, colors);
            // Начинаем работу 
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            // вершины
            // начинаем работать с буфером массива вершин
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboIndexID);
            // начинаем работать с буфером координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboCoordID);
            // Читаем данные о массиве координат из буфера координат
            GL.VertexPointer(DimentionVertex,
               VertexPointerType.Float, shift, proxyArray);
            // цвета вершин
            // начинаем работать с буфером цвета
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboColorID);
            // Читаем данные из буферного массива цветов
            GL.ColorPointer(DimentionColor,
            ColorPointerType.Float, shift, proxyArray);
            // ОТРИСОВКА ПО ЭЛЕМЕНТАМ
            GL.DrawElements(PrimitiveType.Triangles, CountIndexes,
                DrawElementsType.UnsignedInt, 0);
            // Выключаем использование вершинного
            // массива координат
            GL.DisableClientState(ArrayCap.VertexArray);
            // Выключаем использование вершинного
            // массива цветов
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.BindVertexArray(0);
        }
        /// <summary>
        /// Освобождение памяти в видеокарте
        /// </summary>
        /// <param name="eboIndexID">id вершинного буфера</param>
        /// <param name="vboCoordID">id буфера координат</param>
        /// <param name="vboColorID">id буфера цветов</param>
        public static void DelleteVBO_IVC(int eboIndexID, 
            int vboCoordID, int vboColorID)
        {
            if(eboIndexID>0)
                GL.DeleteBuffer(eboIndexID);
            if(vboCoordID>0)
                GL.DeleteBuffer(vboCoordID);
            if(vboColorID>0)
                GL.DeleteBuffer(vboColorID);
        }
        #endregion VBO

        #region Работа с VAO

        /// <summary>
        /// Создание VBO буфферов для отрисовки 
        /// без учета нормалей
        /// </summary>
        /// <param name="indexes">вершинне индексы</param>
        /// <param name="vertexes">координаты вершин</param>
        /// <param name="colors">цвета вершин</param>
        /// <param name="eboIndexID">id вершинного буфера</param>
        /// <param name="vboCoordID">id буфера координат</param>
        /// <param name="vboColorID">id буфера цветов</param>
        /// <returns>id VAO буффера</returns>
        public static int CreateVAO_IVC(uint[] indexes, float[] vertexes,
            float[] colors, ref int eboIndexID, ref int vboCoordID,
            ref int vboColorID)
        {
            int vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);
            eboIndexID = GLB.CreateEBO(indexes);
            vboCoordID = GLB.CreateVBO(vertexes);
            vboColorID = GLB.CreateVBO(colors);
            // Включаем использование вершинного масивов и масивов цветов
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            // начинаем работать с буфером массива вершин
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboIndexID);
            // начинаем работать с буфером координат
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboCoordID);
            // Читаем данные о массиве координат из буфера координат
            GL.VertexPointer(DimentionVertex,
               VertexPointerType.Float, shift, proxyArray);
            // цвета вершин
            // начинаем работать с буфером цвета
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboColorID);
            // Читаем данные из буферного массива цветов
            GL.ColorPointer(DimentionColor,
            ColorPointerType.Float, shift, proxyArray);

            // зевершаем работать с настройкой VAO
            GL.BindVertexArray(0);
            // завершение работы с буфферами VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Выключаем использование вершинного
            // массива координат
            GL.DisableClientState(ArrayCap.VertexArray);
            // Выключаем использование вершинного
            // массива цветов
            GL.DisableClientState(ArrayCap.ColorArray);
            return vaoID;
        }

        /// <summary>
        /// начало работы с VAO
        /// </summary>
        /// <param name="vaoID">id VAO буффера</param>
        public static void BindVAO(int vaoID)
        {
            GL.BindVertexArray(vaoID);
        }
        /// <summary>
        /// отрисовка из буффера VAO
        /// </summary>
        /// <param name="CountIndexes">Размерность индекса вершин</param>
        /// <param name="vboCoordID">id буфера координат</param>
        /// <param name="colors">цвета вершин</param>
        public static void DrawVAO_IVC(int CountIndexes, int vboColorID, float[] colors)
        {
            // смена цвета вершин
            if (colors != null)
                ChangeData(vboColorID, colors);
            GL.DrawElements(PrimitiveType.Triangles, 
                CountIndexes,
                DrawElementsType.UnsignedInt, 0);
        }
        /// <summary>
        /// конец работы с VAO
        /// </summary>
        /// <param name="vaoID"></param>
        public static void UnBindVAO()
        {
            GL.BindVertexArray(0);
        }
        /// <summary>
        /// Удаление буфера VAO
        /// </summary>
        /// <param name="vaoID"></param>
        /// <param name="eboIndexID"></param>
        /// <param name="vboCoordID"></param>
        /// <param name="vboColorID"></param>
        public static void DelleteVAO_IVC(int vaoID, int eboIndexID,
          int vboCoordID, int vboColorID)
        {
            // удаление данных VAO из ВК
            if(vaoID>0)
                GL.DeleteVertexArray(vaoID);
            if (eboIndexID > 0)
                GL.DeleteBuffer(eboIndexID);
            if (vboCoordID > 0)
                GL.DeleteBuffer(vboCoordID);
            if (vboColorID > 0)
                GL.DeleteBuffer(vboColorID);
        }
        #endregion 

        #region Работа с вершинными массивами 
        /// <summary>
        /// Отрисовка данных через вершинные массивы 
        /// </summary>
        /// <param name="indexes">вершинне индексы</param>
        /// <param name="vertexes">координаты вершин</param>
        /// <param name="colors">цвета вершин</param>
        public static void DrawTriangles(uint[] indexes, 
            float[] vertexes, float[] colors)
        {
            // Начинаем работу с вершинным массивом координат
            GL.EnableClientState(ArrayCap.VertexArray);
            // Начинаем работу с вершинным массивом цветов вершин
            GL.EnableClientState(ArrayCap.ColorArray);
            // Читаем данные из массива координат
            GL.VertexPointer(DimentionVertex,
                VertexPointerType.Float, 0, vertexes);
            // Читаем данные из массива цветов
            GL.ColorPointer(DimentionColor,
                ColorPointerType.Float, 0, colors);
            // ОТРИСОВКА ПО ЭЛЕМЕНТАМ
            GL.DrawElements(PrimitiveType.Triangles, indexes.Length,
                DrawElementsType.UnsignedInt, indexes);
            // Выключаем работу с вершинным массивом цветов
            GL.DisableClientState(ArrayCap.ColorArray);
            // Выключаем работу с вершинным массивом координат
            GL.DisableClientState(ArrayCap.VertexArray);
        }
        /// <summary>
        /// Отрисовка данных через вершинные массивы 
        /// </summary>
        /// <param name="indexes">вершинне индексы</param>
        /// <param name="vertexes">координаты вершин</param>
        /// <param name="colors">цвета вершин</param>
        /// <param name="normals">нормали к поверхности</param>
        public static void DrawArrayNormalTriangles(uint[] indexes, 
            float[] vertexes, float[] colors, float[] normals)
        {
            // Начинаем работу с вершинным массивом координат,...
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            // Читаем данные из массива координат
            GL.VertexPointer(DimentionVertex,
                VertexPointerType.Float, shift, vertexes);
            GL.NormalPointer(NormalPointerType.Float, shift, normals);
            // Читаем данные из массива цветов
            GL.ColorPointer(DimentionColor,
                ColorPointerType.Float, shift, colors);
            // ОТРИСОВКА ПО ЭЛЕМЕНТАМ
            GL.DrawElements(PrimitiveType.Triangles, indexes.Length,
                DrawElementsType.UnsignedInt, indexes);
            // Выключаем работу с вершинным массивом цветов ...
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }

        /// <summary>
        /// Отрисовка данных через вершинные массивы 
        /// </summary>
        /// <param name="CountTriangles">количество треугольников</param>
        /// <param name="vertexes">координаты вершин</param>
        /// <param name="colors">цвета вершин</param>
        /// <param name="normals">нормали к поверхности</param>
        public static void DrawArrayNormal(int CountTriangles, 
            float[] vertexes, float[] colors, float[] normals)
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.VertexPointer(DimentionVertex, VertexPointerType.Float, shift, vertexes);
            GL.NormalPointer(NormalPointerType.Float, shift, normals);
            GL.ColorPointer(DimentionColor, ColorPointerType.Float, shift, colors);
            GL.DrawArrays(PrimitiveType.TriangleFan, shift, CountTriangles);
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }
        #endregion

        /// <summary>
        /// Установка цвета в массив цветов
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="R">красный цвет от 0 до 1</param>
        /// <param name="G">зеленый цвет от 0 до 1</param>
        /// <param name="B">голубой цвет от 0 до 1</param>
        /// <param name="Alpha">прозрачность от 0 до 1</param>
        public static void SetColor4(float[] colors,
        float R = 1f, float G = 1f, float B = 1f, float Alpha = 1f)
        {
            int k = 0;
            for (int i = 0; i < colors.Length / 4; i++)
            {
                colors[k] = R;
                colors[k + 1] = G;
                colors[k + 2] = B;
                colors[k + 3] = Alpha;
                k += 4;
            }
        }

    }
}
