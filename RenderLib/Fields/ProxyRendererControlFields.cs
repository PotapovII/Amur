//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 13.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using System.Drawing;
    using System.Windows.Forms;
    /// <summary>
    /// ОО: Обертка для реального контрола используемая
    /// для удобства работы с контролом в формах
    /// </summary>
    public class ProxyRendererControlFields : IProxyRendererFields
    {
        /// <summary>
        /// Индекс состаяния работ
        /// </summary>
        public int IndexTask { get=> renderer.IndexTask; set => renderer.IndexTask = value; }
        /// <summary>
        /// Точки створа
        /// </summary>
        public PointF[] Points { get=> renderer.Points; set=> renderer.Points = value; }
        ///// <summary>
        ///// индекс точки стрвора
        ///// </summary>
        //public int pIndex { get=> renderer.pIndex; set=> renderer.pIndex = value; }
        /// <summary>
        /// Установка створа
        /// </summary>
        /// <param name="points"></param>
        public void SetTargetLine(PointF[] points)
        {
            renderer.SetTargetLine(points);
        }
        /// <summary>
        /// Реальный рендер (контрол)
        /// </summary>
        IProxyRendererFields renderer = null;
        public ProxyRendererControlFields()
        {
            renderer = new CPRenderControlFields();
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
        public ColorSchemeFields colorScheme
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
        public void SetData(SavePointData data)
        {
            renderer.SetData(data);
        }
    }
}
