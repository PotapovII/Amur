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
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 06.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using System;

    using MemLogLib;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    /// <summary>
    /// ОО: Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
    /// </summary>
    [Serializable]
    public class RiverSectionVortexPhiTri : AComplexFEMTask
    {
        /// <summary>
        /// Коэффициент релаксации.
        /// </summary>
        protected double w;
        /// <summary>
        /// Количество узлов в сетке
        /// </summary>
        protected uint CountKnots;
        /// <summary>
        /// Задача для расчета функции тока
        /// </summary>
        protected CFEPoissonTaskTri taskPhi;
        /// <summary>
        /// Задача для расчета функции вихря
        /// </summary>
        protected CTransportEquationsTri taskVortex;
        /// <summary>
        /// Задача для расчета функции вязкости
        /// </summary>
        protected CTransportEquationsTri taskViscosity;
        /// <summary>
        /// Задача для расчета русловой скорости
        /// </summary>
        protected CTransportEquationsTri taskUx;
        /// <summary>
        /// условная вязкость для задачи Пуассона
        /// </summary>
        public double[] PhiMu;
        /// <summary>
        /// функции тока
        /// </summary>
        public double[] Phi;
        /// <summary>
        /// функции тока на n-1 слое
        /// </summary>
        protected double[] Phi_old;
        /// <summary>
        /// функции вихря
        /// </summary>
        public double[] Vortex;
        /// <summary>
        /// функции вихря на n-1 слое
        /// </summary>
        protected double[] Vortex_old;
        /// <summary>
        /// нелинейная правая часть для вихря
        /// </summary>
        public double[] VortexQ;
        /// <summary>
        /// функции вихря
        /// </summary>
        public double[] eddyViscosity_old;
        public double[] eVQ;
        /// <summary>
        /// функции скорости по руслу (+)
        /// </summary>
        public double[] Vx;
        /// <summary>
        /// функции скорости по руслу (+) на n-1 слое
        /// </summary>
        public double[] Vx_old;
        /// <summary>
        /// функции скорости ->
        /// </summary>
        public double[] Vy;
        /// <summary>
        /// функции скорости по руслу (+) на n-1 слое
        /// </summary>
        public double[] Vy_old;
        /// <summary>
        /// функции скорости ^
        /// </summary>
        public double[] Vz;
        #region Массивы для определения функции вихря на дне потока
        /// <summary>
        /// адреса узлов на свободной поверхности потока и дне
        /// </summary>
        protected uint[] boundaryAdress = null;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] boundaryBedAdress = null;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] boundaryWaterLevelAdress = null;
        /// <summary>
        /// значения вихря на дне 
        /// </summary>
        protected double[] boundaryBedVortexValue;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected double[] boundaryWaterLevelValue = null;
        /// <summary>
        /// Значение Phi на границе
        /// </summary>
        protected double[] boundaryPhiValue = null;
        /// <summary>
        /// Значение Vortex на границе
        /// </summary>
        protected double[] boundaryVortexValue = null;
        /// <summary>
        /// Массив значений в узлах для задачи функции тока
        /// </summary>
        protected double[] bcVx = null;
        #endregion
        protected double epsPhi;
        protected double epsVortex;
        protected double normPhi;
        protected double normVortex;
        protected double residual;
        int n = 0;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public RiverSectionVortexPhiTri(IMWDistance wMesh, IAlgebra algebra, TypeTask typeTask, double w = 0.3) : base(wMesh, algebra, typeTask)
        {
            
            taskPhi = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
            taskVortex = new CTransportEquationsTri(wMesh, algebra, typeTask);
            taskViscosity = new CTransportEquationsTri(wMesh, algebra, typeTask);
            taskUx = new CTransportEquationsTri(wMesh, algebra, typeTask);
            this.wMesh = wMesh;
            this.w = w;
            CountKnots = (uint)mesh.CountKnots;
            MEM.Alloc(CountKnots, ref Phi);
            MEM.Alloc(CountKnots, ref Phi_old);
            MEM.Alloc(CountKnots, ref Vortex);
            MEM.Alloc(CountKnots, ref VortexQ);
            MEM.Alloc(CountKnots, ref Vortex_old);
            MEM.Alloc(CountKnots, ref eddyViscosity_old);
            MEM.Alloc(CountKnots, ref Vx);
            MEM.Alloc(CountKnots, ref Vx_old);
            MEM.Alloc(CountKnots, ref Vy);
            MEM.Alloc(CountKnots, ref Vy_old);
            MEM.Alloc(CountKnots, ref Vz);
            MEM.Alloc(CountKnots, ref eVQ);
            MEM.Alloc(mesh.CountKnots, ref PhiMu, 1);

            /// адреса узлов на свободной поверхности потока и дне
            boundaryAdress = ((IMWCrossSection)wMesh).GetBoundaryAdress();
            boundaryBedAdress = ((IMWCrossSection)wMesh).GetBoundaryBedAdress();
            MEM.Alloc(boundaryBedAdress.Length, ref bcVx, 0);
            MEM.Alloc(boundaryAdress.Length, ref boundaryPhiValue, 0);
            MEM.Alloc(boundaryAdress.Length, ref boundaryVortexValue, 0);
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public RiverSectionVortexPhiTri(IMWCross wMesh, IAlgebra algebra, TypeTask typeTask, double w = 0.3) 
            : this((IMWDistance)wMesh, algebra, typeTask) { }
        
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexPhi_BCVelocity(ref double[] Phi, ref double[] Vortex,
            ref double[] Vx, ref double[] Vy, ref double[] Vz, ref double[] eddyViscosity,
            ref double[] tauY, ref double[] tauZ, double[] Q, double[] wlVelosity, int VortexBC_G2, int VetrexTurbTask,
            ECalkDynamicSpeed typeEddyViscosity)
        {
            n = 0;
            IMWCrossSection wm = (IMWCrossSection)wMesh;
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx, 0);
            double J = Q[0] / (SPhysics.GRAV * SPhysics.rho_w);
            // цикл по нелинейности
            for (n = 0; n < 100 && residual > 1e-4; n++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                MEM.MemCopy(ref Vx_old, Vx);
                // расчет потоковой скорости в створе
                Console.WriteLine("Vx:");
                taskUx.TransportEquationsTaskSUPG(ref Vx, eddyViscosity, Vy, Vz, boundaryBedAdress, bcVx, Q);
                // релаксация потоковой скорости в створе
                for (int i = 0; i < CountKnots; i++)
                    Vx[i] = (1 - w) * Vx_old[i] + w * Vx[i];
                // расчет поля вязкости
                //taskViscosity.TransportEquationsTaskSUPG(ref eddyViscosity, eddyViscosity_old, Vy, Vz, bc_Phi, bv_Phi, eVQ);
                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWRiver)wMesh, typeEddyViscosity,  Vx, J);
                // релаксация функции вязкости
                for (int i = 0; i < CountKnots; i++)
                    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];
                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex(Phi, Vortex, wlVelosity, w, ref boundaryVortexValue, VortexBC_G2);
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz, boundaryAdress, boundaryVortexValue, VortexQ);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex);
                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz);

                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsPhi = 0.0;
                normPhi = 0.0;
                epsVortex = 0.0;
                normVortex = 0.0;
                for (int i = 0; i < CountKnots; i++)
                {
                    normPhi += Phi[i] * Phi[i];
                    normVortex += Vortex[i] * Vortex[i];
                    epsPhi += (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]);
                    epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                residual = Math.Max(Math.Sqrt(epsPhi / normPhi), Math.Sqrt(epsVortex / normVortex));
                Console.WriteLine("Шаг {0} точность {1}", n, residual);
            }
            taskUx.CalkTauInterpolation(ref tauY, ref tauZ, Vx, eddyViscosity);
        }
    }
}
