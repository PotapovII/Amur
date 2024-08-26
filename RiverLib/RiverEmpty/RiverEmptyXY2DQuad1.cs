////---------------------------------------------------------------------------
////                          ПРОЕКТ  "River"
////              создано  :   30.06.2022 Потапов И.И.
////---------------------------------------------------------------------------
//namespace RiverLib
//{
//    using System;
//    using CommonLib;
//    using RiverLib.IO;
//    using CommonLib.IO;
//    /// <summary>
//    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
//    ///  расчета полей скорости и напряжений в речном потоке
//    /// </summary>
//    [Serializable]
//    public class RiverEmptyXY2DQuad : ARiverEmpty
//    {
//        public override ITaskFileNames taskFileNemes()
//        {
//            ITaskFileNames fn = new TaskFileNames();
//            fn.NameCPParams = "NameCPParams.txt";
//            fn.NameBLParams = "NameBLParams.txt";
//            fn.NameRSParams = "NameRSParams.txt";
//            fn.NameRData = "NameRData.txt";
//            fn.NameEXT = "(*.tsk)|*.tsk|";
//            fn.NameEXTImport = "(*.txt)|*.txt|";
//            return fn;
//        }
//        /// <summary>
//        /// Наименование задачи
//        /// </summary>
//        public override string Name => "прокси гидрадинамика 3 => 4";
//        public override IMesh BedMesh() { return mesh; }
//        /// <summary>
//        /// Создает экземпляр класса
//        /// </summary>
//        /// <returns></returns>
//        public override IRiver Clone()
//        {
//            return new RiverEmptyXY2DQuad(new RiverStreamParams());
//        }
//        public RiverEmptyXY2DQuad(RiverStreamParams p) : base(p)
//        {
//            _typeTask = TypeTask.streamXY2D;
//        }
//        /// <summary>
//        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
//        /// </summary>
//        /// <param name="sp">контейнер данных</param>
//        public override void AddMeshPolesForGraphics(ISavePoint sp)
//        {
//            sp.Add("Напряжение ", tauX, tauY);
//            sp.Add("Напряжение tauX", tauX);
//            sp.Add("Напряжение tauY", tauY);
//            sp.Add("Отметки дна", zeta0);
//            sp.Add("Придонное давление", P);
//        }
//        /// <summary>
//        /// Создает экземпляр класса конвертера
//        /// </summary>
//        /// <returns></returns>
//        public override IOFormater<IRiver> GetFormater()
//        {
//            return new RiverFormatReader2DTri();
//        }
//    }
//}
