////---------------------------------------------------------------------------
////              Реализация библиотеки для моделирования 
////               гидродинамических и русловых процессов
////                      - (C) Copyright 2019 -
////                       ALL RIGHT RESERVED
////                        ПРОЕКТ "BLLib"
////---------------------------------------------------------------------------
////                   разработка: Потапов И.И.
////                          27.12.19
////---------------------------------------------------------------------------
////                Модуль BLLib для расчета донных деформаций 
////                 (учет движения только влекомых наносов)
////         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
////---------------------------------------------------------------------------
//namespace BLLib
//{
//    using System;
//    using MeshLib;
//    using SaveDataLib;
//    using CommonLib;
//    /// <summary>
//    /// ОО: Класс для решения одномерной задачи о 
//    /// расчете донных деформаций русла вдоль потока
//    /// </summary>
//    [Serializable]
//    public class CBedLoadTask : BedLoadParams, IBedLoadTask
//    {
//        ///// <summary>
//        ///// гравитационная постоянная (м/с/с)
//        ///// </summary>
//        //public double g = 9.81;
//        /// <summary>
//        /// тангенс угла phi
//        /// </summary>
//        public double tanphi;
//        /// <summary>
//        /// критические напряжения на ровном дне
//        /// </summary>
//        public double tau0 = 0;
//        /// <summary>
//        /// транзитный расход на входе
//        /// </summary>
//        public double Gtran_in = 0;
//        /// <summary>
//        /// транзитный расход на выходе
//        /// </summary>
//        public double Gtran_out = 0;
//        /// <summary>
//        /// относительная плотность
//        /// </summary>
//        public double rho_b;
//        /// <summary>
//        /// параметр стратификации активного слоя, 
//        /// в котором переносятся донные частицы
//        /// </summary>
//        public double s;
//        /// <summary>
//        /// коэффициент сухого трения
//        /// </summary>
//        public double Fa0;
//        /// <summary>
//        /// константа расхода влекомых наносов
//        /// </summary>
//        public double G1;
//        /// <summary>
//        /// <summary>
//        /// Флаг отладки
//        /// </summary>
//        public int debug = 0;
//        /// <summary>
//        /// Поле сообщений о состоянии задачи
//        /// </summary>
//        public string Message = "Ok";
//        /// <summary>
//        /// длина расчетной области
//        /// </summary>
//        public double L;

//        #region Рабочие массивы
//        /// <summary>
//        /// массив координаты узлов
//        /// </summary>
//        public double[] x = null;
//        /// <summary>
//        /// массив донных отметок
//        /// </summary>
//        public double[] Zeta = null;
//        /// <summary>
//        /// массив донных отметок на предыдущем слое по времени
//        /// </summary>
//        public double[] Zeta0 = null;
//        /// <summary>
//        /// Учет лавинного осыпания 
//        /// </summary>
//        public bool isAvalanche = false;

//        #endregion

//        #region Краевые условия

//        /// <summary>
//        /// тип задаваемых ГУ
//        /// </summary>
//        public BCondition BCBed;

//        #endregion

//        #region Служебные переменные
//        /// <summary>
//        /// Количество расчетных узлов для дна
//        /// </summary>
//        public int Count;
//        /// <summary>
//        /// Количество расчетных подобластей
//        /// </summary>
//        public int N;
//        ///// <summary>
//        ///// шаг дискретной схемы по х
//        ///// </summary>
//        //public double dx;
//        /// <summary>
//        /// текущее время расчета 
//        /// </summary>
//        public double time = 0;
//        /// <summary>
//        /// текущая итерация по времени 
//        /// </summary>
//        public int CountTime = 0;
//        /// <summary>
//        /// количество узлов по времени
//        /// </summary>
//        public int LengthTime = 200000;
//        /// <summary>
//        /// относительная точность при вычислении 
//        /// изменения донной поверхности
//        /// </summary>
//        protected double eZeta = 0.000001;
//        /// <summary>
//        /// расчетный период времени, сек 
//        /// </summary>
//        public double T;
//        /// <summary>
//        /// шаг по времени
//        /// </summary>
//        public double dtime;
//        /// <summary>
//        /// расчетный шаг по времени
//        /// </summary>
//        public double rdt;
//        /// <summary>
//        /// множитель для приведения придонного давления к напору
//        /// </summary>
//        double gamma;
//        /// <summary>
//        ///  косинус гамма - косинус угола между 
//        ///  нормалью к дну и вертикальной осью
//        /// </summary>
//        double[] CosGamma = null;

