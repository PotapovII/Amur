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
//                 кодировка : 03.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using System;

    using MeshLib;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: Решение задачи Пуассона на IMesh сетке 
    /// </summary>
    [Serializable]
    public class FEMPoissonTask : AFETask, IFEPoissonTask
    {
        /// <summary>
        /// функции формы для КЭ
        /// </summary>
        protected AbFunForm FForm { get; set; }
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        protected private int cu ;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected double[] u = null;
        /// <summary>
        /// правая часть
        /// </summary>
        protected double[] elem_Q = null;
        /// <summary>
        /// Скорость в узлах
        /// </summary>
        protected double[] elem_U = null;
        /// <summary>
        /// вязкость в узлах КЭ 
        /// </summary>
        protected double[] elem_mu = null;
        /// <summary>
        /// координаты узлов КЭ 
        /// </summary>
        protected double[] elem_x = null;
        protected double[] elem_y = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public FEMPoissonTask(IMesh mesh, IAlgebra algebra)
        {
            SetTask(mesh, algebra);
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public virtual void InitLocal(int cu)
        {
            base.InitLocal(cu, 1);
        }
        /// <summary>
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        public virtual void FEPoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double Q)
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
                    MEM.Alloc(cu, ref elem_mu);
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    // функции формы
                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        double Mu = 0;
                        for (int ai = 0; ai < cu; ai++)
                            Mu += elem_mu[ai] * ff.N[ai];

                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += Q * ff.N[ai] * DWJ;
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с переменной правой частью
        /// </summary>
        public virtual void FEPoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double[] Q)
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
                    MEM.Alloc(cu, ref elem_mu);
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    MEM.Alloc(cu, ref elem_Q);
                    mesh.ElemValues(Q, elem, ref elem_Q);
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

                        double eQ = 0;
                        for (int ai = 0; ai < cu; ai++)
                            eQ += elem_Q[ai] * ff.N[ai];

                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += eQ * ff.N[ai] * DWJ;
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
        /// Интерполяция поля МКЭ с КЭ в узлы КЭ
        /// </summary>
        /// <param name="TauZ"></param>
        /// <param name="tmpTauZ"></param>
        public virtual void Interpolation(ref double[] TauZ, double[] elemTauZ)
        {
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // узлы
                TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                // память
                InitLocal(knots.Length);
                //Координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
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
                    // подготовка ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] += ff.N[ai] * ff.N[aj] * DWJ;
                    // подготовка ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] += elemTauZ[elem] * ff.N[j] * DWJ;
                }
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauZ);
            algebra.Clear();
        }
        /// <summary>
        /// Расчет интеграла по площади расчетной области для функции U 
        /// (например расхода воды через створ, если U - скорость потока в узлах)
        /// </summary>
        /// <param name="U">функции</param>
        /// <param name="Area">площадь расчетной области </param>
        /// <returns>интеграла по площади расчетной области для функции U</returns>
        /// <exception cref="Exception"></exception>
        public virtual double RiverFlowRate(double[] U,ref double Area)
        {
            Area = 0;
            double riverFlowRateCalk = 0;
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // узлы
                TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                // память
                InitLocal(knots.Length);
                //Координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                // функции формы
                AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                MEM.Alloc(knots.Length, ref elem_U);
                mesh.ElemValues(U, elem, ref elem_U);
                // установка координат узлов
                ff.SetGeoCoords(x, y);
                // цикл по точкам интегрирования
                for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                {
                    ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    double DWJ = ff.DetJ * pIntegration.weight[pi];
                    double pU = 0;
                    for (int ai = 0; ai < cu; ai++)
                        pU += elem_mu[ai] * ff.N[ai];
                    
                    for (int ai = 0; ai < cu; ai++)
                    {
                        riverFlowRateCalk += pU * ff.N[ai] * DWJ;
                        Area += ff.N[ai] * DWJ;
                    }
                }
            }
            if (double.IsNaN(riverFlowRateCalk) == true)
                throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Интеграл поля U по области с площадью Area
        /// </summary>
        /// <param name="U">Поле</param>
        /// <param name="Area">площадью области</param>
        /// <returns></returns>
        public virtual double SimpleRiverFlowRate(double[] U, ref double Area)
        {
            return RiverFlowRate(U, ref Area);
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void CalkTauInterpolation(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity)
        {
            try
            {
                double[] tmpTausZ = null;
                double[] tmpTausY = null;
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausY, "tmpTausY");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausZ, "tmpTausZ");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // память
                    InitLocal(knots.Length);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // функции формы
                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                    // получит вязкость в узлах
                    MEM.Alloc(knots.Length, ref elem_mu);
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    double Sigma_xy = 0;
                    double Sigma_xz = 0;
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        double pMu = 0;
                        for (int ai = 0; ai < cu; ai++)
                            pMu += elem_mu[ai] * ff.N[ai];

                        for (int ai = 0; ai < cu; ai++)
                        {
                            Sigma_xy += pMu * ff.DN_x[ai] ;
                            Sigma_xz += pMu * ff.DN_y[ai] ;
                        }
                    }
                    tmpTausZ[elem] = Sigma_xy;
                    tmpTausY[elem] = Sigma_xz;
                }
                Interpolation(ref TauZ, tmpTausZ);
                Interpolation(ref TauY, tmpTausY);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void SolveTaus(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity)
        {
            CalkTauInterpolation(ref TauY, ref TauZ, U, eddyViscosity);
        }
        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result) { }

        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalcVelosity(double[] Phi, ref double[] Vx, ref double[] Vy) 
        {
            try
            {
                double[] elem_Phi = null;
                double[] tmpVx = null;
                double[] tmpVy = null;
                MEM.Alloc((uint)mesh.CountElements, ref tmpVx, "tmpVx");
                MEM.Alloc((uint)mesh.CountElements, ref tmpVy, "tmpVy");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // память
                    cu = knots.Length;
                    InitLocal(cu);
                    MEM.Alloc(cu, ref elem_Phi, "elem_Phi");
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    mesh.ElemValues(Phi, elem, ref elem_Phi);

                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // функции формы
                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    double dPhidx = 0;
                    double dPhidy = 0;
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        for (int ai = 0; ai < cu; ai++)
                        {
                            dPhidx += elem_Phi[ai] * ff.DN_x[ai];
                            dPhidy += elem_Phi[ai] * ff.DN_y[ai];
                        }
                    }
                    tmpVx[elem] = - dPhidy;
                    tmpVy[elem] = dPhidx;
                }
                Interpolation(ref Vx, tmpVx);
                Interpolation(ref Vy, tmpVy);
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
        public void CalcVortex(double[] Vx, double[] Vy, ref double[] Vortex)
        {
            try
            {
                double[] elem_Vx = null;
                double[] elem_Vy = null;
                double[] tmpVortex = null;
                MEM.Alloc((uint)mesh.CountElements, ref tmpVortex, "tmpVortex");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // память
                    cu = knots.Length;
                    InitLocal(cu);
                    MEM.Alloc(cu, ref elem_Vx, "elem_Vx");
                    MEM.Alloc(cu, ref elem_Vy, "elem_Vy");
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    mesh.ElemValues(Vx, elem, ref elem_Vx);
                    mesh.ElemValues(Vy, elem, ref elem_Vy);

                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // функции формы
                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    double eVortex = 0;
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        for (int ai = 0; ai < cu; ai++)
                        {
                            eVortex += (  elem_Vx[ai] * ff.DN_y[ai] 
                                        - elem_Vy[ai] * ff.DN_x[ai] );
                        }
                    }
                    tmpVortex[elem] = - eVortex;
                }
                Interpolation(ref Vortex, tmpVortex);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
    }
}
