//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      07 04 2016 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    /// <summary>
    /// точка графика двойной точности
    /// </summary>
    public class GPoint : IComparable<GPoint>
    {
        /// <summary>
        /// значение аргумента в точке
        /// </summary>
        public double X;
        /// <summary>
        /// значение функции в точке
        /// </summary>
        public double Y;
        /// <summary>
        /// Вспомогательное поле
        /// </summary>
        public int Marker;
        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        public GPoint(double X, double Y) { this.X = X; this.Y = Y; Marker = 0; }
        public static GPoint Zero { get { return new GPoint(0, 0); } }
        public int CompareTo(GPoint obj)
        {
            return this.X.CompareTo(obj.X);
        }
        public void SetMarker(int k)
        {
            Marker = k;
        }
        public static GPoint MinMax { get { return new GPoint(double.MinValue, double.MaxValue); } }
        /// <summary>
        /// вычисление интевала для точек мини-макс
        /// </summary>
        public double Length { get { return Y - X; } }
        /// <summary>
        /// вычисление максимума
        /// </summary>
        public double Max { get { return Math.Max(Math.Abs(X), Math.Abs(Y)); } }
        /// <summary>
        /// вычисление минимума
        /// </summary>
        public double Min { get { return Math.Min(Math.Abs(X), Math.Abs(Y)); } }
        public override string ToString() { return "X= " + X.ToString() + " Y = " + Y.ToString() + " M = " + Marker.ToString(); }
    }
}
