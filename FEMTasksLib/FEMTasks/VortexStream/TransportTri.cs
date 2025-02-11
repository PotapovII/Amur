#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.01.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.FEMTasks.VortexStream
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using AlgebraLib;

    /// <summary>
    /// ОО: Решение задачи переноса на симплекс сетке
    /// </summary>
    [Serializable]
    public class TransportTri : AWRAP_FETaskTri
    {

        /// <summary>
        /// Плотность воды
        /// </summary>
        protected double rho_w = SPhysics.rho_w;
        #region Поля результатов

        /// <summary>
        /// скорость в створе
        /// </summary>
        public double[] Ux;
        //public double[] Ux_old;
        /// <summary>
        /// Предыдущее решение
        /// </summary>
        protected double[] result_old = null;
        #endregion

        /// <summary>
        /// Параметр SUPG стабилизации 
        /// </summary>
        public double omega = 0.5;
        /// <summary>
        /// Матрица масс
        /// </summary>
        protected double[][] C =
        {
            new double[3] { 1 / 12.0, 1 / 24.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 12.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 24.0, 1 / 12.0 }
        };
        public TransportTri() 
        {
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra,IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref Ux);
            //MEM.Alloc(mesh.CountKnots, ref Ux_old);
            if (algebra == null)
                algebra = new AlgebraGauss((uint)mesh.CountKnots);
        }
        #region Стационарные решения задачи
        /// <summary>
        /// Нахождение поля скорости в створе потока с учетом вторичных скоростей в створе  
        /// </summary>
        /// <param name="U">искомая компонента скорости</param>
        /// <param name="eddyViscosity">турбулентная вязкость</param>
        /// <param name="R0">радиус звкругления створа</param>
        /// <param name="Ring">флаг задачи 0 - плоская 1 - цилиндрические координаты</param>
        /// <param name="Vy">радиальная/горизонтальная скорость в створе</param>
        /// <param name="Vz">вертикальная скорость в створе</param>
        /// <param name="bc">узлы для граничных условий</param>
        /// <param name="bv">граничные условия</param>
        /// <param name="Q">правая часть</param>
        public void SolveTransportEquations(ref double[] U, double[] eddyViscosity, double R0, int Ring,
            double[] Vy, double[] Vz, uint[] bc, double[] bv, double[] Q)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
                // выделить память под локальные массивы
                for (int elem = 0; elem < mesh.CountElements; elem++)
                {
                    elemEx = elem;
                    // локальная матрица часть СЛАУ
                    double[][] LaplMatrix = new double[3][]
                    {
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 }
                    };
                    // локальная правая часть СЛАУ
                    double[] LocalRight = { 0, 0, 0 };
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    uint[] knots = { i0, i1, i2 };
                    double R = 0;
                    double eVx;
                    double eVy;
                    double dMu_dy = 0;
                    double mu_R1 = 0, mu_R2 = 0;
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    if (Ring == 1)
                    {
                        R = R0 + (X[i0] + X[i1] + X[i2]) / 3;
                        // градингт вязкости по радиусу
                        dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                        mu_R1 = dMu_dy / R + mU / (R * R);
                        mu_R2 = mU / R;
                    }
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = rho_w * mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];
                        double L_SUPG = omega * (Hx[elem] * Ax * b[i] + Hy[elem] * Ay * c[i]);

                        for (int j = 0; j < cu; j++)
                        {
                            // конверктивные члены плоские
                            double additionC = N_NVx * b[j] + N_NVy * c[j];
                            //double additionC = 0;
                            // конверктивные члены цилиндрические
                            double additionC_R = Ring * C[i][j] * (eVx / R) * S[elem];
                            // конверктивные члены плоские SUPG
                            double additionSUPG = L_SUPG * (eVx * b[j] + eVy * c[j]) * S[elem] / 3;
                            // конверктивные члены цилиндрические SUPG 
                            double additionSUPG_R = Ring * L_SUPG * (eVx / R) * S[elem] / 3;

                            //  вязкие члены плоские
                            double additionD = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            //  вязкие члены цилиндрические 
                            double additionD_R1 = Ring * (C[i][j] * mu_R1 - mu_R2 * b[i] / 3) * S[elem];
                            //  вязкие члены цилиндрические SUPG 
                            double additionD_R2 = Ring * L_SUPG * (mu_R1 / 3 - mu_R2 * b[i]) * S[elem];

                            LaplMatrix[i][j] = additionD + additionD_R1 + additionD_R2 +
                                (additionC + additionC_R + additionSUPG + additionSUPG_R) * rho_w;
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                    {
                        double Q1 = mQ * S[elem] / 3;
                        double Qu = S[elem] / 3 * omega * Hx[elem] * Ax * b[j] * mQ;
                        double Qv = S[elem] / 3 * omega * Hy[elem] * Ay * c[j] * mQ;
                        LocalRight[j] = Q1 + Qu + Qv;
                    }
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bv, bc);
                algebra.Solve(ref U);
                foreach (var ee in U)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("FEPoissonTask >> algebra");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elemEx.ToString());
            }
        }
        /// <summary>
        /// Нахождение поля скорости в створе потока с учетом вторичных скоростей в створе  
        /// </summary>
        /// <param name="U">искомая компонента скорости</param>
        /// <param name="eddyViscosity">турбулентная вязкость</param>
        /// <param name="R0">радиус звкругления створа</param>
        /// <param name="Ring">флаг задачи 0 - плоская 1 - цилиндрические координаты</param>
        /// <param name="Phi"></param>
        /// <param name="bc">узлы для граничных условий</param>
        /// <param name="bv">граничные условия</param>
        /// <param name="Q">правая часть</param>
        public void SolveTransportEquations(ref double[] U, double[] eddyViscosity, double R0, int Ring,
            double[] Phi, uint[] bc, double[] bv, double[] Q)
        {
            int elemEx = 0;
            try
            {
                MEM.Alloc(mesh.CountKnots, ref U);
                algebra.Clear();
                // выделить память под локальные массивы
                for (int elem = 0; elem < mesh.CountElements; elem++)
                {
                    elemEx = elem;
                    // локальная матрица часть СЛАУ
                    double[][] LaplMatrix = new double[3][]
                    {
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 }
                    };
                    // локальная правая часть СЛАУ
                    double[] LocalRight = { 0, 0, 0 };
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    uint[] knots = { i0, i1, i2 };
                    double R;
                    double eVx;
                    double eVy;
                    double dMu_dy = 0;
                    double mu_R1 = 0, mu_R2 = 0;
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;

                    if (Ring == 0)
                    {
                        R = 1;
                        eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);
                    }
                    else
                    {
                        R = R0 + (X[i0] + X[i1] + X[i2]) / 3;
                        eVx = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2])/R;
                        eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2])/R;
                        // градингт вязкости по радиусу
                        dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                        mu_R1 = dMu_dy / R + mU / (R * R);
                        mu_R2 = mU / R;
                    }
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = rho_w * mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double L_SUPG = omega * (Hx[elem] * Ax * b[i] + Hy[elem] * Ay * c[i]);
                        double La = (1 / 3.0 + L_SUPG);
                        for (int j = 0; j < cu; j++)
                        {
                            // конверктивные члены плоские
                            double C_agb = La * ((c[0] * b[j] - b[0] * c[j]) * Phi[i0] +
                                                 (c[1] * b[j] - b[1] * c[j]) * Phi[i1] +
                                                 (c[2] * b[j] - b[2] * c[j]) * Phi[i2]) * S[elem];
                            // конверктивные члены цилиндрические
                            double additionC_R = Ring * C[i][j] * (eVx / R) * S[elem];
                            // конверктивные члены цилиндрические SUPG 
                            double additionSUPG_R = Ring * L_SUPG * (eVx / R) * S[elem] / 3;
                            //  вязкие члены плоские
                            double additionD = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            //  вязкие члены цилиндрические 
                            double additionD_R1 = Ring * (C[i][j] * mu_R1 - mu_R2 * b[i] / 3) * S[elem];
                            //  вязкие члены цилиндрические SUPG 
                            double additionD_R2 = Ring * L_SUPG * (mu_R1 / 3 - mu_R2 * b[i]) * S[elem];
                            LaplMatrix[i][j] = additionD + additionD_R1 + additionD_R2 +
                                (C_agb + additionC_R  + additionSUPG_R) * rho_w;
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                    {
                        double Q1 = mQ * S[elem] / 3;
                        double Qu = S[elem] / 3 * omega * Hx[elem] * Ax * b[j] * mQ;
                        double Qv = S[elem] / 3 * omega * Hy[elem] * Ay * c[j] * mQ;
                        LocalRight[j] = Q1 + Qu + Qv;
                    }
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bv, bc);
                algebra.Solve(ref U);
                if(Debug > 0)
                    foreach (var ee in U)
                        if (double.IsNaN(ee) == true)
                            throw new Exception("FEPoissonTask >> algebra");

                MEM.Copy(Ux, U);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elemEx.ToString());
            }
        }
        #endregion
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result)
        {

        }
    }
}
