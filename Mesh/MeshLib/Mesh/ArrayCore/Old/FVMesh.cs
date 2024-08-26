////---------------------------------------------------------------------------
////                          ПРОЕКТ  "МКЭ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 04.02.2021 Потапов И.И.
////---------------------------------------------------------------------------
//namespace MeshLib
//{
//    using CommonLib;
//    using System;
//    //---------------------------------------------------------------------------
//    //  ОО: TriFVMesh - базистная техузловая контрольно - объемная сетка 
//    //---------------------------------------------------------------------------
//    [Serializable]
//    public class TriFVMesh : TriMesh, IFVMesh
//    {
//        /// <summary>
//        /// Сопряженные узлы для граней КЭ
//        /// </summary>
//        int[][] conjugateElementKnots;
//        /// <summary>
//        /// Номера сопряженных КЭ для граней КЭ (-1 если грань - граница)
//        /// </summary>
//        int[][] conjugateElement;
//        /// <summary>
//        /// Длина граней КЭ
//        /// </summary>
//        double[][] elementLength;
//        /// <summary>
//        /// Производная от симплекс функции формы по направлению Х
//        /// </summary>
//        double[][] eDx;
//        /// <summary>
//        /// Производная от симплекс функции формы по направлению Y
//        /// </summary>
//        double[][] eDy;
//        /// <summary>
//        /// Площадь конечного элемента
//        /// </summary>
//        double[] area;
//        /// <summary>
//        /// Площадь контрольного объема для узлов сетки
//        /// </summary>
//        double[] areaFV;
//        public TriFVMesh() { }
//        public TriFVMesh(TriFVMesh m) : base(m)
//        {
//            CalculationOfExtensions();
//        }
//        public TriFVMesh(IMesh m) : base((TriMesh)m)
//        {
//            CalculationOfExtensions();
//        }
//        /// <summary>
//        /// Клонирование объекта сетки
//        /// </summary>
//        public new IMesh Clone()
//        {
//            return new TriFVMesh(this);
//        }
//        /// <summary>
//        /// Расчет расширений
//        /// </summary>
//        /// <param name="iApplySandslide"></param>
//        public void CalculationOfExtensions()
//        {
//            area = new double[CountElements];
//            areaFV = new double[CountKnots];
//            conjugateElementKnots = new int[CountElements][];
//            conjugateElement = new int[CountElements][];
//            elementLength = new double[CountElements][];
//            eDx = new double[CountElements][];
//            eDy = new double[CountElements][];

