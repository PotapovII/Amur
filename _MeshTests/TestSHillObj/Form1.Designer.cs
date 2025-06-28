namespace TestSHillObj
{
    partial class Form1
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
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.tbCountsKnot = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbShow = new System.Windows.Forms.CheckBox();
            this.lbPoints = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(27, 131);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(167, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Генерация сетки";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(27, 102);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(167, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Генерация точек";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(27, 169);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(167, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Генерация сетки";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // tbCountsKnot
            // 
            this.tbCountsKnot.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbCountsKnot.Location = new System.Drawing.Point(94, 23);
            this.tbCountsKnot.Name = "tbCountsKnot";
            this.tbCountsKnot.Size = new System.Drawing.Size(100, 26);
            this.tbCountsKnot.TabIndex = 5;
            this.tbCountsKnot.Text = "5000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(24, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Узлов";
            // 
            // cbShow
            // 
            this.cbShow.AutoSize = true;
            this.cbShow.Checked = true;
            this.cbShow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbShow.Location = new System.Drawing.Point(27, 64);
            this.cbShow.Name = "cbShow";
            this.cbShow.Size = new System.Drawing.Size(157, 24);
            this.cbShow.TabIndex = 7;
            this.cbShow.Text = "Отрисовка узлов";
            this.cbShow.UseVisualStyleBackColor = true;
            // 
            // lbPoints
            // 
            this.lbPoints.AutoSize = true;
            this.lbPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbPoints.Location = new System.Drawing.Point(24, 207);
            this.lbPoints.Name = "lbPoints";
            this.lbPoints.Size = new System.Drawing.Size(18, 20);
            this.lbPoints.TabIndex = 8;
            this.lbPoints.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(981, 671);
            this.Controls.Add(this.lbPoints);
            this.Controls.Add(this.cbShow);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbCountsKnot);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox tbCountsKnot;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbShow;
        private System.Windows.Forms.Label lbPoints;
    }
}

