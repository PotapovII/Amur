﻿//---------------------------------------------------------------------------
//      Класс TPSolver предназначен для решения САУ - прогонкой 
//                        - (C) Copyright 2021 -
//                          ALL RIGHT RESERVED
//                        разработка: Потапов И.И.
//                               23.07.21
//---------------------------------------------------------------------------
//   Реализация библиотеки для решения задачи турбулентного теплообмена
//                   методом контрольного объема
//          Решение САУ методом сопряженных градиентов C#
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using System.Threading.Tasks;
    [Serializable]
    public class ParrallelPAlgebraCG : ITPSolver
    {
        /// <summary>
        /// смещение сетки по y
        /// </summary>
        protected int ist;
        /// <summary>
        /// смещение сетки по x
        /// </summary>
        protected int jst;
        /// <summary>
        /// смещение сетки по y
        /// </summary>
        protected int imax;
        /// <summary>
        /// смещение сетки по x
        /// </summary>
        protected int jmax;
        int jend;
        int iend;
        /// <summary>
        /// Коэффициенты дискретной схемы - восток
        /// </summary>
        protected double[][] Ae;
        /// <summary>
        /// Коэффициенты дискретной схемы - запад
        /// </summary>
        protected double[][] Aw;
        /// <summary>
        /// Коэффициенты дискретной схемы - север
        /// </summary>
        protected double[][] An;
        /// <summary>
        /// Коэффициенты дискретной схемы - юг
        /// </summary>
        protected double[][] As;
        /// <summary>
        /// Коэффициенты дискретной схемы - центр
        /// </summary>
        protected double[][] Ap;
        /// <summary>
        /// Правая часть
        /// </summary>
        protected double[][] Right;
        /// <summary>
        /// Граничные условия
        /// </summary>
        BCond bc = null;
        /// <summary>
        /// Решение
        /// </summary>
        protected double[][] X;
        /// <summary>
        /// вектор ошибки
        /// </summary>
        protected double[][] R;
        /// <summary>
        /// сопряженный вектор
        /// </summary>
        protected double[][] P;
        /// <summary>
        /// произведение сопряженного вектора на матрицу
        /// </summary>
        protected double[][] AP;

        protected double Alpha, Betta;
        public ParrallelPAlgebraCG(int imax = 0, int jmax = 0)
        {
            this.imax = imax;
            this.jmax = jmax;
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
            ist--;
            jst--;
            imax++;
            jmax++;
            this.imax = imax;
            this.jmax = jmax;
            jend = jmax - 1;
            iend = imax - 1;
            this.ist = ist;
            this.jst = jst;
            this.Ae = Ae;
            this.Aw = Aw;
            this.An = An;
            this.As = As;
            this.Ap = Ap;
            this.Right = sc;
            this.bc = bc;
            this.X = X;
            double Alpha, Betta, Rho, pAp;
            // начальное приближение
            R = MEM.PAlloc2DClear(imax, jmax, R);
            P = MEM.PAlloc2DClear(imax, jmax, P);
            AP = MEM.PAlloc2DClear(imax, jmax, AP);
            X = SetBCondition(X);         // X => X0 - начальное условие
            R = GetResidual(R, X);   // R = A*X0
                                     // R0 = A*X0 - F             // вектор ошибки
            Parallel.For(ist, imax, (i, state) =>
            {
                for (int j = jst; j < jmax; j++)
                {
                    R[i][j] -= Right[i][j];
                    // начальное приближение
                    P[i][j] = R[i][j];   // P0 = R0
                }
            });
            //for (int i = ist; i < imax; i++)
            //    for (int j = jst; j < jmax; j++)
            //    {
            //        R[i][j] -= Right[i][j];
            //        // начальное приближение
            //        P[i][j] = R[i][j];   // P0 = R0
            //    }
            //LOG.Print("R", R, 4);
            //LOG.Print("P", P, 4);
            // цикл по сходимости
            int MaxIter = 2 * jmax * imax;
            int iters;
            for (iters = 0; iters < MaxIter; iters++)
            {
                AP = GetResidual(AP, P);  // AP = A * P
                                          // граничные условия
                                          // AP && BCond
                AP = SetBConditionAP(AP);
                //for (int i = ist; i < imax; i++)
                //    for (int j = jst; j < jmax; j++)
                //        if (BC[i][j] == 1)
                //            AP[i][j] = 0;
                // минимизация ошибки решения по текущему направлению
                Rho = MultyVector(R, R);
                pAp = MultyVector(P, AP);
                if (pAp <= Rho && Rho < MEM.Error10)
                    break;
                Alpha = -Rho / pAp;
                // Модификация вектора решения и вектора ошибки
                // Проверка сходимости
                bool flag = true;
                X = CalcX(ref flag, X, Alpha);
                if (flag == true)
                    break;
                // поправка ошибки
                Parallel.For(ist, imax, (i, state) =>
                {
                    for (int j = jst; j < jmax; j++)
                        R[i][j] += Alpha * AP[i][j];
                });
                // расчет нового сопряженного направления
                Betta = MultyVector(R, R) / Rho;
                // новый вектор направлений                
                Parallel.For(ist, imax, (i, state) =>
                {
                    for (int j = jst; j < jmax; j++)
                        P[i][j] = R[i][j] + Betta * P[i][j];
                });
                //LOG.Print("R", R, 4);
                //LOG.Print("P", P, 4);
            }
            LOG.Print("X", X, 4);
            if (iters < MaxIter)
                return true;
            else
                return false;
        }
        protected double[][] CalcX(ref bool flag, double[][] X, double Al)
        {
            double ck = 0;
            double ax = 0;
            double error;
            Parallel.For(ist, imax, (i, state) =>
            {
                //   for (int i = ist; i < imax; i++)
                for (int j = jst; j < jmax; j++)
                {
                    X[i][j] += Al * P[i][j];
                    if (ck < Math.Abs(Al * P[i][j]))
                        ck = Math.Abs(Al * P[i][j]);
                    if (ax < Math.Abs(X[i][j]))
                        ax = Math.Abs(X[i][j]);
                }
            });
            error = ck / ax;
            if (ax > 0)
            {
                if (error < MEM.Error8)
                {
                    flag = true;
                    return X;

                }
            }
            else
               if (ck < MEM.Error8)
            {
                flag = true;
                return X;
            }
            flag = false;
            return X;
        }

        /// <summary>
        /// Операция умножения вектора на матрицу
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        public double[][] GetResidual(double[][] R, double[][] X, int IsRight = 1)
        {
            // Расчет вертикальных коэффициентов As[i][j], An[i][j]
            //             1       imax  
            //i=0,j=0|-------------|--> i x
            //       |             |
            //       |             |
            //       | 0         0 |
            //       |             |     Ae[i][j] = Aw[i+1][j] - flow
            //       |             |     An[i][j] = As[i][j+1] - flow
            //       |             |
            //       |-------------|
            //       | y    1  
            //       V j          
            //     
            // int i, j;
            R = MEM.PAlloc2DClear(imax, jmax, R);

            Parallel.For(ist + 1, iend, (ii, state) =>
            {
                for (int jj = jst + 1; jj < jend; jj++)
                {
                    R[ii][jj] = Ap[ii][jj] * X[ii][jj]
                              - Ae[ii][jj] * X[ii + 1][jj]
                              - Aw[ii][jj] * X[ii - 1][jj]
                              - An[ii][jj] * X[ii][jj + 1]
                              - As[ii][jj] * X[ii][jj - 1];
                }
            });
            //             1       imax  
            //i=0,j=0|-------------|--> i x
            //       |      |      |
            //       |      |      |
            //       |             |
            //       |             |
            //       |             |
            //       |             |
            //       |-------------|
            //       | y    1  
            //       V j          
            int j = jst;
            Parallel.For(ist + 1, iend, (ii, state) =>
            {
                R[ii][j] = Ap[ii][j] * X[ii][j]
                          - Ae[ii][j] * X[ii + 1][j]
                          - Aw[ii][j] * X[ii - 1][j]
                          - An[ii][j] * X[ii][j + 1];
                //- As[ii][j] * X[ii][j - 1];
            });
            //             1       imax  
            //i=0,j=0|-------------|--> i x
            //       |             |
            //       |             |
            //       |             |
            //       |             |
            //       |      |      |
            //       |      |      |
            //       |-------------|
            //       | y    1  
            //       V j          
            j = jmax - 1;
            Parallel.For(ist + 1, iend, (ii, state) =>
            {
                //for (i = ist + 1; i < imax - 1; i++)
                //{
                R[ii][j] = Ap[ii][j] * X[ii][j]
                          - Ae[ii][j] * X[ii + 1][j]
                          - Aw[ii][j] * X[ii - 1][j]
                          //- An[ii][j] * X[ii][j + 1]
                          - As[ii][j] * X[ii][j - 1];
            });
            int i = ist;
            Parallel.For(jst + 1, jend, (ii, state) =>
            {
                //for (ii = jst + 1; ii < jmax - 1; ii++)
                //{
                R[i][ii] = Ap[i][ii] * X[i][ii]
                        - Ae[i][ii] * X[i + 1][ii]
                        //- Aw[i][ii] * X[i - 1][ii]
                        - An[i][ii] * X[i][ii + 1]
                        - As[i][ii] * X[i][ii - 1];
            });
            i = imax - 1;
            Parallel.For(jst + 1, jend, (ii, state) =>
            {
                //for (ii = jst + 1; ii < jmax - 1; ii++)
                //{
                R[i][ii] = Ap[i][ii] * X[i][ii]
                          //-  Ae[i][ii] * X[i + 1][ii]
                          - Aw[i][ii] * X[i - 1][ii]
                          - An[i][ii] * X[i][ii + 1]
                          - As[i][ii] * X[i][ii - 1];
            });
            i = ist;
            j = jst;
            R[i][j] = Ap[i][j] * X[i][j]
                    - Ae[i][j] * X[i + 1][j]
                    //- Aw[i][j] * X[i - 1][j]
                    - An[i][j] * X[i][j + 1];
            //- As[i][j] * X[i][j - 1];

            i = ist;
            j = jmax - 1;
            R[i][j] = Ap[i][j] * X[i][j]
                        - Ae[i][j] * X[i + 1][j]
                      //- Aw[i][j] * X[i - 1][j]
                      //- An[i][j] * X[i][j + 1]
                      - As[i][j] * X[i][j - 1];

            i = imax - 1;
            j = jst;
            R[i][j] = Ap[i][j] * X[i][j]
                      //- Ae[i][j] * X[i + 1][j]
                      - Aw[i][j] * X[i - 1][j]
                      - An[i][j] * X[i][j + 1];
            //- As[i][j] * X[i][j - 1];
            i = imax - 1;
            j = jmax - 1;
            R[i][j] = Ap[i][j] * X[i][j]
                      //- Ae[i][j] * X[i + 1][j]
                      - Aw[i][j] * X[i - 1][j]
                      //- An[i][j] * X[i][j + 1]
                      - As[i][j] * X[i][j - 1];

            //LOG.Print("X", X, 4);
            //LOG.Print("Right", Right, 4);
            //LOG.Print("R", R, 4);
            //LOG.Print("Ap", Ap, 4);
            //LOG.Print("Ae", Ae, 4);
            //LOG.Print("Aw", Aw, 4);
            //LOG.Print("An", An, 4);
            //LOG.Print("As", As, 4);
            return R;
        }

        /// <summary>
        /// Установка граничных условий
        /// </summary>
        /// <param name="Value"></param>
        protected double[][] SetBConditionAP(double[][] AP)
        {
            Parallel.For(ist, imax, (ii, state) =>
            {
                if (bc.BC[BCond.S_index][ii] == 1)
                    AP[ii][jst] = 0;
                if (bc.BC[BCond.N_index][ii] == 1)
                    AP[ii][jend] = 0;
            });
            Parallel.For(jst, jmax, (ii, state) =>
            {
                if (bc.BC[BCond.W_index][ii] == 1)
                    AP[ist][ii] = 0;
                if (bc.BC[BCond.E_index][ii] == 1)
                    AP[iend][ii] = 0;
            });
            return AP;
        }
        /// <summary>
        /// Установка граничных условий
        /// </summary>
        /// <param name="Value"></param>
        protected double[][] SetBCondition(double[][] X)
        {
            int i, j;
            // Расчет вертикальных коэффициентов As[i][j], An[i][j]
            //              1      imax  
            //i=0,j=0|-------------|--> i x
            //       |      S      |
            //       |             |
            //       | 0         0 |
            //    W  |             | E    Ae[i][j] = Aw[i+1][j] - flow
            //       |             |      An[i][j] = As[i][j+1] - flow
            //       |      N      |
            //       |-------------|
            //       | y    1  
            //       V j          
            j = jst;
            Parallel.For(ist, imax, (ii, state) =>
            {
                if (bc.BC[BCond.S_index][ii] == 1)
                {
                    Right[ii][j] = bc.Value[BCond.S_index][ii];
                    X[ii][j] = bc.Value[BCond.S_index][ii];
                    Ap[ii][j] = 1;
                    Ae[ii][j] = 0;
                    Aw[ii][j] = 0;
                    An[ii][j] = 0;
                }
            });
            //LOG.Print("Right", Right, 4);
            j = jmax - 1;
            Parallel.For(ist, imax, (ii, state) =>
            {
                if (bc.BC[BCond.N_index][ii] == 1)
                {
                    Right[ii][j] = bc.Value[BCond.N_index][ii];
                    X[ii][j] = bc.Value[BCond.N_index][ii];
                    Ap[ii][j] = 1;
                    Ae[ii][j] = 0;
                    Aw[ii][j] = 0;
                    As[ii][j] = 0;
                }
            });
            i = ist;
            Parallel.For(jst, jmax, (ii, state) =>
            {
                if (bc.BC[BCond.W_index][ii] == 1)
                {
                    Right[i][ii] = bc.Value[BCond.W_index][ii];
                    X[i][ii] = bc.Value[BCond.W_index][ii];
                    Ap[i][ii] = 1;
                    Ae[i][ii] = 0;
                    An[i][ii] = 0;
                    As[i][ii] = 0;
                }
            });
            i = imax - 1;
            Parallel.For(jst, jmax, (ii, state) =>
            {
                if (bc.BC[BCond.E_index][ii] == 1)
                {
                    Right[i][ii] = bc.Value[BCond.E_index][ii];
                    X[i][ii] = bc.Value[BCond.E_index][ii];
                    Ap[i][ii] = 1;
                    Aw[i][ii] = 0;
                    An[i][ii] = 0;
                    As[i][ii] = 0;
                }
            });
            //LOG.Print("X", X, 4);
            //LOG.Print("Right", Right, 4);
            //LOG.Print("Ap", Ap, 4);
            //LOG.Print("Ae", Ae, 4);
            //LOG.Print("Aw", Aw, 4);
            //LOG.Print("An", An, 4);
            //LOG.Print("As", As, 4);
            return X;
        }

        /// <summary>
        /// Скалярное произведение векторов
        /// </summary>
        protected double MultyVector(double[][] a, double[][] b)
        {
            double Sum = 0;
            //Parallel.For(ist, imax, (i, state) =>
            //{
            //for (int i = ist; i < imax; i++)
            //   for (int j = jst; j < jmax; j++)
            //       Sum += a[i][j] * b[i][j];
            //});

            Parallel.For(ist, imax, (i, state) =>
            {
                // for (int i = ist; i < imax; i++)
                for (int j = jst; j < jmax; j++)
                    Sum += a[i][j] * b[i][j];
            });

            return Sum;
        }

        public static void Test()
        {
            int ist = 0;
            int jst = 0;
            int imax = 10;
            int jmax = 12;
            double[][] Ae = null;
            double[][] Aw = null;
            double[][] An = null;
            double[][] As = null;
            double[][] Ap = null;
            double[][] Right = null;
            double[][] X = null;
            //int[][] BC = null;
            //double[][] BCValue = null;
            MEM.Alloc2DClear(imax, jmax, ref Ae);
            MEM.Alloc2DClear(imax, jmax, ref Aw);
            MEM.Alloc2DClear(imax, jmax, ref Ap);
            MEM.Alloc2DClear(imax, jmax, ref An);
            MEM.Alloc2DClear(imax, jmax, ref As);
            MEM.Alloc2DClear(imax, jmax, ref Right);
            MEM.Alloc2DClear(imax, jmax, ref X);
            //BC = new int[4][];
            //BC[0] = new int[imax];
            //BC[1] = new int[imax];
            //BC[2] = new int[jmax];
            //BC[3] = new int[jmax];
            //BCValue = new double[4][];
            //BCValue[0] = new double[imax];
            //BCValue[1] = new double[imax];
            //BCValue[2] = new double[jmax];
            //BCValue[3] = new double[jmax];
            //MEM.Alloc2DClear(imax, jmax, ref BC);
            //MEM.Alloc2DClear(imax, jmax, ref BCValue);
            //             1       imax  
            //i=0,j=0|-------------|--> i x
            //       |             |
            //       |             |
            //       | 0         0 |
            //       |             |     Ae[i][j] ~ Aw[i+1][j] 
            //       |             |     An[i][j] ~ As[i][j+1]
            //       |             |
            //       |-------------|
            //       | y    1  
            //       V j          
            BCond bc = new BCond(imax, jmax);
            //bc.BC = BC;
            //bc.Value = BCValue;
            int i, j;
            for (i = ist; i < imax; i++)
            {
                bc.BC[0][i] = 1; // однородные ГУ скорости равны нулю 
                bc.Value[0][i] = 3;
                bc.BC[1][i] = 1; // однородные ГУ скорости равны нулю 
                bc.Value[1][i] = 3;
            }
            for (i = jst; i < jmax; i++)
            {
                bc.BC[2][i] = 1; // однородные ГУ скорости равны нулю 
                bc.Value[2][i] = 4;
                bc.BC[3][i] = 1; // однородные ГУ скорости равны нулю 
                bc.Value[3][i] = 4;
            }


            for (i = ist; i < imax; i++)
                for (j = jst; j < jmax; j++)
                {
                    Ap[i][j] = 4;
                    Ae[i][j] = -1;
                    Aw[i][j] = -1;
                    An[i][j] = -1;
                    As[i][j] = -1;
                    Right[i][j] = 1;
                }
            for (i = ist; i < imax; i++)
            {
                Ap[i][jst] = 3;
                Ap[i][jmax - 1] = 3;
            }
            for (j = jst; j < jmax; j++)
            {
                Ap[ist][j] = 3;
                Ap[imax - 1][j] = 3;
            }
            Ap[ist][jst] = 2;
            Ap[ist][jmax - 1] = 2;
            Ap[imax - 1][jst] = 2;
            Ap[imax - 1][jmax - 1] = 2;
            PAlgebraCG a = new PAlgebraCG();
            a.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, Right, bc, ref X);
            LOG.Print("X", X, 4);
        }



        public static void Test1()
        {
            int ist = 1;
            int jst = 1;
            int i0 = ist - 1;
            int j0 = jst - 1;
            int imax = 300;
            int jmax = 200;
            int Nx = imax + 1;
            int Ny = jmax + 1;
            double[][] Ae = null;
            double[][] Aw = null;
            double[][] An = null;
            double[][] As = null;
            double[][] Ap = null;
            double[][] Right = null;
            double[][] X = null;
            //int[][] BC = null;
            //double[][] BCValue = null;
            MEM.Alloc2DClear(Nx, Ny, ref Ae);
            MEM.Alloc2DClear(Nx, Ny, ref Aw);
            MEM.Alloc2DClear(Nx, Ny, ref Ap);
            MEM.Alloc2DClear(Nx, Ny, ref An);
            MEM.Alloc2DClear(Nx, Ny, ref As);
            MEM.Alloc2DClear(Nx, Ny, ref Right);
            MEM.Alloc2DClear(Nx, Ny, ref X);
            //BC = new int[4][];
            //BC[0] = new int[imax];
            //BC[1] = new int[imax];
            //BC[2] = new int[jmax];
            //BC[3] = new int[jmax];
            //BCValue = new double[4][];
            //BCValue[0] = new double[imax];
            //BCValue[1] = new double[imax];
            //BCValue[2] = new double[jmax];
            //BCValue[3] = new double[jmax];
            //MEM.Alloc2DClear(imax, jmax, ref BC);
            //MEM.Alloc2DClear(imax, jmax, ref BCValue);
            //             1       imax  
            //i=0,j=0|-------------|--> i x
            //       |             |
            //       |             |
            //       | 0         0 |
            //       |             |     Ae[i][j] ~ Aw[i+1][j] 
            //       |             |     An[i][j] ~ As[i][j+1]
            //       |             |
            //       |-------------|
            //       | y    1  
            //       V j          
            int i, j;
            #region ГУ
            BCond bc = new BCond(Nx, Ny);
            for (i = i0; i < Nx; i++)
            {
                bc.BC[0][i] = 1; // однородные ГУ скорости равны нулю 
                bc.Value[0][i] = 3;
                bc.BC[1][i] = 1; // однородные ГУ скорости равны нулю 
                bc.Value[1][i] = 6;
            }
            //for (i = j0; i < Ny; i++)
            //{
            //    bc.BC[2][i] = 1; // однородные ГУ скорости равны нулю 
            //    bc.Value[2][i] = 3;
            //    bc.BC[3][i] = 1; // однородные ГУ скорости равны нулю 
            //    bc.Value[3][i] = 3;
            //}
            #endregion
            #region СЛАУ
            for (i = i0; i < Nx; i++)
                for (j = j0; j < Ny; j++)
                {
                    Ap[i][j] = 4;
                    Ae[i][j] = 1;
                    Aw[i][j] = 1;
                    An[i][j] = 1;
                    As[i][j] = 1;
                    Right[i][j] = 1;
                    X[i][j] = 3;
                }
            for (i = i0; i < Nx; i++)
            {
                Ap[i][j0] = 3;
                Ap[i][Ny - 1] = 3;
            }
            for (j = j0; j < Ny; j++)
            {
                Ap[i0][j] = 3;
                Ap[Nx - 1][j] = 3;
            }
            Ap[i0][j0] = 2;
            Ap[i0][Ny - 1] = 2;
            Ap[Nx - 1][j0] = 2;
            Ap[Nx - 1][Ny - 1] = 2;
            #endregion
            ITPSolver a = new ParrallelPAlgebraCG();

            a.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, Right, bc, ref X);
            LOG.Print("X", X, 4);
            ITPSolver a1 = new TPSolver(imax, jmax);
            a1.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, Right, bc, ref X);
            LOG.Print("X", X, 4);
        }

        //public static void Main()
        //{
        //    Test1();
        //}
    }
}