using System.Collections.Generic;

namespace AlgebraLib
{
    /// <summary>
    /// ОО: Столбец упакованных индексов разреженной матрицы формате CRS
    /// </summary>
    public class SparseColIndex
    {
        /// <summary>
        /// Упакованный столбец индексов 
        /// </summary>
        public List<SparseElementIndex> Col;
        /// <summary>
        /// Размер упакованной столбеца
        /// </summary>
        public int Count { get => Col.Count; }
        public SparseColIndex(int N = 1)
        {
            Col = new List<SparseElementIndex>(N);
        }
        public SparseElementIndex this[int index]
        {
            get
            {
                return Col[(int)index];
            }
            set
            {
                Col[(int)index] = value;
            }
        }
        public void Clear()
        {
            Col.Clear();
        }
    }
}
