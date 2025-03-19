/// ---------------------------------------------------------------
/// Реплика с моего кода 1993 года (язык С), из пакета 
///     по расчету полимеризации топлива в РДДТ 
/// ---------------------------------------------------------------    
///               только генерация сетки
///                     Потапов И.И. 
///                      13.03.2025
/// ---------------------------------------------------------------
namespace MeshGeneratorsLib.SPIN
{
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    public class StepGenerator
    {
        //  4---------------(3)-------------3      ---  ---
        //  |                               |       |    |
        //  |                               |       |    |
        // (4) Ny2         [1]             (2)      H2   |
        //  |                               |       |    H
        //  |     Nx1           Nx2         |       |    |    ===>>>>   
        //  0----(0)---1----------(1)-------2      ---   |  
        //             |                    |       |    |
        //  ^ Y,j     (5) Ny1  [2]  (7)     |       H1   |
        //  |          |                    |       |    |
        //  0===> X,i  5--------(6)---------6      -------- 
        //        i
        //  |---L1---|--------L2----------|
        //  |---------------L-------------|
        /// <summary>
        /// Высота ступени и канала
        /// </summary>
        public double H1, H2;
        /// <summary>
        /// Длина ступени и канала
        /// </summary>
        public double L1, L2;
        /// <summary>
        /// Количество узлов по Х в области, ступени и канала
        /// </summary>
        public int Nx, Nx1, Nx2;
        /// <summary>
        /// Количество узлов по Y в области, ступени и канала
        /// </summary>
        public int Ny, Ny1, Ny2;
        public StepGenerator() {}
        public void Set(int Nx1, int Nx2, int Ny1, int Ny2,
                        double H1, double H2, double L1, double L2)
        {
            this.Nx1 = Nx1;
            this.Nx2 = Nx2;
            this.Ny1 = Ny1;
            this.Ny2 = Ny2;
            this.H1 = H1;
            this.H2 = H2;
            this.L1 = L1;
            this.L2 = L2;
            Nx = Nx1 + Nx2 - 1;
            Ny = Ny1 + Ny2 - 1;
        }
        public void GetMesh(ref ComplecsMesh mesh)
        {
            int CountNodes = Nx * Ny - Nx1 * Ny1;
            int CountElems = 2 * (Nx - 1) * (Ny - 1) - 2 * Nx1 * Ny1;
            int bcCounter = 2 * (Nx-1) + 2 * (Ny-1);

            mesh = new ComplecsMesh();
            MEM.Alloc(CountNodes, ref mesh.CoordsX, "mesh.CoordsX");
            MEM.Alloc(CountNodes, ref mesh.CoordsY, "mesh.CoordsY");

            MEM.Alloc(CountElems, 3, ref mesh.AreaElems, "mesh.AreaElems");
            MEM.Alloc(CountElems, ref mesh.AreaElemsFFType, "mesh.AreaElemsFFType");

            MEM.Alloc(bcCounter, 2, ref mesh.BoundElems, "mesh.BoundElems");
            MEM.Alloc(bcCounter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");
            MEM.Alloc(bcCounter, ref mesh.BoundKnots, "mesh.BoundKnots");
            MEM.Alloc(bcCounter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");


            //  4---------------(3)-------------3      ---  ---
            //  |                               |       |    |
            //  |                               |       |    |
            // (4) Ny2         [1]             (2)      H2   |
            //  |                               |       |    H
            //  |     Nx1           Nx2         |       |    |     
            //  0----(0)---1----------(1)-------2      ---   |
            //             |                    |       |    |
            //  ^ Y,j     (5) Ny1  [2]  (7)     |       H1   |
            //  |          |                    |       |    |
            //  0===> X,i  5--------(6)---------6      -------- 
            //        i
            //  |---L1---|--------L2----------|
            //  |---------------L-------------|
            int[][] map = null;
            MEM.Alloc(Nx, Ny, ref map);
            //double[][] mapX = null, mapY = null;
            //MEM.Alloc(Nx, Ny, ref mapX);
            //MEM.Alloc(Nx, Ny, ref mapY);
            double dx1 = L1 / Nx1;
            double dx2 = L2 / (Nx2 - 2);
            double dy1 = H1 / Ny1; 
            double dy2 = H2 / (Ny2 - 2);
            double x, y;
            int knot = 0;
            // создание карты сетки
            for (int i = 0; i < Nx; i++)
            {
                if (i < Nx1)
                    x = i * dx1;
                else
                    x = L1 + (i - Nx1) * dx2;
                for (int j = 0; j < Ny; j++)
                {
                    if (j < Ny1)
                        y = j * dy1;
                    else
                        y = H1 + (j - Ny1) * dy2;

                    if (i < Nx1 && j < Ny1)
                        map[i][j] = -1;
                    else
                    {
                        mesh.CoordsX[knot] = x;
                        mesh.CoordsY[knot] = y;
                        //mapX[i][j] = x;
                        //mapY[i][j] = y;
                        map[i][j] = knot++;
                    }
                }
            }
            
            LOG.Print("map", map);
            //LOG.Print("mapX", mapX);
            //LOG.Print("mapY", mapY);
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    if (i < Nx1 && j < Ny1)
                        continue;
                    mesh.AreaElems[elem][0] = (uint)map[i][ j];
                    mesh.AreaElems[elem][1] = (uint)map[i + 1][j];
                    mesh.AreaElems[elem][2] = (uint)map[i + 1][j + 1];
                    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                    elem++;
                    mesh.AreaElems[elem][0] = (uint)map[i + 1][j + 1];
                    mesh.AreaElems[elem][1] = (uint)map[i][j + 1];
                    mesh.AreaElems[elem][2] = (uint)map[i][j];
                    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                    elem++;
                }
            }
            //LOG.Print("mesh.AreaElems", mesh.AreaElems,0);
            int k = 0;
            //// низ угол из 3 частей
            for (int i = 0; i < Nx1; i++)
            {
                mesh.BoundElems[k][0] = (uint)map[i][Ny1];
                mesh.BoundElems[k][1] = (uint)map[i + 1][Ny1];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[i][Ny1];
            }
            for (int j = 0; j < Ny1; j++)
            {
                mesh.BoundElems[k][0] = (uint)map[Nx1][Ny1 - j];
                mesh.BoundElems[k][1] = (uint)map[Nx1][Ny1 - j - 1];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[Nx1][Ny1 - j];
            }
            for (int i = Nx1; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = (uint)map[i][0];
                mesh.BoundElems[k][1] = (uint)map[i + 1][0];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[i][0];
            }
            // правая сторона
            for (int j = 0; j < Ny - 1; j++)
            {
                mesh.BoundElems[k][0] = (uint)map[Nx - 1][j];
                mesh.BoundElems[k][1] = (uint)map[Nx - 1][j + 1];
                mesh.BoundElementsMark[k] = 1;
                // задана производная
                mesh.BoundKnotsMark[k] = 1;
                mesh.BoundKnots[k++] = (int)map[Nx - 1][j];
            }
            // верх
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = (uint)map[Nx - i - 1][Ny - 1];
                mesh.BoundElems[k][1] = (uint)map[Nx - i - 2][Ny - 1];
                mesh.BoundElementsMark[k] = 2;
                // задана производная
                mesh.BoundKnotsMark[k] = 2;
                mesh.BoundKnots[k++] = (int)map[Nx - i - 1][Ny- 1];
            }
            // левая сторона
            for (int j = 0; j < Ny2 - 2; j++)
            {
                mesh.BoundElems[k][0] = (uint)map[0][Ny - 1 - j];
                mesh.BoundElems[k][1] = (uint)map[0][Ny - 2 - j];
                mesh.BoundElementsMark[k] = 3;
                // задана функция
                mesh.BoundKnotsMark[k] = 3;
                mesh.BoundKnots[k++] = (int)map[0][Ny - 1 - j];
            }
            //LOG.Print("mesh.BoundElems", mesh.BoundElems, 0);
            //LOG.Print("map", map);
            //LOG.Print("mesh.BoundKnots", mesh.BoundKnots);
            //LOG.Print("mesh.BoundKnotsMark", mesh.BoundKnotsMark);
        }

