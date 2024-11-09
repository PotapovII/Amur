//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 28.07.2022 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using CommonLib.Geometry;
    using System;
    /// <summary>
    /// ОО: Точки функции h вдоль ось створа, 
    /// используется для постоения функций в сечениях (створе)
    /// опорной сетки
    /// </summary>
    public class TargetLinePoint : HPoint
    {
        /// <summary>
        /// Значение функции в точке (глубина реки)  
        /// </summary>
        public double h;
        /// <summary>
        /// расстояние по створу от начальной точки створа до точки
        /// </summary>
        public double s;
        /// <summary>
        /// Ка
        /// </summary>
        public TargetLinePoint() : base()
        {
            h = 0;
            s = 0;
        }
        public TargetLinePoint(TargetLinePoint p) : base((HPoint)p) 
        { 
            h = p.h; 
            s = p.s; 
        } 
        public TargetLinePoint(double x, double y, double h, double s = 0):base(x,y)
        {
            this.x = x;
            this.y = y;
            this.h = h;
            this.s = s;
        }
        public override int CompareTo(object obj)
        {
            TargetLinePoint a = obj as TargetLinePoint;
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
        public override string ToString()
        {
            string str = x.ToString() + "  " + y.ToString() + "  " + h.ToString() + "  " + s.ToString();
            return str;
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new TargetLinePoint(this);
    }
}