//        double[] G0 = null;
//        double[] A = null;
//        double[] B = null;
//        double[] C = null;

//        double[] CE = null;
//        double[] CW = null;
//        double[] CP = null;
//        double[] S = null;

//        double[] AE = null;
//        double[] AW = null;
//        double[] AP = null;
//        double[] AP0 = null;

//        double[] ps = null;

//        double dz, dx, dp;
//        double mtau, chi;
//        #endregion

//        /// <summary>
//        /// Конструктор по умолчанию/тестовый
//        /// </summary>
//        public CBedLoadTask(BedLoadParams p) : base(p)
//        {
//            InitBedLoad();
//        }
//        /// <summary>
//        /// Конструктор с параметрами/тестовый
//        /// </summary>
//        public CBedLoadTask(double rho_w, double rho_s, double phi,
//        double d50, double epsilon, double kappa, double cx, double f)
//        {
//            this.rho_w = rho_w;
//            this.rho_s = rho_s;
//            this.phi = phi;
//            this.d50 = d50;
//            this.epsilon = epsilon;
//            this.f = f;
//            this.kappa = kappa;
//            this.cx = cx;
//            InitBedLoad();
//        }
//        public void ReInitBedLoad(BedLoadParams p)
//        {
//            SetParams(p);
//            InitBedLoad();
//        }
//        /// <summary>
//        /// Расчет постоянных коэффициентов задачи
//        /// </summary>
//        public void InitBedLoad()
//        {
//            gamma = 1.0 / (rho_w * g);
//            // тангенс угла внешнего откоса
//            tanphi = Math.Tan(phi / 180 * Math.PI);
//            // сухое трение
//            Fa0 = tanphi * (rho_s - rho_w) * g;
//            // критические напряжения на ровном дне
//            tau0 = 9.0 / 8.0 * kappa * kappa * d50 * Fa0 / cx;
//            // константа расхода влекомых наносов
//            G1 = 4.0 / (3.0 * kappa * Math.Sqrt(rho_w) * Fa0 * (1 - epsilon));
//            // относительная плотность
//            rho_b = (rho_s - rho_w) / rho_w;
//            // параметр стратификации активного слоя, 
//            // в котором переносятся донные частицы
//            s = f * rho_b;
//        }
//        /// </summary>
//        /// <param name="BCBed">граничные условий</param>
//        /// <param name="sand">узлы размыва</param>
//        /// <param name="x">координаты узлов</param>
//        /// <param name="Zeta0">начальный уровень дна</param>
//        /// <param name="dtime">Шаг по времени</param>
//        /// </summary>
//        public void SetTask(IMesh mesh, BCondition BCBed, double[] x, double[] Zeta0,
//                            double dtime, bool isAvalanche = true)
//        {
//            this.x = x;
//            this.Zeta0 = Zeta0;
//            this.Count = x.Length;
//            this.N = Count - 1;
//            this.L = x[N] - x[0];
//            this.BCBed = BCBed;
//            this.dtime = dtime;
//            this.isAvalanche = isAvalanche;
//            // узловые массивы
//            Zeta = new double[Count];
//            ps = new double[Count];

//            CosGamma = new double[N];

//            A = new double[N];
//            B = new double[N];
//            C = new double[N];
//            G0 = new double[N];

//            CE = new double[Count];
//            CW = new double[Count];
//            CP = new double[Count];
//            S = new double[Count];

//            AE = new double[Count];
//            AW = new double[Count];
//            AP = new double[Count];
//            AP0 = new double[Count];
//        }

//        /// <summary>
//        /// Установка текущего шага по времени
//        /// </summary>
//        /// <param name="dtime"></param>
//        public void SetDTime(double dtime)
//        {
//            this.dtime = dtime;
//        }
//        /// <summary>
//        /// Переустановка граничных условий 
//        /// </summary>
//        /// <param name="typeBCBed">Тип граничных условий</param>
//        public void ReStartBCBedTask(BCondition BCBed)
//        {
//            this.BCBed = BCBed;
//        }
//        /// <summary>
//        /// Тестовая печать поля
//        /// </summary>
//        /// <param name="Name">имя поля</param>
//        /// <param name="mas">массив пля</param>
//        /// <param name="FP">точность печати</param>
//        public void PrintMas(string Name, double[] mas, int FP = 8)
//        {
//            string Format = " {0:F6}";
//            if (FP != 6)
//                Format = " {0:F" + FP.ToString() + "}";

