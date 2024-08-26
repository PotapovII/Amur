//---------------------------------------------------------------------------
//             (С) Программист: Потапов Игорь Иванович
//                 дата кодировки: 04 12 2009
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 23.02.2021 Потапов И.И. 
//---------------------------------------------------------------------------
namespace RiverLib
{
    using AlgebraLib;
    using CommonLib;
    using CommonLib.Physics;
    using MemLogLib;
    using MeshLib;
    using System;
    /// <summary>
    /// ОО: Cтационарное решение одномерного уравнения мелкой воды 
    /// на сетке с переменным шагом и переменным коэффициентом шероховатости
    /// </summary>
    [Serializable]
    public class RiverStatic1DSW : ARiver1DSW
    {
        public override IRiver Clone()
        {
            return new RiverStatic1DSW(new River1DSWParams());
        }
        /// <summary>
        /// свободная поверхность потока
        /// </summary>
        public double[] Eta;
        public RiverStatic1DSW(River1DSWParams p, IMesh mesh = null, IAlgebra algebra = null) :
            base(p, mesh, algebra)
        {
            MEM.Alloc<double>(Params.CountKnots, ref Eta);
            name = "решение задачи мелкой воды, обратной прогонкой";
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
            sp.AddCurve("Глубина потока", x, H);
            sp.AddCurve("Средняя скорость потока", x, U);
            sp.AddCurve("Свободная поверхность потока", x, Eta);
            double[] Tau = null;
            CalkTau(ref Tau);
            sp.AddCurve("Придонные касательные напряжения", x, Tau);
        }
        /// <summary>
        /// Тесты
        /// </summary>
        public void Examples(int testNamber = 0)
        {
            //par.Mu = 0.01;
            //par.rho_w = 1000;

            River1DSWParams par = new River1DSWParams();
            par.Lambda = 0.005;
            par.Maning = 0.2;
            par.J = 0.0001;
            par.U0 = 1;
            par.H0 = 1;
            par.typeBCInlet = 0;
            par.typeBCOut = 1;
            par.CountKnots = 20;
            RiverStatic1DSW task = new RiverStatic1DSW(par);
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
    }
}
