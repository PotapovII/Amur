//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка Потапов И.И.
//							08.02.2021
//---------------------------------------------------------------------------
namespace BLLib
{
    using MemLogLib;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// ОО: Трехслойная модель дна с настраиваемой фракционностью активного слоя 
    /// по высоте шероховатости активного слоя 
    /// ---------------------------------------------------------------------
    /// BedLoad - транспортный слой дна
    /// ---------------------------------------------------------------------
    /// Surface - активный слой дна
    /// ---------------------------------------------------------------------
    /// SubSurface - пасивный слой дна
    /// ---------------------------------------------------------------------
    /// </summary>
    /// <summary>
    /// ОО: Параметры задачи используемые при расчете донных деформаций для 
    /// двухcлойной модели дна с настраиваемой фракционностью пасивного слоя по 
    /// высоте шероховатости активного слоя 
    /// </summary>
    [Serializable]
    public class BedMixModelParams : BedLoadParams
    {
        /// <summary>
        /// Параметр для расчета шероховатости Ks
        /// </summary>
        [DisplayName("Параметр для расчета Ks")]
        [Category("Дно")]
        public double KsFactor { get; set; }
        /// <summary>
        /// Количество фракций
        /// </summary>
        [DisplayName("Количество фракций донного материала")]
        [Category("Дно")]
        public int CountMix { get; protected set; }
        /// <summary>
        /// Глубина активного слоя
        /// </summary>
        [DisplayName("Глубина активного слоя")]
        [Category("Дно")]
        public double Ha { get; set; }
        #region ДАННЫЕ В УЗЛАХ
        /// <summary>
        /// Высота шероховатости в узлах КЭ сетки
        /// </summary>
        public double[] Ks;
        /// <summary>
        /// Диаметры фракций песка в мм
        /// </summary>
        public double[] SandDiam;
        // -------------------- Объемные доли частиц в узлах -------------------------
        /// <summary>
        /// Объемные доли частиц в активном слое 
        /// в узлах  для уаждой фракции f 
        /// </summary>
        public double[][] FractionSurface;
        /// <summary>
        /// Объемные доли частиц в пасивном слое 
        /// в узлах для уаждой фракции f
        /// </summary>
        public double[][] FractionSubSurface;
        /// <summary>
        /// Объемные доли частиц в потоке влекомых наносов 
        /// в узлах для уаждой фракции f
        /// </summary>
        public double[][] FractionBedLoad;
        // ----- Процентное распределение гранулометрического состава  в узлах ------
        /// <summary>
        /// Процентное распределение гранулометрического состава  
        /// в активном слое в узлах  для каждой фракции f 
        /// </summary>
        public double[][] PercentFinerSurface;
        /// <summary>
        /// Процентное распределение гранулометрического состава
        /// в пасивном слое в узлах  для каждой фракции f
        /// </summary>
        public double[][] PercentFinerSubSurface;
        /// <summary>
        /// Процентное распределение гранулометрического состава 
        /// в потоке влекомых наносов в узлах для каждой фракции f
        /// </summary>
        public double[][] PercentFinerBedLoad;
        #endregion
        #region ДАННЫЕ В КЭ
        // ------------------- Объемные доли частиц на КЭ ----------------------
        /// <summary>
        /// Объемные доли частиц в активном слое 
        /// на КЭ для каждой фракции f 
        /// </summary>
        public double[][] Elems_FractionSurface;
        /// <summary>
        /// Объемные доли частиц в пасивном слое 
        /// на КЭ для каждой фракции f 
        /// </summary>
        public double[][] Elems_FractionSubSurface;
        /// <summary>
        /// Объемные доли частиц в несущем слое наносов
        /// на КЭ для каждой фракции f 
        /// </summary>
        public double[][] Elems_FractionBedLoad;
        /// <summary>
        /// Процентное распределение гранулометрического состава  
        /// на КЭ для каждой фракции f
        /// </summary>
        public double[][] Elems_PercentFinerSurface;
        /// <summary>
        /// Процентное распределение гранулометрического состава 
        /// в потоке влекомых наносов во всех узлах границы втекания 
        /// для каждой фракции f
        /// </summary>
        public double[] FeedPercentFinerBedLoad;
        /// <summary>
        /// Объемные доли частиц в активном слое 
        /// в потоке влекомых наносов во всех узлах границы втекания 
        /// для каждой фракции f
        /// </summary>
        public double[] FeedFractionBedLoad;
        /// <summary>
        /// Интервалы для выбора грулометрии по донной шероховатости
        /// </summary>
        public double[] KsRanges;
        #endregion
        /// <summary>
        /// "База фракций" для пассивного слоя, выбор зависит от шероховатости активного слоя
        /// </summary>
        public double[][] Init_RangePercentFinerSurface;
        /// <summary>
        /// Начальные условия для пассивного слоя
        /// </summary>
        public double[] Init_PercentFinerSubSurface;
        /// <summary>
        /// Погрешность при расчете суммарых долей фракций
        /// </summary>
        protected double ErrGsum = 0.000000000001;
        /// <summary>
        /// фракции донного материала, определены в логарифмической 
        /// шкале с основание 2 в (миллиметрах)
        /// </summary>
        protected static double[] di = { 256, 128, 64, 32, 16, 8, 4, 2, 1, 0.5, 0.25, 0.125 };
        public BedMixModelParams() : base()
        {
            double[] KsRanges = { 0.1, 0.3, 0.7, 1 };
            double[] FeedPercentFinerBedLoad =
            { 100, 100, 100, 90,  73,  56,  42,  33,  30,  15,  6,   0 };
            double[] Init_PercentFinerSubSurface =
            { 100,  90,  80,  70,  60,  56,  42,  33,  30,  15,  6,   0 };
            // Диаметры фракций песка в мм
            double[] SandDiam =
            { 0.181, 0.0905, 0.04225, 0.02263, 0.01131, 0.00566, 0.00283, 0.00141, 0.000707, 0.000353, 0.000177, 0.0 };
            double[] KsI0 = { 100, 90, 80, 70, 60, 56, 42, 33, 30, 15, 6, 0 };
            double[] KsI1 = { 100, 90, 80, 70, 60, 56, 42, 33, 30, 15, 6, 0 };
            double[] KsI2 = { 100, 90, 80, 70, 60, 56, 42, 33, 30, 15, 6, 0 };
            double[] KsI3 = { 100, 90, 80, 70, 60, 56, 42, 33, 30, 15, 6, 0 };
            this.KsFactor = 2.5;
            this.Ha = 0.001;
            this.CountMix = 12;
            this.KsRanges = KsRanges;
            this.FeedPercentFinerBedLoad = FeedPercentFinerBedLoad;
            this.Init_PercentFinerSubSurface = Init_PercentFinerSubSurface;
            this.SandDiam = SandDiam;
            this.Init_RangePercentFinerSurface = new double[4][];
            this.Init_RangePercentFinerSurface[0] = KsI0;
            this.Init_RangePercentFinerSurface[1] = KsI1;
            this.Init_RangePercentFinerSurface[2] = KsI2;
            this.Init_RangePercentFinerSurface[3] = KsI3;
        }

