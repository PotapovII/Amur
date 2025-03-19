//---------------------------------------------------------------------------
//                Проект "Русловые процессы"
//                  - (C) Copyright 2021
//                        Потапов И.И.
//                         21.04.21
//---------------------------------------------------------------------------
//                         07.01.2024
//          добавлен солитон физических параметров
//     убраны физические параметры из прочих таблиц свойств
//---------------------------------------------------------------------------
namespace CPForm
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Runtime.Serialization.Formatters.Binary;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using ChannelProcessLib;
    using CommonLib.Physics;
    using System.Collections.Generic;

    using RenderLib.PDG;
    /// <summary>
    /// Базистная форма задачи
    /// </summary>
    public partial class TaskForm : Form
    {
        /// <summary>
        /// Русловая задача
        /// </summary>
        public ChannelProcessPro task = null;
        /// <summary>
        /// Сетка задачи
        /// </summary>
        protected IMesh mesh = null;
        /// <summary>
        /// Флаг паузы в расчете
        /// </summary>
        /// bool pausa = false;
        protected TaskState taskState = TaskState.startTask;
        /// <summary>
        /// шаг сохранения
        /// </summary>
        protected int period = 1;
        /// <summary>
        /// Флаг сохранения рузультата
        /// </summary>
        protected bool flagSave = true;
        /// <summary>
        /// Имя файла для сохранения результатов
        /// </summary>
        protected string NameSave;
        /// <summary>
        /// Индекс гидродинамической задачи
        /// </summary>
        int idxRiver;
        /// <summary>
        /// Индекс задачи донных деформаций
        /// </summary>
        int idxBload;
        /// <summary>
        /// Менеджер гидродинамических задач
        /// </summary>
        ManagerRiverTask mrt;
        /// <summary>
        /// Менеджер задач донных деформаций
        /// </summary>
        ManagerBedLoadTask mbt;
        /// <summary>
        /// Форма логирования задачи
        /// </summary>
        FormLogger fLogger = null;
        /// <summary>
        /// Журнал задачи
        /// </summary>
        ILogger<IMessageItem> logger = Logger.Instance;

        public PropertyGrid SPYS { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="fLogger"></param>
        /// <param name="idxRiver"></param>
        /// <param name="idxBload"></param>
        /// <param name="mrt"></param>
        /// <param name="mbt"></param>
        public TaskForm(FormLogger fLogger, int idxRiver, int idxBload, ManagerRiverTask mrt, ManagerBedLoadTask mbt)
        {
            InitializeComponent();

            this.fLogger = fLogger;
            this.idxBload = idxBload;
            this.idxRiver = idxRiver;
            this.mrt = mrt;
            this.mbt = mbt;
            IRiver rtask = mrt.Clone(idxRiver);
            IBedLoadTask btask = mbt.Clone(idxBload);
            task = new ChannelProcessPro(rtask, btask);
            SetHeadToLogger();
            // файлы расширения задачи
            ITaskFileNames taskFileNames = rtask.taskFileNemes();
            string ext = taskFileNames.NameEXT;
            #region Ввод/вывод
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            openFileDialog1.Filter = "файл задачи  " + ext +
                                     "Все файлы (*.*)|*.*";
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            saveFileDialog1.Filter = "файл задачи  " + ext +
                                     "Все файлы (*.*)|*.*";
            // импорт                
            openFileDialog2.Filter = taskFileNames.NameEXTImport;
            #endregion
            SetGrid();
            gdI_Control1.AddOwner(this);
        }
        //protected static void SetSplitter(PropertyGrid proGrid)
        //{
        //    Type type = proGrid.GetType();
        //    BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        //    FieldInfo field = type.GetField("gridView", flags);
        //    object gridView = field.GetValue(proGrid);
        //    Type gridType = gridView.GetType();
        //    BindingFlags flagsTwo = BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance;
        //    gridType.InvokeMember("MoveSplitterTo", flagsTwo, null, gridView, new object[] { 180 });
        //}

        #region Методы синхронизации при передаче данных м/д потоками
        /// <summary>
        /// Метод синхронизирует передачу объекта SavePoint между разными потоками
        /// (потока вычислений и потока контрола)
        /// </summary>
        /// <param name="sp"></param>
        protected void SetSavePoint(ISavePoint sp)
        {
            if (this.InvokeRequired)
            {
                SendParam d = new SendParam(gdI_Control1.SendSavePoint);
                this.Invoke(d, new object[] { sp });
            }
            else
                gdI_Control1.SendSavePoint(sp);
        }
        protected void SetSavePointCurves(ISavePoint sp)
        {
            if (this.InvokeRequired)
            {
                SendParam d = new SendParam(gdI_Control1.SetSavePointCurves);
                this.Invoke(d, new object[] { sp });
            }
            else
                gdI_Control1.SetSavePointCurves(sp);
        }
        private void SetText(string text)
        {
            if (this.InvokeRequired)
            {
                SendText d = new SendText(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
                toolStripStatusLabelTime.Text = text;
        }
        private void SetFlag(bool flag)
        {
            if (this.InvokeRequired)
            {
                SendValue<bool> d = new SendValue<bool>(SetFlag);
                this.Invoke(d, new object[] { flag });
            }
            else
            {
                smStartTask.Enabled = flag;
                smReStartTask.Enabled = flag;
                smStopTask.Enabled = !flag;
                smPauseTask.Enabled = !flag;
            }
        }
        #endregion

        protected void BaseForm_Load(object sender, EventArgs e)
        {
            smReStartTask.Enabled = false;
            smStopTask.Enabled = false;
            smPauseTask.Enabled = false;
            IRiver rtask = task.RiverTask();
            IOFormater<IRiver> loader = rtask.GetFormater();
            smImportData.Visible = loader.SupportImport;
            smExtExportDsta.Visible = loader.SupportExport;
        }
        /// <summary>
        /// Метод обратного вызова для расчетного потока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                double time = task.ps.time;
                if (MEM.Equals(time, 0) == true)
                    period = 1;
                if (taskState == TaskState.startTask)
                {
                    taskState = TaskState.runTask;
                    //sps = new SavePoint();
                }
                else
                    SetTaskParams();
                if (taskState == TaskState.restartTask)
                {
                    taskState = TaskState.runTask;
                }
                // установка параметров задачи из гридов в задачи
                
                task.sendParam = SetSavePoint;
                task.sendTimeParam = SetSavePointCurves;
                toolStripStatusLabelStatus.Text = "расчет задачи";
                NameSave = task.ps.saveFileNeme;
                for (;;)
                {
                    if (taskState == TaskState.runTask || 
                        taskState == TaskState.runOneStep)
                    {
                        // шаг расчета задачи
                        time = task.SolverStep();
                        //
                        if (task.channelProcessError != ChannelProcessError.notError)
                            throw new Exception(task.GetError());
                        SetText(time.ToString("F4"));
                    }
                    // расчет завершен?
                    if (MEM.Equals(time, task.ps.timeMax, task.ps.dtime / 2) == true || time > task.ps.timeMax)
                        taskState = TaskState.stopTask;
                    // сохранение состояния задачи с периодом TimeSavePeriod
                    if (task.ps.flagSave == true || 
                        taskState == TaskState.stopTask || 
                        taskState == TaskState.runOneStep)
                    {
                        if (time >= period * task.ps.TimeSavePeriod || 
                            taskState == TaskState.stopTask || 
                            taskState == TaskState.runOneStep)
                        {
                            DateTime timeNow = DateTime.Now;
                            string dstr = timeNow.ToString();
                            string str = dstr.Replace(":", ".");
                            string FileName = NameSave + " " + (time).ToString("F4") +
                                " сек " + str + ".tsk";
                            WR.SerializableTask(task, FileName, time);
                            if (taskState == TaskState.runOneStep)
                                taskState = TaskState.stopTask;
                            if (taskState == TaskState.stopTask)
                            {
                                FileName = "..\\..\\Result\\" + FileName;
                                WR.SerializableTask(task, FileName, time);
                            }
                            if (taskState == TaskState.stopTask)
                            {
                                SetFlag(true);
                                toolStripStatusLabelStatus.Text = "расчет остановлен";
                                break;
                            }
                            period++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
                logger.Error("Нештатное завершение процесса расчета", "backgroundWorker1_DoWork()");
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Нештатное завершение процесса расчета");
                if (taskState == TaskState.stopTask)
                {
                    SetFlag(true);
                    taskState = TaskState.stopTask;
                    toolStripStatusLabelStatus.Text = "ошибка вычислений";
                }
            }
        }

   
        /// <summary>
        /// Установка грида по свойствам задачи
        /// </summary>
        protected void SetGrid()
        {
            pg_TaskParams.SelectedObject      = task.GetParams();
            pg_TaskRiverParams.SelectedObject = task.RiverTask().GetParams();
            pg_TaskBedParams.SelectedObject   = task.BedLoadTask().GetParams();
            pr_Physics.SelectedObject =  SPhysics.PHYS;
        }
        /// <summary>
        /// установка параметров задачи из гридов в задачу
        /// </summary>
        protected void SetTaskParams()
        {
            task.SetParams(pg_TaskParams.SelectedObject);
            task.RiverTask().SetParams(pg_TaskRiverParams.SelectedObject);
            task.BedLoadTask().SetParams(pg_TaskBedParams.SelectedObject);
            SPhysics.PHYS.SetParams(pr_Physics.SelectedObject);
            // синхронизация времени в задачах
            task.TimeSynchronization();
        }
        #region Переопределяемые методы
        protected virtual void RunTask()
        {
            LOG.Clear();
            smStartTask.Enabled = false;
            smReStartTask.Enabled = false;
            smPauseTask.Enabled = true;
            smStopTask.Enabled = true;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
            toolStripStatusLabelStatus.Text = "расчет задачи";
        }
        protected virtual string PauseTask()
        {
            string[] str = { "Пауза", "Отмена паузы" };
            int ID;
            if (taskState == TaskState.runTask)
            {
                ID = 1;
                toolStripStatusLabelStatus.Text = "пауза расчета";
                smPauseTask.Text = "Отмена паузы";
                taskState = TaskState.pauseTask;
            }
            else
            {
                ID = 0;
                toolStripStatusLabelStatus.Text = "расчет задачи";
                smPauseTask.Text = "Пауза";
                taskState = TaskState.runTask;
            }
            return str[ID];
        }
        protected virtual void StopTask()
        {
            smStartTask.Enabled = true;
            smReStartTask.Enabled = true;
            toolStripStatusLabelStatus.Text = "расчет остановлен";
            taskState = TaskState.stopTask;
        }


        #endregion

        #region Ввод / вывод данных
        /// <summary>
        /// Загрузка контекста задачи в двоичном форматк
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smLoadData_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    task = new ChannelProcessPro();

                    WR.DeserializeTask(openFileDialog1.FileName, ref task);
                    IRiver rtask = task.RiverTask();
                    IOFormater<IRiver> loader = rtask.GetFormater();
                    smImportData.Visible = loader.SupportImport;
                    smExtExportDsta.Visible = loader.SupportExport;

                    SetHeadToLogger();

                    this.Text = Name + " " + openFileDialog1.FileName;
                    SetGrid();
                    SavePoint sp = (SavePoint)task.GetSavePoint();
                    ISavePoint sps = task.sps;
                    SetSavePoint(sp);
                    SetSavePointCurves(sps);
                    SetText(task.ps.time.ToString("F4"));
                    toolStripStatusLabelStatus.Text = "загрузка выполнена";
                    smReStartTask.Enabled = true;
                    smStopTask.Enabled = false;
                    logger.Info("загрузка задачи из файла: " + openFileDialog1.FileName + "выполнена");
                    smStartTask.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabelStatus.Text = "ошибка чтения" + ex.Message;
                Logger.Instance.Exception(ex);
                logger.Exception(ex);
            }
        }
        /// <summary>
        /// Сохранение контекста задачи в двоичном форматк
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smExportData_Click(object sender, EventArgs e)
        {
            try
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (task == null) return;
                    // создаем объект BinaryFormatter
                    BinaryFormatter formatter = new BinaryFormatter();
                    // получаем поток, куда будем записывать сериализованный объект
                    using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.OpenOrCreate))
                    {
                        formatter.Serialize(fs, task);
                    }
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabelStatus.Text = "ошибка сохранения" + ex.Message;
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Импорт задачи из файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smImportData_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    IRiver rtask = mrt.Clone(idxRiver);
                    IOFormater<IRiver> loader = rtask.GetFormater();
                    loader.Read(openFileDialog2.FileName, ref rtask);
                    IBedLoadTask btask = mbt.Clone(idxBload);
                    task = new ChannelProcessPro(rtask, btask);
                    SetHeadToLogger();
                    if (task.channelProcessError != ChannelProcessError.notError)
                        throw new Exception(task.GetError());
                    this.Text = Name + " " + openFileDialog2.FileName;
                    // установка параметров загруженной задачи в гриды окна управления
                    SetGrid();
                    // отображение данных загруженной задачи
                    SavePoint sp = (SavePoint)task.GetSavePoint();
                    if (sp != null)
                    { 
                        SetSavePoint(sp);
                        SetSavePointCurves(task.sps);
                    }
                    toolStripStatusLabelStatus.Text = "загрузка выполнена";
                    smReStartTask.Enabled = true;
                    smStopTask.Enabled = false;
                    smStartTask.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabelStatus.Text = "ошибка чтения" + ex.Message;
                Logger.Instance.Exception(ex);
            }
        }
        private void smExtExportData_Click(object sender, EventArgs e)
        {
            IRiver rtask = task.RiverTask();
            IOFormater<IRiver> loader = rtask.GetFormater();
            string tmp = saveFileDialog1.Filter;
            saveFileDialog1.Filter = loader.FilterSD;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                loader.Write(rtask, saveFileDialog1.FileName);
            saveFileDialog1.Filter = tmp;
        }
        #endregion
        /// <summary>
        /// Пауза в расчетах
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smPauseTask_Click(object sender, EventArgs e)
        {
            PauseTask();
        }
        /// <summary>
        /// Старт задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void smStartTask_Click(object sender, EventArgs e)
        {
            taskState = TaskState.startTask;
            IRiver rtask = mrt.Clone(idxRiver);
            IBedLoadTask btask = mbt.Clone(idxBload);
            task = new ChannelProcessPro(rtask, btask);
            // загрузка параметров по умолчанию в созданную задачу
            SetTaskParams();
            // получение загрузчика задачи
            IOFormater<IRiver> loader = rtask.GetFormater();
            // получение от загрузчика задачи списка тестовых кейсов
            List<string> TaskNames = loader.GetTestsName();
            // загрузка задачи из файла / или класса чтения
            uint testTaskID = 0;
            // при наличии тестовых кейсов запуск окна выбора
            if (TaskNames.Count > 1)
            {
                FTestsList ff = new FTestsList(TaskNames);
                if (ff.ShowDialog() == DialogResult.OK)
                    testTaskID = (uint)ff.GetTaskID();
            }
            // загрузка задачи по умолчанию или тестовых кейсов
            task.LoadComputationalDomainDefault(testTaskID);
            // установка параметров загруженной задачи в гриды окна управления
            SetGrid();
            // запуск задачи на выполнение
            RunTask();
            logger.Info("Запуск задачи на выполнение");
        }
        private void smReStartTask_Click(object sender, EventArgs e)
        {
            taskState = TaskState.restartTask;
            RunTask();
            logger.Info("Перезапуск задачи на выполнение");
        }
        private void smOpeStepTask_Click(object sender, EventArgs e)
        {
            taskState = TaskState.runOneStep;
            RunTask();
            logger.Info("Запуск одного шага выполнения задачи");
        }
        private void smStopTask_Click(object sender, EventArgs e)
        {
            StopTask();
            smStopTask.Enabled = false;
            smPauseTask.Enabled = false;
            smReStartTask.Enabled = true;
        }
        private void smExit_Click(object sender, EventArgs e)
        {
            StopTask();
            Close();
        }
        private void smDelSaveTask_Click(object sender, EventArgs e)
        {
            // выбрать папку
            DirectoryInfo Dir = new DirectoryInfo(Application.StartupPath);
            // выбрать все файлы из папки по расширению .tsk
            FileInfo[] files = Dir.GetFiles("*.tsk").Where(p => p.Extension == ".tsk").ToArray();
            // пройти по циклу
            foreach (FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    // удалить файлы из списка
                    File.Delete(file.FullName);
                }
                catch(Exception ex) 
                {
                    toolStripStatusLabelStatus.Text = "ошибка при удаления сохраненных задач" + ex.Message;
                    Logger.Instance.Exception(ex);
                }
            }
        }
        private void smDelSaveCurve_Click(object sender, EventArgs e)
        {
            // выбрать папку
            DirectoryInfo Dir = new DirectoryInfo(Application.StartupPath);
            // выбрать все файлы из папки по расширению .rpsp
            FileInfo[] files = Dir.GetFiles("*.cvs").Where(p => p.Extension == ".cvs").ToArray();
            // пройти по циклу
            foreach (FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    // удалить файлы из списка
                    File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    toolStripStatusLabelStatus.Text = "ошибка при удаления кривых" + ex.Message;
                    Logger.Instance.Exception(ex);
                }
            }
        }
        private void smDelSavePointTask_Click(object sender, EventArgs e)
        {
            // выбрать папку
            DirectoryInfo Dir = new DirectoryInfo(Application.StartupPath);
            // выбрать все файлы из папки по расширению .rpsp
            FileInfo[] files = Dir.GetFiles("*.rpsp").Where(p => p.Extension == ".rpsp").ToArray();
            // пройти по циклу
            foreach (FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    // удалить файлы из списка
                    File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    toolStripStatusLabelStatus.Text = "ошибка при удаления точек сохранения" + ex.Message;
                    Logger.Instance.Exception(ex);
                }
            }
        }

        protected void SetHeadToLogger()
        {
            logger.ClearHeaderInfo();
            logger.AddHeaderInfo(task.RiverTask().Name);
            logger.AddHeaderInfo(task.BedLoadTask().Name);
        }
        private void smObaut_Click(object sender, EventArgs e)
        {
            FormInfo fi = new FormInfo();
            fi.Show();
        }
        /// <summary>
        /// определения ширины клонок в проперти гриде
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcParam_SelectedIndexChanged(object sender, EventArgs e)
        {
            int splitterPositionCP = pg_TaskParams.GetInternalLabelWidth();
            pg_TaskParams.MoveSplitterTo(splitterPositionCP);
            pg_TaskRiverParams.MoveSplitterTo(splitterPositionCP + 20);
            pg_TaskBedParams.MoveSplitterTo(splitterPositionCP + 20);
        }
        private void TaskForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(fLogger!=null)
                fLogger.Close();
            Console.WriteLine("Выход");
        }

        private void tsm_ClearSavePoint_Click(object sender, EventArgs e)
        {
            task?.sps?.ClearСurve();
        }
    }
}
