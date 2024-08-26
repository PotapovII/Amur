
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 02.04.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using CommonLib.Geometry;
    using System;
    /// <summary>
    /// Геометрия - треугольник
    /// </summary>
    public class GEO
    {
        /// <summary>
        /// Длина отрезка
        /// </summary>
        /// <returns></returns>
        public static double Length(IHPoint A, IHPoint B)
        {
            double Length = Math.Sqrt((B.X - A.X)* (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));
            return Length;
        }
        /// <summary>
        /// Длина отрезка
        /// </summary>
        public static double Length(double x1, double y1, double x2, double y2)
        {
            double Length = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            return Length;
        }
        /// <summary>
        /// площадь треугольника 
        /// </summary>
        public static double TriangleArea(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double S = 0.5 * ((x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1));
            return S;
        }
        /// <summary>
        /// Площадь теугольника
        /// </summary>
        /// <param name="v1">вершина 1</param>
        /// <param name="v2">вершина 2</param>
        /// <param name="v3">вершина 3</param>
        /// <returns></returns>
        public static double TriangleArea(IHPoint v1, IHPoint v2, IHPoint v3)
        {
            double S = Math.Abs(v1.X * (v2.Y - v3.Y) + v2.X * (v3.Y - v1.Y) + v3.X * (v1.Y - v2.Y)) / 2.0;
            return S;
        }
        /// <summary>
        ///  Вычисление площади КЭ
        /// </summary>
        /// <param name="x">массив координат елемента Х</param>
        /// <param name="y">массив координат елемента Y</param>
        /// <returns></returns>
        public static double TriangleArea(double[] x, double[] y)
        {
            double S = (x[0] * (y[1] - y[2]) + x[1] * (y[2] - y[0]) + x[2] * (y[0] - y[1])) / 2.0;
            return S;
        }
        /// <summary>
        /// Центр треугольника
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static HPoint TriangleCenter(HPoint v1, HPoint v2, HPoint v3)
        {
            return (v1 + v2 + v3) / 3;
        }
        /// <summary>
        /// Центр треугольника
        /// </summary>
        public static IHPoint TriangleCenter(IHPoint v1, IHPoint v2, IHPoint v3)
        {
            IHPoint c = v1.IClone();
            c.X = (v1.X + v2.X + v3.X) / 3;
            c.Y = (v1.Y + v2.Y + v3.Y) / 3;
            return c;
        }
        public static HPoint GetNormalOrt(HPoint p1, HPoint p2)
        {
            HPoint dif = p2 - p1;
            HPoint n = new HPoint(dif.y, -dif.x);
            n.Normalize();
            return n;
        }
        /// <summary>
        /// Нормаль к грани
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static HPoint GetNormal(HPoint p1, HPoint p2)
        {
            HPoint dif = p2 - p1;
            HPoint n = new HPoint(dif.y, -dif.x);
            return n;
        }
        /// <summary>
        /// Центр выпуклого треугольника
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public static HPoint PolygonCentroid(IHPoint[] vertex)
        {
            HPoint PolyGeomCentr = new HPoint();
            foreach (HPoint v in vertex)
            {
                PolyGeomCentr += v;
            }
            PolyGeomCentr /= vertex.Length;
            HPoint PolygonCentr = new HPoint();
            double polygonarea = 0;
            for (int n = 0; n < vertex.Length; n++)
            {
                HPoint v1 = (HPoint)vertex[n];
                HPoint v2;
                if (n == vertex.Length - 1)
                    v2 = (HPoint)vertex[0];
                else
                    v2 = (HPoint)vertex[n + 1];
                HPoint TriangleGeomCentr = TriangleCenter(v1, v2, PolyGeomCentr);
                double triarea = TriangleArea(v1, v2, PolyGeomCentr);
                polygonarea += triarea;
                PolygonCentr += triarea * TriangleGeomCentr;
            }
            PolygonCentr /= polygonarea;
            return PolygonCentr;
        }
        /// <summary>
        /// Площадь выпуклого полигона
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public static double PolygonArea(IHPoint[] vertex)
        {
            HPoint PolyGeomCentr = new HPoint();
            foreach (HPoint v in vertex)
            {
                PolyGeomCentr += v;
            }
            PolyGeomCentr /= vertex.Length;

            double polygonarea = 0;
            for (int n = 0; n < vertex.Length; n++)
            {
                IHPoint v1 = vertex[n];
                IHPoint v2;
                if (n == vertex.Length - 1)
                    v2 = vertex[0];
                else
                    v2 = vertex[n + 1];
                double triarea = TriangleArea(v1, v2, PolyGeomCentr);
                polygonarea += triarea;
            }
            return polygonarea;
        }
    }
}