        /// <summary>
        /// Формирование фракционного описания донной поверхности
        /// </summary>
        /// <param name="mesh">КЭ сетка</param>
        /// <param name="Ks">высота шероховатости дна по которой определяются диапазоны фракций</param>
        /// <param name="KsRanges">диапазоны для видов фракционного состава дна в активном слое</param>
        /// <param name="Init_RangePercentFinerSurface">диапазоны видов фракционного состава активного слоя</param>
        /// <param name="Init_PercentFinerSubSurface">фракционный состав фракционного состава пасивного слоя</param>
        /// <param name="FeedPercentFinerBedLoad">фракция на входе в область</param>
        /// <param name="KsFactor">фактор шероховатости</param>
        public BedMixModelParams(BedMixModelParams bmmp) : base(bmmp)
        {
            SetParams(bmmp);
        }
        public void SetParams(BedMixModelParams bmmp)
        {
            base.SetParams(bmmp);
            this.Ha = bmmp.Ha;
            this.KsFactor = bmmp.KsFactor;
            this.KsRanges = bmmp.KsRanges;
            this.SandDiam = bmmp.SandDiam;
            this.CountMix = bmmp.Init_RangePercentFinerSurface[0].Length;
            this.FeedPercentFinerBedLoad = bmmp.FeedPercentFinerBedLoad;
            this.Init_RangePercentFinerSurface = bmmp.Init_RangePercentFinerSurface;
            this.Init_PercentFinerSubSurface = bmmp.Init_PercentFinerSubSurface;
            this.CountMix = Init_RangePercentFinerSurface[0].Length;
        }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public override void SetParams(object p)
        {
            SetParams((BedMixModelParams)p);
        }
        public override object GetParams()
        {
            return (BedMixModelParams)this;
        }

