//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 02.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.Locators
{
    using System;
    using CommonLib.Geometry;
    using MemLogLib;
    /// <summary>
    /// ОО: Определяет принадлежит ли точка отрезку
    /// </summary>
    public class LineLocator
    {
        /// <summary>
        /// 1 точка отрезка
        /// </summary>
        IHPoint pA;
        /// <summary>
        /// 2 точка отрезка
        /// </summary>        
        IHPoint pB;
        double x0, x1, y0, y1, dx, dy, Length;
        public LineLocator()
        {
        }
        public LineLocator(IHPoint pA, IHPoint pB)
        {
            Set(pA, pB);
        }
        public void Set(IHPoint pA, IHPoint pB)
        {
            this.pA = pA;
            this.pB = pB;
            x0 = Math.Min(pA.X, pB.X);
            x1 = Math.Max(pA.X, pB.X);
            y0 = Math.Min(pA.Y, pB.Y);
            y1 = Math.Max(pA.Y, pB.Y);
            dx = Math.Abs(x1 - x0);
            dy = Math.Abs(y1 - y0);
            Length = Math.Sqrt(dx * dx + dy * dy);
        }
        /// <summary>
        /// Принадлежность точки линии
        /// </summary>
        public bool IsLiesOn(IHPoint test)
        {
            if ((x0 <= test.X && test.X <= x1) && (y0 <= test.Y && test.Y <= y1))
            {
                double f = (test.X - pA.X) * (pB.Y - pA.Y) - (test.Y - pA.Y) * (pB.X - pA.X);
                if (Math.Abs(f/Length) < MEM.Error10)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Принадлежность точки линии
        /// </summary>
        public bool IsLiesOn(double X, double Y)
        {
            double dx_test = Math.Abs(X - x0);
            double dy_test = Math.Abs(Y - y0);
            if ((x0 <= X && X <= x1) && (y0 <= Y && Y <= y1))
                if (Math.Abs(dy * dx_test - dx * dy_test) < MEM.Error10)
                    return true;
            return false;
        }
        /// <summary>
        /// Принадлежность точки линии
        /// </summary>
        /// <param name="pA"></param>
        /// <param name="pB"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public static bool IsLiesonSegment(IHPoint pA, IHPoint pB, IHPoint test)
        {
            double x0 = Math.Min(pA.X, pB.X);
            double x1 = Math.Max(pA.X, pB.X);
            double y0 = Math.Min(pA.Y, pB.Y);
            double y1 = Math.Max(pA.Y, pB.Y);
            double dx = Math.Abs(x1 - x0);
            double dy = Math.Abs(y1 - y0);
            double dx_test = Math.Abs(test.X - x0);
            double dy_test = Math.Abs(test.Y - y0);
            if ( (x0 <= test.X && test.X <= x1) && (y0 <= test.Y && test.Y <= y1) )
            {
                //if( dx < MEM.Error10 )
                //{
                //    if( dx_test < MEM.Error10 ) 
                //        return true;
                //}
                //if( dy < MEM.Error10 )
                //{
                //    if(dy_test < MEM.Error10 )
                //        return true;
                //}
                if (Math.Abs(dy * dx_test - dx * dy_test) < MEM.Error10)
                    return true;
            }
            return false;
        }
    }
}
