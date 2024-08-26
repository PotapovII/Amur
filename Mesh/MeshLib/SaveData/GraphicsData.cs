//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07 04 2020 Потапов И.И.
//               перекодировка : 03 01 2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using CommonLib.Areas;
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Контейнер кривых
    /// </summary>
    [Serializable]
    public class GraphicsData : IGraphicsData
    {
        public int Count => curves.Count;
        /// <summary>
        /// Список кривых 
        /// </summary>
        public List<GraphicsCurve> curves;
        public GraphicsData()
        {
            curves = new List<GraphicsCurve>(100);
        }
        public GraphicsData(GraphicsData g)
        {
            curves = new List<GraphicsCurve>(g.curves.Count);
            foreach (var c in g.curves)
                curves.Add(new GraphicsCurve(c));
        }
        /// <summary>
        /// Сложение объеков GraphicsData
        /// </summary>
        /// <param name="g"></param>
        public void Add(IGraphicsData ig)
        {
            GraphicsData g = ig as GraphicsData;
            if (g != null)
            {
                if (curves == null)
                    curves = new List<GraphicsCurve>(g.curves.Count);
                foreach (var c in g.curves)
                {
                    curves.Add(new GraphicsCurve(c));
                }
            }
        }
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddCurve(string Name, double[] x, double[] y)
        {
            if (curves == null)
                curves = new List<GraphicsCurve>();
            GraphicsCurve curve = new GraphicsCurve(Name);
            for (uint i = 0; i < x.Length; i++)
                curve.Add(x[i], y[i]);
            curves.Add(curve);
        }
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="e"></param>
        public void Add(IGraphicsCurve ee)
        {
            GraphicsCurve e = ee as GraphicsCurve;
            if (e != null)
                curves.Add(e);
        }
        /// <summary>
        /// Очистка контейнера
        /// </summary>
        public void Clear(TypeGraphicsCurve TGraphicsCurve = TypeGraphicsCurve.TimeCurve) 
        {
            if (TGraphicsCurve == TypeGraphicsCurve.AllCurve)
                curves.Clear();
            else
            {
                List<GraphicsCurve> tmpcurves = new List<GraphicsCurve>();
                foreach (var p in curves)
                    if (p.TGraphicsCurve != TGraphicsCurve)
                        tmpcurves.Add(p);
                curves = tmpcurves;
            }
        }
        /// <summary>
        /// Количество выжеленных линий
        /// </summary>
        /// <returns></returns>
        public int SelectCount()
        {
            int s = 0;
            for (int i = 0; i < curves.Count; i++)
                if (curves[i].Check == true)
                    s++;
            return s;
        }
        /// <summary>
        /// Получить регион для кривых 
        /// </summary>
        /// <returns></returns>
        public RectangleWorld GetRegion()
        {
            if (curves.Count == 0)
                return new RectangleWorld(0, 0, 0, 0);
            else
            {
                RectangleWorld a = new RectangleWorld(0, 0, 0, 0);
                for (int i = 0; i < curves.Count; i++)
                {
                    if (curves[i].Check == true)
                    {
                        RectangleWorld b = GetRegion(curves[i]);
                        a = RectangleWorld.Extension(ref a, ref b);
                    }
                }
                return a;
            }
        }
        /// <summary>
        /// Получить регион кривой
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public RectangleWorld GetRegion(GraphicsCurve curve)
        {
            double xmin = curve[0].x;
            double ymin = curve[0].y;
            double xmax = xmin;
            double ymax = ymin;
            double x, y;
            for (int i = 0; i < curve.Count; i++)
            {
                x = curve[i].x;
                y = curve[i].y;
                if (double.IsNaN(x) == true || double.IsInfinity(x))
                    x = 0;
                if (double.IsNaN(y) == true || double.IsInfinity(y))
                    y = 0;
                xmin = Math.Min(x, xmin);
                ymin = Math.Min(y, ymin);
                xmax = Math.Max(x, xmax);
                ymax = Math.Max(y, ymax);
            }
            return new RectangleWorld(xmin, xmax, ymin, ymax);
        }
        /// <summary>
        /// получить список названий кривых
        /// </summary>
        /// <returns></returns>
        public List<string> GraphicNames()
        {
            List<string> names = new List<string>();
            foreach (var p in curves)
                names.Add(p.Name);
            return names;
        }
        public GraphicsCurve GetCurve(uint index)
        {
            return curves[(int)index % curves.Count];
        }

        /// <summary>
        /// Получить подмножество IGraphicsData с кривыми заданного типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IGraphicsData GetSubIGraphicsData(TypeGraphicsCurve TGraphicsCurve)
        {
            IGraphicsData sub = new GraphicsData();
            foreach (var p in curves)
                if(p.TGraphicsCurve == TGraphicsCurve || TGraphicsCurve == TypeGraphicsCurve.AllCurve)
                    sub.Add(p);
            return sub;
        }
    }
}
