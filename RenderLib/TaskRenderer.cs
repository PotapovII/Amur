//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 05.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 20.12.2023 Потапов И.И.
// перевод на BaseColorScheme
//---------------------------------------------------------------------------
namespace RenderLib
{
    using GeometryLib.World;
    using System;
    using System.Drawing;
    /// <summary>
    /// ОО: Класс методов для отрисовки системы координат
    /// </summary>
    public class TaskRenderer
    {
        /// <summary>
        /// ОО: Управляет преобразованием мировых координат в экраные (2D)
        /// </summary>
        protected WorldScaler zoom;
        /// <summary>
        /// Использование методов класса в случаях его использования а не наследования
        /// </summary>
        public void RenderCoordReper(WorldScaler zoom, Graphics g, BaseColorScheme colorScheme)
        {
            this.zoom = zoom;
            RenderCoordReper(g, colorScheme);
        }
        /// <summary>
        /// Отрисовка координтных осей
        /// </summary>
        /// <param name="g"></param>
        protected void RenderCoordReper(Graphics g, BaseColorScheme colorScheme)
        {
            // репер оконных координт
            PointF p0 = new PointF();
            PointF px = p0;
            PointF py = p0;
            zoom.WorldReper(ref p0, ref px, ref py);
            // координтные оси
            g.DrawLine(colorScheme.PenReper, p0, px);
            g.DrawLine(colorScheme.PenReper, p0, py);
            string res;
            // репер мировых координт
            PointF pw0 = p0;
            zoom.ScreenToWorld(ref pw0);
            PointF pwx = px;
            zoom.ScreenToWorld(ref pwx);
            PointF pwy = py;
            zoom.ScreenToWorld(ref pwy);
            // горизонтальные координаты
            float[] masX = WorldScaler.GetScaleVector(pw0.X, pwx.X);
            for (int i = 0; i < masX.Length; i++)
            {
                PointF pt = new PointF(masX[i], pw0.Y);
                zoom.WorldToScreen(ref pt);
                g.FillEllipse(colorScheme.BrushReper, pt.X - 2.5f, p0.Y - 2.5f, 5, 5);
                float v = colorScheme.ScaleValue(masX[i]);
                res = String.Format(colorScheme.FormatTextReper, v);
                g.DrawString(res, colorScheme.FontReper, colorScheme.BrushTextReper,
                    new PointF(pt.X, p0.Y - 2 * colorScheme.FontReper.Size));
            }
            // вертикальные координаты
            float[] masY = WorldScaler.GetScaleVector(pw0.Y, pwy.Y);
            for (int i = 0; i < masY.Length; i++)
            {
                PointF pt = new PointF(pw0.X, masY[i]);
                zoom.WorldToScreen(ref pt);
                g.FillEllipse(colorScheme.BrushReper, p0.X - 2.5f, pt.Y - 2.5f, 5, 5);
                float v = colorScheme.ScaleValue(masY[i]);
                res = String.Format(colorScheme.FormatTextReper, v);
                g.DrawString(res, colorScheme.FontReper, colorScheme.BrushTextReper,
                    new PointF(p0.X, pt.Y - 2 * colorScheme.FontReper.Size));
            }
        }
    }
}



