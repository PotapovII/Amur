//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//    Метод контрольных объемов 1 порядка на 3/4 узловых КО
//                   разработка: Потапов И.И.
//---------------------------------------------------------------------------
//                          27.07.22
//---------------------------------------------------------------------------
namespace BLLib
{
    using CommonLib;
    using MemLogLib;
    using System;
    using MeshLib;
    using GeometryLib;
    using System.Linq;
    using AlgebraLib;
    using CommonLib.Physics;
    using CommonLib.Geometry;

    /// <summary>
    /// ОО: Класс для решения плановой задачи о 
    /// расчете донных деформаций русла на симплекс сетке
    /// </summary>
    [Serializable]
    public class BedLoadTask2DFVM : ABedLoadFEMTask2D
    {
        public override IBedLoadTask Clone()
        {
            return new BedLoadTask2DFVM(new BedLoadParams());
        }
        /// <summary>
        /// Поток наносов по Х в улах
        /// </summary>
        protected double[] Gx;
        /// <summary>
        /// Поток наносов по У в улах
        /// </summary>
        protected double[] Gy;
        /// <summary>
        /// Источник от транзитного расхода в узлах
        /// </summary>
        protected double[] Q = null;
        /// <summary>
        /// Модуль вектора касательных напряжений в узлах
        /// </summary>
        public double[] Tau = null;
        protected double[] G0_elem = null;
        
        /// <summary>
        /// локальные переменные
        /// </summary>
        protected double MatrixK, MatrixM;
        protected double dZetadX, dZetadY, dPX, dPY;
        protected double cosA, sinA;
        protected double cos2, sin2, cs2;
        protected double ss;
        #region FV
        /// <summary>
        ///  Значение функции на конечном объеме
        /// </summary>
        public double[] Zeta_elems0 = null;
        /// <summary>
        ///  Значение функции на конечном объеме
        /// </summary>
        public double[] Zeta_elems = null;
        /// <summary>
        ///  Значение функции гранях области
        /// </summary>
        public double[] Zeta_facets = null;
        /// <summary>
        /// Источник на конечном объеме
        /// </summary>
        public double[] Q_elems = null;
        public double[] Q_elems_x = null;
        public double[] Q_elems_y = null;


        #region Переменные для работы с КЭ аналогом
        /// <summary>
        /// Придонное давление в узлах КЭ
        /// </summary>
        protected double[] press;
        /// <summary>
        /// Отметки дна в узлах КЭ
        /// </summary>
        protected double[] zeta;
        protected double mTx, mTy;
        #endregion
        /// <summary>
        /// Компоненты напорного тензоров в опорных узлах конечного объема
        /// </summary>
        public double[] H_xx = null;
        public double[] H_xy = null;
        public double[] H_yx = null;
        public double[] H_yy = null;
        /// <summary>
        /// Компоненты гравитационно-диффузионного тензора в опорных узлах конечного объема
        /// </summary>
        public double[] S_xx = null;
        public double[] S_xy = null;
        public double[] S_yx = null;
        public double[] S_yy = null;

        public double[] Q_x = null;
        public double[] Q_y = null;
        public double[] G0 = null;

