//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2022 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 правка  :   04.07.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshAdapterLib
{
    using System.Collections.Generic;
    using MeshGeneratorsLib;
    using GeometryLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Geometry;
    using MeshLib;
    using MemLogLib;
    using System;
    using MeshGeneratorsLib.Renumberation;

    /// <summary>
    /// Тестовые генераторы КЭ сеток
    /// </summary>
    public static class CreateMesh
    {
        [Obsolete("Метод возможно будет не совместим в дальнейшем", false)]
        public static IFEMesh GetMesh(double diametrFE, double L, double H, 
            TypeMesh typeMesh = TypeMesh.Triangle,
            TypeRangeMesh typeRangeMesh = TypeRangeMesh.mRange1,
            int reNumberation = 1, Direction direction =  Direction.toRight)
        {
            // meshData
            //TypeMesh[] meshTypes = { TypeMesh.MixMesh, TypeMesh.Triangle, TypeMesh.Rectangle };
            TypeMesh meshType = typeMesh;
            TypeRangeMesh MeshRange = typeRangeMesh;
            //int meshMethod = 0;
            int meshMethod = 1;
            double RelaxMeshOrthogonality = 0.1;
            int CountParams = 0;
            bool flagMidle = true;
            // Данные для сетки
            HMeshParams meshData = new HMeshParams(meshType, MeshRange, meshMethod,
                      new HPoint(diametrFE, diametrFE), RelaxMeshOrthogonality, reNumberation,
                      direction, CountParams, flagMidle);

            HMapSegment[] mapSegment = new HMapSegment[4];
            // количество параметров на границе (задан 1)
            double[] param = { 5 };
            // определение вершин
            VMapKnot p0 = new VMapKnot(0, 0, param);
            VMapKnot p1 = new VMapKnot(L, 0, param);
            VMapKnot p2 = new VMapKnot(L, H, param);
            VMapKnot p3 = new VMapKnot(0, H, param);
            // определение ребер
            List<VMapKnot> Knots0 = new List<VMapKnot>() { new VMapKnot(p0), new VMapKnot(p1) };
            List<VMapKnot> Knots1 = new List<VMapKnot>() { new VMapKnot(p1), new VMapKnot(p2) };
            List<VMapKnot> Knots2 = new List<VMapKnot>() { new VMapKnot(p2), new VMapKnot(p3) };
            List<VMapKnot> Knots3 = new List<VMapKnot>() { new VMapKnot(p3), new VMapKnot(p0) };
            // определение сегментов
            mapSegment[0] = new HMapSegment(Knots0, 0, 1);
            mapSegment[1] = new HMapSegment(Knots1, 1, 0);
            mapSegment[2] = new HMapSegment(Knots2, 2, 1);
            mapSegment[3] = new HMapSegment(Knots3, 3, 0);
            // определение область для генерации КЭ сетки
            HMapSubArea subArea = new HMapSubArea();
            HMapFacet[] facet = new HMapFacet[4];
            for (int i = 0; i < 4; i++)
            {
                facet[i] = new HMapFacet();
                facet[i].Add(mapSegment[i]);
                subArea.Add(facet[i]);
            }
            IHTaskMap mapMesh = new HTaskMap(subArea);
            IFERenumberator Renumberator = new FERenumberator();
            //mapMesh.Add(subArea);
            DirectorMeshGenerator mg = new DirectorMeshGenerator(null, meshData, mapMesh, Renumberator);
            // генерация КЭ сетки
            IFEMesh feMesh = mg.Create();
            return feMesh;
        }

        [Obsolete("Метод возможно будет не совместим в дальнейшем", false)]
        public static IFEMesh GetMesh(int Nx,int Ny, double L, double H,
           TypeMesh typeMesh = TypeMesh.Triangle,
           TypeRangeMesh typeRangeMesh = TypeRangeMesh.mRange1,
           int reNumberation = 1, Direction direction = Direction.toRight)
        {
            double dx = L / (Nx - 1);
            double dy = H / (Ny - 1);

            TypeMesh meshType = typeMesh;
            TypeRangeMesh MeshRange = typeRangeMesh;
            //int meshMethod = 0;
            int meshMethod = 1;
            double RelaxMeshOrthogonality = 0.1;
            int CountParams = 0;
            bool flagMidle = true;
            // Данные для сетки
            HMeshParams meshData = new HMeshParams(meshType, MeshRange, meshMethod,
                      new HPoint(dx,dy), RelaxMeshOrthogonality, reNumberation,
                      direction, CountParams, flagMidle);

            HMapSegment[] mapSegment = new HMapSegment[4];
            // количество параметров на границе (задан 1)
            double[] param = { 5 };
            // определение вершин
            VMapKnot p0 = new VMapKnot(0, 0, param);
            VMapKnot p1 = new VMapKnot(L, 0, param);
            VMapKnot p2 = new VMapKnot(L, H, param);
            VMapKnot p3 = new VMapKnot(0, H, param);
            // определение ребер
            List<VMapKnot> Knots0 = new List<VMapKnot>() { new VMapKnot(p0), new VMapKnot(p1) };
            List<VMapKnot> Knots1 = new List<VMapKnot>() { new VMapKnot(p1), new VMapKnot(p2) };
            List<VMapKnot> Knots2 = new List<VMapKnot>() { new VMapKnot(p2), new VMapKnot(p3) };
            List<VMapKnot> Knots3 = new List<VMapKnot>() { new VMapKnot(p3), new VMapKnot(p0) };
            // определение сегментов
            mapSegment[0] = new HMapSegment(Knots0, 0, 1);
            mapSegment[1] = new HMapSegment(Knots1, 1, 0);
            mapSegment[2] = new HMapSegment(Knots2, 2, 1);
            mapSegment[3] = new HMapSegment(Knots3, 3, 0);
            // определение область для генерации КЭ сетки
            HMapSubArea subArea = new HMapSubArea();
            HMapFacet[] facet = new HMapFacet[4];
            for (int i = 0; i < 4; i++)
            {
                facet[i] = new HMapFacet();
                facet[i].Add(mapSegment[i]);
                subArea.Add(facet[i]);
            }
            IHTaskMap mapMesh = new HTaskMap(subArea);
            IFERenumberator Renumberator = new FERenumberator();
            //mapMesh.Add(subArea);
            DirectorMeshGenerator mg = new DirectorMeshGenerator(null, meshData, mapMesh, Renumberator);
            // генерация КЭ сетки
            IFEMesh feMesh = mg.Create();
            return feMesh;
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
        public static void GetRectangleMesh_XY(ref ComplecsMesh mesh, int Nx, int Ny, double dx, double dy, ref uint[,] map)
        {
            mesh = new ComplecsMesh();
            
            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = (Nx - 1) * (Ny - 1);

            MEM.Alloc(CountElems, 4,ref mesh.AreaElems, "mesh.AreaElems");
            MEM.Alloc(CountElems,ref mesh.AreaElemsFFType, "mesh.AreaElemsFFType");
            
            MEM.Alloc(counter, 2, ref mesh.BoundElems, "mesh.BoundElems");
            MEM.Alloc(counter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");

            MEM.Alloc(counter, ref mesh.BoundKnots, "mesh.BoundKnots");
            MEM.Alloc(counter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");

            MEM.Alloc(CountNodes, ref mesh.CoordsX, "mesh.CoordsX");
            MEM.Alloc(CountNodes, ref mesh.CoordsY, "mesh.CoordsY");

            map = new uint[Nx, Ny];
            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = dy * j;
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
                    mesh.AreaElems[elem][0] = map[i, j];
                    mesh.AreaElems[elem][1] = map[i + 1, j];
                    mesh.AreaElems[elem][2] = map[i + 1, j + 1];
                    mesh.AreaElems[elem][3] = map[i, j + 1];
                    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Rectangle_L1;
                    elem++;
                }
            }
            k = 0;
            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k][0] = map[Nx - 1, i];
                mesh.BoundElems[k][1] = map[Nx - 1, i + 1];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[Nx - 1, i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = map[Nx - 1 - i, Ny - 1];
                mesh.BoundElems[k][1] = map[Nx - 2 - i, Ny - 1];
                mesh.BoundElementsMark[k] = 1;
                // задана производная
                mesh.BoundKnotsMark[k] = 1;
                mesh.BoundKnots[k++] = (int)map[Nx - 1 - i, Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k][0] = map[0, Ny - i - 1];
                mesh.BoundElems[k][1] = map[0, Ny - i - 2];
                mesh.BoundElementsMark[k] = 2;
                // задана производная
                mesh.BoundKnotsMark[k] = 2;
                mesh.BoundKnots[k++] = (int)map[0, Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = map[i, 0];
                mesh.BoundElems[k][1] = map[i + 1, 0];
                mesh.BoundElementsMark[k] = 3;
                // задана функция
                mesh.BoundKnotsMark[k] = 3;
                mesh.BoundKnots[k++] = (int)map[i, 0];
            }
        }

        public static void GetRectangleTriMesh_XY(ref ComplecsMesh mesh, int Nx, int Ny, double dx, double dy)
        {
            mesh = new ComplecsMesh();

            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);

            MEM.Alloc(CountElems, 3, ref mesh.AreaElems, "mesh.AreaElems");
            MEM.Alloc(CountElems, ref mesh.AreaElemsFFType, "mesh.AreaElemsFFType");

            MEM.Alloc(counter, 2, ref mesh.BoundElems, "mesh.BoundElems");
            MEM.Alloc(counter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");

            MEM.Alloc(counter, ref mesh.BoundKnots, "mesh.BoundKnots");
            MEM.Alloc(counter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");

            MEM.Alloc(CountNodes, ref mesh.CoordsX, "mesh.CoordsX");
            MEM.Alloc(CountNodes, ref mesh.CoordsY, "mesh.CoordsY");

            uint[,] map = new uint[Nx, Ny];

            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = dy * j;
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
                    mesh.AreaElems[elem][0] = map[i, j];
                    mesh.AreaElems[elem][1] = map[i + 1, j];
                    mesh.AreaElems[elem][2] = map[i + 1, j + 1];
                    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                    elem++;
                    mesh.AreaElems[elem][0] = map[i + 1, j + 1];
                    mesh.AreaElems[elem][1] = map[i, j + 1];
                    mesh.AreaElems[elem][2] = map[i, j];
                    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                    elem++;
                }
            }
            k = 0;
            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k][0] = map[Nx - 1, i];
                mesh.BoundElems[k][1] = map[Nx - 1, i + 1];
                mesh.BoundElementsMark[k] = 0;
                // задана функция
                mesh.BoundKnotsMark[k] = 0;
                mesh.BoundKnots[k++] = (int)map[Nx - 1, i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = map[Nx - 1 - i, Ny - 1];
                mesh.BoundElems[k][1] = map[Nx - 2 - i, Ny - 1];
                mesh.BoundElementsMark[k] = 1;
                // задана производная
                mesh.BoundKnotsMark[k] = 1;
                mesh.BoundKnots[k++] = (int)map[Nx - 1 - i, Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k][0] = map[0, Ny - i - 1];
                mesh.BoundElems[k][1] = map[0, Ny - i - 2];
                mesh.BoundElementsMark[k] = 2;
                // задана производная
                mesh.BoundKnotsMark[k] = 2;
                mesh.BoundKnots[k++] = (int)map[0, Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = map[i, 0];
                mesh.BoundElems[k][1] = map[i + 1, 0];
                mesh.BoundElementsMark[k] = 3;
                // задана функция
                mesh.BoundKnotsMark[k] = 3;
                mesh.BoundKnots[k++] = (int)map[i, 0];
            }
        }

        public static void GetRectangleTriMesh_XY(ref IMesh mesh, int Nx, int Ny, double L, double H)
        {
            ComplecsMesh cmesh = null;
            double dx = L / (Nx - 1);
            double dy = H / (Ny - 1);
            GetRectangleTriMesh_XY(ref cmesh, Nx, Ny, dx, dy);
            mesh = cmesh;
        }

        public static void GetRectangleTriMesh(ref TriMesh mesh, int Nx, int Ny, double L, double H, int flag = 0)
        {
            double dx = L / (Nx - 1);
            double dy = H / (Ny - 1);
            GetRectangleTriMesh(ref mesh, dx, dy, Nx, Ny, flag);
        }

        public static void GetRectangleTriMesh(ref TriMesh mesh, double dx, double dy, int Nx, int Ny, int flag = 0)
        {
            mesh = new TriMesh();

            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);

            MEM.Alloc(CountElems, ref mesh.AreaElems, "mesh.AreaElems");

            MEM.Alloc(counter, ref mesh.BoundElems, "mesh.BoundElems");
            MEM.Alloc(counter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");

            MEM.Alloc(counter, ref mesh.BoundKnots, "mesh.BoundKnots");
            MEM.Alloc(counter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");

            MEM.Alloc(CountNodes, ref mesh.CoordsX, "mesh.CoordsX");
            MEM.Alloc(CountNodes, ref mesh.CoordsY, "mesh.CoordsY");

            uint[,] map = new uint[Nx, Ny];


            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double ym = dy * j;
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
                    mesh.AreaElems[elem][0] = map[i, j];
                    mesh.AreaElems[elem][1] = map[i + 1, j];
                    mesh.AreaElems[elem][2] = map[i + 1, j + 1];
                    elem++;
                    mesh.AreaElems[elem][0] = map[i + 1, j + 1];
                    mesh.AreaElems[elem][1] = map[i, j + 1];
                    mesh.AreaElems[elem][2] = map[i, j];
                    elem++;
                }
            }

            int[] mask0 = { 0, 1, 2, 3 };
            int[] mask1 = { 1, 2, 3, 0 };
            int[] mask = null;
            if (flag == 0)
                mask = mask0;
            else
                mask = mask1;

            k = 0;
            // низ
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k][0] = map[Nx - 1, i];
                mesh.BoundElems[k][1] = map[Nx - 1, i + 1];
                mesh.BoundElementsMark[k] = mask[0];
                // задана функция
                mesh.BoundKnotsMark[k] = mask[0];
                mesh.BoundKnots[k++] = (int)map[Nx - 1, i];
            }
            // правая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = map[Nx - 1 - i, Ny - 1];
                mesh.BoundElems[k][1] = map[Nx - 2 - i, Ny - 1];
                mesh.BoundElementsMark[k] = mask[1];
                // задана производная
                mesh.BoundKnotsMark[k] = mask[1];
                mesh.BoundKnots[k++] = (int)map[Nx - 1 - i, Ny - 1];
            }
            // верх
            for (int i = 0; i < Ny - 1; i++)
            {
                mesh.BoundElems[k][0] = map[0, Ny - i - 1];
                mesh.BoundElems[k][1] = map[0, Ny - i - 2];
                mesh.BoundElementsMark[k] = mask[2];
                // задана производная
                mesh.BoundKnotsMark[k] = mask[2];
                mesh.BoundKnots[k++] = (int)map[0, Ny - i - 1];
            }
            // левая сторона
            for (int i = 0; i < Nx - 1; i++)
            {
                mesh.BoundElems[k][0] = map[i, 0];
                mesh.BoundElems[k][1] = map[i + 1, 0];
                mesh.BoundElementsMark[k] = mask[3];
                // задана функция
                mesh.BoundKnotsMark[k] = mask[3];
                mesh.BoundKnots[k++] = (int)map[i, 0];
            }
        }
    }
}
