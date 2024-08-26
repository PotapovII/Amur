//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      07 04 2016 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System.Drawing;
    /// <summary>
    /// Класс для поддержки преобразования мировых координат в экранные
    /// </summary>
    public class CoScale
    {
        /// <summary>
        /// Текущие границы области
        /// </summary>
        public static double xmax = double.MinValue;
        public static double xmin = double.MaxValue;
        public static double ymax = double.MinValue;
        public static double ymin = double.MaxValue;

        /// <summary>
        /// Нейтральные границы области
        /// </summary>
        public static double Lxmax = double.MinValue;
        public static double Lxmin = double.MaxValue;
        public static double Lymax = double.MinValue;
        public static double Lymin = double.MaxValue;

        public static int scale = 1000;
        public static int shiftX = 80;
        public static int shiftY = 80;
        #region Преобразование координат
        /// <summary>
        /// Сохраняем основной диапазон координат
        /// </summary>
        public static void SetInitSubArea()
        {
            Lxmax = xmax;
            Lxmin = xmin;
            Lymax = ymax;
            Lymin = ymin;
        }
        /// <summary>
        /// Востанавливаем основной диапазон координат
        /// </summary>
        public static void ReSatrtArea()
        {
            xmax = Lxmax;
            xmin = Lxmin;
            ymax = Lymax;
            ymin = Lymin;
        }
        /// <summary>
        /// выбираем подобласть отрисовки
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="x_"></param>
        /// <param name="y_"></param>
        public static void SetSubArea(double _x, double _y, double x_, double y_)
        {
            xmax = x_ > Lxmax ? Lxmax : x_;
            xmin = _x < Lxmin ? Lxmin : _x;
            ymax = y_ > Lymax ? Lymax : y_;
            ymin = _y < Lymin ? Lymin : _y;
        }
        public static int ScaleX(double d)
        {
            // double s = (d - xmin) / (xmax - xmin);
            return (int)((d - xmin) / (xmax - xmin) * scale) + shiftX;
        }

        public static int ScaleY(double d)
        {
            return (int)((1 - (d - ymin) / (ymax - ymin)) * scale) + shiftY;
        }
        public static double ScaleRealX(int x)
        {
            return (x - shiftX) * (xmax - xmin) / scale + xmin;
        }
        public static double ScaleRealY(int y)
        {
            return (1 - (double)(y - shiftY) / scale) * (ymax - ymin) + ymin;
        }
        /// <summary>
        /// Копирование ректангла из одного битмапа в другой битмап по ректанглу
        /// </summary>
        /// <param name="srcBitmap">откуда я копирую область, заданную Rectangle srcRegion</param>
        /// <param name="srcRegion"></param>
        /// <param name="destBitmap">приемник</param>
        /// <param name="destRegion">область в которую копируем</param>
        public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }
        public static void DrawLine(Graphics g, Pen p, double xa, double ya, double xb, double yb)
        {
            int ixa = ScaleX(xa); int iya = ScaleY(ya);
            int ixb = ScaleX(xb); int iyb = ScaleY(yb);
            g.DrawLine(p, new Point(ixa, iya), new Point(ixb, iyb));
        }
        public static void DrawEllipse(Graphics g, Pen p, double xa, double ya, int d)
        {
            int ixa = ScaleX(xa); int iya = ScaleY(ya);
            Rectangle r = new Rectangle(ixa, iya, d, d);
            g.DrawEllipse(p, r);
        }

        public static void DrawString(Graphics g, string str, Font font, Brush brush, double xa, double ya, int shift = 0)
        {
            int ix = ScaleX(xa) + shift; int iy = ScaleY(ya) + shift;
            g.DrawString(str, font, brush, ix, iy);
        }

        #endregion
    }
}
