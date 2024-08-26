//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 30.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 12-19.06.2026 Потапов И.И.
//    1. изменера структура класса, убрано наследование параметров задачи
//    2. Разделена загрузка
//      2.1 параметров задачи по умолчанию
//      2.2 параметров задачи из файла
//      2.3 загрузка прочих данных задачи 
//       (геометрии области, расчетной области и функци поведения)
//    
//---------------------------------------------------------------------------

namespace ChannelProcessLib
{
    using System;
    using System.IO;
    using System.ComponentModel;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using GeometryLib;

    using CommonLib;
    using CommonLib.ChannelProcess;
    using CommonLib.IO;

    [Serializable]
    public enum ChannelProcessError
    {
        /// <summary>
        /// ошибок нет
        /// </summary>
        notError = 0,
        /// <summary>
        /// ошибка при решении задачи гидродинамики
        /// </summary>
        riverError,
        /// <summary>
        /// ошибка при герерации секи для задачи гидродинамики
        /// </summary>
        riverMeshError,
        /// <summary>
        /// ошибка при решении задачи донных изменений
        /// </summary>
        bedError,
        /// <summary>
        /// ошибка чтения/записи
        /// </summary>
        riverIO,
    }
    [Serializable]
    public enum ChannelProcessState
    {
        /// <summary>
        /// создание русловой задачи
        /// </summary>
        NoCreateState = 0,
        /// <summary>
        /// создание русловой задачи
        /// </summary>
        CreateState = 1,
        /// <summary>
        /// создание/загрузка расчетной области - сетки задачи
        /// </summary>
        WorkAreaState = 2,
        /// <summary>
        /// создание/загрузка расчетных полей задачи
        /// </summary>
        FieldsState = 3
    }


    /// <summary>
    /// ОО: Класс для управления решением задач моделирующих русловые процессы
    /// </summary>
    [Serializable]
    public class ChannelProcessPro : IPropertyTask
    {
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики
        /// Используется в интерфейсе IRiver
        /// </summary>
        ITaskFileNames fileTaskNames;
        ///// <summary>
        ///// метод свойств задачи определяет 
        ///// </summary>
        ///// <param name="p"></param>
        public object GetParams() => ps.GetParams();
        ///// <summary>
        ///// Установка свойств задачи
        ///// </summary>
        ///// <param name="p"></param>
        public void SetParams(object p) => ps.SetParams(p);
        ///// <summary>
        ///// Чтение параметров задачи из файла
        ///// </summary>
        ///// <param name="p"></param>
        public void LoadParams(string fileName) => ps.LoadParams(fileName);
        /// <summary>
        /// Свойства задачи
        /// </summary>
        public CProcParams ps = new CProcParams();
        /// <summary>
        /// Состояние задачи
        /// </summary>
        ChannelProcessState cProcessState;
        #region Свойства
        /// <summary>
        /// Ошибки задачи
        /// </summary>
        public ChannelProcessError channelProcessError { get; set; }
        public string GetError()
        {
            switch (channelProcessError)
            {
                case ChannelProcessError.riverIO:
                    return "формат данных не корректен";
                case ChannelProcessError.riverError:
                    return "ошибка при решении задачи гидродинамики";
                case ChannelProcessError.riverMeshError:
                    return "ошибка при генерации сетки для задачи гидродинамики";
                case ChannelProcessError.bedError:
                    return "ошибка при выислении донных изменений";
                default:
                    return "расчет устойив";
            }
        }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        [DisplayName("Имя задачи")]
        [Category("Задача")]
        public string Name
        {
            get => "Русловой процесс (" +
                  riverTask != null ? riverTask.Name : "" + " : "
                + bedLoadTask != null ? bedLoadTask.Name : "" + ")";
        }
        /// <summary>
        /// гидродинамическая задача- с-во
        /// </summary>
        public IRiver RiverTask() => riverTask;
        /// <summary>
        /// гидродинамическая задача
        /// </summary>
        protected IRiver riverTask = null;
        /// <summary>
        /// задача донных деформаций
        /// </summary>
        public IBedLoadTask BedLoadTask() => bedLoadTask;
        /// <summary>
        /// задача донных деформаций
        /// </summary>
        protected IBedLoadTask bedLoadTask = null;
        #endregion
        /// <summary>
        /// Ссылка на метод синхронизации (ф.о.в.)
        /// </summary>
        [NonSerialized]
        public SendParam sendParam = null;
        /// <summary>
        /// Ссылка на метод синхронизации (ф.о.в.)
        /// </summary>
        [NonSerialized]
        public SendParam sendTimeParam = null;
        /// <summary>
        /// отметки дна
        /// </summary>
        public double[] Zeta = null;
        /// <summary>
        /// придонное касательное напряжение
        /// </summary>
        public double[] tau = null;
        /// <summary>
        /// придонное касательное напряжение по х
        /// </summary>
        public double[] tauX = null;
        /// <summary>
        /// придонное касательное напряжение по y
        /// </summary>
        public double[] tauY = null;
        /// <summary>
        /// придонное давление
        /// </summary>
        public double[] P = null;
        /// <summary>
        /// Поля определяемые из контекста задачи (концентрация наносов и т.д.)
        /// </summary>
        public double[][] CS = null;
        /// <summary>
        /// Эволюция кривых
        /// </summary>
        [NonSerialized]
        public ISavePoint sps = null;
        /// <summary>
        /// Журнал задачи
        /// </summary>
        [NonSerialized]
        ILogger<IMessageItem> logger = Logger.Instance;
        /// <summary>
        /// конструктор по умолчанию для чтения свойств задачи по умолчанию
        /// </summary>
        public ChannelProcessPro()
        {
            cProcessState = ChannelProcessState.NoCreateState;
            logger = Logger.Instance;
        }
        /// <summary>
        /// конструктор с параметрами
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="blt"></param>
        /// <param name="taskNameFile">имя файла для загрузки костомных параметров задачи</param>
        /// <exception cref="Exception"></exception>

