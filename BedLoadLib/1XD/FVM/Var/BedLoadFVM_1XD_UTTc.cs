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
//                  Вариант условия начала движения торогания частиц
//                              Потапов И.И.
//                               12.04.24
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
    using System;
    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете донных деформаций русла вдоль потока
    /// </summary>
    [Serializable]
    public class BedLoadFVM_1XD_UTTc : BedLoadFVM_1XD
    {
        public override IBedLoadTask Clone()
        {
            return new BedLoadFVM_1XD_UTTc(new BedLoadParams1D());
        }
        public BedLoadFVM_1XD_UTTc() : this(new BedLoadParams1D()) { }
        public BedLoadFVM_1XD_UTTc(BedLoadParams1D p) : base(p)
        {

            name = "1DX (d50) деформация дна U(T-Tc)";
        }
        /// <summary>
        /// Расчет коэффициентов  на грани  P--e--E
        /// </summary>
        protected override void BuilderABC(double COEF_B = 1)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            double epsilon = SPhysics.PHYS.epsilon;
            double tau0 = SPhysics.PHYS.tau0;
            double kappa = SPhysics.PHYS.kappa;
            double rho_w = SPhysics.rho_w;
            double Fa0 = SPhysics.PHYS.Fa0;
            double Ws = SPhysics.PHYS.Ws;
       
            double chi2 = 0;
            // цикл по интервалам
            for (int i = 0; i < N; i++)
            {
                // tau0Elem[i] = tau0;
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
                    chi2 = Math.Min(1 + MEM.Error10, tau0 / mtau);
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
                                           // Добавка Бэгнольда для взвешенных наносов
                        double kappa0 = 0.05;
                        double kappa1 = kappa0 + (kappa - kappa0) * (Math.Exp(-mtau / tau0));
                        // критические напряжения на ровном дне
                        double G1 = 4.0 / (3.0 * kappa1 * Math.Sqrt(rho_w) * Fa0 * (1 - epsilon));
                        double GG = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                        double dzf = dZeta[i] / tanphi;
                        double scale = DRate(dzf, 0);
                        GG *= scale;
                        double Ra = Ws / (kappa * us + MEM.Error12);
                        us = Math.Sqrt(mtau / rho_w);
                        A[i] = Math.Max(0, 1 - chi2);
                        A[i] += 0;
                        A[i] *= GG;
                        B[i] = Math.Abs(GG) / tanphi;
                        C[i] = 0;
                        G0[i] = 1;

                    }
                }
            }

        }

        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            base.AddMeshPolesForGraphics(sp);

        }
    }
}
