//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 24.12.2023 Потапов И.И.
//---------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonLib.Geometry;
using CommonLib.Areas;

namespace GeometryLib.Areas
{
    /// <summary>
    /// ОО: IMHesgGrid предназначенн для хеширования 
    /// частиц и нахождения ближайших соседей
    /// </summary>
    public class MHesgGrid : IMHesgGrid
    {
        /// <summary>
        /// Хешируемые частицы
        /// </summary>
        List<IHPoint> particles;
        /// <summary>
        /// Ближайшие соседи в ячейках
        /// система координат
        /// ---------> y
        /// |
        /// |
        /// V x
        /// </summary>
        List<IHPoint>[,] grid;
        /// <summary>
        /// Размер области хеширования
        /// </summary>
        RectangleWorld rectWorld;
        /// <summary>
        /// Число ячеек по Х
        /// </summary>
        int countX;
        /// <summary>
        /// Число ячеек по Y
        /// </summary>
        int countY;
        /// <summary>
        /// Хешируемые частицы
        /// </summary>
        public List<IHPoint> Particles => particles;
        /// <summary>
        /// Ближайшие соседи в ячейках
        /// система координат
        /// ---------> y
        /// |
        /// |
        /// V x
        /// </summary>
        public List<IHPoint>[,] Grid => grid;
        /// <summary>
        /// Размер области хеширования
        /// </summary>
        public RectangleWorld RectWorld => rectWorld;
        /// <summary>
        /// Размер ячейки сетки
        /// </summary>
        public float HeshSizeСellX;
        /// <summary>
        /// Размер ячейки сетки
        /// </summary>
        public float HeshSizeСellY;
        /// <summary>
        /// Число ячеек по Х
        /// </summary>
        public int CountX => countX;
        /// <summary>
        /// Число ячеек по Y
        /// </summary>
        public int CountY => countY;
        /// <summary>
        /// Чистим хеш таблицу области
        /// </summary>
        public void ClearGrid()
        {
            Parallel.For(0, grid.GetLength(0), xx =>
            {
                for (int yy = 0; yy < grid.GetLength(1); yy++)
                    grid[xx, yy].Clear();
            });
        }
        public MHesgGrid(List<IHPoint> particles, int countX, int countY)
        {
            List<IHPoint> p = new List<IHPoint>();
            p.AddRange(particles);
            Set(p, countX, countY);
        }

        public MHesgGrid(IHPoint[] particles, int countX, int countY)
        {
            List<IHPoint> p = new List<IHPoint>();
            p.AddRange(particles);
            Set(p, countX, countY);
        }

        protected void Set(List<IHPoint> particles, int countX, int countY)
        {
            this.particles = particles;
            this.countX = countX;
            this.countY = countY;
            grid = new List<IHPoint>[countX, countY];
            rectWorld = new RectangleWorld();
            foreach (var p in particles)
                rectWorld.Extension(p);
            HeshSizeСellX = (rectWorld.Width / (countX - 1));
            HeshSizeСellY = (rectWorld.Height / (countY - 1));
        }

        /// <summary>
        /// Пересоздание сетки хеширования
        /// </summary>
        public void ReBuildHeshGrid(List<IHPoint> particles, int countX, int countY)
        {
            Set(particles, countX, countY);
            BuildHeshGrid();
        }

        protected void GetHesh(IHPoint pi, out int ix, out int iy)
        {
            ix = (int)((pi.X - rectWorld.Left) / HeshSizeСellX);
            iy = (int)((pi.Y - rectWorld.Bottom) / HeshSizeСellY);
            ix = ix < 0 ? 0 : ix;
            iy = iy < 0 ? 0 : iy;
            ix = ix > countX - 1 ? countX - 1 : ix;
            iy = iy > countY - 1 ? countY - 1 : iy;
        }


        /// <summary>
        /// Создание сетки хеширования
        /// </summary>
        public void BuildHeshGrid()
        {
            // Очистка хеша
            ClearGrid();
            // Постройка хеша 
            //Parallel.For(0, particles.Count, i =>
            //{
            //    IHPoint pi = Particles[i];
            //    int ix, iy;
            //    GetHesh(pi, out ix, out iy);
            //    grid[ix, iy].Add(pi); // ???
            //});

            // Постройка хеша 
            for (int i = 0; i < particles.Count; i++)
            {
                IHPoint pi = Particles[i];
                int ix, iy;
                GetHesh(pi, out ix, out iy);
                grid[ix, iy].Add(pi);
            }
        }
        /// <summary>
        /// Найти ближайших соседей
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual List<IHPoint> FindNeighboringParticles(IHPoint point,int range = 1)
        {
            List<IHPoint> alist = new List<IHPoint>();
            IHPoint pi = point;
            int ix, iy;
            GetHesh(pi, out ix, out iy);
            if(range == 0 && grid[ix, iy].Count > 0)
            {
                alist.AddRange(grid[ix, iy]);
                return alist;
            }
            else
            {
                int bondx = 3;
                int bondy = 3;
                int stx = 1;
                int sty = 1;
                if (iy == 0) sty = 0;
                if (ix == 0) stx = 0;
                if ((iy == countY - 1) || (iy == 0)) bondy = 2;
                if ((ix == countX - 1) || (ix == 0)) bondx = 2;
                for (int x = 0; x < bondx; x++)
                    for (int y = 0; y < bondy; y++)
                        alist.AddRange(grid[ix - stx + x, iy - sty + y]);
                return alist;
            }
        }
    }
}
