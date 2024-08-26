//---------------------------------------------------------------------------
//                          ПРОЕКТ  "River"
//              создано  :   17.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using CommonLib;
    using System;
    using RiverLib.IO;
    using CommonLib.IO;
    /// <summary>
    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class RiverEmptyX1D : ARiverEmpty
    {
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverEmptyX1D(new RiverStreamParams());
        }
        public RiverEmptyX1D(RiverStreamParams p) : base(p)
        {
            _typeTask = TypeTask.streamX1D;
        }
    }
}
