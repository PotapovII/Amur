//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 03.03.2024 Потапов И.И.
//                форматы ( crf rvx ) для створа реки
//---------------------------------------------------------------------------
namespace NPRiverLib.IO
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using System.Collections.Generic;


    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Function;

    using NPRiverLib.APRiver1YD;
    using NPRiverLib.APRiver1YD.Params;
    using NPRiverLib.IO._1XD.Tests;

    [Serializable]
    public class TaskReader1YD_RvY : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        public const string Ext_RvY = ".rvy";
        public const string Ext_Crf = ".crf";
        public TaskReader1YD_RvY()
        {
            extString = new List<string>() { Ext_RvY, Ext_Crf };
            SupportImport = true;
            SupportExport = true;
        }
        public override string Read(string filename, ref IRiver river, uint testID = 0)
        {
            // расширение файла
            if (testID == 0) // Загрузка задачи по умолчанию
            {
                string FileEXT = Path.GetExtension(filename).ToLower();
                switch (FileEXT)
                {
                    case Ext_RvY:
                        Read_RvY(filename, ref river);
                        return river.Name + ": " + filename;
                    case Ext_Crf:
                        Read_Crf(filename, ref river);
                        return river.Name + ": " + filename;
                }
            }
            else
            {
                if (testID == 1) // Загрузка задачи c поиском 
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    string filter = "(*" + Ext_Crf + ")|*" + Ext_Crf + "| ";
                    filter += " All files (*.*)|*.*";
                    ofd.Filter = filter;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        Read_Crf(ofd.FileName, ref river);
                        return river.Name + ": " + filename;
                    }
                }
                else
                {
                    Test_CS_1YD.GetTest(ref river, testID);
                    return river.Name;
                }
            }
            return "Тестовая задача не выбрана";
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
                    Write_RvY(river, filename);
                    break;
                case Ext_Crf:
                    Write_Crf(river, filename);
                    break;
            }
        }
        public void Write_Crf(IRiver river, string filename)
        {
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно сохранить выбранный объект задачи в формате *.RvY, river_1YD == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    river_1YD.csData.Save(file);
                    //IDigFunction[] crossFunctions = river_1YD.csData.crossFunctions;
                    //for (int i = 0; i < crossFunctions.Length; i++)
                    //    crossFunctions[i].Save(file);
                    file.Close();
                }
                river = river_1YD;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }
        public void Write_RvY(IRiver river, string filename)
        {
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно сохранить выбранный объект задачи в формате *.RvY, river_1YD == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    river_1YD.csData.Save(file);
                    //IDigFunction[] crossFunctions = river_1YD.csData.crossFunctions;
                    //for (int i=0; i< crossFunctions.Length; i++)
                    //    crossFunctions[i].Save(file);
                    //file.Close();
                }
                river = river_1YD;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
            }
        }
        /// <summary>
        /// Прочитать файл, речной формат данных 1DX для створа
        /// </summary>
        /// <param name="filename">Путь к файлу для чтения..</param>
        /// <returns>Экземпляр интерфейса<see cref="IMesh" /> interface.</returns>
        public void Read_RvY(string filename, ref IRiver river)
        {
            filename = WR.path + filename;
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи в формате *.RvY, river_1YD == null");
            if (IsSupported(filename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(filename))
                    {
                        if(river_1YD as TriSroSecRiverTask1YD !=null)
                            river_1YD = river as TriSroSecRiverTask1YD;
                        //if (river_1YD as TriSecRiverTask1YD != null)
                        //    river_1YD = river as TriSecRiverTask1YD;
                        if (river_1YD as CSRiver1_1YD_old != null)
                            river_1YD = river as CSRiver1_1YD_old;
                        if (river_1YD as CSRiver3_1YD != null)
                            river_1YD = river as CSRiver3_1YD;
                        river_1YD.LoadData(file);
                        file.Close();
                    }
                    river = river_1YD;
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
        public void Read_Crf(string filename, ref IRiver river)
        {
            string pfilename;
            if (filename[1] != ':')
                pfilename = WR.path + filename;
            else
                pfilename = filename;
            APRiverFEM1YD river_1YD = river as APRiverFEM1YD;
            if (river_1YD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            if (IsSupported(pfilename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(pfilename))
                    {
                        // создание и чтение свойств задачи                
                        RSCrossParams p = new RSCrossParams();
                        p.Load(file);
                        river_1YD.SetParams(p);
                        /// Данные о створе
                        CrossSectionData csData = new CrossSectionData();
                        csData.Load(file);
                        river_1YD.LoadData(csData.crossFunctions);
                        file.Close();
                    }
                    river = river_1YD;
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
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTestsName() => Test_CS_1YD.GetTestsName();   
    }
}
