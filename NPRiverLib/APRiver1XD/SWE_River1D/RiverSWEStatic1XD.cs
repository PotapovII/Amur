//---------------------------------------------------------------------------
//             (С) Программист: Потапов Игорь Иванович
//                 дата кодировки: 04 12 2009
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 23.02.2021 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 23.07.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver_1XD.River1DSW
{
    using MeshLib;
    using MemLogLib;

    using CommonLib;
    using CommonLib.Physics;

    using System;
    using MeshGeneratorsLib.TapeGenerator;
    using CommonLib.IO;
    using NPRiverLib.IO;

    /// <summary>
    /// ОО: Cтационарное решение одномерного уравнения мелкой воды 
    /// на сетке с переменным шагом и переменным коэффициентом шероховатости
    /// </summary>
    [Serializable]
    public class RiverSWEStatic1XD : ARiverSWE1XD
    {
        public override IRiver Clone()
        {
            return new RiverSWEStatic1XD(new RiverSWEParams1XD());
        }
        public RiverSWEStatic1XD(RiverSWEParams1XD p) :base(p)
        {
            name = "Задача мелкой воды 1D,(обратная прогонка)";
            Version = "RiverSWEStatic1XD 20.07.2024";
        }
        /// <summary>
        /// Решатель задачи
        /// </summary>
        /// <param name="Zeta">уровень дна</param>
        public override void SolverStep()
        {
            double g = SPhysics.GRAV;
            double delta;
            //double n2 = Maning * Maning; 
            //double LambdaJ;
            double Fr;
            double Q = Params.U0 * Params.H0;
            double d = 0.001;
            // Расчет глубин
            for (int j = U.Length - 1; j > 0; j--)
            {
                double U2 = U[j] * U[j];
                double Hj = H[j];
                // текущий Фруд
                Fr = U2 / (g * (Hj + d));

                double dZeta = zeta[j] - zeta[j - 1];
                double dx = x[j] - x[j - 1];

                //if (Ks != null)
                //    LambdaJ = GetLambda(j);
                //else
                //    LambdaJ = Lambda;
                //delta = dx * (dZeta / dx + LambdaJ * Fr) / (1 - Fr * (1 - 3 * LambdaJ * dx / (2 * H[j])));

                double a2 = Params.Maning * Params.Maning;
                double n13 = 1.0 / 3.0;
                double tr = a2 * Math.Pow(d / (Hj + d), n13);
                double znam = Math.Max(d, 1 - Fr * (1 + tr * 5.0 * dx / (3.0 * Hj)));
                delta = (dZeta - dx * Fr * tr) / znam;

                if (Math.Abs(delta) > 0.1 * H[j])
                {
                    delta = dZeta;
                    // throw new Exception("Грубая сетка, склонность к распаду решения");
                }
                //if (double.IsNaN(delta) == true)
                //    delta = delta;
                // поправка глубин
                H[j - 1] = H[j] + delta;
                // поправка скоростей
                U[j - 1] = Q / H[j - 1];

                if (double.IsNaN(U[j - 1]) == true)
                    U[j] = U[j];
            }
            H[0] = H[0];
            // поправка свободной поверхности
            for (int j = 0; j < Eta.Length; j++)
                Eta[j] = zeta[j] + H[j];
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

            TapeMeshGenerator.Convert2DFrom1D(ref rezult, Eta);
            sp.Add("Свободная поверхность потока", rezult);
            sp.AddCurve("Свободная поверхность потока", x, Eta);

            double[] Tau = null;
            CalkTau(ref Tau);
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, Tau);
            sp.Add("Придонные касательные напряжения", rezult);
            sp.AddCurve("Придонные касательные напряжения", x, Tau);
        }
        /// <summary>
        /// Тесты
        /// </summary>
        public void Examples(int testNamber = 0)
        {
            RiverSWEParams1XD par = new RiverSWEParams1XD();
            par.Lambda = 0.005;
            par.Maning = 0.2;
            par.J = 0.0001;
            par.U0 = 1;
            par.H0 = 1;
            par.typeBCInlet = 0;
            par.typeBCOut = 1;
            par.CountKnots = 20;
            RiverSWEStatic1XD task = new RiverSWEStatic1XD(par);
            double L = 100;
            double[] Tau = new double[Params.CountKnots];
            double[] Zeta = new double[Params.CountKnots];
            double[] x = new double[Params.CountKnots];
            double dx = L / (Params.CountKnots - 1);
            double z0 = Params.J * L;
            double dz = z0 / Params.CountKnots;
            for (int i = 0; i < U.Length; i++)
            {
                x[i] = i * dx;
                Zeta[i] = z0 - i * dz;
            }
            task.Set(new TwoMesh(x, Zeta), null);
            // Задача с ровным дном
            task.SolverStep();
            LOG.Print("U", task.U);
            LOG.Print("H", task.H);
            LOG.Print("zeta", Zeta);
            task.CalkTau(ref Tau);
            LOG.Print("Tau", Tau);
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
