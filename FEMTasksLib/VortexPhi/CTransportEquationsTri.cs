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
//                 кодировка : 07.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;

    /// <summary>
    /// ОО: Решение задачи переноса на симплекс сетке
    /// </summary>
    [Serializable]
    public class CTransportEquationsTri : CFEPoissonTaskTri
    {
        /// <summary>
        /// Параметр SUPG стабилизации 
        /// </summary>
        public double omega = 0.5;
        protected double[] Hx;
        protected double[] Hy;
        /// <summary>
        /// Матрица масс
        /// </summary>
        protected double[][] C =
        {
            new double[3] { 1 / 12.0, 1 / 24.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 12.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 24.0, 1 / 12.0 }
        };
        public CTransportEquationsTri(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask) 
            : base(wMesh, algebra, typeTask)  
        {
            Hx = wMesh.GetHx();
            Hy = wMesh.GetHx(); 
        }
        public override void SetTask(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask)
        {
            base.SetTask(wMesh, algebra, typeTask);
            Hx = wMesh.GetHx();
            Hy = wMesh.GetHx();
        }
        /// <summary>
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// центральная схема условно устойчивая схема
        /// </summary>
        public virtual void TransportEquationsTask(ref double[] U, double[] eddyViscosity,
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
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];
                        for (int j = 0; j < cu; j++)
                        {
                            double addition1 = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            double addition2 = N_NVx * b[j];
                            double addition3 = N_NVy * c[j];



                            LaplMatrix[i][j] = addition1 + (addition2 + addition3);// * SPhysics.rho_w;
                        }
                    }
                    if(elem<5)
                        LOG.Print("LaplMatrix", LaplMatrix);
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = mQ * S[elem] / 3;
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
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// со SUPG стабилизацией 
        /// </summary>
        public virtual void TransportEquationsTaskSUPG(ref double[] U, double[] eddyViscosity,
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

                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }

                    double Gamma = 0;
                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];

                        for (int j = 0; j < cu; j++)
                        {

                            double addition1 = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            //double addition1 = (b[i] * b[j] + c[i] * c[j]) * S[elem] * SPhysics.mu;
                            double addition2 = N_NVx * b[j];
                            double addition3 = N_NVy * c[j];
                            double additionSUPGx_x = S[elem] * omega * Hx[elem] * Ax * b[i] * (eVx * b[j] + eVy * c[j]) / 3;
                            double additionSUPGx_y = S[elem] * omega * Hy[elem] * Ay * c[i] * (eVx * b[j] + eVy * c[j]) / 3;
                           // LaplMatrix[i][j] = addition1 + (addition2 + addition3 + additionSUPGx_x + additionSUPGx_y);
                            LaplMatrix[i][j] = addition1 + (addition2 + addition3 + additionSUPGx_x + additionSUPGx_y) * SPhysics.rho_w;
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
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// со SUPG стабилизацией 
        /// </summary>
        public virtual void TransportEquationsTaskSUPG_R(ref double[] U, double[] eddyViscosity, double R0, int Ring,
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
                    double R = R0 + (X[i0] + X[i1] + X[i2]) / 3;
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = SPhysics.rho_w * mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    double dMu_dy = 0;
                    double mu_R1 = 0, mu_R2 = 0;
                    if (Ring == 1)
                    {
                        // градингт вязкости по радиусу
                        dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                        mu_R1 = dMu_dy / R + mU / (R * R);
                        mu_R2 = mU / R;
                    }

                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }

                    //double Gamma = 0;
                    //if (Pe > 2)
                    //    Gamma = Hk / (2 * mV);
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];

                        double L_SUPG =  omega * ( Hx[elem] * Ax * b[i] + Hy[elem] * Ay * c[i] );

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
                            double additionD_R1 = Ring * ( C[i][j] * mu_R1 - mu_R2 * b[i] / 3 ) * S[elem];
                            //  вязкие члены цилиндрические SUPG 
                            double additionD_R2 = Ring * L_SUPG *( mu_R1 /3  - mu_R2 * b[i] ) * S[elem];

                            LaplMatrix[i][j] = additionD + additionD_R1 + additionD_R2 +
                                (additionC + additionC_R + additionSUPG  + additionSUPG_R) * SPhysics.rho_w;
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
        /// Нахождение поля вязкости из решения задачи Spalart - Allmaras 
        /// МКЭ со SUPG стабилизацией 
        /// </summary>
        public virtual void TransportEquationsTaskSUPG_SpalartAllmaras(ref double[] Mu, double[] eddyViscosity, double R0, int Ring,
            double[] Vx, double[] Vy, double[] Vz, uint[] bc, double[] bv)
        {
            int elemEx = 0;
            double nu = SPhysics.nu;
            #region Spalart - Allmaras константы
            double kappa = SPhysics.kappa_w;
            double kappa2 = kappa * kappa;
            double sigma = 2 / 3.0;
            double Cb1 = 0.1355;
            double Cb2 = 0.622;
            double Cw3_6 = 64;
            double Cw3 = 2;
            double Cw2 = 0.3;
            double Cw1 = Cb1 / kappa2 + (1 + Cb2) / sigma;
            double Ct1 = 1;
            double Ct2 = 2;
            double Ct3 = 1.2;
            double Ct4 = 0.5;
            double Utrim = 0.1;
            double n16 = 1 / 6.0;
            double tau = 0.001;
            double btau = 1 / tau;
            #endregion
            try
            {
                IMWCross wm = (IMWCross)wMesh;
                if (wm == null)
                    throw new Exception("Не совпадение моделей врапера сетки в методе " +
                        " CTransportEquationsTri.TransportEquationsTaskSUPG_SpalartAllmaras");
                double[] Distance = wm.GetDistance();
                double[] Hp = wm.GetHp();

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
                    double R = R0 + (X[i0] + X[i1] + X[i2]) / 3;
                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;

                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double nuP = mU / SPhysics.rho_w;
                    // Градиенты вязкости
                    double dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                    double dMu_dz = eddyViscosity[i0] * c[0] + eddyViscosity[i1] * c[1] + eddyViscosity[i2] * c[2];
                    double dp = (Distance[i0] + Distance[i1] + Distance[i2])/3;

                    double dNu_dy = dMu_dy / SPhysics.rho_w;
                    double dNu_dz = dMu_dz / SPhysics.rho_w;

                    double dVx_dy = Vx[i0] * b[0] + Vx[i1] * b[1] + Vx[i2] * b[2];
                    //double dVy_dy = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                    //double dVz_dy = Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2];

                    double dVx_dz = Vx[i0] * c[0] + Vx[i1] * c[1] + Vx[i2] * c[2];
                    //double dVy_dz = Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];
                    //double dVz_dz = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];

                    double chi = nuP / nu;
                    double chi3 = chi * chi * chi;
                    double fv1 = chi3 / (chi3 + 357.911);
                    double fv2 = 1 - chi / (1 + chi * fv1);

                    double kd = kappa * dp;
                    double kd2 = kd * kd;
                    // Плоские инварианты
                    double Omegaii = Math.Sqrt(dVx_dy * dVx_dy + dVx_dz * dVx_dz);

                    double Nuii = Cb2 * (dNu_dy * dNu_dy + dNu_dz * dNu_dz) / sigma;

                    double Eii = Omegaii + fv2 * nuP / kd2;
                    double r = Math.Min(nuP / (Eii * kd2), 10);
                    double gw = r + Cw2 * (Math.Pow(r, 6) - r);
                    double fw = gw * Math.Pow((1 + Cw3_6) / (Math.Pow(gw, 6) + Cw3_6), n16);
                    double ft2 = Ct3 * Math.Exp(-Ct4 * chi * chi);

                    double Pe = mV * Hk / (2 * nuP);
                    double Ax = 0;
                    double Ay = 0;
                    double mu_R1 = 0, mu_R2 = 0;

                    if (Ring == 1)
                    {
                        // градингт вязкости по радиусу
                        mu_R1 = dNu_dy / R + nuP / (R * R);
                        mu_R2 = nuP / R;
                    }

                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }

                    double Gamma = 0;
                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);
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
                            // конверктивные члены цилиндрические
                            double additionC_R = Ring * C[i][j] * (nuP / R) * S[elem];
                            // конверктивные члены плоские SUPG
                            double additionSUPG = L_SUPG * (eVx * b[j] + eVy * c[j]) * S[elem] / 3;
                            // конверктивные члены цилиндрические SUPG 
                            double additionSUPG_R = Ring * L_SUPG * (nuP / R) * S[elem] / 3;

                            //  вязкие члены плоские
                            double additionD = (mU + SPhysics.mu) * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            //  вязкие члены цилиндрические 
                            double additionD_R1 = Ring * (C[i][j] * mu_R1 - mu_R2 * b[i] / 3) * S[elem];
                            //  вязкие члены цилиндрические SUPG 
                            double additionD_R2 = Ring * L_SUPG * (mu_R1 / 3 - mu_R2 * b[i]) * S[elem];

                            LaplMatrix[i][j] = additionD + additionD_R1 + additionD_R2 +
                                (additionC + additionC_R + additionSUPG + additionSUPG_R) * SPhysics.rho_w;
                        }
                        double Ap0 = btau + (Cw1 * fw - Cb1 * ft2 / kappa2) * nuP / (dp * dp) - Cb1 * (1 - ft2) * Eii;

                        //for (i = 0; i < cu; i++)
                        //    LaplMatrix[i][i] += Ap0 * S[elem] / 3;

                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    double mQ = (btau * nuP + Nuii);
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
                algebra.Solve(ref Mu);
                foreach (var ee in Mu)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("FEPoissonTask >> algebra");
                // Пересчет
                for (uint i = 0; i < Mu.Length; i++)
                {
                    double chi = Mu[i] / nu;
                    double chi3 = chi * chi * chi;
                    double fw = chi3 / (chi3 + 357.911);
                    Mu[i] = SPhysics.rho_w * fw * Mu[i] + SPhysics.mu;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elemEx.ToString());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected double[] bv = null;
        /// <summary>
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// со SUPG стабилизацией 
        /// </summary>
        public virtual void TransportEquationsTaskSUPG(ref double[] U, double[] eddyViscosity,
            double[] Vy, double[] Vz, uint[] bc, double J)
        {
            int elemEx = 0;
            double mQ = SPhysics.rho_w * SPhysics.GRAV * J;
            MEM.Alloc(bc.Length, ref bv);
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
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }
                    else
                    {
                        Ax = 0; Ay = 0;
                    }
                    double Gamma = 0;
                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];
                        for (int j = 0; j < cu; j++)
                        {
                            double addition1 = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            double addition2 = N_NVx * b[j];
                            double addition3 = N_NVy * c[j];
                            double additionSUPGx_x = S[elem] * omega * Hx[elem] * Ax * b[i] * (eVx * b[j] + eVy * c[j]) / 3;
                            double additionSUPGx_y = S[elem] * omega * Hy[elem] * Ay * c[i] * (eVx * b[j] + eVy * c[j]) / 3;
                            LaplMatrix[i][j] = addition1 + (addition2 + addition3 + additionSUPGx_x + additionSUPGx_y) * SPhysics.rho_w;
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
        /// Нахождение поля вихря для цилиндрической или плоской задачи с SUPG стабилизацией 
        /// </summary>
        public virtual void TaskSUPGTransportVortex(ref double[] Vortex, double[] eddyViscosity, double R0, int Ring,
            double[] Vx, double[] Vy, double[] Vz, double[] tauYY, double[] tauYZ, double[] tauZZ,  
            uint[] bc, double[] bv, int VetrexTurbTask)
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

                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    //double mU = SPhysics.mu;
                    //double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    
                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }

                    double Gamma = 0;
                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);

                    double R = R0 + (X[i0] + X[i1] + X[i2]) / 3;
                    //double NN_R = S[elem] * mU / (R * R);
                    double NN_R = 0; // mU / (R * R);

                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];

                        for (int j = 0; j < cu; j++)
                        {
                            double addition1 = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            
                            double addition2 = N_NVx * b[j];
                            double addition3 = N_NVy * c[j];

                            double additionSUPGx_x = S[elem] * omega * Hx[elem] * Ax * b[i] * (eVx * b[j] + eVy * c[j] + NN_R * Ring) / 3;
                            double additionSUPGx_y = S[elem] * omega * Hy[elem] * Ay * c[i] * (eVx * b[j] + eVy * c[j] + NN_R * Ring) / 3;

                            LaplMatrix[i][j] = addition1 + (addition2 + addition3 + additionSUPGx_x + additionSUPGx_y) * SPhysics.rho_w;
                        }
                    }
                    // Учет центрбежных сил Ring == 1  ( mu * L_alpha * N_beta / R ^ 2)
                    //if (Ring == 1)
                    //{
   
                    //    for (int i = 0; i < cu; i++)
                    //        for (int j = 0; j < cu; j++)
                    //        {
                    //            LaplMatrix[i][j] += C[i][j] * NN_R;
                    //        }
                    //}
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Учет центрбежных сил Ring == 1
                    
                    if (Ring == 1)
                    {
                        double mQ = 0;
                        double U2_R0 = Vx[i0] * Vx[i0] / (R0 + X[i0]);
                        double U2_R1 = Vx[i1] * Vx[i1] / (R0 + X[i1]);
                        double U2_R2 = Vx[i2] * Vx[i2] / (R0 + X[i2]);
                        //double U2_R0 = Vx[i0] * Vx[i0] / R;
                        //double U2_R1 = Vx[i1] * Vx[i1] / R;
                        //double U2_R2 = Vx[i2] * Vx[i2] / R;
                        mQ = - (U2_R0 * c[0] + U2_R1 * c[1] + U2_R2 * c[2]) * SPhysics.rho_w;
                        // Вычисление ЛПЧ
                        for (int j = 0; j < cu; j++)
                        {
                            double Q1 = mQ * S[elem] / 3;
                            double Qu = S[elem] / 3 * omega * Hx[elem] * Ax * b[j] * mQ;
                            double Qv = S[elem] / 3 * omega * Hy[elem] * Ay * c[j] * mQ;
                            LocalRight[j] = Q1 + Qu + Qv;
                        }
                    }
                    //if (VetrexTurbTask == 0)
                    //{
                    //    // Добавка связанная с турбулентной вязкостью в уравнении для вихря
                    //    // не учитывается при решении других задач переноса
                    //    double Tzz_Tyy_0 = tauZZ[i0] - tauYY[i0];
                    //    double Tzz_Tyy_1 = tauZZ[i1] - tauYY[i1];
                    //    double Tzz_Tyy_2 = tauZZ[i2] - tauYY[i2];

                    //    double d_Tzz_Tyy_dy = Tzz_Tyy_0 * b[0] + Tzz_Tyy_1 * b[1] + Tzz_Tyy_2 * b[2];
                    //    double d_Tzz_Tyy_dz = Tzz_Tyy_0 * c[0] + Tzz_Tyy_1 * c[1] + Tzz_Tyy_2 * c[2];

                    //    double dTdy = tauYZ[i0] * b[0] + tauYZ[i1] * b[1] + tauYZ[i2] * b[2];
                    //    double dTdz = tauYZ[i0] * c[0] + tauYZ[i1] * c[1] + tauYZ[i2] * c[2];

                    //    double S3 = S[elem] / 3;

                    //    for (int j = 0; j < cu; j++)
                    //    {
                    //        LocalRight[j] += (b[j] * (dTdy + d_Tzz_Tyy_dz) -
                    //                         c[j] * (dTdz - d_Tzz_Tyy_dy)) * S3;
                    //    }
                    //}
                    //if (VetrexTurbTask == 1)
                    //{
                    //    double dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                    //    double dMu_dz = eddyViscosity[i0] * c[0] + eddyViscosity[i1] * c[1] + eddyViscosity[i2] * c[2];

                    //    double dV_dy = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                    //    double dV_dz = Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];
                    //    double dW_dy = Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2];
                    //    double dW_dz = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];
                    //    double S3 = S[elem] / 3;
                        
                    //    for (int j = 0; j < cu; j++)
                    //    {
                    //        LocalRight[j] += (b[j] * (dMu_dy * dW_dy - dMu_dz * dV_dy) -
                    //                          c[j] * (dMu_dy * dW_dz - dMu_dz * dV_dz)) * S3;
                    //    }
                    //}
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bv, bc);
                algebra.Solve(ref Vortex);
                foreach (var ee in Vortex)
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
        /// Нахождение поля вихря для цилиндрической или плоской задачи с SUPG стабилизацией 
        /// 03 05 2024  - 03 06 2024
        /// </summary>
        public virtual void TaskSUPGTransportVortex_R(ref double[] Vortex, double[] eddyViscosity, double R0, int Ring,
            double[] Vx, double[] Vy, double[] Vz, double[] tauYY, double[] tauYZ, double[] tauZZ,
            uint[] bc, double[] bv, int VetrexTurbTask)
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
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double R = R0 + (X[i0] + X[i1] + X[i2]) / 3;
                    double dMu_dy = 0, dMu_dz = 0;
                    if (Ring == 1)
                    {
                        // градиент вязкости, компоненты 
                        // по радиусу
                        dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                        // по глубине
                        dMu_dz = eddyViscosity[i0] * c[0] + eddyViscosity[i1] * c[1] + eddyViscosity[i2] * c[2];
                    }
                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;

                    double Vm_x = eVx - dMu_dy / SPhysics.rho_w;
                    double Vm_y = eVy - dMu_dz / SPhysics.rho_w;
                    double mV = Math.Sqrt(Vm_x * Vm_x + Vm_y * Vm_y);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;

                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = Vm_x / mV;
                        Ay = Vm_y / mV;
                    }
                    double Gamma = 0;
                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);
                    double rho_w = SPhysics.rho_w;
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
                            // вязкие конверктивные члены плоские
                            double additionC_Mu = - (dMu_dy * b[j] + dMu_dz * c[j]) * S[elem] / 3 / rho_w;
                            // конверктивные члены плоские SUPG
                            double additionSUPG =      L_SUPG * (eVx * b[j] + eVy * c[j]) * S[elem] / 3;
                            // вязкие конверктивные члены плоские SUPG
                            double additionSUPG_mu = - L_SUPG * (dMu_dy * b[j] + dMu_dz * c[j]) * S[elem] / rho_w;
                            //  вязкие члены плоские
                            double additionD   = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            //  вязкие члены цилиндрические 
                            double additionD_R = Ring * mU / R * c[i] * S[elem] / 3;
                            LaplMatrix[i][j] = additionD + additionD_R + 
                                (additionC + additionC_Mu + additionSUPG + additionSUPG_mu) * rho_w;
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Учет центрбежных сил Ring == 1
                    int flagDiff = 2;
                    //bool flagDiff = false;
                    // без перебрасывания производной на весовую функцию
                    if (Ring == 1)
                    {
                        switch (flagDiff)
                        {
                            case 0: // старый вариант дает "теже результаты" 
                                {
                                    double mQ = 0;
                                    double U2_R0 = Vx[i0] * Vx[i0] / (R0 + X[i0]);
                                    double U2_R1 = Vx[i1] * Vx[i1] / (R0 + X[i1]);
                                    double U2_R2 = Vx[i2] * Vx[i2] / (R0 + X[i2]);
                                    mQ = (U2_R0 * c[0] + U2_R1 * c[1] + U2_R2 * c[2]) * SPhysics.rho_w;
                                    // Вычисление ЛПЧ
                                    for (int i = 0; i < cu; i++)
                                    {
                                        double Q1 = mQ * S[elem] / 3;
                                        //double Qu = S[elem] / 3 * omega * Hx[elem] * Ax * b[i] * mQ;
                                        //double Qv = S[elem] / 3 * omega * Hy[elem] * Ay * c[i] * mQ;
                                        double Qu = S[elem] * omega * Hx[elem] * Ax * b[i] * mQ;
                                        double Qv = S[elem] * omega * Hy[elem] * Ay * c[i] * mQ;
                                        LocalRight[i] = - (Q1 + Qu + Qv);
                                    }
                                }
                                break;
                            case 1:
                                {
                                    double eU = (Vx[i0] + Vx[i1] + Vx[i2]) / 3;
                                    double eQ = eU * eU / R * SPhysics.rho_w;
                                    for (int i = 0; i < cu; i++)
                                        LocalRight[i] = eQ * c[i] * S[elem] / 3;
                                }
                                break;
                            case 2:
                                {
                                    double eU = (Vx[i0] + Vx[i1] + Vx[i2]) / 3;
                                    double dU = c[0]*Vx[i0] + c[1] * Vx[i1] + c[2] * Vx[i2];
                                    double eQ = 2 * SPhysics.rho_w * eU * dU / R;
                                    for (int i = 0; i < cu; i++)
                                    {
                                        double L_SUPG = omega * (Hx[elem] * Ax * b[i] + Hy[elem] * Ay * c[i]);
                                        LocalRight[i] = - ( eQ  / 3 + L_SUPG * eQ) * S[elem];
                                    }
                                }
                                break;
                        }
                    }

                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bv, bc);
                algebra.Solve(ref Vortex);
                foreach (var ee in Vortex)
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
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// со SUPG стабилизацией 
        /// </summary>
        public virtual void TaskSUPGTransportVortex_Plane(ref double[] Vortex, double[] eddyViscosity, 
          double[] Vx, double[] Vy, double[] Vz, double[] tauYY, double[] tauYZ, double[] tauZZ,
          uint[] bc, double[] bv, int VetrexTurbTask)
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

                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;

                    double eVx = (Vy[i0] + Vy[i1] + Vy[i2]) / 3;
                    double eVy = (Vz[i0] + Vz[i1] + Vz[i2]) / 3;
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;

                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }
                    else
                    {
                        Ax = 0; Ay = 0;
                    }
                    double Gamma = 0;
                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);

                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vy[i0] * C[i][0] + Vy[i1] * C[i][1] + Vy[i2] * C[i][2]) * S[elem];
                        double N_NVy = (Vz[i0] * C[i][0] + Vz[i1] * C[i][1] + Vz[i2] * C[i][2]) * S[elem];

                        for (int j = 0; j < cu; j++)
                        {
                            double addition1 = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];

                            double addition2 = N_NVx * b[j];
                            double addition3 = N_NVy * c[j];

                            double additionSUPGx_x = S[elem] * omega * Hx[elem] * Ax * b[i] * (eVx * b[j] + eVy * c[j]) / 3;
                            double additionSUPGx_y = S[elem] * omega * Hy[elem] * Ay * c[i] * (eVx * b[j] + eVy * c[j]) / 3;

                            LaplMatrix[i][j] = addition1 + (addition2 + addition3 + additionSUPGx_x + additionSUPGx_y) * SPhysics.rho_w;
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Учет центрбежных сил Ring == 1
                    double mQ = 0;
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                    {
                        double Q1 = mQ * S[elem] / 3;
                        double Qu = S[elem] / 3 * omega * Hx[elem] * Ax * b[j] * mQ;
                        double Qv = S[elem] / 3 * omega * Hy[elem] * Ay * c[j] * mQ;
                        LocalRight[j] = Q1 + Qu + Qv;
                    }
                    //if (VetrexTurbTask == 0)
                    //{
                    //    // Добавка связанная с турбулентной вязкостью в уравнении для вихря
                    //    // не учитывается при решении других задач переноса
                    //    double Tzz_Tyy_0 = tauZZ[i0] - tauYY[i0];
                    //    double Tzz_Tyy_1 = tauZZ[i1] - tauYY[i1];
                    //    double Tzz_Tyy_2 = tauZZ[i2] - tauYY[i2];

                    //    double d_Tzz_Tyy_dy = Tzz_Tyy_0 * b[0] + Tzz_Tyy_1 * b[1] + Tzz_Tyy_2 * b[2];
                    //    double d_Tzz_Tyy_dz = Tzz_Tyy_0 * c[0] + Tzz_Tyy_1 * c[1] + Tzz_Tyy_2 * c[2];

                    //    double dTdy = tauYZ[i0] * b[0] + tauYZ[i1] * b[1] + tauYZ[i2] * b[2];
                    //    double dTdz = tauYZ[i0] * c[0] + tauYZ[i1] * c[1] + tauYZ[i2] * c[2];

                    //    double S3 = S[elem] / 3;

                    //    for (int j = 0; j < cu; j++)
                    //    {
                    //        LocalRight[j] += (b[j] * (dTdy + d_Tzz_Tyy_dz) -
                    //                         c[j] * (dTdz - d_Tzz_Tyy_dy)) * S3;
                    //    }
                    //}
                    //if (VetrexTurbTask == 1)
                    //{
                    //    double dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                    //    double dMu_dz = eddyViscosity[i0] * c[0] + eddyViscosity[i1] * c[1] + eddyViscosity[i2] * c[2];

                    //    double dV_dy = Vy[i0] * b[0] + Vy[i1] * b[1] + Vy[i2] * b[2];
                    //    double dV_dz = Vy[i0] * c[0] + Vy[i1] * c[1] + Vy[i2] * c[2];
                    //    double dW_dy = Vz[i0] * b[0] + Vz[i1] * b[1] + Vz[i2] * b[2];
                    //    double dW_dz = Vz[i0] * c[0] + Vz[i1] * c[1] + Vz[i2] * c[2];
                    //    double S3 = S[elem] / 3;

                    //    for (int j = 0; j < cu; j++)
                    //    {
                    //        LocalRight[j] += (b[j] * (dMu_dy * dW_dy - dMu_dz * dV_dy) -
                    //                          c[j] * (dMu_dy * dW_dz - dMu_dz * dV_dz)) * S3;
                    //    }
                    //}
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bv, bc);
                algebra.Solve(ref Vortex);
                foreach (var ee in Vortex)
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
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void Calk_Q_forVortex(double[] eddyVis, double[] Vortex, ref double[] Q)
        {
            try
            {
                double[] Selem = null;
                double[] Qv = null;
                double[] tmpQ = null;
                MEM.Alloc((uint)mesh.CountElements, ref Selem, "Selem");
                MEM.Alloc((uint)mesh.CountElements, ref Qv, "Qv");
                MEM.Alloc((uint)mesh.CountKnots, ref tmpQ, "tmpQ");

                // Parallel.For(0, mesh.CountElements, (elem, state) =>
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dEddyVisdx = eddyVis[i0] * b[0] + eddyVis[i1] * b[1] + eddyVis[i2] * b[2];
                    double dEddyVisdy = eddyVis[i0] * c[0] + eddyVis[i1] * c[1] + eddyVis[i2] * c[2];
                    double dVortexdx = Vortex[i0] * b[0] + Vortex[i1] * b[1] + Vortex[i2] * b[2];
                    double dVortexdy = Vortex[i0] * c[0] + Vortex[i1] * c[1] + Vortex[i2] * c[2];
                    double S3 = S[elem];
                    Qv[elem] = ( dEddyVisdx * dVortexdx + dEddyVisdy * dVortexdy ) * S[elem];
                    Selem[i0] += S3;
                    Selem[i1] += S3;
                    Selem[i2] += S3;
                    tmpQ[i0] += Qv[elem] / 3;
                    tmpQ[i1] += Qv[elem] / 3;
                    tmpQ[i2] += Qv[elem] / 3;

                }//);
                for (int i = 0; i < tmpQ.Length; i++)
                {
                    Q[i] = tmpQ[i] / Selem[i];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void Calk_Q_forVortex(double[] eddyVis, double[] V, double[] W, ref double[] Q)
        {
            try
            {
                double[] Selem = null;
                double[] Qv = null;
                double[] tmpQ = null;
                MEM.Alloc((uint)mesh.CountElements, ref Selem, "Selem");
                MEM.Alloc((uint)mesh.CountElements, ref Qv, "Qv");
                MEM.Alloc((uint)mesh.CountKnots, ref tmpQ, "tmpQ");

                // Parallel.For(0, mesh.CountElements, (elem, state) =>
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dEddyVisdx = eddyVis[i0] * b[0] + eddyVis[i1] * b[1] + eddyVis[i2] * b[2];
                    double dEddyVisdy = eddyVis[i0] * c[0] + eddyVis[i1] * c[1] + eddyVis[i2] * c[2];

                    double dVdx = V[i0] * b[0] + V[i1] * b[1] + V[i2] * b[2];
                    double dVdy = V[i0] * c[0] + V[i1] * c[1] + V[i2] * c[2];

                    double dWdx = W[i0] * b[0] + W[i1] * b[1] + W[i2] * b[2];
                    double dWdy = W[i0] * c[0] + W[i1] * c[1] + W[i2] * c[2];

                    double S3 = S[elem];

                    Qv[elem] = (dEddyVisdx * dWdx - dEddyVisdy * dVdx +
                                dEddyVisdx * dWdy - dEddyVisdy * dVdy ) * S[elem];

                    Selem[i0] += S3;
                    Selem[i1] += S3;
                    Selem[i2] += S3;
                    tmpQ[i0] += Qv[elem] / 3;
                    tmpQ[i1] += Qv[elem] / 3;
                    tmpQ[i2] += Qv[elem] / 3;

                }//);
                for (int i = 0; i < tmpQ.Length; i++)
                {
                    Q[i] = tmpQ[i] / Selem[i];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
    }
}
