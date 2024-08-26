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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                    06.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using AlgebraLib;

    /// <summary>
    /// ОО: Решение задачи Пуассона на симплекс сетке
    /// </summary>
    [Serializable]
    public class CFEPoissonTaskTri : AComplexFEMTask
    {
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        protected uint cu = 3;
        /// <summary>
        /// Список узлов для КЭ
        /// </summary>
        protected TriElement[] eKnots;
        /// <summary>
        /// Граничные КЭ
        /// </summary>
        protected TwoElement[] beKnots;

        double[] tmpTausZ = null;
        double[] tmpTausY = null;
        double[] Selem = null;
        public CFEPoissonTaskTri(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask) :
            base(wMesh, algebra, typeTask)
        {
            eKnots = mesh.GetAreaElems();
            beKnots = mesh.GetBoundElems();
        }
        public override void SetTask(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask) 
        {
            base.SetTask(wMesh, algebra, typeTask);
            eKnots = mesh.GetAreaElems();
            beKnots = mesh.GetBoundElems();
        }
        //public void CreateAlgebra()
        //{
        //    if (algebra == null)
        //    {
        //        uint NH = mesh.GetWidthMatrix();
        //        algebra = new AlgebraLUTape((uint)mesh.CountKnots, (int)NH + 1, (int)NH + 1);
        //    }
        //}

        /// <summary>
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="Q">правая часть</param>
        public virtual void PoissonTask(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double[] Q, double R_midle =0, int Ring = 0)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
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
                    double eddyViscosityConst = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];

                    double R_elem = R_midle + (X[i0] + X[i1] + X[i1]) / 3;
                    if (Ring == 1)
                    {
                        //for (int ai = 0; ai < cu; ai++)
                        //    for (int aj = 0; aj < cu; aj++)
                        //        LaplMatrix[ai][aj] += eddyViscosityConst * (b[aj] / (3.0 * R_elem)) * S[elem];
                    }
                    else
                        R_elem = 1;
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = R_elem * mQ * S[elem] / 3;
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="Q">правая часть</param>
        public virtual void PoissonTask0(ref double[] U, uint[] bc, double[] bv, double[] Q, double R_midle = 0, int Ring = 0)
        {
            int elemEx = 0;
            try
            {
                // локальная матрица часть СЛАУ
                double[][] LaplMatrix = new double[3][]
                {
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 }
                };

                algebra.Clear();
                for (int elem = 0; elem < mesh.CountElements; elem++)
                {
                    elemEx = elem;
                    // локальная правая часть СЛАУ
                    double[] LocalRight = { 0, 0, 0 };
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    uint[] knots = { i0, i1, i2 };
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;

                    if (Ring == 0)
                    {
                        // Вычисление ЛЖМ
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] = (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];
                        // Вычисление ЛПЧ
                        for (int j = 0; j < cu; j++)
                            LocalRight[j] = mQ * S[elem] / 3;
                    }
                    else
                    {
                        double R_elem = R_midle + (X[i0] + X[i1] + X[i1]) / 3;
                        double nR = R_elem / 3;
                        // Вычисление ЛЖМ
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] = (b[ai] * b[aj] + c[ai] * c[aj] + nR * c[aj]) * S[elem];
                        // Вычисление ЛПЧ
                        for (int j = 0; j < cu; j++)
                            LocalRight[j] = R_elem * mQ * S[elem] / 3;
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
                Logger.Instance.Info("Элементов обработано :" + elemEx.ToString());
            }
        }

        /// <summary>
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="Q">правая часть</param>
        public virtual void PoissonTask_Plane(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double[] Q)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
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
                    double eddyViscosityConst = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];
                    // добавление вновь сформированной ЛЖМ в ГМЖ
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="Q">правая часть</param>
        public virtual void PoissonTaskPhiTrue(ref double[] U, double[] U_old, uint[] bc, double[] bv, double[] Q, double R_midle = 0, int Ring = 0)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
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
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];

                    double R_elem = R_midle + (X[i0] + X[i1] + X[i1]) / 3;
                    //if (Ring == 1)
                    //{
                    //    for (int ai = 0; ai < cu; ai++)
                    //        for (int aj = 0; aj < cu; aj++)
                    //            LaplMatrix[ai][aj] += (b[aj] / (3.0 * R_elem));
                    //}
                    //else
                    //    R_elem = 1;
                    double phi_old =  (U_old[i0] + U_old[i1] + U_old[i1]) / 3;
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = R_elem * mQ * S[elem] / 3 - phi_old * b[j] / (3.0 * R_elem);
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="Q">правая часть</param>
        public virtual void PoissonTaskBack(ref double[] U, double[] eddyViscosity, uint[] bc, double[] bv, double[] Q, double R_midle = 0, int Ring = 0)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
                //OrderablePartitioner<Tuple<int, int>> OrdPartitioner_Tau = Partitioner.Create(0, mesh.CountElements);
                //Parallel.ForEach(OrdPartitioner_Tau,
                //       (range, loopState) =>
                //       {
                //           for (int elem = range.Item1; elem < range.Item2; elem++)
                //           { }
                //       });

                //Parallel.For(0, mesh.CountElements, (elem, state) =>
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
                    double eddyViscosityConst = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    double R_elem = R_midle + (X[i0] + X[i1] + X[i1]) / 3;
                    if (Ring == 0)
                        R_elem = 1;
                    
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem] / R_elem;
                    
                    if (Ring == 1)
                    {
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += eddyViscosityConst * (b[aj] / (3.0 * R_elem* R_elem));
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = mQ * S[elem] / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                // });
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="Q">правая часть</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="bec">адреса ГУ 2 рода/поток на границе</param>
        /// <param name="bev">значения ГУ 2 рода/поток на границе</param>
        public virtual void PoissonTask(ref double[] U, double[] eddyViscosity, double[] Q,
            uint[] bc, double[] bv, uint[] bec, double[] bev)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
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
                    double eddyViscosityConst = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = mQ * S[elem] / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                for (int i = 0; i < bec.Length; i++)
                {
                    uint idx = bec[i];
                    double uy = bev[i];
                    uint i0 = beKnots[idx].Vertex1;
                    uint i1 = beKnots[idx].Vertex2;
                    uint[] knots = { i0, i1};
                    double L = Math.Abs(X[i0]-X[i1]);
                    double[] LblRight = new double[] { uy * L / 2, uy * L / 2 };
                    algebra.AddToRight(LblRight, knots);
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
        /// Нахождение поля скоростей из решения задачи Пуассона МКЭ с постоянной правой частью
        /// </summary>
        /// <param name="U">искомая функция</param>
        /// <param name="eddyViscosity">диффузия</param>
        /// <param name="Q">правая часть</param>
        /// <param name="bc">адреса ГУ 1 рода</param>
        /// <param name="bv">значения ГУ 1 рода</param>
        /// <param name="bec">адреса ГУ 2 рода/поток на границе</param>
        /// <param name="bev">значения ГУ 2 рода/поток на границе</param>
        public virtual void PoissonTask(ref double[] U, double[] eddyViscosity, double[] Q,
            uint[] bc, double[] bv, uint[] bec, double[][] bev)
        {
            int elemEx = 0;
            try
            {
                algebra.Clear();
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
                    double eddyViscosityConst = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double mQ = (Q[i0] + Q[i1] + Q[i2]) / 3;
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = eddyViscosityConst * (b[ai] * b[aj] + c[ai] * c[aj]) * S[elem];
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = mQ * S[elem] / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                for (int i = 0; i < bec.Length; i++)
                {
                    uint idx = bec[i];
                    uint i0 = eKnots[idx].Vertex1;
                    uint i1 = eKnots[idx].Vertex2;
                    uint[] knots = { i0, i1 };
                    double L = Math.Sqrt((X[i0] - X[i1]) * (X[i0] - X[i1]) + (Y[i0] - Y[i1]) * (Y[i0] - Y[i1]));
                    double q1 = (2 * bev[idx][0] + bev[idx][1]) * L / 6;
                    double q2 = (bev[idx][0] + 2 * bev[idx][1]) * L / 6;
                    double[] LblRight = new double[] { q1, q2 };
                    algebra.AddToRight(LblRight, knots);
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
        /// Интерполяция поля МКЭ
        /// </summary>
        public virtual void Interpolation(ref double[] TauZ, double[] tmpTauZ)
        {
            algebra.Clear();
            //Parallel.For(0, mesh.CountElements, (elem, state) =>
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // локальная матрица часть СЛАУ
                double[][] LaplMatrix = new double[3][]
                {
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 }
                };
                // локальная правая часть СЛАУ
                double[] LocalRight = { 0, 0, 0 };
                uint[] knots = {
                    eKnots[elem].Vertex1, eKnots[elem].Vertex2, eKnots[elem].Vertex3
                };
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S[elem] / 12.0;
                        else
                            LaplMatrix[ai][aj] = S[elem] / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = tmpTauZ[elem] * S[elem] / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }// });
            algebra.Solve(ref TauZ);
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
            double area = 0;
            double riverFlowRateCalk = 0;
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            //    Parallel.For(0, mesh.CountElements, (elem, state) =>
            {
                double mU = (U[eKnots[elem].Vertex1] +
                             U[eKnots[elem].Vertex2] +
                             U[eKnots[elem].Vertex3]) / cu;
                // расход по живому сечению
                riverFlowRateCalk += S[elem] * mU;
                area += S[elem];
            }// });
            Area = area;
            if (double.IsNaN(riverFlowRateCalk) == true)
                throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public void CalkTauInterpolation(ref double[] TauY, ref double[] TauZ, double[] U, double[] eddyViscosity)
        {
            try
            {
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausY, "tmpTausY");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausZ, "tmpTausZ");
                // Parallel.For(0, mesh.CountElements, (elem, state) =>
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double eddyVis = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    tmpTausZ[elem] = eddyVis * (U[i0] * b[0] + U[i1] * b[1] + U[i2] * b[2]);
                    tmpTausY[elem] = eddyVis * (U[i0] * c[0] + U[i1] * c[1] + U[i2] * c[2]);
                }//);
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
            try
            {
                double[] tauY = TauY; 
                double[] tauZ = TauZ;
                MEM.Alloc((uint)mesh.CountKnots, ref tauY, "tauY");
                MEM.Alloc((uint)mesh.CountKnots, ref tauZ, "tauZ");

                MEM.Alloc((uint)mesh.CountElements, ref Selem, "Selem");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausY, "tmpTausY");
                MEM.Alloc((uint)mesh.CountElements, ref tmpTausZ, "tmpTausZ");
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] = 0;
                    TauZ[i] = 0;
                }
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                //    Parallel.For(0, mesh.CountElements, (elem, state) =>
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double eddyVis = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    double S3 = S[elem];
                    tmpTausZ[elem] = eddyVis * (U[i0] * b[0] + U[i1] * b[1] + U[i2] * b[2]) * S[elem];
                    tmpTausY[elem] = eddyVis * (U[i0] * c[0] + U[i1] * c[1] + U[i2] * c[2]) * S[elem];

                    Selem[i0] += S3;
                    Selem[i1] += S3;
                    Selem[i2] += S3;

                    tauZ[i0] += tmpTausZ[elem] / 3;
                    tauZ[i1] += tmpTausZ[elem] / 3;
                    tauZ[i2] += tmpTausZ[elem] / 3;

                    tauY[i0] += tmpTausY[elem] / 3;
                    tauY[i1] += tmpTausY[elem] / 3;
                    tauY[i2] += tmpTausY[elem] / 3;
                }//);
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] = tauY[i]/Selem[i];
                    TauZ[i] = tauZ[i]/Selem[i];
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
        public void CalcVelosity_Plane(double[] Phi, ref double[] Vx, ref double[] Vy)
        {
            try
            {
                double[] tmpVx = null;
                double[] tmpVy = null;
                MEM.Alloc((uint)mesh.CountElements, ref tmpVx, "tmpVx");
                MEM.Alloc((uint)mesh.CountElements, ref tmpVy, "tmpVy");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dPhidx = Phi[i0] * b[0] + Phi[i1] * b[1] + Phi[i2] * b[2];
                    double dPhidy = Phi[i0] * c[0] + Phi[i1] * c[1] + Phi[i2] * c[2];
                    tmpVx[elem] =   dPhidy;
                    tmpVy[elem] = - dPhidx;
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
        public void CalcVelosity(double[] Phi, ref double[] Vx, ref double[] Vy, double R_midle = 0, int Ring = 0)
        {
            try
            {
                double[] tmpVx = null;
                double[] tmpVy = null;
                MEM.Alloc((uint)mesh.CountElements, ref tmpVx, "tmpVx");
                MEM.Alloc((uint)mesh.CountElements, ref tmpVy, "tmpVy");
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double dPhidx = Phi[i0] * b[0] + Phi[i1] * b[1] + Phi[i2] * b[2];
                    double dPhidy = Phi[i0] * c[0] + Phi[i1] * c[1] + Phi[i2] * c[2];
                    if (Ring == 0)
                    {
                        tmpVx[elem] = dPhidy;
                        tmpVy[elem] = -dPhidx;
                    }
                    else
                    {
                        double R_elem = R_midle + (X[i0] + X[i1] + X[i1]) / 3;
                        tmpVx[elem] = dPhidy / R_elem;
                        tmpVy[elem] = -dPhidx / R_elem;
                    }
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
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        /// <param name="Ux">скорость в створе</param>
        /// <param name="eddyViscosity">вязкость в створе</param>
        /// <param name="Local">вид интерполяции с елементов в узлы</param>
        public void Calk_TauXY_TauXZ(ref double[] TauXY, ref double[] TauXZ, double[] Ux, double[] eddyViscosity, bool Local = true)
        {
            try
            {
                double[] eTauXY = null;
                double[] eTauXZ = null;
                MEM.Alloc((uint)mesh.CountElements, ref eTauXY, "eTauXY");
                MEM.Alloc((uint)mesh.CountElements, ref eTauXZ, "eTauXZ");
                MEM.Alloc((uint)mesh.CountKnots, ref TauXY, "eTauXY");
                MEM.Alloc((uint)mesh.CountKnots, ref TauXZ, "eTauXZ");
                // Parallel.For(0, mesh.CountElements, (elem, state) =>
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    double eddyVis = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / 3;
                    eTauXY[elem] = eddyVis * (Ux[i0] * b[0] + Ux[i1] * b[1] + Ux[i2] * b[2]);
                    eTauXZ[elem] = eddyVis * (Ux[i0] * c[0] + Ux[i1] * c[1] + Ux[i2] * c[2]);
                }//);
                if (Local == true)
                {
                    wMesh.ConvertField(ref TauXY, eTauXY);
                    wMesh.ConvertField(ref TauXZ, eTauXZ);
                }
                else
                {
                    Interpolation(ref TauXY, eTauXY);
                    Interpolation(ref TauXZ, eTauXZ);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

    }

}