//            Console.WriteLine(Name);
//            for (int i = 0; i < mas.Length; i++)
//            {
//                Console.Write(Format, mas[i]);
//            }
//            Console.WriteLine();
//        }
//        public void PrintMatrix(int FP = 8)
//        {
//            string Format = " {0:F6}";
//            if (FP != 6)
//                Format = " {0:F" + FP.ToString() + "}";

//            for (int i = 0; i < AP.Length; i++)
//            {
//                for (int j = 0; j < AP.Length; j++)
//                {
//                    double a = 0;
//                    if (i == j + 1)
//                        a = AW[i];
//                    if (i == j)
//                        a = AP[i];
//                    if (i == j - 1)
//                        a = AE[i];
//                    Console.Write(Format, a);
//                }
//                Console.WriteLine();
//            }
//            Console.WriteLine();
//        }
//        ///  /// <summary>
//        /// Вычисление текущих расходов и их градиентов для построения графиков
//        /// </summary>
//        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
//        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
//        /// <param name="tau">придонное касательное напряжение</param>
//        /// <param name="P">придонное давление</param>
//        public void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
//        {
//            if (ps == null) return;
//            if (Gs == null)
//            {
//                Gs = new double[4][]; // idxAll, idxTransit, zeta, press 
//                dGs = new double[4][];
//                for(int i=0; i<Gs.Length; i++)
//                {
//                    Gs[i] = new double[N];
//                    dGs[i] = new double[N];
//                }
//            }
//            // Расчет деформаций дна от влекомых наносов
//            // Давление в узлах Zeta,  Zeta0
//            if (P.Length == N)
//            {
//                for (int j = 1; j < N; j++)
//                    ps[j] = 0.5 * (P[j] + P[j - 1]) * gamma; // (rho_w * g);
//                                                             // !!!линейная интерполяция плоха!!! нужна квадратичная
//                ps[0] = (2 * P[1] - P[2]) * gamma; // (rho_w * g);
//                ps[N] = (2 * P[N - 1] - P[N - 2]) * gamma; // (rho_w * g);
//            }
//            else
//            {
//                for (int j = 0; j < P.Length; j++)
//                    ps[j] = P[j] * gamma; // (rho_w * g);
//            }

//            // Расчет коэффициентов  на грани  P--e--E
//            for (int i = 0; i < N; i++)
//            {
//                mtau = Math.Abs(tau[i]);
//                chi = Math.Sqrt(tau0 / mtau);
//                dx = x[i + 1] - x[i];
//                dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                dp = (ps[i + 1] - ps[i]) / dx;
//                // косинус гамма
//                CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                double A = Math.Max(0, 1 - chi);
//                double B = (chi / 2 + A * (1 + s) / s) / tanphi;
//                double C = A / (s * tanphi);
//                // Расход массовый! только для отрисовки !!! 
//                // для расчетов - объемный
//                double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                Gs[idxTransit][i] = G0 * A;
//                Gs[zeta][i] = -G0 * B * dz;
//                Gs[press][i] = -G0 * C * dp;
//                Gs[idxAll][i] = Gs[idxTransit][i] + Gs[zeta][i] + Gs[press][i];
//            }
//            for (int i = 0; i < N - 1; i++)
//            {
//                dx = x[i + 1] - x[i];
//                dGs[idxTransit][i] = (dGs[idxTransit][i+1] - dGs[idxTransit][i]) / dx;
//                dGs[zeta][i] = (dGs[zeta][i + 1] - dGs[zeta][i]) / dx;
//                dGs[press][i] = (dGs[press][i + 1] - dGs[press][i]) / dx;
//                dGs[idxAll][i] = (dGs[idxAll][i + 1] - dGs[idxAll][i]) / dx;
//            }
//        }

//        public void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            if(sp!=null)
//            {

//            }
//        }

