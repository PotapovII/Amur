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
//namespace BLLib
//{
//    using System;
//    using SaveDataLib;
//    using MeshLib;
//    using AlgebraLib;
//    /// <summary>
//    /// ОО: Класс для решения одномерной задачи о 
//    /// расчете донных деформаций русла вдоль потока
//    /// </summary>
//    [Serializable]
//    public class CBedLoadTask2D : BedLoadParams, IBedLoadTask
//    {
//        /// <summary>
//        /// Модель влекомого движения донного матеиала
//        /// </summary>
//        public TypeBLModel blm { get => blm; }
//        TypeBLModel blm;
//        /// <summary>
//        /// Сетка решателя
//        /// </summary>
//        public IMesh Mesh { get => mesh; }
//        protected IMesh mesh;
//        /// <summary>
//        /// Алгебра задачи
//        /// </summary>
//        public IAlgebra Algebra { get => algebra; }
//        protected IAlgebra algebra;
//        /// <summary>
//        /// Для правой части
//        /// </summary>
//        protected IAlgebra Ralgebra;
//        /// <summary>
//        /// Текщий уровень дна
//        /// </summary>
//        public double[] CZeta { get => Zeta0;  }
//        /// <summary>
//        /// массив донных отметок на предыдущем слое по времени
//        /// </summary>
//        protected double[] Zeta0 = null;
//        /// <summary>
//        /// гравитационная постоянная (м/с/с)
//        /// </summary>
//        public double g = 9.81;
//        /// <summary>
//        /// тангенс угла phi
//        /// </summary>
//        public double tanphi;
//        /// <summary>
//        /// критические напряжения на ровном дне
//        /// </summary>
//        public double tau0 = 0;
//        ///// <summary>
//        ///// транзитный расход на входе
//        ///// </summary>
//        //public double Gtran_in = 0;
//        ///// <summary>
//        ///// транзитный расход на выходе
//        ///// </summary>
//        //public double Gtran_out = 0;
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
//        #region Рабочие переменные
//        /// <summary>
//        /// массив донных отметок
//        /// </summary>
//        protected double[] Zeta = null;
//        /// <summary>
//        /// Учет лавинного осыпания 
//        /// </summary>
//        public bool isAvalanche = false;
//        /// <summary>
//        /// Узлы КЭ
//        /// </summary>
//        protected uint[] knots = null;
//        /// <summary>
//        /// координаты узлов КЭ
//        /// </summary>
//        protected double[] x = null;
//        protected double[] y = null;
//        /// <summary>
//        /// локальная матрица часть СЛАУ
//        /// </summary>
//        protected double[][] LaplMatrix = null;
//        /// <summary>
//        /// локальная матрица часть СЛАУ
//        /// </summary>
//        private double[][] RMatrix = null;
//        /// <summary>
//        /// Параметр схемы по времени
//        /// </summary>
//        private double theta;
//        /// <summary>
//        /// локальная правая часть СЛАУ
//        /// </summary>
//        protected double[] LocalRight = null;
//        /// <summary>
//        /// адреса ГУ
//        /// </summary>
//        protected uint[] adressBound = null;
//        /// <summary>
//        /// Вспомогательная правая часть
//        /// </summary>
//        private double[] MRight = null;

//        protected NumInegrationPoints pIntegration;
//        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
//        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
//        #endregion

//        #region Краевые условия

//        /// <summary>
//        /// тип задаваемых ГУ
//        /// </summary>
//        public BCondition BCBed;

//        #endregion

//        #region Служебные переменные
//        /// <summary>
//        /// текущее время расчета 
//        /// </summary>
//        public double time = 0;
//        /// <summary>
//        /// текущая итерация по времени 
//        /// </summary>
//        public int CountTime = 0;
//        /// <summary>
//        /// относительная точность при вычислении 
//        /// изменения донной поверхности
//        /// </summary>
//        protected double eZeta = 0.000001;
//        /// <summary>
//        /// шаг по времени
//        /// </summary>
//        public double dtime;
//        /// <summary>
//        /// множитель для приведения придонного давления к напору
//        /// </summary>
//        double gamma;

