////---------------------------------------------------------------------------
////              Реализация библиотеки для моделирования 
////               гидродинамических и русловых процессов
////                      - (C) Copyright 2021 -
////                       ALL RIGHT RESERVED
////                        ПРОЕКТ "BLLib"
////---------------------------------------------------------------------------
////    Метод контрольных объемов 1 порядка на 3/4 узловых КО
////                   разработка: Потапов И.И.
////                          27.07.22
////---------------------------------------------------------------------------
//namespace BLLib
//{
//    using CommonLib;
//    using MemLogLib;
//    using System;
//    using MeshLib;
//    using GeometryLib;
//    using System.Linq;
//    using AlgebraLib;
//    /// <summary>
//    /// ОО: Класс для решения плановой задачи о 
//    /// расчете донных деформаций русла на симплекс сетке
//    /// </summary>
//    [Serializable]
//    public class BedLoadTaskZeta2DFVM : BedLoadTask2DFVM
//    {
//        public override IBedLoadTask Clone()
//        {
//            return new BedLoadTaskZeta2DFVM(new BedLoadParams());
//        }
//        /// <summary>
//        /// Конструктор 
//        /// </summary>
//        public BedLoadTaskZeta2DFVM(BedLoadParams p) : base(p)
//        {
//            name = "плановая деформация дна MFV (NL).";
//        }
      
//        /// <summary>
//        /// Вычисление изменений формы донной поверхности 
//        /// на одном шаге по времени по модели 
//        /// Петрова А.Г. и Потапова И.И. 2014
//        /// Реализация решателя - методом контрольных объемов,
//        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
//        /// Коэффициенты донной подвижности, определяются 
//        /// как среднее гармонические величины         
//        /// </summary>
//        /// <param name="Zeta0">текущая форма дна</param>
//        /// <param name="tau">придонное касательное напряжение</param>
//        /// <param name="P">придонное давление</param>
//        /// <returns>новая форма дна</returns>
//        /// </summary>
//        public override void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY, double[] P = null)
//        {
//            try
//            {
//                this.P = P;
//                if (mesh == null) return;
//                MEM.VAlloc<double>(Zeta0.Length, 0, ref Zeta);
//                double bs = 1 / s;
//                // интерполяция 
//                // fvMesh.ConvertKnotsToElements(Zeta0, ref Zeta_elems);
//                #region  Расчет коэффциентов в узлах
//                for (int i = 0; i < mesh.CountKnots; i++)
//                {
//                    tau[i] = Math.Sqrt(tauX[i] * tauX[i] + tauY[i] * tauY[i]);
//                    mTx = tauX[i];
//                    mTy = tauY[i];
//                    if (tau[i] > MEM.Error6)
//                    {
//                        cosA = mTx / (tau[i] + MEM.Error10);
//                        sinA = mTy / (tau[i] + MEM.Error10);
//                    }
//                    else
//                    {
//                        cosA = 1; sinA = 0;
//                    }
//                    cos2 = cosA * cosA;
//                    sin2 = sinA * sinA;
//                    cs2 = sinA * cosA;
//                    double mtauS = mTx * cosA + mTy * sinA;
//                    mtau = tau[i];
//                    // здесь нужен переключатель
//                    if (tau0 > mtau)
//                    {
//                        chi = 1;
//                        A[i] = 0;
//                        B[i] = 0;
//                        C[i] = 0;
//                        D[i] = 0;
//                        // ПРОВЕРИТЬ!
//                        // требуется умножить на G0
//                        S_xx[i] = 0;
//                        S_xy[i] = 0;
//                        S_yx[i] = 0;
//                        S_yy[i] = 0;
//                        // ?
//                        // требуется разделить на s и умножить на G0
//                        H_xx[i] = 0;
//                        H_xy[i] = 0;
//                        H_yx[i] = 0;
//                        H_yy[i] = 0;

//                        Q_x[i] = 0;
//                        Q_y[i] = 0;
//                    }
//                    else
//                    {
//                        // 27.12.2021
//                        //G0 = rho_s * G1 * mtauS * Math.Sqrt(mtau) / CosGamma[i];
//                        // интерполировать с КЭ
//                        G0[i] = G1 * mtauS * Math.Sqrt(mtau); // CosGamma[i];
//                        chi = Math.Sqrt(tau0 / mtau);
//                        //double scale = 1.0;//Math.Min(0.1, 1 - dZx* dZx);
//                        A[i] = Math.Max(0, 1 - chi);
//                        B[i] = (chi / 2 + A[i]) / tanphi;
//                        C[i] = 0;
//                        D[i] = A[i] * 4.0 / 5.0 / tanphi;
//                        // ПРОВЕРИТЬ!
//                        // требуется умножить на G0
//                        S_xx[i] = D[i] * sin2 + B[i] * cos2;
//                        S_xy[i] = cs2 * (B[i] - D[i]);
//                        S_yx[i] = cs2 * (B[i] - D[i]);
//                        S_yy[i] = D[i] * cos2 + B[i] * sin2;
//                        // ?
//                        // требуется разделить на s и умножить на G0
//                        H_xx[i] = D[i] * sin2 + C[i] * cos2;
//                        H_xy[i] = cs2 * (C[i] - D[i]);
//                        H_yx[i] = cs2 * (C[i] - D[i]);
//                        H_yy[i] = D[i] * cos2 + C[i] * sin2;

