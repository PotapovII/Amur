namespace MeshLib.ATable
{
    using CommonLib;
    using MemLogLib;
    /// <summary>
    /// Конечный элемент/адресная таблица
    /// </summary>
    public class Elements : IElements
    {
        /// <summary>
        /// Номера узлов на конечном элементе или 
        /// неизвестных в адресной таблице
        /// </summary>
        public uint[] Vertex { get; set; }
        /// <summary>
        /// Тип функции формы используемой для этого элемента
        /// </summary>
        public TypeFunForm TFunForm { get; set; }
        public Elements()
        {
            TFunForm = TypeFunForm.Form_2D_Triangle_L1;
            Vertex = new uint[3];
        }
        public Elements(Elements e)
        {
            TFunForm = e.TFunForm;
            Vertex = MEM.Copy(Vertex, e.Vertex);
        }
    }
}