//        /// <summary>
//        ///  косинус гамма - косинус угола между 
//        ///  нормалью к дну и вертикальной осью
//        /// </summary>
//        double[] CosGamma = null;
//        double[] tau = null;
//        double[] G0 = null;
//        double[] A = null;
//        double[] B = null;
//        double[] C = null;
//        double[] ps = null;

//        double dz, dx, dp;
//        double mtau, chi;
//        #endregion

//        /// <summary>
//        /// Конструктор по умолчанию/тестовый
//        /// </summary>
//        public CBedLoadTask2D(BedLoadParams p) : base(p)
//        {
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
//        /// <summary>
//        /// Установка исходных данных
//        /// </summary>
//        /// <param name="mesh">Сетка расчетной области</param>
//        /// <param name="algebra">Решатель для СЛАУ </param>
//        /// <param name="BCBed">граничные условия</param>
//        /// <param name="Zeta0">начальный уровень дна</param>
//        /// <param name="theta">Параметр схемы по времени</param>
//        /// <param name="dtime">шаг по времени</param>
//        /// <param name="isAvalanche">флаг использования лавинной модели</param>
//        public void SetTask(IMesh mesh, IAlgebra algebra, BCondition BCBed, TypeBLModel blm, double[] Zeta0, double theta, double dtime)
//        {
//            this.blm = blm;
//            this.mesh = mesh;
//            this.algebra = algebra;
//            this.Zeta0 = Zeta0;
//            this.BCBed = BCBed;
//            this.theta = theta;
//            this.dtime = dtime;
//            IPointsA.SetInt((int)mesh.typeRangeMesh, mesh.First);
//            IPointsB.SetInt((int)mesh.typeRangeMesh, mesh.Second);
//            if (algebra == null)
//            {
//                // получение ширины ленты для алгебры
//                int WidthMatrix = (int)mesh.GetWidthMatrix();
//                this.algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);
//            }
//            uint Count = (uint)mesh.CountElements;
//            // узловые массивы
//            Zeta = new double[Count];
//            ps = new double[Count];
//            CosGamma = new double[Count];
//            tau = new double[Count];
//            A = new double[Count];
//            B = new double[Count];
//            C = new double[Count];
//            G0 = new double[Count];
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
//        /// создание/очистка ЛМЖ и ЛПЧ ...
//        /// </summary>
//        /// <param name="cu">количество неизвестных</param>
//        public virtual void InitLocal(int cu, int cs = 1)
//        {
//            Alloc<double>(cu, ref x);
//            Alloc<double>(cu, ref y);
//            // с учетом степеней свободы
//            int Count = cu * cs;
//            AllocClear(Count, ref LocalRight);
//            Alloc2DClear(Count, ref LaplMatrix);
//            Alloc2DClear(Count, ref RMatrix);
//        }

