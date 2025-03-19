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
    using GeometryLib.Locators;
    /// <summary>
    /// Линия
    /// </summary>
    public class HLine : IHLine
    {
        /// <summary>
        /// Начало линии
        /// </summary>
        public IHPoint A { get; set; } = null;
        /// <summary>
        /// Конец линии
        /// </summary>
        public IHPoint B { get; set; } = null;

        public HLine()
        {
            A = new HKnot();
            B = new HKnot();
        }
        public HLine(IHPoint a, IHPoint b)
        {
            A = a.IClone();
            B = b.IClone();
        }
        /// <summary>
        /// Проверить существование точки пересечения двух линий
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool IsCrossing(IHLine a, IHLine b)
        {
            return CrossLineUtils.IsIntersectingAlternative(a.A, a.B, b.A, b.B);
        }
        /// <summary>
        /// Проверить существование точки пересечения двух линий
        /// </summary>
        public bool IsCrossing(IHPoint a, IHPoint b, IHPoint c, IHPoint d)
        {
            return CrossLineUtils.IsIntersectingAlternative(a, b, c, d);
        }
        /// <summary>
        /// Проверить существование точки пересечения двух линий и вернуть точку пересечения
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsCrossing(IHLine a, IHLine b, ref IHPoint p)
        {
            return CrossLineUtils.IsIntersectingAlternative(a.A, a.B, b.A, b.B, ref p);
        }
        /// <summary>
        /// Проверить существование точки пересечения двух отрезков
        /// </summary>
        public bool IsCrossing(HPoint v11, HPoint v12, HPoint v21, HPoint v22)
        {
            return CrossLineUtils.IsCrossing(v11, v12, v21, v22);
        }
        /// <summary>
        /// Проверить существование точки пересечения двух отрезков и вычислить ее
        /// </summary>
        public bool IsCrossing(HPoint v11, HPoint v12, HPoint v21, HPoint v22, ref IHPoint p)
        {
            return CrossLineUtils.IsCrossing(v11, v12, v21, v22, ref p);
        }
    }

    /// <summary>
    /// Линия
    /// </summary>
    public class CloudKnotLine : HLine,  IHLine
    {
        public CloudKnotLine(IHLine line): this(line.A,line.B)
        {
        }

        public CloudKnotLine()
        {
            A = new CloudKnot(); 
            B = new CloudKnot(); 
        }
        public CloudKnotLine(IHPoint a, IHPoint b)
        {
            A = a.IClone();
            B = b.IClone() ;
        }
        public CloudKnotLine(CloudKnot a, CloudKnot b)
        {
            A = new CloudKnot(a);
            B = new CloudKnot(b);
        }
        public override string ToString()
        {
            return ((CloudKnot)A).ToString() +" "+ ((CloudKnot)B).ToString();
        }
        public static CloudKnotLine Parse(string line)
        {
            string[] mas = (line.Trim()).Split(' ');
            CloudKnot A = CloudKnot.ReadCloudKnot(mas, 0);
            CloudKnot B = CloudKnot.ReadCloudKnot(mas, 10);
            CloudKnotLine Line = new CloudKnotLine(A, B);
            return Line;
        }

    }

}
