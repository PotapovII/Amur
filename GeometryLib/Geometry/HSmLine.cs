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
//           кодировка : 03.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
using CommonLib.Geometry;

namespace GeometryLib.Geometry
{
    /// <summary>
    /// Линия сглаживания 
    /// ОО: камеральная обработка эксперриментального облака данных 
    /// построенного по точкам наблюдения, основные функции:
    /// продолжение расчетной области области (одна вершина не связана с точкой облака
    /// - значения на линии постоянные и определяются связаной вершиной)
    /// линейная интерполяция в расчетной области - обе вершины связаны с точками облака
    /// </summary>
    public class HSmLine : CloudKnotLine, IHSmLine
    {
        /// <summary>
        /// Линия выбрана
        /// </summary>
        public int Selected { get; set; }
        /// <summary>
        /// Вершина A связана
        /// </summary>
        public bool LinkA { get; set; }
        /// <summary>
        /// Вершина B связана
        /// </summary>
        public bool LinkB { get; set; }
        /// <summary>
        /// Наличие связи 
        /// </summary>
        public bool Link { get => LinkA || LinkB; }
        /// <summary>
        /// Количество внутренних вершин линии
        /// </summary>
        public int Count { get; set; }
        public HSmLine(IHPoint a, IHPoint b) : base(a, b)
        {
            Count = 5;
            LinkA = false;
            LinkB = false;
            Selected = 0;
        }
        public HSmLine(CloudKnot a, CloudKnot b) : base(a, b)
        {
            Count = 5;
            LinkA = false;
            LinkB = false;
            Selected = 0;
        }
        public HSmLine(CloudKnot a, CloudKnot b, 
            int Count, bool LinkA, bool LinkB, int Selected) : base(a, b)
        {
            this.Count = Count;
            this.LinkA = LinkA;
            this.LinkB = LinkB;
            this.Selected = Selected;
        }
        public HSmLine(HSmLine sLine) : base(sLine.A, sLine.B)
        {
            Count = sLine.Count;
            LinkA = sLine.LinkA;
            LinkB = sLine.LinkB;
            Selected = sLine.Selected;
        }
        /// <summary>
        /// Длина отрезка
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return GEO.Length(A,B);
        }
        public override string ToString()
        {
            string line = Count.ToString()
                + " " + (LinkA == true ? 1 : 0).ToString()
                + " " + (LinkB == true ? 1 : 0).ToString()
                + " " + Selected.ToString();
                line += " " + A.ToString() + " " + B.ToString();
            return line;
        }
        public static HSmLine Parse(string line)
        {
            string[] mas = (line.Trim()).Split(' ');
            int Count = int.Parse(mas[0]);
            bool LinkA = (int.Parse(mas[1]) == 1) ? true : false;
            bool LinkB = (int.Parse(mas[2]) == 1) ? true : false;
            int Selected = int.Parse(mas[3]);
            CloudKnot A = CloudKnot.ReadCloudKnot(mas, 5);
            CloudKnot B = CloudKnot.ReadCloudKnot(mas, 17);
            HSmLine sLine = new HSmLine(A, B, Count, LinkA, LinkB, Selected);
            return sLine;
        }
        /// <summary>
        /// Выгрузка интерполяционных данных о линии сглаживания
        /// </summary>
        /// <returns></returns>
        public CloudKnot[] GetInterpolationData()
        {
            CloudKnot[] nods = null;
            if (LinkA == true && LinkB == true)
            {
                nods = new CloudKnot[Count];
                double ds = 1.0 / (Count + 1);
                for (int i = 0; i < Count; i++)
                    nods[i] = CloudKnot.Interpolation((CloudKnot)A, (CloudKnot)B, (i+1) * ds);
            }
            else
            {
                int shiftA = LinkA == true ? 1 : 0;
                int shiftB = LinkB == true ? 1 : 0;
                int CountAll = Count + 2;
                nods = new CloudKnot[CountAll - shiftA - shiftB];
                double ds = 1.0 / (Count + 1);
                int k = 0;
                CloudKnot S = null;
                if (LinkA == true)
                    S = (CloudKnot)A;
                else if (LinkB == true)
                    S = (CloudKnot)B;
                if(S != null)
                {
                    for (int i = shiftA; i < CountAll - shiftB; i++)
                    {
                        HPoint p = HPoint.Interpolation((CloudKnot)A, (CloudKnot)B, i * ds);
                        nods[k] = new CloudKnot(p, S.Attributes, S.mark);
                        k++;
                    }
                }
            }
            return nods;
        }

    }
}
