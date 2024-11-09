//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    using System;
    using System.IO;
    using CommonLib.Geometry;
    using CommonLib.Mesh.RVData;

    /// <summary>
    /// Узел КЭ сетки для генерации сетки
    /// </summary>
    public class RivNode : IDPoint
    {
        /// <summary>
        /// идентификационный номер
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Координата х точки
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Координата х точки
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Координата Z узла
        /// </summary>
        public double Z { get; set; }
        /// <summary>
        /// Координата х0 узла
        /// </summary>
        public double Xo;
        /// <summary>
        /// Координата y0 узла
        /// </summary>
        public double Yo;
        /// <summary>
        /// пограничный флаг различного назначения
        /// </summary>
        public BoundaryNodeFlag BoundNodeFlag;
        /// <summary>
        /// может узел перемещаться или нет
        /// can node be moved or not
        /// </summary>
        public RVFixedNodeFlag Fixed;
        /// <summary>
        /// указатель на один из треугольников, который содержит этот узел
        /// pointer to one of the triangles that Contains this node
        /// </summary>
        public RivTriangle TriangleNodeOwner;

        public RivNode(int id = 1, double X = 10.0, double Y = 10.0, double Z = 10.0) 
        {
            ID = id;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            BoundNodeFlag = BoundaryNodeFlag.internalNode;
            Fixed = RVFixedNodeFlag.floatingNode;
            TriangleNodeOwner = null;
            SaveLocation();
        }
        public RivNode(RivNode p)
        {
            ID = p.ID;
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
            BoundNodeFlag = p.BoundNodeFlag;
            Fixed = p.Fixed;
            TriangleNodeOwner = p.TriangleNodeOwner;
            Xo= p.Xo;
            Yo= p.Yo;
        }
        public int CompareTo(object obj)
        {
            RivNode a = obj as RivNode;
            if (a != null)
            {
                if (X < a.X)
                    return -1;
                if (X > a.X)
                    return 1;
                return 0;
            }
            else
                throw new Exception("ошибка приведения типа");
        }
        public IHPoint IClone() { return new RivNode(this); }

        public virtual int CountPapams() { return 0; }
        public virtual int CountVariables() { return 0; }
        public virtual double GetPapam(int i)
        {
            if (i == 1)
            {
                return Z;
            }
            else
                return 0.0;
        }
        public virtual double GetVariable(int i) { return 0; }

        public void Init(double X , double Y , double Z )
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            SaveLocation();
        }
        public void Assignt(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        /// <summary>
        /// Расстояние м/д узлами
        /// </summary>
        /// <param name="otherNode"></param>
        /// <returns></returns>
        public double Distance(IHPoint otherNode)
        {
            if (otherNode != null)
            {
                double dx = otherNode.X - X;
                double dy = otherNode.Y - Y;
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
        public virtual void Interpolation(RivNode[] points, double[] formFunctions)
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
            for (int i = 0; i < formFunctions.Length; i++)
            {
                if (points[i] != null)
                {
                    X += formFunctions[i] * points[i].Xo;
                    Y += formFunctions[i] * points[i].Yo;
                    Z += formFunctions[i] * points[i].Z;
                }
            }
            SaveLocation();
        }
        /// <summary>
        /// Сохранение плановых координат в буфер
        /// </summary>
        public void SaveLocation()
        {
            Xo = X;
            Yo = Y;
        }
        /// <summary>
        /// Востановление плановых координат из буфера
        /// </summary>
        public void RestoreLocation()
        {
            X = Xo;
            Y = Yo;
        }
        public static void Write(StreamWriter file, RivNode n)
        {
            if (n != null)
            {
                file.Write(n.ID + " ");
                int fxc = (int)RVFixedNodeFlag.fixedNode;
                file.Write(fxc.ToString() + " ");
                file.Write(n.Xo.ToString("F8") + " ");
                file.Write(n.Yo.ToString("F8") + " ");
                file.Write(n.Z.ToString("F4") + " ");
            }
        }
    }
}
