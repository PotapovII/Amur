namespace MeshLib.Mesh.RecMesh
{
    using System;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: RectFDMesh - базистная разностная сетка в четырех гранной области 
    /// </summary>
    [Serializable]
    public class ChannelRectangleMesh : ARectangleMesh
    {
        #region Поля
        
        #endregion
        public ChannelRectangleMesh(int Nx, int Ny, double Lx,
            double Ly, double Xmin = 0, double Ymin = 0)
                : base(Nx, Ny, Lx, Ly, Xmin, Ymin)
        {
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
        public override void CreateMesh(int[] Mark,  int[] Y_init = null, int nx = 25)
        {
            dx = Lx / (Nx - 1);
            dy = Ly / (Ny - 1);
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

            MEM.Alloc2D(Nx, Ny, ref map);
            MEM.Alloc2D(Nx, Ny, ref X);
            MEM.Alloc2D(Nx, Ny, ref Y);
            // 0 ------------------> j (y)
            // |
            // |    Система координат
            // |
            // |
            // V i (x)
            int k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = Xmin + i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = Ymin + dy * j;
                    CoordsX[k] = xm;
                    CoordsY[k] = ym;
                    map[i][j] = k++;
                    X[i][j] = xm;
                    Y[i][j] = ym;
                }
            }
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    // левый уклон сетки
                    AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i][j + 1]);
                    elem++;
                    AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                    elem++;
                    // правый уклон сетки
                    //AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]);
                    //elem++;
                    //AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i][j]);
                    //elem++;
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
            GetLinks();
        }
    }
}
