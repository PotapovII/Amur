//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 13.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.Convertors
{
    using System;
    /// <summary>
    /// Расширенный формат GPX
    /// </summary>
    public class ExtStandartOld : StandartGPX
    {
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
        /// <summary>
        /// срезанная глубина
        /// </summary>
        public double cutDepth;
        public ExtStandartOld() : base() { }

        public override string GetCommand()
        {
            /// широта X
            double e = lat;
            /// долгота Y
            double n = lon;
            string HD = "2000-01-01";
            string tmp = time.ToString("yyyy-MM-dd HH:mm:ss:fff");
            string Stime = HD + tmp.Substring(10, 13);

            string SdataTime = dataTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
            string s = "insert into knot "
                + "(knot_depth, knot_fulldepth, knot_step, knot_speed, knot_course, knot_datetime"
                + ",knot_n, knot_e, knot_latitude, knot_longitude, knot_temperature, knot_marker)"
                + "values (" + cutDepth.ToString() + "," + depth.ToString() + "," + step.ToString() +","+ speed.ToString() + "," + course.ToString() + ",'" + SdataTime + "'," + n.ToString() + ","
                + e.ToString() + "," + lat.ToString() + "," + lon.ToString() + "," + T.ToString() + ", 0 )";
            return s;
        }
        public new string ToString()
        {
            string s = step.ToString() + ", ";
            s += time.ToString() + ", ";
            s += speed.ToString() + ", ";
            s += course.ToString() + ", ";
            s += cutDepth.ToString() + ", ";
            s += base.ToString();
            return s;
        }
    }

}
