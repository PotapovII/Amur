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
    using System;
    using System.Drawing;
    using CommonLib.DrvGraphics;
    using GeometryLib.World;
    /// <summary>
    /// Абстракция графического движка
    /// </summary>

    /// <summary>
    /// ОО: Класс методов для отрисовки системы координат
    /// </summary>
    public class TaskRendererReper
    {
        protected ReperColorScheme cs;
        /// <summary>
        /// ОО: Управляет преобразованием мировых координат в экраные (2D)
        /// </summary>
        protected WorldScaler zoom;

        /// <summary>
        /// Использование методов класса в случаях его использования а не наследования
        /// </summary>
        public void RenderCoordReper(ReperColorScheme cs, WorldScaler zoom, IDrvGraphic g)
        {
            this.cs = cs;
            this.zoom = zoom;
            RenderCoordReper(g);
        }
        /// <summary>
        /// Отрисовка координтных осей
        /// </summary>
        /// <param name="g"></param>
        protected void RenderCoordReper(IDrvGraphic g)
        {
            // репер оконных координт
            PointF p0 = new PointF();
            PointF px = p0;
            PointF py = p0;
            zoom.WorldReper(ref p0, ref px, ref py);
            // координтные оси
            g.DrawLine(cs.PenReper, p0, px);
            g.DrawLine(cs.PenReper, p0, py);
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

                g.FillEllipse(cs.BrushReper, pt.X - 2.5f, p0.Y - 2.5f, 5, 5);
                res = String.Format(cs.FormatTextReper, masX[i]);
                g.DrawString(res, cs.FontReper, cs.BrushTextReper,
                    new PointF(pt.X, p0.Y - 2 * cs.FontReper.Size));
            }
            // вертикальные координаты
            float[] masY = WorldScaler.GetScaleVector(pw0.Y, pwy.Y);
            for (int i = 0; i < masY.Length; i++)
            {
                PointF pt = new PointF(pw0.X, masY[i]);
                zoom.WorldToScreen(ref pt);
                g.FillEllipse(cs.BrushReper, p0.X - 2.5f, pt.Y - 2.5f, 5, 5);
                res = String.Format(cs.FormatTextReper, masY[i]);
                g.DrawString(res, cs.FontReper, cs.BrushTextReper,
                    new PointF(p0.X, pt.Y - 2 * cs.FontReper.Size));
            }
        }
    }
}



