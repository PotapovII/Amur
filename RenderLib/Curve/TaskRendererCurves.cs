//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 05.10.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using MemLogLib;
    using System;
    using System.Drawing;
    using MeshLib;
    using GeometryLib.World;

    /// <summary>
    /// ОО: Класс методов для отрисовки кривых
    /// </summary>
    public class TaskRendererCurves : TaskRenderer
    {
        /// <summary>
        /// Флаги объектов отрисовки
        /// </summary>
        public RenderOptionsCurves renderOptions = new RenderOptionsCurves();
        /// <summary>
        /// Цветовая схема отрисовки
        /// </summary>
        public ColorScheme colorScheme = new ColorScheme();
        /// <summary>
        /// одномерная сетка задачи в точке сохранения
        /// </summary>
        GraphicsData graphicsData;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref = "TaskRendererFields" />.
        /// </summary>
        public TaskRendererCurves(GraphicsData data)
        {
            SetTaskRendererCurves(data);
        }
        public void SetTaskRendererCurves(GraphicsData data)
        {
            if (data == null)
                return;
            graphicsData = data;
        }
        /// <summary>
        /// Визуализирует данные (сетку и поля)
        /// !!! Должно управлять визуализацией
        /// </summary>
        public void Render(Graphics g, WorldScaler zoom)
        {
            try
            {
                this.zoom = zoom;
                // Отрисовка функций 
                RenderCurve(g);
                // узлы
                RenderPoints(g);
                // Отрисовка координатных осей
                if (renderOptions.coordReper == true)
                    RenderCoordReper(g, colorScheme);
            }
            catch (Exception exc)
            {
                Logger.Instance.Exception(exc);
            }
        }
        /// <summary>
        /// Отрисовка узлов сетки и ее границы
        /// </summary>
        /// <param name="g"></param>
        private void RenderPoints(Graphics g)
        {
            if (graphicsData == null) return;
            PointF pt;
            string res;
            // Нарисуйте точки в области
            int Count = graphicsData.curves.Count;
            for (int index = 0; index < Count; index++)
            {
                var curve = graphicsData.curves[index];
                if (curve.Check == false)
                    continue;
                for (int i = 0; i < curve.Count; i++)
                {
                    if (renderOptions.coordInv == false)
                        pt = new PointF((float)curve[i].x, (float)curve[i].y);
                    else
                        pt = new PointF((float)curve[i].y, (float)curve[i].x);
                    
                    zoom.WorldToScreen(ref pt);
                    // узлы сетки
                    if (renderOptions.showMesh == true)
                    {
                        g.FillEllipse(colorScheme.BrushPoint, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);
                    }
                    // номера узлов сетки
                    if (renderOptions.showKnotNamber == true)
                    {
                        PointF pp = new PointF(pt.X + 10f, pt.Y);  
                        g.DrawString(i.ToString(), colorScheme.FontReper, colorScheme.BrushPoint, pp);
                    }
                    // числовые величины
                    if (renderOptions.opValuesKnot == true)
                    {
                        pt.X += 10f; pt.Y = pt.Y - colorScheme.FontValue.Size - 3;
                        res = ": " + string.Format(colorScheme.FormatText, curve[i].y);
                        g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                    }
                }
            }
        }
        /// <summary>
        /// Отрисовка функций 
        /// </summary>
        /// <param name="g"></param>
        private void RenderCurve(Graphics g)
        {
            int index = 0;
            int i = 0;
            try
            {
                if (graphicsData == null)
                    return;
                int SelectCount = graphicsData.SelectCount();
                int SelectIndex = 0;
                int Count = graphicsData.curves.Count;

                //Pen pen = new Pen(Color.Black, 2);
                for (index = 0; index < Count; index++)
                {
                    var curve = graphicsData.curves[index];
                    if (curve.Check == false)
                        continue;
                    PointF p0, p1;

                    int width = 2;
                    if (renderOptions.indexValues == index)
                        width = 3;
                    Pen pen;
                    if (renderOptions.opAutoColorCurves == true)
                        pen = colorScheme.GetPenGraphLine(SelectIndex, 0, SelectCount, width);
                    else
                        pen = colorScheme.PenGraphLine;
                    for (i = 0; i < curve.Count - 1; i++)
                    {
                        if(renderOptions.coordInv == false)
                        {
                            p0 = new PointF((float)curve[i].x, (float)curve[i].y);
                            p1 = new PointF((float)curve[i + 1].x, (float)curve[i + 1].y);
                        }
                        else
                        {
                            p0 = new PointF((float)curve[i].y, (float)curve[i].x);
                            p1 = new PointF((float)curve[i + 1].y, (float)curve[i + 1].x);
                        }
                        zoom.WorldToScreen(ref p0);
                        zoom.WorldToScreen(ref p1);
                        g.DrawLine(pen, p0, p1);
                    }
                    SelectIndex++;
                }
            }
            catch (Exception ex)
            {
                var curve = graphicsData.curves[index];
                var p = curve.GetGCurve().points.ToArray();
                Console.WriteLine();
                Console.WriteLine("i = {0}",i);
                Console.WriteLine();
                for (int j = 0; j < curve.Count - 1; j++)
                    Console.Write(" x[{1}] = {0:F4}", p[j].X,j);
                Console.WriteLine();
                for (int j = 0; j < curve.Count - 1; j++)
                    Console.Write(" y[{1}] = {0:F4}", p[j].Y, j);
                Console.WriteLine();
                Console.WriteLine(ex.Message);
            }
        }
    }
}



