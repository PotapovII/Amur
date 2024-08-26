//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.06.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    using GeometryLib;
    /// <summary>
    /// ОО: IRectFVMesh - базистная контрольно - объемная сетка в четырех гранной области 
    /// </summary>
    //===========================================================================================================================
    //  Переворачиваем систему координат в первом квадранте на 90 градусов по частовй стрелке
    //===========================================================================================================================
    //      ------->  y, U
    //      |
    //      |
    //      |
    //      |
    //      V  x V
    //                           
    //             i=0,j=0      j=1           j=2           j=3          j=4          j=5 = jmax                                  j (y)  
    //      > ---- V*------->-----V*---->------V*----->-----V*------>-----V*----->---- V* -- xu[0][j] ----------------- x[0][j]  ----->
    //        i=0  |       "     |     "      |      "      |      "      |      "     |             ^      Dx[0][j]         ^
    //             |       "     |     "      |      "      |      "      |      "     |             |        V              | 
    //             V ======"=====V====="======V======"======V======"======V======"=====V  -------- hx[0][j]  ------         dx
    //             |       "     |     "      |      "      |      "      |      "     |             |        ^              | 
    //             |       "     |     "      |      "      |      "      |      "     |             V        |              V 
    //      > ---- *------->-----*----->------*------>------*------>------*------>-----* --- xu[1][j] ---- Dx[1][j] --- x[1][j] 
    //         i=1 |       "     |     "      |      "      |      "      |      "     |             ^        |
    //             |       "     |     "      |      "      |      "      |      "     |             |        V
    //             V ======"=====V====="======V======"======V======"======V======"=====V  -------- hx[1][j] <= x[1][j] - x[0][j];
    //             |       "     |     "      |      "      |      "      |      "     |             |        ^ 
    //             |       "     |     "      |      "      |      "      |      "     |             V        |
    //      > ---- *------->-----*----->------*------>------*------>------*------>-----* --- xu[2][j] ---- Dx[2][j] --- x[2][j]
    //        i=2  |       "     |     "      |      "      |      "      |      "     |             ^        |
    //             |       "     |     "      |      "      |      "      |      "     |             |        V 
    //             V ======"=====V====="======V======"======V======"======V======"=====V  -------- hx[2][j]  ------   
    //             |       "     |     "      |      "      |      "      |      "     |             |        ^
    //             |       "     |     "      |      "      |      "      |      "     |             V      Dx[3][j]    
    //      > ---- V*------>-----V*---->------V*----->------V*----->------V*----->-----V* -- xu[4][j]  ---------------- x[3][j]  
    //  i=3 = imax |       |     |     |      |      |      |      |      |      |     |         
    //      |      |       |     |     |      |      |      |      |      |      |     |         
    //           yv[i][0]  |  yv[i][1] |  yv[i][2]   |  yv[i][3]   |  yv[i][4]   |  yv[i][5] 
    //             |       |           |             |             |             |     |
    //             |<Dy[0]>|<--Dy[1]-->|<---Dy[2]--->|<---Dy[3]--->|<---Dy[4]--->|Dy[5]|
    //             |
    //             |<- hy[i][0]->|<-hy[i][1]->|<-hy[i][2]-->|<- hy[i][3]->|<-hy[i][4]->|
    //             |             |            |             |             |            |     
    //            y[i][0]     y[i][1]      y[i][2]       y[i][3]       y[i][4]      y[i][5]               
    //             |
    //             |<--- dy ---->|               
    //             V i (x)          
    //
    //  Nx = imax +1 => 4;
    //  Ny = jmax + 1 => 6;
    //  * - узел КО сетки (давление, температура или другие скалярные функции
    //  V и > - узелы скорости на КО сетки при решении задач гидродинамики и переноса

    public class RectFVMesh : IRectFVMesh
    {
        /// <summary>
        /// Сетка триангуляции для вывода полей на рендеренга (Заменить на IFEMesh!!!!!!!)
        /// </summary>
        protected TriMesh triMesh = null;


        #region IMesh
        /// <summary>
        /// Порядок сетки на которой работает функция формы
        /// </summary>
        public TypeRangeMesh typeRangeMesh => TypeRangeMesh.mRange0;
        /// <summary>
        /// Тип КЭ сетки в 1D и 2D
        /// </summary>
        public TypeMesh typeMesh => TypeMesh.Rectangle;
        /// <summary>
        /// Вектор конечных элементов в области
        /// </summary>
        public TriElement[] GetAreaElems() => triMesh.GetAreaElems();
        /// <summary>
        /// Вектор конечных элементов на границе
        /// </summary>
        public TwoElement[] GetBoundElems() => triMesh.GetBoundElems();
        /// <summary>
        /// Получить массив маркеров для граничных элементов 
        /// </summary>
        public int[] GetBElementsBCMark() => triMesh.GetBElementsBCMark();
        /// <summary>
        /// Массив граничных узловых точек
        /// </summary>
        public int[] GetBoundKnots() => triMesh.GetBoundKnots();
        /// <summary>
        /// Массив меток  для граничных узловых точек
        /// </summary>
        public int[] GetBoundKnotsMark() => triMesh.GetBoundKnotsMark();
        /// <summary>
        /// Координаты X или Y для узловых точек 
        /// </summary>
        public double[] GetCoords(int dim)
        {
            return triMesh.GetCoords(dim);
        }
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int CountElements => triMesh.CountElements;
        /// <summary>
        /// Количество граничных элементов
        /// </summary>
        public int CountBoundElements => triMesh.CountBoundElements;
        /// <summary>
        /// Количество узлов
        /// </summary>
        public int CountKnots => triMesh.CountKnots;
        /// <summary>
        /// Количество граничных узлов
        /// </summary>
        public int CountBoundKnots => triMesh.CountBoundKnots;
        /// <summary>
        /// Диапазон координат для узлов сетки
        /// </summary>
        public void MinMax(int dim, ref double min, ref double max)
        {
            triMesh.MinMax(dim, ref min, ref max);
        }
        /// <summary>
        /// Получить узлы элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementKnots(uint i, ref uint[] knots)
        {
            return triMesh.ElementKnots(i, ref knots);
        }
        /// <summary>
        /// Получить узлы граничного элемента
        /// </summary>
        /// <param name="i">номер элемента</param>
        public TypeFunForm ElementBoundKnots(uint i, ref uint[] bknot)
        {
            return triMesh.ElementBoundKnots(i, ref bknot);
        }
        /// <summary>
        /// Получить координаты Х вершин КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>Координаты Х вершин КЭ</returns>
        public void GetElemCoords(uint i, ref double[] X, ref double[] Y) => 
            triMesh.GetElemCoords(i, ref X, ref Y);
        /// <summary>
        /// Получить значения функции связанной с сеткой в вершинах КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>значения функции связанной с сеткой в вершинах КЭ</returns>
        public void ElemValues(double[] Values, uint i, ref double[] elementValue) =>
                    triMesh.ElemValues(Values, i, ref elementValue);
        /// <summary>
        /// Получить максимальную разницу м/д номерами узнов на КЭ
        /// </summary>
        /// <param name="i">номер элемента</param>
        /// <returns>максимальная разница м/д номерами узнов на КЭ</returns>
        public uint GetMaxKnotDecrementForElement(uint i)
        {
            return triMesh.GetMaxKnotDecrementForElement(i);
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        /// <param name="element">номер конечного элемента</param>
        /// <returns></returns>
        public double ElemSquare(uint element)
        {
            return triMesh.ElemSquare(element);
        }
        /// <summary>
        /// Вычисление площади КЭ
        /// </summary>
        public double ElemSquare(TriElement element)
        {
            return triMesh.ElemSquare(element);
        }
        /// <summary>
        /// Вычисление длины граничного КЭ
        /// </summary>
        /// <param name="belement">номер граничного конечного элемента</param>
        /// <returns></returns>
        public double GetBoundElemLength(uint belement)
        {
            return triMesh.GetBoundElemLength(belement);
        }
        /// <summary>
        /// Получить выборку граничных узлов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        public uint[] GetBoundKnotsByMarker(int id)
        {
            return triMesh.GetBoundKnotsByMarker(id);
        }
        /// <summary>
        /// Получить выборку граничных элементов по типу ГУ
        /// </summary>
        /// <param name="i">тип ГУ</param>
        /// <returns></returns>
        public uint[][] GetBoundElementsByMarker(int id)
        {
            return triMesh.GetBoundElementsByMarker(id);
        }
        /// <summary>
        /// Получить тип граничных условий для граничного элемента
        /// </summary>
        /// <param name="elem">граничный элемент</param>
        /// <returns>ID типа граничных условий</returns>
        public int GetBoundElementMarker(uint elem)
        {
            return triMesh.GetBoundElementMarker(elem);
        }
        /// <summary>
        /// Ширина ленты в глобальной матрице жнсткости
        /// </summary>
        /// <returns></returns>
        public uint GetWidthMatrix() => triMesh.GetWidthMatrix();
        /// <summary>
        /// Клонирование объекта сетки
        /// </summary>
        public IMesh Clone() { return new RectFVMesh(this); }
        /// <summary>
        /// Тестовая печать КЭ сетки в консоль
        /// </summary>
        public void Print() => print();

        #endregion

        #region RectFVMesh
        /// <summary>
        /// Количество узлов по направлению X 
        /// (количество узлов по i)
        /// </summary>
        public int Nx { get; set; }
        /// <summary>
        /// Количество узлов по направлению Y
        /// (количество узлов по j)
        /// </summary>
        public int Ny { get; set; }
        /// <summary>
        /// Количество контрольных объемов по направлению X 
        /// (количество узлов по i)
        /// </summary>
        public int imax { get; set; }
        /// <summary>
        /// Количество контрольных объемов по направлению Y
        /// (количество узлов по j)
        /// </summary>
        public int jmax { get; set; }
        /// <summary>
        /// Координаты х для цетров контрольных объемов
        /// </summary>
        public double[][] x { get; set; }
        /// <summary>
        /// Координаты y для цетров контрольных объемов
        /// </summary>
        public double[][] y { get; set; }
        /// <summary>
        /// Координаты узловых точек для скорости u (для   параллельно - перпендикулярной сетки совпадают с x[][])
        /// </summary>
        public double[][] xu { get; set; }
        /// <summary>
        /// Координаты узловых точек для скорости v (для   параллельно - перпендикулярной сетки совпадают с y[][])
        /// </summary>
        public double[][] yv { get; set; }
        /// <summary>
        /// Расстояние между узловыми точками для скорости u
        /// или ширина контрольного объема
        /// </summary>
        public double[][] Dx { get; set; }
        /// <summary>
        /// Расстояние между узловыми точками для скорости v
        /// или высота контрольного объема
        /// </summary>
        public double[][] Dy { get; set; }
        /// <summary>
        /// Расстояние между центрами контрольных объемов по х
        /// </summary>
        public double[][] hx { get; set; }
        /// <summary>
        /// Расстояние между центрами контрольных объемов по у
        /// </summary>
        public double[][] hy { get; set; }
        #endregion
        double xa = 0;
        double xb = 0;
        double ya = 0;
        double yb = 0;

        #region
        /// <summary>
        /// Получить образ симплекс сетки для модуля графики
        /// </summary>
        /// <returns></returns>
        protected TriMesh GetMesh()
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
            // Узлы
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
        #endregion
        public RectFVMesh(int imax, int jmax, double xa, double xb, double ya, double yb)
        {
            this.imax = imax;
            this.jmax = jmax;
            Nx = imax + 1;
            Ny = jmax + 1;
            Init();
            OnGridCalculationRectangle(xa, xb, ya, yb);
        }
        public RectFVMesh(RectFVMesh mesh)
        {
            SetMesh(mesh);
            triMesh = GetMesh();
        }
        public RectFVMesh(IMesh mesh)
        {
            RectFVMesh m = mesh as RectFVMesh;
            if (m != null)
            {
                SetMesh(m);
                triMesh = GetMesh();
            }
            else
            {
                triMesh = mesh as TriMesh;
                if (triMesh != null)
                    CreateMesh(triMesh);
            }
        }
        public void print()
        {
            if (triMesh != null)
                triMesh.Print();

        }
        protected void Init()
        {


            x = new double[Nx][];
            xu = new double[Nx][];
            Dx = new double[imax][];
            hx = new double[Nx][];
            for (int i = 0; i < Nx; i++)
            {
                x[i] = new double[Ny];
                xu[i] = new double[Ny];
                hx[i] = new double[Ny];
            }
            for (int i = 0; i < imax; i++)
                Dx[i] = new double[Ny];

            y = new double[Nx][];
            yv = new double[Nx][];
            Dy = new double[Nx][];
            hy = new double[Nx][];
            for (int i = 0; i < Nx; i++)
            {
                y[i] = new double[Ny];
                yv[i] = new double[Ny];
                Dy[i] = new double[jmax];
                hy[i] = new double[Ny];
            }
        }

        public void SetMesh(RectFVMesh mesh)
        {
            imax = mesh.imax;
            jmax = mesh.jmax;
            Nx = mesh.Nx;
            Ny = mesh.Ny;

            MEM.Copy(x, mesh.x);
            MEM.Copy(xu, mesh.xu);
            MEM.Copy(Dx, mesh.Dx);
            MEM.Copy(hx, mesh.hx);

            MEM.Copy(y, mesh.y);
            MEM.Copy(yv, mesh.yv);
            MEM.Copy(Dy, mesh.Dy);
            MEM.Copy(hy, mesh.hy);
        }

        public void CreateMesh(TriMesh mesh)
        {
            try
            {
                int Imax = 2, Jmax = 2;
                double[] tx = mesh.GetCoords(0);
                double[] ty = mesh.GetCoords(1);
                Locator.FindMinStep(tx, ref Imax, ref xa, ref xb);
                Locator.FindMinStep(ty, ref Jmax, ref ya, ref yb);
                imax = Imax;
                jmax = Jmax;
                Nx = imax + 1;
                Ny = jmax + 1;
                Init();
                OnGridCalculationRectangle(xa, xb, ya, yb);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Расчет координат сетки задачи в прямоугольнике 
        /// </summary>
        public void OnGridCalculationRectangle(double xa, double xb, double ya, double yb)
        {
            int i, j;
            double Lx = xb - xa;
            double dx = Lx / (imax - 1);
            for (j = 0; j < Ny; j++)
            {
                x[0][j] = xu[1][j] = 0;
            }
            for (i = 1; i < imax; i++)
            {
                for (j = 0; j < Ny; j++)
                {
                    // координаты узловых точек для скорости u
                    xu[i + 1][j] = xu[i][j] + dx + xa;
                    // x - координата цетра контрольного объема 
                    x[i][j] = (xu[i + 1][j] + xu[i][j]) / 2;
                    // расстояние между центрами контрольных объемов по х
                    hx[i][j] = x[i][j] - x[i - 1][j];
                    // расстояние между узловыми точеками для скорости u
                    Dx[i][j] = xu[i + 1][j] - xu[i][j];
                }
            }
            for (j = 0; j < Ny; j++)
            {
                x[imax][j] = xu[imax][j];
                hx[imax][j] = x[imax][j] - x[imax - 1][j];
            }
            double Ly = yb - ya;
            double dy = Ly / (jmax - 1);
            for (i = 0; i < Nx; i++)
            {
                y[i][0] = yv[i][1] = 0;
            }
            for (i = 0; i < Nx; i++)
            {
                for (j = 1; j < jmax; j++)
                {
                    // координаты узловых точек для скорости v
                    yv[i][j + 1] = yv[i][j] + dy + ya;
                    // y - координата цетра контрольного объема
                    y[i][j] = (yv[i][j + 1] + yv[i][j]) / 2;
                    // расстояние между центрами контрольных объемов по у
                    hy[i][j] = y[i][j] - y[i][j - 1];
                    // расстояние между узловыми точеками для скорости v
                    Dy[i][j] = yv[i][j + 1] - yv[i][j];
                }
                y[i][jmax] = yv[i][jmax];
                hy[i][jmax] = y[i][jmax] - y[i][jmax - 1];
            }
        }
        /// <summary>
        /// Конверсися генерируемой сетки в расчетной области в формат КО сетки
        /// </summary>
        public void ConvertMeshToMesh()
        {
            //===========================================================================================================================
            //  Переворачиваем систему координат в первом квадранте на 90 градусов по частовй стрелке
            //===========================================================================================================================
            //      ------->  y, V
            //      |
            //      |
            //      |
            //      V  x V
            //                                        yv[i][j] = 0.5 * (yy[i][j] + yy[i][j - 1]);
            //===========================================================================================================================
            //                      yy[i][0]               yy[i][1]             yy[i][2]              yy[i][4]     
            //        
            //                      y[i][0]     y[i][1]      |        y[i][2]      |        y[i][3]   y[i][4]
            //                                               |                     | 
            //                         |           |         |           |         |          |          |
            //              x[0][j]   *00V>-->-----*V--------01>---------*V--------02>--------*V---->--*04V>---------> y (j)  V
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |  
            //              x[1][j]    *>          *         >           *         >          *          *>
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |  
            //                         10V---------V---------11----------V---------12---------V----------13V  -----
            //                         |                     |                     |                     |     ^
            //                         |                     |                     |                     |     |
            //                         |                     |                     |                     |     |
            //                         |                     |                     |                     |     |
            //               x[2][j]   *>          *         >           *         >          *          *>    dx
            //                         |                     |                     |                     |     |
            //                         |                     |                     |                     |     |
            //                         |                     |                     |                     |     |
            //                         |                     |                     |                     |     V
            //                         20V---------V---------21----------V----------22--------V----------23V  -----
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |  
            //               x[3][j]   *>          *         >           *         >          *          *>  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |
            //                         30V---------V---------31----------V---------32---------V----------33V
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |  
            //                x[4][j]  *>          *         >           *         >          *     >    *  
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |  
            //                         |                     |                     |                     |
            //                         |                     |                     |                     |  
            //                x[5][j] *40V>-->----*V--------41>---------*V--------42>--------*V---->---*43V>
            //                         |                     |
            //                         |                     |
            //                         |< ------ dy -------->| 
            //                         V
            //                       x (i) V


            int Ny = 5;
            int Nx = 6;
            int imax = Nx - 1;
            int jmax = Ny - 1;
            double Lx = 14;
            double Ly = 6;

            #region Базовая опорная сетка имеет на 1 узел меньше по каждой оси
            double dx = Lx / (imax - 1);
            double dy = Ly / (jmax - 1);
            double[][] xx = null;
            double[][] yy = null;
            MEM.Alloc2D(imax, jmax, ref xx);
            MEM.Alloc2D(imax, jmax, ref yy);
            for (int i = 0; i < imax; i++)
            {
                double X = dx * i;
                for (int j = 0; j < jmax; j++)
                {
                    double Y = dy * j;
                    xx[i][j] = X;
                    yy[i][j] = Y;
                }
            }
            #endregion

            #region Память
            x = new double[Nx][];
            xu = new double[Nx][];
            Dx = new double[imax][];
            hx = new double[Nx][];
            for (int i = 0; i < Nx; i++)
            {
                x[i] = new double[Ny];
                xu[i] = new double[Ny];
                hx[i] = new double[Ny];
            }
            for (int i = 0; i < imax; i++)
                Dx[i] = new double[Ny];

            y = new double[Nx][];
            yv = new double[Nx][];
            Dy = new double[Nx][];
            hy = new double[Nx][];
            for (int i = 0; i < Nx; i++)
            {
                y[i] = new double[Ny];
                yv[i] = new double[Ny];
                Dy[i] = new double[jmax];
                hy[i] = new double[Ny];
            }
            #endregion


            #region Генерация КО сетки по опорной сетке

            // координаты узловых точек для скорости u
            for (int i = 1; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    xu[i][j] = 0.5 * (xx[i][j] + xx[i][j - 1]);

            // координаты узловых точек для скорости v
            for (int i = 0; i < Nx; i++)
                for (int j = 1; j < Ny; j++)
                    yv[i][j] = 0.5 * (yy[i][j] + yy[i][j - 1]);


            #region // Расчет координат центров КО
            // в области
            for (int i = 1; i < Ny; i++)
            {
                for (int j = 1; j < Ny; j++)
                {
                    x[i][j] = 0.25 * (xx[i - 1][j - 1] + xx[i - 1][j] + xx[i][j - 1] + xx[i][j]);
                    y[i][j] = 0.25 * (yy[i - 1][j - 1] + yy[i - 1][j] + yy[i][j - 1] + yy[i][j]);
                }
            }
            // в углах области
            y[0][0] = yy[0][0];
            y[imax][0] = yy[imax - 1][0];
            y[0][jmax] = yy[0][jmax - 1];
            y[imax][jmax] = yy[imax - 1][jmax - 1];
            x[0][0] = xx[0][0];
            x[imax][0] = xx[imax - 1][0];
            x[0][jmax] = xx[0][jmax - 1];
            x[imax][jmax] = xx[imax - 1][jmax - 1];
            // на границах по оси X (i)
            for (int i = 1; i < imax; i++)
            {
                y[i][0] = 0.5 * (yy[i][0] + yy[i - 1][0]);
                y[i][jmax] = 0.5 * (yy[i][jmax - 1] + yy[i - 1][jmax - 1]);

                x[i][0] = 0.5 * (xx[i][0] + xx[i - 1][0]);
                x[i][jmax] = 0.5 * (xx[i][jmax - 1] + xx[i - 1][jmax - 1]);
            }
            // на границах по осях Y  (j)
            for (int j = 1; j < jmax; j++)
            {
                y[0][j] = 0.5 * (yy[0][j] + yy[0][j - 1]);
                y[imax][j] = 0.5 * (yy[imax - 1][j] + yy[imax - 1][j - 1]);

                x[0][j] = 0.5 * (xx[0][j] + xx[0][j - 1]);
                x[imax][j] = 0.5 * (xx[imax - 1][j] + xx[imax - 1][j - 1]);
            }
            #endregion

            // Расчет шага
            for (int i = 0; i < imax - 1; i++)
            {
                for (int j = 0; j < jmax; j++)
                {
                    int ii = j + 1;
                    dx = xx[i][j] - xx[ii][j];
                    dy = yy[i][j] - yy[ii][j];
                    // расстояние  H между узловыми точеками для скорости u (высота стенки КО) для qv = v H 
                    Dx[i + 1][j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
            for (int i = 0; i < Nx - 1; i++)
            {
                for (int j = 0; j < jmax - 1; j++)
                {
                    // расстояние W между узловыми точеками для скорости v (длина стенки КО) для qx = u W 
                    int jj = j + 1;
                    dx = xx[i][j] - xx[i][jj];
                    dy = yy[i][j] - yy[i][jj];
                    Dy[i][jj] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
            for (int i = 0; i < imax; i++)
            {
                for (int j = 0; j < jmax + 1; j++)
                {
                    int ii = i + 1;
                    dx = x[i][j] - x[ii][j];
                    dy = y[i][j] - y[ii][j];
                    // расстояние между центрами контрольных объемов по х (для вычисления производных по оси Х)
                    hx[ii][j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < jmax; j++)
                {
                    int jj = j + 1;
                    dx = x[i][j] - x[i][jj];
                    dy = y[i][j] - y[i][jj];
                    // расстояние между центрами контрольных объемов по у (для вычисления производных по оси Y)
                    hy[i][jj] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
            #endregion
        }
    }
}
