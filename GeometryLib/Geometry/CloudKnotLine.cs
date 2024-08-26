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
using CommonLib.Geometry;

namespace GeometryLib.Geometry
{
    /// <summary>
    /// Линия
    /// </summary>
    public class CloudKnotLine : IHLine
    {
        /// <summary>
        /// Начало линии
        /// </summary>
        public IHPoint A { get; set; }
        /// <summary>
        /// Конец линии
        /// </summary>
        public IHPoint B { get; set; }
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
    }

}
