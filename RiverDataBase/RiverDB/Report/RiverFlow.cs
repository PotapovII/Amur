namespace RiverDB.Report
{
    /// <summary>
    /// Расход речного потока
    /// Кривые связи между отметкой гидропоста в см и расходом реки в м^3/с 
    /// </summary>
    public interface IRiverFlow
    {
        /// <summary>
        /// Название реки/гидропоста
        /// </summary>
        string RiverName { get; }
        /// <summary>
        /// Расход речного потока
        /// </summary>
        /// <param name="H">уровень воды по гидропосту в (см)</param>
        /// <returns>расход речного потока в м^3/с</returns>
        double GetRiverFlow(double H);
    }
    /// <summary>
    /// Расход речного потока
    /// Кривые связи между отметкой гидропоста в см и расходом реки в м^3/с 
    /// </summary>
    public class RiverFlowZerro : IRiverFlow
    {
        /// <summary>
        /// Название реки/гидропоста
        /// </summary>
        public string RiverName { get; } = "г.п. не определен";
        /// <summary>
        /// Расход речного потока
        /// </summary>
        /// <param name="H">уровень воды по гидропосту в (см)</param>
        /// <returns>расход речного потока в м^3/с</returns>
        public double GetRiverFlow(double H) => 0;
    }
    /// <summary>
    /// Расход речного потока реки Амур
    /// </summary>
    public class RiverFlowAmur : IRiverFlow
    {
        /// <summary>
        /// Название реки/гидропоста
        /// </summary>
        public string RiverName { get; } = "Амур. Хабаровский г.п.";
        /// <summary>
        /// Расход речного потока
        /// </summary>
        /// <param name="H">уровень воды по гидропосту в (см)</param>
        /// <returns>расход речного потока в м^3/с</returns>
        public double GetRiverFlow(double H)
        {
            return 0.025 * H * H + 26.4793 * H + 7170.74;
        }
    }
    /// <summary>
    /// Расход речного потока протоки Пемзенской
    /// </summary>
    public class RiverFlowPemza : IRiverFlow
    {
        /// <summary>
        /// Доля протоки в общем расходе реки
        /// </summary>
        double q;
        /// <summary>
        /// Расход речного потока реки Амур. Хабаровский г.п.
        /// </summary>
        RiverFlowAmur ra = new RiverFlowAmur();
        /// <summary>
        /// Название реки/гидропоста
        /// </summary>
        public string RiverName { get; } = "пр. Пемзенская. Хабаровский г.п.";

        public RiverFlowPemza(double q = 0.38)
        {
            this.q = q;
        }
        /// <summary>
        /// по кривой перехода
        /// по Н.Н. Бортин, В.M. Mилаев, А.М. Горчаков 2020 DOI: 10.35567/1999-4508-2020-2-5
        /// и данных 
        /// </summary>
        /// <param name="H"></param>
        /// <returns></returns>
        public double GetRiverFlow(double H)
        {
            return q * ra.GetRiverFlow(H);
        }
    }
    /// <summary>
    /// Расход речного потока протоки Бешенной
    /// </summary>
    public class RiverFlowMad : IRiverFlow
    {
        /// <summary>
        /// Доля протоки в общем расходе реки
        /// </summary>
        double q;
        /// <summary>
        /// Расход речного потока реки Амур. Хабаровский г.п.
        /// </summary>
        RiverFlowAmur ra = new RiverFlowAmur();
        /// <summary>
        /// Название реки/гидропоста
        /// </summary>
        public string RiverName { get; } = "пр. Бешенная. Хабаровский г.п.";

        public RiverFlowMad(double q = 0.08)
        {
            this.q = q;
        }
        /// <summary>
        /// Расход речного потока
        /// </summary>
        /// <param name="H">уровень воды по гидропосту в (см)</param>
        /// <returns>расход речного потока в м^3/с</returns>
        public double GetRiverFlow(double H)
        {
            return q * ra.GetRiverFlow(H);
        }
    }
    /// <summary>
    /// Расход речного потока протоки Бешенной
    /// </summary>
    public class RiverFlowKomsomolskNaAmure : IRiverFlow
    {
        /// <summary>
        /// Название реки/гидропоста
        /// </summary>
        public string RiverName { get; } = "Комсомольск на Амуре г.п.";
        /// <summary>
        /// Расход речного потока
        /// </summary>
        /// <param name="H">уровень воды по гидропосту в (см)</param>
        /// <returns>расход речного потока в м^3/с</returns>
        public double GetRiverFlow(double H)
        {
            // ВОДНЫЕ РЕСУРСЫ, 2023, том 50, № 4, с. 367–384
            double Q;
            if (H < 0.10700e3)
            {
                double t3 = H + 0.29300e3;
                double t4 = t3 * t3;
                Q = 0.973865610328638650e4 + H * 0.229988262910798192e2 + t4 * (-0.266453525910037560e-16) + t4 * t3 * 0.469483568075119982e-5;
            }
            else if (H < 0.70700e3)
            {
                double t11 = H - 0.10700e3;
                double t12 = t11 * t11;
                Q = 0.979799882629107924e4 + H * 0.252523474178403760e2 + t12 * 0.563380281690141310e-2 + t12 * t11 * 0.130575117370892007e-4;
            }
            else
            {
                double t18 = H - 0.70700e3;
                double t19 = t18 * t18;
                Q = -0.103321596244128159e3 + H * 0.461150234741784004e2 + t19 * 0.291373239436619712e-1 + t19 * t18 * (-0.485622065727699508e-4);
            }
            return Q;
        }
    }
}
