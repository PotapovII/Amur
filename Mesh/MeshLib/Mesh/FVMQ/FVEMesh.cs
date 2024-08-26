//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И. 
//                кодировка : 26.06.2022 Потапов И.И. 
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib.Geometry;
    using GeometryLib;
    using MemLogLib;
    
    [Serializable]
    public class FVEMesh
    {
        HPoint a = null;
        HPoint b = null;
        int imax = 0;
        int jmax = 0;
        double xa, xb, ya, yb, Lx, Ly, dx, dy;
        public int Nx => imax + 1;
        public int Ny => jmax + 1;
        public FVElem[][] elems = null;
        /// <summary>
        /// Формирование прямоугольной сетки в области (HPoint a, HPoint b)
        /// </summary>
        /// <param name="imax"></param>
        /// <param name="jmax"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public FVEMesh(int imax, int jmax, HPoint a, HPoint b)
        {
            this.imax = imax;
            this.jmax = jmax;
            this.a = new HPoint(a);
            this.b = new HPoint(b);
            xa = Math.Min(a.x, b.x);
            xb = Math.Max(a.x, b.x);
            ya = Math.Min(a.y, b.y);
            yb = Math.Max(a.y, b.y);
            Lx = xb - xa;
            Ly = yb - ya;
            dx = Lx / imax;
            dy = Ly / jmax;
            HPoint[][] p = null;
            MEM.Alloc2D(Nx, Ny, ref p);
            for (int i = 0; i < Nx; i++)
            {
                double x = xa + i * dx;
                for (int j = 0; j < Ny; j++)
                {
                    double y = ya + j * dy;
                    p[i][j] = new HPoint(x, y);
                }
            }
            MEM.Alloc2D(imax, jmax, ref elems);
            for (int i = 0; i < imax; i++)
                for (int j = 0; j < jmax; j++)
                    elems[i][j] = new FVElem(p[i][j], p[i + 1][j], p[i + 1][j + 1], p[i][j + 1]);
        }
    }
    [Serializable]
    public class FVSTask
    {
        double maxerror;
        double[][] Q = null;
        double[][] U = null;
        FVEMesh fmesh = null;
        HPoint a = null;
        HPoint b = null;
        int imax = 0;
        int jmax = 0;
        public FVSTask(int imax, int jmax, HPoint a, HPoint b)
        {
            this.imax = imax;
            this.jmax = jmax;
            this.a = new HPoint(a);
            this.b = new HPoint(b);
            fmesh = new FVEMesh(imax, jmax, a, b);
            MEM.Alloc2DClear(fmesh.Nx, fmesh.Ny, ref U, 0);
            MEM.Alloc2DClear(fmesh.Nx, fmesh.Ny, ref Q, 1);
        }
        public double Step()
        {
            maxerror = 0;
            for (int i = 1; i < imax - 1; i++)
                for (int j = 1; j < jmax - 1; j++)
                {
                    U[i][j] = 0.25 * (U[i][j] + U[i + 1][j] + U[i + 1][j + 1] + U[i][j + 1]);
                }
            return maxerror;
        }
        public void Solver()
        {
            for (int iter = 0; iter < 10000; iter++)
            {
                maxerror = Step();
                if (maxerror < MEM.Error8)
                    break;
            }
        }
    }
}
