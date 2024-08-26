namespace RenderLib.Fields
{
    partial class FunCreator
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbName = new System.Windows.Forms.Label();
            this.lbFun = new System.Windows.Forms.ListBox();
            this.cbEvil = new System.Windows.Forms.CheckBox();
            this.lbArg = new System.Windows.Forms.ListBox();
            this.btDel = new System.Windows.Forms.Button();
            this.tbArg = new System.Windows.Forms.TextBox();
            this.btAdd = new System.Windows.Forms.Button();
            this.labFun = new System.Windows.Forms.Label();
            this.lblArg = new System.Windows.Forms.Label();
            this.tbFun = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.lbName);
            this.panel1.Controls.Add(this.lbFun);
            this.panel1.Controls.Add(this.cbEvil);
            this.panel1.Controls.Add(this.lbArg);
            this.panel1.Controls.Add(this.btDel);
            this.panel1.Controls.Add(this.tbArg);
            this.panel1.Controls.Add(this.btAdd);
            this.panel1.Controls.Add(this.labFun);
            this.panel1.Controls.Add(this.lblArg);
            this.panel1.Controls.Add(this.tbFun);
            this.panel1.Location = new System.Drawing.Point(5, 7);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(234, 405);
            this.panel1.TabIndex = 8;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbName.Location = new System.Drawing.Point(13, 11);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(83, 20);
            this.lbName.TabIndex = 10;
            this.lbName.Text = "Название";
            // 
            // lbFun
            // 
            this.lbFun.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbFun.FormattingEnabled = true;
            this.lbFun.ItemHeight = 20;
            this.lbFun.Location = new System.Drawing.Point(122, 151);
            this.lbFun.Name = "lbFun";
            this.lbFun.Size = new System.Drawing.Size(103, 244);
            this.lbFun.TabIndex = 8;
            this.lbFun.SelectedIndexChanged += new System.EventHandler(this.lbFun_SelectedIndexChanged);
            // 
            // cbEvil
            // 
            this.cbEvil.AutoSize = true;
            this.cbEvil.Checked = true;
            this.cbEvil.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEvil.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbEvil.Location = new System.Drawing.Point(15, 40);
            this.cbEvil.Name = "cbEvil";
            this.cbEvil.Size = new System.Drawing.Size(107, 24);
            this.cbEvil.TabIndex = 7;
            this.cbEvil.Text = "Эволюция";
            this.cbEvil.UseVisualStyleBackColor = true;
            this.cbEvil.CheckedChanged += new System.EventHandler(this.cbEvil_CheckedChanged);
            // 
            // lbArg
            // 
            this.lbArg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbArg.FormattingEnabled = true;
            this.lbArg.ItemHeight = 20;
            this.lbArg.Location = new System.Drawing.Point(8, 151);
            this.lbArg.Name = "lbArg";
            this.lbArg.Size = new System.Drawing.Size(108, 244);
            this.lbArg.TabIndex = 5;
            this.lbArg.SelectedIndexChanged += new System.EventHandler(this.lbArg_SelectedIndexChanged);
            // 
            // btDel
            // 
            this.btDel.BackColor = System.Drawing.SystemColors.Control;
            this.btDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btDel.Location = new System.Drawing.Point(119, 68);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(97, 27);
            this.btDel.TabIndex = 6;
            this.btDel.Text = "Удалить";
            this.btDel.UseVisualStyleBackColor = false;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            // 
            // tbArg
            // 
            this.tbArg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbArg.Location = new System.Drawing.Point(13, 119);
            this.tbArg.Name = "tbArg";
            this.tbArg.Size = new System.Drawing.Size(100, 26);
            this.tbArg.TabIndex = 0;
            // 
            // btAdd
            // 
            this.btAdd.BackColor = System.Drawing.SystemColors.Control;
            this.btAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btAdd.Location = new System.Drawing.Point(13, 68);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(100, 27);
            this.btAdd.TabIndex = 1;
            this.btAdd.Text = "Добавить";
            this.btAdd.UseVisualStyleBackColor = false;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // labFun
            // 
            this.labFun.AutoSize = true;
            this.labFun.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labFun.Location = new System.Drawing.Point(119, 98);
            this.labFun.Name = "labFun";
            this.labFun.Size = new System.Drawing.Size(83, 20);
            this.labFun.TabIndex = 4;
            this.labFun.Text = "Значение";
            // 
            // lblArg
            // 
            this.lblArg.AutoSize = true;
            this.lblArg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblArg.Location = new System.Drawing.Point(13, 98);
            this.lblArg.Name = "lblArg";
            this.lblArg.Size = new System.Drawing.Size(58, 20);
            this.lblArg.TabIndex = 2;
            this.lblArg.Text = "Время";
            // 
            // tbFun
            // 
            this.tbFun.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbFun.Location = new System.Drawing.Point(119, 119);
            this.tbFun.Name = "tbFun";
            this.tbFun.Size = new System.Drawing.Size(100, 26);
            this.tbFun.TabIndex = 3;
            // 
            // FunCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "FunCreator";
            this.Size = new System.Drawing.Size(247, 420);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lbFun;
        private System.Windows.Forms.CheckBox cbEvil;
        private System.Windows.Forms.ListBox lbArg;
        private System.Windows.Forms.Button btDel;
        private System.Windows.Forms.TextBox tbArg;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Label labFun;
        private System.Windows.Forms.Label lblArg;
        private System.Windows.Forms.TextBox tbFun;
        private System.Windows.Forms.Label lbName;
    }
}
