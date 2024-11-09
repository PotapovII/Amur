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
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: RectFDMesh - базистная разностная сетка 
    /// в четырех гранной области c косым уступом
    /// 
    ///  -----0--------------------------> y (j)
    ///  |      |                   |  
    ///  |        |                 |
    ///  |       |                  |
    ///  Lx1  |                     |
    ///  |   |                      |
    ///  |    |                     |
    ///  |    |                     |
    ///  --- |                      |
    ///      ------------------------
    ///      |-------- Ly2 ---------|
    ///      |
    ///      |
    ///      V x (i)     
    /// 
    /// </summary>
    [Serializable]
    public class ChannelRectMesh : ARectangleMesh
    {
        public ChannelRectMesh(int Nx,int Ny, double Lx, double Ly, double Xmin = 0, double Ymin = 0)
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
        public override void CreateMesh(int[] Mark, int[] _Y_init = null, int nx = 25)
        {
            this.dx = Lx / (Nx - 1);
            this.dy = Ly / (Ny - 1);

            MEM.Alloc2D(Nx, Ny, ref map);

            if (_Y_init == null)
            {
                MEM.Alloc(Nx, ref this.Y_init);
                for (int i = 0; i < Nx; i++)
                    Y_init[i] = 0;
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

            // узлов на контуре
            int counter = 2 * (Nx - 1) + (Ny - Y_init[0] - 1) + (Ny - Y_init[Nx - 1] - 1);
            // элементов в области
            int CountElems = 0;
            for (int i = 0; i < Nx - 1; i++)
            {
                int n1 = Ny - Y_init[i];
                int n2 = Ny - Y_init[i+1];
                int dn = n1 - n2;
                int elems = 2 * (n1 - 1) - dn;
                CountElems += elems;
            }

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
                    // левый уклон сетки
                    if (map[i][j] > -1 && map[i + 1][j] > -1)
                    {
                        AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j], map[i][j + 1]);
                        elem++;
                        AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                        elem++;
                    }
                    else
                    {
                        if (map[i][j] > -1)
                        {
                            AreaElems[elem] = new TriElement(map[i][j], map[i + 1][j + 1], map[i][j + 1]);
                            elem++;
                        }
                        else
                        {
                            AreaElems[elem] = new TriElement(map[i + 1][j + 1], map[i][j + 1], map[i + 1][j]);
                            elem++;
                        }
                    }
                }
            }
            //Console.WriteLine("TriElement");
            //for (int i = 0; i < AreaElems.Length; i++)
            //    Console.Write(i.ToString() + " " + AreaElems[i].ToString() + " ");
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
            for (int j = Y_init[Nx - 1]; j < Ny - 1; j++)
            {
                BoundElems[countBE] = new TwoElement(map[Nx - 1][j], map[Nx - 1][j + 1]);
                BoundElementsMark[countBE] = Mark[1];
                countBE++;
            }
            // верх
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundElems[countBE] = new TwoElement(map[Nx - 1 - i][Ny - 1], map[Nx - 2 - i][Ny - 1]);
                BoundElementsMark[countBE] = Mark[2];
                countBE++;
            }
            // левая сторона
            for (int j = Ny-1 ; j > Y_init[0]; j--)
            {
                BoundElems[countBE] = new TwoElement(map[0][j], map[0][j - 1]);
                BoundElementsMark[countBE] = Mark[3];
                countBE++;
            }
            #endregion 

            int countKnot = 0;
            #region Граничные узлы
            // низ - дно
            for (int i = 0; i < Nx - 1; i++)
            {
                BoundKnotsMark[countKnot] = Mark[0];
                BoundKnots[countKnot++] = (int)map[i][Y_init[i]];
            }
            // правая сторона
            for (int j = Y_init[Nx - 1]; j < Ny; j++)
            {
                // задана производная
                BoundKnotsMark[countKnot] = Mark[1];
                BoundKnots[countKnot++] = (int)map[Nx - 1][j];
            }
            // верх
            for (int i = 1; i < Nx - 1; i++)
            {
                // задана производная
                BoundKnotsMark[countKnot] = Mark[2];
                BoundKnots[countKnot++] = (int)map[Nx - 1 - i][Ny - 1];
            }
            // левая сторона
            for (int j = Ny - 1; j > Y_init[0]; j--)
            {
                // задана функция
                BoundKnotsMark[countKnot] = Mark[3];
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
            CalkDmin(2);
            hydraulicRadius = HydraulicRadius(2);
        }
    }
}
