//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using CommonLib;
    using CommonLib.BedLoad;
    using CommonLib.Physics;

    [Serializable]
    public abstract class ABedLoadFVM_1YD : 
        ABedLoadFVM_1D<BedLoadParams1D>, IBedLoadTask
    {
        public ABedLoadFVM_1YD(BedLoadParams1D p) 
            : base(p, TypeTask.streamY1D) 
        { 
        }
        /// <summary>
        /// Расчет производных и критических напряжений 
        /// для однородного донного материала
        /// </summary>
        protected void CalkCritTauType()
        {
            double tanphi = SPhysics.PHYS.tanphi;
            double tau0 = SPhysics.PHYS.tau0;

            for (int i = 0; i < N; i++)
            {
                dZeta[i] = (Zeta0[i + 1] - Zeta0[i]) / (x[i + 1] - x[i]);
                CosGamma[i] = Math.Sqrt(1 / (1 + dZeta[i] * dZeta[i]));
                // критическое напряжение
            }
            // критическое напряжение
            bool ft = false;
            if (ft == true)
                for (int i = 0; i < N; i++)
                {
                    //tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * (1 + dZeta[i] / tanphi));
                    tau0Elem[i] = CosGamma[i] * Math.Max(tau0, tau0 * (1 + dZeta[i] / tanphi));
                }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    //tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * (1 + dZeta[i] / tanphi));
                    tau0Elem[i] = CosGamma[i] * tau0;
                }
            }
        }
    }
}
