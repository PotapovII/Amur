//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                               27.12.19
//---------------------------------------------------------------------------
//  Учет ситуаций сухой-мокрый, корректная обработка сулучаев tau == 0
//  переход на интерфейс через абстрактный класс ABedLoadTask1D
//                              Потапов И.И.
//                               14.04.21
//---------------------------------------------------------------------------
//               Добавление модели TypeBLModel.BLModel_2021 
//          разделение гидростатики и напорной части давления
//                              Потапов И.И.
//                               01.10.21
//---------------------------------------------------------------------------
//             Добавлен учет расходов от взвешенных наносов
//                              Потапов И.И.
//                               14.01.24
//---------------------------------------------------------------------------
namespace BLLib
{
    using GeometryLib;
    using CommonLib;
    using MemLogLib;
    using MeshLib;
    using System;
    using CommonLib.Physics;

    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете донных деформаций русла вдоль потока
    /// </summary>
    [Serializable]
    public class CBedLoadTask1D : ABedLoadTask1D
    {
        protected double[] mChi = null;
        protected double[] tauC = null;
        protected double[] Kappa = null;
        public override IBedLoadTask Clone()
        {
            return new CBedLoadTask1D(new BedLoadParams());
        }
        public CBedLoadTask1D(BedLoadParams p) : base(p)
        {
            name = "1DX (d50) деформация дна T(U-Uc)";
            tTask = TypeTask.streamX1D;
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, IBoundaryConditions BConditions)
        {
            base.SetTask(mesh, Zeta0, BConditions);
            MEM.Alloc<double>(Count, ref Kappa);
            taskReady = true;
        }

        ///  /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public override void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
        {
            if (tau == null)
                tau = this.tau;
            if (tau == null)
                return;

            if (blm == TypeBLModel.BLModel_2010 || blm == TypeBLModel.BLModel_2014 || blm == TypeBLModel.BLModel_2021)
            {
                MEM.Alloc2DClear(4, Count, ref Gs);
                MEM.Alloc2DClear(4, Count, ref dGs);
                // Расчет коэффициентов  на грани  P--e--E
                for (int i = 0; i < N; i++)
                {
                    dx = x[i + 1] - x[i];

                    dp = (ps[i + 1] - ps[i]) / dx;
                    Gs[idxTransit][i] = G0[i] * A[i];
                    Gs[idxZeta][i] = -G0[i] * B[i] * dZeta[i];
                    Gs[idxPress][i] = -G0[i] * C[i] * dp;
                    Gs[idxAll][i] = Gs[idxTransit][i] + Gs[idxZeta][i] + Gs[idxPress][i];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    dx = x[i + 1] - x[i];
                    dGs[idxTransit][i] = (Gs[idxTransit][i + 1] - Gs[idxTransit][i]) / dx;
                    dGs[idxZeta][i] = (Gs[idxZeta][i + 1] - Gs[idxZeta][i]) / dx;
                    dGs[idxPress][i] = (Gs[idxPress][i + 1] - Gs[idxPress][i]) / dx;
                    dGs[idxAll][i] = (Gs[idxAll][i + 1] - Gs[idxAll][i]) / dx;
                }
            }
            if (blm == TypeBLModel.BLModel_1991)
            {
                MEM.Alloc2DClear(3, Count, ref Gs);
                MEM.Alloc2DClear(3, Count, ref dGs);
                // Расчет коэффициентов  на грани  P--e--E
                BuilderABC();
                for (int i = 0; i < Count - 1; i++)
                {
                    Gs[idxTransit][i] = G0[i] * A[i];
                    Gs[idxZeta][i] = -G0[i] * B[i] * dZeta[i];
                    Gs[idxAll][i] = Gs[idxTransit][i] + Gs[idxZeta][i];
                }
                for (int i = 0; i < N - 1; i++)
                {
                    dx = x[i + 1] - x[i];
                    dGs[idxTransit][i] = (Gs[idxTransit][i + 1] - Gs[idxTransit][i]) / dx;
                    dGs[idxZeta][i] = (Gs[idxZeta][i + 1] - Gs[idxZeta][i]) / dx;
                    dGs[idxAll][i] = Gs[idxTransit][i] + Gs[idxZeta][i];
                }
            }
        }

