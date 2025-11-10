//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//---------------------------------------------------------------------------
//                 кодировка : 02.02.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1YD
{
    using System;
    using System.IO;
    

    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;
    using NPRiverLib.APRiver1YD.Params;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.BedLoad;
    using CommonLib.Function;
    using CommonLib.ChannelProcess;

    using MeshGeneratorsLib.StripGenerator;
    using System.Linq;
    using CommonLib.Geometry;

    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class CSRiverProxy_1YD : APRiverFEM1YD
    {
        /// Нормальная скорость на WL
        /// </summary>
        public IDigFunction VelosityUx = null;
        /// <summary>
        /// Радиальная скорость на WL
        /// </summary>
        public IDigFunction VelosityUy = null;
        /// <summary>
        /// Конструктор
        /// </summary>
        public CSRiverProxy_1YD() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public CSRiverProxy_1YD(RSCrossParams p) : base(p)
        {
            Version = "CSRiverProxy_1YD 02.02.2025";
            name = "Гравитационный (прокси) поток Ux";
        }

        #region Переопределение абстрактные методы IRiver, ITask ...
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной 
        /// поверхности по контексту задачи усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] _tauX, ref double[] _tauY, ref double[] P,
                ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            _tauX = tau;
        }

        /// <summary>
        /// Расчет полей глубины и скоростей на текущем шаге по времени
        /// </summary>
        public override void SolverStep()
        {
            int flagErr = 0;
            int i = 0, j = 0, nod = 0;
            double u_star = 0, H = 0;
            try
            {
                // расчет уровня свободной поверхности реки
                double waterLevel0 = waterLevel;
                CalkWaterLevel();
                flagErr = 1;
                // определение расчетной области потока и построение КЭ сетки
                if (MEM.Equals(waterLevel0, waterLevel, 0.001) != true ||
                    Erosion != EBedErosion.NoBedErosion)
                    UpDateDomain();
                double[] X = mesh.GetCoords(0);
                double[] Y = mesh.GetCoords(1);
                
                if (eTaskReady == ETaskStatus.TaskReady)
                {
                    flagErr = 2;
                    // расчет осевой скорости потока
                    for (i = 0; i < riverGates.Length; i++)
                    {
                        if (riverGates[i].Length == 1)
                        {
                            nod = riverGates[i][0];
                            Ux[nod] = 0;
                            TauZ[nod] = 0;
                        }    
                        else
                        {
                            int nodH = riverGates[i][0];
                            int nod0 = riverGates[i][riverGates[i].Length-1];
                            if(Y[nodH] - Y[nod0] < 0)
                            {
                                int e = nodH;
                                nodH = nod0;
                                nod0 = e;
                            }

                            //if(Y[nodS] < Y[nodE])
                            H = Y[nodH] - Y[nod0];
                            double ks = 0.01;
                            //if (H<0.5)
                            //    ks = 0.001;
                            //if (H < 0.1)
                            //    ks = 0.0001;
                            ks = 0.0001 + 0.02 * Math.Exp(-1 / H);
                            u_star = Math.Sqrt( GRAV * H * Params.J );
                            for (j = 0; j < riverGates[i].Length; j++)
                            {
                                nod = riverGates[i][j];
                                double h = H - (Y[nod]- Y[nod0]);
                                TauY[nod] = 0;
                                TauZ[nod] = rho_w * GRAV * h * Params.J;
                                double z = Y[nod] - Y[nod0];
                                
                                if (z > ks)
                                    Ux[nod] = u_star / kappa_w * Math.Log(z / ks);
                                else
                                    Ux[nod] = 0;
                            }
                        }
                    }
                    CalkTauBed(ref tau);
                    flagErr = 3;
                    time += dtime;
                }
            }
            catch (Exception ex)
            {
                string ErrorName = RiverError.ErrorName(flagErr);
                Logger.Instance.Error(ErrorName, "ASectionalRiverTask");
                Logger.Instance.Exception(ex);
            }
        }
        ///// <summary>
        ///// Нахождение полей напряжений и их сглаживание по методу Галеркина
        ///// </summary>
        //protected override void SolveTaus()
        //{
        //    uint ch = 0;
        //    try
        //    {
        //        // Производные от функций форм по x
        //        double[][] dNdx = wMesh.GetdNdx();
        //        // Производные от функций форм по y
        //        double[][] dNdy = wMesh.GetdNdy();
        //        // площади КЭ
        //        double[] SVE = wMesh.GetElemS();
        //        double[] SE = wMesh.GetS();
        //        double[] u = { 0, 0, 0 };
        //        double S;
        //        uint[] knots = { 0, 0, 0 };
        //        double[] tmpTausZ = new double[mesh.CountElements];
        //        double[] tmpTausY = new double[mesh.CountElements];
        //        for (int i = 0; i < TauZ.Length; i++)
        //        {
        //            TauY[i] = 0;
        //            TauZ[i] = 0;
        //        }
        //        for (uint i = 0; i < mesh.CountElements; i++)
        //        {
        //            ch = i;
        //            mesh.ElementKnots(i, ref knots);
        //            mesh.ElemValues(Ux, i, ref u);
        //            // получить среднюю вязкость на КЭ
        //            double Mu = (eddyViscosity[knots[0]] + eddyViscosity[knots[1]] + eddyViscosity[knots[2]]) / 3;
        //            double[] b = dNdx[i];
        //            S = SE[i];
        //            double Ey = u[0] * b[0] + u[1] * b[1] + u[2] * b[2];
        //            tmpTausY[i] = Mu * Ey;
        //            TauY[(int)knots[0]] += S * tmpTausY[i] / 3;
        //            TauY[(int)knots[1]] += S * tmpTausY[i] / 3;
        //            TauY[(int)knots[2]] += S * tmpTausY[i] / 3;
        //        }
        //        for (int i = 0; i < TauZ.Length; i++)
        //        {
        //            TauY[i] /= SVE[i];
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Instance.Exception(ex);
        //    }
        //}

        /// <summary>
        /// Расчет напряжений и концентрации на дне канала
        /// и их интерполяция на расчетную сетку дна
        /// </summary>
        /// <param name="bottom_x">координаты Х узлов донно - береговой поверхности</param>
        /// <param name="bottom_y">координаты У узлов донно - береговой поверхности</param>
        /// <param name="tauBed">напряжение на дне/стенках</param>
        /// <param name="concentBed">концентрация на дне/стенках</param>
        protected override void CalkTauBed(ref double[] tauBed)
        {
            // массив касательных напряжений в узлах донно - береговой поверхности
            MEM.Alloc(bottom_x.Length - 1, ref tauBed);
            //SolveTaus();
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(0);
            // количество узлов на нижней границе области
            MEM.Alloc(bounds.Length - 1, ref tau_xy);
            MEM.Alloc(bounds.Length - 1, ref tau_xz);
            MEM.Alloc(bounds.Length - 1, ref Coord);
            // пробегаем по граничным узлам и записываем для них Ty, Tz 
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < bounds.Length - 1; i++)
            {
                uint i0 = bounds[i];
                uint i1 = bounds[i + 1];
                tau_xz[i] = 0.5 * (TauZ[i0] + TauZ[i1]);
                tau_xy[i] = 0.5 * (TauY[i0] + TauY[i1]);
                Coord[i] = 0.5 * (xx[i0] + xx[i1]);
            }
            double left = xx[bounds[0]];
            double right = xx[bounds[bounds.Length - 1]];
            // формируем сплайны напряжений в натуральной координате
            IDigFunction tauZ = new DigFunction(Coord, tau_xz);
            IDigFunction tauY = new DigFunction(Coord, tau_xy);
            for (int i = 0; i < tauBed.Length; i++)
            {
                double xtau = 0.5 * (bottom_x[i] + bottom_x[i + 1]);
                if (xtau < left || right < xtau)
                    tauBed[i] = 0;
                else
                {
                    double L = HPoint.Length(bottom_x[i + 1], bottom_y[i + 1], bottom_x[i], bottom_y[i]);
                    if (L > 0)
                    {
                        double CosG = (bottom_x[i + 1] - bottom_x[i]) / L;
                        double SinG = -(bottom_y[i + 1] - bottom_y[i]) / L;
                        tauBed[i] = tauZ.FunctionValue(xtau) * CosG +
                                    tauY.FunctionValue(xtau) * SinG;
                    }
                    else
                        tauBed[i] = 0;
                    if (double.IsNaN(tauBed[i]) == true)
                    {
                        tauBed[i] = 0;
                        throw new Exception("Расчет напряжений выполнен на вертикальных стенках");
                    }
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tauBed.Max();
            /// Сдвиговые напряжения средние
            tauMid = tauBed.Sum() / tauBed.Length;

        }

        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            CreateCalculationDomain();
        }

        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            base.AddMeshPolesForGraphics(sp);
        }
        #endregion
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            // генерация сетки
            bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
            CrossStripMeshOption op = new CrossStripMeshOption();
            op.AxisOfSymmetry = axisOfSymmetry;

            if (meshGenerator == null)
                meshGenerator = SMGManager.GetMeshGenerator(Params.typeMeshGenerator, op);
            mesh = meshGenerator.CreateMesh(ref WetBed, ref riverGates, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            MEM.Alloc(Params.CountKnots, ref tau, "tau");
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
            // создание общего враппера сетки для задачи
            Set(mesh, algebra);

            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Обновить расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected void UpDateDomain()
        {
            try
            {
                // генерация сетки
                bool axisOfSymmetry = Params.axisSymmetry == 1 ? true : false;
                CrossStripMeshOption op = new CrossStripMeshOption();
                op.AxisOfSymmetry = axisOfSymmetry;
                if (meshGenerator == null)
                    meshGenerator = new HStripMeshGenerator(op);
                mesh = meshGenerator.CreateMesh(ref WetBed, ref riverGates, waterLevel, bottom_x, bottom_y);
                right = meshGenerator.Right();
                left = meshGenerator.Left();
                // получение ширины ленты для алгебры
                int WidthMatrix = (int)mesh.GetWidthMatrix();
                // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
                algebra = new AlgebraLUTape((uint)mesh.CountKnots, WidthMatrix, WidthMatrix);
                // создание общего враппера сетки для задачи
                Set(mesh, algebra);
                if (mesh.CountKnots != Ux.Length)
                {
                    MEM.Alloc(mesh.CountKnots, ref Ux, "Ux");
                    // память под напряжения в области
                    MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
                    MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
                    MEM.Alloc(Params.CountKnots, ref tau, "tau");
                    unknowns.Clear();
                    unknowns.Add(new Unknown("Скорость Ux", Ux, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
                    unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));
                }
                eTaskReady = ETaskStatus.TaskReady;
            }
            catch(Exception ex)
            {
                Logger.Instance.Error(ex.Message, "CSRiverProxy_1YD.UpDateDomain");
            }
        }


        /// <summary>
        /// Загрузка геометрии области из файла
        /// </summary>
        /// <param name="file"></param>
        public override void LoadData(StreamReader file)
        {
            base.LoadData(file);
            CreateCalculationDomain();
        }
        /// <summary>
        /// Загрузка задачи из тестовых данных
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions)
        {
            base.LoadData(crossFunctions);
            CreateCalculationDomain();
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new CSRiverProxy_1YD(Params);
        }
        /// <summary>
        /// Создает экземпляр класса конвертера
        /// </summary>
        /// <returns></returns>
        public override IOFormater<IRiver> GetFormater()
        {
            return new TaskReader1YD_RvY();
        }
    }
}
