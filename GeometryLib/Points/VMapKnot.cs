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
//           кодировка : 25.12.2020 Потапов И.И. (c++=> c#)
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using CommonLib.Geometry;
    using MemLogLib;
    [Serializable]
    public class VMapKnot : HKnot
    {
        /// <summary>
        /// Радиус разбиения
        /// </summary>
        public double R;
        /// <summary>
        /// Свойства в узлах (состояние контексное) от задачи
        /// </summary>
        public double[] Values = null;
        public VMapKnot() : base() { R = 1; Values = new double[1]; }
        public VMapKnot(double xx, double yy, double[] v, double R = 1, int marker = 0, int typeEx = 0)
            : base(xx, yy, marker, typeEx)
        {
            this.R = R;
            MEM.MemCopy(ref Values, v);
        }
        public VMapKnot(VMapKnot k) : base(k)
        {
            R = k.R;
            MEM.MemCopy(ref Values, k.Values);
        }

        public override string ToString(string format = "F4")
        {
            string s = " " + x.ToString(format) +
                   " " + y.ToString(format) +
                   " " + marker.ToString() +
                   " " + typeEx.ToString() +
                   " " + R.ToString(format);
            for (int j = 0; j < Values.Length; j++)
                s += " " + Values[j].ToString(format);
            return s;
        }

        public new static VMapKnot Parse(string line)
        {
            string[] mas = (line.Trim()).Split(' ');
            int id = int.Parse(mas[0]);
            double xx = double.Parse(mas[1], MEM.formatter);
            double yy = double.Parse(mas[2], MEM.formatter);
            int marker = int.Parse(mas[3]);
            int typeEx = int.Parse(mas[4]);
            double R = double.Parse(mas[5], MEM.formatter);
            int count = mas.Length - 6;
            count = count <= 0 ? 1 : count;
            double[] v = new double[count];
            for (int j = 6; j < mas.Length; j++)
                v[j - 6] = double.Parse(mas[j], MEM.formatter);
            VMapKnot p = new VMapKnot(xx, yy, v, R, marker, typeEx);
            return p;
        }
        /// <summary>
        /// Ключ для словарей
        /// </summary>
        /// <returns></returns>
        public string GetHash()
        {
            return x.ToString(WR.format5) + y.ToString(WR.format5);
        }

    /// <summary>
    /// (старое) выбераем параметры с максимальным BoundType в узле
    /// 06 03 2021 копируем свойства с узла аргумента
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public VMapKnot SelectBC(VMapKnot a)
        {
            MEM.MemCopy(ref Values, a.Values);
            return this;
            //// количество свойств на слое
            //int CountP = Values.Length;
            //if (CountP == null) 
            //    return this;
            //// Количество временных слоев
            //int CountTime = Values[0].MaxTime;
            //// сохранение индекса временного слоя
            //int CurInxTime = Values[0].TimeIndex;
            //// Параметры
            //for (int TimeIdx = 0; TimeIdx < CountTime; TimeIdx++)
            //{
            //    // смена временного слоя
            //    Values[0].TimeIndex = TimeIdx;
            //    for (int i = 0; i < CountP; i++)
            //    {
            //        if (Values[i].BoundType < a.Values[i].BoundType && a.Values[i].BoundType == 2)
            //        {
            //            Values[i].BoundType = a.Values[i].BoundType;
            //            Values[i].Value = a.Values[i].Value;
            //        }
            //    }
            //}
            //// Востановление индекса временного слоя
            //Values[0].TimeIndex = CurInxTime;
            //return this;
        }
        public override HPoint Clone()
        {
            return new VMapKnot(this);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new VMapKnot(this);
    }
}
