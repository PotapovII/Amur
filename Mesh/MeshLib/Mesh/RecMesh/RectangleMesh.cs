namespace MeshLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: RectFDMesh - базистная разностная сетка в четырех гранной области 
    /// </summary>
    [Serializable]
    public class RectangleMeshTri : TriMesh
    {
        #region Поля
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public int Nx { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public int Ny { get; set; }
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public double dx { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public double dy { get; set; }
        /// <summary>
        /// Epks ctnrb
        /// </summary>
        public uint[][] map = null;
        #endregion
        public RectangleMeshTri(double Lx, double Ly, 
            int nx, int ny, int[] Mark)
        {
            CreateMesh(Lx, Ly, nx, ny, Mark);
        }
        /// <summary>
        /// Генерация триангуляционной КЭ сетки в прямоугольной области
        /// </summary>
        /// <param name="mesh">результат</param>
        /// <param name="Nx">узлов по Х</param>
        /// <param name="Ny">узлов по У</param>
        /// <param name="dx">шаг по Х</param>
        /// <param name="dy">шаг по У</param>
        /// <param name="Mark">признаки границ для ГУ</param>
        public void CreateMesh(double Lx, double Ly, 
            int nx, int ny, int[] Mark)
        {
            Nx = nx;
            Ny = ny;
            this.dx = Lx / (Nx - 1);
            this.dy = Ly / (Ny - 1);
            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);
            MEM.Alloc(CountElems, ref AreaElems);
            MEM.Alloc(CountNodes, ref CoordsX);
            MEM.Alloc(CountNodes, ref CoordsY);
            MEM.Alloc(counter, ref BoundElems);
            MEM.Alloc(counter, ref BoundKnots);
            MEM.Alloc(counter, ref BoundKnotsMark);
            MEM.Alloc(counter, ref BoundElementsMark);
            

            MEM.Alloc2D(nx, ny, ref map);
            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double ym = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double xm = dy * j;
                    CoordsX[k] = xm;
                    CoordsY[k] = ym;
                    map[i][j] = k++;
                }
            }
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]);
                    elem++;
                    AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i][j]);
                    elem++;
                }
            }
            k = 0;
            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[Nx - 1][i], map[Nx - 1][i + 1]);
                BoundElementsMark[k] = Mark[0];
                // задана функция
                BoundKnotsMark[k] = Mark[0];
                BoundKnots[k++] = (int)map[Nx - 1][i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[Nx - 1 - i][Ny - 1], map[Nx - 2 - i][Ny - 1]);
                BoundElementsMark[k] = Mark[1];
                // задана производная
                BoundKnotsMark[k] = Mark[1];
                BoundKnots[k++] = (int)map[Nx - 1 - i][Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[0][Ny - i - 1], map[0][Ny - i - 2]);
                BoundElementsMark[k] = Mark[2];
                // задана производная
                BoundKnotsMark[k] = Mark[2];
                BoundKnots[k++] = (int)map[0][Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[i][0], map[i + 1][0]);
                BoundElementsMark[k] = Mark[3];
                // задана функция
                BoundKnotsMark[k] = Mark[3];
                BoundKnots[k++] = (int)map[i][0];
            }
        }
    }
    /// <summary>
    /// ОО: RectFDMesh - базистная разностная сетка в четырех гранной области 
    /// </summary>
    [Serializable]
    public class RectangleMesh : RenderMesh
    {
        #region Поля
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public int Nx { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public int Ny { get; set; }
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public double dx { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public double dy { get; set; }
        /// <summary>
        /// Epks ctnrb
        /// </summary>
        public uint[][] map = null;
        #endregion
        public RectangleMesh(double Lx, double Ly,
            int nx, int ny, int[] Mark)
        {
            CreateMesh(Lx, Ly, nx, ny, Mark);
        }
        /// <summary>
        /// Генерация триангуляционной КЭ сетки в прямоугольной области
        /// </summary>
        /// <param name="mesh">результат</param>
        /// <param name="Nx">узлов по Х</param>
        /// <param name="Ny">узлов по У</param>
        /// <param name="dx">шаг по Х</param>
        /// <param name="dy">шаг по У</param>
        /// <param name="Mark">признаки границ для ГУ</param>
        public void CreateMesh(double Lx, double Ly,
            int nx, int ny, int[] Mark)
        {
            Nx = nx;
            Ny = ny;
            this.dx = Lx / (Nx - 1);
            this.dy = Ly / (Ny - 1);
            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);
            MEM.Alloc(CountElems, ref AreaElems);
            MEM.Alloc(CountNodes, ref CoordsX);
            MEM.Alloc(CountNodes, ref CoordsY);
            MEM.Alloc(counter, ref BoundElems);
            MEM.Alloc(counter, ref BoundKnots);
            MEM.Alloc(counter, ref BoundKnotsMark);
            MEM.Alloc(counter, ref BoundElementsMark);

            MEM.Alloc2D(nx, ny, ref map);
            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double ym = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double xm = dy * j;
                    CoordsX[k] = xm;
                    CoordsY[k] = ym;
                    map[i][j] = k++;
                }
            }
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]);
                    elem++;
                    AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i][j]);
                    elem++;
                }
            }
            k = 0;
            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[Nx - 1][i], map[Nx - 1][i + 1]);
                BoundElementsMark[k] = Mark[0];
                // задана функция
                BoundKnotsMark[k] = Mark[0];
                BoundKnots[k++] = (int)map[Nx - 1][i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[Nx - 1 - i][Ny - 1], map[Nx - 2 - i][Ny - 1]);
                BoundElementsMark[k] = Mark[1];
                // задана производная
                BoundKnotsMark[k] = Mark[1];
                BoundKnots[k++] = (int)map[Nx - 1 - i][Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[0][Ny - i - 1], map[0][Ny - i - 2]);
                BoundElementsMark[k] = Mark[2];
                // задана производная
                BoundKnotsMark[k] = Mark[2];
                BoundKnots[k++] = (int)map[0][Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[k] = new TwoElement(map[i][0], map[i + 1][0]);
                BoundElementsMark[k] = Mark[3];
                // задана функция
                BoundKnotsMark[k] = Mark[3];
                BoundKnots[k++] = (int)map[i][0];
            }
        }
    }

}
