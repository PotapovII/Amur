
namespace RenderLib
{
    partial class BMShow
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
            this.tabControlOption = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxScaleYmin = new System.Windows.Forms.TextBox();
            this.tbYLine = new System.Windows.Forms.TextBox();
            this.cbLine = new System.Windows.Forms.CheckBox();
            this.cbRevers = new System.Windows.Forms.CheckBox();
            this.checScale = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbScale = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxScaleXmax = new System.Windows.Forms.TextBox();
            this.textBoxScaleXmin = new System.Windows.Forms.TextBox();
            this.btActive = new System.Windows.Forms.Button();
            this.rbManual = new System.Windows.Forms.RadioButton();
            this.textBoxScaleYmax = new System.Windows.Forms.TextBox();
            this.rbAuto = new System.Windows.Forms.RadioButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btFont = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.btColor = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cListBoxFiltr = new System.Windows.Forms.CheckedListBox();
            this.cbCoord2K = new System.Windows.Forms.CheckBox();
            this.pBox = new System.Windows.Forms.PictureBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.tabControlOption.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlOption
            // 
            this.tabControlOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlOption.Controls.Add(this.tabPage1);
            this.tabControlOption.Controls.Add(this.tabPage2);
            this.tabControlOption.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabControlOption.Location = new System.Drawing.Point(873, 369);
            this.tabControlOption.Name = "tabControlOption";
            this.tabControlOption.SelectedIndex = 0;
            this.tabControlOption.Size = new System.Drawing.Size(329, 324);
            this.tabControlOption.TabIndex = 87;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(321, 295);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Масштаб";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.textBoxScaleYmin);
            this.panel1.Controls.Add(this.tbYLine);
            this.panel1.Controls.Add(this.cbLine);
            this.panel1.Controls.Add(this.cbRevers);
            this.panel1.Controls.Add(this.checScale);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cbScale);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.textBoxScaleXmax);
            this.panel1.Controls.Add(this.textBoxScaleXmin);
            this.panel1.Controls.Add(this.btActive);
            this.panel1.Controls.Add(this.rbManual);
            this.panel1.Controls.Add(this.textBoxScaleYmax);
            this.panel1.Controls.Add(this.rbAuto);
            this.panel1.Location = new System.Drawing.Point(-10, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(325, 283);
            this.panel1.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(25, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 20);
            this.label5.TabIndex = 85;
            this.label5.Text = "Ymin";
            // 
            // textBoxScaleYmin
            // 
            this.textBoxScaleYmin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxScaleYmin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxScaleYmin.Location = new System.Drawing.Point(79, 76);
            this.textBoxScaleYmin.Name = "textBoxScaleYmin";
            this.textBoxScaleYmin.Size = new System.Drawing.Size(239, 26);
            this.textBoxScaleYmin.TabIndex = 84;
            this.textBoxScaleYmin.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            this.textBoxScaleYmin.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            // 
            // tbYLine
            // 
            this.tbYLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbYLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbYLine.Location = new System.Drawing.Point(119, 228);
            this.tbYLine.Name = "tbYLine";
            this.tbYLine.Size = new System.Drawing.Size(197, 26);
            this.tbYLine.TabIndex = 83;
            // 
            // cbLine
            // 
            this.cbLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLine.AutoSize = true;
            this.cbLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbLine.Location = new System.Drawing.Point(21, 228);
            this.cbLine.Name = "cbLine";
            this.cbLine.Size = new System.Drawing.Size(92, 24);
            this.cbLine.TabIndex = 82;
            this.cbLine.Text = "Уровень";
            this.cbLine.UseVisualStyleBackColor = true;
            this.cbLine.CheckedChanged += new System.EventHandler(this.cbLine_CheckedChanged);
            // 
            // cbRevers
            // 
            this.cbRevers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbRevers.AutoSize = true;
            this.cbRevers.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbRevers.Location = new System.Drawing.Point(206, 170);
            this.cbRevers.Name = "cbRevers";
            this.cbRevers.Size = new System.Drawing.Size(82, 24);
            this.cbRevers.TabIndex = 81;
            this.cbRevers.Text = "Реверс";
            this.cbRevers.UseVisualStyleBackColor = true;
            this.cbRevers.CheckedChanged += new System.EventHandler(this.cbRevers_CheckedChanged);
            // 
            // checScale
            // 
            this.checScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checScale.AutoSize = true;
            this.checScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checScale.Location = new System.Drawing.Point(21, 198);
            this.checScale.Name = "checScale";
            this.checScale.Size = new System.Drawing.Size(181, 24);
            this.checScale.TabIndex = 21;
            this.checScale.Text = "Сохранить масштаб";
            this.checScale.UseVisualStyleBackColor = true;
            this.checScale.CheckedChanged += new System.EventHandler(this.checScale_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(24, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 20);
            this.label2.TabIndex = 20;
            this.label2.Text = "Ymax";
            // 
            // cbScale
            // 
            this.cbScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbScale.AutoSize = true;
            this.cbScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbScale.Location = new System.Drawing.Point(21, 168);
            this.cbScale.Name = "cbScale";
            this.cbScale.Size = new System.Drawing.Size(138, 24);
            this.cbScale.TabIndex = 15;
            this.cbScale.Text = "Авто масштаб";
            this.cbScale.UseVisualStyleBackColor = true;
            this.cbScale.CheckedChanged += new System.EventHandler(this.cbScale_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(27, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 20);
            this.label4.TabIndex = 19;
            this.label4.Text = "xb";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(27, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 20);
            this.label3.TabIndex = 18;
            this.label3.Text = "xa";
            // 
            // textBoxScaleXmax
            // 
            this.textBoxScaleXmax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxScaleXmax.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxScaleXmax.Location = new System.Drawing.Point(79, 134);
            this.textBoxScaleXmax.Name = "textBoxScaleXmax";
            this.textBoxScaleXmax.Size = new System.Drawing.Size(239, 26);
            this.textBoxScaleXmax.TabIndex = 17;
            this.textBoxScaleXmax.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            this.textBoxScaleXmax.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            // 
            // textBoxScaleXmin
            // 
            this.textBoxScaleXmin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxScaleXmin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxScaleXmin.Location = new System.Drawing.Point(79, 105);
            this.textBoxScaleXmin.Name = "textBoxScaleXmin";
            this.textBoxScaleXmin.Size = new System.Drawing.Size(239, 26);
            this.textBoxScaleXmin.TabIndex = 16;
            this.textBoxScaleXmin.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            this.textBoxScaleXmin.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            // 
            // btActive
            // 
            this.btActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btActive.BackColor = System.Drawing.SystemColors.Control;
            this.btActive.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btActive.Location = new System.Drawing.Point(209, 199);
            this.btActive.Name = "btActive";
            this.btActive.Size = new System.Drawing.Size(104, 24);
            this.btActive.TabIndex = 15;
            this.btActive.Text = "Ок";
            this.btActive.UseVisualStyleBackColor = false;
            this.btActive.Click += new System.EventHandler(this.rbAuto_CheckedChanged);
            // 
            // rbManual
            // 
            this.rbManual.AutoSize = true;
            this.rbManual.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbManual.Location = new System.Drawing.Point(134, 18);
            this.rbManual.Name = "rbManual";
            this.rbManual.Size = new System.Drawing.Size(69, 21);
            this.rbManual.TabIndex = 13;
            this.rbManual.Text = "Выбор";
            this.rbManual.UseVisualStyleBackColor = true;
            this.rbManual.CheckedChanged += new System.EventHandler(this.rbAuto_CheckedChanged);
            // 
            // textBoxScaleYmax
            // 
            this.textBoxScaleYmax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxScaleYmax.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxScaleYmax.Location = new System.Drawing.Point(79, 47);
            this.textBoxScaleYmax.Name = "textBoxScaleYmax";
            this.textBoxScaleYmax.Size = new System.Drawing.Size(239, 26);
            this.textBoxScaleYmax.TabIndex = 10;
            this.textBoxScaleYmax.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            this.textBoxScaleYmax.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBoxScale_MouseClick);
            // 
            // rbAuto
            // 
            this.rbAuto.AutoSize = true;
            this.rbAuto.Checked = true;
            this.rbAuto.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbAuto.Location = new System.Drawing.Point(52, 18);
            this.rbAuto.Name = "rbAuto";
            this.rbAuto.Size = new System.Drawing.Size(57, 21);
            this.rbAuto.TabIndex = 12;
            this.rbAuto.TabStop = true;
            this.rbAuto.Text = "Авто";
            this.rbAuto.UseVisualStyleBackColor = true;
            this.rbAuto.CheckedChanged += new System.EventHandler(this.rbAuto_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(321, 295);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Графика";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel2.Controls.Add(this.btFont);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.numericUpDown1);
            this.panel2.Controls.Add(this.btColor);
            this.panel2.Location = new System.Drawing.Point(-2, 7);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(325, 204);
            this.panel2.TabIndex = 13;
            // 
            // btFont
            // 
            this.btFont.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btFont.Location = new System.Drawing.Point(21, 95);
            this.btFont.Name = "btFont";
            this.btFont.Size = new System.Drawing.Size(193, 28);
            this.btFont.TabIndex = 85;
            this.btFont.Text = "Размер подписей";
            this.btFont.UseVisualStyleBackColor = true;
            this.btFont.Click += new System.EventHandler(this.btFont_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(17, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(134, 20);
            this.label6.TabIndex = 84;
            this.label6.Text = "Толщина кривых";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.numericUpDown1.Location = new System.Drawing.Point(157, 19);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(57, 23);
            this.numericUpDown1.TabIndex = 83;
            this.numericUpDown1.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // btColor
            // 
            this.btColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btColor.Location = new System.Drawing.Point(21, 61);
            this.btColor.Name = "btColor";
            this.btColor.Size = new System.Drawing.Size(193, 28);
            this.btColor.TabIndex = 82;
            this.btColor.Text = "Цвет кривых";
            this.btColor.UseVisualStyleBackColor = true;
            this.btColor.Click += new System.EventHandler(this.button3_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBox1.Location = new System.Drawing.Point(877, 53);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(56, 24);
            this.checkBox1.TabIndex = 86;
            this.checkBox1.Text = "Все";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(896, 699);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 25);
            this.button1.TabIndex = 85;
            this.button1.Text = "Сохранить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.Location = new System.Drawing.Point(1070, 699);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(115, 25);
            this.button2.TabIndex = 84;
            this.button2.Text = "Закрыть";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(1082, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 20);
            this.label1.TabIndex = 83;
            this.label1.Text = "Список кривых";
            // 
            // cListBoxFiltr
            // 
            this.cListBoxFiltr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cListBoxFiltr.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.cListBoxFiltr.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cListBoxFiltr.FormattingEnabled = true;
            this.cListBoxFiltr.Location = new System.Drawing.Point(873, 80);
            this.cListBoxFiltr.Name = "cListBoxFiltr";
            this.cListBoxFiltr.Size = new System.Drawing.Size(329, 340);
            this.cListBoxFiltr.TabIndex = 82;
            this.cListBoxFiltr.SelectedIndexChanged += new System.EventHandler(this.cListBoxFiltr_SelectedIndexChanged);
            // 
            // cbCoord2K
            // 
            this.cbCoord2K.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCoord2K.AutoSize = true;
            this.cbCoord2K.Checked = true;
            this.cbCoord2K.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCoord2K.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbCoord2K.Location = new System.Drawing.Point(877, 23);
            this.cbCoord2K.Name = "cbCoord2K";
            this.cbCoord2K.Size = new System.Drawing.Size(149, 24);
            this.cbCoord2K.TabIndex = 81;
            this.cbCoord2K.Text = "Полный график";
            this.cbCoord2K.UseVisualStyleBackColor = true;
            this.cbCoord2K.CheckedChanged += new System.EventHandler(this.cbCoord2K_CheckedChanged);
            // 
            // pBox
            // 
            this.pBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pBox.BackColor = System.Drawing.SystemColors.Window;
            this.pBox.Location = new System.Drawing.Point(8, 17);
            this.pBox.Name = "pBox";
            this.pBox.Size = new System.Drawing.Size(860, 712);
            this.pBox.TabIndex = 80;
            this.pBox.TabStop = false;
            // 
            // BMShow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1210, 745);
            this.Controls.Add(this.tabControlOption);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cListBoxFiltr);
            this.Controls.Add(this.cbCoord2K);
            this.Controls.Add(this.pBox);
            this.Name = "BMShow";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.BMShow_Paint);
            this.tabControlOption.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlOption;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxScaleYmin;
        private System.Windows.Forms.TextBox tbYLine;
        private System.Windows.Forms.CheckBox cbLine;
        private System.Windows.Forms.CheckBox cbRevers;
        private System.Windows.Forms.CheckBox checScale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbScale;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxScaleXmax;
        private System.Windows.Forms.TextBox textBoxScaleXmin;
        private System.Windows.Forms.Button btActive;
        private System.Windows.Forms.RadioButton rbManual;
        private System.Windows.Forms.TextBox textBoxScaleYmax;
        private System.Windows.Forms.RadioButton rbAuto;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btFont;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button btColor;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox cListBoxFiltr;
        private System.Windows.Forms.CheckBox cbCoord2K;
        private System.Windows.Forms.PictureBox pBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.FontDialog fontDialog1;
    }
}