//            double x1, y1, x2, y2, x3, y3;
//            for (uint elem = 0; elem < CountElements; elem++)
//            {
//                conjugateElement[elem] = new int[3];
//                conjugateElementKnots[elem] = new int[3];
//                elementLength[elem] = new double[3];
//                eDx[elem] = new double[3];
//                eDy[elem] = new double[3];
//                //
//                TriElement knot = Element(elem);
//                uint node1 = knot.Vertex1;
//                uint node2 = knot.Vertex2;
//                uint node3 = knot.Vertex3;
//                x1 = CoordsX[node1];
//                x2 = CoordsX[node2];
//                x3 = CoordsX[node3];
//                y1 = CoordsY[node1];
//                y2 = CoordsY[node2];
//                y3 = CoordsY[node3];
//                // Вычисление площади КЭ  
//                area[elem] = 0.5 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
//                // длины сторон КЭ
//                elementLength[elem][0] = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
//                elementLength[elem][1] = Math.Sqrt((x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3));
//                elementLength[elem][2] = Math.Sqrt((x3 - x1) * (x3 - x1) + (y3 - y1) * (y3 - y1));
//                // Производная от симплекс функции формы по направлению Х
//                // херня !!!!!!!!!!!!!!! это не производная (делитель 6!!)
//                eDx[elem][0] = (y2 - y3) / 6.0;
//                eDx[elem][1] = (y3 - y1) / 6.0;
//                eDx[elem][2] = (y1 - y2) / 6.0;
//                // Element Matrix df/dy
//                eDy[elem][0] = -(x2 - x3) / 6.0;
//                eDy[elem][1] = -(x3 - x1) / 6.0;
//                eDy[elem][3] = -(x1 - x2) / 6.0;
//                // Вычисление площади КО собираем кусочки площади КЭ в узле
//                areaFV[node1] += area[elem] / 3.0;
//                areaFV[node2] += area[elem] / 3.0;
//                areaFV[node3] += area[elem] / 3.0;
//                // ПОИСК СОПРЯЖЕННЫХ ЭЛЕМЕНТОВ
//                conjugateElementKnots[elem][0] = -1;
//                conjugateElementKnots[elem][1] = -1;
//                conjugateElementKnots[elem][2] = -1;
//                conjugateElement[elem][0] = -1;
//                conjugateElement[elem][1] = -1;
//                conjugateElement[elem][2] = -1;
//                for (int j = 0; j < CountElements; j++)
//                {
//                    TriElement knot1 = Element(elem);
//                    uint nodeA = knot1.Vertex1;
//                    uint nodeB = knot1.Vertex2;
//                    uint nodeC = knot1.Vertex3;
//                    // Upwind node to side 1-2
//                    if ((node1 == nodeB) && (node2 == nodeA))
//                    {
//                        conjugateElementKnots[elem][0] = (int)nodeC;
//                        conjugateElement[elem][0] = j;
//                    }
//                    if ((node1 == nodeA) && (node2 == nodeC))
//                    {
//                        conjugateElementKnots[elem][0] = (int)nodeB;
//                        conjugateElement[elem][0] = j;
//                    }
//                    if ((node1 == nodeC) && (node2 == nodeB))
//                    {
//                        conjugateElementKnots[elem][0] = (int)nodeA;
//                        conjugateElement[elem][0] = j;
//                    }
//                    // Upwind node to side 2-3
//                    if ((node2 == nodeB) && (node3 == nodeA))
//                    {
//                        conjugateElementKnots[elem][1] = (int)nodeC;
//                        conjugateElement[elem][1] = j;
//                    }
//                    if ((node2 == nodeA) && (node3 == nodeC))
//                    {
//                        conjugateElementKnots[elem][1] = (int)nodeB;
//                        conjugateElement[elem][1] = j;
//                    }
//                    if ((node2 == nodeC) && (node3 == nodeB))
//                    {
//                        conjugateElementKnots[elem][1] = (int)nodeA;
//                        conjugateElement[elem][1] = j;
//                    }
//                    // Upwind node to side 3-1
//                    if ((node3 == nodeB) && (node1 == nodeA))
//                    {
//                        conjugateElementKnots[elem][2] = (int)nodeC;
//                        conjugateElement[elem][2] = j;
//                    }
//                    if ((node3 == nodeA) && (node1 == nodeC))
//                    {
//                        conjugateElementKnots[elem][2] = (int)nodeB;
//                        conjugateElement[elem][2] = j;
//                    }
//                    if ((node3 == nodeC) && (node1 == nodeB))
//                    {
//                        conjugateElementKnots[elem][2] = (int)nodeA;
//                        conjugateElement[elem][2] = j;
//                    }
//                }
//            }
//        }
//        /// <summary>
//        /// Получить сопряженные узлы элементов ( 6 3 1 ) сопряженных с элементом ( 4 5 2 )
//        /// если 1 грань треугольника с узлами (4 5) внешняя граница то 
//        /// получим следующие номера узлов ( -1 3 1 )  сопряженных с элементом (k)
//        /// 1-----2----3    1-----2----3
//        /// |    /|   /     |    /|   /
//        /// |   3 |  /      |   3 |  /  
//        /// |  /  2 /       |  /  2 /  
//        /// | / k |/        | / k |/ 
//        /// 4--1--5         4--1--5 
//        /// |    /
//        /// |   /
//        /// |  /
//        /// | /
//        /// |/
//        /// 6
//        /// </summary>
//        /// <param name="element">номер элемента</param>
//        public int[] ConjugateElementKnots(uint element)
//        {
//            return conjugateElementKnots[element];
//        }
//        /// <summary>
//        /// Получить номера элементов ( c b a ) сопряженных с элементом ( k )
//        /// если 1 грань треугольника с узлами (4 5) внешняя граница то 
//        /// получим следующие номера элементов ( -1 b a ) сопряженных c элементом ( k )  
//        /// 1-----2----3    1-----2----3
//        /// |  a /| b /     | a  /| b /
//        /// |   3 |  /      |   3 |  /  
//        /// |  /  2 /       |  /  2 /  
//        /// | / k |/        | /   |/ 
//        /// 4--1--5         4--1--5 
//        /// |    /
//        /// | c /
//        /// |  /
//        /// | /
//        /// |/
//        /// 6
//        /// </summary>
//        /// <param name="element">номер элемента</param>
//        public int[] ConjugateElement(uint element)
//        {
//            return conjugateElement[element];
//        }

//        /// <summary>
//        /// Взять координату Х для i - го узла
//        /// </summary>
//        public double X(uint i) { return CoordsX[i]; }
//        /// <summary>
//        /// Взять координату Y для i - го узла
//        /// </summary>
//        public double Y(uint i) { return CoordsY[i]; }
//        /// <summary>
//        /// Получить координаты Х для всех узлов сетки
//        /// </summary>
//        public double[] GetX()
//        {
//            return CoordsX;
//        }
//        /// <summary>
//        /// Получить координаты Y для всех узлов сетки
//        /// </summary>
//        public double[] GetY()
//        {
//            return CoordsY;
//        }
//        /// <summary>
//        /// Длина граней КЭ элемента 12 23 31
//        /// </summary>
//        public double[] ElementLength(uint element)
//        {
//            return elementLength[element];
//        }
//        /// <summary>
//        /// Производные для функций формы КЭ по x
//        /// </summary>
//        public double[] Dx(uint element)
//        {
//            return eDx[element];
//        }
//        /// <summary>
//        /// Производные для функций формы КЭ по x
//        /// </summary>
//        public double[] Dy(uint element)
//        {
//            return eDy[element];
//        }
//        /// <summary>
//        /// Площадь КЭ
//        /// </summary>
//        /// <param name="element"></param>
//        /// <returns></returns>
//        public double Area(uint element)
//        {
//            return area[element];
//        }
//        /// <summary>
//        /// Площадь КО
//        /// </summary>
//        /// <param name="узел КО"></param>
//        public double AreaFV(uint knot)
//        {
//            return areaFV[knot];
//        }
//    }
//}