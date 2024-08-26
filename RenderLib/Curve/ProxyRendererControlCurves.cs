namespace RenderLib
{
    //---------------------------------------------------------------------------
    //                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
    //                         проектировщик:
    //                           Потапов И.И.
    //---------------------------------------------------------------------------
    //                 кодировка : 13.12.2020 Потапов И.И.
    //---------------------------------------------------------------------------
    using System.Windows.Forms;
    using System.Drawing;
    using MeshLib;
    /// <summary>
    /// ОО: Обертка для реального контрола используемая
    /// для удобства работы с контролом в формах
    /// </summary>
    public class ProxyRendererControlCurves : IProxyRendererCurves
    {
        /// <summary>
        /// Реальный рендер (контрол)
        /// </summary>
        IProxyRendererCurves renderer = null;
        public ProxyRendererControlCurves()
        {
            renderer = new CPRenderControlCurves();
        }
        /// <summary>
        /// Доступ к контролу для его регистрации на форме/других контролах
        /// </summary>
        public Control RenderControl
        {
            get { return (Control)renderer; }
        }
        /// <summary>
        /// Инициализация компонент контрола
        /// </summary>
        public void Initialize()
        {
            renderer.Initialize();
        }
        /// <summary>
        /// Установка опций отрисовки
        /// </summary>
        public RenderOptionsCurves renderOptions
        {
            get { return renderer.renderOptions; }
            set { renderer.renderOptions = value; }
        }
        /// <summary>
        /// Установка цветовой схемы
        /// </summary>
        public ColorScheme colorScheme
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
        /// Изменение масштаба при выбоне новой группы кривых 
        /// </summary>
        public void ReScale()
        {
            renderer.ReScale();
        }
        /// <summary>
        /// Установка данных для отрисовки
        /// </summary>
        /// <param name="data">данные для отрисовки</param>
        public void SetData(GraphicsData data)
        {
            renderer.SetData(data);
            ReScale();
        }
        public void SetRegion(PointF a, PointF b)
        {
            renderer.SetRegion(a, b);
        }
    }
}
