namespace Clootils
{
    partial class MainForm
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.checkedListExamples = new System.Windows.Forms.CheckedListBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.comboBoxPlatform = new System.Windows.Forms.ComboBox();
            this.checkedListDevices = new System.Windows.Forms.CheckedListBox();
            this.groupBoxPlatform = new System.Windows.Forms.GroupBox();
            this.groupBoxDevices = new System.Windows.Forms.GroupBox();
            this.buttonRunAll = new System.Windows.Forms.Button();
            this.buttonCopyLog = new System.Windows.Forms.Button();
            this.groupBoxOptions = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.groupBoxPlatform.SuspendLayout();
            this.groupBoxDevices.SuspendLayout();
            this.groupBoxOptions.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.tableLayoutPanel.SetColumnSpan(this.splitContainer, 2);
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(4, 162);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.checkedListExamples);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.textBoxLog);
            this.splitContainer.Size = new System.Drawing.Size(1169, 677);
            this.splitContainer.SplitterDistance = 201;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 2;
            // 
            // checkedListExamples
            // 
            this.checkedListExamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListExamples.FormattingEnabled = true;
            this.checkedListExamples.Location = new System.Drawing.Point(0, 0);
            this.checkedListExamples.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkedListExamples.Name = "checkedListExamples";
            this.checkedListExamples.Size = new System.Drawing.Size(201, 677);
            this.checkedListExamples.TabIndex = 0;
            // 
            // textBoxLog
            // 
            this.textBoxLog.AcceptsTab = true;
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(0, 0);
            this.textBoxLog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(963, 677);
            this.textBoxLog.TabIndex = 1;
            this.textBoxLog.WordWrap = false;
            // 
            // comboBoxPlatform
            // 
            this.comboBoxPlatform.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPlatform.FormattingEnabled = true;
            this.comboBoxPlatform.Location = new System.Drawing.Point(7, 21);
            this.comboBoxPlatform.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxPlatform.Name = "comboBoxPlatform";
            this.comboBoxPlatform.Size = new System.Drawing.Size(394, 23);
            this.comboBoxPlatform.TabIndex = 7;
            // 
            // checkedListDevices
            // 
            this.checkedListDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListDevices.FormattingEnabled = true;
            this.checkedListDevices.Location = new System.Drawing.Point(4, 19);
            this.checkedListDevices.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkedListDevices.Name = "checkedListDevices";
            this.checkedListDevices.Size = new System.Drawing.Size(745, 131);
            this.checkedListDevices.TabIndex = 9;
            // 
            // groupBoxPlatform
            // 
            this.groupBoxPlatform.AutoSize = true;
            this.groupBoxPlatform.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxPlatform.Controls.Add(this.comboBoxPlatform);
            this.groupBoxPlatform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxPlatform.Location = new System.Drawing.Point(4, 3);
            this.groupBoxPlatform.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxPlatform.Name = "groupBoxPlatform";
            this.groupBoxPlatform.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxPlatform.Size = new System.Drawing.Size(408, 67);
            this.groupBoxPlatform.TabIndex = 11;
            this.groupBoxPlatform.TabStop = false;
            this.groupBoxPlatform.Text = "Platform";
            // 
            // groupBoxDevices
            // 
            this.groupBoxDevices.AutoSize = true;
            this.groupBoxDevices.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxDevices.Controls.Add(this.checkedListDevices);
            this.groupBoxDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDevices.Location = new System.Drawing.Point(420, 3);
            this.groupBoxDevices.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxDevices.Name = "groupBoxDevices";
            this.groupBoxDevices.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel.SetRowSpan(this.groupBoxDevices, 2);
            this.groupBoxDevices.Size = new System.Drawing.Size(753, 153);
            this.groupBoxDevices.TabIndex = 12;
            this.groupBoxDevices.TabStop = false;
            this.groupBoxDevices.Text = "Devices";
            // 
            // buttonRunAll
            // 
            this.buttonRunAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonRunAll.Location = new System.Drawing.Point(7, 31);
            this.buttonRunAll.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonRunAll.Name = "buttonRunAll";
            this.buttonRunAll.Size = new System.Drawing.Size(88, 27);
            this.buttonRunAll.TabIndex = 14;
            this.buttonRunAll.Text = "Run";
            this.buttonRunAll.UseVisualStyleBackColor = true;
            this.buttonRunAll.Click += new System.EventHandler(this.buttonRunAll_Click);
            // 
            // buttonCopyLog
            // 
            this.buttonCopyLog.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonCopyLog.Location = new System.Drawing.Point(314, 31);
            this.buttonCopyLog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCopyLog.Name = "buttonCopyLog";
            this.buttonCopyLog.Size = new System.Drawing.Size(88, 27);
            this.buttonCopyLog.TabIndex = 15;
            this.buttonCopyLog.Text = "Copy Log";
            this.buttonCopyLog.UseVisualStyleBackColor = true;
            this.buttonCopyLog.Click += new System.EventHandler(this.buttonCopyLog_Click);
            // 
            // groupBoxOptions
            // 
            this.groupBoxOptions.AutoSize = true;
            this.groupBoxOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxOptions.Controls.Add(this.buttonRunAll);
            this.groupBoxOptions.Controls.Add(this.buttonCopyLog);
            this.groupBoxOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOptions.Location = new System.Drawing.Point(4, 76);
            this.groupBoxOptions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxOptions.Name = "groupBoxOptions";
            this.groupBoxOptions.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxOptions.Size = new System.Drawing.Size(408, 80);
            this.groupBoxOptions.TabIndex = 17;
            this.groupBoxOptions.TabStop = false;
            this.groupBoxOptions.Text = "Options";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.groupBoxPlatform, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.groupBoxOptions, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.groupBoxDevices, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.splitContainer, 0, 2);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.Size = new System.Drawing.Size(1176, 842);
            this.tableLayoutPanel.TabIndex = 18;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1176, 842);
            this.Controls.Add(this.tableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clootils";
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.groupBoxPlatform.ResumeLayout(false);
            this.groupBoxDevices.ResumeLayout(false);
            this.groupBoxOptions.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.ComboBox comboBoxPlatform;
        private System.Windows.Forms.CheckedListBox checkedListDevices;
        private System.Windows.Forms.GroupBox groupBoxPlatform;
        private System.Windows.Forms.GroupBox groupBoxDevices;
        private System.Windows.Forms.Button buttonRunAll;
        private System.Windows.Forms.Button buttonCopyLog;
        private System.Windows.Forms.GroupBox groupBoxOptions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.CheckedListBox checkedListExamples;

    }
}