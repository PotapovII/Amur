//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.09.22
//---------------------------------------------------------------------------
//                Модуль BLLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//---------------------------------------------------------------------------
namespace BLLib
{
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;
    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете береговых деформаций русла в створе реки
    /// реализованный в потоках наносов 
    /// </summary>
    [Serializable]
    public class BedLoadStreamTask_1YD : ABedLoadTask1D
    {
        /// <summary>
        /// Параметр Шильдса
        /// </summary>
        double[] Theta = null;
        double normaTheta;
        public override IBedLoadTask Clone()
        {
            return new BedLoadStreamTask_1YD(new BedLoadParams());
        }
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public BedLoadStreamTask_1YD(BedLoadParams p) : base(p)
        {
            InitBedLoad();
            normaTheta = SPhysics.PHYS.normaTheta;
            name = "деформация поперечного одно-фракционного дна в Q.";
            tTask = TypeTask.streamY1D;
        }
            
        public override void InitBedLoad()
        {
            base.InitBedLoad();
            normaTheta = SPhysics.PHYS.normaTheta;
        }
        /// <summary>
        /// Вычисление текущего поперечного расхода 
        /// в затопленном створе реки для построения графиков
        /// </summary>
        /// <param name="tau">напряжения на дне</param>
        /// <param name="Gs">расход</param>
        /// <param name="dGs">градиент расхода</param>
        public override void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
        {
            if (tau == null)
                tau = this.tau;
            if (tau == null)
                return;
            if (blm == TypeBLModel.BLModel_1991)
            {
                double tanphi = SPhysics.PHYS.tanphi;
                double G1 = SPhysics.PHYS.G1;
                double tau0 = SPhysics.PHYS.tau0;

                MEM.Alloc2DClear(3, Count, ref Gs);
                MEM.Alloc2DClear(3, Count, ref dGs);
                double Ai, Bi, G0;
                for (int i = 0; i < N; i++)
                {
                    if (DryWet[i] > 1)
                    {
                        mtau = Math.Abs(tau[i]);
                        chi = Math.Sqrt(tau0 / mtau);
                        //dx = x[i + 1] - x[i];
                        //dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                        // косинус гамма
                        //CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                        Ai = Math.Max(0, 1 - Math.Sqrt(chi));
                        Bi = (chi / 2 + A[i]) / tanphi;
                        // Расход массовый! только для отрисовки !!! 
                        // для расчетов - объемный
                        G0 = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                        Gs[idxTransit][i] = G0 * Ai;
                        double D = 4.0 / 5.0 / tanphi / CosGamma[i];
                        Gs[idxZeta][i] = -D * G0 * dZeta[i] / dx;
                    }
                    else
                    {
                        Gs[idxTransit][i] = 0;
                        Gs[idxZeta][i] = 0;
                    }
                    Gs[idxAll][i] = Gs[idxTransit][i] + Gs[idxZeta][i];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    if (DryWet[i] > 0)
                    {
                        dx = x[i + 1] - x[i];
                        dGs[idxTransit][i] = (dGs[idxTransit][i + 1] - dGs[idxTransit][i]) / dx;
                        dGs[idxZeta][i] = (dGs[idxZeta][i + 1] - dGs[idxZeta][i]) / dx;
                        dGs[idxAll][i] = dGs[idxTransit][i] + dGs[idxZeta][i];
                    }
                    else
                    {
                        dGs[idxTransit][i] = 0;
                        dGs[idxZeta][i] = 0;
                        dGs[idxAll][i] = 0;
                    }
                }
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                double tau0 = SPhysics.PHYS.tau0;
                double[][] Gs = null;
                double[][] dGs = null;
                Calk_Gs(ref Gs, ref dGs, tau);
                if (Gs != null)
                {
                    for (int i = 0; i < X.Length; i++)
                    {
                        X[i] = 0.5 * (x[i + 1] + x[i]);
                        //dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                        //tau0Elem[i] = tau0 * (1 + dz / tanphi);
                        tau0Elem[i] = tau0;
                        zetaElem[i] = 0.5 * (Zeta0[i + 1] + Zeta0[i]) * 100;
                    }
                    sp.AddCurve("Отметки дна", X, Zeta);

                    MEM.AllocClear(Count, ref Theta);
                    for (int i = 0; i < tau.Length; i++)
                        Theta[i] = tau[i] / normaTheta;
                    sp.AddCurve("Параметр Шильдса", X, Theta);

                    if (Gs.Length == 3)
                    {
                        sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit]);
                        sp.AddCurve("Гравитационный расход наносов", X, Gs[idxZeta]);
                        sp.AddCurve("Полный расход наносов", X, Gs[idxAll]);

                        sp.AddCurve("Градиент транзитного расхода наносов", X, dGs[idxTransit]);
                        sp.AddCurve("Градиент гравитационного расхода наносов", X, dGs[idxZeta]);
                        sp.AddCurve("Градиент полного расхода наносов", X, dGs[idxAll]);
                    }
                    else
                    {
                        sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit]);
                        sp.AddCurve("Гравитационный расход наносов", X, Gs[idxZeta]);
                        sp.AddCurve("Напоный расход наносов", X, Gs[idxPress]);
                        sp.AddCurve("Полный расход наносов", X, Gs[idxAll]);

