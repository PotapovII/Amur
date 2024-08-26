////---------------------------------------------------------------------------
////                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 30.08.2021 Потапов И.И.
////---------------------------------------------------------------------------
//namespace ChannelProcessLib
//{
//    using System;
//    using System.IO;
//    using System.ComponentModel;
//    using System.Collections.Generic;

//    using MeshLib;
//    using MemLogLib;
//    using GeometryLib;

//    using CommonLib;
//    using CommonLib.ChannelProcess;

//    [Serializable]
//    public enum ChannelProcessError
//    {
//        /// <summary>
//        /// ошибок нет
//        /// </summary>
//        notError = 0,
//        /// <summary>
//        /// ошибка при решении задачи гидродинамики
//        /// </summary>
//        riverError,
//        /// <summary>
//        /// ошибка при герерации секи для задачи гидродинамики
//        /// </summary>
//        riverMeshError,
//        /// <summary>
//        /// ошибка при решении задачи донных изменений
//        /// </summary>
//        bedError,
//        /// <summary>
//        /// ошибка чтения/записи
//        /// </summary>
//        riverIO,
//    }

//    /// <summary>
//    /// ОО: Класс для управления решением задач моделирующих русловые процессы
//    /// </summary>
//    [Serializable]
//    public class ChannelProcess : CProcParams, IPropertyTask
//    {
//        #region Свойства
//        public ChannelProcessError channelProcessError { get; set; }
//        public string GetError()
//        {
//            switch (channelProcessError)
//            {
//                case ChannelProcessError.riverIO:
//                    return "формат данных не корректен";
//                case ChannelProcessError.riverError:
//                    return "ошибка при решении задачи гидродинамики";
//                case ChannelProcessError.riverMeshError:
//                    return "ошибка при генерации сетки для задачи гидродинамики";
//                case ChannelProcessError.bedError:
//                    return "ошибка при выислении донных изменений";
//                default:
//                    return "расчет устойив";
//            }
//        }
//        /// <summary>
//        /// Наименование задачи
//        /// </summary>
//        [DisplayName("Имя задачи")]
//        [Category("Задача")]
//        public string Name
//        {
//            get => "Русловой процесс (" +
//                  riverTask != null ? riverTask.Name : "" + " : "
//                + bedLoadTask != null ? bedLoadTask.Name : "" + ")";
//        }
//        /// <summary>
//        /// гидродинамическая задача- с-во
//        /// </summary>
//        public IRiver RiverTask() => riverTask;
//        /// <summary>
//        /// гидродинамическая задача
//        /// </summary>
//        protected IRiver riverTask = null;
//        /// <summary>
//        /// задача донных деформаций
//        /// </summary>
//        public IBedLoadTask BedLoadTask() => bedLoadTask;
//        /// <summary>
//        /// задача донных деформаций
//        /// </summary>
//        protected IBedLoadTask bedLoadTask = null;
//        #endregion
//        /// <summary>
//        /// Ссылка на метод синхронизации (ф.о.в.)
//        /// </summary>
//        [NonSerialized]
//        public SendParam sendParam = null;
//        /// <summary>
//        /// Ссылка на метод синхронизации (ф.о.в.)
//        /// </summary>
//        [NonSerialized]
//        public SendParam sendTimeParam = null;
//        /// <summary>
//        /// отметки дна
//        /// </summary>
//        public double[] Zeta = null;
//        /// <summary>
//        /// придонное касательное напряжение
//        /// </summary>
//        public double[] tau = null;
//        /// <summary>
//        /// придонное касательное напряжение по х
//        /// </summary>
//        public double[] tauX = null;
//        /// <summary>
//        /// придонное касательное напряжение по y
//        /// </summary>
//        public double[] tauY = null;
//        /// <summary>
//        /// придонное давление
//        /// </summary>
//        public double[] P = null;
//        /// <summary>
//        /// Поля определяемые из контекста задачи (концентрация наносов и т.д.)
//        /// </summary>
//        public double[][] CS = null;
//        /// <summary>
//        /// Эволюция кривых
//        /// </summary>
//        [NonSerialized]
//        public ISavePoint sps = null;
//        /// <summary>
//        /// Журнал задачи
//        /// </summary>
//        [NonSerialized]
//        ILogger<IMessageItem> logger = Logger.Instance;
//        /// <summary>
//        /// конструктор по умолчанию
//        /// </summary>
//        public ChannelProcess() : base(new CProcParams())
//        {
//            logger = Logger.Instance;
//        }
//        /// <summary>
//        /// конструктор с параметрами
//        /// </summary>
//        /// <param name="rt"></param>
//        /// <param name="blt"></param>
//        public ChannelProcess(IRiver rt, IBedLoadTask blt, bool loadRiver = false, string taskNameFile = "") :
//            base(new CProcParams())
//        {
//            channelProcessError = ChannelProcessError.notError;
//            logger = Logger.Instance;
//            Set(rt, blt, loadRiver, taskNameFile);
//        }
//        /// <summary>
//        /// конструктор копирования
//        /// </summary>
//        /// <param name="cp"></param>
//        public ChannelProcess(ChannelProcess cp) : base(cp)
//        {
//            riverTask = cp.riverTask;
//            bedLoadTask = cp.bedLoadTask;
//            sendParam = cp.sendParam;
//            Zeta = cp.Zeta;
//            tauX = cp.tauX;
//            tauY = cp.tauY;
//            P = cp.P;
//            count = cp.count;
//            // отметки дна получаем отдельно от сетки, в случае с плановой задачей
//            // в сетке находятся только координты узлов х и у
//            riverTask.GetZeta(ref Zeta);
//            bedLoadTask.SetTask(riverTask.BedMesh(), Zeta, riverTask.BoundCondition());
//        }
//        /// <summary>
//        /// Установка задач
//        /// </summary>
//        /// <param name="rt"></param>
//        /// <param name="blt"></param>
//        /// <param name="cpp"></param>
//        /// <param name="taskNameFile"></param>
//        public ChannelProcess(IRiver rt, IBedLoadTask blt, CProcParams cpp, bool loadRiver = false, string taskNameFile = "") :
//            base(cpp)
//        {
//            Set(rt, blt, loadRiver, taskNameFile);
//        }
//        /// <summary>
//        /// Установка задач 
//        /// </summary>
//        /// <param name="rt">гидро поток</param>
//        /// <param name="blt">дно</param>
//        /// <param name="taskNameFile"></param>
//        public void Set(IRiver rt, IBedLoadTask blt, bool loadRiver = false, string taskNameFile = "")
//        {
//            riverTask = rt;
//            bedLoadTask = blt;
//            if (rt.typeTask != blt.typeTask)
//                throw new Exception("русловая задача (" +
//                riverTask.Name + " : " + bedLoadTask.Name + ") не совместима!");
//            // Синхронизация шага по времени
//            TimeSynchronization();
//            // загрузка заднных
//            if (loadRiver == false)
//                LoadTask(taskNameFile);
//            else
//                BedLink();
//        }
//        /// <summary>
//        /// Загрузка параметров задачи
//        /// </summary>
//        public void LoadTask(string taskNameFile = "")
//        {
//            try
//            {
//                // получаем имена файлов для данных по умолчанию
//                ITaskFileNames fn = riverTask.taskFileNemes();
//                fn.NameBLParams = bedLoadTask.NameBLParams();
//                if (taskNameFile == "")
//                {
//                    Logger.Instance.Info("Используется конфигурация по умолчанию ChannelProcess.LoadTask()");
//                    logger.Info("Используется конфигурация по умолчанию ChannelProcess.LoadTask()");
//                }
//                else
//                {
//                    using (StreamReader file = new StreamReader(taskNameFile))
//                    {
//                        if (file == null)
//                        {
//                            Logger.Instance.Info("Файл конфигурации не обнаружен ChannelProcess.LoadTask()");
//                            Logger.Instance.Info("будет используется конфигурация по умолчанию");
//                            logger.Warning("Файл конфигурации не обнаружен", "ChannelProcess.LoadTask()");
//                            logger.Info("будет используется конфигурация по умолчанию");
//                        }
//                        else
//                        {
//                            fn.NameCPParams = file.ReadLine();
//                            fn.NameBLParams = file.ReadLine();
//                            fn.NameRSParams = file.ReadLine();
//                            fn.NameRData = file.ReadLine();
//                        }
//                    }
//                }
//                LoadParams(fn.NameCPParams);
//                riverTask.LoadParams(fn.NameRSParams);
//                bedLoadTask.LoadParams(fn.NameBLParams);
//                // загрузка уровней дна, расходов, 
//                riverTask.LoadData(fn.NameRData);
//                // передача данных о сетке и дне
//                if (bedErosion != EBedErosion.NoBedErosion
//                    || riverTask.typeTask == TypeTask.streamY1D)
//                    BedLink();
//            }
//            catch (Exception ep)
//            {
//                channelProcessError = ChannelProcessError.riverIO;
//                Logger.Instance.Info("Ошибка при загрузке параметров задачи: ChannelProcess.LoadTask()");
//                Logger.Instance.Exception(ep);
//                logger.Error("Ошибка при загрузке параметров задачи", "ChannelProcess.LoadTask()");
//                logger.Exception(ep);
//            }
//        }
//        /// <summary>
//        /// передача данных о сетке и дне
//        /// </summary>
//        protected void BedLink()
//        {
//            riverTask.GetZeta(ref Zeta);
//            // передача данных о сетке и дне
//            IMesh BedMesh = riverTask.BedMesh();
//            bedLoadTask.SetTask(BedMesh, Zeta, riverTask.BoundCondition());
//            if (bedLoadTask.TaskReady() == true)
//                logger.Info("Метод загрузки данных завершен успешно");
//        }

