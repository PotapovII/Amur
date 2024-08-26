//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    using System.IO;
    public enum StatusFlag
    {
        NotActive,
        Activate
    };
    /// <summary>
    /// Класс ARivElement - это абстрактный базовый класс для различных объектов, 
    /// состоящих из связанных наборов CountNodes.
    /// Таким образом, он устанавливает только базовую форму таблицы связности 
    /// с направленным списком CountNodes и смежных объектов, которые должны 
    /// быть выделены конкретными объектами. 
    /// Этот объект объявляет некоторые из основных геометрических операций, 
    /// необходимых в анализе методом конечных элементов, через чистые функции.
    /// Реализация зависит от конкретных подклассов.
    /// ARivElement является производным от RVItem, поэтому его можно перечислить с 
    /// помощью контроллера типа RVList.
    /// </summary>
    public abstract class ARivElement 
    {
        public int ID;

        protected StatusFlag status;
        public StatusFlag Status => status; 
        public void Deactivate() { status = StatusFlag.NotActive; }
        public void Activate() { status = StatusFlag.Activate; }
        
        public ARivElement(int ID = 1) 
        {
            this.ID = ID;
            status = StatusFlag.Activate;
        }
        public abstract RivNode[] GetNodes();
        public abstract RivNode GetNode(int i);
        public abstract int GetNodeIndex(RivNode node);
        public abstract void SetNode(int i, RivNode elemNodes);
        public abstract ARivElement[] GetOwners { get; }
        public abstract ARivElement GetOwner(int i);
        public abstract void SetOwner(int i, ARivElement ap);
        public abstract int Count();
        public abstract int CountOwners();
        public abstract string NameType();
        public abstract ARivElement Inside(RivNode otherNodeP);
        /// <summary>
        /// Поиск индекса для выбранного объекта
        /// </summary>
        /// <param name="ap"></param>
        /// <returns></returns>
        public virtual int reflectAdj(ARivElement ap)
        {
            for (int j = CountOwners() - 1; j >= 0; j--)
            {
                var ep = GetOwners[j];
                if (ep == ap)
                    return j;
            }
            return -1;
        }
        public RivNode Centroid()
        {
            double x = 0.0, y = 0.0;

            for (int i = 0; i < Count(); i++)
            {
                x += GetNode(i).X;
                y += GetNode(i).Y;
            }
            RivNode nd = new RivNode(1, x / Count(), y / Count(), 0.0);
            return nd;
        }

        public void Write(StreamWriter file, ARivElement seg)
        {
            file.Write(seg.ID.ToString() + "  ");
            if (seg != null)
            {
                for (int i = 0; i < seg.Count(); i++)
                {
                    if (seg.GetNode(i) != null)
                        file.Write(seg.GetNode(i).ID + "  ");
                    else
                        file.Write("-1");
                }
                file.WriteLine();
            }
        }
    }
}
