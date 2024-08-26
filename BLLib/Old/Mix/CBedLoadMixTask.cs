////---------------------------------------------------------------------------
////                 Реализация библиотеки для моделирования 
////                  гидродинамических и русловых процессов
////---------------------------------------------------------------------------
////                Модуль BLLib для расчета донных деформаций 
////                 (учет движения только влекомых наносов)
////                по русловой модели Петрова П.Г. от 1991 г.
////        адаптация модели к многофракционным наносам Потапов И.И. 2021
////                        разработка: Потапов И.И.
////                          21.01.21-13.02.2021
////---------------------------------------------------------------------------
//namespace BLLib
//{
//    using System;
//    using System.Linq;
//    using MeshLib;
//    using SaveDataLib;
//    /// <summary>
//    /// ОО: Класс для решения одномерной задачи по расчету донных 
//    /// деформаций русла сложенного из многих фракций вдоль потока
//    /// </summary>
//    [Serializable]
//    public class CBedLoadMixTask : BedMixModelParams, IBedLoadTask
//    {
//        /// <summary>
//        /// гравитационная постоянная (м/с/с)
//        /// </summary>
//        public double g = 9.81;
//        /// <summary>
//        /// тангенс угла phi
//        /// </summary>
//        public double tanphi;
//        /// <summary>
//        /// критические напряжения на ровном дне для частиц d50
//        /// </summary>
//        public double tau0 = 0;
//        /// <summary>
//        /// критические напряжения на ровном дне для частиц фракции f
//        /// </summary>
//        public double[] tauF0;
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
//        /// <summary>
//        /// Поле касательных напряжений на текущей итерации
//        /// </summary>
//        protected double[] tau = null;

//        #region Рабочие массивы
//        /// <summary>
//        /// массив координаты узлов
//        /// </summary>
//        public double[] x = null;
//        /// <summary>
//        /// массив донных отметок
//        /// </summary>
//        public double[] Zeta = null;
//        /// массив приращения донных отметок на текущем слое по времени
//        /// </summary>
//        public double[] dZeta = null;
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
//        /// <summary>
//        /// Количество фракций дна
//        /// </summary>
//        public int CountFraction;
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

//        double[] S = null;

//        double[] AE = null;
//        double[] AW = null;
//        double[] AP = null;
//        double[] AP0 = null;

//        double[] A = null;
//        double[] B = null;
//        double[][] Gf = null;
//        double[][] Af = null;
//        double[][] Bf = null;
//        public double[][] dZetaf = null;

//        double dz, dx;
//        double mtau, chi;
//        #endregion

//        bool transitBC = true;

//        public CBedLoadMixTask(BedMixModelParams p) : base(p)
//        {
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
//            this.time = 0;
//            this.x = x;
//            this.Zeta0 = Zeta0;
//            this.Count = x.Length;
//            this.N = Count - 1;
//            this.L = x[N] - x[0];
//            this.BCBed = BCBed;
//            this.dtime = dtime;
//            this.isAvalanche = isAvalanche;

//            Zeta = new double[Count];
//            dZeta = new double[Count];
//            S = new double[Count];
//            dZetaf = new double[Count][];
//            for (int i = 0; i < dZetaf.Length; i++)
//                dZetaf[i]=new double[CountMix];
//            //
//            AE = new double[Count];
//            AW = new double[Count];
//            AP = new double[Count];
//            A = new double[N];
//            B = new double[N];
//            Af = new double[N][];
//            Bf = new double[N][];
//            Gf = new double[N][];
//            for (int i = 0; i < A.Length; i++)
//            {
//                Af[i] = new double[CountMix];
//                Bf[i] = new double[CountMix];
//                Gf[i] = new double[CountMix];
//            }
//            CosGamma = new double[N];
//            G0 = new double[N];
//            // узловые массивы
//            Zeta = new double[Count];
//            AP0 = new double[Count];
//            // инициализация массивов для расчета донных фракций
//            InitParamsForMesh(Count, Count - 1);
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
//                        double a = 0;
//                        if (i == j + 1)
//                            a = AW[i];
//                        if (i == j)
//                            a = AP[i];
//                        if (i == j - 1)
//                            a = AE[i];
//                        Console.Write(Format, a);
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
//        public void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau = null, double[] P = null)
//        {
//            if(tau == null && this.tau == null)
//            {
//                Gs = null; dGs = null; return;
//            }
//            if (tau == null)
//                tau = this.tau;