//        /// <summary>
//        /// Вычисление изменений формы донной поверхности 
//        /// на одном шаге по времени по модели 
//        /// Петрова А.Г. и Потапова И.И. 2014
//        /// Реализация решателя - методом контрольных объемов,
//        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
//        /// Коэффициенты донной подвижности, определяются 
//        /// как среднее гармонические величины         
//        /// </summary>
//        /// <param name="Zeta0">текущая форма дна</param>
//        /// <param name="tau">придонное касательное напряжение</param>
//        /// <param name="P">придонное давление</param>
//        /// <returns>новая форма дна</returns>
//        /// </summary>
//        public void CalkZetaFDM(ref double[] Zeta, double[] tau, double[] P=null, bool idxTransit = true)
//        {
//            try
//            {
//                MEM.Alloc<double>(Zeta0.Length, ref Zeta);
//                // Расчет деформаций дна от влекомых наносов
//                if (P != null)
//                {
//                    // Петров А.Г. - Потапов И.И. 2014
//                    // Давление в узлах Zeta,  Zeta0
//                    for (int j = 1; j < N; j++)
//                        ps[j] = 0.5 * (P[j] + P[j - 1]) * gamma; // (rho_w * g);
//                                                                 // линейная интерполяция плоха!!! нужна квадратичная
//                    ps[0] = (2 * P[1] - P[2]) * gamma; // (rho_w * g);
//                    ps[N] = (2 * P[N - 1] - P[N - 2]) * gamma; // (rho_w * g);

//                    // Расчет коэффициентов  на грани  P--e--E
//                    for (int i = 0; i < N; i++)
//                    {
//                        mtau = Math.Abs(tau[i]);
//                        chi = Math.Sqrt(tau0 / mtau);
//                        dx = x[i + 1] - x[i];
//                        dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                        dp = (ps[i + 1] - ps[i]) / dx;
//                        // косинус гамма
//                        CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                        A[i] = Math.Max(0, 1 - chi);
//                        B[i] = (chi / 2 + A[i] * (1 + s) / s) / tanphi;
//                        C[i] = A[i] / (s * tanphi);
//                        G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                    }
//                    // расчет коэффициентов схемы
//                    for (int i = 1; i < N; i++)
//                    {
//                        double dxe = x[i + 1] - x[i];
//                        double dxw = x[i] - x[i - 1];
//                        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                        CE[i] = G0[i] * C[i] / dxe;
//                        CW[i] = G0[i - 1] * C[i - 1] / dxw;
//                        CP[i] = CE[i] + CW[i];
//                        AP0[i] = dxp / dtime;
//                        AE[i] = G0[i] * B[i] / dxe;
//                        AW[i] = G0[i - 1] * B[i - 1] / dxw;
//                        AP[i] = AE[i] + AW[i] + AP0[i];
//                        S[i] = CE[i] * ps[i + 1] - CP[i] * ps[i] + CW[i] * ps[i - 1] -
//                            (G0[i] * A[i] - G0[i - 1] * A[i - 1]) + AP0[i] * Zeta0[i];
//                    }
//                }
//                else
//                {
//                    Console.WriteLine(" G ");
//                    // Петров П.Г. 1991
//                    // Расчет коэффициентов  на грани  P--e--E
//                    for (int i = 0; i < N; i++)
//                    {
//                        mtau = Math.Abs(tau[i]);
//                        chi = Math.Sqrt(tau0 / mtau);
//                        dx = x[i + 1] - x[i];
//                        dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                        dp = (ps[i + 1] - ps[i]) / dx;
//                        A[i] = Math.Max(0, 1 - Math.Sqrt(chi));
//                        B[i] = (chi / 2 + A[i]) / tanphi;
//                        // косинус гамма
//                        CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                        G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];

//                        double G = G0[i] * (A[i] - B[i] * dz);
//                        Console.Write(" " + G.ToString("F9"));
//                    }

//                    Console.WriteLine();
//                    Console.WriteLine(" S ");

