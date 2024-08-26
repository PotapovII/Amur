namespace MeshLib.ATable
{
    using MemLogLib;
    using GeometryLib;
    /// <summary>
    ///  Маски неизвестных в узле
    ///  !!! Узлы могут не совпадать с узлами базисной КЭ сетки 
    ///  используемой для описания геометрии области, поэтому в нагркзку
    ///  к вестору неизвестных в узле храним и его координаты
    /// </summary>
    public class TablKnot : HKnot, ITablKnot
    {
        /// <summary>
        /// Вектор наличия неизвестной в узле 0 или 1
        /// </summary>
        public uint[] Mask { get; }
        uint[] mask = null;
        public TablKnot(HKnot p, uint[] m) : base(p)
        {
            MEM.MemCopy<uint>(ref mask, m);
        }
        public TablKnot(TablKnot p) : base(p)
        {
            MEM.MemCopy<uint>(ref mask, p.mask);
        }
    }
}
