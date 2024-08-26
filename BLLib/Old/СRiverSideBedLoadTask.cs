////---------------------------------------------------------------------------
////              Реализация библиотеки для моделирования 
////               гидродинамических и русловых процессов
////                      - (C) Copyright 2019 -
////                       ALL RIGHT RESERVED
////                        ПРОЕКТ "BLLib"
////---------------------------------------------------------------------------
////                   разработка: Потапов И.И.
////                          11.12.19
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
//    /// <summary>
//    /// ОО: Класс для решения одномерной задачи о 
//    /// расчете береговых деформаций русла в створе реки
//    /// </summary>
//    [Serializable]
//    public class СRiverSideBedLoadTask : BedLoadParams, IBedLoadTask
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
//        public double tau0=0;
//        /// <summary>
//        /// относительная плотность
//        /// </summary>
//        public double rho_b;
//        /// <summary>
//        /// коэффициент сухого трения
//        /// </summary>
//        public double Fa0;
//        /// <summary>
//        /// константа расхода влекомых наносов
//        /// </summary>
//        public double G1;
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
//        /// <summary>
//        /// Погрешность при вычислении коэффициентов
//        /// </summary>
//        double ErrAE = 0.000000001;
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
//        /// <summary>
//        /// Флаг для определения сухого-мокрого дна
//        /// </summary>
//        public int[] DryWet = null;
//        /// <summary>
//        /// Коэффициенты "диффузии" дна
//        /// </summary>
//        double[] G0 = null;
//        /// <summary>
//        /// параметр Курранта для дифф. уравнения
//        /// </summary>
//        public double Kurant()
//        {
//            double kurant;
//            double minL = double.MaxValue;
//            for (int i = 0; i < x.Length - 1; i++)
//            {
//                double dx = x[i + 1] - x[1];
//                if (dx < minL)
//                    minL = dx;
//            }
//            double minD = G1 * tau0 * Math.Sqrt(tau0);
//            kurant = minL / (2.0 * minD);
//            return kurant;
//        }
//        double[] S = null;
//        double[] AE = null;
//        double[] AW = null;
//        double[] AP = null;
//        double[] AP0 = null;
//        double dz, dx;
//        double mtau, chi;
//        #endregion

//        /// <summary>
//        /// Конструктор по умолчанию/тестовый
//        /// </summary>
//        public СRiverSideBedLoadTask(BedLoadParams p): base (p)
//        {
//            InitBedLoad();
//        }
//        /// <summary>
//        /// Конструктор с параметрами/тестовый
//        /// </summary>
//        public СRiverSideBedLoadTask(double rho_w, double rho_s, double phi, 
//        double d50, double epsilon, double kappa, double cx)
//        {
//            this.rho_w = rho_w;
//            this.rho_s = rho_s;
//            this.phi = phi;
//            this.d50 = d50;
//            this.epsilon = epsilon;
//            this.kappa = kappa;
//            this.cx = cx;
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

//            CosGamma = new double[N];
//            G0 = new double[N];
//            S = new double[Count];
//            DryWet = new int[Count];

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
//        /// <summary>
//        /// Вычисление текущего поперечного расхода 
//        /// в затопленном створе реки для построения графиков
//        /// </summary>
//        /// <param name="tau">напряжения на дне</param>
//        /// <param name="Gs">расход</param>
//        /// <param name="dGs">градиент расхода</param>
//        public void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
//        {
//            if (Gs == null)
//            {
//                Gs = new double[1][];
//                dGs = new double[1][];
//                for(int i=0; i<Gs.Length; i++)
//                {
//                    Gs[i] = new double[tau.Length];
//                    dGs[i] = new double[tau.Length];
//                }
//            }
//            for (int i = 0; i < N; i++)
//            {
//                if (DryWet[i] > 1)
//                {
//                    mtau = Math.Abs(tau[i]);
//                    chi = Math.Sqrt(tau0 / mtau);
//                    dx = x[i + 1] - x[i];
//                    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                    // косинус гамма
//                    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                    // Расход массовый! только для отрисовки !!! 
//                    // для расчетов - объемный
//                    double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                    Gs[0][i] = G0*dz/dx;
//                }
//                else
//                    Gs[0][i] = 0;
//            }
//            for (int i = 0; i < N; i++)
//            {
//                if (DryWet[i] > 0)
//                {
//                    dx = x[i + 1] - x[i];
//                    dGs[0][i] = (Gs[0][i + 1] - Gs[0][i]) / dx;
//                }
//                else
//                {
//                    dGs[0][i] = 0;
//                }
//            }
//        }
//        public void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            if (sp != null)
//            {