//                    // расчет коэффициентов схемы
//                    for (int i = 1; i < N; i++)
//                    {
//                        double dxe = x[i + 1] - x[i];
//                        double dxw = x[i] - x[i - 1];
//                        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                        AP0[i] = dxp / dtime;
//                        AE[i] = G0[i] * B[i] / dxe;
//                        AW[i] = G0[i - 1] * B[i - 1] / dxw;
//                        AP[i] = AE[i] + AW[i] + AP0[i];
//                        double dA = - (G0[i] * A[i] - G0[i - 1] * A[i - 1]);
//                        Console.Write(" " + dA.ToString("F5"));
//                        S[i] = dA + AP0[i] * Zeta0[i];
//                    }
//                    Console.WriteLine();
//                }
//                //PrintMas("AW", AW);
//                //PrintMas("AP", AP);
//                //PrintMas("AE", AE);
//                //PrintMas("S", S);
//                //Console.WriteLine();
//                //PrintMatrix();
//                if (BCBed.Inlet == TypeBoundCond.Neumann)
//                {
//                    Gtran_in = G0[0] * A[0];
//                    AE[0] = AW[1];
//                    AP[0] = AW[1];
//                    S[0] = BCBed.InletValue - Gtran_in;
//                }
//                if (BCBed.Outlet == TypeBoundCond.Neumann)
//                {
//                    Gtran_out = G0[N - 1] * A[N - 1];
//                    AP[N] = AE[N - 1];
//                    AW[N] = AE[N - 1];
//                    if (idxTransit == true)
//                        S[N] = 0;
//                    else
//                        S[N] = BCBed.OutletValue - Gtran_out;
//                }
//                //PrintMas("AW", AW);
//                //PrintMas("AP", AP);
//                //PrintMas("AE", AE);
//                //PrintMas("S", S);
//                //Console.WriteLine();
//                //PrintMatrix();
//                // Прогонка
//                Solver solver = new Solver(Count);
//                solver.SetSystem(AW, AP, AE, S, Zeta);
//                // выполнение граничных условий Dirichlet
//                solver.CalkBCondition(BCBed);
//                Zeta = solver.SolveSystem();
//                //PrintMas("AW", AW);
//                //PrintMas("AP", AP);
//                //PrintMas("AE", AE);
//                //PrintMas("S", S);
//                //Console.WriteLine();
//                //PrintMatrix();
//                // Zeta = solver.SolveSystem();
//                // Сглаживание дна по лавинной моделе
//                if (isAvalanche == true)
//                    SAvalanche.Lavina(Zeta, x, tanphi, 0.6, 0);
//                // переопределение начального значения zeta 
//                // для следующего шага по времени
//                for (int j = 0; j < Zeta.Length; j++)
//                    Zeta0[j] = Zeta[j];

//            }
//            catch (Exception e)
//            {
//                Message = e.Message;
//                for (int j = 0; j < Zeta.Length; j++)
//                    Zeta[j] = Zeta0[j];
//            }
//        }

//        public static void Test0()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double d50 = 0.001;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double f = 0.1;
//            double cx = 0.5;
//            BedLoadParams blp = new BedLoadParams(rho_w, rho_s, phi, d50, epsilon, kappa, cx, f);
//            BedLoadParams blp1 = new BedLoadParams();
//            CBedLoadTask bltask = new CBedLoadTask(blp);
//            //CBedLoadTask bltask = new CBedLoadTask(rho_w, rho_s, phi, d50, epsilon, kappa, cx, f);
//            // задача Дирихле
//            BCondition BCBed = new BCondition(TypeBoundCond.Dirichlet, TypeBoundCond.Dirichlet, 1, 1);

//            int NN = 15;
//            double dx = 1.0 / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            double[] Zeta = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                //Zeta0[i] = 0.2;
//                Zeta0[i] = 1.0;
//            }
//            double dtime = 1; // 0.01;
//            bool isAvalanche = false;
//            //bool isAvalanche = true;
//            bltask.SetTask(null,BCBed, x, Zeta0, dtime, isAvalanche);
//            double T = 2 * bltask.tau0;
//            double[] tau = new double[NN - 1];
//            double[] P = new double[NN - 1];

//            for (int i = 0; i < NN - 1; i++)
//            {
//                tau[i] = T;
//                P[i] = 1;
//            }

//            for (int i = 0; i < 50; i++)
//            {
//                bltask.CalkZetaFDM(ref Zeta, tau, P);
//                bltask.PrintMas("zeta", Zeta);
//            }
//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }

