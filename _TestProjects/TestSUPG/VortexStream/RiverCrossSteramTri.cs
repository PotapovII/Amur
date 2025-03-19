#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining dNdx copy of this software and associated documentation
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
//                  - (C) Copyright 2024
//                        Потапов И.И.
//                         13.01.25
//---------------------------------------------------------------------------
// Стационарная задача о движении речного потока в створе канала
// в переменных Ux,Phi,Vortex
//---------------------------------------------------------------------------
namespace TestSUPG
{
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;
    using MeshLib.Wrappers;
    using MemLogLib;
    using System;
    using FEMTasksLib.FEMTasks.VortexStream;
    using CommonLib.EddyViscosity;
    using EddyViscosityLib;

    /// <summary>
    ///  ОО: Решатель для задачи 
    /// </summary>
    [Serializable]
    public abstract class ARiverCrossSteramTri
    {
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        protected int SigmaTask;
        /// <summary>
        /// радиус изгиба русла
        /// </summary>
        protected double RadiusMin;
        /// <summary>
        /// Шаг по времени
        /// </summary>
        protected double dt;
        /// <summary>
        /// Расчетное время
        /// </summary>
        protected double Time;
        /// <summary>
        /// вихревая вязкость
        /// </summary>
        public double[] eddyViscosity = null;
        /// <summary>
        /// Турбулентная вязкость воды
        /// </summary>
        protected double mu = 1;
        /// <summary>
        /// Функция тока
        /// </summary>
        public double[] Phi;
        /// <summary>
        /// потоковая скорость
        /// </summary>
        public double[] Ux;
        /// <summary>
        /// Правая часть
        /// </summary>
        public double[] Q;
        /// <summary>
        /// Уклон русла
        /// </summary>
        public double J;
        /// <summary>
        /// узлы на контуре
        /// </summary>
        protected uint[] bknotsUx = null;
        // узлы на WL
        protected double[] bUx = null;
        /// <summary>
        /// Определить скорости Ug на свободной поверхности потока
        /// </summary>
        public bool FixUxWL = false;
        /// <summary>
        /// Определить скорости Vg на свободной поверхности потока
        /// </summary>
        public bool FixUrWL = false;

        public int NoLineMax;

        public IDigFunction VelosityUx = null;
        public IDigFunction VelosityUy = null;

        public TransportTri taskUx;
        public NSVortexStreamTri taskPV;

        protected IMeshWrapper wMesh;

