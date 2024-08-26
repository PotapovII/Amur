//---------------------------------------------------------------------------
//                          ПРОЕКТ  "River"
//              создано  :   17.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using CommonLib;
    using System;
    /// <summary>
    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class RiverEmptyY1D : ARiverEmpty
    {
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverEmptyY1D(new RiverStreamParams());
        }
        public RiverEmptyY1D(RiverStreamParams p) : base(p)
        {
            _typeTask = TypeTask.streamY1D;
        }
    }
}
