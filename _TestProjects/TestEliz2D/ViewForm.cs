using GeometryLib;
using MeshLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GeometryLib.Areas;

namespace TestEliz2D
{
    public partial class ViewForm : Form
    {
        //                             -->
        //   XYT   ________________________________________________________________
        //        | topLayer   |                topLayer           |  topLayer     |
        //        |            |                                   |               |
        //    XYt |------------|-----------------------------------|---------------|
        // ||     |            |                                   |               |  
        // \/     |            |                                   |               |  ||
        //        |            |                                   |               |  \/
        //    XYb | -----------|-----------------------------------|---------------|
        //        |bottomLayer |     bottomLayer                   | bottomLayer   |
        //   XYB   ________________________________________________________________
        //        |      L1    |              L2                   |      L3       |
        //                                   ->
        Graphics g;
        double[] XB, YB;
        double[] XT, YT, Xb, Yb, Xt, Yt;
        KsiMesh M;
        bool flagArea = false;
        bool flagMesh = false;

        private void button1_Resize(object sender, EventArgs e)
        {
            button1.Location = new Point(this.Width-100, 30);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Invalidate();
        }

        RectangleF Rec;


        int WS, HS;
        double MinX, MinY;
        public ViewForm()
        {
            InitializeComponent();
        }
        public ViewForm(AreasProfile2D area)
        {
            this.XT = area.XTop;
            this.YT = area.YTop;
            this.XB = area.XBottom;
            this.YB = area.YBottom;
            this.Xt = area.Xt;
            this.Yt = area.Yt;
            this.Xb = area.Xb;
            this.Yb = area.Yb;

            InitializeComponent();
            flagArea = true;
            flagMesh = false;
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flagArea)
            {
                saveAreaFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveAreaFileDialog.ShowDialog();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (flagArea)
            {
                openAreaFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openAreaFileDialog.ShowDialog();
            }
        }

        private void saveAreaFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            /// структура файла .ar
            /// 
            /// NX
            /// BottomLayer
            /// TopLayer
            /// XB[i], YB[i], XT[i], YT[i]
            try
            {
                using (StreamWriter outputFile = new StreamWriter(saveAreaFileDialog.FileName))
                {
                    outputFile.WriteLine(XB.Length.ToString());
                    outputFile.WriteLine(Yb[0] - YB[0]);
                    outputFile.WriteLine(YT[0] - Yt[0]);
                    outputFile.WriteLine("XB, YB, XT, YT");
                    for (int i = 0; i < XB.Length; i++)
                    {
                        outputFile.Write(XB[i].ToString() + " " + YB[i].ToString() + " " + XT[i].ToString() + " " + YT[i].ToString());
                        outputFile.WriteLine();

                    }
                    outputFile.Close();
                    
                }
                statusStrip1.Text = "Ok!";
            }
            catch (Exception ex) 
            {
                statusStrip1.Text = ex.Message;
            }
        }

