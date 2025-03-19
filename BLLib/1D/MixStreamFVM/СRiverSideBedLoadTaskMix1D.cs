//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BLLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//                по русловой модели Петрова П.Г. от 1991 г.
//        адаптация модели к многофракционным наносам Потапов И.И. 2021
//                        разработка: Потапов И.И.
//                          21.01.21-13.02.2021
//---------------------------------------------------------------------------
namespace BLLib
{
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;

    /// <summary>
    /// ОО: Класс для решения одномерной задачи по расчету донных 
    /// деформаций русла сложенного из многих фракций поперек потока
    /// </summary>
    [Serializable]
    public class СRiverSideBedLoadTaskMix1D : ABedLoadTaskMix1D, IBedLoadTask
    {
        public override IBedLoadTask Clone()
        {
            return new СRiverSideBedLoadTaskMix1D(new BedMixModelParams());
        }
        ///// <summary>
        ///// гравитационная постоянная (м/с/с)
        ///// </summary>
        //public double g = 9.81;
        /// <summary>
        /// критические напряжения на ровном дне для частиц фракции f
        /// </summary>
        public double[] tauF0;

        bool transitBC = true;

        public СRiverSideBedLoadTaskMix1D(BedMixModelParams p) : base(p)
        {
            InitBedLoad();
            name = "деформация поперечного много-фракционного дна.";
            tTask = TypeTask.streamY1D;
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
                double tanphi = SPhysics.PHYS.tanphi;
                double G1 = SPhysics.PHYS.G1;
                double tau0 = SPhysics.PHYS.tau0;
                double rho_s = SPhysics.PHYS.rho_s;
                double d50 = SPhysics.PHYS.d50;

                this.tau = tau;
                MEM.Alloc(Zeta0.Length, ref Zeta);
                //
                //   W       w       P       e       E  
                //---o-------^-------o-------^-------o------
                //   i-1             i              i+1 
                //         tau[i-1]        tau[i]   
                // Расчет коэффициентов  на грани  P--e--E
                // Расчет долей и процентов в активном слое
                KnotToElement(FractionSurface, ref Elems_FractionSurface);
                KnotToElement(PercentFinerSurface, ref Elems_PercentFinerSurface);
                double tau_0 = SPhysics.PHYS.tau0;
                double normTau = SPhysics.PHYS.Fa0;
                // цикл по узлам
                for (int i = 0; i < N; i++)
                {
                    A[i] = 0;
                    B[i] = 0;
                    dx = x[i + 1] - x[i];
                    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                    mtau = Math.Abs(tau[i]);
                    // косинус гамма
                    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                    G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                    // найти фракцию песка в узле
                    double d50_sand = Finer(Elems_PercentFinerSurface[i], 50.0);
                    // найти критическое напряжение сдвига для среднего d50 размера донной поверхности 
                    double taussrg = tau_0 * d50;
                    // ------- Пересчет долей наносов bedMixModel.FractionBedLoad в несущем слое 
                    for (int fr = 0; fr < CountMix - 1; fr++)
                    {
                        // Степенной параметр b для текущей фракции fr
                        double power_b = 0.67 / (1 + Math.Exp(1.5 - SandDiam[fr] / d50_sand));
                        // критические напряжения на ровном дне для текущей фракции fr
                        double tau0_fr = taussrg * Math.Pow((SandDiam[fr] / d50_sand), -1.0 * power_b);
                        chi = Math.Sqrt(tau0_fr / Math.Abs(tau[i]));
                        Af[i][fr] = Math.Max(0, 1 - Math.Sqrt(chi));
                        Bf[i][fr] = 4.0 / 5.0 * Af[i][fr] / tanphi;
                        // расход наносов для фракции fr
                        Gf[i][fr] = -Elems_FractionSurface[i][fr] * G0[i] * Bf[i][fr] * dz;
                        A[i] += Af[i][fr];
                        B[i] += Bf[i][fr];
                    }
                    Gf[i][CountMix - 1] = 0;
                    // расчет объемныех долей несущего водогрунтового слоя
                    Gs[i] = GetElemFraction(Gf[i], ref Elems_FractionBedLoad[i]);
                    // 
                    if (Math.Abs(Gs[i]) <= MEM.Error8)
                        DryWet[i] = 0; // сухая или мокрая не размываемая часть дна
                    else
                        DryWet[i] = 1; // размываемая  часть дна
                }
                // персчет долей фракций из элементных в узловые
                ElementToKnot(Elems_FractionBedLoad, ref FractionBedLoad);
                // расчет коэффициентов схемы
                for (int i = 1; i < N; i++)
                {
                    if (DryWet[i] == 0)
                    {
                        // сухая область берега
                        AE[i] = 0;
                        AW[i] = 0;
                        AP[i] = 1;
                        S[i] = Zeta0[i];
                    }
                    else
                    {
                        double dxe = x[i + 1] - x[i];
                        double dxw = x[i] - x[i - 1];
                        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
                        AP0[i] = dxp / dtime;
                        AE[i] = G0[i] * B[i] / dxe;
                        AW[i] = G0[i - 1] * B[i - 1] / dxw;
                        AP[i] = AE[i] + AW[i] + AP0[i];
                        S[i] = AP0[i] * Zeta0[i];
                    }
                }
                //  Выполнение граничных условий Неймана
                if (BCondIn.typeBC == TypeBoundCond.Neumann)
                {
                    AE[0] = AW[1];
                    AP[0] = AW[1];
                    Gtran_in = 0;
                    dx = x[1] - x[0];
                    dz = (Zeta0[1] - Zeta0[0]) / dx;
                    // собираем сумарные наносы на входе с учетом долей поступающих фракций
                    for (int fr = 0; fr < CountMix - 1; fr++)
                        Gtran_in += FeedFractionBedLoad[fr] * G0[0] * (Af[0][fr] - Bf[0][fr] * dz);
                    // определяем разницу заданного потока с BCBed.InletValue с вычисленным
                    S[0] = BCondIn.valueBC - Gtran_in;
                }
                if (BCondOut.typeBC == TypeBoundCond.Neumann)
                {
                    AP[N] = AE[N - 1];
                    AW[N] = AE[N - 1];
                    if (transitBC == true)
                        S[N] = 0;
                    else
                    {
                        dx = x[N - 1] - x[N - 2];
                        dz = (Zeta0[N - 1] - Zeta0[N - 2]) / dx;
                        Gtran_out = 0;
                        for (int fr = 0; fr < CountMix - 1; fr++)
                            Gtran_out += FractionBedLoad[N - 1][fr] * G0[N - 1] * (Af[N - 1][fr] - Bf[N - 1][fr] * dz);
                        S[N] = BCondOut.valueBC - Gtran_out;
                    }
                }
                // Прогонка
                Solver solver = new Solver(Count);
                solver.SetSystem(AW, AP, AE, S, Zeta);
                // выполнение граничных условий Dirichlet
                solver.CalkBCondition(BCondIn, BCondOut);
                Zeta = solver.SolveSystem();
                // находим приращение по дну между слоями по времени
                // полное dZeta и по фракциям dZetaf
                for (int n = 0; n < Zeta.Length; n++)
                {
                    dZeta[n] = Zeta[n] - Zeta0[n];
                    for (int fr = 0; fr < CountMix; fr++)
                        dZetaf[n][fr] = dZeta[n] * FractionBedLoad[n][fr];
                }
                // расчет объемных деформаций для f фракций 
                for (int n = 0; n < Zeta.Length; n++)
                {
                    if (DryWet[n] == 1)  // размываемая  часть дна
                    {
                        if (dZeta[n] > 0)
                        {
                            // намыв дна
                            for (int fr = 0; fr < CountMix; fr++)
                            {
                                double alpha_fb = FractionBedLoad[n][fr];
                                double alpha_f = FractionSurface[n][fr];
                                if (Ha - Math.Abs(dZeta[n]) <= 0)
                                    dVf[fr] = Area[n] * alpha_fb * Ha;
                                else
                                    dVf[fr] = Area[n] * (alpha_fb * dZetaf[n][fr] + alpha_f * (Ha - Math.Abs(dZeta[n])));
                            }
                        }
                        else
                        {
                            // размыв дна
                            for (int fr = 0; fr < CountMix; fr++)
                            {
                                double alpha_fb = FractionBedLoad[n][fr];
                                double alpha_f = FractionSurface[n][fr];
                                double alpha_fp = FractionSubSurface[n][fr];
                                if (Ha - Math.Abs(dZeta[n]) <= 0)
                                    dVf[fr] = Area[n] * Ha * alpha_fp;
                                else
                                    dVf[fr] = Area[n] * (alpha_fb * dZetaf[n][fr] + alpha_f * Ha + alpha_fp * Math.Abs(dZeta[n]));
                            }
                        }
                        // расчет долей для f фракций в активном слое 
                        GetElemFraction(dVf, ref FractionSurface[n]);
                    }
                }
                // Расчет процентного содержания фракций по их долям
                RefrshFraction(dZeta);
                // Сглаживание дна по лавинной моделе
                // лавинка традиционныя
                if (isAvalanche == AvalancheType.AvalancheSimple)
                    avalanche.Lavina(ref Zeta);
                // переопределение начального значения zeta 
                // для следующего шага по времени
                for (int n = 0; n < Zeta.Length; n++)
                    Zeta0[n] = Zeta[n];
            }
            catch (Exception e)
            {
                Message = e.Message;
                for (int n = 0; n < Zeta.Length; n++)
                    Zeta[n] = Zeta0[n];
            }
        }
        public static void Test0()
        {
            double tau0 = SPhysics.PHYS.tau0;

            double zeta0 = 0.2;
            int NN = 15;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                Zeta0[i] = zeta0;
            }
            double KsFactor = 2.5;
            double[] KsRanges = { 0.1, 0.3, 0.7, 1 };
            double[] FeedPercentFinerBedLoad =
            { 100, 100, 100, 90,  73,  56,  42,  33,  30,  15,  6,   0 };
            double[] Init_PercentFinerSubSurface =
            { 100,  90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 };
            // Диаметры фракций песка в мм
            double[] SandDiam = { 0.181, 0.0905, 0.04225, 0.02263, 0.01131, 0.00566, 0.00283, 0.00141, 0.000707, 0.000353, 0.000177, 0.0 };
            double[,] KsI = {
            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 },
            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 },
            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 },
            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 } };
            double[][] Init_RangePercentFinerSurface = new double[KsI.GetLength(0)][];
            for (int i = 0; i < KsI.GetLength(0); i++)
            {
                Init_RangePercentFinerSurface[i] = new double[KsI.GetLength(1)];
                for (int fi = 0; fi < KsI.GetLength(1); fi++)
                    Init_RangePercentFinerSurface[i][fi] = KsI[i, fi];
            }
            double Ha = 0.001;
            BedLoadParams blp = new BedLoadParams();
 
            BedMixModelParams bedMixModelParams = new BedMixModelParams(KsFactor, Ha, SandDiam, KsRanges,
                                FeedPercentFinerBedLoad, Init_RangePercentFinerSurface, Init_PercentFinerSubSurface,
                                blp);

            CBedLoadMixTask1D mblp = new CBedLoadMixTask1D(bedMixModelParams);
            CBedLoadMixTask1D bltask = new CBedLoadMixTask1D(mblp);

            // задача Дирихле
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Dirichlet, 2 * zeta0);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Dirichlet, 2 * zeta0);

            double dtime = 1; // 0.01;
            bltask.dtime = dtime;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, null, BConditions);
            
            double T = 2 * tau0;
            double dT = 0;// 0.5*T / (Zeta0.Length);
            double Ti = T;
            double[] tau = new double[NN - 1];
            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = Ti; Ti += dT;
            }
            for (int i = 0; i < 50; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau);
                bltask.PrintMas("zeta", Zeta);
            }

            Console.Read();
            Console.WriteLine();
            Console.WriteLine();
        }

        //public static void Main()
        //{
        //    // Гру
        //    Test0();
        //}
    }
}


