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
//                    Проект "Home" dNdy++
//                  - (C) Copyright 2003
//                        Потапов И.И.
//                         14.11.03
//---------------------------------------------------------------------------
//         сильно, упрощенный перенос с dNdy++ на dNdy#
//                 нет адресных таблиц ...
//                      Потапов И.И.
//                      07.04.2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using MemLogLib;
    using System;
    /// <summary>
    ///  ОО: Решатель для задачи Навье-Стокса в формулировке вихрь-функция тока
    /// </summary>
    [Serializable]
    public class VortexStreamFunction : AFETask
    {
        private double Mu;
        private double Q;
        private int indexBC;
        /// <summary>
        /// температура
        /// </summary>
        private double[] t, t_old;
        /// <summary>
        /// градиенты температуры
        /// </summary>
        private double[] tx, ty;
        /// <summary>
        /// функция тока
        /// </summary>
        private double[] psi, psi_old;
        /// <summary>
        /// скорости
        /// </summary>
        private double[] vx, vy;
        /// <summary>
        /// Функция вихря
        /// </summary>
        private double[] omega, omega_old;
        private double[] S;
        private double[] nodS;
        private double[][] dNdx;
        private double[][] dNdy;
        private double residual = 0.0;

        /// <summary>
        ///  Число Прандтля.
        /// </summary>
        double Pr = 16.0;
        /// <summary>
        /// Число Грасгофа.
        /// </summary>
        double Gr = 1000.0;
        /// <summary>
        /// Коэффициент релаксации.
        /// </summary>
        double w = 0.3;

        public VortexStreamFunction(double Mu, double Q, int indexBC)
        {
            this.Mu = Mu;
            this.Q = Q;
            this.indexBC = indexBC;
        }

        public void CalkGrad(double[] p, ref double[] dpdx, ref double[] dpdy)
        {
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                uint node0 = knots[0];
                uint node1 = knots[1];
                uint node2 = knots[2];
                double eU = dNdy[elem][0] * p[node0] + dNdy[elem][1] * p[node1] + dNdy[elem][2] * p[node2];
                double eV = dNdx[elem][0] * p[node0] + dNdx[elem][1] * p[node1] + dNdx[elem][2] * p[node2];
                dpdx[node0] += eU * S[elem] / 3.0;
                dpdx[node1] += eU * S[elem] / 3.0;
                dpdx[node2] += eU * S[elem] / 3.0;
                dpdy[node0] += eV * S[elem] / 3.0;
                dpdy[node1] += eV * S[elem] / 3.0;
                dpdy[node2] += eV * S[elem] / 3.0;
            }
            for (uint nod = 0; nod < mesh.CountKnots; nod++)
            {
                dpdx[nod] /= nodS[nod];
                dpdy[nod] /= nodS[nod];
            }
        }

        public override void SolveTask(ref double[] result)
        {
            int node_count = mesh.CountKnots;
            tx = new double[node_count];
            ty = new double[node_count];
            vx = new double[node_count];
            vy = new double[node_count];
            nodS = new double[node_count];
            S = new double[mesh.CountElements];
            MEM.Alloc2D(mesh.CountElements, 3, ref dNdx);
            MEM.Alloc2D(mesh.CountElements, 3, ref dNdy);

            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                S[elem] = mesh.ElemSquare(elem);

            }
            double[] x = mesh.GetCoords(0);
            double[] y = mesh.GetCoords(1);
            double x1, y1, x2, y2, x3, y3;

            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                uint node0 = knots[0];
                uint node1 = knots[1];
                uint node2 = knots[2];
                x1 = x[node0];
                x2 = x[node1];
                x3 = x[node2];
                y1 = y[node0];
                y2 = y[node1];
                y3 = y[node2];
                // Вычисление площади КЭ  
                S[elem] = 0.5 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
                double V = 1 / (2 * S[elem]);
                // Производная от симплекс функции формы по направлению Х
                dNdx[elem][0] = (y[1] - y[2]) * V;
                dNdx[elem][1] = (y[2] - y[0]) * V;
                dNdx[elem][2] = (y[0] - y[1]) * V;
                // Element Matrix df/dy
                dNdy[elem][0] = (x[2] - x[1]) * V;
                dNdy[elem][1] = (x[0] - x[2]) * V;
                dNdy[elem][3] = (x[1] - x[0]) * V;
                // Вычисление площади КО собираем кусочки площади КЭ в узле
                nodS[node0] += S[elem] / 3.0;
                nodS[node1] += S[elem] / 3.0;
                nodS[node2] += S[elem] / 3.0;
            }

            int[] ABound = mesh.GetBoundKnots();
            uint[] bknot = new uint[ABound.Length];
            for (int i = 0; i < ABound.Length; i++)
                bknot[i] = (uint)ABound[i];

            int n;
            for (n = 0; n < 10000; n++)
            {
                MEM.MemCopy(ref t_old, t);
                MEM.MemCopy(ref psi_old, psi);
                MEM.MemCopy(ref omega_old, omega);

                MEM.AllocClear(node_count, ref vx);
                MEM.AllocClear(node_count, ref vy);
                int cu = 3;
                double[][] C = null; //[3][3];     //Локальная матрица массы

                CalkGrad(psi, ref vx, ref vy);
                //************************************************************************************************************************
                // Скорость на стенках.
                //************************************************************************************************************************
                int[] bk = mesh.GetBoundKnots();
                for (uint nod = 0; nod < bk.Length; nod++)
                {
                    vx[bk[nod]] = 0;
                    vy[bk[nod]] = 0;
                }
                //************************************************************************************************************************
                // Решаем уравнение для T.
                //************************************************************************************************************************
                algebra.Clear();
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    MEM.Alloc2DClear(3, 3, ref C);
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    double detD = 2 * Math.Abs(S[elem]);
                    C[0][0] = detD / 12.0; C[0][1] = detD / 24.0; C[0][2] = detD / 24.0;
                    C[1][0] = detD / 24.0; C[1][1] = detD / 12.0; C[1][2] = detD / 24.0;
                    C[2][0] = detD / 24.0; C[2][1] = detD / 24.0; C[2][2] = detD / 12.0;
                    // вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                        {
                            LaplMatrix[ai][aj] = (dNdx[elem][ai] * dNdx[elem][aj] + dNdy[elem][ai] * dNdy[elem][aj]) * S[elem] / Pr;
                            LaplMatrix[ai][aj] += (vx[knots[0]] * C[ai][0] + vx[knots[1]] * C[ai][1] + vx[knots[2]] * C[ai][2]) * dNdx[elem][aj];
                            LaplMatrix[ai][aj] += (vy[knots[0]] * C[ai][0] + vy[knots[1]] * C[ai][1] + vy[knots[2]] * C[ai][2]) * dNdy[elem][aj];
                        }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                }
                // удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
                algebra.BoundConditions(0.0, bound);
                algebra.Solve(ref t);

                for (int i = 0; i < node_count; i++)
                    t[i] = (1 - w) * t_old[i] + w * t[i];

                //************************************************************************************************************************
                // Решаем уравнение для Omega.
                //************************************************************************************************************************
                algebra.Clear();
                CalkGrad(t, ref tx, ref ty);
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    MEM.Alloc2DClear(3, 3, ref C);
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    double detD = 2 * Math.Abs(S[elem]);
                    C[0][0] = detD / 12.0; C[0][1] = detD / 24.0; C[0][2] = detD / 24.0;
                    C[1][0] = detD / 24.0; C[1][1] = detD / 12.0; C[1][2] = detD / 24.0;
                    C[2][0] = detD / 24.0; C[2][1] = detD / 24.0; C[2][2] = detD / 12.0;
                    // вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                        {
                            LaplMatrix[ai][aj] = Mu * (dNdx[elem][ai] * dNdx[elem][aj] + dNdy[elem][ai] * dNdy[elem][aj]) * S[elem];
                            LaplMatrix[ai][aj] += (vx[knots[0]] * C[ai][0] + vx[knots[1]] * C[ai][1] + vx[knots[2]] * C[ai][2]) * dNdx[elem][aj];
                            LaplMatrix[ai][aj] += (vy[knots[0]] * C[ai][0] + vy[knots[1]] * C[ai][1] + vy[knots[2]] * C[ai][2]) * dNdy[elem][aj];
                        }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // вычисление ЛПЧ
                    for (int i = 0; i < cu; i++)
                        for (int j = 0; j < cu; j++)
                            LocalRight[j] += tx[knots[j]] * C[i][j] * Gr;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                // дно ящика
                uint[] bound0 = mesh.GetBoundKnotsByMarker(0);
                algebra.BoundConditions(0.0, bound0);
                // верх ящика
                uint[] bound2 = mesh.GetBoundKnotsByMarker(2);
                algebra.BoundConditions(0.0, bound2);
                // слево
                uint[] bound1 = mesh.GetBoundKnotsByMarker(1);
                double[] Wbound1 = new double[bound1.Length];
                for (int i = 0; i < bound1.Length; i++)
                {

                }
                algebra.BoundConditions(Wbound1, bound1);
                // справо
                uint[] bound3 = mesh.GetBoundKnotsByMarker(3);
                double[] Wbound3 = new double[bound1.Length];
                for (int i = 0; i < bound1.Length; i++)
                {
                    //- omega_old[top_omega_bc1[i] + nx] / 2.0 - 3.0 * psi[top_omega_bc1[i] + nx] / (hy * hy);
                }
                algebra.BoundConditions(Wbound3, bound3);
                algebra.Solve(ref omega);
                for (int i = 0; i < node_count; i++)
                    omega[i] = (1 - w) * omega_old[i] + w * omega[i];

                //************************************************************************************************************************
                // Решаем уравнение для Psi.
                //************************************************************************************************************************
                algebra.Clear();
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    MEM.Alloc2DClear(3, 3, ref C);
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    // вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = (dNdx[elem][ai] * dNdx[elem][aj] + dNdy[elem][ai] * dNdy[elem][aj]) * S[elem];
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // вычисление ЛПЧ
                    LocalRight[0] = (2 * omega[knots[0]] + omega[knots[1]] + omega[knots[2]]) * S[elem] / 12.0;
                    LocalRight[1] = (omega[knots[0]] + 2 * omega[knots[1]] + omega[knots[2]]) * S[elem] / 12.0;
                    LocalRight[2] = (omega[knots[0]] + omega[knots[1]] + 2 * omega[knots[2]]) * S[elem] / 12.0;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                // дно ящика
                algebra.BoundConditions(0.0, bknot);
                algebra.Solve(ref psi);
                for (int i = 0; i < node_count; i++)
                    psi[i] = (1 - w) * psi_old[i] + w * psi[i];

                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                double epsT = 0.0;
                double normT = 0.0;
                double epsPsi = 0.0;
                double normPsi = 0.0;
                double epsOmega = 0.0;
                double normOmega = 0.0;

                for (int i = 0; i < node_count; i++)
                {
                    normT += t[i] * t[i];
                    epsT += (t[i] - t_old[i]) * (t[i] - t_old[i]);
                    normPsi += psi[i] * psi[i];
                    epsPsi += (psi[i] - psi_old[i]) * (psi[i] - psi_old[i]);
                    normOmega += omega[i] * omega[i];
                    epsOmega += (omega[i] - omega_old[i]) * (omega[i] - omega_old[i]);
                }
                residual = Math.Max(Math.Sqrt(epsT / normT) / w, Math.Max(Math.Sqrt(epsPsi / normPsi) / w, Math.Sqrt(epsOmega / normOmega) / w));

                Console.WriteLine("n {0} residual {1}", n, residual);
                //cout << n << ": " << residual << endl;
                if (residual < 1e-6)
                    break;
            }
            Console.WriteLine("n {0} residual {1}", n, residual);

        }
    }
}



