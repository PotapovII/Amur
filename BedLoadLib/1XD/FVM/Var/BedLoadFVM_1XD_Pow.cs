//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                               27.12.19
//---------------------------------------------------------------------------
//  Учет ситуаций сухой-мокрый, корректная обработка сулучаев tau == 0
//  переход на интерфейс через абстрактный класс ABedLoadTask1D
//                              Потапов И.И.
//                               14.04.21
//---------------------------------------------------------------------------
//               Добавление модели TypeBLModel.BLModel_2021 
//          разделение гидростатики и напорной части давления
//                              Потапов И.И.
//                               01.10.21
//---------------------------------------------------------------------------
//                      рефакторинг : Потапов И.И.
//                              27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using CommonLib;
    using CommonLib.BedLoad;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;
    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете донных деформаций русла вдоль потока
    /// </summary>
    [Serializable]
    public class BedLoadFVM_1XD_Pow : BedLoadFVM_1XD
    {
        public override IBedLoadTask Clone()
        {
            return new BedLoadFVM_1XD_Pow(new BedLoadParams1D());
        }
        public BedLoadFVM_1XD_Pow() : this(new BedLoadParams1D()) { }
        public BedLoadFVM_1XD_Pow(BedLoadParams1D p) : base(p)
        {
            name = "1DX (d50) деформация дна T^(1/n)(T-Tc)";
        }
        /// <summary>
        /// Расчет коэффициентов  на грани  P--e--E
        /// </summary>
        protected override void BuilderABC(double COEF_B=1)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double kappa = SPhysics.PHYS.kappa;
            
            double NR = 1.5;
            double NR2 = (2 - NR) / (2.0 * NR);
            double NK = 3.0 / 4.0 * NR * NR / (NR + 1.0) * Math.Pow(kappa* kappa*rho_w, NR2);
            // цикл по интервалам
            for (int i = 0; i < N; i++)
            {
                tau0Elem[i] = tau0;
                mtau = Math.Abs(tau[i]);
                // Контроль сухой (не размываемой) части берега
                if (mtau < MEM.Error8)
                {
                    DryWetEelm[i] = 0; // сухая (не размываемая) часть дна
                    A[i] = 0;
                    B[i] = 0;
                    C[i] = 0;
                    G0[i] = 0;
                }
                else
                {
                    //dx = x[i + 1] - x[i];
                    //dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                    // критическое напряжение
                    // tau0Elem[i] = Math.Max(0, tau0 * (1 + dz / tanphi));
                    // tau0Elem[i] = tau0;
                    // влияние уклона на критическое напряжение учтено при выводе уравнений модели
                    //chi = Math.Min(1.0000000000001, Math.Sqrt(tau0Elem[i] / mtau));
                    double chi2 = Math.Min(1 + MEM.Error10, tau0Elem[i] / mtau);
                    //double chi2 = Math.Min(1 + MEM.Error10, tau0 / mtau);
                    //CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                    if (chi2 >= 1)
                    {
                        DryWetEelm[i] = 0; // мокрая (не размываемая) часть дна
                        A[i] = 0;
                        B[i] = 0;
                        C[i] = 0;
                        G0[i] = 0;
                    }
                    else
                    {
                        DryWetEelm[i] = 1; // размываемая  часть дна
                        A[i] = Math.Max(0, (1 - chi2));
                        //if (blm == TypeBLModel.BLModel_2021 || blm == TypeBLModel.BLModel_1991)
                        B[i] = 1.0 / tanphi;
                        C[i] = 0;
                        G0[i] = G1 * tau[i] * Math.Pow(mtau, NR2) * NK / CosGamma[i];
                    }
                }
            }

        }
      }
}
