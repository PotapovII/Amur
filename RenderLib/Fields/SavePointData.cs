//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 30.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using MemLogLib;
    using GeometryLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MeshLib;
    using CommonLib.Areas;
    using GeometryLib.Vector;

    /// <summary>
    /// ОО: Сохраняет данные задачи в удобной для рендеринга структуре данных.
    /// Враппер для SavePoint
    /// </summary>
    [Serializable]
    public class SavePointData
    {
        /// <summary>
        /// сетка задачи в точке сохранения
        /// </summary>
        public IRenderMesh mesh;
        /// <summary>
        /// Контейнер кривых
        /// </summary>
        public GraphicsData graphicsData;
        /// <summary>
        /// поля задачи в точке сохранения
        /// </summary>
        List<IField> poles = new List<IField>();
        /// <summary>
        /// Границы физических координат
        /// </summary>
        public RectangleWorld World;
        /// <summary>
        /// Данные для отрисовки
        /// </summary>
        public SavePoint data;
        /// <summary>
        /// Количество полей связанных с сеткой TriMesh
        /// </summary>
        public int PoleCount { get { return data.poles.Count; } }
        /// <summary>
        /// Получение данных о сетке и расчетных полей на ней
        /// определение мирового региона для отрисовки области и данных
        /// </summary>
        /// <param name="data">данные о сетке и расчетных полей на ней</param>
        /// <param name="AccountingСurves">флаг учита масштаба кривых при расчете области отрисовки</param>
        public void SetSavePoint(SavePoint data, bool AccountingСurves = false)
        {
            this.mesh = data.mesh;
            this.graphicsData = data.graphicsData as GraphicsData;
            this.poles = data.poles;
            RectangleWorld Rmesh = new RectangleWorld();
            if (mesh != null)
            {
                double MinX=0, MaxX = 0, MinY = 0, MaxY = 0;
                mesh.MinMax(0, ref MinX, ref MaxX);
                mesh.MinMax(1, ref MinY, ref MaxY);
                Rmesh = new RectangleWorld((float)MinX,(float)MaxX,(float)MinY,(float)MaxY);
            }
            RectangleWorld Rdata = new RectangleWorld();
            if (graphicsData != null)
            {
                Rdata = graphicsData.GetRegion();
            }
            if (mesh != null && graphicsData != null && AccountingСurves == true)
                this.World = RectangleWorld.Extension(ref Rmesh, ref Rdata);
            else
            {
                if (graphicsData != null && mesh == null)
                {
                    this.World = Rdata;
                }
                else
                    if ((graphicsData == null || AccountingСurves == false) && mesh != null)
                {
                    this.World = Rmesh;
                }
            }
        }
        public List<string> PoleNames()
        {
            List<string> names = new List<string>();
            if (poles != null)
                foreach (var p in poles)
                    names.Add(p.Name);
            return names;
        }
        public IField GetPole(uint index)
        {
            IField Field = poles[(int)index % poles.Count];
            if( Field.Dimention == 1)
                return new Field1D((Field1D)Field);
            else
                return new Field2D((Field2D)Field);
        }
        public List<string> GraphicNames()
        {
            return graphicsData != null ? graphicsData.GraphicNames() : null;
        }
        public GraphicsCurve GetCurve(uint index)
        {
            return graphicsData != null ? graphicsData.GetCurve(index) : null;
        }
        public bool GetPoleMinMax(int indexPole, ref double MinV, ref double MaxV, ref double SumV, ref double AreaV, ref double[] Values, ref double[] VX, ref double[] VY, ref int Dim)
        {
            if (indexPole == -1)
                return false;
            IField pole = GetPole((uint)indexPole);
            Dim = pole.Dimention;
            if (pole == null)
                return false;
            if (pole.Dimention == 1)
                Values = ((Field1D)pole).Values;
            else
            {
                Vector2[] val = ((Field2D)pole).Values;
                MEM.Alloc<double>(val.Length, ref Values);
                MEM.Alloc<double>(val.Length, ref VX);
                MEM.Alloc<double>(val.Length, ref VY);
                for (uint i = 0; i < Values.Length; i++)
                {
                    VX[i] = val[i].X;
                    VY[i] = val[i].Y;
                    Values[i] = val[i].Length();
                }
            }
            SumV = 0;
            double SArea = 0;
            TriElement[] elems = mesh.GetAreaElems();
            if (elems != null && Values.Length == mesh.CountKnots)
            {
                for (uint elem = 0; elem < elems.Length; elem++)
                {
                    TriElement e = elems[elem];
                    double S = Math.Abs(mesh.ElemSquare(e));
                    SArea += S;
                    SumV += (Values[e.Vertex1] + Values[e.Vertex2] + Values[e.Vertex3]) * S / 3;
                }
                SumV /= SArea;
            }
            else
            {
                SumV = Values.Sum() / Values.Length;
            }
            AreaV = SArea;
            MaxV = Values.Max();
            MinV = Values.Min();
            if (Math.Abs(MaxV - MinV) < MEM.Error9) return false;
            return true;
        }
    }
}
