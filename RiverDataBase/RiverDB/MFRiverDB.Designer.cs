namespace RiverDB
{
    partial class MFRiverDB
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.конвертерToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gpxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.ExittTSM = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmListData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_ExperimentData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Knots = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_GPost = new System.Windows.Forms.ToolStripMenuItem();
            this.отчетыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmWaterLevel = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.генерацияToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.экспортДанныхToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.конвертерToolStripMenuItem,
            this.tsmListData,
            this.генерацияToolStripMenuItem,
            this.отчетыToolStripMenuItem,
            this.tsmOptions});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(644, 31);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // конвертерToolStripMenuItem
            // 
            this.конвертерToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gpxToolStripMenuItem,
            this.экспортДанныхToolStripMenuItem,
            this.toolStripMenuItem1,
            this.ExittTSM});
            this.конвертерToolStripMenuItem.Name = "конвертерToolStripMenuItem";
            this.конвертерToolStripMenuItem.Size = new System.Drawing.Size(70, 25);
            this.конвертерToolStripMenuItem.Text = "Файлы";
            // 
            // gpxToolStripMenuItem
            // 
            this.gpxToolStripMenuItem.Name = "gpxToolStripMenuItem";
            this.gpxToolStripMenuItem.Size = new System.Drawing.Size(242, 26);
            this.gpxToolStripMenuItem.Text = "Импорт в базу данных";
            this.gpxToolStripMenuItem.Click += new System.EventHandler(this.gpxToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(296, 6);
            // 
            // ExittTSM
            // 
            this.ExittTSM.Name = "ExittTSM";
            this.ExittTSM.Size = new System.Drawing.Size(299, 26);
            this.ExittTSM.Text = "Выход";
            this.ExittTSM.Click += new System.EventHandler(this.ExittTSM_Click);
            // 
            // tsmListData
            // 
            this.tsmListData.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_ExperimentData,
            this.tsm_Knots,
            this.tsm_GPost});
            this.tsmListData.Name = "tsmListData";
            this.tsmListData.Size = new System.Drawing.Size(110, 25);
            this.tsmListData.Text = "Объекты DB";
            // 
            // tsm_ExperimentData
            // 
            this.tsm_ExperimentData.Name = "tsm_ExperimentData";
            this.tsm_ExperimentData.Size = new System.Drawing.Size(263, 26);
            this.tsm_ExperimentData.Text = "Уровни на гидропостах";
            this.tsm_ExperimentData.Click += new System.EventHandler(this.tsm_ExperimentData_Click);
            // 
            // tsm_Knots
            // 
            this.tsm_Knots.Name = "tsm_Knots";
            this.tsm_Knots.Size = new System.Drawing.Size(263, 26);
            this.tsm_Knots.Text = "Экспедиционные данные";
            this.tsm_Knots.Click += new System.EventHandler(this.tsm_Knots_Click);
            // 
            // tsm_GPost
            // 
            this.tsm_GPost.Name = "tsm_GPost";
            this.tsm_GPost.Size = new System.Drawing.Size(263, 26);
            this.tsm_GPost.Text = "Гидропосты";
            this.tsm_GPost.Click += new System.EventHandler(this.tsm_GPost_Click);
            // 
            // отчетыToolStripMenuItem
            // 
            this.отчетыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmWaterLevel});
            this.отчетыToolStripMenuItem.Name = "отчетыToolStripMenuItem";
            this.отчетыToolStripMenuItem.Size = new System.Drawing.Size(76, 25);
            this.отчетыToolStripMenuItem.Text = "Отчеты";
            // 
            // tsmWaterLevel
            // 
            this.tsmWaterLevel.Name = "tsmWaterLevel";
            this.tsmWaterLevel.Size = new System.Drawing.Size(180, 26);
            this.tsmWaterLevel.Text = "Уровни";
            this.tsmWaterLevel.Click += new System.EventHandler(this.tsmWaterLevel_Click);
            // 
            // tsmOptions
            // 
            this.tsmOptions.Name = "tsmOptions";
            this.tsmOptions.Size = new System.Drawing.Size(99, 25);
            this.tsmOptions.Text = "Настройки";
            this.tsmOptions.Click += new System.EventHandler(this.tsmOptions_Click);
            // 
            // генерацияToolStripMenuItem
            // 
            this.генерацияToolStripMenuItem.Name = "генерацияToolStripMenuItem";
            this.генерацияToolStripMenuItem.Size = new System.Drawing.Size(99, 25);
            this.генерацияToolStripMenuItem.Text = "Генерация";
            this.генерацияToolStripMenuItem.Click += new System.EventHandler(this.tsm_CreareCloudMesh_Click);
            // 
            // экспортДанныхToolStripMenuItem
            // 
            this.экспортДанныхToolStripMenuItem.Name = "экспортДанныхToolStripMenuItem";
            this.экспортДанныхToolStripMenuItem.Size = new System.Drawing.Size(242, 26);
            this.экспортДанныхToolStripMenuItem.Text = "Экспорт данных";
            this.экспортДанныхToolStripMenuItem.Visible = false;
            // 
            // MFRiverDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 360);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MFRiverDB";
            this.Text = "ИС Амур";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem конвертерToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gpxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmListData;
        private System.Windows.Forms.ToolStripMenuItem tsm_ExperimentData;
        private System.Windows.Forms.ToolStripMenuItem tsm_Knots;
        private System.Windows.Forms.ToolStripMenuItem tsm_GPost;
        private System.Windows.Forms.ToolStripMenuItem tsmOptions;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem ExittTSM;
        private System.Windows.Forms.ToolStripMenuItem отчетыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmWaterLevel;
        private System.Windows.Forms.ToolStripMenuItem генерацияToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem экспортДанныхToolStripMenuItem;
    }
}