        public BedMixModelParams(double KsFactor, double Ha, double[] SandDiam, double[] KsRanges,
                double[] FeedPercentFinerBedLoad, double[][] Init_RangePercentFinerSurface, double[] Init_PercentFinerSubSurface,
                BedLoadParams blp) :
        base(blp)
        {

            this.KsFactor = KsFactor;
            this.CountMix = CountMix;
            this.Ha = Ha;
            this.SandDiam = SandDiam;
            this.KsRanges = KsRanges;
            this.FeedPercentFinerBedLoad = FeedPercentFinerBedLoad;
            this.Init_RangePercentFinerSurface = Init_RangePercentFinerSurface;
            this.Init_PercentFinerSubSurface = Init_PercentFinerSubSurface;
            this.CountMix = Init_RangePercentFinerSurface[0].Length;
        }

        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public override void Load(StreamReader file)
        {
            string line;
            string[] sline;
            base.Load(file);
            try
            {
                //Параметр для расчета шероховатости KsFactor
                this.KsFactor = LOG.GetDouble(file.ReadLine());
                // Количество фракций донного материала
                this.Ha = LOG.GetDouble(file.ReadLine());
                // ------------------------------------------------------------------------------
                // Массивы фракционного состава дна
                // ------------------------------------------------------------------------------
                // SandDiam
                line = file.ReadLine();
                sline = line.Split(' ');
                SandDiam = new double[sline.Length - 1];
                for (int i = 1; i < sline.Length; i++)
                    SandDiam[i - 1] = double.Parse(sline[i].Trim(), LOG.formatter);

                this.CountMix = sline.Length - 1;

                // KsRanges
                line = file.ReadLine();
                sline = line.Split(' ');
                KsRanges = new double[sline.Length - 1];
                for (int i = 1; i < sline.Length; i++)
                    KsRanges[i - 1] = double.Parse(sline[i].Trim(), LOG.formatter);

                // FeedPercentFinerBedLoad
                line = file.ReadLine();
                sline = line.Split(' ');
                FeedPercentFinerBedLoad = new double[sline.Length - 1];
                for (int i = 1; i < sline.Length; i++)
                    FeedPercentFinerBedLoad[i - 1] = double.Parse(sline[i].Trim(), LOG.formatter);

                // Init_PercentFinerSubSurface
                line = file.ReadLine();
                sline = line.Split(' ');
                Init_PercentFinerSubSurface = new double[sline.Length - 1];
                for (int i = 1; i < sline.Length; i++)
                    Init_PercentFinerSubSurface[i - 1] = double.Parse(sline[i].Trim(), LOG.formatter);

                // KsI
                double[][] KsI = new double[KsRanges.Length][];
                for (int s = 0; s < KsRanges.Length; s++)
                {
                    line = file.ReadLine();
                    sline = line.Split(' ');
                    KsI[s] = new double[sline.Length - 1];
                    for (int i = 1; i < sline.Length; i++)
                        KsI[s][i - 1] = double.Parse(sline[i].Trim(), LOG.formatter);
                }
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
                Logger.Instance.Info("Не согласованность версии программы и файла данных");
                Logger.Instance.Info("Гранулометрические данные не инициализированны!");
            }
        }
        /// <summary>
        /// Выделение памяти под рабочии массивы
        /// </summary>
        /// <param name="CountKnots"></param>
        /// <param name="CountElements"></param>
        public override void InitParamsForMesh(int CountKnots, int CountElements)
        {
            bool flagSet = false;
            if (Ks == null)
                flagSet = true;
            else
                if (Ks.Length != CountKnots)
                flagSet = true;
            //Ks = new double[CountKnots];
            //FeedFractionBedLoad = new double[CountMix];
            MEM.Alloc<double>(CountKnots, ref Ks);
            MEM.Alloc<double>(CountMix, ref FeedFractionBedLoad);

            MEM.Alloc2D<double>(CountKnots, CountMix, ref FractionSurface);
            MEM.Alloc2D<double>(CountKnots, CountMix, ref FractionBedLoad);
            MEM.Alloc2D<double>(CountKnots, CountMix, ref FractionSubSurface);
            MEM.Alloc2D<double>(CountKnots, CountMix, ref PercentFinerSurface);
            MEM.Alloc2D<double>(CountKnots, CountMix, ref PercentFinerSubSurface);
            MEM.Alloc2D<double>(CountKnots, CountMix, ref PercentFinerBedLoad);

            MEM.Alloc2D<double>(CountElements, CountMix, ref Elems_FractionSurface);
            MEM.Alloc2D<double>(CountElements, CountMix, ref Elems_FractionSubSurface);
            MEM.Alloc2D<double>(CountElements, CountMix, ref Elems_FractionBedLoad);
            MEM.Alloc2D<double>(CountElements, CountMix, ref Elems_PercentFinerSurface);

            //FractionSurface = new double[CountKnots][];
            //FractionBedLoad = new double[CountKnots][];
            //FractionSubSurface = new double[CountKnots][];
            //PercentFinerSurface = new double[CountKnots][];
            //PercentFinerSubSurface = new double[CountKnots][];
            //PercentFinerBedLoad = new double[CountKnots][];
            //Elems_FractionSurface = new double[CountElements][];
            //Elems_FractionSubSurface = new double[CountElements][];
            //Elems_FractionBedLoad = new double[CountElements][];
            //Elems_PercentFinerSurface = new double[CountElements][];


            //for (int n = 0; n < CountKnots; n++)
            //{
            //	FractionSurface[n] = new double[CountMix];
            //	FractionSubSurface[n] = new double[CountMix];
            //	FractionBedLoad[n] = new double[CountMix];
            //	PercentFinerSurface[n] = new double[CountMix];
            //	PercentFinerSubSurface[n] = new double[CountMix];
            //	PercentFinerBedLoad[n] = new double[CountMix];
            //}
            //for (int n = 0; n < CountElements; n++)
            //{
            //	Elems_FractionSurface[n] = new double[CountMix];
            //	Elems_FractionSubSurface[n] = new double[CountMix];
            //	Elems_FractionBedLoad[n] = new double[CountMix];
            //	Elems_PercentFinerSurface[n] = new double[CountMix];
            //}
            if (flagSet == true)
            {
                // настройка рабочих массивов
                for (int n = 0; n < CountKnots; n++)
                {
                    int id = Get_f(Ks[n]);
                    for (int f = 0; f < CountMix; f++)
                    {
                        PercentFinerSurface[n][f] = Init_RangePercentFinerSurface[id][f];
                        PercentFinerSubSurface[n][f] = Init_PercentFinerSubSurface[f];
                        PercentFinerBedLoad[n][f] = 100;
                    }
                    // вычисляем объемные доли 
                    FindFraction(PercentFinerSurface[n], ref FractionSurface[n]);
                    FindFraction(PercentFinerSubSurface[n], ref FractionSubSurface[n]);
                    double d90 = Finer(PercentFinerSurface[n], 90.0);
                    //  вычисляем новую шероховатость дна
                    Ks[n] = KsFactor * d90;
                }
                // вычисляем объемные доли для потока на входе в область
                FindFraction(FeedPercentFinerBedLoad, ref FeedFractionBedLoad);
            }
        }

