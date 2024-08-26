//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 19.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace GeometryLib.Aalgorithms
{
    using MemLogLib;
    using CommonLib.Geometry;

    using System.Collections.Generic;
    /// <summary>
    /// Поиск выпуклой оболочки, алгоритм Грэхема
    /// </summary>
    public interface IConvexHull
    {
        /// <summary>
        /// Найти выпуклую оболочку для заданного набора точек по алгоритму Грэхема
        /// </summary>
        /// <param name="points">Набор точек для поиска выпуклой оболочки.</param>
        /// <returns>Возвращает набор точек, которые образуют выпуклую оболочку</returns>
        void FindHull(IHPoint[] points, ref IHPoint[] result);
    }
    /// <summary>
    /// Поиск выпуклой оболочки, алгоритм Грэхема
    /// </summary>
    public class ConvexHull : IConvexHull
    {
        public ConvexHull() { }
        /// <summary>
        /// Найти выпуклую оболочку для заданного набора точек по алгоритму Грэхема
        /// </summary>
        /// <param name="points">Набор точек для поиска выпуклой оболочки</param>
        /// <param name="result">Возвращает набор точек, которые образуют выпуклую оболочку</param>
        public void FindHull(IHPoint[] points, ref IHPoint[] result)
        {
            if (points.Length <= 3)
            {
                result = points;
                MEM.Copy(result, result);
            }
            List<PointToProcess> pointsToProcess = new List<PointToProcess>();
            // преобразовать входные точки в точки, которые мы можем обработать
            for(int i=0; i<points.Length; i++)
                pointsToProcess.Add(new PointToProcess(points[i],i));

            // найти точку с наименьшим значением X и наименьшим значением Y
            int startID = 0;
            PointToProcess startPoint = pointsToProcess[0];

            for (int i = 1, n = pointsToProcess.Count; i < n; i++)
            {
                if ((pointsToProcess[i].X < startPoint.X) ||
                     ((pointsToProcess[i].X == startPoint.X) && 
                     (pointsToProcess[i].Y < startPoint.Y)))
                {
                    startPoint = pointsToProcess[i];
                    startID = i;
                }
            }
            // удалить только что найденную точку
            pointsToProcess.RemoveAt(startID);

            // найти альфа (тангенс угла наклона прямой) и квадрат
            // расстояния до первого угла
            for (int i = 0, n = pointsToProcess.Count; i < n; i++)
            {
                var dx = pointsToProcess[i].X - startPoint.X;
                var dy = pointsToProcess[i].Y - startPoint.Y;
                // квадратный корень не нужен, так как в нашем случае он не важен
                pointsToProcess[i].Distance = dx * dx + dy * dy;
                // тангенс угла наклона линий
                pointsToProcess[i].Alpha = (dx == 0) ? float.PositiveInfinity : (float)dy / dx;
            }

            // сортировка точек по углу и расстоянию
            pointsToProcess.Sort();
            // список точек оболочки
            List<PointToProcess> convexHullTemp = new List<PointToProcess>();

            // добавить первый угол, который всегда находится на оболочке
            convexHullTemp.Add(startPoint);
            // добавить еще одну точку, которая образует линию с наименьшим наклоном
            convexHullTemp.Add(pointsToProcess[0]);
            pointsToProcess.RemoveAt(0);

            PointToProcess lastPoint = convexHullTemp[1];
            PointToProcess prevPoint = convexHullTemp[0];

            while (pointsToProcess.Count != 0)
            {
                PointToProcess newPoint = pointsToProcess[0];

                // пропустить любую точку, которая имеет тот же наклон,
                // что и предыдущая, или имеет нулевое расстояние до первой точки
                if ((newPoint.Alpha == lastPoint.Alpha) || 
                    (newPoint.Distance == 0))
                {
                    pointsToProcess.RemoveAt(0);
                    continue;
                }
                // проверяем, находится ли текущая точка слева от двух последних точек
                if ((newPoint.X - prevPoint.X) * (lastPoint.Y - newPoint.Y) - 
                    (lastPoint.X - newPoint.X) * (newPoint.Y - prevPoint.Y) < 0)
                {
                    // добавляем точку в оболочку
                    convexHullTemp.Add(newPoint);
                    // и удаляем ее из списка точек для обработки
                    pointsToProcess.RemoveAt(0);

                    prevPoint = lastPoint;
                    lastPoint = newPoint;
                }
                else
                {
                    // удалить последнюю точку из оболочки
                    convexHullTemp.RemoveAt(convexHullTemp.Count - 1);
                    lastPoint = prevPoint;
                    prevPoint = convexHullTemp[convexHullTemp.Count - 2];
                }
            }

            // конвертировать точки обратно
            //List<IHPoint> convexHull = new List<IHPoint>();
            //foreach (PointToProcess pt in convexHullTemp)
            //{
            //    convexHull.Add(pt.IClone());
            //}
            //result = convexHull.ToArray();

            // выбрать из points точки оболочки сохраняя при этом их тип
            MEM.Alloc(convexHullTemp.Count, ref result);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = points[convexHullTemp[i].ID];
            }
        }
        /// <summary>
        /// Внутренний класс - компаратор для сортировки облака точек
        /// </summary>
        protected class PointToProcess : HPoint
        {
            public int ID;
            public double Alpha;
            public double Distance;
            public PointToProcess(IHPoint point, int iD):base(point)
            {
                Alpha = 0;
                Distance = 0;
                ID = iD;
            }
            public PointToProcess(PointToProcess p) : base(p)
            {
                Alpha = p.Alpha;
                Distance = p.Distance;
                ID = p.ID;
            }
            public override int CompareTo(object obj)
            {
                PointToProcess another = (PointToProcess)obj;
                return (Alpha < another.Alpha) ? -1 : 
                        (Alpha > another.Alpha) ? 1 :
                        ((Distance > another.Distance) ? -1 : 
                        (Distance < another.Distance) ? 1 : 0);
            }
            public override IHPoint IClone()
            {
                return new PointToProcess(this);
            }
        }
    }
}
