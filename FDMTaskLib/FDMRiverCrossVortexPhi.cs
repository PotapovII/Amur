namespace FDMTaskLib
{
    using System;
    using MemLogLib;
    using MeshLib.Mesh.RecMesh;
    /// <summary>
    /// ОО: Тестовая задача вихрь функция тока 
    /// </summary>
    [Serializable]
    public class FDMRiverCrossVortexPhi
    {
        int M = 1000;
        double w = 0.3;
        double V;
        double[][] Phi, Phi0;
        double[][] Vortex, Vortex0;
        double[][] mV, mW;
        int Nx,Ny;
        double Ap, Ae, Aw, An, As, dS, dx, dy;

        ChannelRectangleMesh mesh = null;
        public FDMRiverCrossVortexPhi(double V, int Nx, int Ny, double Lx,
            double Ly, double Xmin = 0, double Ymin = 0)
        {
            this.Nx = Nx;
            this.Ny = Ny;
            this.V = V;
            mesh = new ChannelRectangleMesh(Nx, Ny, Lx, Ly, Xmin, Ymin);
            MEM.Alloc(Nx, Ny, ref Phi);
            MEM.Alloc(Nx, Ny, ref Phi0);
            MEM.Alloc(Nx, Ny, ref Vortex);
            MEM.Alloc(Nx, Ny, ref Vortex0);
            MEM.Alloc(Nx, Ny, ref mV);
            MEM.Alloc(Nx, Ny, ref mW);
            Ap = 2 / (mesh.dx * mesh.dx + mesh.dy * mesh.dy);
            Ae = Aw = 1 / (mesh.dx * mesh.dx);
            An = As = 1 / (mesh.dy * mesh.dy);
            dS = mesh.dx * mesh.dy;
            dx = mesh.dx;
            dy = mesh.dy;
        }
        public void Solver()
        {
            Console.Clear();
            for (int n = 0; n < 1000; n++)
            {
                MEM.MemCopy(ref Phi0, Phi);
                MEM.MemCopy(ref Vortex0, Vortex);
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
        }

        public void Relax(ref double[][] a, double[][] b)
        {
            for (int i = 0; i < mesh.Nx; i++)
                for (int j = 0; j < mesh.Ny; j++)
                {
                    a[i][j] = a[i][j] * w + b[i][j] * (1 - w);
                }
        }
        public double Error(double[][] a, double[][] b)
        {
            double sum = 0;
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                {
                    sum += Math.Abs(a[i][j] - b[i][j]) / (Nx * Ny);
                }
            return sum;
        }

        public void SolverVortex()
        {
            for (int j = 1; j < Ny - 1; j++)
            {
                Vortex[0][j] = -2 * Phi[1][j] / (dy * dy) - 2 * V / dy;
            }
            for (int n = 0; n < M; n++)
            {
                for (int i = 1; i < Nx - 1; i++)
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        Vortex[i][j] = (Ae * Vortex[i + 1][j] + Aw * Vortex[i - 1][j] + 
                                        An * Vortex[i][j + 1] + As * Vortex[i][j - 1]) / Ap;
                    }
            }
        }
        public void SolverPhi()
        {
            for (int n = 0; n < M; n++)
            {
                for (int i = 1; i < Nx - 1; i++)
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        Phi[i][j] = (Ae * Phi[i + 1][j] + Aw * Phi[i - 1][j] + 
                                     An * Phi[i][j + 1] + As * Phi[i][j - 1] + Vortex[i][j])/ Ap;
                    }
            }
        }
        public void Velocity()
        {
            for (int i = 0; i < Nx; i++)
                for (int j = 1; j < Ny - 1; j++)
                {
                    mV[i][j] = (Phi[i][j + 1] - Phi[i][j - 1]) / dy / 2;
                }
            for (int i = 1; i < Nx - 1; i++)
                for (int j = 0; j < Ny; j++)
                {
                    mW[i][j] = -(Phi[i + 1][j] - Phi[i - 1][j]) / dx / 2;
                }
            for (int j = 0; j < Ny; j++)
            {
                mW[0][j] = -(Phi[1][j] - Phi[0][j]) / dx;
                mW[Nx - 1][j] = -(Phi[Nx - 1][j] - Phi[Nx - 2][j]) / dx;
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public virtual void Show()
        {
          //  SavePoint sp = new SavePoint("");

            //sp.SetSavePoint(0, this);
            //mesh.GetValueTo1D(mMu, ref Mu);
            //sp.Add("Mu", Mu);
            //mesh.GetValueTo1D(mesh.d_min, ref Mu);
            //sp.Add("d_min", Mu);

            //mesh.GetValueTo1D(mTauX, ref TauX);
            //sp.Add("TauX", TauX);
            //mesh.GetValueTo1D(mTauY, ref TauY);
            //sp.Add("TauY", TauY);
            //// векторное поле на сетке
            //sp.Add("Tau", TauX, TauY);


            //taskU.GetGrad(mU, ref dU_dx, ref dU_dy);

            //mesh.GetValueTo1D(dU_dx, ref TauX);
            //sp.Add("dU_dx", TauX);
            //mesh.GetValueTo1D(dU_dy, ref TauY);
            //sp.Add("dU_dy", TauY);
            //sp.Add("|dU|", TauX, TauY);

            //taskU.GetGrad(mMu, ref dMu_dx, ref dMu_dy);
            //mesh.GetValueTo1D(dMu_dx, ref TauX);
            //sp.Add("dMu_dx", TauX);
            //mesh.GetValueTo1D(dMu_dy, ref TauY);
            //sp.Add("dMu_dy", TauY);
            //// векторное поле на сетке
            //sp.Add("|dNu|", TauX, TauY);

            //GetBTau();

            //sp.AddCurve("", mesh.X[0], bNormalTau);
            //sp.AddCurve("Касательные придонные напряжения", mesh.X[0], bNormalTau);
            //sp.AddCurve("Касательные напряжения гидростатика", mesh.X[0], bNormalTauGS);
            //sp.AddCurve("Придонное давление", mesh.X[0], bPress);
            //sp.AddCurve("Донных профиль", mesh.X[0], Zeta);

        }

    }
}
