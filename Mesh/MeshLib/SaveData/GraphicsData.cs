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
                    Add(new GraphicsCurve(c));
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
            Add(curve);
        }
        /// <summary>
        /// Добавление кривой
        /// </summary>
        /// <param name="e"></param>
        public void Add(IGraphicsCurve ee)
        {
            GraphicsCurve e = ee as GraphicsCurve;
     
            //int idx = 1;
            if (e != null)
            {
                //MM:
                //foreach (var p in curves)
                //{
                //    if (e.CurveName == p.CurveName)
                //    {
                //        e.CurveName = e.CurveName + idx.ToString();
                //        idx++;
                //        goto MM;
                //    }
                //}
                curves.Add(e);
            }
        }
        /// <summary>
        /// Удаление кривой по имени
        /// </summary>
        /// <param name="Name">Название поля</param>
        public void RemoveCurve(string Name)
        {
            foreach (var curve in curves)
            {
                if (curve.Name == Name)
                {
                    curves.Remove(curve);
                    break;
                }
            }
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
        /// Количество выделенных линий
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
        public RectangleWorld GetRegion(bool coordInv = false)
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
                        RectangleWorld b = GetRegion(curves[i], coordInv);
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
        public RectangleWorld GetRegion(GraphicsCurve curve, bool coordInv = false)
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
            if(coordInv == false)
                return new RectangleWorld(xmin, xmax, ymin, ymax);
            else
                return new RectangleWorld(ymin, ymax, xmin, xmax);
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
        /// <summary>
        /// Имена групп кривых
        /// </summary>
        /// <returns></returns>
        public List<string> GraphicGroupNames()
        {
            var names = new List<string>();
            foreach (var p in curves)
            {
                string gName = GroupNameFilter(p.Name);
                if (names.Contains(gName) == false)
                    names.Add(gName);
            }
            return names;
        }

        public GraphicsCurve GetCurve(uint index)
        {
            return curves[(int)index % curves.Count];
        }

        /// <summary>
        /// Получить подмножество IGraphicsData с кривыми заданного типа 
        /// в активных подгруппах
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IGraphicsData GetSubIGraphicsData(TypeGraphicsCurve TGraphicsCurve, List<string> filter)
        {
            IGraphicsData sub = new GraphicsData();
            foreach (var p in curves)
            {
                var Name = GroupNameFilter(p.Name);
                if (filter.Contains(Name) == true &&
                   (p.TGraphicsCurve == TGraphicsCurve || 
                    TGraphicsCurve == TypeGraphicsCurve.AllCurve))
                        sub.Add(p);
            }
            return sub;
        }
        /// <summary>
        /// Групповой фильтр
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public string GroupNameFilter(string Name)
        {
            try
            {
                string[] lines = Name.Split('#',':');
                if (lines.Length == 1)
                    return Name.Trim();
                else
                    return lines[1].Trim();
            }
            catch
            {
                return "Ошибка распознования имени группы";
            }
        }
    }
}
