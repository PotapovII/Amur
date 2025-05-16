//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                      09.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshAdapterLib
{
    using System;
    using System.Collections.Generic;
    using TriangleNet.Geometry;
    public static class CreateArea
    {
        public static IPolygon CreatePolygon(List<Contour> contours)
        {
            // При игнорировании ID библиотека фигова замыкает контур области
            // иногда не корректно работатет
            //foreach (var contour in contours)
            //    polygon.Add(contour);
            IPolygon polygon = new Polygon();
            int k = 0;
            foreach (var contour in contours)
                for (int i = 0; i < contour.Points.Count; i++)
                {
                    Vertex vi = contour.Points[i];
                    vi.ID = k;
                    polygon.Add(vi);
                    k++;
                }
            return polygon;
        }
        public static List<Contour> CreateContoursRectangle(double L = 10, double H = 5)
        {
            int label = 0;
            int N = 11;
            double dx = L / (N - 1);
            double dy = H / (N - 1);

            List<Contour> contours = new List<Contour>();
            var pointsB = new List<Vertex>();
            for (int i = 0; i < N - 1; i++)
                pointsB.Add(new Vertex(dx * i, 0, label));
            Contour contourB = new Contour(pointsB, label++);
            contours.Add(contourB);
            var pointsR = new List<Vertex>();
            for (int i = 0; i < N - 1; i++)
                pointsR.Add(new Vertex(L, dy * i, label));
            Contour contourR = new Contour(pointsR, label++);
            contours.Add(contourR);
            var pointsT = new List<Vertex>();
            for (int i = 0; i < N - 1; i++)
                pointsT.Add(new Vertex(L - dx * i, H, label));
            Contour contourT = new Contour(pointsT, label++);
            contours.Add(contourT);
            var pointsL = new List<Vertex>();
            for (int i = 0; i < N - 1; i++)
                pointsL.Add(new Vertex(0, H - i * dy, label));
            Contour contourL = new Contour(pointsL, label++);
            contours.Add(contourL);
            return contours;
        }
        public static IPolygon CreateRectanglePolygon(double L = 10, double H = 5)
        {
            return CreatePolygon(CreateContoursRectangle(L, H));
            //IPolygon polygon = new Polygon();
            //var poly = new Polygon();
            //int label = 1;
            //poly.Add(new Contour(new Vertex[4]
            //{
            //    new Vertex(0, 0, label),
            //    new Vertex(L, 0, label),
            //    new Vertex(L, H, label),
            //    new Vertex(0, H, label)
            //}, label));
            //return poly;
        }
        public static IPolygon CreateRectanglePolygon1(double L = 10, double H = 5)
        {
            var poly = new Polygon();
            int label = 1;
            poly.Add(new Contour(new Vertex[4]
            {
                new Vertex(0, 0, label),
                new Vertex(L, 0, label),
                new Vertex(L, H, label),
                new Vertex(0, H, label)
            }, label));
            return poly;
        }
        public static IPolygon CreateCirclePolygon(double r = 1, double h = 0.1)
        {
            var poly = new Polygon();
            var center = new Point(0, 0);
            // Outer contour.
            poly.Add(Circle(r, center, h, 3));

            return poly;
        }

        public static IPolygon CreateRectangle_Tube(int Nx, int Ny, int Nr, double L = 10, double H = 5, double Xh = 5, double Yh = 0.6, double Rh = 0.5, bool hole=false, bool flag = false)
        {
            var poly = new Polygon();
            List<Vertex> Points = new List<Vertex>();
            List<int> Markers = new List<int>();
            double dx = L / (Nx - 1);
            double dy = H / (Ny - 1);
            // дно
            int label = 1;
            for (int i = 0; i < Nx; i++)
            { 
                Points.Add(new Vertex(dx * i, 0, label)); 
                Markers.Add(label); 
            }
            // исток
            label = 2;
            for (int j = 0; j < Ny; j++)
            {
                Points.Add(new Vertex(L, j * dy, label)); 
                Markers.Add(label);
            }
            // WL
            label = 3;
            for (int i = 0; i < Nx; i++)
            {
                Points.Add(new Vertex(L - dx * i, H, label));
                Markers.Add(label);
            }
            // вток
            label = 4;
            for (int j = 0; j < Ny; j++)
            {
                Points.Add(new Vertex(0, H - dy*j, label));
                Markers.Add(label);
            }
            poly.Add(new Contour(Points, Markers));

            if (hole == true)
            {
                Point center;
                label = 5;
                //bool flag = false;
                Vertex[] hole0;
                if (flag == false)
                {
                    hole0 = new Vertex[4]
                       {
                     new Vertex(L/2, 0.2, label),
                     new Vertex(L/2+1, 0.2, label),
                     new Vertex(L/2+1, 1.2, label),
                     new Vertex(L/2, 1.2, label)
                       };
                    center = new Point(L / 2 + 0.5, .6);
                }
                else
                {
                    center = new Point(Xh, Yh);
                    var points = new List<Vertex>(Nr);
                    double x, y, dphi = 2 * Math.PI / Nr;
                    for (int i = 0; i < Nr; i++)
                    {
                        x = center.X + Rh * Math.Cos(i * dphi);
                        y = center.Y + Rh * Math.Sin(i * dphi);

                        points.Add(new Vertex(x, y, label));
                    }
                    hole0 = points.ToArray();

                }
                poly.Add(new Contour(hole0, label), center);
            }
            return poly;
        }

        public static IPolygon CreateATubePolygon(double L = 10, double H = 5, double Xh = 5, double Yh = 0.6, double Rh = 0.5, bool flag = false)
        {
            double h = 0.1;
            var poly = new Polygon();
            Point center;
            int label = 1;
            poly.Add(new Contour(new Vertex[4]
            {
                new Vertex(0, 0, label),
                new Vertex(L, 0, label),
                new Vertex(L, H, label),
                new Vertex(0, H, label)
            }, label));
            label = 2;

            // Vertex[] hole0 = new Vertex[4]
            //{
            //      new Vertex(L/2, 0.2, label),
            //      new Vertex(L/2+1, 0.2, label),
            //      new Vertex(L/2+1, 1.2, label),
            //      new Vertex(L/2, 1.2, label)
            //};
            //poly.Add(new Contour(hole0, 2), new Point(L / 2 + 0.5, .6));


            label = 2;
            //bool flag = false;
            Vertex[] hole0;
            if (flag == false)
            {
                hole0 = new Vertex[4]
                   {
                     new Vertex(L/2, 0.2, label),
                     new Vertex(L/2+1, 0.2, label),
                     new Vertex(L/2+1, 1.2, label),
                     new Vertex(L/2, 1.2, label)
                   };
                center = new Point(L / 2 + 0.5, .6);
            }
            else
            {
                int n = (int)(2 * Math.PI * Rh / h);
                center = new Point(Xh, Yh);
                var points = new List<Vertex>(n);
                double x, y, dphi = 2 * Math.PI / n;
                for (int i = 0; i < n; i++)
                {
                    x = center.X + Rh * Math.Cos(i * dphi);
                    y = center.Y + Rh * Math.Sin(i * dphi);

                    points.Add(new Vertex(x, y, label));
                }
                hole0 = points.ToArray();

            }
            poly.Add(new Contour(hole0, 2), center);

            // Inner contour (hole).
            //poly.Add(Circle(Rh, center, h, label), center);

            return poly;
        }
        public static IPolygon CreateRingPolygon(double r = 4.0, double h = 0.2)
        {
            var poly = new Polygon();

            var center = new Point(0, 0);

            // Radius should be at least 4.0.
            r = Math.Max(r, 4.0);

            // Outer contour.
            poly.Add(Circle(r, center, h, 3));

            // Center contour (internal).
            poly.Add(Circle((r + 2.0) / 2, center, h, 2));

            // Inner contour (hole).
            poly.Add(Circle(2.0, center, h, 1), center);

            return poly;
        }
        /// <summary>
        /// Create a circular contour.
        /// </summary>
        /// <param name="r">The radius.</param>
        /// <param name="center">The center point.</param>
        /// <param name="h">The desired segment length.</param>
        /// <param name="label">The boundary label.</param>
        /// <returns>A circular contour.</returns>
        public static Contour Circle(double r, Point center, double h, int label = 0)
        {
            int n = (int)(2 * Math.PI * r / h);

            var points = new List<Vertex>(n);

            double x, y, dphi = 2 * Math.PI / n;

            for (int i = 0; i < n; i++)
            {
                x = center.X + r * Math.Cos(i * dphi);
                y = center.Y + r * Math.Sin(i * dphi);

                points.Add(new Vertex(x, y, label));
            }

            return new Contour(points, label, true);
        }
    }
}
