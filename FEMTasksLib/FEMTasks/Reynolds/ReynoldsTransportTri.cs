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
    using System.Collections.Generic;

    using AlgebraLib;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;
    /// <summary>
    /// ОО: Решение задачи переноса на симплекс сетке
    /// </summary>
    [Serializable]
    public class ReynoldsTransportTri : AWRAP_FETaskTri
    {
        /// <summary>
        /// Уклон канала
        /// </summary>
        protected double J;
        /// <summary>
        /// Объемная сила
        /// </summary>
        protected double mQ;
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        protected int SigmaTask;
        /// <summary>
        /// радиус изгиба русла
        /// </summary>
        protected double RadiusMin;
        /// <summary>
        /// Плотность воды
        /// </summary>
        protected double rho_w = SPhysics.rho_w;
        /// <summary>
        /// Радиальная/боковая скорости на WL
        /// </summary>
        public IDigFunction VelosityUx = null;
        /// <summary>
        /// граничные условия
        /// </summary>
        protected double[] bCondUx = null;
        /// <summary>
        /// граничные узлы
        /// </summary>
        protected uint[] bIndexUx = null; 

        #region Поля результатов
        /// <summary>
        /// скорость в створе
        /// </summary>
        public double[] Ux;
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
        public ReynoldsTransportTri(double J, double RadiusMin, int SigmaTask, IDigFunction VelosityUx = null) 
        {
            this.J = J;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.VelosityUx = VelosityUx;
            mQ = rho_w * SPhysics.GRAV * J;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra,IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref Ux);
            if (algebra == null)
                algebra = new AlgebraGauss((uint)mesh.CountKnots);
            CalkCrossSectionBC(mesh, VelosityUx, ref bCondUx, ref bIndexUx);
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
        public void SolveTaskUx(ref double[] U, double[] eddyViscosity, double[] Phi)
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

                    if (SigmaTask == 0)
                    {
                        R = 1;
                        eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);
                    }
                    else
                    {
                        R = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;
                        eVx = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2])/R;
                        eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2])/R;
                        // градингт вязкости по радиусу
                        dMu_dy = eddyViscosity[i0] * b[0] + eddyViscosity[i1] * b[1] + eddyViscosity[i2] * b[2];
                        mu_R1 = dMu_dy / R + mU / (R * R);
                        mu_R2 = mU / R;
                    }
                    //double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    
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
                            double additionC_R = SigmaTask * C[i][j] * (eVx / R) * S[elem];
                            // конверктивные члены цилиндрические SUPG 
                            double additionSUPG_R = SigmaTask * L_SUPG * (eVx / R) * S[elem] / 3;
                            //  вязкие члены плоские
                            double additionD = mU * (b[i] * b[j] + c[i] * c[j]) * S[elem];
                            //  вязкие члены цилиндрические 
                            double additionD_R1 = SigmaTask * (C[i][j] * mu_R1 - mu_R2 * b[i] / 3) * S[elem];
                            //  вязкие члены цилиндрические SUPG 
                            double additionD_R2 = SigmaTask * L_SUPG * (mu_R1 / 3 - mu_R2 * b[i]) * S[elem];
                            LaplMatrix[i][j] = additionD + additionD_R1 + additionD_R2 +
                                (C_agb + additionC_R  + additionSUPG_R) * rho_w;
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int i = 0; i < cu; i++)
                    {
                        LocalRight[i] = mQ * (1 + omega * (Hx[elem] * Ax * b[i] + Hy[elem] * Ay * c[i])) * S[elem] / 3.0;
                    }
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bCondUx, bIndexUx);
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
     
        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result){}
        /// <summary>
        /// Расчет граничных условий
        /// </summary>
        public static void CalkCrossSectionBC(IMesh mesh, IDigFunction velosity, ref double[] bCond, ref uint[] bIndex)
        {
            List<double> LUx = new List<double>();
            List<uint> LIdx = new List<uint>();
            var bknotsMark = mesh.GetBoundKnotsMark();
            var bknots = mesh.GetBoundKnots();
            double[] X = mesh.GetCoords(0);
            for (uint bknot = 0; bknot < mesh.CountBoundKnots; bknot++)
            {
                int mark = bknotsMark[bknot];
                uint knot = (uint)bknots[bknot];
                if (mark == 2 && velosity != null) // свободная поверхность
                {
                    double y = X[knot];
                    double bUx = velosity.FunctionValue(y);
                    LUx.Add(bUx);
                    LIdx.Add(knot);
                }
                else
                {
                    LUx.Add(0);
                    LIdx.Add(knot);
                }
            }
            // граничные условия
            bCond = LUx.ToArray();
            // граничные узлы
            bIndex = LIdx.ToArray();
        }
    }
}