//                        Q_x[i] = A[i] * cosA;
//                        Q_y[i] = A[i] * sinA;
//                    }
//                }
//                #endregion 
//                //double mm1 = tau.Max();
//                //double mm2 = tau.Min();
//                //double mm = mm1 - mm2;
//                // Расчет актуального шага по времени
//                double dtimeBuf = dtime;
//                double maxVolume = fvMesh.AreaElems.Max(x => x.Volume);
//                double maxMu = G0_elem.Max();
//                double dtimeCu = 0.5 * maxVolume / (2 * maxMu + MEM.Error5);
//                int Niters = (int)(dtime / (dtimeCu + MEM.Error10)) + 1;
//                dtime = dtime / Niters;
//                #region Поиск Zeta_elems на n+1 слое
//                // цикл по сходимости
//                for (int index = 0; index < Niters; index++)
//                {
//                    #region  Расчет уклонов на КО
//                    for (uint eID = 0; eID < mesh.CountElements; eID++)
//                    {
//                        // получить узлы КЭ
//                        TypeFunForm typeff = fvMesh.ElementKnots(eID, ref knots);
//                        if (FunFormHelp.CheckFF(typeff) == 0)

//                            pIntegration = IPointsA;
//                        else
//                            pIntegration = IPointsB;
//                        ff = FunFormsManager.CreateKernel(typeff);
//                        int cu = knots.Length;
//                        InitLocal(cu);
//                        // получить узлы КЭ
//                        fvMesh.ElementKnots(eID, ref knots);
//                        // координаты и площадь
//                        fvMesh.GetElemCoords(eID, ref x, ref y);
//                        // установка координат узлов
//                        ff.SetGeoCoords(x, y);
//                        // получение значений донных отметок на КЭ
//                        mesh.ElemValues(Zeta0, eID, ref zeta);
//                        mesh.ElemValues(tau, eID, ref Tau);
//                        // определение градиента давления по х,у
//                        if (blm != TypeBLModel.BLModel_1991 && P != null)
//                            fvMesh.ElemValues(P, eID, ref press);
//                        G0_elem[eID] = 0;
//                        dZetadX = 0;
//                        dZetadY = 0;
//                        // цикл по точкам интегрирования
//                        for (int pi = 0; pi < pIntegration.weight.Length; pi++)
//                        {
//                            //  ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
//                            ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
//                            double DWJ = ff.DetJ * pIntegration.weight[pi];
//                            for (int ai = 0; ai < cu; ai++)
//                            {
//                                dZetadX += ff.DN_x[ai] * zeta[ai];
//                                dZetadY += ff.DN_y[ai] * zeta[ai];
//                            }
//                        }
//                        double mtau = Tau.Sum() / cu;
//                        G0_elem[eID] = G1 * mtau * Math.Sqrt(mtau);

//                        mesh.ElemValues(Q_x, eID, ref Tau);
//                        Q_elems_x[eID] = Tau.Sum() / cu;
//                        mesh.ElemValues(Q_y, eID, ref Tau);
//                        Q_elems_y[eID] = Tau.Sum() / cu;

//                        double tan2 = dZetadX * dZetadX + dZetadY * dZetadY;
//                        TanGamma[eID] = Math.Sqrt(tan2);
//                        GammaX[eID] = dZetadX / tanphi;
//                        GammaY[eID] = dZetadY / tanphi;
//                        CosGamma[eID] = 1.0 / Math.Sqrt(1 + tan2);
//                    }
//                    #endregion
//                    BoundaryCondition();
//                    // цикл по КО
//                    for (int eID = 0; eID < fvMesh.CountElements; eID++)
//                    {
//                        double Qf;
//                        double AkZk, AP, APP, Ak, Zeta_k = 0;
//                        double SXX, SXY, SYX, SYY;
//                        //double G0elem, HEk, HFk;
//                        //double HXX, HXY, HYX, HYY;
//                        int p1, p2;
//                        double mL, mD;
//                        HPoint L, D;
//                        double G00, G0k, G0f, G0fs;
//                        double SEk, SFk;
//                        double Qxk, Qyk, Qx0, Qy0, Qxf, Qyf;
//                        double Qe;
//                        IFVElement fvElem = fvMesh.AreaElems[eID];

