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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   31.03.2024 Потапов И.И.
//---------------------------------------------------------------------------
//             Эксперимент: прототип "Расчетная область"
//---------------------------------------------------------------------------
namespace MeshLib.Wrappers
{
    using MemLogLib;
    using CommonLib;
    using CommonLib.Mesh;
    using CommonLib.Geometry;
    using GeometryLib.Locators;

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using CommonLib.Physics;
    using CommonLib.Function;
    using GeometryLib;



    /// <summary>
    /// ОО: Обертка для КЭ сетки - выполняет предварительные вычисления
    /// при решении задач методом КЭ в канале без стенок (x,z)
    /// </summary>
    [Serializable]
    public class MWCrossSectionTri : MWCrossTri, IMWCrossSection
    {
        /// <summary>
        /// индексы приповерхностных конечных элементов в которые попадает точка наблюдения
        /// </summary>
        public int[] GetBedElems() => bedElems;
        /// <summary>
        /// координаты точки отложенной по нормали к дну в расчетную область 
        /// на величину delta * step
        /// </summary>
        /// <returns></returns>
        public IHPoint[] GetBedPoint() => bedPoint;
        /// <summary>
        /// адреса донных узлов
        /// </summary>
        public uint[] GetBoundaryBedAdress() => boundaryBedAdress;

        /// <summary>
        /// индексы приповерхностных конечных элементов в которые попадает точка
        /// </summary>
        public int[] GetWaterLevelElems() => waterLevelElems;
        /// <summary>
        /// Шаг дискретизации по дну
        /// </summary>
        /// <returns></returns>
        public double[] GetpLengthBed() => pLengthBed;
        /// <summary>
        /// координаты точки отложенной по нормали к свободной поверхности потока 
        /// в расчетную область на величину delta * step
        /// </summary>
        /// <returns></returns>
        public IHPoint[] GetWaterLevelPoint() => waterLevelPoint;
        /// <summary>
        /// адреса узлов на свободной поверхности потока
        /// </summary>
        public uint[] GetBoundaryWaterLevelAdress() => boundaryWaterLevelAdress;
        /// <summary>
        /// Шаг дискретизации по свободной поверхности
        /// </summary>
        /// <returns></returns>
        public double[] GetpLengthWaterLevel() => pLengthWaterLevel;
        /// <summary>
        /// адреса узлов на свободной поверхности потока и дне
        /// </summary>
        public uint[] GetBoundaryAdress() => boundaryAdress;
        /// <summary>
        /// координаты точки отложенной по нормали к дну в расчетную область 
        /// на величину delta * step
        /// </summary>
        /// <returns></returns>
        public IHPoint[] GetMuBedPoint() => muBedPoint;
        /// <summary>
        /// растояние от донного узла до точки наблюдения 0.05 * H
        /// </summary>
        /// <returns></returns>
        public double[] GetMuLengthBed() => muLengthBed;
        /// <summary>
        /// индексы придонных конечных элементов в которые попадает точка наблюдения
        /// при расчете динамической скорости на растоянии от дна 0.05 * H
        /// </summary>
        public int[] GetMuBedElems() => muBedElems;
        /// <summary>
        /// Минимальный радиус - растояние от оси z до выпоклого берега чечения
        /// </summary>
        public double GetR_midle() => R_min;
        /// <summary>
        /// Флаг постановки 0 - плоская задача, 1 - осесимметричная задача
        /// </summary>
        public int GetRing() => Ring;
        /// <summary>
        /// индексы придонных конечных элементов в которые попадает точка наблюдения
        /// при расчете динамической скорости на растоянии от дна 0.05 * H
        /// </summary>
        protected int[] muBedElems = null;
        /// <summary>
        /// координаты точки отложенной по нормали к дну в расчетную область 
        /// на величину 0.05 * H
        /// </summary>
        /// <returns></returns>
        protected IHPoint[] muBedPoint = null;
        /// <summary>
        /// растояние от донного узла до точки наблюдения 0.05 * H
        /// </summary>
        protected double[] muLengthBed = null;
        /// <summary>
        /// Отметка свободной поверхности
        /// </summary>
        public double WaterLevel { get => WL; } 
        /// <summary>
        /// индексы придонных конечных элементов в которые попадает точка наблюдения
        /// для расчета граничных условий
        /// </summary>
        protected int[] bedElems = null;
        /// <summary>
        /// координаты точки отложенной по нормали к дну в расчетную область 
        /// на величину delta * step
        /// </summary>
        /// <returns></returns>
        protected IHPoint[] bedPoint = null;

