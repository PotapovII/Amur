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
    using CommonLib;
    using MemLogLib;
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Решение задачи переноса на симплекс сетке
    /// </summary>
    [Serializable]
    public class FETransportEquationsTri : FEPoissonTaskTri
    {
        /// <summary>
        /// Скорости Vx в узлах КЭ 
        /// </summary>
        protected double[] elem_Vx = { 0, 0, 0 };
        /// <summary>
        /// Скорости Vy в узлах КЭ 
        /// </summary>
        protected double[] elem_Vy = { 0, 0, 0 };
        /// <summary>
        /// Матрица масс
        /// </summary>
        protected double[][] C =
        {
            new double[3] { 1 / 12.0, 1 / 24.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 12.0, 1 / 24.0 },
            new double[3] { 1 / 24.0, 1 / 24.0, 1 / 12.0 }
        };
        double[] dNdx = { 0, 0, 0 };
        double[] dNdy = { 0, 0, 0 };

        public FETransportEquationsTri(IMesh mesh, IAlgebra algebra) 
            : base(mesh, algebra) 
        {
        }
        /// <summary>
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// центральная схема условно устойчивая схема
        /// </summary>
        public virtual void CFETransportEquationsTask(ref double[] U, double[] eddyViscosity,
            double[] Vx, double[] Vy, uint[] bc, double[] bv, double[] Q)
        {
            uint elem = 0;
            double mQ, eddyViscosityConst;
            try
            {
                algebra.Clear();
                // выделить память под локальные массивы
                InitLocal(cu);
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    mesh.ElementKnots(elem, ref knots);
                    //Координаты и площадь
                    //mesh.GetElemCoords(elem, ref X, ref Y);
                    // получит вязкость в узлах
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    eddyViscosityConst = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;
                    // получить правую часть в узлах
                    mesh.ElemValues(Q, elem, ref elem_Q);
                    mQ = (elem_Q[0] + elem_Q[1] + elem_Q[2]) / 3;

                    double DJ = (X[knots[1]] - X[knots[0]]) *
                                (Y[knots[2]] - Y[knots[0]]) -
                                (X[knots[2]] - X[knots[0]]) *
                                (Y[knots[1]] - Y[knots[0]]);

                    dNdx[0] = (Y[knots[1]] - Y[knots[2]]) / DJ;
                    dNdx[1] = (Y[knots[2]] - Y[knots[0]]) / DJ;
                    dNdx[2] = (Y[knots[0]] - Y[knots[1]]) / DJ;

                    dNdy[0] = (X[knots[2]] - X[knots[1]]) / DJ;
                    dNdy[1] = (X[knots[0]] - X[knots[2]]) / DJ;
                    dNdy[2] = (X[knots[1]] - X[knots[0]]) / DJ;

                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vx[knots[0]] * C[i][0] + Vx[knots[1]] * C[i][1] + Vx[knots[2]] * C[i][2]) * DJ / 2;
                        double N_NVy = (Vy[knots[0]] * C[i][0] + Vy[knots[1]] * C[i][1] + Vy[knots[2]] * C[i][2]) * DJ / 2;

                        for (int j = 0; j < cu; j++)
                        {
                            double addition1 = eddyViscosityConst * (dNdx[i] * dNdx[j] + dNdy[i] * dNdy[j]) * DJ / 2;
                            double addition2 = N_NVx * dNdx[j];
                            double addition3 = N_NVy * dNdy[j];
                            LaplMatrix[i][j] = addition1 + addition2 + addition3;
                        }
                    }
                    if(elem<5)
                        LOG.Print("LaplMatrix", LaplMatrix);
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = mQ * DJ / 6;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                if (bv == null)
                    algebra.BoundConditions(0.0, bc);
                else
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
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }
        /// <summary>
        /// Получить сеточное чило Пекле задачи
        /// </summary>
        /// <param name="eddyViscosity"></param>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        /// <returns></returns>
        public double Get_Pe_h(double[] eddyViscosity, double[] Vx, double[] Vy)
        {
            double minEV = eddyViscosity.Min();
            double maxVx = Vx.Min();
            double maxVy = Vy.Min();
            double maxV = Math.Min(maxVx, maxVy);
            mesh.ElementKnots(0, ref knots);
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            double[] dxs = { X[knots[2]] - X[knots[1]], X[knots[0]] - X[knots[2]], X[knots[1]] - X[knots[0]] };
            double dx = dxs.Max();
            double[] dys = { Y[knots[1]] - Y[knots[2]], Y[knots[2]] - Y[knots[0]], Y[knots[0]] - Y[knots[1]] };
            double dy = dys.Max();
            double ds = Math.Min(dx, dy);
            double Pe_h = maxV * ds / minEV / 2;
            return Pe_h;
        }

        /// <summary>
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// со SUPG стабилизацией 
        /// </summary>
        public virtual void FETransportEquationsTaskSUPG(ref double[] U, double[] eddyViscosity,
            double[] Vx, double[] Vy, uint[] bc, double[] bv, double[] Q)
        {
            uint elem = 0;
            double mQ, eddyViscosityConst, eVx, eVy, mV, Hk, Pe, Gamma;
            double Ax,Ay,omega = 0.5;
            double[] dNdx = new double[cu];
            double[] dNdy = new double[cu];
            double Pe_h = Get_Pe_h(eddyViscosity, Vx, Vy);
            //if (Pe_h < 1) 
            //    omega = 0;
            //else 
                omega = 0.5;

            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
            try
            {
                algebra.Clear();
                // выделить память под локальные массивы
                InitLocal(cu);
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    mesh.ElementKnots(elem, ref knots);
                    //Координаты и площадь
                    //mesh.GetElemCoords(elem, ref X, ref Y);
                    // получит вязкость в узлах
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);

                    eddyViscosityConst = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;

                    eVx = (Vx[knots[0]] + Vx[knots[1]] + Vx[knots[2]]) / 3;
                    eVy = (Vy[knots[0]] + Vy[knots[1]] + Vy[knots[2]]) / 3;
                    mQ = (Q[knots[0]] + Q[knots[1]] + Q[knots[2]]) / 3;
                    // DJ = 2 S
                    double DJ = (X[knots[1]] - X[knots[0]]) *
                                (Y[knots[2]] - Y[knots[0]]) -
                                (X[knots[2]] - X[knots[0]]) *
                                (Y[knots[1]] - Y[knots[0]]);
                    double S = DJ / 2;

                    mV = Math.Sqrt(eVx * eVx + eVy * eVy);

                    Ax = eVx / mV;
                    Ay = eVy / mV;

                    Hk = Math.Sqrt(DJ / 2);
                    Pe = mV * Hk / (2 * eddyViscosityConst);

                    if (Pe > 2)
                        Gamma = Hk / (2 * mV);
                    else
                        Gamma = 0;

                    double[] dxs = { X[knots[2]] - X[knots[1]], X[knots[0]] - X[knots[2]], X[knots[1]] - X[knots[0]] };
                    double dx = dxs.Max();
                    double[] dys = { Y[knots[1]] - Y[knots[2]], Y[knots[2]] - Y[knots[0]], Y[knots[0]] - Y[knots[1]] };
                    double dy = dys.Max();
                    double S1 = dx * dy / 2;

                    dNdx[0] = (Y[knots[1]] - Y[knots[2]]) / DJ;
                    dNdx[1] = (Y[knots[2]] - Y[knots[0]]) / DJ;
                    dNdx[2] = (Y[knots[0]] - Y[knots[1]]) / DJ;

                    dNdy[0] = (X[knots[2]] - X[knots[1]]) / DJ;
                    dNdy[1] = (X[knots[0]] - X[knots[2]]) / DJ;
                    dNdy[2] = (X[knots[1]] - X[knots[0]]) / DJ;

                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double N_NVx = (Vx[knots[0]] * C[i][0] + Vx[knots[1]] * C[i][1] + Vx[knots[2]] * C[i][2]) * S;
                        double N_NVy = (Vy[knots[0]] * C[i][0] + Vy[knots[1]] * C[i][1] + Vy[knots[2]] * C[i][2]) * S;

                        for (int j = 0; j < cu; j++)
                        {
                            double addition1 = eddyViscosityConst * (dNdx[i] * dNdx[j] + dNdy[i] * dNdy[j]) * S;
                            double addition2 = N_NVx * dNdx[j];
                            double addition3 = N_NVy * dNdy[j];
                            
                            double additionSUPGx_x = S / 3 * omega * dx * Ax * dNdx[i] * (eVx * dNdx[j] + eVy * dNdy[j]);
                            double additionSUPGx_y = S / 3 * omega * dy * Ay * dNdy[i] * (eVx * dNdx[j] + eVy * dNdy[j]);

                            LaplMatrix[i][j] = addition1 + addition2 + addition3 + additionSUPGx_x + additionSUPGx_y;
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                    {
                        double Q1 = mQ * S / 3;
                        double Qu = S / 3 * omega * dx * Ax * dNdx[j] * mQ;
                        double Qv = S / 3 * omega * dy * Ay * dNdy[j] * mQ;
                        LocalRight[j] = Q1 + Qu + Qv;
                    }
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                if (bv == null)
                    algebra.BoundConditions(0.0, bc);
                else
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
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }

    }

}