        public void GetMesh(ref TriMesh mesh)
        {
            int CountNodes = Nx * Ny - Nx1 * Ny1;
            int CountElems = 2 * (Nx - 1) * (Ny - 1) - 2 * Nx1 * Ny1;
            int bcCounter = 2 * (Nx - 1) + 2 * (Ny - 1);
            mesh = new TriMesh();
            MEM.Alloc(CountNodes, ref mesh.CoordsX, "mesh.CoordsX");
            MEM.Alloc(CountNodes, ref mesh.CoordsY, "mesh.CoordsY");

            MEM.Alloc(CountElems, ref mesh.AreaElems, "mesh.AreaElems");

            MEM.Alloc(bcCounter, ref mesh.BoundElems, "mesh.BoundElems");
            MEM.Alloc(bcCounter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");
            MEM.Alloc(bcCounter, ref mesh.BoundKnots, "mesh.BoundKnots");
            MEM.Alloc(bcCounter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");


            //  4---------------(3)-------------3      ---  ---
            //  |                               |       |    |
            //  |                               |       |    |
            // (4) Ny2         [1]             (2)      H2   |
            //  |                               |       |    H
            //  |     Nx1           Nx2         |       |    |     
            //  0----(0)---1----------(1)-------2      ---   |
            //             |                    |       |    |
            //  ^ Y,j     (5) Ny1  [2]  (7)     |       H1   |
            //  |          |                    |       |    |
            //  0===> X,i  5--------(6)---------6      -------- 
            //        i
            //  |---L1---|--------L2----------|
            //  |---------------L-------------|
            int[][] map = null;
            MEM.Alloc(Nx, Ny, ref map);
            //double[][] mapX = null, mapY = null;
            //MEM.Alloc(Nx, Ny, ref mapX);
            //MEM.Alloc(Nx, Ny, ref mapY);
            double dx1 = L1 / Nx1;
            double dx2 = L2 / (Nx2 - 2);
            double dy1 = H1 / Ny1;
            double dy2 = H2 / (Ny2 - 2);
            double x, y;
            int knot = 0;
            // создание карты сетки
            for (int i = 0; i < Nx; i++)
            {
                if (i < Nx1)
                    x = i * dx1;
                else
                    x = L1 + (i - Nx1) * dx2;
                for (int j = 0; j < Ny; j++)
                {
                    if (j < Ny1)
                        y = j * dy1;
                    else
                        y = H1 + (j - Ny1) * dy2;

                    if (i < Nx1 && j < Ny1)
                        map[i][j] = -1;
                    else
                    {
                        mesh.CoordsX[knot] = x;
                        mesh.CoordsY[knot] = y;
                        //mapX[i][j] = x;
                        //mapY[i][j] = y;
                        map[i][j] = knot++;
                    }
                }
            }

            //LOG.Print("map", map);
            //LOG.Print("mapX", mapX);
            //LOG.Print("mapY", mapY);
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    if (i < Nx1 && j < Ny1)
                        continue;
                    mesh.AreaElems[elem][0] = (uint)map[i][j];
                    mesh.AreaElems[elem][1] = (uint)map[i + 1][j];
                    mesh.AreaElems[elem][2] = (uint)map[i + 1][j + 1];
                    elem++;
                    mesh.AreaElems[elem][0] = (uint)map[i + 1][j + 1];
                    mesh.AreaElems[elem][1] = (uint)map[i][j + 1];
                    mesh.AreaElems[elem][2] = (uint)map[i][j];
                    elem++;
                }
            }
            //LOG.Print("mesh.AreaElems", mesh.AreaElems,0);
            int k = 0;
            //// низ угол из 3 частей
            for (int i = 0; i < Nx1; i++)
            {
                mesh.BoundElems[k][0] = (uint)map[i][Ny1];
                mesh.BoundElems[k][1] = (uint)map[i + 1][Ny1];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[i][Ny1];
            }
            for (int j = 0; j < Ny1; j++)
            {
                mesh.BoundElems[k][0] = (uint)map[Nx1][Ny1 - j];
                mesh.BoundElems[k][1] = (uint)map[Nx1][Ny1 - j - 1];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[Nx1][Ny1 - j];
            }
            for (int i = Nx1; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = (uint)map[i][0];
                mesh.BoundElems[k][1] = (uint)map[i + 1][0];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[i][0];
            }
            // правая сторона
            for (int j = 0; j < Ny - 1; j++)
            {
                mesh.BoundElems[k][0] = (uint)map[Nx - 1][j];
                mesh.BoundElems[k][1] = (uint)map[Nx - 1][j + 1];
                mesh.BoundElementsMark[k] = 1;
                // задана производная
                mesh.BoundKnotsMark[k] = 1;
                mesh.BoundKnots[k++] = (int)map[Nx - 1][j];
            }
            // верх
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = (uint)map[Nx - i - 1][Ny - 1];
                mesh.BoundElems[k][1] = (uint)map[Nx - i - 2][Ny - 1];
                mesh.BoundElementsMark[k] = 2;
                // задана производная
                mesh.BoundKnotsMark[k] = 2;
                mesh.BoundKnots[k++] = (int)map[Nx - i - 1][Ny - 1];
            }
            // левая сторона
            for (int j = 0; j < Ny2 - 2; j++)
            {
                mesh.BoundElems[k][0] = (uint)map[0][Ny - 1 - j];
                mesh.BoundElems[k][1] = (uint)map[0][Ny - 2 - j];
                mesh.BoundElementsMark[k] = 3;
                // задана функция
                mesh.BoundKnotsMark[k] = 3;
                mesh.BoundKnots[k++] = (int)map[0][Ny - 1 - j];
            }
            //LOG.Print("mesh.BoundElems", mesh.BoundElems);
            //LOG.Print("map", map);
            //LOG.Print("mesh.BoundKnots", mesh.BoundKnots);
            //LOG.Print("mesh.BoundKnotsMark", mesh.BoundKnotsMark);
        }

    }
}
