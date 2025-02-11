namespace RiverDB.Report
{
    partial class FormReportWL
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormReportWL));
            this.cListBoxDates = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbAllData = new System.Windows.Forms.CheckBox();
            this.btWaterlevel = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btRiverFlow = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.bt1 = new System.Windows.Forms.Button();
            this.tb2 = new System.Windows.Forms.TextBox();
            this.tb21 = new System.Windows.Forms.TextBox();
            this.btLoadRateWL = new System.Windows.Forms.Button();
            this.pnRF = new System.Windows.Forms.Panel();
            this.rbMad = new System.Windows.Forms.RadioButton();
            this.rbAmur = new System.Windows.Forms.RadioButton();
            this.rbPemza = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.pnRF.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cListBoxDates
            // 
            this.cListBoxDates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cListBoxDates.BackColor = System.Drawing.SystemColors.Window;
            this.cListBoxDates.CheckOnClick = true;
            this.cListBoxDates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cListBoxDates.FormattingEnabled = true;
            this.cListBoxDates.Location = new System.Drawing.Point(15, 65);
            this.cListBoxDates.Name = "cListBoxDates";
            this.cListBoxDates.Size = new System.Drawing.Size(238, 424);
            this.cListBoxDates.TabIndex = 78;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(8, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 20);
            this.label1.TabIndex = 84;
            this.label1.Text = "Годы с данными по уровням";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.cbAllData);
            this.panel1.Controls.Add(this.cListBoxDates);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(17, 60);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(263, 495);
            this.panel1.TabIndex = 85;
            // 
            // cbAllData
            // 
            this.cbAllData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAllData.AutoSize = true;
            this.cbAllData.BackColor = System.Drawing.SystemColors.ControlLight;
            this.cbAllData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbAllData.Location = new System.Drawing.Point(16, 35);
            this.cbAllData.Name = "cbAllData";
            this.cbAllData.Size = new System.Drawing.Size(98, 24);
            this.cbAllData.TabIndex = 89;
            this.cbAllData.Text = "Все годы";
            this.cbAllData.UseVisualStyleBackColor = false;
            this.cbAllData.CheckedChanged += new System.EventHandler(this.cbAllData_CheckedChanged);
            // 
            // btWaterlevel
            // 
            this.btWaterlevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btWaterlevel.Location = new System.Drawing.Point(290, 101);
            this.btWaterlevel.Name = "btWaterlevel";
            this.btWaterlevel.Size = new System.Drawing.Size(192, 29);
            this.btWaterlevel.TabIndex = 86;
            this.btWaterlevel.Text = "Графики уровней (см)";
            this.btWaterlevel.UseVisualStyleBackColor = true;
            this.btWaterlevel.Click += new System.EventHandler(this.btWaterlevel_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.Location = new System.Drawing.Point(388, 526);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(93, 29);
            this.button2.TabIndex = 87;
            this.button2.Text = "Выход";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btRiverFlow
            // 
            this.btRiverFlow.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btRiverFlow.Location = new System.Drawing.Point(13, 9);
            this.btRiverFlow.Name = "btRiverFlow";
            this.btRiverFlow.Size = new System.Drawing.Size(156, 29);
            this.btRiverFlow.TabIndex = 88;
            this.btRiverFlow.Text = "Расходы (м^3/с)";
            this.btRiverFlow.UseVisualStyleBackColor = true;
            this.btRiverFlow.Click += new System.EventHandler(this.btRiverFlow_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(16, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 20);
            this.label2.TabIndex = 527;
            this.label2.Text = "Название гидропоста";
            // 
            // bt1
            // 
            this.bt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bt1.Image = ((System.Drawing.Image)(resources.GetObject("bt1.Image")));
            this.bt1.Location = new System.Drawing.Point(444, 27);
            this.bt1.Name = "bt1";
            this.bt1.Size = new System.Drawing.Size(37, 27);
            this.bt1.TabIndex = 526;
            this.bt1.UseVisualStyleBackColor = true;
            this.bt1.Click += new System.EventHandler(this.bt1_Click);
            // 
            // tb2
            // 
            this.tb2.Enabled = false;
            this.tb2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb2.Location = new System.Drawing.Point(16, 28);
            this.tb2.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.tb2.Name = "tb2";
            this.tb2.Size = new System.Drawing.Size(48, 26);
            this.tb2.TabIndex = 525;
            // 
            // tb21
            // 
            this.tb21.Enabled = false;
            this.tb21.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb21.Location = new System.Drawing.Point(70, 28);
            this.tb21.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.tb21.Name = "tb21";
            this.tb21.Size = new System.Drawing.Size(366, 26);
            this.tb21.TabIndex = 524;
            // 
            // btLoadRateWL
            // 
            this.btLoadRateWL.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btLoadRateWL.Location = new System.Drawing.Point(290, 66);
            this.btLoadRateWL.Name = "btLoadRateWL";
            this.btLoadRateWL.Size = new System.Drawing.Size(192, 29);
            this.btLoadRateWL.TabIndex = 528;
            this.btLoadRateWL.Text = "Загрузить данные";
            this.btLoadRateWL.UseVisualStyleBackColor = true;
            this.btLoadRateWL.Click += new System.EventHandler(this.btLoadRateWL_Click);
            // 
            // pnRF
            // 
            this.pnRF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnRF.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.pnRF.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnRF.Controls.Add(this.rbMad);
            this.pnRF.Controls.Add(this.rbAmur);
            this.pnRF.Controls.Add(this.rbPemza);
            this.pnRF.Controls.Add(this.btRiverFlow);
            this.pnRF.Location = new System.Drawing.Point(290, 136);
            this.pnRF.Name = "pnRF";
            this.pnRF.Size = new System.Drawing.Size(192, 148);
            this.pnRF.TabIndex = 529;
            // 
            // rbMad
            // 
            this.rbMad.AutoSize = true;
            this.rbMad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbMad.Location = new System.Drawing.Point(13, 104);
            this.rbMad.Name = "rbMad";
            this.rbMad.Size = new System.Drawing.Size(131, 24);
            this.rbMad.TabIndex = 92;
            this.rbMad.Text = "пр. Бешенная";
            this.rbMad.UseVisualStyleBackColor = true;
            // 
            // rbAmur
            // 
            this.rbAmur.AutoSize = true;
            this.rbAmur.Checked = true;
            this.rbAmur.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbAmur.Location = new System.Drawing.Point(13, 44);
            this.rbAmur.Name = "rbAmur";
            this.rbAmur.Size = new System.Drawing.Size(65, 24);
            this.rbAmur.TabIndex = 4;
            this.rbAmur.TabStop = true;
            this.rbAmur.Text = "Амур";
            this.rbAmur.UseVisualStyleBackColor = true;
            // 
            // rbPemza
            // 
            this.rbPemza.AutoSize = true;
            this.rbPemza.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbPemza.Location = new System.Drawing.Point(13, 74);
            this.rbPemza.Name = "rbPemza";
            this.rbPemza.Size = new System.Drawing.Size(141, 24);
            this.rbPemza.TabIndex = 0;
            this.rbPemza.Text = "пр.Пемзенская";
            this.rbPemza.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.pnRF);
            this.panel2.Controls.Add(this.btLoadRateWL);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.bt1);
            this.panel2.Controls.Add(this.btWaterlevel);
            this.panel2.Controls.Add(this.tb2);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.tb21);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(490, 565);
            this.panel2.TabIndex = 530;
            // 
            // FormReportWL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 565);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormReportWL";
            this.Text = "Отчеты об уровнях и расходах";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnRF.ResumeLayout(false);
            this.pnRF.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox cListBoxDates;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btWaterlevel;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btRiverFlow;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Button bt1;
        public System.Windows.Forms.TextBox tb2;
        public System.Windows.Forms.TextBox tb21;
        private System.Windows.Forms.Button btLoadRateWL;
        private System.Windows.Forms.CheckBox cbAllData;
        private System.Windows.Forms.Panel pnRF;
        private System.Windows.Forms.RadioButton rbMad;
        private System.Windows.Forms.RadioButton rbAmur;
        private System.Windows.Forms.RadioButton rbPemza;
        private System.Windows.Forms.Panel panel2;
    }
}