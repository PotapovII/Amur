//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 06.03.2024 Потапов И.И. 
//---------------------------------------------------------------------------
namespace GeometryLib.Locators
{
    using CommonLib;
    using CommonLib.Geometry;
    public class TriMeshLocators
    {
        TriElement[] elems;
        double[] x;
        double[] y;

        public TriMeshLocators(IMesh mesh)
        {
            x = mesh.GetCoords(0);
            y = mesh.GetCoords(1);
            elems = mesh.GetAreaElems();
        }
        /// <summary>
        /// Поиск элемента без хеширования
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public int QueryElement(IHPoint point)
        {
            for(int elem = 0; elem < elems.Length; elem++)
            {
                if (IsPointInTriangle(point,elem) == true)
                    return elem;
            }
            return -1;
        }
        /// <summary>
        /// Определение принадлежит ли первая точка массива треугольнику (три точки) 
        /// </summary>
        /// <returns></returns>
        public bool IsPointInTriangle(IHPoint p, int elem)
        {
            uint i1 = elems[elem].Vertex1;
            uint i2 = elems[elem].Vertex2;
            uint i3 = elems[elem].Vertex3;
            double a = (x[i1] - p.X) * (y[i2] - y[i1]) - (x[i2] - x[i1]) * (y[i1] - p.Y);
            double b = (x[i2] - p.X) * (y[i3] - y[i2]) - (x[i3] - x[i2]) * (y[i2] - p.Y);
            double c = (x[i3] - p.X) * (y[i1] - y[i3]) - (x[i1] - x[i3]) * (y[i3] - p.Y);
            if ((a >= 0 && b >= 0 && c >= 0) || (a <= 0 && b <= 0 && c <= 0))
            {
                // Console.WriteLine("Принадлежит треугольнику");
                return true;
            }
            else
            {
                // Console.WriteLine("Не принадлежит треугольнике");
                return false;
            }
        }
    }
}
