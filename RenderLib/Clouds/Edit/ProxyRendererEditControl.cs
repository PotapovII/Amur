//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 29.11.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using GeometryLib;
    using GeometryLib.Areas;
    using GeometryLib.Vector;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using CommonLib.Areas;
    using CommonLib.Delegate;

    /// <summary>
    /// ОО: Обертка для реального контрола используемая
    /// для удобства работы с контролом в формах
    /// </summary>
    public class ProxyRendererEditControl : IProxyRendererEditClouds
    {
        /// <summary>
        /// Границы расчетной области
        /// </summary>
        public IMArea Area { get => renderer.Area; }
        /// <summary>
        /// Реальный рендер (контрол)
        /// </summary>
        IProxyRendererEditClouds renderer = null;

        public ProxyRendererEditControl()
        {
            renderer = new CPRenderEditControlClouds();
        }
        /// <summary>
        /// Загрузка данных в вершине
        /// </summary>
        public SendData<CloudKnot> sendPintData 
        { 
            get => renderer.sendPintData; 
            set => renderer.sendPintData = value; 
        }

        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        public List<CloudKnot> Conturs 
        { 
            get => renderer.Conturs; 
            set => renderer.Conturs = value; 
        }
        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        public void UpCloudKnot(CloudKnot p)
        {
            renderer.UpCloudKnot(p);
        }
        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        public void DelCloudKnot()
        {
            renderer.DelCloudKnot();
        }
        /// <summary>
        /// Очистка текущего контура
        /// </summary>
        public void ClearCurContur()
        {
            renderer.ClearCurContur();
        }
        /// <summary>
        /// Доступ к контролу для его регистрации на форме/других контролах
        /// </summary>
        public Control RenderControl
        {
            get { return (Control)renderer; }
        }
        public void Initialize()
        {
            renderer.Initialize();
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        public RenderOptionsFields renderOptions
        {
            get { return renderer.renderOptions; }
            set { renderer.renderOptions = value; }
        }
        /// <summary>
        /// Установка цветовой схемы
        /// </summary>
        public ColorSchemeClouds colorScheme
        {
            get { return renderer.colorScheme; }
            set { renderer.colorScheme = value; }
        }
        /// <summary>
        /// Масштабирование рисунка от центра x, y
        /// </summary>
        /// <param name="x">координата мыши по х</param>
        /// <param name="y">координата мыши по у</param>
        /// <param name="delta">направление колеса мыши</param>
        public void ZoomWheel(float x, float y, int delta)
        {
            renderer.ZoomWheel(x, y, delta);
        }
        /// <summary>
        /// Масштабирование рисунка к максимальному от центра x, y
        /// </summary>
        /// <param name="x">координата мыши по х</param>
        /// <param name="y">координата мыши по у</param>
        public void ZoomMax(float x, float y)
        {
            renderer.ZoomMax(x, y);
        }
        /// <summary>
        /// Изменение области отрисовки к начальной
        /// </summary>
        public void HandleResize()
        {
            renderer.HandleResize();
        }
        /// <summary>
        /// Установка данных для отрисовки
        /// </summary>
        /// <param name="data">данные для отрисовки</param>
        public void SetData(IClouds data)
        {
            renderer.SetData(data);
        }
        //public void SetRegion(PointF a, PointF b)
        //{
        //    renderer.SetRegion(a, b);
        //}

        #region Работа с контурами фигур и генерация сетки
        public IMFigura GetFig(int idx)
        {
            return renderer.GetFig(idx);
        }
        public void SetStatusFig(int idx, FigureStatus status = FigureStatus.SelectedShape)
        {
            renderer.SetStatusFig(idx, status);
        }
        public void SetStatusFig(int idxFig, int idxPoint, FigureStatus status = FigureStatus.SelectedShape)
        {
            renderer.SetStatusFig(idxFig, idxPoint, status);
        }
        public void AddBoundMark(int FID, int[] idxs)
        {
            renderer.AddBoundMark(FID, idxs);
        }
        /// <summary>
        /// Установка индекса активной метки
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectIndex(int index)
        {
            renderer.SetSelectIndex(index);
        }
        /// <summary>
        /// Добавить фигуру
        /// </summary>
        /// <returns></returns>
        public List<string> AddFigs()
        {
            return renderer.AddFigs();
        }

        public List<string> RemoveFig(int index)
        {
            return renderer.RemoveFig(index);
        }

        /// <summary>
        /// Возвращение типа контура
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FigureType GetFType(int index)
        {
            return renderer.GetFType(index);
        }
        /// <summary>
        /// Установка типа контура
        /// </summary>
        public void SetFType(int index, FigureType ft)
        {
            renderer.SetFType(index, ft);
        }
        /// <summary>
        /// Удалить фигуры
        /// </summary>
        public void ClearFigs()
        {
            renderer.ClearFigs();
        }
        /// <summary>
        /// Обновление узла
        /// </summary>
        /// <param name="p"></param>
        /// <param name="indexFig"></param>
        /// <param name="indexPoint"></param>
        public void UpDatePoint(Vector2 coord, double[] atr, int indexFig, int indexPoint)
        {
            renderer.UpDatePoint( coord, atr,  indexFig,  indexPoint);
        }

        /// <summary>
        /// Обновление активных позиций фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="PointsSelectedIndex"></param>
        /// <param name="SegmentSelectedIndex"></param>
        /// <param name="CountKnots"></param>
        /// <param name="Mark"></param>
        /// <param name="p"></param>
        public void UpDateFig(int FigSelectedIndex, int PointsSelectedIndex, int SegmentSelectedIndex, int CountKnots, int Mark, CloudKnot p)
        {
            renderer.UpDateFig(FigSelectedIndex, PointsSelectedIndex, SegmentSelectedIndex, CountKnots, Mark, p);
        }

        /// <summary>
        /// Групповое обновление количества узлов на сегментах
        /// </summary>

        public void UpDateFigure(int FigSelectedIndex,int CountKnots)
        {
            renderer.UpDateFigure(FigSelectedIndex, CountKnots);
        }

        /// <summary>
        /// Выделение узла при отрисовке
        /// </summary>
        /// <param name="FigSelectedIndex">номер фигуры</param>
        /// <param name="PointsSelectedIndex">номер узла</param>
        public void SelectKnot(int FigSelectedIndex, int PointsSelectedIndex)
        {
            renderer.SelectKnot(FigSelectedIndex, PointsSelectedIndex);
        }
        /// <summary>
        /// Выделение сегмента при отрисовке
        /// </summary>
        /// <param name="FigSelectedIndex">номер фигуры</param>
        /// <param name="PointsSelectedIndex">номер сегмента</param>
        public void SelectSegment(int FigSelectedIndex, int SegmentSelectedIndex)
        {
            renderer.SelectSegment(FigSelectedIndex, SegmentSelectedIndex);
        }
        //public void ClearNewFig()
        //{
        //    conturs.Clear();
        //    // отрисовка отображения
        //    this.Render();
        //}
        //public void SetRegion(PointF a, PointF b)
        //{
        //    Area.SetRegion(a, b);
        //    HandleResize();
        //}
        //public RectangleWorld GetRegion()
        //{
        //    return Area.GetRegion();
        //}
        #endregion

    }
}
