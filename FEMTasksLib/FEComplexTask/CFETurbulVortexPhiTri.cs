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
    using MeshLib;
    using MemLogLib;

    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Geometry;
    using GeometryLib.Locators;
    
    using System;
    using System.Collections.Generic;
    using CommonLib.Physics;

    /// <summary>
    /// ОО: Задача с вынужденной конвекцией, на верхней крышке области заданн
    /// 1. Вихрь
    /// 2. Скорость
    /// </summary>
    [Serializable]
    public class CFETurbulVortexPhiTri : AComplexFEMTask
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
        /// <summary>
        /// Скорости на КЭ
        /// </summary>
        public double[] eVy, eVz, bVortex;

        #region Массивы для определения функции вихря на дне потока
        /// <summary>
        /// Элемент для расчета
        /// </summary>
        private uint[] bedElems = null;
        /// <summary>
        /// Придонные координаты точки для  расчета
        /// </summary>
        private HPoint[] bedPoint = null;
        private double[] pLenth = null;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] bcBedAdress = null;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] bcWaterLevelAdress = null;
        /// <summary>
        /// значения вихря на дне 
        /// </summary>
        protected double[] bcBedVortexValue;
        /// <summary>
        /// Массив граничных узлов для задачи вихря
        /// </summary>
        protected uint[] bcVortexAdress = null;
        /// <summary>
        /// Массив значений в узлах для задачи вихря
        /// </summary>
        protected double[] bcVortexValue = null;
        /// <summary>
        /// Массив граничных узлов для задачи  функции тока
        /// </summary>
        protected uint[] bcPhiAdress = null;
        /// <summary>
        /// Массив значений в узлах для задачи функции тока
        /// </summary>
        protected double[] bcPhiValue = null;
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
        public CFETurbulVortexPhiTri(IMWDistance wMesh, IAlgebra algebra, TypeTask typeTask, double w = 0.3) : base(wMesh, algebra, typeTask)
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
            MEM.Alloc(CountKnots, ref bVortex);
            MEM.Alloc(mesh.CountElements, ref eVy);
            MEM.Alloc(mesh.CountElements, ref eVz);
            MEM.Alloc(CountKnots, ref eVQ);
            MEM.Alloc(mesh.CountKnots, ref PhiMu, 1);
            CalkBCArray();
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="algebra">линейный решатель</param>
        public CFETurbulVortexPhiTri(IMWCross wMesh, IAlgebra algebra, TypeTask typeTask, double w = 0.3) 
            : this((IMWDistance)wMesh, algebra, typeTask) { }
        /// <summary>
        /// Расчет хеша для граничных условий 
        /// </summary>
        public void CalkBCArray()
        {
            try
            {
                int[] bknots = mesh.GetBoundKnots();
                int[] Marks = mesh.GetBoundKnotsMark();
                TwoElement[] bElems = mesh.GetBoundElems();
                int[] elemMarks = mesh.GetBElementsBCMark();
                List<uint> idx_Bed = new List<uint>();
                List<uint> idx_Top = new List<uint>();
                List<uint> idx_BC = new List<uint>();

                for (int i = 0; i < bElems.Length; i++)
                {
                    if (elemMarks[i] == 0) // Дно
                    {
                        TwoElement be = bElems[i];
                        if (idx_Bed.Contains(be.Vertex1) == false)
                            idx_Bed.Add(be.Vertex1);
                        if (idx_Bed.Contains(be.Vertex2) == false)
                            idx_Bed.Add(be.Vertex2);
                    }
                }
                int CountBed = idx_Bed.Count;
                bcBedAdress = idx_Bed.ToArray();
                for (int i = 0; i < bElems.Length; i++)
                {
                    if (elemMarks[i] == 2) // крышка
                    {
                        TwoElement be = bElems[i];
                        if (idx_Bed.Contains(be.Vertex1) == false)
                            idx_Bed.Add(be.Vertex1);
                        if (idx_Bed.Contains(be.Vertex2) == false)
                            idx_Bed.Add(be.Vertex2);
                        if (idx_Top.Contains(be.Vertex1) == false)
                            idx_Top.Add(be.Vertex1);
                        if (idx_Top.Contains(be.Vertex2) == false)
                            idx_Top.Add(be.Vertex2);
                    }
                }
                bcWaterLevelAdress = idx_Top.ToArray();
                idx_BC.AddRange(idx_Bed);
                idx_BC.AddRange(idx_Top);
                if (typeTask == TypeTask.streamX1D)
                {
                    bcVortexAdress = idx_BC.ToArray();
                    MEM.Alloc(bcVortexAdress.Length, ref bcBedVortexValue, 0);
                }
                uint shift = 0;
                if (typeTask == TypeTask.streamY1D)
                {
                    shift = 1;
                    MEM.Alloc(bcBedAdress.Length, ref bcVx, 0);
                    MEM.Alloc(idx_BC.Count, ref bcPhiAdress);
                    MEM.Alloc(idx_BC.Count, ref bcPhiValue, 0);
                    for (int i = 0; i < idx_BC.Count; i++)
                        bcPhiAdress[i] = idx_BC[i];
                }
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                MEM.Alloc(CountBed, ref bedElems, "bedElems");
                MEM.Alloc(CountBed, ref bedPoint, "bedPoint");
                MEM.Alloc(CountBed, ref pLenth, "pLenth");
                int BCount = CountBed - 1;
                for (uint nod = 1; nod < BCount; nod++)
                {
                    HPoint pe = new HPoint(X[bcBedAdress[nod + 1]], Y[bcBedAdress[nod + 1]]);
                    HPoint pw = new HPoint(X[bcBedAdress[nod - 1]], Y[bcBedAdress[nod - 1]]);
                    HPoint p = new HPoint(X[bcBedAdress[nod]], Y[bcBedAdress[nod]]);
                    HPoint tau = pe - pw;
                    double Length2 = tau.Length() / 4;
                    pLenth[nod] = Length2;
                    HPoint P = p + tau.GetOrtogonalLeft() * Length2;
                    bedPoint[nod] = P;
                }
                if (typeTask == TypeTask.streamX1D)
                {
                    {
                        // 0
                        HPoint pe = new HPoint(X[bcBedAdress[1]], Y[bcBedAdress[1]]);
                        HPoint p = new HPoint(X[bcBedAdress[0]], Y[bcBedAdress[0]]);
                        HPoint tau = pe - p;
                        double Length2 = tau.Length();
                        pLenth[0] = Length2;
                        HPoint P = p + tau.GetOrtogonalLeft() * Length2;
                        bedPoint[0] = P;
                    }
                    {
                        // BCount
                        HPoint pw = new HPoint(X[bcBedAdress[BCount - 1]], Y[bcBedAdress[BCount - 1]]);
                        HPoint p = new HPoint(X[bcBedAdress[BCount]], Y[bcBedAdress[BCount]]);
                        HPoint tau = p - pw;
                        double Length2 = tau.Length();
                        pLenth[BCount] = Length2;
                        HPoint P = p + tau.GetOrtogonalLeft() * Length2;
                        bedPoint[BCount] = P;
                    }
                }
                // поиск трехугольного элемента для аппроксимации
                TriMeshLocators triMeshLocators = new TriMeshLocators(mesh);
                HFForm2D_TriangleAnaliticL1 triff = new MeshLib.HFForm2D_TriangleAnaliticL1();
                for (uint p = shift; p < CountBed - shift; p++)
                {
                    int idx = triMeshLocators.QueryElement(bedPoint[p]);
                    if (idx > -1)
                        bedElems[p] = (uint)idx;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданн вихрь
        /// TypeTask.streamY1D
        /// </summary>
        public virtual void CalkVortexPhi_BCVortex(ref double[] Phi, ref double[] Vortex, ref double[] mVx, 
                                   ref double[] Vy, ref double[] Vz, ref double[] tauY, ref double[] tauZ, 
                        ref double[] eddyViscosity, double[] Q, uint[] bcVortexAdress, double[] bcVortexValue,
                        ECalkDynamicSpeed typeEddyViscosity)
        {
            this.bcVortexAdress = bcVortexAdress;
            this.bcVortexValue = bcVortexValue;
            n = 0;
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref mVx, 0);
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
                taskUx.TransportEquationsTaskSUPG(ref Vx, eddyViscosity, Vy, Vz, bcBedAdress, bcVx, Q);
                // релаксация потоковой скорости в створе
                for (int i = 0; i < CountKnots; i++)
                {
                    Vx[i] = (1 - w) * Vx_old[i] + w * Vx[i];
                    mVx[i] = Vx[i];
                }
                // расчет поля вязкости
                //taskViscosity.TransportEquationsTaskSUPG(ref eddyViscosity, eddyViscosity_old, Vy, Vz, bc_Phi, bv_Phi, eVQ);
                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWRiver)wMesh, typeEddyViscosity, Vx, J);
                // релаксация функции вязкости
                for (int i = 0; i < CountKnots; i++)
                    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];
                // расчет нелинейной части для вихря
                taskVortex.Calk_Q_forVortex(eddyViscosity, Vortex, ref VortexQ);
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz, bcVortexAdress, bcVortexValue, VortexQ);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask(ref Phi, PhiMu, bcPhiAdress, bcPhiValue, Vortex);
                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz);
                // расчет граничных условий на дне для вихря
                CalkBCVortex(Phi, Vortex, w, bcBedAdress, ref bcBedVortexValue);
                for (int i = 0; i < bcBedAdress.Length; i++)
                {
                    for(int j = 0; j < bcVortexAdress.Length; j++)
                    {
                        if(bcVortexAdress[j] == bcBedAdress[i])
                        {
                            bcVortexValue[j] = bcBedVortexValue[i];
                            break;
                        }    
                    }
                }
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
                residual = Math.Max( Math.Sqrt(epsPhi / normPhi) , Math.Sqrt(epsVortex / normVortex) );
                Console.WriteLine("Шаг {0} точность {1}", n, residual);
            }
            taskUx.CalkTauInterpolation(ref tauY, ref tauZ, mVx, eddyViscosity);
        }

        #region Тесты 1
        /// <summary>
        /// Задача с продольном потоке с крыкой скольжения, на входе котороо заданна функция тока 
        /// TypeTask.streamX1D
        /// Тест 1
        /// </summary>
        public virtual void CalkVortexPhi_StreamBCPhi(ref double[] Phi, ref double[] Vortex,  
                              ref double[] Vx, ref double[] Vy,  ref double[] eddyViscosity, 
                              uint[] bcPhiAdress, double[] bcPhiValue, ECalkDynamicSpeed typeEddyViscosity)
        {
            n = 0;
            this.bcPhiAdress = bcPhiAdress;
            this.bcPhiValue = bcPhiValue;

            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx);
            MEM.Alloc(mesh.CountKnots, ref Vy);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            MEM.Alloc(mesh.CountKnots, ref Vortex);
            // цикл по нелинейности
            for (n = 0; n < 1000 && residual > 1e-5; n++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                MEM.MemCopy(ref Vx_old, Vx);
                MEM.MemCopy(ref Vy_old, Vy);
                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask(ref Phi, PhiMu, bcPhiAdress, bcPhiValue, Vortex);
                // релаксация функции тока
                if(n== 0)
                    for (int i = 0; i < CountKnots; i++)
                        Phi_old[i] = Phi[i];
                else
                    for (int i = 0; i < CountKnots; i++)
                        Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];

                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vx, ref Vy);
                // расчет поля вязкости
                // SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWCrossSection)wMesh, typeEddyViscosity, Vx);
                //for (int i = 0; i < CountKnots; i++)
                //    eddyViscosity[i] = eddyViscosity[i]/ SPhysics.rho_w;
                // релаксация функции вязкости
                //for (int i = 0; i < CountKnots; i++)
                //    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];

                // расчет граничных условий на дне для вихря
                CalkBCVortex(Phi, Vortex, w);
                // расчет нелинейной части для вихря
                //taskVortex.Calk_Q_forVortex(eddyViscosity, Vortex, ref VortexQ);
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz, bcVortexAdress, bcBedVortexValue, VortexQ);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
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
                residual = Math.Max(Math.Sqrt(epsPhi /(normPhi + MEM.Error12)), Math.Sqrt(epsVortex / (normVortex + MEM.Error12)));
                Console.WriteLine("Шаг {0} точность {1}", n, residual);
            }
        }

        /// <summary>
        /// Задача с продольном потоке с крыкой скольжения, на входе котороо заданна функция тока 
        /// алгоритм из книги Маза А.
        /// TypeTask.streamX1D
        /// Тест 2
        /// </summary>
        public virtual void CalkVortexPhi_Mazo(ref double[] Phi, ref double[] Vortex,
                              ref double[] Vx, ref double[] Vy, ref double[] eddyViscosity,
                              uint[] bcPhiAdress, double[] bcPhiValue, uint[] bcAdress_WL, uint[] bcAdress_WaLL,
                              uint[] bcAdress_IN, uint[] bcAdress_OUT,  double dt)
        {
            n = 0;
            this.bcPhiAdress = bcPhiAdress;
            this.bcPhiValue = bcPhiValue;

            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx);
            MEM.Alloc(mesh.CountKnots, ref Vy);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            MEM.Alloc(mesh.CountKnots, ref Vortex);
            // цикл по нелинейности
            for (n = 0; n < 100 && residual > 1e-5; n++)
            {
                MEM.MemCopy(ref Vortex_old, Vortex);
                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask(ref Phi, PhiMu, bcPhiAdress, bcPhiValue, Vortex);
                // расчет поля скорости на КЭ
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(ref eVy, ref eVz, Phi);
                // Расчет функции вихря в узлах области
                taskPhi.CalcBoudaryVortex(ref bVortex, Phi);
                // расчет граничных условий для вихря
                MEM.Alloc(bcPhiAdress.Length, ref bcVortexValue);
                // расчет граничных условий на дне для вихря
                for (int i = 0; i < bcAdress_WL.Length; i++)
                    bVortex[bcAdress_WL[i]] = 0;
                for (int i = 0; i < bcAdress_IN.Length; i++)
                {
                    uint idxIN = bcAdress_IN[i];
                    double z = Y[idxIN];
                    double omega = -2 + 2 * z;
                    bVortex[idxIN] = omega;
                }
                for (int i = 0; i < bcAdress_OUT.Length; i++)
                {
                    uint idxOUT = bcAdress_OUT[i];
                    double z = Y[idxOUT];
                    double omega = -2 + 2 * z;
                    bVortex[idxOUT] = omega;
                }

                for (int k = 0, i = 0; i < bcPhiAdress.Length; i++)
                    bcVortexValue[k++] = bVortex[bcPhiAdress[i]];
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskPhi.CalcVortex(ref Vortex, Vortex_old, eddyViscosity, eVy, eVz, bcPhiAdress, bcVortexValue, dt);

                //taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz, bcPhiAdress, bcVortexValue, VortexQ);
                // релаксация вихря
                //for (int i = 0; i < CountKnots; i++)
                //    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsVortex = 0.0;
                normVortex = 0.0;
                for (int i = 0; i < CountKnots; i++)
                {
                    normVortex += Vortex[i] * Vortex[i];
                    epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                residual =  Math.Sqrt(epsVortex / (normVortex + MEM.Error12));
                Console.WriteLine("Шаг {0} точность {1}", n, residual);
            }
            taskPhi.CalcVelosity(Phi, ref Vx, ref Vy);
        }

        /// <summary>
        /// Задача о течении в каверне с подвижной крыкой
        /// TypeTask.streamY1D
        /// Тест 3
        /// </summary>
        public virtual void CalkVortexPhiCavern(ref double[] Phi, ref double[] Vortex,
                              ref double[] Vx, ref double[] Vy, ref double[] eddyViscosity, double[] velosityUy,
                              uint[] bcPhiAdress, double[] bcPhiValue, uint[] bcAdress_WL, uint[] bcAdress_WaLL,
                              uint[] bcAdress_IN, uint[] bcAdress_OUT, double dt, bool velLocal, bool stoks, int BCIndex = 2)
        {
            n = 0;
            int Ring = 0;
            double R_midle = 0;
            this.bcPhiAdress = bcPhiAdress;
            this.bcPhiValue = bcPhiValue;
            uint[] boundaryAdress = ((IMWCrossSection)wMesh).GetBoundaryAdress();
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx);
            MEM.Alloc(mesh.CountKnots, ref Vy);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            MEM.Alloc(mesh.CountKnots, ref Vortex);

            for (int i = 0; i < bcPhiValue.Length; i++)
                bcPhiValue[i] = 0;

            IMWCrossSection wm = (IMWCrossSection)wMesh;
            // цикл по нелинейности
            for (n = 0; n < 1000 && residual > 1e-5; n++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                MEM.MemCopy(ref Vx_old, Vx);
                MEM.MemCopy(ref Vy_old, Vy);
                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex(Phi, Vortex, velosityUy, w, ref bcVortexValue, BCIndex);
                // расчет вихря

                Console.WriteLine("Vortex:");
                if(stoks == true) // только уравнение пуассона для расчета вихря
                    taskPhi.PoissonTask(ref Vortex, PhiMu, boundaryAdress, bcVortexValue, VortexQ);
                else
                    taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz,
                                            boundaryAdress, bcVortexValue, VortexQ);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask(ref Phi, PhiMu, bcPhiAdress, bcPhiValue, Vortex);
                // релаксация функции тока
                //if (n == 0)
                //    for (int i = 0; i < CountKnots; i++)
                //        Phi_old[i] = Phi[i];
                //else
                for (int i = 0; i < CountKnots; i++)
                     Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];

                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vx, ref Vy, R_midle, Ring, velLocal);
                //************************************************************************************************************************
                // Считаем невязку
                //************************************************************************************************************************
                epsVortex = 0.0;
                normVortex = 0.0;
                for (int i = 0; i < CountKnots; i++)
                {
                    normVortex += Vortex[i] * Vortex[i];
                    epsVortex += (Vortex[i] - Vortex_old[i]) * (Vortex[i] - Vortex_old[i]);
                }
                residual = Math.Sqrt(epsVortex / (normVortex + MEM.Error12));
                Console.WriteLine("Шаг {0} точность {1}", n, residual);
            }
        }


        /// <summary>
        /// Задача о течении в каверне с подвижной крыкой
        /// TypeTask.streamY1D
        /// Тест 3
        /// </summary>
        public virtual void CalkVortexPhiCavern_Mazo(ref double[] Phi, ref double[] Vortex,
                              ref double[] Vx, ref double[] Vy, ref double[] eddyViscosity, double[] velosityUy,
                              uint[] bcPhiAdress, double[] bcPhiValue, uint[] bcAdress_WL, uint[] bcAdress_WaLL,
                              uint[] bcAdress_IN, uint[] bcAdress_OUT, double dt, int BCIndex = 0)
        {
            n = 0;
            this.bcPhiAdress = bcPhiAdress;
            this.bcPhiValue = bcPhiValue;

            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Vx);
            MEM.Alloc(mesh.CountKnots, ref Vy);
            MEM.Alloc(mesh.CountKnots, ref Phi);
            MEM.Alloc(mesh.CountKnots, ref Vortex);

            for (int i = 0; i < bcPhiValue.Length; i++)
                bcPhiValue[i] = 0;

            IMWCrossSection wm = (IMWCrossSection)wMesh;
            // цикл по нелинейности
            for (n = 0; n < 1000 && residual > 1e-5; n++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                MEM.MemCopy(ref Vx_old, Vx);
                MEM.MemCopy(ref Vy_old, Vy);


                // расчет граничных условий на дне для вихря
                wm.CalkBoundaryVortex(Phi, Vortex, velosityUy, w, ref bcVortexValue, BCIndex);

                // расчет граничных условий на дне для вихря
                // расчет граничных условий на дне для вихря
                //wm.CalkBoundaryVortex(Phi, Vortex, velosityUy, w, ref boundaryVortexValue, VortexBC_G2);
                CalkBCVortex(Phi, Vortex, w);
                // расчет нелинейной части для вихря
                //taskVortex.Calk_Q_forVortex(eddyViscosity, Vortex, ref VortexQ);
                for (int k = 0, i = 0; i < bcPhiAdress.Length; i++)
                    bcVortexValue[k++] = bVortex[bcPhiAdress[i]];
                // расчет вихря
                Console.WriteLine("Vortex:");

                taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz,
                    bcVortexAdress, bcVortexValue, VortexQ);

                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];

                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTask(ref Phi, PhiMu, bcPhiAdress, bcPhiValue, Vortex);
                // релаксация функции тока
                if (n == 0)
                    for (int i = 0; i < CountKnots; i++)
                        Phi_old[i] = Phi[i];
                else
                    for (int i = 0; i < CountKnots; i++)
                        Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];

                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vx, ref Vy);
                // расчет поля вязкости
                // SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWCrossSection)wMesh, typeEddyViscosity, Vx);
                //for (int i = 0; i < CountKnots; i++)
                //    eddyViscosity[i] = eddyViscosity[i]/ SPhysics.rho_w;
                // релаксация функции вязкости
                //for (int i = 0; i < CountKnots; i++)
                //    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];

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
                residual = Math.Max(Math.Sqrt(epsPhi / (normPhi + MEM.Error12)), Math.Sqrt(epsVortex / (normVortex + MEM.Error12)));
                Console.WriteLine("Шаг {0} точность {1}", n, residual);
            }
        }


        #endregion 
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexPhi_BCVelocity(ref double[] Phi, ref double[] Vortex,
            ref double[] Vx, ref double[] Vy, ref double[] Vz, ref double[] eddyViscosity,
            ref double[] tauY, ref double[] tauZ, double[] Q, uint[] bc_Vy, double[] bv_Vy, ECalkDynamicSpeed typeEddyViscosity)
        {
            n = 0;
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
                taskUx.TransportEquationsTaskSUPG(ref Vx, eddyViscosity, Vy, Vz, bcBedAdress, bcVx, Q);
                // релаксация потоковой скорости в створе
                for (int i = 0; i < CountKnots; i++)
                    Vx[i] = (1 - w) * Vx_old[i] + w * Vx[i];
                // расчет поля вязкости
                //taskViscosity.TransportEquationsTaskSUPG(ref eddyViscosity, eddyViscosity_old, Vy, Vz, bc_Phi, bv_Phi, eVQ);
                SPhysics.PHYS.calkTurbVisc(ref eddyViscosity, typeTask, (IMWRiver)wMesh, typeEddyViscosity, Vx, J);
                // релаксация функции вязкости
                for (int i = 0; i < CountKnots; i++)
                    eddyViscosity[i] = (1 - w) * eddyViscosity_old[i] + w * eddyViscosity[i];
                // расчет функции тока
                Console.WriteLine("Phi:");
                //taskPhi.PoissonTask(ref Phi, PhiMu, bcPhiAdress, bcPhiValue, Vortex);
                taskPhi.PoissonTask(ref Phi, PhiMu, Vortex, bcBedAdress, bcPhiValue, bc_Vy, bv_Vy);
                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz);
                // расчет граничных условий на дне для вихря
                CalkBCVortex(Phi, Vortex, w, bcBedAdress, ref bcBedVortexValue);
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz, bcBedAdress, bcBedVortexValue, VortexQ);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
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

        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalkBCVortex(double[] Phi, double[] Vortex, double w)
        {
            try
            {
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                TriElement[] AreaElems = mesh.GetAreaElems();
                // Расчет вихря в граничных узлах
                uint shift = 0;
                if (typeTask == TypeTask.streamY1D)
                {
                    shift = 1;
                    bcBedVortexValue[0] = 0;
                    bcBedVortexValue[bcBedVortexValue.Length - 1] = 0;
                }
                for (uint nod = shift; nod < bcBedAdress.Length - shift; nod++)
                {
                    uint idx = bcBedAdress[nod];
                    IHPoint p = bedPoint[nod];
                    uint ne = bedElems[nod];

                    uint i0 = AreaElems[ne].Vertex1;
                    uint i1 = AreaElems[ne].Vertex2;
                    uint i2 = AreaElems[ne].Vertex3;

                    double[] x = { X[i0], X[i1], X[i2] };
                    double[] y = { Y[i0], Y[i1], Y[i2] };

                    HFForm2D_TriangleAnaliticL1 triff = new HFForm2D_TriangleAnaliticL1();
                    triff.SetGeoCoords(x, y);
                    triff.CalkForm(p.X, p.Y);

                    double phi = Phi[i0] * triff.N[0] + Phi[i1] * triff.N[1] + Phi[i2] * triff.N[2];
                    double vortex = Vortex[i0] * triff.N[0] + Vortex[i1] * triff.N[1] + Vortex[i2] * triff.N[2];
                    // 2 order
                    double newVortex = -3.0 * phi / (pLenth[nod] * pLenth[nod]) - vortex / 2.0;
                    // 1 order
                    //double newVortex_ = - 2 * phi / (pLenth[nod] * pLenth[nod]);
                    bcBedVortexValue[nod] = (1 - w) * Vortex[idx] + w * newVortex;
                }
                //for (uint nod = shift; nod < bcBedAdress.Length-1; nod++)
                //{
                //   // bcBedVortexValue[nod] = bcBedVortexValue[nod]*(1-w) +w*bcBedVortexValue[nod - 1];
                //}
                if (typeTask == TypeTask.streamY1D)
                {
                    bcBedVortexValue[0] = bcBedVortexValue[1];
                    bcBedVortexValue[bcBedAdress.Length - 1] = bcBedVortexValue[bcBedAdress.Length - 2];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalkBCVortex(double[] Phi, double[] Vortex, double w, uint[] bcBedAdress, ref double[] bcBedVortexValue)
        {
            try
            {
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                TriElement[] AreaElems = mesh.GetAreaElems();
                MEM.Alloc(bcBedAdress.Length, ref bcBedVortexValue, "bcBedVortexValue");
                HFForm2D_TriangleAnaliticL1 triff = new HFForm2D_TriangleAnaliticL1();
                // Расчет вихря в граничных узлах
                for (uint nod = 1; nod < bcBedVortexValue.Length - 1; nod++)
                {
                    uint idx = bcBedAdress[nod];
                    
                    IHPoint p = bedPoint[nod];
                    uint ne = bedElems[nod];
                    // узлы
                    uint i0 = AreaElems[ne].Vertex1;
                    uint i1 = AreaElems[ne].Vertex2;
                    uint i2 = AreaElems[ne].Vertex3;

                    double[] x = { X[i0], X[i1], X[i2] };
                    double[] y = { Y[i0], Y[i1], Y[i2] };

                    triff.SetGeoCoords(x, y);
                    triff.CalkForm(p.X, p.Y);
                    double phi = Phi[i0] * triff.N[0] + Phi[i1] * triff.N[1] + Phi[i2] * triff.N[2];
                    double vortex = Vortex[i0] * triff.N[0] + Vortex[i1] * triff.N[1] + Vortex[i2] * triff.N[2];
                    // 2 order
                    double newVortex = -3.0 * phi / (pLenth[nod] * pLenth[nod]) - vortex / 2.0;
                    bcBedVortexValue[nod] = (1 - w) * Vortex[idx];
                    bcBedVortexValue[nod] += w * newVortex;
                }
                
                bcBedVortexValue[0] = 0;
                bcBedVortexValue[bcBedVortexValue.Length - 1] = 0;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

    }
}
