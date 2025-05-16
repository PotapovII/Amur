//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07 04 2020 Потапов И.И.
//               перекодировка : 03 01 2021 Потапов И.И.
//---------------------------------------------------------------------------
//               перекодировка : 29 09 2022 Потапов И.И.
//               добавлено поле типа кривой TypeGraphicsCurve
//---------------------------------------------------------------------------
namespace MeshLib
{
    using CommonLib;
    using MemLogLib;
    using GeometryLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommonLib.IO;
    using CommonLib.Geometry;
    using CommonLib.Function;

    /// <summary>
    /// ОО: данные о кривой для отрисовки
    /// </summary>
    [Serializable]
    public class GraphicsCurve : IGraphicsCurve
    {
        /// <summary>
        /// Тип кривой
        /// </summary>
        public TypeGraphicsCurve TGraphicsCurve { get; set; }
        /// <summary>
        /// Масштаб графика по Х
        /// </summary>
        public double ScaleX
        {
            get 
            { 
                if (scaleX < MEM.Error10) 
                    return 1; 
                else 
                    return scaleX; 
            }
            set 
            { 
                if (value < MEM.Error10) 
                    scaleX = 1; 
                else 
                    scaleX = value; }
        }
        double scaleX = 1;
        /// <summary>
        /// Масштаб графика по Y
        /// </summary>
        public double ScaleY
        {
            get 
            { 
                if (scaleY < MEM.Error10) 
                    return 1; 
                else 
                    return scaleY; 
            }
            set 
            { 
                if (value < MEM.Error10) 
                    scaleY = 1; 
                else 
                    scaleY = value; 
            }
        }
        double scaleY = 1;
        /// <summary>
        /// Задание масштабов кривой
        /// </summary>
        /// <param name="opCurveScale"></param>
        /// <param name="opAutoScaleX"></param>
        /// <param name="opAutoScaleY"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        public void SetScale(int opCurveScale, bool opAutoScaleX, bool opAutoScaleY, double sx, double sy)
        {
            switch (opCurveScale)
            {
                case 0:
                    ScaleX = 1; ScaleY = 1;
                    break;
                case 1:
                    {
                        if (opAutoScaleX == true)
                        {
                            double sc = Math.Max(Math.Abs(MinX()), Math.Abs(MaxX()));
                            if (sc < MEM.Error10) sc = 1;
                            ScaleX = sc;
                        }
                        else
                            ScaleX = 1;

                        if (opAutoScaleY == true)
                        {
                            double sc = Math.Max(Math.Abs(MinY()), Math.Abs(MaxY()));
                            if (sc < MEM.Error10) sc = 1;
                            ScaleY = sc; ;
                        }
                        else
                            ScaleY = 1;
                    }
                    break;
                case 2:
                    {
                        double sc = Math.Max(Math.Abs(MinY()), Math.Abs(MaxY()));
                        if (sc < MEM.Error10) sc = 1;
                        if (opAutoScaleX == true)
                        {
                            ScaleX = sc;
                        }
                        else
                            ScaleX = 1;
                        if (opAutoScaleY == true)
                            ScaleY = sc;
                        else
                            ScaleY = 1;
                    }
                    break;
                case 3:
                    ScaleX = sx; ScaleY = sy;
                    break;
            }
        }
        /// <summary>
        /// Имя кривой
        /// </summary>
        public string CurveName { get => Name; set => Name = value; }
        /// <summary>
        /// название кривой - легенда
        /// </summary>
        public string Name;
        /// <summary>
        /// флаг отрисовки участвует ли кривая в отрисовке (определении мини-макса координат)
        /// </summary>
        public bool Check;
        /// <summary>
        /// количество точек кривой
        /// </summary>
        public int Count => points.Count;
        /// <summary>
        /// получить точку кривой
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public void GetPoint(int index, ref double x, ref double y)
        {
            x = points[index].x / ScaleX;
            y = points[index].y / ScaleY;
        }
        /// <summary>
        /// индексатор
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public HPoint this[int index]
        {
            get
            {
                var x = points[index].x / ScaleX;
                var y = points[index].y / ScaleY;
                HPoint p = new HPoint(x, y);
                return p;
            }
        }
        /// <summary>
        /// Список точек кривой
        /// </summary>
        List<HPoint> points;
        public GraphicsCurve(string Name = "без имени",
        TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve, bool Check = true)
        {
            this.TGraphicsCurve = tGraphicsCurve;
            this.Name = Name;
            this.Check = Check;
            this.scaleX = 1;
            this.scaleY = 1;
            points = new List<HPoint>(10000);
            TGraphicsCurve = tGraphicsCurve;
        }

        public GraphicsCurve(string Name, double[] x, double[] y,
            TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve, bool Check = true)
        {
            this.TGraphicsCurve = tGraphicsCurve;
            this.Name = Name;
            this.Check = Check;
            this.scaleX = 1;
            this.scaleY = 1;
            if (x != null && y != null)
            {
                points = new List<HPoint>(x.Length);
                for (int i = 0; i < x.Length; i++)
                    points.Add(new HPoint(x[i], y[i]));
            }
        }