//            if (Gs == null)
//            {
//                Gs = new double[4][]; // idxAll, idxTransit, zeta, idxPress 
//                dGs = new double[4][];
//                for (int i = 0; i < Gs.Length; i++)
//                {
//                    Gs[i] = new double[tau.Length];
//                    dGs[i] = new double[tau.Length];
//                }
//            }
//            // Расчет деформаций дна от влекомых наносов
//            // Давление в узлах Zeta,  Zeta0
//            // Расчет коэффициентов  на грани  P--e--E
//            for (int i = 0; i < N; i++)
//            {
//                mtau = Math.Abs(tau[i]);
//                chi = Math.Sqrt(tau0 / mtau);
//                dx = x[i + 1] - x[i];
//                dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                // косинус гамма
//                CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                double A = Math.Max(0, 1 - chi);
//                double B = (chi / 2 + A) / tanphi;
//                // Расход массовый! только для отрисовки !!! 
//                // для расчетов - объемный
//                double G0 = rho_s * G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                Gs[idxTransit][i] = G0 * A;
//                Gs[zeta][i] = -G0 * B * dz;
//                Gs[idxAll][i] = Gs[idxTransit][i] + Gs[zeta][i];
//            }
//            for (int i = 0; i<N - 1; i++)
//            {
//                dx = x[i + 1] - x[i];
//                dGs[idxTransit][i] = (dGs[idxTransit][i + 1] - dGs[idxTransit][i]) / dx;
//                dGs[zeta][i] = (dGs[zeta][i + 1] - dGs[zeta][i]) / dx;
//                dGs[idxAll][i] = (dGs[idxAll][i + 1] - dGs[idxAll][i]) / dx;
//            }
//        }
//        /// <summary>
//        /// Добавление полей класса в точку сохранения для отрисовки данных
//        /// </summary>
//        /// <param name="sp"></param>
//        public void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            if (sp != null)
//            {
//                double[][] Gs = null;
//                double[][] dGs = null;
//                Calk_Gs(ref Gs, ref dGs);
//                if (Gs != null)
//                {
//                    double[] X = new double[x.Length - 1];
//                    for (int i = 0; i < X.Length; i++)
//                        X[i] = 0.5 * (x[i + 1] + x[i]);

//                    sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit]);
//                    sp.AddCurve("Гравитационный расход наносов", X, Gs[zeta]);
//                    sp.AddCurve("Полный расход наносов", X, Gs[idxAll]);

//                    sp.AddCurve("Градиент транзитного расхода наносов", X, dGs[idxTransit]);
//                    sp.AddCurve("Градиент гравитационного расхода наносов", X, dGs[zeta]);
//                    sp.AddCurve("Градиент полного расхода наносов", X, dGs[idxAll]);
//                }

