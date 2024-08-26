namespace RiverDB.Convertors
{
    using System;
    /// <summary>
    /// Функции класса для преобразования
    /// геодезических координат из координатной системы Пулково 1942 
    /// в координатную систему WGS84
    /// ------------------------------------------------------------
    /// Все угловые значения передаются и возвращаются в градусах,
    /// высоты передаются и возвращаются в метрах
    /// Сделано на основании ГОСТ Р 51794-2001 Российской Федерации
    /// ------------------------------------------------------------
    /// Широты и долготы передаются и возвращаются в градусах (то есть, 
    /// если есть широты и долготы есть в градусах/минутах/секундах, 
    /// их сначала нужно перевести в градусы по формуле Г+М/60+С/3600 )
    /// </summary>
    public class Convertor_SK42_to_WGS84
    {
        /// Число Пи 
        static double Pi = 3.14159265358979;
        /// <summary>
        /// Число угловых секунд в радиане
        /// </summary>
        static double ro = 206264.8062;
        /// <summary>
        /// Эллипсоид Красовского (Пулково 1942)
        /// Большая полуось
        /// </summary>
        static double aP = 6378245;
        /// <summary>
        /// Сжатие
        /// </summary>
        static double alP = 1 / 298.3;
        /// <summary>
        /// Квадрат эксцентриситета
        /// </summary>
        static double e2P = 2 * alP - alP * alP;
        /// <summary>
        /// Эллипсоид GRS80 (WGS84)
        /// Большая полуось
        /// </summary>
        static double aW = 6378137;
        /// <summary>
        ///  Сжатие
        /// </summary>
        static double alW = 1.0 / 298.257223563;
        /// <summary>
        /// Квадрат эксцентриситета
        /// </summary>
        static double e2W = 2 * alW - alW * alW;
        // Вспомогательные значения для преобразования эллипсоидов
        static double a = (aP + aW) / 2;
        static double e2 = (e2P + e2W) / 2;
        static double da = aW - aP;
        static double de2 = e2W - e2P;
        // Линейные элементы трансформирования, в метрах
        static double dx = 23.92;
        static double dy = -141.27;
        static double dz = -80.9;
        // Угловые элементы трансформирования, в секундах
        static double wx = 0;
        static double wy = -0.35;
        static double wz = -0.82;
        // Дифференциальное различие масштабов
        static double ms = -0.12 * 10e-6;
        /// <summary>
        /// Получение широты X
        /// </summary>
        /// <param name="Bd">широта  X</param>
        /// <param name="Ld">долгота Y</param>
        /// <param name="H">высота</param>
        /// <returns>широта</returns>
        public static double WGS84Lat(double Bd, double Ld, double H)
        {
            double B = Bd * Math.PI / 180;
            double L = Ld * Math.PI / 180;
            double tmp = (1 - e2 * Math.Sin(B) * Math.Sin(B));
            double M = a * (1 - e2) / (tmp * Math.Sqrt(tmp));
            double N = a / Math.Sqrt(1 - e2 * Math.Sin(B) * Math.Sin(B));

            double dB = ro / (M + H) * (N / a * e2 * Math.Sin(B) * Math.Cos(B) * da
            + (N * N / (a * a) + 1) * N * Math.Sin(B) * Math.Cos(B) * de2 / 2
            - (dx * Math.Cos(L) + dy * Math.Sin(L)) * Math.Sin(B) + dz * Math.Cos(B))
            - wx * Math.Sin(L) * (1 + e2 * Math.Cos(2 * B))
            + wy * Math.Cos(L) * (1 + e2 * Math.Cos(2 * B))
            - ro * ms * e2 * Math.Sin(B) * Math.Cos(B);

            return Bd + dB / 3600;
        }
        /// <summary>
        /// Получение долготы
        /// </summary>
        /// <param name="Bd">широта</param>
        /// <param name="Ld">долгота</param>
        /// <param name="H">высота</param>
        /// <returns>долгота</returns>
        public static double WGS84Long(double Bd, double Ld, double H)
        {
            double B = Bd * Pi / 180;
            double L = Ld * Pi / 180;
            double N = a / Math.Sqrt(1 - e2 * Math.Sin(B) * Math.Sin(B));

            double dL = ro / ((N + H) * Math.Cos(B)) * (-dx * Math.Sin(L) + dy * Math.Cos(L))
                        + Math.Tan(B) * (1 - e2) * (wx * Math.Cos(L) + wy * Math.Sin(L)) - wz;
            return Ld + dL / 3600;
        }
        /// <summary>
        /// Получение высоты
        /// </summary>
        /// <param name="Bd">широта</param>
        /// <param name="Ld">долгота</param>
        /// <param name="H">высота</param>
        /// <returns>высота</returns>
        public static double WGS84Alt(double Bd, double Ld, double H)
        {
            double B = Bd * Pi / 180;
            double L = Ld * Pi / 180;
            double N = a / Math.Sqrt(1 - e2 * Math.Sin(B) * Math.Sin(B));
            double dH = -a / N * da + N * Math.Sin(B) * Math.Sin(B) * de2 / 2
            + (dx * Math.Cos(L) + dy * Math.Sin(L)) * Math.Cos(B) + dz * Math.Sin(B)
            - N * e2 * Math.Sin(B) * Math.Cos(B) * (wx / ro * Math.Sin(L) - wy / ro * Math.Cos(L))
            + (a * a / N + H) * ms;
            return H + dH;
        }

        // Функции преобразования в координатную проекцию Гаусса-Крюгера
        public static double SK42BTOX(double B, double L, double H)
        {
            double Pi = Math.PI;
            double No = (6 + L) / 6.0; No = (int)No;
            double Lo = (L - (3 + 6 * (No - 1))) / 57.29577951;
            double Bo = B * Pi / 180;
            double SinBo = Math.Sin(Bo);
            double SinBo2 = SinBo * SinBo;
            double SinBo4 = SinBo2 * SinBo2;
            double SinBo6 = SinBo4 * SinBo2;
            double Lo2 = Lo * Lo;
            double Xa = Lo2 * (109500 - 574700 * SinBo2 + 863700 * SinBo4 - 398600 * SinBo6);
            double Xb = Lo2 * (278194 - 830174 * SinBo2 + 572434 * SinBo4 - 16010 * SinBo6 + Xa);
            double Xc = Lo2 * (672483.4 - 811219.9 * SinBo2 + 5420 * SinBo4 - 10.6 * SinBo6 + Xb);
            double Xd = Lo2 * (1594561.25 + 5336.535 * SinBo2 + 26.79 * SinBo4 + 0.149 * SinBo6 + Xc);
            return 6367558.4968 * Bo - Math.Sin(Bo * 2) * (16002.89 + 66.9607 * SinBo2 + 0.3515 * SinBo4 - Xd);
        }
        public static double SK42LTOY(double B, double L, double H)
        {
            double No = (6 + L) / 6; No = (int)No;
            double Lo = (L - (3 + 6 * (No - 1))) / 57.29577951;
            double Bo = B * Pi / 180;
            double SinBo = Math.Sin(Bo);
            double CosBo = Math.Cos(Bo);
            double SinBo2 = SinBo * SinBo;
            double SinBo4 = SinBo2 * SinBo2;
            double SinBo6 = SinBo4 * SinBo2;
            double Lo2 = Lo * Lo;
            double Ya = Lo2 * (79690 - 866190 * SinBo2 + 1730360 * SinBo4 - 945460 * SinBo6);
            double Yb = Lo2 * (270806 - 1523417 * SinBo2 + 1327645 * SinBo4 - 21701 * SinBo6 + Ya);
            double Yc = Lo2 * (1070204.16 - 2136826.66 * SinBo2 + 17.98 * SinBo4 - 11.99 * SinBo6 + Yb);
            return (5 + 10 * No) * 1e5 + Lo * CosBo * (6378245 + 21346.1415 * SinBo2 + 107.159 * SinBo4 + 0.5977 * SinBo6 + Yc);
        }
        // Функции преобразования из координатной проекции Гаусса-Крюгера
        // в градусы
        public static double SK42XTOB(double X, double Y, double Z)
        {
            double No = Y * 1e-6; No = (int)No;
            double Bi = X / 6367558.4968;
            double SinBi = Math.Sin(Bi);
            double SinBi2 = SinBi * SinBi;
            double SinBi4 = SinBi2 * SinBi2;
            double Bo = Bi + Math.Sin(Bi * 2) * (0.00252588685 - 0.0000149186 * SinBi2 + 0.00000011904 * SinBi4);
            double SinBo = Math.Sin(Bo);
            double CosBo = Math.Cos(Bo);
            double SinBo2 = SinBo * SinBo;
            double SinBo4 = SinBo2 * SinBo2;
            double SinBo6 = SinBo4 * SinBo2;
            double Zo = (Y - (10 * No + 5) * 1e5) / (6378245 * CosBo);
            double Zo2 = Zo * Zo;
            double Ba = Zo2 * (0.01672 - 0.0063 * SinBo2 + 0.01188 * SinBo4 - 0.00328 * SinBo6);
            double Bb = Zo2 * (0.042858 - 0.025318 * SinBo2 + 0.014346 * SinBo4 - 0.001264 * SinBo6 - Ba);
            double Bc = Zo2 * (0.10500614 - 0.04559916 * SinBo2 + 0.00228901 * SinBo4 - 0.00002987 * SinBo6 - Bb);
            double dB = Zo2 * Math.Sin(Bo * 2) * (0.251684631 - 0.003369263 * SinBo2 + 0.000011276 * SinBo4 - Bc);
            return (Bo - dB) * 180 / Pi;
        }

        public static double SK42YTOL(double X, double Y, double Z)
        {
            double No = Y * 1e-6; No = (int)No;
            double Bi = X / 6367558.4968;
            double SinBi = Math.Sin(Bi);
            double SinBi2 = SinBi * SinBi;
            double SinBi4 = SinBi2 * SinBi2;
            double Bo = Bi + Math.Sin(Bi * 2) * (0.00252588685 - 0.0000149186 * SinBi2 + 0.00000011904 * SinBi4);
            double SinBo = Math.Sin(Bo);
            double CosBo = Math.Cos(Bo);
            double SinBo2 = SinBo * SinBo;
            double SinBo4 = SinBo2 * SinBo2;
            double SinBo6 = SinBo4 * SinBo2;
            double Zo = (Y - (10 * No + 5) * 1e5) / (6378245 * CosBo);
            double Zo2 = Zo * Zo;
            double La = Zo2 * (0.0038 + 0.0524 * SinBo2 + 0.0482 * SinBo4 + 0.0032 * SinBo6);
            double Lb = Zo2 * (0.01225 + 0.09477 * SinBo2 + 0.03282 * SinBo4 - 0.00034 * SinBo6 - La);
            double Lc = Zo2 * (0.0420025 + 0.1487407 * SinBo2 + 0.005942 * SinBo4 - 0.000015 * SinBo6 - Lb);
            double Ld = Zo2 * (0.16778975 + 0.16273586 * SinBo2 - 0.0005249 * SinBo4 - 0.00000846 * SinBo6 - Lc);
            double dL = Zo * (1 - 0.0033467108 * SinBo2 - 0.0000056002 * SinBo4 - 0.0000000187 * SinBo6 - Ld);
            return (6 * (No - 0.5) / 57.29577951 + dL) * 180 / Pi;
        }
        public static void Test()
        {
            //// широта X
            double Lat = 5364814.2;
            //// долгота Y
            double Lon = 23505946.2;
            double H = 10;
            Console.WriteLine("В метрах: широта X :Lat ={0} долгота Y: Lon = {1}", Lat, Lon);

            double Latitude = Convertor_SK42_to_WGS84.SK42XTOB(Lat, Lon, H);
            double Longitude = Convertor_SK42_to_WGS84.SK42YTOL(Lat, Lon, H);

            Console.WriteLine("В градусах: широта X :Latitude ={0} долгота Y: Longitude = {1}", Lat, Lon);

            double Lat1 = Convertor_SK42_to_WGS84.SK42BTOX(Latitude, Longitude, H);
            double Lon1 = Convertor_SK42_to_WGS84.SK42LTOY(Latitude, Longitude, H);

            Console.WriteLine("В метрах: широта X :Lat ={0} долгота Y: Lon = {1}", Lat1, Lon1);

            double erx = (Lat1 - Lat) * 100 / Lat;
            double ery = (Lon1 - Lon) * 100 / Lon;

            Console.WriteLine("Ошибка в процентах: для широты X:= {0} для долготы Y := {1}", erx, ery);

        }

    }
}