                        sp.AddCurve("Градиент транзитного расхода наносов", X, dGs[idxTransit]);
                        sp.AddCurve("Градиент гравитационного расхода наносов", X, dGs[idxZeta]);
                        sp.AddCurve("Градиент напорного расхода наносов", X, dGs[idxPress]);
                        sp.AddCurve("Градиент полного расхода наносов", X, dGs[idxAll]);
                    }
                }
            }
        }
        /// <summary>
        /// Транзитний расход наносов через сечение 
        /// (или половина расхода если есть симметрия в расчетной области)
        /// </summary>
        /// <param name="tau">напряжения на дне</param>
        /// <param name="J">уклон русла по потоку</param>
        public double GStream(double[] tau, double J)
        {
            double Q = 0;
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double rho_s = SPhysics.PHYS.rho_s;

            for (int i = 1; i < tau.Length; i++)
            {
                if (DryWet[i] > 0)
                {
                    mtau = Math.Abs(tau[i]);
                    chi = Math.Sqrt(tau0 / mtau);
                    dx = x[i] - x[i - 1];
                    double A = Math.Max(0, 1 - chi);
                    double B = (chi / 2 + A) / tanphi;
                    // Расход массовый! только для отрисовки и инфо! 
                    double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau);
                    Q += G0 * (A + B * J) * dx;
                }
            }
            return Q;
        }
        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta0">текущая форма дна</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <returns>новая форма дна</returns>
        /// </summary>
        public override void CalkZetaFDM(ref double[] Zeta, double[] tau, double[] tauY = null, double[] P = null, double[][] CS = null)
        {
            try
            {
                //double s = SPhysics.PHYS.s;
                double tanphi = SPhysics.PHYS.tanphi;
                double rho_b = SPhysics.PHYS.rho_b;
                double G1 = SPhysics.PHYS.G1;
                double epsilon = SPhysics.PHYS.epsilon;
                double tau0 = SPhysics.PHYS.tau0;
                //double g = SPhysics.GRAV;
                double kappa = SPhysics.PHYS.kappa;
                double rho_w = SPhysics.rho_w;
                double Fa0 = SPhysics.PHYS.Fa0;
                double rho_s = SPhysics.PHYS.rho_s;
                double gamma = SPhysics.PHYS.gamma;
                double RaC = SPhysics.PHYS.RaC;
                double Ws = SPhysics.PHYS.Ws;
                double d50 = SPhysics.PHYS.d50;
                double nu = SPhysics.nu;

                this.tau = tau;
                this.P = P;
                MEM.Alloc(Zeta0.Length, ref Zeta);


                #region расчет расходов влекомых наносов
                
                double dz = 0, mdz = 0;
                double Wc = 0.1;
                double tauC0, K, tauC;
                double C0 = 16.0 / (15.0 * kappa * Math.Sqrt(rho_w) * Fa0 * (1 - epsilon) * tanphi * tanphi);
                double[] dZ = new double[Zeta.Length];
                //производная по поверхности дна 
                for (int i = 0; i < Zeta.Length - 1; i++)
                    dZ[i] = (Zeta0[i + 1] - Zeta0[i]) / dx;
                for (int i = 0; i < Zeta0.Length - 1; i++)
                {
                    dz = dZ[i];
                    mdz = Math.Abs(dz);
                    // косинус гамма
                    double CosGamma1 = Math.Sqrt(1 / (1 + dz * dz));
                    // Критические напряжения
                    tauC0 = CosGamma1 * tau0;
                    K = dz / tanphi;
                    // напряжение начала трогания донных частиц
                    tauC = tauC0 * (1 + K);
                    // при высоких уклонах диаметр частиц не влияет на процесс обрушения
                    if (K <= -1)
                        tauC = 0;
//double J = 0.002;
//double CWater = 0.1;
                    //sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit]);
                    //sp.AddCurve("Гравитационный расход наносов", X, Gs[idxZeta]);
                    //                   double CritDeep = tauC / (rho_w * g * J);
                    //                    CritH[i] = CWater - CritDeep + Zeta[0];
                    // Поперечный расход
                    if (tau[i] > tauC)
                    {
                        Gs[idxTransit][i] = 0;
                        Gs[idxZeta][i] = - C0 * Math.Pow(tau[i], 1.5) * dz;///CosGamma; 
                    }
                    else
                    {
                        Gs[idxTransit][i] = 0;
                        Gs[idxZeta][i] = 0; 
                    }
                }
                 // ГУ
                Gs[idxZeta][Gs.Length - 1] = Gs[idxZeta][Gs.Length - 2];
                #endregion
                // расчет текущего шага по времени
                dGs[idxZeta][0] = 0;  
                for (int i = 1; i < Gs.Length; i++)
                    dGs[idxZeta][i] = Gs[idxZeta][i] - Gs[idxZeta][i - 1];

                dGs[idxZeta][0] = dGs[idxZeta][1];
                double WC = dtime / (1 - epsilon) / dx;
                double[] dZeta = new double[Zeta.Length];
                for (int i = 0; i < Zeta.Length; i++)
                    dZeta[i] = - Wc * dGs[idxZeta][i];

                //расчет донных изменений Zeta
                for (int i = 0; i < Zeta.Length; i++)
                {
                    if (Math.Abs(dZeta[i]) > 0.2)
                        throw new Exception("Бяда!");
                    Zeta[i] = Zeta[i] + dZeta[i];
                }
    
                //// Расчет производных от геометрии
                //double dz;
                //for (int i = 0; i < N; i++)
                //{
                //    dx = x[i + 1] - x[i];
                //    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                //    dZeta[i] = dz;
                //    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                //    // критическое напряжение
                //    tau0Elem[i] = Math.Max(0, tau0 * (1 + dz / tanphi));
                //}

                ////double tauGF, dtau = 0;
                //// Расчет коэффициентов  на грани  P--e--E
                //for (int i = 0; i < N; i++)
                //{
                //    mtau = Math.Abs(tau[i]);
                //    if (tau[i] < MEM.Error8)
                //    {
                //        // сухая - не размываемая часть берега
                //        DryWet[i] = 0;
                //        G0[i] = 0;
                //        A[i] = 0;
                //    }
                //    else
                //    {
                //        //chi = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0 / mtau));
                //        chi = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0Elem[i] / mtau));
                //        if (chi >= 1)
                //        {
                //            DryWetEelm[i] = 0; // мокрая -  не размываемая часть дна
                //            A[i] = 0;
                //            G0[i] = 0;
                //            DryWet[i] = 1;
                //        }
                //        else
                //        {
                //            DryWet[i] = 2; // мокрая - размываемая часть дна
                //            A[i] = Math.Max(0, 1 - chi);
                //            G0[i] = G1 * tau[i] * Math.Sqrt(mtau) * A[i] / CosGamma[i];
                //        }
                //    }
                //}
                //if (DryWet[N - 1] > 0)
                //    DryWet[N] = DryWet[N - 1];
                //if (DryWet[1] > 0)
                //    DryWet[0] = DryWet[1];
                //// расчет коэффициентов схемы
                //for (int i = 1; i < N - 1; i++)
                //{
                //    if (DryWet[i] < 2)
                //    {
                //        // не деформируемая часть дна
                //        AE[i] = 0;
                //        AW[i] = 0;
                //        AP[i] = 1;
                //        S[i] = Zeta0[i];
                //    }
                //    else
                //    {
                //        // затопленная часть
                //        double dxe = x[i + 1] - x[i];
                //        double dxw = x[i] - x[i - 1];
                //        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
                //        AP0[i] = dxp / dtime;
                //        // выбор способа осреднения коэййициентов диффузии

                //        double tmpGe = G0[i] + G0[i + 1];
                //        double tmpGw = G0[i] + G0[i - 1];
                //        if (Math.Abs(tmpGe) < MEM.Error10)
                //            AE[i] = G0[i + 1] / dxe;
                //        else
                //            AE[i] = 2 * G0[i] * G0[i + 1] / (tmpGe * dxe);

                //        if (Math.Abs(tmpGw) < MEM.Error10)
                //            AW[i] = G0[i - 1] / dxw;
                //        else
                //            AW[i] = 2 * G0[i] * G0[i - 1] / (tmpGw * dxw);

                //        // расчет ведущего коэффициента
                //        AP[i] = AE[i] + AW[i] + AP0[i];
                //        // расчет правой части
                //        S[i] = AP0[i] * Zeta0[i];
                //    }
                //}
                //// последний узел
                //{
                //    int i = N - 1;
                //    if (DryWet[i] < 2)
                //    {
                //        // сухая область берега
                //        AE[i] = 0;
                //        AW[i] = 0;
                //        AP[i] = 1;
                //        S[i] = Zeta0[i];
                //    }
                //    else
                //    {
                //        double dxe = x[i + 1] - x[i];
                //        double dxw = x[i] - x[i - 1];
                //        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
                //        AP0[i] = dxp / dtime;

                //        AE[i] = G0[i] / dxe;
                //        AW[i] = 2 * G0[i] * G0[i - 1] / ((G0[i] + G0[i - 1]) * dxw);

                //        AP[i] = AE[i] + AW[i] + AP0[i];

                //        S[i] = AP0[i] * Zeta0[i];
                //    }
                //}
                ////PrintMas("AW", AW);
                ////PrintMas("AP", AP);
                ////PrintMas("AE", AE);
                ////PrintMas("S", S);
                ////Console.WriteLine();
                ////PrintMatrix();
                //if (BCondIn.typeBC == TypeBoundCond.Neumann)
                //{
                //    if (DryWet[0] < 2) // если берег сухой
                //    {
                //        AP[0] = 1;
                //        S[0] = Zeta0[0];
                //    }
                //    else // затопленный берег
                //    {
                //        AE[0] = AW[1];
                //        AP[0] = AW[1];
                //        S[0] = 0;
                //    }
                //}
                //if (BCondOut.typeBC == TypeBoundCond.Neumann)
                //{
                //    if (DryWet[N] < 2) // если берег сухой
                //    {
                //        AP[N] = 1;
                //        S[N] = Zeta0[N];
                //    }
                //    else // затопленный берег
                //    {
                //        AP[N] = AE[N - 1];
                //        AW[N] = AE[N - 1];
                //        S[N] = 0;
                //    }
                //}
                ////PrintMas("AW", AW);
                ////PrintMas("AP", AP);
                ////PrintMas("AE", AE);
                ////PrintMas("S", S);
                ////Console.WriteLine();
                ////PrintMatrix();
                //// Прогонка
                //Solver solver = new Solver(Count);
                //solver.SetSystem(AW, AP, AE, S, Zeta);
                //// выполнение граничных условий Dirichlet
                //solver.CalkBCondition(BCondIn, BCondOut);
                //Zeta = solver.SolveSystem();
                ////PrintMas("AW", AW);
                ////PrintMas("AP", AP);
                ////PrintMas("AE", AE);
                ////PrintMas("S", S);
                ////Console.WriteLine();
                ////PrintMatrix();
                //// Zeta = solver.SolveSystem();
                // Сглаживание дна по лавинной моделе
                if (isAvalanche == AvalancheType.AvalancheSimple)
                    avalanche.Lavina(ref Zeta);
                // переопределение начального значения zeta 
                // для следующего шага по времени
                for (int j = 0; j < Zeta.Length; j++)
                {
                    bool fn = double.IsNaN(Zeta[j]);
                    if (fn == false)
                        Zeta0[j] = Zeta[j];
                    else
                        throw new Exception("реализовалось деление на ноль");
                }

            }
            catch (Exception e)
            {
                Message = e.Message;
            }
        }
        /// <summary>
        /// Затопленный уступ - гидростатика
        /// </summary>
        public static void Test0()
        {
            double tau0 = SPhysics.PHYS.tau0;
            double g = SPhysics.GRAV;

            BedLoadParams blp = new BedLoadParams();
            blp.isAvalanche = AvalancheType.AvalancheSimple;

            CBedLoadTask_1XD bltask = new CBedLoadTask_1XD(blp);
            // задача Дирихле


            int NN = 30;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                if (i < 10)
                    Zeta0[i] = 0.3;
                if (i >= 10 && i < 20)
                    Zeta0[i] = 0.3 - 0.01 * (i - 10);
                if (i >= 20 && i < NN)
                    Zeta0[i] = 0.2;
            }

            bltask.PrintMas("zeta0", Zeta0, 4);
            double dtime = 0.1;
            bltask.dtime = dtime;

            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, null, BConditions);
            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            Console.WriteLine("Tau_c {0}", tau0);
            // гидростатика
            double J = 0.01;
            double eta = 0.35;
            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = SPhysics.rho_w * g * J * (eta - 0.5 * (Zeta0[i] + Zeta0[i + 1]));
            }
            bltask.PrintMas("Tau", tau, 3);
            for (int t = 0; t < 5; t++)
            {
                double[] Zeta = null;
                bltask.CalkZetaFDM(ref Zeta, tau);
                bltask.PrintMas("zeta", Zeta, 4);
                for (int i = 0; i < NN - 1; i++)
                {
                    tau[i] = SPhysics.rho_w * g * J * (eta - 0.5 * (Zeta[i] + Zeta[i + 1]));
                }
                bltask.PrintMas("Tau", tau, 3);
            }
            Console.Read();
            Console.WriteLine();
            Console.WriteLine();
        }
        /// <summary>
        /// Сухой берег - гидростатика
        /// </summary>
        public static void Test1()
        {
            double tau0 = SPhysics.PHYS.tau0;
            double g = SPhysics.GRAV;

            BedLoadParams blp = new BedLoadParams();

            blp.isAvalanche = AvalancheType.NonAvalanche;

            CBedLoadTask_1XD bltask = new CBedLoadTask_1XD(blp);
            // задача Дирихле

            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Neumann, 0);

            int NN = 30;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                if (i < 10)
                    Zeta0[i] = 0.4;
                if (i >= 10 && i < 20)
                    Zeta0[i] = 0.4 - 0.02 * (i - 10);
                if (i >= 20 && i < NN)
                    Zeta0[i] = 0.2;
            }

            bltask.PrintMas("zeta0", Zeta0, 4);
            double dtime = 0.1;
            bltask.dtime = dtime;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, null, BConditions);
            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            Console.WriteLine("Tau_c {0}", tau0);
            // гидростатика
            double J = 0.001;
            double eta = 0.35;
            for (int i = 0; i < NN - 1; i++)
            {
                double Tau = SPhysics.rho_w * g * J * (eta - 0.5 * (Zeta0[i] + Zeta0[i + 1]));
                tau[i] = Math.Max(0, Tau);
            }
            bltask.PrintMas("Tau", tau, 3);
            for (int t = 0; t < 50; t++)
            {
                double[] Zeta = null;
                bltask.CalkZetaFDM(ref Zeta, tau);
                bltask.PrintMas("zeta", Zeta, 4);
                for (int i = 0; i < NN - 1; i++)
                {
                    double Tau = SPhysics.rho_w * g * J * (eta - 0.5 * (Zeta[i] + Zeta[i + 1]));
                    tau[i] = Math.Max(0, Tau);
                }
                bltask.PrintMas("Tau", tau, 3);
            }
            Console.Read();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
