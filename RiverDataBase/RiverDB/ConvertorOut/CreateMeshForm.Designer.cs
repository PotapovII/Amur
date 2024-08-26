namespace RiverDB.ConvertorOut
{
    partial class CreateMeshForm
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
            this.cbSegmentSplitting = new System.Windows.Forms.CheckBox();
            this.cbConformingDelaunay = new System.Windows.Forms.CheckBox();
            this.cbCreateContur = new System.Windows.Forms.CheckBox();
            this.checkBoxSM = new System.Windows.Forms.CheckBox();
            this.checkBoxReMeshDel = new System.Windows.Forms.CheckBox();
            this.nUDCountSmooth = new System.Windows.Forms.NumericUpDown();
            this.label32 = new System.Windows.Forms.Label();
            this.nUDMaximumAngle = new System.Windows.Forms.NumericUpDown();
            this.label31 = new System.Windows.Forms.Label();
            this.nUDMinimumAngle = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.cbConformDel = new System.Windows.Forms.CheckBox();
            this.cbConvex = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbSweepline = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.btnMesh = new System.Windows.Forms.Button();
            this.btnSmooth = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.slMaxArea = new System.Windows.Forms.TrackBar();
            this.label20 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbNumSeg = new System.Windows.Forms.Label();
            this.lbNumTri = new System.Windows.Forms.Label();
            this.lbNumVert = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbQuality = new System.Windows.Forms.CheckBox();
            this.gdI_Control1 = new RenderLib.GDI_Control();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.stStatResultTest = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.nUDCountSmooth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMaximumAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMinimumAngle)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slMaxArea)).BeginInit();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbSegmentSplitting
            // 
            this.cbSegmentSplitting.AutoSize = true;
            this.cbSegmentSplitting.Checked = true;
            this.cbSegmentSplitting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSegmentSplitting.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbSegmentSplitting.Location = new System.Drawing.Point(19, 161);
            this.cbSegmentSplitting.Name = "cbSegmentSplitting";
            this.cbSegmentSplitting.Size = new System.Drawing.Size(238, 24);
            this.cbSegmentSplitting.TabIndex = 138;
            this.cbSegmentSplitting.Text = "Создавать узлы на контуре";
            this.cbSegmentSplitting.UseVisualStyleBackColor = true;
            // 
            // cbConformingDelaunay
            // 
            this.cbConformingDelaunay.AutoSize = true;
            this.cbConformingDelaunay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbConformingDelaunay.Location = new System.Drawing.Point(237, 243);
            this.cbConformingDelaunay.Name = "cbConformingDelaunay";
            this.cbConformingDelaunay.Size = new System.Drawing.Size(195, 24);
            this.cbConformingDelaunay.TabIndex = 137;
            this.cbConformingDelaunay.Text = "Триангуляция Делоне";
            this.cbConformingDelaunay.UseVisualStyleBackColor = true;
            // 
            // cbCreateContur
            // 
            this.cbCreateContur.AutoSize = true;
            this.cbCreateContur.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbCreateContur.Location = new System.Drawing.Point(191, 357);
            this.cbCreateContur.Name = "cbCreateContur";
            this.cbCreateContur.Size = new System.Drawing.Size(245, 24);
            this.cbCreateContur.TabIndex = 136;
            this.cbCreateContur.Text = "Создавать выпуклый контур";
            this.cbCreateContur.UseVisualStyleBackColor = true;
            // 
            // checkBoxSM
            // 
            this.checkBoxSM.AutoSize = true;
            this.checkBoxSM.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBoxSM.Location = new System.Drawing.Point(19, 190);
            this.checkBoxSM.Name = "checkBoxSM";
            this.checkBoxSM.Size = new System.Drawing.Size(243, 24);
            this.checkBoxSM.TabIndex = 135;
            this.checkBoxSM.Text = "Сглаживание  триангуляции";
            this.checkBoxSM.UseVisualStyleBackColor = true;
            // 
            // checkBoxReMeshDel
            // 
            this.checkBoxReMeshDel.AutoSize = true;
            this.checkBoxReMeshDel.Checked = true;
            this.checkBoxReMeshDel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxReMeshDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBoxReMeshDel.Location = new System.Drawing.Point(19, 215);
            this.checkBoxReMeshDel.Name = "checkBoxReMeshDel";
            this.checkBoxReMeshDel.Size = new System.Drawing.Size(257, 24);
            this.checkBoxReMeshDel.TabIndex = 134;
            this.checkBoxReMeshDel.Text = "Перенумерация сетки Делоне";
            this.checkBoxReMeshDel.UseVisualStyleBackColor = true;
            // 
            // nUDCountSmooth
            // 
            this.nUDCountSmooth.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nUDCountSmooth.Location = new System.Drawing.Point(230, 100);
            this.nUDCountSmooth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nUDCountSmooth.Name = "nUDCountSmooth";
            this.nUDCountSmooth.Size = new System.Drawing.Size(50, 26);
            this.nUDCountSmooth.TabIndex = 133;
            this.nUDCountSmooth.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label32.Location = new System.Drawing.Point(19, 102);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(169, 20);
            this.label32.TabIndex = 132;
            this.label32.Text = "Циклов сглаживания";
            // 
            // nUDMaximumAngle
            // 
            this.nUDMaximumAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nUDMaximumAngle.Location = new System.Drawing.Point(230, 69);
            this.nUDMaximumAngle.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.nUDMaximumAngle.Minimum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nUDMaximumAngle.Name = "nUDMaximumAngle";
            this.nUDMaximumAngle.Size = new System.Drawing.Size(50, 26);
            this.nUDMaximumAngle.TabIndex = 131;
            this.nUDMaximumAngle.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label31.Location = new System.Drawing.Point(19, 71);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(161, 20);
            this.label31.TabIndex = 130;
            this.label31.Text = "Максимальный угол";
            // 
            // nUDMinimumAngle
            // 
            this.nUDMinimumAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nUDMinimumAngle.Location = new System.Drawing.Point(230, 39);
            this.nUDMinimumAngle.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nUDMinimumAngle.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nUDMinimumAngle.Name = "nUDMinimumAngle";
            this.nUDMinimumAngle.Size = new System.Drawing.Size(50, 26);
            this.nUDMinimumAngle.TabIndex = 129;
            this.nUDMinimumAngle.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label12.Location = new System.Drawing.Point(19, 42);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(154, 20);
            this.label12.TabIndex = 128;
            this.label12.Text = "Минимальный угол";
            // 
            // label23
            // 
            this.label23.BackColor = System.Drawing.Color.LightGray;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label23.ForeColor = System.Drawing.Color.Black;
            this.label23.Location = new System.Drawing.Point(19, 270);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(413, 73);
            this.label23.TabIndex = 139;
            this.label23.Text = "Убедитесь, что все треугольники в сетке действительно являются треугольниками Дел" +
    "оне, а не просто ограниченными Делоне.";
            // 
            // cbConformDel
            // 
            this.cbConformDel.AutoSize = true;
            this.cbConformDel.Checked = true;
            this.cbConformDel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConformDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbConformDel.Location = new System.Drawing.Point(19, 243);
            this.cbConformDel.Name = "cbConformDel";
            this.cbConformDel.Size = new System.Drawing.Size(199, 24);
            this.cbConformDel.TabIndex = 140;
            this.cbConformDel.Text = "Соответствие Делоне";
            this.cbConformDel.UseVisualStyleBackColor = true;
            // 
            // cbConvex
            // 
            this.cbConvex.AutoSize = true;
            this.cbConvex.Checked = true;
            this.cbConvex.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbConvex.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbConvex.Location = new System.Drawing.Point(19, 357);
            this.cbConvex.Name = "cbConvex";
            this.cbConvex.Size = new System.Drawing.Size(149, 24);
            this.cbConvex.TabIndex = 142;
            this.cbConvex.Text = "Выпуклая сетка";
            this.cbConvex.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.LightGray;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(19, 384);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(413, 73);
            this.label1.TabIndex = 141;
            this.label1.Text = "Используйте опцию выпуклой сетки, если выпуклая оболочка должна быть включена в в" +
    "ыходные данные.";
            // 
            // cbSweepline
            // 
            this.cbSweepline.AutoSize = true;
            this.cbSweepline.Checked = true;
            this.cbSweepline.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSweepline.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbSweepline.Location = new System.Drawing.Point(19, 465);
            this.cbSweepline.Name = "cbSweepline";
            this.cbSweepline.Size = new System.Drawing.Size(291, 24);
            this.cbSweepline.TabIndex = 144;
            this.cbSweepline.Text = "Использовать алгоритм Sweepline";
            this.cbSweepline.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.LightGray;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(19, 492);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(413, 73);
            this.label2.TabIndex = 143;
            this.label2.Text = "Используйте алгоритм Sweepline для триангуляции вместо стандартного алгоритма «Ра" +
    "зделяй и& Властвуй».";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuStrip1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.menuStrip1.Size = new System.Drawing.Size(1334, 25);
            this.menuStrip1.TabIndex = 145;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuFile
            // 
            this.menuFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileOpen,
            this.menuFileSave,
            this.toolStripSeparator3,
            this.toolStripSeparator2,
            this.menuFileQuit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 24);
            this.menuFile.Text = "File";
            // 
            // menuFileOpen
            // 
            this.menuFileOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.menuFileOpen.Name = "menuFileOpen";
            this.menuFileOpen.Size = new System.Drawing.Size(180, 22);
            this.menuFileOpen.Text = "Open";
            this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
            // 
            // menuFileSave
            // 
            this.menuFileSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.menuFileSave.Enabled = false;
            this.menuFileSave.Name = "menuFileSave";
            this.menuFileSave.Size = new System.Drawing.Size(180, 22);
            this.menuFileSave.Text = "Save";
            this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // menuFileQuit
            // 
            this.menuFileQuit.Name = "menuFileQuit";
            this.menuFileQuit.Size = new System.Drawing.Size(180, 22);
            this.menuFileQuit.Text = "Quit";
            this.menuFileQuit.Click += new System.EventHandler(this.menuFileQuit_Click);
            // 
            // btnMesh
            // 
            this.btnMesh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnMesh.Location = new System.Drawing.Point(302, 68);
            this.btnMesh.Name = "btnMesh";
            this.btnMesh.Size = new System.Drawing.Size(130, 28);
            this.btnMesh.TabIndex = 148;
            this.btnMesh.Text = "Триангуляция";
            this.btnMesh.UseVisualStyleBackColor = true;
            this.btnMesh.Click += new System.EventHandler(this.btnMesh_Click);
            // 
            // btnSmooth
            // 
            this.btnSmooth.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnSmooth.Location = new System.Drawing.Point(302, 102);
            this.btnSmooth.Name = "btnSmooth";
            this.btnSmooth.Size = new System.Drawing.Size(130, 28);
            this.btnSmooth.TabIndex = 149;
            this.btnSmooth.Text = "Сглаживание";
            this.btnSmooth.UseVisualStyleBackColor = true;
            this.btnSmooth.Click += new System.EventHandler(this.btnSmooth_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(21, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(196, 20);
            this.label3.TabIndex = 151;
            this.label3.Text = "Максимальная площадь";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(404, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 20);
            this.label4.TabIndex = 153;
            this.label4.Text = "0";
            // 
            // slMaxArea
            // 
            this.slMaxArea.Location = new System.Drawing.Point(223, 11);
            this.slMaxArea.Maximum = 100;
            this.slMaxArea.Name = "slMaxArea";
            this.slMaxArea.Size = new System.Drawing.Size(167, 45);
            this.slMaxArea.TabIndex = 154;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.ForeColor = System.Drawing.Color.Black;
            this.label20.Location = new System.Drawing.Point(20, 570);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(186, 21);
            this.label20.TabIndex = 161;
            this.label20.Text = "Информация по сетке";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(20, 615);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 20);
            this.label5.TabIndex = 158;
            this.label5.Text = "Сегментов:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(20, 637);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(124, 20);
            this.label6.TabIndex = 159;
            this.label6.Text = "Техугольников:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(20, 594);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 20);
            this.label7.TabIndex = 160;
            this.label7.Text = "Вершин:";
            // 
            // lbNumSeg
            // 
            this.lbNumSeg.BackColor = System.Drawing.SystemColors.Control;
            this.lbNumSeg.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbNumSeg.ForeColor = System.Drawing.Color.Black;
            this.lbNumSeg.Location = new System.Drawing.Point(145, 617);
            this.lbNumSeg.Name = "lbNumSeg";
            this.lbNumSeg.Size = new System.Drawing.Size(70, 17);
            this.lbNumSeg.TabIndex = 156;
            this.lbNumSeg.Text = "0";
            this.lbNumSeg.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbNumTri
            // 
            this.lbNumTri.BackColor = System.Drawing.SystemColors.Control;
            this.lbNumTri.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbNumTri.ForeColor = System.Drawing.Color.Black;
            this.lbNumTri.Location = new System.Drawing.Point(145, 639);
            this.lbNumTri.Name = "lbNumTri";
            this.lbNumTri.Size = new System.Drawing.Size(70, 18);
            this.lbNumTri.TabIndex = 155;
            this.lbNumTri.Text = "0";
            this.lbNumTri.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbNumVert
            // 
            this.lbNumVert.BackColor = System.Drawing.SystemColors.Control;
            this.lbNumVert.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbNumVert.ForeColor = System.Drawing.Color.Black;
            this.lbNumVert.Location = new System.Drawing.Point(145, 594);
            this.lbNumVert.Name = "lbNumVert";
            this.lbNumVert.Size = new System.Drawing.Size(70, 18);
            this.lbNumVert.TabIndex = 157;
            this.lbNumVert.Text = "0";
            this.lbNumVert.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.cbQuality);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label20);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.nUDMinimumAngle);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label31);
            this.panel1.Controls.Add(this.lbNumSeg);
            this.panel1.Controls.Add(this.nUDMaximumAngle);
            this.panel1.Controls.Add(this.lbNumTri);
            this.panel1.Controls.Add(this.label32);
            this.panel1.Controls.Add(this.lbNumVert);
            this.panel1.Controls.Add(this.nUDCountSmooth);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.checkBoxReMeshDel);
            this.panel1.Controls.Add(this.checkBoxSM);
            this.panel1.Controls.Add(this.btnSmooth);
            this.panel1.Controls.Add(this.cbCreateContur);
            this.panel1.Controls.Add(this.btnMesh);
            this.panel1.Controls.Add(this.cbConformingDelaunay);
            this.panel1.Controls.Add(this.cbSegmentSplitting);
            this.panel1.Controls.Add(this.cbSweepline);
            this.panel1.Controls.Add(this.label23);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cbConformDel);
            this.panel1.Controls.Add(this.cbConvex);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.slMaxArea);
            this.panel1.Location = new System.Drawing.Point(879, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(455, 682);
            this.panel1.TabIndex = 162;
            // 
            // cbQuality
            // 
            this.cbQuality.AutoSize = true;
            this.cbQuality.Checked = true;
            this.cbQuality.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbQuality.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbQuality.Location = new System.Drawing.Point(19, 131);
            this.cbQuality.Name = "cbQuality";
            this.cbQuality.Size = new System.Drawing.Size(234, 24);
            this.cbQuality.TabIndex = 162;
            this.cbQuality.Text = "Улучшение качества сетки";
            this.cbQuality.UseVisualStyleBackColor = true;
            // 
            // gdI_Control1
            // 
            this.gdI_Control1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gdI_Control1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gdI_Control1.Location = new System.Drawing.Point(1, 27);
            this.gdI_Control1.Name = "gdI_Control1";
            this.gdI_Control1.Size = new System.Drawing.Size(868, 657);
            this.gdI_Control1.TabIndex = 163;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.stStatResultTest});
            this.statusStrip1.Location = new System.Drawing.Point(0, 685);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1334, 26);
            this.statusStrip1.TabIndex = 164;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(83, 21);
            this.toolStripStatusLabel1.Text = "Результат:";
            // 
            // stStatResultTest
            // 
            this.stStatResultTest.BackColor = System.Drawing.SystemColors.Control;
            this.stStatResultTest.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.stStatResultTest.Name = "stStatResultTest";
            this.stStatResultTest.Size = new System.Drawing.Size(27, 21);
            this.stStatResultTest.Text = "ок";
            // 
            // CreateMeshForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ClientSize = new System.Drawing.Size(1334, 711);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.gdI_Control1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "CreateMeshForm";
            this.Text = "CreateMeshForm";
            ((System.ComponentModel.ISupportInitialize)(this.nUDCountSmooth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMaximumAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMinimumAngle)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.slMaxArea)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cbSegmentSplitting;
        private System.Windows.Forms.CheckBox cbConformingDelaunay;
        private System.Windows.Forms.CheckBox cbCreateContur;
        private System.Windows.Forms.CheckBox checkBoxSM;
        private System.Windows.Forms.CheckBox checkBoxReMeshDel;
        private System.Windows.Forms.NumericUpDown nUDCountSmooth;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.NumericUpDown nUDMaximumAngle;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.NumericUpDown nUDMinimumAngle;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.CheckBox cbConformDel;
        private System.Windows.Forms.CheckBox cbConvex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbSweepline;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem menuFileSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuFileQuit;
        private System.Windows.Forms.Button btnMesh;
        private System.Windows.Forms.Button btnSmooth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar slMaxArea;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbNumSeg;
        private System.Windows.Forms.Label lbNumTri;
        private System.Windows.Forms.Label lbNumVert;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbQuality;
        private RenderLib.GDI_Control gdI_Control1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel stStatResultTest;
    }
}