﻿//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          11.12.19
//---------------------------------------------------------------------------
//                Модуль BedLoadLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;

    using MeshLib;
    using MemLogLib;

    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете береговых деформаций русла в створе реки
    /// </summary>
    [Serializable]
    public class BedLoadFVM_1YD : ABedLoadFVM_1YD
    {
        public override IBedLoadTask Clone()
        {
            return new BedLoadFVM_1YD(Params);
        }
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public override string NameBLParams()
        {
            return "NameBLParams.txt";
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
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public BedLoadFVM_1YD() : this(new BedLoadParams1D()){}

        public BedLoadFVM_1YD(BedLoadParams1D p) : base(p)
        {
            name = "деформация поперечного дна (d50) FVM";
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
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
    
            if (tau == null)
                tau = this.tau;
            if (tau == null)
                return;
            if (Params.blm == TypeBLModel.BLModel_1991)
            {
                MEM.Alloc2DClear(3, Count, ref Gs);
                MEM.Alloc2DClear(3, Count, ref dGs);
                double Ai, Bi, G0;
                for (int i = 0; i < N; i++)
                {
                    if (DryWet[i] > 1)
                    {
                        if(Roughness != null)
                            SPhysics.PHYS.GetLocalBedLoadParams(Roughness[i], ref tau0, ref tanphi,ref G1);
                        
                        mtau = Math.Abs(tau[i]);
                        chi = Math.Sqrt(tau0 / mtau);
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
                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = 0.5 * (x[i + 1] + x[i]);
                }
                sp.AddCurve("Отметки дна", x, Zeta0);
                if (Roughness != null)
                    sp.AddCurve("Шероховатость дна", x, Roughness);
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
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            
            double Q = 0;
            for (int i = 1; i < tau.Length; i++)
            {
                if (Roughness != null)
                    SPhysics.PHYS.GetLocalBedLoadParams(Roughness[i], ref tau0, ref tanphi, ref G1);
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
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double epsilon = SPhysics.PHYS.epsilon;
            double tau0 = SPhysics.PHYS.tau0;
            try
            {
                this.tau = tau;
                this.P = P;
                MEM.Alloc(Zeta0.Length, ref Zeta);
                // Расчет производных от геометрии
                double dz;
                for (int i = 0; i < N; i++)
                {
                    dx = x[i + 1] - x[i];
                    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
                    dZeta[i] = dz;
                    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
                }
                // Расчет коэффициентов  на грани  P--e--E
                for (int i = 0; i < N; i++)
                {
                    mtau = Math.Abs(tau[i]);
                    if (tau[i] < MEM.Error8)
                    {
                        // сухая - не размываемая часть берега
                        DryWet[i] = 0;
                        G0[i] = 0;
                        A[i] = 0;
                    }
                    else
                    {
                        if (Roughness != null)
                            SPhysics.PHYS.GetLocalBedLoadParams(Roughness[i], ref tau0, ref tanphi, ref G1);
                        // критическое напряжение
                        tau0Elem[i] = Math.Max(0, tau0 * (1 + dZeta[i] / tanphi));
                        //chi = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0 / mtau));
                        chi = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0Elem[i] / mtau));
                        if (chi >= 1)
                        {
                            DryWetEelm[i] = 0; // мокрая -  не размываемая часть дна
                            A[i] = 0;
                            G0[i] = 0;
                            DryWet[i] = 1;
                        }
                        else
                        {
                            DryWet[i] = 2; // мокрая - размываемая часть дна
                            A[i] = Math.Max(0, 1 - chi);
                            G0[i] = G1 * tau[i] * Math.Sqrt(mtau) * A[i] / CosGamma[i];
                        }
                    }
                }
                if (DryWet[N - 1] > 0)
                    DryWet[N] = DryWet[N - 1];
                if (DryWet[1] > 0)
                    DryWet[0] = DryWet[1];
                // расчет коэффициентов схемы
                for (int i = 1; i < N - 1; i++)
                {
                    if (DryWet[i] < 2)
                    {
                        // не деформируемая часть дна
                        AE[i] = 0;
                        AW[i] = 0;
                        AP[i] = 1;
                        S[i] = Zeta0[i];
                    }
                    else
                    {
                        // затопленная часть
                        double dxe = x[i + 1] - x[i];
                        double dxw = x[i] - x[i - 1];
                        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
                        AP0[i] = dxp / dtime;
                        // выбор способа осреднения коэффициентов диффузии
                        double tmpGe = G0[i] + G0[i + 1];
                        double tmpGw = G0[i] + G0[i - 1];
                        if (Math.Abs(tmpGe) < MEM.Error10)
                            AE[i] = G0[i + 1] / dxe;
                        else
                            AE[i] = 2 * G0[i] * G0[i + 1] / (tmpGe * dxe);

                        if (Math.Abs(tmpGw) < MEM.Error10)
                            AW[i] = G0[i - 1] / dxw;
                        else
                            AW[i] = 2 * G0[i] * G0[i - 1] / (tmpGw * dxw);
                        // расчет ведущего коэффициента
                        AP[i] = AE[i] + AW[i] + AP0[i];
                        // расчет правой части
                        S[i] = AP0[i] * Zeta0[i];
                    }
                }
                // последний узел
                {
                    int i = N - 1;
                    if (DryWet[i] < 2)
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

                        AE[i] = G0[i] / dxe;
                        AW[i] = 2 * G0[i] * G0[i - 1] / ((G0[i] + G0[i - 1]) * dxw);

                        AP[i] = AE[i] + AW[i] + AP0[i];

                        S[i] = AP0[i] * Zeta0[i];
                    }
                }
                //PrintMas("AW", AW);
                //PrintMas("AP", AP);
                //PrintMas("AE", AE);
                //PrintMas("S", S);
                //Console.WriteLine();
                //PrintMatrix();
                if (Params.BCondIn.typeBC == TypeBoundCond.Neumann)
                {
                    if (DryWet[0] < 2) // если берег сухой
                    {
                        AP[0] = 1;
                        S[0] = Zeta0[0];
                    }
                    else // затопленный берег
                    {
                        AE[0] = AW[1];
                        AP[0] = AW[1];
                        S[0] = 0;
                    }
                }
                if (Params.BCondOut.typeBC == TypeBoundCond.Neumann)
                {
                    if (DryWet[N] < 2) // если берег сухой
                    {
                        AP[N] = 1;
                        S[N] = Zeta0[N];
                    }
                    else // затопленный берег
                    {
                        AP[N] = AE[N - 1];
                        AW[N] = AE[N - 1];
                        S[N] = 0;
                    }
                }
                //PrintMas("AW", AW);
                //PrintMas("AP", AP);
                //PrintMas("AE", AE);
                //PrintMas("S", S);
                //Console.WriteLine();
                //PrintMatrix();
                // Прогонка
                Solver solver = new Solver(Count);
                solver.SetSystem(AW, AP, AE, S, Zeta);
                // выполнение граничных условий Dirichlet
                solver.CalkBCondition(Params.BCondIn, Params.BCondOut);
                Zeta = solver.SolveSystem();
                //PrintMas("AW", AW);
                //PrintMas("AP", AP);
                //PrintMas("AE", AE);
                //PrintMas("S", S);
                //Console.WriteLine();
                //PrintMatrix();
                // Сглаживание дна по лавинной моделе
                if (Params.isAvalanche == AvalancheType.AvalancheSimple)
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
                Logger.Instance.Error(e.Message, "ASectionalRiverTask");
                Logger.Instance.Exception(e);
            }
        }
        /// <summary>
        /// Затопленный уступ - гидростатика
        /// </summary>
        public static void Test0()
        {
            double tau0 = SPhysics.PHYS.tau0;
            double g = SPhysics.GRAV;
            double rho_w = SPhysics.rho_w;
            BedLoadParams1D blp = new BedLoadParams1D();
            blp.isAvalanche = AvalancheType.AvalancheSimple;

            BedLoadFVM_1YD bltask = new BedLoadFVM_1YD(blp);
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

            LOG.PrintMas("zeta0", Zeta0, 4);
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
                tau[i] = rho_w * g * J * (eta - 0.5 * (Zeta0[i] + Zeta0[i + 1]));
            }
            LOG.PrintMas("Tau", tau, 3);
            for (int t = 0; t < 5; t++)
            {
                double[] Zeta = null;
                bltask.CalkZetaFDM(ref Zeta, tau);
                LOG.PrintMas("zeta", Zeta, 4);
                for (int i = 0; i < NN - 1; i++)
                {
                    tau[i] = rho_w * g * J * (eta - 0.5 * (Zeta[i] + Zeta[i + 1]));
                }
                LOG.PrintMas("Tau", tau, 3);
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
            double rho_w = SPhysics.rho_w;

            BedLoadParams1D blp = new BedLoadParams1D();
            blp.isAvalanche = AvalancheType.NonAvalanche;

            BedLoadFVM_1YD bltask = new BedLoadFVM_1YD(blp);
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

            LOG.PrintMas("zeta0", Zeta0, 4);
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
                double Tau = rho_w * g * J * (eta - 0.5 * (Zeta0[i] + Zeta0[i + 1]));
                tau[i] = Math.Max(0, Tau);
            }
            LOG.PrintMas("Tau", tau, 3);
            for (int t = 0; t < 50; t++)
            {
                double[] Zeta = null;
                bltask.CalkZetaFDM(ref Zeta, tau);
                LOG.PrintMas("zeta", Zeta, 4);
                for (int i = 0; i < NN - 1; i++)
                {
                    double Tau = rho_w * g * J * (eta - 0.5 * (Zeta[i] + Zeta[i + 1]));
                    tau[i] = Math.Max(0, Tau);
                }
                LOG.PrintMas("Tau", tau, 3);
            }
            Console.Read();
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
