#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//           кодировка : 01.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.Geometry
{
    using CommonLib.Geometry;
    /// <summary>
    /// Класс описывающий линию речного створа
    /// </summary>
    public class CrossLine : HLine
    {
        /// <summary>
        /// Название створа
        /// </summary>
        public string Name;

        public CrossLine(string Name, double xa, double ya, double xb, double yb)
        {
            this.Name = Name;
            A = new HPoint(xa, ya);
            B = new HPoint(xb, yb);
        }
        public CrossLine(string Name, IHPoint a, IHPoint b)
        {
            this.Name = Name;
            A = new HPoint(a);
            B = new HPoint(b);
        }
        public CrossLine(string Name, IHLine line)
        {
            this.Name = Name;
            A = new HPoint(line.A);
            B = new HPoint(line.B);
        }
        /// <summary>
        /// Конвертация в строку
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + ":" + A.X.ToString() + ":" + A.Y.ToString() + ":" + B.X.ToString() + ":" + B.Y.ToString();
        }
        /// <summary>
        /// Конвертация из строки / без защиты от д...
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static CrossLine Parse(string line)
        {
            string[] lines = line.Split(':');
            CrossLine cl = new CrossLine(
            lines[0],
            double.Parse(lines[1]),
            double.Parse(lines[2]),
            double.Parse(lines[3]),
            double.Parse(lines[4]));
            return cl;
        }
    }
}