//                // -------------- поля
//                //for (int ff = 0; ff < CountMix; ff++)
//                //{
//                //    double[] data = new double[x.Length];
//                //    for (int i = 0; i < data.Length; i++)
//                //        data[i] = FractionSurface[i][ff];
//                //    sp.Add("Доля фракции " + ff.ToString() + " в активном слое", data);
//                //}
//                //for (int ff = 0; ff < CountMix; ff++)
//                //{
//                //    double[] data = new double[x.Length];
//                //    for (int i = 0; i < data.Length; i++)
//                //        data[i] = FractionBedLoad[i][ff];
//                //    sp.Add("Доля фракции " + ff.ToString() + " в несущем слое", data);
//                //}
//                //for (int ff = 0; ff < CountMix; ff++)
//                //{
//                //    double[] data = new double[x.Length];
//                //    for (int i = 0; i < data.Length; i++)
//                //        data[i] = FractionSubSurface[i][ff];
//                //    sp.Add("Доля фракции " + ff.ToString() + " в пасивном слое", data);
//                //}
//                // ------------ Кривые
//                for (int ff = 0; ff < CountMix; ff++)
//                {
//                    double[] data = new double[x.Length];
//                    for (int i = 0; i < data.Length; i++)
//                        data[i] = FractionSurface[i][ff];
//                    sp.AddCurve("Доля фракции " + ff.ToString() + " в активном слое", x, data);
//                }
//                for (int ff = 0; ff < CountMix; ff++)
//                {
//                    double[] data = new double[x.Length];
//                    for (int i = 0; i < data.Length; i++)
//                        data[i] = FractionBedLoad[i][ff];
//                    sp.AddCurve("Доля фракции " + ff.ToString() + " в несущем слое", x , data);
//                }
//                //for (int ff = 0; ff < CountMix; ff++)
//                //{
//                //    double[] data = new double[x.Length];
//                //    for (int i = 0; i < data.Length; i++)
//                //        data[i] = FractionSubSurface[i][ff];
//                //    sp.Add("Доля фракции " + ff.ToString() + " в пасивном слое", x , data);
//                //}

//                //for (int ff = 0; ff < CountMix; ff++)
//                //    sp.Add("Процент фракции " + ff.ToString() + " в активном слое", PercentFinerSurface[ff]);

//                //for (int ff = 0; ff < CountMix; ff++)
//                //    sp.Add("Процент фракции " + ff.ToString() + " в активном слое", PercentFinerBedLoad[ff]);

//                //for (int ff = 0; ff < CountMix; ff++)
//                //    sp.Add("Процент фракции " + ff.ToString() + " в пасивном слое", PercentFinerSubSurface[ff]);


//            }
//        }
//        public void PrintKnotFr(string Name, double[][] mas, int FP = 8)
//        {
//            Console.WriteLine(Name);
//            string Format = " {0:F6}";
//            if (FP != 6)
//                Format = " {0:F" + FP.ToString() + "}";

//            for (int fr = 0; fr < mas[0].Length; fr++)
//            {
//                Console.WriteLine("Фракция: "+fr.ToString() + 
//                    " диаметр "+ (1000*SandDiam[fr]).ToString() + " мм"); 
//                for (int n = 0; n < mas.Length; n++)
//                {
//                    Console.Write(Format, mas[n][fr]);
//                }
//                Console.WriteLine();
//            }
//        }
//        /// <summary>
//        /// Аппроксимация фракций с элементов в узлы КЭ сетки
//        /// </summary>
//        /// <param name="elem"></param>
//        /// <param name="knot"></param>
//        public void ElementToKnot(ref double[][] elem, ref double[][] knot)
//        {
//            for (int fr = 0; fr < CountMix; fr++)
//            {
//                knot[0][fr] = elem[0][fr];
//                knot[elem.Length][fr] = elem[elem.Length - 1][fr];
//            }
//            for (int i = 1; i < knot.Length-1; i++)
//            {
//                double dxe = x[i + 1] - x[i];
//                double dxw = x[i] - x[i - 1];
//                for (int fr = 0; fr < CountMix; fr++)
//                    knot[i][fr] = (elem[i][fr] * dxe + elem[i-1][fr] * dxw) / (dxw + dxe);
//            }
//        }
//        /// <summary>
//        /// Конветация узловых фракций к элементам
//        /// </summary>
//        /// <param name="knot"></param>
//        /// <param name="elem"></param>
//        public void KnotToElement(ref double[][] knot, ref double[][] elem)
//        {
//            for (int i = 0; i < elem.Length; i++)
//                for (int fr = 0; fr < CountMix; fr++)
//                    elem[i][fr] = 0.5 * (knot[i+1][fr] + knot[i][fr]);
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
//                this.tau = tau;
//                if (Zeta == null)
//                    Zeta = new double[Zeta0.Length];
//                double[] Area = new double[Zeta.Length];
//                Area[0] = 0.5 * (x[1] - x[0]);
//                Area[Area.Length - 1] = 0.5 * (x[Area.Length - 1] - x[Area.Length - 2]);
//                for (int n = 1; n < Zeta.Length - 1; n++)
//                    Area[n] = 0.5 * (x[n + 1] - x[n - 1]);

