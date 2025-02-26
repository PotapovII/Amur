namespace TriangleNet.Topology
{
    using System;
    using TriangleNet.Geometry;
    /// <сводка>
    /// Ориентированный треугольник.
    /// </сводка>
    /// Включает указатель на треугольник и ориентацию.
    /// Ориентация обозначает грань треугольника.Таким образом, 
    /// возможны три направления. По соглашению каждое ребро 
    /// всегда направлено против часовой стрелки относительно 
    /// соответствующего треугольника.
    public struct Otri
    {
        /// <summary>
        /// треугольник
        /// </summary>
        internal Triangle tri;
        /// <summary>
        /// Индекс грани сопряжения для поиска сопряженных треугольников
        /// Нпример  tri.neighbors[orient].tri;
        /// </summary>
        internal int orient; // Ranges from 0 to 2.

        public Triangle Triangle
        {
            get { return tri; }
            set { tri = value; }
        }

        public override string ToString()
        {
            if (tri == null)
            {
                return "O-TID [null]";
            }
            return String.Format("O-TID {0}", tri.hash);
        }

        #region Otri primitives (public)

        // For fast access Для быстрого доступа
        static readonly int[] plus1Mod3 = { 1, 2, 0 };
        static readonly int[] minus1Mod3 = { 2, 0, 1 };

        // Все следующие примитивы описаны Гибасом и Столфи.
        // Однако Гуибас и Столфи используют структуру данных на основе ребер,
        // а я использую структуру данных на основе треугольника.
        //
        // lnnext: находит следующее ребро (против часовой стрелки) треугольника.
        //
        // onext: вращается против часовой стрелки вокруг вершины;
        // то есть он находит следующее ребро с тем же началом в направлении
        // против часовой стрелки. Это ребро является частью другого треугольника.
        //
        // oprev: вращение вокруг вершины по часовой стрелке; то есть он находит
        // следующее ребро с тем же началом в направлении по часовой стрелке.
        // Этот край часть другого треугольника.
        //
        // dnext: вращение вокруг вершины против часовой стрелки; то есть он
        // находит следующее ребро с тем же пунктом назначения в направлении
        // против часовой стрелки. Это ребро является частью другого треугольника.
        //
        // dprev: вращается вокруг вершины по часовой стрелке; то есть он находит
        // следующее ребро с тем же пунктом назначения в направлении по часовой
        // стрелке. Это ребро является частью другого треугольника.
        //
        // rnext: перемещает один край против часовой стрелки вокруг соседнего
        // треугольника. (Лучше всего это понять, прочитав Гибаса и Столфи.
        // Это предполагает двойную замену треугольников.)
        //
        // rprev: перемещает один край по часовой стрелке вокруг соседнего треугольника.
        // (Лучше всего это понять, прочитав Гибаса и Столфи.Это предполагает двойную
        // замену треугольников.)

        // The following primitives are all described by Guibas and Stolfi.
        // However, Guibas and Stolfi use an edge-based data structure,
        // whereas I use a triangle-based data structure.
        //
        // lnext: finds the next edge (counterclockwise) of a triangle.
        //
        // onext: spins counterclockwise around a vertex; that is, it finds 
        // the next edge with the same origin in the counterclockwise direction. This
        // edge is part of a different triangle.
        //
        // oprev: spins clockwise around a vertex; that is, it finds the 
        // next edge with the same origin in the clockwise direction.  This edge is 
        // part of a different triangle.
        //
        // dnext: spins counterclockwise around a vertex; that is, it finds 
        // the next edge with the same destination in the counterclockwise direction.
        // This edge is part of a different triangle.
        //
        // dprev: spins clockwise around a vertex; that is, it finds the 
        // next edge with the same destination in the clockwise direction. This edge 
        // is part of a different triangle.
        //
        // rnext: moves one edge counterclockwise about the adjacent 
        // triangle. (It's best understood by reading Guibas and Stolfi. It 
        // involves changing triangles twice.)
        //
        // rprev: moves one edge clockwise about the adjacent triangle.
        // (It's best understood by reading Guibas and Stolfi.  It involves
        // changing triangles twice.)

        /// <summary>
        /// Find the abutting triangle; same edge. [sym(abc) -> ba*]
        /// </summary>
        /// Note that the edge direction is necessarily reversed, because the handle specified 
        /// by an oriented triangle is directed counterclockwise around the triangle.
        /// </remarks>

        /// <сводка>
        /// Находим примыкающий треугольник; тот же край. [sym(abc) -> ba*]
        /// </сводка>
        /// Обратите внимание, что направление края обязательно меняется 
        /// на противоположное, поскольку маркер, заданный ориентированным
        /// треугольником, направлен вокруг треугольника против часовой стрелки.
        /// </примечания>
        public void Sym(ref Otri ot)
        {
            ot.tri = tri.neighbors[orient].tri;
            ot.orient = tri.neighbors[orient].orient;
        }

        /// <summary>
        /// Find the abutting triangle; same edge. [sym(abc) -> ba*]
        /// Найдите примыкающий треугольник; тот же край. [sym(abc) -> ba*]
        /// </summary>
        public void Sym()
        {
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the next edge (counterclockwise) of a triangle. [lnext(abc) -> bca]
        /// Найдите следующую сторону (против часовой стрелки) треугольника. [lnext(abc) -> bca]
        /// </summary>
        public void Lnext(ref Otri ot)
        {
            ot.tri = tri;
            ot.orient = plus1Mod3[orient];
        }

        /// <summary>
        /// Find the next edge (counterclockwise) of a triangle. [lnext(abc) -> bca]
        /// </summary>
        public void Lnext()
        {
            orient = plus1Mod3[orient];
        }

        /// <summary>
        /// Find the previous edge (clockwise) of a triangle. [lprev(abc) -> cab]
        /// </summary>
        public void Lprev(ref Otri ot)
        {
            ot.tri = tri;
            ot.orient = minus1Mod3[orient];
        }

        /// <summary>
        /// Find the previous edge (clockwise) of a triangle. [lprev(abc) -> cab]
        /// Найдите предыдущее ребро (по часовой стрелке) треугольника. [lprev(abc) -> cab]
        /// </summary>
        public void Lprev()
        {
            orient = minus1Mod3[orient];
        }

        /// <summary>
        /// Find the next edge counterclockwise with the same origin. [onext(abc) -> ac*]
        /// </summary>
        public void Onext(ref Otri ot)
        {
            //Lprev(ref ot);
            ot.tri = tri;
            ot.orient = minus1Mod3[orient];

            //ot.SymSelf();
            int tmp = ot.orient;
            ot.orient = ot.tri.neighbors[tmp].orient;
            ot.tri = ot.tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the next edge counterclockwise with the same origin. [onext(abc) -> ac*]
        /// </summary>
        public void Onext()
        {
            //LprevSelf();
            orient = minus1Mod3[orient];

            //SymSelf();
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the next edge clockwise with the same origin. [oprev(abc) -> a*b]
        /// </summary>
        public void Oprev(ref Otri ot)
        {
            //Sym(ref ot);
            ot.tri = tri.neighbors[orient].tri;
            ot.orient = tri.neighbors[orient].orient;

            //ot.LnextSelf();
            ot.orient = plus1Mod3[ot.orient];
        }

        /// <summary>
        /// Find the next edge clockwise with the same origin. [oprev(abc) -> a*b]
        /// </summary>
        public void Oprev()
        {
            //SymSelf();
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;

            //LnextSelf();
            orient = plus1Mod3[orient];
        }

        /// <summary>
        /// Find the next edge counterclockwise with the same destination. [dnext(abc) -> *ba]
        /// </summary>
        public void Dnext(ref Otri ot)
        {
            //Sym(ref ot);
            ot.tri = tri.neighbors[orient].tri;
            ot.orient = tri.neighbors[orient].orient;

            //ot.LprevSelf();
            ot.orient = minus1Mod3[ot.orient];
        }

        /// <summary>
        /// Find the next edge counterclockwise with the same destination. [dnext(abc) -> *ba]
        /// </summary>
        public void Dnext()
        {
            //SymSelf();
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;

            //LprevSelf();
            orient = minus1Mod3[orient];
        }

        /// <summary>
        /// Find the next edge clockwise with the same destination. [dprev(abc) -> cb*]
        /// </summary>
        public void Dprev(ref Otri ot)
        {
            //Lnext(ref ot);
            ot.tri = tri;
            ot.orient = plus1Mod3[orient];

            //ot.SymSelf();
            int tmp = ot.orient;
            ot.orient = ot.tri.neighbors[tmp].orient;
            ot.tri = ot.tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the next edge clockwise with the same destination. [dprev(abc) -> cb*]
        /// </summary>
        public void Dprev()
        {
            //LnextSelf();
            orient = plus1Mod3[orient];

            //SymSelf();
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the next edge (counterclockwise) of the adjacent triangle. [rnext(abc) -> *a*]
        /// </summary>
        public void Rnext(ref Otri ot)
        {
            //Sym(ref ot);
            ot.tri = tri.neighbors[orient].tri;
            ot.orient = tri.neighbors[orient].orient;

            //ot.LnextSelf();
            ot.orient = plus1Mod3[ot.orient];

            //ot.SymSelf();
            int tmp = ot.orient;
            ot.orient = ot.tri.neighbors[tmp].orient;
            ot.tri = ot.tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the next edge (counterclockwise) of the adjacent triangle. [rnext(abc) -> *a*]
        /// </summary>
        public void Rnext()
        {
            //SymSelf();
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;

            //LnextSelf();
            orient = plus1Mod3[orient];

            //SymSelf();
            tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the previous edge (clockwise) of the adjacent triangle. [rprev(abc) -> b**]
        /// </summary>
        public void Rprev(ref Otri ot)
        {
            //Sym(ref ot);
            ot.tri = tri.neighbors[orient].tri;
            ot.orient = tri.neighbors[orient].orient;

            //ot.LprevSelf();
            ot.orient = minus1Mod3[ot.orient];

            //ot.SymSelf();
            int tmp = ot.orient;
            ot.orient = ot.tri.neighbors[tmp].orient;
            ot.tri = ot.tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Find the previous edge (clockwise) of the adjacent triangle. [rprev(abc) -> b**]
        /// </summary>
        public void Rprev()
        {
            //SymSelf();
            int tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;

            //LprevSelf();
            orient = minus1Mod3[orient];

            //SymSelf();
            tmp = orient;
            orient = tri.neighbors[tmp].orient;
            tri = tri.neighbors[tmp].tri;
        }

        /// <summary>
        /// Origin [org(abc) -> a]
        /// </summary>
        public Vertex Org()
        {
            return tri.vertices[plus1Mod3[orient]];
        }

        /// <summary>
        /// Destination [dest(abc) -> b]  Пункт назначения [пункт назначения(abc) -> b]
        /// </summary>
        public Vertex Dest()
        {
            return tri.vertices[minus1Mod3[orient]];
        }

        /// <summary>
        /// Apex [apex(abc) -> c]
        /// </summary>
        public Vertex Apex()
        {
            return tri.vertices[orient];
        }

        /// <summary>
        /// Copy an oriented triangle.
        /// </summary>
        public void Copy(ref Otri ot)
        {
            ot.tri = tri;
            ot.orient = orient;
        }

        /// <summary>
        /// Test for equality of oriented triangles.
        /// </summary>
        public bool Equals(Otri ot)
        {
            return ((tri == ot.tri) && (orient == ot.orient));
        }

        #endregion

        #region Otri primitives (internal)

        /// <summary>
        /// Set Origin
        /// </summary>
        internal void SetOrg(Vertex v)
        {
            tri.vertices[plus1Mod3[orient]] = v;
        }

        /// <summary>
        /// Set Destination
        /// </summary>
        internal void SetDest(Vertex v)
        {
            tri.vertices[minus1Mod3[orient]] = v;
        }

        /// <summary>
        /// Set Apex
        /// </summary>
        internal void SetApex(Vertex v)
        {
            tri.vertices[orient] = v;
        }

        /// <summary>
        /// Bond two triangles together at the resepective handles. [bond(abc, bad)]
        /// </summary>
        internal void Bond(ref Otri ot)
        {
            tri.neighbors[orient].tri = ot.tri;
            tri.neighbors[orient].orient = ot.orient;

            ot.tri.neighbors[ot.orient].tri = this.tri;
            ot.tri.neighbors[ot.orient].orient = this.orient;
        }

        /// <сводка>
        /// Односторонний разрыв связи с треугольником.
        /// </сводка>
        /// <remarks> Обратите внимание, что другой треугольник все равно будет считать, 
        /// что он соединен с этим треугольником. Однако обычно другой треугольник 
        /// полностью удаляется или привязывается к другому треугольнику, поэтому это не имеет значения.
        /// </примечания>
        internal void Dissolve(Triangle dummy)
        {
            tri.neighbors[orient].tri = dummy;
            tri.neighbors[orient].orient = 0;
        }

        /// <summary>
        /// Заразить треугольник.
        /// </summary>
        internal void Infect()
        {
            tri.infected = true;
        }

        /// <summary>
        ///Вылечить треугольник от вируса.
        /// </summary>
        internal void Uninfect()
        {
            tri.infected = false;
        }

        /// <summary>
        /// Проверьте треугольник на вирусную инфекцию.
        /// </summary>
        internal bool IsInfected()
        {
            return tri.infected;
        }

        /// <summary>
        /// Находит подотрезок, примыкающий к треугольнику.
        /// </summary>
        internal void Pivot(ref Osub os)
        {
            os = tri.subsegs[orient];
        }

        /// <summary>
        /// Прикрепите треугольник к подсегменту.
        /// </summary>
        internal void SegBond(ref Osub os)
        {
            tri.subsegs[orient] = os;
            os.seg.triangles[os.orient] = this;
        }

        /// <summary>
        /// Dissolve a bond (from the triangle side).
        /// </summary>
        internal void SegDissolve(SubSegment dummy)
        {
            tri.subsegs[orient].seg = dummy;
        }

        /// <summary>
        /// Check a triangle's deallocation.
        /// </summary>
        internal static bool IsDead(Triangle tria)
        {
            return tria.neighbors[0].tri == null;
        }

        /// <summary>
        /// Set a triangle's deallocation.
        /// </summary>
        internal static void Kill(Triangle tri)
        {
            tri.neighbors[0].tri = null;
            tri.neighbors[2].tri = null;
        }

        #endregion
    }
}
