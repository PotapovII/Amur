//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//       исходный код решателя создан П.С.Тимош, аспирант, 2021-2022
//          (Вычислительный центр Дальневосточного отделения РАН)
//---------------------------------------------------------------------------
//               кодировка адаптации : 15.10.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using System.Threading.Tasks;
    using MemLogLib;
    using CommonLib;
    using CommonLib.IO;
    using MeshLib;
    using RiverLib.IO;
    using GeometryLib.Vector;
    /// <summary>
    /// Флаг ГУ задачи
    /// </summary>
    [Flags]
    [Serializable]
    public enum NodeType
    {
        None = 0b_0000_0000,
        Bordary = 0b_0000_0001,
        Input = 0b_0000_0010,
        Output = 0b_0000_0100,
        InputBordary = Input | Bordary,
        OutputBordary = Output | Bordary,
        Wet = 0b_0001_0000,
        Dry = 0b_0010_0000,
        Solve = 0b_1000_0000,
        SolveWet = Solve | Wet,
        SolveDry = Solve | Dry,
        BordaryWet = Bordary | Wet,
        BordaryDry = Bordary | Dry,
    }
    /// <summary>
    /// Тип функций для граничных условий задачи
    /// </summary>
    public delegate void BorderCondition();
    /// <summary>
    /// Тип функций для начальных условий задачи
    /// </summary>
    public delegate void InitialCondition();
    /// <summary>
    ///  ОО: Определение класса RiverSW_FCT - для расчета полей расходов, 
    ///  скорости, глубин и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class RiverSW_FCT : RiverSW_FCTParams, IRiver
    {
        #region поля задачи
        /// <summary>
        /// Симплекс сетка области для отрисовки поляй задачи
        /// </summary>
        RectangleMesh mesh = null;
        /// <summary>
        /// Метод установки НУ задачи
        /// </summary>
        public InitialCondition initialCondition { get; set; } = null;
        /// <summary>
        /// Метод установки ГУ задачи
        /// </summary>
        public BorderCondition borderCondition { get; set; } = null;
        /// <summary>
        /// Скорость по оси X
        /// </summary>
        protected double[,] tau_x;
        /// <summary>
        /// Скорость по оси X
        /// </summary>
        protected double[,] tau_y;
        /// <summary>
        /// Скорость по оси X
        /// </summary>
        protected double[,] u;
        /// <summary>
        /// Скорость по оси Y
        /// </summary>
        protected double[,] v;
        /// <summary>
        /// Глубина водного канала
        /// </summary>
        protected double[,] h;
        /// <summary>
        /// Уровень дна канала
        /// </summary>
        protected double[,] zeta;
        /// <summary>
        /// расход по х
        /// </summary>
        protected double[,] qx;
        /// <summary>
        /// расход по у
        /// </summary>
        protected double[,] qy;
        /// <summary>
		/// Область решения
		/// </summary>
        protected bool[,] regionSolver;
        /// <summary>
		/// Область входной границы
		/// </summary>
        protected bool[,] inputBoundary;
        /// <summary>
        /// Область выходной границы
        /// </summary>
        protected bool[,] outputBoundary;
        /// <summary>
        /// Тип узла
        /// </summary>
        protected NodeType[,] map;
        /// <summary>
        /// Вектор потоков
        /// </summary>
        protected Vector3[,] Q;
        /// <summary>
        /// Дельта Q для FCT
        /// </summary>
        protected Vector3[,] DxQ, DyQ;
        /// <summary>
        /// дивергентный вектор
        /// </summary>
        protected Vector3[,] F, G;
        /// <summary>
        /// Диффузия решения
        /// </summary>
        protected Vector3[,] fD, gD;
        /// <summary>
        /// Антидиффузия решения
        /// </summary>
        protected Vector3[,] fAD, gAD;
        /// <summary>
        /// Параметр расхода воды
        /// </summary>
        protected const double Qh = 1.01f;
        /// <summary>
        /// Параметры сглаживания разрыва в полях задачи
        /// </summary>
        private const double ETA = 1.0f / 6;
        private const double ET1 = 1.0f / 3;
        private const double ET2 = -1.0f / 6;
        /// <summary>
        /// шаг сетки по х и у
        /// </summary>
        protected double dx, dy;
        #endregion 

        #region IRiver
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public string Name { get => "Мелкая вода CD FCT"; }
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        public TypeTask typeTask { get => TypeTask.streamXY2D; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public string versionData { get => "River2D 15.10.2022"; }
        /// <summary>
        /// граничные условия
        /// </summary>
        protected IBoundaryConditions BoundCondition1D = null;
        /// <summary>
        /// Получить граничные условия для задачи донных деформаций
        /// </summary>
        /// <returns></returns>
        public IBoundaryConditions BoundCondition { get => BoundCondition1D; set => BoundCondition1D = value; }
        /// <summary>
        /// Имена файлов с данными для задачи гидродинамики при загрузке
        /// по умолчанию
        /// </summary>
        public ITaskFileNames taskFileNemes()
        {
            ITaskFileNames fn = new TaskFileNames();
            fn.NameCPParams = "NameCPParams.txt";
            fn.NameBLParams = "NameBLParams.txt";
            fn.NameRSParams = "NameRSParams.txt";
            fn.NameRData = "NameRData.txt";
            fn.NameEXT = "(*.tsk)|*.tsk|";
            fn.NameEXTImport = "(*.txt)|*.txt|";
            return fn;
        }
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public IMesh BedMesh()
        {
            return null;
        }
        /// <summary>
        /// КЭ сетка
        /// </summary>
        public IMesh Mesh => mesh.Clone();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        public IUnknown[] Unknowns => unknowns;
        protected IUnknown[] unknowns =
        {
            new SUnknown("Удельный расход потока по х", true, TypeFunForm.Form_2D_Triangle_L1),
            new SUnknown("Удельный расход потока по у", true, TypeFunForm.Form_2D_Triangle_L1),
            new SUnknown("Осредненная по глубине скорость х", true, TypeFunForm.Form_2D_Triangle_L1),
            new SUnknown("Осредненная по глубине скорость у", true, TypeFunForm.Form_2D_Triangle_L1),
            new SUnknown("Глубина потока", true, TypeFunForm.Form_2D_Triangle_L1)
        };
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep()
        {
            //      private const double ETA = 1.0f / 6;
            //      private const double ET1 = 1.0f / 3;
            //      private const double ET2 = -1.0f / 6;

            for (uint j = 1; j < Ny - 1; ++j)
            {
                var alpha = dtime * (u[0, j] + u[1, j]) / dx;
                var nu = ETA + ET1 * alpha * alpha;
                fD[0, j] = nu * (Q[1, j] - Q[0, j]);
            }
            for (uint i = 0; i < Nx-1; i++)
            {
                var epsilon = dtime * ((v[i, 0] + v[i, 1]) / dy);
                var lambda = ETA + ET1 * epsilon * epsilon;
                gD[i, 0] = lambda * (Q[i, 1] - Q[i, 0]);
            }
            // Обход внутренних узлов области
            for (uint i = 1; i < Nx - 1; i++)
            {
                Vector3 R = new Vector3();
                for (uint j = 1; j < Ny - 1; ++j)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        var alpha = dtime * (u[i, j] + u[i + 1, j]) / dx;
                        var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
                        var nu = ETA + ET1 * alpha * alpha;
                        var lambda = ETA + ET1 * epsilon * epsilon;
                        fD[i, j] = nu * (Q[i + 1, j] - Q[i, j]);
                        gD[i, j] = lambda * (Q[i, j + 1] - Q[i, j]);

                        var Fr = F[i + 1, j];
                        var Fl = F[i - 1, j];
                        var Gr = G[i, j + 1];
                        var Gl = G[i, j - 1];

                        var Fx = 0.5 * (Fr - Fl) / dx;
                        var Gy = 0.5 * (Gr - Gl) / dy;

                        if (map[i, j].HasFlag(NodeType.Bordary) == true)
                        {
                            if (h[i + 1, j] < MEM.Error2)
                            {
                                //var dz = zeta[i + 1, j] - zeta[i, j];
                                //var dx1 = h[i, j] * dx / dz;
                                Fr = F[i, j];
                                Fx = (Fr - Fl) / dx;
                            }
                            else if (h[i - 1, j] < MEM.Error2)
                            {
                                //var dz = zeta[i + 1, j] - zeta[i, j];
                                //var dx1 = h[i, j] * dx / dz;

                                Fl = F[i, j];
                                Fx = (Fr - Fl) / dx;
                            }
                            if (h[i, j + 1] < MEM.Error2)
                            {
                                //var dz = zeta[i, j + 1] - zeta[i, j];
                                //var dy1 = h[i, j] * dy / dz;

                                Gr = G[i, j];
                                Gy = (Gr - Gl) / dy;
                            }
                            else if (h[i, j - 1] < MEM.Error2)
                            {
                                //var dz = zeta[i, j - 1] - zeta[i, j];
                                //var dy1 = h[i, j] * dy / dz;

                                Gl = G[i, j];
                                Gy = (Gr - Gl) / dy;
                            }
                        }

                        // Схема центральных разностей
                        Q[i, j] = Q[i, j] - dtime * (Fx + Gy);

                        if (h[i, j] > MEM.Error2)
                        {
                            var Cs = (double)Math.Pow(h[i, j], 1f / 6f) / roughness;
                            var Lambda = Physics.g / Cs / Cs;
                            var sqrtU2V2 = (double)Math.Sqrt(u[i, j] * u[i, j] + v[i, j] * v[i, j]);
                            tau_x[i, j] = Lambda * sqrtU2V2 * u[i, j];
                            tau_y[i, j] = Lambda * sqrtU2V2 * v[i, j];
                        }
                        else
                        {
                            tau_x[i, j] = 0;
                            tau_y[i, j] = 0;
                        }
                        R.Y = tau_x[i, j];
                        R.Z = tau_y[i, j];
                        // Учет трения дна
                        Q[i, j] -= dtime * R;
                    }
                }
            }
            // по пространству
            Print(Q, "Q 2");
            #region  FCT коррекция решения
            for (int j = 0; j < Ny; j++)
            {
                var alpha = dtime * (u[0, j] + u[1, j]) / dx;
                var mu = ETA + 0.25f * ET2 * alpha * alpha;
                fAD[0, j] = mu * (Q[1, j] - Q[0, j]);
            }
            
            for (int i = 0; i < Nx; i++)
            {
                var epsilon = dtime * (v[i, 0] + v[i, 1]) / dy;
                var kappa = ETA + 0.25f * ET2 * epsilon * epsilon;
                gAD[i, 0] = kappa * (Q[i, 1] - Q[i, 0]);
            }

            // расчет антидифузионного потока
            for (int i = 1; i < Nx - 1; i++)
            {
                for (int j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        var alpha   = dtime * (u[i, j] + u[i + 1, j]) / dx;
                        var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
                        var mu      = ETA + ET2 * alpha * alpha;
                        var kappa   = ETA + ET2 * epsilon * epsilon;

                        fAD[i, j] = mu * (Q[i + 1, j] - Q[i, j]);
                        gAD[i, j] = kappa * (Q[i, j + 1] - Q[i, j]);
                        
                        Q[i, j] = Q[i, j] + fD[i, j] - fD[i - 1, j] + gD[i, j] - gD[i, j - 1];
                        DxQ[i - 1, j] = Q[i, j] - Q[i - 1, j];
                        DyQ[i, j - 1] = Q[i, j] - Q[i, j - 1];
                    }
                }
            }
            for (int i = 0; i < Nx; i++)
                DyQ[i, Ny - 2] = Q[i, Ny - 1] - Q[i, Ny - 2];
            for (int j = 0; j < Ny; j++)
                DxQ[Nx - 2, j] = Q[Nx - 1, j] - Q[Nx - 2, j];
            // Ограничение антидиффузионных членов
            for (int i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        fAD[i, j] = limiter(fAD[i, j], DxQ[i - 1, j], DxQ[i + 1, j]);
                        gAD[i, j] = limiter(gAD[i, j], DyQ[i, j - 1], DyQ[i, j + 1]);
                        // учет антидиффузии в решении
                        Q[i, j] = Q[i, j] - (fAD[i, j] - fAD[i - 1, j] + gAD[i, j] - gAD[i, j - 1]);
                    }
                }
            }
            #endregion

            Print(Q, "Q 3");
            borderCondition();

            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        h[i, j] = Q[i, j].X;
                        if (h[i, j] > MEM.Error2)
                        {
                            u[i, j] = Q[i, j].Y / Q[i, j].X;
                            v[i, j] = Q[i, j].Z / Q[i, j].X;
                            qx[i, j] = Q[i, j].Y;
                            qy[i, j] = Q[i, j].Z;
                        }
                        else
                        {
                            u[i, j] = 0;
                            v[i, j] = 0;
                            qx[i, j] = 0;
                            qy[i, j] = 0;
                            Q[i, j].X = 0;
                            h[i, j] = 0;
                        }
                    }
                }
            }
            // Обновление дивергентных векторов
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        h[i, j] = Q[i, j].X;

                        F[i, j].X = qx[i, j];
                        //F[i, j].Y = qx[i, j] * qx[i, j] / h[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                        F[i, j].Y = qx[i, j] * u[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                        F[i, j].Z = qx[i, j] * v[i, j];

                        G[i, j].X = qy[i, j];
                        G[i, j].Y = qy[i, j] * u[i, j];
                        //G[i, j].Z = qy[i, j] * qy[i, j] / h[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                        G[i, j].Z = qy[i, j] * v[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                    }
                }
            }

            
            int iMax = 0, jMax = 0;
            for (int i = 0; i < Nx; ++i)
            {
                for (int j = 0; j < Ny; ++j)
                {
                    if (h[iMax, jMax] < h[i, j])
                    {
                        iMax = i;
                        jMax = j;
                    }
                }
            }
            var hFree = h[iMax, jMax] + zeta[iMax, jMax];

            //TODO Сделать учет впадин 
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (map[i, j].HasFlag(NodeType.Input) || map[i, j].HasFlag(NodeType.Output))
                    {
                        continue;
                    }

                    if (h[i, j] > MEM.Error2)
                    {
                        map[i, j] = NodeType.SolveWet;
                    }
                    else if (h[i, j] > 0)
                    {
                        map[i, j] = NodeType.BordaryWet;
                    }
                    else if (h[i, j] > -MEM.Error2)
                    {
                        map[i, j] = NodeType.BordaryDry;
                        h[i, j] = hFree - zeta[i, j];
                    }
                    else
                    {
                        map[i, j] = NodeType.SolveDry;
                        h[i, j] = hFree - zeta[i, j];
                    }
                }
            }
        }
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public void SolverStep1()
        {
            diffuseBorder();
            Print(Q, "Q 1");
            // Обход внутренних узлов области
            Parallel.For(1, Nx - 1, (i) =>
            {
                Vector3 R = new Vector3();
                for (uint j = 1; j < Ny - 1; ++j)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        diffuseInner((uint)i, (uint)j);

                        var Fr = F[i + 1, j];
                        var Fl = F[i - 1, j];
                        var Gr = G[i, j + 1];
                        var Gl = G[i, j - 1];

                        var Fx = 0.5 * (Fr - Fl) / dx;
                        var Gy = 0.5 * (Gr - Gl) / dy;

                        if (map[i, j].HasFlag(NodeType.Bordary) == true)
                        {
                            if (h[i + 1, j] < MEM.Error2)
                            {
                                //var dz = zeta[i + 1, j] - zeta[i, j];
                                //var dx1 = h[i, j] * dx / dz;
                                Fr = F[i, j];
                                Fx = (Fr - Fl) / dx;
                            }
                            else if (h[i - 1, j] < MEM.Error2)
                            {
                                //var dz = zeta[i + 1, j] - zeta[i, j];
                                //var dx1 = h[i, j] * dx / dz;

                                Fl = F[i, j];
                                Fx = (Fr - Fl) / dx;
                            }
                            if (h[i, j + 1] < MEM.Error2)
                            {
                                //var dz = zeta[i, j + 1] - zeta[i, j];
                                //var dy1 = h[i, j] * dy / dz;

                                Gr = G[i, j];
                                Gy = (Gr - Gl) / dy;
                            }
                            else if (h[i, j - 1] < MEM.Error2)
                            {
                                //var dz = zeta[i, j - 1] - zeta[i, j];
                                //var dy1 = h[i, j] * dy / dz;

                                Gl = G[i, j];
                                Gy = (Gr - Gl) / dy;
                            }
                        }

                        // Схема центральных разностей
                        Q[i, j] = Q[i, j] - dtime * (Fx + Gy);

                        if (h[i, j] > MEM.Error2)
                        {
                            var Cs = (double)Math.Pow(h[i, j], 1f / 6f) / roughness;
                            var lambda = Physics.g / Cs / Cs;
                            var sqrtU2V2 = (double)Math.Sqrt(u[i, j] * u[i, j] + v[i, j] * v[i, j]);
                            tau_x[i, j] = lambda * sqrtU2V2 * u[i, j];
                            tau_y[i, j] = lambda * sqrtU2V2 * v[i, j];
                        }
                        else
                        {
                            tau_x[i, j] = 0;
                            tau_y[i, j] = 0;
                        }
                        R.Y = tau_x[i, j];
                        R.Z = tau_y[i, j];
                        // Учет трения дна
                        Q[i, j] -= dtime * R;
                    }
                }
            });// по пространству
            Print(Q, "Q 2");
            // коррекция решения
            FCT();
            Print(Q, "Q 3");
            borderCondition();

            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        h[i, j] = Q[i, j].X;
                        if (h[i, j] > MEM.Error2)
                        {
                            u[i, j] = Q[i, j].Y / Q[i, j].X;
                            v[i, j] = Q[i, j].Z / Q[i, j].X;
                            qx[i, j] = Q[i, j].Y;
                            qy[i, j] = Q[i, j].Z;
                        }
                        else
                        {
                            u[i, j] = 0;
                            v[i, j] = 0;
                            qx[i, j] = 0;
                            qy[i, j] = 0;
                            Q[i, j].X = 0;
                            h[i, j] = 0;
                        }
                    }
                }
            }
            // Обновление дивергентных векторов
            Parallel.For(0, Nx, (i) =>
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        h[i, j] = Q[i, j].X;
                        //if (h[i, j] > MEM.Error2)
                        //{
                        //    u[i, j] = Q[i, j].Y / Q[i, j].X;
                        //    v[i, j] = Q[i, j].Z / Q[i, j].X;
                        //    qx[i, j] = Q[i, j].Y;
                        //    qy[i, j] = Q[i, j].Z;
                        //}
                        //else
                        //{
                        //    u[i, j] = 0;
                        //    v[i, j] = 0;
                        //    qx[i, j] = 0;
                        //    qy[i, j] = 0;
                        //    Q[i, j].X = 0;
                        //    h[i, j] = 0;
                        //}

                        F[i, j].X = qx[i, j];
                        //F[i, j].Y = qx[i, j] * qx[i, j] / h[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                        F[i, j].Y = qx[i, j] * u[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                        F[i, j].Z = qx[i, j] * v[i, j];

                        G[i, j].X = qy[i, j];
                        G[i, j].Y = qy[i, j] * u[i, j];
                        //G[i, j].Z = qy[i, j] * qy[i, j] / h[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                        G[i, j].Z = qy[i, j] * v[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                    }
                }
            });

            var hFree = freeWater();
            //TODO Сделать учет впадин 
            Parallel.For(0, Nx, (i) =>
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (map[i, j].HasFlag(NodeType.Input) || map[i, j].HasFlag(NodeType.Output))
                    {
                        continue;
                    }

                    if (h[i, j] > MEM.Error2)
                    {
                        map[i, j] = NodeType.SolveWet;
                    }
                    else if (h[i, j] > 0)
                    {
                        map[i, j] = NodeType.BordaryWet;
                    }
                    else if (h[i, j] > -MEM.Error2)
                    {
                        map[i, j] = NodeType.BordaryDry;
                        h[i, j] = hFree - zeta[i, j];
                    }
                    else
                    {
                        map[i, j] = NodeType.SolveDry;
                        h[i, j] = hFree - zeta[i, j];
                    }
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double freeWater()
        {
            int iMax = 0, jMax = 0;
            for (int i = 0; i < Nx; ++i)
            {
                for (int j = 0; j < Ny; ++j)
                {
                    if (h[iMax, jMax] < h[i, j])
                    {
                        iMax = i;
                        jMax = j;
                    }
                }
            }
            return h[iMax, jMax] + zeta[iMax, jMax];
        }
        /// <summary>
        /// Чтение входных данных задачи из файла
        /// геометрия канала по умолчанию
        /// эволюция расходов/уровней
        /// </summary>
        /// <param name="p"></param>
        public void LoadData(string fileName)
        {

        }
        /// <summary>
        /// Установка объектоа КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public void Set(IMesh mesh, IAlgebra algebra = null)
        {

        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public void SetZeta(double[] zeta, bool flagBLoad)
        {

        }
        /// <summary>
        /// Получить отметки дна
        /// </summary>
        /// <param name="zeta"></param>
        public void GetZeta(ref double[] zeta)
        {

        }

        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public IRiver Clone()
        {
            return new RiverSW_FCT(this);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public ITaskFormat<IRiver> GetFormater()
        {
            return new RiverFormatReader2DTri();
        }
        double[] b_zeta = null, b_h = null, b_qx = null, b_qy = null, b_u = null, b_v = null;
        double[] tauX = null, tauY = null, P = null;
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {

            Get(ref b_zeta, ref b_h, ref b_qx, ref b_qy, ref b_u, ref b_v);

            sp.Add("Расход ", b_qx, b_qy);
            sp.Add("Расход qx", b_qx);
            sp.Add("Расход qy", b_qy);

            sp.Add("Скорость ", b_u, b_v);
            sp.Add("Скорость x", b_u);
            sp.Add("Скорость y", b_v);

            sp.Add("Отметки дна", b_zeta);
            sp.Add("Глубина", b_h);

            GetTau(ref tauX, ref tauY, ref P, StressesFlag.Nod);

            sp.Add("Напряжения ", tauX, tauY);
            sp.Add("Напряжения tauX", tauX);
            sp.Add("Напряжения tauY", tauY);
            sp.Add("Уровень свободной повверхности eta", P);
        }

        protected void Get(ref double[] b_zeta, ref double[] b_h, ref double[] b_qx, ref double[] b_qy, ref double[] b_u, ref double[] b_v)
        {
            int CountKnots = mesh.CountKnots;
            MEM.Alloc(CountKnots, ref b_zeta);
            MEM.Alloc(CountKnots, ref b_h);
            MEM.Alloc(CountKnots, ref b_qx);
            MEM.Alloc(CountKnots, ref b_qy);
            MEM.Alloc(CountKnots, ref b_u);
            MEM.Alloc(CountKnots, ref b_v);
            int k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    b_u[k] = u[i, j];
                    b_v[k] = v[i, j];
                    b_qx[k] = qx[i, j];
                    b_qy[k] = qy[i, j];
                    b_h[k] = h[i, j];
                    b_zeta[k] = zeta[i, j];
                    k++;
                }
            }
        }

        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, StressesFlag sf = StressesFlag.Nod)
        {
            int CountKnots = mesh.CountKnots;
            MEM.Alloc(CountKnots, ref tauX);
            MEM.Alloc(CountKnots, ref tauY);
            MEM.Alloc(CountKnots, ref P);
            int k = 0;
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    tauX[k] = tau_x[i, j];
                    tauY[k] = tau_y[i, j];
                    P[k] = Physics.rho_w * Physics.g * h[i, j];
                    k++;
                }
            }
        }
        #endregion

        /// <summary>
        /// установка параметров
        /// </summary>
        /// <param name="p"></param>
        public new void SetParams(object p)
        {
            base.SetParams(p);
            InitTask();
        }

        protected void InitTask()
        {
            int[] BCMark = new int[] { 0, 1, 2, 3 };
            TypeBoundCond[] BCType = new TypeBoundCond[]
            {
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann,
                TypeBoundCond.Dirichlet,
                TypeBoundCond.Neumann
            };
            switch (TaskIndex)
            {
                case FCTTaskIndex.CylinderDecay:
                    // Растекание водного столба
                    initialCondition = initialCondition_CylinderDecay;
                    borderCondition = borderCondition_CylinderDecay;
                    break;
                case FCTTaskIndex.ConditionX:
                    initialCondition = initialConditionX;
                    borderCondition = borderConditionX;
                    break;
                case FCTTaskIndex.ConditionY:
                    initialCondition = initialConditionY;
                    borderCondition = borderConditionY;
                    break;
                case FCTTaskIndex.Trapeziform_X:
                    // Начальные условия для потока вдоль X трапециевидной формы
                    initialCondition = initialConditionX_Trapeziform;
                    // Граничные условия для потока вдоль X трапециевидной формы
                    borderCondition = borderConditionX_Trapeziform;
                    break;
                case FCTTaskIndex.Trapeziform_Y:
                    initialCondition = initialConditionY_Trapeziform;
                    borderCondition = borderConditionY_Trapeziform;
                    break;
                case FCTTaskIndex.ParabGradient_SW:
                    initialCondition = initialConditionX_ParabolicGradientForm_SW;
                    borderCondition = borderConditionX_ParabolicGradientForm_SW;
                    break;
                case FCTTaskIndex.ParabGradient:
                    initialCondition = initialConditionX_ParabolicGradientForm;
                    borderCondition = borderConditionX_ParabolicGradientForm;
                    break;
                case FCTTaskIndex.Parab_SW:
                    initialCondition = initialConditionX_Parabolicform_SW;
                    borderCondition = borderConditionX_Parabolicform_SW;
                    break;
                case FCTTaskIndex.Parabolic:
                    // Начальные условия для потока вдоль X параболической формы
                    initialCondition = initialConditionX_Parabolicform;
                    // Граничные условия для потока вдоль X параболической формы
                    borderCondition = borderConditionX_Parabolicform;
                    break;
                case FCTTaskIndex.Vform_X:
                    // Начальные условия для потока вдоль X V-образной формы
                    initialCondition = initialConditionX_Vform;
                    // Граничные условия для потока вдоль X V-образной формы
                    borderCondition = borderConditionX_Vform;
                    break;
                case FCTTaskIndex.Dike_Х:
                    // Начальные условия для задачи прорыва плотины при потоке вдоль X
                    initialCondition = initialConditionX_Dike;
                    // Граничные условия для задачи прорыва плотины при потоке потока вдоль X
                    borderCondition = borderConditionX_Dike;
                    break;
            }
            memoryAllocation(BCMark, BCType);
            initialization();
        }
        public RiverSW_FCT(RiverSW_FCTParams p) : base(p)
        {
            InitTask();
        }
        /// <summary>
        /// Функция для выделения памяти под массивы
        /// </summary>
        private void memoryAllocation(int[] BCMark, TypeBoundCond[] BCType)
        {
            mesh = new RectangleMesh(Lx, Ly, Nx, Ny, BCMark, BCType);
            dx = mesh.dx;
            dy = mesh.dy;
            // Выделение памяти под рабочии массивы
            MEM.Alloc2D(Nx, Ny, ref u);
            MEM.Alloc2D(Nx, Ny, ref v);
            MEM.Alloc2D(Nx, Ny, ref h);
            MEM.Alloc2D(Nx, Ny, ref zeta);
            MEM.Alloc2D(Nx, Ny, ref qx);
            MEM.Alloc2D(Nx, Ny, ref qy);
            MEM.Alloc2D(Nx, Ny, ref tau_x);
            MEM.Alloc2D(Nx, Ny, ref tau_y);
            // Вектора потоков
            MEM.Alloc2D(Nx, Ny, ref Q);
            MEM.Alloc2D(Nx, Ny, ref DxQ);
            MEM.Alloc2D(Nx, Ny, ref DyQ);
            // дивергентный вектор
            MEM.Alloc2D(Nx, Ny, ref F);
            MEM.Alloc2D(Nx, Ny, ref G);
            // Дифузия и антидифузия дивергентного вектора
            MEM.Alloc2D(Nx, Ny, ref fD);
            MEM.Alloc2D(Nx, Ny, ref gD);
            MEM.Alloc2D(Nx, Ny, ref fAD);
            MEM.Alloc2D(Nx, Ny, ref gAD);

            // Выделение памяти под вспомогательные массивы
            MEM.Alloc2D(Nx, Ny, ref regionSolver);
            MEM.Alloc2D(Nx, Ny, ref inputBoundary);
            MEM.Alloc2D(Nx, Ny, ref outputBoundary);
            MEM.Alloc2D(Nx, Ny, ref map);
        }
        ///// <summary>
        ///// Начальные условия нулевые
        ///// </summary>
        //public void initialConditionEmpty()
        //{
        //    for (uint i = 0; i < Nx; i++)
        //    {
        //        for (uint j = 0; j < Ny; j++)
        //        {
        //            u[i, j] = 0f;
        //            v[i, j] = 0f;
        //            h[i, j] = 1f;
        //            zeta[i, j] = 0f;
        //        }
        //    }
        //    for (uint i = 1; i < Nx - 1; ++i)
        //    {
        //        for (uint j = 1; j < Ny - 1; ++j)
        //        {
        //            regionSolver[i, j] = true;
        //        }
        //    }
        //    for (uint j = 1; j < Ny - 1; j++)
        //    {
        //        inputBoundary[0, j] = true;
        //    }
        //    for (uint j = 1; j < Ny - 1; j++)
        //    {
        //        outputBoundary[Nx - 1, j] = true;
        //    }
        //}
        ///// <summary>
        ///// Граничные условия
        ///// </summary>
        //public void borderConditionEmpty()
        //{
        //    var h = 1.0f;
        //    var u = 0.0f;
        //    var v = 0.0f;

        //    for (int i = 0; i < Nx; ++i)
        //    {
        //        Q[i, 0] = Q[i, 1];
        //        Q[i, Ny - 1] = Q[i, Ny - 2];
        //    }
        //    // Условие на входе канала
        //    for (uint j = 1; j < Ny - 1; j++)
        //    {
        //        Q[0, j].X = h;
        //        Q[0, j].Y = h * u;
        //        Q[0, j].Z = h * v;
        //    }
        //}
        /// <summary>
        /// Инициализация начальных значений
        /// </summary>
        public void initialization()
        {
            initialCondition();
            // Установка начальных условий для векторных полей F и Q
            Parallel.For(0, Nx, (i) =>
            {
                for (uint j = 0; j < Ny; j++)
                {
                    qx[i, j] = h[i, j] * u[i, j];
                    qy[i, j] = h[i, j] * v[i, j];

                    Q[i, j].X = h[i, j];
                    Q[i, j].Y = qx[i, j];
                    Q[i, j].Z = qy[i, j];

                    F[i, j].X = qx[i, j];
                    F[i, j].Y = qx[i, j] * u[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                    F[i, j].Z = qx[i, j] * v[i, j];

                    G[i, j].X = qy[i, j];
                    G[i, j].Y = qy[i, j] * u[i, j];
                    G[i, j].Z = qy[i, j] * v[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                }
            });
        }


        /// <summary>
        /// Расчет диффузии на границе
        /// </summary>
        protected void diffuseBorder()
        {
            //Parallel.For(0, Nx-1, (i) => {
            //    for (int j = 0; j < Ny-1; j++) {
            //        //if (inputBoundary[i, j]) {
            //            var alpha = dtime * (u[i, j] + u[i, j + 1]) / dx;
            //            var nu = ETA + ET1 * alpha * alpha;
            //            fD[i, j] = nu * (Q[i, j + 1] - Q[i, j]);

            //            var epsilon = dtime * ((v[i, j] + v[i + 1, j]) / dy);
            //            var lambda = ETA + ET1 * epsilon * epsilon;
            //            gD[i, j] = lambda * (Q[i + 1, j] - Q[i, j]);
            //        //}
            //    }
            //});

            //for (int j = 0; j < Ny; j++) {
            //    var alpha = dtime * (u[0, j] + u[1, j]) / dx;
            //    var nu = ETA + ET1 * alpha * alpha;
            //    fD[0, j] = nu * (Q[1, j] - Q[0, j]);
            //}

            //for (int i = 0; i < Nx; i++) {
            //    var epsilon = dtime * ((v[i, 0] + v[i, 1]) / dy);
            //    var lambda = ETA + ET1 * epsilon * epsilon;
            //    gD[i, 0] = lambda * (Q[i, 1] - Q[i, 0]);
            //}

            Parallel.For(0, Ny - 1, (j) =>
            {
                var alpha = dtime * (u[0, j] + u[1, j]) / dx;
                var nu = ETA + ET1 * alpha * alpha;
                fD[0, j] = nu * (Q[1, j] - Q[0, j]);
            });

            Parallel.For(0, Nx - 1, (i) =>
            {
                var epsilon = dtime * ((v[i, 0] + v[i, 1]) / dy);
                var lambda = ETA + ET1 * epsilon * epsilon;
                gD[i, 0] = lambda * (Q[i, 1] - Q[i, 0]);
            });

        }

        /// <summary>
        /// Расчет диффузии узла
        /// </summary>
        /// <param name="i">номер узла по оси x</param>
        /// <param name="j">номер узла по оси y</param>
        protected void diffuseInner(uint i, uint j)
        {
            var alpha = dtime * (u[i, j] + u[i + 1, j]) / dx;
            var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
            var nu = ETA + ET1 * alpha * alpha;
            var lambda = ETA + ET1 * epsilon * epsilon;
            fD[i, j] = nu * (Q[i + 1, j] - Q[i, j]);
            gD[i, j] = lambda * (Q[i, j + 1] - Q[i, j]);
        }

        /// <summary>
        /// Вычисляет шаг по времени на основе условия Куранта
        /// </summary>
        protected double dtEvaluate()
        {
            var umax = maxAbs(u);
            var vmax = maxAbs(v);
            var hmax = maxAbs(h);

            var dhMin = Math.Min(dx, dy);
            //var c = umax * umax + vmax * vmax + Physics.g * hmax * Physics.g * hmax;
            var c = umax * umax + vmax * vmax + Physics.g * hmax;

            return CourantNumber * dhMin / ((double)Math.Sqrt(c) + 0.00001f);
        }
        protected double maxAbs(double[,] a)
        {
            var max = Math.Abs(a[0, 0]);
            foreach (var x in a)
            {
                if (Math.Abs(x) > max)
                    max = x;
            }
            return max;
        }
        /// <summary>
        /// Коррекция потока
        /// </summary>
        protected void FCT()
        {
            // антидиффузия потоков на входной границе
            //Parallel.For(0, Nx-1, (i) => {
            //    for (int j = 0; j < Ny-1; j++) {
            //        //if (inputBoundary[i, j] || map[i, j].HasFlag(NodeType.InputBordary) ) {
            //            var alpha = dtime * (u[i, j] + u[i, j + 1]) / dx;
            //            var mu = ETA + 0.25f * ET2 * alpha * alpha;
            //            fAD[i, j] = mu * (Q[i, j + 1] - Q[i, 0]);

            //            var epsilon = dtime * (v[i, j] + v[i + 1, j]) / dy;
            //            var kappa = ETA + 0.25f * ET2 * epsilon * epsilon;
            //            gAD[i, j] = kappa * (Q[i + 1, j] - Q[i, j]);
            //    //}
            //}
            //});

            //for (int j = 0; j < Ny; j++) {
            //    var alpha = dtime * (u[0, j] + u[1, j]) / dx;
            //    var mu = ETA + 0.25f * ET2 * alpha * alpha;
            //    fAD[0, j] = mu * (Q[1, j] - Q[0, j]);

            //}

            //for (int i = 0; i < Nx; i++) {
            //    var epsilon = dtime * (v[i, 0] + v[i, 1]) / dy;
            //    var kappa = ETA + 0.25f * ET2 * epsilon * epsilon;
            //    gAD[i, 0] = kappa * (Q[i, 1] - Q[i, 0]);
            //}

            //Parallel.For(0, Ny - 1, (j) =>
            for (int j = 0; j < Ny; j++)
            {
                var alpha = dtime * (u[0, j] + u[1, j]) / dx;
                var mu = ETA + 0.25f * ET2 * alpha * alpha;
                fAD[0, j] = mu * (Q[1, j] - Q[0, j]);
            }//);

                //Parallel.For(0, Nx - 1, (i) =>
             for (int i = 0; i < Nx; i++)
             {
                  var epsilon = dtime * (v[i, 0] + v[i, 1]) / dy;
                  var kappa = ETA + 0.25f * ET2 * epsilon * epsilon;
                  gAD[i, 0] = kappa * (Q[i, 1] - Q[i, 0]);
             }//);

            // расчет антидифузионного потока
            //Parallel.For(1, Nx - 1, (i) =>
            for (int i = 1; i < Nx-1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        var alpha   = dtime * (u[i, j] + u[i + 1, j]) / dx;
                        var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
                        var mu = ETA + ET2 * alpha * alpha;
                        var kappa = ETA + ET2 * epsilon * epsilon;

                        fAD[i, j] =    mu * (Q[i + 1, j] - Q[i, j]);
                        gAD[i, j] = kappa * (Q[i, j + 1] - Q[i, j]);

                        Q[i, j] = Q[i, j] + fD[i, j] - fD[i - 1, j] + gD[i, j] - gD[i, j - 1];

                        DxQ[i - 1, j] = Q[i, j] - Q[i - 1, j];
                        DyQ[i, j - 1] = Q[i, j] - Q[i, j - 1];
                    }
                }
            }//);
            Print(Q, "Q F1");
            //Parallel.For(0, Nx, (i) =>
            for (int i = 0; i < Nx; i++)
            {
                DyQ[i, Ny - 2] = Q[i, Ny - 1] - Q[i, Ny - 2];
            }//);
            //Parallel.For(0, Ny, (j) =>
            for (int j = 0; j < Ny; j++)
            {
                DxQ[Nx - 2, j] = Q[Nx - 1, j] - Q[Nx - 2, j];
            }//);


            // Ограничение антидиффузионных членов
            //Parallel.For(1, Nx - 1, (i) =>
            for (int i = 1; i < Nx-1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        fAD[i, j].X = limiter(fAD[i, j].X, DxQ[i - 1, j].X, DxQ[i + 1, j].X);
                        fAD[i, j].Y = limiter(fAD[i, j].Y, DxQ[i - 1, j].Y, DxQ[i + 1, j].Y);
                        fAD[i, j].Z = limiter(fAD[i, j].Z, DxQ[i - 1, j].Z, DxQ[i + 1, j].Z);

                        gAD[i, j].X = limiter(gAD[i, j].X, DyQ[i, j - 1].X, DyQ[i, j + 1].X);
                        gAD[i, j].Y = limiter(gAD[i, j].Y, DyQ[i, j - 1].Y, DyQ[i, j + 1].Y);
                        gAD[i, j].Z = limiter(gAD[i, j].Z, DyQ[i, j - 1].Z, DyQ[i, j + 1].Z);

                        // учет антидиффузии в решении
                        Q[i, j] = Q[i, j] - (fAD[i, j] - fAD[i - 1, j] + gAD[i, j] - gAD[i, j - 1]);
                    }
                }
            }//);
            Print(Q, "Q F2");
            //Test(Q, "Q");
            //Test(fAD, "fAD");
            //Test(gAD, "gAD");
            //Print(Q, "Q");
        }

        public void Print(Vector3[,] f, string str = "")
        {
            return;
            Console.WriteLine();
            Console.WriteLine(str);
            for (int i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                        Console.Write(f[i, j].X.ToString("F9")+" ");
                Console.WriteLine();
            }
            Console.WriteLine();
            for (int i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                    Console.Write(f[i, j].Y.ToString("F9") + " ");
                Console.WriteLine();
            }
            Console.WriteLine();
            for (int i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                    Console.Write(f[i, j].Z.ToString("F9") + " ");
                Console.WriteLine();
            }
        }
        public void Test(Vector3[,] f,string str="")
        {
            for (int i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (MEM.Equals(f[i,j].Z ,0, MEM.Error9)!=true)
                    {
                        Console.WriteLine(str+": qy[{1}][{2}] = {0}", f[i, j].Z,i,j);
                        break;
                    }
                }
            }
        }
        // Ограничитель функций
        protected double limiter(double fAD, double DQM, double DQA)
        {
            var s = Math.Sign(fAD);
            fAD = Math.Abs(fAD);
            var DUM = s * DQM;
            fAD = Math.Min(fAD, DUM);
            DUM = s * DQA;
            fAD = Math.Min(fAD, DUM);
            fAD = Math.Max(fAD, 0);
            fAD = s * fAD;
            return fAD;
        }
        protected Vector3 limiter(Vector3 fAD, Vector3 DQM, Vector3 DQA)
        {
            var s = Vector3.Sign(fAD);
            fAD = Vector3.Abs(fAD);
            var DUM = s * DQM;
            fAD = Vector3.Min(fAD, DUM);
            DUM = s * DQA;
            fAD = Vector3.Min(fAD, DUM);
            fAD = Vector3.Max(fAD, Vector3.Zero);
            fAD = s * fAD;
            return fAD;
        }

        // ---------------------------------------------------------------------------------------
        // Тестовые НУ и ГУ
        // ---------------------------------------------------------------------------------------
        #region Начальные и граничные условия
        /// <summary>
        /// Начальные условия для потока вдоль X
        /// </summary>
        public void initialConditionX()
        {
            // область до скачка
            for (int i = 0; i <= Nx / 2; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0f;
                    v[i, j] = 0;
                    h[i, j] = 10f;
                    zeta[i, j] = 0;
                }
            }

            // область после скачка
            for (int i = Nx / 2 + 1; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    h[i, j] = 1f;
                    zeta[i, j] = 0;
                }
            }


            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                    map[i, j] = NodeType.Solve | NodeType.Wet;
                }
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
                map[0, j] = NodeType.InputBordary;
                outputBoundary[Nx - 1, j] = true;
                map[Nx - 1, j] = NodeType.OutputBordary;
            }
        } // initialConditionX() 

        /// <summary>
        /// Граничные условия для потока вдоль X
        /// </summary>
        public void borderConditionX()
        {
            //var h = 1.0f;
            //var u = 0.501f;
            //var v = 0.0f;

            // Условие на стенках канала
            for (uint i = 1; i < Nx - 1; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }

            //var t = new Vector3(h, h * u, h * v);
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                //Q[0, j] = t;
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
        } // void borderConditionX()

        /// <summary>
        /// Начальные условия для задачи прорыва плотины при потоке вдоль X
        /// </summary>
        public void initialConditionX_Dike()
        {
            // Прорыв дамбы
            // область до скачка
            for (uint i = 0; i <= 94; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0.0f;
                    v[i, j] = 0;
                    h[i, j] = 10;
                    zeta[i, j] = 0;
                }
            }

            // область после скачка
            for (uint i = 104; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    h[i, j] = 5f;
                    zeta[i, j] = 0;
                }
            }
            // область прорыва дамбы
            for (uint i = 95; i < 104; i++)
            {
                for (uint j = 95; j < 170; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    h[i, j] = 5f;
                    zeta[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            // Область дамбы
            for (uint i = 94; i < 105; i++)
            {
                for (uint j = 1; j <= 95; j++)
                {
                    regionSolver[i, j] = false;
                }
            }
            for (uint i = 94; i < 105; i++)
            {
                for (uint j = 169; j < Ny; j++)
                {
                    regionSolver[i, j] = false;
                }
            }

            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
                outputBoundary[Nx - 1, j] = true;
            }
        } // initialConditionX_Dike() 

        /// <summary>
        /// Граничные условия для задачи прорыва плотины при потоке потока вдоль X
        /// </summary>
        public void borderConditionX_Dike()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
        } // void borderConditionX_Dike()


        /// <summary>
        /// Начальные условия для потока вдоль X V-образной формы
        /// </summary>
        public void initialConditionX_Vform()
        {
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (j < Ny * 0.5)
                    {
                        zeta[i, j] = -0.1f * dy * j + 0.05f;
                    }
                    else
                    {
                        zeta[i, j] = 0.1f * dy * j - 0.05f;
                    }
                    h[i, j] = 1 - zeta[i, j];
                    u[i, j] = Qh / h[i, j];
                    v[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
            for (uint j = 1; j < Ny - 1; j++)
            {
                outputBoundary[Nx - 1, j] = true;
            }
        } // initialConditionX_Vform() 

        /// <summary>
        /// Граничные условия для потока вдоль X V-образной формы
        /// </summary>
        public void borderConditionX_Vform()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                var h = 1 - zeta[0, j];
                var u = Qh / h;
                var v = 0f;
                Q[0, j].X = h;
                Q[0, j].Y = h * u;
                Q[0, j].Z = h * v;
            }
        } // void borderConditionX_Vform()

        /// <summary>
        /// Начальные условия для потока вдоль X трапециевидной формы
        /// </summary>
        public void initialConditionX_Trapeziform()
        {
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (j < Ny * 0.3)
                    {
                        zeta[i, j] = -1f / 6f * dy * j + 0.3f * 1f / 6f;
                    }
                    else if (j < Ny * 0.7)
                    {
                        zeta[i, j] = 0;
                    }
                    else
                    {
                        zeta[i, j] = 1f / 6f * dy * j - 0.7f * 1f / 6f;
                    }
                    h[i, j] = 0.1f - zeta[i, j];
                    u[i, j] = Qh / h[i, j];
                    v[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
            for (uint j = 1; j < Ny - 1; j++)
            {
                outputBoundary[Nx - 1, j] = true;
            }
        } // initialConditionX_Vform() 

        /// <summary>
        /// Граничные условия для потока вдоль X трапециевидной формы
        /// </summary>
        public void borderConditionX_Trapeziform()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                var h = 0.1f - zeta[0, j];
                var u = Qh / h;
                var v = 0f;
                Q[0, j].X = h;
                Q[0, j].Y = h * u;
                Q[0, j].Z = h * v;
            }
        } // void borderConditionX_trapeziform()

        /// <summary>
        /// Начальные условия для потока вдоль X параболической формы
        /// </summary>
        public void initialConditionX_Parabolicform()
        {
            var y1 = Ly / 10.0f;
            var y2 = Ly - y1;
            var y0 = (y1 + y2) / 2.0f;
            var zy = (y0 - y1) * (y0 - y2);
            var h0 = Math.Min(Lx, Ly) / 50f;
            var H0 = h0 + 0.0f;

            // область до скачка
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    var y = dy * j;
                    var h1 = h0 * (1 - (y - y1) * (y - y2) / zy);

                    zeta[i, j] = h1;
                    h[i, j] = H0 - h1;
                    if (h[i, j] > MEM.Error2)
                    {
                        map[i, j] = NodeType.SolveWet;
                    }
                    else if (h[i, j] > 0)
                    {
                        map[i, j] = NodeType.BordaryWet;
                    }
                    else if (h[i, j] > -MEM.Error2)
                    {
                        map[i, j] = NodeType.BordaryDry;
                    }
                    else
                    {
                        map[i, j] = NodeType.SolveDry;
                    }

                    if (h[i, j] > MEM.Error2)
                    {
                        u[i, j] = 0;
                    }
                    else
                    {
                        u[i, j] = 0;
                    }
                    v[i, j] = 0;
                }
            }

            //for (uint i = 1; i < Nx - 1; i++) {
            //    for (uint j = 1; j < Ny - 1; j++) {
            //        regionSolver[i, j] = true;
            //        map[i, j] = NodeType.SolveWet;
            //    }
            //}
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
                map[0, j] = NodeType.InputBordary;
            }
            for (uint j = 1; j < Ny - 1; j++)
            {
                outputBoundary[Nx - 1, j] = true;
                map[Nx - 1, j] = NodeType.OutputBordary;
            }
        } // initialConditionX_Parabolicform() 

        /// <summary>
        /// Граничные условия для потока вдоль X параболической формы
        /// </summary>
        public void borderConditionX_Parabolicform()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                var h = this.h[0, j];
                var u = 0f;
                if (h > MEM.Error2)
                {
                    u = 0;
                }
                var v = 0f;
                Q[0, j].X = h;
                Q[0, j].Y = h * u;
                Q[0, j].Z = h * v;
            }
        } // void borderConditionX_Parabolicform()


        /// <summary>
        /// Начальные условия для потока вдоль X параболической формы
        /// </summary>
        public void initialConditionX_Parabolicform_SW()
        {
            var y1 = 0.0f;
            var y2 = Ly;
            var y0 = (y1 + y2) / 2.0f;
            var zy = (y0 - y1) * (y0 - y2);
            var h0 = Math.Min(Lx, Ly) / 50f;
            var H0 = h0 + 0.0f;

            int N0 = Nx / 10;

            // область до скачка
            for (uint i = 0; i < N0; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    var y = dy * j;
                    var h1 = h0 * (1 - (y - y1) * (y - y2) / zy);

                    zeta[i, j] = h1;
                    h[i, j] = H0 - h1;
                    if (h[i, j] > MEM.Error2)
                    {
                        u[i, j] = 5;
                    }
                    else
                    {
                        u[i, j] = 0;
                    }
                    v[i, j] = 0;
                }
            }

            // область после скачка
            for (int i = N0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    var y = dy * j;
                    var h1 = h0 * (1 - (y - y1) * (y - y2) / zy);

                    zeta[i, j] = h1;
                    h[i, j] = 0.1f;

                    u[i, j] = 0;
                    v[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                outputBoundary[Nx - 1, j] = true;
            }
        } // initialConditionX_Parabolicform_SW() 

        /// <summary>
        /// Граничные условия для потока вдоль X параболической формы
        /// </summary>
        public void borderConditionX_Parabolicform_SW()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                var h = this.h[0, j];
                var u = 0f;
                if (h > MEM.Error2)
                {
                    u = 5;
                }
                var v = 0f;
                Q[0, j].X = h;
                Q[0, j].Y = h * u;
                Q[0, j].Z = h * v;
            }
        } // void borderConditionX_Parabolicform()


        /// <summary>
        /// Начальные условия для потока вдоль X параболической формы с наклонным дном
        /// </summary>
        public void initialConditionX_ParabolicGradientForm()
        {
            var y1 = 0.0f;
            var y2 = Ly;
            var y0 = (y1 + y2) / 2.0f;
            var zy = (y0 - y1) * (y0 - y2);
            var h0 = Math.Min(Lx, Ly) / 50f;
            var H0 = h0 + 0.0f;

            var angle = 0.25 * (float)Math.PI / 180.0f;

            // область до скачка
            for (uint i = 0; i < Nx; i++)
            {
                var lr = dx * (Nx - i);
                var zetaX = lr * (float)Math.Tan(angle);

                for (uint j = 0; j < Ny; j++)
                {
                    var y = dy * j;
                    var h1 = (h0 - zetaX) * (1 - (y - y1) * (y - y2) / zy);
                    zeta[i, j] = h1 + zetaX;
                    h[i, j] = H0 - h1;
                    if (h[i, j] > MEM.Error2)
                    {
                        u[i, j] = 0;
                    }
                    else
                    {
                        u[i, j] = 0;
                    }
                    v[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
            for (uint j = 1; j < Ny - 1; j++)
            {
                outputBoundary[Nx - 1, j] = true;
            }
        } // initialConditionX_ParabolicGradientForm() 

        /// <summary>
        /// Граничные условия для потока вдоль X параболической формы с наклонным дном
        /// </summary>
        public void borderConditionX_ParabolicGradientForm()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                var h = this.h[0, j];
                var u = 0f;
                if (h > MEM.Error2)
                {
                    u = 0;
                }
                var v = 0f;
                Q[0, j].X = h;
                Q[0, j].Y = h * u;
                Q[0, j].Z = h * v;
            }
        } // void borderConditionX_ParabolicGradientForm()


        /// <summary>
        /// Начальные условия для потока вдоль X параболической формы с наклонным дном
        /// </summary>
        public void initialConditionX_ParabolicGradientForm_SW()
        {
            var y1 = 0.0f;
            var y2 = Ly;
            var y0 = (y1 + y2) / 2.0f;
            var zy = (y0 - y1) * (y0 - y2);
            var h0 = Math.Min(Lx, Ly) / 50f;
            var H0 = h0 + 0.0f;

            var angle = 1 * (float)Math.PI / 180.0f;

            int N0 = Nx / 10;

            // область до скачка
            for (uint i = 0; i < N0; i++)
            {
                var lr = dx * (Nx - i);
                var zetaX = lr * (float)Math.Tan(angle);

                for (uint j = 0; j < Ny; j++)
                {
                    var y = dy * j;
                    var h1 = (h0 - zetaX) * (1 - (y - y1) * (y - y2) / zy);
                    zeta[i, j] = h1 + zetaX;
                    h[i, j] = H0 - h1;
                    if (h[i, j] > MEM.Error2)
                    {
                        u[i, j] = 5;
                    }
                    else
                    {
                        u[i, j] = 0;
                    }
                    v[i, j] = 0;
                }
            }

            // область после скачка
            for (int i = N0; i < Nx; i++)
            {
                var lr = dx * (Nx - i);
                var zetaX = lr * (float)Math.Tan(angle);

                for (uint j = 0; j < Ny; j++)
                {
                    var y = dy * j;
                    var h1 = (h0 - zetaX) * (1 - (y - y1) * (y - y2) / zy);
                    zeta[i, j] = h1 + zetaX;
                    h[i, j] = 0.1f;
                    u[i, j] = 0;
                    v[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                inputBoundary[0, j] = true;
            }
            for (int j = 1; j < Ny - 1; j++)
            {
                outputBoundary[Nx - 1, j] = true;
            }
        } // initialConditionX_ParabolicGradientForm_SW() 

        /// <summary>
        /// Граничные условия для потока вдоль X параболической формы с наклонным дном
        /// </summary>
        public void borderConditionX_ParabolicGradientForm_SW()
        {
            // Условие на стенках канала
            for (uint i = 1; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }
            // Условие на входе канала
            for (uint j = 0; j < Ny; j++)
            {
                var h = this.h[0, j];
                var u = 0f;
                if (h > MEM.Error2)
                {
                    u = 5;
                }
                var v = 0f;
                Q[0, j].X = h;
                Q[0, j].Y = h * u;
                Q[0, j].Z = h * v;
            }
        } // void borderConditionX_ParabolicGradientForm_SW()


        /// <summary>
        /// Начальные условия для потока вдоль Y
        /// </summary>
        public void initialConditionY()
        {
            // область до скачка
            //var angle = 1 * (float)Math.PI / 180.0f;

            //// область после скачка
            //for (uint i = 0; i < Nx; i++) {
            //    for (uint j = 0; j < Ny; j++) {
            //        zeta[i, j] = 1.0f + (1.0f - dy * j) * (float)Math.Sin(angle);
            //        u[i, j] = 0;
            //        v[i, j] = 0.0f;
            //        h[i, j] = 0.5f; // 2.0f - zeta[i, j];
            //    }
            //}

            //for (uint i = 1; i < Nx - 1; i++) {
            //    for (uint j = 1; j < Ny - 1; j++) {
            //        regionSolver[i, j] = true;
            //    }
            //}
            //for (int i = 1; i < Nx - 1; i++) {
            //    inputBoundary[i, 0] = true;
            //    outputBoundary[i, Ny - 1] = true;
            //}

            // область до скачка
            for (int i = 0; i < Nx; i++)
            {
                for (uint j = 0; j <= Ny / 2; j++)
                {
                    u[i, j] = 0f;
                    v[i, j] = 0;
                    h[i, j] = 10f;
                    zeta[i, j] = 0;
                }
            }

            // область после скачка
            for (int i = 0; i < Nx; i++)
            {
                for (int j = Ny / 2 + 1; j < Ny; j++)
                {
                    u[i, j] = 0;
                    v[i, j] = 0;
                    h[i, j] = 1f;
                    zeta[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                    map[i, j] = NodeType.Solve | NodeType.Wet;
                }
            }
            for (int i = 1; i < Nx - 1; i++)
            {
                inputBoundary[i, 0] = true;
                map[i, 0] = NodeType.InputBordary;
                outputBoundary[i, Ny - 1] = true;
                map[i, Ny - 1] = NodeType.OutputBordary;
            }

        } // initialConditionY() 

        /// <summary>
        /// Граничные условия для потока вдоль Y
        /// </summary>
        public void borderConditionY()
        {
            //var h = 0.1f; // 2.0f - zeta[0, 0];
            //var u = 0.0f;
            //var v = 0.0f;

            //// Условие на стенках канала
            //for (uint j = 1; j < Ny; j++) {
            //    Q[0, j] = Q[1, j];
            //    Q[Nx - 1, j] = Q[Nx - 2, j];
            //}
            //// Условие на входе канала
            //for (uint i = 0; i < Nx; i++) {
            //    Q[i, 0].X = h;
            //    Q[i, 0].Y = h * u;
            //    Q[i, 0].Z = h * v;
            //}

            // Условие на входе канала
            for (uint i = 0; i < Nx; i++)
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            }

            //var t = new Vector3(h, h * u, h * v);
            //  Условие на стенках канала
            for (uint j = 1; j < Ny - 1; j++)
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
        } // void borderConditionY()

        /// <summary>
        /// Начальные условия для потока вдоль Y трапециевидной формы
        /// </summary>
        public void initialConditionY_Trapeziform()
        {
            for (uint i = 0; i < Nx; i++)
            {
                for (uint j = 0; j < Ny; j++)
                {
                    if (i < Nx * 0.3)
                    {
                        zeta[i, j] = -1f / 6f * dx * i + 3f / 10f * 1f / 6f;
                    }
                    else if (i < Nx * 0.7)
                    {
                        zeta[i, j] = 0;
                    }
                    else
                    {
                        zeta[i, j] = 1f / 6f * dx * i - 7f / 10f * 1f / 6f;
                    }
                    h[i, j] = 1 - zeta[i, j];
                    u[i, j] = Qh / h[i, j];
                    v[i, j] = 0;
                }
            }

            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (uint i = 1; i < Nx - 1; i++)
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                }
            }
            for (int i = 1; i < Nx - 1; i++)
            {
                inputBoundary[i, 0] = true;
                outputBoundary[i, Ny - 1] = true;
            }
        } // initialConditionY_Trapeziform() 

        /// <summary>
        /// Граничные условия для потока вдоль Y трапециевидной формы
        /// </summary>
        public void borderConditionY_Trapeziform()
        {
            // Условие на стенках канала
            for (uint j = 1; j < Ny; j++)
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            }
            // Условие на входе канала
            for (uint i = 0; i < Nx; i++)
            {
                var h = 1 - zeta[i, 0];
                var u = Qh / h;
                var v = 0f;
                Q[i, 0].X = h;
                Q[i, 0].Y = h * u;
                Q[i, 0].Z = h * v;
            }
        } // void borderConditionY_trapeziform()

        /// <summary>
        /// Начальные условия для задачи о растекание водного столба
        /// </summary>
        public void initialCondition_CylinderDecay()
        {
            // область до скачка
            Parallel.For(0, Nx, (i) =>
            {
                for (uint j = 0; j < Ny; j++)
                {
                    h[i, j] = 0.5f;
                    u[i, j] = 0;
                    v[i, j] = 0;
                }
            });

            // область скачка
            var ic = Nx / 2;
            var jc = Ny / 2;
            double rh = Lx / 10;
            //var r = (int)(10 / dx);
            var r = (int)(rh / dx);
            for (int i = ic - r; i <= ic + r; i++)
            {
                for (int j = jc - r; j <= jc + r; j++)
                {
                    var x = i - ic;
                    var y = j - jc;
                    if (x * x + y * y <= r * r)
                    {
                        u[i, j] = 0;
                        v[i, j] = 0;
                        h[i, j] = 2f;
                    }
                }
            }

            Parallel.For(1, Nx - 1, (i) =>
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    regionSolver[i, j] = true;
                    map[i, j] = NodeType.SolveWet;
                }
            });

        } // initialCondition_CylinderDecay() 

        /// <summary>
        /// Граничные условия для задачи о растекание водного столба
        /// </summary>
        public void borderCondition_CylinderDecay()
        {
            // Условие на стенках канала
            Parallel.For(1, Ny, (j) =>
            {
                Q[0, j] = Q[1, j];
                Q[Nx - 1, j] = Q[Nx - 2, j];
            });

            Parallel.For(0, Nx, (i) =>
            {
                Q[i, 0] = Q[i, 1];
                Q[i, Ny - 1] = Q[i, Ny - 2];
            });

        } // void borderCondition_CylinderDecay()

        #endregion
    }
}
