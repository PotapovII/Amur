namespace TriangleNet.Tools
{
    using System;

    /// <сводка>
    /// Матрица смежности для сетки
    /// </сводка>
    public class AdjacencyMatrix
    {
        /// <summary>
        /// Количество записей смежности.
        /// </summary>
        int nnz;
        /// <summary>
        /// Указатели на фактическую структуру смежности прил. 
        /// Информация о строке k хранится в записях от pcol(k) 
        /// до pcol(k+1)-1 в adj. Размер: N + 1
        /// </summary>
        int[] pcol;

        /// <summary>
        /// Структура смежности. Для каждой строки он содержит 
        /// индексы столбцов ненулевых записей. Размер: nnz
        /// </summary>
        int[] irow;

        /// <summary>
        /// Получает количество столбцов (узлов сетки).
        /// </summary>
        public readonly int N;

        /// <summary>
        /// Получает указатели столбцов.
        /// </summary>
        public int[] ColumnPointers
        {
            get { return pcol; }
        }

        /// <summary>
        /// Получает индексы строк.
        /// </summary>
        public int[] RowIndices
        {
            get { return irow; }
        }

        public AdjacencyMatrix(MeshNet mesh)
        {
            this.N = mesh.vertices.Count;

            // Настройте массив указателей смежности adj_row.
            this.pcol = AdjacencyCount(mesh);
            this.nnz = pcol[N];

            // Set up the adj adjacency array.
            this.irow = AdjacencySet(mesh, this.pcol);

            SortIndices();
        }

        public AdjacencyMatrix(int[] pcol, int[] irow)
        {
            this.N = pcol.Length - 1;

            this.nnz = pcol[N];

            this.pcol = pcol;
            this.irow = irow;

            if (pcol[0] != 0)
            {
                throw new ArgumentException("Expected 0-based indexing.", "pcol");
            }

            if (irow.Length < nnz)
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Вычисляет пропускную способность матрицы смежности.
        /// </summary>
        /// <returns>Пропускная способность матрицы смежности.</returns>
        public int Bandwidth()
        {
            int band_hi;
            int band_lo;
            int col;
            int i, j;

            band_lo = 0;
            band_hi = 0;

            for (i = 0; i < N; i++)
            {
                for (j = pcol[i]; j < pcol[i + 1]; j++)
                {
                    col = irow[j];
                    band_lo = Math.Max(band_lo, i - col);
                    band_hi = Math.Max(band_hi, col - i);
                }
            }

            return band_lo + 1 + band_hi;
        }

        #region Adjacency matrix


        /// <summary>
        /// Подсчитывает смежности в триангуляции.
        /// --------------------------------------------------------------
        /// Эта процедура вызывается для подсчета смежностей, так что
        /// соответствующий объем памяти может быть выделен для 
        /// хранения при создании структуры смежности.
        /// Предполагается, что в триангуляции участвуют трехузловые треугольники. 
        /// Два узла являются «смежными», если они оба являются узлами 
        /// некоторого треугольника.Кроме того, узел считается смежным сам с собой.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        int[] AdjacencyCount(MeshNet mesh)
        {
            int n = N;
            int n1, n2, n3;
            int tid, nid;

            int[] pcol = new int[n + 1];

            // Установите каждый узел смежным с самим собой.
            for (int i = 0; i < n; i++)
            {
                pcol[i] = 1;
            }

            // Рассмотрите каждый треугольник.
            foreach (var tri in mesh.triangles)
            {
                tid = tri.id;

                n1 = tri.vertices[0].id;
                n2 = tri.vertices[1].id;
                n3 = tri.vertices[2].id;

                // Add edge (1,2) if this is the first occurrence, that is, if 
                // the edge (1,2) is on a boundary (nid <= 0) or if this triangle
                // is the first of the pair in which the edge occurs (tid < nid).
                nid = tri.neighbors[2].tri.id;

                if (nid < 0 || tid < nid)
                {
                    pcol[n1] += 1;
                    pcol[n2] += 1;
                }

                // Add edge (2,3).
                nid = tri.neighbors[0].tri.id;

                if (nid < 0 || tid < nid)
                {
                    pcol[n2] += 1;
                    pcol[n3] += 1;
                }

                // Add edge (3,1).
                nid = tri.neighbors[1].tri.id;

                if (nid < 0 || tid < nid)
                {
                    pcol[n3] += 1;
                    pcol[n1] += 1;
                }
            }

            // We used PCOL to count the number of entries in each column.
            // Convert it to pointers into the ADJ array.
            for (int i = n; i > 0; i--)
            {
                pcol[i] = pcol[i - 1];
            }

            pcol[0] = 0;
            for (int i = 1; i <= n; i++)
            {
                pcol[i] = pcol[i - 1] + pcol[i];
            }

            return pcol;
        }

        /// <summary>
        /// Эту процедуру можно использовать для создания сжатого хранилища столбцов 
        /// для дискретизации конечных элементов линейного 
        /// треугольника уравнения Пуассона в двух измерениях.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="pcol"></param>
        /// <returns></returns>
        int[] AdjacencySet(MeshNet mesh, int[] pcol)
        {
            int n = this.N;

            int[] col = new int[n];

            // Копия ввода строк смежности.
            Array.Copy(pcol, col, n);

            int i, nnz = pcol[n];

            // Выходной список хранит фактическую информацию о смежности.
            int[] list = new int[nnz];

            // Установите каждый узел смежным с самим собой.
            for (i = 0; i < n; i++)
            {
                list[col[i]] = i;
                col[i] += 1;
            }
            // Номера вершин
            int n1, n2, n3;
            // Треугольник и идентификатор соседа.
            int tid, nid; 
            // Цикл по треугольникам
            foreach (var tri in mesh.triangles)
            {
                tid = tri.id;

                n1 = tri.vertices[0].id;
                n2 = tri.vertices[1].id;
                n3 = tri.vertices[2].id;

                // Add edge (1,2) if this is the first occurrence, that is, if 
                // the edge (1,2) is on a boundary (nid <= 0) or if this triangle
                // is the first of the pair in which the edge occurs (tid < nid).
                nid = tri.neighbors[2].tri.id;

                if (nid < 0 || tid < nid)
                {
                    list[col[n1]++] = n2;
                    list[col[n2]++] = n1;
                }

                // Add edge (2,3).
                nid = tri.neighbors[0].tri.id;

                if (nid < 0 || tid < nid)
                {
                    list[col[n2]++] = n3;
                    list[col[n3]++] = n2;
                }

                // Add edge (3,1).
                nid = tri.neighbors[1].tri.id;

                if (nid < 0 || tid < nid)
                {
                    list[col[n1]++] = n3;
                    list[col[n3]++] = n1;
                }
            }

            return list;
        }
        /// <summary>
        /// сортировка по возрастанию
        /// </summary>
        public void SortIndices()
        {
            int k1, k2, n = N;

            int[] list = this.irow;

            // По возрастанию сортируйте записи для каждого столбца.
            for (int i = 0; i < n; i++)
            {
                k1 = pcol[i];
                k2 = pcol[i + 1];
                Array.Sort(list, k1, k2 - k1);
            }
        }

        #endregion
    }
}
