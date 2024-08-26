//---------------------------------------------------------------------------
//  Проектировщик Потапов Игорь Иванович
//  Разработка 1.03.2002
//  Основные определения для описания цифровой
//  модели местности вариант 1.0
//---------------------------------------------------------------------------
//  кодировка : 05.03.2021 Потапов И.И. (c++=> c#)
//  при переносе не были сохранены механизмы поддержки
//  изменения параметров задачи во времени отвечающие за
//  переменные граничные условия для/на граничных сегментах 
//  м/б позже востановлю
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System.Collections.Generic;
    using CommonLib;
    /// <summary>
    /// ОО: класс для описания ребер подобласти
    /// </summary>
    public class HNumRibs 
    {
        /// <summary>
        /// тип граничного услоия 
        /// </summary>
        public  TypeBoundCond TypeBC { get; set; }
        /// <summary>
        /// Метка границы 
        /// </summary>
        public int MarkBC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        protected int Index;
        /// <summary>
        /// номера сегментов принадлежащих ребру
        /// </summary>
        public List<int> IndexSegments;

        public HNumRibs()
        {
            //ID = 0;
            Index = 0;
            IndexSegments = new List<int>();
        }
        public int Increment()
        {
            int seg = 0;
            int Count = IndexSegments.Count;
            if (Count > 0)
            {
                seg = Index % Count;
                Index++;
                if (Index >= Count)
                    Index -= Count;
            }
            return seg;
        }
        public void Clear()
        {
            IndexSegments.Clear();
        }
        public void Add(int Number)
        {
            IndexSegments.Add(Number);
        }
        /// <summary>
        /// Вывод простого объекта в строку
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            string str = "ID: " //+ ID.ToString() 
                + " Flag: " + MarkBC.ToString() + " TypeBC: " + TypeBC.ToString();
            foreach (int a in IndexSegments)
                str += " " + a.ToString();
            return str;
        }
    }

}