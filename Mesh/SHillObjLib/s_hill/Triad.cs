namespace SHillObjLib
{
    using System.Diagnostics;

    using MemLogLib;
    using CommonLib.Geometry;
    
    public class Triad
    {
        /// <summary>
        /// Индексы вершин
        /// </summary>
        public int a, b, c;
        /// <summary>
        /// смежные ребра указывают на соседние треугольники.
        /// adjacent edges index to neighbouring triangle.
        /// </summary>
        public int ab, bc, ac;
        /// <summary>
        /// Положение и радиус окружности в квадрате
        /// Position and radius squared of circumcircle
        /// </summary>
        public double circumcircleX, circumcircleY, circumcircleR2;

        public Triad(int x, int y, int z) 
        {
            a = x; b = y; c = z; ab = -1; bc = -1; ac = -1; 
            circumcircleR2 = -1; //x = 0; y = 0;
        }

        public void Initialize(int a, int b, int c, int ab, int bc, int ac, HPoint[] points)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.ab = ab;
            this.bc = bc;
            this.ac = ac;
            FindCircumcirclePrecisely(points);
        }

        /// <summary>
        /// Если текущая ориентация не по часовой стрелке, поменяйте местами b<->c
        /// </summary>
        public void MakeClockwise(HPoint[] points)
        {
            double centroidX = (points[a].x + points[b].x + points[c].x) / 3.0f;
            double centroidY = (points[a].y + points[b].y + points[c].y) / 3.0f;

            double dr0  = points[a].x - centroidX, 
                   dc0 = points[a].y  - centroidY;
            double dx01 = points[b].x - points[a].x, 
                   dy01 = points[b].y - points[a].y;
            double df = -dx01 * dc0 + dy01 * dr0;
            if (df > 0)
            {
                // Нужно поменять местами вершины b<->c и ребра ab<->bc
                int t = b;
                b = c;
                c = t;

                t = ab;
                ab = ac;
                ac = t;
            }
        }

        /// <summary>
        /// Найдите местоположение и радиус^2 окружности (через все 3 точки)
        /// Это самая важная процедура во всем наборе кода.  Она должна быть
        /// численно стабильной, когда точки почти совпадают.
        /// </summary>
        public bool FindCircumcirclePrecisely(HPoint[] points)
        {
            // Use coordinates relative to point `a' of the triangle
            // Используйте координаты относительно точки а треугольника
            HPoint pa = points[a], pb = points[b], pc = points[c];

            double xba = pb.x - pa.x;
            double yba = pb.y - pa.y;
            double xca = pc.x - pa.x;
            double yca = pc.y - pa.y;

            // Squares of lengths of the edges incident to `a'
            // Квадраты длин ребер, падающих на `a"
            double balength = xba * xba + yba * yba;
            double calength = xca * xca + yca * yca;

            // Calculate the denominator of the formulae. 
            // Вычислите знаменатель формулы.
            double D = xba * yca - yba * xca;
            if ( MEM.Equals(D, 0, MEM.Error12) == true )
            {
                circumcircleX = 0;
                circumcircleY = 0;
                circumcircleR2 = -1;
                return false;
            }
            double denominator = 0.5 / D;
            // Calculate offset (from pa) of circumcenter
            // Вычислить смещение (от pa) центра окружности
            double xC = (yca * balength - yba * calength) * denominator;
            double yC = (xba * calength - xca * balength) * denominator;

            double radius2 = xC * xC + yC * yC;
            if ((radius2 > 1e10 * balength || radius2 > 1e10 * calength))
            {
                circumcircleX = 0;
                circumcircleY = 0;
                circumcircleR2 = -1;
                return false;
            }

            circumcircleR2 = (double)radius2;
            circumcircleX = (double)(pa.x + xC);
            circumcircleY = (double)(pa.y + yC);

            return true;
        }
        /// <summary>
        /// Возвращает значение true, если вершина p находится внутри окружности этого треугольника
        /// Return true iff HPoint p is inside the circumcircle of this triangle
        /// </summary>
        public bool InsideCircumcircle(HPoint p)
        {
            double dx = circumcircleX - p.x;
            double dy = circumcircleY - p.y;
            double r2 = dx * dx + dy * dy;
            return r2 < circumcircleR2;
        }

        /// <summary>
        /// Измените любой индекс соседнего треугольника, соответствующий fromIndex, на toIndex
        /// Change any adjacent triangle index that matches fromIndex, to toIndex
        /// </summary>
        public void ChangeAdjacentIndex(int fromIndex, int toIndex)
        {
            if (ab == fromIndex)
                ab = toIndex;
            else if (bc == fromIndex)
                bc = toIndex;
            else if (ac == fromIndex)
                ac = toIndex;
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// Определите, какое ребро соответствует triangleIndex, 
        /// а затем какая вершина - vertexIndex
        /// Установите индексы противоположной вершины, 
        /// левого и правого ребер соответственно
        /// 
        /// Determine which edge matches the triangleIndex, then which vertex the vertexIndex
        /// Set the indices of the opposite vertex, left and right edges accordingly
        /// </summary>
        public void FindAdjacency(int vertexIndex, int triangleIndex, 
                                  out int indexOpposite, 
                                  out int indexLeft, 
                                  out int indexRight)
        {
            if (ab == triangleIndex)
            {
                indexOpposite = c;

                if (vertexIndex == a)
                {
                    indexLeft = ac;
                    indexRight = bc;
                }
                else
                {
                    indexLeft = bc;
                    indexRight = ac;
                }
            }
            else if (ac == triangleIndex)
            {
                indexOpposite = b;

                if (vertexIndex == a)
                {
                    indexLeft = ab;
                    indexRight = bc;
                }
                else
                {
                    indexLeft = bc;
                    indexRight = ab;
                }
            }
            else if (bc == triangleIndex)
            {
                indexOpposite = a;

                if (vertexIndex == b)
                {
                    indexLeft = ab;
                    indexRight = ac;
                }
                else
                {
                    indexLeft = ac;
                    indexRight = ab;
                }
            }
            else
            {
                Debug.Assert(false);
                indexOpposite = indexLeft = indexRight = 0;
            }
        }
        public override string ToString()
        {
            return string.Format("Triad vertices {0} {1} {2} ; edges {3} {4} {5}", a, b, c, ab, ac, bc);
        }
    }
}
