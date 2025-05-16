//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          27.02.21
//---------------------------------------------------------------------------
//                  добавлен контроль потери массы
//                               27.03.22
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.BedLoad;
    using CommonLib.Physics;
    using MemLogLib;
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Класс для решения плановой задачи о 
    /// расчете донных деформаций русла на симплекс сетке
    /// больших уклонов донной поверхности
    /// </summary>
    [Serializable]
    public class CBedLoadFEMTask2DTri_GMAXN : CBedLoadFEMTaskTri_2D
    {
        /// <summary>
        /// Аппроксимация интегральных ядрер в функции расхода
        /// </summary>
        BLCore2D core = new BLCore2D();
        public override IBedLoadTask Clone()
        {
            return new CBedLoadFEMTask2DTri_GMAXN(new BedLoadParams2D());
        }
        /// <summary>
        /// Конструктор 
        /// </summary>
        public CBedLoadFEMTask2DTri_GMAXN(BedLoadParams2D p) : base(p)
        {
            double s = SPhysics.PHYS.s;
            cu = 3;
            a = new double[cu];
            b = new double[cu];
            ss = (1 + s) / s;
            name = "плановая деформация одно-фракционного дна с б.а. уклонами.";
        }
        /// <summary>
        /// Расчет коэффициентов A B C
        /// </summary>
        public override void CalkCoeff(uint elem, double mtauS, double dZx, double dZy)
        {
            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double rho_b = SPhysics.PHYS.rho_b;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double g = SPhysics.GRAV;

            // здесь нужен переключатель
            if (tau0 > mtau)
            {
                chi = 1;
                A[elem] = 0;
                B[elem] = 0;
                C[elem] = 0;
                D[elem] = 0;
            }
            else
            {
                // 27.12.2021
                G0 = G1 * mtauS * Math.Sqrt(mtau) / CosGamma[elem];
                chi = Math.Sqrt(tau0 / mtau);
                // 2D вариант асимптотик
                //double epsZ = 0.05;
                double scaleX = 1.0;// / Math.Max(epsZ, 1 - dZx * dZx);// - 37.0 / 105 * (9.0 / 74 + dZx) * dZy * dZy / (Math.Max(epsZ, 1 - dZx));
                double scaleY = 1;// 1 - 8.0 / 7 * dZx + 25.0 / 21 * dZx * dZx + 17.0 / 42 * dZy * dZy;

                if (Params.blm != TypeBLModel.BLModel_1991)
                {
                    // определение градиента давления по х,у
                    mesh.ElemValues(P, elem, ref press);
                    dPX = 0;
                    dPY = 0;
                    for (int ai = 0; ai < cu; ai++)
                    {
                        dPX += a[ai] * press[ai] / (rho_b * g);
                        dPY += b[ai] * press[ai] / (rho_b * g);
                    }
                    if (Params.blm != TypeBLModel.BLModel_2010)
                    {
                        A[elem] = scaleX * Math.Max(0, 1 - chi);
                        B[elem] = (scaleX * chi / 2 + A[elem]) / tanphi;
                        C[elem] = A[elem] / (s * tanphi);
                    }
                    else
                    {
                        A[elem] = scaleX * Math.Max(0, 1 - chi);
                        B[elem] = (scaleX * chi / 2 + A[elem] * (1 + s) / s) / tanphi;
                        C[elem] = A[elem] / (s * tanphi);
                    }
                }
                else
                {
                    A[elem] = scaleX * Math.Max(0, 1 - chi);
                    B[elem] = (scaleX * chi / 2 + A[elem]) / tanphi;
                    C[elem] = 0;
                    ss = 1;
                }
                D[elem] = scaleY * Math.Max(0, 1 - chi) * 4.0 / 5.0 / tanphi;
            }
        }
    }
}