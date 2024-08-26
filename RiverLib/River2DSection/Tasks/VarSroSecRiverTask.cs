namespace RiverLib
{
    //---------------------------------------------------------------------------
    //                          ПРОЕКТ  "DISER"
    //                  создано  :   9.03.2007 Потапов И.И.
    //---------------------------------------------------------------------------
    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
    //            перенесено с правкой : 06.12.2020 Потапов И.И. 
    //            создание родителя : 21.02.2022 Потапов И.И. 
    //---------------------------------------------------------------------------
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;
    /// <summary>
    ///  ОО: Определение класса SrossSectionalRiverTask - расчет полей скорости 
    ///     и напряжений в живом сечении потока МКЭ на произвольной сетке
    /// </summary>
    [Serializable]
    public class VarSroSecRiverTask : ASectionalRiverTask, IRiver
    {
        #region Свойства
        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        double Mu = SPhysics.mu;
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public new string VersionData() => "VarSroSecRiverTask 21.02.2022"; 
        #endregion
   
        public VarSroSecRiverTask(RiverStreamParams p) : base(p)
        {
            name = "гидрадинамика створа (произвольные КЭ)";
            Init();
        }
        /// <summary>
        /// Нахождение поля скоростей
        /// </summary>
        public override void SolveU()
        {
            uint elem = 0;
            try
            {
                algebra.Clear();
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить тип и узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // выбрать по типу квадратурную схему для интегрирования
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // определить количество узлов на КЭ
                    cu = knots.Length;
                    // выделить память под локальные массивы
                    InitLocal(cu);
                    // получить координаты узлов
                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    // получить функции формы КЭ
                    ff = FunFormsManager.CreateKernel(typeff);
                    // передать координат узлов в функции формы
                    ff.SetGeoCoords(elem_x, elem_y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // вычислить вязкость в точке интегрирования
                        Mu = 0;
                        for (int ai = 0; ai < cu; ai++)
                            Mu += elem_mu[ai] * ff.N[ai];
                        // вычислить глобальыне производные в точках интегрирования
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // получть произведение якобиана и веса для точки интегрирования
                        double DWJ = ff.DetJ * pIntegration.weight[pi];
                        // локальная матрица жесткости                    
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += Mu * (ff.DN_x[ai] * ff.DN_x[aj] + ff.DN_y[ai] * ff.DN_y[aj]) * DWJ;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] +=  Q * ff.N[ai] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                algebra.BoundConditions(0.0, bound);
                if (Params.AxisSymmetry == 0)
                {
                    bound = mesh.GetBoundKnotsByMarker(0);
                    algebra.BoundConditions(0.0, bound);
                }
                algebra.Solve(ref U);
                foreach (var ee in U)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("SolveVelosity >> algebra");
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }
        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public override void SolveTaus()
        {
            double DWJ;
            uint elem = 0;
            double Mu;
            double dUx;
            double dUy;
            double _Tau;
            try
            {
                algebra.Clear();
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить тип и узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // выбрать по типу квадратурную схему для интегрирования
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // определить количество узлов на КЭ
                    int cu = knots.Length;
                    // выделить память под локальные массивы
                    InitLocal(cu);
                    // получить координаты узлов
                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    mesh.ElemValues(U, elem, ref elem_U);
                    // получить функции формы КЭ
                    ff = FunFormsManager.CreateKernel(typeff);
                    // передать координат узлов в функции формы
                    ff.SetGeoCoords(elem_x, elem_y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // вычислить глобальыне производные в точках интегрирования
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        _Tau = 0;
                        // вычислить напрядене в узлах для точки интегрирования
                        //for (int ai = 0; ai < cu; ai++)
                        //    _Tau += elem_mu[ai] * ff.N[ai] * elem_U[ai] * ff.DN_x[ai];
                        Mu = 0;
                        dUx = 0;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            // вычислить вязкость в точке интегрирования
                            Mu += elem_mu[ai] * ff.N[ai];
                            // >>>> горизональная производная 
                            dUx += elem_U[ai] * ff.DN_x[ai];
                        }
                        _Tau = Mu * dUx;
                        // получть произведение якобиана и веса для точки интегрирования
                        DWJ = ff.DetJ * pIntegration.weight[pi];
                        // локальная матрица жесткости                    
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += ff.N[ai] * ff.N[aj] * DWJ;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += _Tau * ff.N[ai] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.Solve(ref TauY);
                //for (int i = 0; i < TauY.Length; i++)
                //    TauY[i] /= rho_w;

                algebra.Clear();
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // получить тип и узлы КЭ
                    TypeFunForm typeff = mesh.ElementKnots(elem, ref knots);
                    // выбрать по типу квадратурную схему для интегрирования
                    if (FunFormHelp.CheckFF(typeff) == 0)
                        pIntegration = IPointsA;
                    else
                        pIntegration = IPointsB;
                    // определить количество узлов на КЭ
                    int cu = knots.Length;
                    // выделить память под локальные массивы
                    InitLocal(cu);
                    // получить координаты узлов
                    mesh.GetElemCoords(elem, ref elem_x, ref elem_y);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    mesh.ElemValues(U, elem, ref elem_U);
                    // получить функции формы КЭ
                    ff = FunFormsManager.CreateKernel(typeff);
                    // передать координат узлов в функции формы
                    ff.SetGeoCoords(elem_x, elem_y);
                    // цикл по точкам интегрирования
                    for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                    {
                        // вычисление фф в точках интегрирования
                        ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // вычислить глобальыне производные в точках интегрирования
                        ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                        // получть произведение якобиана и веса для точки интегрирования
                        DWJ = ff.DetJ * pIntegration.weight[pi];

                        _Tau = 0;
                        // вычислить напрядене в узлах для точки интегрирования
                        Mu = 0;
                        dUy = 0;
                        for (int ai = 0; ai < cu; ai++)
                        {
                            // вычислить вязкость в точке интегрирования
                            Mu += elem_mu[ai] * ff.N[ai];
                            // >>>> горизональная производная 
                            dUy += elem_U[ai] * ff.DN_y[ai];
                        }
                        _Tau = Mu * dUy;
                        // Вычисление ЛПЧ
                        for (int ai = 0; ai < cu; ai++)
                            LocalRight[ai] += _Tau * ff.N[ai] * DWJ;
                        // локальная матрица жесткости                    
                        for (int ai = 0; ai < cu; ai++)
                            for (int aj = 0; aj < cu; aj++)
                                LaplMatrix[ai][aj] += ff.N[ai] * ff.N[aj] * DWJ;
                    }
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                algebra.Solve(ref TauZ);
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
                Logger.Instance.Info("Элементов всего :" + mesh.CountKnots.ToString());
                Logger.Instance.Info("Элементов обработано :" + elem.ToString());
            }
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new VarSroSecRiverTask(new RiverStreamParams());
        }
    }
}
