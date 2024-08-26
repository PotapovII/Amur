//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 23.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    /// <summary>
    /// Отчетная информация о речном процессе - кадр
    /// </summary>
    [Serializable]
    public class ReportDataRiver
    {
        /// <summary>
        /// время замера
        /// </summary>
        public double time;
        /// <summary>
        /// кинетическая энергия потока
        /// </summary>
        public double Ekin;
        /// <summary>
        /// потонциальная энергия потока
        /// </summary>
        public double Epot;
        /// <summary>
        /// полная энергия потока
        /// </summary>
        public double EnergyFlow;
        /// <summary>
        /// площадь живого сечения
        /// </summary>
        public double area;
        /// <summary>
        /// ширина живого сечения по поверхности
        /// </summary>
        public double Width;
        /// <summary>
        /// глубина живого сечения
        /// </summary>
        public double Depth;
        /// <summary>
        /// средняя скорость
        /// </summary>
        public double midleVelocity;
        /// <summary>
        /// массовый расход
        /// </summary>
        public double riverRate;

        public ReportDataRiver() { }
    }
}
