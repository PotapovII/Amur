//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using CommonLib.Mesh.RVData;
    using System;
    using System.IO;


    /// <summary>
    /// Узел КЭ сетки
    /// </summary>
    public class RVNode : RVPoint
    {
        /// <summary>
        /// Координата z узла
        /// </summary>
        public double Z { get=>z; set=>z=value; }
        /// <summary>
        /// Координата х0 узла
        /// </summary>
        public double Xo => xo;
        /// <summary>
        /// Координата y0 узла
        /// </summary>
        public double Yo => yo;
        /// <summary>
        /// координата хo/ xo coordinate							
        /// </summary>
        private double xo;
        /// <summary>
        /// координата уo/ yo coordinate							
        /// </summary>
        private double yo;
        /// <summary>
        /// отметка дна/bed elevation
        /// </summary>
        protected double z;
        /// <summary>
        /// пограничный флаг различного назначения
        /// boundary flag for various purposes
        /// </summary>
        public BoundaryNodeFlag BoundNodeFlag { get; set; }
        /// <summary>
        /// может узел перемещаться или нет
        /// can node be moved or not
        /// </summary>
        public RVFixedNodeFlag Fixed { get; set; } 
        /// <summary>
        /// указатель на один из треугольников, который содержит этот узел
        /// pointer to one of the triangles that Contains this node
        /// </summary>
        public RVTriangle TriangleNodeOwner { get; set; }

        public RVNode(int id = 1, double xcoord = 10.0, double ycoord = 10.0, double zcoord = 10.0) : base(id, xcoord , ycoord)
        {
            BoundNodeFlag = BoundaryNodeFlag.internalNode;
            Fixed = RVFixedNodeFlag.floatingNode;
            TriangleNodeOwner = null;
            z = zcoord;
            SaveLocation();
        }

        public virtual int CountPapams() { return 0; }
        public virtual int CountVariables() { return 0; }
        public virtual double GetPapam(int i)
        {
            if (i == 1)
            {
                return z;
            }
            else
                return 0.0;
        }
        public virtual double GetVariable(int i) { return 0; }
        
        public void Init(double xcoord, double ycoord, double zcoord)
        {
            x = xcoord;
            y = ycoord;
            z = zcoord;
            SaveLocation();
        }
        public void Assignt(double xcoord, double ycoord, double zcoord)
        {
            x = xcoord;
            y = ycoord;
            z = zcoord;
        }
        /// <summary>
        /// Расстояние м/д узлами
        /// </summary>
        /// <param name="otherNode"></param>
        /// <returns></returns>
        public double Distance(RVNode otherNode)
        {
            if (otherNode != null)
            {
                double dx = otherNode.x - x;
                double dy = otherNode.y - y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
            else
                return (-1.0);
        }
        /// <summary>
        /// Интерполяция координат в точке для котрой определены функции формы formFunctions
        /// </summary>
        /// <param name="n"></param>
        /// <param name="points"></param>
        /// <param name="formFunctions"></param>
        public virtual void Interpolation(int n, RVNode[] points, double[] formFunctions)
        {
            x = 0.0;
            y = 0.0;
            z = 0.0;
            for (int i = 0; i < n; i++)
            {
                if (points[i] != null)
                {
                    x += formFunctions[i] * points[i].Xo;
                    y += formFunctions[i] * points[i].Yo;
                    z += formFunctions[i] * points[i].Z;
                }
            }
            SaveLocation();
        }
        /// <summary>
        /// Сохранение плановых координат в буфер
        /// </summary>
        public void SaveLocation()
        {
            xo = x;
            yo = y;
        }
        /// <summary>
        /// Востановление плановых координат из буфера
        /// </summary>
        public void RestoreLocation()
        {
            x = xo;
            y = yo;
        }
        public static void Write(StreamWriter file, RVNode n)
        {
            if (n != null)
            {
                file.Write(n.ID);

                if (n.Fixed == RVFixedNodeFlag.fixedNode)
                    file.Write(" x ");
                else
                    if (n.Fixed == RVFixedNodeFlag.slidingNode)
                    file.Write(" s ");
                file.Write(n.xo.ToString("F8") + " ");
                file.Write(n.yo.ToString("F8") + " ");
                for (int i = 1; i <= n.CountPapams(); i++)
                    file.Write(n.GetPapam(i).ToString("F4") + " ");
            }
        }
    }
}
