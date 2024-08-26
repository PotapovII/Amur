//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 18.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.ConvertorIn
{
    using System;
    /// <summary>
    /// Перевод координат из WGS-84 в прямоугольную систему привязанную к WGS-84 координатам города/места
    /// </summary>
    public class Convert_WGS84_To_CityMetrs
    {
        /// <summary>
        /// Количество КМ в 1 градусе
        /// </summary>
        double EOne;
        double NOne;
        /// <summary>
        /// Координаты города в WGS84
        /// Широта в WGS-84
        /// </summary>
        double city_N;
        /// <summary>
        /// Долгота в WGS-84
        /// </summary>
        double city_E;
        /// <summary>
        /// Хабаровск
        /// </summary>
        public Convert_WGS84_To_CityMetrs()
        {
            // Широта в WGS-84
            double Khabarowsk_N = 48.480831;
            // Долгота в WGS-84
            double Khabarowsk_E = 135.092773;
            city_N = Khabarowsk_N;
            city_E = Khabarowsk_E;
            NOne = 1000 * 40008.55 / 360;
            EOne = NOne * Math.Cos(Math.PI*Khabarowsk_N/180);
        }

        public Convert_WGS84_To_CityMetrs(double city_E, double city_N)
        {
            this.city_N = city_N;
            this.city_E = city_E;
            NOne = 1000 * 40008.55 / 360;
            EOne = NOne * Math.Cos(Math.PI * city_N / 180);
        }
        public void WGS84_To_LocalCity(ref double E, ref double N)
        {
            double dN = N - city_N;
            double dE = E - city_E;
            N = dN * NOne;
            E = dE * EOne;
        }
    }
}