//                double[] Gs = new double[Zeta.Length]; 
//                double tau_0 = 9 * kappa * kappa * tanphi * rho_b * rho_w * g /( 8 * cx); 
//                double normTau = rho_b * rho_w * g * tanphi;
//                //
//                //   W       w       P       e       E  
//                //---o-------^-------o-------^-------o------
//                //   i-1             i              i+1 
//                //         tau[i-1]        tau[i]   
//                // Расчет коэффициентов  на грани  P--e--E
//                // Расчет долей и процентов в активном слое
//                KnotToElement(ref FractionSurface, ref Elems_FractionSurface);
//                KnotToElement(ref PercentFinerSurface, ref Elems_PercentFinerSurface);

//                //for (int n = 0; n < bedMixModel.Elems_FractionBedLoad.Length; n++)
//                //    PrintMas("Несущая фракция на элементе " + n.ToString(), bedMixModel.Elems_FractionSurface[n]);
//                //for (int n = 0; n < bedMixModel.Elems_FractionBedLoad.Length; n++)
//                //    PrintMas("Проценты фракция на элементе " + n.ToString(), bedMixModel.Elems_PercentFinerSurface[n]);

//                for (int i = 0; i < N; i++)
//                {
//                    A[i] = 0;
//                    B[i] = 0;
//                    dx = x[i + 1] - x[i];
//                    dz = (Zeta0[i + 1] - Zeta0[i]) / dx;
//                    mtau = Math.Abs(tau[i]);
//                    // косинус гамма
//                    CosGamma[i] = Math.Sqrt(1 / (1 + dz * dz));
//                    G0[i] = G1 * tau[i] * Math.Sqrt(mtau) / CosGamma[i];
//                    // найти фракцию песка в узле
//                    double ds50 = Finer(Elems_PercentFinerSurface[i], 50.0);
//                    // найти критическое напряжение сдвига для среднего d50 размера донной поверхности 
//                    //double taussrg = 0.021 + 0.015 * Math.Exp(-20.0 * sfraction);
//                    // критические напряжения на ровном дне для d50
//                    double taussrg = tau_0 * d50;
//                    double tausg_over_taussrg = tau[i] / taussrg;
//                    // ------- Пересчет долей наносов bedMixModel.FractionBedLoad в несущем слое 

//                    for (int fr = 0; fr < CountMix - 1; fr++)
//                    {
//                        // Степенной параметр b для текущей фракции fr
//                        double power_b = 0.67 / (1 + Math.Exp(1.5 - SandDiam[fr] / ds50));
//                        // критические напряжения на ровном дне для текущей фракции fr
//                        double tau0_fr = taussrg * Math.Pow((SandDiam[fr] / ds50), -1.0 * power_b);
//                        chi = Math.Sqrt(tau0_fr / Math.Abs(tau[i]));
//                        Af[i][fr] = Math.Max(0, 1 - Math.Sqrt(chi));
//                        Bf[i][fr] = (chi / 2 + Af[i][fr]) / tanphi;
//                        //Gf[i][fr] = Math.Max(0, Elems_FractionSurface[i][fr] * G0[i] * (Af[i][fr] - Bf[i][fr] * dz));
//                        Gf[i][fr] = Elems_FractionSurface[i][fr] * G0[i] * (Af[i][fr] - Bf[i][fr] * dz);
//                        A[i] += Af[i][fr];
//                        B[i] += Bf[i][fr];
//                        if (double.IsNaN(A[i]) == true)
//                            A[i] = A[i];
//                    }
//                    Gf[i][CountMix - 1] = 0;

//                    double Gsum = Gf[i].Sum();
//                    Gs[i] = Gsum;
//                    // Первичный расчет долей
//                    for (int fr = 0; fr < CountMix; fr++)
//                        Elems_FractionBedLoad[i][fr] = Math.Abs(Gf[i][fr]) / (Math.Abs(Gsum) + ErrGsum);

