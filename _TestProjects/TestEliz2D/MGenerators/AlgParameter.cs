namespace TestEliz2D
{
    // // ////////////////////////////////не переименовывать классы Gererator и Parameter!!!! от имени зависит положение в листбоксе контрола
    public class AlgParameter : Parameter
    {

        public AlgParameter()
        {
        }
        /// <summary>
        /// Параметры для алгебраического разбиения расчетной области
        /// </summary>
        /// <param name="Nx">Количество узлов по x</param>
        /// <param name="Ny">Количество узлов по y</param>
        /// <param name="index">Тип сетки: 0 - треугольная, 1 - смешанная, 2 - четырехугольная</param>
        /// <param name="flagFirst">Флаг первичности сетки</param>
        public AlgParameter(int Nx, int Ny, int index, double P, double Q):base(Nx, Ny, index, P, Q) { }
    }
}