        /// <summary>
        /// конструктор с параметрами
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="blt"></param>
        /// <param name="newTask">создана новая задача или нет</param>
        /// <param name="taskNameFile">имя файла для загрузки костомных параметров задачи</param>
        /// <exception cref="Exception"></exception>
        public ChannelProcessPro(IRiver rt, IBedLoadTask blt, string taskNameFileDefault = "") :
            this()
        {
            logger = Logger.Instance;
            riverTask = rt;
            bedLoadTask = blt;
            if (rt.typeTask != blt.typeTask)
                throw new Exception("русловая задача (" +
                riverTask.Name + " : " + bedLoadTask.Name + ") не совместима!");
            channelProcessError = ChannelProcessError.notError;
            cProcessState = ChannelProcessState.CreateState;
            // Синхронизация шага по времени
            TimeSynchronization();
            if (riverTask.TaskReady() == false)
            {
                // При создании новой задачи загрузить параметры по умолчанию
                LoadAllParams(taskNameFileDefault);
            }
            else
            {
                // при готовности гидрадинамической задачи и наличии размыва создать связь
                if (riverTask.TaskReady() == true && ps.bedErosion != EBedErosion.NoBedErosion)
                    BedLink();
            }
        }
        /// <summary>
        /// Загрузка только параметров задачи
        /// </summary>
        public void LoadAllParams(string taskNameFileDefault = "")
        {
            try
            {
                // получаем имена файлов для данных по умолчанию
                fileTaskNames = riverTask.taskFileNemes();
                fileTaskNames.NameBLParams = bedLoadTask.NameBLParams();
                if (taskNameFileDefault == "")
                {
                    Logger.Instance.Info("Используется конфигурация по умолчанию ChannelProcess.LoadTask()");
                    logger.Info("Используется конфигурация по умолчанию ChannelProcess.LoadTask()");
                }
                else
                {
                    using (StreamReader file = new StreamReader(taskNameFileDefault))
                    {
                        if (file == null)
                        {
                            Logger.Instance.Info("Файл конфигурации не обнаружен ChannelProcess.LoadTask()");
                            Logger.Instance.Info("будет используется конфигурация по умолчанию");
                            logger.Warning("Файл конфигурации не обнаружен", "ChannelProcess.LoadTask()");
                            logger.Info("будет используется конфигурация по умолчанию");
                        }
                        else
                        {
                            fileTaskNames.NameCPParams = file.ReadLine();
                            fileTaskNames.NameBLParams = file.ReadLine();
                            fileTaskNames.NameRSParams = file.ReadLine();
                            fileTaskNames.NameRData = file.ReadLine();
                        }
                    }
                }
                LoadParams(fileTaskNames.NameCPParams);
                riverTask.LoadParams(fileTaskNames.NameRSParams);
                bedLoadTask.LoadParams(fileTaskNames.NameBLParams);
                // создание задачи с параметрами определенными не по умолчанию,
                // а загруженными из файлов конфигураций
                cProcessState = ChannelProcessState.CreateState;
            }
            catch (Exception ep)
            {
                channelProcessError = ChannelProcessError.riverIO;
                Logger.Instance.Info("Ошибка при загрузке параметров задачи: ChannelProcess.LoadTask()");
                Logger.Instance.Exception(ep);
                logger.Error("Ошибка при загрузке параметров задачи", "ChannelProcess.LoadTask()");
                logger.Exception(ep);
                // создание задачи с параметрами определенными по умолчанию
                cProcessState = ChannelProcessState.NoCreateState;
            }
        }
        /// <summary>
        /// Загрузка расчетной области по умолчанию
        /// </summary>
        public void LoadComputationalDomainDefault(uint testTaskID)
        {
            try
            {
                if (riverTask.TaskReady() == false)
                {
                    ITaskFileNames fn = riverTask.taskFileNemes();
                    IOFormater<IRiver> loader = riverTask.GetFormater();
                    // загрузка уровней дна, расходов, ...
                    loader.Read(fn.NameRData, ref riverTask, testTaskID);
                    riverTask.GetZeta(ref Zeta);
                }
                IMesh mesh = riverTask.Mesh();
                if (mesh?.CountKnots == 0)
                    throw new Exception("Задача по умлчанию не загружена");
                if (ps.bedErosion != EBedErosion.NoBedErosion)
                    BedLink();
                if (riverTask.TaskReady() == false)
                    throw new Exception("Задача гидродинамики не готова к расчету");
                cProcessState = ChannelProcessState.WorkAreaState;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info("Ошибка при загрузке расчетной области задачи: ChannelProcess.LoadComputationalDomainDefault()");
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// передача данных о сетке и дне в задачу донных деформаций
        /// вызов необходим при решении задач донных деформаций
        /// </summary>
        public void BedLink()
        {
            riverTask.GetZeta(ref Zeta);
            // передача данных о сетке и дне
            IMesh BedMesh = riverTask.BedMesh();
            bedLoadTask.SetTask(BedMesh, Zeta, riverTask.BoundCondition());
            if (bedLoadTask.TaskReady() == true)
                logger.Info("Метод загрузки данных завершен успешно");
        }
        /// <summary>
        /// Расчет русловых процессов
        /// </summary>
        public double SolverStep()
        {
            try
            {
                logger = Logger.Instance;
                if (sps == null) sps = new SavePoint();
                // =========== ГИДРОДИНАМИКА ========
                channelProcessError = ChannelProcessError.riverMeshError;
                riverTask.time = ps.time;
                // генерация сетки для задачи гидродинамики
                riverTask.SetZeta(Zeta, ps.bedErosion);
                channelProcessError = ChannelProcessError.riverError;
                // расчет гидрадинамики  (скоростей потока)
                riverTask.SolverStep();
                ps.dtime = riverTask.dtime;
                IMesh bedMesh = null;
                // расчет донных деформаций
                if (ps.bedErosion != EBedErosion.NoBedErosion)
                {
                    channelProcessError = ChannelProcessError.bedError;
                    bedLoadTask.dtime = ps.dtime;
                    // расчет  придонных касательных напряжений на дне
                    riverTask.GetTau(ref tauX, ref tauY, ref P, ref CS);
                    // ========== БИБЛИОТЕКА === BLLib === 
                    if (bedLoadTask.TaskReady() == false)
                    {
                        riverTask.GetZeta(ref Zeta);
                        bedMesh = riverTask.BedMesh();
                        bedLoadTask.SetTask(riverTask.BedMesh(), Zeta, riverTask.BoundCondition());
                    }
                    if (bedLoadTask.TaskReady() == true)
                    {
                        bedLoadTask.time = ps.time;
                        bedLoadTask.CalkZetaFDM(ref Zeta, tauX, tauY, P, CS);
                    }
                    else
                    {
                        if (bedMesh != null)
                            Logger.Instance.Info("не загружена сетка для дна, возможно задача гидрадинамики требует загрузки");
                        else
                        {
                            Logger.Instance.Info("ошибка! не активирована задача донных деформаций: ChannelProcess.SolverStep()");
                            logger.Error("ошибка! не активирована задача донных деформаций", "ChannelProcess.SolverStep()");
                        }
                    }
                }
                // текущее время
                ps.time += ps.dtime;
                // сохранение результатов расчета
                SaveData();
                // шаг расчета выполнен
                channelProcessError = ChannelProcessError.notError;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                logger.Exception(ex);
            }
            return ps.time;
        }
        /// <summary>
        /// Создание точки сохранения данных
        /// </summary>
        /// <returns></returns>
        public ISavePoint GetSavePoint()
        {
            IMesh mesh = riverTask.Mesh();
            if (mesh != null)
            {
                SavePoint sp = new SavePoint();
                sp.SetSavePoint(ps.time, riverTask.Mesh());
                riverTask.AddMeshPolesForGraphics(sp);
                double[] x = riverTask.Mesh().GetCoords(0);
                double[] y = riverTask.Mesh().GetCoords(1);
                sp.Add(new Field1D("Координаты х", x));
                sp.Add(new Field1D("Координаты y", y));
                if (ps.bedErosion != EBedErosion.NoBedErosion)
                    bedLoadTask.AddMeshPolesForGraphics(sp);
                return sp;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Синхронизация шага по времени
        /// </summary>
        public void TimeSynchronization()
        {
            riverTask.dtime = ps.dtime;
            bedLoadTask.dtime = ps.dtime;
        }
        /// <summary>
        /// Метод буфферезации кадра решаемой задачи
        /// </summary>
        protected void SaveData()
        {
            if (MEM.Equals(ps.time, ps.dtime) == true)
                ps.count = 1;
            if (ps.time >= ps.count * ps.timeSavePeriod || MEM.Equals(ps.time, ps.dtime) == true)
            {
                // текущий кадр
                ISavePoint sp = GetSavePoint();
                // отсылка кадра на обновление постпроцессору
                if (sendParam != null)
                    sendParam((SavePoint)sp);
                // сборка кривых со всех кадров
                if (sendTimeParam != null && sps != null)
                {
                    // кривые текущего кадра
                    List<GraphicsCurve> curves = ((GraphicsData)sp.graphicsData).curves;
                    // очистка от старых эволюционных кривых (функций от времени)
                    sps.ClearСurve(TypeGraphicsCurve.TimeCurve);
                    foreach (var c in curves)
                    {
                        string tm = (((int)(1000 * ps.time)) / 1000).ToString() + "#";
                        //string tm = time.ToString("F4") + " ";
                        c.Name = tm + c.Name;
                        sps.AddCurve(c);
                    }
                    sendTimeParam((SavePoint)sps);
                }
                // сохранение кадра в файл
                if (ps.flagSP == true)
                {
                    string NameSP = "Сохранение кадра расчета, t = " + ps.time.ToString() + " секунд ";
                    sp.SerializableSavePoint(sp, NameSP);
                    Logger.Instance.Info(NameSP);
                    logger.Info(NameSP);
                }
                if (MEM.Equals(ps.time, ps.dtime) != true)
                    ps.count++;
            }
        }
    }
}
