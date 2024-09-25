namespace RiverDB.Convertors
{
    partial class ConvertForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.lbExp = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cListBoxDatesGPX = new System.Windows.Forms.CheckedListBox();
            this.cListBoxDates = new System.Windows.Forms.CheckedListBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsLab = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.filesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.convertDBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.insertButton = new System.Windows.Forms.ToolStripButton();
            this.btInsertData = new System.Windows.Forms.ToolStripButton();
            this.ts_ClearBadData = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tsb_VelocityMark = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(4, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 20);
            this.label2.TabIndex = 583;
            this.label2.Text = "Точки замера глубин";
            // 
            // lbExp
            // 
            this.lbExp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbExp.FormattingEnabled = true;
            this.lbExp.ItemHeight = 20;
            this.lbExp.Location = new System.Drawing.Point(8, 42);
            this.lbExp.Name = "lbExp";
            this.lbExp.Size = new System.Drawing.Size(583, 524);
            this.lbExp.TabIndex = 584;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(744, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 20);
            this.label4.TabIndex = 593;
            this.label4.Text = "Точки gpx";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(596, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 20);
            this.label3.TabIndex = 592;
            this.label3.Text = "Все точки xls / dat";
            // 
            // cListBoxDatesGPX
            // 
            this.cListBoxDatesGPX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cListBoxDatesGPX.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.cListBoxDatesGPX.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cListBoxDatesGPX.FormattingEnabled = true;
            this.cListBoxDatesGPX.Location = new System.Drawing.Point(746, 42);
            this.cListBoxDatesGPX.Name = "cListBoxDatesGPX";
            this.cListBoxDatesGPX.Size = new System.Drawing.Size(146, 529);
            this.cListBoxDatesGPX.TabIndex = 591;
            // 
            // cListBoxDates
            // 
            this.cListBoxDates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cListBoxDates.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.cListBoxDates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cListBoxDates.FormattingEnabled = true;
            this.cListBoxDates.Location = new System.Drawing.Point(597, 42);
            this.cListBoxDates.Name = "cListBoxDates";
            this.cListBoxDates.Size = new System.Drawing.Size(146, 529);
            this.cListBoxDates.TabIndex = 590;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLab});
            this.statusStrip1.Location = new System.Drawing.Point(0, 629);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(922, 22);
            this.statusStrip1.TabIndex = 602;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsLab
            // 
            this.tsLab.Name = "tsLab";
            this.tsLab.Size = new System.Drawing.Size(0, 17);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filesToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(922, 29);
            this.menuStrip1.TabIndex = 604;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // filesToolStripMenuItem
            // 
            this.filesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadDBToolStripMenuItem,
            this.toolStripMenuItem2,
            this.convertDBToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            this.filesToolStripMenuItem.Size = new System.Drawing.Size(149, 25);
            this.filesToolStripMenuItem.Text = "Источник данных";
            // 
            // loadDBToolStripMenuItem
            // 
            this.loadDBToolStripMenuItem.Name = "loadDBToolStripMenuItem";
            this.loadDBToolStripMenuItem.Size = new System.Drawing.Size(229, 26);
            this.loadDBToolStripMenuItem.Text = "Загрузка данных";
            this.loadDBToolStripMenuItem.Click += new System.EventHandler(this.loadDBToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(226, 6);
            // 
            // convertDBToolStripMenuItem
            // 
            this.convertDBToolStripMenuItem.Name = "convertDBToolStripMenuItem";
            this.convertDBToolStripMenuItem.Size = new System.Drawing.Size(229, 26);
            this.convertDBToolStripMenuItem.Text = "Синхронизация в БД";
            this.convertDBToolStripMenuItem.Visible = false;
            this.convertDBToolStripMenuItem.Click += new System.EventHandler(this.convertDBToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(226, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(229, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(83, 25);
            this.helpToolStripMenuItem.Text = "Помошь";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertButton,
            this.btInsertData,
            this.ts_ClearBadData,
            this.toolStripButton1,
            this.tsb_VelocityMark});
            this.toolStrip1.Location = new System.Drawing.Point(0, 29);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(922, 31);
            this.toolStrip1.TabIndex = 605;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.lbExp);
            this.panel1.Controls.Add(this.cListBoxDates);
            this.panel1.Controls.Add(this.cListBoxDatesGPX);
            this.panel1.Location = new System.Drawing.Point(5, 52);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(905, 576);
            this.panel1.TabIndex = 606;
            // 
            // insertButton
            // 
            this.insertButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.insertButton.Image = global::RiverDB.Properties.Resources.LoadNodes;
            this.insertButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(28, 28);
            this.insertButton.Text = "Загрузка данных";
            this.insertButton.Click += new System.EventHandler(this.insertButton_Click);
            // 
            // btInsertData
            // 
            this.btInsertData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btInsertData.Image = global::RiverDB.Properties.Resources.Convert;
            this.btInsertData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btInsertData.Name = "btInsertData";
            this.btInsertData.Size = new System.Drawing.Size(28, 28);
            this.btInsertData.Text = "Синхронизация в БД";
            this.btInsertData.Visible = false;
            this.btInsertData.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // ts_ClearBadData
            // 
            this.ts_ClearBadData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ts_ClearBadData.Image = global::RiverDB.Properties.Resources.DelNodes;
            this.ts_ClearBadData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ts_ClearBadData.Name = "ts_ClearBadData";
            this.ts_ClearBadData.Size = new System.Drawing.Size(28, 28);
            this.ts_ClearBadData.Text = "toolStripButton1";
            this.ts_ClearBadData.Click += new System.EventHandler(this.ts_ClearBadData_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::RiverDB.Properties.Resources.FCloud;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton1.Text = "Выичсление  срезанных глубин";
            this.toolStripButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // tsb_VelocityMark
            // 
            this.tsb_VelocityMark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_VelocityMark.Image = global::RiverDB.Properties.Resources.FLCloud;
            this.tsb_VelocityMark.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_VelocityMark.Name = "tsb_VelocityMark";
            this.tsb_VelocityMark.Size = new System.Drawing.Size(28, 28);
            this.tsb_VelocityMark.Text = "Маркировка скоростных узлов";
            this.tsb_VelocityMark.Click += new System.EventHandler(this.tsb_VelocityMark_Click);
            // 
            // ConvertForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 651);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Name = "ConvertForm";
            this.Text = "Конвертор в базу данных";
            this.Load += new System.EventHandler(this.ConvertForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbExp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox cListBoxDatesGPX;
        private System.Windows.Forms.CheckedListBox cListBoxDates;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsLab;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem filesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadDBToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton insertButton;
        private System.Windows.Forms.ToolStripButton btInsertData;
        private System.Windows.Forms.ToolStripMenuItem convertDBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripButton ts_ClearBadData;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton tsb_VelocityMark;
    }
}