        /// <summary>
        /// Формирование данных для отрисовки полей несвязанных/связанных 
        /// с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                // плотность жидкости
                double rho_w = SPhysics.rho_w;
                // Рауз
                double RaC = SPhysics.PHYS.RaC;
                // гидравлическая крупеость
                double Ws = SPhysics.PHYS.Ws;
                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = 0.5 * (x[i + 1] + x[i]);
                }
                double zs = DMath.Integtal(x, Zeta0);
                double zv = DMath.IntegtalAbs(x, Zeta0);
                string ss = zs.ToString("F4");
                string ss1 = (zs / zv).ToString("F4");
                sp.AddCurve("Отметки дна (узлы): " + ss + " E: " + ss1, x, Zeta0);
                sp.AddCurve("Придонное напряжение", X, tau);
                sp.AddCurve("Придонное критические напряжение", X, tau0Elem);
                MEM.Alloc(x.Length, ref tauC, "tauC");
                for (int i = 0; i < X.Length; i++)
                {
                    double us = Math.Sqrt(Math.Abs(tau[i]) / rho_w);
                    double Ra = SPhysics.PHYS.Ra(us);
                    tauC[i] = Math.Min(  Ra/ RaC, 1);
                }
                sp.AddCurve("Критическая ГКН", X, tauC);
                sp.AddCurve("Каппа", X, Kappa);
                
