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
namespace FEMTasksLib.FVMTasks
{
    using CommonLib;
    using MemLogLib;
    using System;
    class HeatMassTri : AFETask
    {
        IFVMesh vmesh;
        /// <summary>
        /// Параметр схемы по времени
        /// </summary>
        private double theta;
        /// <summary>
        /// Шаг по времени
        /// </summary>
        private double dt;
        /// <summary>
        /// теплоемкость
        /// </summary>
        private double Cr;
        /// <summary>
        /// плотность
        /// </summary>
        private double Ro;
        /// <summary>
        /// теплопроводность
        /// </summary>
        private double Lam;
        /// <summary>
        /// объемный тепловой источник
        /// </summary>
        private double Q;
        /// <summary>
        /// коэффициент теплопередачи
        /// </summary>
        private double Hp;
        /// <summary>
        /// Параметр метода интегрирования
        /// </summary>
        private double St;
        protected int indexBC;
        //Градиенты от функций форм
        protected const int cu = 3;
        protected double[] a = new double[cu];
        protected double[] dNdx = new double[cu];
        protected double[] VelosityX = null;
        protected double[] VelosityY = null;
        protected double[] VX = { 0, 0, 0 };
        protected double[] VY = { 0, 0, 0 };
        public HeatMassTri(double theta, double dt, double Cr, double Ro, double Lam, double Q, double Hp,
            double St, int indexBC, double[] VelosityX, double[] VelosityY)
        {
            vmesh = (IFVMesh)mesh;
            vmesh.CalculationOfExtensions();
            this.theta = theta;
            this.dt = dt;
            this.Cr = Cr;
            this.Ro = Ro;
            this.Lam = Lam;
            this.Q = Q;
            this.Hp = Hp;
            this.St = St;
            this.indexBC = indexBC;
            this.VelosityX = VelosityX;
            this.VelosityY = VelosityY;
        }
        public override void SolveTask(ref double[] result)
        {
            MEM.Alloc<double>(mesh.CountKnots, ref result);
            algebra.Clear();
            InitLocal(cu, 1);
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                mesh.ElementKnots(elem, ref knots);
                // координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                // получение поля скоростей из буффера задачи
                mesh.ElemValues(VelosityX, elem, ref VX);
                mesh.ElemValues(VelosityY, elem, ref VY);
                // площадь
                double S = mesh.ElemSquare(elem);
                // Расчет диаметра КЭ
                double Diam = Math.Sqrt(S);
                a[0] = (y[1] - y[2]);
                dNdx[0] = (x[2] - x[1]);
                a[1] = (y[2] - y[0]);
                dNdx[1] = (x[0] - x[2]);
                a[2] = (y[0] - y[1]);
                dNdx[2] = (x[1] - x[0]);
                double u = (VX[0] + VX[1] + VX[2]) / 3;
                double v = (VY[0] + VY[1] + VY[2]) / 3;
                double Velosity = Math.Sqrt(u * u + v * v);
                double Pecle = Velosity * Ro * Diam / Lam;
                double Gamma = 0;
                if (Pecle > 0)
                    Gamma = Diam * Ro * Ro / (2 * Velosity * Ro);   // 0.5


                double Sigma = 3 * Ro / (Lam * dt);
                double DSigma = Ro / (Lam * dt);
                double pu = Ro * u / Lam;
                double pv = Ro * v / Lam;
                double S1 = (2 - St);
                double S2 = (1 + St);
                //double MTime;
                //double RegConvI, RegConvJ, DRegConvJ;
                //double Conv, ConvJ;
                double Difuz;
                // вычисление ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                {

                    for (int aj = 0; aj < cu; aj++)
                    {
                        //Conv = u * a[ai] + v * dNdx[ai];
                        Difuz = Gamma * DSigma * (a[ai] * a[aj] + dNdx[ai] * dNdx[aj]) / (4 * S);

                        LaplMatrix[ai][aj] = Difuz;
                    }
                }
                //// цикл по узлам
                //for (i = 0; i < CountT; i++)
                //{
                //    RegConvI = Gamma * (DSigma * FT.N(i) + 0.5 * (pu * FT.DNx(i) + pv * FT.DNy(i)));

                //    for (j = 0; j < CountT; j++)
                //    {
                //        RegConvJ = (DSigma * FT.N(j) + 0.5 * (pu * FT.DNx(j) + pv * FT.DNy(j)));
                //        DRegConvJ = (DSigma * FT.N(j) - 0.5 * (pu * FT.DNx(j) + pv * FT.DNy(j)));

                //        MTime = Sigma * FT.N(i) * FT.N(j);
                //        //
                //        ConvJ = pu * FT.DNx(j) + pv * FT.DNy(j);
                //        //
                //        Difuz = FT.DNx(i) * FT.DNx(j) + FT.DNy(i) * FT.DNy(j);
                //        // ведущая матрица
                //        LMatrix[i][j] += ((MTime + S1 * (FT.N(i) * ConvJ + Difuz))
                //                       + RegConvI * RegConvJ + Gamma * DSigma * (Difuz)) * DJW;

                //        // ведомая матрица
                //        DLMatrix[i][j] += ((MTime - S2 * (FT.N(i) * ConvJ + Difuz))
                //                        + RegConvI * DRegConvJ) * DJW;
                //    }
                //    // Правая часть
                //    LRight[i] += (3 * FT.N(i) + RegConvI) * Q * DJW;
                //}


                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                // вычисление ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = Q * S / 3;
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            //algebra.Print();
            // удовлетворение ГУ
            uint[] bound = mesh.GetBoundKnotsByMarker(indexBC);
            algebra.BoundConditions(0.0, bound);
            //algebra.Print();
            algebra.Solve(ref result);
            //algebra.Print();
        }

    }
}
