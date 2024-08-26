//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 04.11.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderEditLib
{
    using System.Collections.Generic;
    using System.Drawing;
    /// <summary>
    /// Класс для работы в контрола режиме графического редактора
    /// </summary>
    public class OptionsEdit
    {
        /// <summary>
        /// Список контура
        /// </summary>
        public List<PointF> points { get; set; }
        /// <summary>
        /// координаты точки первого клика
        /// </summary>
        public PointF Pa { get; set; }
        /// <summary>
        /// координаты точки второго клика
        /// </summary>
        public PointF Pb { get; set; }
        /// <summary>
        /// Счетчик кликов мыши
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// Обглвление масштаба
        /// </summary>
        public bool ckScaleUpdate { get; set; } 
        /// <summary>
        /// 
        /// </summary>
        public OptionsEdit()
        {
            points = new List<PointF>();
        }
        public OptionsEdit(OptionsEdit o)
        {
            Pa = o.Pa;
            Pb = o.Pb;
            index = o.index;
            points = o.points;
        }
        /// <summary>
        /// добавить точку при щелчке мыши
        /// </summary>
        /// <param name="e"></param>
        public void Add(PointF e)
        {
            if (index == 0)
                Pa = e;
            if (index == 1)
                Pb = e;
            points.Add(e);
            index++;
        }
        /// <summary>
        /// Сбросить состояние
        /// </summary>
        public void Clear()
        {
            index = 0;
            points.Clear();
        }
    }
}