//                    // Убираем артифактные всплески при Gsum -> 0 для случаев когда часть расходов
//                    // в Gf[i][fr] имеет разный знак на напорных склонах дна
//                    Gsum = Elems_FractionBedLoad[i].Sum();
//                    for (int fr = 0; fr < CountMix; fr++)
//                        Elems_FractionBedLoad[i][fr] = Elems_FractionBedLoad[i][fr] / (Gsum + ErrGsum);

//                    for (int fr = 0; fr < CountMix; fr++)
//                        if (double.IsNaN(Elems_FractionBedLoad[i][fr]) == true)
//                            Elems_FractionBedLoad[i][fr] = Elems_FractionBedLoad[i][fr];


//                }
//                //for (int n = 0; n < bedMixModel.Elems_FractionBedLoad.Length; n++)
//                //    PrintMas("Несущая фракция на элементе " + n.ToString(), bedMixModel.Elems_FractionBedLoad[n]);

//                // персчет долей фракций из элементных в узловые
//                ElementToKnot(ref Elems_FractionBedLoad, ref FractionBedLoad);

//                //for (int n = 0; n < bedMixModel.FractionBedLoad.Length; n++)
//                //    PrintMas("Несущая фракция в узле " + n.ToString(), bedMixModel.FractionBedLoad[n]);

//                // расчет коэффициентов схемы
//                for (int i = 1; i < N; i++)
//                {
//                    double dxe = x[i + 1] - x[i];
//                    double dxw = x[i] - x[i - 1];
//                    double dxp = 0.5 * (x[i + 1] - x[i - 1]);
//                    AP0[i] = dxp / dtime;

//                    AE[i] = G0[i] * B[i] / dxe;
//                    AW[i] = G0[i - 1] * B[i - 1] / dxw;
//                    AP[i] = AE[i] + AW[i] + AP0[i];
//                    S[i] = -(G0[i] * A[i] - G0[i - 1] * A[i - 1]) + AP0[i] * Zeta0[i];
//                }
//                //PrintMas("AW", AW);
//                //PrintMas("AP", AP);
//                //PrintMas("AE", AE);
//                //PrintMas("S", S);
//                //Console.WriteLine();
//                //PrintMatrix();
//                //  Выполнение граничных условий Неймана
//                if (BCBed.Inlet == TypeBoundCond.Neumann)
//                {
//                    AE[0] = AW[1];
//                    AP[0] = AW[1];
//                    Gtran_in = 0;
//                    dx = x[1] - x[0];
//                    dz = (Zeta0[1] - Zeta0[0]) / dx;
//                    // собираем сумарные наносы на входе с учетом долей поступающих фракций
//                    for (int fr = 0; fr < CountMix - 1; fr++)
//                        Gtran_in += FeedFractionBedLoad[fr] * G0[0] * (Af[0][fr] - Bf[0][fr] * dz);
//                    // определяем разницу заданного потока с BCBed.InletValue с вычисленным
//                    S[0] = BCBed.InletValue - Gtran_in;
//                }
//                if (BCBed.Outlet == TypeBoundCond.Neumann)
//                {
//                    AP[N] = AE[N - 1];
//                    AW[N] = AE[N - 1];
//                    if (transitBC == true)
//                        S[N] = 0;
//                    else
//                    {
//                        dx = x[N - 1] - x[N - 2];
//                        dz = (Zeta0[N - 1] - Zeta0[N - 2]) / dx;
//                        Gtran_out = 0;
//                        for (int fr = 0; fr < CountMix - 1; fr++)
//                            Gtran_out += FractionBedLoad[N - 1][fr] * G0[N - 1] * (Af[N - 1][fr] - Bf[N - 1][fr] * dz);
//                        S[N] = BCBed.OutletValue - Gtran_out;
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

//                //for (int n = 0; n < Zeta.Length; n++)
//                //    if (double.IsNaN(Zeta[n]) == true)
//                //        Zeta[n] = Zeta[n];
//                //PrintMas("Zeta", Zeta);
//                //PrintMas("Gs", Gs);

