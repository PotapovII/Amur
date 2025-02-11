namespace TestSUPG
{
    using AlgebraLib;
    using CommonLib;
    using MemLogLib;
    using MeshAdapterLib;
    using MeshLib;
    using System;
    using System.Linq;

    //
    //  *----------------------*---> Y , j
    //  |                      V Ny
    //  |                      V
    //  |                      V  Velosity
    //  |                      V
    //  |                      V
    //  *----------------------*
    //  | Nx
    //  V X, i

    /// <summary>
    /// Тестовая задача Стокса в переменных Phi,Vortex
    /// две степени свободы в узле, с "точными" граничными условиями для Vortex
    /// </summary>
    class TestStoksFem2D
    {
        public double[][] LaplMatrix = new double[8][];
        public double h, l;
        int Nx;
        int Ny;
        int CountS, CountU;
        public double[] Phi;
        public double[] Vortex;
        public double[] PhiVortex ;
        public double[] U ;
        public double[] V ;

        public double[][] mVortex = null;
        public double[][] mPhi = null;
        public double[][] mU = null;
        public double[][] mV = null;

        public uint[] bcIndex = null;
        public int[] Index = null;
        public uint[] bcIndexBed = null;
        public uint[] bcIndexTop = null;
        uint cs = 2;
        public const double u = 1;
        IAlgebra algebra = null;
        IMesh mesh = null;
        ComplecsMesh cmesh = null;
        uint[,] map = null;
        public TestStoksFem2D(int Nx=20, int Ny=20, double l=1, double h=1)
        {
            this.l = l;
            this.h = h;
            this.Nx = Nx;
            this.Ny = Ny;
            CountS = Nx * Ny;
            CountU = 2*CountS;

            double t1 = 0.1e1 / l * h;
            double t2 = l / h;
            double t3 = -t1 / 0.3e1 - t2 / 0.3e1;
            double t4 = t1 / 0.3e1 - t2 / 0.6e1;
            double t5 = -t3 / 0.2e1;
            t1 = -t1 / 0.6e1 + t2 / 0.3e1;
            t2 = l * h / 0.9e1;
            double t6 = l * h / 0.18e2;
            double t7 = l * h / 0.36e2;
            LaplMatrix[0] = new double[8] { t3, t2, t4, t6, t5, t7, t1, t6 };
            LaplMatrix[1] = new double[8] { 0, t3, 0, t4, 0, t5, 0, t1 };
            LaplMatrix[2] = new double[8] { t4, t6, t3, t2, t1, t6, t5, t7 };
            LaplMatrix[3] = new double[8] { 0, t4, 0, t3, 0, t1, 0, t5 };
            LaplMatrix[4] = new double[8] { t5, t7, t1, t6, t3, t2, t4, t6 };
            LaplMatrix[5] = new double[8] { 0, t5, 0, t1, 0, t3, 0, t4 };
            LaplMatrix[6] = new double[8] { t1, t6, t5, t7, t4, t6, t3, t2 };
            LaplMatrix[7] = new double[8] { 0, t1, 0, t5, 0, t4, 0, t3 };

            MEM.Alloc(CountS, ref Phi);
            MEM.Alloc(CountS, ref Vortex);
            MEM.Alloc(CountS, ref U);
            MEM.Alloc(CountS, ref V);
            MEM.Alloc(CountU, ref PhiVortex);
            MEM.Alloc(Nx, Ny, ref mU);
            MEM.Alloc(Nx, Ny, ref mV);
            MEM.Alloc(Nx, Ny, ref mPhi);
            MEM.Alloc(Nx, Ny, ref mVortex);
            
            algebra = new AlgebraLU((uint)CountU);
            LOG.Print("LaplMatrix", LaplMatrix, 3, 4);
            
            CreateMesh.GetRectangleMesh_XY(ref cmesh, Nx, Ny, l, h, ref map);
            mesh = cmesh;
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            Index = mesh.GetBoundKnots();
            for (int i = 0; i < bcIndex.Length; i++)
                bcIndex[i] = (uint)Index[i] * cs;
    }

    public void Do()
        {
            uint[] knots = { 0, 0, 0, 0 };
            uint cs = 2;
            algebra.Clear();

            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                uint[] idx = {
                        knots[0] * cs,
                        knots[0] * cs + 1,
                        knots[1] * cs,
                        knots[1] * cs + 1,
                        knots[2] * cs,
                        knots[2] * cs + 1,
                        knots[3] * cs,
                        knots[3] * cs + 1
                    };
                algebra.AddToMatrix(LaplMatrix, idx);
                //algebra.Print(3, 8);
            }
            
            //algebra.Print(3,8);
            // граничные уцсловия для функции тока
            uint[] ColAdress = null;
            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
            int[] Marker = mesh.GetBoundKnotsMark();
            double Right;
            for (int i = 0; i < bcIndex.Length; i++)
            {
                if (Marker[i] == 2)
                    Right = - u;
                else
                    Right = 0;
                VortexBC(bcIndex[i], ColAdress, Right);
            }
            algebra.BoundConditions(0, bcIndex);
            algebra.Solve(ref PhiVortex);

            LOG.Print("PhiVortex", PhiVortex, 3);
            for (int node = 0; node < CountS; node++)
            {
                Phi[node] = PhiVortex[node * cs];
                Vortex[node] = PhiVortex[node * cs + 1];
            }
            LOG.Print("Vortex", Vortex, 3);
            LOG.Print("Phi", Phi, 5);
            GetMatric(Phi, ref mPhi);
            GetMatric(Vortex, ref mVortex);
            LOG.Print("mPhi", mPhi, 5);
            LOG.Print("mVortex", mVortex, 5);

            for (uint i = 0; i < Nx; i++)
            {
                for (int j = 1; j < Ny - 1; j++)
                    mV[i][j] = (mPhi[i][j + 1] - mPhi[i][j - 1]) / (2 * h);
                mV[i][Ny - 1] = (mPhi[i][Ny - 1] - mPhi[i][Ny - 2]) / h;
            }
            for (uint i = 1; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny; j++)
                    mU[i][j] = (mPhi[i+1][j] - mPhi[i-1][j]) / (2 * l);
            }
            LOG.Print("mV", mV, 5);
            LOG.Print("mV", mU, 5);

            Console.WriteLine("map");
            for (uint i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                    Console.Write(map[i,j].ToString("F3") + " ");
                Console.WriteLine();
            }
            LOG.Print("bcIndex", bcIndex);
            LOG.Print("Index", Index);
            LOG.Print("Marker", Marker);
            Console.Read();
        }
        public void GetMatric(double[] v, ref double[][] m)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    m[i][j] = v[i * Ny + j];
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
