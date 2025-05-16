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
    
    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Function;

    using MemLogLib;
    using GeometryLib;
    using NPRiverLib.APRiver_1XD;
    using NPRiverLib.IO._1XD.Tests;
    using System.Windows.Forms;

    [Serializable]
    public class TaskReaderFEM_1XD : ATaskFormat<IRiver>
    {
        /// <summary>
        /// Поддержка внешних форматов загрузки
        /// </summary>
        public override bool SupportImport { get; }
        /// <summary>
        /// Поддержка внешних форматов сохранения
        /// </summary>
        public override bool SupportExport { get; }

        const string Ext_prf = ".prf";
        public TaskReaderFEM_1XD()
        {
            extString = new List<string>() { Ext_prf };
            SupportImport = true;
            SupportExport = true;
        }
        public override void Read(string filename, ref IRiver river, uint testID = 0)
        {
            // расширение файла
            if (testID == 0) // Загрузка по умолчанию
            {
                string FileEXT = Path.GetExtension(filename).ToLower();
                switch (FileEXT)
                {
                    case Ext_prf:
                        Read_prf(filename, ref river);
                        break;
                }
            }
            else
            if (testID == 1) // Загрузка по умолчанию
            {
                OpenFileDialog ofd = new OpenFileDialog();
                string filter = "(*" + Ext_prf + ")|*" + Ext_prf + "| ";
                filter += " All files (*.*)|*.*";
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                    Read_prf(ofd.FileName, ref river);
                return;
            }
            else
            {
                Test_Cannal_1XD.GetTest(ref river, testID);
            }
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
                case Ext_prf:
                    Write_prf(river, filename);
                    break;
            }
        }
        public void Write_prf(IRiver river, string filename)
        {
            TriFEMRiver_1XD river_1XD = river as TriFEMRiver_1XD;
            if (river_1XD == null)
                throw new NotSupportedException("Не возможно сохранить выбранный объект задачи в формате *.RvY, river_1YD == null");
            try
            {
                using (StreamWriter file = new StreamWriter(filename))
                {
                    IDigFunction[] BCVelosity = river_1XD.BCVelosity;
                    for (int i = 0; i < BCVelosity.Length; i++)
                        BCVelosity[i].Save(file);
                    file.Close();
                }
                river = river_1XD;
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
        public void Read_prf(string filename, ref IRiver river)
        {
            string pfilename;
            if (filename[1] != ':')
                pfilename = WR.path + filename;
            else
                pfilename = filename;
            TriFEMRiver_1XD river_1XD = river as TriFEMRiver_1XD;
            if (river_1XD == null)
                throw new NotSupportedException("Не возможно загрузить выбранный объект задачи river_1YD == null");
            if (IsSupported(pfilename) == true)
            {
                try
                {
                    using (StreamReader file = new StreamReader(pfilename))
                    {
                        // создание и чтение свойств задачи                
                        FEMParams_1XD p = new FEMParams_1XD();
                        p.Load(file);
                        river_1XD.SetParams(p);
                        // Нормальная скорость на WL
                        IDigFunction BCVelosityIN = new DigFunction();
                        // Радиальная скорость на WL
                        IDigFunction BCVelosityOUT = new DigFunction();
                        // геометрия дна
                        BCVelosityIN.Load(file);
                        // свободная поверхность
                        BCVelosityOUT.Load(file);
                        IDigFunction[] BCVelosity = new IDigFunction[2]
                        {
                            BCVelosityIN, BCVelosityOUT
                        };
                        river_1XD.LoadData(BCVelosity);
                        file.Close();
                    }
                    river = river_1XD;
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
        public override List<string> GetTestsName() => Test_Cannal_1XD.GetTestsName();
    }
}
