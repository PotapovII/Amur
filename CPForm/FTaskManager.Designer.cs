
namespace CPForm
{
    partial class FTaskManager
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

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.lb_river = new System.Windows.Forms.ListBox();
            this.lb_bload = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rb2XY = new System.Windows.Forms.RadioButton();
            this.rb1Y = new System.Windows.Forms.RadioButton();
            this.rb1X = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonExit = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lb_river
            // 
            this.lb_river.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_river.FormattingEnabled = true;
            this.lb_river.ItemHeight = 18;
            this.lb_river.Items.AddRange(new object[] {
            "1"});
            this.lb_river.Location = new System.Drawing.Point(13, 116);
            this.lb_river.Name = "lb_river";
            this.lb_river.Size = new System.Drawing.Size(431, 328);
            this.lb_river.TabIndex = 0;
            // 
            // lb_bload
            // 
            this.lb_bload.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_bload.FormattingEnabled = true;
            this.lb_bload.ItemHeight = 18;
            this.lb_bload.Items.AddRange(new object[] {
            "1"});
            this.lb_bload.Location = new System.Drawing.Point(462, 116);
            this.lb_bload.Name = "lb_bload";
            this.lb_bload.Size = new System.Drawing.Size(431, 328);
            this.lb_bload.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Тип задачи";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.rb2XY);
            this.panel1.Controls.Add(this.rb1Y);
            this.panel1.Controls.Add(this.rb1X);
            this.panel1.Location = new System.Drawing.Point(19, 31);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(874, 44);
            this.panel1.TabIndex = 5;
            // 
            // rb2XY
            // 
            this.rb2XY.AutoSize = true;
            this.rb2XY.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rb2XY.Location = new System.Drawing.Point(510, 9);
            this.rb2XY.Name = "rb2XY";
            this.rb2XY.Size = new System.Drawing.Size(217, 22);
            this.rb2XY.TabIndex = 2;
            this.rb2XY.Text = "Плановая русловая задача";
            this.rb2XY.UseVisualStyleBackColor = true;
            this.rb2XY.Click += new System.EventHandler(this.rb1Y_Click);
            // 
            // rb1Y
            // 
            this.rb1Y.AutoSize = true;
            this.rb1Y.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rb1Y.Location = new System.Drawing.Point(254, 9);
            this.rb1Y.Name = "rb1Y";
            this.rb1Y.Size = new System.Drawing.Size(239, 22);
            this.rb1Y.TabIndex = 1;
            this.rb1Y.Text = "Русловой поток в створе реки";
            this.rb1Y.UseVisualStyleBackColor = true;
            this.rb1Y.Click += new System.EventHandler(this.rb1Y_Click);
            // 
            // rb1X
            // 
            this.rb1X.AutoSize = true;
            this.rb1X.Checked = true;
            this.rb1X.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rb1X.Location = new System.Drawing.Point(9, 9);
            this.rb1X.Name = "rb1X";
            this.rb1X.Size = new System.Drawing.Size(231, 22);
            this.rb1X.TabIndex = 0;
            this.rb1X.TabStop = true;
            this.rb1X.Text = "Продольный русловой поток";
            this.rb1X.UseVisualStyleBackColor = true;
            this.rb1X.Click += new System.EventHandler(this.rb1Y_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.buttonExit);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.buttonOk);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.lb_river);
            this.panel2.Controls.Add(this.lb_bload);
            this.panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panel2.Location = new System.Drawing.Point(8, 6);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(902, 525);
            this.panel2.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(462, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(317, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "Задача по расчету донных деформаций";
            // 
            // buttonExit
            // 
            this.buttonExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonExit.Location = new System.Drawing.Point(669, 454);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(222, 27);
            this.buttonExit.TabIndex = 9;
            this.buttonExit.Text = "Выход";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(13, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(376, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Задача по расчету гидрадинамического потока";
            // 
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonOk.Location = new System.Drawing.Point(669, 487);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(222, 27);
            this.buttonOk.TabIndex = 8;
            this.buttonOk.Text = "Создать задачу";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // FTaskManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 543);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FTaskManager";
            this.Text = "Менеджер русловых задач";
            this.Load += new System.EventHandler(this.FTaskManager_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lb_river;
        private System.Windows.Forms.ListBox lb_bload;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rb2XY;
        private System.Windows.Forms.RadioButton rb1Y;
        private System.Windows.Forms.RadioButton rb1X;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
    }
}

