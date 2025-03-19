//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          27.02.21
//---------------------------------------------------------------------------
//                  добавлен контроль потери массы
//                          27.03.22
//---------------------------------------------------------------------------
namespace BLLib
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Класс для решения плановой задачи о 
    /// расчете донных деформаций русла на симплекс сетке
    /// </summary>
    [Serializable]
    public class CBedLoadFEMTask2D : ABedLoadFEMTask2D
    {
        public override IBedLoadTask Clone()
        {
            return new CBedLoadFEMTask2D(new BedLoadParams());
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
        /// Площадь КО связанного с узлом
        /// </summary>
        protected double[] Sknot;
        /// <summary>
        /// Поток наносов по Х на элементе
        /// </summary>
        protected double[] GxE;
        /// <summary>
        /// Поток наносов по У на элементе
        /// </summary>
        protected double[] GyE;
        /// <summary>
        /// Плошадь КЭ
        /// </summary>
        protected double S = 0;
        /// <summary>
        /// Касательные напряжения по Х в узлах КЭ
        /// </summary>
        protected double[] Tx;
        /// <summary>
        /// Касательные напряжения по У в узлах КЭ
        /// </summary>
        protected double[] Ty;
        /// <summary>
        /// Придонное давление в узлах КЭ
        /// </summary>
        protected double[] press;
        /// <summary>
        /// Отметки дна в узлах КЭ
        /// </summary>
        protected double[] zeta;
        /// <summary>
        /// Матрица донной подвижности для  КЭ
        /// </summary>
        protected double[,] BB = new double[2, 2];
        /// <summary>
        /// Матрица давления для  КЭ
        /// </summary>
        protected double[,] H = new double[2, 2];
        /// <summary>
        /// Матрица масс для КЭ
        /// </summary>
        protected double[,] MM;
        /// <summary>
        /// Модуль вектора касательных напряжений в узлах
        /// </summary>
        public double[] Tau = null;
        /// <summary>
        /// Индексы "сухих" узлов
        /// </summary>
        protected uint[] DryWetNumbers;
        /// <summary>
        /// Отметки дна в "сухих" узлах
        /// </summary>
        protected double[] DryWetZeta;
        /// <summary>
        /// локальные переменные
        /// </summary>
        protected double G0, MatrixK, MatrixM;
        protected double dZetadX, dZetadY, dPX, dPY;
        protected double cosA, sinA;
        protected double cos2, sin2, cs2;
        protected double ss;
        protected double pA, pB, pC, pD, pCosGamma;
        protected double mTx;
        protected double mTy;
        protected double dZx;
        protected double dZy;
        protected double tan2;
        protected double mtauS;
        /// <summary>
        /// Функции формы
        /// </summary>
        AbFunForm ff = null;
        /// <summary>
        /// Квадратурные точки для численного интегрирования
        /// </summary>
        protected NumInegrationPoints pIntegration;
        protected NumInegrationPoints IPointsA = new NumInegrationPoints();
        protected NumInegrationPoints IPointsB = new NumInegrationPoints();
        /// <summary>
        /// Конструктор 
        /// </summary>
        public CBedLoadFEMTask2D(BedLoadParams p) : base(p)
        {
            double s = SPhysics.PHYS.s;
            ss = (1 + s) / s;
            name = "плановая деформация одно-фракционного дна.";
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
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions = null)
        {
            taskReady = false;

            if (mesh == null)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                return;
            }
                
            taskReady = false;
            if (mesh.CountKnots == 0)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTask2DTri) SetTask пуст");
                return;
            }
            uint NN = (uint)Math.Sqrt(mesh.CountKnots);

            base.SetTask(mesh, Zeta0, Roughness, BConditions);

            MEM.Alloc<int>(mesh.CountKnots, ref DryWet);
            MEM.Alloc<double>(mesh.CountKnots, ref Tau);
            MEM.Alloc<double>(mesh.CountKnots, ref Gx);
            MEM.Alloc<double>(mesh.CountKnots, ref Gy);
            MEM.Alloc<double>(mesh.CountKnots, ref Sknot);
            MEM.Alloc<double>(mesh.CountElements, ref GxE);
            MEM.Alloc<double>(mesh.CountElements, ref GyE);


            IPointsA.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_L1);
            IPointsB.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_L1);

            taskReady = true;
        }
        /// <summary>
        /// Модель дна: расчет коэффициентов A B C
        /// </summary>
        public override void CalkCoeff(uint elem, double mtauS, double dZx, double dZy)
        {
            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double rho_b = SPhysics.PHYS.rho_b;
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double g = SPhysics.GRAV;

            // здесь нужен переключатель
            if (tau0 > mtau)
            {
                chi = 1;
                pA = 0;
                pB = 0;
                pC = 0;
                pD = 0;
            }
            else
            {
                // 27.12.2021
                G0 = G1 * mtauS * Math.Sqrt(mtau) / pCosGamma;
                chi = Math.Sqrt(tau0 / mtau);
                //double scale = 1.0 / (1 - 0.5 * dZx * dZx);
                double scale = 1;

                if (blm != TypeBLModel.BLModel_1991)
                {
                    // определение градиента давления по х,у
                    dPX = 0;
                    dPY = 0;
                    for (int ai = 0; ai < cu; ai++)
                    {
                        dPX += ff.DN_x[ai] * press[ai] / (rho_b * g);
                        dPY += ff.DN_y[ai] * press[ai] / (rho_b * g);
                    }
                    if (blm != TypeBLModel.BLModel_2010)
                    {
                        pA = Math.Max(0, 1 - chi);
                        pB = (chi / 2 + pA) / tanphi;
                        pC = pA / (s * tanphi);
                    }
                    else
                    {
                        pA = Math.Max(0, 1 - chi);
                        pB = (chi / 2 + pA * (1 + s) / s) / tanphi;
                        pC = A[elem] / (s * tanphi);
                    }
                }
                else
                {
                    pA = scale * Math.Max(0, 1 - chi);
                    pB =  (scale * chi / 2 + pA) / tanphi;
                    pC = 0;
                    ss = 1;
                }
                pD = pA * 4.0 / 5.0 / tanphi;
            }
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
            try
            {
                double s = SPhysics.PHYS.s;
                double tanphi = SPhysics.PHYS.tanphi;
                double rho_b = SPhysics.PHYS.rho_b;
                double G1 = SPhysics.PHYS.G1;
                double epsilon = SPhysics.PHYS.epsilon;
                double tau0 = SPhysics.PHYS.tau0;
                double g = SPhysics.GRAV;
                double kappa = SPhysics.PHYS.kappa;
                //double rho_w = SPhysics.rho_w;
                double Fa0 = SPhysics.PHYS.Fa0;
                double rho_s = SPhysics.PHYS.rho_s;


                // IFEMesh fmesh = mesh as IFEMesh;
                this.P = P;
                bool DryWetMech = false;
                if (mesh == null) return;
                MEM.Alloc<double>(Zeta0.Length, ref Zeta);
                MEM.Alloc<double>(Zeta0.Length, ref MRight);

                CreateAlgebra();

                for (int i = 0; i < Sknot.Length; i++)
                {
                    Sknot[i] = 0;
                    Gx[i] = 0;
                    Gy[i] = 0;
                }

                #region // расчет неразмываемых узлов
                int CountDryWet = 0;
                if (DryWetMech == true)
                {
                    for (int i = 0; i < DryWet.Length; i++)
                    {
                        // квадрат напряжений ?
                        Tau[i] = Math.Abs(tauX[i] * tauX[i] + tauY[i] * tauY[i]);
                        if (Tau[i] / tau0 < 1)
                            DryWet[i] = 1; // сухой
                        else
                            DryWet[i] = 0;
                    }
                    CountDryWet = DryWet.Sum();
                    if (CountDryWet > 0)
                    {
                        DryWetNumbers = new uint[CountDryWet];
                        DryWetZeta = new double[CountDryWet];
                        int k = 0;
                        for (uint i = 0; i < DryWet.Length; i++)
                        {
                            if (DryWet[i] == 1)
                            {
                                DryWetNumbers[k] = i;
                                DryWetZeta[k] = Zeta0[i];
                                k++;
                            }
                        }
                    }
                }
                #endregion
                // цикл по КЭ
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    ff = FunFormsManager.CreateKernel(typeff);
                    int cu = knots.Length;
                    InitLocal(cu);
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    // координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // установка координат узлов
                    ff.SetGeoCoords(x, y);
                    // получение значений донных отметок на КЭ
                    mesh.ElemValues(Zeta0, elem, ref zeta);
                    // определение градиента давления по х,у
                    if (blm != TypeBLModel.BLModel_1991)
                        mesh.ElemValues(P, elem, ref press);
                    // напряжения заданны в узлах
                    mesh.ElemValues(tauX, elem, ref Tx);
                    mesh.ElemValues(tauY, elem, ref Ty);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        double DWJ = ff.DetJ * pIntegration.weight[pi];

                        dZetadX = 0;
                        dZetadY = 0;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            dZetadX += ff.DN_x[ai] * zeta[ai];
                            dZetadY += ff.DN_y[ai] * zeta[ai];
                        }

                        if (blm != TypeBLModel.BLModel_1991)
                        {
                            dPX = 0;
                            dPY = 0;
                            for (int ai = 0; ai < cu; ai++)
                            {
                                dPX += ff.DN_x[ai] * press[ai] / (rho_b * g);
                                dPY += ff.DN_y[ai] * press[ai] / (rho_b * g);
                            }
                        }
                        dZx = dZetadX / tanphi;
                        dZy = dZetadY / tanphi;
                        tan2 = dZetadX * dZetadX + dZetadY * dZetadY;
                        pCosGamma = 1.0 / Math.Sqrt(1 + tan2);

                        mTx = 0;
                        mTy = 0;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            mTx += ff.N[ai] * Tx[ai];
                            mTy += ff.N[ai] * Ty[ai];
                        }

                        mtau = Math.Sqrt(mTx * mTx + mTy * mTy);
                        if (mtau > MEM.Error6)
                        {
                            cosA = mTx / (mtau + MEM.Error10);
                            sinA = mTy / (mtau + MEM.Error10);
                        }
                        else
                        {
                            cosA = 1;
                            sinA = 0;
                        }
                        cos2 = cosA * cosA;
                        sin2 = sinA * sinA;
                        cs2 = sinA * cosA;

                        mtauS = mTx * cosA + mTy * sinA;
                        // Модель дна: расчет коэффициентов pA pB pC pD
                        CalkCoeff(elem, mtauS, dZx, dZy);
                        // тензор диффузионного осыпания
                        BB[0, 0] = ss * pD * sin2 + pB * cos2;
                        BB[0, 1] = cs2 * (pB - ss * pD);
                        BB[1, 0] = cs2 * (pB - ss * pD);
                        BB[1, 1] = ss * pD * cos2 + pB * sin2;
                        // тензор напорного осыпания
                        H[0, 0] = -pD / s * sin2 - pC * cos2;
                        H[0, 1] = cs2 * (pD / s - pC);
                        H[1, 0] = cs2 * (pD / s - pC);
                        H[1, 1] = -pD / s * cos2 - pC * sin2;

                        for (int ai = 0; ai < cu; ai++)
                        {
                            for (int aj = 0; aj < cu; aj++)
                            {
                                // Матрица масс
                                MatrixM = MM[ai, aj] * ff.N[ai] * ff.N[aj] * DWJ;
                                // Матрица жесткости
                                MatrixK = G0 * (BB[0, 0] * ff.DN_x[ai] * ff.DN_x[aj] +
                                                BB[0, 1] * ff.DN_x[ai] * ff.DN_y[aj] +
                                                BB[1, 0] * ff.DN_y[ai] * ff.DN_x[aj] +
                                                BB[1, 1] * ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                                // Левая матрица n+1 слоя по времени
                                LaplMatrix[ai][aj] += MatrixM / dtime + theta * MatrixK;
                                // Правая матрица n слоя по времени
                                RMatrix[ai][aj] += MatrixM / dtime - (1 - theta) * MatrixK;
                            }
                        }
                        // учет транзитного расхода
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += G0 * A[elem] * (ff.DN_x[ai] * cosA + ff.DN_y[ai] * sinA) * DWJ;
                        // учет градиента придонного давления
                        if (blm != TypeBLModel.BLModel_1991)
                            for (int ai = 0; ai < cu; ai++)
                                LocalRight[ai] += G0 * (ff.DN_x[ai] * (H[0, 0] * dPX + H[0, 1] * dPY) +
                                                        ff.DN_y[ai] * (H[1, 0] * dPX + H[1, 1] * dPY)) * DWJ;

                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    Ralgebra.AddToMatrix(RMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                #region отладка + ГУ
                if (CountDryWet > 0 && DryWetMech == true)
                    Ralgebra.BoundConditions(DryWetZeta, DryWetNumbers);
                #endregion

                Ralgebra.getResidual(ref MRight, Zeta0, 0);
#if DEBUG        
                SparseAlgebra ckalgebra = algebra as SparseAlgebra;
                if (ckalgebra != null)
                {
                    ckalgebra.CheckMas(MRight, "MRight");
                    ckalgebra.CheckSYS("2 algebra");
                }
#endif
                algebra.CopyRight(MRight);
#if DEBUG
                #region отладка
                if (ckalgebra != null)
                    ckalgebra.CheckSYS("3 algebra");

                if (DryWetMech == true)
                {
                    //Удовлетворение ГУ
                    //for (int bi = 0; bi < 4; bi++)
                    //{
                    //    uint[] bound1 = mesh.GetBoundKnotsByMarker(bi);
                    //    double[] TL = new double[bound1.Length];
                    //    for (int i = 0; i < TL.Length; i++)
                    //        TL[i] = Zeta0[bound1[i]];
                    //    algebra.BoundConditions(TL, bound1);
                    //}
                    if (CountDryWet > 0 && DryWetMech == true)
                        algebra.BoundConditions(DryWetZeta, DryWetNumbers);
                    if (ckalgebra != null)
                        ckalgebra.CheckSYS("algebra 4");
                }
                #endregion
#endif
                algebra.Solve(ref Zeta);
                var iZ0 = IntZeta(Zeta0);
                var iZ = IntZeta(Zeta);
                double errorZ = 100 * (iZ0.int_Z - iZ.int_Z) / (iZ0.int_Z + MEM.Error10);
                double errorZL2 = 100 * (iZ0.int_Z2 - iZ.int_Z2) / (iZ0.int_Z2 + MEM.Error10);

                // Сглаживание дна по лавинной модели
                if (isAvalanche == AvalancheType.AvalancheQuad_2021)
                    avalanche.Lavina(ref Zeta);

                #region Контроль баланса массы за 1 шаг по времени
                var aiZ = IntZeta(Zeta);

                double a_errorZ = 100 * (aiZ.int_Z - iZ0.int_Z) / (iZ0.int_Z + MEM.Error10);
                Logger.Instance.AddHeaderInfo("Integral mass balance control");
                Logger.Instance.AddHeaderInfo("Интегральный контроль баланса массы");
                string str = " int (Zeta0) = " + iZ0.int_Z.ToString() +
                             " int (Zeta)= " + iZ.int_Z.ToString();
                Logger.Instance.AddHeaderInfo(str);
                str = " errorZ = " + errorZ.ToString("F6") + " %" +
                      " errorZL2 = " + errorZL2.ToString("F6") + " %";
                Logger.Instance.AddHeaderInfo(str);
                str = " a_int (1) = S = " + aiZ.Area.ToString("") +
                      " a_int (Zeta) = " + aiZ.int_Z.ToString("") +
                      " a_errorZ = " + a_errorZ.ToString("F6") + " %";
                Logger.Instance.AddHeaderInfo(str);
                Logger.Instance.AddHeaderInfo("min Zeta = " + Zeta.Min() + "  max Zeta = " + Zeta.Max());
                #endregion

                // переопределение начального значения zeta 
                // для следующего шага по времени
                for (int j = 0; j < Zeta.Length; j++)
                    Zeta0[j] = Zeta[j];
            }
            catch (Exception e)
            {
                Message = e.Message;
                Logger.Instance.Exception(e);
                for (int j = 0; j < Zeta.Length; j++)
                    Zeta[j] = Zeta0[j];
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                double[] dw = new double[DryWet.Length];
                for (int i = 0; i < dw.Length; i++)
                    dw[i] = DryWet[i];
                sp.Add("DryWet", dw);
                sp.Add("Поток Gx", Gx);
                sp.Add("Поток Gy", Gy);
                sp.Add("Поток G", Gx, Gy);
            }
        }

