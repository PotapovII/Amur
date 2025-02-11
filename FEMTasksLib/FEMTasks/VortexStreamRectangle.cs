#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                    Проект "Home" dNdy++
//                  - (C) Copyright 2024
//                        Потапов И.И.
//                         21.12.24
//---------------------------------------------------------------------------
// Тестовая задача Стокса в переменных Phi,Vortex
// две степени свободы в узле, с "точными" граничными условиями для Vortex
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using AlgebraLib;
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
    using MemLogLib;
    using MeshAdapterLib;
    using MeshLib;
    using System;
    /// <summary>
    ///  ОО: Решатель для задачи Навье-Стокса в формулировке вихрь-функция тока
    /// </summary>
    [Serializable]
    public class VortexStreamRectangle : AFETask
    {
        /// <summary>
        /// шаг сетки по Х
        /// </summary>
        protected double l;
        /// <summary>
        /// шаг сетки по У
        /// </summary>
        protected double h;
        /// <summary>
        /// узлов по Х
        /// </summary>
        protected int Nx;
        /// <summary>
        /// узлов по Y
        /// </summary>
        protected int Ny;
        /// <summary>
        /// Количество неизвестных в САУ
        /// </summary>
        protected int CountU;
        /// <summary>
        /// Создаваемая КЭ сетка 
        /// </summary>
        protected ComplecsMesh cmesh = null;
        /// <summary>
        /// Карта КЭ сетки
        /// </summary>
        protected uint[,] map = null;
        /// <summary>
        /// неизвестных в узле сетки
        /// </summary>
        protected uint cs = 2;
        /// <summary>
        /// Вектор неизвестных
        /// </summary>
        protected double[] PhiVortex;
        /// <summary>
        /// функия тока
        /// </summary>
        public double[] Phi;
        /// <summary>
        /// функция вихря
        /// </summary>
        public double[] Vortex;
        /// <summary>
        /// скорость по Х
        /// </summary>
        public double[] U;
        /// <summary>
        /// скорость по У
        /// </summary>
        public double[] V;
        /// <summary>
        /// функция вихря на разностной сетке
        /// </summary>
        protected double[][] mVortex = null;
        /// <summary>
        /// функия тока на разностной сетке
        /// </summary>
        protected double[][] mPhi = null;
        /// <summary>
        /// скорость по Х на разностной сетке
        /// </summary>
        protected double[][] mU = null;
        /// <summary>
        /// скорость по У на разностной сетке
        /// </summary>
        protected double[][] mV = null;
        /// <summary>
        /// адреса функии тока на границе
        /// </summary>
        protected uint[] bcIndex = null;
        /// <summary>
        /// номера граничных узлов
        /// </summary>
        protected int[] Index = null;
        /// <summary>
        /// маркеры граничных узлов
        /// </summary>
        protected int[] Marker = null;
        /// <summary>
        /// адреса строки САУ
        /// </summary>
        protected uint[] ColAdress = null;
        /// <summary>
        /// Скорость крышки
        /// </summary>
        protected double u;

        public VortexStreamRectangle(double u = 1, double l = 1, double h = 1, int Nx = 20, int Ny = 20)
        {
            this.u = u;
            this.l = l;
            this.h = h;
            this.Nx = Nx;
            this.Ny = Ny;
            // создаем сетку для прямоугольной тестовой области
            CreateMesh.GetRectangleMesh_XY(ref cmesh, Nx, Ny, l, h, ref map);
            CountU = 2 * cmesh.CountKnots;
            // создаем дискретный аналог - ЛМЖ для задачи вихрь функция тока
            double t1 = 0.1e1 / l * h;
            double t2 = l / h;
            double t3 = -t1 / 0.3e1 - t2 / 0.3e1;
            double t4 = t1 / 0.3e1 - t2 / 0.6e1;
            double t5 = -t3 / 0.2e1;
            t1 = -t1 / 0.6e1 + t2 / 0.3e1;
            t2 = l * h / 0.9e1;
            double t6 = l * h / 0.18e2;
            double t7 = l * h / 0.36e2;
            LaplMatrix = new double[8][];
            LaplMatrix[0] = new double[8] { t3, t2, t4, t6, t5, t7, t1, t6 };
            LaplMatrix[1] = new double[8] { 0, t3, 0, t4, 0, t5, 0, t1 };
            LaplMatrix[2] = new double[8] { t4, t6, t3, t2, t1, t6, t5, t7 };
            LaplMatrix[3] = new double[8] { 0, t4, 0, t3, 0, t1, 0, t5 };
            LaplMatrix[4] = new double[8] { t5, t7, t1, t6, t3, t2, t4, t6 };
            LaplMatrix[5] = new double[8] { 0, t5, 0, t1, 0, t3, 0, t4 };
            LaplMatrix[6] = new double[8] { t1, t6, t5, t7, t4, t6, t3, t2 };
            LaplMatrix[7] = new double[8] { 0, t1, 0, t5, 0, t4, 0, t3 };
            LOG.Print("LaplMatrix", LaplMatrix, 3, 4);
            // выделяем память
            MEM.Alloc(cmesh.CountKnots, ref Phi);
            MEM.Alloc(cmesh.CountKnots, ref Vortex);
            MEM.Alloc(cmesh.CountKnots, ref U);
            MEM.Alloc(cmesh.CountKnots, ref V);
            MEM.Alloc(CountU, ref PhiVortex);
            MEM.Alloc(Nx, Ny, ref mU);
            MEM.Alloc(Nx, Ny, ref mV);
            MEM.Alloc(Nx, Ny, ref mPhi);
            MEM.Alloc(Nx, Ny, ref mVortex);
            // создаем алгебру
            algebra = new AlgebraLU((uint)CountU);
            mesh = cmesh;
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            Index = mesh.GetBoundKnots();
            Marker = mesh.GetBoundKnotsMark();
            for (int i = 0; i < bcIndex.Length; i++)
                bcIndex[i] = (uint)Index[i] * cs;
            knots = new uint[4] { 0, 0, 0, 0 };
            
            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
        }

        public VortexStreamRectangle(double u = 1, int Nx = 22, int Ny = 20)
        {
            this.u = u;
            this.l = 1.0 / (Nx - 1);
            this.h = 1.0 / (Ny - 1);
            this.Nx = Nx;
            this.Ny = Ny;
            // создаем сетку для прямоугольной тестовой области
            CreateMesh.GetRectangleMesh_XY(ref cmesh, Nx, Ny, l, h, ref map);
            CountU = 2 * cmesh.CountKnots;
            // создаем дискретный аналог - ЛМЖ для задачи вихрь функция тока
            double t1 = 0.1e1 / l * h;
            double t2 = l / h;
            double t3 = -t1 / 0.3e1 - t2 / 0.3e1;
            double t4 = t1 / 0.3e1 - t2 / 0.6e1;
            double t5 = -t3 / 0.2e1;
            t1 = -t1 / 0.6e1 + t2 / 0.3e1;
            t2 = l * h / 0.9e1;
            double t6 = l * h / 0.18e2;
            double t7 = l * h / 0.36e2;
            LaplMatrix = new double[8][];
            LaplMatrix[0] = new double[8] { t3, t2, t4, t6, t5, t7, t1, t6 };
            LaplMatrix[1] = new double[8] { 0, t3, 0, t4, 0, t5, 0, t1 };
            LaplMatrix[2] = new double[8] { t4, t6, t3, t2, t1, t6, t5, t7 };
            LaplMatrix[3] = new double[8] { 0, t4, 0, t3, 0, t1, 0, t5 };
            LaplMatrix[4] = new double[8] { t5, t7, t1, t6, t3, t2, t4, t6 };
            LaplMatrix[5] = new double[8] { 0, t5, 0, t1, 0, t3, 0, t4 };
            LaplMatrix[6] = new double[8] { t1, t6, t5, t7, t4, t6, t3, t2 };
            LaplMatrix[7] = new double[8] { 0, t1, 0, t5, 0, t4, 0, t3 };
            LOG.Print("LaplMatrix", LaplMatrix, 3, 4);
            // выделяем память
            MEM.Alloc(cmesh.CountKnots, ref Phi);
            MEM.Alloc(cmesh.CountKnots, ref Vortex);
            MEM.Alloc(cmesh.CountKnots, ref U);
            MEM.Alloc(cmesh.CountKnots, ref V);
            MEM.Alloc(CountU, ref PhiVortex);
            MEM.Alloc(Nx, Ny, ref mU);
            MEM.Alloc(Nx, Ny, ref mV);
            MEM.Alloc(Nx, Ny, ref mPhi);
            MEM.Alloc(Nx, Ny, ref mVortex);
            // создаем алгебру
           // algebra = new AlgebraLU((uint)CountU);
            algebra = new AlgebraGauss((uint)CountU);
            mesh = cmesh;
            MEM.Alloc(mesh.CountBoundKnots, ref bcIndex);
            Index = mesh.GetBoundKnots();
            Marker = mesh.GetBoundKnotsMark();
            for (int i = 0; i < bcIndex.Length; i++)
                bcIndex[i] = (uint)Index[i] * cs;
            knots = new uint[4] { 0, 0, 0, 0 };

            MEM.Alloc(algebra.N, ref ColAdress);
            for (uint i = 0; i < ColAdress.Length; i++)
                ColAdress[i] = i;
        }

        public override void SolveTask(ref double[] result)
        {
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
            double Right;
            for (int i = 0; i < bcIndex.Length; i++)
            {
                if (Marker[i] == 2)
                    Right = - u * l;
                else
                    Right = 0;
                VortexBC(bcIndex[i], ColAdress, Right);
            }
            //algebra.Print(3,8);
            algebra.BoundConditions(0, bcIndex);
            //algebra.Print(3,8);
            algebra.Solve(ref PhiVortex);
            result = PhiVortex;
            CalkUV();
        }
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        /// <param name="IndexRow"></param>
        /// <param name="ColAdress"></param>
        /// <param name="Right"></param>
        protected void VortexBC(uint IndexRow, uint[] ColAdress, double Right)
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
        public void GetMatric(double[] v, ref double[][] m)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    m[i][j] = v[i * Ny + j];
        }
        public void GetVector(double[][] m, ref double[] v)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    v[i * Ny + j] = m[i][j];
        }

        protected void CalkUV()
        {
          //  LOG.Print("PhiVortex", PhiVortex, 3);
            for (int node = 0; node < mesh.CountKnots; node++)
            {
                Phi[node] = PhiVortex[node * cs];
                Vortex[node] = PhiVortex[node * cs + 1];
            }
            //LOG.Print("Vortex", Vortex, 3);
            //LOG.Print("Phi", Phi, 5);
            GetMatric(Phi, ref mPhi);
            GetMatric(Vortex, ref mVortex);
            //LOG.Print("mPhi", mPhi, 5);
            //LOG.Print("mVortex", mVortex, 5);

            for (uint i = 0; i < Nx; i++)
            {
                for (int j = 1; j < Ny - 1; j++)
                    mU[i][j] = (mPhi[i][j + 1] - mPhi[i][j - 1]) / (2 * h);
                mU[i][Ny - 1] = (mPhi[i][Ny - 1] - mPhi[i][Ny - 2]) / h;
            }
            for (uint i = 1; i < Nx - 1; i++)
            {
                for (int j = 0; j < Ny; j++)
                    mV[i][j] = -(mPhi[i + 1][j] - mPhi[i - 1][j]) / (2 * l);
            }
            for (int j = 0; j < Ny; j++)
                mV[0][j] = -(mPhi[1][j] - mPhi[0][j]) / l;

            //LOG.Print("mV", mV, 5);
            //LOG.Print("mU", mU, 5);

            GetVector(mU, ref U);
            GetVector(mV, ref V);

            // Console.Read();
        }
    }
}