//        #region  Утилиты
//        /// <summary>
//        /// Выделение памяти или очистка для ЛМЖ
//        /// </summary>
//        /// <param name="Count"></param>
//        /// <param name="LaplMatrix"></param>
//        public void Alloc2DClear(int Count, ref double[][] LaplMatrix)
//        {
//            if (LaplMatrix == null)
//            {
//                LaplMatrix = new double[Count][];
//                for (int i = 0; i < Count; i++)
//                    LaplMatrix[i] = new double[Count];
//            }
//            else
//            {
//                if (Count == LaplMatrix.Length)
//                {
//                    for (int i = 0; i < Count; i++)
//                    {
//                        for (int j = 0; j < Count; j++)
//                            LaplMatrix[i][j] = 0;
//                    }
//                }
//                else
//                {
//                    LaplMatrix = new double[Count][];
//                    for (int i = 0; i < Count; i++)
//                        LaplMatrix[i] = new double[Count];
//                }
//            }
//        }
//        /// <summary>
//        /// Выделение памяти или очистка для ЛПЧ
//        /// </summary>
//        /// <param name="N"></param>
//        /// <param name="X"></param>
//        public void AllocClear(int N, ref double[] X)
//        {
//            if (X == null)
//                X = new double[N];
//            else
//            {
//                if (X.Length != N)
//                    X = new double[N];
//                else
//                    for (int i = 0; i < N; i++)
//                        X[i] = 0;
//            }
//        }
//        /// <summary>
//        /// Выделение памяти 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="N"></param>
//        /// <param name="X"></param>
//        public void Alloc<T>(int N, ref T[] X)
//        {
//            if (X == null)
//                X = new T[N];
//            else
//                if (X.Length != N)
//                X = new T[N];
//        }
//        /// <summary>
//        /// Получение адресов неизвестных
//        /// </summary>
//        /// <param name="bound">узлы опорной сетки</param>
//        /// <param name="adressBound">неизвестные</param>
//        /// <param name="cs">количество степеней свободы в узле</param>
//        public void GetAdress(uint[] bound, ref uint[] adressBound, int cs = 2)
//        {
//            Alloc<uint>(bound.Length * cs, ref adressBound);
//            if (cs == 1)
//                adressBound = bound;
//            for (int ai = 0; ai < bound.Length; ai++)
//            {
//                int li = cs * ai;
//                for (uint i = 0; i < cs; i++)
//                    adressBound[li + i] = bound[ai] * (uint)cs + i;
//            }
//        }
//        /// <summary>
//        /// Печать ЛМЖ для отладки
//        /// </summary>
//        /// <param name="M"></param>
//        /// <param name="Count"></param>
//        /// <param name="F"></param>
//        public void Print(double[][] M, int F = 2)
//        {
//            Console.WriteLine("Matrix");
//            string SF = "F" + F.ToString();
//            for (int i = 0; i < M.Length; i++)
//            {
//                for (int j = 0; j < M[i].Length; j++)
//                    Console.Write(" " + M[i][j].ToString(SF));
//                Console.WriteLine();
//            }
//        }
//        public void Print(double[] M, int F = 2)
//        {
//            Console.WriteLine("R");
//            string SF = "F" + F.ToString();
//            for (int i = 0; i < M.Length; i++)
//            {
//                Console.Write(" " + M[i].ToString(SF));
//                Console.WriteLine();
//            }
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
//        #endregion 