        /// <summary>
        /// адреса донных узлов
        /// </summary>
        protected uint[] boundaryBedAdress = null;
        /// <summary>
        /// индексы приповерхностных конечных элементов в которые попадает точка
        /// </summary>
        protected int[] waterLevelElems = null;
        /// <summary>
        /// растояние от донного узла до точки наблюдения
        /// </summary>
        protected double[] pLengthBed = null;
        /// <summary>
        /// координаты точки отложенной по нормали к свободной поверхности потока 
        /// в расчетную область на величину delta * step
        /// </summary>
        /// <returns></returns>
        protected IHPoint[] waterLevelPoint = null;
        /// <summary>
        /// адреса узлов на свободной поверхности потока
        /// </summary>
        protected uint[] boundaryWaterLevelAdress = null;
        /// <summary>
        /// растояние от узла на свободной поверхности до точки наблюдения
        /// </summary>
        protected double[] pLengthWaterLevel = null;
        /// <summary>
        /// адреса узлов на свободной поверхности потока и дне
        /// </summary>
        protected uint[] boundaryAdress = null;
        /// <summary>
        /// левая полуобласть
        /// </summary>
        protected bool half;
        /// <summary>
        /// Сдвиг для правого берега 
        /// </summary>
        protected uint shift;
        /// <summary>
        /// Минимальный радиус
        /// </summary>
        protected double R_min;
        /// <summary>
        /// Флаг постановки 0 - плоская постановка 1 закругленный канал
        /// </summary>
        protected int Ring;
        /// <summary>
        /// Координаты точек x донной поверхности
        /// </summary>
        protected double[] fx = null;
        /// <summary>
        /// Координаты точек y донной поверхности
        /// </summary>
        protected double[] fy = null;
        /// <summary>
        /// Отметка свободной поверхности
        /// </summary>
        protected double WL;

        protected double Ymin;

        public MWCrossSectionTri(IMesh mesh) 
            : this(mesh, 0, 1, true, SСhannelForms.halfPorabolic) { }

        public MWCrossSectionTri(IMesh mesh, bool half, 
           SСhannelForms channelSectionForms = SСhannelForms.porabolic) 
            : this(mesh, 0, 1, half, channelSectionForms)
        { }

