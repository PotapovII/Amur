namespace CommonLib.FDM
{
    using CommonLib.Function;
    public interface IQuadMesh
    {
        /// <summary>
        /// Движение узлов сетки по дну
        /// </summary>
        bool topBottom { get; set; }
        /// <summary>
        /// Движение узлов сетки по вертикальными границам
        /// </summary>
        bool leftRight { get; set; }
        /// <summary>
        /// функция дна
        /// </summary>
        IFunction1D Function { get; set; }
        /// <summary>
        /// Установка функции дна
        /// </summary>
        void SetFunction(IDigFunction fun);
        /// <summary>
        /// Инициализация сетки
        /// </summary>
        /// <param name="L"></param>
        /// <param name="H"></param>
        /// <param name="Q"></param>
        /// <param name="P"></param>
        void InitQMesh(double L, double H, bool topBottom, bool leftRight, IDigFunction function = null);
        /// <summary>
        /// Определения индексов сетки для центральной области с деформируемым дном
        /// </summary>
        /// <param name="count">Количество узлов в деформируемой области</param>
        /// <param name="In">начальный узел области деформации</param>
        /// <param name="Out">конечный узел области деформации</param>
        /// <param name="xStart">левая граница области деформации</param>
        /// <param name="xEnd">правая граница области деформации</param>
        /// <param name="getX0">флаг учета левой границы</param>
        void GetZetaArea(ref int count, ref int In, ref int Out,
                                double xStart, double xEnd, bool getX0);
        /// <summary>
        /// Получение новой границы области и формирование сетки
        /// </summary>
        /// <param name="Zeta"></param>
        void CreateNewQMesh(double[] Zeta, int bottomTube, int jdxTube, IFunction1D botton, int NCoord = 100);
        /// <summary>
        /// Вычисление координат узлов сетки методом Зейделя
        /// </summary>
        void CalkXYI(int Count = 5, IFunction1D botton = null);
        /// <summary>
        /// Получение текущих донных отметок
        /// </summary>
        /// <param name="Zeta"></param>
        void GetBed(ref double[] Zeta);
        /// <summary>
        /// движение узлов вдоль донной поверхности
        /// </summary>
        double MoveBotton(IFunction1D botton = null);
        /// <summary>
        /// Конверсия генерируемой сетки в формат КО сетки
        /// </summary>
        /// <param name="xu">координаты узловых точек для скорости u</param>
        /// <param name="yv">координаты узловых точек для скорости v</param>
        /// <param name="x">центры ячеек сетки по X</param>
        /// <param name="y">центры ячеек сетки по Y</param>
        /// <param name="Hx">расстояние между узловыми точеками для скорости u</param>
        /// <param name="Hy">расстояние между узловыми точеками для скорости v</param>
        /// <param name="Dx">расстояние между центрами контрольных объемов по х</param>
        /// <param name="Dy">расстояние между центрами контрольных объемов по у</param>
        void ConvertMeshToMesh(ref double[][] xu, ref double[][] yv,
                                    ref double[][] x, ref double[][] y,
                                    ref double[][] Hx, ref double[][] Hy,
                                    ref double[][] Dx, ref double[][] Dy);

        /// <summary>
        /// Конверсия генерируемой сетки в формат КО сетки
        /// </summary>
        /// <param name="xu">координаты узловых точек для скорости u</param>
        /// <param name="yv">координаты узловых точек для скорости v</param>
        /// <param name="x">центры ячеек сетки по X</param>
        /// <param name="y">центры ячеек сетки по Y</param>
        /// <param name="Hx">расстояние между узловыми точеками для скорости u</param>
        /// <param name="Hy">расстояние между узловыми точеками для скорости v</param>
        /// <param name="Dx">расстояние между центрами контрольных объемов по х</param>
        /// <param name="Dy">расстояние между центрами контрольных объемов по у</param>
        void StartGeometryMesh(ref double[][] xu, ref double[][] yv,
                               ref double[][] x, ref double[][] y,
                               ref double[][] Hx, ref double[][] Hy,
                                ref double[][] Dx, ref double[][] Dy);
    }
}
