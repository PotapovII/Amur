//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          19.12.2021
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    /// <summary>
    /// ОО: Реализация класса SAvalanche вычисляющего осыпание склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// </summary>
    [Serializable]
    class SAvalanche
    {
        /// <summary>
        /// Метод лавинного обрушения правого склонов 
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс угла внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public static void Lavina(double[] Z, double[] ds,
                         double tf, double Relax, int Step)
        {
            int NN = (int)(10.0 / (Relax + 0.1)) + 1;
            for (int i = 0; i < NN; i++)
            {
                LavinaRight(Z, ds, tf, Relax, Step);
                LavinaLeft(Z, ds, tf, Relax, Step);
            }
        }
        /// <summary>
        /// Метод лавинного обрушения правого склона, 
        /// обрушение с правой стороны
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public static void LavinaRight(double[] Z, double[] ds,
                                double tf, double Relax, int Step)
        {

            int idx = 0;
            for (int i = 1; i < Z.Length; i++)
            {
                double dh = (ds[i] - ds[i - 1]) * tf;
                double dz = Z[i] - Z[i - 1];
                if (dz < 0)
                {
                    if (-dz > dh)
                    {
                        double delta = (dh + dz) * Relax;
                        Z[i] -= delta;
                        Z[i - 1] += delta;
                        idx++;
                        if (idx == Step)
                            break;
                    }

                }
            }
        }
        /// <summary>
        /// Метод лавинного обрушения правого склона,
        /// обрушение с левой стороны
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public static void LavinaLeft(double[] Z, double[] ds,
                            double tf, double Relax, int Step)
        {

            int idx = 0;
            for (int i = 1; i < Z.Length; i++)
            {
                double dh = (ds[i] - ds[i - 1]) * tf;
                int k = Z.Length - i - 1;
                int kp = Z.Length - i;
                double dz = Z[kp] - Z[k];
                if (dz > 0)
                {
                    if (dz > dh)
                    {
                        double delta = (dz - dh) * Relax;
                        Z[k] += delta;
                        Z[kp] -= delta;
                        idx++;
                        if (idx == Step)
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// Метод лавинного обрушения склонов с учетом многофракционности материала
        /// </summary>
        /// <param name="fraction"></param>
        /// <param name="ha"></param>
        /// <param name="fraction0"></param>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public static void LavinaMix(double[][] fraction, double[] ha, double[] fraction0,
            double[] Z, double[] ds, double tf, double Relax, int Step)
        {
            for (int i = 0; i < 3; i++)
            {
                LavinaRightMix(fraction, ha, fraction0, Z, ds, tf, Relax, Step);
                LavinaRightMix(fraction, ha, fraction0, Z, ds, tf, Relax, Step);
            }
        }
        /// <summary>
        /// Метод лавинного обрушения правого склона, 
        /// обрушение с правой стороны
        /// </summary>
        /// <param name="fraction"></param>
        /// <param name="ha"></param>
        /// <param name="fraction0"></param>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public static void LavinaRightMix(double[][] fraction, double[] ha, double[] fraction0,
            double[] Z, double[] ds, double tf, double Relax, int Step)
        {
            int idx = 0;
            for (int i = 1; i < Z.Length; i++)
            {
                double dh = (ds[i] - ds[i - 1]) * tf;
                double dz = Z[i] - Z[i - 1];
                if (dz < 0)
                {
                    if (-dz > dh)
                    {
                        double delta = (dh + dz) * Relax;
                        Z[i] -= delta;
                        Z[i - 1] += delta;
                        for (int f = 0; f < fraction.Length; f++)
                        {
                            fraction[f][i - 1] = fraction[f][i - 1] * Math.Max(0, ha[i - 1] - delta) + fraction0[f] * delta;
                            fraction[f][i] = fraction[f][i - 1] * delta + fraction[f][i] * Math.Max(0, ha[i] - delta);
                        }
                        idx++;
                        if (idx == Step)
                            break;
                    }

                }
            }
        }
        /// <summary>
        /// Метод лавинного обрушения правого склона,
        /// обрушение с левой стороны
        /// </summary>
        /// <param name="fraction"></param>
        /// <param name="ha"></param>
        /// <param name="fraction0"></param>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public static void LavinaLeftMix(double[][] fraction, double[] ha, double[] fraction0,
            double[] Z, double[] ds, double tf, double Relax, int Step)
        {

            int idx = 0;
            for (int i = 1; i < Z.Length; i++)
            {
                double dh = (ds[i] - ds[i - 1]) * tf;
                int k = Z.Length - i - 1;
                int kp = Z.Length - i;
                double dz = Z[kp] - Z[k];
                if (dz > 0)
                {
                    if (dz > dh)
                    {
                        double delta = (dz - dh) * Relax;
                        Z[k] += delta;
                        Z[kp] -= delta;
                        for (int f = 0; f < fraction.Length; f++)
                        {
                            fraction[f][i] = fraction[f][i] * Math.Max(0, ha[i] - delta) + fraction0[f] * delta;
                            fraction[f][i - 1] = fraction[f][i] * delta + fraction[f][i - 1] * Math.Max(0, ha[i - 1] - delta);
                        }
                        idx++;
                        if (idx == Step)
                            break;
                    }
                }
            }
        }
    }
}
