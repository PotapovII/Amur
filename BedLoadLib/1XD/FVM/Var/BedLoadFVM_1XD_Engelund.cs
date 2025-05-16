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
    using System;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете донных деформаций русла вдоль потока
    /// </summary>
    [Serializable]
    public class BedLoadFVM_1XD_Engelund : BedLoadFVM_1XD
    {
        public override IBedLoadTask Clone()
        {
            return new BedLoadFVM_1XD_Engelund(new BedLoadParams1D());
        }
        public BedLoadFVM_1XD_Engelund() : this(new BedLoadParams1D()) { }
        public BedLoadFVM_1XD_Engelund(BedLoadParams1D p) : base(p)
        {
            name = "1DX (d50) деформация дна T(U-Uc) Engelund";
        }
        /// <summary>
        /// Расчет коэффициентов  на грани  P--e--E
        /// </summary>
        protected override void BuilderABC(double COEF_B = 1.5)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            double rho_b = SPhysics.PHYS.rho_b;
            double tau0 = SPhysics.PHYS.tau0;
            double rho_w = SPhysics.rho_w;
            
            double d50 = SPhysics.PHYS.d50;
            
            double db = d50 * Math.Pow(rho_b * GRAV / (nu * nu), 1.0 / 3);
            double theta0 = 0.3 / (1 + 1.2 * db) + 0.055 * (1 - Math.Exp(-0.02 * db));
            double Tcr0 = theta0 * (rho_s - rho_w) * GRAV * d50;
            double COEF_A = 0.7;
            COEF_B = 2;
            double COEF_Q =  18.74;
            double G01 = COEF_Q / (Math.Sqrt(rho_w) * (rho_s - rho_w) * GRAV);
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
                    double alphT = Math.Max(0, (1 + dZeta[i] / tanphi));
                    double Tcr = Math.Max(0, Tcr0 * alphT);
                    double chi = Math.Min(1 + MEM.Error10, Tcr / mtau);
                    //CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                    if (chi >= 1)
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
                        double q0 = G01 * tau[i] * Math.Sqrt(mtau);
                        q0 *= Math.Max(0, (1 - chi)) * (1 - COEF_A * Math.Sqrt(chi));
                        A[i] = q0;
                        //if (blm == TypeBLModel.BLModel_2021 || blm == TypeBLModel.BLModel_1991)
                        B[i] = COEF_B * q0;
                        C[i] = 0;
                        G0[i] = 1;
                    }
                }
            }

        }
      }
}
