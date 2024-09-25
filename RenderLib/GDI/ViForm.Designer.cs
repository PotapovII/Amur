
namespace RenderLib
{
    partial class ViForm
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
            this.gdI_Control1 = new RenderLib.GDI_Control();
            this.SuspendLayout();
            // 
            // gdI_Control1
            // 
            this.gdI_Control1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gdI_Control1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gdI_Control1.Location = new System.Drawing.Point(0, 0);
            this.gdI_Control1.Name = "gdI_Control1";
            this.gdI_Control1.Size = new System.Drawing.Size(991, 712);
            this.gdI_Control1.TabIndex = 0;
            // 
            // ViForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(991, 712);
            this.Controls.Add(this.gdI_Control1);
            this.Name = "ViForm";
            this.Text = "ViForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ViForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private GDI_Control gdI_Control1;
    }
}