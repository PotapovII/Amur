namespace TestSUPG
{
    partial class FVortexStream
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_w = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lb_MeshGen = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lb_Geometry = new System.Windows.Forms.ListBox();
            this.label40 = new System.Windows.Forms.Label();
            this.lb_Algebra = new System.Windows.Forms.ListBox();
            this.checkBoxView = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.textBoxDiam = new System.Windows.Forms.TextBox();
            this.lb_Ring = new System.Windows.Forms.ListBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.tb_J = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cb_Mu_YZ = new System.Windows.Forms.CheckBox();
            this.balel_Us = new System.Windows.Forms.Label();
            this.ls_Type__U_star = new System.Windows.Forms.ListBox();
            this.cb_smoothing = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listBoxAMu = new System.Windows.Forms.ListBox();
            this.label41 = new System.Windows.Forms.Label();
            this.cbBoundaryG2_Ux = new System.Windows.Forms.CheckBox();
            this.lb_CrossNamber = new System.Windows.Forms.ListBox();
            this.label37 = new System.Windows.Forms.Label();
            this.lb_VortexBC_G2 = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.listBoxAMu2 = new System.Windows.Forms.ListBox();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Info;
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.tb_w);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.lb_MeshGen);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.lb_Geometry);
            this.panel2.Controls.Add(this.label40);
            this.panel2.Controls.Add(this.lb_Algebra);
            this.panel2.Controls.Add(this.checkBoxView);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.label35);
            this.panel2.Controls.Add(this.textBoxDiam);
            this.panel2.Controls.Add(this.lb_Ring);
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.label34);
            this.panel2.Controls.Add(this.label32);
            this.panel2.Controls.Add(this.tb_J);
            this.panel2.Location = new System.Drawing.Point(13, 13);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(263, 629);
            this.panel2.TabIndex = 52;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(11, 128);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 20);
            this.label5.TabIndex = 138;
            this.label5.Text = "Релаксация";
            // 
            // tb_w
            // 
            this.tb_w.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb_w.Location = new System.Drawing.Point(167, 128);
            this.tb_w.Margin = new System.Windows.Forms.Padding(4);
            this.tb_w.Name = "tb_w";
            this.tb_w.Size = new System.Drawing.Size(81, 23);
            this.tb_w.TabIndex = 139;
            this.tb_w.Text = "0.3";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(18, 255);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(148, 20);
            this.label4.TabIndex = 137;
            this.label4.Text = "Генераторы сетки";
            // 
            // lb_MeshGen
            // 
            this.lb_MeshGen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_MeshGen.FormattingEnabled = true;
            this.lb_MeshGen.ItemHeight = 20;
            this.lb_MeshGen.Items.AddRange(new object[] {
            "Инкрементный",
            "Карта треугольный",
            "Карта комплекс"});
            this.lb_MeshGen.Location = new System.Drawing.Point(15, 279);
            this.lb_MeshGen.Margin = new System.Windows.Forms.Padding(4);
            this.lb_MeshGen.Name = "lb_MeshGen";
            this.lb_MeshGen.Size = new System.Drawing.Size(233, 64);
            this.lb_MeshGen.TabIndex = 136;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(15, 176);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 20);
            this.label3.TabIndex = 135;
            this.label3.Text = "Геометрия канала";
            // 
            // lb_Geometry
            // 
            this.lb_Geometry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_Geometry.FormattingEnabled = true;
            this.lb_Geometry.ItemHeight = 20;
            this.lb_Geometry.Items.AddRange(new object[] {
            "Траперция",
            "Парабола"});
            this.lb_Geometry.Location = new System.Drawing.Point(15, 200);
            this.lb_Geometry.Margin = new System.Windows.Forms.Padding(4);
            this.lb_Geometry.Name = "lb_Geometry";
            this.lb_Geometry.Size = new System.Drawing.Size(233, 44);
            this.lb_Geometry.TabIndex = 134;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label40.Location = new System.Drawing.Point(18, 432);
            this.label40.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(123, 20);
            this.label40.TabIndex = 131;
            this.label40.Text = "Решатель САУ";
            // 
            // lb_Algebra
            // 
            this.lb_Algebra.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_Algebra.FormattingEnabled = true;
            this.lb_Algebra.ItemHeight = 20;
            this.lb_Algebra.Items.AddRange(new object[] {
            "Ленточный Гаусс",
            "Би сопряженные градиенты"});
            this.lb_Algebra.Location = new System.Drawing.Point(18, 453);
            this.lb_Algebra.Margin = new System.Windows.Forms.Padding(4);
            this.lb_Algebra.Name = "lb_Algebra";
            this.lb_Algebra.Size = new System.Drawing.Size(233, 44);
            this.lb_Algebra.TabIndex = 130;
            // 
            // checkBoxView
            // 
            this.checkBoxView.AutoSize = true;
            this.checkBoxView.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkBoxView.Location = new System.Drawing.Point(18, 401);
            this.checkBoxView.Name = "checkBoxView";
            this.checkBoxView.Size = new System.Drawing.Size(208, 24);
            this.checkBoxView.TabIndex = 85;
            this.checkBoxView.Text = "Визуализация КЭ сетки";
            this.checkBoxView.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label11.Location = new System.Drawing.Point(14, 65);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(181, 20);
            this.label11.TabIndex = 53;
            this.label11.Text = "Генератор КЭ сетки";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label8.Location = new System.Drawing.Point(12, 97);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(126, 17);
            this.label8.TabIndex = 52;
            this.label8.Text = "dx= % от max(L,H)";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label35.Location = new System.Drawing.Point(19, 507);
            this.label35.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(103, 20);
            this.label35.TabIndex = 123;
            this.label35.Text = "Тип створа";
            // 
            // textBoxDiam
            // 
            this.textBoxDiam.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxDiam.Location = new System.Drawing.Point(168, 97);
            this.textBoxDiam.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDiam.Name = "textBoxDiam";
            this.textBoxDiam.Size = new System.Drawing.Size(81, 23);
            this.textBoxDiam.TabIndex = 52;
            this.textBoxDiam.Text = "10";
            // 
            // lb_Ring
            // 
            this.lb_Ring.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_Ring.FormattingEnabled = true;
            this.lb_Ring.ItemHeight = 20;
            this.lb_Ring.Items.AddRange(new object[] {
            "плоский",
            "поворот"});
            this.lb_Ring.Location = new System.Drawing.Point(22, 530);
            this.lb_Ring.Margin = new System.Windows.Forms.Padding(4);
            this.lb_Ring.Name = "lb_Ring";
            this.lb_Ring.Size = new System.Drawing.Size(227, 44);
            this.lb_Ring.TabIndex = 96;
            this.lb_Ring.SelectedIndexChanged += new System.EventHandler(this.lb_Ring_SelectedIndexChanged);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button3.Location = new System.Drawing.Point(18, 365);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(179, 29);
            this.button3.TabIndex = 45;
            this.button3.Text = "Генерация КЭ сетки ДГ";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label34
            // 
            this.label34.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label34.Location = new System.Drawing.Point(15, 9);
            this.label34.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(125, 20);
            this.label34.TabIndex = 90;
            this.label34.Text = "Уклон канала";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label32.Location = new System.Drawing.Point(17, 33);
            this.label32.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(15, 17);
            this.label32.TabIndex = 62;
            this.label32.Text = "J";
            // 
            // tb_J
            // 
            this.tb_J.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb_J.Location = new System.Drawing.Point(168, 33);
            this.tb_J.Margin = new System.Windows.Forms.Padding(4);
            this.tb_J.Name = "tb_J";
            this.tb_J.Size = new System.Drawing.Size(83, 23);
            this.tb_J.TabIndex = 63;
            this.tb_J.Text = "0.001";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.Info;
            this.panel3.Controls.Add(this.listBoxAMu2);
            this.panel3.Controls.Add(this.cb_Mu_YZ);
            this.panel3.Controls.Add(this.balel_Us);
            this.panel3.Controls.Add(this.ls_Type__U_star);
            this.panel3.Controls.Add(this.cb_smoothing);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.listBoxAMu);
            this.panel3.Controls.Add(this.label41);
            this.panel3.Controls.Add(this.cbBoundaryG2_Ux);
            this.panel3.Controls.Add(this.lb_CrossNamber);
            this.panel3.Controls.Add(this.label37);
            this.panel3.Controls.Add(this.lb_VortexBC_G2);
            this.panel3.Controls.Add(this.button2);
            this.panel3.Location = new System.Drawing.Point(287, 12);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(457, 631);
            this.panel3.TabIndex = 62;
            // 
            // cb_Mu_YZ
            // 
            this.cb_Mu_YZ.AutoSize = true;
            this.cb_Mu_YZ.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cb_Mu_YZ.Location = new System.Drawing.Point(318, 272);
            this.cb_Mu_YZ.Name = "cb_Mu_YZ";
            this.cb_Mu_YZ.Size = new System.Drawing.Size(128, 24);
            this.cb_Mu_YZ.TabIndex = 154;
            this.cb_Mu_YZ.Tag = "";
            this.cb_Mu_YZ.Text = "Анизотропия";
            this.cb_Mu_YZ.UseVisualStyleBackColor = true;
            // 
            // balel_Us
            // 
            this.balel_Us.AutoSize = true;
            this.balel_Us.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.balel_Us.Location = new System.Drawing.Point(13, 536);
            this.balel_Us.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.balel_Us.Name = "balel_Us";
            this.balel_Us.Size = new System.Drawing.Size(278, 20);
            this.balel_Us.TabIndex = 150;
            this.balel_Us.Text = "Расчет динамической скорости";
            // 
            // ls_Type__U_star
            // 
            this.ls_Type__U_star.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ls_Type__U_star.FormattingEnabled = true;
            this.ls_Type__U_star.ItemHeight = 20;
            this.ls_Type__U_star.Items.AddRange(new object[] {
            "HJ По улону и глубине канала ",
            "GS По гидростатике канала ",
            "LU По придонной скорости"});
            this.ls_Type__U_star.Location = new System.Drawing.Point(16, 559);
            this.ls_Type__U_star.Margin = new System.Windows.Forms.Padding(4);
            this.ls_Type__U_star.Name = "ls_Type__U_star";
            this.ls_Type__U_star.Size = new System.Drawing.Size(430, 64);
            this.ls_Type__U_star.TabIndex = 149;
            // 
            // cb_smoothing
            // 
            this.cb_smoothing.AutoSize = true;
            this.cb_smoothing.Checked = true;
            this.cb_smoothing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_smoothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cb_smoothing.Location = new System.Drawing.Point(133, 30);
            this.cb_smoothing.Name = "cb_smoothing";
            this.cb_smoothing.Size = new System.Drawing.Size(139, 24);
            this.cb_smoothing.TabIndex = 135;
            this.cb_smoothing.Tag = "";
            this.cb_smoothing.Text = "Сглаживавние";
            this.cb_smoothing.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(13, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 20);
            this.label2.TabIndex = 134;
            this.label2.Text = "Скорость потока нп  WL";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(13, 280);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(280, 20);
            this.label1.TabIndex = 133;
            this.label1.Text = "Модель турбулентной вязкости";
            // 
            // listBoxAMu
            // 
            this.listBoxAMu.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBoxAMu.FormattingEnabled = true;
            this.listBoxAMu.ItemHeight = 18;
            this.listBoxAMu.Items.AddRange(new object[] {
            "Модель Буссинеска (const)",
            "Модель Караушева",
            "Модель Прандтля",
            "Модель Великанова",
            "Модель Адаби 12",
            "Модель Адаби 19",
            "Лео К. ван Рейн",
            "Модель ван Дриста",
            "Модель ГЛС 95  ",
            "Модель LES",
            "Модель Дерек Г. и К.",
            "Модель Потапова И.И."});
            this.listBoxAMu.Location = new System.Drawing.Point(16, 303);
            this.listBoxAMu.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxAMu.Name = "listBoxAMu";
            this.listBoxAMu.Size = new System.Drawing.Size(211, 220);
            this.listBoxAMu.TabIndex = 132;
            this.listBoxAMu.SelectedIndexChanged += new System.EventHandler(this.listBoxAMu_SelectedIndexChanged);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label41.Location = new System.Drawing.Point(13, 167);
            this.label41.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(73, 20);
            this.label41.TabIndex = 131;
            this.label41.Text = "Створы";
            // 
            // cbBoundaryG2_Ux
            // 
            this.cbBoundaryG2_Ux.AutoSize = true;
            this.cbBoundaryG2_Ux.Checked = true;
            this.cbBoundaryG2_Ux.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbBoundaryG2_Ux.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbBoundaryG2_Ux.Location = new System.Drawing.Point(16, 31);
            this.cbBoundaryG2_Ux.Name = "cbBoundaryG2_Ux";
            this.cbBoundaryG2_Ux.Size = new System.Drawing.Size(96, 24);
            this.cbBoundaryG2_Ux.TabIndex = 130;
            this.cbBoundaryG2_Ux.Tag = "";
            this.cbBoundaryG2_Ux.Text = "Ux =Ux(y)";
            this.cbBoundaryG2_Ux.UseVisualStyleBackColor = true;
            // 
            // lb_CrossNamber
            // 
            this.lb_CrossNamber.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_CrossNamber.FormattingEnabled = true;
            this.lb_CrossNamber.ItemHeight = 20;
            this.lb_CrossNamber.Items.AddRange(new object[] {
            "Створ 15",
            "Створ 18",
            "Створ 21",
            "Створ тест"});
            this.lb_CrossNamber.Location = new System.Drawing.Point(16, 191);
            this.lb_CrossNamber.Margin = new System.Windows.Forms.Padding(4);
            this.lb_CrossNamber.Name = "lb_CrossNamber";
            this.lb_CrossNamber.Size = new System.Drawing.Size(211, 84);
            this.lb_CrossNamber.TabIndex = 95;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label37.Location = new System.Drawing.Point(12, 55);
            this.label37.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(270, 20);
            this.label37.TabIndex = 126;
            this.label37.Text = "Граничные условия для вихря ";
            // 
            // lb_VortexBC_G2
            // 
            this.lb_VortexBC_G2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lb_VortexBC_G2.FormattingEnabled = true;
            this.lb_VortexBC_G2.ItemHeight = 20;
            this.lb_VortexBC_G2.Items.AddRange(new object[] {
            "Vortex = 0  на контуре",
            "Vortex = 0 на поверхности WL",
            "Задана скорость Uy на WL",
            "Vortex = 0 на дне и Uy на WL"});
            this.lb_VortexBC_G2.Location = new System.Drawing.Point(16, 79);
            this.lb_VortexBC_G2.Margin = new System.Windows.Forms.Padding(4);
            this.lb_VortexBC_G2.Name = "lb_VortexBC_G2";
            this.lb_VortexBC_G2.Size = new System.Drawing.Size(430, 84);
            this.lb_VortexBC_G2.TabIndex = 125;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.Location = new System.Drawing.Point(235, 193);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(211, 29);
            this.button2.TabIndex = 120;
            this.button2.Text = "Расчет";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // listBoxAMu2
            // 
            this.listBoxAMu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBoxAMu2.FormattingEnabled = true;
            this.listBoxAMu2.ItemHeight = 18;
            this.listBoxAMu2.Items.AddRange(new object[] {
            "Модель Буссинеска (const)",
            "Модель Караушева",
            "Модель Прандтля",
            "Модель Великанова",
            "Модель Адаби 12",
            "Модель Адаби 19",
            "Лео К. ван Рейн",
            "Модель ван Дриста",
            "Модель ГЛС 95  ",
            "Модель LES",
            "Модель Дерек Г. и К.",
            "Модель Потапова И.И."});
            this.listBoxAMu2.Location = new System.Drawing.Point(235, 303);
            this.listBoxAMu2.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxAMu2.Name = "listBoxAMu2";
            this.listBoxAMu2.Size = new System.Drawing.Size(211, 220);
            this.listBoxAMu2.TabIndex = 155;
            // 
            // FVortexStream
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 655);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Name = "FVortexStream";
            this.Text = "Канал Розовского";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.ListBox lb_Algebra;
        private System.Windows.Forms.CheckBox checkBoxView;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxDiam;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.CheckBox cbBoundaryG2_Ux;
        private System.Windows.Forms.ListBox lb_CrossNamber;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.ListBox lb_VortexBC_G2;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.ListBox lb_Ring;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tb_J;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBoxAMu;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cb_smoothing;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lb_Geometry;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lb_MeshGen;
        private System.Windows.Forms.Label balel_Us;
        private System.Windows.Forms.ListBox ls_Type__U_star;
        private System.Windows.Forms.CheckBox cb_Mu_YZ;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_w;
        private System.Windows.Forms.ListBox listBoxAMu2;
    }
}