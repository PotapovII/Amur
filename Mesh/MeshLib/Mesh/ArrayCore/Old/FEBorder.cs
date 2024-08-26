////---------------------------------------------------------------------------
////                          ПРОЕКТ  "МКЭ"
////                         проектировщик:
////                           Потапов И.И.
////---------------------------------------------------------------------------
////                 кодировка : 15.07.2022 Потапов И.И.
////---------------------------------------------------------------------------
//namespace MeshLib
//{
//    using System;
//    using CommonLib;
//    /// <summary>
//    /// Маркер границы привязан не к граничным узлам а 
//    /// к граничным элементам
//    /// </summary>
//    [Serializable]
//    public class FEBorder : IFEBorder
//    {
//        /// <summary>
//        /// тип граничного услоия 
//        /// </summary>
//        public TypeBoundCond TypeBC { get; set; }
//        /// <summary>
//        /// Метка границы 
//        /// </summary>
//        public int MarkBC { get; set; }
//        public FEBorder() 
//        {
//            MarkBC = 0;
//            TypeBC = TypeBoundCond.Dirichlet;
//        }
//        public FEBorder(int m, TypeBoundCond t = TypeBoundCond.Dirichlet)
//        {
//            MarkBC = m;
//            TypeBC = t;
//        }
//        public FEBorder(IFEBorder p)
//        {
//            MarkBC = p.MarkBC;
//            TypeBC = p.TypeBC;
//        }
//    }
//}