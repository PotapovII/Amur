//---------------------------------------------------------------------------
//      Класс TPSolver предназначен для решения САУ - прогонкой 
//                              Потапов И.И.
//                        - (C) Copyright 2015 -
//                          ALL RIGHT RESERVED
//                               31.07.15
//---------------------------------------------------------------------------
//   Реализация библиотеки для решения задачи турбулентного теплообмена
//                   методом контрольного объема
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using System;
    using MemLogLib;
    /// <summary>
    /// Tri-Diagonal Matrix Algorithm
    /// </summary>
    [Serializable]
    public class TPSolver : ITPSolver
    {
        int ijmax = 0;
        /// <summary>
        /// вектор верхней кодиагонали
        /// </summary>
        double[] C = null;
        /// <summary>
        /// вектор правой части
        /// </summary>
        double[] R = null;
        /// <summary>
        ///  Значение i для последнего контрольного объема в направлении Х
        /// </summary>
        protected int imax;
        /// <summary>
        /// Значение j для последнего контрольного объема в направлении Y
        /// </summary>
        protected int jmax;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="imax">Значение i для последнего КО по Х</param>
        /// <param name="jmax">Значение j для последнего КО по Y</param>
        public TPSolver(int imax, int jmax)
        {
            this.imax = imax;
            this.jmax = jmax;
            ijmax = Math.Max(imax, jmax);
            C = new double[ijmax];
            R = new double[ijmax];
        }
        /// <summary>
        /// Метод техдиагональной матричной прогонки 
        /// </summary>
        /// <param name="ist">смещение сетки по х</param>
        /// <param name="jst">смещение сетки по х</param>
        /// <param name="Ae">Коэффициенты дискретной схемы - восток</param>
        /// <param name="Aw">Коэффициенты дискретной схемы - запад</param>
        /// <param name="An">Коэффициенты дискретной схемы - север</param>
        /// <param name="As">Коэффициенты дискретной схемы - юг</param>
        /// <param name="Ap">Коэффициенты дискретной схемы - центр</param>
        /// <param name="sc">Правая часть</param>
        /// <param name="X">Решение</param>
        public bool OnTDMASolver(int ist, int jst, int imax, int jmax,
             double[][] Ae, double[][] Aw, double[][] An, double[][] As, double[][] Ap,
             double[][] sc, BCond bc, ref double[][] X, int CountIter = 1)
        {
            double denom, temp;
            int i, j;
            //LOG.Print("X0", X, 4);

            //LOG.Print("Right", sc, 4);
            //LOG.Print("Ap", Ap, 4);
            //LOG.Print("Ae", Ae, 4);
            //LOG.Print("Aw", Aw, 4);
            //LOG.Print("An", An, 4);
            //LOG.Print("As", As, 4);
            if(ERR.INF_NAN("X",X) == false)
            {
                for (j = jst; j < jmax; j++)
                    for (i = ist; i < imax; i++)
                        if (double.IsInfinity(X[i][j]) == true ||
                            double.IsNaN(X[i][j]) == true)
                                X[i][j] = 0;
            }

            CountIter = 1;
            for (int iter = 0; iter < CountIter; iter++)
            {
                // 
                for (j = jst; j < jmax; j++)
                {
                    // прямой ход для вычисления коэффициентов C и R для j столбца
                    C[ist - 1] = 0;
                    R[ist - 1] = X[ist - 1][j];
                    for (i = ist; i < imax; i++)
                    {
                        denom = Ap[i][j] - Aw[i][j] * C[i - 1];
                        C[i] = Ae[i][j] / denom;
                        temp = sc[i][j] + An[i][j] * X[i][j + 1] + As[i][j] * X[i][j - 1];
                        R[i] = (temp + Aw[i][j] * R[i - 1]) / denom;
                    }
                    // обратный ход
                    for (i = imax - 1; i > ist - 1; i--)
                        X[i][j] = X[i + 1][j] * C[i] + R[i];
                }
                for (j = jmax - 1; j > jst - 1; j--)
                {
                    C[ist - 1] = 0;
                    R[ist - 1] = X[ist - 1][j];
                    for (i = ist; i < imax; i++)
                    {
                        denom = Ap[i][j] - Aw[i][j] * C[i - 1];
                        C[i] = Ae[i][j] / denom;
                        temp = sc[i][j] + An[i][j] * X[i][j + 1] + As[i][j] * X[i][j - 1];
                        R[i] = (temp + Aw[i][j] * R[i - 1]) / denom;
                    }
                    for (i = imax - 1; i > ist - 1; i--)
                        X[i][j] = X[i + 1][j] * C[i] + R[i];
                }
                for (i = ist; i < imax; i++)
                {
                    C[jst - 1] = 0;
                    R[jst - 1] = X[i][jst - 1];
                    for (j = jst; j < jmax; j++)
                    {
                        denom = Ap[i][j] - As[i][j] * C[j - 1];
                        C[j] = An[i][j] / denom;
                        temp = sc[i][j] + Ae[i][j] * X[i + 1][j] + Aw[i][j] * X[i - 1][j];
                        R[j] = (temp + As[i][j] * R[j - 1]) / denom;
                    }
                    for (j = jmax - 1; j > jst - 1; j--)
                        X[i][j] = X[i][j + 1] * C[j] + R[j];

                }
                for (i = imax - 1; i > ist - 1; i--)
                {
                    C[jst - 1] = 0;
                    R[jst - 1] = X[i][jst - 1];
                    for (j = jst; j < jmax; j++)
                    {
                        denom = Ap[i][j] - As[i][j] * C[j - 1];
                        C[j] = An[i][j] / denom;
                        temp = sc[i][j] + Ae[i][j] * X[i + 1][j] + Aw[i][j] * X[i - 1][j];
                        R[j] = (temp + As[i][j] * R[j - 1]) / denom;
                    }
                    for (j = jmax - 1; j > jst - 1; j--)
                        X[i][j] = X[i][j + 1] * C[j] + R[j];
                }
            }
            // LOG.Print("X", X, 4);
            return true;
        }
    }

}