        #region МЕТОДЫ

        #region работа с процентным составом
        /// <summary>
        /// Обновление процентного состава фракций по их новому долевому содержанию
        /// </summary>
        /// <param name="dZeta"></param>
        public void RefrshFraction(double[] dZeta)
        {
            for (uint node = 0; node < FractionBedLoad.Length; node++)
            {
                if (Math.Abs(dZeta[node]) < MEM.Error8) continue;
                // вычисляет новое процентное содержание фракций в несущем слое
                PercentFiner(FractionBedLoad[node], ref PercentFinerBedLoad[node]);
                // вычисляет новое процентное содержание фракций в активном слое
                PercentFiner(FractionSurface[node], ref PercentFinerSurface[node]);
                double d90mm = Finer(PercentFinerSurface[node], 90.0);
                Ks[node] = d90mm * KsFactor;
            }
        }
        /// <summary>
        /// вычисляет содержание объемных долей фракций из процентного их содержания 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void FindFraction(double[] input, ref double[] output)
        {
            for (int i = 0; i < input.Length - 1; i++)
                output[i] = (input[i] - input[i + 1]) / 100;
            output[input.Length - 1] = 0;
        }
        /// <summary>
        /// вычисляет процентное содержание фракций из объемных долей 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void PercentFiner(double[] input, ref double[] output)   // computes % finer from fractions
        {
            int i;
            output[0] = 100.0;
            for (i = 1; i < input.Length - 1; i++) output[i] = output[i - 1] - input[i - 1] * 100.0;
            output[11] = 0;
        }
        #endregion
        /// <summary>
        /// Возвращает номер гранулометрического состав по шероховатости дна
        /// </summary>
        /// <param name="ks">шероховатость дна</param>
        /// <returns></returns>
        public int Get_f(double ks)
        {
            for (int i = 0; i < KsRanges.Length; i++)
                if (ks < KsRanges[i])
                    return i;
            return KsRanges.Length;
        }
        /// <summary>
        /// вычисляет размер зерна d_pc таким образом, чтобы pc% материала 
        /// было мельче: таким образом, если pc = 90.0, то на выходе будет D90
        /// </summary>
        /// <param name="PercentSediments">процентный гран состав</param>
        /// <param name="pc">процент</param>
        /// <returns></returns>
        public static double Finer(double[] PercentSediments, double pc)
        {
            int i = 0; do { i++; } while (PercentSediments[i] >= pc);
            double pp = PercentSediments[i - 1] - PercentSediments[i];
            double xp = pc - PercentSediments[i];
            double d_pc = Math.Exp(Math.Log(di[i]) + ((Math.Log(di[i - 1]) - Math.Log(di[i])) / pp) * xp);
            return d_pc / 1000;
        }
        /// <summary>
        /// Нормировка фракций в долях
        /// </summary>
        /// <param name="grains"></param>
        public void NormalizeFraction(ref double[] grains)
        {
            double sum = 0;
            for (int i = 0; i < grains.Length; i++)
            {
                //if (Math.Abs( grains[i] ) < 1e-4)
                if (grains[i] < 1e-4)
                    grains[i] = 0;
                sum += grains[i];
            }
            if (ErrGsum < sum)
                for (int i = 0; i < grains.Length; i++)
                    grains[i] = grains[i] / Math.Abs(sum);
        }

