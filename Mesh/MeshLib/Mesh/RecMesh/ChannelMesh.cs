//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 05.07.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: RectFDMesh - базистная разностная сетка 
    /// в четырех гранной области c косым уступом
    /// 
    ///  -----0--------------------------> y (j)
    ///  |        |                 |  
    ///  Lx1      |                 |
    ///  |        |                 |
    ///  |        |                 |
    ///  -----   /                  |
    ///  |      /                   |
    ///  Lx2   /                    |
    ///  |    /                     |
    ///  --- |                      |
    ///  |   |                      |
    ///  |   |                      |
    ///  Lx3 |                      |
    ///  |   |                      |
    ///  --- |                      |
    ///      ------------------------
    ///      |-Ly1|----- Ly2 -------|
    ///      |
    ///      |
    ///      V x (i)     
    /// 
    /// </summary>
    [Serializable]
    public class ChannelMesh : ARectangleMesh
    {
        #region Поля
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        public override int Nx => Nx1 + Nx2 + Nx3 - 2;
        /// <summary>
        /// количество узлов (i) по направлению X в первой подобласти
        /// </summary>
        public int Nx1 { get; set; }
        /// <summary>
        /// количество узлов (i) по направлению X во первой подобласти
        /// </summary>
        public int Nx2 { get; set; }
        /// <summary>
        /// количество узлов (i) по направлению X в третей подобласти
        /// </summary>
        public int Nx3 { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public override int Ny => Ny1 + Ny2 - 1;
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public int Ny1 { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        public int Ny2 { get; set; }
        /// <summary>
        /// Ширина области
        /// </summary>
        public override double Lx => Lx1 + Lx2 + Lx3;
        public double Lx1 { get; set; }
        public double Lx2 { get; set; }
        public double Lx3 { get; set; }
        /// <summary>
        /// Высота области
        /// </summary>
        public override double Ly=>Ly1 + Ly2;
        public double Ly1 { get; set; }
        public double Ly2 { get; set; }

        #endregion
        public ChannelMesh(double Lx1, double Lx2, double Lx3, double Ly1, double Ly2, 
            double Xmin = 0, double Ymin = 0)
            : base(Xmin, Ymin)
        {
            this.Lx1 = Lx1;
            this.Lx2 = Lx2;
            this.Lx3 = Lx3;
            this.Ly1 = Ly1;
            this.Ly2 = Ly2;
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
            // Поиск кратного размера для dx
            int LookN = 1000;
            //this.dx = Lx / (nx - 1);
            //Nx2 = (int)(Lx2 / dx) - 2;
            Nx2 = nx;
            for (int i = 0; i < LookN; i++)
            {
                Nx2++;
                double N1 = (Lx1 * Nx2 + Lx2 - Lx1) / Lx2;
                double N3 = (Lx2 - Lx3 + Nx2 * Lx3) / Lx2;
                Nx1 = (int)N1;
                Nx3 = (int)N3;
                double err = (N1 - Nx1) / Nx1 + (N3 - Nx3) / Nx3;
                if ( err < MEM.Error4)
                    break;
                if (i == LookN-1)
                    Console.WriteLine("Поиск кратных узлов по Х при построении сеток завершен неудачно");
            }
            // Поиск кратного размера для dy
            Ny1 = Nx2;
            Ny2 = (int)( (Ny1 * Ly2 + Ly1 - Ly2) / Ly1 );
      

            int[] nxs = { Nx1, Nx2, Nx3 };
            int[] nys = { Ny1, Ny2 };
            //LOG.Print("nxs", nxs);
            //LOG.Print("nys", nys);

            this.dx = Lx / (Nx - 1);
            this.dy = Ly / (Ny - 1);

            MEM.Alloc2D(Nx, Ny, ref map);
            MEM.Alloc(Nx, ref  Y_init);

            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    map[i][j] = -1;

            int i0 = Nx1 + Nx2 - 1;

            for (int i = 0; i < Nx1; i++)
                Y_init[i] = Ny1 - 1;
            int ik = 0;
            for (int i = Nx1 - 1; i < i0; i++)
                Y_init[i] = Ny1 - 1 - ik++;

            // LOG.Print("Y_init", Y_init);
            // всего узлов
            int CountNodes = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = Y_init[i]; j < Ny; j++)
                    map[i][j] = CountNodes++;

            // LOG.Print("map", map);

            // узлов на контуре
            int counter = 2 * (Nx - 1) + (Ny - 1) + (Ny2 - 1);
            // узлов на контуре
            int CountElems = 2 * (Nx - 1) * (Ny2 - 1)
                           + 2 * (Nx3 - 1) * (Ny1 - 1)
                               + (Nx2 - 1) * (Ny1 - 1);

            Alloc(CountElems, CountNodes, counter);
            //MEM.Alloc(CountElems, ref AreaElems);
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

            int elem = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = Y_init[i]; j < Ny - 1; j++)
                {
                    
                    if( i > 0 && Y_init[i] == j)
                        if( Y_init[i] >= 0  && Y_init[i] < Y_init[i - 1] )
                        {
                            AreaElems[elem] = new TriElement(map[i][j + 1], map[i - 1][j + 1], map[i][j]);
                            elem++;
                        }
                    // левый уклон сетки
                    if (map[i][j] > -1)
                    {
                        AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i][j + 1]);
                        elem++;
                    }
                    AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                    elem++;
                }
            }
            //Console.WriteLine("TriElement");
            //for (int i = 0; i < AreaElems.Length; i++)
            //    Console.WriteLine(i.ToString() +" "+ AreaElems[i].ToString());
            //Console.WriteLine();

            int countBE = 0;
            #region Граничные элементы
            // низ - дно
            for (int i = 0; i < Nx-1; i++)
            {
                BoundElems[countBE] = new TwoElement(map[i][Y_init[i]], map[i + 1][Y_init[i + 1]]);
                if (i < Nx1)
                {
                    BoundElementsMark[countBE] = Mark[0];
                }
                else
                {
                    if (i < i0 )
                    {
                        BoundElementsMark[countBE] = Mark[1];
                    }
                    else
                    {
                        BoundElementsMark[countBE] = Mark[2];
                    }
                }
                countBE++;
            }
            // правая сторона
            for (int j = 0; j < Ny - 1; j++)
            {
                BoundElems[countBE] = new TwoElement(map[Nx - 1][j], map[Nx - 1][j + 1]);
                BoundElementsMark[countBE] = Mark[3];
                countBE++;
            }
            // верх
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[countBE] = new TwoElement(map[Nx - 2 - i][Ny - 1], map[Nx - 1 - i][Ny - 1]);
                BoundElementsMark[countBE] = Mark[4];
                countBE++;
            }
            // левая сторона
            for (int j = Ny-1 ; j > Y_init[0]; j--)
            {
                BoundElems[countBE] = new TwoElement(map[0][j-1], map[0][j]);
                BoundElementsMark[countBE] = Mark[5];
                countBE++;
            }
            #endregion 

            int countKnot = 0;
            #region Граничные узлы
            // низ - дно
            for (int i = 0; i < Nx - 1; i++)
            {
                if (i < Nx1 - 1)
                {
                    BoundKnotsMark[countKnot] = Mark[0];
                }
                else
                {
                    if (i < i0)
                    {
                        BoundKnotsMark[countKnot] = Mark[1];
                    }
                    else
                    {
                        BoundKnotsMark[countKnot] = Mark[2];
                    }
                }
                BoundKnots[countKnot++] = (int)map[i][Y_init[i]];
            }
            // правая сторона
            for (int j = 0; j < Ny; j++)
            {
                // задана производная
                BoundKnotsMark[countKnot] = Mark[3];
                BoundKnots[countKnot++] = (int)map[Nx - 1][j];
            }
            // верх
            for (int i = 1; i < Nx - 1; i++)
            {
                // задана производная
                BoundKnotsMark[countKnot] = Mark[4];
                BoundKnots[countKnot++] = (int)map[Nx - 1 - i][Ny - 1];
            }
            // левая сторона
            for (int j = Ny - 1; j > Y_init[0]; j--)
            {
                // задана функция
                BoundKnotsMark[countKnot] = Mark[5];
                BoundKnots[countKnot++] = (int)map[0][j];
            }
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

            //LOG.Print("CoordsX", CoordsX);
            //LOG.Print("CoordsY", CoordsY);
            CalkDmin(4);

            hydraulicRadius = HydraulicRadius(4);
        }
    }
}
