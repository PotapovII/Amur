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
    using CommonLib.Areas;
    using GeometryLib.Vector;
    
    /// <summary>
    /// ОО: Сохраняет данные задачи в удобной для рендеринга структуре данных.
    /// Враппер для SavePoint
    /// </summary>
    [Serializable]
    public class CloudData
    {
        /// <summary>
        /// сетка задачи в точке сохранения
        /// </summary>
        public IClouds clouds;
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
        //public SaveCloud data;
        /// <summary>
        /// Количество полей связанных с сеткой TriMesh
        /// </summary>
        public int PoleCount { get { return clouds.AttributNames.Length; } }
        /// <summary>
        /// Получение данных о сетке и расчетных полей на ней
        /// определение мирового региона для отрисовки области и данных
        /// </summary>
        /// <param name="data">данные о сетке и расчетных полей на ней</param>
        /// <param name="AccountingСurves">флаг учита масштаба кривых при расчете области отрисовки</param>
        public void SetSavePoint(IClouds clouds, bool AccountingСurves = false)
        {
            this.clouds = clouds;
            
            RectangleWorld range = new RectangleWorld();
            if (clouds != null)
            {
                double MinX = 0, MaxX = 0, MinY = 0, MaxY = 0;
                clouds.MinMax(0, ref MinX, ref MaxX);
                clouds.MinMax(1, ref MinY, ref MaxY);
                range = new RectangleWorld((float)MinX, (float)MaxX, (float)MinY, (float)MaxY);
            }
            RectangleWorld Rdata = new RectangleWorld();
            if (clouds != null && AccountingСurves == true)
                this.World = RectangleWorld.Extension(ref range, ref Rdata);
            else
            {
                if (clouds == null)
                {
                    this.World = Rdata;
                }
                else
                    if (AccountingСurves == false && clouds != null)
                {
                    this.World = range;
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
            if (Field.Dimention == 1)
                return new Field1D((Field1D)Field);
            else
                return new Field2D((Field2D)Field);
        }
        /// <summary>
        /// Получить заначения для точек обласка
        /// </summary>
        /// <param name="indexPole"></param>
        /// <param name="MinV"></param>
        /// <param name="MaxV"></param>
        /// <param name="Values"></param>
        /// <param name="VX"></param>
        /// <param name="VY"></param>
        /// <param name="Dim"></param>
        /// <returns></returns>
        public bool GetPoleMinMax(int indexPole, ref double MinV, ref double MaxV, ref double[] Values, ref double[] VX, ref double[] VY, ref int Dim)
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
            MaxV = Values.Max();
            MinV = Values.Min();
            if (Math.Abs(MaxV - MinV) < MEM.Error9) return false;
            return true;
        }
    }
}
