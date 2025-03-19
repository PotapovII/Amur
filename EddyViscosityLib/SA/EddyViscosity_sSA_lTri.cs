
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using CommonLib.Physics;
    using CommonLib.EddyViscosity;

    using System;
    using MemLogLib;
    using FEMTasksLib.FEMTasks.VortexStream;
    using CommonLib;

    /// <summary>
    /// Стационарная модель  Спаларта-Аллмареса 
    /// </summary>
    [Serializable]
    public class EddyViscosity_sSA_lTri : AEddyViscosity_SA_Tri
    {
        
        public EddyViscosity_sSA_lTri(ETurbViscType eTurbViscType, BEddyViscosityParam p, TypeTask tt)
            : base(eTurbViscType, tt, p.NLine)
        {
            cs = 1;
        }
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.Ux = Ux;
            this.Vy = Vy;
            this.Vz = Vz;
            this.Phi = Phi;
            this.Vortex = Vortex;
            SolveTask(ref eddyViscosity);
        }
        /// <summary>
        /// Определение вязкости  К-Е модели на текущем шаге по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        public override void SolveTask(ref double[] eddyViscosity)
        {
            uint ee = 0;
            try
            {
                //double[] Vx = null;
                //double[] Vy = null;
                //// расчет поля скорости
                //Console.WriteLine("Vy ~ Vz:");
                //VSUtils.CalcVelosity(Phi, ref Vx, ref Vy, wMesh, 0, 0);
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    MEM.MemCopy(ref Mut_old, Mut);
                    if (nIdx == 0)
                    {
                        // расчет узловых функций
                        Calk_Fv();
                    }
                    // расчет приведенной вихревой вязкости
                    Console.WriteLine("Mut : шаг по нелинейности: " + nIdx.ToString());
                    algebra.Clear();
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(cu, ref LaplMatrix);
                        MEM.AllocClear(cu, ref LocalRight);
                        //double[] b = dNdx[elem];
                        //double[] c = dNdy[elem];
                        //uint i0 = eKnots[elem].Vertex1;
                        //uint i1 = eKnots[elem].Vertex2;
                        //uint i2 = eKnots[elem].Vertex3;
                        //// получить узлы КЭ
                        //uint[] knots = { i0, i1, i2 };
                        //double Se = S[elem];

                        //// Вычисление ЛПЧ от объемных сил    
                        //double eVx =    c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        //double eVy = - (b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        //double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        //double Hk = Math.Sqrt(Se) / 1.4;
                        //// сеточное число Пекле
                        //double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        //double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
                        //double Ax = 0;
                        //double Ay = 0;
                        //if (MEM.Equals(mV, 0) != true)
                        //{
                        //    Ax = eVx / mV;
                        //    Ay = eVy / mV;
                        //}
                        //// расчет средних по элементу функций SA
                        //// вихрь на КЭ
                        //double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;

                        //double dUdy = b[0] * Ux[i0] + b[1] * Ux[i1] + b[2] * Ux[i2];
                        //double dUdz = c[0] * Ux[i0] + c[1] * Ux[i1] + c[2] * Ux[i2];
                        //double eMut = (Mut[i0] + Mut[i1] + Mut[i2]) / 3.0;
                        //double dMutdy = b[0] * Mut[i0] + b[1] * Mut[i1] + b[2] * Mut[i2];
                        //double dMutdz = c[0] * Mut[i0] + c[1] * Mut[i1] + c[2] * Mut[i2];

                        //double dp = (distance[i0] + distance[i1] + distance[i2]) / 3.0;

                        //double fv2e = (fv2[i0] + fv2[i1] + fv2[i2]) / 3.0;
                        //double ft2e = (ft2[i0] + ft2[i1] + ft2[i2]) / 3.0;

                        //double eMut_t = eMut / Sigma + SPhysics.mu;

                        //double eOmega_ii = Math.Sqrt(2 * (dUdy * dUdy + dUdz * dUdz + eVortex * eVortex));
                        ////                        
                        //double kd = SPhysics.kappa_w * dp;
                        //double kd2 = kd * kd;
                        //double dp2 = dp * dp;



                        //double Eii;
                        //// Ограничитель 1 типа на Eii
                        //Eii = Math.Min(0.3 * eOmega_ii, eOmega_ii + fv2e * eMut / (rho_w * kd2));

                        //// Ограничитель 2 типа на Eii
                        ////double c2 = 0.7;
                        ////double c3 = 0.9;
                        ////double eOmega_mu = fv2e * eMut / (rho_w * kd2);

                        ////if(eOmega_mu >  - c2*eOmega_ii)
                        ////    Eii = eOmega_ii + fv2e * eMut / (rho_w * kd2);
                        ////else
                        ////    Eii = eOmega_ii + eOmega_ii*(c2*c2* eOmega_ii + c3 * eOmega_mu)/
                        ////          (eOmega_ii * (c3 - 2 *c2)  - eOmega_mu);

                        ////double Eii = eOmega_ii + fv2e * eMut / ( rho_w * kd2 );
                        //double re = Math.Min(eMut /( rho_w * Eii * kd2), 10);
                        //double ge = re + C_w2 * (Math.Pow(re, 6) - re);
                        //double ge6 = Math.Pow(ge, 6);
                        //double fwe = ge * Math.Pow((1 + C_w3_6) / (ge6 + C_w3_6), b6);

                        //// генерация ЛЧ
                        //double eP = rho_w * C_b1 * (1 - ft2e) * eOmega_ii;
                        //// диссипация ЛЧ
                        ////double eD = -(((1 - ft2e) * fv2e + ft2e) * C_b1 * back_kap2 - C_v1 * fwe) / back_d2;
                        //double eD = C_w1 * fwe / dp2 - C_b1 * ft2e / kd2;
                        //double qMut = eD * eMut - eP;

                        double[] b = dNdx[elem];
                        double[] c = dNdy[elem];
                        uint i0 = eKnots[elem].Vertex1;
                        uint i1 = eKnots[elem].Vertex2;
                        uint i2 = eKnots[elem].Vertex3;
                        // получить узлы КЭ
                        uint[] knots = { i0, i1, i2 };
                        double Se = S[elem];

                        // Вычисление ЛПЧ от объемных сил    
                        double eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        double eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);

                        double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                        double Hk = Math.Sqrt(Se) / 1.4;
                        // сеточное число Пекле
                        double eMu = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3.0;
                        double Pe = SPhysics.rho_w * mV * Hk / (2 * eMu);
                        double Ax = 0;
                        double Ay = 0;
                        if (MEM.Equals(mV, 0) != true)
                        {
                            Ax = eVx / mV;
                            Ay = eVy / mV;
                        }
                        double dp = (distance[i0] + distance[i1] + distance[i2]) / 3.0;
                        double kd = SPhysics.kappa_w * dp;
                        double kd2 = kd * kd;
                        double dp2 = dp * dp;

                        // расчет средних по элементу функций SA
                        // вихрь на КЭ
                        double eVortex = (Vortex[i0] + Vortex[i1] + Vortex[i2]) / 3.0;
                        double dUdy = b[0] * Ux[i0] + b[1] * Ux[i1] + b[2] * Ux[i2];
                        double dUdz = c[0] * Ux[i0] + c[1] * Ux[i1] + c[2] * Ux[i2];
                        double eOmega_ii = Math.Sqrt(2 * (dUdy * dUdy + dUdz * dUdz + eVortex * eVortex));

                        double eMut = (Mut[i0] + Mut[i1] + Mut[i2]) / 3.0;
                        double eMut_t = eMut / Sigma + SPhysics.mu;
                        double dMutdy = b[0] * Mut[i0] + b[1] * Mut[i1] + b[2] * Mut[i2];
                        double dMutdz = c[0] * Mut[i0] + c[1] * Mut[i1] + c[2] * Mut[i2];
                        double fv2e = (fv2[i0] + fv2[i1] + fv2[i2]) / 3.0;
                        double ft2e = (ft2[i0] + ft2[i1] + ft2[i2]) / 3.0;
                        // Расчет нелинейного источника турбулености на КЭ
                        double Q_mut = CalcQS_SA(eOmega_ii, eMut, fv2e, ft2e, kd2, dp2);
                        // m-1
                        double eMut_old = (Mut_cur[i0] + Mut_cur[i1] + Mut_cur[i2]) / 3.0;
                        double eMut_t_old = eMut_old / Sigma + SPhysics.mu;
                        double dMutdy_old = b[0] * Mut_cur[i0] + b[1] * Mut_cur[i1] + b[2] * Mut_cur[i2];
                        double dMutdz_old = c[0] * Mut_cur[i0] + c[1] * Mut_cur[i1] + c[2] * Mut_cur[i2];
                        double fv2e_old = (fv2_old[i0] + fv2_old[i1] + fv2_old[i2]) / 3.0;
                        double ft2e_old = (ft2_old[i0] + ft2_old[i1] + ft2_old[i2]) / 3.0;
                        // Расчет нелинейного источника турбулености на КЭ
                        double Q_mut_old = CalcQS_SA(eOmega_ii, eMut_old, fv2e_old, ft2e_old, kd2, dp2);


                        // вычисление ЛЖМ для задачи Навье - Стокса/Стокса
                        for (int ai = 0; ai < cu; ai++)
                        {
                            double L_SUPG = omega * (Hx[elem] * Ax * b[ai] + Hy[elem] * Ay * c[ai]);
                            double Lb = (1 / 3.0 + L_SUPG);
                            double La = rho_w * Lb;
                            for (int aj = 0; aj < cu; aj++)
                            {
                                // матрица масс
                                double M_ab = MM[ai][aj] * Se;
                                // матрица масс SUPG
                                //double m_ab = M_ab + L_SUPG * Se / 3.0;
                                //double _M_ab = rho_w * m_ab;

                                // матрица масс SUPG
                                double _M_ab_ = (M_ab + L_SUPG * Se / 3.0);
                                double _M_ab = rho_w * _M_ab_;
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_ab = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                    (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                    (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[ai][aj] = C_ab + eMut_t * K_ab + _M_ab_ * Q_mut;
                            }
                            LocalRight[ai] = Lb * C_b2 / Sigma * (dMutdy * dMutdy + dMutdz * dMutdz) * Se;
                        }



                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound, cs);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    // Выполнение граничных условий для функции Mut
                    // WallData 
                    double[] bcMu = null;
                    double[] tauWalls = null;
                    wallData.GetBoundaryWall_Mu_SA(Phi, ref bcMu, ref tauWalls);
                    algebra.BoundConditions(bcMu, bcIndex);
                    //algebra.BoundConditions(0, bcIndex);
                    // решение
                    algebra.Solve(ref Mut);
                    //MEM.Relax(ref Mut, Mut_old, 0.5);
                    // расчет узловых функций
                    Calk_Fv();
                    Calk_Fv_eddyViscosity();
                    //************************************************************************************************************************
                    // Считаем невязку
                    //************************************************************************************************************************
                    double epsVortex = 0.0;
                    double normVortex = 0.0;

                    for (int i = 0; i < Mut.Length; i++)
                    {
                        normVortex += Mut[i] * Mut[i];
                        epsVortex += (Mut_old[i] - Mut[i]) * (Mut_old[i] - Mut[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / (normVortex + MEM.Error14));
                    Console.WriteLine("n {0} residual {1}", nIdx, residual);
                    if (residual < MEM.Error5)
                        break;
                    eddyViscosity = this.eddyViscosity;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }

    
}
}
