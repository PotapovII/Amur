//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 20.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.TapeGenerator
{
    using MeshLib;
    using MemLogLib;
    using CommonLib;
    /// <summary>
    /// Генератор сетки для визуализации одномерных задач
    /// </summary>
    public class TapeMeshGenerator
    {
        // ============================
        //       Система координат     
        //     dy                      
        //   |---|----------> Y j      
        //   | dx                      
        //   |---                      
        //   |
        //   |
        //   |
        //   V X  i
        // ============================
        /// <summary>
        /// Генерация триангуляционной КЭ сетки в для квази одномерной области
        /// </summary>
        public static IMesh CreateMesh(double[] xx, double[] zeta, double[] wl)
        {
            TriMesh mesh = new TriMesh();
            int Nx = xx.Length;
            int Ny = 2;
            int counter = 2 * (Nx - 1) + 2 * (Ny - 1);
            int CountNodes = Nx * Ny;
            int CountElems = 2 * (Nx - 1) * (Ny - 1);

            MEM.Alloc(CountElems, ref mesh.AreaElems, "mesh.AreaElems");
            MEM.Alloc(counter, ref mesh.BoundElems, "mesh.BoundElems");
            MEM.Alloc(counter, ref mesh.BoundElementsMark, "mesh.BoundElementsMark");
            MEM.Alloc(counter, ref mesh.BoundKnots, "mesh.BoundKnots");
            MEM.Alloc(counter, ref mesh.BoundKnotsMark, "mesh.BoundKnotsMark");
            MEM.Alloc(counter, ref mesh.CoordsX, "mesh.CoordsX");
            MEM.Alloc(counter, ref mesh.CoordsY, "mesh.CoordsY");

            uint[,] map = new uint[Nx, Ny];

            uint k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                mesh.CoordsX[k] = xx[i];
                mesh.CoordsY[k] = zeta[i]; 
                map[i, 0] = k++;
                mesh.CoordsX[k] = xx[i];
                mesh.CoordsY[k] = wl[i];// - zeta[i];
                map[i, 1] = k++;
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
            return mesh;
        }
        /// <summary>
        /// Конвертирует 1 мерный массив в псевдодвумерный
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void Convert2DFrom1D(ref double[] result, double[] values)
        {
            MEM.Alloc(2*values.Length, ref result);
            for(int i=0; i< values.Length; i++)
            {
                result[2 * i] = values[i];
                result[2 * i + 1] = values[i];
            }
        }
    }
}
