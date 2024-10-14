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
//                 кодировка : 09.04.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.FESimpleTask
{
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using System.Linq;

    /// <summary>
    /// ОО: Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
    /// </summary>
    [Serializable]
    public class RiverCrossVortexPhiTri : ATriFEMTask
    {
        /// <summary>
        /// Массив для отладки
        /// </summary>
        public double[] tmpRPhi;
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
        /// Задача для расчета функции вихря
        /// </summary>
        protected CTransportEquationsTri taskMu;
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
        /// функции вихревой вязкости на n-1 слое
        /// </summary>  
        public double[] eddyViscosity_old;
        /// <summary>
        /// функции скорости по руслу (+) на n-1 слое
        /// </summary>
        public double[] Ux_old;
        /// <summary>
        /// функции скорости по руслу (+) на n-1 слое
        /// </summary>
        public double[] Uy_old;
        /// <summary>
        /// функции скорости по руслу (+) на n-1 слое
        /// </summary>
        public double[] Uz_old;

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
        public RiverCrossVortexPhiTri(IMeshWrapperСhannelSectionCFG wMesh, IAlgebra algebra, TypeTask typeTask,
            double w = 0.3) :
            base(wMesh, algebra, typeTask)
        {

            taskPhi = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
            taskVortex = new CTransportEquationsTri(wMesh, algebra, typeTask);
            taskViscosity = new CTransportEquationsTri(wMesh, algebra, typeTask);
            taskUx = new CTransportEquationsTri(wMesh, algebra, typeTask);
            taskMu = new CTransportEquationsTri(wMesh, algebra, typeTask);
            this.wMesh = wMesh;
            this.w = w;

        }
        protected override void Init()
        {
            base.Init();
            CountKnots = (uint)mesh.CountKnots;
            MEM.Alloc(CountKnots, ref Phi);
            MEM.Alloc(CountKnots, ref Phi_old);
            MEM.Alloc(CountKnots, ref Vortex);
            MEM.Alloc(CountKnots, ref Vortex_old);
            MEM.Alloc(CountKnots, ref eddyViscosity_old);

            MEM.Alloc(CountKnots, ref Ux_old);
            MEM.Alloc(CountKnots, ref Uy_old);
            MEM.Alloc(CountKnots, ref Uz_old);
            MEM.Alloc(mesh.CountKnots, ref PhiMu, 1);

            /// адреса узлов на свободной поверхности потока и дне
            boundaryAdress = ((IMeshWrapperСhannelSectionCFG)wMesh).GetBoundaryAdress();
            boundaryBedAdress = ((IMeshWrapperСhannelSectionCFG)wMesh).GetBoundaryBedAdress();
            MEM.Alloc(boundaryBedAdress.Length, ref bcVx, 0);
            MEM.Alloc(boundaryAdress.Length, ref boundaryPhiValue, 0);
            MEM.Alloc(boundaryAdress.Length, ref boundaryVortexValue, 0);
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public RiverCrossVortexPhiTri(IMeshWrapperCrossCFG wMesh, IAlgebra algebra, TypeTask typeTask, double w = 0.05)
            : this((IMeshWrapperСhannelSectionCFG)wMesh, algebra, typeTask, w) { }

        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexPhi_BCVelocity(
            ref double[] _Phi,
            ref double[] _Vortex,
            ref double[] Vx,
            ref double[] Vy,
            ref double[] Vz,
            ref double[] eddyViscosity,
            ref double[] tauXY,
            ref double[] tauXZ,
            ref double[] tauYY,
            ref double[] tauYZ,
            ref double[] tauZZ,
            double[] velosityUx,
            double[] velosityUy,
            double[] Q,
            int VetrexTurbTask,
            bool flagUx,
            int VortexBC_G2, ECalkDynamicSpeed typeEddyViscosity)
        {
            Init();
            _Phi = Phi;
            _Vortex = Vortex;
            Vx = Ux;
            Vy = Uy;
            Vz = Uz;
            this.eddyViscosity = eddyViscosity;
            tauXY = TauXY;
            tauXZ = TauXZ;
            tauYY = TauYY;
            tauYZ = TauYZ;
            tauZZ = TauZZ;
            n = 0;
            IMeshWrapperСhannelSectionCFG wm = (IMeshWrapperСhannelSectionCFG)wMesh;
            double R_midle = wm.GetR_midle();
            int Ring = wm.GetRing();

            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx, 0);
            double J = Q[0] / (SPhysics.GRAV * SPhysics.rho_w);

            uint[] bAdress = null;
            if (flagUx == true)
            {
                wm.CalkBoundaryUx(velosityUx, ref bcVx);
                bAdress = boundaryAdress;
            }
            else
                bAdress = boundaryBedAdress;
            // цикл по нелинейности
            for (n = 0; n < 1500; n++)
            {
                //    MEM.MemCopy(ref Ux_old, Ux);
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                // расчет граничных условий для скорости Ux
                Console.WriteLine("Vx:");
                // релаксация потоковой скорости в створе
                taskUx.TransportEquationsTaskSUPG_R(ref Ux, eddyViscosity, R_midle, Ring, Uy, Uz, bAdress, bcVx, Q);
                //for (int i = 0; i < CountKnots; i++)
                //    Ux[i] = (1 - w) * Ux_old[i] + w * Ux[i];

                for (int i = 0; i < bcVx.Length; i++)
                    Console.Write(" {0} : {1:f4} ", bAdress[i], bcVx[i]);
                Console.WriteLine();

                //LOG.Print("bcVx", bcVx);
                //LOG.Print("bAdress", bAdress);

                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMeshWrapperCrossCFG)wMesh, typeEddyViscosity, Ux, J);
                // релаксация функции вязкости
                for (int i = 0; i < CountKnots; i++)
                    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];
                // расчет турбулентных напряжений для вихря
                if (VetrexTurbTask == 0)
                    Calk_TauYY_TauZZ_TauYZ();
                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex(Phi, Vortex, velosityUy, w, ref boundaryVortexValue, VortexBC_G2);
                for (int ii = 0; ii < boundaryVortexValue.Length; ii++)
                    boundaryVortexValue[ii] = 0;
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TaskSUPGTransportVortex_R(ref Vortex, eddyViscosity, R_midle, Ring, Ux, Uy, Uz, TauYY, TauYZ, TauZZ,
                    boundaryAdress, boundaryVortexValue, VetrexTurbTask);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                // расчет функции тока
                Console.WriteLine("Phi:");
                //taskPhi.PoissonTaskBack(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);
                taskPhi.PoissonTask(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);

                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz, R_midle, Ring);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsPhi = 0.0;
                normPhi = 0.0;
                epsVortex = 0.0;
                normVortex = 0.0;
                double[] Se = wm.GetElemS();
                for (int i = 0; i < CountKnots; i++)
                {
                    normPhi += Se[i] * Phi[i] * Phi[i];
                    normVortex += Se[i] * Vortex[i] * Vortex[i];
                    epsPhi += Se[i] * (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]);
                    epsVortex += Se[i] * (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                //residual = Math.Max(Math.Sqrt(epsPhi / normPhi), Math.Sqrt(epsVortex / normVortex));
                residual = Math.Sqrt(epsVortex / normVortex);
                Console.WriteLine("Шаг {0} точность {1} normPhi {2} normVortex {3}", n, residual, normPhi, normVortex);
                if (mesh.CountKnots < 10000)
                {
                    if (residual < 1e-4)
                        break;
                }
                else
                {
                    if (residual < 1e-5)
                        break;
                }
            }
            if (VetrexTurbTask == 1)
                Calk_TauYY_TauZZ_TauYZ();
            Calk_TauXY_TauXZ();
        }

        double[] F_Phi = null;
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexStream(
            ref double[] _Phi,
            ref double[] _Vortex,
            ref double[] Vx,
            ref double[] Vy,
            ref double[] Vz,
            ref double[] eddyViscosityUx,
            ref double[] eddyViscosity,
            ref double[] tauXY,
            ref double[] tauXZ,
            ref double[] tauYY,
            ref double[] tauYZ,
            ref double[] tauZZ,
            ref double[] RR,
            double[] velosityUx, uint[] mAdressU,
            double[] velosityUy,
            double[] Q,
            int VetrexTurbTask,
            bool flagUx,
            int VortexBC_G2,
            ECalkDynamicSpeed typeEddyViscosity, 
            bool flagLes, int idxMu2)
        {
            bool Local = true;
            Init();
            _Phi = Phi;
            _Vortex = Vortex;
            Vx = Ux;
            Vy = Uy;
            Vz = Uz;
            this.eddyViscosity = eddyViscosity;
            tauXY = TauXY;
            tauXZ = TauXZ;
            tauYY = TauYY;
            tauYZ = TauYZ;
            tauZZ = TauZZ;
            n = 0;
            IMeshWrapperСhannelSectionCFG wm = (IMeshWrapperСhannelSectionCFG)wMesh;
            double R_midle = wm.GetR_midle();
            int Ring = wm.GetRing();
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx, 0);
            MEM.Alloc(mesh.CountKnots, ref RR, 0);
            double J = Q[0] / (SPhysics.GRAV * SPhysics.rho_w);
            double[][] tDef = null;
            double[] E2 = null;
            uint[] bAdress = null;
            if (flagUx == true)
            {
                bcVx = velosityUx;
                bAdress = mAdressU;
            }
            else
                bAdress = boundaryBedAdress;

            double[] eddyViscosityUx_old = null;
            MEM.MemCopy(ref eddyViscosityUx, eddyViscosity);
            MEM.MemCopy(ref eddyViscosityUx_old, eddyViscosity);
            X = mesh.GetCoords(0);             
            for (int i = 0; i < mesh.CountKnots; i++)
                RR[i] = R_midle + X[i];

            // цикл по нелинейности
            for (n = 0; n < 1000; n++)
            {
                MEM.MemCopy(ref Ux_old, Ux);
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                MEM.MemCopy(ref eddyViscosityUx_old, eddyViscosityUx);

                // расчет граничных условий для скорости Ux
                Console.WriteLine("Vx:");
                // релаксация потоковой скорости в створе
                taskUx.TransportEquationsTaskSUPG_R(ref Ux, eddyViscosityUx, R_midle, Ring, Uy, Uz, bAdress, bcVx, Q);
                //for (int i = 0; i < bcVx.Length; i++)2
                //    Console.Write(" {0} : {1:f4} ", bAdress[i],bcVx[i]);
                //Console.WriteLine();
                #region Расчет вязкости

                if (SPhysics.PHYS.turbViscType == ETurbViscType.Les_Smagorinsky_Lilly_1996 ||
                    SPhysics.PHYS.turbViscType == ETurbViscType.Derek_G_Goring_and_K_1997 ||
                    SPhysics.PHYS.turbViscType == ETurbViscType.PotapobII_2024)
                {
                    Calk_tensor_deformations(ref tDef, ref E2);
                    SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, wm, typeEddyViscosity, E2, J);
                }
                else
                    SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, wm, typeEddyViscosity, Ux, J);

                if (flagLes == true)
                {
                    MEM.MemCopy(ref eddyViscosityUx, eddyViscosity);
                    ETurbViscType tmp = SPhysics.PHYS.turbViscType;
                    SPhysics.PHYS.turbViscType = (ETurbViscType)idxMu2;

                    if (SPhysics.PHYS.turbViscType == ETurbViscType.Les_Smagorinsky_Lilly_1996 ||
                        SPhysics.PHYS.turbViscType == ETurbViscType.Derek_G_Goring_and_K_1997 ||
                        SPhysics.PHYS.turbViscType == ETurbViscType.PotapobII_2024)
                    {
                        Calk_tensor_deformations(ref tDef, ref E2);
                        SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, wm, typeEddyViscosity, E2, J);
                    }
                    else
                        SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, wm, typeEddyViscosity, Ux, J);

                    SPhysics.PHYS.turbViscType = tmp;
                }
                else
                    MEM.MemCopy(ref eddyViscosityUx, eddyViscosity);

                //taskMu.TransportEquationsTaskSUPG_SpalartAllmaras(ref eddyViscosity, eddyViscosity, R_midle, Ring, Ux, Uy, Uz, boundaryBedAdress, bcVx);
                // релаксация функции вязкости
                if (flagLes == true || SPhysics.PHYS.turbViscType == ETurbViscType.Les_Smagorinsky_Lilly_1996)
                {
                    // накопление эволюционное
                    double dt = 0.5;
                    Console.WriteLine(" int Mu_t " + (1000 * eddyViscosity.Sum()/ eddyViscosity.Length).ToString());
                    for (int i = 0; i < CountKnots; i++)
                        eddyViscosity[i] = (1 - dt * w) * eddyViscosity_old[i] + w * eddyViscosity[i];

                    for (int i = 0; i < boundaryBedAdress.Length; i++)
                        eddyViscosity[boundaryBedAdress[i]] = SPhysics.mu;

                    Console.WriteLine(" int Mu_t " + (1000 * eddyViscosity.Sum() / eddyViscosity.Length).ToString());
                }
                else
                {
                    for (int i = 0; i < CountKnots; i++)
                        eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];
                }

                for (int i = 0; i < CountKnots; i++)
                    eddyViscosityUx[i] = (1 - w) * eddyViscosityUx_old[i] + w * eddyViscosityUx[i];
                #endregion
                // расчет турбулентных напряжений для вихря
                if (VetrexTurbTask == 0)
                    Calk_TauYY_TauZZ_TauYZ();
                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex(Phi, Vortex, velosityUy, w, ref boundaryVortexValue, VortexBC_G2);

                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TaskSUPGTransportVortex_R(ref Vortex, eddyViscosity, R_midle, Ring, Ux, Uy, Uz, TauYY, TauYZ, TauZZ,
                    boundaryAdress, boundaryVortexValue, VetrexTurbTask);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                // расчет функции тока
                Console.WriteLine("Phi:");
                //taskPhi.PoissonTaskBack(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);
                //taskPhi.PoissonTask0(ref Phi, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);
                taskPhi.PoissonTaskCircle(ref Phi, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);
                tmpRPhi = taskPhi.tmp;
                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz, R_midle, Ring, Local);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsPhi = 0.0;
                normPhi = 0.0;
                epsVortex = 0.0;
                normVortex = 0.0;
                double[] Se = wm.GetElemS();
                for (int i = 0; i < CountKnots; i++)
                {
                    normPhi += Se[i] * Phi[i] * Phi[i];
                    normVortex += Se[i] * Vortex[i] * Vortex[i];
                    epsPhi += Se[i] * (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]);
                    epsVortex += Se[i] * (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                //residual = Math.Max(Math.Sqrt(epsPhi / normPhi), Math.Sqrt(epsVortex / normVortex));
                residual = Math.Sqrt(epsVortex / normVortex);
                Console.WriteLine("Шаг {0} точность {1} normPhi {2} normVortex {3}", n, residual, normPhi, normVortex);
                if (mesh.CountKnots < 10000)
                {
                    if (residual < 1e-4)
                        break;
                }
                else
                {
                    if (residual < 1e-5) 
                        break;
                }
            }
            if (VetrexTurbTask == 1)
                Calk_TauYY_TauZZ_TauYZ();
            Calk_TauXY_TauXZ();
        }


        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexStreamTub(
            ref double[] _Phi,
            ref double[] _Vortex,
            ref double[] Vx,
            ref double[] Vy,
            ref double[] Vz,
            ref double[] eddyViscosity,
            ref double[] tauXY,
            ref double[] tauXZ,
            ref double[] tauYY,
            ref double[] tauYZ,
            ref double[] tauZZ,
            double[] velosityUx, uint[] mAdressU,
            double[] velosityUy,
            double[] Q,
            int VetrexTurbTask,
            bool flagUx,
            int VortexBC_G2, ECalkDynamicSpeed typeEddyViscosity)
        {
            Init();
            _Phi = Phi;
            _Vortex = Vortex;
            Vx = Ux;
            Vy = Uy;
            Vz = Uz;
            this.eddyViscosity = eddyViscosity;
            tauXY = TauXY;
            tauXZ = TauXZ;
            tauYY = TauYY;
            tauYZ = TauYZ;
            tauZZ = TauZZ;
            n = 0;
            IMeshWrapperСhannelSectionCFG wm = (IMeshWrapperСhannelSectionCFG)wMesh;
            double R_midle = wm.GetR_midle();
            int Ring = wm.GetRing();
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx, 0);
            double J = Q[0] / (SPhysics.GRAV * SPhysics.rho_w);

            uint[] bAdress = null;
            if (flagUx == true)
            {
                bcVx = velosityUx;
                bAdress = mAdressU;
            }
            else
                bAdress = boundaryBedAdress;
            // цикл по нелинейности
            for (n = 0; n < 1000; n++)
            {
                //    MEM.MemCopy(ref Ux_old, Ux);
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                // расчет граничных условий для скорости Ux
                Console.WriteLine("Vx:");
                // релаксация потоковой скорости в створе
                taskUx.TransportEquationsTaskSUPG_R(ref Ux, eddyViscosity, R_midle, Ring, Uy, Uz, bAdress, bcVx, Q);
                //for (int i = 0; i < bcVx.Length; i++)
                //    Console.Write(" {0} : {1:f4} ", bAdress[i],bcVx[i]);
                //Console.WriteLine();
                //J = 0.001;
                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMeshWrapperCrossCFG)wMesh, typeEddyViscosity, Ux, J);
                taskMu.TransportEquationsTaskSUPG_R(ref eddyViscosity, eddyViscosity, R_midle, Ring, Uy, Uz, bAdress, bcVx, Q);
                // релаксация функции вязкости
                for (int i = 0; i < CountKnots; i++)
                    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];
                // расчет турбулентных напряжений для вихря
                if (VetrexTurbTask == 0)
                    Calk_TauYY_TauZZ_TauYZ();
                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex(Phi, Vortex, velosityUy, w, ref boundaryVortexValue, VortexBC_G2);

                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TaskSUPGTransportVortex_R(ref Vortex, eddyViscosity, R_midle, Ring, Ux, Uy, Uz, TauYY, TauYZ, TauZZ,
                    boundaryAdress, boundaryVortexValue, VetrexTurbTask);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                // расчет функции тока
                Console.WriteLine("Phi:");
                //taskPhi.PoissonTaskBack(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);
                taskPhi.PoissonTask(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex, R_midle, Ring);

                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz, R_midle, Ring);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsPhi = 0.0;
                normPhi = 0.0;
                epsVortex = 0.0;
                normVortex = 0.0;
                double[] Se = wm.GetElemS();
                for (int i = 0; i < CountKnots; i++)
                {
                    normPhi += Se[i] * Phi[i] * Phi[i];
                    normVortex += Se[i] * Vortex[i] * Vortex[i];
                    epsPhi += Se[i] * (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]);
                    epsVortex += Se[i] * (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                //residual = Math.Max(Math.Sqrt(epsPhi / normPhi), Math.Sqrt(epsVortex / normVortex));
                residual = Math.Sqrt(epsVortex / normVortex);
                Console.WriteLine("Шаг {0} точность {1} normPhi {2} normVortex {3}", n, residual, normPhi, normVortex);
                if (mesh.CountKnots < 10000)
                {
                    if (residual < 1e-4)
                        break;
                }
                else
                {
                    if (residual < 1e-5)
                        break;
                }
            }
            if (VetrexTurbTask == 1)
                Calk_TauYY_TauZZ_TauYZ();
            Calk_TauXY_TauXZ();
        }

        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexPhi_Plane(
            ref double[] _Phi,
            ref double[] _Vortex,
            ref double[] Vx,
            ref double[] Vy,
            ref double[] Vz,
            ref double[] eddyViscosity,
            ref double[] tauXY,
            ref double[] tauXZ,
            ref double[] tauYY,
            ref double[] tauYZ,
            ref double[] tauZZ,
            double[] velosityUy,
            double J,
            int VetrexTurbTask, ECalkDynamicSpeed typeEddyViscosity)
        {
            Init();
            _Phi = Phi;
            _Vortex = Vortex;
            Vx = Ux;
            Vy = Uy;
            Vz = Uz;
            this.eddyViscosity = eddyViscosity;
            tauXY = TauXY;
            tauXZ = TauXZ;
            tauYY = TauYY;
            tauYZ = TauYZ;
            tauZZ = TauZZ;

            n = 0;
            IMeshWrapperСhannelSectionCFG wm = (IMeshWrapperСhannelSectionCFG)wMesh;
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx, 0);

            // цикл по нелинейности
            for (n = 0; n < 1500; n++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);

                Console.WriteLine("Vx:");
                // расчет потоковой скорости в створе
                taskUx.TransportEquationsTaskSUPG(ref Ux, eddyViscosity, Uy, Uz, boundaryBedAdress, J);
                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMeshWrapperCrossCFG)wMesh, typeEddyViscosity, Ux, J);
                // расчет турбулентных напряжений для вихря
                if (VetrexTurbTask == 0)
                    Calk_TauYY_TauZZ_TauYZ();
                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex_Plane(Phi, Vortex, velosityUy, w, ref boundaryVortexValue);
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TaskSUPGTransportVortex_Plane(ref Vortex, eddyViscosity, Ux, Uy, Uz, 
                    TauYY, TauYZ, TauZZ, boundaryAdress, boundaryVortexValue, VetrexTurbTask);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask_Plane(ref Phi, PhiMu, boundaryAdress, boundaryPhiValue, Vortex);
                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity_Plane(Phi, ref Vy, ref Vz);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsPhi = 0.0;
                normPhi = 0.0;
                epsVortex = 0.0;
                normVortex = 0.0;
                double[] Se = wm.GetElemS();
                for (int i = 0; i < CountKnots; i++)
                {
                    normPhi += Se[i] * Phi[i] * Phi[i];
                    normVortex += Se[i] * Vortex[i] * Vortex[i];
                    epsPhi += Se[i] * (Phi[i] - Phi_old[i]) * (Phi[i] - Phi_old[i]);
                    epsVortex += Se[i] * (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                //residual = Math.Max(Math.Sqrt(epsPhi / normPhi), Math.Sqrt(epsVortex / normVortex));
                residual = Math.Sqrt(epsVortex / normVortex);
                Console.WriteLine("Шаг {0} точность {1} normPhi {2} normVortex {3}", n, residual, normPhi, normVortex);
                if (mesh.CountKnots < 10000)
                    if (residual < 1e-4) break;
                    else
                    if (residual < 1e-5) break;
            }
            if (VetrexTurbTask == 1)
                Calk_TauYY_TauZZ_TauYZ();
            Calk_TauXY_TauXZ();
        }
    }
}
