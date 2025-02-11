////---------------------------------------------------------------------------
////                   ПРОЕКТ  "Русловые процессы"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////              кодировка : 03.03.2024 Потапов И.И.
////                форматы ( RvX ) для створа реки
////---------------------------------------------------------------------------
//namespace NPRiverLib.IO
//{
//    using System;
//    using System.IO;
//    using System.Collections.Generic;

//    using MemLogLib;
//    using CommonLib;
//    using CommonLib.IO;
//    using NPRiverLib.APRiver1YD;
//    using NPRiverLib.APRiver1YD.Params;

//    [Serializable]
//    public class TaskReader1YD_Rose : ATaskFormat<IRiver>
//    {
//        /// <summary>
//        /// Поддержка внешних форматов загрузки
//        /// </summary>
//        public override bool SupportImport { get; }
//        /// <summary>
//        /// Поддержка внешних форматов сохранения
//        /// </summary>
//        public override bool SupportExport { get; }

//        const string Ext_RvY = ".rvy";
//        public TaskReader1YD_Rose()
//        {
//            extString = new List<string>() { Ext_RvY };
//            SupportImport = true;
//            SupportExport = true;
//        }
//        public override void Read(string filename, ref IRiver river, uint testID = 0)
//        {
//            // расширение файла
//            string FileEXT = Path.GetExtension(filename).ToLower();
//            switch (FileEXT)
//            {
//                case Ext_RvY:
//                    Read_RoseF(filename, ref river);
//                    return;
//            }
//            DefaultRead(filename, ref river);
//        }
//        /// <summary>
//        /// Сохраняем сетку на диск речной формат данных 1DX для створа
//        /// </summary>
//        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
//        /// <param name="filename">Путь к файлу для сохранения.</param>
//        public override void Write(IRiver river, string filename)
//        {
//            // расширение файла
//            string FileEXT = Path.GetExtension(filename).ToLower();
//            switch (FileEXT)
//            {
//                case Ext_RvY:
//                    Write_Rose(river, filename);
//                    break;
//            }
//        }
//        public void Write_Rose(IRiver river, string filename)
//        {
//            return;
//        }
//        /// <summary>
//        /// Прочитать файл, речной формат данных 1DX для створа
//        /// </summary>
//        /// <param name="filename">Путь к файлу для чтения..</param>
//        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
//        public void Read_RoseF(string filename, ref IRiver river)
//        {
//            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
//            if (river_1YD == null)
//                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
//            if (IsSupported(filename) == true)
//            {
//                try
//                {
//                    using (StreamReader file = new StreamReader(filename))
//                    {
//                        var value = LOG.GetInt(file.ReadLine());  // Версия
//                        if (value != 1)
//                            throw new Exception("Не согласованность версий");
//                        RSCrossParams p = new RSCrossParams();
//                        p.Load(file);
//                        river_1YD = new TriSroSecRiverTask1YD(p);
//                        river_1YD.LoadData(file);
//                        file.Close();
//                    }
//                    river = river_1YD;
//                }
//                catch (Exception ex)
//                {
//                    Logger.Instance.Info(ex.Message);
//                }
//            }
//            else
//                throw new NotSupportedException("Could not load '" + filename + "' file.");
//        }

//        /// <summary>
//        /// Прочитать файл, речной формат данных 1DX для створа
//        /// </summary>
//        /// <param name="filename">Путь к файлу для чтения..</param>
//        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
//        public void DefaultRead(string filename, ref IRiver river)
//        {
//            TriRoseRiverTask1YD river_1YD = river as TriRoseRiverTask1YD;
//            if (river_1YD == null)
//                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
//            river_1YD.DefaultCalculationDomain();
//        }
//    }
//}
