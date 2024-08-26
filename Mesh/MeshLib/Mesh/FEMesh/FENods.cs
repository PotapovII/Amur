//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//---------------------------------------------------------------------------
//  кодировка HNodFE (последняя правка) 14.03.2001 Потапов И.И. (c++)
//  перенос  HNodFE => FENods :   23.01.2022 Потапов И.И. (с++ => c#)
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// ОО: Узел произвольной КЭ сетки
    /// </summary>
    [Serializable]
    public class FENods : IFENods
    {
        /// <summary>
        /// номер узла
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Метка границы 
        /// </summary>
        public int MarkBC { get; set; }
        public FENods()
        {
            ID = 0;
            MarkBC = 0;
        }
        public FENods(IFENods n)
        {
            ID = n.ID;
            MarkBC = n.MarkBC;
        }
        public FENods(int ID, int MarkBC = 0)
        {
            this.ID = ID;
            this.MarkBC = MarkBC;
        }
        /// <summary>
        /// Вывод простого объекта в строку
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ID.ToString() + " " + MarkBC.ToString();
        }
        /// <summary>
        /// Чтение простого объекта из строки
        /// </summary>
        public IFENods Parse(string line)
        {
            try
            {
                string[] lines = line.Split(' ');
                return new FENods(int.Parse(lines[0]), int.Parse(lines[1]));
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
                return new FENods();
            }
        }
    }

}
