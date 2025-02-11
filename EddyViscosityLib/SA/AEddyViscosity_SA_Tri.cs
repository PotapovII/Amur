
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

    using System;
    using MemLogLib;
    using MeshLib.Wrappers;
    using FEMTasksLib.AllWallFunctions;
    using System.Runtime.CompilerServices;
    using CommonLib.EddyViscosity;

    /// <summary>
    /// Базовый класс для моделей Спаларта-Аллмареса 
    /// </summary>
    [Serializable]
    public abstract class AEddyViscosity_SA_Tri : ADiffEddyViscosityTri
    {
        /// <summary>
        /// Обертка для функций стенки учитывающая tri сетку
        /// </summary>
        protected WallData wallData = null;
        /// <summary>
        /// Матрица масс
        /// </summary>
        protected double[,,] MMM = null;
        /// <summary>
        /// вихревую вязкость приведенная
        /// </summary>
        public double[] Mut;
        protected double[] Mut_old;
        protected double[] Mut_cur;
        /// <summary>
        /// растояние до стенки
        /// </summary>
        public double[] distance;
        /// <summary>
        /// массивы для контроля функций
        /// </summary>
        public double[] xi;
        public double[] fv1;
        public double[] fv2;
        public double[] ft2;
        public double[] mQ_mut;
        public double[] eQ_mut;

        protected double[] xi_old;
        protected double[] fv1_old;
        protected double[] fv2_old;
        protected double[] ft2_old;
        

        #region Константы модели SA
        protected const double Sigma = 2.0 / 3.0;

        protected const double C_b1 = 0.1355;
        protected const double C_b2 = 0.622;

        protected const double C_v1 = 7.1;
        protected double C_v1_3 = C_v1 * C_v1 * C_v1;
        protected const double C_t1 = 1;
        protected const double C_t2 = 2;
        protected const double C_t3 = 1.2;
        protected const double C_t4 = 0.5;
        protected double back_kap2 = 1.0 / (SPhysics.kappa_w * SPhysics.kappa_w);
        protected double C_w1 = C_b1 / (SPhysics.kappa_w * SPhysics.kappa_w) + (1 + C_b2) / Sigma;
        protected const double C_w2 = 0.3;
        protected const double C_w3 = 2;
        protected const double C_w3_6 = 64; // C_w3^6
        protected double back_rho_kap2 = 1 / (SPhysics.rho_w * SPhysics.kappa_w * SPhysics.kappa_w);
        protected double b6 = 1 / 6.0;
        #endregion

        /// <summary>
        /// выполняет вычисления растояние от узла до ближайшей стенки канала
        /// </summary>
        protected MWRiverDistance mWRiverDistance = null;
        public AEddyViscosity_SA_Tri(ETurbViscType eTurbViscType, int nLine)
            : base(eTurbViscType, nLine) { }
        public override void SetTask(IMesh mesh, IAlgebra algebra, IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref xi);
            MEM.Alloc(mesh.CountKnots, ref fv1);
            MEM.Alloc(mesh.CountKnots, ref fv2);
            MEM.Alloc(mesh.CountKnots, ref ft2);
            MEM.Alloc(mesh.CountKnots, ref mQ_mut);
            MEM.Alloc(mesh.CountElements, ref eQ_mut);

            MEM.Alloc(mesh.CountKnots, ref xi_old);
            MEM.Alloc(mesh.CountKnots, ref fv1_old);
            MEM.Alloc(mesh.CountKnots, ref fv2_old);
            MEM.Alloc(mesh.CountKnots, ref ft2_old);

            MEM.Alloc(mesh.CountKnots, ref Mut);
            MEM.Alloc(mesh.CountKnots, ref Mut_cur);
            MEM.Alloc(mesh.CountKnots, ref Mut_old);
            MEM.Alloc(mesh.CountKnots, ref MRight);
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            for (int i = 0; i < Index.Length; i++)
                bcIndex[i] = (uint)(Index[i]);

            mWRiverDistance = new MWRiverDistance(mesh, Params.channelForms);
            distance = mWRiverDistance.GetDistance();
            // Матрица масс трерьего ранга
            MMM = new double[cu, cu, cu];
            for (int i = 0; i < cu; i++)
                for (int j = 0; j < cu; j++)
                    for (int k = 0; k < cu; k++)
                        MMM[i, j, k] = 2;
            for (int i = 0; i < cu; i++)
                MMM[i, i, i] = 4;
            MMM[0, 1, 2] = 1;
            MMM[0, 2, 1] = 1;
            MMM[1, 0, 2] = 1;
            MMM[1, 2, 0] = 1;
            MMM[2, 0, 1] = 1;
            MMM[2, 1, 0] = 1;
            for (int i = 0; i < cu; i++)
                for (int j = 0; j < cu; j++)
                    for (int k = 0; k < cu; k++)
                        MMM[i, j, k] = MMM[i, j, k] / 60.0;
            for (int i = 0; i < Mut.Length; i++)
            {
                Mut[i] = 1000 * SPhysics.mu;
                Mut_cur[i] = Mut[i];
                Mut_old[i] = Mut[i];
            }
            wallData = new WallData(mesh, new WallFunctionVolkov());
        }

        /// <summary>
        /// расчет узловых функций
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Calk_Fv()
        {
            for (int i = 0; i < Mut.Length; i++)
            {
                xi[i] = Mut[i] / mu;
                double xi3 = xi[i] * xi[i] * xi[i];
                fv1[i] = xi3 / (xi3 + C_v1_3);
                fv2[i] = 1 - xi[i] / (1 + xi[i] * fv1[i]);
                ft2[i] = C_t3 * Math.Exp(- C_t4 * xi[i] * xi[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Calk_Fv_eddyViscosity()
        {
            for (int i = 0; i < Mut.Length; i++)
                eddyViscosity[i] = fv1[i] * Mut[i];
        }

        bool flagQ = false;
        /// <summary>
        /// Расчет нелинейного источника турбулености на КЭ
        /// </summary>
        /// <param name="eOmega_ii"></param>
        /// <param name="eMut"></param>
        /// <param name="fv2e"></param>
        /// <param name="ft2e"></param>
        /// <param name="kd2"></param>
        /// <param name="dp2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected double CalcQS_SA(double eOmega_ii, double eMut, double fv2e, double ft2e, double kd2, double dp2)
        {
            double Eii;
            if (flagQ == true)
            {
                // Ограничитель 1 типа на Eii
                Eii = Math.Min(0.3 * eOmega_ii, eOmega_ii + fv2e * eMut / (rho_w * kd2));
            }
            else
            {
                // Ограничитель 2 типа на Eii

                double c2 = 0.7;
                double c3 = 0.9;
                double eOmega_mu = fv2e * eMut / (rho_w * kd2);

                if (eOmega_mu > -c2 * eOmega_ii)
                    Eii = eOmega_ii + fv2e * eMut / (rho_w * kd2);
                else
                    Eii = eOmega_ii + eOmega_ii * (c2 * c2 * eOmega_ii + c3 * eOmega_mu) /
                          (eOmega_ii * (c3 - 2 * c2) - eOmega_mu);
            }
            double re = Math.Min(eMut / (rho_w * Eii * kd2), 10);
            double ge = re + C_w2 * (Math.Pow(re, 6) - re);
            double ge6 = Math.Pow(ge, 6);
            double fwe = ge * Math.Pow((1 + C_w3_6) / (ge6 + C_w3_6), b6);
            // генерация ЛЧ
            double eP = rho_w * C_b1 * (1 - ft2e) * eOmega_ii;
            // диссипация ЛЧ
            //double eD = -(((1 - ft2e) * fv2e + ft2e) * C_b1 * back_kap2 - C_v1 * fwe) / back_d2;
            double eD = C_w1 * fwe / dp2 - C_b1 * ft2e / kd2;
            double Q_mut = eD * eMut - eP;
            return Q_mut;
        }

    }
}
