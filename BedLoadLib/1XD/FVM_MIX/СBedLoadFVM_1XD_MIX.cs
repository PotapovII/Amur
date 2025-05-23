﻿//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BedLoadLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//                по русловой модели Петрова П.Г. от 1991 г.
//        адаптация модели к многофракционным наносам Потапов И.И. 2021
//                        разработка: Потапов И.И.
//                          21.01.21-13.02.2021
//---------------------------------------------------------------------------
//                   рефакторинг: 07.04.25 Потапов И.И.
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;


    using MeshLib;
    using MemLogLib;

    using CommonLib;
    using CommonLib.Function;
    using CommonLib.Physics;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Класс для решения одномерной задачи по расчету донных 
    /// деформаций русла сложенного из многих фракций вдоль потока
    /// </summary>
    [Serializable]
    public class СBedLoadFVM_1XD_MIX : ABedLoadFVM_1D_MIX
    {
        public override IBedLoadTask Clone()
        {
            return new СBedLoadFVM_1XD_MIX(new BedLoadParams1D_MIX());
        }
        /// <summary>
        /// Загрузка полей задачи из форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>

        public override void LoadData(IDigFunction[] crossFunctions = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public override string NameBLParams()
        {
            return "NameBLParams.txt";
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

        public СBedLoadFVM_1XD_MIX() : this(new BedLoadParams1D_MIX()){}

        public СBedLoadFVM_1XD_MIX(BedLoadParams1D_MIX p) : base(p, TypeTask.streamX1D)
        {
            name = "FVM 1DX Mix деформация дна T(U-Uc_f) МКЭ";
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
                this.tau = tau;
                MEM.Alloc(Zeta0.Length, ref Zeta);
                //
                //   W       w       P       e       E  
                //---o-------^-------o-------^-------o------
                //   i-1             i              i+1 
                //         tau[i-1]        tau[i]   
                // Расчет коэффициентов  на грани  P--e--E
                // Расчет долей и процентов в активном слое
                KnotToElement(ParamsEx.FractionSurface, ref ParamsEx.Elems_FractionSurface);
                KnotToElement(ParamsEx.PercentFinerSurface, ref ParamsEx.Elems_PercentFinerSurface);
                double tau_0 = SPhysics.PHYS.tau0;
                double normTau = SPhysics.PHYS.Fa0;
                // цикл по узлам
                for (int i = 0; i < N; i++)
                {
                    A[i] = 0;
                    B[i] = 0;
                    dx = x[i + 1] - x[i];
                    double dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                    mtau = Math.Abs(tau[i]);
                    // косинус гамма
                    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                    G0[i] = SPhysics.PHYS.G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                    // найти фракцию песка в узле
                    double d50_sand = BedLoadParams1D_MIX.Finer(ParamsEx.Elems_PercentFinerSurface[i], 50.0);
                    // найти критическое напряжение сдвига для среднего d50 размера донной поверхности 
                    double taussrg = tau_0 * SPhysics.PHYS.d50;
                    // ------- Пересчет долей наносов bedMixModel.FractionBedLoad в несущем слое 
                    for (int fr = 0; fr < ParamsEx.CountMix - 1; fr++)
                    {
                        // Степенной параметр b для текущей фракции fr
                        double power_b = 0.67 / (1 + Math.Exp(1.5 - ParamsEx.SandDiam[fr] / d50_sand));
                        // критические напряжения на ровном дне для текущей фракции fr
                        double tau0_fr = taussrg * Math.Pow((ParamsEx.SandDiam[fr] / d50_sand), -1.0 * power_b);
                        chi = Math.Sqrt(tau0_fr / Math.Abs(tau[i]));
                        Af[i][fr] = Math.Max(0, 1 - Math.Sqrt(chi));
                        Bf[i][fr] = (chi / 2 + Af[i][fr]) / SPhysics.PHYS.tanphi;
                        // расход наносов для фракции fr
                        Gf[i][fr] = ParamsEx.Elems_FractionSurface[i][fr] * G0[i] * (Af[i][fr] - Bf[i][fr] * dz);
                        A[i] += Af[i][fr];
                        B[i] += Bf[i][fr];
                    }
                    Gf[i][ParamsEx.CountMix - 1] = 0;
                    // расчет объемныех долей несущего водогрунтового слоя
                    Gs[i] = ParamsEx.GetElemFraction(Gf[i], ref ParamsEx.Elems_FractionBedLoad[i]);
                    // 
                    if (Math.Abs(Gs[i]) <= MEM.Error8)
                        DryWet[i] = 0; // сухая или мокрая не размываемая часть дна
                    else
                        DryWet[i] = 1; // размываемая  часть дна
                }
                // персчет долей фракций из элементных в узловые
                ElementToKnot(ParamsEx.Elems_FractionBedLoad, ref ParamsEx.FractionBedLoad);
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
                        S[i] = -(G0[i] * A[i] - G0[i - 1] * A[i - 1]) + AP0[i] * Zeta0[i];
                    }
                }
                //  Выполнение граничных условий Неймана
                if (ParamsEx.BCondIn.typeBC == TypeBoundCond.Neumann)
                {
                    AE[0] = AW[1];
                    AP[0] = AW[1];
                    Gtran_in = 0;
                    dx = x[1] - x[0];
                    dz = (Zeta0[1] - Zeta0[0]) / dx;
                    // собираем сумарные наносы на входе с учетом долей поступающих фракций
                    for (int fr = 0; fr < ParamsEx.CountMix - 1; fr++)
                        Gtran_in += ParamsEx.FeedFractionBedLoad[fr] * G0[0] * (Af[0][fr] - Bf[0][fr] * dz);
                    // определяем разницу заданного потока с BCBed.InletValue с вычисленным
                    S[0] = ParamsEx.BCondOut.valueBC - Gtran_in;
                }
                if (ParamsEx.BCondOut.typeBC == TypeBoundCond.Neumann)
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
                        for (int fr = 0; fr < ParamsEx.CountMix - 1; fr++)
                            Gtran_out += ParamsEx.FractionBedLoad[N - 1][fr] * G0[N - 1] * (Af[N - 1][fr] - Bf[N - 1][fr] * dz);
                        S[N] = ParamsEx.BCondOut.valueBC - Gtran_out;
                    }
                }
                // Прогонка
                Solver solver = new Solver(Count);
                solver.SetSystem(AW, AP, AE, S, Zeta);
                // выполнение граничных условий Dirichlet
                solver.CalkBCondition(ParamsEx.BCondIn, ParamsEx.BCondOut);
                Zeta = solver.SolveSystem();
                // находим приращение по дну между слоями по времени
                // полное dZeta и по фракциям dZetaf
                for (int n = 0; n < Zeta.Length; n++)
                {
                    dZeta[n] = Zeta[n] - Zeta0[n];
                    for (int fr = 0; fr < ParamsEx.CountMix; fr++)
                        dZetaf[n][fr] = dZeta[n] * ParamsEx.FractionBedLoad[n][fr];
                }
                // расчет объемных деформаций для f фракций 
                for (int n = 0; n < Zeta.Length; n++)
                {
                    if (DryWet[n] == 1)  // размываемая  часть дна
                    {
                        if (dZeta[n] > 0)
                        {
                            // намыв дна
                            for (int fr = 0; fr < ParamsEx.CountMix; fr++)
                            {
                                double alpha_fb = ParamsEx.FractionBedLoad[n][fr];
                                double alpha_f = ParamsEx.FractionSurface[n][fr];
                                if (ParamsEx.Ha - Math.Abs(dZeta[n]) <= 0)
                                    dVf[fr] = Area[n] * alpha_fb * ParamsEx.Ha;
                                else
                                    dVf[fr] = Area[n] * (alpha_fb * dZetaf[n][fr] + alpha_f * (ParamsEx.Ha - Math.Abs(dZeta[n])));
                            }
                        }
                        else
                        {
                            // размыв дна
                            for (int fr = 0; fr < ParamsEx.CountMix; fr++)
                            {
                                double alpha_fb = ParamsEx.FractionBedLoad[n][fr];
                                double alpha_f = ParamsEx.FractionSurface[n][fr];
                                double alpha_fp = ParamsEx.FractionSubSurface[n][fr];
                                if (ParamsEx.Ha - Math.Abs(dZeta[n]) <= 0)
                                    dVf[fr] = Area[n] * ParamsEx.Ha * alpha_fp;
                                else
                                    dVf[fr] = Area[n] * (alpha_fb * dZetaf[n][fr] + alpha_f * ParamsEx.Ha + alpha_fp * Math.Abs(dZeta[n]));
                            }
                        }
                        // расчет долей для f фракций в активном слое 
                        ParamsEx.GetElemFraction(dVf, ref ParamsEx.FractionSurface[n]);
                    }
                }
                // Расчет процентного содержания фракций по их долям
                ParamsEx.RefrshFraction(dZeta);
                // Сглаживание дна по лавинной моделе
                // лавинка традиционныя
                if (ParamsEx.isAvalanche == AvalancheType.AvalancheSimple)
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
            BedLoadParams1D_MIX blp = new BedLoadParams1D_MIX();
            //blp.rho_w = 1000;
            //blp.rho_s = 2650;
            //blp.phi = 30;
            //blp.d50 = 0.001;
            //blp.epsilon = 0.3;
            //blp.kappa = 0.2;
            //blp.f = 0.1;
            //blp.cx = 0.5;

            BedLoadParams1D_MIX bedMixModelParams = new BedLoadParams1D_MIX();

            СBedLoadFVM_1XD_MIX mblp = new СBedLoadFVM_1XD_MIX(bedMixModelParams);
            СBedLoadFVM_1XD_MIX bltask = new СBedLoadFVM_1XD_MIX(bedMixModelParams);

            // задача Дирихле

            blp.BCondIn = new BoundCondition1D(0, 2 * zeta0);
            blp.BCondOut = new BoundCondition1D(0, 2 * zeta0);


            double dtime = 1; // 0.01;
            bltask.dtime = dtime;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, null, BConditions);

            double T = 2 * SPhysics.PHYS.tau0;
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
