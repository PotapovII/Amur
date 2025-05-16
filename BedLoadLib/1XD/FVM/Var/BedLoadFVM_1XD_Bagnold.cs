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
    using System;
    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете донных деформаций русла вдоль потока
    /// </summary>
    [Serializable]
    public class BedLoadFVM_1XD_Bagnold : BedLoadFVM_1XD
    {
        public override IBedLoadTask Clone()
        {
            return new BedLoadFVM_1XD_Bagnold(new BedLoadParams1D());
        }

        public BedLoadFVM_1XD_Bagnold() : this(new BedLoadParams1D()) {}

        public BedLoadFVM_1XD_Bagnold(BedLoadParams1D p) : base(p)
        {
            name = "деформация дна FVM (d50) T(U-Uc) + добавка Бэгнольда";
        }
        /// <summary>
        /// Расчет коэффициентов  на грани  P--e--E
        /// </summary>
        protected override void BuilderABC(double COEF_B = 1)
        {
            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double RaC = SPhysics.PHYS.RaC;
            double Ws = SPhysics.PHYS.Ws;

            // цикл по интервалам
            for (int i = 0; i < N; i++)
            {
                //tau0Elem[i] = tau0;
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
                    chi = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0 / mtau));
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
                        // Добавка Бэгнольда для взвешенных наносов
                        us = Math.Sqrt(mtau / rho_w);
                        double Ra = Ws / (kappa_w * us + MEM.Error12);
                        us = Math.Sqrt(mtau / rho_w);
                        // поправка Бэгнольда на взвешенные наносы
                        A[i] = Math.Max(0, 1 - chi);
                        if (Params.blm == TypeBLModel.BLModel_2021 || 
                            Params.blm == TypeBLModel.BLModel_1991)
                            B[i] = (chi / 2 + A[i]) / tanphi;
                        else
                            B[i] = (chi / 2 + A[i] * (1 + s) / s) / tanphi; // TypeBLModel.BLModel_2014
                        B[i] *= COEF_B;
                        C[i] = A[i] / (s * tanphi);

                        G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                    }
                }
            }
        }
    }
}
