//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12.06.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib.River2D
{
    using System;
    using System.ComponentModel;
    /// <summary>
    ///  ОО: Параметры для класса River2D 
    /// </summary>
    [Serializable]
    public class ParamsRiver
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
        public ParamsRiver()
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
        }
        public ParamsRiver(ParamsRiver p)
        {
            if (p != null)
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
            }
            else
                SetParams(new ParamsRiver());
        }
        public void SetParams(ParamsRiver p)
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
            SetParams((ParamsRiver)p);
        }
        public void LoadParams(string fileName = "")
        {
            //if (fileName == "")
            //    fileName = "NameRSParams.txt";
            //string message = "Файл парамеров задачи - доные деформации - не обнаружен";
            //LOG.LoadParams(Load, message, fileName);
        }
    }
}
