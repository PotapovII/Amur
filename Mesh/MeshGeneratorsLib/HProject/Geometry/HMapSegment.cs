//---------------------------------------------------------------------------
//                          ПРОЕКТ  "H?"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                кодировка : 2.10.2000 Потапов И.И. (c++)
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
// кодировка : 05.03.2021 Потапов И.И. (c++=> c#)
// при переносе не были сохранены механизмы поддержки
// изменения параметров задачи во времени отвечающие за
// переменные во времени граничные условия для/на граничных сегментах м/б позже востановлю
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using CommonLib;
    using GeometryLib;
    using MemLogLib;
    using System.Collections.Generic;
    using System.Linq;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Поддержка описания части границы для подобласти
    /// </summary>
    [Serializable]
    public class HMapSegment : IComparable
    {
        ///// <summary>
        ///// номер / флаг сегмента
        ///// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Метка границы 
        /// </summary>
        public int[] MarkBC = null;
        /// <summary>
        /// опорные точки сегмента
        /// </summary>
        public VMapKnot[] Knots = null;
        /// <summary>
        /// Количество узлов в сегменте
        /// </summary>
        public int Count { get { return Knots==null? 0 : Knots.Length; } } 

        double[] Fx = null;
        double[] Fy = null;
        double[] Ar = null;

        public HMapSegment() 
        {
            ID = 0;
        }
        public HMapSegment(List<VMapKnot> knots, int ID = 0, int markBC = 0)
        {
            this.ID = ID;
            Knots = knots.ToArray();
            if (Count < 2)
                throw new Exception("Количество узлов в сегменте меньше 2");
            MEM.VAlloc(Knots.Length, markBC, ref MarkBC, "MarkBC");
            for (int i = 0; i < Knots.Length; i++)
            {
                Knots[i].marker = markBC;
            }
        }
        public HMapSegment(HMapSegment s)
        {
            ID = s.ID;
            MEM.MemCpy(ref MarkBC, s.MarkBC);
            MEM.Alloc(s.Knots.Length, ref Knots);
            for (int i = 0; i < s.Knots.Length; i++)
                Knots[i] = new VMapKnot(s.Knots[i]);
        }
        public HMapSegment(List<VMapKnot> knots, int[] markBC, int ID=0)
        {
            this.ID = ID;
            MEM.MemCpy(ref MarkBC, markBC);
            MEM.Alloc(knots.Count, ref Knots);
            for (int i = 0; i < knots.Count; i++)
                Knots[i] = new VMapKnot(knots[i]);
        }
        public void Clear()
        {
            ID = 0;
            MarkBC = null;
            Knots = null;
        }
        /// <summary>
        /// Установить флаг сегмента в marker узлов
        /// </summary>
        public void SetMarkBCToKnots()
        {
            if (Knots == null) return;
            for (int i = 0; i < Knots.Length; i++)
            {
                Knots[i].marker = MarkBC[i];
            }
        }
        #region IComparable

        /// <summary>
        /// Ключ для словарей: по первому и последнему узлу сегмента
        /// </summary>
        /// <returns></returns>
        public string GetHash()
        {
            List<VMapKnot> cknots = new List<VMapKnot>();
            cknots.Add(Knots[0]);
            cknots.Add(Knots[Knots.Length-1]);
            cknots.Sort();
            string hesh = cknots[0].GetHash() + "_" + cknots[1].GetHash();
            return hesh;
        }
        public int CompareTo(object obj)
        {
            HMapSegment other = obj as HMapSegment;
            return ID.CompareTo(other.ID);
        }
        //int CompareTo(HMapSegment other)
        //{
        //    return ID.CompareTo(other.ID);
        //}
        #endregion
        /// <summary>
        /// Длина сегмента
        /// </summary>
        /// <param name="Precision"></param>
        /// <returns>Длина сегмента</returns>
        public double Length(int Precision = 3)
        {
            double Length = 0;
            int Count;
            try
            {
                Count = Knots.Count();
                double dx, dy;
                int MaxNode = 2; // кол-во узлов трассировки
                if (Count > 2) MaxNode = Precision * Count;
                //
                MEM.Alloc(Count, ref Fx, "Fx");
                MEM.Alloc(Count, ref Fy, "Fy");
                MEM.Alloc(Count, ref Ar, "Ar");
                // Определение аргументов для сплайнов
                double step = 1.0 / (Count - 1);
                for (int i = 0; i < Count; i++)
                {
                    Ar[i] = i * step;
                    Fx[i] = Knots[i].x;
                    Fy[i] = Knots[i].y;
                }
                // вычисление сплайна
                TSpline SplY = new TSpline();
                TSpline SplX = new TSpline();
                SplX.Set(Fx, Ar);
                SplY.Set(Fy, Ar);
                // трассировка кривой
                double Step = 1.0 / (MaxNode - 1);
                double CXa = SplX.Value(0);
                double CYa = SplY.Value(0);
                double CXb, CYb;
                for (int i = 1; i < MaxNode; i++)
                {
                    CXb = SplX.Value(Step * i);
                    CYb = SplY.Value(Step * i);
                    dx = CXb - CXa;
                    dy = CYb - CYa;
                    // вычисленеие длины кривой
                    Length += Math.Sqrt(dx * dx + dy * dy);
                    CXa = CXb;
                    CYa = CYb;
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
            return Length;
        }
        /// <summary>
        /// переписывает узлы сегмента в противоположенном направлении
        /// </summary>
        public void Reverse()
        {
            List<VMapKnot> list = new List<VMapKnot>();
            foreach(var v in Knots)
                list.Add(v);
            list.Reverse();
            Knots = list.ToArray();
            //?
            MarkBC.Reverse();
        }
        /// <summary>
        /// Количество узлов на которые разбивается сегмент
        /// </summary>
        /// <param name="Diam">Средний диаметр разбиения</param>
        /// <param name="Rang">Порядок сетки</param>
        /// <returns></returns>
        public int BaseKnots(int Rang = 1,bool middle=true)
        {
            int BKnots;
            if (middle == true)
            {
                // в среднем
                double Diam = 0;
                try
                {
                    Diam = Knots.Sum(x => x.R);
                    //for (int i = 0; i < Knots.Count; i++)
                    //    Diam += Knots[i].R;
                    if (Diam < MEM.Error5)
                        return Rang + 1;
                    // средний диаметр
                    Diam /= Knots.Length;
                    double LengthSegment = Length();
                    BKnots = (int)(LengthSegment / Diam);
                    // определение кратности разбиения
                    BKnots /= Rang;
                    if (BKnots == 0)
                        BKnots = Rang + 1;
                    else
                        BKnots = Rang * BKnots + 1;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Exception(ex);
                    Logger.Instance.Warning("Средний диаметр сегмента равен нулю", "HMapSegment.BaseKnots()");
                    BKnots = Rang + 1;
                }
            }
            else
            {
                // Пока так
                BKnots = Rang + 1;
            }
            return BKnots;
        }
        /// <summary>
        /// Растяжение сегмента для 
        /// </summary>
        /// <param name="SetKnots">количество точек в сегменте</param>
        /// <returns></returns>
        public HMapSegment Stretching(int SetKnots)
        {
            HMapSegment NewSegment = null; 
            TSpline spline = new TSpline();
            try
            {
                // Обработка первого сегмента
                int Count = Knots.Length;
                // уходим если он пуст :)
                if (Count == 0)
                {
                    Logger.Instance.Warning("Попытка растяжения пустого сегмента", "HMapSegment.Stretching()");
                    return NewSegment;
                }
                MEM.Alloc(Count, ref Fx, "Fx");
                MEM.Alloc(Count, ref Ar, "Ar");
                // Определение аргументов для сплайнов
                double Step = 1.0 / (Count - 1);
                for (int i = 0; i < Count; i++)
                    Ar[i] = i * Step;
                // привязка свойств
                Step = 1.0 / (SetKnots - 1);
                // Инициализация узлов нового сегмента
                // Первым узлом старого сегмента
                List<VMapKnot> vMapKnots = new List<VMapKnot>();
                VMapKnot Knot = Knots[0];
                for (int i = 0; i < SetKnots; i++)
                    vMapKnots.Add(new VMapKnot(Knot));
                NewSegment = new HMapSegment(vMapKnots, ID, MarkBC[0]);
                double Value;
                // Вычисление координат для узлов сегмента
                // X
                for (int j = 0; j < Count; j++)
                    Fx[j] = Knots[j].x;
                spline.Set(Fx, Ar);
                for (int k = 0; k < SetKnots; k++)
                {
                    Value = spline.Value(Step * k);
                    NewSegment.Knots[k].x = Value;
                }
                for (int j = 0; j < Count; j++)
                    Fx[j] = Knots[j].y;
                spline.Set(Fx, Ar);
                for (int k = 0; k < SetKnots; k++)
                {
                    Value = spline.Value(Step * k);
                    NewSegment.Knots[k].y = Value;
                }
                // цикл по свойствам
                for (int i = 0; i < NewSegment.Knots[0].Values.Length; i++)
                {
                    for (int j = 0; j < Count; j++)
                        Fx[j] = Knots[j].Values[i];
                    spline.Set(Fx, Ar);
                    for (int k = 0; k < SetKnots; k++)
                    {
                        Value = spline.Value(Step * k);
                        NewSegment.Knots[k].Values[i] = Value;
                    }
                }

                NewSegment.ID = ID;
                //NewSegment.MarkBC = MarkBC;
                //NewSegment.TypeBC = TypeBC;
                NewSegment.SetMarkBCToKnots();
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Error("Ошибка при растяжения сегмента", "HMapSegment.Stretching()");
            }
            return NewSegment;
        }
        public static bool operator ==(HMapSegment a, HMapSegment b)
        {
            int Count = b.Count;
            if (Count != a.Count)
                return false;
            // начала сигментов совпадают
            if (HPoint.Equals(b.Knots[0], a.Knots[0]) == true)
            {
                // перебор по всем узлам сегментов
                for (int i = 1; i < Count; i++)
                    if (HPoint.Equals(b.Knots[i], a.Knots[i])==false)
                        return false;
            }
            else
            {
                // перебор по всем узлам сегментов
                // на встречю :)
                int j = Count - 1;
                for (int i = 0; i < Count; i++)
                    if (HPoint.Equals(b.Knots[i], a.Knots[j--]) == false)
                        return false;
            }
            return true;
        }
        public static bool operator !=(HMapSegment a, HMapSegment b)
        {
            if (a == b) return false;
            return true;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is HMapSegment))
                return false;
            else
                return this == ((HMapSegment)obj);
        }
        /// <summary>
        /// Сложение сегментов
        /// </summary>
        /// <param name="a">сегмент</param>
        /// <param name="b">сегмент</param>
        /// <returns>полученный сегмент</returns>
        public static HMapSegment operator + (HMapSegment a, HMapSegment b)
        {
            HMapSegment Segment = new HMapSegment();
            try
            {
                a.SetMarkBCToKnots();
                b.SetMarkBCToKnots();
                int CountA = a.Count;
                int CountB = b.Count;
                if ((CountA != 0) && (CountB != 0))
                {
                    // A-----B     C----D
                    VMapKnot A = a.Knots[0];
                    VMapKnot B = a.Knots[CountA - 1];
                    VMapKnot C = b.Knots[0];
                    VMapKnot D = b.Knots[CountB - 1];
                    if (HPoint.Equals(B, C)==true)
                    {
                        // A-----B  +  C----D
                        AddSegment(ref Segment, a, b);
                        //Segment = a;
                        //for (int i = 1; i < CountB; i++)
                        //    Segment.Knots.Add(b.Knots[i]);
                    }
                    else
                    if (HPoint.Equals(D, A) == true)
                    {
                        AddSegment(ref Segment, b, a);
                        //Segment = b;
                        //for (int i = 1; i < CountA; i++)
                        //    Segment.Knots.Add(a.Knots[i]);
                    }
                    if (Segment.Count != CountA + CountB - 1)
                        throw new Exception("Недопустимая операция суммирования сегментов, нет общих точек");
                }
                else
                    if ((CountA == 0) && (CountB != 0))
                    Segment = b;
                else
                        if (CountA != 0)
                    Segment = a;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Error("Ошибка при сложении сегментов", "HMapSegment operator +()");
            }
            return Segment;
        }
        public static void  AddSegment(ref HMapSegment s, HMapSegment a, HMapSegment b)
        {
            try
            {
                if (HPoint.Equals(a.Knots[a.Knots.Length - 1], b.Knots[0]) == true)
                {
                    s = new HMapSegment();
                    List<VMapKnot> vMapKnots = new List<VMapKnot>();
                    for (int i = 0; i < a.Knots.Length; i++)
                        vMapKnots.Add(new VMapKnot(a.Knots[i]));
                    for (int i = 1; i < b.Knots.Length; i++)
                        vMapKnots.Add(new VMapKnot(b.Knots[i]));
                    s.Knots = vMapKnots.ToArray();
                    int Length = s.Knots.Length;
                    MEM.Alloc(Length, ref s.MarkBC);
                    for (int i = 0; i < a.MarkBC.Length; i++)
                    {
                        s.MarkBC[i] = a.MarkBC[i];
                    }
                    int k = 0;
                    for (int i = 1; i < b.MarkBC.Length; i++)
                    {
                        s.MarkBC[a.MarkBC.Length + k] = b.MarkBC[i];
                        k++;
                    }
                }
                else
                    throw new Exception("Сегменты не согласованы!");
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
    }
}
