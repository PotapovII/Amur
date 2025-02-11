//---------------------------------------------------------------------------
//   Реализация библиотеки методов решения систем алгебраических уравнений
//                     - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                   разработка: Потапов И.И.
//                          31.07.2021 
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using CommonLib;
    using System;
    using System.Diagnostics;
    using System.Linq;
    /// <summary>
    /// ОО: Класс с тестовыми примерами
    /// </summary>
    class AlgebraTestClass
    {
        /// <summary>
        /// Порядок СЛАУ
        /// </summary>
        public static uint N { get => Nx * Ny; }
        /// <summary>
        /// Количество узлов по Х
        /// </summary>
        public static uint Nx = 4;
        /// <summary>
        /// Количество узлов по У
        /// </summary>
        public static uint Ny = 4;
        /// <summary>
        /// Число пекле по Х
        /// </summary>
        public static double Pex = 5;
        /// <summary>
        /// Число пекле по У
        /// </summary>
        public static double Pey = 5;
        /// <summary>
        /// Количество КЭ
        /// </summary>
        public static uint CountFE { get => (Nx - 1) * (Ny - 1); }
        public static int isQ = 0;
        public static int DebugFlag = 1;

        /// <summary>
        /// Пример использования класса
        /// </summary>
        public static void Examples<TypeAlgebra>(TypeAlgebra algebra) where TypeAlgebra : IAlgebra
        {
            //
            //  Способ использования для разностных схем
            //
            // коэффициенты матрицы
            double[] A1 = { 2, -1, 0 };
            double[] A2 = { -1, 2, -1 };
            double[] A3 = { 0, -1, 2 };
            // адреса коэффициентов в матрице
            uint[] C1 = { 0, 1, 2 };
            // векторрешения
            double[] X = { 0, 0, 0 };
            //// вектор правой части
            //double[] R = { 0, 0, 4 };
            AlgebraGauss a = new AlgebraGauss(3);
            //// порядок системы
            //a.SetSystem(3);
            // формируем систему
            a.AddStringSystem(A1, C1, 0, 0);
            a.AddStringSystem(A2, C1, 1, 0);
            a.AddStringSystem(A3, C1, 2, 4);
            // решаем систему
            a.Solve(ref X);
            // смотрим ответ
            Console.WriteLine("{0} {1} {2}", X[0], X[1], X[2]);
            Console.ReadLine();
            //
            //  Способ использования для конечно элементных схем
            //
            // Локальная матрица жесткости
            double[][] K = new double[2][];
            double[] K1 = { 1, -1 };
            double[] K2 = { -1, 1 };
            K[0] = K1; K[1] = K2;
            // Массивы связности
            uint[] L1 = { 0, 1 }; uint[] L2 = { 1, 2 };
            // локальная правая часть
            double[] P = { 0, 0 };
            //
            double[] BC = { 4, 2 };
            uint[] LC = { 0, 2 };
            //
            // Очистка системы - повторное использование системы без выделения памяти
            //
            a.Clear();
            //  Формирование ГМЖ системы
            a.AddToMatrix(K, L1);
            a.AddToMatrix(K, L2);
            // Формирование правой части
            a.AddToRight(P, L1);
            a.AddToRight(P, L2);
            // Выполнение граничных условий
            a.BoundConditions(BC, LC);
            // решаем систему
            a.Solve(ref X);
            // смотрим ответ
            Console.WriteLine("{0} {1} {2}", X[0], X[1], X[2]);
        }
        /// <summary>
        /// Одномерная трехдиаганальная 
        /// </summary>
        /// <typeparam name="TypeAlgebra"></typeparam>
        /// <param name="algebra"></param>
        public static void Test_1D_PoissonTask<TypeAlgebra>(TypeAlgebra algebra) where TypeAlgebra : IAlgebra
        {
            double[] m0 = { 1, -1 };
            double[] m1 = { -1, 1 };
            double[][] m = new double[2][];
            m[0] = m0;
            m[1] = m1;
            double[] B = { 0, 0 };
            double[] BC = { 0, 4 };

            uint[] ad = { 0, 0 };
            uint[] adb = { 0, 0 };
            uint NE = N - 1;
            BC[1] = NE;
            double[] X = new double[N];

            for (uint i = 0; i < NE; i++)
            {
                ad[0] = i; ad[1] = i + 1;
                algebra.AddToMatrix(m, ad);
                algebra.AddToRight(B, ad);
            }
            adb[0] = 0; adb[1] = NE;
            uint[] adb0 = { 0 };
            uint[] adb1 = { NE };
            double[] BC0 = { 0 };
            double[] BC1 = { NE };
            algebra.BoundConditions(BC0, adb0);
            // algebra.Print();
            algebra.BoundConditions(BC1, adb1);
            //algebra.Print();
            algebra.Solve(ref X);
            for (int j = 0; j < N; j++)
                Console.Write(" " + X[j].ToString("F4"));
        }

        public static string GetStopWatch(Stopwatch sw)
        {
            string s = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds,
            sw.Elapsed.Milliseconds / 10);
            LOG.Print("Общее время расчета ", s);
            return s;
        }


        /// <summary>
        /// Двухмерная задача пуассона решаемая МКЭ (квадратные КЭ 1Х1)
        /// </summary>
        /// <typeparam name="TypeAlgebra"></typeparam>
        /// <param name="algebra"></param>
        public static void Test_2D_PoissonTask<TypeAlgebra>(TypeAlgebra algebra) where TypeAlgebra : IAlgebra
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            algebra.Clear();
            // ЛМЖ
            double[] m0 = { 2.0 / 3, -1.0 / 6, -1.0 / 3, -1.0 / 6 };
            double[] m1 = { -1.0 / 6, 2.0 / 3, -1.0 / 6, -1.0 / 3 };
            double[] m2 = { -1.0 / 3, -1.0 / 6, 2.0 / 3, -1.0 / 6 };
            double[] m3 = { -1.0 / 6, -1.0 / 3, -1.0 / 6, 2.0 / 3 };
            double[][] m = new double[4][];
            m[0] = m0;
            m[1] = m1;
            m[2] = m2;
            m[3] = m3;
            uint k = 0;
            uint[,] map = new uint[Nx, Ny];
            for (uint i = 0; i < Nx; i++)
                for (uint j = 0; j < Ny; j++)
                    map[i, j] = k++;
            int NE = (int)((Nx - 1) * (Ny - 1));
            uint[][] adres = null;
            MEM.Alloc2D<uint>(NE, 4, ref adres);
            k = 0;
            for (uint i = 0; i < Nx - 1; i++)
                for (uint j = 0; j < Ny - 1; j++)
                {
                    adres[k][0] = map[i + 1, j];
                    adres[k][1] = map[i + 1, j + 1];
                    adres[k][2] = map[i, j + 1];
                    adres[k][3] = map[i, j];
                    k++;
                }
            double[] X = new double[N];
            double[] B = { isQ * 0.25, isQ * 0.25, isQ * 0.25, isQ * 0.25 };
            for (uint i = 0; i < NE; i++)
            {
                algebra.AddToMatrix(m, adres[i]);
                algebra.AddToRight(B, adres[i]);
            }
            //algebra.Print();
            double[] BC0 = new double[Ny];
            double[] BC2 = new double[Ny];
            uint[] adb0 = new uint[Ny];
            uint[] adb2 = new uint[Ny];
            for (uint j = 0; j < Ny; j++)
            {
                BC2[j] = 0;
                BC0[j] = Nx - 1;
                adb2[j] = map[0, j];
                adb0[j] = map[Nx - 1, j];
            }
            algebra.BoundConditions(BC0, adb0);
            // algebra.Print();
            algebra.BoundConditions(BC2, adb2);
            // algebra.Print();
            algebra.Solve(ref X);
            stopWatch.Stop();
            string s = GetStopWatch(stopWatch);
            LOG.Print("Общее время расчета ", s);

            k = 0;
            if (DebugFlag > 1)
                for (uint i = 0; i < Nx; i++)
                {
                    for (uint j = 0; j < Ny; j++)
                        Console.Write(" " + X[k++].ToString("F5"));
                    Console.WriteLine();
                }
        }
        protected static double DifFlow(double Diff, double Flow)
        {
            if (Flow == 0.0f)
                return Diff;
            double temp = Diff - 0.1 * Math.Abs(Flow);
            if (temp <= 0.0f)
                return 0.0f;
            temp = temp / Diff;
            return (Diff * temp * temp * temp * temp * temp);
        }
        /// <summary>
        /// Двухмерная задача пуассона решаемая МКЭ (квадратные КЭ 1Х1)
        /// </summary>
        /// <typeparam name="TypeAlgebra"></typeparam>
        /// <param name="algebra"></param>
        public static void Test_2D_MassTransfer<TypeAlgebra>(TypeAlgebra algebra) where TypeAlgebra : IAlgebra
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            algebra.Clear();
            uint imax = Nx - 1;
            uint jmax = Ny - 1;

            double[][] Ae;
            double[][] Aw;
            double[][] An;
            double[][] As;
            double[][] Ap;
            double[][] Ap0;
            double[][] sc;
            double[][] sp;
            Ae = new double[Nx][];
            Aw = new double[Nx][];
            An = new double[Nx][];
            As = new double[Nx][];
            Ap = new double[Nx][];
            Ap0 = new double[Nx][];
            sc = new double[Nx][];
            sp = new double[Nx][];
            for (int i = 0; i < imax + 1; i++)
            {
                Ae[i] = new double[Ny];
                Aw[i] = new double[Ny];
                An[i] = new double[Ny];
                As[i] = new double[Ny];
                Ap[i] = new double[Ny];
                Ap0[i] = new double[Ny];
                sc[i] = new double[Ny];
                sp[i] = new double[Ny];

            }

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
            int ist = 1;
            int jst = 1;
            double V = 0, U = 0;
            double L = 1, H = 1;
            double rho = 1000;
            double gam = 1;
            double q = 100;
            double flow, diff;
            // Расчет площади КО,
            double vol = L * H;
            #region
            for (int i = ist; i < imax; i++)
            {
                // граница сверху
                // Конвективный поток
                flow = rho * V * H;
                // Диффузионный поток
                diff = gam * L / H;
                // Вычисление коэффициента
                As[i][jst] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                for (int j = jst; j < jmax; j++)
                {
                    if (j < jmax - 1)
                    {
                        // Балансровка схемы при сгущениях
                        double fy = 1.0 / 2.0;
                        // Средняя плотность
                        double arho = rho * fy + rho * (1 - fy);
                        // Средняя "проводимость"
                        double agam = gam * fy + gam * (1 - fy);
                        // Конвективный поток
                        flow = arho * V * L;
                        // Диффузионный поток
                        diff = gam * gam / agam * L / H;
                    }
                    else
                    {
                        // граница снизу
                        // Конвективный поток
                        flow = rho * V * L;
                        // Диффузионный поток
                        diff = gam * L / H;
                    }
                    // Вычисление коэффициентов
                    As[i][j + 1] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                    An[i][j] = As[i][j + 1] - flow;
                }
            }
            // Расчет горизонтальных коэффициентов Ae[i][j], Aw[i][j]
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //for (int j = jst; j < jmax - 1; j++)
            for (int j = jst; j < jmax; j++)
            {
                // граница справа (ось симметрии)
                // Конвективный поток
                flow = rho * U * H;
                // Диффузионный поток
                diff = gam * H / L;
                // Вычисление коэффициента
                Aw[ist][j] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                // В области
                for (int i = 1; i < imax; i++)
                {
                    if (i < imax - 1)
                    {
                        // Балансровка схемы при сгущениях
                        double fx = L / H / 2;
                        // Средняя плотность
                        double arho = rho * fx + rho * (1 - fx);
                        // Средняя "проводимость"
                        double agam = gam * fx + gam * (1 - fx);
                        // Конвективный поток
                        flow = arho * U * H;
                        // Диффузионный поток
                        diff = gam * gam / agam * H / L;
                    }
                    else
                    {
                        // Стенка с права (твердая стенка)
                        // Конвективный поток
                        flow = rho * U * H;
                        // Диффузионный поток
                        diff = gam * H / L;
                    }
                    // Вычисление коэффициентов
                    Aw[i + 1][j] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                    Ae[i][j] = Aw[i + 1][j] - flow;
                }
            }
            // Расчет правых частей
            for (int j = jst; j < jmax; j++)
            {
                for (int i = ist + 1; i < imax; i++)
                {
                    sc[i][j] = q;
                    sp[i][j] = 0;
                }
            }
            for (int i = ist; i < imax; i++)
            {
                for (int j = jst; j < jmax; j++)
                {
                    // Расчет Ap0, Ap, 
                    //Ap0[i][j] = rho * vol / dt;
                    Ap0[i][j] = 0;
                    Ap[i][j] = Ap0[i][j] + Aw[i][j] + Ae[i][j] + An[i][j] + As[i][j];
                    // коррекция правой части временными членами
                    sc[i][j] = q * vol;
                }
            }
            #endregion
            // ЛМЖ
            uint k = 0;
            uint[,] map = new uint[Nx, Ny];
            for (uint i = 0; i < Nx; i++)
                for (uint j = 0; j < Ny; j++)
                    map[i, j] = k++;
            int NE = (int)((Nx - 1) * (Ny - 1));

            LOG.Print("Ap", Ap, 3);
            LOG.Print("Ae", Ae, 3);
            LOG.Print("Aw", Aw, 3);
            LOG.Print("An", An, 3);
            LOG.Print("As ", As, 3);
            try
            {

                for (uint i = 1; i < Nx - 1; i++)
                    for (uint j = 1; j < Ny - 1; j++)
                    {
                        k = map[i, j];
                        uint iE = map[i + 1, j];
                        uint iW = map[i - 1, j];
                        uint iN = map[i, j + 1];
                        uint iS = map[i, j - 1];
                        uint iP = map[i, j];
                        uint[] adress = { iP, iE, iW, iN, iS };
                        double aE = -Ae[i][j];
                        double aW = -Aw[i][j];
                        double aN = -An[i][j];
                        double aS = -As[i][j];
                        double aP = Ap[i][j];
                        double[] A = { aP, aE, aW, aN, aS };
                        algebra.AddStringSystem(A, adress, k, sc[i][j]);
                    }


                double[] BC0 = new double[2 * Nx + 2 * Ny - 4];
                uint[] adb0 = new uint[2 * Nx + 2 * Ny - 4];

                //LOG.Print("map", map);
                for (uint j = 0; j < BC0.Length / 2; j++)
                    BC0[j] = 1;

                k = 0;
                for (uint j = 0; j < Ny - 1; j++)
                    adb0[k++] = map[0, j];
                for (uint j = 0; j < Nx - 1; j++)
                    adb0[k++] = map[j, Ny - 1];
                //  LOG.Print("adb0", adb0);

                for (uint j = 1; j < Ny; j++)
                    adb0[k++] = map[Nx - 1, j];
                for (uint j = 1; j < Nx; j++)
                    adb0[k++] = map[j, 0];

                //LOG.Print("adb0", adb0);

                algebra.BoundConditions(BC0, adb0);
                //algebra.Print();
                double[] X = new double[N];
                algebra.Solve(ref X);
                stopWatch.Stop();
                string s = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds,
                stopWatch.Elapsed.Milliseconds / 10);
                LOG.Print("Общее время расчета ", s);
                k = 0;
                if (DebugFlag > 1)
                    for (uint i = 0; i < Nx; i++)
                    {
                        for (uint j = 0; j < Ny; j++)
                            Console.Write(" " + X[k++].ToString("F5"));
                        Console.WriteLine();
                    }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }

        public static void Main()
        {
            IAlgebra algebra = null;
            DebugFlag = 1;
            Nx = 5;
            Ny = 7;
            uint N = AlgebraTestClass.N;
            // размерность задачи
            int dimention = 2;
            int typeTest = 1;
            // номер метода
            Console.WriteLine(" Nx = {0}, Ny = {1}", Nx, Ny);
            int method = 1;
            for (method = 0; method < 2; method++)
            {
                Console.WriteLine();
                LOG.TPrint("Индекс метода в тесте", method);
                //switch (method)
                //{
                //    case 0:
                //        algebra = new AlgebraLUTape(N, (int)Nx + 1, (int)Nx + 1);
                //        break;
                //    case 1:
                if (method == 0)
                    algebra = new SparseAlgebraGMRES(N, 10, true);
                else
                    algebra = new SparseAlgebraGMRES(N);
                //        break;
                //    case 2:
                //        algebra = new SparseAlgebraBeCG(N);
                //        break;
                //    case 3:
                //        algebra = new SparseAlgebraCG(N);
                //        break;
                //}
                if (dimention == 1)
                {
                    Test_1D_PoissonTask(algebra);
                    IAlgebraResult result = algebra.Result;
                    result.Print();
                }
                else
                {
                    if (typeTest == 0)
                    {
                        Test_2D_PoissonTask(algebra);
                        IAlgebraResult result = algebra.Result;
                        result.Print();
                    }
                    else
                    {
                        Test_2D_MassTransfer(algebra);
                        IAlgebraResult result = algebra.Result;
                        result.Print();
                    }
                }
            }
            Console.Read();
        }

        public static void Main1()
        {
            IAlgebra algebra = null;
            Nx = 40;
            Ny = 40;
            uint N = AlgebraTestClass.N;
            // размерность задачи
            int dimention = 2;
            int typeTest = 1;
            // номер метода
            Console.WriteLine(" Nx = {0}, Ny = {1}", Nx, Ny);
            int method = 1;
            for (method = 0; method < 12; method++)
            {
                Console.WriteLine();
                LOG.TPrint("Индекс метода в тесте", method);
                switch (method)
                {
                    case 0:
                        algebra = new AlgebraLU(N);
                        break;
                    case 1:
                        algebra = new AlgebraLUMax(N);
                        break;
                    case 2:
                        algebra = new AlgebraLUTape(N, (int)Nx + 1, (int)Nx + 1);
                        break;
                    case 3:
                        algebra = new AlgebraGauss(N);
                        break;
                    case 4:
                        algebra = new AlgebraGaussTape(N, Nx + 2);
                        break;
                    case 5:
                        algebra = new AlgebraCholetsky(N);
                        break;
                    case 6:
                        algebra = new AlgebraGMRESS(N);
                        break;
                    case 7:
                        algebra = new SparseAlgebraCG(N);
                        break;
                    case 8:
                        algebra = new SparseAlgebraBeCG(N);
                        break;
                    case 9:
                        algebra = new SparseAlgebraGMRES(N);
                        break;
                    case 10:
                        algebra = new AlgebraGauss(N);
                        break;
                    case 11:
                        algebra = new AlgebraCG_FEM(N, CountFE);
                        break;
                    case 12:
                        //algebra = new SparseAlgebraGaussStr(N);
                        break;
                }
                if (dimention == 1)
                {
                    Test_1D_PoissonTask(algebra);
                    IAlgebraResult result = algebra.Result;
                    result.Print();
                }
                else
                {
                    if (typeTest == 0)
                    {
                        Test_2D_PoissonTask(algebra);
                        IAlgebraResult result = algebra.Result;
                        result.Print();
                    }
                    else
                    {
                        Test_2D_MassTransfer(algebra);
                        IAlgebraResult result = algebra.Result;
                        result.Print();
                    }
                }
            }
            Console.Read();
        }

        /// <summary>
        /// Тесты на построение предобуславливателей ILU(1)
        /// </summary>
        public static void Main0()
        {
            int n = 7;
            double sum = 0;
            double[,] A = { { 9, 0, 0, 3, 1, 0, 1 },
                            { 0, 11, 2, 1, 0, 0, 2 },
                            { 0, 1, 10, 2, 0, 0, 0 },
                            { 2, 1, 2, 9, 1, 0, 0 },
                            { 1, 0, 0, 1, 12, 0, 1 },
                            { 0, 0, 0, 0, 0, 8, 0 },
                            { 2, 2, 0, 0, 3, 0, 8 } };

            double[][] B = new double[n][];
            uint[][] ad = new uint[n][];

            B[0] = new double[4] { 9, 3, 1, 1 };
            ad[0] = new uint[4] { 0, 3, 4, 6 };

            B[1] = new double[4] { 11, 2, 1, 2 };
            ad[1] = new uint[4] { 1, 2, 3, 6 };

            B[2] = new double[3] { 1, 10, 2 };
            ad[2] = new uint[3] { 1, 2, 3 };

            B[3] = new double[5] { 2, 1, 2, 9, 1 };
            ad[3] = new uint[5] { 0, 1, 2, 3, 4 };

            B[4] = new double[4] { 1, 1, 12, 1 };
            ad[4] = new uint[4] { 0, 3, 4, 6 };

            B[5] = new double[1] { 8 };
            ad[5] = new uint[1] { 5 };

            B[6] = new double[4] { 2, 2, 3, 8 };
            ad[6] = new uint[4] { 0, 1, 4, 6 };

            double[,] lu = new double[n, n];

            //for (int i = 0; i < n; i++)
            //{
            //    for (int j = i; j < n; j++)
            //    {
            //        sum = 0;
            //        for (int k = 0; k < i; k++)
            //            sum += lu[i, k] * lu[k, j];
            //        lu[i, j] = A[i, j] - sum;
            //    }
            //    double aa = (1 / lu[i, i]);
            //    for (int j = i + 1; j < n; j++)
            //    {
            //        sum = 0;
            //        for (int k = 0; k < i; k++)
            //            sum += lu[j, k] * lu[k, i];
            //        lu[j, i] = aa * (A[j, i] - sum);
            //    }
            //}
            //LOG.Print("A", A, 3);
            //LOG.Print("lu", lu, 3);

            double[,] ILU = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (Math.Abs(A[i, j]) > MEM.Error10)
                    {
                        sum = 0;
                        for (int k = 0; k < i; k++)
                            sum += ILU[i, k] * ILU[k, j];
                        ILU[i, j] = A[i, j] - sum;
                    }
                }
                double aa = (1 / ILU[i, i]);
                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(A[i, j]) > MEM.Error10)
                    {
                        sum = 0;
                        for (int k = 0; k < i; k++)
                            sum += ILU[j, k] * ILU[k, i];
                        ILU[j, i] = aa * (A[j, i] - sum);
                    }
                }
            }
            LOG.Print("Full ILU", ILU, 5);

            SparseAlgebraGMRES algebra = new SparseAlgebraGMRES((uint)n, 10, true);
            for (uint j = 0; j < n; j++)
            {
                algebra.AddStringSystem(B[j], ad[j], j, 0);
            }
            double[] R = new double[n];
            double[] X = new double[n];
            double[] Y = new double[n];
            double[] S = new double[n];
            for (int i = 0; i < n; i++)
            {
                R[i] = 1;
                double[] b = B[i];
                S[i] = B[i].Sum();
            }

            algebra.TP(R, ref X);
            LOG.Print("algebra R ", R);
            LOG.Print("algebra X ", X);
            //algebra.Print();

            // поиск решения

            double t;
            for (int i = 0; i < n; i++)
            {
                R[i] = 1;
                double[] b = B[i];
                S[i] = B[i].Sum();
            }
            // find solution of Ly = b
            for (int i = 0; i < n; i++)
            {
                sum = 0;
                for (int k = 0; k < i; k++)
                    sum += ILU[i, k] * Y[k];
                Y[i] = R[i] - sum;
            }
            // find solution of Ux = y
            for (int i = n - 1; i >= 0; i--)
            {
                t = 1.0 / ILU[i, i];
                sum = 0;
                for (int k = i + 1; k < n; k++)
                    sum += ILU[i, k] * X[k];
                X[i] = t * (Y[i] - sum);
            }
            LOG.Print(" R ", R);
            LOG.Print(" Y ", Y);
            LOG.Print(" X ", X);
            LOG.Print(" S ", S);

            double[,] AL = new double[n, n];
            double[,] AU = new double[n, n];
            // IUL
            for (int i = 0; i < n; i++)
                AL[i, i] = 1;

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (A[i, j] > 0)
                    {
                        sum = 0;
                        for (int k = 0; k < i; k++)
                            sum += AL[i, k] * AU[k, j];
                        AU[i, j] = A[i, j] - sum;
                    }
                }
                for (int j = i + 1; j < n; j++)
                {
                    if (A[i, j] > 0)
                    {
                        sum = 0;
                        for (int k = 0; k < i; k++)
                            sum += AL[j, k] * AU[k, i];
                        AL[j, i] = (1 / AU[i, i]) * (A[j, i] - sum);
                    }
                }
            }

            LOG.Print("A", A, 3);
            LOG.Print("IAL", AL, 3);
            LOG.Print("IAU", AU, 3);
        }
    }
}
