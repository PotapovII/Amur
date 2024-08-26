namespace RenderLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Windows.Forms;
    using MeshLib;
    public partial class BMShow : Form
    {
        NumberFormatInfo formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
        static double SYYmax;
        static double SYYmin;
        static double SX0;
        static double SX1;
        static bool CheckScale = false;

        //string TName = "Построитель функций";
        GData gdata = null;
        GPoint MinMax_X;
        GPoint MinMax_Y;
        //Font fontCoord = new Font("Arial", 12);
        Font fontCoord = new Font("Times New Roman", 14);
        /// <summary>
        /// начало отрезка по Х
        /// </summary>
        double X0, NX0;
        /// <summary>
        ///  конец отрезка по Х
        /// </summary>
        double X1, NX1;
        /// <summary>
        /// длина области Х
        /// </summary>
        double LYmax;
        double LYmax0;
        double LYmin;
        double LYmin0;
        Bitmap img = null;
        Graphics g = null;
        //bool Flag = false;
        bool FlagReShow = false;

        Pen pen = null;
        public BMShow()
        {
            InitializeComponent();
            //this.Text = TName;
            saveFileDialog1.Filter = "graphic files(*.bmp)|*.bmp|All files(*.*)|*.*";
            int n = (int)numericUpDown1.Value;
            pen = new Pen(colorDialog1.Color, n);
            checScale.Checked = CheckScale;
            textBoxScaleYmin.Text = "0";
        }
        /// <summary>
        ///  установка данных для отрисовки кривых в окне построения кривых
        /// </summary>
        /// <param name="gdata"></param>
        /// <param name="GrphName"></param>
        public void SetData(GData gdata, string GrphName = "")
        {
            Name = GrphName;
            img = new Bitmap(pBox.Width, pBox.Height);
            g = Graphics.FromImage(img);
            this.gdata = gdata;
            if (gdata != null)
            {
                MinMax_X = gdata.MinMax_X();
                MinMax_Y = gdata.MinMax_Y();

                X0 = MinMax_X.X;
                X1 = MinMax_X.Y;
                NX0 = X0;
                NX1 = X1;
                LYmax0 = MinMax_Y.Max;
                LYmax = LYmax0;
                LYmin0 = 0;
                LYmin = LYmin0;
                cListBoxFiltr.Items.Clear();
                // список кривых
                for (int i = 0; i < gdata.curves.Count; i++)
                    cListBoxFiltr.Items.Add(gdata.curves[i].Name, true);
                // отрисовка 
                textBoxScaleYmax.Text = LYmax0.ToString();
                textBoxScaleXmin.Text = MinMax_X.X.ToString();
                textBoxScaleXmax.Text = MinMax_X.Y.ToString();
                if (MinMax_Y.X >= 0)
                {
                    cbCoord2K.Checked = false;
                    textBoxScaleYmin.Enabled = true;
                    textBoxScaleYmin.Text = LYmin.ToString();
                }
                else
                {
                    textBoxScaleYmin.Enabled = false;
                    textBoxScaleYmin.Text = "0";
                }
                cbScale.Checked = false;
            }
        }
        private void BMShow_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                ReScales();

                Pen penBlack = new Pen(Color.Black, 2);

                if (g != null)
                {
                    if (pBox.Width != img.Width || pBox.Height != img.Height || FlagReShow == true)
                    {
                        img = new Bitmap(pBox.Width, pBox.Height);
                        g = Graphics.FromImage(img);
                        // Flag = cbCoord2K.Checked;
                        FlagReShow = false;
                    }
                    //привязка системы координат
                    int CountPoint = 11;
                    int W = pBox.Width - 120;
                    int H = pBox.Height - 20;
                    if (cbCoord2K.Checked == true)
                        H = (pBox.Height) / 2;
                    int DWTop = 50;
                    int DWBotton = H - 50;
                    int DWTopBotton = 2 * DWBotton - DWTop;

                    float fontSize = fontCoord.Size;
                    int maxS = 0;
                    for (int i = 0; i < CountPoint; i++)
                    {
                        string SLength = ScaleGraph.Scale(LYmax, i);
                        if (maxS < SLength.Length)
                            maxS = SLength.Length;
                    }
                    int DWLeft = (int)(90 * (fontSize / 12f));

                    int ShiftDWLeft;
                    if (maxS < 3)
                        ShiftDWLeft = (int)((maxS + 1) * fontSize);
                    else
                        if (maxS < 5)
                        ShiftDWLeft = (int)((maxS) * fontSize);
                    else
                        ShiftDWLeft = (int)((maxS - 1) * fontSize);

                    int FontLeft = DWLeft - ShiftDWLeft;

                    int DWRight = W - 50;
                    int CountStart = 0;

                    Point a = new Point(DWLeft, DWBotton);
                    Point b = new Point(DWRight + 10, DWBotton);
                    Point c = new Point(DWLeft, DWTop - 10);
                    Point mc = new Point(DWLeft, DWTopBotton);
                    // координатные оси
                    g.DrawLine(penBlack, a, b);

                    if (cbCoord2K.Checked == true)
                    {
                        g.DrawLine(penBlack, c, mc);
                        CountStart = 1;
                    }
                    else
                        g.DrawLine(penBlack, a, c);

                    //горизонтальные
                    int LengthX = DWRight - DWLeft;
                    int DeltX = (int)(LengthX / 4);
                    int DeltXG = (int)(LengthX / 10);

                    for (int i = CountStart; i < CountPoint; i++)
                    {
                        int x = DWLeft + DeltXG * i;
                        a = new Point(x, DWBotton);
                        b = new Point(x, DWBotton + 5);
                        g.DrawLine(penBlack, a, b);
                        string SLength = ScaleGraph.ScaleMinMax(X1, X0, i).ToString();
                        g.DrawString(SLength, fontCoord, Brushes.Black, x - 10, DWBotton + 10);
                    }

                    //вертикальные
                    // 1 квартель
                    int LengthY = DWBotton - DWTop;
                    int DeltY = (int)(LengthY / 10);
                    for (int i = 0; i < CountPoint; i++)
                    {
                        int y = DWBotton - DeltY * i;
                        a = new Point(DWLeft - 5, y);
                        b = new Point(DWLeft, y);
                        g.DrawLine(penBlack, a, b);
                        string SLength = ScaleGraph.ScaleMinMax(LYmax, LYmin, i); //Scale(LYmax, i);
                        g.DrawString(SLength, fontCoord, Brushes.Black, FontLeft, y - fontSize + 2);
                    }
                    // 4 квартель
                    if (cbCoord2K.Checked == true)
                    {
                        for (int i = 1; i < CountPoint; i++)
                        {
                            int y = DWBotton + DeltY * i;
                            a = new Point(DWLeft - 5, y);
                            b = new Point(DWLeft, y);
                            g.DrawLine(penBlack, a, b);
                            string SLength = ScaleGraph.Scale(LYmax, i);
                            g.DrawString("-" + SLength, fontCoord, Brushes.Black, FontLeft, y - fontSize + 2);
                        }
                    }

                    //g.DrawLine(new Pen(Color.Black, 2), new Point(10, 20), new Point(333, 444));
                    //отрисовка графиков
                    if (gdata != null)
                    {
                        //
                        double WLengthX = X1 - X0;
                        double WLengthY = LYmax - LYmin;
                        if (cbLine.Checked == true)
                        {
                            double Y = 0;
                            int xa = DWLeft;
                            int xb = DWLeft + (int)(LengthX);
                            try
                            {
                                Y = double.Parse(tbYLine.Text, formatter);
                            }
                            catch
                            {

                            }
                            //int ya = DWTop + (int)(LengthY * (1 - Y / WLengthY));
                            int ya = DWTop + (int)(LengthY * (1 - (Y - LYmin) / WLengthY));
                            a = new Point(xa, ya);
                            b = new Point(xb, ya);

                            g.DrawLine(pen, a, b);

                        }
                        //количество кривых
                        for (int idx = 0; idx < gdata.curves.Count; idx++)
                        {
                            GCurve curve = gdata.curves[idx];
                            List<GPoint> pps = curve.points;
                            int Count = curve.Count;

                            double dx = curve.Get_dx;
                            // Отрисока кривой
                            CheckState flag = cListBoxFiltr.GetItemCheckState(idx);
                            if (flag == CheckState.Unchecked)
                                continue;
                            //количество узлов
                            for (int i = 0; i < Count - 1; i++)
                            {
                                var pp = pps[i];
                                var pe = pps[i + 1];

                                int xa, xb;
                                // Реверс
                                if (cbRevers.Checked)
                                {
                                    xa = DWLeft + (int)(LengthX * (1 - (pp.X - X0) / WLengthX));
                                    xb = DWLeft + (int)(LengthX * (1 - (pe.X - X0) / WLengthX));
                                }
                                else
                                {
                                    xa = DWLeft + (int)(LengthX * (pp.X - X0) / WLengthX);
                                    xb = DWLeft + (int)(LengthX * (pe.X - X0) / WLengthX);
                                }

                                double mya = pp.Y;
                                double myb = pe.Y;
                                if (cbCoord2K.Checked == true)
                                {
                                    mya = Math.Abs(pp.Y);
                                    myb = Math.Abs(pe.Y);
                                }

                                // рисуем только то что попадает в интервал
                                bool ffa = LYmin <= mya && mya <= LYmax;
                                bool ffb = LYmin <= myb && myb <= LYmax;
                                if (ffa == true && ffb == true)
                                {
                                    int ya = DWTop + (int)(LengthY * (1 - (pp.Y - LYmin) / WLengthY));
                                    int yb = DWTop + (int)(LengthY * (1 - (pe.Y - LYmin) / WLengthY));

                                    a = new Point(xa, ya);
                                    b = new Point(xb, yb);

                                    g.DrawLine(pen, a, b);
                                }
                            }
                        }
                    }

                    pBox.Image = img;
                }
                //g.DrawString(GraphName, new Font("Arial", 14), Brushes.Black, 160, 15);

            }
            catch (Exception ee)
            {
                Name = ee.Message;
            }
        }

        private void cListBoxFiltr_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < gdata.curves.Count; i++)
            {
                CheckState flag = cListBoxFiltr.GetItemCheckState(i);
                if (flag == CheckState.Unchecked)
                    gdata.curves[i].Check = false;
                else
                    gdata.curves[i].Check = true;
            }
            rbAuto_CheckedChanged(sender, e);
            FlagReShow = true;
            Invalidate();
        }

        private void btShow_Click(object sender, EventArgs e)
        {
            FlagReShow = true;
            Invalidate();
        }
        public void ReScales()
        {
            //this.Text = TName;
            if (rbAuto.Checked == true)
            {
                LYmax = LYmax0;
                LYmin = LYmin0;
                X0 = NX0;
                X1 = NX1;
                MinMax_X = gdata.MinMax_X();
                MinMax_Y = gdata.MinMax_Y();
                textBoxScaleXmin.Text = MinMax_X.X.ToString();
                textBoxScaleXmax.Text = MinMax_X.Y.ToString();
            }
            else
            {
                try
                {
                    if (cbScale.Checked == true)
                    {
                        LYmax = ScaleValue.ScaleConvert(LYmax0);
                        LYmin = ScaleValue.ScaleConvert(LYmin0);
                        X1 = ScaleValue.ScaleConvert(NX1);
                        X0 = ScaleValue.ScaleConvert(NX0);
                        textBoxScaleXmin.Text = X0.ToString();
                        textBoxScaleXmax.Text = X1.ToString();
                        textBoxScaleYmax.Text = LYmax.ToString();
                    }
                    else
                    {
                        LYmax = double.Parse(textBoxScaleYmax.Text, formatter);
                        LYmin = double.Parse(textBoxScaleYmin.Text, formatter);
                        X0 = double.Parse(textBoxScaleXmin.Text, formatter);
                        X1 = double.Parse(textBoxScaleXmax.Text, formatter);
                    }
                }
                catch (Exception eee)
                {
                    this.Text = eee.Message;
                    textBoxScaleYmax.Text = LYmax0.ToString();
                    textBoxScaleYmin.Text = LYmin0.ToString();
                    LYmax = LYmax0;
                    LYmin = LYmin0;
                }
            }
            if (checScale.Checked == false)
            {
                SYYmax = LYmax;
                SYYmin = LYmin;
                SX0 = X0;
                SX1 = X1;
            }
            else
            {
                LYmax = SYYmax;
                LYmin = SYYmin;
                X0 = SX0;
                X1 = SX1;
                textBoxScaleXmin.Text = X0.ToString();
                textBoxScaleXmax.Text = X1.ToString();
                textBoxScaleYmax.Text = LYmax.ToString();
                textBoxScaleYmin.Text = LYmin.ToString();
            }
        }

        private void rbAuto_CheckedChanged(object sender, EventArgs e)
        {
            FlagReShow = true;
            Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(pBox.Width, pBox.Height);
                pBox.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cListBoxFiltr.Items.Count; i++)
                cListBoxFiltr.SetItemChecked(i, checkBox1.Checked);
            FlagReShow = true;
            Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pen = new Pen(colorDialog1.Color, 1);
                FlagReShow = true;
                Invalidate();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int n = (int)numericUpDown1.Value;
            pen = new Pen(colorDialog1.Color, n);
            FlagReShow = true;
            Invalidate();
        }

        private void btFont_Click(object sender, EventArgs e)
        {
            fontDialog1.Font = fontCoord;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                fontCoord = fontDialog1.Font;
                FlagReShow = true;
                Invalidate();
            }
        }

        private void textBoxScale_MouseClick(object sender, MouseEventArgs e)
        {
            rbManual.Checked = true;
            rbAuto_CheckedChanged(sender, e);
        }

        private void checScale_CheckedChanged(object sender, EventArgs e)
        {
            CheckScale = checScale.Checked;
            if (checScale.Checked == true)
            {
                textBoxScaleXmin.Enabled = false;
                textBoxScaleXmax.Enabled = false;
                textBoxScaleYmax.Enabled = false;
                textBoxScaleYmin.Enabled = false;
            }
            else
            {
                textBoxScaleXmin.Enabled = true;
                textBoxScaleXmax.Enabled = true;
                textBoxScaleYmax.Enabled = true;
                if (cbCoord2K.Checked == false)
                    textBoxScaleYmin.Enabled = true;
                else
                    textBoxScaleYmin.Enabled = false;
            }
            rbAuto_CheckedChanged(sender, e);
        }


        private void cbCoord2K_CheckedChanged(object sender, EventArgs e)
        {
            FlagReShow = true;
            if (cbCoord2K.Checked == false)
            {
                if (checScale.Checked == false)
                    textBoxScaleYmin.Enabled = true;
                textBoxScaleYmin.Text = LYmin.ToString("f6");
            }
            else
            {
                textBoxScaleYmin.Enabled = false;
                textBoxScaleYmin.Text = "0";
            }
            Invalidate();
        }

        private void cbLine_CheckedChanged(object sender, EventArgs e)
        {
            FlagReShow = true;
            Invalidate();
        }

        private void cbRevers_CheckedChanged(object sender, EventArgs e)
        {
            FlagReShow = true;
            Invalidate();
        }

        private void cbScale_CheckedChanged(object sender, EventArgs e)
        {
            if (cbScale.Checked == true)
            {
                rbManual.Checked = true;
                rbAuto_CheckedChanged(sender, e);
            }
        }

        private void textBoxScale_Click(object sender, EventArgs e)
        {
            rbManual.Checked = true;
            rbAuto_CheckedChanged(sender, e);
        }
    }
}
