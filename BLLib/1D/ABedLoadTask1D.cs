//---------------------------------------------------------------------------
//                 Реализация библиотеки для моделирования 
//                  гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                Модуль BLLib для расчета донных деформаций 
//                 (учет движения только влекомых наносов)
//         по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//                              Потапов И.И.
//                               14.04.21
//---------------------------------------------------------------------------

namespace BLLib
{
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using System;

    /// <summary>
    /// ОО: Абстрактный класс для 1D задач,
    /// реализует общий интерфейст задач по деформациям дна
    /// </summary>
    [Serializable]
    public abstract class ABedLoadTask1D : ABedLoadTask
    {
        #region  Свойства
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public new string VersionData() => "ABedLoadTask1D 01.09.2021"; 
        protected double us, AB;
        /// <summary>
        /// Расход наносов на КО
        /// </summary>
        protected double[][] Gs = null;
        /// <summary>
        /// Расход градиентов наносов на КО
        /// </summary>
        protected double[][] dGs = null;
        /// <summary>
        /// координаты центров КО
        /// </summary>
        protected double[] X = null;
        /// <summary>
        /// критическое напряжение на КО
        /// </summary>
        public double[] tau0Elem = null;
        /// <summary>
        /// отметки дна в центрах КО
        /// </summary>
        protected double[] zetaElem = null;
        #endregion 

        #region Краевые условия
        /// <summary>
        /// транзитный расход на входе
        /// </summary>
        public double Gtran_in = 0;
        /// <summary>
        /// транзитный расход на выходе
        /// </summary>
        public double Gtran_out = 0;
        #endregion

        #region Служебные переменные
        /// <summary>
        /// Количество расчетных подобластей
        /// </summary>
        protected int N;
        /// <summary>
        ///  косинус гамма - косинус угола между 
        ///  нормалью к дну и вертикальной осью
        /// </summary>
        protected double[] CosGamma = null;

        protected double[] G0 = null;
        protected double[] A = null;
        protected double[] B = null;
        protected double[] C = null;
        /// <summary>
        /// Поток взвешенных наносов по Х в улах
        /// </summary>        
        protected double[] GCx = null;

        protected double[] CE = null;
        protected double[] CW = null;
        protected double[] CP = null;
        protected double[] S = null;

        protected double[] AE = null;
        protected double[] AW = null;
        protected double[] AP = null;
        protected double[] AP0 = null;

        protected double[] ps = null;
        /// <summary>
        /// Флаг для определения сухого-мокрого дна
        /// </summary>
        public int[] DryWetEelm = null;
        /// <summary>
        /// длина расчетной области
        /// </summary>
        protected double L;
        /// <summary>
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int debug = 0;
        /// <summary>
        /// Поле сообщений о состоянии задачи
        /// </summary>
        public string Message = "Ok";
        #endregion
        #region Рабочие массивы
        /// <summary>
        /// массив координаты узлов
        /// </summary>
        public double[] x = null;
        #endregion
        /// <summary>
        /// Конструктор по умолчанию/тестовый
        /// </summary>
        public ABedLoadTask1D(BedLoadParams p) : base(p)
        {
        }
        /// <summary>
        /// Вычисление числа Курранта
        /// </summary>
        /// <returns></returns>
        public double Kurant()
        {
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double kurant;
            double minL = double.MaxValue;
            for (int i = 0; i < x.Length - 1; i++)
            {
                double dx = x[i + 1] - x[i];
                if (dx < minL)
                    minL = dx;
            }
            double minD = G1 * tau0 * Math.Sqrt(tau0);
            kurant = minL / (2.0 * minD);
            return kurant;
        }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, IBoundaryConditions BConditions)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            if (mesh.CountElements==0) return;
            base.SetTask(mesh, Zeta0, BConditions);
            this.x = mesh.GetCoords(0);
            if(mesh.GetCoords(1)!=null)
                this.Zeta0 = mesh.GetCoords(1);
            this.N = Count - 1;
            this.L = x[N] - x[0];
            // узловые массивы
            MEM.Alloc<double>(N, ref A);
            MEM.Alloc<double>(N, ref B);
            MEM.Alloc<double>(N, ref C);
            MEM.Alloc<double>(N, ref G0);
            MEM.Alloc<double>(N, ref dZeta);
            MEM.Alloc<double>(N, ref CosGamma);
            MEM.Alloc<int>(N, ref DryWetEelm);
            MEM.Alloc<double>(N, ref GCx);

