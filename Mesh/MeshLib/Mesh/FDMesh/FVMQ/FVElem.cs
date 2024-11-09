//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И. 
//                кодировка : 26.06.2022 Потапов И.И. 
//---------------------------------------------------------------------------
namespace MeshLib
{
    using System;
    using CommonLib.Geometry;
    using GeometryLib;
    using MemLogLib;
    /// <summary>
    /// Образ контрольного объема для произвольной сетки
    /// 
    ///     | <------hw ------->|
    ///     
    ///    x0 y0               x3 y3   
    /// ---- |---------*---------|-----> j (y)
    ///  ^   |         ^         |  ^
    ///  |   |         |         |  |
    ///  |   |        dw         |  |
    ///  |   |         |         |  |
    ///  |   |         V         |  |
    /// hs   |<--ds--->*<---dn-->|  hn
    ///  |   |         ^         |  |
    ///  |   |         |         |  |
    ///  |   |        de         |  |
    ///  |   |         |         |  |
    ///  V   |         V         |  V
    /// ---- |---------*---------|---
    ///    x1 y1                x2 y2  
    ///      |                   
    ///      | <------he ------->|
    ///      |  
    ///      V  i (x)
    ///      
    /// </summary>
    /// <summary>
    /// Класс - одномерная функция растяжения и сгущения
    ///     По К. Флетчеру ВМ в ДЖ т. 2 ст. 123
    ///         (С) Потапов И.И. 23.11.2014
    /// </summary>
    [Serializable]
    public class FVElem : IFVElem
    {
        /// <summary>
        /// Опорные точки
        /// </summary>
        protected HPoint p0;
        protected HPoint p1;
        protected HPoint p2;
        protected HPoint p3;
        /// <summary>
        ///  Координаты центра КО
        /// </summary>
        public HPoint p { get; set; }
        /// <summary>
        /// длина грани КО ( x0 y0 -  x1 y1 )
        /// </summary>
        public double hs { get; set; }
        /// <summary>
        /// длина грани КО ( x1 y1 -  x2 y2 )
        /// </summary>
        public double he { get; set; }
        /// <summary>
        /// длина грани КО ( x2 y2 -  x3 y3 )
        /// </summary>
        public double hn { get; set; }
        /// <summary>
        /// длина грани КО ( x3 y3 -  x0 y0 )
        /// </summary>
        public double hw { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани e
        /// </summary>
        public double de { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани w
        /// </summary>
        public double dw { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани n
        /// </summary>
        public double dn { get; set; }
        /// <summary>
        /// Растояние от центра КЭ к центрц грани s
        /// </summary>
        public double ds { get; set; }
        /// <summary>
        /// Площадь КО
        /// </summary>
        public double S { get; set; }
        public FVElem() { }
        public FVElem(HPoint p0, HPoint p1, HPoint p2, HPoint p3)
        {
            this.p0 = new HPoint(p0);
            this.p1 = new HPoint(p1);
            this.p2 = new HPoint(p2);
            this.p3 = new HPoint(p3);

            this.p = 0.25 * (p0 + p1 + p2 + p3);
            this.hs = HPoint.Length(p0, p1);
            this.he = HPoint.Length(p1, p2);
            this.hn = HPoint.Length(p2, p3);
            this.hw = HPoint.Length(p3, p1);
            this.ds = HPoint.Length(p, 0.5 * (p0 + p1));
            this.de = HPoint.Length(p, 0.5 * (p1 + p2));
            this.dn = HPoint.Length(p, 0.5 * (p2 + p3));
            this.dw = HPoint.Length(p, 0.5 * (p3 + p0));
        }
        public FVElem(IFVElem fv)
        {
            Set(fv);
        }
        public void Set(IFVElem fv)
        {
            fv.GetVertex(ref p0, ref p1, ref p2, ref p3);
            this.p = new HPoint(fv.p);
            this.hs = fv.hs;
            this.he = fv.he;
            this.hn = fv.hn;
            this.hw = fv.hw;
            this.ds = fv.ds;
            this.de = fv.de;
            this.dn = fv.dn;
            this.dw = fv.dw;
            this.S = fv.S;
        }
        public void GetVertex(ref HPoint k0, ref HPoint k1, ref HPoint k2, ref HPoint k3)
        {
            k0 = new HPoint(p0);
            k1 = new HPoint(p1);
            k2 = new HPoint(p2);
            k3 = new HPoint(p3);
        }
    }
}
