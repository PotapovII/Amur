
namespace TestSHillObj
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Collections.Generic;

    using SHillObjLib;
    using CommonLib.Geometry;
    using MeshLib;
    using RenderLib;
    using CommonLib;
    using MeshAdapterLib;

    public partial class Form1 : Form
    {
        Random randy = new Random(138);
        List<HPoint> points = new List<HPoint>();
        double xMax, xMin, yMax, yMin;
        MeshGeneratorSHillObj triangulator = null;

        private void button2_Click(object sender, EventArgs e)
        {
            if (points.Count != 0)
            {
                triangulator = new MeshGeneratorSHillObj();
                Stopwatch watch = new Stopwatch();
                Hull hull = null;
                watch.Start();
                triangles = triangulator.Triangulation(points.ToArray(), true, ref hull);
                watch.Stop();
                Invalidate();
                lbPoints.Text = "Старт";
            }
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            int L = 600;
            int H = 600;
            int shX = 250;
            int shY = 20;
            
            if (points.Count > 0)
            {
                Graphics g = e.Graphics;
                Pen p = new Pen(Color.Black, 1);
                for (int i = 0; i < points.Count; i++)
                {
                    int X = (int)((points[i].x - xMin) / (xMax - xMin) * L) + shX;
                    int Y = (int)((points[i].y - yMin) / (yMax - yMin) * H) + shY;
                    g.DrawEllipse(p, new Rectangle(X - 2, Y - 2, 4, 4));
                }
            }
            if (triangulator != null)
            {
                Graphics g = e.Graphics;
                Pen p = new Pen(Color.Black, 1);

                for (int i = 0; i < triangles.Count; i++)
                {
                    Triad t = triangles[i];
                    int a = t.a;
                    int b = t.b;
                    int c = t.c;
                    int aX = (int)((points[a].x - xMin) / (xMax - xMin) * L) + shX;
                    int aY = (int)((points[a].y - yMin) / (yMax - yMin) * H) + shY;
                    int bX = (int)((points[b].x - xMin) / (xMax - xMin) * L) + shX;
                    int bY = (int)((points[b].y - yMin) / (yMax - yMin) * H) + shY;
                    int cX = (int)((points[c].x - xMin) / (xMax - xMin) * L) + shX;
                    int cY = (int)((points[c].y - yMin) / (yMax - yMin) * H) + shY;
                    Point A = new Point(aX, aY);
                    Point B = new Point(bX, bY);
                    Point C = new Point(cX, cY);
                    g.DrawLine(p, A, B);
                    g.DrawLine(p, B, C);
                    g.DrawLine(p, C, A);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lbPoints.Text = "Старт";
            int CountsKnot = int.Parse(tbCountsKnot.Text);
            triangulator = null;
            // Generate random points.
            points.Clear();
            HPoint[] p = new HPoint[CountsKnot];
            for (int i = 0; i < CountsKnot; i++)
            {
                int x = randy.Next(100000);
                int y = randy.Next(100000);
                p[i] = new HPoint(x, y);
                points.Add(p[i]);
            }
            triangulator = new MeshGeneratorSHillObj();
            Stopwatch watch = new Stopwatch();
            Hull hull = null;
            watch.Start();
            triangles = triangulator.Triangulation(p, true, ref hull);
            watch.Stop();

            IMesh mesh = MeshAdapterSHillObj.CreateMesh(triangles.ToArray(), hull.ToArray(), p);
            
            hull.Clear();
            triangles.Clear();
            points.Clear();

            ShowMesh(mesh);
            lbPoints.Text = "Старт";


            
        }
        protected void ShowMesh(IMesh mesh)
        {
            if (mesh != null)
            {
                SavePoint data = new SavePoint();
                data.SetSavePoint(0, mesh);
                double[] xx = mesh.GetCoords(0);
                double[] yy = mesh.GetCoords(1);
                data.Add("Координата Х", xx);
                data.Add("Координата Y", yy);
                data.Add("Координаты ХY", xx, yy);
                Form form = new ViForm(data);
                form.Show();
            }
        }
        List<Triad> triangles = null;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            lbPoints.Text = "Старт";
            int CountsKnot = int.Parse(tbCountsKnot.Text);
            points.Clear();
            triangulator = null;
            // Generate random points.
            SortedDictionary<int, Point> ps = new SortedDictionary<int, Point>();
            for (int i = 0; i < CountsKnot; i++)
            {
                int x = randy.Next(100000);
                int y = randy.Next(100000);
                points.Add(new HPoint(x, y));
            }
            xMax = points[0].x;
            xMin = points[0].x;
            yMax = points[0].y;
            yMin = points[0].y;
            //
            for (int i = 0; i < points.Count; i++)
            {
                if (xMax < points[i].x)
                    xMax = points[i].x;
                if (xMin > points[i].x)
                    xMin = points[i].x;
                if (yMax < points[i].y)
                    yMax = points[i].y;
                if (yMin > points[i].y)
                    yMin = points[i].y;
            }
            if(cbShow.Checked == true)
                Invalidate();
            lbPoints.Text = "Ок";
        }

    }
}