//            }
//        }
//        /// <summary>
//        /// Транзитний расход наносов через сечение 
//        /// (или половина расхода если есть симметрия в расчетной области)
//        /// </summary>
//        /// <param name="tau">напряжения на дне</param>
//        /// <param name="J">уклон русла по потоку</param>
//        public double GStream(double[] tau, double J)
//        {
//            double Q = 0;
//            for (int i = 1; i < tau.Length; i++)
//            {
//                if (DryWet[i] > 0)
//                {
//                    mtau = Math.Abs(tau[i]);
//                    chi = Math.Sqrt(tau0 / mtau);
//                    dx = x[i] - x[i-1];
//                    double A = Math.Max(0, 1 - chi);
//                    double B = (chi / 2 + A) / tanphi;
//                    // Расход массовый! только для отрисовки и инфо! 
//                    double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau);
//                    Q += G0 * (A + B * J) * dx;
//                }
//            }
//            return Q;
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
//        /// <returns>новая форма дна</returns>
//        /// </summary>
//        public void CalkZetaFDM(ref double[] Zeta, double[] tau, double[] P = null, bool GloverFlory = false)
//        {
//            try
//            {
//                if (Zeta == null)
//                    Zeta = new double[Zeta0.Length];

//                double tauGF, dtau=0;
//                // Расчет коэффициентов  на грани  P--e--E
//                for (int i = 0; i < N; i++)
//                {
//                    dx = x[i + 1] - x[i];
//                    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                    // косинус гамма
//                    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                    mtau = Math.Abs(tau[i]);
//                    if (GloverFlory == true)
//                    {