//        ///  /// <summary>
//        /// Вычисление текущих расходов и их градиентов для построения графиков
//        /// </summary>
//        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
//        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
//        /// <param name="tau">придонное касательное напряжение</param>
//        /// <param name="P">придонное давление</param>
//        public void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
//        {
//            //if (Gs == null)
//            //{
//            //    Gs = new double[4][]; // idxAll, idxTransit, zeta, press 
//            //    dGs = new double[4][];
//            //    for (int i = 0; i < Gs.Length; i++)
//            //    {
//            //        Gs[i] = new double[tau.Length];
//            //        dGs[i] = new double[tau.Length];
//            //    }
//            //}
//            //// Расчет деформаций дна от влекомых наносов
//            //// Давление в узлах Zeta,  Zeta0
//            //for (int j = 1; j < N; j++)
//            //    ps[j] = 0.5 * (P[j] + P[j - 1]) * gamma; // (rho_w * g);
//            //                                             // !!!линейная интерполяция плоха!!! нужна квадратичная
//            //ps[0] = (2 * P[1] - P[2]) * gamma; // (rho_w * g);
//            //ps[N] = (2 * P[N - 1] - P[N - 2]) * gamma; // (rho_w * g);
//            //// Расчет коэффициентов  на грани  P--e--E
//            //for (int i = 0; i < N; i++)
//            //{
//            //    mtau = Math.Abs(tau[i]);
//            //    chi = Math.Sqrt(tau0 / mtau);
//            //    dx = x[i + 1] - x[i];
//            //    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//            //    dp = (ps[i + 1] - ps[i]) / dx;
//            //    // косинус гамма
//            //    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//            //    double A = Math.Max(0, 1 - chi);
//            //    double B = (chi / 2 + A * (1 + s) / s) / tanphi;
//            //    double C = A / (s * tanphi);
//            //    // Расход массовый! только для отрисовки !!! 
//            //    // для расчетов - объемный
//            //    double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//            //    Gs[idxTransit][i] = G0 * A;
//            //    Gs[zeta][i] = -G0 * B * dz;
//            //    Gs[press][i] = -G0 * C * dp;
//            //    Gs[idxAll][i] = Gs[idxTransit][i] + Gs[zeta][i] + Gs[press][i];
//            //}
//            //for (int i = 0; i < N - 1; i++)
//            //{
//            //    dx = x[i + 1] - x[i];
//            //    dGs[idxTransit][i] = (dGs[idxTransit][i + 1] - dGs[idxTransit][i]) / dx;
//            //    dGs[zeta][i] = (dGs[zeta][i + 1] - dGs[zeta][i]) / dx;
//            //    dGs[press][i] = (dGs[press][i + 1] - dGs[press][i]) / dx;
//            //    dGs[idxAll][i] = (dGs[idxAll][i + 1] - dGs[idxAll][i]) / dx;
//            //}
//        }
//        public void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            if (sp != null)
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
//        public void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY, double[] P = null)
//        {
//            try
//            {
//                double err = 0.0000001;
//                if (Zeta == null)
//                    Zeta = new double[Zeta0.Length];
//                // Подготовка коэффициетов
//                // напряжения заданны на элементе
//                if(tauX.Length == mesh.CountElements)
//                {
//                    for (uint elem = 0; elem < mesh.CountElements; elem++)
//                    {
//                        tau[elem] = Math.Sqrt(tauX[elem] * tauX[elem] + tauY[elem] * tauY[elem]);
//                        double cosA = tauX[elem] / ( tau[elem] + err);
//                        double sinA = tauY[elem] / (tau[elem] + err);
//                    }    
//                }

//                algebra.Clear();
//                Ralgebra.Clear();
//                for (uint elem = 0; elem < mesh.CountElements; elem++)
//                {
//                    // получить узлы КЭ
//                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
//                    // определить тип объекта для численного интегрирования 
//                    if (mesh.First == typeff)
//                        pIntegration = IPointsA;
//                    else
//                        pIntegration = IPointsB;
//                    // получить функции формы
//                    AbFunForm ff = FunFormsManager.CreateKernel(typeff);
//                    int cu = knots.Length;
//                    InitLocal(cu);
//                    // Получить координаты узлов
//                    mesh.ElemX(elem, ref x);
//                    mesh.ElemY(elem, ref y);
//                    // установка координат узлов в функции формы
//                    ff.SetGeoCoords(x, y);


//                    // цикл по точкам интегрирования
//                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
//                    {
//                        // вычисление глоб. производных от функции формы
//                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
//                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
//                        double DWJ = ff.DetJ * pIntegration.weight[pi];

//                        double Mu = 1;
//                        double Q = 1;

//                        for (int ai = 0; ai < cu; ai++)
//                            for (int aj = 0; aj < cu; aj++)
//                            {

//                                // Лапласс
//                                double Difuz = Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]);
//                                // Матрица масс
//                                double MassTime = ff.N[ai] * ff.N[aj] / dtime;

//                                LaplMatrix[ai][aj] += (MassTime + theta * Difuz) * DWJ;

//                                RMatrix[ai][aj] += (MassTime - (1 - theta) * Difuz) * DWJ;
//                            }
//                        // Вычисление ЛПЧ
//                        for (int ai = 0; ai < cu; ai++)
//                            LocalRight[ai] += Q * ff.N[ai] * DWJ;
//                    }
//                    // добавление вновь сформированной ЛЖМ в ГМЖ
//                    algebra.AddToMatrix(LaplMatrix, knots);
//                    // добавление вновь сформированной ЛЖМ в ГМЖ
//                    Ralgebra.AddToMatrix(RMatrix, knots);
//                    // добавление вновь сформированной ЛПЧ в ГПЧ
//                    algebra.AddToRight(LocalRight, knots);
//                }
//                Ralgebra.getResidual(ref MRight, Zeta0, 0);
//                algebra.CopyRight(MRight);
//                //Удовлетворение ГУ


