//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 01.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Drawing;
    using CommonLib;
    using CommonLib.DrvGraphics;
    using CommonLib.Points;
    /// <summary>
    /// Реализация графики для нативного рисования в виндовс формах
    /// </summary>
    public class DrvGraphicNative : IDrvGraphic
    {
        Graphics g;
        public DrvGraphicNative(Graphics g)
        {
            this.g = g;
        }
        public void SetGraphic(Graphics g)
        {
            this.g = g;
        }
        /// <summary>
        /// Отрисовка линии
        /// </summary>
        public void DrawLine(Pen pen, IFPoint pa, IFPoint pb)
        {
            g.DrawLine(pen, pa.X, pa.Y, pb.X,pb.Y);
        }
        public void DrawLine(Pen pen, PointF pa, PointF pb)
        {
            g.DrawLine(pen, pa, pb);
        }
        /// <summary>
        /// Отрисовка эллипса
        /// </summary>
        public void FillEllipse(SolidBrush brush, RectangleF rec)
        {
            g.FillEllipse(brush, rec);
        }
        public void FillEllipse(SolidBrush brush, float x, float y, float w, float h)
        {
            g.FillEllipse(brush, x, y, w, h);
        }
        /// <summary>
        /// Отрисовка строки
        /// </summary>
        public void DrawString(String text, Font font, SolidBrush brushText, PointF point)
        {
            g.DrawString(text, font, brushText, point);
        }
        public void DrawString(String text, Font font, SolidBrush brushText, IFPoint point)
        {
            g.DrawString(text, font, brushText, new PointF(point.X,point.Y));
        }
        /// <резюме>
        /// Функция GradientFill заполняет прямоугольные и треугольные структуры
        /// </summary>
        /// <param name = "pVertex"> Массив структур TriVertex, каждая из которых определяет вершину треугольника </param>
        /// <param name = "pMesh"> Массив структур TriElement в режиме треугольника </param>
        /// <param name = "ulMode"> Определяет режим градиентной заливки </param>
        /// <returns> Если функция завершается успешно, возвращается значение true, false </returns>
        public void GradientFill(TriVertex[] pVertex, TriElement[] pMesh, GradientFillMode ulMode)
        {
            var hdc = g.GetHdc();
            FillMethods.GradientFill(hdc, pVertex, (uint)pVertex.Length,
            pMesh, (uint)pMesh.Length, GradientFillMode.GRADIENT_FILL_TRIANGLE);
            g.ReleaseHdc(hdc);
        }

        /// <summary>
        /// Отрисовка полигона по точкам
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="points"></param>
        public void DrawPolygon(Pen pen, PointF[] points)
        {
            g.DrawPolygon(pen, points);
        }
        /// <summary>
        /// Отрисовка треугольника по точкам
        /// </summary>
        public void DrawTriangle(Pen pen, PointF[] points)
        {
            g.DrawLine(pen, points[0], points[1]);
            g.DrawLine(pen, points[1], points[2]);
            g.DrawLine(pen, points[2], points[0]);
        }
        public void DrawTriangle(Pen pen, IFPoint[] points)
        {
            g.DrawLine(pen, points[0].X, points[0].Y, points[1].X, points[1].Y);
            g.DrawLine(pen, points[1].X, points[1].Y, points[2].X, points[2].Y);
            g.DrawLine(pen, points[2].X, points[2].Y, points[0].X, points[0].Y);
        }
    }
}
