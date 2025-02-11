
namespace TestSUPG
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;
    /// <summary>
    /// Одномерный тест для задачи в постановке 
    /// вихрь - функция тока
    /// в без итерационной формулировки КЭ задачи
    /// </summary>
    class TestFEM0
    {
        

        public const int N = 11;
        public const int n = N - 1;
        public const double H = 10;
        public const double u = 1;
        public const double h = H/n;
        public const double h2 = h * h;
        public double[] Phi = new double[N];
        public double[] Vortex = new double[N];
        public double[] PhiVortex = new double[2*N];
        public double[] PhiVortex_old = new double[2*N];
        public double[] U = new double[N];
        public double[] V = new double[N];
        public double[] mu = new double[N];
        public double[] VortexA = new double[N];
        public double[] PhiA = new double[N];
        public double[] VA = new double[N];

        public double[][] LaplMatrix = new double[4][];
        public double[] bc = { 0, 0 };
        public uint[] bcIndex = { 0,  2 * N-2 };
        public uint cs = 2;
        IAlgebra algebra = null;
        IMesh mesh = null;   
        public TestFEM0()
        {
            LaplMatrix[0] = new double[4] { -1 / h, h / 3.0,  1 / h, h / 6.0 };
            LaplMatrix[1] = new double[4] { 0,     -1 / h,    0,     1 / h };
            LaplMatrix[2] = new double[4] { 1 / h,  h / 6.0, -1 / h, h / 3.0 };
            LaplMatrix[3] = new double[4] { 0,      1 / h,    0,    -1 / h };

            mesh = new TwoMesh(N, H);
            algebra = new AlgebraLU(2*N);
            LOG.Clear();
            LOG.Print("LaplMatrix", LaplMatrix, 3);
        }

        public void Do()
        {
            for (int node = 0; node < N; node++)
            {
                double z = node * h / H;
                VortexA[node] = - u / H * (6 * z - 2);
                VA[node] = u * z * (3 * z - 2);
                PhiA[node] = u * H * z * z * (z - 1);
            }
            uint[] knots = { 0, 0 };

            
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                uint[] idx = {
                        knots[0] * cs,
                        knots[0] * cs + 1,
                        knots[1] * cs,
                        knots[1] * cs + 1
                    };
                algebra.AddToMatrix(LaplMatrix, idx);
            
            }
            algebra.Print(3, 4);
            double[] ColElems = null;
            double[] ColElems1 = null;
            uint[] ColAdress = null;
            MEM.Alloc(algebra.N, ref ColElems);
            MEM.Alloc(algebra.N, ref ColElems1);
            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
            double R = 0;
            bool Flag = false;
            if (Flag == true)
            {
                algebra.BoundConditions(bc, bcIndex);
                uint IndexRow = 1;
                ColElems[IndexRow] = 1;
                ColElems[IndexRow + 1] = 2 / h2;
                R = 0;
                algebra.AddStringSystem(ColElems, ColAdress, IndexRow, R);
                R = -2 * u / h;
                IndexRow = 2 * N - 1;
                ColElems1[IndexRow - 3] = 2 / h2;
                ColElems1[IndexRow] = 1;
                algebra.AddStringSystem(ColElems1, ColAdress, IndexRow, R);
                algebra.Print(3, 4);
                algebra.Solve(ref PhiVortex);
            }
            else
            {
                uint IndexRow = 0;
                R = 0;
                VortexBC(IndexRow, ColAdress, R);
                IndexRow = 2 * N - 2;
                R = - u;
                VortexBC(IndexRow, ColAdress, R);
                algebra.BoundConditions(bc, bcIndex);
                algebra.Print(3, 4);
                algebra.Solve(ref PhiVortex);
            }
       
            algebra.Print(3,4);
            LOG.Print("X", PhiVortex, 3);
            for (int node = 0; node < N; node++)
            {
                Phi[node] = PhiVortex[node * cs];
                Vortex[node] = PhiVortex[node * cs + 1];
            }
            for (int i = 1; i < n; i++)
            {
                V[i] = (Phi[i + 1] - Phi[i - 1]) / (2 * h);
            }

            V[n] = (Phi[n] - Phi[n - 1]) / h;

            LOG.Print("VortexAnalytics", VortexA, 5);
            LOG.Print("Vortex", Vortex, 5);
            LOG.Print("PhiAnalytics", PhiA, 5);
            LOG.Print("Phi", Phi, 5);
            LOG.Print("VAnalytics", VA, 5);
            LOG.Print("V", V, 5);

            Console.Read();
        }


        public void VortexBC(uint IndexRow, uint[] ColAdress, double Right)
        {
            double R = 0;
            double[] ColElems = null;
            MEM.Alloc(algebra.N, ref ColElems);
            algebra.GetStringSystem(IndexRow, ref ColElems, ref R);
            double sumVortex = 0;
            for (int i = 0; i < ColElems.Length; i++)
            {
                if (i % cs == 1)
                {
                    sumVortex += ColElems[i]; ColElems[i] = 0;
                }
            }
            ColElems[IndexRow + 1] = sumVortex;
            IndexRow++;
            algebra.AddStringSystem(ColElems, ColAdress, IndexRow, R + Right);
        }
    }
}
