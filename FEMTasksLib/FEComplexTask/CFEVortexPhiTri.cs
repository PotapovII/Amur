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
    /// <summary>
    /// ОО: Задача с вынужденной конвекцией, на верхней крышке области заданн
    /// 1. Вихрь
    /// 2. Скорость
    /// </summary>
    [Serializable]
    public class CFEVortexPhiTri : AComplexFEMTask
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
        /// условная вязкость для задачи Пуассона
        /// </summary>
        public double[] PhiMu;
        public uint[]   bc_Phi;
        public double[] bv_Phi;
        /// <summary>
        /// функции тока
        /// </summary>
        public double[] Phi;
        /// <summary>
        /// функции тока
        /// </summary>
        protected double[] Phi_old;
        /// <summary>
        /// функции вихря
        /// </summary>
        public double[] Vortex;
        /// <summary>
        /// функции вихря
        /// </summary>
        public double[] eddyViscosity_old;
        public double[] eVQ;
        /// <summary>
        /// функции вихря
        /// </summary>
        protected double[] Vortex_old;
        /// <summary>
        /// функции скорости ->
        /// </summary>
        public double[] Vy;
        /// <summary>
        /// функции скорости ^
        /// </summary>
        public double[] Vz;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] bcBedAdress = null;
        /// <summary>
        /// значения вихря на дне
        /// </summary>
        protected double[] bcBedVortexValue;

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
        public CFEVortexPhiTri(IMeshWrapper wMesh, IAlgebra algebra, TypeTask typeTask, double w = 0.3) : base(wMesh, algebra, typeTask)
        {
            this.w = w;
            taskPhi = new CFEPoissonTaskTri(wMesh, algebra, typeTask);
            taskVortex = new CTransportEquationsTri(wMesh, algebra, typeTask);
            taskViscosity = new CTransportEquationsTri(wMesh, algebra, typeTask);

            CountKnots = (uint)mesh.CountKnots;
            MEM.Alloc(CountKnots, ref Phi);
            MEM.Alloc(CountKnots, ref Phi_old);
            MEM.Alloc(CountKnots, ref Vortex);
            MEM.Alloc(CountKnots, ref Vortex_old);
            MEM.Alloc(CountKnots, ref eddyViscosity_old);
            MEM.Alloc(CountKnots, ref Vy);
            MEM.Alloc(CountKnots, ref Vz);
            MEM.Alloc(CountKnots, ref eVQ);
            MEM.Alloc(mesh.CountKnots, ref PhiMu, 1);
            int[] bknots = mesh.GetBoundKnots();
            MEM.Alloc(bknots.Length, ref bc_Phi);
            MEM.Alloc(bknots.Length, ref bv_Phi, 0);
            for (int i = 0; i < bknots.Length; i++)
                bc_Phi[i] = (uint)bknots[i];
        }
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданн вихрь
        /// </summary>
        public virtual void CalkVortexPhi_BCVortex(ref double[] Phi, ref double[] Vortex,
            ref double[] Vy, ref double[] Vz, double[] eddyViscosity, double[] Q,
            uint[] bcVortexAdress, double[] bcVortexValue)
        {
            n = 0;
            residual = double.MaxValue;
            MEM.Alloc(mesh.CountKnots, ref Q, 0);
            int[] bknots = mesh.GetBoundKnots();
            int[] Marks = mesh.GetBoundKnotsMark();
            List<uint> idx_Bed = new List<uint>();
            for (int i = 0; i < Marks.Length; i++)
            {
                if (Marks[i] == 0) // Низ
                    idx_Bed.Add((uint)bknots[i]);
            }
            bcBedAdress = idx_Bed.ToArray();
            
            MEM.Copy(eddyViscosity_old, eddyViscosity);

            // цикл по нелинейности
            for (n = 0; n < 150 && residual > 1e-4; n++)
            {
                MEM.MemCopy(ref Phi_old, Phi);
                MEM.MemCopy(ref Vortex_old, Vortex);
                MEM.MemCopy(ref eddyViscosity_old, eddyViscosity);
                // учитываем краевые условия первого рода
                // расчет вихря
                Console.WriteLine("Vortex:");
                taskVortex.TransportEquationsTaskSUPG(ref Vortex, eddyViscosity, Vy, Vz, bcVortexAdress, bcVortexValue, Q);
                // релаксация вихря
                for (int i = 0; i < CountKnots; i++)
                    Vortex[i] = (1 - w) * Vortex_old[i] + w * Vortex[i];
                // расчет функции тока
                Console.WriteLine("Phi:");
                taskPhi.PoissonTaskPhiTrue(ref Phi, Phi_old, bc_Phi, bv_Phi, Vortex);
                
                // релаксация функции тока
                for (int i = 0; i < CountKnots; i++)
                    Phi[i] = (1 - w) * Phi_old[i] + w * Phi[i];
                // расчет поля скорости
                Console.WriteLine("Vy ~ Vz:");
                taskPhi.CalcVelosity(Phi, ref Vy, ref Vz);
                // расчет ГУ для функции вихря
                CalkBCVortex(Phi, Vortex, w, bcBedAdress, ref bcBedVortexValue);
                for(int i = 0; i < bcBedAdress.Length; i++)
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
        }
        /// <summary>
        /// Задача с вынужденной конвекцией, на верхней крышке области заданна скорость
        /// </summary>
        public virtual void CalkVortexPhi_BCVelocity(ref double[] Phi, ref double[] Vortex,
            ref double[] Vy, ref double[] Vz, double[] eddyViscosity, double[] Q,
            uint[] bc_Vy, double[] bv_Vy)
        {

        }
        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalkBCVortex(double[] Phi, double[] Vortex, double w, uint[] bcBedAdress, ref double[] bcBedVortexValue)
        {
            try
            {
                int cu = 3;
                uint[] knots = { 0, 0, 0};
                double[] x = { 0, 0, 0 };
                double[] y = { 0, 0, 0 };
                double[] elem_Phi = null;
                double[] elem_Vortex = null;
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                uint[] bedElems = null;
                HPoint[] bedPoint = null;
                double[] pLenth = null;
                MEM.Alloc(bcBedAdress.Length, ref bedElems, "bedElems");
                MEM.Alloc(bcBedAdress.Length, ref bedPoint, "bedPoint");
                MEM.Alloc(bcBedAdress.Length, ref pLenth, "pLenth");
                MEM.Alloc(bcBedAdress.Length, ref bcBedVortexValue, "bcBedVortexValue");
                
                for (uint nod = 1; nod < bcBedAdress.Length - 1; nod++)
                {
                    HPoint pe = new HPoint(X[bcBedAdress[nod + 1]], Y[bcBedAdress[nod + 1]]);
                    HPoint pw = new HPoint(X[bcBedAdress[nod - 1]], Y[bcBedAdress[nod - 1]]);
                    HPoint p = new HPoint(X[bcBedAdress[nod]], Y[bcBedAdress[nod]]);
                    HPoint tau = pe - pw;
                    double Length2 = tau.Length() / 4;
                    pLenth[nod] = Length2;
                    HPoint P =  p + tau.GetOrtogonalLeft() * Length2;
                    bedPoint[nod] = P;
                }

                // поиск трехугольного элемента для аппроксимации
                TriMeshLocators triMeshLocators = new TriMeshLocators(mesh);
                HFForm2D_TriangleAnaliticL1 triff = new MeshLib.HFForm2D_TriangleAnaliticL1();
                for (uint p = 1; p < bedPoint.Length-1; p++)
                {
                    int idx = triMeshLocators.QueryElement(bedPoint[p]);
                    if (idx > -1)
                        bedElems[p] = (uint)idx;
                }
                // Расчет вихря в граничных узлах
                for (uint nod = 1; nod < bcBedVortexValue.Length-1; nod++)
                {
                    uint idx = bcBedAdress[nod];
                    uint elem = bedElems[nod];
                    IHPoint p = bedPoint[nod];
                    // узлы
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // память
                    cu = knots.Length;
                    MEM.Alloc(cu, ref x, "x");
                    MEM.Alloc(cu, ref y, "y");
                    MEM.Alloc(cu, ref elem_Phi, "elem_Phi");
                    MEM.Alloc(cu, ref elem_Vortex, "elem_Vortex");
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    mesh.ElemValues(Phi, elem, ref elem_Phi);
                    mesh.ElemValues(Vortex, elem, ref elem_Vortex);
                    triff.SetGeoCoords(x, y);
                    triff.CalkForm(p.X, p.Y);
                    double phi = 0;
                    double vortex = 0;
                    for (int i = 0; i < cu; i++)
                    {
                        phi += elem_Phi[i] * triff.N[i];
                        vortex += elem_Vortex[i] * triff.N[i];
                    }
                    bcBedVortexValue[nod] = (1 - w) * Vortex[idx];
                    bcBedVortexValue[nod] += w * (-vortex / 2.0 - 3.0 * phi / (pLenth[nod] * pLenth[nod]));
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
