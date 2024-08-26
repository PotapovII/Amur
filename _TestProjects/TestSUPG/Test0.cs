
namespace TestSUPG
{
    using CommonLib.Physics;
    using MemLogLib;
    using System;

    class Test0
    {
        public double w = 0.3;
        public const int N = 11;
        public const int n = N-1;
        public double[] Phi = new double[N];
        public double[] Vortex = new double[N];
        public double[] Phi_old = new double[N];
        public double[] Vortex_old = new double[N];
        public double[] U = new double[N];
        public double[] U_old = new double[N];
        public double[] V = new double[N];
        public double[] mu = new double[N];
        public double[] VortexA = new double[N];

        public void Do()
        {
            double H = 0.14;
            double u = 0.05;
            double h = H / (N - 1);
            double h2 = h*h;
            for (int node = 0; node < N; node++)
            {
                double z = node * h;
                VortexA[node] = -6 * u * z / (H * H) + 2 * u / H;
            }
            for (int sn = 0; sn < 100; sn++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);

                for (int k = 0; k < 100; k++)
                {
                    Vortex[0] = - Phi[1] / h2;
                    Vortex[n] = - 2 * Phi[n - 1] / h2 - 2*u/h;
                    //Vortex[n] =  - 2 * u / h;
                    for (int i = 1; i < N - 1; i++)
                        Vortex[i] = (Vortex[i + 1] + Vortex[i - 1])/2;
                }
                for (int i = 0; i < N; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                for (int k = 0; k < 100; k++)
                {
                    for (int i = 1; i < N - 1; i++)
                        Phi[i] = (Phi[i + 1] + Phi[i - 1]) / 2 + Vortex[i] * h2/2;
                }
                for (int i = 0; i < N; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];

                for (int i = 1; i < n; i++)
                {
                    V[i] = (Phi[i + 1] - Phi[i - 1]) / (2 * h);
                }

                V[n] = (Phi[n] - Phi[n - 1]) / h;
                
                double normPhi = 0;
                double normVortex = 0;
                double epsPhi = 0;
                double epsVortex = 0;
                for (int i = 0; i < N; i++)
                {
                    normPhi += Phi[i] * Phi[i]/N;
                    normVortex += Vortex[i] * Vortex[i] / N;
                    epsPhi += (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]) / N / N;
                    epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]) / N / N;
                }
                
                if (epsVortex < 0.00000001)
                {
                    Console.WriteLine("-- Прямой створ --");
                    LOG.Print("VortexA", VortexA, 3);
                    LOG.Print("Vortex", Vortex, 3);
                    LOG.Print("Phi", Phi, 3);
                    LOG.Print("V", V, 3);
                    Console.WriteLine(" u = {0}, h = {1}, V = {2}", u, h, V[n]);
                    Console.WriteLine(" iter = {2} epsVortex = {0}, normVortex = {1}", epsVortex, normVortex, sn);
                    break;
                }
            }
        }
        public void DoR()
        {
            //double R = 5 - 1.325;
            double R = 5;
            double R2 = R* R;
            double Mu;
            double rho = 1000;
            double J = 0.001;
            double Q = rho* 9.81*J;
            double H = 0.14;
            double u = 0.0;
            double h = H / (N - 1);
            double h2 = h * h;
            double Ap;

            //for (int i = 0; i < N; i++)
            //    mu[i] = Mu + 
            double u_star = Math.Sqrt(SPhysics.GRAV * H * J);
            for (int node = 0; node < N; node++)
            {
                double Re_star = H * u_star / SPhysics.nu;
                double C1 = Re_star / (0.46 * Re_star - 5.98);
                double Ca = Math.Exp(-(0.34 * Re_star - 11.5) / (0.46 * Re_star - 5.98));
                double xi = node * h / H;
                double mu_t0 = SPhysics.rho_w * u_star * node * h * Ca * Math.Exp(-C1 * xi);
                mu[node] = 0.2; // mu_t0 + SPhysics.mu;
            }
            for (int node = 0; node < N; node++)
            {
                double z = node * h;
                VortexA[node] = -6 * u * z * z / (H * H) + 2 * u / H;
            }

            for (int sn = 0; sn < 100; sn++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref U_old, U);

                for (int k = 0; k < 100; k++)
                {
                    U[0] = 0;
                    U[n] = U[n-1];
                    for (int i = 1; i < N - 1; i++)
                    {
                        Mu = mu[i];
                        Ap = 2 * Mu / h2; // + a * (rho * V[i] / R + 2 * Mu / R2);
                        U[i] = (Mu * (U[i + 1] + U[i - 1]) / h2 + Q) / Ap;
                    }
                }
                for (int i = 0; i < N; i++)
                    U[i] = (1 - w) * U_old[i] + w * U[i];

                for (int k = 0; k < 100; k++)
                {
                    Vortex[0] = - Phi[1] / h2 / R;
                    Vortex[n] = - 2 * Phi[n - 1] / h2 / R - 2 * u / h;
                    for (int i = 1; i < N - 1; i++)
                    {
                        Mu = mu[i];
                        Ap = 2 * Mu / h2; // + Mu / R2;
                        double u2 = U[i + 1]* U[i + 1] - U[i - 1] * U[i - 1];
                        Vortex[i] = (Mu * (Vortex[i + 1] + Vortex[i - 1]) / h2 - rho / R * u2 /(2*h) ) / Ap;
                    }
                }
                for (int i = 0; i < N; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                for (int k = 0; k < 200; k++)
                {
                    for (int i = 1; i < N - 1; i++)
                        Phi[i] = (Phi[i + 1] + Phi[i - 1]) / 2 + R * Vortex[i] * h2 / 2;
                }
                for (int i = 0; i < N; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];

                for (int i = 1; i < n; i++)
                {
                    V[i] = (Phi[i + 1] - Phi[i - 1]) / (2 * h * R);
                }

                V[n] = (Phi[n] - Phi[n - 1]) / (h * R);

              
                double normPhi = 0;
                double normVortex = 0;
                double epsPhi = 0;
                double epsVortex = 0;
                for (int i = 0; i < N; i++)
                {
                    normPhi += Phi[i] * Phi[i] / N;
                    normVortex += Vortex[i] * Vortex[i] / N;
                    epsPhi += (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]) / N / N;
                    epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]) / N / N;
                }
                
                if (epsVortex < 0.00000001)
                {
                    Console.WriteLine("-- Поворот --");
                    LOG.Print("U", U, 3);
                    LOG.Print("Vortex", Vortex, 3);
                    LOG.Print("VortexA", VortexA, 3);
                    LOG.Print("Phi", Phi, 3);
                    LOG.Print("V", V, 3);
                    LOG.Print("mu", mu, 3);
                    Console.WriteLine(" u = {0}, h = {1}, V = {2}", u, h, V[n]);
                    Console.WriteLine(" iter = {2} epsVortex = {0}, normVortex = {1}", epsVortex, normVortex, sn);
                    break;
                }
            }
        }
    }
}
