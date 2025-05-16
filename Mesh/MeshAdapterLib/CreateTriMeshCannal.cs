namespace MeshAdapterLib
{
    using System;

    using MeshLib;
    using TriangleNet;

    using TriangleNet.Tools;
    using TriangleNet.Meshing;
    using TriangleNet.Geometry;
    using TriangleNet.Smoothing;

    public class CreateTriMeshCannal
    {

        public static TriMesh GetMesh(QualityMeshNetOptions op, int Nx, int Ny,
                                      int Nr, double L, double H, double Xh, double Yh,
                                      double Rh, bool hole = false)
        {
            // создать контур
            IPolygon polygon = CreateArea.CreateRectangle_Tube(Nx, Ny, Nr, L, H, Xh, Yh, Rh, hole, true);
            MeshNet meshDel1 = CreateCrossSection.Create(polygon, op);
            TriMesh mesh = MeshAdapter.ConvertMeshNetToTriMesh(meshDel1);
            return mesh;
        }

        /// <summary>
        /// Создание сетки mesh для прямоугольного канала L x H с круглой дыркой радиуса Rh
        /// с центров в точке (Xh, Yh)
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="H"></param>
        /// <param name="L"></param>
        /// <param name="Xh"></param>
        /// <param name="Yh"></param>
        /// <param name="Rh"></param>
        /// <param name="diametrFE"></param>
        public static void GetMesh(ref TriMesh mesh, double H, double L,
            double Xh, double Yh, double Rh, double diametrFE, bool sm = false)
        {
            ConstraintOptions options = new ConstraintOptions();
            QualityOptions quality = new QualityOptions();
            options.SegmentSplitting = 1;
            Statistic statisticMesh = new Statistic();
            // создать контур
            double dx = 1.0 * diametrFE / 100;
            int Nx = (int)(L / dx) + 1;
            int Ny = (int)(H / dx) + 1;
            int Nr = (int)(2 * Math.PI * Rh / dx) + 1;
            IPolygon polygon = CreateArea.CreateRectangle_Tube(Nx, Ny, Nr, L, H, Xh, Yh, Rh, true);
            double newLargestArea = 0.3 * dx * dx;
            quality.MaximumArea = newLargestArea;
            // генерация сетки контуру
            MeshNet meshDel1 = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
            //if (sm == true)
            //    for (int i = 0; i < 1; i++)
            //    {
            //        var smoother = new SimpleSmoother();
            //        smoother.Smooth(meshDel1);
            //    }
            // приведение сетки к 
            mesh = MeshAdapter.ConvertMeshNetToTriMesh(meshDel1);
        }

        /// <summary>
        /// Создание сетки mesh для прямоугольного канала L x H с круглой дыркой радиуса Rh
        /// с центров в точке (Xh, Yh)
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="H"></param>
        /// <param name="L"></param>
        /// <param name="Xh"></param>
        /// <param name="Yh"></param>
        /// <param name="Rh"></param>
        /// <param name="diametrFE"></param>
        public static void GetMesh(ref TriMesh mesh, int Nx, int Ny,
            int Nr, double L, double H,
            double Xh, double Yh,
            double Rh, bool hole = false, bool sm = false)
        {
            ConstraintOptions options = new ConstraintOptions();
            QualityOptions quality = new QualityOptions();
            options.SegmentSplitting = 1;
            //Statistic statisticMesh = new Statistic();
            // создать контур
            IPolygon polygon = CreateArea.CreateRectangle_Tube(Nx, Ny, Nr, L, H, Xh, Yh, Rh, hole, true);
            double dx = (L / Nx - 1);
            double dy = (H / Ny - 1);
            double newLargestArea = 0.01 * dx * dy;
            quality.MaximumArea = newLargestArea;
            // генерация сетки контуру
            MeshNet meshDel1 = (MeshNet)(new GenericMesher()).Triangulate(polygon, options, quality);
            if (sm == true)
                for (int i = 0; i < 10; i++)
                {
                    var smoother = new SimpleSmoother();
                    smoother.Smooth(meshDel1);
                }
            // приведение сетки к 
            mesh = MeshAdapter.ConvertMeshNetToTriMesh(meshDel1);
        }


        public static void GetIPolygon(ref TriMesh mesh, int Nx, int Ny,
                            int Nr, double L, double H,
                            double Xh, double Yh,
                            double Rh, bool hole = false, bool sm = false)
        {
            IPolygon polygon = CreateArea.CreateRectangle_Tube(Nx, Ny, Nr, L, H, Xh, Yh, Rh, hole, true);
        }
    }
}