//                        // напряжение не размывающее склон Гловера - Флори
//                        // ограничение на уклон дна
//                        double arg = Math.Min(1, dz * dz / (tanphi * tanphi));
//                        // критическое напряжение по Гловеру - Флори
//                        tauGF = Fa0 * d50 * Math.Sqrt(1 - arg);
//                        dtau = Math.Max(0, tau[i] - tauGF);
//                        G0[i] = G1 * dtau * Math.Sqrt(mtau) / CosGamma[i];
//                    }
//                    else 
//                    {
//                        G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                    }
//                    // Контроль сухой (не размываемой) части берега
//                    if (Math.Abs(G0[i]) < ErrAE)
//                        DryWet[i] = 0;
//                    else
//                    {
//                        if (GloverFlory == true)
//                        {
//                            if (dtau < ErrAE)
//                            {
//                                G0[i] = 0;
//                                DryWet[i] = 1;
//                            }
//                            else
//                                DryWet[i] = 2;
//                        }
//                        else
//                            DryWet[i] = 2;
//                    }
//                }
//                if(DryWet[N-1]>0)
//                    DryWet[N] = DryWet[N-1];
//                if (DryWet[1] > 0)
//                    DryWet[0] = DryWet[1];
//                // расчет коэффициентов схемы
//                for (int i = 1; i < N - 1; i++)
//                {
//                    if (DryWet[i] < 2)
//                    {
//                        // сухая область берега
//                        AE[i] = 0;
//                        AW[i] = 0;
//                        AP[i] = 1;
//                        S[i] = Zeta0[i];
//                    }
//                    else
//                    {
//                        // затопленная часть
//                        double dxe = x[i + 1] - x[i];
//                        double dxw = x[i] - x[i - 1];
//                        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                        AP0[i] = dxp / dtime;
//                        // выбор способа осреднения коэййициентов диффузии
//                        double tmpGe = G0[i] + G0[i + 1];
//                        double tmpGw = G0[i] + G0[i - 1];
//                        if (Math.Abs(tmpGe) < ErrAE)
//                            AE[i] = G0[i + 1] / dxe;
//                        else
//                            AE[i] = 2 * G0[i] * G0[i + 1] / (tmpGe * dxe);
//                        if (Math.Abs(tmpGw) < ErrAE)
//                            AW[i] = G0[i - 1] / dxw;
//                        else
//                            AW[i] = 2 * G0[i] * G0[i - 1] / (tmpGw * dxw);
//                        // расчет ведущего коэффициента
//                        AP[i] = AE[i] + AW[i] + AP0[i];
//                        // расчет правой части
//                        S[i] = AP0[i] * Zeta0[i];
//                    }
//                }
//                {
//                    int i = N - 1;
//                    if (DryWet[i] < 2)
//                    {
//                        // сухая область берега
//                        AE[i] = 0;
//                        AW[i] = 0;
//                        AP[i] = 1;
//                        S[i] = Zeta0[i];
//                    }
//                    else
//                    {
//                        double dxe = x[i + 1] - x[i];
//                        double dxw = x[i] - x[i - 1];
//                        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                        AP0[i] = dxp / dtime;

//                        AE[i] = G0[i] / dxe;
//                        AW[i] = 2 * G0[i] * G0[i - 1] / ((G0[i] + G0[i - 1]) * dxw);

//                        AP[i] = AE[i] + AW[i] + AP0[i];

//                        S[i] = AP0[i] * Zeta0[i];
//                    }
//                }
//                //PrintMas("AW", AW);
//                //PrintMas("AP", AP);
//                //PrintMas("AE", AE);
//                //PrintMas("S", S);
//                //Console.WriteLine();
//                //PrintMatrix();
//                if (BCBed.Inlet == TypeBoundCond.Neumann)
//                {
//                    if (DryWet[0] < 2) // если берег сухой
//                    {
//                        AP[0] = 1;
//                        S[0] = Zeta0[0];
//                    }
//                    else // затопленный берег
//                    {
//                        AE[0] = AW[1];
//                        AP[0] = AW[1];
//                        S[0] = 0;
//                    }
//                }
//                if (BCBed.Outlet == TypeBoundCond.Neumann)
//                {
//                    if (DryWet[N] < 2) // если берег сухой
//                    {
//                        AP[N] = 1;
//                        S[N] = Zeta0[N];
//                    }
//                    else // затопленный берег
//                    {
//                        AP[N] = AE[N - 1];
//                        AW[N] = AE[N - 1];
//                        S[N] = 0;
//                    }
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
//                {
//                    bool fn = double.IsNaN(Zeta[j]);
//                    if (fn == false)
//                        Zeta0[j] = Zeta[j];
//                    else
//                        throw new Exception("реализовалось деление на ноль");
//                }

//            }
//            catch (Exception e)
//            {
//                Message = e.Message;
//            }
//        }
//        /// <summary>
//        /// Затопленный уступ - гидростатика
//        /// </summary>
//        public static void Test0()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double d50 = 0.001;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double cx = 0.5;
//            BedLoadParams blp = new BedLoadParams(rho_w, rho_s, phi, d50, epsilon, kappa, cx);

//            СRiverSideBedLoadTask bltask = new СRiverSideBedLoadTask(blp);
//            // задача Дирихле
//            BCondition BCBed = new BCondition(TypeBoundCond.Neumann, TypeBoundCond.Neumann, 0, 0);

