//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 06.07.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    using GeometryLib.Vector;
    using MemLogLib;
    /// <summary>
    /// ОО: ARectangleMesh - базистная разностная сетка в четырех гранной области 
    /// </summary>
    [Serializable]
    public abstract class ARectangleMesh : RenderMesh, IRectangleMesh
    {
        /// <summary>
        /// Связь между 1D и 2Dузлами
        /// </summary>
        public MeshIndex[] link = null;
        #region Поля
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public virtual int Nx { get;  set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public virtual int Ny { get;  set; }
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public double dx { get;  set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public double dy { get;  set; }
        public virtual double Lx { get;  set; }
        public virtual double Ly { get;  set; }
        public double Xmin { get;  set; }
        public double Ymin { get;  set; }
        /// <summary>
        /// Узлы сетки
        /// </summary>
        public int[][] map = null;
        /// <summary>
        /// координаты узлов сетки
        /// </summary>
        public double[][] X = null, Y = null;
        /// <summary>
        /// минимальное растояние от границы Дирехле до узла сетки
        /// </summary>
        public double[][] d_min = null;
        /// <summary>
        /// минимальное растояние от границы Дирехле до узла сетки
        /// </summary>
        public double[][] d_min_dx = null;
        /// <summary>
        /// минимальное растояние от границы Дирехле до узла сетки
        /// </summary>
        public double[][] d_min_dy = null;
        /// <summary>
        /// Максимальное растояние от стенки
        /// </summary>
        public double MaxD;
        /// <summary>
        /// Начальные узлы сетки по j
        /// </summary>
        public int[] Y_init = null;
        /// <summary>
        /// гидравлический радиус
        /// </summary>
        public double hydraulicRadius = - 1;
        #endregion
        public ARectangleMesh(double Xmin=0, double Ymin=0)
        {
            this.Xmin = Xmin;
            this.Ymin = Ymin;
        }
        public ARectangleMesh(int Nx, int Ny, double Lx, double Ly,  double Xmin=0, double Ymin=0)
        {
            Set(Nx, Ny, Lx, Ly, Xmin, Ymin);
        }
        public void Set(int Nx, int Ny, double Lx, double Ly, double Xmin = 0, double Ymin = 0)
        {
            this.Xmin = Xmin;
            this.Ymin = Ymin;
            this.Lx = Lx;
            this.Ly = Ly;
            this.Nx = Nx;
            this.Ny = Ny;
        }

        public abstract void CreateMesh(int[] Mark, int[] Y_init = null, int nx = 25);

        public virtual void Alloc(int CountElems, int CountNodes,  int counter)
        {
            MEM.Alloc(CountElems, ref AreaElems);
            MEM.Alloc(CountNodes, ref CoordsX);
            MEM.Alloc(CountNodes, ref CoordsY);
            MEM.Alloc(counter, ref BoundElems);
            MEM.Alloc(counter, ref BoundKnots);
            MEM.Alloc(counter, ref BoundKnotsMark);
            MEM.Alloc(counter, ref BoundElementsMark);

            MEM.Alloc(Nx, Ny, ref d_min, "d_min");
            MEM.Alloc(Nx, Ny, ref d_min_dx, "d_min_dx");
            MEM.Alloc(Nx, Ny, ref d_min_dy, "d_min_dy");
            MEM.Alloc2D(Nx, Ny, ref X);
            MEM.Alloc2D(Nx, Ny, ref Y);
        }


        public virtual void CalkDmin(int waterLevelMark)
        {
            for (int i = 1; i < Nx - 1; i++)
            {
                for (int j = Y_init[i] + 1; j < Ny - 1; j++)
                {
                    double x = X[i][j];
                    double y = Y[i][j];
                    double r_min = Ly + Lx;
                    for (int b = 0; b < CountBoundElements; b++)
                    {
                        if (BoundElementsMark[b] != waterLevelMark)
                        {
                            uint b1 = BoundElems[b].Vertex1;
                            uint b2 = BoundElems[b].Vertex2;
                            double xb1 = CoordsX[b1];
                            double xb2 = CoordsX[b2];
                            double yb1 = CoordsY[b1];
                            double yb2 = CoordsY[b2];
                            double rx1 = xb1 - x;
                            double rx2 = xb1 - x;
                            double rx3 = 0.5 * (xb1 + xb2) - x;
                            double ry1 = yb1 - y;
                            double ry2 = yb1 - y;
                            double ry3 = 0.5 * (yb1 + yb2) - y;
                            double r1 = rx1 * rx1 + ry1 * ry1;
                            double r2 = rx2 * rx2 + ry2 * ry2;
                            double r3 = rx3 * rx3 + ry3 * ry3;
                            r_min = Math.Min(r_min, r1);
                            r_min = Math.Min(r_min, r2);
                            r_min = Math.Min(r_min, r3);
                        }
                    }
                    d_min[i][j] = Math.Sqrt(r_min);
                }
            }
            for (int i = 0; i < Nx; i++)
            {
                d_min[i][Ny - 1] = d_min[i][Ny - 2];
            }


            //d_min_dx
            //    d_min_dy
        }

        public virtual void CalkVDmin()
        {
            MaxD = 0;
            double maxU = 0;
            double maxV = 0;
            for (int i = 1; i < Nx - 1; i++)
            {
                for (int j = Y_init[i] + 1; j < Ny - 1; j++)
                {
                    d_min_dx[i][j] = -(d_min[i + 1][j] - d_min[i - 1][j]) / dx;
                    d_min_dy[i][j] = -(d_min[i][j + 1] - d_min[i][j - 1]) / dy;
                    if (MaxD < d_min[i][j])
                        MaxD = d_min[i][j];
                    if (maxU < Math.Abs(d_min_dx[i][j]))
                        maxU = Math.Abs(d_min_dx[i][j]);
                    if (maxV < Math.Abs(d_min_dy[i][j]))
                        maxV = Math.Abs(d_min_dy[i][j]);
                }
            }
            for (int i = 1; i < Nx - 1; i++)
            {
                for (int j = Y_init[i] + 1; j < Ny - 1; j++)
                {
                    d_min_dx[i][j] /= maxU;
                    d_min_dy[i][j] /= maxV;
                }
            }
        }

        /// <summary>
        /// Получить номер узла по индексам сетки
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public uint Map(int i, int j)
        {
            return (uint)map[i][j];
        }
        /// <summary>
        /// Получит узел нормальный к граничному
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public MeshIndex GetNormalKnotsindex(int k)
        {
            int i = link[k].i;
            int j = link[k].j;
            // определение границы
            // левая стенка
            if (i == 0) 
                return new MeshIndex(k, map[i + 1][j], DirectMeshIndex.W);
            // правая стенка
            if (i == Nx - 1) 
                return new MeshIndex(k, map[Nx - 2][j], DirectMeshIndex.E);
            // верхняя крышка
            if (j == Ny - 1)
                return new MeshIndex(k, map[i][Ny - 2], DirectMeshIndex.N);
            return new MeshIndex(); 
        }
        /// <summary>
        /// Получить значения поля для IRenderMesh
        /// </summary>
        /// <param name="source">источник</param>
        /// <param name="result">результат</param>
        public void GetValueTo1D(double[][] source, ref double[] result)
        {
            if(link == null)
                GetLinks();
            MEM.Alloc(CountKnots, ref result);
            for (int k = 0; k < link.Length; k++)
            {
                int i = link[k].i;
                int j = link[k].j;
                result[k] = source[i][j];
            }
        }
        /// <summary>
        /// Массив связи 1D и 2D
        /// </summary>
        /// <param name="source">источник</param>
        /// <param name="result">результат</param>
        public void GetLinks()
        {
            MEM.Alloc(CountKnots, ref link);
            int k = 0;
            if (Y_init == null)
            {
                for (int i = 0; i < Nx; i++)
                    for (int j = 0; j < Ny; j++)
                        link[k++] = new MeshIndex(i, j);
                    
            }
            else
            {
                for (int i = 0; i < Nx; i++)
                    for (int j = Y_init[i]; j < Ny; j++)
                        link[k++] = new MeshIndex(i, j);
            }
        }
        /// <summary>
        /// Получить значения поля для IRectangleMesh
        /// </summary>
        /// <param name="source">источникparam>
        /// <param name="result">результат</param>
        public void Get1DValueTo2D(double[] source, ref double[][] result)
        {
            if (link == null)
                GetLinks();
            MEM.Alloc2D(Nx, Ny, ref result);
            for (int k = 0; k < link.Length; k++)
            {
                int i = link[k].i;
                int j = link[k].j;
                result[i][j] = source[k];
            }
        }
        /// <summary>
        /// Вычисление поля source в точке point
        /// </summary>
        /// <param name="point">точка</param>
        /// <returns>значение поля</returns>
        public double CalkCeilValue(ref Vector2 point, double[][] source)
        {
            double cx = point.X - Xmin;
            double cy = point.Y - Ymin;

            int xi = (int)(cx / dx);
            int yi = (int)(cy / dy);

            xi = xi > Nx - 2 ? Nx - 2 : xi;
            yi = yi > Ny - 2 ? Ny - 2 : yi;

            if(Y_init == null)
            {
                // сделать для верхнего правого треугольника 
                double a = cx - xi * dx;
                double b = cy - yi * dy;

                double u0 = source[xi][yi];
                double u1 = source[xi + 1][yi];
                double u2 = source[xi + 1][yi + 1];
                double u3 = source[xi][yi + 1];

                double N0 = (1 - a / dx) * (1 - b / dy);
                double N1 = a / dx * (1 - b / dy);
                double N2 = a / dx * (b / dy);
                double N3 = (1 - a / dx) * (b / dy);

                return N0 * u0 + N1 * u1 + N2 * u2 + N3 * u3;
            }
            if (Y_init[xi] >= yi)
            {
                double a = cx - xi * dx;
                double b = cy - yi * dy;

                double u0 = source[xi][yi];
                double u1 = source[xi + 1][yi];
                double u2 = source[xi + 1][yi + 1];
                double u3 = source[xi][yi + 1];

                double N0 = (1 - a / dx) * (1 - b / dy);
                double N1 = a / dx * (1 - b / dy);
                double N2 = a / dx * (b / dy);
                double N3 = (1 - a / dx) * (b / dy);

                return N0 * u0 + N1 * u1 + N2 * u2 + N3 * u3;
            }
            else
            {
                // сделать для верхнего правого треугольника 
                double a = cx - xi * dx;
                double b = cy - yi * dy;

                double u0 = source[xi][yi];
                double u1 = source[xi + 1][yi];
                double u2 = source[xi + 1][yi + 1];
                double u3 = source[xi][yi + 1];

                double N0 = (1 - a / dx) * (1 - b / dy);
                double N1 = a / dx * (1 - b / dy);
                double N2 = a / dx * (b / dy);
                double N3 = (1 - a / dx) * (b / dy);

                return N0 * u0 + N1 * u1 + N2 * u2 + N3 * u3;
            }
        }

        

        /// <summary>
        /// вычисление гидравлического радиуса
        /// </summary>
        /// <param name="waterLevelMark">маркер свободной поверхности</param>
        /// <returns>гидравлический радиус</returns>
        public double HydraulicRadius(int waterLevelMark = -1)
        {
            double Area = GetSquareArea();
            double WetLength = GetLengthWetBoundary(waterLevelMark);
            return Area / WetLength;
        }
    }
}
