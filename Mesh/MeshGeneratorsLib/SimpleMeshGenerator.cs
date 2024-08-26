namespace MeshGeneratorsLib
{
    using MemLogLib;
    using CommonLib;
    using MeshLib;
    using System;
    public class SimpleMeshGenerator
    {

        public static void GetTetrangleMeshTestPole(ref double[] Field, uint N, double L, double Amp)
        {
            uint CountNodes = N * N;

            Field = new double[CountNodes];
            double h = L / (N - 1);
            uint k = 0;
            for (uint i = 0; i < N; i++)
            {
                double y = i * h;
                for (int j = 0; j < N; j++)
                {
                    double x = h * j;
                    double qx = x / L;
                    double qy = y / L;
                    Field[k] = Amp * 65536 * Math.Pow(qx * (qx - 1) * qy * (qy - 1), 4);
                    k++;
                }
            }
        }
        /// <summary>
        /// Генератор TriMesh сетки в квадратной области со стороной L
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="N">количество узлов на стороне квадрата</param>
        /// <param name="L">длина стороны квадарата</param>
        /// <param name="Flag">флаги рнаницы</param>
        public static void GetTetrangleMesh(ref TriMesh mesh, int N, double L, int[] Flag)
        {
            int[] LFlag = { 0, 1, 1, 0 };
            if (Flag == null)
                Flag = LFlag;
            mesh = new TriMesh();
            int counter = 4 * (N - 1);
            int CountNodes = N * N;
            int CountElems = 2 * (N - 1) * (N - 1);

            mesh.AreaElems = new TriElement[CountElems];

            mesh.BoundElems = new TwoElement[counter];
            mesh.BoundElementsMark = new int[counter];

            mesh.BoundKnots = new int[counter];
            mesh.BoundKnotsMark = new int[counter];

            mesh.CoordsX = new double[CountNodes];
            mesh.CoordsY = new double[CountNodes];

            double cc = L / 2;
            uint[,] map = new uint[N, N];
            double h = L / (N - 1);
            uint k = 0;
            for (uint i = 0; i < N; i++)
            {
                double ym = i * h;
                for (int j = 0; j < N; j++)
                {
                    double xm = h * j;
                    mesh.CoordsX[k] = xm;
                    mesh.CoordsY[k] = ym;
                    map[i, j] = k++;
                }
            }
            int elem = 0;
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    mesh.AreaElems[elem] = new TriElement(map[i, j], map[i + 1, j], map[i + 1, j + 1]);
                    elem++;
                    mesh.AreaElems[elem] = new TriElement(map[i + 1, j + 1], map[i, j + 1], map[i, j]);
                    elem++;
                }
            }
            k = 0;
            //bool fl = true;
            bool fl = false;
            if (fl == true)
            {
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[N - 1, i], map[N - 1, i + 1]);
                    mesh.BoundElementsMark[k] = Flag[0];
                    // задана функция
                    mesh.BoundKnotsMark[k] = 0;
                    mesh.BoundKnots[k++] = (int)map[N - 1, i];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[N - 1 - i, N - 1], map[N - 2 - i, N - 1]);
                    mesh.BoundElementsMark[k] = Flag[1];
                    // задана производная
                    mesh.BoundKnotsMark[k] = 1;
                    mesh.BoundKnots[k++] = (int)map[N - 1 - i, N - 1];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[0, N - i - 1], map[0, N - i - 2]);
                    mesh.BoundElementsMark[k] = Flag[2];
                    // задана производная
                    mesh.BoundKnotsMark[k] = 1;
                    mesh.BoundKnots[k++] = (int)map[0, N - i - 1];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[i, 0], map[i + 1, 0]);
                    mesh.BoundElementsMark[k] = Flag[3];
                    // задана функция
                    mesh.BoundKnotsMark[k] = 0;
                    mesh.BoundKnots[k++] = (int)map[i, 0];
                }
            }
            else
            {
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[0, i], map[0, i + 1]);
                    mesh.BoundElementsMark[k] = Flag[0];
                    // задана производная
                    mesh.BoundKnotsMark[k] = Flag[0];
                    mesh.BoundKnots[k++] = (int)map[0, i];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[i, N - 1], map[i + 1, N - 1]);
                    mesh.BoundElementsMark[k] = Flag[1];
                    // задана функция
                    mesh.BoundKnotsMark[k] = Flag[1];
                    mesh.BoundKnots[k++] = (int)map[i, N - 1];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[N - 1, N - 1 - i], map[N - 1, N - 2 - i]);
                    mesh.BoundElementsMark[k] = Flag[2];
                    mesh.BoundKnotsMark[k] = Flag[2];
                    mesh.BoundKnots[k++] = (int)map[N - 1, N - 1 - i];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    mesh.BoundElems[k] = new TwoElement(map[N - 1 - i, 0], map[N - 2 - i, 0]);
                    mesh.BoundElementsMark[k] = Flag[3];
                    // задана функция
                    mesh.BoundKnotsMark[k] = Flag[3];
                    mesh.BoundKnots[k++] = (int)map[N - 1 - i, 0];
                }
            }
        }

        // ======================================================
        //       Система координат        Обход узлов
        //     dy                                     i
        //   |---|----------> Y j      -------------> 0 j
        //   | dx                      -------------> 1 j 
        //   |---                      -------------> 2 j
        //   |
        //   |
        //   |
        //   V X  i
        // ======================================================
        /// <summary>
        /// Генерация триангуляционной КЭ сетки в прямоугольной области
        /// </summary>
        /// <param name="mesh">результат</param>
        /// <param name="Nx">узлов по Х</param>
        /// <param name="Ny">узлов по У</param>
        /// <param name="dx">шаг по Х</param>
        /// <param name="dy">шаг по У</param>
        /// <param name="Flag">признаки границ для ГУ</param>
        public static void GetRectangleMesh(ref TriMesh mesh, int Nx, int Ny, double dx, double dy, int[] Flag)
        {
            int[] LFlag = { 0, 1, 1, 0 };
            if (Flag == null)
                Flag = LFlag;

            mesh = new TriMesh();
            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);

            mesh.AreaElems = new TriElement[CountElems];

            mesh.BoundElems = new TwoElement[counter];
            mesh.BoundElementsMark = new int[counter];

            mesh.BoundKnots = new int[counter];
            mesh.BoundKnotsMark = new int[counter];

            mesh.CoordsX = new double[CountNodes];
            mesh.CoordsY = new double[CountNodes];

            uint[,] map = new uint[Nx, Ny];

            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double ym = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double xm = dy * j;
                    mesh.CoordsX[k] = xm;
                    mesh.CoordsY[k] = ym;
                    map[i, j] = k++;
                }
            }
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    mesh.AreaElems[elem] = new TriElement(map[i, j], map[i + 1, j], map[i + 1, j + 1]);
                    elem++;
                    mesh.AreaElems[elem] = new TriElement(map[i + 1, j + 1], map[i, j + 1], map[i, j]);
                    elem++;
                }
            }
            k = 0;

            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[Nx - 1, i], map[Nx - 1, i + 1]);
                mesh.BoundElementsMark[k] = Flag[0];
                // задана функция
                mesh.BoundKnotsMark[k] = Flag[0];
                mesh.BoundKnots[k++] = (int)map[Nx - 1, i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[Nx - 1 - i, Ny - 1], map[Nx - 2 - i, Ny - 1]);
                mesh.BoundElementsMark[k] = Flag[1];
                // задана производная
                mesh.BoundKnotsMark[k] = Flag[1];
                mesh.BoundKnots[k++] = (int)map[Nx - 1 - i, Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[0, Ny - i - 1], map[0, Ny - i - 2]);
                mesh.BoundElementsMark[k] = Flag[2];
                // задана производная
                mesh.BoundKnotsMark[k] = Flag[2];
                mesh.BoundKnots[k++] = (int)map[0, Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k] = new TwoElement(map[i, 0], map[i + 1, 0]);
                mesh.BoundElementsMark[k] = Flag[3];
                // задана функция
                mesh.BoundKnotsMark[k] = Flag[3];
                mesh.BoundKnots[k++] = (int)map[i, 0];
            }

        }
        /// <summary>
        /// Генерация КЭ сетки 1 поряжка в прямоугольнике
        /// </summary>
        /// <param name="tm">тип сетки</param>
        /// <param name="Nx">количество узлов по Х</param>
        /// <param name="Ny">количество узлов по Y</param>
        /// <param name="Lx">длина области по Х</param>
        /// <param name="Ly">длина области по У</param>
        /// <param name="MarkBC">метка границы</param>
        /// <param name="BC">тип ГУ на границе</param>
        /// <returns></returns>
        public static MeshCore GetTetrangleMesh(TypeMesh tm, int Nx, int Ny, double Lx, double Ly, int[] MarkBC, TypeBoundCond[] BC)
        {
            MeshCore mesh = new MeshCore();
            int CountNodes = Nx * Ny;
            int CountBoundElems = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountElems = (Nx - 1) * (Ny - 1);
            int cu = 4;
            int bcu = 2;
            int ffID = (int)TypeFunForm.Form_2D_Rectangle_L1;
            if (tm == TypeMesh.Triangle)
            {
                ffID = (int)TypeFunForm.Form_2D_Triangle_L1;
                CountElems *= 2;
                cu = 3;
            }
            MEM.Alloc2D(cu, CountNodes, ref mesh.points);
            MEM.Alloc2D(cu, CountElems, ref mesh.elems);
            MEM.Alloc(CountElems, ref mesh.fform);
            MEM.Alloc2D(bcu, CountBoundElems, ref mesh.boundary);
            MEM.Alloc2D(bcu, CountBoundElems, ref mesh.boundaryMark);

            int[,] map = null;
            MEM.Alloc2D(Nx, Nx, ref map);

            double dx = Lx / (Nx - 1);
            double dy = Ly / (Ny - 1);
            int k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = dy * j;
                    mesh.points[0][k] = xm;
                    mesh.points[1][k] = ym;
                    map[i, j] = k++;
                }
            }
            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    if (tm == TypeMesh.Triangle)
                    {
                        mesh.elems[0][elem] = map[i, j];
                        mesh.elems[1][elem] = map[i + 1, j];
                        mesh.elems[2][elem] = map[i + 1, j + 1];
                        elem++;
                        mesh.elems[0][elem] = map[i + 1, j + 1];
                        mesh.elems[1][elem] = map[i, j + 1];
                        mesh.elems[2][elem] = map[i, j];
                        elem++;
                    }
                    if (tm == TypeMesh.Rectangle)
                    {
                        mesh.elems[0][elem] = map[i, j];
                        mesh.elems[1][elem] = map[i + 1, j];
                        mesh.elems[2][elem] = map[i + 1, j + 1];
                        mesh.elems[3][elem] = map[i, j + 1];
                        elem++;
                    }
                }
            }
            for (uint i = 0; i < CountElems; i++)
                mesh.fform[i] = ffID;
            k = 0;
            // низ
            for (int j = 0; j < Ny - 1; j++)
            {
                mesh.boundary[0][k] = map[Nx - 1, j];
                mesh.boundary[1][k] = map[Nx - 1, j + 1];
                mesh.boundaryMark[0][k] = MarkBC[1];
                mesh.boundaryMark[1][k] = (int)BC[1];
                k++;
            }
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.boundary[0][k] = map[Nx - 1 - i, Ny - 1];
                mesh.boundary[1][k] = map[Nx - 2 - i, Ny - 1];
                mesh.boundaryMark[0][k] = MarkBC[2];
                mesh.boundaryMark[1][k] = (int)BC[2];
                k++;
            }
            for (int j = 0; j < Ny - 1; j++)
            {
                mesh.boundary[0][k] = map[0, Ny - j - 1];
                mesh.boundary[1][k] = map[0, Ny - j - 2];
                mesh.boundaryMark[0][k] = MarkBC[3];
                mesh.boundaryMark[1][k] = (int)BC[3];
                k++;
            }
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.boundary[0][k] = map[i, 0];
                mesh.boundary[1][k] = map[i + 1, 0];
                mesh.boundaryMark[0][k] = MarkBC[0];
                mesh.boundaryMark[1][k] = (int)BC[0];
                k++;
            }
           
            // mesh.Print();

            return mesh;
        }
    }
}
