//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 06.07.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using GeometryLib.Vector;
    /// <summary>
    /// Прямоугольная сетка
    /// </summary>
    // 0 ------------------> j (y)
    // |
    // |    Система координат
    // |
    // |
    // V i (x)
    public interface IRectangleMesh
    {
        /// <summary>
        /// длина области
        /// </summary>
        double Lx { get; set; }
        /// <summary>
        /// высота области
        /// </summary>
        double Ly { get; set; }
        /// <summary>
        /// узлов по х
        /// </summary>
        int Nx { get; set; }
        /// <summary>
        /// узлов по у
        /// </summary>
        int Ny { get; set; }
        /// <summary>
        /// количество узлов (i) по направлению X 
        /// </summary>
        double dx { get; set; }
        /// <summary>
        /// количество узлов (j) по направлению Y
        /// </summary>
        double dy { get; set; }
        /// <summary>
        /// Начало координат по Х
        /// </summary>
        double Xmin { get; set; }
        /// <summary>
        /// Начало координат по У
        /// </summary>
        double Ymin { get; set; }
        /// <summary>
        /// Получить номер узла по индексам сетки
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        uint Map(int i, int j);
        /// <summary>
        /// Вычисление поля source в точке point
        /// </summary>
        double CalkCeilValue(ref Vector2 point, double[][] source);
        /// <summary>
        /// Получить значения поля для IRenderMesh
        /// </summary>
        /// <param name="source">источник</param>
        /// <param name="result">результат</param>
        void GetValueTo1D(double[][] source, ref double[] result);
        /// <summary>
        /// Получить значения поля для IRectangleMesh
        /// </summary>
        /// <param name="source">источникparam>
        /// <param name="result">результат</param>
        void Get1DValueTo2D(double[] source, ref double[][] result);
    }
}
