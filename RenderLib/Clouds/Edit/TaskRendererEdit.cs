//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.10.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System;
    using System.Drawing;
    using CommonLib;
    using MemLogLib;
    using GeometryLib;
    using System.Linq;
    using GeometryLib.Vector;
    using GeometryLib.World;

    /// <summary>
    /// ОО: Класс методов для отрисовки данных
    /// </summary>
    public class TaskRendererEdit : TaskRenderer
    {
        double[] Values = null;
        double MaxV = 0;
        double MinV = 0;
        double[] ValuesX = null;
        double[] ValuesY = null;
        /// <summary>
        /// Флаги объектов отрисовки
        /// </summary>
        public RenderOptionsFields renderOptions = new RenderOptionsFields();
        /// <summary>
        /// Цветовая схема отрисовки
        /// </summary>
        public ColorSchemeFields colorScheme = new ColorSchemeFields();
        /// <summary>
        /// Сетка для отрисовки геометрии области и полей
        /// </summary>
        IClouds clouds;
        /// <summary>
        /// буфера для координат сетки
        /// </summary>
        double[] X;
        double[] Y;
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref = "TaskRendererFields" />.
        /// </summary>
        public TaskRendererEdit(IClouds clouds)
        {
            if (clouds == null) return;
            this.clouds = clouds;
            X = clouds.GetCoords(0);
            Y = clouds.GetCoords(1);
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
                // граница - узлы и флаги
                RenderBoundaryKnots(g);
                // узлы
                if (renderOptions.showKnotNamber == true)
                    RenderPoints(g);
                // числовые величины
                if (renderOptions.opValuesKnot == true)
                    RenderValuePoints(g);
                // векторые поля
                if (renderOptions.opVectorValues == true)
                    RenderVectorField(g);
                // отрисовка фильтра
                if(renderOptions.opTargetLine == true)
                    RenderTargetLine(g);
                // Отрисовка координатных осей
                if (renderOptions.coordReper == true)
                    RenderCoordReper(g, colorScheme);
            }
            catch (Exception exc)
            {
                Logger.Instance.Info(exc.Message);
            }
        }
        /// <summary>
        /// отрисовка створа
        /// </summary>
        /// <param name="g"></param>
        public void RenderTargetLine(Graphics g)
        {
            if (renderOptions.showFilter == 0)
            {
                PointF pa = new PointF((float)renderOptions.a.X, (float)renderOptions.a.Y);
                PointF pb = new PointF((float)renderOptions.b.X, (float)renderOptions.b.Y);
                zoom.WorldToScreen(ref pa);
                zoom.WorldToScreen(ref pb);
                g.FillEllipse(colorScheme.BrushPoint, pa.X - 1.5f, pa.Y - 1.5f, 3, 3);
                g.FillEllipse(colorScheme.BrushPoint, pb.X - 1.5f, pb.Y - 1.5f, 3, 3);
                PointF pet = new PointF(pb.X, pa.Y);
                PointF pwb = new PointF(pa.X, pb.Y);

                g.DrawLine(colorScheme.PenBoundaryLine, pa, pet);
                g.DrawLine(colorScheme.PenBoundaryLine, pet, pb);
                g.DrawLine(colorScheme.PenBoundaryLine, pb, pwb);
                g.DrawLine(colorScheme.PenBoundaryLine, pwb, pa);
            }
        }
        /// <summary>
        /// Отрисовка узлов 
        /// </summary>
        /// <param name="g"></param>
        private void RenderPoints(Graphics g)
        {
            if (clouds == null) return;
            int i;
            PointF pt;
            // Нарисуйте точки в области
            for (i = 0; i < X.Length; i++)
            {
                pt = new PointF((float)X[i], (float)Y[i]);
                zoom.WorldToScreen(ref pt);

                g.FillEllipse(colorScheme.BrushPoint, pt.X - 1.5f, pt.Y - 1.5f, 3, 3);

                if (renderOptions.showKnotNamber == true)
                {
                    PointF pp = new PointF(pt.X + 10f, pt.Y);
                    g.DrawString(i.ToString(), colorScheme.FontKnot, colorScheme.BrushTextKnot, pp);
                }
            }
        }
        /// <summary>
        /// Отрисовка полей в узлах сетки
        /// </summary>
        /// <param name="g"></param>
        private void RenderValuePoints(Graphics g)
        {
            // Индекс поля
            if (clouds == null) return;
            int indexPole = renderOptions.indexValues;
            // Поле
            IField pole = clouds.GetPole(indexPole);
            if (pole == null)
                return;
            int i;
            PointF pt;
            string res;
            if (pole.Dimention == 1)
            {
                ValuesX = ((Field1D)pole).Values;
                for (i = 0; i < X.Length; i++)
                {
                    pt = new PointF((float)X[i], (float)Y[i]);
                    zoom.WorldToScreen(ref pt);
                    pt.X += 10f; pt.Y = pt.Y - colorScheme.FontValue.Size - 3;
                    res = ": " + String.Format(colorScheme.FormatText, ValuesX[i]);
                    g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                }
            }
            else
            {
                //pole.GetValue(ref ValuesX, ref ValuesY);
                Vector2[] val = ((Field2D)pole).Values;
                MEM.Alloc(val.Length, ref ValuesX);
                MEM.Alloc(val.Length, ref ValuesY);
                for (i = 0; i < X.Length; i++)
                {
                    ValuesX[i] = val[i].X;
                    ValuesY[i] = val[i].Y;
                }
                for (i = 0; i < X.Length; i++)
                {
                    pt = new PointF((float)X[i], (float)Y[i]);
                    zoom.WorldToScreen(ref pt);
                    pt.X += 10f; pt.Y = pt.Y - colorScheme.FontValue.Size - 3;
                    res = "> " + String.Format(colorScheme.FormatText, ValuesX[i]);
                    g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                    pt.Y = pt.Y - colorScheme.FontValue.Size - 2;
                    res = "^ " + String.Format(colorScheme.FormatText, ValuesY[i]);
                    g.DrawString(res, colorScheme.FontValue, colorScheme.BrushTextValues, pt);
                }
            }
        }

        /// <summary>
        /// Отрисовка граничных узлов области
        /// </summary>
        /// <param name="g"></param>
        private void RenderBoundaryKnots(Graphics g)
        {
            PointF p0;
            if (renderOptions.showBoudaryKnots == true)
            {
                int indexPole = renderOptions.indexValues;
                // Поле
                IField pole = clouds.GetPole(indexPole);
                if (pole == null)
                    return;
                if (pole.Dimention == 1)
                {
                    Values = ((Field1D)pole).Values;
                    MinV = Values.Min();
                    MaxV = Values.Max();

                }
                for (int i = 0; i < X.Length; i++)
                {
                    p0 = new PointF((float)X[i], (float)Y[i]);
                    zoom.WorldToScreen(ref p0);
                    Color col = colorScheme.RGBBrush(Values[i], MinV, MaxV);
                    SolidBrush solidBrush = new SolidBrush(col);
                    g.FillEllipse(solidBrush, p0.X - 1.5f, p0.Y - 1.5f, 3, 3);
                }
            }
            else
            {
                for (int i = 0; i < X.Length; i++)
                {
                    p0 = new PointF((float)X[i], (float)Y[i]);
                    zoom.WorldToScreen(ref p0);
                    g.FillEllipse(colorScheme.BrushPoint, p0.X - 1.5f, p0.Y - 1.5f, 3, 3);
                }
            }
        }
        
        /// <summary>
        /// Векторное поле
        /// </summary>
        /// <param name="g"></param>
        private void RenderVectorField(Graphics g)
        {
            if (clouds == null) return;
            // Индекс поля
            if (renderOptions.indexValues == -1)
                return;
            IField pole = clouds.GetPole(renderOptions.indexValues);
            int Dim = 0;
            if (CloudsUtils.GetPoleMinMax(pole, ref MinV, ref MaxV, ref Values, ref ValuesX, ref ValuesY, ref Dim) == false)
                return;
            if (Dim == 1)
                return;
            if (ValuesY == null)
                return;
            int line = 50;
            PointF pt, pv;
            double LValue = MaxV - MinV;
            for (int i = 0; i < Values.Length; i++)
            {
                pt = new PointF((float)X[i], (float)Y[i]);
                zoom.WorldToScreen(ref pt);
                g.FillEllipse(colorScheme.BrushPoint, pt.X - 2.5f, pt.Y - 2.5f, 5, 5);

                float dxv = (float)(line * ValuesX[i] / LValue);
                float dyv = (float)(line * ValuesY[i] / LValue);
                pv = new PointF(pt.X + dxv, pt.Y - dyv);
                g.DrawLine(colorScheme.PenVectorLine, pt, pv);
            }
        }



    }
}