        public double[] CosGammaKnot = null;
        protected double[] GammaXKnot = null;
        protected double[] GammaYKnot = null;
        /// <summary>
        /// Функции формы
        /// </summary>
        protected AbFunForm ff = null;
        /// <summary>
        /// Квадратурные точки для численного интегрирования
        /// </summary>
        protected NumInegrationPoints pIntegration;
        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
        /// <summary>
        /// КО сетка
        /// </summary>
        public IFVComMesh FVMesh => fvMesh;
        protected IFVComMesh fvMesh;
        #endregion
        /// <summary>
        /// Конструктор 
        /// </summary>
        public BedLoadTask2DFVM(BedLoadParams p) : base(p)
        {
            name = "плановая деформация дна MFV (NL).";
        }
        /// <summary>
        /// Установка исходных данных
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="algebra">Решатель для СЛАУ </param>
        /// <param name="BCBed">граничные условия</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        /// <param name="theta">Параметр схемы по времени</param>
        /// <param name="dtime">шаг по времени</param>
        /// <param name="isAvalanche">флаг использования лавинной модели</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, IBoundaryConditions BConditions)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            try
            {
                taskReady = false;
                if (mesh == null)
                {
                    Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                    return;
                }
                if (mesh.CountKnots == 0)
                {
                    Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                    return;
                }
                fvMesh = new FVComMesh(mesh);
                fvMesh.ConvertKnotsToElements(Zeta0, ref Zeta_elems0);

                MEM.Alloc(fvMesh.CountElements, ref Zeta_elems);
                MEM.Alloc(fvMesh.CountElements, ref Zeta_elems);
                MEM.Alloc(fvMesh.CountElements, ref Q_elems);
                MEM.Alloc(fvMesh.CountElements, ref Q_elems_x);
                MEM.Alloc(fvMesh.CountElements, ref Q_elems_y);
                MEM.Alloc(fvMesh.CountElements, ref G0_elem);

                MEM.Alloc<double>(fvMesh.CountKnots, ref S_xx);
                MEM.Alloc<double>(fvMesh.CountKnots, ref S_xy);
                MEM.Alloc<double>(fvMesh.CountKnots, ref S_yx);
                MEM.Alloc<double>(fvMesh.CountKnots, ref S_yy);

                MEM.Alloc<double>(fvMesh.CountKnots, ref H_xx);
                MEM.Alloc<double>(fvMesh.CountKnots, ref H_xy);
                MEM.Alloc<double>(fvMesh.CountKnots, ref H_yx);
                MEM.Alloc<double>(fvMesh.CountKnots, ref H_yy);

                MEM.Alloc<double>(fvMesh.CountKnots, ref Q_x);
                MEM.Alloc<double>(fvMesh.CountKnots, ref Q_y);
                MEM.Alloc<double>(fvMesh.CountKnots, ref G0);
                MEM.Alloc<double>(fvMesh.CountKnots, ref Q);

                MEM.Alloc<double>(fvMesh.CountKnots, ref CosGammaKnot);
                MEM.Alloc<double>(fvMesh.CountKnots, ref GammaXKnot);
                MEM.Alloc<double>(fvMesh.CountKnots, ref GammaYKnot);

                MEM.Alloc<double>(fvMesh.CountKnots, ref Q);

                // Вычисление коеффициентов
                for (int eID = 0; eID < fvMesh.CountElements; eID++)
                    fvMesh.AreaElems[eID].InitElement();

                #region подавление base.SetTask ABedLoadFEMTask2D (дабы не выделять память дважды)
                BaseSetTask(mesh, Zeta0, BConditions);

                algebra = new SparseAlgebraCG((uint)mesh.CountKnots);
                Ralgebra = algebra.Clone();
                int Count = mesh.CountElements;
                // на элементах
                MEM.Alloc<double>(Count, ref CosGamma);
                MEM.Alloc<double>(Count, ref GammaX);
                MEM.Alloc<double>(Count, ref GammaY);
                MEM.Alloc<double>(Count, ref TanGamma);
                // в узлах
                MEM.Alloc<double>(mesh.CountKnots, ref A);
                MEM.Alloc<double>(mesh.CountKnots, ref B);
                MEM.Alloc<double>(mesh.CountKnots, ref C);
                MEM.Alloc<double>(mesh.CountKnots, ref D);
                MEM.Alloc<double>(mesh.CountKnots, ref tau);
                MEM.Alloc<double>(mesh.CountKnots, ref ps);
                avalanche = new Avalanche2DX(mesh, tanphi, DirectAvalanche.AvalancheXY, 0.3);
                #endregion

                MEM.Alloc<int>(mesh.CountKnots, ref DryWet);
                MEM.Alloc<double>(mesh.CountKnots, ref Gx);
                MEM.Alloc<double>(mesh.CountKnots, ref Gy);
                MEM.Alloc<double>(mesh.CountKnots, ref Tau);
                MEM.Alloc(fvMesh.Facets.Length, ref Zeta_facets);

                IPointsA.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_L1);
                IPointsB.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_L1);