//                        G00 = G0_elem[eID];
//                        Qx0 = Q_elems_x[eID];
//                        Qy0 = Q_elems_y[eID];
//                        if (MEM.Equals(G00, 0) == true)
//                            continue;
//                        // просмотр граней
//                        Qe = 0;
//                        AkZk = 0;
//                        AP = 0;
//                        for (int nf = 0; nf < fvElem.Vertex.Length; nf++)
//                        {
//                            IFVFacet facet = fvElem.Facets[nf];

//                            p1 = facet.Pointid1;
//                            p2 = facet.Pointid2;
//                            // вектор ребра
//                            L = facet.FVertex;
//                            if (fvElem.Nodes[nf] == facet.Pointid2)
//                                L = -L;
//                            mL = facet.Length;

//                            SXX = 0.5 * (S_xx[p1] + S_xx[p2]);
//                            SXY = 0.5 * (S_xy[p1] + S_xy[p2]);
//                            SYX = 0.5 * (S_yx[p1] + S_yx[p2]);
//                            SYY = 0.5 * (S_yy[p1] + S_yy[p2]);

//                            #region
//                            //HXX = 0.5 * (H_xx[p1] + H_xx[p2]);
//                            //HXY = 0.5 * (H_xy[p1] + H_xy[p2]);
//                            //HYX = 0.5 * (H_yx[p1] + H_yx[p2]);
//                            //HYY = 0.5 * (H_yy[p1] + H_yy[p2]);
//                            #endregion
//                            IFVElement nb = fvElem.NearestElements[nf];
//                            if (nb == null)
//                            {
//                                D = fvElem.VecFacetsDistance[nf];
//                                mD = fvElem.NearFacetsDistance[nf];
//                                // !!!
//                                G0k = G0_elem[facet.Owner.Id];
//                                if (facet.BoundaryType == TypeBoundCond.Dirichlet)
//                                {
//                                    Zeta_k = Zeta_facets[facet.Id];
//                                }
//                                else
//                                {
//                                    if (facet.BoundaryType == TypeBoundCond.Neumann)
//                                        Zeta_k = Zeta_facets[facet.Id];
//                                }
//                                Qxk = 0.5 * (Q_x[p1] + Q_x[p2]);
//                                Qyk = 0.5 * (Q_y[p1] + Q_y[p2]);
//                            }
//                            else
//                            {
//                                D = fvElem.VecDistance[nf];
//                                mD = fvElem.Distance[nf];
//                                Zeta_k = Zeta_elems0[nb.Id];
//                                G0k = G0_elem[nb.Id];
//                                Qxk = Q_elems_x[nb.Id];
//                                Qyk = Q_elems_y[nb.Id];
//                            }

//                            // расчет коэффициента гравитационной диффузии на грани
//                            //G0f = (G00 * G0k) / ((1 - facet.Alpha) * G00 + facet.Alpha * G0k + MEM.Error10);
//                            //Qxf = (Qx0 * Qxk) / ((1 - facet.Alpha) * Qx0 + facet.Alpha * Qxk + MEM.Error10);
//                            //Qyf = (Qy0 * Qyk) / ((1 - facet.Alpha) * Qy0 + facet.Alpha * Qyk + MEM.Error10);
//                            G0f = G00 * (1 - facet.Alpha) + facet.Alpha * G0k;
//                            Qxf = Qx0 * (1 - facet.Alpha) + facet.Alpha * Qxk;
//                            Qyf = Qy0 * (1 - facet.Alpha) + facet.Alpha * Qyk;

//                            G0fs = G0f / s;

//                            SEk = SXX * L.y * L.y - (SXY + SYX) * L.x * L.y + SXX * L.x * L.x;

//                            SFk = SXX * D.y * L.y - SXY * D.x * L.y +
//                                         SYX * D.y * L.x + SYY * D.x * L.x;
//                            #region
//                            //HEk = HXX * L.y * L.y - (HXY + HYX) * L.x * L.y + HXX * L.x * L.x;

//                            //HFk = HXX * D.y * L.y - HXY * D.x * L.y +
//                            //             HYX * D.y * L.x + HYY * D.x * L.x;
//                            #endregion
//                            // учет транзитного расхода и укловнов
//                            SEk = G0f * SEk;
//                            SFk = G0f * SFk;

