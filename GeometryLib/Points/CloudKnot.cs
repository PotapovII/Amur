//---------------------------------------------------------------------------
//                         ПРОЕКТ  "RiverDB"
//                          проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 20.10.2023 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib
{
    using System;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib.Geometry;
    [Serializable]
    public static class AtrCK
    {
        /// <summary>
        /// Индексы атрибутов
        /// </summary>
        public const int idx_H      = 0;
        public const int idx_Hs     = 1;
        public const int idx_T      = 2;
        public const int idx_Vx     = 3;
        public const int idx_Vy     = 4;
        public const int idx_Ice    = 5;
        public const int idx_ks     = 6;
        /// <summary>
        /// Количество всех атрибутов 
        /// </summary>
        public const int Count = 7;
        /// <summary>
        /// Количество базовых атрибутов 
        /// </summary>
        public const int CountBase = 5;
        /// <summary>
        /// флаг создания внешнего атрибута - толщина льда
        /// </summary>
        public static bool ice = false;
        /// <summary>
        /// флаг создания внешнего атрибута - шероховатость дна
        /// </summary>
        public static bool ks = false;
        /// <summary>
        /// Названия атрибутов
        /// </summary>
        static string[] names = { "Глубина", "Срез.Глубина", "Температура", "Скорость", "Курс",  "Лед", "Шероховатость" };
        /// <summary>
        /// Количество атрибутов точки
        /// </summary>
        public static int CountAttributes => names.Length;
        /// <summary>
        /// Создать пустой массив атрибутов
        /// </summary>
        /// <returns></returns>
        public static double[] CreateZerro() => new double[CountAttributes];
        /// <summary>
        /// Название атрибутов точки
        /// </summary>
        public static string[] AtrNames => names; 
    }
    /// <summary>
    /// Атрибуты точек наблюдения из БД + внешние атрибуты точки
    /// </summary>
    [Serializable]
    public static class AttributesCloudKnot1
    {
        /// <summary>
        /// флаг создания внешнего атрибута - толщина льда
        /// </summary>
        public static bool ice = false;
        /// <summary>
        /// флаг создания внешнего атрибута - шероховатость дна
        /// </summary>
        public static bool ks = false;
        /// <summary>
        /// Количество атрибутов точки
        /// </summary>
        public static int CountAttributes => AtrNames.Length;
        /// <summary>
        /// Создать пустой массив атрибутов
        /// </summary>
        /// <returns></returns>
        public static double[] CreateZerro()=>new double[CountAttributes];
        /// <summary>
        /// Название атрибутов точки
        /// </summary>
        public static string[] AtrNames
        {
            get
            {
                List<string> names = new List<string>(new string[5]
                {  "Глубина", 
                   "Срез.Глубина", 
                   "Температура", 
                   "Скорость", 
                   "Курс" });
                if (ice == true)
                    names.Add(NameIce);
                if (ks == true)
                    names.Add(NameKs);
                return names.ToArray();
            }
        }
        /// <summary>
        /// Название атрибута лед
        /// </summary>
        public static string NameIce = "Лед";
        /// <summary>
        /// Название атрибута Шероховатость
        /// </summary>
        public static string NameKs = "Шероховатость";

    }
    /// <summary>
    /// Узел для облака данных
    /// </summary>
    [Serializable]
    public class CloudKnot : HPoint
    {
        /// <summary>
        /// Маркер узла (состояние контексное) от задачи
        /// </summary>
        public int ID;
        /// <summary>
        /// Свойства в узлах (состояние контексное) от задачи
        /// </summary>
        public double[] Attributes = null;
        /// <summary>
        /// Маркер узла (состояние контексное) от задачи
        /// </summary>
        public int mark;
        /// <summary>
        /// Время создания/ используется для сортировки точек в облаке
        /// </summary>
        [NonSerialized]
        public DateTime time;
        /// <summary>
        /// номер группы для сортированных точек в облаке
        /// </summary>
        [NonSerialized]
        public int timeGroupID = 0;
        public CloudKnot() : base() { Attributes = new double[1]; mark = 0; }
        public CloudKnot(double xx, double yy, double[] atr, int mark = 0, int ID = -1)
            : base(xx, yy)
        {
            this.mark = mark;
            this.ID = ID;
            MEM.MemCopy(ref Attributes, atr);
        }
        public CloudKnot(HPoint coord, double[] Atrs, int mark = 0, int ID = -1)
            : base(coord)
        {
            this.mark = mark;
            this.ID = ID;
            MEM.MemCopy(ref Attributes, Atrs);
        }
        public CloudKnot(CloudKnot p) : base(p)
        {
            this.mark = p.mark;
            this.ID = p.ID;
            MEM.MemCopy(ref Attributes, p.Attributes);
        }

        public override string ToString(string Filter = "F6")
        {
            string s = " " + x.ToString("F15") +
                       " " + y.ToString("F15") +
                       " " + mark.ToString() +
                       " " + ID.ToString();
            for (int j = 0; j < Attributes.Length; j++)
                s += " " + Attributes[j].ToString(Filter);
            return s;
        }
        public override string ToString()
        {
            string s = " " + x.ToString("F15") +
                       " " + y.ToString("F15") +
                       " " + mark.ToString() +
                       " " + ID.ToString();
            for (int j = 0; j < Attributes.Length; j++)
                s += " " + Attributes[j].ToString("F4");
            return s;
        }

        public new static CloudKnot Parse(string line)
        {
            string[] mas = (line.Trim()).Split(' ');
            double xx = double.Parse(mas[0], MEM.formatter); // 0
            double yy = double.Parse(mas[1], MEM.formatter); // 1
            int _mark = int.Parse(mas[2]);                   // 2
            int _ID = int.Parse(mas[3]);                     // 3
            int count = mas.Length - 4;                     
            count = count <= 0 ? 1 : count;
            double[] v = new double[count];
            for (int j = 4; j < mas.Length; j++)
                v[j - 4] = double.Parse(mas[j], MEM.formatter);
            CloudKnot p = new CloudKnot(xx, yy, v, _mark, _ID);
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
        public override HPoint Clone()
        {
            return new CloudKnot(this);
        }
        /// <summary>
        /// Создает копию объекта
        /// </summary>
        /// <returns></returns>
        public override IHPoint IClone() => new CloudKnot(this);

        /// <summary>
        /// Интерполирует CloudKnot точку между точками A и B
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="s">параметр интерполяции изменяется от 0 до 1</param>
        /// <returns></returns>
        public static CloudKnot Interpolation(CloudKnot A, CloudKnot B, double s, int mark = 0, int ID = 0)
        {
            double N1 = 1 - s;
            double N2 = s;
            double x = A.x * N1 + B.x * N2;
            double y = A.y * N1 + B.y * N2;
            CloudKnot V = new CloudKnot(x, y, new double[A.Attributes.Length], mark, ID);
            for (int k = 0; k < A.Attributes.Length; k++)
                V.Attributes[k] = A.Attributes[k] * N1 + B.Attributes[k] * N2;
            return V;
        }
        /// <summary>
        /// Чтение обласночй точки из массива
        /// </summary>
        /// <param name="mas">источник</param>
        /// <param name="position">кратно 8 элементам массива</param>
        /// <returns>облачная точка</returns>
        public static CloudKnot ReadCloudKnot(string[] mas, int position)
        {
            string p1 = mas[position];
            for (int i = 0; i < 8; i++)
                p1 += " " + mas[position + 1 + i];
            CloudKnot A = Parse(p1);
            return A;
        }
    }
}
