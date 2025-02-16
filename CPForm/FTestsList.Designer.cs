namespace CPForm
{
    partial class FTestsList
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
            this.bt_Select = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lb_DefaultTasks = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // bt_Select
            // 
            this.bt_Select.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_Select.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bt_Select.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bt_Select.Location = new System.Drawing.Point(296, 453);
            this.bt_Select.Name = "bt_Select";
            this.bt_Select.Size = new System.Drawing.Size(120, 29);
            this.bt_Select.TabIndex = 0;
            this.bt_Select.Text = "Старт";
            this.bt_Select.UseVisualStyleBackColor = true;
            this.bt_Select.Click += new System.EventHandler(this.bt_Select_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(399, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Тестовые задачи, запускаемые по умолчанию";
            // 
            // lb_DefaultTasks
            // 
            this.lb_DefaultTasks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb_DefaultTasks.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_DefaultTasks.FormattingEnabled = true;
            this.lb_DefaultTasks.ItemHeight = 20;
            this.lb_DefaultTasks.Location = new System.Drawing.Point(8, 49);
            this.lb_DefaultTasks.Name = "lb_DefaultTasks";
            this.lb_DefaultTasks.Size = new System.Drawing.Size(408, 384);
            this.lb_DefaultTasks.TabIndex = 2;
            this.lb_DefaultTasks.DoubleClick += new System.EventHandler(this.lb_DefaultTasks_DoubleClick);
            // 
            // FTestsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(422, 494);
            this.Controls.Add(this.lb_DefaultTasks);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bt_Select);
            this.Name = "FTestsList";
            this.Text = "Тесты";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_Select;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lb_DefaultTasks;
    }
}