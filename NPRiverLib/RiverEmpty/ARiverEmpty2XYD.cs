//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.RiverEmpty
{
    using System;
    using CommonLib;
    using CommonLib.IO;

    /// <summary>
    ///  ОО: Определение класса ARiverEmpty2XYD - заглушки для задачи 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
     public class ARiverEmpty2XYD : ARiverBaseEmpty
    {
        public ARiverEmpty2XYD() : base(TypeTask.streamXY2D)
        {
            name = "заглушки для задачи гидродинамики 2XYD)";
            Version = "ARiverEmpty2XYD 25.07.2024";
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new ProxyTaskFormat<IRiver>();
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new ARiverEmpty2XYD();
        }
    }
}
