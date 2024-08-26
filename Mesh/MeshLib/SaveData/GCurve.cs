//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      07 04 2016 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Данные о кривой
    /// 07 04 2020 добавлени метод анализа GetListMaxY()
    /// </summary>
    public class GCurve
    {
        /// <summary>
        /// название кривой - легенда
        /// </summary>
        public string Name;
        /// <summary>
        /// флаг отрисовки
        /// </summary>
        public bool Check = true;
        /// <summary>
        /// Список точек кривой
        /// </summary>
        public List<GPoint> points = new List<GPoint>();
        public GCurve() { Name = "без имени"; }
        public GCurve(string Name = "без имени")
        {
            this.Name = Name;
            points.Clear();
        }
        public GCurve(string Name, double[] x, double[] y)
        {
            this.Name = Name;
            points.Clear();
            for (int i = 0; i < x.Length; i++)
                Add(x[i], y[i]);
        }
        /// <summary>
        /// количество кривых
        /// </summary>
        public int Count { get { return points.Count; } }
        /// <summary>
        /// постоянный шаг дискретизации интервала кривой
        /// </summary>
        public double Get_dx { get { return MinMax_X().Length / (Count - 1); } }
        /// <summary>
        /// добавить точку в кривую
        /// </summary>
        /// <param name="e">точка</param>
        public void Add(GPoint e) { points.Add(e); }
        /// <summary>
        /// добавить точку в кривую
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="y">координата Y</param>
        public void Add(double x, double y) { points.Add(new GPoint(x, y)); }
        public void RemoveAt(int i) { points.RemoveAt(i); }
        /// <summary>
        /// очистка точек кривой из списка
        /// </summary>
        public void Clear() { points.Clear(); }
        /// <summary>
        /// Получение максимумов в области концевые точки выброшены
        /// </summary>
        /// <returns></returns>
        public List<GPoint> GetListMaxY(int key, double err = 0.00000001)
        {
            List<GPoint> pmax = new List<GPoint>();
            for (int i = 1; i < points.Count - 1; i++)
            {
                if (i == 2 || i == 1 || i == points.Count - 1 || i == points.Count - 2)
                {
                    if (points[i - 1].Y < points[i].Y && points[i].Y > points[i + 1].Y)
                    {
                        if (Math.Abs(points[i].Y) > err)
                            pmax.Add(new GPoint(points[i].X, points[i].Y));
                    }
                }
                else
                {
                    double pm2 = points[i - 2].Y;
                    double pm1 = points[i - 1].Y;
                    double p0 = points[i].Y;
                    double pp1 = points[i + 1].Y;
                    double xm1 = points[i - 1].X;
                    double x0 = points[i].X;
                    double xp1 = points[i + 1].X;
                    double pp2 = points[i + 2].Y;
                    if (pm2 <= pm1 && pm1 < p0 && p0 > pp1 && pp1 >= pp2)
                    {
                        if (Math.Abs(points[i].Y) > err)
                        {
                            if (key == 0)
                            {
                                pmax.Add(new GPoint(points[i].X, points[i].Y));
                            }
                            else
                            {
                                // координата максимума
                                double xx = points[i].X;
                                double xz1 = -pm1 + 2 * p0 - pp1;
                                double xz2 = xz1 * xz1;
                                // координата максимального значения  (поиск по параболе)
                                double xMax = ((32 * x0 * p0 * p0 + ((-32 * x0 + 4 * xm1 - 4 * xp1) * pm1 + (-4 * xm1 + 4 * xp1 - 32 * x0) * pp1) * p0 + (6 * x0 - xm1 + 3 * xp1) * pm1 * pm1 + (-2 * xm1 + 20 * x0 - 2 * xp1) * pp1 * pm1 + (3 * xm1 - xp1 + 6 * x0) * pp1 * pp1)) / 8.0 / xz2;
                                double pp = points[i].Y;
                                double cc = 16 * p0 * p0 - 8 * pp1 * p0 - 8 * pm1 * p0 + pm1 * pm1 - 2 * pm1 * pp1 + pp1 * pp1;
                                // максимальное значения (поиск по параболе)
                                double pMax = 1.0 / 8 * cc / (-pm1 + 2 * p0 - pp1);
                                GPoint maxPoint = new GPoint(xMax, pMax);
                                GPoint cp = points[i];

                                double ee = Math.Abs(pp - pMax) / Math.Abs(pp + 0.000000001);
                                if (ee > 0.001)
                                    pmax.Add(new GPoint(points[i].X, points[i].Y));
                                else
                                    pmax.Add(maxPoint);
                            }
                        }
                    }
                }
            }
            return pmax;
        }
        /// <summary>
        /// Сортировка по аргументу
        /// </summary>
        public void SortX()
        {
            points.Sort((one, two) => one.X.CompareTo(two.X));
        }
        /// <summary>
        /// Сортировка по значению
        /// </summary>
        public void SortY()
        {
            points.Sort((one, two) => one.Y.CompareTo(two.Y));
        }
        /// <summary>
        /// Получение точки мини-макс для аргумента кривой
        /// в параметре Х которой хранится минимум аргумента для кривой
        /// в параметре Y которой хранится максимум аргумента для кривой
        /// </summary>
        /// <returns>точка мини-макс </returns>
        public GPoint MinMax_X()
        {
            if (points.Count > 0)
            {
                GPoint mm = new GPoint(points[0].X, points[0].X);
                foreach (GPoint p in points)
                {
                    double a = p.X;
                    if (mm.X > a) mm.X = a;
                    if (mm.Y < a) mm.Y = a;
                }
                return mm;
            }
            else
                return GPoint.Zero;
        }
        /// <summary>
        /// Получение точки мини-макс для значения кривой
        /// в параметре Х которой хранится минимум значения для кривой
        /// в параметре Y которой хранится максимум значения для кривой
        /// </summary>
        /// <returns>точка мини-макс </returns>
        public GPoint MinMax_Y()
        {
            if (points.Count > 0)
            {

                GPoint mm = new GPoint(points[0].Y, points[0].Y);
                foreach (GPoint p in points)
                {
                    double a = p.Y;
                    if (mm.X > a) mm.X = a;
                    if (mm.Y < a) mm.Y = a;
                }
                return mm;
            }
            else
                return GPoint.Zero;
        }
    }
}