                if (sedimentShow == true)
                {

                    Calk_Gs(ref Gs, ref dGs, tau);
                    if (Gs != null)
                    {
                        MEM.Alloc(x.Length, ref mChi);
                        for (int i = 0; i < mChi.Length; i++)
                        {
                            mChi[i] = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0Elem[i] / (Math.Abs(tau[i]) + MEM.Error10)));
                        }
                        sp.AddCurve("Параметр chi", X, mChi);
                        // 
                        for (int i = 0; i < X.Length; i++)
                        {
                            mChi[i] = Ws / Math.Sqrt(Math.Abs(tau[i]) / rho_w) / 0.41;
                        }
                        sp.AddCurve("Число Рауза", X, mChi);
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
        }

        /// <summary>
        /// Расчет коэффициентов  на грани  P--e--E
        /// </summary>
        protected virtual void BuilderABC(double COEF_B = 1)
        {
            //  Расчет производных и критических напряжений для однородного донного материала
            CalkCritTauType();

            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;

            // цикл по интервалам
            for (int i = 0; i < N; i++)
            {
                mtau = Math.Abs(tau[i]);
                // Контроль сухой (не размываемой) части берега
                if (mtau < MEM.Error8)
                {
                    DryWetEelm[i] = 0; // сухая (не размываемая) часть дна
                    A[i] = 0;
                    B[i] = 0;
                    C[i] = 0;
                    G0[i] = 0;
                }
                else
                {
                    double dzf = dZeta[i] / tanphi;
                    // влияние уклона на критическое напряжение учтено при выводе уравнений модели
                    chi = Math.Min(1 + MEM.Error10, Math.Sqrt(tau0Elem[i] / mtau));
                    // CosGamma[i] = Math.Sqrt(1 / (1 + dZeta[i] * dZeta[i]));
                    if (chi >= 1)
                    {
                        DryWetEelm[i] = 0; // мокрая (не размываемая) часть дна
                        A[i] = 0;
                        B[i] = 0;
                        C[i] = 0;
                        G0[i] = 0;
                    }
                    else
                    {
                        DryWetEelm[i] = 1; // размываемая  часть дна
                        A[i] = Math.Max(0, 1 - chi);
                        if (blm == TypeBLModel.BLModel_2021 || blm == TypeBLModel.BLModel_1991)
                            B[i] = (chi / 2 + A[i]) / tanphi;
                        else
                            B[i] = (chi / 2 + A[i] * (1 + s) / s) / tanphi; // TypeBLModel.BLModel_2014
                        C[i] = A[i] / (s * tanphi);
                        double GG = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
                        double scale = DRate(dzf, 0);
                        G0[i] = GG * scale;
                    }
                }
            }
        }
        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2021
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta0">текущая форма дна</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        /// <returns>новая форма дна</returns>
        /// </summary>
        public override void CalkZetaFDM(ref double[] Zeta, double[] tau, double[] tauY = null, double[] P = null, double[][] CS = null)
        {
            try
            {
                double gamma = SPhysics.PHYS.gamma;
                // 
                // ГУ по ставим старому
                //
                BConditions = null;
                //
                this.tau = tau;
                this.P = P;
                MEM.Alloc(Zeta0.Length, ref Zeta);

                // коррекция ГГУ
                if (BCondIn.typeBC == TypeBoundCond.Dirichlet)
                    Zeta0[0] = BCondIn.valueBC;
                if (BCondOut.typeBC == TypeBoundCond.Dirichlet)
                    Zeta0[N] = BCondOut.valueBC;
                // Расчет деформаций дна от влекомых наносов
                if (blm == TypeBLModel.BLModel_2014 || blm == TypeBLModel.BLModel_2021)
                {
                    ConvertElemToNode(P, ref ps, gamma);
                }
                // +2024:
                // Если расходы концентрации заданны симмируем их для всех фракций
                if (CS != null)
                {
                    for (int i = 0; i < N; i++)
                    {
                        GCx[i] = 0;
                        for (int f = 0; f < CS.Length; f++)
                            GCx[i] += CS[f][i];
                    }
                }
                // Расчет производных от геометрии
                // Расчет коэффициентов  на грани  P--e--E
                // цикл по интервалам
                BuilderABC();
                // вычисление сухих узлов по сухим элементам
                for (int i = 1; i < Count - 1; i++)
                    DryWet[i] = (DryWetEelm[i - 1] + DryWetEelm[i]) / Math.Max(1, DryWetEelm[i - 1] + DryWetEelm[i]);
                DryWet[0] = DryWetEelm[0];
                DryWet[Count - 1] = DryWetEelm[N - 1];
                // цикл по внутренним узлам 
                // расчет коэффициентов схемы для вунутренних узлов
                for (int i = 1; i < Count - 1; i++)
                {
                    double dGs = GCx[i] - GCx[i - 1];
                    double dxe = x[i + 1] - x[i];
                    double dxw = x[i] - x[i - 1];
                    double dxp = 0.5 * (x[i + 1] - x[i - 1]);
                    AP0[i] = dxp / dtime;

                    if (DryWet[i] == 0)
                    {

                        if (dGs < 0 && Math.Abs(dGs / dxp) > MEM.Error8)
                        {
                            AP0[i] = dxp / dtime;
                            AE[i] = 0;
                            AW[i] = 0;
                            AP[i] = 1;
                            S[i] = -dGs / AP0[i] + Zeta0[i];
                        }
                        else
                        {
                            // сухая область берега
                            AE[i] = 0;
                            AW[i] = 0;
                            AP[i] = 1;
                            S[i] = Zeta0[i];
                        }
                    }
                    else
                    {
                        AE[i] = Math.Abs(G0[i] * B[i]) / dxe;
                        AW[i] = Math.Abs(G0[i - 1] * B[i - 1]) / dxw;
                        AP[i] = AE[i] + AW[i] + AP0[i];
                        // +2024:  - (GCx[i] - GCx[i-1])
                        S[i] = -(G0[i] * A[i] - G0[i - 1] * A[i - 1]) - dGs + AP0[i] * Zeta0[i];

                        if (blm != TypeBLModel.BLModel_1991)
                        {
                            CE[i] = G0[i] * C[i] / dxe;
                            CW[i] = G0[i - 1] * C[i - 1] / dxw;
                            CP[i] = CE[i] + CW[i];
                            S[i] += CE[i] * ps[i + 1] - CP[i] * ps[i] + CW[i] * ps[i - 1];
                        }
                    }
                }
                // ВХОД
                // граничные узлы
                if (BCondIn.typeBC == TypeBoundCond.Neumann || BCondOut.typeBC == TypeBoundCond.Transit)
                {
                    if (DryWet[0] == 0) // если берег сухой/не размываемый (автоматом условия Дирихле)
                    {
                        AP[0] = 1;
                        S[0] = Zeta0[0];
                    }
                    else // затопленный берег
                    {
                        if (BCondIn.typeBC == TypeBoundCond.Transit)
                        {
                            S[0] = 0;
                        }
                        else
                        {
                            // +2024:  + GCx[0]
                            Gtran_in = G0[0] * A[0] + GCx[0];
                            S[0] = BCondIn.valueBC - Gtran_in;
                        }
                        AE[0] = AW[1];
                        AP[0] = AW[1];
                    }
                }
                // ВЫВХОД
                if (BCondOut.typeBC == TypeBoundCond.Neumann || BCondOut.typeBC == TypeBoundCond.Transit)
                {
                    if (DryWet[N] == 0) // если берег сухой/не размываемый (автоматом условия Дирихле)
                    {
                        AP[N] = 1;
                        S[N] = Zeta0[N];
                    }
                    else // исток
                    {
                        // +2024:  + GCx[N - 1]
                        Gtran_out = G0[N - 1] * A[N - 1] + GCx[N - 1];
                        AP[N] = AE[N - 1];
                        AW[N] = AE[N - 1];
                        if (BCondOut.typeBC == TypeBoundCond.Transit)
                            S[N] = 0;
                        else
                            S[N] = -(BCondOut.valueBC - Gtran_out);
                    }
                }
                // Прогонка
                Solver solver = new Solver(Count);
                solver.SetSystem(AW, AP, AE, S, Zeta);
                // выполнение граничных условий Dirichlet
                solver.CalkBCondition(BCondIn, BCondOut);
                Zeta = solver.SolveSystem();
                // Сглаживание дна по лавинной моделе
                if (isAvalanche == AvalancheType.AvalancheSimple)
                    avalanche.Lavina(ref Zeta);
                // переопределение начального значения zeta 
                // для следующего шага по времени
                for (int j = 0; j < Zeta.Length; j++)
                    Zeta0[j] = Zeta[j];
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
                for (int j = 0; j < Zeta.Length; j++)
                    Zeta[j] = Zeta0[j];
            }
        }
        #region тесты
        /// <summary>
        /// Засыпание каверны |_| с лавинным осыпание боковых сторон на начальном этапе
        /// </summary>
        public static void Test0()
        {
            double z0 = 0.2;
            double tau0 = SPhysics.PHYS.tau0;
            BedLoadParams blp = new BedLoadParams();
            blp.blm = TypeBLModel.BLModel_1991;
           // blp.d50 = 0.001;
            blp.isAvalanche = AvalancheType.AvalancheSimple;
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Dirichlet, 1);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Dirichlet, 1);

            CBedLoadTask1D bltask = new CBedLoadTask1D(blp);
            int NN = 10;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                Zeta0[i] = z0;
            }
            double dt = 1; // 0.01;
            bltask.dtime = dt;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, BConditions);

            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            double[] P = new double[NN - 1];

            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = T;
                P[i] = 1;
            }
            for (int i = 0; i < 100; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, null, P);
                bltask.PrintMas("zeta: " + i.ToString(), Zeta, 4);
            }
            Console.Read();
        }
        /// <summary>
        /// Размыв каверны набегающим потоком => |_/
        /// </summary>
        public static void Test1()
        {
            double z0 = 0.2;
            double tau0 = SPhysics.PHYS.tau0;
            BedLoadParams blp = new BedLoadParams();
            blp.blm = TypeBLModel.BLModel_1991;
           // blp.d50 = 0.001;
            blp.isAvalanche = AvalancheType.AvalancheSimple;
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Transit);

            CBedLoadTask1D bltask = new CBedLoadTask1D(blp);
            int NN = 10;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                Zeta0[i] = z0;
            }
            double dt = 0.1; // 0.01;
            bltask.dtime = dt;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, BConditions);

            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            double[] P = new double[NN - 1];

            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = T;
                P[i] = 1;
            }
            for (int i = 0; i < 35; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, null, P);
                bltask.PrintMas("zeta: " + i.ToString(), Zeta, 4);
            }
            Console.Read();
        }
        /// <summary>
        /// Размыв намыв наносов \/\
        /// </summary>
        public static void Test2()
        {
            double z0 = 1;
            BedLoadParams blp = new BedLoadParams();
            blp.blm = TypeBLModel.BLModel_2014;
            double tau0 = SPhysics.PHYS.tau0;
            //blp.d50 = 0.001;
            blp.isAvalanche = AvalancheType.NonAvalanche;
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Dirichlet, z0);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Dirichlet, z0);

            CBedLoadTask1D bltask = new CBedLoadTask1D(blp);
            int NN = 20;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                Zeta0[i] = z0;
            }
            double dt = 1; // 0.01;
            bltask.dtime = dt;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, BConditions);

            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            double[] P = new double[NN - 1];

            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = T + 0.1 * T * Math.Sin(Math.PI * i / (NN - 2));
                P[i] = 1;
            }
            LOG.Print("tau", tau, 4);
            for (int i = 0; i < 500; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, null, P);
                LOG.Print("zeta", Zeta, 4);
            }
            Console.Read();
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Test3()
        {
            double z0 = 0.2;
            BedLoadParams blp = new BedLoadParams();
            blp.blm = TypeBLModel.BLModel_2014;
            double tau0 = SPhysics.PHYS.tau0;
            //blp.d50 = 0.001;
            blp.isAvalanche = AvalancheType.NonAvalanche;
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Neumann, 0.0001);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Dirichlet, z0);

            CBedLoadTask1D bltask = new CBedLoadTask1D(blp);
            int NN = 10;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                Zeta0[i] = z0;
            }
            double dt = 1;
            bltask.dtime = dt;
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, BConditions);

            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            double[] P = new double[NN - 1];

            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = T;
                P[i] = 1;
            }
            for (int i = 0; i < 50; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, null, P);
                bltask.PrintMas("zeta", Zeta, 4);
            }
            Console.Read();

        }
        /// <summary>
        /// Размыв экс. каверны 
        /// </summary>
        public static void Test4()
        {
            double tanphi = SPhysics.PHYS.tanphi;
            double tau0 = SPhysics.PHYS.tau0;

            BedLoadParams blp = new BedLoadParams();
            blp.blm = TypeBLModel.BLModel_1991;
            blp.isAvalanche = AvalancheType.AvalancheSimple;
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Transit, 0);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Transit, 0);

            CBedLoadTask1D bltask = new CBedLoadTask1D(blp);
            int NN = 100;
            double L = 20;
            double x0 = 10;
            double zetaAmp = - 0.5;
            double r = 0.5;
            double dx = L / (NN - 1);
            double[] x = new double[NN];
            double[] f = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            double[] ZetaE = new double[NN];
            double[] ZetaN = new double[NN];
            double[] ErrorZeta = new double[NN];
            double[] tau = new double[NN - 1];
            double[] P = new double[NN - 1];
            double a, b, c, e, e0, xx, aa;
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                aa = (x[i] - x0) * (x[i] - x0);
                Zeta0[i] = 0;
                ZetaE[i] = MEM.SE( zetaAmp * Math.Exp( - aa ) );
            }
            double dzetadx;
            double chi = 0.9;
            double T_inf = tau0/(chi * chi);
            for (int i = 0; i < NN - 1; i++)
            {
                xx = 0.5 * (x[i] + x[i + 1]);
                aa = xx - x0;
                dzetadx = -2 * zetaAmp * aa * Math.Exp(-aa * aa);
                a = -tanphi + dzetadx;
                b = (tanphi - 0.5 * dzetadx) * chi;
                c = (1 - chi) * tanphi;
                a = -a;
                b = -b;
                c = -c;
                e0 = -108 * c * a * a - 8 * b * b * b + 12 * Math.Sqrt(3) * Math.Sqrt(c * (27 * c * a * a + 4 * b * b * b)) * a;
                e = Math.Pow(e0, 1.0 / 3);
                f[i] = e / (6 * a) + 2 * b * b / (3 * a * e) - b / (3 * a);
                tau[i] = T_inf* f[i] * f[i];
                P[i] = 1;
            }
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, BConditions);
            bltask.dtime = 1;// dx;
            //bltask.dtime = bltask.Kurant();
            int NT = 30 * 3600;// *5;
            for (int i = 0; i < NT; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, null, P);
                //bltask.PrintMas("zeta: " + i.ToString(), Zeta, 4);
            }
            for (int i = 0; i < NN; i++)
            {
                ZetaN[i] = Zeta[i] - Zeta[0];
                ErrorZeta[i] = Math.Abs((Zeta[i] - ZetaE[i]));
            }
            
            bltask.PrintMas("zeta: " + NT.ToString(), Zeta, 4);
            bltask.PrintMas("ZetaN: ", ZetaN, 4);
            bltask.PrintMas("ZetaE: ", ZetaE, 4);
            bltask.PrintMas("ErrorZeta", ErrorZeta, 4);
            Console.WriteLine("Время размыва = {0} итераций {1}", NT * bltask.dtime/3600, NT);
            Console.Read();
        }

        /// <summary>
        /// Обратный размыв под параболой
        /// </summary>
        public static void Test5()
        {
            double tau0 = SPhysics.PHYS.tau0;
            BedLoadParams blp = new BedLoadParams();
            blp.blm = TypeBLModel.BLModel_1991;
            blp.isAvalanche = AvalancheType.AvalancheSimple;
            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Transit, 0);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Transit, 0);
            CBedLoadTask1D bltask = new CBedLoadTask1D(blp);
            int NN = 30;
            double Tmax = 20;
            double L = 20;
            double x1 = 5;
            double x2 = 15;
            double xc = 10;
            double dx = L / (NN - 1);
            double[] x = new double[NN];
            double[] tau = new double[NN];
            double[] P = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] ZetaN = new double[NN];
            double[] Zeta = null;
            double chi = 0.9;
            double T_inf = tau0 / (chi * chi);
            //T_inf = 0;
            int sg = 1;
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                Zeta0[i] = 0;
                if (x[i] <= x2 && x[i] >= x1)
                    tau[i] = sg * (T_inf + Tmax * (x[i] - x1) * (x[i] - xc) * (x[i] - x2) / ((xc - x1) * (xc - x2)));
                else
                    tau[i] = sg * T_inf;
                P[i] = 1;
            }
            LOG.Print("Tau: ", tau, 4);
            BoundaryConditionsVar BConditions = new BoundaryConditionsVar(1);
            bltask.SetTask(new TwoMesh(x, Zeta0), Zeta0, BConditions);
            bltask.dtime = 1;// dx;
            int NT = 30 * 3600;// *5;
            for (int i = 0; i < NT; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, null, P);
                LOG.Print("zeta: " + i.ToString(), Zeta, 4);
            }
            LOG.Print("zeta: " + NT.ToString(), Zeta, 4);
            LOG.Print("ZetaN: ", ZetaN, 4);
            Console.WriteLine("Время размыва = {0} итераций {1}", NT * bltask.dtime / 3600, NT);
            Console.Read();
        }

        public static void Main()
        {
            // Засыпание каверны |_| с лавинным осыпание боковых сторон на начальном этапе
            //Test0();
            // Размыв каверны набегающим потоком => |_/
            //Test1();
            // Размыв намыв наносов \/\
            //Test2();
            //Test3();
            /// <summary>
            /// Размыв экс. каверны 
            /// </summary>
            Test5();
        }
        #endregion
    }
}