            MEM.Alloc<int>(Count, ref DryWet);
            MEM.Alloc<double>(Count, ref Zeta);
            MEM.Alloc<double>(Count, ref ps);
            MEM.Alloc<double>(Count, ref CE);
            MEM.Alloc<double>(Count, ref CW);
            MEM.Alloc<double>(Count, ref CP);
            MEM.Alloc<double>(Count, ref S);
            MEM.Alloc<double>(Count, ref AE);
            MEM.Alloc<double>(Count, ref AW);
            MEM.Alloc<double>(Count, ref AP);
            MEM.Alloc<double>(Count, ref AP0);

            MEM.Alloc(mesh.CountElements, ref X);
            MEM.Alloc(mesh.CountElements, ref tau0Elem);
            MEM.Alloc(mesh.CountElements, ref zetaElem);

            avalanche = new Avalanche1DX_Old(mesh, tanphi, 0.6);

            taskReady = true;
        }
        /// <summary>
        /// Расчет производных и критических напряжений для однородного донного материала
        /// </summary>
        protected void CalkCritTauType()
        {
            double tanphi = SPhysics.PHYS.tanphi;
            double tau0 = SPhysics.PHYS.tau0;
          
            for (int i = 0; i < N; i++)
            {
                dZeta[i] = (Zeta0[i + 1] - Zeta0[i]) / (x[i + 1] - x[i]);
                CosGamma[i] = Math.Sqrt(1 / (1 + dZeta[i] * dZeta[i]));
                // критическое напряжение
            }
            // критическое напряжение
            //switch (сritTauType)
            //{
            //    // линейная знаковая модель
            //    case ECritTauType.LineCritTayType:
            // tau0Elem[0] = CosGamma[i] * Math.Max(0.5 * tau0, tau0 * (1 + dZeta[0] / tanphi));
            bool ft = false;
            if (ft == true)
                for (int i = 0; i < N; i++)
                {
                    //tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * (1 + dZeta[i] / tanphi));
                    tau0Elem[i] = CosGamma[i] * Math.Max(tau0, tau0 * (1 + dZeta[i] / tanphi));
                }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    //tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * (1 + dZeta[i] / tanphi));
                    tau0Elem[i] = CosGamma[i] * tau0;
                }
            }
            //        break;
            //    // линейная беззнаковая модель
            //    case ECritTauType.LineABSCritTayType:
            //        for (int i = 0; i < N; i++)
            //        {
            //            tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * (1 - Math.Abs(dZeta[i] / tanphi)));
            //        }
            //        break;
            //    // квадратичная беззнаковая модель
            //    case ECritTauType.SqrtCritTayType:
            //        for (int i = 0; i < N; i++)
            //        {
            //            tau0Elem[i] = CosGamma[i] * Math.Max(0.05 * tau0, tau0 * Math.Sqrt(1 - (dZeta[i] / tanphi) * (dZeta[i] / tanphi)));
            //        }
            //        break;
            //}
        }

        /// <summary>
        /// Конвертация элементных данных в узловые
        /// </summary>
        /// <param name="P_elem"></param>
        /// <param name="P_node"></param>
        /// <param name="scale"></param>
        protected void ConvertElemToNode(double[] P_elem, ref double[] P_node, double scale = 1)
        {
            if (P_elem == null)
            {
                for (int j = 1; j < N; j++)
                    P_node[j] = 0;
            }
            else
            if (P_elem.Length != P_node.Length)
            {
                for (int j = 1; j < N; j++)
                    P_node[j] = 0.5 * (P_elem[j] + P_elem[j - 1]) * scale;
                P_node[0] = (2 * P_elem[1] - P_elem[2]) * scale;
                P_node[N] = (2 * P_elem[N - 1] - P_elem[N - 2]) * scale;
            }
        }


        #region Методы отладки
        /// <summary>
        /// Тестовая печать поля
        /// </summary>
        /// <param name="Name">имя поля</param>
        /// <param name="mas">массив пля</param>
        /// <param name="FP">точность печати</param>
        public void PrintMas(string Name, double[] mas, int FP = 8)
        {
            string Format = " {0:F6}";
            if (FP != 6)
                Format = " {0:F" + FP.ToString() + "}";

            Console.WriteLine(Name);
            for (int i = 0; i < mas.Length; i++)
            {
                Console.Write(Format, mas[i]);
            }
            Console.WriteLine();
        }
        public void PrintMatrix(int FP = 8)
        {
            string Format = " {0:F6}";
            if (FP != 6)
                Format = " {0:F" + FP.ToString() + "}";

            for (int i = 0; i < AP.Length; i++)
            {
                for (int j = 0; j < AP.Length; j++)
                {
                    double a = 0;
                    if (i == j + 1)
                        a = AW[i];
                    if (i == j)
                        a = AP[i];
                    if (i == j - 1)
                        a = AE[i];
                    Console.Write(Format, a);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        #endregion
    }
}
