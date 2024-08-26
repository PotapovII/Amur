namespace RiverDB.Convertors
{
    using ConnectLib;
    using System;
    /// <summary>
    /// Усеченный формат GPX Knots
    /// </summary>
    public class StandartGPX : IStandartSQLCommand
    {
        /// <summary>
        /// широта X
        /// </summary>
        public double lat;
        /// <summary>
        /// долгота Y
        /// </summary>
        public double lon;
        /// <summary>
        /// Дата и время измерения
        /// </summary>
        public DateTime dataTime;
        /// <summary>
        /// наблюдаемая глубина
        /// </summary>
        public double depth;
        /// <summary>
        /// наблюдаемая температура воды
        /// </summary>
        public double T;
        public StandartGPX() { }
        public StandartGPX(double lat, double lon, DateTime dataTime, double depth, double T)
        {
            this.lat = lat;
            this.lon = lon;
            this.dataTime = dataTime;
            this.depth = depth;
            this.T = T;
        }
        public virtual string GetCommand()
        {
            DateTime d = DateTime.Parse(dataTime.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind);
            string FDT = d.ToString("yyyy-MM-dd HH:mm:ss:fff");
            string v = "insert into forknot(forknot_latitude,forknot_longitude,forknot_datetime,forknot_fulldepth,forknot_temperature)" +
            "values(" + lat.ToString() + "," + lon.ToString() + ",'" + FDT.ToString() + "'," + depth.ToString() + "," + T.ToString() + ")";
            return v;
        }
        public new string ToString()
        {
            string s = "lat:" + lat.ToString() + ", ";
            s += "lon:" + lon.ToString() + ", ";
            s += dataTime.ToString() + ", ";
            s += depth.ToString() + ", ";
            s += T.ToString();
            return s;
        }
    }

}
