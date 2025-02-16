using System.ComponentModel;

namespace TestEliz2D
{
    //// // ////////////////////////////////не переименовывать классы Gererator и Parameter!!!! от имени зависит положение в листбоксе контрола
    public class FletcherParameter : Parameter
    {
        /// <summary>
        ///  Предварительный интерполяционный параметр для внутренней поверхности Z2
        /// </summary>
        double _S2 = 0.25f;
        /// <summary>
        ///  Предварительный интерполяционный параметр для внутренней поверхности Z3
        /// </summary>
        double _S3 = 0.9f;
        /// <summary>
        /// Параметр однородности внутренних точек
        /// </summary>
        double _AW = 0.5f;
        /// <summary>
        /// P-параметр сгущения нижней границы
        /// </summary>
        double _PAC = 1.0f;
        /// <summary>
        /// Q-параметр сгущения нижней границы
        /// </summary>
        double _QAC = 0.5f;
        /// <summary>
        /// P-параметр сгущения верхней границы
        /// </summary>
        double _PFD = 1.0f;
        /// <summary>
        /// Q-параметр сгущения верхней границы
        /// </summary>
        double _QFD = 0.5f;
        /// <summary>
        /// P-параметр сгущения левой границы
        /// </summary>
        double _PAF = 1.0f;
        /// <summary>
        /// Q-параметр сгущения левой границы
        /// </summary>
        double _QAF = 0.5f;
        /// <summary>
        /// P-параметр сгущения правой границы
        /// </summary>
        double _PCD = 1.0f;
        /// <summary>
        /// Q-параметр сгущения правой границы
        /// </summary>
        double _QCD = 0.5f;
        //
        [Description("S2 Предварительный интерполяционный параметр для внутренней поверхности Z2"), Category("Параметры разбиения Флетчера"), DisplayName("S2")]
        public double S2
        {
            get { return _S2; }
            set { _S2 = value; }
        }
        //
        [Description("S3 Предварительный интерполяционный параметр для внутренней поверхности Z3"), Category("Параметры разбиения Флетчера"), DisplayName("S3")]
        public double S3
        {
            get { return _S3; }
            set { _S3 = value; }
        }
        [Description("Параметр однородности внутренних точек"), Category("Параметры разбиения Флетчера"), DisplayName("Aw")]
        public double AW
        {
            get { return _AW; }
            set { _AW = value; }
        }
        //
        [Description("P-параметр сгущения нижней границы"), Category("Параметры разбиения Флетчера"), DisplayName("P ниж. гр.")]
        public double PAC
        {
            get { return _PAC; }
            set { _PAC = value; }
        }
        //
        [Description("Q-параметр сгущения нижней границы"), Category("Параметры разбиения Флетчера"), DisplayName("Q ниж. гр.")]
        public double QAC
        {
            get { return _QAC; }
            set { _QAC = value; }
        }
        //
        [Description("P-параметр сгущения верхней границы"), Category("Параметры разбиения Флетчера"), DisplayName("P верх. гр.")]
        public double PFD
        {
            get { return _PFD; }
            set { _PFD = value; }
        }
        //
        [Description("Q-параметр сгущения верхней границы"), Category("Параметры разбиения Флетчера"), DisplayName("Q верх. гр.")]
        public double QFD
        {
            get { return _QFD; }
            set { _QFD = value; }
        }
        //
        [Description("P-параметр сгущения левой границы"), Category("Параметры разбиения Флетчера"), DisplayName("P лев. гр.")]
        public double PAF
        {
            get { return _PAF; }
            set { _PAF = value; }
        }
        //
        [Description("Q-параметр сгущения левой границы"), Category("Параметры разбиения Флетчера"), DisplayName("Q лев. гр.")]
        public double QAF
        {
            get { return _QAF; }
            set { _QAF = value; }
        }
        //
        [Description("P-параметр сгущения правой границы"), Category("Параметры разбиения Флетчера"), DisplayName("P прав. гр.")]
        public double PCD
        {
            get { return _PCD; }
            set { _PCD = value; }
        }
        //
        [Description("Q-параметр сгущения правой границы"), Category("Параметры разбиения Флетчера"), DisplayName("Q прав. гр.")]
        public double QCD
        {
            get { return _QCD; }
            set { _QCD = value; }
        }
        public FletcherParameter()
        { }
        /// <summary>
        /// Параметры для разбиения расчетной области по методу Флетчера
        /// </summary>
        /// <param name="Nx">Количество узлов по x</param>
        /// <param name="Ny">Количество узлов по y</param>
        /// <param name="index">Тип сетки: 0 - треугольная, 1 - смешанная, 2 - четырехугольная</param>
        /// <param name="S2">Предварительный интерполяционный параметр для внутренней поверхности Z2</param>
        /// <param name="S3"> Предварительный интерполяционный параметр для внутренней поверхности Z3</param>
        /// <param name="AW">Параметр однородности внутренних точек</param>
        /// <param name="PAC">P-параметр сгущения нижней границы</param>
        /// <param name="QAC">Q-параметр сгущения нижней границы</param>
        /// <param name="PFD">P-параметр сгущения верхней границы</param>
        /// <param name="QFD">Q-параметр сгущения верхней границы</param>
        /// <param name="PAF">P-параметр сгущения левой границы</param>
        /// <param name="QAF">Q-параметр сгущения левой границы</param>
        /// <param name="PCD">P-параметр сгущения правой границы</param>
        /// <param name="QCD">Q-параметр сгущения правой границы</param>
        public FletcherParameter(int Nx, int Ny, int index, double S2, double S3, double AW, double PAC, double QAC, double PFD, double QFD, double PAF, double QAF, double PCD, double QCD)
        {
            this.Nx = Nx;
            this.Ny = Ny;
            this.index = index;
            this.S2 = S2;
            this.S3 = S3;
            this.AW = AW;
            this.PAC = PAC;
            this.QAC = QAC;
            this.PFD = PFD;
            this.QFD = QFD;
            this.PAF = PAF;
            this.QAF = QAF;
            this.PCD = PCD;
            this.QCD = QCD;
        }

    }
}
