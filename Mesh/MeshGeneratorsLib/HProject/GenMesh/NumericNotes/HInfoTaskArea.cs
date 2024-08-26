//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?"
//---------------------------------------------------------------------------
//                кодировка : 2.10.2000 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//              кодировка : 04.02.2022 Потапов И.И. (c++=> c#)
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using System.Linq;
    using CommonLib;
    using GeometryLib;
    using MeshLib;
    using System.Collections.Generic;
    using CommonLib.Geometry;

    /// <summary>
    /// Информация для генерации КЭ сетки
    /// Класс выберает уникальные сегменты в области
    /// из которых постоены подобласти - необходимо для одинаковой генерации узлов 
    /// при склейке подобластей (и сеток для них)
    /// </summary>
    public class HInfoTaskArea
    {
        public List<int> SegmentNods = new List<int>();
        //public List<IFENods> SegmentNods = new List<IFENods>();
        /// <summary>
        /// базовые уникальные сегменты и их свойства
        /// </summary>
        public List<HMapSegment> BaseSegments = new List<HMapSegment>();
        /// <summary>
        /// ссылки на сегменты в подобластях
        /// </summary>
        public List<HNumArea> NumAreas = new List<HNumArea>();

        public HInfoTaskArea(){}
        /// <summary>
        /// Очистка
        /// </summary>
        public void Clear()
        {
            NumAreas.Clear();
            BaseSegments.Clear();
            SegmentNods.Clear();
        }
        /// <summary>
        /// преобразование фигурного определения области в древовидное
        /// TO DO : заменить поиск перебором в массиве сегментов, на выборку в словаре.
        /// Пока количество пдобластей не велико пофиг, но позже необходимо!
        /// </summary>
        /// <param name="TaskMap"></param>
        public void Set(IHTaskMap TaskMap)
        {
            HMapSegment Segment;
            if (TaskMap == null) return;
            Clear();
            bool UFlag;
            // количество уникальных сегментов
            int CountBSegm = 0;
            // количество подобластей
            List<HMapSubArea> AreaMaps = TaskMap.GetAreaMaps();
            for (int i = 0; i < TaskMap.CountMapSubArea; i++)
            {
                // формирование новой числовой подобласти
                HNumArea NumArea = new HNumArea();
                NumArea.ID = AreaMaps[i].ID;
                // количество ребер в подобласти
                int CountRibs = AreaMaps[i].Facets.Count;
                //Console.WriteLine("CountRibs =" + CountRibs);
                // цикл по ребрам в подобласти
                for (int j = 0; j < CountRibs; j++)
                {
                    // формирование нового числового ребра
                    HNumRibs NumRib = new HNumRibs();
                   // NumRib.ID = AreaMaps[i].Facets[j].ID;
                    // количество сегментов на ребре
                    int CountSegm = AreaMaps[i].Facets[j].Segments.Count;
                    //Console.WriteLine("CountSegm =" + CountSegm);
                    // цикл по сегментам
                    for (int k = 0; k < CountSegm; k++)
                    {
                        // 1 сегмент
                        Segment = AreaMaps[i].Facets[j].Segments[k];
                        UFlag = true;
                        // проверка на уникальность
                        int CodeSegmenta = -1;
                        for (int iu = 0; iu < CountBSegm; iu++)
                        {
                            if (Segment == BaseSegments[iu])
                            {
                                // добавление информации о сопряженности
                                CodeSegmenta = iu;
                                UFlag = false;
                                break;
                            }
                        }
                        if (UFlag == true)
                        {
                            // добавление нового уникального сегмента
                            BaseSegments.Add(Segment);
                            //Console.WriteLine("SID ="+CountBSegm +"    ID = " +Segment.ID.ToString());
                            CodeSegmenta = CountBSegm;
                            CountBSegm++;
                        }
                        //else 
                        //{
                        //    Console.WriteLine("!!!!!SID =" + CountBSegm + "    ID = " + Segment.ID.ToString());
                        //}
                        NumRib.Add(CodeSegmenta);
                    }
                    // добавление нового числового ребра в подобласть
                    NumArea.Add(NumRib);
                }
                // добавление новой подобласти
                NumAreas.Add(NumArea);
            }
        }
        /// <summary>
        /// Установка среднего диаметра разбиения сегмента на подобласти
        /// </summary>
        /// <param name="MidleDFE">средний диаметр КЭ</param>
        public void SetMidleDiam(double MidleDFE)
        {
            for (int i = 0; i < BaseSegments.Count; i++)
                for (int j = 0; j < BaseSegments[i].Knots.Length; j++)
                    BaseSegments[i].Knots[j].R = MidleDFE;
        }


        public void GetMaxKnot(TypeRangeMesh MeshRange, ref int MaxKnot, ref int MinKnot)
        {
            //  Разбиение сегментов на узлы
            MinKnot = int.MaxValue;
            MaxKnot = int.MinValue;
            for (int Segm = 0; Segm < BaseSegments.Count; Segm++)
            {
                int Knots = BaseSegments[Segm].BaseKnots((int)MeshRange);
                //SegmentNods.Add(new FENods(Knots));
                SegmentNods.Add(Knots);
                if (MinKnot > Knots)
                    MinKnot = Knots;
                if (MaxKnot < Knots)
                    MaxKnot = Knots;
            }
        }

        public int GetRealSegmRibs(int area, TypeRangeMesh MeshRange,
            ref int[] KnotRibs, ref int[] FElemRibs,
            ref HMapSegment[] RealSegmRibs )
        {
            int NRib = RealSegmRibs.Length;
            for (int i = 0; i < NRib; i++)
            {
                KnotRibs[i] = 0;           // количество узлов на границах подобласти
                FElemRibs[i] = 0;          // количество КЭ - в на границах подобласти
                RealSegmRibs[i].Clear(); // граничные сегменты после разбиения и ориентировки
            }
            int CountRib = NumAreas[area].NumRibs.Count;
            for (int rib = 0; rib < CountRib; rib++)
            {
                // количество узлов на границах подобласти
                KnotRibs[rib] = 0;
                // количество сегментов на ребре
                int CountSegm = NumAreas[area].NumRibs[rib].IndexSegments.Count;
                for (int sg = 0; sg < CountSegm; sg++)
                {
                    // индекс сегмента на грани
                    int IndexSegm = NumAreas[area].NumRibs[rib].IndexSegments[sg];
                    // количество узлов на грани
                    //int RealKnots = SegmentNods[IndexSegm].ID;
                    int RealKnots = SegmentNods[IndexSegm];
                    // сумарное количество узлов на грани
                    KnotRibs[rib] += RealKnots;
                    // суммарный (составной) сегмент для всей грани
                    // с заданным количестом узлов
                    RealSegmRibs[rib] = RealSegmRibs[rib] + BaseSegments[IndexSegm].Stretching(RealKnots);
                }
                FElemRibs[rib] = (KnotRibs[rib] - CountSegm) / ((int)MeshRange);
                KnotRibs[rib] -= CountSegm - 1;
            }
            VMapKnot a, b, c;
            for (int i = 0; i < CountRib; i++)
            {
                // координаты начала 1 - сегмента
                a = RealSegmRibs[i].Knots[0];
                // координаты начала и конца 2 - сегмента
                int Next = (i + 1) % CountRib;
                b = RealSegmRibs[Next].Knots[0];
                c = RealSegmRibs[Next].Knots[RealSegmRibs[Next].Knots.Length - 1];
                if (!(HPoint.Equals(a, b) == false && HPoint.Equals(a, c) == false))
                    RealSegmRibs[i].Reverse();
            }
            return CountRib;
        }

    }
}
