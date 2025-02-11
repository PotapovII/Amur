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
    using System;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;

    /// <summary>
    ///  ОО: Определение класса RiverStreamTask - расчет полей скорости 
    ///             и напряжений в живом сечении потока
    /// </summary>
    [Serializable]
    public class TriSroSecRiverTask : ASectionalRiverTask, IRiver
    {
        /// <summary>
        /// Турбулентная вязкость
        /// </summary>
        double Mu = SPhysics.mu;
        #region Свойства
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public new string VersionData() => "TriSroSecRiverTask 21.02.2022";
        #endregion
        public TriSroSecRiverTask() : base(new RiverStreamParams())
        {
            name = "гидрадинамика створа (трехузловые КЭ)";
            this.cu = 3;
            Init();
        }

        public TriSroSecRiverTask(RiverStreamParams p) : base(p)
        {
            name = "гидрадинамика створа (трехузловые КЭ)";
            this.cu = 3;
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
                uint[] knots = { 0, 0, 0 };
                double[] x = { 0, 0, 0 };
                double[] y = { 0, 0, 0 };
                // выделить память под локальные массивы
                InitLocal(cu);
                for (elem = 0; elem < mesh.CountElements; elem++)
                {
                    // узлы
                    mesh.ElementKnots(elem, ref knots);
                    //Координаты и площадь
                    mesh.GetElemCoords(elem, ref x, ref y);

                    // получит вязкость в узлах
                    mesh.ElemValues(mu, elem, ref elem_mu);
                    Mu = (elem_mu[0]+ elem_mu[1] + elem_mu[2]) / 3;
                    //Площадь
                    double S = mesh.ElemSquare(elem);
                    // Градиенты от функций форм
                    double[] a = new double[cu];
                    double[] b = new double[cu];
                    a[0] = (y[1] - y[2]);
                    b[0] = (x[2] - x[1]);
                    a[1] = (y[2] - y[0]);
                    b[1] = (x[0] - x[2]);
                    a[2] = (y[0] - y[1]);
                    b[2] = (x[1] - x[0]);
                    // Вычисление ЛЖМ
                    for (int ai = 0; ai < cu; ai++)
                        for (int aj = 0; aj < cu; aj++)
                            LaplMatrix[ai][aj] = Mu * (a[ai] * a[aj] + b[ai] * b[aj]) / (4 * S);
                    // добавление вновь сформированной ЛЖМ в ГМЖ
                    algebra.AddToMatrix(LaplMatrix, knots);
                    // Вычисление ЛПЧ
                    for (int j = 0; j < cu; j++)
                        LocalRight[j] = Q * S / 3;
                    // добавление вновь сформированной ЛПЧ в ГПЧ
                    algebra.AddToRight(LocalRight, knots);
                }
                //Удовлетворение ГУ
                uint[] bound = mesh.GetBoundKnotsByMarker(1);
                algebra.BoundConditions(0.0, bound);
                algebra.Solve(ref U);
                foreach (var ee in U)
                    if (double.IsNaN(ee) == true)
                        throw new Exception("TriSroSecRiverTask.SolveVelosity >> algebra");
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
        public  void SolveTaus0()
        {
            double S;
            double[] x = { 0, 0, 0 };
            double[] y = { 0, 0, 0 };
            double[] u = { 0, 0, 0 };
            double[] tmpTausZ = new double[mesh.CountElements];
            double[] tmpTausY = new double[mesh.CountElements];
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.GetElemCoords(elem, ref x, ref y);
                mesh.ElemValues(U, elem, ref u);
                // получит вязкость в узлах
                mesh.ElemValues(mu, elem, ref elem_mu);
                Mu = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3;
                //Площадь
                S = mesh.ElemSquare(elem);
                
                double Ez = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0]));
                double Ey = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1]));
                tmpTausZ[elem] = Mu * Ez / (2 * S);
                tmpTausY[elem] = Mu * Ey / (2 * S);
            }
            uint[] knots = { 0, 0, 0 };
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                // площадь КЭ
                S = mesh.ElemSquare(elem);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = tmpTausZ[elem] * S / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauZ);
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                mesh.ElementKnots(elem, ref knots);
                // площадь КЭ
                S = mesh.ElemSquare(elem);
                //подготовка ЛЖМ
                for (int ai = 0; ai < cu; ai++)
                    for (int aj = 0; aj < cu; aj++)
                    {
                        if (ai == aj)
                            LaplMatrix[ai][aj] = 2.0 * S / 12.0;
                        else
                            LaplMatrix[ai][aj] = S / 12.0;
                    }
                //добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, knots);
                //подготовка ЛПЧ
                for (int j = 0; j < cu; j++)
                    LocalRight[j] = tmpTausY[elem] * S / 3;
                //добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, knots);
            }
            algebra.Solve(ref TauY);
        }

        /// <summary>
        /// Нахождение полей напряжений и их сглаживание по методу Галеркина
        /// </summary>
        public override void SolveTaus()
        {
            try
            {
                double[] x = { 0, 0, 0 };
                double[] y = { 0, 0, 0 };
                double[] u = { 0, 0, 0 };
                double S;
                uint[] knots = { 0, 0, 0 };
                double[] Selem = new double[mesh.CountElements];
                double[] tmpTausZ = new double[mesh.CountElements];
                double[] tmpTausY = new double[mesh.CountElements];
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] = 0;
                    TauZ[i] = 0;
                }
                for (uint i = 0; i < mesh.CountElements; i++)
                {
                    mesh.ElementKnots(i, ref knots);
                    mesh.GetElemCoords(i, ref x, ref y);
                    mesh.ElemValues(U, i, ref u);
                    // получит вязкость в узлах
                    mesh.ElemValues(mu, i, ref elem_mu);
                    //u = GetFieldElem(U, i);
                    Mu = (elem_mu[0] + elem_mu[1] + elem_mu[2]) / 3.0;
                    //Площадь
                    S = mesh.ElemSquare(i);
                    // деформации
                    double dN0dz = (x[2] - x[1]) / (2 * S);
                    double dN1dz = (x[0] - x[2]) / (2 * S);
                    double dN2dz = (x[1] - x[0]) / (2 * S);

                    double dN0dy = (y[1] - y[2]) / (2 * S);
                    double dN1dy = (y[2] - y[0]) / (2 * S);
                    double dN2dy = (y[0] - y[1]) / (2 * S);


                    double Ez = (u[0] * (x[2] - x[1]) + u[1] * (x[0] - x[2]) + u[2] * (x[1] - x[0]));
                    double Ey = (u[0] * (y[1] - y[2]) + u[1] * (y[2] - y[0]) + u[2] * (y[0] - y[1]));
                    // 
                    tmpTausZ[i] = Mu * Ez / (2 * S);
                    tmpTausY[i] = Mu * Ey / (2 * S);

                    Selem[(int)knots[0]] += S / 3;
                    Selem[(int)knots[1]] += S / 3;
                    Selem[(int)knots[2]] += S / 3;

                    TauZ[(int)knots[0]] += S * tmpTausZ[i] / 3;
                    TauZ[(int)knots[1]] += S * tmpTausZ[i] / 3;
                    TauZ[(int)knots[2]] += S * tmpTausZ[i] / 3;

                    TauY[(int)knots[0]] += S * tmpTausY[i] / 3;
                    TauY[(int)knots[1]] += S * tmpTausY[i] / 3;
                    TauY[(int)knots[2]] += S * tmpTausY[i] / 3;
                }
                for (int i = 0; i < TauZ.Length; i++)
                {
                    TauY[i] /= Selem[i];
                    TauZ[i] /= Selem[i];
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        public override double RiverFlowRate()
        {
            Area = 0;
            double su, S;
            riverFlowRateCalk = 0;
            for (uint i = 0; i < mesh.CountElements; i++)
            {
                mesh.ElemValues(U, i, ref elem_U);
                su = (elem_U[0]+ elem_U[1]+ elem_U[2]) / cu;
                S = mesh.ElemSquare(i);
                // расход по живому сечению
                riverFlowRateCalk += S * su;
                Area += S;
            }
            //if (double.IsNaN(riverFlowRateCalk) == true)
            //    throw new Exception("riverFlowRateCalk NaN");
            return riverFlowRateCalk;
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSroSecRiverTask(new RiverStreamParams());
        }
    }
}
