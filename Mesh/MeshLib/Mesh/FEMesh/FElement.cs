//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//---------------------------------------------------------------------------
//  кодировка HElemFE (последняя правка) 14.03.2001 Потапов И.И. (c++)
//  перенос  HElemFE => FElement :   23.01.2022 Потапов И.И. (с++ => c#)
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib;
    using MemLogLib;
    /// <summary>
    ///ОО: Конечный элемент произвольной базисной сетки
    /// </summary>
    [Serializable]
    public class FElement : IFElement
    {
        /// <summary>
        /// номер элемента
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Метка границы 
        /// </summary>
        public int MarkBC { get; set; }
        /// <summary>
        /// Количество узлов на элементе
        /// </summary>
        public int Length
        {
            get { return Nods == null ? 0 : Nods.Length; }
        }
        /// <summary>
        /// узлы конечного элемента
        /// </summary>
        public IFENods[] Nods { get; set; }
        /// <summary>
        /// Тип функции формы основного КЭ 
        /// </summary>
        public TypeFunForm TFunForm => tFForm;
        protected TypeFunForm tFForm;
        protected void Set(int ID, int MarkBC, TypeFunForm tFForm = TypeFunForm.Form_2D_Triangle_L1, int cu = 3)
        {
            this.tFForm = tFForm;
            this.MarkBC = MarkBC;
            this.ID = ID;
            Nods = new IFENods[cu];
        }
        protected FElement(int ID, int MarkBC, TypeFunForm tFForm = TypeFunForm.Form_2D_Triangle_L1, int cu = 3)
        {
            Set(ID, MarkBC, tFForm);
        }
        public FElement(int cu = 3, int ID = 0, int MarkBC = 0, 
            TypeFunForm tFForm = TypeFunForm.Form_2D_Triangle_L1)
        {
            Set(ID, MarkBC, tFForm, cu);
            for (int i = 0; i < Nods.Length; i++)
                Nods[i] = new FENods();
        }
        public FElement(int[] nods, int ID = 0, int MarkBC = 0,
            TypeFunForm tFForm = TypeFunForm.Form_2D_Triangle_L1)
        {
            Set(ID, MarkBC, tFForm, nods.Length);
            for (int i = 0; i < Nods.Length; i++)
                Nods[i] = new FENods(nods[i], MarkBC);
        }
        public FElement(IFElement e)
        {
            if (e != null)
            {
                if (e.Nods != null)
                {
                    Set(e.ID, e.MarkBC, e.TFunForm, e.Nods.Length);
                    for (int i = 0; i < Nods.Length; i++)
                        Nods[i] = new FENods(e.Nods[i]);
                }
            }
        }

        public FElement(int Vertex1, int Vertex2, int Vertex3, int ID = 0, int MarkBC = 0)
        {
            Set(ID, MarkBC, TypeFunForm.Form_2D_Triangle_L1, 3);
            Nods[0] = new FENods(Vertex1, MarkBC);
            Nods[1] = new FENods(Vertex2, MarkBC);
            Nods[2] = new FENods(Vertex3, MarkBC);
        }
        public FElement(TriElement e, int ID = 0,int MarkBC =0)
        {
            Set(ID, MarkBC, TypeFunForm.Form_2D_Triangle_L1, 3);
            Nods[0] = new FENods((int)e.Vertex1, MarkBC);
            Nods[1] = new FENods((int)e.Vertex2, MarkBC);
            Nods[2] = new FENods((int)e.Vertex3, MarkBC);
        }
        public FElement(TwoElement e, int ID = 0, int MarkBC = 0)
        {
            Set(ID, MarkBC, TypeFunForm.Form_1D_L1, 2);
            Nods[0] = new FENods((int)e.Vertex1, MarkBC);
            Nods[1] = new FENods((int)e.Vertex2, MarkBC);
        }
        public FElement(int Vertex1, int Vertex2, int ID = 0, int MarkBC = 0)
        {
            Set(ID, MarkBC, TypeFunForm.Form_1D_L1, 2);
            Nods[0] = new FENods(Vertex1, MarkBC);
            Nods[1] = new FENods(Vertex2, MarkBC);
        }
        /// <summary>
        /// индексатор
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IFENods this[int index]
        {
            get => Nods[(int)index];
            set => Nods[(int)index] = value;
        }
        /// <summary>
        /// узлы элемента
        /// </summary>
        /// <param name="knots"></param>
        public void ElementKnots(ref uint[] knots)
        {
            MEM.Alloc(Length, ref knots);
            for (int i = 0; i < Nods.Length; i++)
                knots[i] = (uint)Nods[i].ID; 
        }
        /// <summary>
        /// Вывод простого объекта в строку
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = ID.ToString();
            str += " " + MarkBC.ToString();
            str += " " + ((int)tFForm).ToString();
            for (int i = 0; i < Nods.Length; i++)
                str += " " + Nods[i].ToString();
            return str;
        }
        /// <summary>
        /// Чтение простого объекта из строки
        /// </summary>
        public IFElement Parse(string line)
        {
            try
            {
                int shift = 3;
                string[] lines = line.Split(' ');
                int nods = (lines.Length - shift) / 2;
                IFElement me = new FElement(nods, int.Parse(lines[0]), int.Parse(lines[1]), (TypeFunForm)int.Parse(lines[2]));
                int k = 0;
                for (int i = 2; i < lines.Length - shift; i += 2)
                    me.Nods[k++] = new FENods(int.Parse(lines[i]), int.Parse(lines[i + 1]));
                return new FElement();
            }
            catch (Exception ee)
            {
                Logger.Instance.Exception(ee);
                return new FElement();
            }
        }
        /// <summary>
        /// Проверка на эквивалентнойсть
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(IFElement elem)
        {
            if (elem == null) return false;
            if (Length != elem.Length) return false;
            bool Equal = true;
            for (int i = 0; i < Length; i++)
                if (Nods[i].ID != elem.Nods[i].ID)
                {
                    Equal = false; break;
                }
            if (Equal == true) return true;
            Equal = true;
            for (int i = 0; i < Length; i++)
                if (Nods[Length - i - 1].ID != elem.Nods[i].ID)
                {
                    Equal = false; break;
                }
            if (Equal == true) return true;
            return false;
        }
        /// <summary>
        /// Проверка на эквивалентнойсть
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            IFElement elem = obj as IFElement;
            return Equals(elem);
        }
        /// <summary>
        /// используется для ускорения сравнения двух объектов
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Nods[0].ID * Nods[Length - 1].ID;
        }
    }

}
