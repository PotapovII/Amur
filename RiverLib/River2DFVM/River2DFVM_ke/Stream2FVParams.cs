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
namespace RiverLib.Patankar
{
    using System;
    using System.ComponentModel;

    using System.IO;
    using MemLogLib;
    using CommonLib.EConverter;

    /// <summary>
    /// ОО: Физические параметры модели к-e
    /// </summary>
    [Serializable]
    public class Stream2FVParams
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
        public TauBondaryCondition bcIndex { get; set; }

        [DisplayName("метод СЛАУ")]
        [Description("Тип метода для решения СЛАУ")]
        [Category("Задача")]
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
        /// конструктор
        /// </summary>
        /// <param name="ps"></param>
        public Stream2FVParams(Stream2FVParams ps)
        {
            SetParams(ps);
        }
        /// <summary>
        /// получение ссылки на объект
        /// </summary>
        /// <returns></returns>
        public object GetParams()
        {
            return this;
        }
        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="ps"></param>
        public void SetParams(Stream2FVParams ps)
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

            NonLinearIterations = ps.NonLinearIterations;
            typeMAlgebra = ps.typeMAlgebra;

            topBottom = ps.topBottom;
            leftRight = ps.leftRight;
            localFilterBLMesh = ps.localFilterBLMesh;
            AllBedForce = ps.AllBedForce;

            TemperOrConcentration = ps.TemperOrConcentration;
            if (TemperOrConcentration == true)
                flatTermoTask = false;


            TestCase();
        }

        protected void TestCase()
        {
            int indexExp = TaskIndex;
            double[] AA = { 0.0224, 0.0496, 0.056, 0.064, 0.076, 0.08 };
            // длины волн
            double[] LL = { 0.4, 0.9, 1.03, 1.17, 1.36, 1.42 };

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
        /// Конструктор
        /// </summary>
        public Stream2FVParams()
        {
            TaskIndex = 0;
            typeBedForm = TypeBedForm.PlaneForm;
            CountBoundaryMove = 5;
            wavePeriod = 1;
            //---------------------------
            // Тест 1
            //FV_X = 60;
            //FV_Y = 500;
            //Len1 = 1;
            //Len2 = 9;
            //Len3 = 0;
            //Wen1 = 0.05;
            //Wen2 = 0.05;
            //Wen3 = 0.10;
            //V1_inlet = 0.5;
            //V2_inlet = 0.5;
            //V3_inlet = 0.5;

            //// Тест 2 эксперимент
            //FV_X = 50;
            //FV_Y = 500;
            //Len1 = 0;
            //Len2 = 8;
            //Len3 = 0;
            //Wen1 = 0.007;
            //Wen2 = 0.030;
            //Wen3 = 0.126;
            //V1_inlet = 0.0;
            //V2_inlet = 1.1;
            //V3_inlet = 0.0;


            FV_X = 80;
            FV_Y = 1200;

            FV_X = 40;
            FV_Y = 400;

            Len1 = 0;
            Len2 = 6;
            Len3 = 0;

            TestCase();

            V1_inlet = 1.21;
            V2_inlet = 0.0;
            V3_inlet = 0.0;
            typeBedForm = TypeBedForm.L1_L2sin_L3;

            //---------------------------
            t1 = 0;
            t2 = 0;
            t3 = 0;
            
            flatTermoTask = false;
            TemperOrConcentration = true;
            if(TemperOrConcentration==true)
                flatTermoTask = false;
            
            NonLinearIterations = 15;
            bcIndex = TauBondaryCondition.adhesion;
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
        public virtual void LoadParams(string fileName = "")
        {
            string message = "Файл парамеров задачи River2DFV - не обнаружен";
            WR.LoadParams(Load, message, fileName);
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
            bcIndex = (TauBondaryCondition)LOG.GetInt(file.ReadLine());
            typeMAlgebra = (TypeMAlgebra)LOG.GetInt(file.ReadLine());
            topBottom = LOG.GetBool(file.ReadLine());
            leftRight = LOG.GetBool(file.ReadLine());
            localFilterBLMesh = LOG.GetBool(file.ReadLine());
            AllBedForce = LOG.GetBool(file.ReadLine());
        }
    }
}