//                uint[] bound = mesh.GetBoundKnotsByMarker(1);
//                double[] TL = new double[bound.Length];
//                for (int i = 0; i < TL.Length; i++)
//                    TL[i] = Zeta0[bound[i]];
//                algebra.BoundConditions(TL, bound);

//                bound = mesh.GetBoundKnotsByMarker(3);
//                double[] TR = new double[bound.Length];
//                for (int i = 0; i < TR.Length; i++)
//                    TR[i] = Zeta0[bound[i]];
//                algebra.BoundConditions(TR, bound);

//                //algebra.Print();
//                algebra.Solve(ref Zeta);

//                // Сглаживание дна по лавинной моделе
//                //if (isAvalanche == true)
//                // SAvalanche.Lavina(Zeta, x, tanphi, 0.6, 0);
//                // переопределение начального значения zeta 
//                // для следующего шага по времени
//                for (int j = 0; j < Zeta.Length; j++)
//                    Zeta0[j] = Zeta[j];

//                //// Расчет деформаций дна от влекомых наносов
//                //if (P != null)
//                //{
//                //    // Петров А.Г. - Потапов И.И. 2014
//                //    // Давление в узлах Zeta,  Zeta0
//                //    for (int j = 1; j < N; j++)
//                //        ps[j] = 0.5 * (P[j] + P[j - 1]) * gamma; // (rho_w * g);
//                //                                                 // линейная интерполяция плоха!!! нужна квадратичная
//                //    ps[0] = (2 * P[1] - P[2]) * gamma; // (rho_w * g);
//                //    ps[N] = (2 * P[N - 1] - P[N - 2]) * gamma; // (rho_w * g);

