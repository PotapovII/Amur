
namespace CPForm
{
    partial class TaskForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.tcParam = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pg_TaskParams = new System.Windows.Forms.PropertyGrid();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pg_TaskRiverParams = new System.Windows.Forms.PropertyGrid();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pg_TaskBedParams = new System.Windows.Forms.PropertyGrid();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.pr_Physics = new System.Windows.Forms.PropertyGrid();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.smFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.smLoadData = new System.Windows.Forms.ToolStripMenuItem();
            this.smExportData = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.smImportData = new System.Windows.Forms.ToolStripMenuItem();
            this.smExtExportDsta = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.smExit = new System.Windows.Forms.ToolStripMenuItem();
            this.smtTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smStartTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smPauseTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smReStartTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smOpeStepTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smStopTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.smDelSaveTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smDelSavePointTask = new System.Windows.Forms.ToolStripMenuItem();
            this.smObaut = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.gdI_Control1 = new RenderLib.GDI_Control();
            this.tsm_ClearSavePoint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1.SuspendLayout();
            this.tcParam.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelStatus,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelTime});
            this.statusStrip1.Location = new System.Drawing.Point(0, 815);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1290, 25);
            this.statusStrip1.TabIndex = 177;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(59, 20);
            this.toolStripStatusLabel1.Text = "Статус :";
            // 
            // toolStripStatusLabelStatus
            // 
            this.toolStripStatusLabelStatus.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
            this.toolStripStatusLabelStatus.Size = new System.Drawing.Size(140, 20);
            this.toolStripStatusLabelStatus.Text = "подготовка задачи";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(168, 20);
            this.toolStripStatusLabel2.Text = "Время расчетное в сек";
            // 
            // toolStripStatusLabelTime
            // 
            this.toolStripStatusLabelTime.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.toolStripStatusLabelTime.Name = "toolStripStatusLabelTime";
            this.toolStripStatusLabelTime.Size = new System.Drawing.Size(17, 20);
            this.toolStripStatusLabelTime.Text = "0";
            // 
            // tcParam
            // 
            this.tcParam.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcParam.Controls.Add(this.tabPage1);
            this.tcParam.Controls.Add(this.tabPage2);
            this.tcParam.Controls.Add(this.tabPage3);
            this.tcParam.Controls.Add(this.tabPage4);
            this.tcParam.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tcParam.Location = new System.Drawing.Point(931, 44);
            this.tcParam.Name = "tcParam";
            this.tcParam.SelectedIndex = 0;
            this.tcParam.Size = new System.Drawing.Size(359, 763);
            this.tcParam.TabIndex = 178;
            this.tcParam.SelectedIndexChanged += new System.EventHandler(this.tcParam_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pg_TaskParams);
            this.tabPage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(351, 734);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Задача";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pg_TaskParams
            // 
            this.pg_TaskParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pg_TaskParams.Location = new System.Drawing.Point(3, 3);
            this.pg_TaskParams.Name = "pg_TaskParams";
            this.pg_TaskParams.Size = new System.Drawing.Size(345, 728);
            this.pg_TaskParams.TabIndex = 4;
            this.pg_TaskParams.ToolbarVisible = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.pg_TaskRiverParams);
            this.tabPage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(351, 734);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Речной поток";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // pg_TaskRiverParams
            // 
            this.pg_TaskRiverParams.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pg_TaskRiverParams.Location = new System.Drawing.Point(0, 0);
            this.pg_TaskRiverParams.Name = "pg_TaskRiverParams";
            this.pg_TaskRiverParams.Size = new System.Drawing.Size(351, 635);
            this.pg_TaskRiverParams.TabIndex = 5;
            this.pg_TaskRiverParams.ToolbarVisible = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.pg_TaskBedParams);
            this.tabPage3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(351, 734);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Русло";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pg_TaskBedParams
            // 
            this.pg_TaskBedParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pg_TaskBedParams.Location = new System.Drawing.Point(0, 0);
            this.pg_TaskBedParams.Name = "pg_TaskBedParams";
            this.pg_TaskBedParams.Size = new System.Drawing.Size(351, 734);
            this.pg_TaskBedParams.TabIndex = 6;
            this.pg_TaskBedParams.ToolbarVisible = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.pr_Physics);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(351, 734);
            this.tabPage4.TabIndex = 4;
            this.tabPage4.Text = "Параметры";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // pr_Physics
            // 
            this.pr_Physics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pr_Physics.Location = new System.Drawing.Point(0, 0);
            this.pr_Physics.Name = "pr_Physics";
            this.pr_Physics.Size = new System.Drawing.Size(351, 734);
            this.pr_Physics.TabIndex = 7;
            this.pr_Physics.ToolbarVisible = false;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smFiles,
            this.smtTask,
            this.smOptions});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1290, 29);
            this.menuStrip1.TabIndex = 179;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // smFiles
            // 
            this.smFiles.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smLoadData,
            this.smExportData,
            this.toolStripMenuItem1,
            this.smImportData,
            this.smExtExportDsta,
            this.toolStripMenuItem2,
            this.smExit});
            this.smFiles.Name = "smFiles";
            this.smFiles.Size = new System.Drawing.Size(59, 25);
            this.smFiles.Text = "Файл";
            // 
            // smLoadData
            // 
            this.smLoadData.Name = "smLoadData";
            this.smLoadData.Size = new System.Drawing.Size(234, 26);
            this.smLoadData.Text = "Загрузка данных *.tsk";
            this.smLoadData.Click += new System.EventHandler(this.smLoadData_Click);
            // 
            // smExportData
            // 
            this.smExportData.Name = "smExportData";
            this.smExportData.Size = new System.Drawing.Size(234, 26);
            this.smExportData.Text = "Экспорт данных *.tsk";
            this.smExportData.Click += new System.EventHandler(this.smExportData_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(231, 6);
            // 
            // smImportData
            // 
            this.smImportData.Name = "smImportData";
            this.smImportData.Size = new System.Drawing.Size(234, 26);
            this.smImportData.Text = "Импорт данных";
            this.smImportData.Click += new System.EventHandler(this.smImportData_Click);
            // 
            // smExtExportDsta
            // 
            this.smExtExportDsta.Name = "smExtExportDsta";
            this.smExtExportDsta.Size = new System.Drawing.Size(234, 26);
            this.smExtExportDsta.Text = "Экспорт данных";
            this.smExtExportDsta.Click += new System.EventHandler(this.smExtExportData_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(231, 6);
            // 
            // smExit
            // 
            this.smExit.Name = "smExit";
            this.smExit.Size = new System.Drawing.Size(234, 26);
            this.smExit.Text = "Выход";
            this.smExit.Click += new System.EventHandler(this.smExit_Click);
            // 
            // smtTask
            // 
            this.smtTask.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smStartTask,
            this.smPauseTask,
            this.smReStartTask,
            this.smOpeStepTask,
            this.smStopTask});
            this.smtTask.Name = "smtTask";
            this.smtTask.Size = new System.Drawing.Size(73, 25);
            this.smtTask.Text = "Задача";
            // 
            // smStartTask
            // 
            this.smStartTask.Name = "smStartTask";
            this.smStartTask.Size = new System.Drawing.Size(231, 26);
            this.smStartTask.Text = "Старт задачи";
            this.smStartTask.Click += new System.EventHandler(this.smStartTask_Click);
            // 
            // smPauseTask
            // 
            this.smPauseTask.Name = "smPauseTask";
            this.smPauseTask.Size = new System.Drawing.Size(231, 26);
            this.smPauseTask.Text = "Пауза";
            this.smPauseTask.Click += new System.EventHandler(this.smPauseTask_Click);
            // 
            // smReStartTask
            // 
            this.smReStartTask.Name = "smReStartTask";
            this.smReStartTask.Size = new System.Drawing.Size(231, 26);
            this.smReStartTask.Text = "Рестарт задачт";
            this.smReStartTask.Click += new System.EventHandler(this.smReStartTask_Click);
            // 
            // smOpeStepTask
            // 
            this.smOpeStepTask.Name = "smOpeStepTask";
            this.smOpeStepTask.Size = new System.Drawing.Size(231, 26);
            this.smOpeStepTask.Text = "Один расчетный шаг";
            this.smOpeStepTask.Click += new System.EventHandler(this.smOpeStepTask_Click);
            // 
            // smStopTask
            // 
            this.smStopTask.Name = "smStopTask";
            this.smStopTask.Size = new System.Drawing.Size(231, 26);
            this.smStopTask.Text = "Остановка расчета";
            this.smStopTask.Click += new System.EventHandler(this.smStopTask_Click);
            // 
            // smOptions
            // 
            this.smOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_ClearSavePoint,
            this.toolStripMenuItem3,
            this.smDelSaveTask,
            this.smDelSavePointTask,
            this.smObaut});
            this.smOptions.Name = "smOptions";
            this.smOptions.Size = new System.Drawing.Size(71, 25);
            this.smOptions.Text = "Опции";
            // 
            // smDelSaveTask
            // 
            this.smDelSaveTask.Name = "smDelSaveTask";
            this.smDelSaveTask.Size = new System.Drawing.Size(359, 26);
            this.smDelSaveTask.Text = "Удалить файлы по расширению .tsk";
            this.smDelSaveTask.Click += new System.EventHandler(this.smDelSaveTask_Click);
            // 
            // smDelSavePointTask
            // 
            this.smDelSavePointTask.Name = "smDelSavePointTask";
            this.smDelSavePointTask.Size = new System.Drawing.Size(359, 26);
            this.smDelSavePointTask.Text = "Удалить файлы точек сохранения .rpsp";
            this.smDelSavePointTask.Click += new System.EventHandler(this.smDelSavePointTask_Click);
            // 
            // smObaut
            // 
            this.smObaut.Name = "smObaut";
            this.smObaut.Size = new System.Drawing.Size(359, 26);
            this.smObaut.Text = "О программе";
            this.smObaut.Click += new System.EventHandler(this.smObaut_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // gdI_Control1
            // 
            this.gdI_Control1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gdI_Control1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gdI_Control1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.gdI_Control1.Location = new System.Drawing.Point(12, 44);
            this.gdI_Control1.Name = "gdI_Control1";
            this.gdI_Control1.Size = new System.Drawing.Size(914, 768);
            this.gdI_Control1.TabIndex = 174;
            // 
            // tsm_ClearSavePoint
            // 
            this.tsm_ClearSavePoint.Name = "tsm_ClearSavePoint";
            this.tsm_ClearSavePoint.Size = new System.Drawing.Size(359, 26);
            this.tsm_ClearSavePoint.Text = "Очистка истории точек сохранения";
            this.tsm_ClearSavePoint.Click += new System.EventHandler(this.tsm_ClearSavePoint_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(356, 6);
            // 
            // TaskForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1290, 840);
            this.Controls.Add(this.tcParam);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.gdI_Control1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TaskForm";
            this.Text = "TaskForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaskForm_FormClosing);
            this.Load += new System.EventHandler(this.BaseForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tcParam.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected RenderLib.GDI_Control gdI_Control1;
        protected System.Windows.Forms.StatusStrip statusStrip1;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelStatus;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTime;
        protected System.Windows.Forms.TabControl tcParam;
        protected System.Windows.Forms.TabPage tabPage1;
        protected System.Windows.Forms.TabPage tabPage2;
        protected System.Windows.Forms.TabPage tabPage3;
        protected System.Windows.Forms.PropertyGrid pg_TaskParams;
        protected System.Windows.Forms.PropertyGrid pg_TaskBedParams;
        protected System.Windows.Forms.PropertyGrid pg_TaskRiverParams;
        protected System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem smFiles;
        private System.Windows.Forms.ToolStripMenuItem smtTask;
        private System.Windows.Forms.ToolStripMenuItem smOptions;
        private System.Windows.Forms.ToolStripMenuItem smLoadData;
        private System.Windows.Forms.ToolStripMenuItem smExportData;
        private System.Windows.Forms.ToolStripMenuItem smImportData;
        private System.Windows.Forms.ToolStripMenuItem smStartTask;
        private System.Windows.Forms.ToolStripMenuItem smPauseTask;
        private System.Windows.Forms.ToolStripMenuItem smReStartTask;
        private System.Windows.Forms.ToolStripMenuItem smStopTask;
        private System.Windows.Forms.ToolStripMenuItem smDelSaveTask;
        private System.Windows.Forms.ToolStripMenuItem smDelSavePointTask;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem smExit;
        private System.Windows.Forms.ToolStripMenuItem smObaut;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem smOpeStepTask;
        private System.Windows.Forms.ToolStripMenuItem smExtExportDsta;
        private System.Windows.Forms.TabPage tabPage4;
        protected System.Windows.Forms.PropertyGrid pr_Physics;
        private System.Windows.Forms.ToolStripMenuItem tsm_ClearSavePoint;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
    }
}