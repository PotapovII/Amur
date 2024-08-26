//---------------------------------------------------------------------------
//    Проектировщик Потапов Игорь Иванович
//             Разработка 2002
//                1.03.2002
//  Основные определения для описания цифровой
//                модели местности
//                   вариант 1.0
//---------------------------------------------------------------------------
//           кодировка : 05.03.2021 Потапов И.И. (c++=> c#)
//           при переносе не были сохранены механизмы поддержки
//               изменения параметров задачи во времени отвечающие за
//            переменные граничные условия для/на граничных сегментах 
//                      м/б позже востановлю
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using CommonLib;
    using System.Collections.Generic;
    
    /// <summary>
    /// Цифровая карта - данные описывающие область определения задачи
    /// <summary>
    [Serializable]
    public class HTaskMap : IHTaskMap
    {
        /// <summary>
        /// имя карты для задачи
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// количество подобластей
        /// </summary>
        public int CountMapSubArea => AreaMaps.Count;
        ///// <summary>
        ///// взять подобласти 
        ///// </summary>
        ///// <returns></returns>
        public List<HMapSubArea> GetAreaMaps() => AreaMaps;
        /// <summary>
        /// подобласти 
        /// </summary>
        protected List<HMapSubArea> AreaMaps = new List<HMapSubArea>();
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="typeMesh"></param>
        /// <param name="MeshRange"></param>
        public HTaskMap(string Name ="Simple")
        {
            this.Name = Name;
        }
        public HTaskMap(HMapSubArea Elem, string Name = "Simple") : base()
        {
            AreaMaps.Add(Elem);
        }
        public HTaskMap(HTaskMap taskMap) : base()
        {
            this.Name = taskMap.Name;
            this.AreaMaps.AddRange(taskMap.AreaMaps.ToArray());
        }
        /// <summary>
        /// добавить подобласть
        /// </summary>
        /// <param name="Elem"></param>
        public void Add(HMapSubArea Elem)
        {
            AreaMaps.Add(Elem);
        }
    }
}
