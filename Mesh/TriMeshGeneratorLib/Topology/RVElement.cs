//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using System.IO;
    public enum StatusFlag
    {
        NotActive,
        Activate
    };
    /// <summary>
    /// Класс RVElement - это абстрактный базовый класс для различных объектов, 
    /// состоящих из связанных наборов CountNodes.
    /// Таким образом, он устанавливает только базовую форму таблицы связности 
    /// с направленным списком CountNodes и смежных объектов, которые должны 
    /// быть выделены конкретными объектами. 
    /// Этот объект объявляет некоторые из основных геометрических операций, 
    /// необходимых в анализе методом конечных элементов, через чистые функции.
    /// Реализация зависит от конкретных подклассов.
    /// RVElement является производным от RVItem, поэтому его можно перечислить с 
    /// помощью контроллера типа RVList.
    /// </summary>
    public abstract class RVElement : RVItem
    {
        protected StatusFlag status;
        public StatusFlag Status => status; 
        public void Deactivate() { status = StatusFlag.NotActive; }
        public void Activate() { status = StatusFlag.Activate; }
        
        public RVElement(int id = 1) : base(id)
        {
            status = StatusFlag.Activate;
        }
        public abstract RVNode[] GetNodes();
        public abstract RVNode GetNode(int i);
        public abstract int GetNodeIndex(RVNode node);
        public abstract void SetNode(int i, RVNode elemNodes);
        public abstract RVElement[] GetOwners { get; }
        public abstract RVElement GetOwner(int i);
        public abstract void SetOwner(int i, RVElement ap);
        public abstract int Count();
        public abstract int CountOwners();
        public abstract string NameType();
        public abstract RVElement Inside(RVNode otherNodeP);
        public virtual int reflectAdj(RVElement ap)
        {
            for (int j = CountOwners() - 1; j >= 0; j--)
            {
                var ep = GetOwners[j];
                if (ep == ap)
                    return j;
            }
            return -1;
        }
        public RVNode Centroid()
        {
            double x = 0.0, y = 0.0;

            for (int i = 0; i < Count(); i++)
            {
                x += GetNode(i).X;
                y += GetNode(i).Y;
            }
            RVNode nd = new RVNode(1, x / Count(), y / Count(), 0.0);
            return nd;
        }

        public void Write(StreamWriter file, RVElement seg)
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