//            int NN = 30;
//            double dx = 1.0 / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                if(i<10)
//                    Zeta0[i] = 0.3;
//                if (i >= 10 && i < 20)
//                    Zeta0[i] = 0.3-0.01*(i-10);
//                if (i >= 20 && i < NN)
//                    Zeta0[i] = 0.2;
//            }

//            bltask.PrintMas("zeta0", Zeta0,4);
//            double dtime =  0.1;
//            //bool isAvalanche = false;
//            bool isAvalanche = true;
//            bltask.SetTask(null, BCBed, x, Zeta0, dtime, isAvalanche);
//            double T = 2 * bltask.tau0;
//            double[] tau = new double[NN - 1];
//            Console.WriteLine("Tau_c {0}", bltask.tau0);
//            // гидростатика
//            double J = 0.01;
//            double g = 9.81;
//            double eta = 0.35;
//            for (int i = 0; i < NN - 1; i++)
//            {
//                tau[i] = rho_w * g * J * (eta - 0.5 * (Zeta0[i] + Zeta0[i + 1]));
//            }
//            bltask.PrintMas("Tau", tau, 3);
//            for (int t = 0; t < 5; t++)
//            {
//                double[] Zeta = null; 
//                bltask.CalkZetaFDM(ref Zeta, tau);
//                bltask.PrintMas("zeta", Zeta, 4);
//                for (int i = 0; i < NN - 1; i++)
//                {
//                    tau[i] = rho_w * g * J * (eta - 0.5 * (Zeta[i] + Zeta[i + 1]));
//                }
//                bltask.PrintMas("Tau", tau, 3);
//            }
//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }
//        /// <summary>
//        /// Сухой берег - гидростатика
//        /// </summary>
//        public static void Test1()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double d50 = 0.001;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double cx = 0.5;
//            BedLoadParams blp = new BedLoadParams(rho_w, rho_s, phi, d50, epsilon, kappa, cx);
//            СRiverSideBedLoadTask bltask = new СRiverSideBedLoadTask(blp);
//            // задача Дирихле
//            BCondition BCBed = new BCondition(TypeBoundCond.Neumann, TypeBoundCond.Neumann, 0, 0);
//            int NN = 30;
//            double dx = 1.0 / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                if (i < 10)
//                    Zeta0[i] = 0.4;
//                if (i >= 10 && i < 20)
//                    Zeta0[i] = 0.4 - 0.02 * (i - 10);
//                if (i >= 20 && i < NN)
//                    Zeta0[i] = 0.2;
//            }

//            bltask.PrintMas("zeta0", Zeta0, 4);
//            double dtime = 0.1;
//            //bool isAvalanche = false;
//            bool isAvalanche = true;
//            bltask.SetTask(null, BCBed, x, Zeta0, dtime, isAvalanche);
//            double T = 2 * bltask.tau0;
//            double[] tau = new double[NN - 1];
//            Console.WriteLine("Tau_c {0}", bltask.tau0);
//            // гидростатика
//            double J = 0.001;
//            double g = 9.81;
//            double eta = 0.35;
//            for (int i = 0; i < NN - 1; i++)
//            {
//                double Tau = rho_w * g * J * (eta - 0.5 * (Zeta0[i] + Zeta0[i + 1]));
//                tau[i] = Math.Max(0, Tau);
//            }
//            bltask.PrintMas("Tau", tau, 3);
//            for (int t = 0; t < 50; t++)
//            {
//                double[] Zeta = null;
//                bltask.CalkZetaFDM(ref Zeta, tau);
//                bltask.PrintMas("zeta", Zeta, 4);
//                for (int i = 0; i < NN - 1; i++)
//                {
//                    double Tau = rho_w * g * J * (eta - 0.5 * (Zeta[i] + Zeta[i + 1]));
//                    tau[i] = Math.Max(0, Tau);
//                }
//                bltask.PrintMas("Tau", tau, 3);
//            }
//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }
//    }
//}