        private void openAreaFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                double bottomLayer =0;
                double topLayer = 0;
                int Nx = 0;
                using (StreamReader streamReader = new StreamReader(openAreaFileDialog.FileName))
                {
                     Nx = Convert.ToInt32(streamReader.ReadLine());
                    //
                    XB = new double[Nx];
                    YB = new double[Nx];
                    XT = new double[Nx];
                    YT = new double[Nx];
                    Xb = new double[Nx];
                    Yb = new double[Nx];
                    Xt = new double[Nx];
                    Yt = new double[Nx];
                    //
                    bottomLayer = Convert.ToDouble(streamReader.ReadLine());
                    topLayer = Convert.ToDouble(streamReader.ReadLine());
                    //
                    streamReader.ReadLine();
                    //
                    string[] s;
                    for (int i = 0; i < Nx; i++)
                    {
                        s = streamReader.ReadLine().Split(' ');
                        XB[i] = Convert.ToDouble(s[0]);
                        YB[i] = Convert.ToDouble(s[1]);
                        XT[i] = Convert.ToDouble(s[2]);
                        YT[i] = Convert.ToDouble(s[3]);
                    }

                }
                //
                if (bottomLayer == 0)
                {
                    for (int i = 0; i < Nx; i++)
                    {
                        Xb[i] = XB[i];
                        Yb[i] = YB[i];
                    }
                }
                else
                {
                    for (int i = 0; i < Nx; i++)
                    {
                        Xb[i] = XB[i];
                        Yb[i] = YB[i] + bottomLayer; ;
                    }
                }
                //
                if (topLayer == 0)
                {
                    for (int i = 0; i < Nx; i++)
                    {
                        Xt[i] = XT[i];
                        Yt[i] = YT[i];
                    }
                }
                else
                {
                    for (int i = 0; i < Nx; i++)
                    {
                        Xt[i] = XT[i];
                        Yt[i] = YT[i]-topLayer;
                    }
                }
                //
                statusStrip1.Text = "Ok!";
            }
            catch(Exception ex)
            {
                statusStrip1.Text = ex.Message;
            }
        }

        //
        public ViewForm(KsiMesh Mesh)
        {
            this.M = Mesh;

            InitializeComponent();
            flagMesh = true;
            flagArea = false;
        }
        //
        Pen pen = new Pen(Brushes.Black, 1.5f);
        Pen pen2 = new Pen(Brushes.Green, 2.5f);
        Font f = new Font("Arial", 12.0f);
        Brush BrushText = Brushes.Black;
        //
        private void ViewForm_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            WS = this.Width - 200;
            HS = this.Height - 200;
            //
            if (flagArea)
                DrawArea(pen, pen2, f, BrushText);
            if(flagMesh)
                DrawGrid();
        }

        private void DrawArea(Pen pen, Pen pen2, Font f, Brush BrushText)
        {
            MinX = XB.Min();
            MinY = YB.Min();
            Rec = new RectangleF((float)XB.Min(), (float)YB.Max(), (float)(XB.Max() - XB.Min()), (float)(YB.Max() - YB.Min()));
            if (Rec.Height == 0)
                Rec.Height = 1;
            int cx, cy;
            if (XB != null)
            {
                //
                if ((XT != null) && (Xb != null) && (Xt != null))
                {
                    Rec = new RectangleF((float)XB.Min(), (float)YB.Max(), (float)(XT.Max() - XB.Min()), (float)(YT.Max() - YB.Min()));
                    for (int i = 0; i < XB.Length - 1; i++)
                    {
                        cx = ScaleX(XT[i]);
                        cy = ScaleY(YT[i]);
                        Point Point1 = new Point(cx, cy);
                        //
                        cx = ScaleX(XT[i + 1]);
                        cy = ScaleY(YT[i + 1]);
                        Point Point2 = new Point(cx, cy);
                        //
                        g.DrawLine(pen, Point1, Point2);
                        g.DrawEllipse(pen2, Point1.X, Point1.Y, 2, 2);
                        //g.DrawString(i.ToString(), f, BrushText, Point1.X, Point1.Y - 20);
                        //
                        cx = ScaleX(Xt[i]);
                        cy = ScaleY(Yt[i]);
                        Point1 = new Point(cx, cy);
                        //
                        cx = ScaleX(Xt[i + 1]);
                        cy = ScaleY(Yt[i + 1]);
                        Point2 = new Point(cx, cy);
                        //
                        g.DrawLine(pen, Point1, Point2);
                        //
                        cx = ScaleX(Xb[i]);
                        cy = ScaleY(Yb[i]);
                        Point1 = new Point(cx, cy);
                        //
                        cx = ScaleX(Xb[i + 1]);
                        cy = ScaleY(Yb[i + 1]);
                        Point2 = new Point(cx, cy);
                        //
                        g.DrawLine(pen, Point1, Point2);
                        //
                        if (i == XB.Length - 2)
                        {
                            //g.DrawString((i + 1).ToString(), f, BrushText, Point2.X, Point2.Y - 20);
                            g.DrawEllipse(pen2, Point2.X, Point2.Y, 2, 2);
                        }

                    }
                }
                for (int i = 0; i < XB.Length - 1; i++)
                {
                    cx = ScaleX(XB[i]);
                    cy = ScaleY(YB[i]);
                    Point Point1 = new Point(cx, cy);
                    //
                    cx = ScaleX(XB[i + 1]);
                    cy = ScaleY(YB[i + 1]);
                    Point Point2 = new Point(cx, cy);
                    //
                    g.DrawLine(pen, Point1, Point2);
                    g.DrawEllipse(pen2, Point1.X, Point1.Y, 2, 2);
                    //g.DrawString(i.ToString(), f, BrushText, Point1.X, Point1.Y - 20);
                    if (i == XB.Length - 2)
                    {
                        //g.DrawString((i + 1).ToString(), f, BrushText, Point2.X, Point2.Y - 20);
                        g.DrawEllipse(pen2, Point2.X, Point2.Y, 2, 2);
                    }

                }
                //
                cx = ScaleX(XB[0]);
                cy = ScaleY(YB[0]);
                Point P1 = new Point(cx, cy);
                //
                cx = ScaleX(XT[0]);
                cy = ScaleY(YT[0]);
                Point P2 = new Point(cx, cy);
                //
                g.DrawLine(pen, P1, P2);
                cx = ScaleX(XB[XB.Length-1]);
                cy = ScaleY(YB[XB.Length - 1]);
                P1 = new Point(cx, cy);
                //
                cx = ScaleX(XT[XT.Length - 1]);
                cy = ScaleY(YT[XT.Length - 1]);
                P2 = new Point(cx, cy);
                //
                g.DrawLine(pen, P1, P2);

            }
        }

        protected int ScaleX(double xa)
        {
            int ix = (int)(WS * (xa - MinX) / Rec.Width) + 10;
            return ix;
        }
        protected int ScaleY(double yc)
        {
            int iy = (int)(HS - HS * (yc - MinY) / Rec.Height) + 90;
            return iy;
        }
        //
        int n = 1;
        void DrawGrid()
        {
            Color GridColor = Color.Purple;
            float PenWidth = 1.5f;
            Pen pen0 = new Pen(GridColor, PenWidth);
            Rec = new RectangleF((float)M.X.Min(), (float)M.Y.Max(), (float)(M.X.Max() - M.X.Min()), (float)(M.Y.Max() - M.Y.Min()));
            //
            uint[] kn = M.AreaElems[0];
            double minp = 100;
            double p1p2 = Math.Abs(ScaleX(M.X[kn[0]]) - ScaleX(M.X[kn[1]]));
            if (p1p2 < minp)
                minp = p1p2;
            //
            p1p2 = Math.Abs(ScaleY(M.Y[kn[0]]) - ScaleY(M.Y[kn[1]]));
            if (p1p2 < minp)
                minp = p1p2;
            //
            n = 1;
            //
            if ((minp < 10))
                n = (int)Math.Round(20.0 / minp, MidpointRounding.AwayFromZero) - 1;
            if (minp == 0)
                for (int i = 1; i < M.CountBottom; i++)
                {
                    if (Math.Abs(ScaleX(M.X[M.BottomKnots[0]]) - ScaleX(M.X[M.BottomKnots[i]])) != 0)
                    {
                        if (i == 1)
                            n = 1;
                        else
                            n = i * 20;
                        break;
                    }
                }
            //if (minp < 10)
            //{
            //    int lup = M.LeftKnots[0];
            //    g.FillRectangle(new SolidBrush(GridColor), ScaleX(Rec.X), ScaleY(Rec.Y), ScaleX(Rec.Width), this.Height-20);
            //}
            //else
            //{
            for (int i = 0; i < M.CountElements; i += n)
            {
                uint[] Knots = M.AreaElems[i];
                int p1x = ScaleX(M.X[Knots[0]]);
                int p1y = ScaleY(M.Y[Knots[0]]);
                Point Point1 = new Point(p1x, p1y);
                //
                int p2x = ScaleX(M.X[Knots[1]]);
                int p2y = ScaleY(M.Y[Knots[1]]);
                Point Point2 = new Point(p2x, p2y);
                //
                int p3x = ScaleX(M.X[Knots[2]]);
                int p3y = ScaleY(M.Y[Knots[2]]);
                Point Point3 = new Point(p3x, p3y);
                //
                g.DrawLine(pen0, Point1, Point2);
                g.DrawLine(pen0, Point2, Point3);
                g.DrawLine(pen0, Point3, Point1);
                //
                //g.DrawString(Knots[0].ToString(), f, BrushText, Point1.X, Point1.Y - 20);
                //g.DrawString(Knots[1].ToString(), f, BrushText, Point2.X, Point2.Y - 20);
                //g.DrawString(Knots[2].ToString(), f, BrushText, Point3.X, Point3.Y - 20);
                //


            }
        }
        }
}
