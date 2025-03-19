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
    ///  ОО: Определение класса ARiverEmpty1XD - заглушки для задачи 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
     public class ARiverEmpty1XD : ARiverBaseEmpty
    {
        public ARiverEmpty1XD() : base(TypeTask.streamX1D)
        {
            name = "заглушки для задачи гидродинамики 1XD)";
            Version = "ARiverEmpty1XD 25.07.2024";
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
            return new ARiverEmpty1XD();
        }
        /// <summary>
        /// Получить шероховатость дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void GetRoughness(ref double[] Roughness)
        {
            Roughness = null;
        }

    }
}