        public GraphicsCurve(IDigFunction fun, 
                TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve, 
                bool Check = true)
        {
            double[] x = null;
            double[] y = null;
            fun.GetFunctionData(ref Name, ref x, ref y);
            this.TGraphicsCurve = tGraphicsCurve;
            this.Check = Check;
            this.scaleX = 1;
            this.scaleY = 1;
            if (x != null && y != null)
            {
                points = new List<HPoint>(x.Length);
                for (int i = 0; i < x.Length; i++)
                    points.Add(new HPoint(x[i], y[i]));
            }
        }
        public GraphicsCurve(GraphicsCurve g)
        {
            this.Name = g.Name;
            this.Check = g.Check;
            this.TGraphicsCurve = g.TGraphicsCurve;
            this.scaleX = 1;
            this.scaleY = 1;
            points = new List<HPoint>(g.points.Count);
            foreach (var p in g.points)
                points.Add(new HPoint(p));
        }

        /// <summary>
        /// TODO: убрать Класс GCurve
        /// Получить кривую в формате GCurve
        /// </summary>
        /// <returns></returns>
        public GCurve GetGCurve()
        {
            GCurve curve = new GCurve(Name);
            for (int j = 0; j < points.Count; j++)
                curve.Add(points[j].x, points[j].y);
            return curve;
        }
        /// <summary>
        /// добавить точку в кривую
        /// </summary>
        /// <param name="e">точка</param>
        public void Add(HPoint e)
        {
            points.Add(e);
        }
        /// <summary>
        /// добавить точку в кривую
        /// </summary>
        /// <param name="x">координата X</param>
        /// <param name="y">координата Y</param>
        public void Add(double x, double y)
        {
            points.Add(new HPoint(x, y));
        }

