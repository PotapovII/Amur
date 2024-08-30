//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 18.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib.Geometry;

    using System.Windows.Forms;
    using System.Collections.Generic;
    using CommonLib;

    /// <summary>
    /// ОО: Обертка для реального контрола используемая
    /// для удобства работы с контролом в формах
    /// </summary>
    public class ProxyRendererControlEdit : IProxyRendererEditMesh
    {
        /// <summary>
        /// Индекс состаяния работ
        /// </summary>
        public int IndexTask { get=> renderer.IndexTask; set => renderer.IndexTask = value; }
        /// <summary>
        /// Список узлов добавленных в область
        /// </summary>
        public List<IHPoint> NewNodes { get=> renderer.NewNodes; }
        /// <summary>
        /// добавить узлед с координатами
        /// </summary>
        public void AddNod(IHPoint p)
        {
            renderer.NewNodes.Add(p);
        }
        /// <summary>
        /// Реальный рендер (контрол)
        /// </summary>
        IProxyRendererEditMesh renderer = null;
        public ProxyRendererControlEdit()
        {
            renderer = new CPRenderControlEdit();
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
        public RenderOptionsEdit renderOptions
        {
            get { return renderer.renderOptions; }
            set { renderer.renderOptions = value; }
        }
        /// <summary>
        /// Установка цветовой схемы
        /// </summary>
        public ColorSchemeEdit colorScheme
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
        /// <summary>
        /// Запись данных в списки компонента
        /// </summary>
        /// <param name="cloudData">Данные для отрисовки</param>
        public void SetData(IClouds clouds)
        {
            renderer.SetData(clouds);
        }
    }
}
