//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//               кодировка : 18.09.21 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using CommonLib;
    using CommonLib.IO;

    /// <summary>
    /// Имена файлов с данными для задачи гидродинамики
    /// Используется в интерфейсе IRiver
    /// </summary>
    [Serializable]
    public class TaskFileNames : ITaskFileNames
    {
        /// <summary>
        /// Наименование файла параметров руслового процесса
        /// </summary>
        public string NameCPParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для расчета донных деформаций
        /// </summary>
        public string NameBLParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для расчета речных потоков
        /// </summary>
        public string NameRSParams { get; set; }
        /// <summary>
        /// Наименование файла параметров для генерации сетки для расчета речных потоков
        /// </summary>
        public string NameMeshParams { get; set; }
        /// <summary>
        /// Наименование файла данных для расчета речных потоков
        /// рельеф дна/расходы/...
        /// </summary>
        public string NameRData { get; set; }
        /// <summary>
        /// расширение задачи
        /// </summary>
        public string NameEXT { get; set; }
        /// <summary>
        /// расширение задачи для импорта
        /// </summary>
        public string NameEXTImport { get; set; }
        /// <summary>
        /// расширение задачи для экспорта
        /// </summary>
        public string NameEXTExport { get; set; }

        public TaskFileNames()
        {
            NameCPParams   = "NameCPParams.txt";
            NameBLParams   = "NameBLParams.txt";
            NameRSParams   = "NameRSParams.txt";
            NameMeshParams = "NameMeshParams.txt";
            NameRData = "NameRData.txt";
            NameEXT = "(*.tsk)|*.tsk|";
            NameEXTImport = "";
            NameEXTImport = "";
        }
        public TaskFileNames(ITaskFileNames p)
        {
            NameCPParams = p.NameCPParams;
            NameBLParams = p.NameBLParams;
            NameRSParams = p.NameRSParams;
            NameMeshParams = p.NameMeshParams;
            NameRData = p.NameRData;
            NameEXT = p.NameEXT;
            NameEXTImport = p.NameEXTImport;
            NameEXTExport = p.NameEXTExport;
        }
        
        public TaskFileNames(IOFormater<IRiver> loader)
        {
            NameEXT = "(*.tsk)|*.tsk|";
            if (loader.SupportExport == true)
                NameEXTImport = loader.FilterLD;
            if (loader.SupportImport == true)
                NameEXTExport = loader.FilterSD;
        }
    }

}
