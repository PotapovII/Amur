//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.10.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using CommonLib.Delegate;
    using GeometryLib;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    /// <summary>
    /// ОО: Обертка для реального контрола используемая
    /// для удобства работы с контролом в формах
    /// </summary>
    public class ProxyRendererControlClouds : IProxyRendererClouds
    {
        /// <summary>
        /// Точки фильтра
        /// </summary>
        public PointF[] Points { get=> renderer.Points; set=> renderer.Points = value; }
        /// <summary>
        /// Установка фильтра
        /// </summary>
        /// <param name="points"></param>
        public void SetTargetLine(PointF[] points)
        {
            renderer.SetTargetLine(points);
        }
        /// <summary>
        /// Реальный рендер (контрол)
        /// </summary>
        IProxyRendererClouds renderer = null;

        public ProxyRendererControlClouds()
        {
            renderer = new CPRenderControlClouds();
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
        public void SetRegion(PointF a, PointF b)
        {
            renderer.SetRegion(a, b);
        }
    }
}
