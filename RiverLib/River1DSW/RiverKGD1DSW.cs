//---------------------------------------------------------------------------
//                    ПРОЕКТ  "Донная устойчивость"
//                  30.12.11 Потапов Игорь Иванович
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 23.02.2021 Потапов И.И. 
//---------------------------------------------------------------------------
namespace RiverLib
{
    using CommonLib;
    using CommonLib.Physics;
    using System;
    using System.Linq;
    /// <summary>
    /// ОО: Нестационарное решение одномерного уравгнения мелкой воды 
    /// на сетке с переменным шагом, явная схема 
    /// Елизарова Т.Г., Злотник А.А., Никитина О.В.
    /// </summary>
    [Serializable]
    public class RiverKGD1DSW : ARiver1DSW
    {
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverKGD1DSW(new River1DSWParams());
        }
        /// <summary>
        /// шаг по пространству (м)
        /// </summary>
        public double dx;
        /// <summary>
        /// шаг по времени (с)
        /// </summary>
        public double dt;
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
        public double[] mu;
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
        public RiverKGD1DSW(River1DSWParams p, IMesh mesh = null, IAlgebra algebra = null) :
             base(p, mesh, algebra)
        {
            Init();
            name = "решение задачи мелкой воды (КГД от Елизаровой Т.Г.)";
        }
        void Init()
        {
            int CountKnots = Params.CountKnots;
            double Mu = SPhysics.mu;
            time = 0;
            CountElems = CountKnots - 1;
            // шаг сетки
            U = new double[CountKnots];
            H = new double[CountKnots];
            UN = new double[CountKnots];
            HN = new double[CountKnots];
            mu = new double[CountKnots];
            Tau = new double[CountKnots];
            tau = new double[CountKnots];
            c = new double[CountKnots];
            Jm = new double[CountElems];
            W = new double[CountElems];
            PI = new double[CountElems];
            mU = new double[CountElems];
            mH = new double[CountElems];
            mZeta = new double[CountElems];
            mtau = new double[CountElems];
            mTau = new double[CountElems];
            for (int i = 0; i < CountKnots; i++)
            {
                mu[i] = Mu;
                U[i] = Params.U0;
                H[i] = Params.H0;
            }
        }