//                //    // Расчет коэффициентов  на грани  P--e--E
//                //    for (int i = 0; i < N; i++)
//                //    {
//                //        mtau = Math.Abs(tau[i]);
//                //        chi = Math.Sqrt(tau0 / mtau);
//                //        dx = x[i + 1] - x[i];
//                //        dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                //        dp = (ps[i + 1] - ps[i]) / dx;
//                //        // косинус гамма
//                //        CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                //        A[i] = Math.Max(0, 1 - chi);
//                //        B[i] = (chi / 2 + A[i] * (1 + s) / s) / tanphi;
//                //        C[i] = A[i] / (s * tanphi);
//                //        G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                //    }
//                //    // расчет коэффициентов схемы
//                //    for (int i = 1; i < N; i++)
//                //    {
//                //        double dxe = x[i + 1] - x[i];
//                //        double dxw = x[i] - x[i - 1];
//                //        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                //        CE[i] = G0[i] * C[i] / dxe;
//                //        CW[i] = G0[i - 1] * C[i - 1] / dxw;
//                //        CP[i] = CE[i] + CW[i];
//                //        AP0[i] = dxp / dtime;
//                //        AE[i] = G0[i] * B[i] / dxe;
//                //        AW[i] = G0[i - 1] * B[i - 1] / dxw;
//                //        AP[i] = AE[i] + AW[i] + AP0[i];
//                //        S[i] = CE[i] * ps[i + 1] - CP[i] * ps[i] + CW[i] * ps[i - 1] -
//                //            (G0[i] * A[i] - G0[i - 1] * A[i - 1]) + AP0[i] * Zeta0[i];
//                //    }
//                //}
//                //else
//                //{
//                //    // Петров П.Г. 1991
//                //    // Расчет коэффициентов  на грани  P--e--E
//                //    for (int i = 0; i < N; i++)
//                //    {
//                //        mtau = Math.Abs(tau[i]);
//                //        chi = Math.Sqrt(tau0 / mtau);
//                //        dx = x[i + 1] - x[i];
//                //        dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                //        dp = (ps[i + 1] - ps[i]) / dx;
//                //        A[i] = Math.Max(0, 1 - Math.Sqrt(chi));
//                //        B[i] = (chi / 2 + A[i]) / tanphi;
//                //        // косинус гамма
//                //        CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                //        G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                //    }
//                //    // расчет коэффициентов схемы
//                //    for (int i = 1; i < N; i++)
//                //    {
//                //        double dxe = x[i + 1] - x[i];
//                //        double dxw = x[i] - x[i - 1];
//                //        double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                //        AP0[i] = dxp / dtime;
//                //        AE[i] = G0[i] * B[i] / dxe;
//                //        AW[i] = G0[i - 1] * B[i - 1] / dxw;
//                //        AP[i] = AE[i] + AW[i] + AP0[i];
//                //        S[i] = -(G0[i] * A[i] - G0[i - 1] * A[i - 1]) + AP0[i] * Zeta0[i];
//                //    }
//                //}
//                ////PrintMas("AW", AW);
//                ////PrintMas("AP", AP);
//                ////PrintMas("AE", AE);
//                ////PrintMas("S", S);
//                ////Console.WriteLine();
//                ////PrintMatrix();
//                //if (BCBed.Inlet == TypeBoundCond.Neumann)
//                //{
//                //    Gtran_in = G0[0] * A[0];
//                //    AE[0] = AW[1];
//                //    AP[0] = AW[1];
//                //    S[0] = BCBed.InletValue - Gtran_in;
//                //}
//                //if (BCBed.Outlet == TypeBoundCond.Neumann)
//                //{
//                //    Gtran_out = G0[N - 1] * A[N - 1];
//                //    AP[N] = AE[N - 1];
//                //    AW[N] = AE[N - 1];
//                //    if (idxTransit == true)
//                //        S[N] = 0;
//                //    else
//                //        S[N] = BCBed.OutletValue - Gtran_out;
//                //}
//                ////PrintMas("AW", AW);
//                ////PrintMas("AP", AP);
//                ////PrintMas("AE", AE);
//                ////PrintMas("S", S);
//                ////Console.WriteLine();
//                ////PrintMatrix();
//                //// Прогонка
//                //Solver solver = new Solver(Count);
//                //solver.SetSystem(AW, AP, AE, S, Zeta);
//                //// выполнение граничных условий Dirichlet
//                //solver.CalkBCondition(BCBed);
//                //Zeta = solver.SolveSystem();
//                ////PrintMas("AW", AW);
//                ////PrintMas("AP", AP);
//                ////PrintMas("AE", AE);
//                ////PrintMas("S", S);
//                ////Console.WriteLine();
//                ////PrintMatrix();
//                //// Zeta = solver.SolveSystem();
//                //             algebra.Clear();

//                //// Градиенты от функций форм
//                //double[] a = new double[cu];
//                //double[] b = new double[cu];

//                //double[] m0 = { 2, 1, 1 };
//                //double[] m1 = { 1, 2, 1 };
//                //double[] m2 = { 1, 1, 2 };
//                //double[][] Mass = new double[3][];
//                //Mass[0] = m0;
//                //Mass[1] = m1;
//                //Mass[2] = m2;
//                //double Q = 0;
//                //double alpha = 0.7;
//                //uint[] knots = { 0, 0, 0, 0 };
//                //double[] z = { 0, 0, 0 };
//                //double[] p = { 0, 0, 0 };
//                //double[] x = { 0, 0, 0 };
//                //double[] y = { 0, 0, 0 };
//                //for (uint elem = 0; elem < mesh.CountElements; elem++)
//                //{
//                //    // узлы
//                //    mesh.ElementKnots(elem,ref knots);

//                //    //Координаты и площадь
//                //    mesh.ElemX(elem, ref x);
//                //    mesh.ElemY(elem, ref y);

//                //    //Площадь
//                //    double S = mesh.ElemSquare(x, y);