//        /// <summary>
//        /// Расчет русловых процессов
//        /// </summary>
//        public double SolverStep()
//        {
//            try
//            {
//                logger = Logger.Instance;
//                if (sps == null) sps = new SavePoint();
//                // =========== ГИДРОДИНАМИКА ========
//                // генерация секи для задачи гидродинамики
//                channelProcessError = ChannelProcessError.riverMeshError;
//                riverTask.time = time;
//                //if(Zeta == null) 
//                //    BedLink();
//                riverTask.SetZeta(Zeta, bedErosion);
//                channelProcessError = ChannelProcessError.riverError;
//                // расчет гидрадинамики  (скоростей потока)
//                riverTask.SolverStep();
//                dtime = riverTask.dtime;
//                IMesh bedMesh = null;
//                // расчет донных деформаций
//                if (bedErosion != EBedErosion.NoBedErosion)
//                {
//                    channelProcessError = ChannelProcessError.bedError;
//                    bedLoadTask.dtime = dtime;
//                    // расчет  придонных касательных напряжений на дне
//                    riverTask.GetTau(ref tauX, ref tauY, ref P, ref CS);
//                    // ========== БИБЛИОТЕКА === BLLib === 
//                    if (bedLoadTask.TaskReady() == false)
//                    {
//                        riverTask.GetZeta(ref Zeta);
//                        bedMesh = riverTask.BedMesh();
//                        bedLoadTask.SetTask(riverTask.BedMesh(), Zeta, riverTask.BoundCondition());
//                    }
//                    if (bedLoadTask.TaskReady() == true)
//                    {
//                        bedLoadTask.time = time;
//                        bedLoadTask.CalkZetaFDM(ref Zeta, tauX, tauY, P, CS);
//                    }
//                    else
//                    {
//                        if (bedMesh != null)
//                            Logger.Instance.Info("не загружена сетка для дна, возможно задача гидрадинамики требует загрузки");
//                        else
//                        {
//                            Logger.Instance.Info("ошибка! не активирована задача донных деформаций: ChannelProcess.SolverStep()");
//                            logger.Error("ошибка! не активирована задача донных деформаций", "ChannelProcess.SolverStep()");
//                        }
//                    }
//                }
//                // текущее время
//                time += dtime;
//                // сохранение результатов расчета
//                SaveData();
//                // шаг расчета выполнен
//                channelProcessError = ChannelProcessError.notError;
//            }
//            catch (Exception ex)
//            {
//                Logger.Instance.Exception(ex);
//                logger.Exception(ex);
//            }
//            return time;
//        }
//        /// <summary>
//        /// Создание точки сохранения данных
//        /// </summary>
//        /// <returns></returns>
//        public ISavePoint GetSavePoint()
//        {
//            IMesh mesh = riverTask.Mesh();
//            if (mesh != null)
//            {
//                SavePoint sp = new SavePoint();
//                sp.SetSavePoint(time, riverTask.Mesh());
//                riverTask.AddMeshPolesForGraphics(sp);
//                double[] x = riverTask.Mesh().GetCoords(0);
//                double[] y = riverTask.Mesh().GetCoords(1);
//                sp.Add(new Field1D("Координаты х", x));
//                sp.Add(new Field1D("Координаты y", y));
//                if (bedErosion != EBedErosion.NoBedErosion)
//                    bedLoadTask.AddMeshPolesForGraphics(sp);
//                return sp;
//            }
//            else
//            {
//                return null;
//            }
//        }
//        /// <summary>
//        /// Синхронизация шага по времени
//        /// </summary>
//        public void TimeSynchronization()
//        {
//            riverTask.dtime = dtime;
//            bedLoadTask.dtime = dtime;
//        }
//        /// <summary>
//        /// Метод буфферезации кадра решаемой задачи
//        /// </summary>
//        protected void SaveData()
//        {
//            if (MEM.Equals(time, dtime) == true)
//                count = 1;
//            if (time >= count * timeSavePeriod || MEM.Equals(time, dtime) == true)
//            {
//                // текущий кадр
//                ISavePoint sp = GetSavePoint();
//                // отсылка кадра на обновление постпроцессору
//                if (sendParam != null)
//                    sendParam((SavePoint)sp);
//                // сборка кривых со всех кадров
//                if (sendTimeParam != null && sps != null)
//                {
//                    // кривые текущего кадра
//                    List<GraphicsCurve> curves = ((GraphicsData)sp.graphicsData).curves;
//                    // очистка от старых эволюционных кривых (функций от времени)
//                    sps.ClearСurve(TypeGraphicsCurve.TimeCurve);
//                    foreach (var c in curves)
//                    {
//                        string tm = (((int)(1000 * time)) / 1000).ToString() + "#";
//                        //string tm = time.ToString("F4") + " ";
//                        c.Name = tm + c.Name;
//                        sps.AddCurve(c);
//                    }
//                    sendTimeParam((SavePoint)sps);
//                }
//                // сохранение кадра в файл
//                if (flagSP == true)
//                {
//                    string NameSP = "Сохранение кадра расчета, t = " + time.ToString() + " секунд ";
//                    sp.SerializableSavePoint(sp, NameSP);
//                    Logger.Instance.Info(NameSP);
//                    logger.Info(NameSP);
//                }
//                if (MEM.Equals(time, dtime) != true)
//                    count++;
//            }
//        }
//    }
//}
