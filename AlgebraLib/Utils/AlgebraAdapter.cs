//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          10.08.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Адаптация для использования стандартных решателей IAlgebra 
    /// для конечно-разностных пятиточечных схем МКО
    /// в четырех угольных областях
    /// </summary>
    public class AlgebraAdapter
    {
        /// <summary>
        /// объект решателя СЛАУ
        /// </summary>
        IAlgebra algebra;
        /// <summary>
        /// одномерный вектор решения
        /// </summary>
        double[] X1D = null;
        /// <summary>
        /// количество КО по х
        /// </summary>
        int imax;
        /// <summary>
        /// количество КО по у
        /// </summary>
        int jmax;
        /// <summary>
        /// количество узлов по х
        /// </summary>
        int Nx;
        /// <summary>
        /// количество узлов по у
        /// </summary>
        int Ny;
        /// <summary>
        /// количество неизвестных
        /// </summary>
        int N;
        /// <summary>
        /// количество конечных элементов
        /// </summary>
        int NE;
        /// <summary>
        /// Карта узлов
        /// </summary>
        uint[,] map = null;
        ///// <summary>
        ///// адреса граничных условий
        ///// </summary>
        //uint[] adb0 = null;
        /// <summary>
        /// граничные условия на южной границе 0
        /// </summary>
        public double[] BC_S = null;
        /// <summary>
        /// граничные условия на северной границе 1
        /// </summary>
        public double[] BC_N = null;
        /// <summary>
        /// граничные условия на западной границе 2
        /// </summary>
        public double[] BC_W = null;
        /// <summary>
        /// граничные условия на восточной границе 3
        /// </summary>
        public double[] BC_E = null;
        /// <summary>
        /// граничные индексы южной границы 0
        /// </summary>
        public uint[] adressBC_S = null;
        /// <summary>
        /// граничные индексы северной границы 1
        /// </summary>
        public uint[] adressBC_N = null;
        /// <summary>
        /// граничные индексы западной границы 2
        /// </summary>
        public uint[] adressBC_W = null;
        /// <summary>
        /// граничные индексы восточной границы 3
        /// </summary>
        public uint[] adressBC_E = null;

        public AlgebraAdapter(int imax, int jmax, IAlgebra algebra = null)
        {
            this.imax = imax;
            this.jmax = jmax;
            Nx = imax + 1;
            Ny = jmax + 1;
            N = Nx * Ny;
            if (algebra != null)
                this.algebra = algebra;
            else
                this.algebra = new SparseAlgebraGMRES((uint)N, 10, false);
            //this.algebra = new SparseAlgebraGMRES((uint)N, 10, true);
            //this.algebra = new AlgebraGauss((uint)N);
            InitMap();
        }

        public void InitMap()
        {
            uint k = 0;
            map = new uint[Nx, Ny];
            for (uint i = 0; i < Nx; i++)
                for (uint j = 0; j < Ny; j++)
                    map[i, j] = k++;
            NE = (Nx - 1) * (Ny - 1);
            adressBC_S = new uint[Nx];
            adressBC_N = new uint[Nx];
            adressBC_W = new uint[Ny];
            adressBC_E = new uint[Ny];
            BC_S = new double[Nx];
            BC_N = new double[Nx];
            BC_W = new double[Ny];
            BC_E = new double[Ny];
        }

        /// <summary>
        /// Установка граничных условий (полный контур) по существующему полю X
        /// </summary>
        public void SetBCondition(int ist, int jst, ref double[][] X)
        {
            int i, j;
            ist--;
            jst--;
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
            for (i = ist; i < Nx; i++)
            {
                adressBC_S[i] = map[i, j];
                BC_S[i] = X[i][j];
            }
            j = jmax;
            for (i = ist; i < Nx; i++)
            {
                adressBC_N[i] = map[i, j];
                BC_N[i] = X[i][j];
            }
            i = ist;
            for (j = jst; j < Ny; j++)
            {
                adressBC_W[j] = map[i, j];
                BC_W[j] = X[i][j];
            }
            i = imax;
            for (j = jst; j < Ny; j++)
            {
                adressBC_E[j] = map[i, j];
                BC_E[j] = X[i][j];
            }
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
        public void OnTDMASolver(int ist, int jst, double[][] Ae, double[][] Aw, double[][] An, double[][] As, double[][] Ap,
             double[][] sc, ref double[][] X, int[] BCFlag = null)
        {
            int[] BCFlagDef = { 1, 1, 1, 1 };
            if (BCFlag == null)
                BCFlag = BCFlagDef;
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

            try
            {
                //for (uint i = 1; i < Nx - 1; i++)
                //    for (uint j = 1; j < Ny - 1; j++)
                //    {
                //        k = map[i, j];
                //        uint iE = map[i + 1, j];
                //        uint iW = map[i - 1, j];
                //        uint iN = map[i, j + 1];
                //        uint iS = map[i, j - 1];
                //        uint iP = map[i, j];
                //        uint[] adress = { iP, iE, iW, iN, iS };
                //        double aE = -Ae[i][j];
                //        double aW = -Aw[i][j];
                //        double aN = -An[i][j];
                //        double aS = -As[i][j];
                //        double aP = Ap[i][j];
                //        double[] A = { aP, aE, aW, aN, aS };
                //        algebra.AddStringSystem(A, adress, k, sc[i][j]);
                //    }
                // Сборка СЛАУ
                uint k = 0;
                for (uint i = 1; i < Nx - 1; i++)
                    for (uint j = 1; j < Ny - 1; j++)
                    {
                        k = map[i, j];
                        //  адреса дискретного аналога { iP, iE, iW, iN, iS };
                        uint[] adress = { map[i, j], map[i + 1, j], map[i - 1, j], map[i, j + 1], map[i, j - 1] };
                        //  коэффициенты дискретного аналога { aP, aE, aW, aN, aS };
                        double[] elems = { Ap[i][j], -Ae[i][j], -Aw[i][j], -An[i][j], -As[i][j] };
                        algebra.AddStringSystem(elems, adress, k, sc[i][j]);
                    }
                //double[] BC0 = new double[2 * Nx + 2 * Ny - 4];
                //uint[] adb0 = new uint[2 * Nx + 2 * Ny - 4];
                ////LOG.Print("map", map);
                //for (uint j = 0; j < BC0.Length / 2; j++)
                //    BC0[j] = 1;
                //k = 0;
                //for (uint j = 0; j < Ny - 1; j++)
                //    adb0[k++] = map[0, j];
                //for (uint j = 0; j < Nx - 1; j++)
                //    adb0[k++] = map[j, Ny - 1];
                ////  LOG.Print("adb0", adb0);
                //for (uint j = 1; j < Ny; j++)
                //    adb0[k++] = map[Nx - 1, j];
                //for (uint j = 1; j < Nx; j++)
                //    adb0[k++] = map[j, 0];
                //LOG.Print("adb0", adb0);

                if (BCFlag[0] == 1)
                    algebra.BoundConditions(BC_S, adressBC_S);
                if (BCFlag[1] == 1)
                    algebra.BoundConditions(BC_N, adressBC_N);
                if (BCFlag[2] == 1)
                    algebra.BoundConditions(BC_W, adressBC_W);
                if (BCFlag[3] == 1)
                    algebra.BoundConditions(BC_E, adressBC_E);
                // algebra.Print(3);
                algebra.Solve(ref X1D);

                k = 0;
                for (uint i = 0; i < Nx; i++)
                {
                    for (uint j = 0; j < Ny; j++)
                        X[i][j] = X1D[k++];
                }
                //k = 0;
                //for (uint i = 0; i < Nx; i++)
                //{
                //    for (uint j = 0; j < Ny; j++)
                //        Console.Write(" " + X1D[k++].ToString("F5"));
                //    Console.WriteLine();
                //}
                //for (uint i = 0; i < Nx; i++)
                //{
                //    for (uint j = 0; j < Ny; j++)
                //        Console.Write(" " + X[i][j].ToString("F5"));
                //    Console.WriteLine();
                //}
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
    }
}