        public ARiverCrossSteramTri(double V, double J, double mu, double[] eddyViscosity,
            double dt, double Time, int NLine, int NoLineMax, int SigmaTask, double RadiusMin)
        {

            this.dt = dt;
            this.Time = Time;
            this.mu = mu;
            this.J = J;
            this.NoLineMax = NoLineMax;
            this.eddyViscosity = eddyViscosity;
            this.mu = mu;
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            taskUx = new TransportTri();
            taskPV = new NSVortexStreamTri(V, eddyViscosity, dt, Time, NoLineMax, SigmaTask, RadiusMin);
        }
        /// <summary>
        /// Установка сетки и решателей
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra"></param>
        /// <param name="algebra2"></param>
        public virtual void SetTask(IMesh mesh, IAlgebra algebra, IAlgebra algebra2)
        {
            wMesh = new MeshWrapperTri(mesh);
            taskUx.SetTask(mesh, algebra, wMesh);
            taskPV.SetTask(mesh, algebra2, wMesh);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            // Правая часть
            double RQ = SPhysics.rho_w * SPhysics.GRAV * J;
            MEM.VAlloc(mesh.CountKnots, RQ, ref Q);
            double[] X = mesh.GetCoords(0);
            if (eddyViscosity == null || eddyViscosity.Length != mesh.CountKnots)
            {
                MEM.Alloc(mesh.CountKnots, ref eddyViscosity);
                for (int i = 0; i < eddyViscosity.Length; i++)
                    eddyViscosity[i] = mu;
            }
            if (FixUxWL == false)
            {
                bknotsUx = mesh.GetBoundKnotsByMarker(0);
                MEM.VAlloc(bknotsUx.Length, 0, ref bUx);
            }
            else
            {
                int[] bknots = mesh.GetBoundKnots();
                int[] marks = mesh.GetBoundKnotsMark();
                MEM.Alloc(bknots.Length, ref bknotsUx);
                MEM.VAlloc(bknots.Length, 0, ref bUx);
                for (uint b = 0; b < bknots.Length; b++)
                {
                    bknotsUx[b] = (uint)bknots[b];
                    if (marks[b] == 2) // WL
                    {
                        double y = X[bknotsUx[b]];
                        bUx[b] = VelosityUx.FunctionValue(y);
                    }
                }
            }
        }
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public abstract void SolveTime();
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public abstract void Solve();
    }
    /// <summary>
    ///  ОО: Решатель для задачи 
    /// </summary>
    [Serializable]
    public class RiverCrossSteramTri : ARiverCrossSteramTri
    {
        /// <summary>
        /// Модель турбулентной вязкости
        /// </summary>
        public IEddyViscosityTri taskViscosity;
        public RiverCrossSteramTri(double V, double J, double mu, double[] eddyViscosity,
            double dt, double Time, int NLine, int NoLineMax, ETurbViscType eTurbViscType, int SigmaTask, double RadiusMin)
            : base(V, J, mu, eddyViscosity, dt, Time, NLine, NoLineMax, SigmaTask, RadiusMin)
        {
            BEddyViscosityParam p = new BEddyViscosityParam(NLine, SigmaTask, J, RadiusMin,  SСhannelForms.halfPorabolic);
            //taskViscosity = new AlgebraEddyViscosityTri(eTurbViscType, p);
            taskViscosity = MuManager.Get(eTurbViscType, p, TypeTask.streamY1D);
        }
        /// <summary>
        /// Установка сетки и решателей
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra"></param>
        /// <param name="algebra2"></param>
        public override void SetTask(IMesh mesh, IAlgebra algebra, IAlgebra algebra2)
        {
            base.SetTask(mesh, algebra, algebra2);
            if (taskViscosity.Cet_cs() == 1)
                taskViscosity.SetTask(mesh, algebra, wMesh);
            else
                taskViscosity.SetTask(mesh, algebra2, wMesh);
        }
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public override void SolveTime()
        {
            double R0 = 0;
            int Ring = 0;

            int NT = (int)(Time / dt) + 1;
            double[] result = null;
            for (int idx = 0; idx < NT; idx++)
            {
                taskUx.SolveTransportEquations(ref Ux, eddyViscosity, R0, Ring, Phi, bknotsUx, bUx, Q);
                //taskViscosity.SolveTask(ref eddyViscosity, Ux, J);
                taskViscosity.SolveTask(ref eddyViscosity, Ux, null, null, null, null, dt);
                taskPV.SolveTaskRe(ref result);
                MEM.Copy(ref Phi, taskPV.Phi);
                Console.WriteLine("Текущее время {0}", idx * dt);
            }
        }
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public override void Solve()
        {
            double R0 = 0;
            int Ring = 0;
            double[] result = null;
            double[] result_old = null;
            for (int idx = 0; idx < NoLineMax; idx++)
            {
                taskUx.SolveTransportEquations(ref Ux, eddyViscosity, R0, Ring, Phi, bknotsUx, bUx, Q);
                taskViscosity.SolveTask(ref eddyViscosity, Ux, null, null, null, null, dt);
                taskPV.SolveTaskRe(ref result);
                MEM.Copy(ref Phi, taskPV.Phi);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                if (idx > 0)
                {
                    double epsVortex = 0.0;
                    double normVortex = 0.0;
                    for (int i = 0; i < result.Length; i++)
                    {
                        normVortex += result_old[i] * result_old[i];
                        epsVortex += (result_old[i] - result[i]) * (result_old[i] - result[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / normVortex);
                    Console.WriteLine("n {0} точность {1}", idx, residual);
                    if (residual < MEM.Error3)
                        break;
                }
                MEM.Copy(ref result_old, result);
            }
        }
    }
    /// <summary>
    ///  ОО: Решатель для задачи 
    /// </summary>
    [Serializable]
    public class RiverCrossSteramDiffTri : ARiverCrossSteramTri
    {
        /// <summary>
        /// Функция вихря
        /// </summary>
        public double[] Vortex;
        /// <summary>
        /// Модель турбулентной вязкости k - e
        /// </summary>
        public IEddyViscosityTri taskViscosity;

        public RiverCrossSteramDiffTri(double V, double J, double mu, double[] eddyViscosity,
            double dt, double Time, int NLine, int NoLineMax,
            int idxVT, ETurbViscType eTurbViscType, int SigmaTask, double RadiusMin)
             : base(V, J, mu, eddyViscosity, dt, Time, NLine, NoLineMax, SigmaTask, RadiusMin)
        {
            BEddyViscosityParam p = new BEddyViscosityParam(NLine, SigmaTask, J, RadiusMin, SСhannelForms.halfPorabolic);
            taskViscosity = MuManager.Get(eTurbViscType, p, TypeTask.streamY1D);
        }
        /// <summary>
        /// Установка сетки и решателей
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra"></param>
        /// <param name="algebra2"></param>
        public override void SetTask(IMesh mesh, IAlgebra algebra, IAlgebra algebra2)
        {
            base.SetTask(mesh, algebra, algebra2);
            if (taskViscosity.Cet_cs() == 1)
                taskViscosity.SetTask(mesh, algebra, wMesh);
            else
                taskViscosity.SetTask(mesh, algebra2, wMesh);
        }
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public override void SolveTime()
        {
            double R0 = 0;
            int Ring = 0;

            int NT = (int)(Time / dt);
            if (NT == 0) NT = 1;
            double[] result = null;
            for (int idx = 0; idx < NT; idx++)
            {
                taskUx.SolveTransportEquations(ref Ux, eddyViscosity, R0, Ring, Phi, bknotsUx, bUx, Q);
                taskPV.SolveTaskRe(ref result);
                MEM.Copy(ref Phi, taskPV.Phi);
                MEM.Copy(ref Vortex, taskPV.Vortex);
                taskViscosity.SolveTask(ref eddyViscosity, Ux, taskPV.Vy, taskPV.Vz, Phi, Vortex, dt);
                Console.WriteLine("Текущее время {0}", idx * dt);
            }
        }
        /// <summary>
        /// Метод для диагностики
        /// </summary>
        public override void Solve()
        {
            double R0 = 0;
            int Ring = 0;
            double[] result = null;
            double[] result_old = null;
            for (int idx = 0; idx < NoLineMax; idx++)
            {
                taskUx.SolveTransportEquations(ref Ux, eddyViscosity, R0, Ring, Phi, bknotsUx, bUx, Q);
                taskPV.SolveTaskRe(ref result);
                MEM.Copy(ref Phi, taskPV.Phi);
                MEM.Copy(ref Vortex, taskPV.Vortex);
                taskViscosity.SolveTask(ref eddyViscosity, Ux, taskPV.Vy, taskPV.Vz, Phi, Vortex, dt);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                if (idx > 0)
                {
                    double epsVortex = 0.0;
                    double normVortex = 0.0;
                    for (int i = 0; i < result.Length; i++)
                    {
                        normVortex += result_old[i] * result_old[i];
                        epsVortex += (result_old[i] - result[i]) * (result_old[i] - result[i]);
                    }
                    double residual = Math.Sqrt(epsVortex / normVortex);
                    Console.WriteLine("n {0} точность {1}", idx, residual);
                    if (residual < MEM.Error3)
                        break;
                }
                MEM.Copy(ref result_old, result);
            }
        }

    }

}



