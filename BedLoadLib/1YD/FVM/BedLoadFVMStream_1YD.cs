//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.09.22
//---------------------------------------------------------------------------
//                Модуль BedLoadLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//---------------------------------------------------------------------------
//              Классический подход расчета донных деформаций
//                  через расчет градиента расхода
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Класс для решения одномерной задачи о 
    /// расчете береговых деформаций русла в створе реки
    /// реализованный в потоках наносов 
    /// </summary>
    [Serializable]
    public class BedLoadFVMStream_1YD : BedLoadFVM_1YD
    {
        /// <summary>
        /// Параметр Шильдса
        /// </summary>
        double[] Theta = null;

        double normaTheta;
        public override IBedLoadTask Clone()
        {
            return new BedLoadFVMStream_1YD(Params);
        }
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public BedLoadFVMStream_1YD() : this(new BedLoadParams1D ()){}
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public BedLoadFVMStream_1YD(BedLoadParams1D p) : base(p)
        {
            normaTheta = SPhysics.PHYS.normaTheta;
            name = "деформация поперечного дна (d50) FVM + dGx";
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            if (sp != null)
            {
                base.AddMeshPolesForGraphics(sp);
                double tau0 = SPhysics.PHYS.tau0;
                double[][] Gs = null;
                double[][] dGs = null;
                Calk_Gs(ref Gs, ref dGs, tau);
                if (Gs != null)
                {
                    MEM.AllocClear(Count, ref Theta);
                    for (int i = 0; i < tau.Length; i++)
                        Theta[i] = tau[i] / normaTheta;
                    sp.AddCurve("Параметр Шильдса", X, Theta);

                    if (Gs.Length == 3)
                    {
                        sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit]);
                        sp.AddCurve("Гравитационный расход наносов", X, Gs[idxZeta]);
                        sp.AddCurve("Полный расход наносов", X, Gs[idxAll]);

                        sp.AddCurve("Градиент транзитного расхода наносов", X, dGs[idxTransit]);
                        sp.AddCurve("Градиент гравитационного расхода наносов", X, dGs[idxZeta]);
                        sp.AddCurve("Градиент полного расхода наносов", X, dGs[idxAll]);
                    }
                    else
                    {
                        sp.AddCurve("Транзитный расход наносов", X, Gs[idxTransit]);
                        sp.AddCurve("Гравитационный расход наносов", X, Gs[idxZeta]);
                        sp.AddCurve("Напоный расход наносов", X, Gs[idxPress]);
                        sp.AddCurve("Полный расход наносов", X, Gs[idxAll]);

                        sp.AddCurve("Градиент транзитного расхода наносов", X, dGs[idxTransit]);
                        sp.AddCurve("Градиент гравитационного расхода наносов", X, dGs[idxZeta]);
                        sp.AddCurve("Градиент напорного расхода наносов", X, dGs[idxPress]);
                        sp.AddCurve("Градиент полного расхода наносов", X, dGs[idxAll]);
                    }
                }
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="Zeta0">текущая форма дна</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <returns>новая форма дна</returns>
        /// </summary>
        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta">текущая вычесленная форма дна</param>
        /// <param name="tau">придонное касательное напряжение в 
        /// потоковом направлении - ось Х</param>
        /// <param name="tauY">придонное касательное напряжение в 
        /// направлении - ось створа У</param>
        /// <param name="P"></param>
        /// <param name="CS">фракции взвешенных наносов</param>
        public override void CalkZetaFDM(ref double[] Zeta, 
                        double[] tau, double[] tauY = null, 
                        double[] P = null, double[][] CS = null)
        {
            try
            {
                double tanphi = SPhysics.PHYS.tanphi;
                double G1 = SPhysics.PHYS.G1;
                double epsilon = SPhysics.PHYS.epsilon;
                double tau0 = SPhysics.PHYS.tau0;
                double Fa0 = SPhysics.PHYS.Fa0;
                double gamma = SPhysics.PHYS.gamma;
                double RaC = SPhysics.PHYS.RaC;
                double Ws = SPhysics.PHYS.Ws;
                double d50 = SPhysics.PHYS.d50;

                this.tau = tau;
                this.P = P;
                MEM.Alloc(Zeta0.Length, ref Zeta);

                #region расчет расходов влекомых наносов
                double dz = 0, mdz = 0;
                double Wc = 0.1;
                double tauC0, K, tauC;
                double C0 = 16.0 / (15.0 * kappa * Math.Sqrt(rho_w) * Fa0 * (1 - epsilon) * tanphi * tanphi);
                double[] dZ = new double[Zeta.Length];
                //производная по поверхности дна 
                for (int i = 0; i < Zeta.Length - 1; i++)
                    dZ[i] = (Zeta0[i + 1] - Zeta0[i]) / dx;
                for (int i = 0; i < Zeta0.Length - 1; i++)
                {
                    dz = dZ[i];
                    mdz = Math.Abs(dz);
                    // косинус гамма
                    double CosGamma1 = Math.Sqrt(1 / (1 + dz * dz));
                    // Критические напряжения
                    tauC0 = CosGamma1 * tau0;
                    K = dz / tanphi;
                    // напряжение начала трогания донных частиц
                    tauC = tauC0 * (1 + K);
                    // при высоких уклонах диаметр частиц не влияет на процесс обрушения
                    if (K <= -1)
                        tauC = 0;
                    // Поперечный расход
                    if (tau[i] > tauC)
                    {
                        Gs[idxTransit][i] = 0;
                        Gs[idxZeta][i] = - C0 * Math.Pow(tau[i], 1.5) * dz;///CosGamma; 
                    }
                    else
                    {
                        Gs[idxTransit][i] = 0;
                        Gs[idxZeta][i] = 0; 
                    }
                }
                 // ГУ
                Gs[idxZeta][Gs.Length - 1] = Gs[idxZeta][Gs.Length - 2];
                #endregion
                // расчет текущего шага по времени
                dGs[idxZeta][0] = 0;  
                for (int i = 1; i < Gs.Length; i++)
                    dGs[idxZeta][i] = Gs[idxZeta][i] - Gs[idxZeta][i - 1];

                dGs[idxZeta][0] = dGs[idxZeta][1];
                double WC = dtime / (1 - epsilon) / dx;
                double[] dZeta = new double[Zeta.Length];
                for (int i = 0; i < Zeta.Length; i++)
                    dZeta[i] = - Wc * dGs[idxZeta][i];

                //расчет донных изменений Zeta
                for (int i = 0; i < Zeta.Length; i++)
                {
                    if (Math.Abs(dZeta[i]) > 0.2)
                        throw new Exception("Бяда!");
                    Zeta[i] = Zeta[i] + dZeta[i];
                }
                // Сглаживание дна по лавинной моделе
                if (Params.isAvalanche == AvalancheType.AvalancheSimple)
                    avalanche.Lavina(ref Zeta);
                // переопределение начального значения zeta 
                // для следующего шага по времени
                for (int j = 0; j < Zeta.Length; j++)
                {
                    bool fn = double.IsNaN(Zeta[j]);
                    if (fn == false)
                        Zeta0[j] = Zeta[j];
                    else
                        throw new Exception("реализовалось деление на ноль");
                }

            }
            catch (Exception e)
            {
                Message = e.Message;
            }
        }
    }
}