//        public static void Test1()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double d50 = 0.001;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double f = 0.1;
//            double cx = 0.5;
//            CBedLoadTask bltask = new CBedLoadTask(rho_w, rho_s, phi, d50, epsilon, kappa, cx, f);
//            // задача Дирихле
//            BCondition BCBed = new BCondition(TypeBoundCond.Dirichlet, TypeBoundCond.Dirichlet, 1, 1);

//            int NN = 15;
//            double dx = 1.0 / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            double[] Zeta = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                Zeta0[i] = 0.2;
//            }
//            double dtime = 1; // 0.01;
//            bool isAvalanche = false;
//            //bool isAvalanche = true;
//            bltask.SetTask(null, BCBed, x, Zeta0, dtime, isAvalanche);
//            double T = 2 * bltask.tau0;
//            double[] tau = new double[NN - 1];
//            double[] P = new double[NN - 1];

//            for (int i = 0; i < NN - 1; i++)
//            {
//                tau[i] = T;
//                P[i] = 1;
//            }
//            for (int i = 0; i < 50; i++)
//            {
//                bltask.CalkZetaFDM(ref Zeta, tau, P);
//                bltask.PrintMas("zeta", Zeta);
//            }
//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }

//        public static void Test2()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double d50 = 0.001;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double f = 0.1;
//            double cx = 0.5;
//            CBedLoadTask bltask = new CBedLoadTask(rho_w, rho_s, phi, d50, epsilon, kappa, cx, f);
//            // задача Дирихле
//            //BCondition BCBed = new BCondition(TypeBoundCond.Dirichlet, TypeBoundCond.Dirichlet, 0.25, 0.25);
//            // задача Неймана
//            double alphaIn = 1.2;
//            double alphaOut = 0.8;
//            double G_transite = .2059325214e-4;
//            BCondition BCBed = new BCondition(TypeBoundCond.Neumann, TypeBoundCond.Neumann, alphaIn * G_transite, alphaOut * G_transite);

//            int NN = 4;
//            double dx = 1.0 / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            double[] Zeta = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                Zeta0[i] = 0.2;
//            }
//            double dtime = 1; // 0.01;
//            bool isAvalanche = true;
//            bltask.SetTask(null, BCBed, x, Zeta0, dtime, isAvalanche);
//            double T = 2 * bltask.tau0;
//            double[] tau = new double[NN - 1];
//            double[] P = new double[NN - 1];

//            for (int i = 0; i < NN - 1; i++)
//            {
//                tau[i] = T;
//                P[i] = 1;
//            }
//            for (int i = 0; i < 50; i++)
//            {
//                bltask.CalkZetaFDM(ref Zeta, tau, P);
//                bltask.PrintMas("zeta", Zeta);
//            }
//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }

//        public static void Test3()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double d50 = 0.001;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double f = 0.1;
//            double cx = 0.5;
//            CBedLoadTask bltask = new CBedLoadTask(rho_w, rho_s, phi, d50, epsilon, kappa, cx, f);
//            // задача Дирихле
//            BCondition BCBed = new BCondition(TypeBoundCond.Dirichlet, TypeBoundCond.Dirichlet, 0.1, 0.1);
//            int NN = 12;
//            double L = 1;
//            double dx = L / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            double[] Zeta = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                Zeta0[i] = 0.1;
//            }
//            double dtime = 1; // 0.01;
//            bool isAvalanche = true;
//            bltask.SetTask(null, BCBed, x, Zeta0, dtime, isAvalanche);
//            double[] tau = new double[NN - 1];
//            double[] P = new double[NN - 1];

//            for (int i = 0; i < NN - 1; i++)
//            {
//                P[i] = 1;
//            }
//            for (int i = 0; i < NN - 1; i++)
//            {
//                double T = 0.0;

//                if (x[i] < 1 / 2.0)
//                    T = - 16 * x[i] * (x[i] - L / 2.0) / (L * L);
//                // tau = 0.05;
//                Zeta0[i] = 0.1;
//                tau[i] = 5 * T;
//            }
//            for (int i = 0; i < 50; i++)
//            {
//                bltask.CalkZetaFDM(ref Zeta, tau, P);
//                bltask.PrintMas("zeta", Zeta);
//            }
//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }


//        //public static void Main()
//        //{
//        //    // Гру
//        //    //Test0();
//        //    // Test1();
//        //    // Test2();
//        //    Test3();
//        //}
//    }
//}
