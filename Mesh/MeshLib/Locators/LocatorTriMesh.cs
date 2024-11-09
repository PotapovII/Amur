//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2022 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          22.07.22
//---------------------------------------------------------------------------
//                          19.07.24
//                  добавлен поск по ребрам
//---------------------------------------------------------------------------
namespace MeshLib.Locators
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using CommonLib.Geometry;
    using GeometryLib;
    using MemLogLib;
    /// <summary>
    /// ОО: Класс для получения значений функции вдоль линии 
    /// (створ) определенной над сеткой
    /// </summary>
    public class LocatorTriMesh
    {
        /// <summary>
        /// Несущая сетка цифоровой модели в узлах которой заданна функция
        /// </summary>
        protected IRenderMesh mesh = null;
        /// <summary>
        /// Точка начала створа
        /// </summary>
        protected HPoint p0 = new HPoint();
        /// <summary>
        /// Точка конца створа
        /// </summary>
        protected HPoint p1 = new HPoint();
        /// <summary>
        /// буфферные переменные
        /// </summary>
        protected double Dx;
        protected double Dy;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="start">координата начала створа</param>
        /// <param name="end">координата конца створа</param>
        public LocatorTriMesh(IRenderMesh mesh, HPoint start, HPoint end)
        {
            this.p0 = start;
            this.p1 = end;
            this.mesh = mesh;
            Dx = p1.x - p0.x;
            Dy = p1.y - p0.y;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="Points">координаты створа</param>
        public LocatorTriMesh(IRenderMesh mesh, IHPoint[] Points)
        {
            this.mesh = mesh;
            this.p0 = new HPoint(Points[0].X, Points[0].Y);
            this.p1 = new HPoint(Points[1].X, Points[1].Y);
            Dx = p1.x - p0.x;
            Dy = p1.y - p0.y;
        }
        /// <summary>
        /// Получить функцию в створе
        /// </summary>
        /// <param name="H">значение функции в узлах сетки</param>
        /// <param name="x">аргумент функции - натуральный параметр створа</param>
        /// <param name="y">значение функции H в точках аргумента</param>
        public void GetCurve(double[] H, ref double[] x, ref double[] y, IHPoint[] Points)
        {
            //List<TargetLinePoint> crossPoint = GetCrossLine(H);
            // Расчет пересечений
            List<TargetLinePoint> crossPoint = GetCrossLinePro(H); 
            TargetLinePoint[] rp = crossPoint.ToArray();
            List<double> xx = new List<double>();
            List<double> yy = new List<double>();
            if (rp.Length > 0)
            {
                MEM.Alloc(rp.Length, ref x);
                MEM.Alloc(rp.Length, ref y);
                Array.Sort(rp);
                xx.Add(rp[0].s);
                yy.Add(rp[0].h);
                for (int i = 1; i < rp.Length; i++)
                {
                    // исключение двойных точек для внутренних ребер
                    // можно конечно создать список ребер сетки,
                    // сделать их словать и обрабатывать только 1 раз, но НЕТ ВРЕМЕНИ ((
                    if (Math.Abs(rp[i].s - rp[i - 1].s) > MEM.Error6)
                    {
                        xx.Add(rp[i].s);
                        yy.Add(rp[i].h);
                    }
                }
                x = xx.ToArray();
                y = yy.ToArray();
            }
        }
        /// <summary>
        /// Проверить существование точки пересечения
        /// </summary>
        /// <param name="q0">координата начала ребра треугольника</param>
        /// <param name="q1">координата конца ребра треугольника</param>
        /// <returns></returns>
        public bool IsIntersecting(TargetLinePoint q0, TargetLinePoint q1, ref TargetLinePoint res)
        {
            bool isIntersecting = false;

            double vx = q1.x - q0.x;
            double vy = q1.y - q0.y;

            double wx = p0.x - q0.x;
            double wy = p0.y - q0.y;

            double denominator = vy * Dx - vx * Dy;
            // Убедитесь, что знаменатель > 0, если это так, то прямые параллельны
            if (MEM.Equals(denominator, 0) == false)
            {
                double u_a = (vx * wy - vy * wx) / denominator;
                double u_b = (Dx * wy - Dy * wx) / denominator;
                //Пересекается, если u_a и u_b находятся между 0 и 1
                if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                {
                    // Точка пересечения
                    double x = p0.x + u_a * Dx;
                    double y = p0.y + u_a * Dy;

                    HPoint cp = new HPoint(x, y);
                    double ss = (cp - p0).Length();
                    double alpha = (cp - q0).Length();
                    double L = Math.Sqrt(vx*vx + vy*vy);
                    double h = (1 - alpha / L) * q0.h + alpha / L * q1.h;
                    res = new TargetLinePoint(x, y, h, ss);

                    isIntersecting = true;
                }
            }
            return isIntersecting;
        }
        /// <summary>
        /// Получить функцию в створе
        /// </summary>
        /// <param name="H">значение функции в узлах сетки</param>
        /// <returns>список точек функции - от натуральной координаты s</returns>
        public List<TargetLinePoint> GetCrossLine(double[] H)
        {
            bool flagCross = false;
            TargetLinePoint point = null;
            List<TargetLinePoint> points = new List<TargetLinePoint>();
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            double[] h = { 0, 0, 0 };
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            int cu = 3;
            TargetLinePoint[] knots = new TargetLinePoint[cu];
            TriElement[] elems = mesh.GetAreaElems();
            for (uint e = 0; e < elems.Length; e++)
            {
                for (int i = 0; i < cu; i++)
                {
                    uint idx = elems[e][i];
                    h[i] = H[idx];
                    x[i] = X[idx];
                    y[i] = Y[idx];
                }

                for(uint i = 0; i < cu; i++)
                    knots[i] = new TargetLinePoint(x[i], y[i], h[i]);
                // бежим по ребрам симплекса
                for (uint i = 0; i < cu; i++)
                {
                    flagCross = IsIntersecting(knots[i], knots[(i + 1) % cu], ref point);
                    if (flagCross == true)
                        points.Add(new TargetLinePoint(point));
                }
            }
            return points;
        }

        /// <summary>
        /// Создать список точеск пересечений створа с гранями треугольников
        /// </summary>
        /// <param name="H">значение функции в узлах сетки</param>
        /// <returns>список точек функции - от натуральной координаты s</returns>
        public List<TargetLinePoint> GetCrossLinePro(double[] H)
        {
            //bool flagCross = false;
            TargetLinePoint point = null;
            List<TargetLinePoint> points = new List<TargetLinePoint>();
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            double[] h = { 0, 0, 0 };
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            int cu = 3;
            TargetLinePoint[] knots = new TargetLinePoint[cu];
            TriElement[] elems = mesh.GetAreaElems();
            for (uint e = 0; e < elems.Length; e++)
            {
                for (int i = 0; i < cu; i++)
                {
                    uint idx = elems[e][i];
                    h[i] = H[idx];
                    x[i] = X[idx];
                    y[i] = Y[idx];
                }
                // бежим по ребрам симплекса
                for (uint i = 0; i < cu; i++)
                {
                    bool isIntersecting = false;
                    double dx = x[(i + 1) % cu] - x[i];
                    double dy = y[(i + 1) % cu] - y[i];
                    double wx = p0.x - x[i];
                    double wy = p0.y - y[i];
                    double denominator = dy * Dx - dx * Dy;
                    // Убедитесь, что знаменатель > 0, если это так, то прямые параллельны
                    double eps = MEM.Error6;
                    if (MEM.Equals(denominator, 0, eps) == false)
                    {
                        double u_a = (dx * wy - dy * wx) / denominator;
                        double u_b = (Dx * wy - Dy * wx) / denominator;
                        //Пересекается, если u_a и u_b находятся между 0 и 1
                        if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                        {
                            // Точка пересечения
                            double xp = p0.x + u_a * Dx;
                            double yp = p0.y + u_a * Dy;
                            HPoint crossPoint = new HPoint(xp, yp);
                            HPoint q0 = new HPoint(x[i], y[i]);
                            double ss =  (crossPoint - p0).Length();
                            double alpha = (crossPoint - q0).Length();
                            double L = Math.Sqrt(dx * dx + dy * dy);
                            double hp = (1 - alpha / L) * h[i] + alpha / L * h[(i + 1) % cu];
                            point = new TargetLinePoint(xp, yp, hp, ss);
                            isIntersecting = true;
                        }
                    }
                    ////////////////////////
                    if (isIntersecting == true)
                        points.Add(new TargetLinePoint(point));
                }
            }
            return points;
        }

        /// <summary>
        /// Создать список точеск пересечений створа с гранями треугольников
        /// </summary>
        /// <param name="H">значение функции в узлах сетки</param>
        /// <returns>список точек функции - от натуральной координаты s</returns>
        public List<TargetLinePoint> GetCrossLineFacet(double[] H)
        {
            //bool flagCross = false;
            TargetLinePoint point = null;
            List<TargetLinePoint> points = new List<TargetLinePoint>();
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            double[] h = { 0, 0, 0 };
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            int cu = 3;
            TargetLinePoint[] knots = new TargetLinePoint[cu];
            TriElement[] elems = mesh.GetAreaElems();
            for (uint e = 0; e < elems.Length; e++)
            {
                for (int i = 0; i < cu; i++)
                {
                    uint idx = elems[e][i];
                    h[i] = H[idx];
                    x[i] = X[idx];
                    y[i] = Y[idx];
                }
                // бежим по ребрам симплекса
                for (uint i = 0; i < cu; i++)
                {
                    bool isIntersecting = false;
                    double dx = x[(i + 1) % cu] - x[i];
                    double dy = y[(i + 1) % cu] - y[i];
                    double wx = p0.x - x[i];
                    double wy = p0.y - y[i];
                    double denominator = dy * Dx - dx * Dy;
                    // Убедитесь, что знаменатель > 0, если это так, то прямые параллельны
                    double eps = MEM.Error6;
                    if (MEM.Equals(denominator, 0, eps) == false)
                    {
                        double u_a = (dx * wy - dy * wx) / denominator;
                        double u_b = (Dx * wy - Dy * wx) / denominator;
                        //Пересекается, если u_a и u_b находятся между 0 и 1
                        if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
                        {
                            // Точка пересечения
                            double xp = p0.x + u_a * Dx;
                            double yp = p0.y + u_a * Dy;
                            HPoint crossPoint = new HPoint(xp, yp);
                            HPoint q0 = new HPoint(x[i], y[i]);
                            double ss = (crossPoint - p0).Length();
                            double alpha = (crossPoint - q0).Length();
                            double L = Math.Sqrt(dx * dx + dy * dy);
                            double hp = (1 - alpha / L) * h[i] + alpha / L * h[(i + 1) % cu];
                            point = new TargetLinePoint(xp, yp, hp, ss);
                            isIntersecting = true;
                        }
                    }
                    ////////////////////////
                    if (isIntersecting == true)
                        points.Add(new TargetLinePoint(point));
                }
            }
            return points;
        }


    }
}