//                //for (int n = 0; n < Zeta.Length-1; n++)
//                //{
//                //    dZeta[n] = Zeta[n + 1] - Zeta0[n];
//                //    for (int fr = 0; fr < CountMix; fr++)
//                //        dZetaf[n][fr] = dZeta[n]* FractionBedLoad[n][fr];
//                //}
//                //dZeta[Zeta.Length - 1] = dZeta[Zeta.Length - 2];
//                //for (int fr = 0; fr < CountMix; fr++)
//                //    dZetaf[Zeta.Length - 1][fr] = dZetaf[Zeta.Length - 2][fr];
//                // находим приращение по дну между слоями по времени
//                // полное dZeta и по фракциям dZetaf
//                for (int n = 0; n < Zeta.Length; n++)
//                {
//                    dZeta[n] = Zeta[n] - Zeta0[n];
//                    for (int fr = 0; fr < CountMix; fr++)
//                        dZetaf[n][fr] = dZeta[n] * FractionBedLoad[n][fr];
//                }
//                double[] dVf = new double[CountMix];

//                //double[] Area = new double[Zeta.Length];
//                //Area[0] = 0.5 * (x[1] - x[0]);
//                //Area[Area.Length-1] = 0.5 * (x[Area.Length - 1] - x[Area.Length - 2]);
//                //for (int n = 1; n < Zeta.Length-1; n++)
//                //    Area[n] = 0.5 * (x[n + 1] - x[n - 1]);
//                //Ha = 0.01;

//                double relax = 1 / 6.0;
//                for (int n = 0; n < Zeta.Length; n++)
//                {
//                    //for (int fr = 0; fr < CountMix; fr++)
//                    //    if (double.IsNaN(FractionBedLoad[n][fr]) == true)
//                    //        FractionBedLoad[n][fr] = FractionBedLoad[n][fr];
//                    if (Math.Abs(dZeta[n]) > 0.000001)
//                    {
//                        if (dZeta[n] > 0)
//                        {
//                            for (int fr = 0; fr < CountMix; fr++)
//                            {
//                                double alpha_fb = FractionBedLoad[n][fr];
//                                double alpha_f = FractionSurface[n][fr];
//                                if (Ha - Math.Abs(dZeta[n]) <= 0)
//                                    dVf[fr] = Area[n] * alpha_fb * Ha;
//                                else
//                                    dVf[fr] = Area[n] * (alpha_fb * dZetaf[n][fr] + alpha_f * (Ha - Math.Abs(dZeta[n])));
//                            }
//                        }
//                        else
//                        {
//                            for (int fr = 0; fr < CountMix; fr++)
//                            {
//                                double alpha_fb = FractionBedLoad[n][fr];
//                                double alpha_f = FractionSurface[n][fr];
//                                double alpha_fp = FractionSubSurface[n][fr];
//                                if (Ha - Math.Abs(dZeta[n]) <= 0)
//                                    dVf[fr] = Area[n] * Ha * alpha_fp;
//                                else
//                                    dVf[fr] = Area[n] * (alpha_fb * dZetaf[n][fr] + alpha_f * Ha + alpha_fp * Math.Abs(dZeta[n]));
//                            }
//                        }
//                        //for (int fr = 0; fr < CountMix; fr++)
//                        //    if (double.IsNaN(FractionSurface[n][fr]) == true)
//                        //        FractionSurface[n][fr] = FractionSurface[n][fr];

//                        double dV = dVf.Sum();
//                        if (Math.Abs(dZeta[n]) > 0.00001)
//                        {
//                            for (int fr = 0; fr < CountMix; fr++)
//                                FractionSurface[n][fr] = FractionSurface[n][fr] * (1 - relax) + relax * Math.Abs(dVf[fr]) / (Math.Abs(dV) + ErrGsum);
//                        }

//                        //for (int fr = 0; fr < CountMix; fr++)
//                        //    if (double.IsNaN(FractionSurface[n][fr]) == true)
//                        //        FractionSurface[n][fr] = FractionSurface[n][fr];

//                        // нормализация долей
//                        NormalizeFraction(ref FractionSurface[n]);
//                    }
//                }
//                // PrintKnotFr("FractionSurface", FractionSurface);

