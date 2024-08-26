﻿//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 28.07.2022 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib.Locators
{
    using CommonLib.Geometry;
    using MemLogLib;
    using System.Runtime.CompilerServices;
    using System;

    /// <summary>
    /// ОО: Работа с двумя линиями
    /// </summary>
    public class CrossLine
    {
        /// <summary>
        /// Проверить принадлежность точки иетервалу
        /// </summary>
        /// <param name="Xa"></param>
        /// <param name="Xb"></param>
        /// <param name="X"></param>
        /// <returns></returns>
        public static bool IsContains(double Xmin, double Xmax, double X)
        {
            return (X >= Xmin && X <= Xmax);
        }


        /// <summary>
        /// Проверить существование точки пересечения
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        public static bool IsIntersectingAlternative(IHPoint p1, IHPoint p2,
                                                     IHPoint p3, IHPoint p4)
        {
            bool isIntersecting = false;
            double denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
            // Убедитесь, что знаменатель > 0, если это так, то прямые параллельны
            if (denominator != 0)
            {
                double u_a = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
                double u_b = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;
                //Пересекается, если u_a и u_b находятся между 0 и 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    isIntersecting = true;
                }
            }
            return isIntersecting;
        }


        /// <summary>
        /// Проверить существование точки пересечения
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="q0"></param>
        /// <param name="q1"></param>
        /// <returns></returns>
        public static bool IsIntersectingAlternative(IHPoint p0, IHPoint p1,
                                                     IHPoint q0, IHPoint q1, ref IHPoint res)
        {
            bool isIntersecting = false;
            double denominator = (q1.Y - q0.Y) * (p1.X - p0.X) - (q1.X - q0.X) * (p1.Y - p0.Y);
            // Убедитесь, что знаменатель > 0, если это так, то прямые параллельны
            if (denominator != 0)
            {
                double u_a = ((q1.X - q0.X) * (p0.Y - q0.Y) - (q1.Y - q0.Y) * (p0.X - q0.X)) / denominator;
                double u_b = ((p1.X - p0.X) * (p0.Y - q0.Y) - (p1.Y - p0.Y) * (p0.X - q0.X)) / denominator;
                //Пересекается, если u_a и u_b находятся между 0 и 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    Intersect(p0, p1, q0, q1, ref res, ref isIntersecting);
                }
            }
            return isIntersecting;
        }


        /// <summary>
        /// Проверить существование точки пересечения
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="q0"></param>
        /// <param name="q1"></param>
        /// <returns></returns>
        public static bool IsIntersecting(IHPoint p0, IHPoint p1, IHPoint q0, IHPoint q1, ref IHPoint res)
        {
            bool isIntersecting = false;

            double ux = p1.X - p0.X;
            double uy = p1.Y - p0.Y;

            double vx = q1.X - q0.X;
            double vy = q1.Y - q0.Y;

            double wx = p0.X - q0.X;
            double wy = p0.Y - q0.Y;

            double denominator = vy * ux - vx * uy;
            // Убедитесь, что знаменатель > 0, если это так, то прямые параллельны
            if (MEM.Equals(denominator, 0) == false)
            {
                double u_a = (vx * wy - vy * wx) / denominator;
                double u_b = (ux * wy - uy * wx) / denominator;
                //Пересекается, если u_a и u_b находятся между 0 и 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    // Точка пересечения
                    res.X = p0.X + u_a * ux;
                    res.Y = p0.Y + u_a * uy;

                    isIntersecting = true;
                }
            }
            return isIntersecting;
        }
        /// <summary>
        /// Вычислить пересечение двух сегментов.
        /// </summary>
        /// <param name="p0">Начальная точка линии 1.</param>
        /// <param name="p1">Конечная точка линии 1.</param>
        /// <param name="q0">Начальная точка линии 2.</param>
        /// <param name="q1">Конечная точка линии 2.</param>
        /// <param name="c0">Точка пересечения.</param>
        /// <remarks>
        /// Это частный случай пересечения сегментов. Если 
        /// вызывающий алгоритм гарантирует, что действительное 
        /// пересечение существует, нет необходимости проверять 
        /// какие-либо особые случаи.
        /// </remarks>
        public static void IntersectLines(IHPoint p0, IHPoint p1, IHPoint q0, IHPoint q1, ref IHPoint c0)
        {
            double ux = p1.X - p0.X;
            double uy = p1.Y - p0.Y;

            double vx = q1.X - q0.X;
            double vy = q1.Y - q0.Y;

            double wx = p0.X - q0.X;
            double wy = p0.Y - q0.Y;

            double d = (ux * vy - uy * vx);
            double s = (vx * wy - vy * wx) / d;

            // Точка пересечения
            c0.X = p0.X + s * ux;
            c0.Y = p0.Y + s * uy;
        }
        /// <summary>
        /// Пересечение 2 прямых
        /// </summary>
        /// <param name="p0">Начальная точка линии 1.</param>
        /// <param name="p1">Конечная точка линии 1.</param>
        /// <param name="q0">Начальная точка линии 2.</param>
        /// <param name="q1">Конечная точка линии 2.</param>
        /// <returns></returns>
        public static void Intersect(IHPoint p0, IHPoint p1, IHPoint q0, IHPoint q1, ref IHPoint res, ref bool flag)
        {
            //Line1
            double A1 = p1.Y - p0.Y;
            double B1 = p0.X - p1.X;
            //Line2
            double A2 = q1.Y - q0.Y;
            double B2 = q0.X - q1.X;

            double C1 = A1 * p0.X + B1 * p0.Y;
            double C2 = A2 * q0.X + B2 * q0.Y;

            double det = A1 * B2 - A2 * B1;

            if (MEM.Equals(det, 0) == true)
            {
                flag = false;
                res = null;
            }
            else
            {
                flag = true;
                res.X = (B2 * C1 - B1 * C2) / det;
                res.Y = (A1 * C2 - A2 * C1) / det;
            }
        }

        public static bool AreLinesIntersecting(IHPoint l1_p1, IHPoint l1_p2, IHPoint l2_p1, IHPoint l2_p2, bool shouldIncludeEndPoints)
        {
            // Чтобы избежать проблем с точностью с плавающей запятой, мы можем добавить небольшое значение
            float epsilon = 0.00001f;

            bool isIntersecting = false;

            double denominator = (l2_p2.Y - l2_p1.Y) * (l1_p2.X - l1_p1.X) - (l2_p2.X - l2_p1.X) * (l1_p2.Y - l1_p1.Y);

            // Убедитесь, что знаменатель > 0, если линии не параллельны
            if (denominator != 0f)
            {
                double u_a = ((l2_p2.X - l2_p1.X) * (l1_p1.Y - l2_p1.Y) - (l2_p2.Y - l2_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;
                double u_b = ((l1_p2.X - l1_p1.X) * (l1_p1.Y - l2_p1.Y) - (l1_p2.Y - l1_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;

                // Пересекаются ли отрезки, если конечные точки совпадают
                if (shouldIncludeEndPoints)
                {
                    // Пересекается, если u_a и u_b находятся между 0 и 1 или ровно 0 или 1
                    if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    // Пересекается, если u_a и u_b находятся между 0 и 1
                    if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
            }

            return isIntersecting;
        }
        /// <summary>
        /// Поиск точки x,y - пересечения отрезка x1,y1-x2,y2 с нормалью опущеной из точки x3 y3
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LookCrossPoint(double x1, double y1, double x2, double y2, double x3, double y3, ref double x, ref double y)
        {
            double L = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            x = (x1 * x1 * x3 - 2 * x1 * x2 * x3 + x2 * x2 * x3 + x2 *
                (y1 - y2) * (y1 - y3) - x1 * (y1 - y2) * (y2 - y3)) / L;
            y = (x2 * x2 * y1 + x1 * x1 * y2 + x2 * x3 * (y2 - y1) - x1 *
                (x3 * (y2 - y1) + x2 * (y1 + y2)) + (y1 - y2) * (y1 - y2) * y3) / L;
        }
        /// <summary>
        /// Поиск точки p - пересечения отрезка p1 , p2  с нормалью опущеной из точки p3
        /// </summary>
        /// <param name="p1">первая точка линии</param>
        /// <param name="p2">вторая точка линии</param>
        /// <param name="p3">точка из которой опускам нормаль на линию</param>
        /// <param name="p">результат пересечения линии и нормали</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LookCrossPoint(IHPoint p1, IHPoint p2, IHPoint p3, ref IHPoint p)
        {
            double L = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            if (Math.Abs(L) > 0.0000001)
            {
                p.X = (p1.X * p1.X * p3.X - 2 * p1.X * p2.X * p3.X + p2.X * p2.X * p3.X + p2.X *
                    (p1.Y - p2.Y) * (p1.Y - p3.Y) - p1.X * (p1.Y - p2.Y) * (p2.Y - p3.Y)) / L;
                p.Y = (p2.X * p2.X * p1.Y + p1.X * p1.X * p2.Y + p2.X * p3.X * (p2.Y - p1.Y) - p1.X *
                    (p3.X * (p2.Y - p1.Y) + p2.X * (p1.Y + p2.Y)) + (p1.Y - p2.Y) * (p1.Y - p2.Y) * p3.Y) / L;
            }
            else
            {
                p.X = 0; p.Y = 0;
            }
            return L;
        }
    }
}
