//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                      09.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    /// <summary>
    /// OO: Простые генераторы КЭ сеток
    /// </summary>
    public class TriMeshCreate
    {
        /// <summary>
        /// Получить образ симплекс сетки по двум 2D массивам координат
        /// </summary>
        public static TriMesh GetMesh(double[][] x, double[][] y)
        {
            TriMesh mesh = new TriMesh();
            int NY = x.Length;
            int NX = x[0].Length;
            uint[][] map = new uint[NY][];
            uint co = 0;
            for (int i = 0; i < NY; i++)
            {
                map[i] = new uint[NX];
                for (int j = 0; j < NX; j++)
                    map[i][j] = co++;
            }
            int CountElems = 2 * (NX - 1) * (NY - 1);
            int CountBElems = 2 * (NX - 1 + NY - 1);
            int CountKnots = NX * NY;
            mesh.AreaElems = new TriElement[CountElems];
            mesh.BoundElems = new TwoElement[CountBElems];
            mesh.BoundElementsMark = new int[CountBElems];
            mesh.BoundKnots = new int[CountKnots];
            mesh.BoundKnotsMark = new int[CountKnots];
            mesh.CoordsX = new double[CountKnots];
            mesh.CoordsY = new double[CountKnots];
            // Элементы
            co = 0;
            for (int i = 0; i < NY - 1; i++)
                for (int j = 0; j < NX - 1; j++)
                {
                    mesh.AreaElems[co++] = new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]);
                    mesh.AreaElems[co++] = new TriElement(map[i][j], map[i + 1][j + 1], map[i][j + 1]);
                }
            // Элементы на границе
            co = 0;
            uint kn = 0;
            for (int i = 0; i < NY - 1; i++)
            {
                mesh.BoundElementsMark[co] = 0;
                mesh.BoundElems[co++] = new TwoElement(map[i][0], map[i + 1][0]);
                mesh.BoundElementsMark[co] = 2;
                mesh.BoundElems[co++] = new TwoElement(map[i][NX - 1], map[i + 1][NX - 1]);
                mesh.BoundKnots[kn++] = (int)map[i][0];
                mesh.BoundKnots[kn++] = (int)map[i + 1][NX - 1];
            }
            for (int j = 0; j < NX - 1; j++)
            {
                mesh.BoundElementsMark[co] = 1;
                mesh.BoundElems[co++] = new TwoElement(map[0][j], map[0][j + 1]);
                mesh.BoundElementsMark[co] = 3;
                mesh.BoundElems[co++] = new TwoElement(map[NY - 1][j], map[NY - 1][j + 1]);
                mesh.BoundKnots[kn++] = (int)map[0][j + 1];
                mesh.BoundKnots[kn++] = (int)map[NY - 1][j];
            }
            // Координаты узлов
            co = 0;
            for (int i = 0; i < NY; i++)
                for (int j = 0; j < NX; j++)
                {
                    mesh.CoordsX[co] = x[i][j];
                    mesh.CoordsY[co] = y[i][j];
                    co++;
                }
            return mesh;
        }
        /// <summary>
        /// Получить образ симплекс сетки по двум 1D массивам координат
        /// и размерностью сетки
        /// </summary>
        public static TriMesh GetMesh(int NY, int NX, double[] x, double[] y)
        {
            TriMesh mesh = new TriMesh();
            uint[][] map = new uint[NY][];
            uint co = 0;
            for (int i = 0; i < NY; i++)
            {
                map[i] = new uint[NX];
                for (int j = 0; j < NX; j++)
                    map[i][j] = co++;
            }
            int CountElems = 2 * (NX - 1) * (NY - 1);
            int CountBElems = 2 * (NX - 1 + NY - 1);
            int CountKnots = NX * NY;
            mesh.AreaElems = new TriElement[CountElems];
            mesh.BoundElems = new TwoElement[CountBElems];
            mesh.BoundElementsMark = new int[CountBElems];
            mesh.BoundKnots = new int[CountBElems];
            mesh.BoundKnotsMark = new int[CountBElems];
            mesh.CoordsX = new double[CountKnots];
            mesh.CoordsY = new double[CountKnots];
            // Элементы
            co = 0;
            for (int i = 0; i < NY - 1; i++)
                for (int j = 0; j < NX - 1; j++)
                {
                    mesh.AreaElems[co++] = new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]);
                    mesh.AreaElems[co++] = new TriElement(map[i][j], map[i + 1][j + 1], map[i][j + 1]);
                }
            // Элементы на границе
            co = 0;
            uint kn = 0;
            for (int i = 0; i < NY - 1; i++)
            {
                mesh.BoundElementsMark[co] = 0;
                mesh.BoundElems[co++] = new TwoElement(map[i][0], map[i + 1][0]);
                mesh.BoundElementsMark[co] = 2;
                mesh.BoundElems[co++] = new TwoElement(map[i][NX - 1], map[i + 1][NX - 1]);
                mesh.BoundKnots[kn++] = (int)map[i][0];
                mesh.BoundKnots[kn++] = (int)map[i + 1][NX - 1];
            }
            for (int j = 0; j < NX - 1; j++)
            {
                mesh.BoundElementsMark[co] = 1;
                mesh.BoundElems[co++] = new TwoElement(map[0][j], map[0][j + 1]);
                mesh.BoundElementsMark[co] = 3;
                mesh.BoundElems[co++] = new TwoElement(map[NY - 1][j], map[NY - 1][j + 1]);
                mesh.BoundKnots[kn++] = (int)map[0][j + 1];
                mesh.BoundKnots[kn++] = (int)map[NY - 1][j];
            }
            // Координаты узлов
            for (int i = 0; i < NX * NY; i++)
            {
                mesh.CoordsX[i] = x[i];
                mesh.CoordsY[i] = y[i];
            }
            return mesh;
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
                    // задана производная
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

    }
}