//                // Расчет процентного содержания фракций по их долям
//                RefrshFraction(dZeta);
//                // Сглаживание дна по лавинной моделе
//                // лавинка традиционныя
//                if (isAvalanche == true)
//                     SAvalanche.Lavina(Zeta, x, tanphi, 0.6, 0);

//                // переопределение начального значения zeta 
//                // для следующего шага по времени
//                for (int n = 0; n < Zeta.Length; n++)
//                    Zeta0[n] = Zeta[n];
//            }
//            catch (Exception e)
//            {
//                Message = e.Message;
//                for (int n = 0; n < Zeta.Length; n++)
//                    Zeta[n] = Zeta0[n];
//            }
//        }
//        public static void Test0()
//        {
//            double rho_w = 1000;
//            double rho_s = 2650;
//            double phi = 30;
//            double epsilon = 0.3;
//            double kappa = 0.2;
//            double f = 0.1;
//            double cx = 0.5;

//            double zeta0 = 0.2;
//            int NN = 15;
//            int Count = NN;
//            double dx = 1.0 / (NN - 1);
//            double[] x = new double[NN];
//            double[] Zeta0 = new double[NN];
//            double[] Zeta = new double[NN];

//            double d50 = 0.001;

//     //       double[] Ks = new double[NN];
//            for (int i = 0; i < NN; i++)
//            {
//                x[i] = i * dx;
//                Zeta0[i] = zeta0;
//              //  Ks[i] = 0.2;
//            }
//            double KsFactor = 2.5;
//            double[] KsRanges = { 0.1, 0.3, 0.7, 1 };
//            double[] FeedPercentFinerBedLoad =
//            { 100, 100, 100, 90,  73,  56,  42,  33,  30,  15,  6,   0 };
//            double[] Init_PercentFinerSubSurface =
//            { 100,  90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 };
//            // Диаметры фракций песка в мм
//            double[] SandDiam = { 0.181, 0.0905, 0.04225, 0.02263, 0.01131, 0.00566, 0.00283, 0.00141, 0.000707, 0.000353, 0.000177, 0.0 };
//            double[,] KsI = {
//            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 },
//            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 },
//            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 },
//            { 100, 90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 } };
//            double[][] Init_RangePercentFinerSurface = new double[KsI.GetLength(0)][];
//            for (int i = 0; i < KsI.GetLength(0); i++)
//            {
//                Init_RangePercentFinerSurface[i] = new double[KsI.GetLength(1)];
//                for (int fi = 0; fi < KsI.GetLength(1); fi++)
//                    Init_RangePercentFinerSurface[i][fi] = KsI[i, fi];
//            }
//            double Ha = 0.001;

//            BedMixModelParams bedMixModelParams = new BedMixModelParams(KsFactor, Ha, SandDiam, KsRanges,
//                                FeedPercentFinerBedLoad, Init_RangePercentFinerSurface, Init_PercentFinerSubSurface,
//                                rho_w, rho_s, phi, d50, epsilon, kappa, cx);

//          CBedLoadMixTask blp = new CBedLoadMixTask(bedMixModelParams);
//          CBedLoadMixTask bltask = new CBedLoadMixTask(blp);

//            // задача Дирихле
//            BCondition BCBed = new BCondition(TypeBoundCond.Dirichlet, TypeBoundCond.Dirichlet, 2*zeta0, 2*zeta0);


//            double dtime = 1; // 0.01;
//            bool isAvalanche = false;
//            bltask.SetTask(null, BCBed, x, Zeta0, dtime, isAvalanche);
//            double T = 2 * bltask.tau0;
//            double dT = 0;// 0.5*T / (Zeta0.Length);
//            double Ti = T;
//            double[] tau = new double[NN - 1];
//            for (int i = 0; i < NN - 1; i++)
//            {
//                tau[i] = Ti; Ti += dT;
//            }
//            for (int i = 0; i < 50; i++)
//            {
//                bltask.CalkZetaFDM(ref Zeta, tau);
//                bltask.PrintMas("zeta", Zeta);
//            }

//            Console.Read();
//            Console.WriteLine();
//            Console.WriteLine();
//        }

//        //public static void Main()
//        //{
//        //    // Гру
//        //    Test0();
//        //}
//    }
//}
