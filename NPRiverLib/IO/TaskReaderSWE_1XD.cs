//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 03.03.2024 Потапов И.И.
//                форматы ( RvX ) для створа реки
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.APRiver_1XD.River1DSW;

    [Serializable]
    public class TaskReaderSWE_1XD : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        const string Ext_RvY = ".chg";
        public TaskReaderSWE_1XD()
        {
            extString = new List<string>() { Ext_RvY };
            SupportImport = true;
            SupportExport = true;
        }
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_RvY:
                    Read_1X(filename, ref river);
                    return;
            }
            DefaultRead(filename, ref river, testID);
        }
        /// <summary>
        /// Сохраняем сетку на диск речной формат данных 1DX для створа
        /// </summary>
        /// <param name="mesh">Экземпляр интерфейса<see cref="IMesh" /> interface.</param>
        /// <param name="filename">Путь к файлу для сохранения.</param>
        public override void Write(IRiver river, string filename)
        {
            // расширение файла
            string FileEXT = Path.GetExtension(filename).ToLower();
            switch (FileEXT)
            {
                case Ext_RvY:
                    Write_1X(river, filename);
                    break;
            }
        }
        public void Write_1X(IRiver river, string filename)
        {
            return;
        }
        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Read_1X(string filename, ref IRiver river)
        {
           
            if (IsSupported(filename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        ARiverSWE1XD river_1XD = null;
                        if (river as RiverSWEStatic1XD != null)
                        {
                            river_1XD = river as RiverSWEStatic1XD;
                        }
                        else
                            if (river as RiverSWE_KGD1XD != null)
                            {
                                river_1XD = river as RiverSWE_KGD1XD;
                            }
                        if (river_1XD == null)
                            throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
                        var value = LOG.GetInt(file.ReadLine());  // Версия
                        if (value != 1)
                            throw new Exception("Не согласованность версий");
                        //RiverSWEParams1XD p = new RiverSWEParams1XD();
                        river_1XD.LoadData(file);
                        file.Close();
                        river = river_1XD;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Info(ex.Message);
                }
            }
            else
                throw new NotSupportedException("Could not load '" + filename + "' file.");
        }

        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void DefaultRead(string filename, ref IRiver river, uint testID)
        {
            ARiverSWE1XD river_1XD = null;
            if (river as RiverSWEStatic1XD != null)
            {
                river_1XD = river as RiverSWEStatic1XD;
            }
            else
                if (river as RiverSWE_KGD1XD != null)
            {
                river_1XD = river as RiverSWE_KGD1XD;
            }
            if (river_1XD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");

            river_1XD.DefaultCalculationDomain(testID);
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTestsName()
        {
            List<string> strings = new List<string>();
            strings.Add("Поток в канале с плоским дном");
            strings.Add("Поток в канале с прорезью на дне");
            strings.Add("Поток в канале с дамбою на дне");
            return strings;
        }
    }
}