        public void RemoveAt(int i)
        {
            points.RemoveAt(i);
        }
        /// <summary>
        /// очистка точек кривой из списка
        /// </summary>
        public void Clear()
        {
            points.Clear();
        }
        /// <summary>
        /// Сортировка точек по аргументу
        /// </summary>
        public void SortX()
        {
            points.Sort((one, two) => one.x.CompareTo(two.x));
        }
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        public void SortY()
        {
            points.Sort((one, two) => one.y.CompareTo(two.y));
        }
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        public double MaxY()
        {
            return points.Max(a => a.y);
        }
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        public double MinY()
        {
            return points.Min(a => a.y);
        }
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        public double MaxX()
        {
            return points.Max(a => a.x);
        }
        /// <summary>
        /// Сортировка точек по значению
        /// </summary>
        public double MinX()
        {
            return points.Min(a => a.x);
        }
        public double IntegtalCurveABS()
        {
            double sum = 0;
            for (int i = 0; i < points.Count - 1; i++)
                sum += (Math.Abs(points[i].y) + Math.Abs(points[i + 1].y)) * (points[i + 1].x - points[i].x);
            return 0.5 * sum;
        }
        /// <summary>
        /// интеграл
        /// </summary>
        /// <returns></returns>
        public double IntegtalCurve()
        {
            double sum = 0;
            for (int i = 0; i < points.Count - 1; i++)
                sum += (points[i].y + points[i + 1].y) * (points[i + 1].x - points[i].x);
            return 0.5 * sum;
        }
        /// <summary>
        /// Нормированный интеграл
        /// </summary>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <returns></returns>
        public double NormalizedIntegralCurve(double xa, double xb)
        {
            double sum = 0;
            for (int i = 0; i < points.Count - 1; i++)
                if ( (points[i].x >= xa) && (points[i + 1].x <= xb))
                {
                    double dp = (points[i].y + points[i + 1].y) / 2.0;
                    double dx = points[i + 1].x - points[i].x;
                    sum += dp * dx;
                }
            return sum / (xb - xa);
        }
        /// <summary>
        /// Cреднее арифметическое
        /// </summary>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <returns></returns>
        public double AverageCurve(double xa, double xb)
        {
            double sum = 0;
            double N = 0;
            for (int i = 0; i < points.Count; i++)
                if ( (points[i].x >= xa) && (points[i].x <= xb) )
                {
                    sum += points[i].y;
                    N++;
                }
            return sum / N;
        }
        /// <summary>
        /// Среднеквадратическое отклонение
        /// </summary>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <returns></returns>
        public double StandardDeviationCurve(double xa, double xb, int n = 0)
        {
            double average = AverageCurve(xa, xb);
            double sum = 0;
            double N = 0;
            for (int i = 0; i < points.Count; i++)
                if ((points[i].x >= xa) && (points[i].x <= xb))
                {
                    sum += (average - points[i].y) * (average - points[i].y);
                    N++;
                }
            return Math.Sqrt( sum / (N - n) );
        }
        /// <summary>
        /// Расчет параметров макимальной ямы размыва
        /// </summary>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <param name="min"></param>
        public void GetCovernInterval(ref double xa, ref double xb, ref double min)
        {
            int idx = 0;
            min = points[idx].y;
            for (int i = 1; i < points.Count; i++)
            {
                if(points[i].y < min)
                {
                    min = points[i].y; idx = i;
                }
            }
            if(min < 0 && points[0].y >= 0 && points[points.Count-1].y >= 0)
            // ход в лево
            if (idx == 0)
                xa = points[idx].x;
            else
            {
                for (int i = idx; i > 0; i--)
                {
                    if (points[i].y < 0 && points[i-1].y >= 0)
                    {
                        xa = points[i-1].x;
                        break;
                    }
                }
            }
            // ход в право
            if (idx == points.Count - 1)
                xb = points[idx].x;
            else
            {
                for (int i = idx; i < points.Count - 1; i++)
                {
                    if (points[i].y < 0 && points[i + 1].y >= 0)
                    {
                        xb = points[i + 1].x;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Расчет параметров макимального холма намыва
        /// </summary>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <param name="min"></param>     
        public void GetHillInterval(ref double xa, ref double xb, ref double max)
        {
            int idx = 0;
            max = points[idx].y;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].y > max)
                {
                    max = points[i].y; idx = i;
                }
            }
            if (max > 0 && points[0].y <= 0 && points[points.Count - 1].y <= 0)
            {
                // ход в лево
                if (idx == 0)
                    xa = points[idx].x;
                else
                {
                    for (int i = idx; i > 0; i--)
                    {
                        if (points[i].y > 0 && points[i - 1].y <= 0)
                        {
                            xa = points[i - 1].x;
                            break;
                        }
                    }
                }
            }
            // ход в право
            if (idx == points.Count - 1)
                xb = points[idx].x;
            else
            {
                for (int i = idx; i < points.Count - 1; i++)
                {
                    if (points[i].y > 0 && points[i + 1].y <= 0)
                    {
                        xb = points[i + 1].x;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Расчет метрики 
        /// </summary>
        /// <param name="A">функция А</param>
        /// <param name="B">функция И</param>
        /// <param name="measure">тип метрики</param>
        /// <param name="result">результат</param>
        public static void CalkMeasure(GraphicsCurve A, GraphicsCurve B, 
                           MeasureType measure, ref double result)
        {
            if (measure == MeasureType.LebesgueMeasure)
                CalkLebesgueMeasure(A, B, measure, ref result);
            else
                CalkChebyshevMeasure(A, B, measure, ref result);
        }
        /// <summary>
        /// Расчет метрики в пространстве С0
        /// </summary>
        /// <param name="A">функция А</param>
        /// <param name="B">функция И</param>
        /// <param name="measure">тип метрики</param>
        /// <param name="result">результат</param>
        public static void CalkChebyshevMeasure(GraphicsCurve A, GraphicsCurve B,
                           MeasureType measure, ref double result)
        {
            double xx, df;
            result = 0;
            if (A.points.Count == B.points.Count)
            {
                for (int i = 0; i < A.points.Count; i++)
                {
                    df = A.points[i].y - B.points[i].y;
                    result = Math.Max(result, Math.Abs(df));
                }
            }
            else
            {
                result = 0;
                GraphicsCurve tmp;
                if (A.points.Count < B.points.Count)
                {
                    tmp = B; B = A; A = tmp;
                }
                DigFunction fB = new DigFunction(B.points.ToArray());
                for (int i = 0; i < A.points.Count; i++)
                {
                    xx = A.points[i].x;
                    df = A.points[i].y - fB.FunctionValue(xx);
                    result = Math.Max(result, Math.Abs(df));
                }
            }
        }

        /// <summary>
        /// Расчет метрики в пространстве L2
        /// </summary>
        /// <param name="A">функция А</param>
        /// <param name="B">функция И</param>
        /// <param name="measure">тип метрики</param>
        /// <param name="result">результат</param>
        public static void CalkLebesgueMeasure(GraphicsCurve A, GraphicsCurve B,
                   MeasureType measure, ref double result)
        {
            double[] x = null;
            double[] y = null;
            result = 0;
            if (A.points.Count == B.points.Count)
            {
                MEM.Alloc(A.points.Count, ref x);
                MEM.Alloc(A.points.Count, ref y);
                for (int i = 0; i < A.points.Count; i++)
                {
                    x[i] = A.points[i].x;
                    double df = A.points[i].y - B.points[i].y;
                    y[i] = df * df;
                }
            }
            else
            {
                GraphicsCurve tmp;
                if (A.points.Count < B.points.Count)
                {
                    tmp = B; B = A; A = tmp;
                }
                MEM.Alloc(A.points.Count, ref x);
                MEM.Alloc(A.points.Count, ref y);
                DigFunction fB = new DigFunction(B.points.ToArray());
                for (int i = 0; i < A.points.Count; i++)
                {
                    x[i] = A.points[i].x;
                    double df = A.points[i].y - fB.FunctionValue(x[i]);
                    y[i] = df * df;
                }
            }
            result = Math.Sqrt(DMath.Integtal(x, y));
            return;
        }


        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public  IOFormater<IGraphicsCurve> GetFormater()
        {
            return new GraphicsCurveFormat();
        }
    }

    public enum MeasureType
    {
        LebesgueMeasure = 0,
        ChebyshevMeasure = 1
    }

}
