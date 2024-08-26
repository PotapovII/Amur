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
            this.btClear = new System.Windows.Forms.Button();
            this.btInsertData = new System.Windows.Forms.Button();
            this.btCansel = new System.Windows.Forms.Button();
            this.btOpenData = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsLab = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(16, 113);
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
            this.lbExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbExp.FormattingEnabled = true;
            this.lbExp.ItemHeight = 16;
            this.lbExp.Location = new System.Drawing.Point(20, 147);
            this.lbExp.Name = "lbExp";
            this.lbExp.Size = new System.Drawing.Size(514, 516);
            this.lbExp.TabIndex = 584;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(696, 113);
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
            this.label3.Location = new System.Drawing.Point(538, 113);
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
            this.cListBoxDatesGPX.Location = new System.Drawing.Point(689, 155);
            this.cListBoxDatesGPX.Name = "cListBoxDatesGPX";
            this.cListBoxDatesGPX.Size = new System.Drawing.Size(146, 508);
            this.cListBoxDatesGPX.TabIndex = 591;
            // 
            // cListBoxDates
            // 
            this.cListBoxDates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cListBoxDates.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.cListBoxDates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cListBoxDates.FormattingEnabled = true;
            this.cListBoxDates.Location = new System.Drawing.Point(540, 155);
            this.cListBoxDates.Name = "cListBoxDates";
            this.cListBoxDates.Size = new System.Drawing.Size(146, 508);
            this.cListBoxDates.TabIndex = 590;
            // 
            // btClear
            // 
            this.btClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btClear.Location = new System.Drawing.Point(412, 12);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(175, 34);
            this.btClear.TabIndex = 597;
            this.btClear.Text = "Очистка НСД";
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // btInsertData
            // 
            this.btInsertData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btInsertData.Enabled = false;
            this.btInsertData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btInsertData.Location = new System.Drawing.Point(218, 12);
            this.btInsertData.Name = "btInsertData";
            this.btInsertData.Size = new System.Drawing.Size(175, 34);
            this.btInsertData.TabIndex = 596;
            this.btInsertData.Text = "Конвертация в БД";
            this.btInsertData.UseVisualStyleBackColor = true;
            this.btInsertData.Click += new System.EventHandler(this.btInsertData_Click);
            // 
            // btCansel
            // 
            this.btCansel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btCansel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCansel.Location = new System.Drawing.Point(607, 12);
            this.btCansel.Name = "btCansel";
            this.btCansel.Size = new System.Drawing.Size(175, 34);
            this.btCansel.TabIndex = 595;
            this.btCansel.Text = "Закрыть";
            this.btCansel.UseVisualStyleBackColor = true;
            this.btCansel.Click += new System.EventHandler(this.btCansel_Click);
            // 
            // btOpenData
            // 
            this.btOpenData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btOpenData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btOpenData.Location = new System.Drawing.Point(20, 12);
            this.btOpenData.Name = "btOpenData";
            this.btOpenData.Size = new System.Drawing.Size(175, 34);
            this.btOpenData.TabIndex = 594;
            this.btOpenData.Text = "Открыть файлы";
            this.btOpenData.UseVisualStyleBackColor = true;
            this.btOpenData.Click += new System.EventHandler(this.btOpenData_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbFilter.Location = new System.Drawing.Point(380, 62);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(100, 26);
            this.tbFilter.TabIndex = 599;
            this.tbFilter.Text = "181";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(16, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(350, 20);
            this.label1.TabIndex = 600;
            this.label1.Text = "Фильтр синхронизации данных по долготе >";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLab});
            this.statusStrip1.Location = new System.Drawing.Point(0, 682);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(853, 22);
            this.statusStrip1.TabIndex = 602;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsLab
            // 
            this.tsLab.Name = "tsLab";
            this.tsLab.Size = new System.Drawing.Size(0, 17);
            // 
            // ConvertForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 704);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.btClear);
            this.Controls.Add(this.btInsertData);
            this.Controls.Add(this.btCansel);
            this.Controls.Add(this.btOpenData);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cListBoxDatesGPX);
            this.Controls.Add(this.cListBoxDates);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbExp);
            this.Name = "ConvertForm";
            this.Text = "ConvertForm";
            this.Load += new System.EventHandler(this.ConvertForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
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
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.Button btInsertData;
        private System.Windows.Forms.Button btCansel;
        private System.Windows.Forms.Button btOpenData;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsLab;
    }
}