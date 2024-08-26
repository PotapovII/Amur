//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2022 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                          19.07.24
//                   добавлен поск по ребрам
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using CommonLib.Geometry;
    using GeometryLib;
    using MemLogLib;
    using MeshLib.Mesh.Locators;

    /// <summary>
    /// ОО: Класс для получения значений функции вдоль линии 
    /// (створ) определенной над сеткой
    /// </summary>
    public class LocatorTriMeshFacet
    {
        /// <summary>
        /// Несущая сетка цифоровой модели в узлах которой заданна функция
        /// </summary>
        protected IRenderMesh mesh = null;
        /// <summary>
        /// создатель массива ребер сетки 
        /// </summary>
        protected CreatorMeshFacets cmf = null;
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
        /// <param name="Points">координаты створа</param>
        public LocatorTriMeshFacet(IRenderMesh mesh)
        {
            this.mesh = mesh;
            cmf = new CreatorMeshFacets(mesh);
        }
        /// <summary>
        /// Установить створ
        /// </summary>
        /// <param name="Points"></param>
        public void SetCrossLine(IHPoint[] Points)
        {
            this.p0 = new HPoint(Points[0].X, Points[0].Y);
            this.p1 = new HPoint(Points[1].X, Points[1].Y);
            Dx = p1.x - p0.x;
            Dy = p1.y - p0.y;
        }
        /// <summary>
        /// Создать список точеск пересечений створа с гранями треугольников
        /// </summary>
        /// <param name="H">значение функции в узлах сетки</param>
        /// <returns>список точек функции - от натуральной координаты s</returns>
        public List<TargetLinePoint> GetCrossLine(double[] H)
        {
            List<TargetLinePoint> points = new List<TargetLinePoint>();
            ISFacet[] facets = cmf.facets;
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            TargetLinePoint point = null;
            // бежим по ребрам 
            for (uint f = 0; f < facets.Length; f++)
            {
                int ia = facets[f].Pointid1;
                int ib = facets[f].Pointid2;
                bool isIntersecting = false;
                double dx = X[ib] - X[ia];
                double dy = Y[ib] - Y[ia];
                double wx = p0.x - X[ia];
                double wy = p0.y - Y[ia];
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
                        HPoint q0 = new HPoint(X[ia], Y[ia]);
                        double ss = (crossPoint - p0).Length();
                        double alpha = (crossPoint - q0).Length();
                        double L = Math.Sqrt(dx * dx + dy * dy);
                        double hp = (1 - alpha / L) * H[ia] + alpha / L * H[ib];
                        point = new TargetLinePoint(xp, yp, hp, ss);
                        isIntersecting = true;
                    }
                }
                ////////////////////////
                if (isIntersecting == true)
                    points.Add(new TargetLinePoint(point));
            }
            return points;
        }
        /// <summary>
        /// Получить функцию в створе
        /// </summary>
        /// <param name="H">значение функции в узлах сетки</param>
        /// <param name="x">аргумент функции - натуральный параметр створа</param>
        /// <param name="y">значение функции H в точках аргумента</param>
        public void GetCurve(double[] H, ref double[] x, ref double[] y)
        {
            List<TargetLinePoint> crossPoint = GetCrossLine(H);
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
    }
}
