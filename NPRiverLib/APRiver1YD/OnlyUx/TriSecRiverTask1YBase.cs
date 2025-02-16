//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//                  создано  :   9.03.2007 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 06.12.2020 Потапов И.И. 
//            создание родителя : 21.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;

    using CommonLib;
    using CommonLib.IO;

    using System;
    using System.Linq;

    using FEMTasksLib.FESimpleTask;
    using NPRiverLib.APRiver1YD.Params;
    using MeshGeneratorsLib.StripGenerator;
    using MeshLib.Wrappers;
    using NPRiverLib.ABaseTask;
    using CommonLib.ChannelProcess;
    using CommonLib.Physics;
    using System.IO;
    using MeshLib;


    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriSecRiverTask1YBase : APRiverFEM1YD
    {
        /// <summary>
        /// FlagStartMesh - первая генерация сетки
        /// </summary>
        protected bool FlagStartMesh = false;
        /// <summary>
        /// Тип модели турбулентной вязкости
        /// </summary>
        public TurbulentViscosityModel ViscosityModel = TurbulentViscosityModel.ViscosityPrandtlKarman;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[] eddyViscosity0 = null;
        /// <summary>
        /// Вязкость потока средняя/максимальна/на КЭ
        /// </summary>
        public double Mu;

        #region КЭ
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
        protected private int cu = 3;
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = { 0, 0, 0 };
        ///// <summary>
        ///// Квадратурные точки для численного интегрирования
        ///// </summary>
        //protected NumInegrationPoints pIntegration;
        //protected NumInegrationPoints IPointsA = new NumInegrationPoints();
        //protected NumInegrationPoints IPointsB = new NumInegrationPoints();
        /// <summary>
        /// функции формы для КЭ
        /// </summary>
        protected AbFunForm ff = null;
        /// <summary>
        /// локальная матрица часть СЛАУ
        /// </summary>
        protected double[][] LaplMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] LocalRight = null;
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected uint[] adressBound = null;
        /// <summary>
        /// координаты узлов КЭ 
        /// </summary>
        protected double[] elem_x = null;
        protected double[] elem_y = null;
        /// <summary>
        /// Скорсть в узлах
        /// </summary>
        protected double[] elem_U = null;
        /// <summary>
        /// вязкость в узлах КЭ 
        /// </summary>
        protected double[] elem_mu = null;
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSecRiverTask1YBase() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSecRiverTask1YBase(RSCrossParams p) : base(p)
        {
            Version = "TriSecRiverTask1YBase 02.01.2024";
            name = "Поток в створе канала mu_t (alg)";
            FlagStartMu = true;
            FlagStartMesh = true;
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            int flagErr = 0;
            try
            {
                // расчет уровня свободной поверхности реки
                SolveWaterLevel();
                flagErr++;
                // определение расчетной области потока и построение КЭ сетки
                SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
                flagErr++;
                // расчет турбулентной вязкости потока
                SolveMu();
                // расчет гидрадинамики  (скоростей потока)
                SolveVelosity();
                flagErr++;
                // расчет  придонных касательных напряжений на дне
                tau = TausToVols(bottom_x, bottom_y);
                flagErr++;
                time += dtime;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (Erosion != EBedErosion.NoBedErosion)
            {
                // сохранение данных 
                unknowns.Clear();
                unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
                unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            }
            base.AddMeshPolesForGraphics(sp);
        }

        /// <summary>
        /// Расчет уровня свободной поверхности реки
        /// </summary>
        protected virtual void SolveWaterLevel()
        {
            if (Params.taskVariant == TaskVariant.flowRateFun)
            {
                riverFlowRate = FlowRate.FunctionValue(time);
                if (time > 0)
                {
                    double Ck = 0.1;
                    double Umax = 5;
                    riverFlowRateCalk = RiverFlowRate();
                    double deltaH = (riverFlowRateCalk - riverFlowRate) / riverFlowRate;
                    double W = bottom_x[bottom_x.Length - 1] - bottom_x[0];
                    double H = riverFlowRateCalk / (W * Umax) * deltaH;
                    double dH = Ck * dtime * H;
                    if (Math.Abs(dH) > 0.005)
                        dH = Math.Sign(dH) * 0.005;
                    waterLevel = waterLevel - dH;
                }
                else
                {
                    riverFlowRateCalk = riverFlowRate;
                    FlagStartMesh = true;
                }
            }
            else
            {
                waterLevel = WaterLevels.FunctionValue(time);
            }
        }
        /// <summary>
        /// Расчет расхода воды
        /// </summary>
        /// <returns>объемный расход воды</returns>
        public virtual double RiverFlowRate()
        {
            Area = 0;
            double su, S;
            riverFlowRateCalk = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                // получить тип и узлы КЭ
                TypeFunForm typeff = mesh.ElementKnots(i, ref knots);
                // выбрать по типу квадратурную схему для интегрирования
                //if (FunFormHelp.CheckFF(typeff) == 0)
                //    pIntegration = IPointsA;
                //else
                //    pIntegration = IPointsB;
                // определить количество узлов на КЭ
                int cu = knots.Length;
                // выделить память под локальные массивы
                InitLocal(cu);
                //Координаты и площадь
                mesh.GetElemCoords(i, ref elem_x, ref elem_y);

                mesh.ElemValues(Ux, i, ref elem_U);
                su = elem_U.Sum() / elem_U.Length;
                //Площадь
                S = mesh.ElemSquare(i);
                // расход по живому сечению
                riverFlowRateCalk += S * su;
                Area += S;
            }
            if (double.IsNaN(riverFlowRateCalk) == true)
                throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public void InitLocal(int cu)
        {
            MEM.Alloc<double>(cu, ref elem_x, "elem_x");
            MEM.Alloc<double>(cu, ref elem_y, "elem_y");
            MEM.Alloc<double>(cu, ref elem_U, "elem_U");
            MEM.Alloc<double>(cu, ref elem_mu, "elem_mu");
            // с учетом степеней свободы
            MEM.AllocClear(cu, ref LocalRight);
            MEM.Alloc2DClear(cu, ref LaplMatrix);
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta">отметки дна</param>
        /// <param name="bedErosion">флаг генерация сетки при размывах дна</param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            bottom_y = zeta;
            if (FlagStartMesh == false)// || Erosion != EBedErosion.NoBedErosion)// || flagBLoad == true)
            {
                SetDataForRiverStream(waterLevel, bottom_x, bottom_y, ref right, ref left);
                FlagStartMesh = true;
            }
        }
        /// <summary>
        /// Установка параметров задачи
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="mesh"></param>
        public virtual void SetDataForRiverStream(double waterLevel, double[] fx, double[] fy, ref HKnot right, ref HKnot left)
        {
            this.bottom_x = fx;
            this.bottom_y = fy;
            // генерация сетки
            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);
            if (mesh.CountKnots != eddyViscosity.Length)
            {
                FlagStartMu = true;
                double mMu = eddyViscosity.Sum() / eddyViscosity.Length;
                MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
                MEM.Alloc(mesh.CountKnots, ref eddyViscosity0, "eddyViscosity0");
                MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
                MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                switch (ViscosityModel)
                {
                    case TurbulentViscosityModel.ViscosityConst:
                        {
                            MEM.MemSet(eddyViscosity, mMu);
                            break;
                        }
                    case TurbulentViscosityModel.ViscosityBlasius:
                        {
                            SetMuRoughness();
                            break;
                        }
                    case TurbulentViscosityModel.ViscosityWolfgangRodi:
                        {
                            SetMuRoughnessWRodi();
                            break;
                        }
                    case TurbulentViscosityModel.ViscosityPrandtlKarman:
                        {
                            SetMuDiff();
                            break;
                        }
                    default:
                        {
                            MEM.MemSet(eddyViscosity, mMu);
                            break;
                        }
                }
            }
        }
      
        #region Расчет вязкости потока
        /// <summary>
        /// вычисление приведенной вязкости методом деления пополам
        /// </summary>
        public void ReCalkMu()
        {
            Logger.Instance.Info("вязкость потока в файле: " + Mu.ToString("F6"));
            // вычисление приведенной вязкости и согласованной скорости
            Mu = DMath.RootBisectionMethod(DFRateMu, 0.001, 10);
            SetMu();
            Logger.Instance.Info("вязкость потока ");
            Logger.Instance.Info("согласованная вязкость потока: " + Mu.ToString("F6"));
            FlagStartMu = true;
        }
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        public void ReCalkRoughness()
        {
            Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
            // вычисление приведенной вязкости и согласованной скорости
            roughness = DMath.RootBisectionMethod(DFRateRoughness, 0.00001, 0.01);
            Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
            FlagStartRoughness = true;
        }
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        public void ReCalkRoughnessWRodi()
        {
            Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
            // вычисление приведенной вязкости и согласованной скорости
            roughness = DMath.RootBisectionMethod(DFRateRoughnessWRodi, 0.000001, 0.01);
            Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
            FlagStartRoughness = true;
        }
        /// <summary>
        /// вычисление начльной согласованная шероховатость дна методом деления пополам
        /// </summary>
        public void ReCalkRoughnessDiff()
        {
            Logger.Instance.Info("шероховатость дна : " + roughness.ToString("F6"));
            /// </summary>
            SetMu();
            SolveU();
            // вычисление приведенной вязкости и согласованной скорости
            roughness = DMath.RootBisectionMethod(DFRateRoughnessDiff, 0.5, 5);
            Logger.Instance.Info("согласованная шероховатость дна : " + roughness.ToString("F6"));
            FlagStartRoughness = true;
        }
        /// <summary>
        /// Функция вычисления вязкости для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateMu(double root_mu)
        {
            Mu = root_mu;
            MEM.MemSet(eddyViscosity, root_mu);
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateRoughness(double Roughness)
        {
            roughness = Roughness;
            SetMuRoughness();
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateRoughnessWRodi(double Roughness)
        {
            roughness = Roughness;
            SetMuRoughnessWRodi();
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Функция вычисления шерховатости русла для заданного расхода, уровня свободной поверхности и геометрии
        /// </summary>
        /// <param name="root_mu">искомый корень для mu = const</param>
        /// <returns></returns>
        public double DFRateRoughnessDiff(double Roughness)
        {
            roughness = Roughness;
            SetMuDiff();
            SolveU();
            double Qrfr = RiverFlowRate();
            double errQ = (Qrfr - riverFlowRate) / riverFlowRate;
            return errQ;
        }
        /// <summary>
        /// Установка вязкости при mu = const
        /// </summary>
        public void SetMu()
        {
            MEM.MemSet(eddyViscosity, Mu);
        }
        /// <summary>
        /// Установка вязкости при mu = mu(roughness)
        /// </summary>
        public void SetMuRoughness()
        {
            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);

            double eddyViscosity0 = 1e-6;
            double mu0 = rho_w * eddyViscosity0;
            double Area = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
                Area += mesh.ElemSquare(i);
            double H0 = Area / WetBed;
            double uStar = Math.Sqrt(SPhysics.GRAV * H0 * Params.J);
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                //double zeta = sg.spline.Value(xx[node]);
                double zeta = meshGenerator.Zeta(xx[node]);
                double H = waterLevel - zeta;
                double z = (yy[node] - zeta);
                double xi = z / H;
                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
                    eddyViscosity[node] = mu0;
                else
                    eddyViscosity[node] = rho_w * kappa_w * uStar * (1 - xi) * (roughness + z) + mu0;
            }
        }
        /// <summary>
        /// Установка вязкости при mu = mu(roughness)
        /// </summary>
        public void SetMuRoughnessWRodi()
        {
            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);
            double eddyViscosity0 = 1e-6;
            double mu0 = rho_w * eddyViscosity0;
            double Area = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
                Area += mesh.ElemSquare(i);
            double H0 = Area / WetBed;
            double uStar = Math.Sqrt(SPhysics.GRAV * H0 * Params.J);
            double Re = uStar * H0 / eddyViscosity0;
            double Pa = 0.2;
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                //double zeta = sg.spline.Value(xx[node]);
                double zeta = meshGenerator.Zeta(xx[node]);
                double H = waterLevel - zeta;
                double z = yy[node] - zeta;
                double xi = z / H;
                double zplus = z * uStar / eddyViscosity0;
                if (H <= MEM.Error10 || MEM.Equals(xi, 1))
                {
                    eddyViscosity[node] = mu0;
                }
                else
                {
                    //if (zplus < 30)
                    //{
                    //    mu[node] = mu0 * (1 - xi);
                    //}
                    //if (zplus > 30 && zplus <= 0.2 * Re)
                    if (zplus <= 0.2 * Re)
                    {
                        eddyViscosity[node] = rho_w * kappa_w * uStar * (1 - xi) * (roughness + z) + mu0;
                    }
                    if (zplus > 0.2 * Re)
                    {
                        eddyViscosity[node] = rho_w * kappa_w * uStar * (1 - xi) * (roughness + z) * H /
                        (H + 2 * Math.PI * Math.PI * Pa * (roughness + z) * Math.Sin(Math.PI * xi)) + mu0;
                    }
                }
            }
        }
        /// <summary>
        /// Установка вязкости при  
        /// </summary>
        public void SetMuDiff()
        {
            //double AreaS = 0;
            //for (uint i = 0; i < mesh.CountElements; i++)
            //    AreaS += mesh.ElemSquare(i);
            //double H0 = AreaS / WetBed;
            double eddyViscosity0 = 1e-6;
            double mu0 = rho_w * eddyViscosity0;
            for (int node = 0; node < mesh.CountKnots; node++)
                eddyViscosity[node] = mu0;
            SolveU();
            double S;
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            uint[] knots = { 0, 0, 0 };
            double[] dU = new double[mesh.CountKnots];
            double[] Area = new double[mesh.CountKnots];

            double FArea = 0;
            MEM.Alloc<double>(cu, ref elem_mu, "elem_mu");
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                mesh.GetElemCoords(elem, ref x, ref y);

                mesh.ElemValues(Ux, elem, ref u);
                // получит вязкость в узлах
                mesh.ElemValues(eddyViscosity, elem, ref elem_mu);
                //Площадь
                S = mesh.ElemSquare(elem);
                FArea += S;
                double dudx = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0])) / (2 * S);
                double dudy = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1])) / (2 * S);

                double du = Math.Sqrt(dudx * dudx + dudy * dudy);

                dU[knots[0]] += du * S / 3;
                dU[knots[1]] += du * S / 3;
                dU[knots[2]] += du * S / 3;

                Area[knots[0]] += S / 3;
                Area[knots[1]] += S / 3;
                Area[knots[2]] += S / 3;
            }
            for (uint i = 0; i < mesh.CountKnots; i++)
                dU[i] /= Area[i];

            double[] xx = mesh.GetCoords(0);
            double[] yy = mesh.GetCoords(1);
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                //double zeta = meshGenerator.spline.Value(xx[node]);
                double zeta = meshGenerator.Zeta(xx[node]);
                double H = waterLevel - zeta;
                double z = yy[node] - zeta;
                double xi = z / H;
                double lm = (kappa_w * z) * Math.Exp(-roughness * xi);
                if (H <= MEM.Error10)
                    eddyViscosity[node] = mu0;
                else
                    eddyViscosity[node] = rho_w * lm * lm * Math.Sqrt(dU[node]) + mu0;
            }
        }


        #endregion
        /// <summary>
        /// Нахождение поля скоростей
        /// </summary>
        public void SolveU()
        {
            uint elem = 0;
            try
            {
                algebra.Clear();
                uint[] knots = { 0, 0, 0 };
                double[] x = { 0, 0, 0 };
                double[] y = { 0, 0, 0 };
                double[] a = { 0, 0, 0 };
                double[] b = { 0, 0, 0 };
                double S;
                // выделить память под локальные массивы
                InitLocal(cu);
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    mesh.ElementKnots(elem, ref knots);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // получить среднюю вязкость на КЭ
                    Mu = (eddyViscosity[knots[0]] + eddyViscosity[knots[1]] + eddyViscosity[knots[2]]) / 3;// mesh.ElementMidleValues(eddyViscosity, elem);
                    //Площадь
                    S = mesh.ElemSquare(elem);
                    // Градиенты от функций форм
                    a[0] = (y[1] - y[2]);
                    b[0] = (x[2] - x[1]);
                    a[1] = (y[2] - y[0]);
                    b[1] = (x[0] - x[2]);
                    a[2] = (y[0] - y[1]);
                    b[2] = (x[1] - x[0]);
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = Mu * (a[ai] * a[aj] + b[ai] * b[aj]) / (4 * S);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = Q * S / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(0);
                algebra.BoundConditions(0.0, bound);
                algebra.Solve(ref Ux);
                foreach (var ee in Ux)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("TriSroSecRiverTask.SolveVelosity >> algebra");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }

        /// <summary>
        /// расчет турбулентной вязкости потока
        /// </summary>
        public virtual void SolveMu()
        {
            uint elem = 0;
            try
            {
                algebra.Clear();
                uint[] knots = { 0, 0, 0 };
                double[] x = { 0, 0, 0 };
                double[] y = { 0, 0, 0 };
                // выделить память под локальные массивы
                InitLocal(cu);
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    mesh.ElementKnots(elem, ref knots);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);

                    // получить среднюю вязкость на КЭ
                    //Mu = mesh.ElementMidleValues(mu, elem);
                    Mu = (eddyViscosity[knots[0]] + eddyViscosity[knots[1]] + eddyViscosity[knots[2]]) / 3;// mesh.ElementMidleValues(eddyViscosity, elem);
                    //Площадь
                    double S = mesh.ElemSquare(elem);
                    // Градиенты от функций форм
                    double[] a = new double[cu];
                    double[] b = new double[cu];
                    a[0] = (y[1] - y[2]);
                    b[0] = (x[2] - x[1]);
                    a[1] = (y[2] - y[0]);
                    b[1] = (x[0] - x[2]);
                    a[2] = (y[0] - y[1]);
                    b[2] = (x[1] - x[0]);
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = Mu * (a[ai] * a[aj] + b[ai] * b[aj]) / (4 * S);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = Q * S / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(0);
                algebra.BoundConditions(0.0, bound);
                algebra.Solve(ref Ux);
                foreach (var ee in Ux)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("TriSroSecRiverTask.SolveVelosity >> algebra");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }


        /// <summary>
        /// расчет параметров потока, скоростей и глубины потока
        /// </summary>
        public virtual void SolveVelosity()
        {
            switch (ViscosityModel)
            {
                case TurbulentViscosityModel.ViscosityConst:
                    {
                        if (FlagStartMu == false)
                            ReCalkMu();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.ViscosityBlasius:
                    {
                        if (FlagStartRoughness == false)
                            ReCalkRoughness();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.ViscosityWolfgangRodi:
                    {
                        if (FlagStartRoughness == false)
                            ReCalkRoughnessWRodi();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.ViscosityPrandtlKarman:
                    {
                        if (FlagStartRoughness == false)
                            ReCalkRoughnessDiff();
                        else
                            SolveU();
                        break;
                    }
                case TurbulentViscosityModel.Viscosity2DXY:
                    {
                        //double Pv, Dv;
                        //double dUx, dUy, dUUV2;
                        //double[] xx = mesh.GetCoordsX();
                        //double[] yy = mesh.GetCoordsY();
                        //uint elem = 0;
                        //// буфферезация вязкости
                        //MEM.MemCpy(ref eddyViscosity0, mu);
                        //// цикл по нелинейности
                        //for (int n = 0; n < 100; n++)
                        //{
                        //    double SPv = 0, SDv = 0, gU = 0;
                        //    try
                        //    {
                        //        double[] elem_H = null;
                        //        double[] elem_Xi = null;
                        //        algebra.Clear();
                        //        for (elem = 0; elem < mesh.CountElements; elem++)
                        //        {
                        //            // получить тип и узлы КЭ
                        //            TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                        //            // выбрать по типу квадратурную схему для интегрирования
                        //            if (mesh.First == typeff)
                        //                pIntegration = IPointsA;
                        //            else
                        //                pIntegration = IPointsB;
                        //            // определить количество узлов на КЭ
                        //            cu = knots.Length;
                        //            // выделить память под локальные массивы
                        //            InitLocal(cu);
                        //            MEM.Alloc(cu, ref elem_H, "elem_H");
                        //            MEM.Alloc(cu, ref elem_Xi, "elem_Xi");
                        //            for (int node = 0; node < cu; node++)
                        //            {
                        //                double zeta = sg.spline.Value(xx[knots[node]]);
                        //                elem_H[node] = waterLevel - zeta;
                        //                if (MEM.Equals(elem_H[node], 0))
                        //                {
                        //                    elem_Xi[node] = 0;
                        //                    elem_H[node] = 0.001;
                        //                }
                        //                else
                        //                    elem_Xi[node] = (yy[node] - zeta) / elem_H[node];
                        //            }
                        //            // получить координаты узлов
                        //            mesh.ElemX(elem, ref elem_x);
                        //            mesh.ElemY(elem, ref elem_y);
                        //            // получит вязкость в узлах
                        //            mesh.ElemValues(mu, elem, ref elem_mu);
                        //            mesh.ElemValues(Ux, elem, ref elem_U);
                        //            // получить функции формы КЭ
                        //            ff = FunFormsManager.CreateKernel(typeff);
                        //            // передать координат узлов в функции формы
                        //            ff.SetGeoCoords(elem_x, elem_y);
                        //            // цикл по точкам интегрирования
                        //            for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                        //            {
                        //                ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        //                // вычислить глобальыне производные в точках интегрирования
                        //                ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        //                Mu = 0;
                        //                dUx = 0;
                        //                dUy = 0;
                        //                for (int ai = 0; ai < cu; ai++)
                        //                {
                        //                    // вычислить вязкость в точке интегрирования
                        //                    Mu += elem_mu[ai] * ff.N[ai];
                        //                    // >>>> горизональная производная 
                        //                    dUx += elem_U[ai] * ff.DN_x[ai];
                        //                    dUy += elem_U[ai] * ff.DN_y[ai];
                        //                }
                        //                dUUV2 = Mu * Math.Sqrt(dUx * dUx + dUy * dUy);
                        //                Pv = 0;
                        //                Dv = 0;
                        //                for (int ai = 0; ai < cu; ai++)
                        //                {
                        //                    Pv += (5 * kappa / sigma * rho_w + 2 * Mu0 / (elem_H[ai] * Math.Sqrt(g * elem_H[ai] * J))) * kappa * g * elem_H[ai] * J;
                        //                    Dv += 6 * kappa * kappa / sigma * (dUUV2 + elem_Xi[ai] * elem_Xi[ai] * elem_H[ai] * rho_w * g * J);
                        //                }
                        //                // вычислить глобальыне производные в точках интегрирования
                        //                ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        //                // получть произведение якобиана и веса для точки интегрирования
                        //                double DWJ = ff.DetJ * pIntegration.weight[pi];
                        //                // локальная матрица жесткости                    
                        //                for (int ai = 0; ai < cu; ai++)
                        //                    for (int aj = 0; aj < cu; aj++)
                        //                        LaplMatrix[ai][aj] += (Mu0 + Mu) * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                        //                // Вычисление ЛПЧ

                        //                for (int ai = 0; ai < cu; ai++)
                        //                {
                        //                    LocalRight[ai] += rho_w * (Pv - Dv) * ff.N[ai] * DWJ;
                        //                }
                        //                SPv += Pv; SDv += Dv; gU += dUUV2;
                        //            }
                        //            // добавление вновь сформированной ЛЖМ в ГМЖ
                        //            algebra.AddToMatrix(LaplMatrix, knots);
                        //            // добавление вновь сформированной ЛПЧ в ГПЧ
                        //            algebra.AddToRight(LocalRight, knots);
                        //        }
                        //        Console.WriteLine(" SPv  = {0}  SDv = {1} gU = {2}", SPv, SDv, gU);
                        //        //Удовлетворение ГУ
                        //        uint[] bound = mesh.GetBoundKnotsByMarker(1);
                        //        algebra.BoundConditions(Mu0, bound);
                        //        bound = mesh.GetBoundKnotsByMarker(2);
                        //        algebra.BoundConditions(Mu0, bound);

                        //        algebra.Solve(ref mu);

                        //        foreach (var ee in mu)
                        //            if (double.IsNaN(ee) == true)
                        //                throw new Exception("SolveVelosity >> algebra");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Logger.Instance.Exception(ex);
                        //        Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                        //        Logger.Instance.Info("Элементов обработано :" + elem.ToString());
                        //    }
                        //    // расчет скорости
                        //    SolveU();
                        //}
                        break;
                    }

            }
            FlagStartMu = true;
        }


        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            CreateCalculationDomain();
        }

        public override void LoadData(StreamReader file)
        {
            base.LoadData(file);
            CreateCalculationDomain();
        }
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
            // генерация сетки
            if (meshGenerator == null)
                //meshGenerator = new HStripMeshGeneratorTri();
                meshGenerator = new HStripMeshGenerator(axisOfSymmetry);
            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);

            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            MEM.Alloc(Params.CountKnots, ref tau, "tau");
            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            if (taskPoisson == null)
                taskPoisson = new FEPoissonTaskTri(mesh, algebra);
            else
                taskPoisson.SetTask(mesh, algebra);

            //wMesh = new MWCrossSectionTri(mesh);

            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSecRiverTask1YBase(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReader1YD_RvY();
        }
    }
}
