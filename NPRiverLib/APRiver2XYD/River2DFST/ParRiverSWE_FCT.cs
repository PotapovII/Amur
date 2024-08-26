//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//       исходный код решателя создан П.С.Тимош, аспирант, 2021-2022
//          (Вычислительный центр Дальневосточного отделения РАН)
//---------------------------------------------------------------------------
//               кодировка адаптации : 15.10.2022 Потапов И.И.
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver2XYD.River2DFST
{
    using System;
    using System.Threading.Tasks;
    
    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;
    
    /// <summary>
    ///  ОО: Определение класса ParRiverSWE_FCT - для расчета полей расходов, 
    ///  скорости, глубин и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class ParRiverSWE_FCT : RiverSWE_FCT_2XYD
    {
        public ParRiverSWE_FCT(SWE_FCTParams_2XYD p) : base(p)
        {
            name = "задача мелкой воды 2XYD FCT Parallel";
            Version = "ParRiverSWE_FCT 25.07.2024";
        }
        public ParRiverSWE_FCT() : this(new SWE_FCTParams_2XYD ())
        {
        }
        /// <summary>
        /// Конфигурация задачи по умолчанию (тестовые задачи)
        /// </summary>
        /// <param name="testTaskID">номер задачи по умолчанию</param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            InitTask();
        }
        #region IRiver
        ///// <summary>
        ///// КЭ сетка
        ///// </summary>
        //public IMesh Mesh() => mesh.Clone();
        ///// <summary>
        ///// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        ///// </summary>
        //public IUnknown[] Unknowns() => unknowns;
        //protected IUnknown[] unknowns =
        //{
        //    new Unknown("Удельный расход потока по х", null, TypeFunForm.Form_2D_Triangle_L1),
        //    new Unknown("Удельный расход потока по у", null, TypeFunForm.Form_2D_Triangle_L1),
        //    new Unknown("Осредненная по глубине скорость х", null, TypeFunForm.Form_2D_Triangle_L1),
        //    new Unknown("Осредненная по глубине скорость у", null, TypeFunForm.Form_2D_Triangle_L1),
        //    new Unknown("Глубина потока", null, TypeFunForm.Form_2D_Triangle_L1)
        //};
        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            diffuseBorder();
            // Обход внутренних узлов области
            Parallel.For(1, Nx - 1, (i) =>
            {
                //for (int i = 1; i < Nx - 1; i++) {
                //Vector3 R = new Vector3();
                for (uint j = 1; j < Ny - 1; ++j)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        diffuseInner((uint)i, (uint)j);

                        var Fr = F[i + 1, j];
                        var Fl = F[i - 1, j];
                        var Gr = G[i, j + 1];
                        var Gl = G[i, j - 1];

                        var Gy = 0.5 * (Gr - Gl) / dy;
                        var Fx = 0.5 * (Fr - Fl) / dx;

                        //if (map[i, j].HasFlag(NodeType.Bordary) == true) {
                        //    if (h[i + 1, j] < MEM.Error2) {
                        //        //var dz = zeta[i + 1, j] - zeta[i, j];
                        //        //var dx1 = h[i, j] * dx / dz;
                        //        Fr = F[i, j];
                        //        Fx = (Fr - Fl) / dx;
                        //    } else if (h[i - 1, j] < MEM.Error2) {
                        //        //var dz = zeta[i + 1, j] - zeta[i, j];
                        //        //var dx1 = h[i, j] * dx / dz;

                        //        Fl = F[i, j];
                        //        Fx = (Fr - Fl) / dx;
                        //    }
                        //    if (h[i, j + 1] < MEM.Error2) {
                        //        //var dz = zeta[i, j + 1] - zeta[i, j];
                        //        //var dy1 = h[i, j] * dy / dz;

                        //        Gr = G[i, j];
                        //        Gy = (Gr - Gl) / dy;
                        //    } else if (h[i, j - 1] < MEM.Error2) {
                        //        //var dz = zeta[i, j - 1] - zeta[i, j];
                        //        //var dy1 = h[i, j] * dy / dz;

                        //        Gl = G[i, j];
                        //        Gy = (Gr - Gl) / dy;
                        //    }
                        //}

                        // Схема центральных разностей
                        Q[i, j] = Q[i, j] - dtime * (Fx + Gy);

                        //if (h[i, j] > MEM.Error2) {
                        //    var Cs = (double)Math.Pow(h[i, j], 1f / 6f) / roughness;
                        //    var lambda = Physics.g / Cs / Cs;
                        //    var sqrtU2V2 = (double)Math.Sqrt(u[i, j] * u[i, j] + v[i, j] * v[i, j]);
                        //    tau_x[i, j] = lambda * sqrtU2V2 * u[i, j];
                        //    tau_y[i, j] = lambda * sqrtU2V2 * v[i, j];
                        //} else {
                        //    tau_x[i, j] = 0;
                        //    tau_y[i, j] = 0;
                        //}
                        //R.Y = tau_x[i, j];
                        //R.Z = tau_y[i, j];
                        //// Учет трения дна
                        //Q[i, j] -= dtime * R;
                    }
                }
            }
            );// по пространству


            if (!zeroCheck_Vector3Z(Q))
            {
                ;
            }

            borderCondition();

            if (!zeroCheck_Vector3Z(Q))
            {
                ;
            }

            // коррекция решения
            FCT();

            if (!zeroCheck_Vector3Z(Q))
            {
                ;
            }

            borderCondition();



            // Обновление дивергентных векторов
            Parallel.For(0, Nx, (i) =>
            {
                //for (int i = 0; i < Nx; i++) { 
                for (uint j = 0; j < Ny; j++)
                {
                    //if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet)) {
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

                    F[i, j].X = qx[i, j];
                    //F[i, j].Y = qx[i, j] * qx[i, j] / h[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                    F[i, j].Y = qx[i, j] * u[i, j] + 0.5f * SPhysics.GRAV * h[i, j] * (h[i, j] + zeta[i, j]);
                    F[i, j].Z = qx[i, j] * v[i, j];

                    G[i, j].X = qy[i, j];
                    G[i, j].Y = qy[i, j] * u[i, j];
                    //G[i, j].Z = qy[i, j] * qy[i, j] / h[i, j] + 0.5f * Physics.g * h[i, j] * (h[i, j] + zeta[i, j]);
                    G[i, j].Z = qy[i, j] * v[i, j] + 0.5f * SPhysics.GRAV * h[i, j] * (h[i, j] + zeta[i, j]);
                }
                //}
            }
            );

            if (!zeroCheck_Vector3Z(Q))
            {
                ;
            }

            if (!equalYFillCheck_Vector3(G))
            {
                ;
            }

            //var hFree = freeWater();
            ////TODO Сделать учет впадин 
            //Parallel.For(0, Nx, (i) => {
            //    for (uint j = 0; j < Ny; j++) {
            //        if (map[i, j].HasFlag(NodeType.Input) || map[i, j].HasFlag(NodeType.Output)) {
            //            continue;
            //        }

            //        if (h[i, j] > MEM.Error2) {
            //            map[i, j] = NodeType.SolveWet;
            //        } else if (h[i, j] > 0) {
            //            map[i, j] = NodeType.BordaryWet;
            //        } else if (h[i, j] > -MEM.Error2) {
            //            map[i, j] = NodeType.BordaryDry;
            //            h[i, j] = hFree - zeta[i, j];
            //        } else {
            //            map[i, j] = NodeType.SolveDry;
            //            h[i, j] = hFree - zeta[i, j];
            //        }
            //    }
            //});
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new ParRiverSWE_FCT(Params);
        }
        #endregion


        /// <summary>
        /// Инициализация начальных значений
        /// </summary>
        protected override void initialization()
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
                    F[i, j].Y = qx[i, j] * u[i, j] + 0.5f * SPhysics.GRAV * h[i, j] * (h[i, j] + zeta[i, j]);
                    F[i, j].Z = qx[i, j] * v[i, j];

                    G[i, j].X = qy[i, j];
                    G[i, j].Y = qy[i, j] * u[i, j];
                    G[i, j].Z = qy[i, j] * v[i, j] + 0.5f * SPhysics.GRAV * h[i, j] * (h[i, j] + zeta[i, j]);
                }
            });
        }
        /// <summary>
        /// Расчет диффузии на границе
        /// </summary>
        protected override void diffuseBorder()
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
        /// Коррекция потока
        /// </summary>
        protected override void FCT()
        {
            Parallel.For(0, Ny - 1, (j) =>
            {
                var alpha = dtime * (u[0, j] + u[1, j]) / dx;
                var mu = ETA + 0.25f * ET2 * alpha * alpha;
                fAD[0, j] = mu * (Q[1, j] - Q[0, j]);
            });

            Parallel.For(0, Nx - 1, (i) =>
            {
                var epsilon = dtime * (v[i, 0] + v[i, 1]) / dy;
                var kappa = ETA + 0.25f * ET2 * epsilon * epsilon;
                gAD[i, 0] = kappa * (Q[i, 1] - Q[i, 0]);
            });

            // расчет антидифузионного потока
            Parallel.For(1, Nx - 1, (i) =>
            {
                for (uint j = 1; j < Ny - 1; j++)
                {
                    if (regionSolver[i, j] || map[i, j].HasFlag(NodeType.Wet))
                    {
                        var alpha = dtime * (u[i, j] + u[i + 1, j]) / dx;
                        var epsilon = dtime * (v[i, j] + v[i, j + 1]) / dy;
                        var mu = ETA + ET2 * alpha * alpha;
                        var kappa = ETA + ET2 * epsilon * epsilon;

                        fAD[i, j] = mu * (Q[i + 1, j] - Q[i, j]);
                        gAD[i, j] = kappa * (Q[i, j + 1] - Q[i, j]);

                        Q[i, j] = Q[i, j] + fD[i, j] - fD[i - 1, j] + gD[i, j] - gD[i, j - 1];

                        DxQ[i - 1, j] = Q[i, j] - Q[i - 1, j];
                        DyQ[i, j - 1] = Q[i, j] - Q[i, j - 1];
                    }
                }
            });

            Parallel.For(0, Nx, (i) =>
            {
                DyQ[i, Ny - 2] = Q[i, Ny - 1] - Q[i, Ny - 2];
            });
            Parallel.For(0, Ny, (j) =>
            {
                DxQ[Nx - 2, j] = Q[Nx - 1, j] - Q[Nx - 2, j];
            });
            // Ограничение антидиффузионных членов
            Parallel.For(1, Nx - 1, (i) =>
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
            });
        }
    }
}
