//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD.River2DSW
{
    using CommonLib;
    using CommonLib.EConverter;
    using System;
    using System.ComponentModel;
    using System.IO;

    [Serializable]
    public enum ECoriolis
    {
        /// <summary>
        /// Учет сил корриолиса
        /// </summary>
        [Description("Учет сил Кориолиса")]
        CoriolisOk = 0,
        /// <summary>
        /// Не учет сил корриолиса
        /// </summary>
        [Description("Не учет сил Кориолиса")]
        CoriolisNo
    }
    /// <summary>
    ///  ОО: Параметры для класса River2D 
    /// </summary>
    [Serializable]
    public class ParamsRiver_2XYD : ITProperty<ParamsRiver_2XYD>
    {
        // ========================== Алгоритм  ========================== 
        [DisplayName("Силы Кориолиса")]
        [Category("Алгоритм")]
        [TypeConverter(typeof(MyEnumConverter))]
        public ECoriolis Coriolis { get; set; }
        /// <summary>
        /// Максимальное изменение решения на итерации задачи
        /// </summary>
        [DisplayName("Максимальное изменение решения на итерации задачи")]
        [Category("Алгоритм")]
        public double maxSolСorrection { get; set; }
        /// <summary>
        /// Текущее время расчета
        /// </summary>
        [DisplayName("Текущее время расчета")]
        [Category("Алгоритм")]
        public double time { get; set; }
        /// <summary>
        /// Рекомендуемый шаг по времени
        /// </summary>
        [DisplayName("Рекомендуемый шаг по времени")]
        [Category("Алгоритм")]
        public double dtime { get; set; }
        /// <summary>
        /// Максимальный шаг по времени
        /// </summary>
        [DisplayName("Максимальный шаг по времени")]
        [Category("Алгоритм")]
        public double dtmax { get; set; }
        /// <summary>
        /// Фактор изменения шага по времени
        /// </summary>
        [DisplayName("Фактор изменения шага по времени")]
        [Category("Алгоритм")]
        public double dtTrend { get; set; }
        /// <summary>
        /// Theta - параметр шага по премени
        /// </summary>
        [DisplayName("Theta - параметр шага по премени")]
        [Category("Алгоритм")]
        public double theta { get; set; }
        /// <summary>
        /// Противопоточный коэффициент
        /// </summary>
        [DisplayName("Противопоточный коэффициент")]
        [Category("Алгоритм")]
        public double UpWindCoeff { get; set; }
        /// <summary>
        /// Минимальная глубина для расчета расхода грунтовых вод
        /// </summary>
        [DisplayName("Минимальная глубина для расчета расхода грунтовых вод")]
        [Category("Алгоритм")]
        public double H_minGroundWater { get; set; }


        // ========================== Физика ========================== 
        /// <summary>
        /// Плотность воды
        /// </summary>
        [DisplayName("Плотность воды")]
        [Category("Физичесие параметры")]
        public double rho_w { get; set; }
        /// <summary>
        /// Коэффициент турбуленоной вязкости 
        /// </summary>
        [DisplayName("Коэффициент турбуленоной вязкости ")]
        [Category("Физичесие параметры")]
        public double turbulentVisCoeff { get; set; }
        /// <summary>
        /// Коэффициент пропускания водоносного горизонта
        /// </summary>
        [DisplayName("Коэффициент пропускания водоносного горизонта")]
        [Category("Физичесие параметры")]
        public double filtrСoeff { get; set; }
        /// <summary>
        ///  Коэффициент водонасыщения
        /// </summary>
        [DisplayName("Коэффициент водонасыщения")]
        [Category("Физичесие параметры")]
        public double droundWaterCoeff { get; set; }
        /// <summary>
        /// Относительная плотность льда
        /// </summary>
        [DisplayName("Относительная плотность льда")]
        [Category("Физичесие параметры")]
        public double iceCoeff { get; set; }
        /// <summary>
        /// Широта в которой распалагается расчетная область (градусы)
        /// </summary>
        [DisplayName("Широта в которой распалагается расчетная область (градусы)")]
        [Category("Физичесие параметры")]
        public double latitudeArea { get; set; }
        public ParamsRiver_2XYD()
        {
            maxSolСorrection = 0.05;
            time = 0;
            dtime = 100;
            dtmax = 100;
            dtTrend = 1;
            theta = 1;
            UpWindCoeff = 0.5;
            H_minGroundWater = 0.01;
            rho_w = 1000;
            turbulentVisCoeff = 0.5;
            filtrСoeff = 0.1;
            droundWaterCoeff = 1;
            iceCoeff = 0.96;
            latitudeArea = 60;
            Coriolis = ECoriolis.CoriolisNo;
        }
        public ParamsRiver_2XYD(ParamsRiver_2XYD p)
        {
           Set(new ParamsRiver_2XYD());
        }
        public void Set(ParamsRiver_2XYD p)
        {
            maxSolСorrection = p.maxSolСorrection;
            time = p.time;
            dtime = p.dtime;
            dtmax = p.dtmax;
            dtTrend = p.dtTrend;
            theta = p.theta;
            UpWindCoeff = p.UpWindCoeff;
            H_minGroundWater = p.H_minGroundWater;
            rho_w = p.rho_w;
            turbulentVisCoeff = p.turbulentVisCoeff;
            filtrСoeff = p.filtrСoeff;
            droundWaterCoeff = p.droundWaterCoeff;
            iceCoeff = p.iceCoeff;
            latitudeArea = p.latitudeArea;
            Coriolis=p.Coriolis;
        }
        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public virtual object GetParams() { return this; }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            SetParams((ParamsRiver_2XYD)p);
        }
        public virtual void Load(StreamReader file)
        {
        }
        public virtual void Save(StreamReader file)
        {
        }
        public ParamsRiver_2XYD Clone(ParamsRiver_2XYD p)
        {
            return new ParamsRiver_2XYD(p);
        }
    }
}
