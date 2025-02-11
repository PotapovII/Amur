//---------------------------------------------------------------------------
//                    ПРОЕКТ  "Донная устойчивость"
//                  30.12.11 Потапов Игорь Иванович
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 23.02.2021 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 20.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver_1XD.River1DSW
{
    using System;
    using System.Linq;

    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using NPRiverLib.IO;
    using MeshGeneratorsLib.TapeGenerator;
    /// <summary>
    /// ОО: Нестационарное решение одномерного уравгнения мелкой воды 
    /// на сетке с переменным шагом, явная схема 
    /// Елизарова Т.Г., Злотник А.А., Никитина О.В.
    /// </summary>
    [Serializable]
    public class RiverSWE_KGD1XD : ARiverSWE1XD
    {
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverSWE_KGD1XD(new RiverSWEParams1XD());
        }
        /// <summary>
        /// шаг по пространству (м)
        /// </summary>
        public double dx;
        /// <summary>
        /// шаг по времени (с)
        /// </summary>
        //public double dt;
        /// <summary>
        /// количество элементов
        /// </summary>
        int CountElems;
        #region Основные неизвестные в узлах сетки CountKnots
        /// <summary>
        /// средняя скорость  на новом слое по времени
        /// </summary>
        public double[] UN;
        /// <summary>
        /// средняя глубина потока на новом слое по времени
        /// </summary>
        public double[] HN;
        /// <summary>
        /// внешние силы
        /// </summary>
        public double[] Tau;
        /// <summary>
        /// коэффициент релаксации
        /// </summary>
        public double[] tau;
        /// <summary>
        /// фиктивная турбулентная вязкость
        /// </summary>
        public double[] mu_e;
        /// <summary>
        /// аналог скорости звука
        /// </summary>
        public double[] c;
        #endregion
        #region Вспомогательные неизвестные в центрах тяжести межузловых элементов
        /// <summary>
        /// средняя скорость
        /// </summary>
        double[] mU;
        /// <summary>
        /// средняя глубина потока 
        /// </summary>
        double[] mH;
        /// <summary>
        /// внешние силы
        /// </summary>
        double[] mTau;
        /// <summary>
        /// уровень дна
        /// </summary>
        double[] mZeta;
        /// <summary>
        /// коэффициент релаксации
        /// </summary>
        double[] mtau;
        /// <summary>
        /// поток
        /// </summary>
        double[] Jm;
        /// <summary>
        /// поправка скорости
        /// </summary>
        double[] W;
        /// <summary>
        /// приведенное напряжение
        /// </summary>
        double[] PI;
        #endregion
        /// <summary>
        /// параметр Курранта
        /// </summary>
        public double betta = 0.4; // 0 < betta < 1
        public double alpha = 0.1;
        public double delta = 0.1;
        /// <summary>
        /// минимальная глубина "мокрого" потока 
        /// </summary>
        double errH = 0.001;
        /// <summary>
        /// параметры схемы
        /// </summary>
        double Cmax, Hmax, gamma;
        public string Message = "Ok";
        /// <summary>
        /// максимальное среднеквадратичное отклонение
        /// </summary>
        public double MaxW;
        /// <summary>
        /// максимальная фиктивная вязкость
        /// </summary>
        public double MaxMu;
        // ---------------------------------------------------------------------------------
        public RiverSWE_KGD1XD(RiverSWEParams1XD p) : base(p)
        {
            name = "Задача мелкой воды 1D,(КГД от Елизаровой Т.Г.)";
            Version = "RiverSWE_KGD1XD 20.07.2024";
        }

        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            time = 0;
            int CountKnots = Params.CountKnots;
            CountElems = CountKnots - 1;
            // шаг сетки
            MEM.Alloc<double>(CountKnots, ref U);
            MEM.Alloc<double>(CountKnots, ref H);
            MEM.Alloc<double>(CountKnots, ref UN);
            MEM.Alloc<double>(CountKnots, ref HN);
            MEM.Alloc<double>(CountKnots, ref mu_e);
            MEM.Alloc<double>(CountKnots, ref Tau);
            MEM.Alloc<double>(CountKnots, ref tau);
            MEM.Alloc<double>(CountKnots, ref c);
            MEM.Alloc<double>(CountElems, ref Jm);
            MEM.Alloc<double>(CountElems, ref W);
            MEM.Alloc<double>(CountElems, ref PI);
            MEM.Alloc<double>(CountElems, ref mU);
            MEM.Alloc<double>(CountElems, ref mH);
            MEM.Alloc<double>(CountElems, ref mZeta);
            MEM.Alloc<double>(CountElems, ref mtau);
            MEM.Alloc<double>(CountElems, ref mTau);
            for (int i = 0; i < CountKnots; i++)
            {
                mu_e[i] = mu;
                U[i] = Params.U0;
                H[i] = Params.H0;
            }
            mesh = TapeMeshGenerator.CreateMesh(x, zeta, H);
            base.Set(mesh, algebra);
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Решатель задачи
        /// </summary>
        /// <param name="zeta">уровень дна</param>
        public override void SolverStep()
        {
            double dU, dH, dZeta, mMu;
            double U2p, U2;
            double H2p, H2;
            double pi0, pi1, pi2;
            double w0, w1;
            double _H, q0, q1, q2;
            double g23 = 2.0 / 3.0 * GRAV;
            double Vmax;
            MaxW = 0;
            #region Решение задачи мелкой воды
            // расчет шага по времени dt коэффициента вязкости mu_e[i] 
            // и параметра регуляризации tau[i] с учетом возможного сухого дна потока 
            for (int i = 0; i < c.Length; i++)
                c[i] = Math.Sqrt(GRAV * H[i]);
            Hmax = H.Max();
            Cmax = Math.Sqrt(GRAV * Hmax);
            Vmax = Math.Abs(U.Max());
            if (Hmax / 10 > Params.H0)
                throw new Exception("Бяда Hmax = " + Hmax.ToString());
            gamma = delta * Hmax;
            // шаг по времени
            dtime = betta * dx / (Cmax + gamma + Vmax);
            MaxMu = 0;
            dx = 0;
            for (int i = 0; i < x.Length - 1; i++)
                dx = Math.Max(dx, x[i + 1] - x[i]);
            // объемная сила 
            // double Qm = J * GRAV; // !!!!!!!!!!!!!!!!!!
            for (int i = 0; i < c.Length; i++)
            {
                // время релаксации
                if (Math.Abs(H[i]) > errH)
                    tau[i] = alpha * dx / (c[i] + Vmax);
                else
                    tau[i] = alpha * dx / (c[i] + gamma + Vmax);
                // отрелаксированная вязкость потока
                mu_e[i] = g23 * tau[i] * H[i] * H[i] + mu;
                if (mu_e[i] > MaxMu)
                    MaxMu = mu_e[i];
                // торможение
                double Ft = CalkTau(U[i], H[i], 0) / rho_w;
                // разгон
                double Fg = CalkTau(U[i], H[i], 2) / rho_w;
                Tau[i] = Fg - Ft;
            }
            // Расчет основных параметров (потоков, приведенного напряжения и
            // скорости релаксации в центрах элементов
            for (int i = 0; i < CountElems; i++)
            {
                // расчет основных неизвестных в центрах элементов
                // скорости
                mU[i] = 0.5 * (U[i + 1] + U[i]);
                // глубины
                mH[i] = 0.5 * (H[i + 1] + H[i]);
                // дна
                mZeta[i] = 0.5 * (zeta[i + 1] + zeta[i]);
                // сил (напряжений)
                mTau[i] = 0.5 * (Tau[i + 1] + Tau[i]);
                // времени релаксации
                mtau[i] = 0.5 * (tau[i + 1] + tau[i]);
                // вязкости
                mMu = 0.5 * (mu_e[i + 1] + mu_e[i]);
                // квадраты функций
                U2 = U[i] * U[i]; U2p = U[i + 1] * U[i + 1];
                H2 = H[i] * H[i]; H2p = H[i + 1] * H[i + 1];
                // расчет основных производных в центрах элементов
                dx = x[i + 1] - x[i];
                dU = (U[i + 1] - U[i]) / dx;
                dH = (H[i + 1] - H[i]) / dx;
                dZeta = (zeta[i + 1] - zeta[i]) / dx;
                // расчет приведенного напряжения
                pi0 = mH[i] * mtau[i];
                pi1 = mMu * dU + GRAV * pi0 * ((U[i + 1] * H[i + 1] - U[i] * H[i])) / dx;
                pi2 = pi0 * mU[i] * (GRAV * dH + 0.5 * (U2p - U2) / dx + GRAV * dZeta - mTau[i]);
                PI[i] = pi1 + pi2;
                // расчет скорости релаксации
                w0 = GRAV * (0.5 * (H2p - H2) / dx + mH[i] * dZeta) - mH[i] * mTau[i];
                w1 = (U2p * H[i + 1] - U2 * H[i]) / dx;
                W[i] = mtau[i] / mH[i] * (w0 + w1);
                // отрелаксированный поток для уравнения неразрывности
                Jm[i] = mH[i] * (mU[i] - W[i]);
            }
            // вычислени глубины и скорости на новом временном слое
            for (int i = 1; i < CountElems; i++)
            {
                dx = x[i] - x[i - 1];
                // глубина но новом слое по времени
                HN[i] = H[i] - dtime / dx * (Jm[i] - Jm[i - 1]);
                // вспомогательыне выражения
                _H = 0.5 * (mH[i] + mH[i - 1]) - tau[i] * (mH[i] * mU[i] + mH[i - 1] * mU[i - 1]) / dx;
                q0 = (mU[i] * Jm[i] - mU[i - 1] * Jm[i - 1]) / dx;
                q1 = 0.5 * GRAV * (mH[i] * mH[i] - mH[i - 1] * mH[i - 1]) / dx;
                q2 = (PI[i] - PI[i - 1]) / dx + _H * (Tau[i] - GRAV * (mZeta[i] - mZeta[i - 1]) / dx);
                // расход на новом слое по времени
                UN[i] = U[i] * H[i] - dtime * (q0 + q1 - q2);
                // вычисление скорости потока с учетом сухого дна
                if (Math.Abs(HN[i]) < errH)
                    UN[i] = 0;
                else
                    UN[i] = UN[i] / HN[i];
            }
            #region  Выполнение граничных условий задачи

            switch (Params.typeBCInlet)
            {
                case 0:
                    // На левой границе сободна глубина
                    U[0] = Params.U0;
                    // На правой границе свободна скорость потока с фиксированной глубиной H0
                    H[H.Length - 1] = Params.H0;
                    break;
                case 1: // хорошая генерация неустойчивости потока
                        // На левой границе сободна глубина
                    U[0] = Params.U0;
                    H[0] = 2 * H[1] - H[2];
                    // На правой границе свободна скорость потока с фиксированной глубиной H0
                    U[H.Length - 1] = 2 * U[H.Length - 2] - U[H.Length - 3];
                    break;
                case 2:
                    H[0] = Params.H0;
                    U[H.Length - 1] = Params.U0;
                    break;
                case 3:
                    H[0] = Params.H0;
                    H[H.Length - 1] = H[0];
                    U[0] = Params.U0;
                    U[H.Length - 1] = Params.U0;
                    break;
                case 4:
                    H[0] = Params.H0;
                    U[H.Length - 1] = Params.U0;
                    break;
                case 5:  // Переодические граничные условия
                         // На левой границе сободна глубина
                    U[0] = 2 * U[1] - U[2];// + VRegim;
                    U[U.Length - 1] = 2 * U[U.Length - 2] - U[U.Length - 3];
                    // На правой границе свободна скорость потока с фиксированной глубиной H0
                    H[0] = Params.H0;
                    H[H.Length - 1] = Params.H0;
                    break;
                case 6:  // Переодические граничные условия
                         // На левой границе сободна глубина
                         //U[0] = 2 * U[1] - U[2] + VRegim;
                         //U[U.Length - 1] = 2 * U[U.Length - 2] - U[U.Length - 3] + VRegim;// + VRegim2;
                    U[0] = U[U.Length - 1];
                    // U[U.Length - 1] = 0;
                    // На правой границе свободна скорость потока с фиксированной глубиной H0
                    H[0] = Params.H0;
                    //H[H.Length - 1] = H0;
                    //H[0] = 2 * H[1] - H[2];
                    // H[H.Length - 1] = 2 * H[H.Length - 2] - H[H.Length - 3];
                    break;
            }
            #endregion
            // сдвиг расчетного слоя
            for (int i = 1; i < H.Length - 1; i++)
            {
                H[i] = HN[i];
                U[i] = UN[i];
                // поправка свободной поверхности
                Eta[i] = zeta[i] + H[i];
                if (double.IsNaN(U[i]))
                    throw new Exception("Идем в разнос! Очень много цифер Нах");
            }
            #endregion
            // приращение текущего расчетного времени 
            time += dtime;
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, H);
            sp.Add("Глубина потока", rezult);
            sp.AddCurve("Глубина потока", x, H);
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, U);
            sp.Add("Средняя скорость потока", rezult);
            sp.AddCurve("Средняя скорость потока", x, U);
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, tau);
            sp.Add("Время релаксации потока", rezult);
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, mu_e);
            sp.Add("Отрелаксированная вязкость потока", rezult);
            double[] Tau = null;
            CalkTau(ref Tau);
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, Tau);
            sp.Add("Придонные касательные напряжения", rezult);
            sp.AddCurve("Придонные касательные напряжения", x, Tau);

            sp.AddCurve("Свободная поверхность потока", x, Eta);
            sp.AddCurve("Время релаксации потока", x, tau);
            sp.AddCurve("Отрелаксированная вязкость потока", x, mu_e);
        }
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReaderSWE_1XD();
        }
    }
}
