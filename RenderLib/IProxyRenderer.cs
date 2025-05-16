//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 13.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.10.2023 Потапов И.И. + clouds
//---------------------------------------------------------------------------

namespace RenderLib
{
    using System.Collections.Generic;
    using System.Drawing;
    using CommonLib;
    using CommonLib.Areas;
    using CommonLib.Delegate;
    using CommonLib.Geometry;
    using GeometryLib;
    using GeometryLib.Vector;
    using MeshLib;

    /// <summary>
    /// Интерфейс отрисовки данных (поддержка масштаба)
    /// </summary>
    public interface IProxyRenderer
    {
        /// <summary>
        /// Масштабирование рисунка от центра x, y
        /// </summary>
        /// <param name="x">координата мыши по х</param>
        /// <param name="y">координата мыши по у</param>
        /// <param name="delta">направление колеса мыши</param>
        void ZoomWheel(float x, float y, int delta);
        /// <summary>
        /// Масштабирование рисунка к максимальному от центра x, y
        /// </summary>
        /// <param name="x">координата мыши по х</param>
        /// <param name="y">координата мыши по у</param>
        void ZoomMax(float x, float y);
        /// <summary>
        /// Изменение области отрисовки к начальной
        /// </summary>
        void HandleResize();
        /// <summary>
        /// Инициализация
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// Интерфейс отрисовки данных для полей задачи
    /// </summary>
    public interface IProxyRendererMesh : IProxyRenderer
    {
        /// <summary>
        /// Индекс состаяния работ
        /// </summary>
        int IndexTask { get; set; }


        /// <summary>
        /// Установка данных для отрисовки
        /// </summary>
        /// <param name="data">данные для отрисовки</param>
        void SetData(SavePointData data);
    }