        /// <summary>
        /// Решатель задачи
        /// </summary>
        /// <param name="zeta">уровень дна</param>
        public override void SolverStep()
        {
            double rho_w = SPhysics.rho_w;
            double g = SPhysics.GRAV;
            double Mu = SPhysics.mu;

            double dU, dH, dZeta, mMu;
            double U2p, U2;
            double H2p, H2;
            double pi0, pi1, pi2;
            double w0, w1;
            double _H, q0, q1, q2;
            double g23 = 2.0 / 3.0 * g;
            double Vmax;
            MaxW = 0;
            #region Решение задачи мелкой воды
            // расчет шага по времени dt коэффициента вязкости mu[i] 
            // и параметра регуляризации tau[i] с учетом возможного сухого дна потока 
            for (int i = 0; i < c.Length; i++)
                c[i] = Math.Sqrt(g * H[i]);
            Hmax = H.Max();
            Cmax = Math.Sqrt(g * Hmax);
            Vmax = Math.Abs(U.Max());
            if (Hmax / 10 > Params.H0)
                throw new Exception("Бяда Hmax = " + Hmax.ToString());
            gamma = delta * Hmax;
            // шаг по времени
            dt = betta * dx / (Cmax + gamma + Vmax);
            MaxMu = 0;
            dx = 0;
            for (int i = 0; i < x.Length - 1; i++)
                dx = Math.Max(dx, x[i + 1] - x[i]);
            // объемная сила 
            // double Qm = J * g; // !!!!!!!!!!!!!!!!!!
            for (int i = 0; i < c.Length; i++)
            {
                // время релаксации
                if (Math.Abs(H[i]) > errH)
                    tau[i] = alpha * dx / (c[i] + Vmax);
                else
                    tau[i] = alpha * dx / (c[i] + gamma + Vmax);
                // отрелаксированная вязкость потока
                mu[i] = g23 * tau[i] * H[i] * H[i] + Mu;
                if (mu[i] > MaxMu)
                    MaxMu = mu[i];
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
                mMu = 0.5 * (mu[i + 1] + mu[i]);
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
                pi1 = mMu * dU + g * pi0 * ((U[i + 1] * H[i + 1] - U[i] * H[i])) / dx;
                pi2 = pi0 * mU[i] * (g * dH + 0.5 * (U2p - U2) / dx + g * dZeta - mTau[i]);
                PI[i] = pi1 + pi2;
                // расчет скорости релаксации
                w0 = g * (0.5 * (H2p - H2) / dx + mH[i] * dZeta) - mH[i] * mTau[i];
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
                HN[i] = H[i] - dt / dx * (Jm[i] - Jm[i - 1]);
                // вспомогательыне выражения
                _H = 0.5 * (mH[i] + mH[i - 1]) - tau[i] * (mH[i] * mU[i] + mH[i - 1] * mU[i - 1]) / dx;
                q0 = (mU[i] * Jm[i] - mU[i - 1] * Jm[i - 1]) / dx;
                q1 = 0.5 * g * (mH[i] * mH[i] - mH[i - 1] * mH[i - 1]) / dx;
                q2 = (PI[i] - PI[i - 1]) / dx + _H * (Tau[i] - g * (mZeta[i] - mZeta[i - 1]) / dx);
                // расход на новом слое по времени
                UN[i] = U[i] * H[i] - dt * (q0 + q1 - q2);
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
            sp.AddCurve("Глубина потока", x, H);
            sp.AddCurve("Средняя скорость потока", x, U);
            sp.AddCurve("Время релаксации потока", x, tau);
            sp.AddCurve("Отрелаксированная вязкость потока", x, mu);
        }
        /// <summary>
        /// Тесты
        /// </summary>
        //public void Examples(int testNamber = 0)
        //{
        //    double Lambda = 0.01;
        //    double Maning = 0.2;
        //    double Mu = 0.01;
        //    double J = 0.0001;
        //    double rho_w = 1000;
        //    TypeSWETau typeTau = TypeSWETau.Darcy;
        //    RiverKGD1DSW p = new RiverKGD1DSW(typeTau, Lambda, Maning, Mu, J, rho_w);
        //    int CountKnots = 20;
        //    double U0 = 1;
        //    double H0 = 1;
        //    int typeBC = 1;
        //    RiverShallowWaterBC bc = new RiverShallowWaterBC(U0, H0, typeBC);
        //    RiverKGD1DSW task = new RiverKGD1DSW(CountKnots, bc, p);
        //    double[] Tau = new double[CountKnots];
        //    double[] U = new double[CountKnots];
        //    double[] H = new double[CountKnots];
        //    double[] zeta = new double[CountKnots];
        //    double[] x = new double[CountKnots];
        //    double L = 100;
        //    double dx = L / (CountKnots - 1);
        //    double z0 = J * L;
        //    double dz = z0 / CountKnots;
        //    for (int i = 0; i < U.Length; i++)
        //    {
        //        x[i] = i*dx;
        //        zeta[i] = z0 - i*dz;
        //        U[i] = U0;
        //        H[i] = H0;
        //    }
        //    task.Set(U, H);
        //    // Задача с ровным дном
        //    for (int time = 0; time < 50; time++)
        //    {
        //        Console.WriteLine(time);
        //        // Задача с ровным дном
        //        task.SolverStep(ref x, ref zeta, Ks);
        //        LOG.Print("U", task.U);
        //        LOG.Print("H", task.H);
        //        LOG.Print("zeta", zeta);
        //        task.CalkTau(ref Tau);
        //        LOG.Print("Tau", Tau);
        //    }
        //}
    }
}
