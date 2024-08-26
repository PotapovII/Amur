// -----------------------------------------------------------------------
// <copyright file="TrianglePool.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet
{
    using System;
    using System.Collections.Generic;
    using TriangleNet.Geometry;
    using TriangleNet.Topology;

    public class TrianglePool : ICollection<Triangle>
    {
        // Определяет размер каждого блока в пуле.
        private const int BLOCKSIZE = 1024;

        // Общее количество выделенных на данный момент треугольников.
        int size;

        // Количество треугольников, используемых в данный момент.
        int count;

        // The pool.
        Triangle[][] pool;

        // Стек свободных треугольников.
        Stack<Triangle> stack;

        public TrianglePool()
        {
            size = 0;

            // При запуске пул должен содержать 2^16 треугольников.
            int n = Math.Max(1, 65536 / BLOCKSIZE);

            pool = new Triangle[n][];
            pool[0] = new Triangle[BLOCKSIZE];

            stack = new Stack<Triangle>(BLOCKSIZE);
        }

        /// <summary>
        /// Достает треугольник из пула.
        /// </summary>
        /// <returns></returns>
        public Triangle Get()
        {
            Triangle triangle;

            if (stack.Count > 0)
            {
                triangle = stack.Pop();
                triangle.hash = -triangle.hash - 1;

                Cleanup(triangle);
            }
            else if (count < size)
            {
                triangle = pool[count / BLOCKSIZE][count % BLOCKSIZE];
                triangle.id = triangle.hash;

                Cleanup(triangle);

                count++;
            }
            else
            {
                triangle = new Triangle();
                triangle.hash = size;
                triangle.id = triangle.hash;

                int block = size / BLOCKSIZE;

                if (pool[block] == null)
                {
                    pool[block] = new Triangle[BLOCKSIZE];

                    // Проверяем, нужно ли изменить размер пула.
                    if (block + 1 == pool.Length)
                    {
                        Array.Resize(ref pool, 2 * pool.Length);
                    }
                }

                // Добавляем треугольник в пул.
                pool[block][size % BLOCKSIZE] = triangle;

                count = ++size;
            }

            return triangle;
        }

        public void Release(Triangle triangle)
        {
            stack.Push(triangle);

            // Отмечаем треугольник как свободный (используется перечислителем).
            triangle.hash = -triangle.hash - 1;
        }

        /// <summary>
        /// Перезапустите пул треугольников
        /// </summary>
        public TrianglePool Restart()
        {
            foreach (var triangle in stack)
            {
                // Reset hash to original value.
                triangle.hash = -triangle.hash - 1;
            }

            stack.Clear();

            count = 0;

            return this;
        }

        /// <summary>
        /// Samples a number of triangles from the pool.
        /// </summary>
        /// <param name="k">The number of triangles to sample.</param>
        /// <param name="random"></param>
        /// <returns></returns>
        internal IEnumerable<Triangle> Sample(int k, Random random)
        {
            int i, count = this.Count;

            if (k > count)
            {
                // TODO: handle Sample special case.
                k = count;
            }

            Triangle t;

            // TODO: improve sampling code (to ensure no duplicates).

            while (k > 0)
            {
                i = random.Next(0, count);

                t = pool[i / BLOCKSIZE][i % BLOCKSIZE];

                if (t.hash >= 0)
                {
                    k--;
                    yield return t;
                }
            }
        }

        private void Cleanup(Triangle triangle)
        {
            triangle.label = 0;
            triangle.area = 0.0;
            triangle.infected = false;

            for (int i = 0; i < 3; i++)
            {
                triangle.vertices[i] = null;

                triangle.subsegs[i] = default(Osub);
                triangle.neighbors[i] = default(Otri);
            }
        }

        public void Add(Triangle item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            stack.Clear();

            int blocks = (size / BLOCKSIZE) + 1;

            for (int i = 0; i < blocks; i++)
            {
                var block = pool[i];

                // Number of triangles in current block:
                int length = (size - i * BLOCKSIZE) % BLOCKSIZE;

                for (int j = 0; j < length; j++)
                {
                    block[j] = null;
                }
            }

            size = count = 0;
        }

        public bool Contains(Triangle item)
        {
            int i = item.hash;

            if (i < 0 || i > size)
            {
                return false;
            }

            return pool[i / BLOCKSIZE][i % BLOCKSIZE].hash >= 0;
        }

        public void CopyTo(Triangle[] array, int index)
        {
            var enumerator = GetEnumerator();

            while (enumerator.MoveNext())
            {
                array[index] = enumerator.Current;
                index++;
            }
        }

        public int Count
        {
            get { return count - stack.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(Triangle item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Triangle> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class Enumerator : IEnumerator<Triangle>
        {
            // TODO: enumerator should be able to tell if collection changed.

            int count;

            Triangle[][] pool;

            Triangle current;

            int index, offset;

            public Enumerator(TrianglePool pool)
            {
                this.count = pool.Count;
                this.pool = pool.pool;

                index = 0;
                offset = 0;
            }

            public Triangle Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }

            public bool MoveNext()
            {
                while (index < count)
                {
                    current = pool[offset / BLOCKSIZE][offset % BLOCKSIZE];

                    offset++;

                    if (current.hash >= 0)
                    {
                        index++;
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                index = offset = 0;
            }
        }
    }
}
