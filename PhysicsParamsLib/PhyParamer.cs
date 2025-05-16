//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.12.23
//---------------------------------------------------------------------------
using MemLogLib;

namespace PhysicsParamsLib
{
    /// <summary>
    /// Физические скалярные параметры общие для всех задач
    /// </summary>
    public class PhyParamer : IPhyParamer
    {
        /// <summary>
        /// Значение
        /// </summary>
        public double Value { get;  }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get;  }
        /// <summary>
        /// Метка параметра 
        /// </summary>
        public string Alias { get; }
        public PhyParamer(double Value, string Alias, string Name)
        {
            this.Value = Value;
            this.Name = Name;
            this.Alias = Alias;
        }
        public PhyParamer(IPhyParamer p)
        {
            this.Value =p.Value;
            this.Name = p.Name;
            this.Alias = p.Alias;
        }
        public static IPhyParamer Convert(string str)
        {
            string[] elems = str.Split(' ');
            double d = LOG.Double(elems[1]);
            IPhyParamer p = new PhyParamer(d, elems[0], elems[2]);
            return p;
        }
       public static IPhyParamer Parse(string str)
        {
            string[] elems = str.Split(' ');
            double d = LOG.Double(elems[1]);
            IPhyParamer p = new PhyParamer(d, elems[0], elems[2]);
            return p;
        }
        public string ToString(string str="")
        {
            string s = Value.ToString(str) + " " + Alias + " " + Name;
            return s;
        }
    }
}
