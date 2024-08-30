//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//                  создано  :   9.03.2007 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 06.12.2020 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver_1YD.Params
{
    using System;
    [Serializable]
    public class TaskEvolution
    {
        /// <summary>
        ///  время
        /// </summary>
        public double time;
        /// <summary>
        /// вязкость
        /// </summary>
        public double eddyViscosityConst;
        /// <summary>
        /// Сдвиговые напряжения максимум
        /// </summary>
        public double tauMax;
        /// <summary>
        /// Сдвиговые напряжения средние
        /// </summary>
        public double tauMid;
        /// <summary>
        /// уровень свободной поверхности
        /// </summary>
        public double waterLevel;
        /// <summary>
        /// гидравлический радиус
        /// </summary>
        public double WetBed;
        /// <summary>
        /// площадь сечения
        /// </summary>
        public double Area;
        /// <summary>
        /// текущий расход потока
        /// </summary>
        public double riverFlowRate;
        /// <summary>
        /// текущий расчетный расход потока 
        /// </summary>
        public double riverFlowRateCalk;
        public TaskEvolution(double time, double eddyViscosityConst, double waterLevel,
                             double tauMax, double tauMid,
                             double WetBed, double Area, double riverFlowRate,
                             double riverFlowRateCalk)
        {
            this.time = time;
            this.eddyViscosityConst = eddyViscosityConst;
            this.tauMid = tauMid;
            this.tauMax = tauMax;
            this.waterLevel = waterLevel;
            this.WetBed = WetBed;
            this.Area = Area;
            this.riverFlowRate = riverFlowRate;
            this.riverFlowRateCalk = riverFlowRateCalk;
        }
        public TaskEvolution(TaskEvolution p)
        {
            this.time = p.time;
            this.eddyViscosityConst = p.eddyViscosityConst;
            this.tauMid = p.tauMid;
            this.tauMax = p.tauMax;
            this.waterLevel = p.waterLevel;
            this.WetBed = p.WetBed;
            this.Area = p.Area;
            this.riverFlowRate = p.riverFlowRate;
            this.riverFlowRateCalk = p.riverFlowRateCalk;
        }
    }

    [Serializable]
    public class ExTaskEvolution : TaskEvolution
    {
        /// <summary>
        /// Отметки да в эксперименте 1
        /// </summary>
        public double riverBed1;
        /// <summary>
        /// Отметки да в эксперименте 2
        /// </summary>
        public double riverBed2;

        public ExTaskEvolution(double time, double eddyViscosityConst, double waterLevel,
                             double tauMax, double tauMid,
                             double WetBed, double Area, double riverFlowRate,
                             double riverFlowRateCalk, double riverBed1, double riverBed2) :
            base(time, eddyViscosityConst, waterLevel, tauMax, tauMid, WetBed, Area, riverFlowRate, riverFlowRateCalk)
        {
            this.riverBed1 = riverBed1;
            this.riverBed2 = riverBed2;
        }
        public ExTaskEvolution(ExTaskEvolution p) : base(p)
        {
            this.riverBed1 = p.riverBed1;
            this.riverBed2 = p.riverBed2;
        }
    }
}