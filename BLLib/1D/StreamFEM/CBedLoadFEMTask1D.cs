//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          25.12.21
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          14.01.24
//       добавлен учет расходов от взвешенных наносов
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
    public class CBedLoadFEMTask1DTri : ABedLoadFEMTask2D
    {
        /// <summary>
        /// Создание экземпляра класса
        /// </summary>
        /// <returns></returns>
        public override IBedLoadTask Clone()
        {
            return new CBedLoadFEMTask1DTri(new BedLoadParams());
        }
        /// <summary>
        /// Количество узлов на КЭ
        /// </summary>
       // const int cu = 3;
        /// <summary>
        /// Поток наносов по Х в улах
        /// </summary>
        double[] Gx;
        /// <summary>
        /// Поток взвешенных наносов по Х в улах
        /// </summary>
        double[] GCx;
        /// <summary>
        /// Площадь КО связанного с узлом
        /// </summary>
        double[] Sknot;
        /// <summary>
        /// Поток наносов по Х на элементе
        /// </summary>
        double[] GxE;
        /// <summary>
        /// Плошадь КЭ
        /// </summary>
        double S = 0;
        /// <summary>
        /// Касательные напряжения по Х в узлах КЭ
        /// </summary>
        double[] Tx = { 0, 0};
        /// <summary>
        /// Касательные напряжения по У в узлах КЭ
        /// </summary>
        double[] Ty = { 0, 0};
        /// <summary>
        /// Придонное давление в узлах КЭ
        /// </summary>
        double[] press = { 0, 0 };
        /// <summary>
        /// Отметки дна в узлах КЭ
        /// </summary>
        double[] zeta = { 0, 0 };
        /// <summary>
        /// Матрица масс для КЭ
        /// </summary>
        double[,] MM = { { 2, 1 }, { 1, 2 } };
        ///// <summary>
        ///// Флаг для определения сухого-мокрого дна
        ///// </summary>
        //public int[] DryWet = null;
        /// <summary>
        /// Флаг для определения сухого-мокрого дна
        /// </summary>
        public int[] DryWetElem = null;
        /// <summary>
        /// Модуль вектора касательных напряжений в узлах
        /// </summary>
        public double[] Tau = null;
        /// <summary>
        /// Вектор касательных напряжений в узлах
        /// </summary>
        public double[] tauX = null;
        /// <summary>
        /// Индексы "сухих" узлов
        /// </summary>
        uint[] DryWetNumbers;
        /// <summary>
        /// Отметки дна в "сухих" узлах
        /// </summary>
        double[] DryWetZeta;
        // локальные переменные
        double G0, MatrixK, MatrixM;
        double dZetadX, dPX;
        //double cosA;//, sinA;
        //double cos2, sin2, cs2;
        /// <summary>
        /// Конструктор 
        /// </summary>
        public CBedLoadFEMTask1DTri(BedLoadParams p) : base(p)
        {
            cu = 2;
            MEM.Alloc(cu, ref a, "a");
            MEM.Alloc(cu, ref b, "b");
            name = "1DX (d50) деформация дна T(U-Uc) МКЭ";
            tTask = TypeTask.streamX1D;
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
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            double tanphi = SPhysics.PHYS.tanphi;
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
            double Relax = 0.5;
            uint NN = (uint)Math.Sqrt(mesh.CountKnots);
            base.SetTask(mesh, Zeta0, Roughness, BConditions);

            avalanche = new Avalanche1DX(mesh, tanphi, Relax);

            MEM.Alloc(mesh.CountKnots, ref DryWet, "DryWet");
            MEM.Alloc(mesh.CountElements, ref DryWetElem, "DryWetElem");
            MEM.Alloc(mesh.CountKnots, ref Tau, "Tau");
            MEM.Alloc(mesh.CountKnots, ref Gx, "Gx");
            MEM.Alloc(mesh.CountKnots, ref GCx, "GСx");
            MEM.Alloc(mesh.CountKnots, ref Sknot, "Sknot");
            MEM.Alloc(mesh.CountElements, ref GxE, "GxE");
            
            taskReady = true;
        }
        /// <summary>
        /// Расчет коэффициентов A B C
        /// </summary>
        public override void CalkCoeff(uint elem, double mtauS, double dZx, double dZy)
        {

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
                double kappa = SPhysics.PHYS.kappa;
                double g = SPhysics.GRAV;
                double rho_w = SPhysics.rho_w;
                double Fa0 = SPhysics.PHYS.Fa0;
                double rho_s = SPhysics.PHYS.rho_s;
                //double nu = SPhysics.nu;
                //double mu = SPhysics.mu;

                this.tauX = tauX;
                bool DryWetMech = true;
                if (mesh == null) return;
                MEM.Alloc<double>(Zeta0.Length, ref Zeta, "Zeta");
                MEM.Alloc<double>(Zeta0.Length, ref MRight, "MRight");

                if (this.algebra == null)
                    this.algebra = new SparseAlgebraCG((uint)mesh.CountKnots);
                else
                    algebra.Clear();
                if (Ralgebra == null)
                    this.Ralgebra = this.algebra.Clone();
                else
                    Ralgebra.Clear();

                //algebra = new Algebra3DTape((uint)mesh.CountKnots);
                //Ralgebra = new Algebra3DTape((uint)mesh.CountKnots);

                for (int i = 0; i < Sknot.Length; i++)
                {
                    Sknot[i] = 0;
                    Gx[i] = 0;
                }
                // + 2024
                // Если расходы концентрации заданны симмируем их для вссех фракций
                if (CS != null)
                {
                    for (int i = 0; i < tauX.Length; i++)
                    {
                        GCx[i] = 0;
                        for (int f = 0; f < CS.Length; f++)
                            GCx[i] += CS[f][i];
                    }
                }                    

                #region // расчет неразмываемых узлов
                int CountDryWet = 0;
                //if (DryWetMech == true)
                //{
                //    for (int i = 0; i < DryWet.Length; i++)
                //    {
                //        Tau[i] = Math.Sqrt(tauX[i] * tauX[i]);
                //        if (Tau[i] / tau0 < 1)
                //            DryWet[i] = 1; // сухой
                //        else
                //            DryWet[i] = 0;
                //    }
                //    CountDryWet = DryWet.Sum();
                //    if (CountDryWet > 0)
                //    {
                //        DryWetNumbers = new uint[CountDryWet];
                //        DryWetZeta = new double[CountDryWet];
                //        int k = 0;
                //        for (uint i = 0; i < DryWet.Length; i++)
                //        {
                //            if (DryWet[i] == 1)
                //            {
                //                DryWetNumbers[k] = i;
                //                DryWetZeta[k] = Zeta0[i];
                //                k++;
                //            }
                //        }
                //    }
                //}
                #endregion
                // цикл по КЭ
                for (uint elem = 0; elem < mesh.CountElements; elem++)
                {
                    InitLocal(cu);
                    // получить узлы КЭ
                    mesh.ElementKnots(elem, ref knots);
                    // координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);
                    // mesh.ElemY(elem, ref y);
                    // площадь
                    //S = mesh.ElemSquare(x, y);
                    S = x[1] - x[0];
                    if (MEM.Equals(S, 0))
                    {
                        Logger.Instance.Info("S = 0 для elem = " + elem.ToString());
                    }
                    
                    a[0] = - 1/S ;
                    a[1] =   1/S ;
                    b[0] = 0;
                    b[1] = 0;
                    // определение градиента дна по х,у
                    mesh.ElemValues(Zeta0, elem, ref zeta);
                    dZetadX = 0;
                    for (int ai = 0; ai < cu; ai++)
                    {
                        dZetadX += a[ai] * zeta[ai];
                    }
                    // dZetadX /= (2 * S);

                    CosGamma[elem] = 1.0 / Math.Sqrt(1 + dZetadX * dZetadX);
                    double mTx = 0;
                    
                    // Подготовка коэффициетов
                    if (tauX.Length == mesh.CountElements)
                    {
                        // напряжения заданны на элементе
                        tau[elem] =  Math.Abs(tauX[elem]);
                        mTx = tauX[elem];
                    }
                    else
                    {
                        // напряжения заданны в узлах
                        mesh.ElemValues(tauX, elem, ref Tx);
                        mTx = Tx.Sum() / 2;
                        tau[elem] = Math.Abs(mTx);
                    }
                    double mtauS = mTx;
                    mtau = tau[elem];
                    // изменения!
                    // G0 = rho_s * G1 * mtauS * Math.Sqrt(mtau) / CosGamma[elem];
                    G0 = G1 * mtauS * Math.Sqrt(mtau) / CosGamma[elem];
                    // изменения!
                    if (tau0 > mtau)
                    {
                        chi = 1;
                        A[elem] = 0;
                        B[elem] = 0;
                        C[elem] = 0;
                    }
                    else
                    {
                        chi = Math.Sqrt(tau0 / mtau);

                        if (blm != TypeBLModel.BLModel_1991)
                        {
                            // определение градиента давления по х,у
                            mesh.ElemValues(P, elem, ref press);
                            dPX = 0;
                            for (int ai = 0; ai < cu; ai++)
                                dPX += a[ai] * press[ai] / (rho_b * g);

                            if (blm != TypeBLModel.BLModel_2010)
                            {
                                A[elem] = Math.Max(0, 1 - chi);
                                B[elem] = (chi / 2 + A[elem]) / tanphi;
                                C[elem] = A[elem] / (s * tanphi);
                            }
                            else
                            {
                                A[elem] = Math.Max(0, 1 - chi);
                                B[elem] = (chi / 2 + A[elem] * (1 + s) / s) / tanphi;
                                C[elem] = A[elem] / (s * tanphi);
                            }
                        }
                        else
                        {
                            A[elem] = Math.Max(0, 1 - chi);
                            B[elem] = (chi / 2 + A[elem]) / tanphi;
                            C[elem] = 0;
                        }
                    }

                    #region добаление диффузионной лавинной модели от 23 11 2021 
                    //if (A[elem] < MEM.Error10)
                    //{
                    //    // тангенс угла
                    //    double tanGamma = Math.Sqrt(1 - CosGamma[elem] * CosGamma[elem]) / CosGamma[elem];
                    //    // лавинная диффузия
                    //    double AvalancheDiff = Math.Max(0, tanGamma - tanphi);
                    //    if (AvalancheDiff < MEM.Error5 ||
                    //        isAvalanche == AvalancheType.NonAvalanche ||
                    //        isAvalanche == AvalancheType.AvalancheQuad_2021)
                    //    {
                    //        B[elem] = 0;
                    //    }
                    //    else
                    //    {
                    //        double Currant = 0.001;
                    //        B[elem] = Currant * dtime * AvalancheDiff * B[elem];
                    //    }
                    //}
                    #endregion 

                    #region Вычисление потоков на элементе и их разброс в узлы
                    GxE[elem] = G0 * (A[elem] - B[elem] * dZetadX);

                    if (B[elem] < MEM.Error10)
                        DryWetElem[elem] = 1; // сухой
                    else
                        DryWetElem[elem] = 0;
                    DryWet[knots[0]] += DryWetElem[elem];
                    DryWet[knots[1]] += DryWetElem[elem];

                    double S2 = S / 2;
                    Sknot[knots[0]] += S2;
                    Sknot[knots[1]] += S2;
                    Gx[knots[0]] += GxE[elem] * S2;
                    Gx[knots[1]] += GxE[elem] * S2;

                    
                    #endregion

                    // вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                        {
                            // Матрица масс
                            MatrixM = MM[ai, aj] * S / 6;
                            // Матрица жесткости
                            MatrixK =  Math.Abs( G0 * B[elem] ) * a[ai] * a[aj]  * S;
                            // Левая матрица n+1 слоя по времени
                            LaplMatrix[ai][aj] = MatrixM / dtime + theta * MatrixK;
                            // Правая матрица n слоя по времени
                            RMatrix[ai][aj] = MatrixM / dtime - (1 - theta) * MatrixK;
                        }

                    // учет транзитного расхода  + 2024: + GCx[elem] 
                    for (int ai = 0; ai < cu; ai++)
                        LocalRight[ai] =  ( G0 * A[elem] + GCx[elem] ) * a[ai] * S;

                    // учет градиента придонного давления
                    if (blm != TypeBLModel.BLModel_1991)
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += - G0  * C[elem] * dPX * a[ai] * S;

                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    Ralgebra.AddToMatrix(RMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }


                CountDryWet = DryWet.Count(xxx=>xxx > 0);
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


                for (int i = 0; i < Sknot.Length; i++)
                {
                    Gx[i] = Gx[i] / Sknot[i];
                }

                if (CountDryWet > 0 && DryWetMech == true)
                    Ralgebra.BoundConditions(DryWetZeta, DryWetNumbers);

                Ralgebra.getResidual(ref MRight, Zeta0, 0);
                if (algebra as SparseAlgebra != null)
                {
                    ((SparseAlgebra)algebra).CheckMas(MRight, "MRight");
                    ((SparseAlgebra)algebra).CheckSYS("2 algebra");
                }
                algebra.CopyRight(MRight);
                
                if (algebra as SparseAlgebra != null)
                    ((SparseAlgebra)algebra).CheckSYS("3 algebra");

                if (DryWetMech == true)
                {
                    if (CountDryWet > 0 && DryWetMech == true)
                        algebra.BoundConditions(DryWetZeta, DryWetNumbers);
                    if (algebra as SparseAlgebra != null)
                        ((SparseAlgebra)algebra).CheckSYS("algebra 4");
                }

                algebra.Solve(ref Zeta);

                // Сглаживание дна по лавинной моделе
               if (isAvalanche == AvalancheType.AvalancheSimple)
                    avalanche.Lavina(ref Zeta);

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
                double[] xx = mesh.GetCoords(0);
                double[] dw = new double[DryWet.Length];
                for (int i = 0; i < dw.Length; i++)
                    dw[i] = DryWet[i];
                sp.Add("DryWet", dw);
                sp.Add("Поток Gx", Gx);

                sp.AddCurve("Отметки дна (узлы): ", xx, Zeta0);
                sp.AddCurve("Придонное напряжение", xx, tauX);
                sp.AddCurve("Поток Gx", xx, Gx);
                
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

            blp.BCondIn = new BoundCondition1D(TypeBoundCond.Dirichlet, 1);
            blp.BCondOut = new BoundCondition1D(TypeBoundCond.Dirichlet, 1);

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