//---------------------------------------------------------------------------
//   Класс FEMParams1XD предназначен для определения параметров задачи 
//                              Потапов И.И.
//                        - (C) Copyright 2025 -
//                          ALL RIGHT RESERVED
//                               07.03.25
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1XD
{
    using System;
    using System.IO;
    using System.ComponentModel;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Tasks;
    using CommonLib.BedLoad;
    using CommonLib.EConverter;
    using CommonLib.EddyViscosity;

    /// <summary>
    /// Тип решателя
    /// </summary>
    [Serializable]
    public enum TypeAlgebra
    {
        /// <summary>
        /// решатель  САУ ленточным методом LU разложения 
        /// </summary>
        [Description("решатель САУ методом Ленточный LU")]
        LUTape,
        /// <summary>
        /// решатель САУ методом GMRES
        /// </summary>
        [Description("решатель САУ методом GMRES")]
        GMRES_P_Sparce,
        /// <summary>
        /// решатель САУ методом би сопряженных градиентов
        /// </summary>
        [Description("решатель САУ методом би сопряженных градиентов")]
        BeCGM_Sparce
    }
    /// <summary>
    /// ОО: Физические параметры модели к-e
    /// </summary>
    [Serializable]
    public class FEMParams_1XD : ITProperty<FEMParams_1XD>
    {
        public int nfmax = 3;
        ///// <summary>
        /// Размерность модели  
        /// колиество переменных задачи: _time, v, pc, t, tke, dis
        /// </summary>
        [DisplayName("Колиество переменных")]
        [Description("Колиество переменных задачи")]
        [Category("Задача")]
        public int CountUnknow => nfmax;
        /// <summary>
        /// Количество КЭ для давления по Х
        /// </summary>
        [DisplayName("Количество КЭ по Х")]
        [Description("Количество КЭ по Х")]
        [Category("Сетка")]
        public int FE_X { get; set; }
        /// <summary>
        /// Количество КЭ для давления по Х
        /// </summary>
        [DisplayName("Количество КЭ по Y")]
        [Description("Количество КЭ по Y")]
        [Category("Сетка")]
        public int FE_Y { get; set; }


        ///// <summary>
        ///// Тип формы дна
        ///// </summary>
        //[DisplayName("Начальной формы дна")]
        //[Description("Начальной формы донной поверхности")]
        //[Category("Начальная форма дна")]
        //[TypeConverter(typeof(MyEnumConverter))]
        //public TypeBedForm typeBedForm { get; set; }
        ///// <summary>
        ///// Амплитуда донной поверхности
        ///// </summary>
        //[DisplayName("Амплитуда дна")]
        //[Description("Амплитуда донных волн на 2 участке (м)")]
        //[Category("Начальная форма дна")]
        //public double bottomWaveAmplitude { get; set; }
        ///// <summary>
        ///// Количество донных волн
        ///// </summary>
        //[DisplayName("Количество волн")]
        //[Description("Количество донных волн на 2 участке")]
        //[Category("Начальная форма дна")]
        //public int wavePeriod { get; set; }
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
        [DisplayName("Длина водотока")]
        [Category("Геометрия области")]
        public double Lx { get { return Wen1 + Wen2 + Wen3; } }
        /// <summary>
        /// Высота расчетной области -> по Y
        /// </summary>
        [DisplayName("Глубина водотока")]
        [Category("Геометрия области")]
        public double Ly { get {  return Len1 + Len2 + Len3; } }
        /// <summary>
        /// Длина водотока на 1 участке (вход потока)
        /// </summary>
        [DisplayName("Глубина  1 участка")]
        [Description("Глубина  водотока на 1 участке")]
        [Category("Геометрия области")]
        public double Len1 { get; set; }
        /// <summary>
        /// Длина водотока на 3 участке (центр)
        /// </summary>
        [DisplayName("Глубина  2 участка")]
        [Description("Глубина  водотока на 3 участке (центр)")]
        [Category("Геометрия области")]
        public double Len2 { get; set; }
        /// <summary>
        /// Длина водотока на 3 участке (истечение)
        /// </summary>
        [DisplayName("Глубина  3 участка")]
        [Description("Глубина  водотока на 3 участке (истечение)")]
        [Category("Геометрия области")]
        public double Len3 { get; set; }
        /// <summary>
        /// Глубина водотока 1 придонный участок
        /// </summary>
        [DisplayName("Длина  1 участка")]
        [Description("Длина  водотока 1 придонный участок")]
        [Category("Геометрия области")]
        public double Wen1 { get; set; }
        /// <summary>
        /// Глубина 2 участка
        /// </summary>
        [DisplayName("Длина  2 участка")]
        [Description("Длина водотока 2 участок")]
        [Category("Геометрия области")]
        public double Wen2 { get; set; }
        /// <summary>
        /// Глубина 3 участка
        /// </summary>
        [DisplayName("Длина  3 участка")]
        [Description("Длина  водотока 3 приповерхностный участок")]
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
        /// Температура в 1 слое
        /// </summary>
        [DisplayName("Концентрация 1 слой")]
        [Description("Концентрация в 1 слое (С)")]
        [Category("Граничные условия притока")]
        public double t1 { get; set; }
        /// <summary>
        /// Температура в 2 слое
        /// </summary>
        [DisplayName("Концентрация 2 слой")]
        [Description("Концентрация в 2 слое (С)")]
        [Category("Граничные условия притока")]
        public double t2 { get; set; }
        /// <summary>
        /// Температура в 3 слое
        /// </summary>
        [DisplayName("Концентрация 3 слой")]
        [Description("Концентрация в 3 слое (С)")]
        [Category("Граничные условия притока")]
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
        public TauBondaryCondition bcTypeOnWL { get; set; }
        [DisplayName("Граничыне условия на выходе из канала")]
        [Description("Граничыне условия на выходе из канала")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public TypeBoundCond outBC { get; set; }
        ///// <summary>
        ///// типы задачи по входной струе
        ///// </summary>
        //[DisplayName("Тип струи на входе")]
        //[Description("Тип струи на входе по умолчанию")]
        //[Category("Задача")]
        //[TypeConverter(typeof(MyEnumConverter))]
        //public TypeStreamTask typeStreamTask { get; set; }

        [DisplayName("Тип задачи")]
        [Description("Определение типа входной струи")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))] 
        public TypeAlgebra typeAlgebra { get; set; }
        /// <summary>
        /// Растояние струи от стенки
        /// </summary>
        [DisplayName("Смешение струи от стенки")]
        [Description("Растояние струи от стенки")]
        [Category("Опции")]
        public double LV { get; set; }
        /// <summary>
        /// Смешение струи от стенки
        /// </summary>
        [TypeConverter(typeof(BooleanTypeConverterYN))]
        [DisplayName("Смешение струи от стенки")]
        [Description("Смешение струи от стенки")]
        [Category("Опции")]
        public bool shiftV { get; set; }

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
        /// Тип гидродинамики 0 - нестационарные 1 - стационарные у-я Рейнольдса 2 - Стокс
        /// нестационар/стационар
        /// </summary>
        [DisplayName("Тип гидродинамики 0 - нестационарные 1 - стационарные у-я Рейнольдса 2 - Стокс")]
        [TypeConverter(typeof(MyEnumConverter))]
        [Category("Задача")]
        public int ReTask { get; set; }
        /// <summary>
        /// Количество итераций по нелинейности на текущем шаге по времени
        /// </summary>
        [DisplayName("Количество итераций по нелинейности на текущем шаге по времени")]
        [Category("Алгоритм")]
        public int NLine { get; set; }
        /// <summary>
        /// Неявности схемы при шаге по времени
        /// </summary>
        [DisplayName("Параметр неявности схемы при шаге по времени")]
        [Category("Задача")]
        public double theta { get; set; }
        /// <summary>
        /// Постоянная вихревая вязкость
        /// </summary>
        [DisplayName("Постоянная вихревая вязкость")]
        [Category("Задача")]
        public double mu_const { get; set; }
        /// <summary>
        /// модель турбулентной вязкости
        /// </summary>
        [DisplayName("Модель турбулентной вязкости")]
        [Category("Задача")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ETurbViscType turbViscType { get; set; }

        #region + 01 05 2025 ОО: Флаг для расчета задачи взвешенных наносов
        [TypeConverter(typeof(MyEnumConverter))]
        [DisplayName("Расчет взвешенных наносов")]
        [Description("Флаг для расчета взвешенных наносов Да - true, Нет -false")]
        [Category("Задача")]
        public BCalkConcentration CalkConcentration { get; set; } = BCalkConcentration.NeCalkConcentration;
        #endregion


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
            Set((FEMParams_1XD)p);
        }
            /// <summary>
            /// конструктор
            /// </summary>
            /// <param name="ps"></param>
        public FEMParams_1XD(FEMParams_1XD ps)
        {
            Set(ps);
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public FEMParams_1XD()
        {
            LV = 0.2;
            shiftV = true;

            TaskIndex = 0;
            outBC = TypeBoundCond.Dirichlet0;
            ////typeBedForm = TypeBedForm.PlaneForm;
            //typeBedForm = TypeBedForm.PlaneForm;
            ////typeStreamTask = TypeStreamTask.OffsetStreamJet0_1h;
            ////typeStreamTask = TypeStreamTask.OffsetStreamJet0_2h;

            //// R. Karki 2007
            ////typeStreamTask = TypeStreamTask.OffsetStreamJet0_2h;
            //typeStreamTask = TypeStreamTask.OffsetStreamJet0_1h;

            CountBoundaryMove = 5;
        //    wavePeriod = 1;

            bedLoadStart_X0 = false;
            bedLoadTauPlus = true;
            streamInsBoundary = false;
            velocityInsBoundary = false;
            //---------------------------
            t1 = 0;
            t2 = 0;
            t3 = 0;
            
           
            NonLinearIterations = 15;
            bcTypeOnWL = TauBondaryCondition.adhesion;
            typeAlgebra = TypeAlgebra.LUTape;

            topBottom = true;
            leftRight = false;
            localFilterBLMesh = true;
            AllBedForce = true;
            if (MEM.Equals( Len1, 0) == true || 
                MEM.Equals( Len2, 0) == true ||
                MEM.Equals( Len2, 0) == true)
                AllBedForce = false;

            ReTask = 0;
            NLine = 100;
            theta = 0.5;
            mu_const = 1;
            turbViscType = ETurbViscType.Boussinesq1865;
            // + 01 05 2025
            CalkConcentration = BCalkConcentration.NotCalkConcentration;
        }
        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="ps"></param>
        public void Set(FEMParams_1XD ps)
        {
            TaskIndex = ps.TaskIndex;
            FE_X = ps.FE_X;
            FE_Y = ps.FE_Y;

            // typeBedForm = ps.typeBedForm;
            outBC=ps.outBC;
            CountBoundaryMove = ps.CountBoundaryMove;

            LV = ps.LV;
            shiftV = ps.shiftV;

            Len1 = ps.Len1;
            Len2 = ps.Len2;
            Len3 = ps.Len3;

            Wen1 = ps.Wen1;
            Wen2 = ps.Wen2;
            Wen3 = ps.Wen3;
            //bottomWaveAmplitude = ps.bottomWaveAmplitude;
            //wavePeriod = ps.wavePeriod;

            t1 = ps.t1;
            t2 = ps.t2;
            t3 = ps.t3;
            V1_inlet = ps.V1_inlet;
            V2_inlet = ps.V2_inlet;
            V3_inlet = ps.V3_inlet;

            bcTypeOnWL = ps.bcTypeOnWL;
            bedLoadStart_X0 = ps.bedLoadStart_X0;
            bedLoadTauPlus = ps.bedLoadTauPlus;

            NonLinearIterations = ps.NonLinearIterations;
            typeAlgebra = ps.typeAlgebra;
          //  typeStreamTask = ps.typeStreamTask;

            topBottom = ps.topBottom;
            leftRight = ps.leftRight;
            localFilterBLMesh = ps.localFilterBLMesh;
            AllBedForce = ps.AllBedForce;

            TemperOrConcentration = ps.TemperOrConcentration;

            streamInsBoundary = ps.streamInsBoundary;
            velocityInsBoundary = ps.velocityInsBoundary;


            ReTask = ps.ReTask;
            NLine = ps.NLine;
            theta = ps.theta;
            mu_const = ps.mu_const;
            turbViscType = ps.turbViscType;
            // + 01 05 2025
            CalkConcentration = ps.CalkConcentration;
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="file"></param>
        public virtual void Load(StreamReader file)
        {
            FE_X = LOG.GetInt(file.ReadLine());
            FE_Y = LOG.GetInt(file.ReadLine());
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

            TemperOrConcentration = LOG.GetBool(file.ReadLine());

            
            outBC = (TypeBoundCond)LOG.GetInt(file.ReadLine());
            CountBoundaryMove = LOG.GetInt(file.ReadLine());
            NonLinearIterations = LOG.GetInt(file.ReadLine());
            bcTypeOnWL = (TauBondaryCondition)LOG.GetInt(file.ReadLine());
            typeAlgebra = (TypeAlgebra)LOG.GetInt(file.ReadLine());
            topBottom = LOG.GetBool(file.ReadLine());
            leftRight = LOG.GetBool(file.ReadLine());
            localFilterBLMesh = LOG.GetBool(file.ReadLine());
            AllBedForce = LOG.GetBool(file.ReadLine());
            bedLoadStart_X0 = LOG.GetBool(file.ReadLine());
            bedLoadTauPlus = LOG.GetBool(file.ReadLine());
            streamInsBoundary = LOG.GetBool(file.ReadLine());
            velocityInsBoundary = LOG.GetBool(file.ReadLine());
            shiftV = LOG.GetBool(file.ReadLine());
            LV = LOG.GetDouble(file.ReadLine());

            ReTask = LOG.GetInt(file.ReadLine());
            NLine = LOG.GetInt(file.ReadLine());
            theta = LOG.GetDouble(file.ReadLine());
            mu_const = LOG.GetDouble(file.ReadLine());
            turbViscType = (ETurbViscType)LOG.GetInt(file.ReadLine());
            // + 01 05 2025
            CalkConcentration = (BCalkConcentration)LOG.GetInt(file.ReadLine());

        }
        public virtual void Save(StreamReader file)
        {
        }
        public FEMParams_1XD Clone(FEMParams_1XD p)
        {
            return new FEMParams_1XD(p);
        }
    }
}
