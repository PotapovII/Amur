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
//                 кодировка : 04.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using MeshLib;
    using MemLogLib;
    using CommonLib;

    using System;
    using System.Linq;

    [Serializable]
    public class FETransportEquationsTask : FEMPoissonTask
    {
        double dx, dy, mV, Ax, Ay, omega = 0.5;
        /// <summary>
        /// Скорости Vx в узлах КЭ 
        /// </summary>
        protected double[] elem_Vx = null;
        /// <summary>
        /// Скорости Vy в узлах КЭ 
        /// </summary>
        protected double[] elem_Vy = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public FETransportEquationsTask(IMesh mesh, IAlgebra algebra) : base(mesh, algebra)
        {
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public override void InitLocal(int cu)
        {
            MEM.Alloc(cu, ref elem_mu, "elem_mu");
            MEM.Alloc(cu, ref elem_Q,  "elem_Q");
            MEM.Alloc(cu, ref elem_Vx, "elem_Vx");
            MEM.Alloc(cu, ref elem_Vy, "elem_Vx");
            base.InitLocal(cu, 1);
        }

        /// <summary>
        /// Нахождение поля скоростей из решения задачи переноса МКЭ  
        /// центральная схема условно устойчивая схема
        /// </summary>
        public virtual void CFETransportEquationsTask(ref double[] U, double[] eddyViscosity,
            double[] Vx, double[] Vy, uint[] bc, double[] bv, double[] Q)
        {
            uint elem = 0;
            try
            {
                algebra.Clear();
                // выделить память под локальные массивы
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    cu = knots.Length;
                    // память
                    InitLocal(cu);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // получит вязкость в узлах

                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    mesh.ElemValues(Q, elem, ref elem_Q);
                    mesh.ElemValues(Vx, elem, ref elem_Vx);
                    mesh.ElemValues(Vy, elem, ref elem_Vy);

                    // функции формы
                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        double Mu = 0;
                        for (int ai = 0; ai < cu; ai++)
                            Mu += elem_mu[ai] * ff.N[ai];

                        double mQ = 0;
                        for (int ai = 0; ai < cu; ai++)
                            mQ += elem_Q[ai] * ff.N[ai];

                        double mVx = 0;
                        for (int ai = 0; ai < cu; ai++)
                            mVx += elem_Vx[ai] * ff.N[ai];

                        double mVy = 0;
                        for (int ai = 0; ai < cu; ai++)
                            mVy += elem_Vy[ai] * ff.N[ai];

                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                            {
                                LaplMatrix[ai][aj] += ( mVx * ff.N[ai] * ff.DN_x[aj] + 
                                                        mVy * ff.N[ai] * ff.DN_y[aj] +
                                                        Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj])) * DWJ;
                            }
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += mQ * ff.N[ai] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
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
            uint elem = 0;
            mesh.ElementKnots(elem, ref knots);
            mesh.GetElemCoords(elem, ref x, ref y);
            dx = x.Max() - x.Min();
            dy = y.Max() - y.Min();
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
            try
            {
                algebra.Clear();
                // выделить память под локальные массивы
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    cu = knots.Length;
                    // память
                    InitLocal(cu);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // получит вязкость в узлах

                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    mesh.ElemValues(Q, elem, ref elem_Q);
                    mesh.ElemValues(Vx, elem, ref elem_Vx);
                    mesh.ElemValues(Vy, elem, ref elem_Vy);

                    // функции формы
                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        double Mu = 0;
                        for (int ai = 0; ai < cu; ai++)
                            Mu += elem_mu[ai] * ff.N[ai];

                        double mQ = 0;
                        for (int ai = 0; ai < cu; ai++)
                            mQ += elem_Q[ai] * ff.N[ai];

                        double mVx = 0;
                        for (int ai = 0; ai < cu; ai++)
                            mVx += elem_Vx[ai] * ff.N[ai];

                        double mVy = 0;
                        for (int ai = 0; ai < cu; ai++)
                            mVy += elem_Vy[ai] * ff.N[ai];

                        mV = Math.Sqrt(mVx * mVx + mVy * mVy);
                        if (MEM.Equals(mV, 0) != true)
                        {
                            dx = x.Max() - x.Min();
                            dy = y.Max() - y.Min();
                            Ax = mVx / mV;
                            Ay = mVy / mV;
                        }
                        else
                        {
                            dx = 0;
                            dy = 0;
                            Ax = 0;
                            Ay = 0;
                        }

                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                            {
                                LaplMatrix[ai][aj] += (
                                        mVx * ff.N[ai] * ff.DN_x[aj] +
                                        mVy * ff.N[ai] * ff.DN_y[aj] +
                                        Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) +
                                     // SUPG
                                        omega * dx * Ax * ff.DN_x[ai] * (mVx * ff.DN_x[aj] + mVy * ff.DN_y[aj]) +
                                        omega * dy * Ay * ff.DN_y[ai] * (mVx * ff.DN_x[aj] + mVy * ff.DN_y[aj]) 
                                        ) * DWJ;
                            }
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                        {
                            LocalRight[ai] += mQ * ( ff.N[ai] + 
                                omega * dx * Ax * ff.DN_x[ai] +
                                omega * dy * Ay * ff.DN_y[ai] ) * DWJ;
                        }
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
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
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }
    }
}
