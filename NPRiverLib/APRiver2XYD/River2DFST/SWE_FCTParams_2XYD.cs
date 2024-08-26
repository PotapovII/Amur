//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 15.10.2022 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver2XYD.River2DFST
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using CommonLib;
    using CommonLib.Physics;
    /// <summary>
    /// Перечисление тестовых задач
    /// </summary>
    [Serializable]
    public enum FCTTaskIndex
    {
        /// <summary>
        /// растекание водного столба
        /// </summary>
        CylinderDecay = 0,
        /// <summary>
        /// поток вдоль X по трапециевидной форме дна
        /// </summary>
        Trapeziform_X,
        /// <summary>
        /// поток вдоль Y по трапециевидной форме дна
        /// </summary>
        Trapeziform_Y,
        /// <summary>
        /// потока вдоль X
        /// </summary>
        ConditionX,
        /// <summary>
        /// потока вдоль У
        /// </summary>
        ConditionY,
        /// <summary>
        /// потока вдоль X по наклонному дну параболической формы 
        /// </summary>
        ParabGradient,
        /// <summary>
        /// потока вдоль X по наклонному дну параболической формы берегами
        /// </summary>
        ParabGradient_SW,
        /// <summary>
        ///  поток вдоль X по руслу с параболической формой дна и суши
        /// </summary>
        Parab_SW,
        /// <summary>
        /// поток вдоль X по руслу с параболической формой дна
        /// </summary>
        Parabolic,
        /// <summary>
        /// поток по X в V-образной форме дна
        /// </summary>
        Vform_X,
        /// <summary>
        /// задачи прорыва плотины при потоке вдоль X
        /// </summary>
        Dike_Х
    }
    /// <summary>
    ///  ОО: Параметры для класса RiverSWE_FCT_2XYD
    /// </summary>
    [Serializable]
    public class SWE_FCTParams_2XYD : ITProperty<SWE_FCTParams_2XYD>
    {
        // ========================== Алгоритм  ========================== 
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
        /// Минимальная глубина для расчета расхода грунтовых вод
        /// </summary>
        [DisplayName("Минимальная глубина для расчета расхода грунтовых вод")]
        [Category("Алгоритм")]
        public double H_minGroundWater { get; set; }
        /// <summary>
        /// Число Куранта
        /// </summary>
        [DisplayName("Число Куранта")]
        [Category("Алгоритм")]
        public double CourantNumber { get; set; }

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
        /// Средний диаметр донных частиц
        /// </summary>
        [DisplayName("Средний диаметр донных частиц")]
        [Category("Физичесие параметры")]
        public double d50 { get; set; }
        /// <summary>
        /// Коэффициент шероховатости дна канала
        /// </summary>
        [DisplayName("Коэффициент шероховатости дна канала")]
        [Category("Физичесие параметры")]
        public double roughness { get; set; }
        /// <summary>
        /// Длина области
        /// </summary>
        [DisplayName("Длина области по Х")]
        [Category("Геометрия области")]
        public double Lx { get; set; }
        /// <summary>
        /// Длина области
        /// </summary>
        [DisplayName("Длина области по У")]
        [Category("Геометрия области")]
        public double Ly { get; set; }
        /// <summary>
        /// Длина области
        /// </summary>
        [DisplayName("Узлов по Х")]
        [Category("Геометрия области")]
        public int Nx { get; set; }
        /// <summary>
        /// Длина области
        /// </summary>
        [DisplayName("Узлов по У")]
        [Category("Геометрия области")]
        public int Ny { get; set; }
        /// <summary>
        /// Граничные условия
        /// </summary>
        [DisplayName("Граничные условия")]
        [Category("Граничные условия")]
        public FCTTaskIndex TaskIndex { get; set; }
        public SWE_FCTParams_2XYD()
        {
            //TaskIndex = FCTTaskIndex.CylinderDecay;
            TaskIndex = FCTTaskIndex.CylinderDecay;
            CourantNumber = 0.8;
            d50 = 0.001;
            roughness = 0.15f / (float)(Math.Sqrt(SPhysics.GRAV) * Math.Pow(d50, 1.0 / 6));
            Nx = 201;
            Ny = 201;
            Lx = 100;
            Ly = 100;
            time = 0;
            dtime = 100;
            dtmax = 100;
            rho_w = 1000;
            filtrСoeff = 0.1;
            H_minGroundWater = 0.01;
            maxSolСorrection = 0.05;
            turbulentVisCoeff = 0.5;
        }
        public SWE_FCTParams_2XYD(SWE_FCTParams_2XYD p)
        {
            if (p != null)
                Set(p);
            else
                Set(new SWE_FCTParams_2XYD());
        }
        public void Set(SWE_FCTParams_2XYD p)
        {
            time = p.time;
            dtime = p.dtime;
            dtmax = p.dtmax;
            d50 = p.d50;
            Lx = p.Lx;
            Ly = p.Ly;
            Nx = p.Nx;
            Ny = p.Ny;
            rho_w = p.rho_w;
            filtrСoeff = p.filtrСoeff;
            TaskIndex = p.TaskIndex;
            CourantNumber = p.CourantNumber;
            maxSolСorrection = p.maxSolСorrection;
            roughness = p.roughness;
            H_minGroundWater = p.H_minGroundWater;
            turbulentVisCoeff = p.turbulentVisCoeff;
        }
        /// <summary>
        /// свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public object GetParams() { return this; }
        /// <summary>
        /// Установка свойств задачи
        /// </summary>
        /// <param name="p"></param>
        public void SetParams(object p)
        {
            SetParams((SWE_FCTParams_2XYD)p);
        }
        //public void LoadParams(string fileName = "")
        //{
        //    //if (fileName == "")
        //    //    fileName = "NameRSParams.txt";
        //    //string message = "Файл парамеров задачи " + fileName + " не обнаружен";
        //    //LOG.LoadParams(Load, message, fileName);
        //}
        public virtual void Load(StreamReader file)
        {
        }
        public virtual void Save(StreamReader file)
        {
        }
        public SWE_FCTParams_2XYD Clone(SWE_FCTParams_2XYD p)
        {
            return new SWE_FCTParams_2XYD(p);
        }
    }
}
