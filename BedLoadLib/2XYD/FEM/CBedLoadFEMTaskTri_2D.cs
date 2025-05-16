//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          27.02.21
//---------------------------------------------------------------------------
//                  добавлен контроль потери массы
//                          27.03.22
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using System.Linq;

    using MemLogLib;
    using AlgebraLib;

    using CommonLib;
    using CommonLib.Function;
    using CommonLib.Physics;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Класс для решения плановой задачи о 
    /// расчете донных деформаций русла на симплекс сетке
    /// </summary>
    [Serializable]
    public class CBedLoadFEMTaskTri_2D : ABedLoadFEM_2D
    {
        public override IBedLoadTask Clone()
        {
            return new CBedLoadFEMTaskTri_2D(Params);
        }
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public override string NameBLParams()
        {
            return "BedLoadParams2D.txt";
        }
        /// <summary>
        /// Загрузка полей задачи из форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions = null)
        {
            throw new NotImplementedException();
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
        protected double[] Tx = { 0, 0, 0 };
        /// <summary>
        /// Касательные напряжения по У в узлах КЭ
        /// </summary>
        protected double[] Ty = { 0, 0, 0 };
        /// <summary>
        /// Придонное давление в узлах КЭ
        /// </summary>
        protected double[] press = { 0, 0, 0 };
        /// <summary>
        /// Отметки дна в узлах КЭ
        /// </summary>
        protected double[] zeta = { 0, 0, 0 };
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
        protected double[,] MM = { { 2, 1, 1 }, { 1, 2, 1 }, { 1, 1, 2 } };
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
        // локальные переменные
        protected double G0, MatrixK, MatrixM;
        protected double dZetadX, dZetadY, dPX, dPY;
        protected double cosA, sinA;
        protected double cos2, sin2, cs2;
        protected double ss;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public CBedLoadFEMTaskTri_2D() : this(new BedLoadParams2D()) { }

        public CBedLoadFEMTaskTri_2D(BedLoadParams2D p) : base(p)
        {
            cu = 3;
            a = new double[cu];
            b = new double[cu];
            double s = SPhysics.PHYS.s;
            ss = (1 + s) / s;
            name = "плановая деформация одно-фракционного дна МКЭ L3.";
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        /// <param name="Roughness">шероховатость дна</param>
        /// <param name="BConditions">граничные условия, 
        /// количество в обзем случае определяется через маркеры 
        /// границ сетеи</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            if (mesh == null)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTaskTri_2D) SetTask пуст");
                return;
            }
            if (mesh.CountKnots == 0)
            {
                Logger.Instance.Info("объект mesh в методе (CBedLoadFEMTaskTri_2D) SetTask пуст");
                return;
            }
            eTaskReady = ETaskStatus.CreateMesh;
            uint NN = (uint)Math.Sqrt(mesh.CountKnots);
            base.SetTask(mesh, Zeta0, Roughness, BConditions);
            MEM.Alloc<int>(mesh.CountKnots, ref DryWet);
            MEM.Alloc<double>(mesh.CountKnots, ref Tau);
            MEM.Alloc<double>(mesh.CountKnots, ref Gx);
            MEM.Alloc<double>(mesh.CountKnots, ref Gy);
            MEM.Alloc<double>(mesh.CountKnots, ref Sknot);
            MEM.Alloc<double>(mesh.CountElements, ref GxE);
            MEM.Alloc<double>(mesh.CountElements, ref GyE);
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Модель дна: расчет коэффициентов A B C
        /// </summary>
        public override void CalkCoeff(uint elem, double mtauS, double dZx, double dZy)
        {
            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double tau0 = SPhysics.PHYS.tau0;
            double rho_b = SPhysics.PHYS.rho_b;
            double G1 = SPhysics.PHYS.G1;
            double g = SPhysics.GRAV;
            // здесь нужен переключатель
            if (tau0 > mtau)
            {
                chi = 1;
                A[elem] = 0;
                B[elem] = 0;
                C[elem] = 0;
                D[elem] = 0;
            }
            else
            {
                // 27.12.2021
                //G0 = rho_s * G1 * mtauS * Math.Sqrt(mtau) / CosGamma[elem];
                G0 = G1 * mtauS * Math.Sqrt(mtau) / CosGamma[elem];
                chi = Math.Sqrt(tau0 / mtau);
                double scale = DRate(dZx, dZy);

                if (Params.blm != TypeBLModel.BLModel_1991 && P!=null)
                {
                    // определение градиента давления по х,у
                    mesh.ElemValues(P, elem, ref press);
                    dPX = 0;
                    dPY = 0;
                    for (int ai = 0; ai < cu; ai++)
                    {
                        dPX += a[ai] * press[ai] / (rho_b * g);
                        dPY += b[ai] * press[ai] / (rho_b * g);
                    }
                    //dPX /= (2 * S);
                    //dPY /= (2 * S);

                    if (Params.blm != TypeBLModel.BLModel_2010)
                    {
                        A[elem] = scale * Math.Max(0, 1 - chi);
                        B[elem] = scale * (chi / 2 + A[elem]) / tanphi;
                        C[elem] = A[elem] / (s * tanphi);
                    }
                    else
                    {
                        A[elem] = scale * Math.Max(0, 1 - chi);
                        B[elem] = scale * (chi / 2 + A[elem] * (1 + s) / s) / tanphi;
                        C[elem] = A[elem] / (s * tanphi);
                    }
                }
                else
                {
                    A[elem] = scale * Math.Max(0, 1 - chi);
                    B[elem] =  (scale * chi / 2 + A[elem]) / tanphi;
                    C[elem] = 0;
                    ss = 1;
                }
                D[elem] = A[elem] * 4.0 / 5.0 / tanphi;
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
                double tau0 = SPhysics.PHYS.tau0;
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
                    InitLocal(cu);
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    // координаты 
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // площадь
                    S = mesh.ElemSquare(elem);
                    if (MEM.Equals(S, 0))
                    {
                        Console.WriteLine("elem = {0}", elem);
                    }
                    a[0] = y[1] - y[2];
                    b[0] = x[2] - x[1];
                    a[1] = y[2] - y[0];
                    b[1] = x[0] - x[2];
                    a[2] = y[0] - y[1];
                    b[2] = x[1] - x[0];

                    // определение градиента дна по х,у
                    mesh.ElemValues(Zeta0, elem, ref zeta);

                    dZetadX = 0;
                    dZetadY = 0;
                    for (int ai = 0; ai < cu; ai++)
                    {
                        dZetadX += a[ai] * zeta[ai];
                        dZetadY += b[ai] * zeta[ai];
                    }
                    dZetadX /= (2 * S);
                    dZetadY /= (2 * S);
                    double dGx = dZetadX / tanphi;
                    double dGy = dZetadY / tanphi;
                    double tan2 = dZetadX * dZetadX + dZetadY * dZetadY;
                    GammaX[elem] = dGx * dGx;
                    TanGamma[elem] = Math.Sqrt(tan2);                    
                    CosGamma[elem] = 1.0 / Math.Sqrt(1 + tan2);
                    double mTx = 0;
                    double mTy = 0;
                    // Подготовка коэффициетов
                    if (tauX.Length == mesh.CountElements)
                    {
                        // напряжения заданны на элементе
                        tau[elem] = Math.Sqrt(tauX[elem] * tauX[elem] + tauY[elem] * tauY[elem]);
                        mTx = tauX[elem];
                        mTy = tauY[elem];
                        if (tau[elem] > eZeta)
                        {
                            cosA = tauX[elem] / tau[elem];
                            sinA = tauY[elem] / tau[elem];
                        }
                        else
                        {
                            cosA = 1; sinA = 0;
                        }
                    }
                    else
                    {
                        // напряжения заданны в узлах
                        mesh.ElemValues(tauX, elem, ref Tx);
                        mesh.ElemValues(tauY, elem, ref Ty);
                        mTx = Tx.Sum() / 3;
                        mTy = Ty.Sum() / 3;
                        tau[elem] = Math.Sqrt(mTx * mTx + mTy * mTy);
                        if (tau[elem] > MEM.Error6)
                        {
                            cosA = mTx / (tau[elem] + MEM.Error10);
                            sinA = mTy / (tau[elem] + MEM.Error10);
                        }
                        else
                        {
                            cosA = 1; sinA = 0;
                        }
                    }
                    
                    cos2 = cosA * cosA;
                    sin2 = sinA * sinA;
                    cs2 = sinA * cosA;
                    double mtauS = mTx * cosA + mTy * sinA;
                    mtau = tau[elem];
                    
                    // Модель дна: расчет коэффициентов A B C
                    CalkCoeff(elem, mtauS, dGx, dGy);

                    double tanGamma = Math.Sqrt(1 - CosGamma[elem] * CosGamma[elem]) / CosGamma[elem];
                    #region добаление диффузионной лавинной модели от 23 11 2021 
                    //if (A[elem] < MEM.Error10)
                    //{
                    //    // тангенс угла
                        
                    //    // лавинная диффузия
                    //    double AvalancheDiff = Math.Max(0, tanGamma - tanphi);
                    //    if (AvalancheDiff < MEM.Error5 ||
                    //        isAvalanche == AvalancheType.NonAvalanche ||
                    //        isAvalanche == AvalancheType.AvalancheQuad_2021)
                    //    {
                    //        B[elem] = 0;
                    //        D[elem] = 0;
                    //    }
                    //    else
                    //    {
                    //        double Currant = 0.001;
                    //        B[elem] = Currant * dtime * AvalancheDiff * B[elem];
                    //        D[elem] = Currant * dtime * AvalancheDiff * D[elem];
                    //    }
                    //}

                    // тангенс угла
                    // double tanGamma = Math.Sqrt(1 - CosGamma[elem] * CosGamma[elem]) / CosGamma[elem];
                    // лавинная диффузия
                    //double AvalancheDiff = Math.Max(0, tanGamma - tanphi);
                    //if (A[elem] < MEM.Error10)
                    //{
                    //    if (AvalancheDiff < MEM.Error5 ||
                    //        isAvalanche == AvalancheType.NonAvalanche ||
                    //        isAvalanche == AvalancheType.AvalancheQuad_2021)
                    //    {
                    //        B[elem] = 0;
                    //        D[elem] = 0;
                    //    }
                    //    else
                    //    {
                    //        double Currant = 0.5;
                    //        B[elem] = Currant * dtime * AvalancheDiff * B[elem];
                    //        D[elem] = Currant * dtime * AvalancheDiff * D[elem];
                    //    }
                    //}
                    //else
                    //{
                    //    if (AvalancheDiff < MEM.Error5 &&
                    //        isAvalanche == AvalancheType.AvalancheSimple)
                    //    {
                    //        double Currant = 0.5;
                    //        B[elem] += Currant * dtime * AvalancheDiff * B[elem];
                    //        D[elem] += Currant * dtime * AvalancheDiff * D[elem];
                    //    }

                    //}
                    #endregion

                    BB[0, 0] = ss * D[elem] * sin2 + B[elem] * cos2;
                    BB[0, 1] = cs2 * (B[elem] - ss * D[elem]);
                    BB[1, 0] = cs2 * (B[elem] - ss * D[elem]);
                    BB[1, 1] = ss * D[elem] * cos2 + B[elem] * sin2;
                    // ?
                    H[0, 0] = -D[elem] / s * sin2 - C[elem] * cos2;
                    H[0, 1] = cs2 * (D[elem] / s - C[elem]);
                    H[1, 0] = cs2 * (D[elem] / s - C[elem]);
                    H[1, 1] = -D[elem] / s * cos2 - C[elem] * sin2;


                    #region Вычисление потоков на элементе и их разброс в узлы
                    GxE[elem] = G0 * (A[elem] * cosA - BB[0, 0] * dZetadX - BB[0, 1] * dZetadY);
                    GyE[elem] = G0 * (A[elem] * sinA - BB[1, 0] * dZetadX - BB[1, 1] * dZetadY);
                    Sknot[knots[0]] += S / 3;
                    Sknot[knots[1]] += S / 3;
                    Sknot[knots[2]] += S / 3;
                    Gx[knots[0]] += GxE[elem] * S / 3;
                    Gx[knots[1]] += GxE[elem] * S / 3;
                    Gx[knots[2]] += GxE[elem] * S / 3;
                    Gy[knots[0]] += GyE[elem] * S / 3;
                    Gy[knots[1]] += GyE[elem] * S / 3;
                    Gy[knots[2]] += GyE[elem] * S / 3;
                    #endregion

                    // вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                        {
                            // Матрица масс
                            MatrixM = MM[ai, aj] * S / 12;
                            // Матрица жесткости
                            MatrixK = G0 * (BB[0, 0] * a[ai] * a[aj] +
                                            BB[0, 1] * a[ai] * b[aj] +
                                            BB[1, 0] * b[ai] * a[aj] +
                                            BB[1, 1] * b[ai] * b[aj]) / (4 * S);
                            // Левая матрица n+1 слоя по времени
                            LaplMatrix[ai][aj] = MatrixM / dtime + theta * MatrixK;
                            // Правая матрица n слоя по времени
                            RMatrix[ai][aj] = MatrixM / dtime - (1 - theta) * MatrixK;
                        }

                    // учет транзитного расхода
                    for (int ai = 0; ai < cu; ai++)
                        LocalRight[ai] = G0 * A[elem] * (a[ai] * cosA + b[ai] * sinA) / 2;

                    // учет градиента придонного давления
                    if (Params.blm != TypeBLModel.BLModel_1991)
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += G0 * (a[ai] * (H[0, 0] * dPX + H[0, 1] * dPY) +
                                                    b[ai] * (H[1, 0] * dPX + H[1, 1] * dPY)) / (12 * S);

                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    Ralgebra.AddToMatrix(RMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }

                for (int i = 0; i < Sknot.Length; i++)
                {
                    Gx[i] = Gx[i] / Sknot[i];
                    Gy[i] = Gy[i] / Sknot[i];
                }

                #region отладка + ГУ
                //((SparseAlgebra)Ralgebra).CheckSYS("1 Ralgebra");
                //((SparseAlgebra)algebra).CheckSYS("1 algebra");
                //for (int bi = 0; bi < 4; bi++)
                //{
                //    uint[] bound1 = mesh.GetBoundKnotsByMarker(bi);
                //    double[] TL = new double[bound1.Length];
                //    for (int i = 0; i < TL.Length; i++)
                //        TL[i] = Zeta0[bound1[i]];
                //    Ralgebra.BoundConditions(TL, bound1);
                //}
                //uint[] bound3 = mesh.GetBoundKnotsByMarker(3);
                //double[] TR = new double[bound3.Length];
                //for (int i = 0; i < TR.Length; i++)
                //    TR[i] = Zeta0[bound3[i]];
                //Ralgebra.BoundConditions(TL, bound1);
                //Ralgebra.BoundConditions(TR, bound3);
                if (CountDryWet > 0 && DryWetMech == true)
                    Ralgebra.BoundConditions(DryWetZeta, DryWetNumbers);
                //((SparseAlgebra)Ralgebra).CheckSYS("2 Ralgebra после ГУ");
                //((SparseAlgebra)algebra).CheckMas(Zeta0, "Zeta0");
                #endregion

                Ralgebra.getResidual(ref MRight, Zeta0, 0);
#if DEBUG        
                //((SparseAlgebra)algebra).Debug = true;
                SparseAlgebra ckalgebra = algebra as SparseAlgebra;
                if (ckalgebra != null)
                {
                    ckalgebra.CheckMas(MRight, "MRight");
                    ckalgebra.CheckSYS("2 algebra");
                }
                //((SparseAlgebra)algebra).Print();
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
                //if(isAvalanche == AvalancheType.AvalancheQuad_2021)
                if (Params.isAvalanche == AvalancheType.AvalancheSimple)
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
    }
}