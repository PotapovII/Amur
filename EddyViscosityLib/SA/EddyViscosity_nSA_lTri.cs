
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.EddyViscosity;

    using System;
    using MemLogLib;
    
    /// <summary>
    /// Нестационарная модель  Спаларта-Аллмареса 
    /// </summary>
    [Serializable]
    public class EddyViscosity_nSA_lTri : AEddyViscosity_SA_Tri
    {
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        protected double theta = 0.5;
        /// <summary>
        /// Шаг по времени
        /// </summary>
        protected double dt;
        /// <summary>
        /// Для правой части
        /// </summary>
        [NonSerialized]
        protected IAlgebra Ralgebra;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        private double[][] RMatrix = null;
        public EddyViscosity_nSA_lTri(ETurbViscType eTurbViscType, BEddyViscosityParam p)
            : base(eTurbViscType, p.NLine)
        {
            cs = 1;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref MRight);
            Ralgebra = algebra.Clone();
        }
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] eddyViscosity, double[] Ux, double[] Vy, double[] Vz, double[] Phi, double[] Vortex, double dt)
        {
            this.dt = dt;
            this.Ux = Ux;
            this.Vy = Vy;
            this.Vz = Vz;
            this.Phi = Phi;
            this.Vortex = Vortex;
            SolveTask(ref eddyViscosity);
        }
        /// <summary>
        /// Определение вязкости по Спаларта-Аллмареса модели на текущем шаге по времени
        /// </summary>
        /// <param name="eddyViscosity"></param>
        public override void SolveTask(ref double[] eddyViscosity)
        {
            uint ee = 0;
            try
            {
                MEM.MemCopy(ref Mut_old, Mut);
                MEM.MemCopy(ref Mut_cur, Mut);
                // итераций по нелинейности на текущем шаге по времени
                for (int nIdx = 0; nIdx < NLine; nIdx++)
                {
                    MEM.Relax(ref Mut_cur, Mut);
                    if (nIdx == 0)
                    {
                        // расчет узловых функций
                        for (int i = 0; i < Mut.Length; i++)
                        {
                            Calk_Fv();
                            MEM.MemCopy(ref xi_old, xi);
                            MEM.MemCopy(ref fv1_old, fv1);
                            MEM.MemCopy(ref fv2_old, fv2);
                        }
                    }
                    // расчет приведенной вихревой вязкости
                    Console.WriteLine("Mut : шаг по нелинейности: " + nIdx.ToString());
                    algebra.Clear();
                    Ralgebra.Clear();
                    for (uint elem = 0; elem < mesh.CountElements; elem++)
                    {
                        ee = elem;
                        // локальная матрица часть СЛАУ
                        MEM.Alloc2DClear(cu, ref LaplMatrix);
                        MEM.Alloc2DClear(cu, ref RMatrix);
                        MEM.AllocClear(cu, ref LocalRight);

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
                        eQ_mut[elem] = Q_mut;

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
                                double _M_ab_ = (M_ab + L_SUPG * Se / 3.0);
                                double _M_ab = rho_w * _M_ab_;
                                // матрица вязкости члены плоские
                                double K_ab = (b[ai] * b[aj] + c[ai] * c[aj]) * Se;
                                // матрица конверкции 
                                double C_ab = La * ((c[0] * b[aj] - b[0] * c[aj]) * Phi[i0] +
                                                    (c[1] * b[aj] - b[1] * c[aj]) * Phi[i1] +
                                                    (c[2] * b[aj] - b[2] * c[aj]) * Phi[i2]) * Se;

                                LaplMatrix[ai][aj] = _M_ab + dt * (1 - theta) * (C_ab + eMut_t * K_ab + _M_ab_ * Q_mut);
                                RMatrix[ai][aj] = _M_ab - dt * theta * (C_ab + eMut_t_old * K_ab + _M_ab_ * Q_mut_old);
                            }
                            LocalRight[ai] = dt * (1 - theta) * Lb * C_b2 / Sigma * (dMutdy * dMutdy + dMutdz * dMutdz) * Se
                                           + dt * theta * Lb * C_b2 / Sigma * (dMutdy_old * dMutdy_old + dMutdz_old * dMutdz_old) * Se;
                        }
                        // получем значения адресов неизвестных
                        GetAdress(knots, ref adressBound, cs);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToMatrix(LaplMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        Ralgebra.AddToMatrix(RMatrix, adressBound);
                        // добавление вновь сформированной ЛЖМ в ГМЖ
                        algebra.AddToRight(LocalRight, adressBound);
                    }
                    wMesh.ConvertField(ref mQ_mut, eQ_mut);

                    Ralgebra.getResidual(ref MRight, Mut_old, 0);
                    algebra.CopyRight(MRight);
                    // Выполнение граничных условий для функции вихря
                    // VortexBC();
                    // Выполнение граничных условий для функции тока
                    double[] bcMu = null;
                    double[] tauWalls = null;
                    wallData.GetBoundaryWall_Mu_SA(Phi, ref bcMu, ref tauWalls);
                    algebra.BoundConditions(bcMu, bcIndex);
                    //algebra.BoundConditions(0, bcIndex);
                    //algebra.Print();
                    // решение
                    algebra.Solve(ref Mut);
                    // MEM.Relax(ref Mut, Mut_old, 0.5);
                    // расчет узловых функций
                    MEM.MemCopy(ref xi_old, xi);
                    MEM.MemCopy(ref fv1_old, fv1);
                    MEM.MemCopy(ref fv2_old, fv2);
                    Calk_Fv();
                    Calk_Fv_eddyViscosity();
                    //************************************************************************************************************************
                    // Считаем невязку
                    //************************************************************************************************************************
                    double eps = 0.0;
                    double norm = 0.0;

                    for (int i = 0; i < Mut.Length; i++)
                    {
                        norm += Mut[i] * Mut[i];
                        eps += (Mut_cur[i] - Mut[i]) * (Mut_cur[i] - Mut[i]);
                    }
                    double residual = Math.Sqrt(eps / (norm + MEM.Error14));
                    Console.WriteLine("n {0} residual {1} normMut {2} ", nIdx, residual, norm / Mut.Length);
                    //if (norm / Mut.Length > 4)
                    //{
                    //    Console.BackgroundColor = ConsoleColor.Red;
                    //    Console.WriteLine(" Ups Press any key ");
                    //    Console.Read();
                    //    Console.BackgroundColor = ConsoleColor.White;
                    //    break;
                    //}
                    if (residual < MEM.Error5)
                        break;
                }
                //MEM.MemCopy(ref Mut_old, Mut);
                for (int i = 0; i < Mut.Length; i++)
                {
                    eddyViscosity[i] = fv1[i] * Mut[i];
                    if (eddyViscosity[i] < 0)
                    {
                        eddyViscosity[i] = 0.0001 * mu;
                        Mut[i] = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ee = " + ee.ToString() + " " + ex.Message);
            }
        }

    }
}
