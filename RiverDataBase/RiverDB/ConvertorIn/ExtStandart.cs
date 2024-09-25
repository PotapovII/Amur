//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 18.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.Convertors
{
    using ConnectLib;
    using System;
    /// <summary>
    /// Полный формат GPX Knots
    /// </summary>
    public class ExtStandart : StandartGPX
    {
        public int ID;
        /// <summary>
        /// срезанная глубина
        /// </summary>
        public double sDepth = 0;
        /// <summary>
        /// шаг между замерами в м
        /// </summary>
        public double step;
        /// <summary>
        /// время между измерениями сек
        /// </summary>
        public DateTime time;
        /// <summary>
        /// скорость курсовая км/час
        /// </summary>
        public double speed;
        /// <summary>
        /// курс в градусах
        /// </summary>
        public double course;
        public ExtStandart() : base() { }
        public ExtStandart(double step, DateTime time, double speed, double course,
            double lat, double lon, DateTime dataTime, double depth, double sDepth, double T, int ID) :
            base(lat, lon, dataTime, depth, T)
        {
            this.step = step;
            this.time = time;
            this.speed = speed;
            this.course = course;
            this.sDepth = sDepth;
            this.ID = ID;
        }
        /// <summary>
        /// Комманда на вставку / обновление
        /// </summary>
        /// <returns></returns>
        public override string GetCommand(TypeCommand com)
        {
            switch(com)
            {
                // Установка маркера скорости
                case TypeCommand.updateDeapth:
                    {
                        string sql = "UPDATE[dbo].[knot] " +
                        "SET [knot_depth] = " + sDepth.ToString("F5") + ",[knot_fulldepth] = " + depth.ToString("F5") +
                        " WHERE knot_id =" + ID.ToString();
                        return sql;
                    }
                case TypeCommand.updateMark:
                    {
                        string SdataTime = dataTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        string sql = "UPDATE[dbo].[knot] " +
                        "SET [knot_marker] = 1  WHERE knot_datetime = '" + SdataTime + "'";
                        return sql;
                    }
                default:
                case TypeCommand.insert:
                    {
                        /// широта X
                        double e = lat;
                        /// долгота Y
                        double n = lon;
                        string SdataTime = dataTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        string s = "insert into knot "
                            + "(knot_depth, knot_fulldepth, knot_step, knot_speed, knot_course, knot_datetime"
                            + ",knot_n, knot_e, knot_latitude, knot_longitude, knot_temperature, knot_marker)"
                            + "values (" + sDepth.ToString() + "," + depth.ToString() + "," + step.ToString() + ","
                            + speed.ToString() + "," + course.ToString() + ",'" + SdataTime + "'," + n.ToString() + ","
                            + e.ToString() + "," + lat.ToString() + "," + lon.ToString() + "," + T.ToString() + ", 0)";
                        return s;
                    }
            }
        }
        public new string ToString()
        {
            string s = step.ToString() + ", ";
            s += time.ToString() + ", ";
            s += speed.ToString() + ", ";
            s += course.ToString() + ", ";
            s += base.ToString();
            return s;
        }
    }
}
