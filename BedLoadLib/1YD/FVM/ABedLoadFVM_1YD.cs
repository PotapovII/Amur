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
    using CommonLib.IO;

    [Serializable]
    public abstract class ABedLoadFVM_1YD : 
        ABedLoadFVM_1D<BedLoadParams1YD>, IBedLoadTask
    {
        /// <summary>
        /// текущий уровень свободной поверхности
        /// </summary>
        protected double waterLevel;
        /// <summary>
        /// текущий уровень свободной поверхности
        /// </summary>
        protected double maxDeapth = 0;
        /// <summary>
        /// Режимная лубина потока
        /// </summary>
        protected double Depth0;

        public ABedLoadFVM_1YD(BedLoadParams1YD p) 
            : base(p, TypeTask.streamY1D) 
        { 
        }
        /// <summary>
        /// Получить буффер обмена параметрами
        /// </summary>
        /// <param name="args"></param>
        public override void SetTaskArgs(ITaskArgs args)
        {
            TaskArgs a = args as TaskArgs;
            if( a != null)
            {
                mTanPhi = a.GetValues("mTanPhi");
                mTau_Adhesion = a.GetValues("mTau_Adhesion");
                mRoughness = a.GetValues("mRoughness");
                waterLevel = a.GetSValues("waterLevel");
                Depth0 = a.GetSValues("Depth0");
            }
        }
        /// <summary>
        /// Расчет производных и критических напряжений 
        /// для однородного донного материала
        /// </summary>
        protected void CalkCritTauType()
        {
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
                    tau0Elem[i] = CosGamma[i] * Math.Max(mTau0[i], mTau0[i] * (1 + dZeta[i] / mTanPhi[i]));
                }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    //tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * (1 + dZeta[i] / tanphi));
                    tau0Elem[i] = CosGamma[i] * mTau0[i];
                }
            }
        }

    }
}