//                //    mesh.ElemValues(Zeta, elem, ref z);
//                //    mesh.ElemValues(P, elem, ref p);

//                //    double Tau = tau[elem];

//                //    a[0] = (y[1] - y[2]);
//                //    b[0] = (x[2] - x[1]);
//                //    a[1] = (y[2] - y[0]);
//                //    b[1] = (x[0] - x[2]);
//                //    a[2] = (y[0] - y[1]);
//                //    b[2] = (x[1] - x[0]);

//                //    double dzdx = 0;
//                //    double dzdy = 0;
//                //    double dpdx = 0;
//                //    double dpdy = 0;

//                //    for (int ai = 0; ai < cu; ai++)
//                //    {
//                //        dzdx += a[ai] * z[ai];
//                //        dzdy += b[ai] * z[ai];
//                //        dpdx += a[ai] * p[ai];
//                //        dpdy += b[ai] * p[ai];
//                //    }
//                //    double Dx = 1;
//                //    double Dy = 1;

//                //    //mtau = Math.Abs(tau[elem]);
//                //    //chi = Math.Sqrt(tau0 / mtau);
//                //    //dx = x[elem + 1] - x[elem];
//                //    //dz = (Zeta0[elem + 1] - Zeta0[elem]) / dx;
//                //    //dp = (ps[elem + 1] - ps[elem]) / dx;
//                //    //A[elem] = Math.Max(0, 1 - Math.Sqrt(chi));
//                //    //B[elem] = (chi / 2 + A[elem]) / tanphi;
//                //    //// косинус гамма
//                //    //CosGamma[elem] = Math.Sqrt(1 / (1 + dz * dz));
//                //    //G0[elem] = G1 * tau[elem] * Math.Sqrt(mtau) / CosGamma[elem];

//                //    // Вычисление ЛЖМ
//                //    for (int ai = 0; ai < cu; ai++)
//                //        for (int aj = 0; aj < cu; aj++)
//                //        {
//                //            LaplMatrix[ai][aj] = Mass[ai][aj]/S/dtime + (1-alpha)*(Dx*a[ai] * a[aj] + Dy*b[ai] * b[aj]) / (4 * S);
//                //        }
//                //    // добавление вновь сформированной ЛЖМ в ГМЖ
//                //    algebra.AddToMatrix(LaplMatrix, knots);

//                //    // Вычисление ЛПЧ
//                //    for (int ai = 0; ai < cu; ai++)
//                //    {
//                //        LocalRight[ai] = 0;
//                //        for (int aj = 0; aj < cu; aj++)
//                //        {
//                //            LocalRight[ai] += ( Mass[ai][aj] / S / dtime - alpha * (Dx * a[ai] * a[aj] + Dy * b[ai] * b[aj]) / (4 * S) )*z[aj];
//                //        }
//                //    }
//                //    // Вычисление ЛПЧ
//                //    Q = 0;
//                //    for (int ai = 0; ai < cu; ai++)
//                //        LocalRight[ai] += Q * S / 3;

//                //    // добавление вновь сформированной ЛПЧ в ГПЧ
//                //    algebra.AddToRight(LocalRight, knots);
//                //}
//                ////Удовлетворение ГУ
//                //uint[] bound = mesh.GetBoundKnotsByMarker(1);
//                //algebra.BoundConditions(0.0, bound);

//                //algebra.Solve(ref Zeta);
//                //// Сглаживание дна по лавинной моделе
//                ////if (isAvalanche == true)
//                ////    SAvalanche.Lavina(Zeta, x, tanphi, 0.6, 0);
//                //// переопределение начального значения zeta 
//                //// для следующего шага по времени
//                //for (int j = 0; j < Zeta.Length; j++)
//                //    Zeta0[j] = Zeta[j];
//            }
//            catch (Exception e)
//            {
//                Message = e.Message;
//                for (int j = 0; j < Zeta.Length; j++)
//                    Zeta[j] = Zeta0[j];
//            }
//        }
//    }
//}