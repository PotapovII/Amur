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
    using MeshLib;
    using CommonLib;
    using MemLogLib;

    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Решение задачи Пуассона на симплекс сетке
    /// </summary>
    [Serializable]
    public class FEPoissonTaskTri : IFEPoissonTask
    {
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int Debug { get; set; }
        /// <summary>
        /// Сетка решателя
        /// </summary>
        public IMesh Mesh { get => mesh; }
        protected IMesh mesh;
        /// <summary>
        /// Алгебра для КЭ задачи
        /// </summary>
        public IAlgebra Algebra { get=> algebra;}
        protected IAlgebra algebra;
        /// <summary>
        /// функции формы для КЭ
        /// </summary>
        public AbFunForm FForm { get; set; }
        public FEPoissonTaskTri(IMesh mesh, IAlgebra algebra)
        {
            SetTask(mesh, algebra);
        }
        public void SetTask(IMesh mesh, IAlgebra algebra)
        {
            this.mesh = mesh;
            this.algebra = algebra;
            X = mesh.GetCoords(0);
            Y = mesh.GetCoords(1);
        }
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        protected private int cu = 3;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected double[] X = null;
        protected double[] Y = null;

        protected uint[] knots = { 0, 0, 0 };
        double[] x = { 0, 0, 0 };
        double[] y = { 0, 0, 0 };
        protected double[] u = { 0, 0, 0 };
        protected double[] elem_Q = { 0, 0, 0 };
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] LaplMatrix = new double[3][] 
        {  
           new double[3]{ 0,0,0 }, 
           new double[3]{ 0,0,0 }, 
           new double[3]{ 0,0,0 } 
        };
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] LocalRight = { 0, 0, 0 };
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected uint[] adressBound = { 0, 0, 0 };
        /// <summary>
        /// координаты узлов КЭ 
        /// </summary>
        protected double[] elem_x = { 0, 0, 0 };
        protected double[] elem_y = { 0, 0, 0 };
        /// <summary>
        /// Скорсть в узлах
        /// </summary>
        protected double[] elem_U = { 0, 0, 0 };
        /// <summary>
        /// вязкость в узлах КЭ 
        /// </summary>
        protected double[] elem_mu = { 0, 0, 0 };

        double[] tmpTausZ = null;
        double[] tmpTausY = null;
        double[] Selem = null;
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public void InitLocal(int cu) { }
        /// <summary>
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        public virtual void FEPoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double Q)
        {
            uint elem = 0;
            double eddyViscosityConst;
            double[] dNdx = new double[cu];
            double[] dNdy = new double[cu];
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
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // получит вязкость в узлах
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    eddyViscosityConst = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;
                    //Площадь
                    double S = mesh.ElemSquare(elem);
                    // Градиенты от функций форм

                    dNdx[0] = (y[1] - y[2]);
                    dNdx[1] = (y[2] - y[0]);
                    dNdx[2] = (y[0] - y[1]);

                    dNdy[0] = (x[2] - x[1]);
                    dNdy[1] = (x[0] - x[2]);
                    dNdy[2] = (x[1] - x[0]);

                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (dNdx[ai] * dNdx[aj] + dNdy[ai] * dNdy[aj]) / (4 * S);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = Q * S / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                if(bv == null)
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с переменной правой частью
        /// </summary>
        public virtual void FEPoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double[] Q)
        {
            uint elem = 0;
            double[] dNdx = new double[cu];
            double[] dNdy = new double[cu];
            try
            {
                algebra.Clear();
                double mQ, eddyViscosityConst;
                // выделить память под локальные массивы
                InitLocal(cu);
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    mesh.ElementKnots(elem, ref knots);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);

                    // получить вязкость в узлах
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    eddyViscosityConst = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;
                    // получить правую часть в узлах
                    mesh.ElemValues(eddyViscosity, elem, ref elem_Q);
                    mQ = (elem_Q[0] + elem_Q[1] + elem_Q[2]) / 3;
                    //Площадь
                    double S = mesh.ElemSquare(elem);
                    // Градиенты от функций форм

                    dNdx[0] = (y[1] - y[2]);
                    dNdx[1] = (y[2] - y[0]);
                    dNdx[2] = (y[0] - y[1]);

                    dNdy[0] = (x[2] - x[1]);
                    dNdy[1] = (x[0] - x[2]);
                    dNdy[2] = (x[1] - x[0]);

                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (dNdx[ai] * dNdx[aj] + dNdy[ai] * dNdy[aj]) / (4 * S);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = mQ * S / 3;
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
        /// Интерполяция поля МКЭ
        /// </summary>
        public virtual void Interpolation(ref double[] TauZ, double[] tmpTauZ)
        {
            double S;
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                // площадь КЭ
                S = mesh.ElemSquare(elem);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = tmpTauZ[elem] * S / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
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
            double su, S;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                // получить тип и узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(i, ref knots);
                // определить количество узлов на КЭ
                int cu = knots.Length;
                //Координаты и площадь
                mesh.GetElemCoords(i, ref elem_x, ref elem_y);
                mesh.ElemValues(U, i, ref elem_U);
                su = elem_U.Sum() / elem_U.Length;
                //Площадь
                S = mesh.ElemSquare(i);
                // расход по живому сечению
                riverFlowRateCalk += S * su;
                Area += S;
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
            Area = 0;
            double su, S;
            double riverFlowRateCalk = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                mesh.ElemValues(U, i, ref elem_U);
                su = elem_U.Sum() / cu;
                S = mesh.ElemSquare(i);
                riverFlowRateCalk += S * su;
                Area += S;
            }
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void CalkTauInterpolation(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity)
        {
            try
            {
                double eddyViscosityConst;
                double S;
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausY, "tmpTausY");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausZ, "tmpTausZ");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    mesh.GetElemCoords(elem, ref x, ref y);
                    mesh.ElemValues(U, elem, ref u);
                    // получит вязкость в узлах
                    mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                    eddyViscosityConst = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;
                    //Площадь
                    S = mesh.ElemSquare(elem);
                    double Ez = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0]));
                    double Ey = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1]));
                    tmpTausZ[elem] = eddyViscosityConst * Ez / (2 * S);
                    tmpTausY[elem] = eddyViscosityConst * Ey / (2 * S);
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
        public void SolveTaus1(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity)
        {
            try
            {
                double S,eddyViscosityConst;
                MEM.Alloc((uint)mesh.CountElements, ref Selem, "Selem");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausY, "tmpTausY");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausZ, "tmpTausZ");
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] = 0;
                    TauZ[i] = 0;
                }
                for (uint i = 0; i < mesh.CountElements; i++)
                {
                    mesh.ElementKnots(i, ref knots);
                    mesh.GetElemCoords(i, ref x, ref y);
                    mesh.ElemValues(U, i, ref u);
                    // получит вязкость в узлах
                    mesh.ElemValues(eddyViscosity, i, ref elem_mu);
                    //u = GetFieldElem(U, i);
                    eddyViscosityConst = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3.0;
                    //Площадь
                    S = mesh.ElemSquare(i);
                    // деформации
                    double dN0dz = (x[2] - x[1]) / (2 * S);
                    double dN1dz = (x[0] - x[2]) / (2 * S);
                    double dN2dz = (x[1] - x[0]) / (2 * S);

                    double dN0dy = (y[1] - y[2]) / (2 * S);
                    double dN1dy = (y[2] - y[0]) / (2 * S);
                    double dN2dy = (y[0] - y[1]) / (2 * S);

                    double Ez = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0]));
                    double Ey = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1]));
                    // 
                    tmpTausZ[i] = eddyViscosityConst * Ez / (2 * S);
                    tmpTausY[i] = eddyViscosityConst * Ey / (2 * S);

                    Selem[(int)knots[0]] += S / 3;
                    Selem[(int)knots[1]] += S / 3;
                    Selem[(int)knots[2]] += S / 3;

                    TauZ[(int)knots[0]] += S * tmpTausZ[i] / 3;
                    TauZ[(int)knots[1]] += S * tmpTausZ[i] / 3;
                    TauZ[(int)knots[2]] += S * tmpTausZ[i] / 3;

                    TauY[(int)knots[0]] += S * tmpTausY[i] / 3;
                    TauY[(int)knots[1]] += S * tmpTausY[i] / 3;
                    TauY[(int)knots[2]] += S * tmpTausY[i] / 3;
                }
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] /= Selem[i];
                    TauZ[i] /= Selem[i];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void SolveTaus(ref double[] TauY, ref double[] TauZ, double[] U, double[] mu)
        {
            double S, Mu;
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            double[] tmpTausZ = new double[mesh.CountElements];
            double[] tmpTausY = new double[mesh.CountElements];
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.GetElemCoords(elem, ref x, ref y);
                mesh.ElemValues(U, elem, ref u);
                // получит вязкость в узлах
                mesh.ElemValues(mu, elem, ref elem_mu);
                Mu = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;
                //Площадь
                S = mesh.ElemSquare(elem);

                double Ez = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0]));
                double Ey = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1]));
                tmpTausZ[elem] = Mu * Ez / (2 * S);
                tmpTausY[elem] = Mu * Ey / (2 * S);
            }
            uint[] knots = { 0, 0, 0 };
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                // площадь КЭ
                S = mesh.ElemSquare(elem);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = tmpTausZ[elem] * S / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauZ);
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                // площадь КЭ
                S = mesh.ElemSquare(elem);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = tmpTausY[elem] * S / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauY);
        }

        /// <summary>
        /// Решение задачи дискретизации задачи и ее рещателя
        /// </summary>
        /// <param name="result">результат решения</param>
        public void SolveTask(ref double[] result) { }
    }

}
