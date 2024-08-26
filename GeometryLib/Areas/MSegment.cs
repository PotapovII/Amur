//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
using CommonLib.Areas;
using System.Collections.Generic;

namespace GeometryLib.Areas
{
    /// <summary>
    /// Связанный сегмент принадлежит фигуре 
    /// </summary>
    public class MSegment : IMSegment
    {
        /// <summary>
        /// Наоменование точек
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Статус фигуры
        /// </summary>
        public FigureStatus Status { get; set; }
        /// <summary>
        /// Координаты точки начала сегмента
        /// </summary>
        public IMPoint pointA { get { return figuraLink.GetPoint(index); } }
        /// <summary>
        /// Координаты точки конца сегмента
        /// </summary>
        public IMPoint pointB { get { return figuraLink.GetPoint(index + 1); } }

        /// <summary>
        /// номер узла A сегмента в фигуре
        /// </summary>
        public int nodeA { get { return index; } }
        /// <summary>
        /// номер узла B сегмента в фигуре
        /// </summary>
        public int nodeB { get { return (index + 1) % figuraLink.Count; } }

        /// <summary>
        /// индекс сегмента в фигуре
        /// </summary>
        public int index { get; }
        /// <summary>
        /// Связь с фигурами
        /// </summary>
        public IMFigura FiguraLink => figuraLink;
        protected IMFigura figuraLink;
        /// <summary>
        /// Количество узлов в полелинии
        /// </summary>
        public int CountKnots { get; set; }
        /// <summary>
        /// Маркер сегмента
        /// </summary>
        public int Marker { get; set; }
        public MSegment(IMFigura FiguraLink, int index, int CountKnots = 2, int Marker=1)
        {
            this.index = index;
            this.figuraLink = FiguraLink;
            this.CountKnots = CountKnots;
            this.Marker = Marker;
            this.Name = "segment" + index.ToString();
            this.Status = FigureStatus.SelectedShape;
        }
        public MSegment(IMSegment p)
        {
            this.index = p.index;
            this.Name = p.Name;
            this.Status = p.Status;
            this.CountKnots = CountKnots;
            this.Marker = Marker;
            figuraLink = ((MSegment)p).figuraLink;
        }
        /// <summary>
        /// Модификация сегмента, фигуры и вершин при приведение его в к состоянию split == 2
        /// </summary>
        public void Split()
        {
            List<IMSegment> Segments = figuraLink.Segments;
            List<CloudKnot> NewPoints = new List<CloudKnot>();
            for (int i = 0; i < Segments.Count; i++)
            {
                IMSegment seg = Segments[i];
                if (seg == null)
                {
                    CloudKnot p = (CloudKnot)seg.pointA;
                    NewPoints.Add(p);
                }
                else
                {
                    if (seg.CountKnots == 2)
                    {
                        CloudKnot p = (CloudKnot)seg.pointA;
                        NewPoints.Add(p);
                    }
                    else
                    {
                        CloudKnot pA = (CloudKnot)seg.pointA;
                        CloudKnot pB = (CloudKnot)seg.pointB;

                        double[] Attributes = new double[pA.Attributes.Length];
                        double ds = 1.0 / (seg.CountKnots - 1);
                        for (int j = 0; j < seg.CountKnots - 1; j++)
                        {
                            double N1 = 1 - j * ds;
                            double N2 = j * ds;
                            double x = pA.x * N1 + pB.x * N2;
                            double y = pA.y * N1 + pB.y * N2;
                            for (int k = 0; k < Attributes.Length; k++)
                                Attributes[k] = pA.Attributes[k] * N1 + pB.Attributes[k] * N2;
                            CloudKnot p = new CloudKnot(x, y, Attributes, Marker);
                            NewPoints.Add(p);
                        }
                    }
                }
                IMFigura newfig = new Figura(figuraLink.Name, figuraLink.FID, NewPoints);
                figuraLink = newfig;
            }
        }
        public string ToString(string Filter = "F15")
        {
            string str = index.ToString();
            str += " " + ((int)Status).ToString();
            str += " " + (CountKnots).ToString();
            str += " " + Marker.ToString();
            return str;
        }

        public static IMSegment Parse(string line, IMFigura FiguraLink)
        {
            string[] mas = (line.Trim()).Split(' ');
            int index = int.Parse(mas[0]);
            int Status = int.Parse(mas[1]);
            int CountKnots = int.Parse(mas[2]);
            int Marker = int.Parse(mas[3]);
            IMSegment sg = new MSegment(FiguraLink, index, CountKnots, Marker);
            sg.Status = (FigureStatus)Status;
            return sg;
        }

    }

}