        /// <summary>
        /// Расчет объемныех долей несущего водогрунтового слоя
        /// </summary>
        /// <param name="Gf">расход наносов на интервале для фракции f</param>
        /// <param name="Fraction">объемныех долей на интервале</param>
        /// <returns></returns>
        public double GetElemFraction(double[] Gf, ref double[] Fraction)
        {
            double alpha;
            double Gsum = Gf.Sum();
            double mGsum = Math.Abs(Gsum);
            if (mGsum > MEM.Error8)
            {
                // Первичный расчет долей
                for (int fr = 0; fr < CountMix; fr++)
                    Fraction[fr] = Math.Abs(Gf[fr]) / mGsum;

                for (int fr = 0; fr < CountMix; fr++)
                    if (Fraction[fr] < MEM.Error5)
                        Fraction[fr] = 0;

                alpha = Fraction.Sum();
                for (int fr = 0; fr < CountMix; fr++)
                    Fraction[fr] = Fraction[fr] / alpha;
            }
            else
            {
                Gsum = 0;
                for (int fr = 0; fr < CountMix; fr++)
                    Fraction[fr] = 0;
            }
            return Gsum;
        }


        #region не активные методы, работа с гравием и т.д.
        /// <summary>
        /// Получение диаметра для заданного в процентах диаметра в узлах сетки
        /// </summary>
        /// <param name="xd_x"></param>
        /// <returns></returns>
        public double[] GetSurfaceLayer_d_x(double xd_x)
        {
            double[] SurfaceLayer_d_x = new double[PercentFinerSurface.Length];
            for (uint node = 0; node < FractionBedLoad.Length; node++)
                SurfaceLayer_d_x[node] = Finer(PercentFinerSurface[node], xd_x);  //  get d10 of surface layer
            return SurfaceLayer_d_x;
        }
        /// <summary>
        /// Вычисляет безразмерное напряжение сдвига Shield
        /// </summary>
        /// <param name="Umean"></param>
        /// <param name="C"></param>
        /// <param name="d50"></param>
        /// <returns></returns>
        public static double Shields(double Umean, double C, double d50)
        {
            // d50 в (мм)
            double specific_gravity = 2.65;
            double tau_star;
            tau_star = Umean * Umean / (C * C * (specific_gravity - 1.0) * d50);
            return (tau_star);
        }
        /// <summary>
        /// finds sand fraction based on surface fraction
        /// находит фракцию песка на основе фракции поверхности
        /// </summary>
        /// <param name="xf"></param>
        /// <returns></returns>
        public static double FindSandFraction(double[] xf)
        {
            double sandfraction = 0;
            int sf = xf.Length / 2;
            if ((sf + 1) < xf.Length) sf++;
            for (int f = sf; f < xf.Length; f++)
                sandfraction += xf[f];
            //sandfraction = xf[7] + xf[8] + xf[9] + xf[10] + xf[11];
            return (sandfraction);
        }
        /// <summary>
        /// Расчет гравийных фракции в донном материале по заданному % содержанию
        /// </summary>
        /// <param name="inputfraction">фракционный состав</param>
        /// <param name="xty">% содержание фракции</param>
        /// <returns></returns>
        public static double FinerGravel(double[] inputfraction, double xty)
        {
            double[] gsize = { 256.0, 128.0, 64.0, 32.0, 16.0, 8.0, 4.0, 2.0 };
            int i;
            double xtdy, sum;
            double[] pfgravel = new double[8];
            double[] gravelfraction = new double[8];
            sum = 0.0;
            for (i = 0; i < gsize.Length; i++)
                sum = sum + inputfraction[i];
            for (i = 0; i < gsize.Length; i++)
                gravelfraction[i] = inputfraction[i] / sum;
            pfgravel[0] = 100.0;
            for (i = 1; i < gsize.Length - 1; i++)
                pfgravel[i] = pfgravel[i - 1] - gravelfraction[i - 1] * 100.0;
            pfgravel[gsize.Length - 1] = 0;
            i = 0;
            do { i++; } while (pfgravel[i] >= xty);
            xtdy = Math.Exp(Math.Log(gsize[i]) + ((Math.Log(gsize[i - 1]) - Math.Log(gsize[i])) / (pfgravel[i - 1] - pfgravel[i])) * (xty - pfgravel[i]));
            return (xtdy);
        }
        #endregion
        #endregion
    }
}
