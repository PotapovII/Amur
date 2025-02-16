using System.ComponentModel;

namespace TestEliz2D
{
    // // ////////////////////////////////не переименовывать классы Gererator и Parameter!!!! от имени зависит положение в листбоксе контрола
    public class OrthoParameter : Parameter
    {
        /// <summary>
        /// Коэффициент релаксации ортотропности
        /// </summary>
        double _RelaxOrtho = 0.9;
        /// <summary>
        /// Коэффициент релаксации ортотропности
        /// </summary>
        double _Tay = 0.9;
        //
        [DisplayName("Коэффициент релаксации ортотропности"), Category("Ортотропные параметры разбиения")]
        public double RelaxOrtho
        {
            get { return _RelaxOrtho; }
            set { _RelaxOrtho = value; }
        }
        [DisplayName("Коэффициент релаксации ортотропности общий"), Category("Ортотропные параметры разбиения")]
        public double Tay
        {
            get { return _Tay; }
            set { _Tay = value; }
        }
        //
        public OrthoParameter()
        { }
        /// <summary>
        /// Параметры для ортотропного разбиения расчетной области
        /// </summary>
        /// <param name="Nx">Количество узлов по x</param>
        /// <param name="Ny">Количество узлов по y</param>
        /// <param name="index">Тип сетки: 0 - треугольная, 1 - смешанная, 2 - четырехугольная</param>
        /// <param name="flagFirst">Флаг первичности сетки</param>
        /// <param name="RelaxOrtho">Коэффициент релаксации</param>
        /// <param name="Tay">Коэффициент релаксации</param>
        public OrthoParameter(int Nx, int Ny, int index, double P, double Q, double RelaxOrtho, double Tay) : base(Nx, Ny, index, P, Q)
        {
            this.RelaxOrtho = RelaxOrtho;
            this.Tay = Tay;
        }

    }
}