        public MWCrossSectionTri(IMesh mesh, double R_min, int Ring, bool half,
            SСhannelForms channelSectionForms = SСhannelForms.porabolic) 
            : base(mesh, channelSectionForms)
        {
            this.half = half;
            if (half == true) // если правого берега нет, то правую "береговую" точку обрабатываем
                shift = 0;
            else
                shift = 1;
            this.WL = mesh.GetCoords(1).Max();
            this.R_min = R_min;
            this.Ring = Ring;
            CalkNormAdress();
        }
        /// <summary>
        /// Установка КЭ сетки
        /// </summary>
        /// <param name="mesh"></param>
        public void CalkNormAdress()
        {
            try
            {
                int[] bknots = mesh.GetBoundKnots();
                int[] Marks = mesh.GetBoundKnotsMark();
                TwoElement[] bElems = mesh.GetBoundElems();
                int[] elemMarks = mesh.GetBElementsBCMark();
                List<uint> idx_Bed = new List<uint>();
                List<uint> idx_WL = new List<uint>();
                List<uint> idx_BC = new List<uint>();

                if (channelSectionForms == SСhannelForms.boxCrossSection || 
                    channelSectionForms == SСhannelForms.trapezoid)
                {
                    for (int i = 0; i < bElems.Length; i++)
                    {
                        if (elemMarks[i] == 3) // Стенки
                        {
                            TwoElement be = bElems[i];
                            if (idx_Bed.Contains(be.Vertex1) == false)
                                idx_Bed.Add(be.Vertex1);
                            if (idx_Bed.Contains(be.Vertex2) == false)
                                idx_Bed.Add(be.Vertex2);
                        }
                    }
                }
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
                    if (elemMarks[i] == 2) // крышка
                    {
                        TwoElement be = bElems[i];
                        if (idx_WL.Contains(be.Vertex1) == false)
                            idx_WL.Add(be.Vertex1);
                        if (idx_WL.Contains(be.Vertex2) == false)
                            idx_WL.Add(be.Vertex2);
                    }
                }
                if (channelSectionForms == SСhannelForms.boxCrossSection
                 //|| channelSectionForms == SСhannelForms.halfPorabolic
                 || channelSectionForms == SСhannelForms.trapezoid)
                {
                    for (int i = 0; i < bElems.Length; i++)
                    {
                        if (elemMarks[i] == 1) // Стенки
                        {
                            TwoElement be = bElems[i];
                            if (idx_Bed.Contains(be.Vertex1) == false)
                                idx_Bed.Add(be.Vertex1);
                            if (idx_Bed.Contains(be.Vertex2) == false)
                                idx_Bed.Add(be.Vertex2);
                        }
                    }
                }

                boundaryBedAdress = idx_Bed.ToArray();
                boundaryWaterLevelAdress = idx_WL.ToArray();
                idx_BC.AddRange(idx_Bed);
                idx_BC.AddRange(idx_WL);
                boundaryAdress = idx_BC.ToArray();

                GetBoundaryPointt(idx_Bed, idx_WL);
                // поиск трехугольного элемента для аппроксимации
                TriMeshLocators triMeshLocators = new TriMeshLocators(mesh);
                HFForm2D_TriangleAnaliticL1 triff = new HFForm2D_TriangleAnaliticL1();
                for (uint p = 1; p < bedPoint.Length - shift; p++)
                {
                    var pp = bedPoint[p];
                    bedElems[p] = triMeshLocators.QueryElement(pp);
                    muBedElems[p] = triMeshLocators.QueryElement(muBedPoint[p]);
                }
                for (uint p = 1; p < waterLevelPoint.Length - shift; p++)
                {
                    waterLevelElems[p] = triMeshLocators.QueryElement(waterLevelPoint[p]);
                }
                MEM.Alloc(boundaryBedAdress.Length, ref fx, "fx");
                MEM.Alloc(boundaryBedAdress.Length, ref fy, "fy");
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                Ymin = Y.Min();
                // Расчет вихря в граничных узлах
                for (uint nod = 0; nod < boundaryBedAdress.Length; nod++)
                {
                    fx[nod] = X[boundaryBedAdress[nod]];
                    fy[nod] = Y[boundaryBedAdress[nod]];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }

        protected void GetBoundaryPointt(List<uint> idx_Bed, List<uint> idx_WL)
        {
            double[] X = mesh.GetCoords(0);
            double[] Y = mesh.GetCoords(1);
            double[] S = GetS();
            double delta = Math.Sqrt(S[0]);
            if (channelSectionForms == SСhannelForms.boxCrossSection)
            {
                TriElement elem = mesh.GetAreaElems()[0];
                delta = Math.Max(Math.Abs(Y[elem[0]] - Y[elem[1]]), Math.Abs(Y[elem[2]] - Y[elem[1]]));
            }
            MEM.Alloc(idx_Bed.Count, ref pLengthBed, "pLengthBed");
            MEM.Alloc(idx_Bed.Count, ref bedElems, "bedElems");
            MEM.Alloc(idx_Bed.Count, ref bedPoint, "bedPoint");
            
            MEM.Alloc(idx_Bed.Count, ref muBedPoint, "muBedPoint");
            MEM.Alloc(idx_Bed.Count, ref muBedElems, "muBedElems");
            MEM.Alloc(idx_Bed.Count, ref muLengthBed, "muLengthBed");
            double Z0 = 0.05 * (WL - Ymin);
            //LOG.Print("idx_Bed", idx_Bed.ToArray());
            for (int nod = 1; nod < idx_Bed.Count - 1; nod++)
            {
                HPoint pe = new HPoint(X[idx_Bed[nod + 1]], Y[idx_Bed[nod + 1]]);
                HPoint pw = new HPoint(X[idx_Bed[nod - 1]], Y[idx_Bed[nod - 1]]);
                HPoint p = new HPoint(X[idx_Bed[nod]], Y[idx_Bed[nod]]);
                HPoint tau = pe - pw;
                pLengthBed[nod] = delta;
                HPoint normal = tau.GetOrt();
                HPoint P = p - normal * delta;
                bedPoint[nod] = P;
                // расчет координат точки для определения динамической скорости
                muLengthBed[nod] = Z0;
                P = p - normal * Z0;
                muBedPoint[nod] = P;
            }
            {
                int nod = 0;
                if (bedPoint[nod] == null)
                {
                    HPoint p = new HPoint(X[idx_Bed[nod]], Y[idx_Bed[nod]]);
                    HPoint pe = new HPoint(X[idx_Bed[nod + 1]], Y[idx_Bed[nod + 1]]);
                    HPoint tau = pe - p;
                    double Length2 = tau.Length() / 2;
                    pLengthBed[nod] = Length2;
                    HPoint normal = tau.GetOrt();
                    HPoint P = p - normal * delta;
                    bedPoint[nod] = P;
                    muLengthBed[nod] = Z0;
                    P = p - normal * Z0;
                    muBedPoint[nod] = P;
                }
                nod = idx_Bed.Count - 1;
                if (bedPoint[nod] == null)
                {
                    HPoint pw = new HPoint(X[idx_Bed[nod - 1]], Y[idx_Bed[nod - 1]]);
                    HPoint p = new HPoint(X[idx_Bed[nod]], Y[idx_Bed[nod]]);
                    HPoint tau = p - pw;
                    double Length2 = tau.Length() / 2;
                    pLengthBed[nod] = Length2;
                    HPoint normal = tau.GetOrt();
                    HPoint P = p - normal * delta;
                    bedPoint[nod] = P;
                    muLengthBed[nod] = Z0;
                    P = p - normal * Z0;
                    muBedPoint[nod] = P;
                }
            }
            MEM.Alloc(idx_WL.Count, ref waterLevelElems, "waterLevelElems");
            MEM.Alloc(idx_WL.Count, ref waterLevelPoint, "waterLevelPoint");
            MEM.Alloc(idx_WL.Count, ref pLengthWaterLevel, "pLengthWaterLevel");

            // LOG.Print("idx_WL", idx_WL.ToArray());
            for (int nod = 1; nod < idx_WL.Count - 1; nod++)
            {
                HPoint pe = new HPoint(X[idx_WL[nod + 1]], Y[idx_WL[nod + 1]]);
                HPoint pw = new HPoint(X[idx_WL[nod - 1]], Y[idx_WL[nod - 1]]);
                HPoint p = new HPoint(X[idx_WL[nod]], Y[idx_WL[nod]]);
                HPoint tau = pe - pw;
                HPoint normal = tau.GetOrt();
                HPoint P = p - normal * delta;
                if(P.Y > p.Y)
                    P = p + normal * delta;
                pLengthWaterLevel[nod] = delta;
                waterLevelPoint[nod] = P;
            }
            {
                int nod = 0;
                if (waterLevelPoint[nod] == null)
                {
                    HPoint p = new HPoint(X[idx_WL[nod]], Y[idx_WL[nod]]);
                    HPoint pe = new HPoint(X[idx_WL[nod + 1]], Y[idx_WL[nod + 1]]);
                    HPoint tau = pe - p;
                    double Length2 = tau.Length() / 2;
                    pLengthWaterLevel[nod] = Length2;
                    HPoint P = p + tau.GetOrt() * Length2;
                    waterLevelPoint[nod] = P;
                }
                nod = idx_WL.Count - 1;
                if (waterLevelPoint[nod] == null)
                {
                    HPoint pw = new HPoint(X[idx_WL[nod - 1]], Y[idx_WL[nod - 1]]);
                    HPoint p = new HPoint(X[idx_WL[nod]], Y[idx_WL[nod]]);
                    HPoint tau = p - pw;
                    double Length2 = tau.Length() / 2;
                    pLengthWaterLevel[nod] = Length2;
                    HPoint P = p + tau.GetOrt() * Length2;
                    waterLevelPoint[nod] = P;
                }
            }
        }
        /// <summary>
        /// Расчет компонент поля скорости по функции тока
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalkBedVortex(double[] Phi, double[] Vortex, double w, ref double[] bcBedVortexValue)
        {
            try
            {
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                TriElement[] AreaElems = mesh.GetAreaElems();
                MEM.Alloc(boundaryBedAdress.Length, ref bcBedVortexValue, "bcBedVortexValue");
                HFForm2D_TriangleAnaliticL1 triff = new HFForm2D_TriangleAnaliticL1();
              //  LOG.Print("boundaryBedAdress", boundaryBedAdress);
                // Расчет вихря в граничных узлах
                for (uint nod = 1; nod < boundaryBedAdress.Length - shift; nod++)
                {
                    uint idx = boundaryBedAdress[nod];
                    IHPoint p = bedPoint[nod];
                    int ne = bedElems[nod];
                    if (ne > -1)
                    {
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
                        //double newVortex = -3.0 * phi / (pLengthBed[nod] * pLengthBed[nod]) - vortex / 2.0;
                        double dy = pLengthBed[nod];
                        double h = pLengthBed[nod];
                        if (Ring == 0)
                        {
                            double newVortex = -2 * phi / (h*h);
                            bcBedVortexValue[nod] = newVortex;

                            //LOG.Print("mx", x);
                            //LOG.Print("my", y);
                            //LOG.Print("FF", triff.N);
                            //Console.WriteLine(" n = {0} phi = {1}", nod, phi);
                            //Console.WriteLine(" x = {0} y = {1}", p.X, p.Y);
                            //Console.WriteLine(" Phi {0}  {1}  {2}", Phi[i0], Phi[i1], Phi[i2]);
                            //Console.WriteLine(" element = {0} ", ne);
                            //Console.WriteLine(" knots {0}  {1}  {2}", i0, i1, i2);


                            //bcBedVortexValue[nod] = (1 - w) * Vortex[idx];
                            //bcBedVortexValue[nod] += w * newVortex;
                        }
                        else
                        {
                            double R = R_min + x.Sum() / 3;
                            double newVortex = -2 * phi / (h * h * R);
                            bcBedVortexValue[nod] = newVortex;
                            //bcBedVortexValue[nod] = (1 - w) * Vortex[idx];
                            //bcBedVortexValue[nod] += w * newVortex;
                        }
                    }
                    else
                    {
                        bcBedVortexValue[nod] = 0;
                    }
                }
                bcBedVortexValue[0] = 0;
                if(half == false)
                    bcBedVortexValue[bcBedVortexValue.Length - 1] = 0;
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
        public void CalkWaterLevelVortex(double[] Phi, double[] Vortex, double[] wlVelosity, double w, ref double[] bcWLVortexValue)
        {
            try
            {
                //     Console.Clear();
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
               // double mm = Y.Max();
                TriElement[] AreaElems = mesh.GetAreaElems();
                MEM.Alloc(boundaryWaterLevelAdress.Length, ref bcWLVortexValue, "bcBedVortexValue");
                HFForm2D_TriangleAnaliticL1 triff = new HFForm2D_TriangleAnaliticL1();
                // Расчет вихря в граничных узлах
                for (uint nod = 1; nod < boundaryWaterLevelAdress.Length - shift; nod++)
                {
                    uint idx = boundaryWaterLevelAdress[nod];
                    IHPoint p = waterLevelPoint[nod];
                    int ne = waterLevelElems[nod];
                    if (ne > -1)
                    {
                        // узлы
                        uint i0 = AreaElems[ne].Vertex1;
                        uint i1 = AreaElems[ne].Vertex2;
                        uint i2 = AreaElems[ne].Vertex3;

                        double[] x = { X[i0], X[i1], X[i2] };
                        double[] y = { Y[i0], Y[i1], Y[i2] };
                        double[] mPhi = { Phi[i0], Phi[i1], Phi[i2] };

                        triff.SetGeoCoords(x, y);
                        triff.CalkForm(p.X, p.Y);
                        double phi =       Phi[i0] * triff.N[0] +    Phi[i1] * triff.N[1] +    Phi[i2] * triff.N[2];
                        double vortex = Vortex[i0] * triff.N[0] + Vortex[i1] * triff.N[1] + Vortex[i2] * triff.N[2];
                        // 2 order
                        //double newVortex = -3.0 * phi / (pLengthBed[nod] * pLengthBed[nod]) - vortex / 2.0;
                        double newVortex = 0;
                        double h = pLengthBed[nod];
                        if (Ring == 0)
                        {
                            newVortex = -2 * phi / (h * h);
                            //LOG.Print("mx", x);
                            //LOG.Print("my", y);
                            //LOG.Print("FF", triff.N);
                            //Console.WriteLine(" n = {0} phi = {1}", nod, phi);
                            //Console.WriteLine(" x = {0} y = {1}", p.X, p.Y);
                            //Console.WriteLine(" Phi {0}  {1}  {2}", Phi[i0], Phi[i1], Phi[i2]);
                            //Console.WriteLine(" element = {0} ", ne);
                            //Console.WriteLine(" knots {0}  {1}  {2}", i0, i1, i2);
                        }
                        else
                        {
                            double R = R_min + x.Sum() / 3;
                            newVortex = -2 * phi / (h * h * R);
                            //newVortex = -2 * phi / (h * h);
                        }
                        long kk = wlVelosity.Length - 1 - nod;
                        newVortex += - 2 * wlVelosity[kk] / h;
                        //bcWLVortexValue[nod] = (1 - w) * Vortex[idx];
                        //bcWLVortexValue[nod] += w * newVortex;
                        bcWLVortexValue[nod] = newVortex;
                        
                    }
                    else
                        bcWLVortexValue[nod] = 0;
                }
                bcWLVortexValue[0] = 0;
                if (half == false)
                    bcWLVortexValue[bcWLVortexValue.Length - 1] = 0;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }



        /// <summary>
        /// Расчет граничных условий для функции Ux
        /// </summary>
        /// <param name="result">результат решения</param>
        public void CalkBoundaryUx(double[] wlVelosityX, ref double[] wl_Ux)
        {
            try
            {
                MEM.Alloc(boundaryAdress.Length, ref wl_Ux, "wlVelosityX");
                // Расчет вихря в граничных узлах
                for (uint nod = 1; nod < boundaryBedAdress.Length - shift; nod++)
                    wl_Ux[nod] = 0;
                int idx = boundaryBedAdress.Length;
                for (uint nod = 1; nod < boundaryWaterLevelAdress.Length - shift; nod++)
                    wl_Ux[idx + nod] = wlVelosityX[idx + nod];
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
        public void CalkBoundaryVortex(double[] Phi, double[] Vortex, double[] wlVelosity, double w, 
                                        ref double[] bcVortexValue, int VortexBC_G2)
        {
            try
            {
                MEM.Alloc(boundaryAdress.Length, ref bcVortexValue, "bcVortexValue");
                
                switch(VortexBC_G2)
                {
                    case 0: // однородные ГУ для вихря
                        for (int i = 0; i < bcVortexValue.Length; i++)
                            bcVortexValue[i] = 0;
                        break;
                    case 1:
                        {
                            // дно
                            double[] bc = null;
                            CalkBedVortex(Phi, Vortex, w, ref bc);
                            for (int i = 0; i < bc.Length; i++)
                                bcVortexValue[i] = bc[i];
                            // однородные ГУ для вихря на WL
                            int idx = boundaryBedAdress.Length;
                            for (int i = idx; i < bc.Length; i++)
                                bcVortexValue[i] = 0;
                        }                            
                         break;
                    case 2:
                        {
                            double[] bc = null;
                            // дно / стенки
                            CalkBedVortex(Phi, Vortex, w, ref bc);
                            for (int i = 0; i < bc.Length; i++)
                                bcVortexValue[i] = bc[i];
                            // WL
                            CalkWaterLevelVortex(Phi, Vortex, wlVelosity, w, ref bc);
                            int idx = boundaryBedAdress.Length;
                            for (int i = 0; i < bc.Length; i++)
                                bcVortexValue[idx + i] = bc[i];

                        }
                        break;
                    case 3:
                        {
                            // дно
                            double[] bc = null;
                            for (int i = 0; i < boundaryBedAdress.Length; i++)
                                bcVortexValue[i] = 0;
                            // WL
                            CalkWaterLevelVortex(Phi, Vortex, wlVelosity, w, ref bc);
                            int idx = boundaryBedAdress.Length;
                            for (int i = 0; i < bc.Length; i++)
                                bcVortexValue[idx + i] = bc[i];

                        }
                        break;
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
        public void CalkBoundaryVortex_Plane(double[] Phi, double[] Vortex, double[] wlVelosity, double w, ref double[] bcVortexValue)
        {
            try
            {
                MEM.Alloc(boundaryAdress.Length, ref bcVortexValue, "bcVortexValue");
                double[] bc = null;
                CalkBedVortex(Phi, Vortex, w, ref bc);
                for (int i = 0; i < bc.Length; i++)
                    bcVortexValue[i] = bc[i];
                CalkWaterLevelVortex(Phi, Vortex, wlVelosity, w, ref bc);
                int idx = boundaryBedAdress.Length;
                for (int i = 0; i < bc.Length; i++)
                    bcVortexValue[idx + i] = bc[i];
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Вычисление функции динамической придонной скорости
        /// </summary>
        /// <param name="Ux"></param>
        /// <returns></returns>
        public void CalkBoundary_U_star(double[] Ux, ref double[] US)
        {
            IDigFunction fun = null;
            try
            {
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                TriElement[] AreaElems = mesh.GetAreaElems();
                double[] XY = null;
                double[] Us = null;
                MEM.Alloc(X.Length, ref US, "Us");
                MEM.Alloc(boundaryBedAdress.Length, ref Us, "US");
                MEM.Alloc(boundaryBedAdress.Length, ref XY, "XY");
                double[] XX = null;
                MEM.Alloc(boundaryBedAdress.Length, ref XX, "XX");
                HFForm2D_TriangleAnaliticL1 triff = new HFForm2D_TriangleAnaliticL1();
                // Расчет 
                for (uint nod = 1; nod < boundaryBedAdress.Length - shift; nod++)
                {
                    uint idx = boundaryBedAdress[nod];
                    IHPoint p = muBedPoint[nod];
                    int ne = muBedElems[nod];
                    if (ne > -1)
                    {
                        double z0 = 0.03 * SPhysics.PHYS.ks;
                        //double z0 = 0.001;
                        //double ks_b = SPhysics.PHYS.d50 / Hp;
                        // узлы
                        uint i0 = AreaElems[ne].Vertex1;
                        uint i1 = AreaElems[ne].Vertex2;
                        uint i2 = AreaElems[ne].Vertex3;
                        double[] x = { X[i0], X[i1], X[i2] };
                        double[] y = { Y[i0], Y[i1], Y[i2] };
                        triff.SetGeoCoords(x, y);
                        triff.CalkForm(p.X, p.Y);
                        double Ub = Ux[i0] * triff.N[0] + Ux[i1] * triff.N[1] + Ux[i2] * triff.N[2];
                        Us[nod] = SPhysics.kappa_w * Ub / Math.Log(muLengthBed[nod] / z0);
                        if (Us[nod] < 0)
                            Us[nod] = 0;
                        XY[nod] = MEM.Shift(X[idx], Y[nod]);
                        XX[nod] = X[idx];
                    }
                    else
                    {
                        Us[nod] = 0;
                        XY[nod] = MEM.Shift(X[idx], Y[idx]);
                        XX[nod] = X[idx];
                    }
                }
                if (channelSectionForms == SСhannelForms.boxCrossSection)
                {
                    XY[XY.Length - 1] = 2 * XY[XY.Length - 2] - XY[XY.Length - 3];
                    fun = new DigFunction(XY, Us, "Динамическая скорость");
                    for (uint k = 0; k < US.Length; k++)
                    {
                        double x = MEM.Shift(X[k], Y[k]);
                        double u_star = fun.FunctionValue(x);
                        US[k] = u_star;
                    }
                }
                else
                {
                    fun = new DigFunction(fx, Us, "Динамическая скорость");
                    for (uint k = 0; k < US.Length; k++)
                    {
                        double x = X[k];
                        double u_star = fun.FunctionValue(x);
                        US[k] = u_star;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// координаты донных узлов
        /// </summary>
        public void BedCoords(ref double[] y, ref double[] z)
        {
            y = fx; z = fy;
        }
    }
}
