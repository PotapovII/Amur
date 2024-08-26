//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 04.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD.River2DSW
{
    using System;
    using GeometryLib;
    using MemLogLib;
    using MeshLib;
    /// <summary>
    /// Решение плановой задачи мелкой воды методом конечных элементов
    /// </summary>
    [Serializable]
    public class GaussPoints
    {
        public double[][] fEtaXi;
        public GaussPoints(int CountElementKnots = 3)
        {
            MEM.Alloc2D(CountElementKnots, CountElementKnots, ref fEtaXi);
            for (int i = 0; i < CountElementKnots; i++)
            {
                for (int j = 0; j < CountElementKnots; j++)
                {
                    if (i == j)
                        fEtaXi[i][j] = 2.0 / 3.0;
                    else
                        fEtaXi[i][j] = 1.0 / 6.0;
                }
            }
        }
    }
}
