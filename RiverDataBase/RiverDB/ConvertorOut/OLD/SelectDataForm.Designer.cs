namespace RiverDB.ConvertorOut
{
    partial class SelectDataForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectDataForm));
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btLoadData = new System.Windows.Forms.Button();
            this.cListBoxDates = new System.Windows.Forms.CheckedListBox();
            this.btSelectData = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControlState = new System.Windows.Forms.TabControl();
            this.tpDB = new System.Windows.Forms.TabPage();
            this.cbAllData = new System.Windows.Forms.CheckBox();
            this.panel10 = new System.Windows.Forms.Panel();
            this.rbMetrix = new System.Windows.Forms.RadioButton();
            this.rbGrad = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btSaveCloud = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tpCreateMesh = new System.Windows.Forms.TabPage();
            this.btCreateCountursMesh = new System.Windows.Forms.Button();
            this.btCreateFilterMesh = new System.Windows.Forms.Button();
            this.cbSegmentSplitting = new System.Windows.Forms.CheckBox();
            this.cbConformingDelaunay = new System.Windows.Forms.CheckBox();
            this.btCreateMesh = new System.Windows.Forms.Button();
            this.cbCreateContur = new System.Windows.Forms.CheckBox();
            this.checkBoxSM = new System.Windows.Forms.CheckBox();
            this.checkBoxReMeshDel = new System.Windows.Forms.CheckBox();
            this.nUDCountSmooth = new System.Windows.Forms.NumericUpDown();
            this.label32 = new System.Windows.Forms.Label();
            this.nUDMaximumAngle = new System.Windows.Forms.NumericUpDown();
            this.label31 = new System.Windows.Forms.Label();
            this.nUDMinimumAngle = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsb_Filter = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.statusPanel = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tbMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.gdI_ControlClouds1 = new RenderLib.GDI_ControlClouds();
            this.panel1.SuspendLayout();
            this.tabControlState.SuspendLayout();
            this.tpDB.SuspendLayout();
            this.panel10.SuspendLayout();
            this.tpCreateMesh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDCountSmooth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMaximumAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMinimumAngle)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.statusPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btLoadData
            // 
            this.btLoadData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btLoadData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btLoadData.Location = new System.Drawing.Point(22, 26);
            this.btLoadData.Name = "btLoadData";
            this.btLoadData.Size = new System.Drawing.Size(253, 26);
            this.btLoadData.TabIndex = 78;
            this.btLoadData.Text = "Загрузка фильтра";
            this.btLoadData.UseVisualStyleBackColor = true;
            this.btLoadData.Click += new System.EventHandler(this.btLoadData_Click);
            // 
            // cListBoxDates
            // 
            this.cListBoxDates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cListBoxDates.BackColor = System.Drawing.SystemColors.ControlLight;
            this.cListBoxDates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cListBoxDates.FormattingEnabled = true;
            this.cListBoxDates.Location = new System.Drawing.Point(19, 143);
            this.cListBoxDates.Name = "cListBoxDates";
            this.cListBoxDates.Size = new System.Drawing.Size(259, 550);
            this.cListBoxDates.TabIndex = 77;
            // 
            // btSelectData
            // 
            this.btSelectData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btSelectData.Enabled = false;
            this.btSelectData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btSelectData.Location = new System.Drawing.Point(22, 53);
            this.btSelectData.Name = "btSelectData";
            this.btSelectData.Size = new System.Drawing.Size(253, 26);
            this.btSelectData.TabIndex = 80;
            this.btSelectData.Text = "Загрузка данных";
            this.btSelectData.UseVisualStyleBackColor = true;
            this.btSelectData.Click += new System.EventHandler(this.btSelectData_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.tabControlState);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(927, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(301, 793);
            this.panel1.TabIndex = 85;
            // 
            // tabControlState
            // 
            this.tabControlState.Controls.Add(this.tpDB);
            this.tabControlState.Controls.Add(this.tpCreateMesh);
            this.tabControlState.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabControlState.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabControlState.Location = new System.Drawing.Point(4, 0);
            this.tabControlState.Name = "tabControlState";
            this.tabControlState.SelectedIndex = 0;
            this.tabControlState.Size = new System.Drawing.Size(293, 789);
            this.tabControlState.TabIndex = 92;
            // 
            // tpDB
            // 
            this.tpDB.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tpDB.Controls.Add(this.cbAllData);
            this.tpDB.Controls.Add(this.panel10);
            this.tpDB.Controls.Add(this.cListBoxDates);
            this.tpDB.Controls.Add(this.btLoadData);
            this.tpDB.Controls.Add(this.btSelectData);
            this.tpDB.Controls.Add(this.label1);
            this.tpDB.Controls.Add(this.btSaveCloud);
            this.tpDB.Controls.Add(this.label2);
            this.tpDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tpDB.Location = new System.Drawing.Point(4, 29);
            this.tpDB.Name = "tpDB";
            this.tpDB.Padding = new System.Windows.Forms.Padding(3);
            this.tpDB.Size = new System.Drawing.Size(285, 756);
            this.tpDB.TabIndex = 0;
            this.tpDB.Text = "Источник";
            // 
            // cbAllData
            // 
            this.cbAllData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAllData.AutoSize = true;
            this.cbAllData.BackColor = System.Drawing.SystemColors.ControlLight;
            this.cbAllData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbAllData.Location = new System.Drawing.Point(23, 110);
            this.cbAllData.Name = "cbAllData";
            this.cbAllData.Size = new System.Drawing.Size(56, 24);
            this.cbAllData.TabIndex = 88;
            this.cbAllData.Text = "Все";
            this.cbAllData.UseVisualStyleBackColor = false;
            this.cbAllData.CheckedChanged += new System.EventHandler(this.cbAllData_CheckedChanged);
            // 
            // panel10
            // 
            this.panel10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel10.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel10.Controls.Add(this.rbMetrix);
            this.panel10.Controls.Add(this.rbGrad);
            this.panel10.Location = new System.Drawing.Point(19, 105);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(256, 32);
            this.panel10.TabIndex = 87;
            // 
            // rbMetrix
            // 
            this.rbMetrix.AutoSize = true;
            this.rbMetrix.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbMetrix.Location = new System.Drawing.Point(172, 3);
            this.rbMetrix.Name = "rbMetrix";
            this.rbMetrix.Size = new System.Drawing.Size(78, 24);
            this.rbMetrix.TabIndex = 87;
            this.rbMetrix.Text = "Метры";
            this.rbMetrix.UseVisualStyleBackColor = true;
            // 
            // rbGrad
            // 
            this.rbGrad.AutoSize = true;
            this.rbGrad.Checked = true;
            this.rbGrad.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rbGrad.Location = new System.Drawing.Point(59, 2);
            this.rbGrad.Name = "rbGrad";
            this.rbGrad.Size = new System.Drawing.Size(91, 24);
            this.rbGrad.TabIndex = 88;
            this.rbGrad.TabStop = true;
            this.rbGrad.Text = "Градусы";
            this.rbGrad.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(11, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 20);
            this.label1.TabIndex = 83;
            this.label1.Text = "Операции с БД";
            // 
            // btSaveCloud
            // 
            this.btSaveCloud.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btSaveCloud.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btSaveCloud.Location = new System.Drawing.Point(29, 723);
            this.btSaveCloud.Name = "btSaveCloud";
            this.btSaveCloud.Size = new System.Drawing.Size(221, 27);
            this.btSaveCloud.TabIndex = 86;
            this.btSaveCloud.Text = "Сохранение  выборки";
            this.btSaveCloud.UseVisualStyleBackColor = true;
            this.btSaveCloud.Click += new System.EventHandler(this.btSaveCloud_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(11, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 20);
            this.label2.TabIndex = 84;
            this.label2.Text = "Настройка фильтра";
            // 
            // tpCreateMesh
            // 
            this.tpCreateMesh.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tpCreateMesh.Controls.Add(this.btCreateCountursMesh);
            this.tpCreateMesh.Controls.Add(this.btCreateFilterMesh);
            this.tpCreateMesh.Controls.Add(this.cbSegmentSplitting);
            this.tpCreateMesh.Controls.Add(this.cbConformingDelaunay);
            this.tpCreateMesh.Controls.Add(this.btCreateMesh);
            this.tpCreateMesh.Controls.Add(this.cbCreateContur);
            this.tpCreateMesh.Controls.Add(this.checkBoxSM);
            this.tpCreateMesh.Controls.Add(this.checkBoxReMeshDel);
            this.tpCreateMesh.Controls.Add(this.nUDCountSmooth);
            this.tpCreateMesh.Controls.Add(this.label32);
            this.tpCreateMesh.Controls.Add(this.nUDMaximumAngle);
            this.tpCreateMesh.Controls.Add(this.label31);
            this.tpCreateMesh.Controls.Add(this.nUDMinimumAngle);
            this.tpCreateMesh.Controls.Add(this.label12);
            this.tpCreateMesh.Location = new System.Drawing.Point(4, 29);
            this.tpCreateMesh.Name = "tpCreateMesh";
            this.tpCreateMesh.Size = new System.Drawing.Size(285, 756);
            this.tpCreateMesh.TabIndex = 1;
            this.tpCreateMesh.Text = "Сетка";
            // 
            // btCreateCountursMesh
            // 
            this.btCreateCountursMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCreateCountursMesh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCreateCountursMesh.Location = new System.Drawing.Point(13, 348);
            this.btCreateCountursMesh.Name = "btCreateCountursMesh";
            this.btCreateCountursMesh.Size = new System.Drawing.Size(245, 27);
            this.btCreateCountursMesh.TabIndex = 128;
            this.btCreateCountursMesh.Text = "Сетка с контуром";
            this.btCreateCountursMesh.UseVisualStyleBackColor = true;
            this.btCreateCountursMesh.Click += new System.EventHandler(this.btCreateCountursMesh_Click);
            // 
            // btCreateFilterMesh
            // 
            this.btCreateFilterMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCreateFilterMesh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCreateFilterMesh.Location = new System.Drawing.Point(12, 315);
            this.btCreateFilterMesh.Name = "btCreateFilterMesh";
            this.btCreateFilterMesh.Size = new System.Drawing.Size(245, 27);
            this.btCreateFilterMesh.TabIndex = 88;
            this.btCreateFilterMesh.Text = "Сетка с фильтром";
            this.btCreateFilterMesh.UseVisualStyleBackColor = true;
            this.btCreateFilterMesh.Click += new System.EventHandler(this.btCreateFilterMesh_Click);
            // 
            // cbSegmentSplitting
            // 
            this.cbSegmentSplitting.AutoSize = true;
            this.cbSegmentSplitting.Checked = true;
            this.cbSegmentSplitting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSegmentSplitting.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbSegmentSplitting.Location = new System.Drawing.Point(12, 121);
            this.cbSegmentSplitting.Name = "cbSegmentSplitting";
            this.cbSegmentSplitting.Size = new System.Drawing.Size(238, 24);
            this.cbSegmentSplitting.TabIndex = 127;
            this.cbSegmentSplitting.Text = "Создавать узлы на контуре";
            this.cbSegmentSplitting.UseVisualStyleBackColor = true;
            // 
            // cbConformingDelaunay
            // 
            this.cbConformingDelaunay.AutoSize = true;
            this.cbConformingDelaunay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbConformingDelaunay.Location = new System.Drawing.Point(12, 149);
            this.cbConformingDelaunay.Name = "cbConformingDelaunay";
            this.cbConformingDelaunay.Size = new System.Drawing.Size(195, 24);
            this.cbConformingDelaunay.TabIndex = 126;
            this.cbConformingDelaunay.Text = "Триангуляция Делоне";
            this.cbConformingDelaunay.UseVisualStyleBackColor = true;
            // 
            // btCreateMesh
            // 
            this.btCreateMesh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCreateMesh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btCreateMesh.Location = new System.Drawing.Point(12, 282);
            this.btCreateMesh.Name = "btCreateMesh";
            this.btCreateMesh.Size = new System.Drawing.Size(245, 27);
            this.btCreateMesh.TabIndex = 87;
            this.btCreateMesh.Text = "Создание сетки слоя";
            this.btCreateMesh.UseVisualStyleBackColor = true;
            this.btCreateMesh.Click += new System.EventHandler(this.btCreateMesh_Click);
            // 
            // cbCreateContur
            // 
            this.cbCreateContur.AutoSize = true;
            this.cbCreateContur.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbCreateContur.Location = new System.Drawing.Point(12, 177);
            this.cbCreateContur.Name = "cbCreateContur";
            this.cbCreateContur.Size = new System.Drawing.Size(245, 24);
            this.cbCreateContur.TabIndex = 125;
            this.cbCreateContur.Text = "Создавать выпуклый контур";
            this.cbCreateContur.UseVisualStyleBackColor = true;
            // 
            // checkBoxSM
            // 
            this.checkBoxSM.AutoSize = true;
            this.checkBoxSM.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBoxSM.Location = new System.Drawing.Point(12, 204);
            this.checkBoxSM.Name = "checkBoxSM";
            this.checkBoxSM.Size = new System.Drawing.Size(243, 24);
            this.checkBoxSM.TabIndex = 124;
            this.checkBoxSM.Text = "Сглаживание  триангуляции";
            this.checkBoxSM.UseVisualStyleBackColor = true;
            // 
            // checkBoxReMeshDel
            // 
            this.checkBoxReMeshDel.AutoSize = true;
            this.checkBoxReMeshDel.Checked = true;
            this.checkBoxReMeshDel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxReMeshDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBoxReMeshDel.Location = new System.Drawing.Point(12, 229);
            this.checkBoxReMeshDel.Name = "checkBoxReMeshDel";
            this.checkBoxReMeshDel.Size = new System.Drawing.Size(257, 24);
            this.checkBoxReMeshDel.TabIndex = 123;
            this.checkBoxReMeshDel.Text = "Перенумерация сетки Делоне";
            this.checkBoxReMeshDel.UseVisualStyleBackColor = true;
            // 
            // nUDCountSmooth
            // 
            this.nUDCountSmooth.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nUDCountSmooth.Location = new System.Drawing.Point(201, 86);
            this.nUDCountSmooth.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nUDCountSmooth.Name = "nUDCountSmooth";
            this.nUDCountSmooth.Size = new System.Drawing.Size(50, 26);
            this.nUDCountSmooth.TabIndex = 120;
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
            this.label32.Location = new System.Drawing.Point(12, 88);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(169, 20);
            this.label32.TabIndex = 119;
            this.label32.Text = "Циклов сглаживания";
            // 
            // nUDMaximumAngle
            // 
            this.nUDMaximumAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nUDMaximumAngle.Location = new System.Drawing.Point(201, 59);
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
            this.nUDMaximumAngle.TabIndex = 118;
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
            this.label31.Location = new System.Drawing.Point(12, 61);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(161, 20);
            this.label31.TabIndex = 117;
            this.label31.Text = "Максимальный угол";
            // 
            // nUDMinimumAngle
            // 
            this.nUDMinimumAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nUDMinimumAngle.Location = new System.Drawing.Point(201, 32);
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
            this.nUDMinimumAngle.TabIndex = 116;
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
            this.label12.Location = new System.Drawing.Point(12, 32);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(154, 20);
            this.label12.TabIndex = 115;
            this.label12.Text = "Минимальный угол";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.tsbRefresh,
            this.tsb_Filter,
            this.toolStripButton2,
            this.toolStripButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(927, 31);
            this.toolStrip1.TabIndex = 88;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::RiverDB.Properties.Resources.Convert;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.btLoadData_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbRefresh.Image = global::RiverDB.Properties.Resources.Cloud;
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Size = new System.Drawing.Size(28, 28);
            this.tsbRefresh.Text = "Обновить набор";
            this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
            // 
            // tsb_Filter
            // 
            this.tsb_Filter.CheckOnClick = true;
            this.tsb_Filter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsb_Filter.Image = global::RiverDB.Properties.Resources.FLCloud;
            this.tsb_Filter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsb_Filter.Name = "tsb_Filter";
            this.tsb_Filter.Size = new System.Drawing.Size(28, 28);
            this.tsb_Filter.Text = "F";
            this.tsb_Filter.Click += new System.EventHandler(this.tsb_Filter_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::RiverDB.Properties.Resources.NCloud;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::RiverDB.Properties.Resources.MeshCloud;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // statusPanel
            // 
            this.statusPanel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.statusPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tbMessage});
            this.statusPanel.Location = new System.Drawing.Point(0, 767);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Size = new System.Drawing.Size(927, 26);
            this.statusPanel.TabIndex = 89;
            this.statusPanel.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(60, 21);
            this.toolStripStatusLabel1.Text = "Статус:";
            // 
            // tbMessage
            // 
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.Size = new System.Drawing.Size(56, 21);
            this.tbMessage.Text = "норма";
            // 
            // gdI_ControlClouds1
            // 
            this.gdI_ControlClouds1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gdI_ControlClouds1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gdI_ControlClouds1.Conturs = ((System.Collections.Generic.List<GeometryLib.CloudKnot>)(resources.GetObject("gdI_ControlClouds1.Conturs")));
            this.gdI_ControlClouds1.FilterPoints = new System.Drawing.PointF[] {
        ((System.Drawing.PointF)(resources.GetObject("gdI_ControlClouds1.FilterPoints"))),
        ((System.Drawing.PointF)(resources.GetObject("gdI_ControlClouds1.FilterPoints1")))};
            this.gdI_ControlClouds1.Location = new System.Drawing.Point(-6, 35);
            this.gdI_ControlClouds1.Name = "gdI_ControlClouds1";
            this.gdI_ControlClouds1.Size = new System.Drawing.Size(896, 729);
            this.gdI_ControlClouds1.TabIndex = 84;
            this.gdI_ControlClouds1.Load += new System.EventHandler(this.gdI_ControlClouds1_Load);
            // 
            // SelectDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1228, 793);
            this.Controls.Add(this.statusPanel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.gdI_ControlClouds1);
            this.Controls.Add(this.panel1);
            this.Name = "SelectDataForm";
            this.Text = "Оцифровка местности";
            this.Load += new System.EventHandler(this.SelectDataForm_Load);
            this.panel1.ResumeLayout(false);
            this.tabControlState.ResumeLayout(false);
            this.tpDB.ResumeLayout(false);
            this.tpDB.PerformLayout();
            this.panel10.ResumeLayout(false);
            this.panel10.PerformLayout();
            this.tpCreateMesh.ResumeLayout(false);
            this.tpCreateMesh.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUDCountSmooth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMaximumAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMinimumAngle)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusPanel.ResumeLayout(false);
            this.statusPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btLoadData;
        private System.Windows.Forms.CheckedListBox cListBoxDates;
        private System.Windows.Forms.Button btSelectData;
        private RenderLib.GDI_ControlClouds gdI_ControlClouds1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btSaveCloud;
        private System.Windows.Forms.Button btCreateMesh;
        private System.Windows.Forms.Button btCreateFilterMesh;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton tsb_Filter;
        private System.Windows.Forms.StatusStrip statusPanel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tbMessage;
        private System.Windows.Forms.TabControl tabControlState;
        private System.Windows.Forms.TabPage tpDB;
        private System.Windows.Forms.TabPage tpCreateMesh;
        private System.Windows.Forms.CheckBox cbSegmentSplitting;
        private System.Windows.Forms.CheckBox cbConformingDelaunay;
        private System.Windows.Forms.CheckBox cbCreateContur;
        private System.Windows.Forms.CheckBox checkBoxReMeshDel;
        private System.Windows.Forms.NumericUpDown nUDCountSmooth;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.NumericUpDown nUDMaximumAngle;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.NumericUpDown nUDMinimumAngle;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.RadioButton rbGrad;
        private System.Windows.Forms.RadioButton rbMetrix;
        private System.Windows.Forms.CheckBox cbAllData;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Button btCreateCountursMesh;
        private System.Windows.Forms.CheckBox checkBoxSM;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}