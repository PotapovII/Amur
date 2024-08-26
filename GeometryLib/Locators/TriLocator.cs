//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 25.12.2023 Потапов И.И. 
//---------------------------------------------------------------------------
using CommonLib.Geometry;

namespace GeometryLib.Locators
{
    public class TriLocator
    {
        /// <summary>
        /// Определение принадлежит ли первая точка массива треугольнику (три точки) 
        /// </summary>
        /// <returns></returns>
        public static bool Locator(double[] x, double[] y)
        {

            double a = (x[1] - x[0]) * (y[2] - y[1]) - (x[2] - x[1]) * (y[1] - y[0]);
            double b = (x[2] - x[0]) * (y[3] - y[2]) - (x[3] - x[2]) * (y[2] - y[0]);
            double c = (x[3] - x[0]) * (y[1] - y[3]) - (x[1] - x[3]) * (y[3] - y[0]);
            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
            {
                // Console.WriteLine("Принадлежит треугольнику");
                return true;
            }
            else
            {
                // Console.WriteLine("Не принадлежит треугольнике");
                return false;
            }
        }
        /// <summary>
        /// Проверьте, лежит ли данная точка внутри треугольника.
        /// </summary>
        /// <param name="p">Point to locate.</param>
        /// <param name="t0">Corner point of triangle.</param>
        /// <param name="t1">Corner point of triangle.</param>
        /// <param name="t2">Corner point of triangle.</param>
        public static bool IsPointInTriangle(IHPoint p, IHPoint t0, IHPoint t1, IHPoint t2)
        {
            // TODO: no need to create new Point instances here
            HPoint d0 = new HPoint(t1.X - t0.X, t1.Y - t0.Y);
            HPoint d1 = new HPoint(t2.X - t0.X, t2.Y - t0.Y);
            HPoint d2 = new HPoint(p.X - t0.X, p.Y - t0.Y);

            // векторное произведение (0, 0, 1) and d0
            HPoint c0 = new HPoint(-d0.Y, d0.X);
            //HPoint c00 = d0.GetOrtogonalLeft();
            // векторное произведение (0, 0, 1) and d1
            HPoint c1 = new HPoint(-d1.Y, d1.X);
            //HPoint c10 = d1.GetOrtogonalLeft();
            // Линейная комбинация d2 = s * d0 + v * d1.
            // Умножаем обе части уравнения на c0 и c1 и находим s и v соответственно
            // s = d2 * c1 / d0 * c1
            // v = d2 * c0 / d1 * c0

            double s = (d2 * c1) / (d0 * c1);
            double v = (d2 * c0) / (d1 * c0);

            if (s >= 0 && v >= 0 && ((s + v) <= 1))
            {
                // Точка находится внутри или на краю этого треугольника.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Если точка облака принадлежит треугольнику получить для нее атрибуты 
        /// через атрибуты вершин 
        /// </summary>
        public static bool Locator(CloudKnot[] p,ref CloudKnot knot)
        {
            double a = (p[0].x - knot.x) * (p[1].y - p[0].y) - (p[1].x - p[0].x) * (p[0].y - knot.y);
            double b = (p[1].x - knot.x) * (p[2].y - p[1].y) - (p[2].x - p[1].x) * (p[1].y - knot.y);
            double c = (p[2].x - knot.x) * (p[0].y - p[2].y) - (p[0].x - p[2].x) * (p[2].y - knot.y);

            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
            {
                // Console.WriteLine("Принадлежит треугольнику");
                double[] N = null;
                double[] DN_x = null;
                double[] DN_y = null;
                CalkFF(p, knot, ref N, ref DN_x, ref DN_y);
                for(int i = 0; i<knot.Attributes.Length; i++)
                {
                    knot.Attributes[i] = 0;
                    for (int k = 0; k < N.Length; k++)
                    {
                        knot.Attributes[i] += N[k] * p[k].Attributes[i];
                    }
                }
                return true;
            }
            else
            {
                // Console.WriteLine("Не принадлежит треугольнике");
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        /// <param name="N"></param>
        /// <param name="DN_x"></param>
        /// <param name="DN_y"></param>
        public static void CalkFF(IHPoint[] t, IHPoint p, ref double[] N, ref double[] DN_x, ref double[] DN_y)
        {
            MemLogLib.MEM.Alloc(3, ref N, "DN_x");
            MemLogLib.MEM.Alloc(3, ref DN_x, "DN_x");
            MemLogLib.MEM.Alloc(3, ref DN_y, "DN_y");

            double S = t[1].X * t[2].Y + t[0].Y * t[2].X + t[0].X * t[1].Y
                     - t[1].Y * t[2].X - t[0].X * t[2].Y - t[0].Y * t[1].X;

            DN_x[0] = (t[1].Y - t[2].Y) / S;
            DN_x[1] = (t[2].Y - t[0].Y) / S;
            DN_x[2] = (t[0].Y - t[1].Y) / S;

            DN_y[1] = (t[0].X - t[2].X) / S;
            DN_y[0] = (t[2].X - t[1].X) / S;
            DN_y[2] = (t[1].X - t[0].X) / S;

            double a0 = (t[1].X * t[2].Y - t[2].X * t[1].Y) / S;
            double a1 = (t[2].X * t[0].Y - t[0].X * t[2].Y) / S;
            double a2 = (t[0].X * t[1].Y - t[1].X * t[0].Y) / S;

            N[0] = a0 + DN_x[0] * p.X + DN_y[0] * p.Y;
            N[1] = a1 + DN_x[1] * p.X + DN_y[1] * p.Y;
            N[2] = a2 + DN_x[2] * p.X + DN_y[2] * p.Y;
        }
    }
}
