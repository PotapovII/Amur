//---------------------------------------------------------------------------
//   Класс ParamsStream2D предназначен для определения физических параметров задачи 
//                              Потапов И.И.
//                        - (C) Copyright 2015 -
//                          ALL RIGHT RESERVED
//                               31.07.15
//---------------------------------------------------------------------------
//   Реализация библиотеки для решения задачи турбулентного теплообмена
//                   методом контрольного объема
//---------------------------------------------------------------------------
//                  Интегрирован в проект "Русловые процессы"
//                              13.02.21
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 16.02.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver_1XD.River2D_FVM_ke
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib;
    using CommonLib.EConverter;
    /// <summary>
    /// Типы донной формы
    /// </summary>
    [Serializable]
    public enum TypeBedForm
    {
        /// <summary>
        /// Плоское дно
        /// </summary>
        [Description("Плоское дно")]
        PlaneForm = 0,
        /// <summary>
        /// Синосоидальная центральная часть дна
        /// </summary>
        [Description("Синосоидальная центральная часть дна")]
        L1_L2sin_L3
    }
    /// <summary>
    /// типы спец. рашателей задачи алгебры
    /// </summary>
    [Serializable]
    public enum TypeMAlgebra
    {
        [Description("Трехдиагональная поперечная прогонка")]
        TriDiagMat_Algorithm = 0,
        [Description("Метод сопряженных градиентов")]
        CGD_Algorithm,
        [Description("Паралельный метод сопряженных градиентов")]
        CGD_ParrallelAlgorithm
    }

    /// <summary>
    /// типы задачи
    /// </summary>
    [Serializable]
    public enum TypeStreamTask
    {
        [Description("Поток из под щита")]
        StreamFromShield = 0,
        [Description("Взвешенная струя")]
        OffsetStreamJet,
        [Description("Взвешенная струя - ровное дно 0.5h")]
        OffsetStreamJet0_0_5h,
        [Description("Взвешенная струя - ровное дно 1h")]
        OffsetStreamJet0_1h,
        [Description("Взвешенная струя - ровное дно 1.5h")]
        OffsetStreamJet0_1_5h,
        [Description("Взвешенная струя - ровное дно 2h")]
        OffsetStreamJet0_2h,
        [Description("Взвешенная струя - ровное дно 2.5h")]
        OffsetStreamJet0_2_5h
    }

    /// <summary>
    /// Граничные условия на верхней границе области
    /// </summary>
    [Serializable]
    public enum RoofCondition
    {
        [Description("Скольжение по крышке")]
        slip = 0,
        [Description("Прилипание на крышке")]
        adhesion = 1
    }
    /// <summary>
    /// ОО: Физические параметры модели к-e
    /// </summary>
    [Serializable]
    public class PatankarParams1XD : ITProperty<PatankarParams1XD>
    {
        public int nfmax = 7;
        ///// <summary>
        /// Размерность модели  
        /// колиество переменных задачи: u, v, pc, t, tke, dis
        /// </summary>
        [DisplayName("Колиество переменных")]
        [Description("Колиество переменных задачи")]
        [Category("Задача")]
        public int CountUnknow => nfmax;

        /// <summary>
        /// Количество КО для давления по Х
        /// </summary>
        [DisplayName("Количество КО по Х")]
        [Description("Количество КО для давления по Х")]
        [Category("Сетка")]
        public int FV_X { get; set; }
        /// <summary>
        /// Количество КО для давления по Х
        /// </summary>
        [DisplayName("Количество КО по У")]
        [Description("Количество КО для давления по У")]
        [Category("Сетка")]
        public int FV_Y { get; set; }

        /// <summary>
        /// Количество КО для давления по Х
        /// </summary>
        [DisplayName("Макс кол. итераций по сетке")]
        [Description("Макс кол. итераций по сетке")]
        [Category("Сетка")]
        public int MaxCoordIters { get; set; }
        /// <summary>
        /// Тип формы дна
        /// </summary>
        [DisplayName("Начальной формы дна")]
        [Description("Начальной формы донной поверхности")]
        [Category("Начальная форма дна")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TypeBedForm typeBedForm { get; set; }
        /// <summary>
        /// Амплитуда донной поверхности
        /// </summary>
        [DisplayName("Амплитуда дна")]
        [Description("Амплитуда донных волн на 2 участке (м)")]
        [Category("Начальная форма дна")]
        public double bottomWaveAmplitude { get; set; }
        /// <summary>
        /// Количество донных волн
        /// </summary>
        [DisplayName("Количество волн")]
        [Description("Количество донных волн на 2 участке")]
        [Category("Начальная форма дна")]
        public int wavePeriod { get; set; }
        /// <summary>
        /// Количество инераций по движению узлов границы
        /// </summary>
        [DisplayName("Итераций по границе")]
        [Description("Количество итераций по движению узлов границы")]
        [Category("Сетка")]
        public int CountBoundaryMove { get; set; }

        /// <summary>
        /// Ширина расчетной области  -> по X
        /// </summary>
        [DisplayName("Глубина водотока")]
        [Category("Геометрия области")]
        public double Lx { get { return Wen1 + Wen2 + Wen3; } }
        /// <summary>
        /// Высота расчетной области -> по Y
        /// </summary>
        [DisplayName("Длина водотока")]
        [Category("Геометрия области")]
        public double Ly { get { return Len1 + Len2 + Len3; } }
        /// <summary>
        /// Длина водотока на 1 участке (вход потока)
        /// </summary>
        [DisplayName("Длина 1 участка")]
        [Description("Длина водотока на 1 участке (вход потока)")]
        [Category("Геометрия области")]
        public double Len1 { get; set; }
        /// <summary>
        /// Длина водотока на 3 участке (центр)
        /// </summary>
        [DisplayName("Длина 2 участка")]
        [Description("Длина водотока на 3 участке (центр)")]
        [Category("Геометрия области")]
        public double Len2 { get; set; }
        /// <summary>
        /// Длина водотока на 3 участке (истечение)
        /// </summary>
        [DisplayName("Длина 3 участка")]
        [Description("Длина водотока на 3 участке (истечение)")]
        [Category("Геометрия области")]
        public double Len3 { get; set; }
        /// <summary>
        /// Глубина водотока 1 придонный участок
        /// </summary>
        [DisplayName("Глубина 1 участка")]
        [Description("Глубина водотока 1 придонный участок")]
        [Category("Геометрия области")]
        public double Wen1 { get; set; }
        /// <summary>
        /// Глубина 2 участка
        /// </summary>
        [DisplayName("Глубина 2 участка")]
        [Category("Геометрия области")]
        public double Wen2 { get; set; }
        /// <summary>
        /// Глубина 3 участка
        /// </summary>
        [DisplayName("Глубина 3 участка")]
        [Description("Глубина водотока 1 приповерхностный участок")]
        [Category("Геометрия области")]
        public double Wen3 { get; set; }

        /// <summary>
        /// Расчет концентрации вместо температуры true == да
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Расчет взвешенных наносов")]
        [Description("Расчет взвешенных наносов")]
        [Category("Физика")]
        public bool TemperOrConcentration { get; set; }
        /// <summary>
        /// Расчет температуры true == да
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Вычисления температуры")]
        [Description("Флаг вычисления поля температуры да/нет")]
        [Category("Физика")]
        public bool flatTermoTask { get; set; }
        /// <summary>
        /// Температура в 1 слое
        /// </summary>
        [DisplayName("Температура 1 слой")]
        [Description("Температура в 1 слое (С)")]
        [Category("Граничные условия на втоке")]
        public double t1 { get; set; }
        /// <summary>
        /// Температура в 2 слое
        /// </summary>
        [DisplayName("Температура 2 слой")]
        [Description("Температура в 2 слое (С)")]
        [Category("Граничные условия на втоке")]
        public double t2 { get; set; }
        /// <summary>
        /// Температура в 3 слое
        /// </summary>
        [DisplayName("Температура 3 слой")]
        [Description("Температура в 3 слое (С)")]
        [Category("Граничные условия на втоке")]
        public double t3 { get; set; }
        /// <summary>
        /// Скорость в 1 придонном слое
        /// </summary>
        [DisplayName("Скорость 1 слоя")]
        [Description("Скорость в 1 придонном слое (м/с)")]
        [Category("Граничные условия на втоке")]
        public double V1_inlet { get; set; }
        /// <summary>
        /// Скорость в 1 придонном слое
        /// </summary>
        [DisplayName("Скорость 2 слоя")]
        [Description("Скорость в 2 придонном слое (м/с)")]
        [Category("Граничные условия на втоке")]
        public double V2_inlet { get; set; }
        /// <summary>
        /// Скорость в 1 придонном слое
        /// </summary>
        [DisplayName("Скорость 3 слоя")]
        [Description("Скорость в 3 придонном слое (м/с)")]
        [Category("Граничные условия на втоке")]
        public double V3_inlet { get; set; }
        /// <summary>
        /// Граничные условия для скоростей на верхней границе области
        /// </summary>
        [DisplayName("ГУ на крышке")]
        [Description("Граничные условия для скоростей на верхней границе области")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public RoofCondition bcIndex { get; set; }
        /// <summary>
        /// типы задачи по входной струе
        /// </summary>
        [DisplayName("Тип струи на входе")]
        [Description("Тип струи на входе по умолчанию")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TypeStreamTask typeStreamTask { get; set; }

        [DisplayName("Тип задачи")]
        [Description("Определение типа входной струи")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))] 
        public TypeMAlgebra typeMAlgebra { get; set; }
        /// <summary>
        /// Максимальное количество итераций по нелинейности
        /// </summary>
        [DisplayName("Кол-во итер.по нелин.")]
        [Description("Максимальное количество итераций по нелинейности")]
        [Category("Алгоритм")]
        public int NonLinearIterations { get; set; }
        /// <summary>
        /// Число Прандтля для уравнения теплопроводности
        /// </summary>
        [DisplayName("Индекс серийной задачи")]
        [Description("Индекс серийной задачи используется в зависимости от контекста")]
        [Category("Алгоритм")]
        public int TaskIndex { get; set; }

        /// <summary>
        /// Расчет температуры true == да
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Деформаяй дна от x = 0")]
        [Description("Деформаяй дна от x = 0 (Да) со второго участуа (Нет)")]
        [Category("Алгоритм")]
        public bool bedLoadStart_X0 { get; set; }
        /// <summary>
        /// Расчет температуры true == да
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Деформаяй дна только Tau>0")]
        [Description("Деформаяй дна только от положительных напряжений (Да) с учетом зон рецеркуляции (Нет)")]
        [Category("Алгоритм")]
        public bool bedLoadTauPlus { get; set; }
        /// <summary>
        /// Расчет температуры true == да
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Начальыне условия по струе")]
        [Description("Струя сформирована в области (Да) только на входе (Нет)")]
        [Category("Алгоритм")]
        public bool streamInsBoundary { get; set; }
        /// <summary>
        /// Расчет температуры true == да
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Подсос на границе втекания")]
        [Description("Подсос на границе втекания (Да) только через сопла (Нет)")]
        [Category("Алгоритм")]
        public bool velocityInsBoundary { get; set; }
        /// <summary>
        /// Движение узлов сетки по горизонтальным границам
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Горизонт. движение узлов")]
        [Description("Движение узлов сетки по горизонтальным границам")]
        [Category("Сетка")]
        public bool topBottom { get; set; }
        /// <summary>
        /// Движение узлов сетки по горизонтальным границам
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Вертик. движение узлов")]
        [Description("Движение узлов сетки по вертикальным границам")]
        [Category("Сетка")]
        public bool leftRight { get; set; }
        /// <summary>
        /// Движение узлов сетки по горизонтальным границам
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Фильтрация на обвал дна")]
        [Description("Фильтрация на обвал дна")]
        [Category("Сетка")]
        public bool localFilterBLMesh { get; set; }
        /// <summary>
        /// Движение узлов сетки по горизонтальным границам
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Расчет напряжений на всем дне")]
        [Description("Расчет напряжений на всем дне или только на размываемом участке")]
        [Category("Опции")]
        public bool AllBedForce { get; set; }
        /// <summary>
        /// получение ссылки на объект
        /// </summary>
        /// <returns></returns>
        public object GetParams() { return this; }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p) 
        { 
            Set((PatankarParams1XD)p);
        }

        protected void TestCaseStreamFromShield()
        {
            int indexExp = TaskIndex;
            double[] AA = { 0.0224, 0.0496, 0.056, 0.064, 0.076, 0.08 };
            // длины волн
            double[] LL = { 0.4, 0.9, 1.03, 1.17, 1.36, 1.42 };

            FV_X = 40;
            FV_Y = 400;
            Len1 = 0;
            Len2 = 6;
            Len3 = 0;
            V1_inlet = 1.21;
            V2_inlet = 0.0;
            V3_inlet = 0.0;

            if (indexExp > -1 && indexExp < AA.Length)
            {
                double L23 = 6.0;
                //// Тест 1 эксперимент 1R8  1 минута
                FV_X = 60;
                FV_Y = 600;
                Len1 = 0.4;
                Len2 = LL[indexExp];
                Len3 = L23 - LL[indexExp];
                bottomWaveAmplitude = -AA[indexExp];
                Wen1 = 0.015;
                Wen2 = 0.1;
                Wen3 = 0.063 - Wen1;
            }
        }

        /// <summary>
        /// Взвешенная струя
        /// </summary>
        protected void TestCaseOffsetStreamJet()
        {
            double U0 = 1.21;
            double H = 0.163;
            double b = 0.015;
            double L23 = 3.0;
            Wen1 = 2*b;
            Wen2 = b;
            Wen3 = H - Wen1 - Wen2;
            int indexExp = TaskIndex;
            double q = 0.0224 / 0.4;
            // длины волн
            double[] LL = { 0.4, 0.6, 0.8, 1.0 };
            double[] AA = { q* LL[0], q * LL[1], q * LL[2], q * LL[3] };
            Len1 = 0;
            Len2 = 2;
            Len3 = 0;
            V1_inlet = 0;
            V2_inlet = U0;
            V3_inlet = 0.0;

            if (indexExp > -1 && indexExp < AA.Length)
            {
                //// Тест 1 эксперимент 1R8  1 минута
                FV_X = 50;
                FV_Y = 500;
                Len1 = 5*b;
                Len2 = LL[indexExp];
                Len3 = L23 - LL[indexExp];
                bottomWaveAmplitude = -AA[indexExp];
            }
        }

        /// <summary>
        /// Взвешенная струя - ровное дно
        /// </summary>
        protected void TestCaseOffsetStreamJet0()
        {
            double db = 0.5;
            if (typeStreamTask == TypeStreamTask.OffsetStreamJet0_1h)
                db = 1;
            else if (typeStreamTask == TypeStreamTask.OffsetStreamJet0_1_5h)
                db = 1.5;
            else if (typeStreamTask == TypeStreamTask.OffsetStreamJet0_2h)
                db = 2;
            else if (typeStreamTask == TypeStreamTask.OffsetStreamJet0_2_5h)
                db = 2.5;
            double U0 = 0.6;
            double H = 0.163;
            double b = 0.015;
            Wen1 = db * b;
            Wen2 = b;
            Wen3 = H - Wen1 - Wen2;
            // длины волн
            //FV_X = 60;
            //FV_Y = 600;
            FV_X = 90;
            FV_Y = 900;
            Len1 = b;
            Len2 = 3 - Len1;
            Len3 = 0;
            bottomWaveAmplitude = 0;
            V1_inlet = 0;
            V2_inlet = U0;
            V3_inlet = 0.0;
        }

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="ps"></param>
        public PatankarParams1XD(PatankarParams1XD ps)
        {
            Set(ps);
        }

        protected void InitTestCase()
        {
            switch (typeStreamTask)
            {
                case TypeStreamTask.StreamFromShield:
                    TestCaseStreamFromShield();
                    break;
                case TypeStreamTask.OffsetStreamJet:
                    TestCaseOffsetStreamJet();
                    break;
                case TypeStreamTask.OffsetStreamJet0_1h:
                case TypeStreamTask.OffsetStreamJet0_1_5h:
                case TypeStreamTask.OffsetStreamJet0_2h:
                case TypeStreamTask.OffsetStreamJet0_2_5h:
                    TestCaseOffsetStreamJet0();
                    break;
            }
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PatankarParams1XD()
        {
            TaskIndex = 0;
            //typeBedForm = TypeBedForm.PlaneForm;
            typeBedForm = TypeBedForm.PlaneForm;
            typeStreamTask = TypeStreamTask.OffsetStreamJet0_1h;
            CountBoundaryMove = 5;
            wavePeriod = 1;

            InitTestCase();
            bedLoadStart_X0 = false;
            bedLoadTauPlus = true;
            streamInsBoundary = false;
            velocityInsBoundary = false;
            //---------------------------
            t1 = 0;
            t2 = 0;
            t3 = 0;
            
            flatTermoTask = false;
            TemperOrConcentration = true;
            if(TemperOrConcentration==true)
                flatTermoTask = false;
            
            NonLinearIterations = 15;
            bcIndex = RoofCondition.adhesion;
            typeMAlgebra = TypeMAlgebra.TriDiagMat_Algorithm;

            topBottom = true;
            leftRight = false;
            localFilterBLMesh = true;
            AllBedForce = true;
            MaxCoordIters = 4*FV_X * FV_Y;

            if (MEM.Equals( Len1, 0) == true || 
                MEM.Equals( Len2, 0) == true ||
                MEM.Equals( Len2, 0) == true)
                AllBedForce = false;
        }
        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="ps"></param>
        public void Set(PatankarParams1XD ps)
        {
            TaskIndex = ps.TaskIndex;
            FV_X = ps.FV_X;
            FV_Y = ps.FV_Y;
            MaxCoordIters = ps.MaxCoordIters;
            typeBedForm = ps.typeBedForm;
            CountBoundaryMove = ps.CountBoundaryMove;

            Len1 = ps.Len1;
            Len2 = ps.Len2;
            Len3 = ps.Len3;

            Wen1 = ps.Wen1;
            Wen2 = ps.Wen2;
            Wen3 = ps.Wen3;
            bottomWaveAmplitude = ps.bottomWaveAmplitude;
            wavePeriod = ps.wavePeriod;

            t1 = ps.t1;
            t2 = ps.t2;
            t3 = ps.t3;
            V1_inlet = ps.V1_inlet;
            V2_inlet = ps.V2_inlet;
            V3_inlet = ps.V3_inlet;
            flatTermoTask = ps.flatTermoTask;
            bcIndex = ps.bcIndex;
            bedLoadStart_X0 = ps.bedLoadStart_X0;
            bedLoadTauPlus = ps.bedLoadTauPlus;

            NonLinearIterations = ps.NonLinearIterations;
            typeMAlgebra = ps.typeMAlgebra;
            typeStreamTask = ps.typeStreamTask;

            topBottom = ps.topBottom;
            leftRight = ps.leftRight;
            localFilterBLMesh = ps.localFilterBLMesh;
            AllBedForce = ps.AllBedForce;

            TemperOrConcentration = ps.TemperOrConcentration;
            if (TemperOrConcentration == true)
                flatTermoTask = false;

            streamInsBoundary = ps.streamInsBoundary;
            velocityInsBoundary = ps.velocityInsBoundary;
            InitTestCase();
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            FV_X = LOG.GetInt(file.ReadLine());
            FV_Y = LOG.GetInt(file.ReadLine());
            MaxCoordIters = LOG.GetInt(file.ReadLine());
            Len1 = LOG.GetDouble(file.ReadLine());
            Len2 = LOG.GetDouble(file.ReadLine());
            Len3 = LOG.GetDouble(file.ReadLine());
            Wen1 = LOG.GetDouble(file.ReadLine());
            Wen2 = LOG.GetDouble(file.ReadLine());
            Wen3 = LOG.GetDouble(file.ReadLine());
            V1_inlet = LOG.GetDouble(file.ReadLine());
            V2_inlet = LOG.GetDouble(file.ReadLine());
            V3_inlet = LOG.GetDouble(file.ReadLine());
            t1 = LOG.GetDouble(file.ReadLine());
            t2 = LOG.GetDouble(file.ReadLine());
            t3 = LOG.GetDouble(file.ReadLine());
            flatTermoTask = LOG.GetBool(file.ReadLine());
            TemperOrConcentration = LOG.GetBool(file.ReadLine());
            if (TemperOrConcentration == true)
                flatTermoTask = false;

            typeBedForm = (TypeBedForm) LOG.GetInt(file.ReadLine());
            CountBoundaryMove = LOG.GetInt(file.ReadLine());
            bottomWaveAmplitude = LOG.GetDouble(file.ReadLine());
            wavePeriod = LOG.GetInt(file.ReadLine());
            NonLinearIterations = LOG.GetInt(file.ReadLine());
            typeStreamTask = (TypeStreamTask)LOG.GetInt(file.ReadLine());
            bcIndex = (RoofCondition)LOG.GetInt(file.ReadLine());
            typeMAlgebra = (TypeMAlgebra)LOG.GetInt(file.ReadLine());
            topBottom = LOG.GetBool(file.ReadLine());
            leftRight = LOG.GetBool(file.ReadLine());
            localFilterBLMesh = LOG.GetBool(file.ReadLine());
            AllBedForce = LOG.GetBool(file.ReadLine());
            bedLoadStart_X0 = LOG.GetBool(file.ReadLine());
            bedLoadTauPlus = LOG.GetBool(file.ReadLine());
            streamInsBoundary = LOG.GetBool(file.ReadLine());
            velocityInsBoundary = LOG.GetBool(file.ReadLine());
        }
        public virtual void Save(StreamReader file)
        {
        }
        public PatankarParams1XD Clone(PatankarParams1XD p)
        {
            return new PatankarParams1XD(p);
        }
    }
}
