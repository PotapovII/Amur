// -------------------------------------------------------------------------------
// Источник 
// https://sharpsnippets.wordpress.com/2014/03/11/c-extension-complementary-color/
// для реализации  23 02 2025  Потапов И.И.
// -------------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Drawing;

    public static class ExtensionsColor
    {
        /// <summary>
        /// метод возвращает контрастный цвет к цвету фона
        /// </summary>
        /// <param name="source">Цвет фона</param>
        /// <param name="preserveOpacity">сохранение непрозрачности</param>
        /// <returns></returns>
        public static Color GetContrast(this Color source, bool preserveOpacity=false)
        {
            Color inputColor = source;
            // если значения RGB близки друг к другу с разницей менее 10 %, то,
            // если значения RGB находятся на более светлой стороне, уменьшите синий цвет на 50 %
            // (в конечном итоге он увеличится при преобразовании, приведенном ниже),
            // если значения RBB находятся на более темной стороне, уменьшите желтый цвет
            // примерно на 50 % (он увеличится при преобразовании)
            byte avgColorValue = (byte)((source.R + source.G + source.B) / 3);
            int diff_r = Math.Abs(source.R - avgColorValue);
            int diff_g = Math.Abs(source.G - avgColorValue);
            int diff_b = Math.Abs(source.B - avgColorValue);
            if (diff_r < 20 && diff_g < 20 && diff_b < 20) //The color is a shade of gray
            {
                // Добавлено для создания темного на светлом и светлого на темном
                if (avgColorValue < 123) //color is dark
                    //                          A  R   G    B 
                    inputColor = Color.FromArgb(inputColor.A, 50, 230,220);
                else
                    inputColor = Color.FromArgb(inputColor.A, 255, 255, 50);
            }
            byte sourceAlphaValue = source.A;
            if (!preserveOpacity)
            {
                // Мы не хотим, чтобы контрастный цвет был прозрачным более чем на 50%.
                sourceAlphaValue = Math.Max(source.A, (byte)127); 
            }
            RGB rgb = new RGB { R = inputColor.R, G = inputColor.G, B = inputColor.B };
            HSB hsb = ConvertToHSB(rgb);
            hsb.H = hsb.H < 180 ? hsb.H + 180 : hsb.H - 180;
            rgb = ConvertToRGB(hsb);
            return Color.FromArgb(sourceAlphaValue, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
        }
        public static RGB ConvertToRGB(HSB hsb)
        {
            // By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
            double chroma = hsb.S * hsb.B;
            double hue2 = hsb.H / 60;
            double x = chroma * (1 - Math.Abs(hue2 % 2 - 1));
            double r1 = 0d;
            double g1 = 0d;
            double b1 = 0d;
            if (hue2 >= 0 && hue2 < 1)
            {
                r1 = chroma;
                g1 = x;
            }
            else if (hue2 >= 1 && hue2 < 2)
            {
                r1 = x;
                g1 = chroma;
            }
            else if (hue2 >= 2 && hue2 < 3)
            {
                g1 = chroma;
                b1 = x;
            }
            else if (hue2 >= 3 && hue2 < 4)
            {
                g1 = x;
                b1 = chroma;
            }
            else if (hue2 >= 4 && hue2 < 5)
            {
                r1 = x;
                b1 = chroma;
            }
            else if (hue2 >= 5 && hue2 <= 6)
            {
                r1 = chroma;
                b1 = x;
            }
            double m = hsb.B - chroma;
            return new RGB()
            {
                R = r1 + m,
                G = g1 + m,
                B = b1 + m
            };
        }
        public static HSB ConvertToHSB(RGB rgb)
        {
            // By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
            double r = rgb.R;
            double g = rgb.G;
            double b = rgb.B;

            double max = Max(r, g, b);
            double min = Min(r, g, b);
            double chroma = max - min;
            double hue2 = 0d;
            if (chroma != 0)
            {
                if (max == r)
                {
                    hue2 = (g - b) / chroma;
                }
                else if (max == g)
                {
                    hue2 = (b - r) / chroma + 2;
                }
                else
                {
                    hue2 = (r - g) / chroma + 4;
                }
            }
            double hue = hue2 * 60;
            if (hue < 0)
            {
                hue += 360;
            }
            double brightness = max;
            double saturation = 0;
            if (chroma != 0)
            {
                saturation = chroma / brightness;
            }
            return new HSB()
            {
                H = hue,
                S = saturation,
                B = brightness
            };
        }
        private static double Max(double d1, double d2, double d3)
        {
            if (d1 > d2)
            {
                return Math.Max(d1, d3);
            }
            return Math.Max(d2, d3);
        }
        private static double Min(double d1, double d2, double d3)
        {
            if (d1 < d2)
            {
                return Math.Min(d1, d3);
            }
            return Math.Min(d2, d3);
        }
        public struct RGB
        {
            internal double R;
            internal double G;
            internal double B;
        }
        public struct HSB
        {
            internal double H;
            internal double S;
            internal double B;
        }
    }
}