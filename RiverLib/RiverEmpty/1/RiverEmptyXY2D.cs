//---------------------------------------------------------------------------
//                          ПРОЕКТ  "River"
//              создано  :   17.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using CommonLib;
    using RiverLib.IO;
    using CommonLib.IO;
    /// <summary>
    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class RiverEmptyXY2D : ARiverEmpty
    {
        public override ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames()
            {
                NameCPParams = "NameCPParams.txt",
                NameBLParams = "NameBLParams.txt",
                NameRSParams = "NameRSParams.txt",
                NameRData = "NameRData.txt",
                //NameEXT = "(*.tsk)|*.tsk|",
                //NameEXTImport = "(*.txt)|*.txt|"
            };
            return fn;
        }
        public override IMesh BedMesh() 
        { 
            return mesh; 
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverEmptyXY2D(new RiverStreamParams());
        }
        public RiverEmptyXY2D(RiverStreamParams p) : base(p)
        {
            _typeTask = TypeTask.streamXY2D;
        }

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            sp.Add("Напряжение ", tauX, tauY);
            sp.Add("Напряжение tauX", tauX);
            sp.Add("Напряжение tauY", tauY);
            sp.Add("Отметки дна", zeta0);
            sp.Add("Придонное давление", P);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new RiverFormatReader2DTri();
        }
    }
}