    /// <summary>
    /// Интерфейс отрисовки данных для полей задачи
    /// </summary>
    public interface IProxyRendererFields : IProxyRendererMesh
    {
        /// <summary>
        /// Точки створа
        /// </summary>
        PointF[] Points { get; set; }
        /// <summary>
        /// Установка створа
        /// </summary>
        /// <param name="points"></param>
        void SetTargetLine(PointF[] points);
        /// <summary>
        /// Установка опций отрисовки данных  задачи
        /// </summary>
        RenderOptionsFields renderOptions { get; set; }
        /// <summary>
        /// Установка цветовой схемы задачи 
        /// </summary>
        ColorSchemeFields colorScheme { get; set; }
    }
    /// <summary>
    /// Интерфейс для работы с сеткой задачи
    /// </summary>
    public interface IProxyRendererEditMesh : IProxyRendererMesh
    {
        /// <summary>
        /// Список узлов добавленных в область
        /// </summary>
        List<IHPoint> NewNodes { get; }
        /// <summary>
        /// добавить узлед с координатами
        /// </summary>
        void AddNod(IHPoint p);
        /// <summary>
        /// Установка опций отрисовки данных  задачи
        /// </summary>
        RenderOptionsEdit renderOptions { get; set; }
        /// <summary>
        /// Установка цветовой схемы задачи 
        /// </summary>
        ColorSchemeEdit colorScheme { get; set; }
        /// <summary>
        /// Запись данных в списки компонента
        /// </summary>
        /// <param name="cloudData">Данные для отрисовки</param>
        void SetData(IClouds clouds);
    }
    /// <summary>
    /// Интерфейс отрисовки данных для кривых задачи
    /// </summary>
    public interface IProxyRendererCurves : IProxyRenderer
    {
        /// <summary>
        /// Установка опций отрисовки данных  задачи
        /// </summary>
        RenderOptionsCurves renderOptions { get; set; }
        /// <summary>
        /// Установка цветовой схемы задачи 
        /// </summary>
        ColorScheme colorScheme { get; set; }
        /// <summary>
        /// Установка данных для отрисовки
        /// </summary>
        /// <param name="data">данные для отрисовки</param>
        void SetData(GraphicsData data);
        /// <summary>
        /// Переустановка региона для отрисовки
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void SetRegion(PointF a, PointF b);
        /// <summary>
        /// Изменение масштаба при выбоне новой группы кривых 
        /// </summary>
        void ReScale();
    }
    /// <summary>
    /// Интерфейс отрисовки данных для полей задачи
    /// </summary>
    public interface IProxyRendererReaderClouds : IProxyRenderer
    {
        /// <summary>
        /// Границы расчетной области
        /// </summary>
        IMArea Area { get; }
        /// <summary>
        /// Установка опций состояний редактора 
        /// </summary>
        EditRenderOptions editRenderOptions { get; set; }
        /// <summary>
        /// Установка опций отрисовки данных  задачи
        /// </summary>
        RenderOptionsFields renderOptions { get; set; }
        /// <summary>
        /// Установка цветовой схемы задачи 
        /// </summary>
        ColorSchemeClouds colorScheme { get; set; }
        /// <summary>
        /// Контур динамического фильтра
        /// </summary>
        List<CloudKnot> Conturs { get; set; }
        /// <summary>
        /// Установка данных для отрисовки
        /// </summary>
        /// <param name="data">данные для отрисовки</param>
        void SetData(IClouds data);
        /// <summary>
        /// Линии сглаживания
        /// </summary>
        List<IHSmLine> SLines { get; set; }
        /// <summary>
        /// Линии сглаживания
        /// </summary>
        IHLine crossLine { get; set; }
        /// <summary>
        /// Загрузка данных о вершине
        /// </summary>
        SendData<CloudKnot> sendPintData { get; set; }
        /// <summary>
        /// Загрузка данных о линиях сглаживания
        /// </summary>
        SendData<List<IHSmLine>> sendSListData { get; set; }
        /// <summary>
        ///  Удаление последней линий сглаживания
        /// </summary>
        /// <param name="p"></param>
        void DelSmLines();
        /// <summary>
        ///  удаление линии створа
        /// </summary>
        /// <param name="p"></param>
        void DelCrossLine();
        /// <summary>
        /// Удаление линий сглаживания
        /// </summary>
        void ClearSmLines();
        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        void UpCloudKnot(CloudKnot p);
        /// <summary>
        /// Программно добавить точку контура
        /// </summary>
        void AddCloudKnotToContur(CloudKnot knot);
        /// <summary>
        ///  обновление данных в точке контура
        /// </summary>
        /// <param name="p"></param>
        void DelCloudKnot();
        /// <summary>
        /// Очистка текущего контура
        /// </summary>
        void ClearCurContur();
        /// <summary>
        /// Загрузка линий сглаживания
        /// </summary>
        /// <param name="sl"></param>
        void LoadSmLines(List<IHSmLine> sl);
        #region Работа с контурами фигур и генерация сетки
        IMFigura GetFig(int idx);
        void SetStatusFig(int idx, FigureStatus status = FigureStatus.SelectedShape);
        void SetStatusFig(int idxFig, int idxPoint, FigureStatus status = FigureStatus.SelectedShape);
        void AddBoundMark(int FID, int[] idx);
        /// <summary>
        /// Установка индекса активной метки
        /// </summary>
        /// <param name="index"></param>
        void SetSelectIndex(int index);
        /// <summary>
        /// Добавить фигуру
        /// </summary>
        /// <returns></returns>
        List<string> AddFigs();
        /// <summary>
        /// Добавить готовую фигуру из загрузки
        /// </summary>
        /// <returns></returns>
        List<string> AddFigs(IMFigura fig);
        /// <summary>
        /// Удалить фигуру
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        List<string> RemoveFig(int index);
        /// <summary>
        /// Возвращение типа контура
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        FigureType GetFType(int index);
        /// <summary>
        /// Установка типа контура
        /// </summary>
        void SetFType(int index, FigureType ft);
        /// <summary>
        /// Установка атрибутов контура
        /// </summary>
        void SetAtributes(int index, double ice, double ks);
        /// <summary>
        /// Удалить фигуры
        /// </summary>
        void ClearFigs();
        /// <summary>
        /// Обновление узла
        /// </summary>
        /// <param name="p"></param>
        /// <param name="indexFig"></param>
        /// <param name="indexPoint"></param>
        void UpDatePoint(Vector2 coord, double[] atr, int indexFig, int indexPoint);
        /// <summary>
        /// Обновление активных позиций фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="PointsSelectedIndex"></param>
        /// <param name="SegmentSelectedIndex"></param>
        /// <param name="CountKnots"></param>
        /// <param name="Mark"></param>
        /// <param name="p"></param>
        //void UpDateFig(int FigSelectedIndex, int PointsSelectedIndex, int SegmentSelectedIndex, int CountKnots, int Mark, CloudKnot p);
        /// <summary>
        /// Обновление точек фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="PointsSelectedIndex"></param>
        /// <param name="p"></param>
        void UpDateFigPoint(int FigSelectedIndex, int PointsSelectedIndex, CloudKnot p);
        /// <summary>
        /// Обновление фигуры
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="SegmentSelectedIndex"></param>
        /// <param name="CountKnots">Количество вершин на сегменте</param>
        /// <param name="Marker">Маркер границы</param>
        void UpDateFigSegment(int FigSelectedIndex, int SegmentSelectedIndex, int CountKnots, int Marker);
        /// <summary>
        /// Групповое обновление количества узлов на сегментах
        /// </summary>
        /// <param name="FigSelectedIndex"></param>
        /// <param name="CountKnots"></param>
        void UpDateFigure(int FigSelectedIndex, int CountKnots);


        /// <summary>
        /// Выделение узла при отрисовке
        /// </summary>
        /// <param name="FigSelectedIndex">номер фигуры</param>
        /// <param name="PointsSelectedIndex">номер узла</param>
        void SelectKnot(int FigSelectedIndex, int PointsSelectedIndex);
        /// <summary>
        /// Выделение сегмента при отрисовке
        /// </summary>
        /// <param name="FigSelectedIndex">номер фигуры</param>
        /// <param name="PointsSelectedIndex">номер сегмента</param>
        void SelectSegment(int FigSelectedIndex, int SegmentSelectedIndex);
        /// <summary>
        /// Выбор активной линии сгласживания
        /// </summary>
        /// <param name="idx">номер линии</param>
        /// <param name="Count">количество внутренних вершин </param>
        double SelectSLines(int idx, ref int Count);
        /// <summary>
        /// Установить новое количество внутренних вершин линии сгласживания
        /// </summary>
        /// <param name="idx">номер линии</param>
        /// <param name="Count">количество внутренних вершин </param>
        void UpDateSLines(int idx, int Count);
        //void ClearNewFig();
        //void SetRegion(PointF a, PointF b);
        //RectangleWorld GetRegion();
        #endregion
    }

}
