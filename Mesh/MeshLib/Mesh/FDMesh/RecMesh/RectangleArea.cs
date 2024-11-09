//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И. 
//                кодировка : 19.10.2024 Потапов И.И. 
//---------------------------------------------------------------------------
namespace MeshLib.TaskArea
{
    using System;

    using MeshLib;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: RectangleMesh - базисная разностная сетка с
    ///   постоянным шагом в прямоугольной области 
    ///       для реализации тестовых задач
    /// 
    ///  ----0-----------------------------> y (j)
    ///      |                         |  
    ///     Lx                         |
    ///      |                         |
    ///      |                         |
    ///      |---------- Ly -----------|
    ///      |
    ///      V x (i)     
    /// 
    public class RectangleArea
    {
        /// <summary>
        /// Узлов по х
        /// </summary>
        public int Nx = 60;
        /// <summary>
        /// Узлов по y
        /// </summary>
        public int Ny = 30;
        /// <summary>
        /// Ширина области
        /// </summary>
        public double Lx = 2;
        /// <summary>
        /// Высота области
        /// </summary>
        public double Ly = 1;
        /// <summary>
        /// к. релаксации
        /// </summary>
        public double w = 0.3;
        /// <summary>
        /// Шаг сетки и площадь ячейки
        /// </summary>
        public double dx, dy, dS;
        public RectangleArea(int Nx, int Ny, double Lx, double Ly, double w = 0.3)
        {
            this.Nx = Nx;
            this.Ny = Ny;
            this.Lx = Lx;
            this.Ly = Ly;
            this.w = w;
            dx = Lx / (Nx - 1);
            dy = Ly / (Ny - 1);
            dS = dx * dy;
        }
        /// <summary>
        /// Генерация объекта симпл - сетки
        /// </summary>
        /// <param name="DEGUG"></param>
        /// <returns></returns>
        public IMesh CreateMesh(bool DEGUG = false)
        {
            TriMesh mesh = new TriMesh();
            uint[,] map = new uint[Nx, Ny];
            uint k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    map[i, j] = k++;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);
            MEM.Alloc(CountElems, ref mesh.AreaElems);
            k = 0;
            for (int i = 0; i < Nx - 1; i++)
                for (int j = 0; j < Ny - 1; j++)
                {
                    mesh.AreaElems[k++] = new TriElement(map[i, j], map[i + 1, j], map[i, j + 1]);
                    mesh.AreaElems[k++] = new TriElement(map[i + 1, j], map[i + 1, j + 1], map[i, j + 1]);
                }
            MEM.Alloc(Nx * Ny, ref mesh.CoordsX);
            MEM.Alloc(Nx * Ny, ref mesh.CoordsY);
            k = 0;
            for (int i = 0; i < Nx; i++)
            {
                double xx = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double yy = j * dy;
                    mesh.CoordsX[k] = xx;
                    mesh.CoordsY[k] = yy;
                    k++;
                }
            }
            int CountBC = 2 * Nx + 2 * Ny - 2;
            MEM.Alloc(CountBC, ref mesh.BoundElems);
            MEM.Alloc(CountBC, ref mesh.BoundElementsMark);
            MEM.Alloc(CountBC, ref mesh.BoundKnots);
            MEM.Alloc(CountBC, ref mesh.BoundKnotsMark);
            // дно
            uint be = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[be].Vertex1 = map[i, 0];
                mesh.BoundElems[be].Vertex2 = map[i + 1, 0];
                mesh.BoundElementsMark[be] = 0;
                be++;
            }
            uint bk = 0;
            for (int i = 0; i < Nx; i++)
            {
                mesh.BoundKnots[bk] = (int)map[i, 0];
                mesh.BoundKnotsMark[bk] = 0;
                bk++;
            }
            // правая стенка
            for (int j = 0; j < Ny - 1; j++)
            {
                mesh.BoundElems[be].Vertex1 = map[Nx - 1, j];
                mesh.BoundElems[be].Vertex2 = map[Nx - 1, j + 1];
                mesh.BoundElementsMark[be] = 1;
                be++;
            }
            for (int j = 1; j < Ny; j++)
            {
                mesh.BoundKnots[bk] = (int)map[Nx - 1, j];
                mesh.BoundKnotsMark[bk] = 1;
                bk++;
            }
            // свободная поверхность
            for (int i = Nx - 2; i > 0; i--)
            {
                mesh.BoundElems[be].Vertex1 = map[i + 1, Ny - 1];
                mesh.BoundElems[be].Vertex2 = map[i, Ny - 1];
                mesh.BoundElementsMark[be] = 2;
                be++;
            }
            for (int i = Nx - 2; i > 1; i--)
            {
                mesh.BoundKnots[bk] = (int)map[i, Ny - 1];
                mesh.BoundKnotsMark[bk] = 2;
                bk++;
            }
            // левая стенка
            for (int j = Ny - 2; j > 0; j--)
            {
                mesh.BoundElems[be].Vertex1 = map[0, j + 1];
                mesh.BoundElems[be].Vertex2 = map[0, j];
                mesh.BoundElementsMark[be] = 3;
                be++;
            }
            for (int j = Ny - 1; j > 1; j--)
            {
                mesh.BoundKnots[bk] = (int)map[0, j];
                mesh.BoundKnotsMark[bk] = 3;
                bk++;
            }
            if (DEGUG == true)
                mesh.Print();
            return mesh;
        }
        /// <summary>
        /// Конвертация данных с двумерной на одномерную сетку
        /// </summary>
        public void Convertor(ref double[] Result, double[,] arg)
        {
            MEM.Alloc(Nx * Ny, ref Result);
            int k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    Result[k++] = arg[i, j];
        }
        /// <summary>
        /// Конвертация данных с двумерной на одномерную сетку
        /// </summary>
        public void Convertor(ref double[] Result, double[][] arg)
        {
            MEM.Alloc(Nx * Ny, ref Result);
            int k = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    Result[k++] = arg[i][j];
        }
        public void Copy(ref double[,] a, double[,] b)
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    a[i, j] = b[i, j];
                }
        }
        public void Copy(ref double[][] a, double[][] b)
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    a[i][j] = b[i][j];
                }
        }
        public void Relax(ref double[,] a, double[,] b)
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    a[i, j] = a[i, j] * w + b[i, j] * (1 - w);
                }
        }
        public void Relax(ref double[][] a, double[][] b)
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    a[i][j] = a[i][j] * w + b[i][j] * (1 - w);
                }
        }
        public double Error(double[,] a, double[,] b)
        {
            double sum = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    sum += Math.Abs(a[i, j] - b[i, j]) / (Nx * Ny);
                }
            return sum;
        }
        public double Error(double[][] a, double[][] b)
        {
            double sum = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    sum += Math.Abs(a[i][j] - b[i][j]) / (Nx * Ny);
                }
            return sum;
        }
        /// <summary>
        /// Пример вывода данных
        /// </summary>
        //public void ExampleShowMesh()
        //{
        //    IMesh mesh = CreateMesh();
        //    if (mesh != null)
        //    {
        //        SavePoint data = new SavePoint();
        //        data.SetSavePoint(0, mesh);
        //        double[] xx = mesh.GetCoords(0);
        //        double[] yy = mesh.GetCoords(1);
        //        data.Add("Координата Х", xx);
        //        data.Add("Координата Y", yy);
        //        data.Add("Координаты ХY", xx, yy);
        //        Form form = new ViForm(data);
        //        form.ShowDialog();
        //    }
        //}
    }
}
