//---------------------------------------------------------------------------
//                     ПРОЕКТ  "RiverSolver"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 04.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.River2D.RiverMesh
{
    using MemLogLib;
    using System;

    [Serializable]
    public class GaussPoints
    {
        public double[][] fEtaXi;  
        public GaussPoints(int CountEKnots = 3)
        {
            MEM.Alloc2D(CountEKnots, CountEKnots, ref fEtaXi);
            for (int i = 0; i < CountEKnots; i++)
            {
                for (int j = 0; j < CountEKnots; j++)
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
