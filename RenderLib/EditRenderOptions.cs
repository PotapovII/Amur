//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.06.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    /// <summary>
    /// Состояние редактора
    /// </summary>
    public enum EditState
    {
        /// <summary>
        /// Не работаем с редактором
        /// </summary>
        NoState = 0,
        /// <summary>
        ///  Работа с контуром
        /// </summary>
        Contur,
        /// <summary>
        ///  Работа с линиями сглаживания
        /// </summary>
        BeLine,
        /// <summary>
        ///  Работа со створом
        /// </summary>
        CrossLine
    }
    /// <summary>
    /// Установка опций для определения состояния редактора 
    /// </summary>
    public class EditRenderOptions
    {
        /// <summary>
        /// Состояние редактора
        /// </summary>
        public EditState editState;
        public EditRenderOptions() 
        {
            editState = EditState.NoState;
        }
        public EditRenderOptions(EditRenderOptions o)
        {
            editState = o.editState;           
        }
    }
}
