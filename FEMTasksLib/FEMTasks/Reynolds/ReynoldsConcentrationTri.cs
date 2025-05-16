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
//                 кодировка : 01.05.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace FEMTasksLib.FEMTasks.VortexStream
{
    using System;
    using System.Collections.Generic;

    using AlgebraLib;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Physics;
    using CommonLib.Function;
    using CommonLib.Geometry;
    using System.Linq;

    /// <summary>
    /// ОО: Решение задачи переноса взвешенных наносов на симплекс сетке
    /// </summary>
    [Serializable]
    
    public class ReynoldsConcentrationTri : AWRAP_FETaskTri
    {
        protected double _alphaB = 1;
        protected double _alphaD = 10;

        /// <summary>
        /// Тип задачи
        /// </summary>
        protected TypeTask typeTask;
        /// <summary>
        /// Число Прандтля для уравнения концентрации наносов
        /// </summary>
        public const double PrandtlС = 0.8;
        /// <summary>
        /// тип задачи 0 - плоская 1 - цилиндрическая
        /// </summary>
        protected int SigmaTask;
        /// <summary>
        /// радиус изгиба русла
        /// </summary>
        protected double RadiusMin;
        /// <summary>
        /// Плотность воды
        /// </summary>
        protected double rho_w = SPhysics.rho_w;
        /// <summary>
        /// гидравлическая урупность частиц
        /// </summary>
        protected double Ws;
        /// <summary>
        /// Радиальная/боковая скорости на WL
        /// </summary>
        public IDigFunction VelosityUx = null;
        /// <summary>
        /// граничные условия
        /// </summary>
        protected double[] bCondS = null;
        /// <summary>
        /// граничные узлы
        /// </summary>
        protected uint[] bIndexS = null; 

        #region Поля результатов
        public double[] Concentration;
        #endregion
        /// <summary>
        /// Параметр SUPG стабилизации 
        /// </summary>
        public double omega = 0.5;
        public ReynoldsConcentrationTri(double RadiusMin, int SigmaTask, TypeTask typeTask) 
        {
            this.SigmaTask = SigmaTask;
            this.RadiusMin = RadiusMin;
            this.Ws = SPhysics.PHYS.Ws;
            this.typeTask = typeTask;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra,IMeshWrapper wMesh = null)
        {
            base.SetTask(mesh, algebra, wMesh);
            MEM.Alloc(mesh.CountKnots, ref Concentration);
            if (algebra == null)
                algebra = new AlgebraGauss((uint)mesh.CountKnots);
            
        }
        /// <summary>
        /// Нахождение поля стационарного поля концентрации наносов в створе потока
        /// с учетом вторичных скорстей потока определяемых через функцию тока
        /// </summary>
        /// <param name="concentration">поле концентрации</param>
        /// <param name="eddyViscosity">турбулентная вязкость</param>
        /// <param name="Phi">функция тока</param>
        /// <param name="Ux">потоковая/окружная скорость</param>
        /// <param name="BCalkConcentration">граничные условия на дне створа/канала 1 - Дирехле, 2 - Неймана</param>
        public void SolveTaskConcentS(ref double[] concentration, 
                    double[] eddyViscosity, double[] Phi, double[] tauBE, int BCalkConcentration)
        {
            int elemEx = 0;
            try
            {
                MEM.Alloc(mesh.CountKnots, ref concentration);
                algebra.Clear();
                CalkCrossSectionBC(mesh, tauBE, BCalkConcentration, ref bCondS, ref bIndexS);
                // выделить память под локальные массивы
                for (int elem = 0; elem < mesh.CountElements; elem++)
                {
                    elemEx = elem;
                    // локальная матрица часть СЛАУ
                    double[][] LaplMatrix = new double[3][]
                    {
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 },
                       new double[3]{ 0,0,0 }
                    };
                    // локальная правая часть СЛАУ
                    double[] LocalRight = { 0, 0, 0 };
                    double[] b = dNdx[elem];
                    double[] c = dNdy[elem];
                    uint i0 = eKnots[elem].Vertex1;
                    uint i1 = eKnots[elem].Vertex2;
                    uint i2 = eKnots[elem].Vertex3;
                    uint[] knots = { i0, i1, i2 };
                    double R, R2;
                    double eVx;
                    double eVy;
                    // коэффициент турбулентной диффузии
                    double mU = (eddyViscosity[i0] + eddyViscosity[i1] + eddyViscosity[i2]) / (3 * PrandtlС);
                    if (SigmaTask == 0)
                    {
                        R = 1; R2 = 1;
                        eVx = c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2];
                        eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2]);
                    }
                    else
                    {
                        R = RadiusMin + (X[i0] + X[i1] + X[i2]) / 3;
                        R2 = R * R;
                        eVx = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2])/R;
                        eVy = -(b[0] * Phi[i0] + b[1] * Phi[i1] + b[2] * Phi[i2])/R;
                    }
                    double mV = Math.Sqrt(eVx * eVx + eVy * eVy);
                    double Hk = (Hx[elem] + Hy[elem]) / 2;
                    double Pe = rho_w * mV * Hk / (2 * mU);
                    double Ax = 0;
                    double Ay = 0;
                    if (MEM.Equals(mV, 0) != true)
                    {
                        Ax = eVx / mV;
                        Ay = eVy / mV;
                    }
                    double Se = S[elem];
                    double Se3 = Se/3;
                    // Вычисление ЛЖМ
                    for (int i = 0; i < cu; i++)
                    {
                        double L_SUPG = omega * (Hx[elem] * Ax * b[i] + Hy[elem] * Ay * c[i]);
                        double La = (1 / 3.0 + L_SUPG);
                        for (int j = 0; j < cu; j++)
                        {
                            // конверктивные члены плоские
                            double C_agb = La * ((c[0] * b[j] - b[0] * c[j]) * Phi[i0] +
                                                 (c[1] * b[j] - b[1] * c[j]) * Phi[i1] +
                                                 (c[2] * b[j] - b[2] * c[j]) * Phi[i2]) * Se;

                            double dPhidz = (c[0] * Phi[i0] + c[1] * Phi[i1] + c[2] * Phi[i2]) / R2;
                            // матрица масс
                            double m_ab = MM[i][j] * Se;
                            // матрица жосткости - вязкие члены плоские
                            double K_ab = (b[i] * b[j] + c[i] * c[j]) * Se;
                            // вектор SUPG
                            double _Fa = La * Se;
                            // матрица масс SUPG
                            double _m_ab = m_ab + L_SUPG * Se3;
                            LaplMatrix[i][j] = rho_w * (C_agb / R - Ws * _Fa * c[j]) + mU * K_ab;
                            if(SigmaTask==1)
                                LaplMatrix[i][j] += rho_w * dPhidz * _m_ab + 1 / R * (mU * b[i] * Se3 + mU * _m_ab);
                        }
                    }
                    algebra.AddToMatrix(LaplMatrix, knots);
                }
                // учет граничных условий Неймана на дне канала
                if (BCalkConcentration == 2)
                {
                    double tau0 = SPhysics.PHYS.tau0;
                    double Db = SPhysics.PHYS.Db;
                    double d50 = SPhysics.PHYS.d50;
                    
                    // длины ГЭ
                    double[] Lb = wMesh.GetLb();
                    int[] bMark = mesh.GetBElementsBCMark();
                    HPoint[] tau = wMesh.GetTau();
                    double[] Y = mesh.GetCoords(1);
                    double min = 0, max = 0;
                    mesh.MinMax(1, ref min, ref max);
                    // выделить память под локальные массивы
                    int belemTau = 0;
                    for (int belem = 0; belem < mesh.CountBoundElements; belem++)
                    {
                        if (bMark[belem] != 0) continue;
                        uint i0 = beKnots[belem].Vertex1;
                        uint i1 = beKnots[belem].Vertex2;
                        uint[] knots = { i0, i1 };
                        double h = Lb[belem];

                        double tauBed = tauBE[belemTau];
                        double u0 = Math.Sqrt(Math.Abs(tauBed / rho_w));
                        double Ra = SPhysics.PHYS.Ra(u0);

                        double z = 0.5 * (Y[i0] + Y[i1]);
                        double H = max - z;
                        double a = H / (2 * d50);
                        double Ds = _alphaD * Ws * Math.Pow(a, Ra);

                        double sinG = tau[belem].Y;
                        double cosG = tau[belem].X;
                        double tanG = sinG / cosG;
                        double tau0c = tau0 * Math.Sqrt(Math.Max(MEM.Error8, 1 - tanG * tanG));
                        double S_bed = (_alphaB / Db) * Math.Max(0, tauBed - tau0c) / tau0c;

                        double mh = Ds * h / 6;
                        double fh = S_bed * Ds * h / 2;
                        // локальная матрица масс 
                        double[][] LaplMatrix = new double[2][]
                        {
                           new double[2]{  2* mh, mh },
                           new double[2]{  mh, 2*mh },
                        };
                        // локальная правая часть СЛАУ
                        double[] LocalRight = { fh, fh };
                        // Вычисление ЛЖМ
                        algebra.AddToMatrix(LaplMatrix, knots);
                        // добавление вновь сформированной ЛПЧ в ГПЧ
                        algebra.AddToRight(LocalRight, knots);
                    }
                }
                //Удовлетворение ГУ
                algebra.BoundConditions(bCondS, bIndexS);
                algebra.Solve(ref concentration);
                if(Debug > 0)
                    foreach (var ee in concentration)
                        if (double.IsNaN(ee) == true)
                            throw new Exception("Ошибка при расчете концентрации частиц ReynoldsConcentrationTri >> algebra");

                MEM.Copy(Concentration, concentration);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elemEx.ToString());
            }
        }
     

        /// <summary>
        /// Решение задачи
        /// </summary>
        /// <param name="result">результат решения</param>
        public override void SolveTask(ref double[] result){}
        /// <summary>
        /// Расчет граничных условий (по граничным элементам)
        /// </summary>
        public void CalkCrossSectionBC(IMesh mesh,double[] tauBE, 
                    int BCalkConcentration,ref double[] bCond, ref uint[] bIndex)
        {
            int[] tmpIndex = null;
            double[] tmpValue = null;
            MEM.Alloc(mesh.CountKnots, ref tmpIndex);
            MEM.Alloc(mesh.CountKnots, ref tmpValue);
            var bknotsMark = mesh.GetBoundKnotsMark();
            var bknots = mesh.GetBoundKnots();
            double[] X = mesh.GetCoords(0);
            double bcS;
            double tau0 = SPhysics.PHYS.tau0;
            double Db = SPhysics.PHYS.Db;
            double alphaB = _alphaB / Db;
            List<double> LUx = new List<double>();
            List<uint> LIdx = new List<uint>();
            double min = 0, max = 0;
            mesh.MinMax(1, ref min, ref max);
            if (BCalkConcentration == 1) // Дирихле - на дне
            {
                double[] LBElem = wMesh.GetLb();
                HPoint[] tau = wMesh.GetTau();
                int count = Marker.Count(x => x == 0);
                uint belemTau = 0;
                for (uint belem = 0; belem < mesh.CountBoundElements; belem++)
                {
                    int mark = mesh.GetBoundElementMarker(belem);
                    if (mark == 0) // Дно
                    {
                        uint i1 = beKnots[belem].Vertex1;
                        uint i2 = beKnots[belem].Vertex2;
                        double tauBed = tauBE[belemTau++];
                        double sinG = tau[belem].Y;
                        double cosG = tau[belem].X;
                        double tanG = sinG/cosG;
                        double tau0c = tau0 * Math.Sqrt(Math.Max(MEM.Error8, 1 - tanG * tanG));
                        bcS = alphaB * Math.Max(0, tauBed - tau0c) / tau0c;
                        tmpIndex[i1]++;
                        tmpIndex[i2]++;
                        tmpValue[i1] += bcS;
                        tmpValue[i2] += bcS;
                    }
                }
                for (uint bknot = 0; bknot < mesh.CountBoundKnots; bknot++)
                {
                    int mark = bknotsMark[bknot];
                    if (mark != 1 || typeTask == TypeTask.streamY1D)
                    {
                        uint knot = (uint)bknots[bknot];
                        LIdx.Add(knot);
                        if (tmpIndex[knot] > 0)
                            LUx.Add(tmpValue[knot] / tmpIndex[knot]);
                        else
                            LUx.Add(0);
                    }
                }
            }
            if (BCalkConcentration == 2) // Нейман- на дне
            {
                for (uint bknot = 0; bknot < mesh.CountBoundKnots; bknot++)
                {
                    int mark = bknotsMark[bknot];
                    uint knot = (uint)bknots[bknot];
                    if (mark == 2) // свободная поверхность
                    {
                        LIdx.Add(knot);
                        LUx.Add(0);
                    }
                }
            }
            // граничные условия
            bCond = LUx.ToArray();
            // граничные узлы
            bIndex = LIdx.ToArray();
        }
    }
}
