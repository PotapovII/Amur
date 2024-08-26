//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                      07 04 2016 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Контейнер кривых
    /// </summary>
    public class GData
    {
        /// <summary>
        /// Список кривых 
        /// </summary>
        public List<GCurve> curves = new List<GCurve>();
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="e"></param>
        public void Add(GCurve e) { curves.Add(e); }
        /// <summary>
        /// Очистка контейнера
        /// </summary>
        public void Clear() { curves.Clear(); }
        /// <summary>
        /// Получение точки мини-макс для аргумента всех кривых
        /// в параметре Х которой хранится минимум аргумента для всех кривых
        /// в параметре Y которой хранится максимум аргумента для всех кривых
        /// </summary>
        /// <returns>точка мини-макс </returns>
        public GPoint MinMax_X()
        {
            if (curves.Count > 0)
            {
                GPoint mm = curves[0].MinMax_X();
                bool fl = true;
                foreach (GCurve c in curves)
                {
                    if (c.Check == true)
                    {
                        if (fl == true)
                        {
                            mm = c.MinMax_X();
                            fl = false;
                        }
                        else
                        {

                            GPoint cmm = c.MinMax_X();
                            if (mm.X > cmm.X) mm.X = cmm.X;
                            if (mm.Y < cmm.Y) mm.Y = cmm.Y;
                        }
                    }
                }
                return mm;
            }
            return new GPoint(0, 0);
        }
        /// <summary>
        /// Получение точки мини-макс для значения функции всех кривых
        /// в параметре Х которой хранится минимум значения функции для всех кривых
        /// в параметре Y которой хранится максимум значения функции для всех кривых
        /// </summary>
        /// <returns>точка мини-макс </returns>
        public GPoint MinMax_Y()
        {
            if (curves.Count > 0)
            {
                GPoint mm = curves[0].MinMax_Y();
                bool fl = true;
                foreach (GCurve c in curves)
                {
                    if (c.Check == true)
                    {
                        if (fl == true)
                        {
                            mm = c.MinMax_Y();
                            fl = false;
                        }
                        else
                        {

                            GPoint cmm = c.MinMax_Y();
                            if (mm.X > cmm.X) mm.X = cmm.X;
                            if (mm.Y < cmm.Y) mm.Y = cmm.Y;
                        }
                    }
                }
                return mm;
            }
            return new GPoint(0, 0);
        }
    }
}