                avalanche = new Avalanche2DFV(fvMesh, tanphi);
                taskReady = true;
            }
            catch (Exception exp)
            {
                Logger.Instance.Exception(exp);
                taskReady = false;
            }
        }
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        public override void InitLocal(int cu, int cs = 1)
        {
            MEM.Alloc<double>(cu, ref x);
            MEM.Alloc<double>(cu, ref y);
            MEM.Alloc<uint>(cu, ref knots);
            MEM.Alloc<double>(cu, ref zeta);
            MEM.Alloc<double>(cu, ref Tau);
        }

        public double mValue(uint fvElem, double[] Value)
        {
            mesh.ElemValues(Value, fvElem, ref Tau);
            return Tau.Sum() / Tau.Length;
        }

        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta0">текущая форма дна</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        /// <returns>новая форма дна</returns>
        /// </summary>
        public override void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY, double[] P = null, double[][] CS = null)
        {
            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double rho_b = SPhysics.PHYS.rho_b;
            double G1 = SPhysics.PHYS.G1;
            double epsilon = SPhysics.PHYS.epsilon;
            double tau0 = SPhysics.PHYS.tau0;
            //double g = SPhysics.GRAV;
            double kappa = SPhysics.PHYS.kappa;
            //double rho_w = SPhysics.rho_w;
            double Fa0 = SPhysics.PHYS.Fa0;
            double rho_s = SPhysics.PHYS.rho_s;
            double gamma = SPhysics.PHYS.gamma;
            double RaC = SPhysics.PHYS.RaC;
            double Ws = SPhysics.PHYS.Ws;
            double d50 = SPhysics.PHYS.d50;
            double nu = SPhysics.nu;

            try
            {
                this.P = P;
                if (mesh == null) return;
                MEM.VAlloc<double>(Zeta0.Length, 0, ref Zeta);
                // расчет модуля придонных напряжений
                for (int i = 0; i < mesh.CountKnots; i++)
                    tau[i] = Math.Sqrt(tauX[i] * tauX[i] + tauY[i] * tauY[i]);
                // Расчет актуального шага по времени
                double dtimeBuf = dtime;
                double maxVolume = fvMesh.AreaElems.Max(x => x.Volume);
                double maxTau = tau.Max();
                double maxMu = G1 * maxTau * Math.Sqrt(maxTau);
                double dtimeCu = 0.5 * maxVolume / (2 * maxMu + MEM.Error5);
                int Niters = (int)(dtime / (dtimeCu + MEM.Error10)) + 1;
                dtime = dtime / Niters;
                #region Поиск Zeta_elems на n+1 слое
                // цикл по времени с требуемым шагом
                for (int index = 0; index < Niters; index++)
                {
                    #region  Расчет уклонов на КО
                    for (uint eID = 0; eID < mesh.CountElements; eID++)
                    {
                        // получить узлы КЭ
                        TypeFunForm typeff = fvMesh.ElementKnots(eID, ref knots);
                        int cu = knots.Length;
                        InitLocal(cu);
                        // получить узлы КЭ
                        fvMesh.ElementKnots(eID, ref knots);
                        // координаты и площадь
                        fvMesh.GetElemCoords(eID, ref x, ref y);
                        // получение значений донных отметок на КЭ
                        mesh.ElemValues(Zeta0, eID, ref zeta);

                        // определение градиента давления по х,у
                        if (blm != TypeBLModel.BLModel_1991 && P != null)
                            fvMesh.ElemValues(P, eID, ref press);
                        if (FunFormHelp.CheckFF(typeff) == 0) // трехугольные КЭ
                            pIntegration = IPointsA;
                        else // четырехугольные КЭ
                            pIntegration = IPointsB;
                        // получить функцию формы
                        ff = FunFormsManager.CreateKernel(typeff);
                        // установка координат узлов
                        ff.SetGeoCoords(x, y);
                        // цикл по точкам интегрирования
                        dZetadX = 0;
                        dZetadY = 0;
                        for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                        {
                            ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                            // double DWJ = ff.DetJ * pIntegration.weight[pi];
                            for (int ai = 0; ai < cu; ai++)
                            {
                                dZetadX += ff.DN_x[ai] * zeta[ai];
                                dZetadY += ff.DN_y[ai] * zeta[ai];
                            }
                        }
                        double tan2 = dZetadX * dZetadX + dZetadY * dZetadY;

                        mesh.ElemValues(tau, eID, ref Tau);
                        double mtau = Tau.Sum() / cu;
                        G0_elem[eID] = G1 * mtau * Math.Sqrt(mtau);

                        mesh.ElemValues(Q_x, eID, ref Tau);
                        Q_elems_x[eID] = Tau.Sum() / cu;

                        mesh.ElemValues(Q_y, eID, ref Tau);
                        Q_elems_y[eID] = Tau.Sum() / cu;

                        TanGamma[eID] = Math.Sqrt(tan2);
                        CosGamma[eID] = 1.0 / Math.Sqrt(1 + tan2);
                        GammaX[eID] = dZetadX / tanphi;
                        GammaY[eID] = dZetadY / tanphi;

                    }
                    #endregion
                    // 
                    fvMesh.ConvertElementsToKnots(CosGamma, ref CosGammaKnot);
                    fvMesh.ConvertElementsToKnots(GammaX, ref GammaXKnot);
                    fvMesh.ConvertElementsToKnots(GammaY, ref GammaYKnot);

                    #region  Расчет коэффциентов в узлах
                    for (int i = 0; i < mesh.CountKnots; i++)
                    {
                        mTx = tauX[i];
                        mTy = tauY[i];
                        if (tau[i] > MEM.Error6)
                        {
                            cosA = mTx / (tau[i] + MEM.Error10);
                            sinA = mTy / (tau[i] + MEM.Error10);
                        }
                        else
                        {
                            cosA = 1; sinA = 0;
                        }
                        cos2 = cosA * cosA;
                        sin2 = sinA * sinA;
                        cs2 = sinA * cosA;
                        double mtauS = mTx * cosA + mTy * sinA;
                        mtau = tau[i];
                        // здесь нужен переключатель
                        if (tau0 > mtau)
                        {
                            chi = 1;
                            A[i] = 0;
                            B[i] = 0;
                            C[i] = 0;
                            D[i] = 0;
                            // ПРОВЕРИТЬ!
                            // требуется умножить на G0
                            S_xx[i] = 0;
                            S_xy[i] = 0;
                            S_yx[i] = 0;
                            S_yy[i] = 0;
                            // ?
                            // требуется разделить на s и умножить на G0
                            H_xx[i] = 0;
                            H_xy[i] = 0;
                            H_yx[i] = 0;
                            H_yy[i] = 0;

                            Q_x[i] = 0;
                            Q_y[i] = 0;
                        }
                        else
                        {
                            // интерполировать с КЭ
                            double scale = DRate(GammaXKnot[i], GammaYKnot[i]);
                            // учет уклонов                            
                            G0[i] = scale * G1 * mtauS * Math.Sqrt(mtau) / CosGammaKnot[i];
                            chi = Math.Sqrt(tau0 / mtau);

                            A[i] = Math.Max(0, 1 - chi);
                            B[i] = (chi / 2 + A[i]) / tanphi;
                            C[i] = 0;
                            D[i] = A[i] * 4.0 / 5.0 / tanphi;
                            // ПРОВЕРИТЬ!
                            // требуется умножить на G0
                            S_xx[i] = D[i] * sin2 + B[i] * cos2;
                            S_xy[i] = cs2 * (B[i] - D[i]);
                            S_yx[i] = cs2 * (B[i] - D[i]);
                            S_yy[i] = D[i] * cos2 + B[i] * sin2;
                            // ?
                            // требуется разделить на s и умножить на G0
                            H_xx[i] = D[i] * sin2 + C[i] * cos2;
                            H_xy[i] = cs2 * (C[i] - D[i]);
                            H_yx[i] = cs2 * (C[i] - D[i]);
                            H_yy[i] = D[i] * cos2 + C[i] * sin2;

                            Q_x[i] = A[i] * cosA;
                            Q_y[i] = A[i] * sinA;
                        }
                    }
                    #endregion

                    BoundaryCondition();
                    // цикл по КО
                    for (int eID = 0; eID < fvMesh.CountElements; eID++)
                    {
                        double Qf;
                        double AkZk, AP, APP, Ak, Zeta_k = 0;
                        double SXX, SXY, SYX, SYY;
                        //double G0elem, HEk, HFk;
                        //double HXX, HXY, HYX, HYY;
                        int p1, p2;
                        double mL, mD;
                        HPoint L, D;
                        double G00, G0k, G0fs;
                        double SEk, SFk;
                        double Qe = 0, Qxk, Qyk, Qx0, Qy0;
                        double G0f, Qxf, Qyf;
                        //double G01f, Qx1f, Qy1f;

                        AkZk = 0;
                        AP = 0;
                        IFVElement fvElem = fvMesh.AreaElems[eID];
                        G00 = G0_elem[eID];
                        Qx0 = Q_elems_x[eID];
                        Qy0 = Q_elems_y[eID];
                        if (MEM.Equals(G00, 0) == true)
                            continue;
                        // просмотр граней
                        for (int nf = 0; nf < fvElem.Vertex.Length; nf++)
                        {
                            IFVFacet facet = fvElem.Facets[nf];
                            p1 = facet.Pointid1;
                            p2 = facet.Pointid2;
                            // вектор ребра
                            L = facet.FVertex;
                            if (fvElem.Nodes[nf] == facet.Pointid2)
                                L = -L;
                            mL = facet.Length;
                            // значение компонент тензора сыпучести на ребре
                            SXX = 0.5 * (S_xx[p1] + S_xx[p2]);
                            SXY = 0.5 * (S_xy[p1] + S_xy[p2]);
                            SYX = 0.5 * (S_yx[p1] + S_yx[p2]);
                            SYY = 0.5 * (S_yy[p1] + S_yy[p2]);
                            #region
                            //HXX = 0.5 * (H_xx[p1] + H_xx[p2]);
                            //HXY = 0.5 * (H_xy[p1] + H_xy[p2]);
                            //HYX = 0.5 * (H_yx[p1] + H_yx[p2]);
                            //HYY = 0.5 * (H_yy[p1] + H_yy[p2]);
                            #endregion
                            // внешний КО сопряженный ребром
                            IFVElement nb = fvElem.NearestElements[nf];
                            if (nb == null || facet.BoundaryFacetsMark != -1) //TypeBoundCond.ImpossibleBC)
                            {
                                // внешняя граница области
                                D = fvElem.VecFacetsDistance[nf];
                                mD = fvElem.NearFacetsDistance[nf];
                                G0k = G0_elem[facet.Owner.Id];
                                Zeta_k = Zeta_facets[facet.Id];
                                Qxk = 0.5 * (Q_x[p1] + Q_x[p2]);
                                Qyk = 0.5 * (Q_y[p1] + Q_y[p2]);
                            }
                            else
                            {
                                // область
                                D = fvElem.VecDistance[nf];
                                mD = fvElem.Distance[nf];
                                Zeta_k = Zeta_elems0[nb.Id];
                                G0k = G0_elem[nb.Id];
                                Qxk = Q_elems_x[nb.Id];
                                Qyk = Q_elems_y[nb.Id];
                            }
                            // расчет коэффициента гравитационной диффузии на и потоков грани
                            //G0f = (G00 * G0k) / ((1 - facet.alpha) * G00 + facet.alpha * G0k + MEM.Error10);
                            //Qxf = (Qx0 * Qxk) / ((1 - facet.alpha) * Qx0 + facet.alpha * Qxk + MEM.Error10);
                            //Qyf = (Qy0 * Qyk) / ((1 - facet.alpha) * Qy0 + facet.alpha * Qyk + MEM.Error10);
                            G0f = G00 * (1 - facet.Alpha) + facet.Alpha * G0k;
                            Qxf = Qx0 * (1 - facet.Alpha) + facet.Alpha * Qxk;
                            Qyf = Qy0 * (1 - facet.Alpha) + facet.Alpha * Qyk;
                            G0fs = G0f / s;
                            SEk = SXX * L.y * L.y - (SXY + SYX) * L.x * L.y + SXX * L.x * L.x;
                            SFk = SXX * D.y * L.y - SXY * D.x * L.y +
                                  SYX * D.y * L.x + SYY * D.x * L.x;
                            #region
                            //HEk = HXX * L.y * L.y - (HXY + HYX) * L.x * L.y + HXX * L.x * L.x;

                            //HFk = HXX * D.y * L.y - HXY * D.x * L.y +
                            //             HYX * D.y * L.x + HYY * D.x * L.x;
                            #endregion
                            // учет транзитного расхода и укловнов
                            SEk = G0f * SEk;
                            SFk = G0f * SFk;
                            Qf = G0f * (Qxf * L.y - Qyf * L.x);
                            Qe += Qf;
                            // продольный коэффциент гравитационной диффузии на k -й грани
                            Ak = SEk / (mD * mL);
                            // центральный коэффициент гравитационной диффузии
                            AP += Ak;
                            // продольная гравитационная диффузия
                            AkZk += Ak * Zeta_k;
                            // поперечная диффузия
                            AkZk += -SFk * (Zeta0[p2] - Zeta0[p1]);
                        }
                        Q_elems[fvElem.Id] = Qe;
                        APP = fvElem.Volume / dtime + AP;
                        // вычисление поправки 
                        Zeta_elems[fvElem.Id] = (AkZk + Zeta_elems0[fvElem.Id] * fvElem.Volume / dtime - Qe) / APP;
                    }
                    // Сглаживание дна по лавинной модели
                    if (isAvalanche == AvalancheType.AvalancheSimple)
                        avalanche.Lavina(ref Zeta_elems);
                    //FVMesh.ConvertElementsToKnots(Zeta_elems, ref Zeta, BoundaryValueDomain, true);
                    fvMesh.ConvertElementsToKnots(Zeta_elems, ref Zeta);
                    // Приведение к Zeta_elems => Zeta_elems0 - сдвиг слоя по времени
                    for (int j = 0; j < Zeta_elems.Length; j++)
                        Zeta_elems0[j] = Zeta_elems[j];
                }
                dtime = dtimeBuf;
                #endregion
                //var iZ0 = IntZeta(Zeta0);
                //var iZ = IntZeta(Zeta);
                //double errorZ = 100 * (iZ0.int_Z - iZ.int_Z) / (iZ0.int_Z + MEM.Error10);
                //double errorZL2 = 100 * (iZ0.int_Z2 - iZ.int_Z2) / (iZ0.int_Z2 + MEM.Error10);
                // Сглаживание дна по лавинной модели
                //if (isAvalanche == AvalancheType.AvalancheQuad_2021)
                //    avalanche.Lavina(ref Zeta);
                #region Контроль баланса массы за 1 шаг по времени
                //var aiZ = IntZeta(Zeta);
                //double a_errorZ = 100 * (aiZ.int_Z - iZ0.int_Z) / (iZ0.int_Z + MEM.Error10);
                //Logger.Instance.AddHeaderInfo("Integral mass balance control");
                //Logger.Instance.AddHeaderInfo("Интегральный контроль баланса массы");
                //string str = " int (Zeta0) = " + iZ0.int_Z.ToString() +
                //             " int (Zeta)= " + iZ.int_Z.ToString();
                //Logger.Instance.AddHeaderInfo(str);
                //str = " errorZ = " + errorZ.ToString("F6") + " %" +
                //      " errorZL2 = " + errorZL2.ToString("F6") + " %";
                //Logger.Instance.AddHeaderInfo(str);
                //str = " a_int (1) = S = " + aiZ.Area.ToString("") +
                //      " a_int (Zeta) = " + aiZ.int_Z.ToString("") +
                //      " a_errorZ = " + a_errorZ.ToString("F6") + " %";
                //Logger.Instance.AddHeaderInfo(str);
                //Logger.Instance.AddHeaderInfo("min Zeta = " + Zeta.Min() + "  max Zeta = " + Zeta.Max());
                #endregion
                // переопределение начального значения zeta 
                // для следующего шага по времени
                //fvMesh.ConvertElementsToKnots(Zeta_elems, ref Zeta);
                //for (int j = 0; j < Zeta.Length; j++)
                //    Zeta0[j] = Zeta[j];
            }
            catch (Exception e)
            {
                Logger.Instance.Exception(e);
                for (int j = 0; j < Zeta_elems.Length; j++)
                    Zeta_elems[j] = Zeta_elems0[j];
            }
        }



        public void BoundaryCondition()
        {
            int f = 0;
            int elem_id;
            double[] BCDir = BConditions.GetValueDir();
            double[] BCNeu = BConditions.GetValueNeu();
            try
            {
                for (f = 0; f < fvMesh.BoundaryFacets.Length; f++)
                {
                    IFVFacet facet = fvMesh.BoundaryFacets[f];
                    switch (facet.BoundaryFacetsMark)
                    {
                        case 0:// TypeBoundCond.Dirichlet
                            Zeta_facets[facet.Id] = BCDir[facet.BoundaryFacetsMark];
                            break;
                        case 1: // TypeBoundCond.Neumann:
                            if (facet.Owner != null)
                            {
                                elem_id = facet.Owner.Id;
                                double SXX, SXY, SYX, SYY, SZZ, Snb;
                                double Qe, Qxk, Qyk;
                                double G0k;

                                int p1 = facet.Pointid1;
                                int p2 = facet.Pointid2;
                                // вектор ребра
                                HPoint L = facet.FVertex;
                                int nf = facet.Owner.GetFacetIndex(facet);
                                if (facet.Owner.Nodes[nf] == facet.Pointid2)
                                    L = -L;
                                
                                //HPoint D = facet.Owner.VecFacetsDistance[nf];
                                //double d = D.Length();
                                //double d1 = facet.Owner.NearFacetsDistance[nf];
                                double d = facet.Owner.Distance[nf];
                                double mL = facet.Length;
                                SXX = 0.5 * (S_xx[p1] + S_xx[p2]);
                                SXY = 0.5 * (S_xy[p1] + S_xy[p2]);
                                SYX = 0.5 * (S_yx[p1] + S_yx[p2]);
                                SYY = 0.5 * (S_yy[p1] + S_yy[p2]);

                                Qxk = 0.5 * (Q_x[p1] + Q_x[p2]);
                                Qyk = 0.5 * (Q_y[p1] + Q_y[p2]);

                                //Qxk = Q_elems_x[elem_id];
                                //Qyk = Q_elems_y[elem_id];

                                Snb = SXX * L[1] * L[1] - (SXY + SYX) * L[0] * L[1] + SYY * L[0] * L[0];

                                G0k = ( 0.5 * (G0[p1] + G0[p2]) + G0_elem[elem_id] ) *0.5;
                                // транзитный расход
                                Qe = (Qxk * L[1] - Qyk * L[0]) * G0k;
                                // "гравитационная" вязкость
                                SZZ = G0k * Snb + MEM.Error10;
                                SZZ = G0k * Snb + MEM.Error12;
                                // сдвиг функции из цента КО на стенку (однородные ГУ)
                                double bcNeu = BCNeu[facet.BoundaryFacetsMark];
                                double dZeta = mL*d * (Qe - bcNeu) / SZZ;
                                Zeta_facets[facet.Id] = Zeta_elems[elem_id] + dZeta;
                            }
                            break;
                        case 2: // TypeBoundCond.Neumann0:
                            if (facet.Owner != null)
                            {
                                // снос значения из центра КО на границу 
                                // 1 порядок точности
                                Zeta_facets[facet.Id] =  Zeta_elems[facet.Owner.Id];
                            }
                            break;
                        case 3: //TypeBoundCond.Transit:
                            if (facet.Owner != null)
                            {
                                elem_id = facet.Owner.Id;
                                Zeta_facets[facet.Id] = Zeta_elems[elem_id];
                            }
                            break;
                        case 4: // TypeBoundCond.Periodic:
                            if (facet.Owner != null)
                            {
                                int periodicID = GetPeriodic(facet.Id);
                                Zeta_facets[facet.Id] = Zeta_facets[periodicID];
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// ПРОКСИ - сопряжения
        /// для переодических границ нужно построить словарь соответсвия граней
        /// и по ключу фасетки находить сопряженную фасетку
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual int GetPeriodic(int key)
        {
            int value = key;
            return value;
        }
        /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public override void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null) { }
        /// <summary>
        /// Модель дна: расчет коэффициентов A B C
        /// </summary>
        public override void CalkCoeff(uint fvElem, double mtauS, double dZx, double dZy) { }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                sp.Add("Отметки дна", Zeta);
                double[] Qp = null;
                if (G0_elem != null)
                {
                    fvMesh.ConvertElementsToKnots(G0_elem, ref Qp);
                    sp.Add("Транзитный множитель", Qp);
                }
                if (Q_elems != null)
                {
                    fvMesh.ConvertElementsToKnots(Q_elems, ref Qp);
                    sp.Add("Транзитный поток", Qp);
                }
                
                sp.Add("A", A);
                sp.Add("B", B);
                sp.Add("C", C);
                sp.Add("D", D);
            }
        }
    }
}