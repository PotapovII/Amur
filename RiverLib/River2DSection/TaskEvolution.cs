//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//                  создано  :   9.03.2007 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 06.12.2020 Потапов И.И. 
//---------------------------------------------------------------------------
namespace RiverLib
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
        public double Mu;
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
        public double GR;
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
        public TaskEvolution(double time, double Mu, double waterLevel,
                             double tauMax, double tauMid,
                             double GR, double Area, double riverFlowRate,
                             double riverFlowRateCalk)
        {
            this.time = time;
            this.Mu = Mu;
            this.tauMid = tauMid;
            this.tauMax = tauMax;
            this.waterLevel = waterLevel;
            this.GR = GR;
            this.Area = Area;
            this.riverFlowRate = riverFlowRate;
            this.riverFlowRateCalk = riverFlowRateCalk;
        }
        public TaskEvolution(TaskEvolution p)
        {
            this.time = p.time;
            this.Mu = p.Mu;
            this.tauMid = p.tauMid;
            this.tauMax = p.tauMax;
            this.waterLevel = p.waterLevel;
            this.GR = p.GR;
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

        public ExTaskEvolution(double time, double Mu, double waterLevel,
                             double tauMax, double tauMid,
                             double GR, double Area, double riverFlowRate,
                             double riverFlowRateCalk, double riverBed1, double riverBed2):
            base(time, Mu, waterLevel,tauMax, tauMid,GR, Area, riverFlowRate,riverFlowRateCalk)
        {
            this.riverBed1 = riverBed1;
            this.riverBed2 = riverBed2;
        }
        public ExTaskEvolution(ExTaskEvolution p):base(p)
        {
            this.riverBed1 = p.riverBed1;
            this.riverBed2 = p.riverBed2;
        }
    }
}