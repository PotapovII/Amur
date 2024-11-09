//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 18.07.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: RectFDMesh - базистная разностная сетка 
    /// в четырехгранной трапециевидной области
    /// 
    ///  -----0--------------------------> y (j)
    ///  |                 /|
    ///  |               /  |
    ///  |             /    |
    ///  Lm          /      |
    ///  |         /        |
    ///  |       /          |
    ///  |     /            |
    ///  ---- |             |  
    ///  |    |             |
    ///  |    |             |
    ///  Lx0  |            Lx
    ///  |    |             |
    ///  |    |             |
    ///  |    |             |
    ///  ---  |             |
    ///  |     \            |
    ///  |       \          |
    ///  |         \        |
    ///  |           \      |
    ///  |             \    |
    ///  |   |           \  |
    ///  |   |             \|
    ///  |---|---- Ly ------|
    ///      |
    ///      |
    ///      V x (i)     
    /// 
    ///  При использовании должен быть подобран баланс узлов из условия
    ///  Lxm/(Nx1-1) == Lx0/(Nx2-1) 
    ///  Тогда
    ///  Nx = 2 * Nx1 + Nx2 - 1;
    ///  Ny = Nx1;
    ///  при m!=1 получаем в общем случае сетку с dx != dy
    /// </summary>
    [Serializable]
    public class ChannelTrapezMesh : ARectangleMesh
    {
        /// <summary>
        /// Длина донной части
        /// </summary>
        public double Lx0 => Lx - 2 * m * Ly;
        /// <summary>
        /// заложение откосов  Lm/Ly
        /// </summary>
        public double m = 1;
        public ChannelTrapezMesh(int Nx,int Ny, double Lx, double Ly, double m, double Xmin = 0, double Ymin = 0)
            : base(Nx, Ny, Lx, Ly, Xmin, Ymin)
        {
            this.m = m; 
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
        public override void CreateMesh(int[] Mark, int[] _Y_init = null, int nx = 25)
        {
            this.dx = Lx / (Nx - 1);
            this.dy = Ly / (Ny - 1);

            MEM.Alloc2D(Nx, Ny, ref map);

            if (_Y_init == null)
            {
                MEM.Alloc(Nx, ref this.Y_init);
                int rIdx = 0;
                for (int i = 0; i < Nx; i++)
                {
                    if (i < Ny)
                        Y_init[i] = Ny - 1 - i;
                    else
                    {
                        if (i > Nx - Ny - 1)
                            Y_init[i] = rIdx++;
                        else
                            Y_init[i] = 0;
                    }
                }
            }
            else
                this.Y_init = _Y_init;

            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    map[i][j] = -1;
            
            // всего узлов
            int CountNodes = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = Y_init[i]; j < Ny; j++)
                    map[i][j] = CountNodes++;

            int CountNodes1 = (Ny+1)  * Ny + (Nx - (2*Ny)) * Ny;

            //LOG.Print("map", map);
            //LOG.Print("Y_init", Y_init);


            // узлов на контуре
            int counter = 2 * (Nx - 1) + (Ny - Y_init[0] - 1) + (Ny - Y_init[Nx - 1] - 1);
            // элементов в области
            //int CountElems = 2*(Nx-2*Ny-1)* (Ny - 1) + 2 * (Ny - 1) * (Ny - 1);
            // MEM.Alloc(CountElems, ref AreaElems);

            Alloc(0, CountNodes, counter);
            //MEM.Alloc(CountNodes, ref CoordsX);
            //MEM.Alloc(CountNodes, ref CoordsY);
            //MEM.Alloc(counter, ref BoundElems);
            //MEM.Alloc(counter, ref BoundKnots);
            //MEM.Alloc(counter, ref BoundKnotsMark);
            //MEM.Alloc(counter, ref BoundKnotsType);
            //MEM.Alloc(counter, ref BoundElementsMark);
            //MEM.Alloc(counter, ref BoundElementsType);

            //MEM.Alloc(Nx, Ny, ref d_min, "d_min");
            //MEM.Alloc(Nx, Ny, ref d_min_dx, "d_min_dx");
            //MEM.Alloc(Nx, Ny, ref d_min_dy, "d_min_dy");
            //MEM.Alloc2D(Nx, Ny, ref X);
            //MEM.Alloc2D(Nx, Ny, ref Y);
            // 0 ------------------> j (y)
            // |
            // |    Система координат
            // |
            // |
            // V i (x)
            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                double xm = Xmin + i * dx;
                for (int j = Y_init[i]; j < Ny; j++)
                {
                    double ym = Ymin + dy * j;
                    CoordsX[k] = xm;
                    CoordsY[k] = ym;
                    X[i][j] = xm;
                    Y[i][j] = ym;
                    k++;
                }
            }
            
            //LOG.Print("X", X, 4);
            //LOG.Print("Y", Y, 4);
            List <TriElement> lElems = new List<TriElement>();
            int elem = 0;

            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny - 1; j++)
                {
                    // левый уклон сетки
                    if (map[i][j] > -1 && map[i + 1][j] > -1)
                    {
                        if (i < Nx / 2)
                        {
                            //AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i][j + 1]);
                            lElems.Add(new TriElement(map[i][j], map[i + 1][j], map[i][j + 1]));
                            elem++;
                            //AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                            lElems.Add(new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]));
                            elem++;
                        }
                        else
                        {
                            //AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i][j + 1]);
                            lElems.Add(new TriElement(map[i][j], map[i + 1][j], map[i + 1][j + 1]));
                            elem++;
                            //AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                            lElems.Add(new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i][j]));
                            elem++;
                        }
                    }
                    else
                    {
                        if (map[i][j] > -1)
                        {
                            //AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j + 1], map[i][j + 1]);
                            lElems.Add(new TriElement(map[i][j], map[i + 1][j + 1], map[i][j + 1]));
                            elem++;
                        }
                        if (map[i + 1][j] > -1)
                        {
                            //AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                            lElems.Add(new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]));
                            elem++;
                        }
                    }
                }
            }
            AreaElems = lElems.ToArray();
            //Console.WriteLine("TriElement");
            //for (int i = 0; i < AreaElems.Length; i++)
            //    Console.Write(i.ToString() + ": " + AreaElems[i].ToString() + " ");
            //Console.WriteLine();

            int countBE = 0;
            #region Граничные элементы
            // низ - дно
            for (int i = 0; i < Nx-1; i++)
            {
                BoundElems[countBE] = new TwoElement(map[i][Y_init[i]], map[i + 1][Y_init[i + 1]]);
                BoundElementsMark[countBE] = Mark[0];
                countBE++;
            }
            // правая сторона
            //for (int j = Y_init[Nx - 1]; j < Ny - 1; j++)
            //{
            //    BoundElems[countBE] = new TwoElement(map[Nx - 1][j], map[Nx - 1][j + 1]);
            //    BoundElementsMark[countBE] = Mark[1];
            //    BoundElementsType[countBE] = BCType[1];
            //    countBE++;
            //}
            // верх
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[countBE] = new TwoElement(map[Nx - 1 - i][Ny - 1], map[Nx - 2 - i][Ny - 1]);
                BoundElementsMark[countBE] = Mark[1];
                countBE++;
            }
            // левая сторона
            //for (int j = Ny-1 ; j > Y_init[0]; j--)
            //{
            //    BoundElems[countBE] = new TwoElement(map[0][j], map[0][j - 1]);
            //    BoundElementsMark[countBE] = Mark[3];
            //    BoundElementsType[countBE] = BCType[3];
            //    countBE++;
            //}
            #endregion 

            int countKnot = 0;
            #region Граничные узлы
            // низ - дно
            for (int i = 0; i < Nx; i++)
            {
                BoundKnotsMark[countKnot] = Mark[0];
                BoundKnots[countKnot++] = (int)map[i][Y_init[i]];
            }
            // правая сторона
            //for (int j = Y_init[Nx - 1]; j < Ny; j++)
            //{
            //    // задана производная
            //    BoundKnotsMark[countKnot] = Mark[1];
            //    BoundKnotsType[countKnot] = BCType[1];
            //    BoundKnots[countKnot++] = (int)map[Nx - 1][j];
            //}
            // верх
            for (int i = 1; i < Nx - 1; i++)
            {
                // задана производная
                BoundKnotsMark[countKnot] = Mark[1];
                BoundKnots[countKnot++] = (int)map[Nx - 1 - i][Ny - 1];
            }
            // левая сторона
            //for (int j = Ny - 1; j > Y_init[0]; j--)
            //{
            //    // задана функция
            //    BoundKnotsMark[countKnot] = Mark[3];
            //    BoundKnotsType[countKnot] = BCType[3];
            //    BoundKnots[countKnot++] = (int)map[0][j];
            //}
            #endregion 
            //Console.WriteLine("TwoElement");
            //for (int i = 0; i < BoundElems.Length; i++)
            //    Console.Write(BoundElems[i].ToString());
            //Console.WriteLine();
            //Console.WriteLine("TwoElement");
            //for (int i = 0; i < BoundElems.Length; i++)
            //    Console.Write(BoundElems[i].ToString());
            //Console.WriteLine();
            //LOG.Print("map", map);
            //LOG.Print("BoundKnots", BoundKnots);
            //LOG.Print("BoundKnotsMark", BoundKnotsMark);

            //LOG.Print("CoordsX", CoordsX,3);
            //LOG.Print("CoordsY", CoordsY,3);
            CalkDmin(1);
            CalkVDmin();
            hydraulicRadius = HydraulicRadius(1);
        }

        public void Get(int i, ref double A, ref double B, ref double C)
        {
            int i0 = i;
            int j0 = Y_init[i0];
            double x0 = X[i0][j0];
            double y0 = Y[i0][j0];
            int i1 = i + 1;
            int j1 = Y_init[i1];
            double x1 = X[i1][j1];
            double y1 = Y[i1][j1];
            double dx = x1 - x0;
            double dy = y1 - y0;
            A =   dy;
            B = - dx;
            C = y0 * dx - x0 * dy;
        }
        /// <summary>
        /// Расчет растояния от точки (x,y) до прямой (A B C)
        /// </summary>
        public double CalkDmin(double x, double y,double A, double B, double C)
        {
            return Math.Abs(A * x + B * y + C) / Math.Sqrt(A * A + B * B);
        }


        public override void CalkDmin(int waterLevelMark)
        {
            double A0 = 0, B0 = 0, C0 = 0;
            double A1 = 0, B1 = 0, C1 = 0;
            double A2 = 0, B2 = 0, C2 = 0;
            Get(   0  , ref A0, ref B0, ref C0);
            Get(Nx / 2, ref A1, ref B1, ref C1);
            Get(Nx - 2, ref A2, ref B2, ref C2);

            for (int i = 1; i < Nx - 1; i++)
            {
                for (int j = Y_init[i] + 1; j < Ny - 1; j++)
                {
                    double x = X[i][j];
                    double y = Y[i][j];
                    double d0 = CalkDmin(x, y, A0, B0, C0);
                    double d1 = CalkDmin(x, y, A1, B1, C1);
                    double d2 = CalkDmin(x, y, A2, B2, C2);
                    double r_min = Ly + Lx;
                    r_min = Math.Min(r_min, d0);
                    r_min = Math.Min(r_min, d1);
                    r_min = Math.Min(r_min, d2);
                    d_min[i][j] = r_min;
                }
            }
            for (int i = 0; i < Nx; i++)
            {
                d_min[i][Ny - 1] = d_min[i][Ny - 2];
            }
        }

    }
}