//                            Qf = G0f * (Qxf * L.y - Qyf * L.x);
//                            Qe += Qf;
//                            // Console.WriteLine("eID = " + eID.ToString() + "  Qf = " + Qf.ToString("F8") + "  Ly = " + L.y.ToString() + "  Qxk = " + Qxk.ToString("F8") + "  G0f = " + G0f.ToString("F8") + "  Lx = " + L.x.ToString() + "  Qyk = " + Qyk.ToString("F8"));
//                            // продольный коэффциент гравитационной диффузии на k -й грани
//                            Ak = SEk / (mD * mL);
//                            // центральный коэффициент гравитационной диффузии
//                            AP += Ak;
//                            // продольная гравитационная диффузия
//                            AkZk += Ak * Zeta_k;
//                            // поперечная диффузия
//                            AkZk += -SFk * (Zeta0[p2] - Zeta0[p1]);
//                        }
//                        // 
//                        Q_elems[fvElem.Id] = Qe;
//                        //Console.WriteLine("eID = " + eID.ToString() + "  Qe = " + Qe.ToString("F8"));
//                        // вычисление поправки 
//                        APP = fvElem.Volume / dtime + AP;
//                        Zeta_elems[fvElem.Id] = (AkZk + Zeta_elems0[fvElem.Id] * fvElem.Volume / dtime - Qe) / APP;
//                    }
//                    // Приведение к Zeta_elems => Zeta
//                    for (int j = 0; j < Zeta_elems.Length; j++)
//                        Zeta_elems0[j] = Zeta_elems[j];
//                    //FVMesh.ConvertElementsToKnots(Zeta_elems, ref Zeta, BoundaryValueDomain, true);
//                    fvMesh.ConvertElementsToKnots(Zeta_elems, ref Zeta);
//                    // Сглаживание дна по лавинной модели
//                    if (isAvalanche == AvalancheType.AvalancheQuad_2021)
//                        avalanche.Lavina(ref Zeta);
//                    for (int j = 0; j < Zeta.Length; j++)
//                        Zeta0[j] = Zeta[j];
//                    fvMesh.ConvertKnotsToElements(Zeta0, ref Zeta_elems0);

//                }
//                dtime = dtimeBuf;
//                #endregion

//                var iZ0 = IntZeta(Zeta0);
//                var iZ = IntZeta(Zeta);
//                double errorZ = 100 * (iZ0.int_Z - iZ.int_Z) / (iZ0.int_Z + MEM.Error10);
//                double errorZL2 = 100 * (iZ0.int_Z2 - iZ.int_Z2) / (iZ0.int_Z2 + MEM.Error10);
//                // Сглаживание дна по лавинной модели
//                if (isAvalanche == AvalancheType.AvalancheQuad_2021)
//                    avalanche.Lavina(ref Zeta);
//                #region Контроль баланса массы за 1 шаг по времени
//                //var aiZ = IntZeta(Zeta);
//                //double a_errorZ = 100 * (aiZ.int_Z - iZ0.int_Z) / (iZ0.int_Z + MEM.Error10);
//                //Logger.Instance.AddHeaderInfo("Integral mass balance control");
//                //Logger.Instance.AddHeaderInfo("Интегральный контроль баланса массы");
//                //string str = " int (Zeta0) = " + iZ0.int_Z.ToString() +
//                //             " int (Zeta)= " + iZ.int_Z.ToString();
//                //Logger.Instance.AddHeaderInfo(str);
//                //str = " errorZ = " + errorZ.ToString("F6") + " %" +
//                //      " errorZL2 = " + errorZL2.ToString("F6") + " %";
//                //Logger.Instance.AddHeaderInfo(str);
//                //str = " a_int (1) = S = " + aiZ.Area.ToString("") +
//                //      " a_int (Zeta) = " + aiZ.int_Z.ToString("") +
//                //      " a_errorZ = " + a_errorZ.ToString("F6") + " %";
//                //Logger.Instance.AddHeaderInfo(str);
//                //Logger.Instance.AddHeaderInfo("min Zeta = " + Zeta.Min() + "  max Zeta = " + Zeta.Max());
//                #endregion
//                // переопределение начального значения zeta 
//                // для следующего шага по времени


//                //fvMesh.ConvertElementsToKnots(Zeta_elems, ref Zeta);
//                //for (int j = 0; j < Zeta.Length; j++)
//                //    Zeta0[j] = Zeta[j];
//            }
//            catch (Exception e)
//            {
//                Logger.Instance.Exception(e);
//                for (int j = 0; j < Zeta_elems.Length; j++)
//                    Zeta_elems[j] = Zeta_elems0[j];
//            }
//        }

//    }
//}