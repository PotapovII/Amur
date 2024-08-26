//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                кодировка : 2.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
//                интеграция : 15.08.2023 Потапов И.И. 
//---------------------------------------------------------------------------
namespace RiverDB.RAnalytics
{
    using System;
    /// <summary>
    /// Определение класса точка в створе реки
    /// </summary>
    [Serializable]
    public class RiverPoint : IComparable
    {
        /// <summary>
        /// Координата по х
        /// </summary>
        public double x;
        /// <summary>
        /// Координата по y
        /// </summary>
        public double y;
        /// <summary>
        /// глубина потока
        /// </summary>
        public double h;
        /// <summary>
        /// натуральная координата створа
        /// </summary>
        public double s;
        public RiverPoint(double x, double y, double h, double s = 0)
        {
            this.x = x;
            this.y = y;
            this.h = h;
            this.s = s;
        }
        /// <summary>
        /// Для сортировки точек по натуральной координате
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int CompareTo(object obj)
        {
            RiverPoint a = obj as RiverPoint;
            if (a != null)
            {
                if (s < a.s)
                    return -1;
                if (s > a.s)
                    return 1;
                return 0;
            }
            else
                throw new Exception("ошибка приведения типа");
        }
        /// <summary>
        /// приведение к строке
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            string str = x.ToString() + "  " + y.ToString() + "  " + h.ToString() + "  " + s.ToString();
            return str;
        }
    }

}
