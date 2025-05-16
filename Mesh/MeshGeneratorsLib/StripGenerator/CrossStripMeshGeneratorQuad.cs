//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 04.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    using System;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using GeometryLib;

    /// <summary>
    /// ОО: Фронтальный генератор КЭ трехузловой сетки для русла реки в 
    /// створе с заданной геометрией дна и уровнем свободной поверхности
    /// </summary>
    [Serializable]
    public class CrossStripMeshGeneratorQuad : IStripMeshGenerator
    {
        /// <summary>
        /// Опции для генерации Ленточной КЭ сетки 
        /// </summary>
        public CrossStripMeshOption Option { get; }

        protected double[] xx, yy;
        protected HKnot left, right;
        /// <summary>
        /// Правая береговая точка
        /// </summary>
        public HKnot Right() => right;
        /// <summary>
        /// Левая береговая точка
        /// </summary>
        public HKnot Left() => left;
        /// <summary>
        /// флаг левой берега
        /// </summary>
        public bool DryLeft() => false;
        /// <summary>
        /// флаг правого берега
        /// </summary>
        public bool DryRight() => false;

        /// <summary>
        /// интерполяция дна
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public double Zeta(double arg)
        {
            return yy[0];
        }

        /// <summary>
        /// количество узлов по y и z
        /// </summary>
        protected int Ny = 0, Nz;
        /// <summary>
        /// размер створа по по y и z
        /// </summary>
        protected double W,  H;
        /// <summary>
        /// шаг сетки
        /// </summary>
        protected double dy, dz;
        /// <summary>
        /// Создаваемая сетка
        /// </summary>
        protected ComplecsMesh mesh = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="MAXElem">максимальное количество КЭ сетки</param>
        /// <param name="MAXKnot">максимальное количество узлов сетки</param>
        public CrossStripMeshGeneratorQuad(CrossStripMeshOption Option)
        {
            this.Option = Option;
        }

        public void GetMap(ref uint[][] map)
        {
            MEM.Alloc(Ny, Nz, ref map, "map");
            uint k = 0;
            for (uint i = 0; i < Ny; i++)
                for (int j = 0; j < Nz; j++)
                    map[i][j] = k++;
        }

        public IMesh CreateMesh(ref double WetBed, ref int[][] riverGates, 
                double WaterLevel, double[] xx, double[] yy, int Ny = 0)
        {
            int elem = 0;
            uint[][] map = null;
            try
            {
                this.xx = xx;
                this.yy = yy;
                left = new HKnot(xx[0], yy[1]);
                right = new HKnot(xx[1], yy[1]);
                W = xx[1] - xx[0];
                H = WaterLevel - yy[0];

                this.Ny = Ny;
                double dy = W / (Ny - 1);
                Nz = (int)(H / dy) + 1;
                double dz = H / (Nz - 1);

                mesh = new ComplecsMesh();

                int counter = 2 * (Ny - 1) + 2 * (Nz - 1);
                int CountNodes = Ny * Nz;
                int CountElems = 2 * (Ny - 1) * (Nz - 1);

                MEM.Alloc(CountElems, 3, ref mesh.AreaElems, "mesh.AreaElems");
                MEM.Alloc(CountElems, ref mesh.AreaElemsFFType, "mesh.AreaElemsFFType");

                MEM.Alloc(counter, 2, ref mesh.BoundElems, "mesh.BoundElems");
                MEM.Alloc(counter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");

                MEM.Alloc(counter, ref mesh.BoundKnots, "mesh.BoundKnots");
                MEM.Alloc(counter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");

                MEM.Alloc(CountNodes, ref mesh.CoordsX, "mesh.CoordsX");
                MEM.Alloc(CountNodes, ref mesh.CoordsY, "mesh.CoordsY");

                
                GetMap(ref map);
                uint k = 0;
                //   0----- --> Z (j)
                //   |    |
                //   |    |
                //   |    | L
                //   |    |
                //   |____|
                //   |  H
                //   V
                //   Y (i)
                for (uint i = 0; i < Ny; i++)
                {
                    double ym = i * dy;
                    for (int j = 0; j < Nz; j++)
                    {
                        double zm = dz * j;
                        mesh.CoordsX[k] = ym;
                        mesh.CoordsY[k] = zm;
                        k++;
                    }
                }
                elem = 0;
                for (int i = 0; i < Ny - 1; i++)
                {
                    for (int j = 0; j < Nz - 1; j++)
                    {

                        //double dx1 = mesh.CoordsX[map[i][j]] - mesh.CoordsX[map[i + 1][j + 1]];
                        //double dy1 = mesh.CoordsY[map[i][j]] - mesh.CoordsY[map[i + 1][j + 1]];
                        //double L1 = dx1 * dx1 + dy1 * dy1;

                        //double dx2 = mesh.CoordsX[map[i + 1][j]] - mesh.CoordsX[map[i][j + 1]];
                        //double dy2 = mesh.CoordsY[map[i + 1][j]] - mesh.CoordsY[map[i][j + 1]];
                        //double L2 = dx2 * dx2 + dy2 * dy2;
                        //if (L1 <= 0.99 * L2)
                        //{
                            //  ij ----- i+1 j
                            //  |  \    1  |
                            //  |     \    |
                            //  |  2     \ |   
                            //  ij+1---- i+1j+1
                            mesh.AreaElems[elem][0] = map[i][j];
                            mesh.AreaElems[elem][1] = map[i + 1][j];
                            mesh.AreaElems[elem][2] = map[i + 1][j + 1];
                            mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                            elem++;
                            mesh.AreaElems[elem][0] = map[i + 1][j + 1];
                            mesh.AreaElems[elem][1] = map[i][j + 1];
                            mesh.AreaElems[elem][2] = map[i][j];
                            mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                            elem++;
                        //}
                        //else
                        //{
                        //    //  ij ----- i+1 j
                        //    //  |  1    /  |
                        //    //  |     /    |
                        //    //  |  /     2 |   
                        //    //  ij+1---- i+1j+1
                        //    mesh.AreaElems[elem][0] = map[i][j];
                        //    mesh.AreaElems[elem][1] = map[i][j + 1];
                        //    mesh.AreaElems[elem][2] = map[i + 1][j];
                        //    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                        //    elem++;
                        //    mesh.AreaElems[elem][0] = map[i][j + 1];
                        //    mesh.AreaElems[elem][1] = map[i + 1][j + 1];
                        //    mesh.AreaElems[elem][2] = map[i + 1][j];
                        //    mesh.AreaElemsFFType[elem] = TypeFunForm.Form_2D_Triangle_L1;
                        //    elem++;
                        //}
                    }
                }
                k = 0;
                //   0----- --> Z (j)
                //   |    |
                //   |    |
                //   |    | L
                //   |    |
                //   |____|
                //   |  H
                //   V
                //   Y (i)
                // низ
                for (int i = 0; i < Ny - 1; i++)
                {
                    mesh.BoundElems[k][0] = map[i][0];
                    mesh.BoundElems[k][1] = map[i + 1][0];
                    mesh.BoundElementsMark[k] = 0;
                    // задана функция
                    mesh.BoundKnotsMark[k] = 0;
                    mesh.BoundKnots[k++] = (int)map[i][0];
                }
                // правая сторона
                for (int i = 0; i < Nz - 1; i++)
                {
                    mesh.BoundElems[k][0] = map[Ny - 1][i];
                    mesh.BoundElems[k][1] = map[Ny - 1][i + 1];
                    mesh.BoundElementsMark[k] = 0;
                    // задана функция
                    mesh.BoundKnotsMark[k] = 1;
                    mesh.BoundKnots[k++] = (int)map[Ny - 1][i];
                }
                // верх
                for (int i = 0; i < Ny - 1; i++)
                {
                    mesh.BoundElems[k][0] = map[Ny - 1 - i][Nz - 1];
                    mesh.BoundElems[k][1] = map[Ny - 2 - i][Nz - 1];
                    mesh.BoundElementsMark[k] = 2;
                    // задана производная
                    mesh.BoundKnotsMark[k] = 2;
                    mesh.BoundKnots[k++] = (int)map[Ny - 1 - i][Nz - 1];
                }
                // левая сторона
                for (int i = 0; i < Nz - 1; i++)
                {
                    mesh.BoundElems[k][0] = map[0][Nz - i - 1];
                    mesh.BoundElems[k][1] = map[0][Nz - i - 2];
                    mesh.BoundElementsMark[k] = 3;
                    // задана производная
                    mesh.BoundKnotsMark[k] = 3;
                    mesh.BoundKnots[k++] = (int)map[0][Nz - i - 1];
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("ОШИБКА: Проверте согласованность данных по геометрии створа");
            }
            return mesh;
        }

    }
}

