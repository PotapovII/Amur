//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace ViewerGL
{
    using CommonLib;
    using System;
    using System.Drawing;
    using OpenTK;
    using OpenTK.Graphics;
    using OpenTK.Graphics.OpenGL;
    /// <summary>
    /// ОО: IMesh - трехузловая сетка для работы с графикой
    /// Объекты вершинных массивов (VAO)
    /// </summary>
    public class RenderMesh2DArray 
    {
        /// <summary>
        /// размерность пространства точек
        /// </summary>
        int DimentionVertex = 3;
        /// <summary>
        /// размерность пространства цвета
        /// </summary>
        int DimentionColor = 4;
        /// <summary>
        /// Сетка
        /// </summary>
        protected IRenderMeshGL mesh = null;
        /// <summary>
        /// инддексы вершин в КЭ сетке
        /// </summary>
        protected uint[] indexes = null;    
        /// <summary>
        /// Координаты вершин в КЭ сетке
        /// </summary>
        protected float[] vertexes = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh"></param>
        public RenderMesh2DArray(IRenderMeshGL mesh)
        {
            this.mesh = mesh;
            vertexes = mesh.GetCoords();
            indexes = mesh.GetAreaElems();
       

            //Print("vertexes", vertexes, 3);
            //Print("indexes", indexes, 3);
        }
        public void Print(string name, float[] mas,int shift)
        {
            Console.WriteLine(name);
            for (int i = 0; i < mas.Length / shift; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < shift; j++)
                {
                    int k = i * shift + j;
                    Console.Write(mas[k].ToString("F4") + " ");
                }
            }
            Console.WriteLine();
        }
        public void Print(string name, uint[] mas, int shift)
        {
            Console.WriteLine(name);
            for (int i = 0; i < mas.Length/shift; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < shift; j++)
                {
                    int k = i * shift + j;
                    Console.Write(mas[k].ToString("F4") + " ");
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Отрисовка данных через вершинные массивы
        /// </summary>
        public void DrawArray(float[] colors)
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
    }
}