#if DEBUG 
        #region Тесты
        public static void Test0()
        {
            double tau0 = SPhysics.PHYS.tau0;
            BedLoadParams blp = new BedLoadParams();
            CBedLoadTask_1XD bltask = new CBedLoadTask_1XD(blp);
            //CBedLoadTask bltask = new CBedLoadTask(rho_w, rho_s, phi, d50, epsilon, kappa, cx, f);
            // задача Дирихле

            blp.BCondIn = new BoundCondition1D(0, 1);
            blp.BCondOut = new BoundCondition1D(0, 1);

            int NN = 15;
            double dx = 1.0 / (NN - 1);
            double[] x = new double[NN];
            double[] Zeta0 = new double[NN];
            double[] Zeta = new double[NN];
            for (int i = 0; i < NN; i++)
            {
                x[i] = i * dx;
                //Zeta0[i] = 0.2;
                Zeta0[i] = 1.0;
            }

            //bool isAvalanche = true;
            //bltask.SetTask(BCBed, x, Zeta0, dtime, isAvalanche);
            double T = 2 * tau0;
            double[] tau = new double[NN - 1];
            double[] P = new double[NN - 1];

            for (int i = 0; i < NN - 1; i++)
            {
                tau[i] = T;
                P[i] = 1;
            }

            for (int i = 0; i < 50; i++)
            {
                bltask.CalkZetaFDM(ref Zeta, tau, P);
                bltask.PrintMas("zeta", Zeta);
            }
            Console.Read();
            Console.WriteLine();
            Console.WriteLine();
        }

        #endregion

        //public static void Main()
        //{
        //    // Гру
        //    //Test0();
        //   // Test1();
        //   // Test2();
        //}
#endif
    }
}