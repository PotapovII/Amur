//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 21.06.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshLib.Locators
{
    using CommonLib;
    /// <summary>
    /// Связь
    /// </summary>
    public class Link : ILink
    {
        /// <summary>
        /// Ключ
        /// </summary>
        public int Key { get; set; }
        /// <summary>
        /// Адресс
        /// </summary>
        public int Adress { get; set; }
        public Link()
        {
            Key = 0; Adress = 0;
        }
        public Link(int k, int a)
        {
            Key = k; Adress = a;
        }
    }
}
