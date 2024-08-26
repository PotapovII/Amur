
namespace RenderLib
{
    partial class FVCurves
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
            this.gdI_Curves_Control1 = new RenderLib.GDI_Curves_Control();
            this.SuspendLayout();
            // 
            // gdI_Curves_Control1
            // 
            this.gdI_Curves_Control1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gdI_Curves_Control1.Location = new System.Drawing.Point(13, 13);
            this.gdI_Curves_Control1.Name = "gdI_Curves_Control1";
            this.gdI_Curves_Control1.Size = new System.Drawing.Size(760, 699);
            this.gdI_Curves_Control1.TabIndex = 0;
            // 
            // FVCurves
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(779, 718);
            this.Controls.Add(this.gdI_Curves_Control1);
            this.Name = "FVCurves";
            this.Text = "FVCurves";
            this.ResumeLayout(false);

        }
        #endregion

        private GDI_Curves_Control gdI_Curves_Control1;
    }
}