namespace TestSUPG
{
    using MemLogLib;
    using System;
    public class PhiVortex
    {
        int Nx = 50;
        int Ny = 30;
        int M = 1000;
        double Lx = 2, Ly = 1;
        double w = 0.3;
        double dx,dy, dS;
        double[,] Phi, Phi0;
        double[,] Vortex, Vortex0;
        double[,] mV, mW;
        double V = 0.5;
        public PhiVortex()
        {
            Phi = new double[Nx, Ny];
            Vortex = new double[Nx, Ny];
            Phi0 = new double[Nx, Ny];
            Vortex0 = new double[Nx, Ny];
            mV = new double[Nx, Ny];
            mW = new double[Nx, Ny];
            dx = Lx / (Nx - 1);
            dy = Ly / (Ny - 1);
            dS = dx * dy;
        }
        public void Solver()
        {
            Console.Clear();
            for (int n = 0; n < 1000; n++)
            {
                Copy(ref Phi0, Phi);
                Copy(ref Vortex0, Vortex);
                SolverVortex();
                Relax(ref Vortex, Vortex0);
                SolverPhi();
                Relax(ref Phi, Phi0);
                LOG.Print("Phi", Phi, 6);
                LOG.Print("Vortex", Vortex, 6);
                if (Error(Vortex0, Vortex) < 0.00001)
                {
                    Velocity();
                    LOG.Print("Phi", Phi, 6);
                    LOG.Print("Vortex", Vortex, 6);
                    LOG.Print("Vx", mV, 6);
                    LOG.Print("Vy", mW, 6);
                    Console.WriteLine("Итерация " + n.ToString());
                    break;
                }
            }
            Console.WriteLine("Нажать любую клавишу");
            Console.ReadKey();
        }
        public void SolverPhi()
        {
            for (int n = 0; n < M; n++)
            {
                for (int i = 1; i < Nx - 1; i++)
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        Phi[i, j] = 0.25 * (Phi[i + 1, j] + Phi[i - 1, j] + Phi[i, j + 1] + Phi[i, j - 1]) + 0.25 * dS * Vortex[i, j];
                    }
            }
        }
        /// <summary>
        /// ОО: RectFDMesh - базистная разностная сетка 
        /// в четырех гранной области c косым уступом
        /// 
        ///  ----0-----------------------------> y (j)
        ///      |                         |  
        ///     Lx                         |
        ///      |                         |
        ///      |                         |
        ///      |---------- Ly -----------|
        ///      |
        ///      V x (i)     
        /// 
        public void SolverVortex()
        {
            for (int i = 0; i < Nx; i++)
            {
                Vortex[i, Ny-1] = - 2 * Phi[i, Ny - 2] / (dy * dy) - 2 * V / dy;
                Vortex[i, 0]    = - 2 * Phi[i, 1] / (dy * dy);
            }
            for (int j = 0; j < Ny; j++)
            {
                Vortex[0, j]    = -2 * Phi[1, j] / (dx * dx);
                Vortex[Nx-1, j] = -2 * Phi[Nx - 2, j] / (dx * dx);
            }

            for (int n = 0; n < M; n++)
            {
                for (int i = 1; i < Nx - 1; i++)
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        Vortex[i, j] = 0.25 * (Vortex[i + 1, j] + Vortex[i - 1, j] + Vortex[i, j + 1] + Vortex[i, j - 1]);
                    }
            }
        }
        public void Copy(ref double[,] a, double[,] b)
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    a[i, j] = b[i, j];
                }
        }
        public void Relax(ref double[,] a, double[,] b)
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    a[i, j] = a[i, j] * w + b[i, j] * (1 - w) ;
                }
        }
        public double Error(double[,] a, double[,] b)
        {
            double sum = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    sum += Math.Abs(a[i, j] - b[i, j]) / (Nx* Ny);
                }
            return sum;
        }
        public void Velocity()
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 1; j < Ny - 1; j++)
                {
                    mV[i, j] = (Phi[i, j + 1] - Phi[i, j - 1]) / dy /2;
                }
            for (int i = 0; i < Nx; i++)
            {
                mV[i, Ny - 1] = (Phi[i, Ny - 1] - Phi[i, Ny - 2]) / dy;
            }
            for (int i = 1; i < Nx - 1; i++)
                for (int j = 0; j < Ny; j++)
                {
                    mW[i, j] = - (Phi[i + 1, j] - Phi[i - 1, j]) / dx /2;
                }
            for (int j = 1; j < Ny - 1; j++)
            {
                mW[0, j] = -(Phi[1, j] - Phi[0, j]) / dx;
                mW[Nx - 1, j] = -(Phi[Nx - 1, j] - Phi[Nx - 2, j]) / dx;
            }
        }
    }
}
