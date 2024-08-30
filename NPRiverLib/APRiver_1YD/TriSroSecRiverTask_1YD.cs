//---------------------------------------------------------------------------
//                          ПРОЕКТ  "DISER"
//                  создано  :   9.03.2007 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//            перенесено с правкой : 06.12.2020 Потапов И.И. 
//            создание родителя : 21.02.2022 Потапов И.И. 
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 09.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_1YD
{
    using MemLogLib;
    using AlgebraLib;
    using GeometryLib;

    using NPRiverLib.IO;

    using CommonLib;
    using CommonLib.IO;
    using CommonLib.Points;

    using System;
    using System.Linq;

    using FEMTasksLib.FESimpleTask;
    using NPRiverLib.APRiver_1YD.Params;
    using MeshGeneratorsLib.StripGenerator;
    using MeshLib.CArea;

    /// <summary>
    ///  ОО: Определение класса TriSroSecRiverTask_1YD - расчет полей скорости, вязкости 
    ///       и напряжений в живом сечении потока методом КЭ (симплекс элементы)
    /// </summary>
    [Serializable]
    public class TriSroSecRiverTask_1YD : APRiverFEM_1YD
    {
        
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSroSecRiverTask_1YD() : this(new RSCrossParams()) { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public TriSroSecRiverTask_1YD(RSCrossParams p) : base(p)
        {
            Version = "TriSroSecRiverTask 21.02.2022";
            name = "Поток в створе канала (КЭ)";
        }
        /// <summary>
        /// Расчет касательных напряжений на дне канала
        /// </summary>
        /// <param name="mesh">сетка</param>
        /// <param name="TauZ">напражения на сетке</param>
        /// <param name="TauY">напражения на сетке</param>
        /// <param name="xv">координаты Х узлов КО</param>
        /// <param name="yv">координаты У узлов КО</param>
        protected override double[] TausToVols(in double[] xv, in double[] yv)
        {
            // расчет напряжений Txy  Txz
            taskU.SolveTaus(ref TauY, ref TauZ, U, eddyViscosity);
            // граничные узлы на нижней границе области
            uint[] bounds = mesh.GetBoundKnotsByMarker(0);
            // количество узлов на нижней границе области
            TSpline tauSplineZ = new TSpline();
            TSpline tauSplineY = new TSpline();
            MEM.Alloc(bounds.Length, ref tauY);
            MEM.Alloc(bounds.Length, ref tauZ);
            MEM.Alloc(bounds.Length, ref Coord);
            // пробегаем по граничным узлам и записываем для них Ty, Tz T
            double[] xx = mesh.GetCoords(0);
            for (int i = 0; i < bounds.Length - 1; i++)
            {
                tauZ[i] = 0.5 * (TauZ[bounds[i]] + TauZ[bounds[i + 1]]);
                tauY[i] = 0.5 * (TauY[bounds[i]] + TauY[bounds[i + 1]]);
                Coord[i] = 0.5 * (xx[bounds[i]] + xx[bounds[i + 1]]);
            }
            double left = xx[bounds[0]];
            double right = xx[bounds[bounds.Length - 1]];
            // формируем сплайны напряжений в натуральной координате
            tauSplineZ.Set(tauZ, Coord, (uint)bounds.Length);
            tauSplineY.Set(tauY, Coord, (uint)bounds.Length);
            // массив касательных напряжений
            MEM.Alloc(xv.Length - 1, ref tau);
            for (int i = 0; i < tau.Length; i++)
            {
                double xtau = 0.5 * (xv[i] + xv[i + 1]);
                if (xtau < left || right < xtau)
                    tau[i] = 0;
                else
                {
                    double L = HPoint.Length(xv[i + 1], yv[i + 1], xv[i], yv[i]);
                    double CosG = (xv[i + 1] - xv[i]) / L;
                    double SinG = (yv[i + 1] - yv[i]) / L;
                    tau[i] = tauSplineZ.Value(xtau) * CosG +
                             tauSplineY.Value(xtau) * SinG;
                    if (double.IsNaN(tau[i]) == true)
                        throw new Exception("Mesh for RiverStreamTask");
                }
            }
            // Сдвиговые напряжения максимум
            tauMax = tau.Max();
            /// Сдвиговые напряжения средние
            tauMid = tau.Sum() / tau.Length;
            return tau;
        }
        /// <summary>
        /// Создать расчетную область
        /// </summary>
        /// <param name="right"></param>
        /// <param name="left"></param>
        protected override void CreateCalculationDomain()
        {
            // генерация сетки
            if (meshGenerator == null)
                //meshGenerator = new HStripMeshGeneratorTri();
                meshGenerator = new HStripMeshGenerator();
            mesh = meshGenerator.CreateMesh(ref WetBed, waterLevel, bottom_x, bottom_y);
            right = meshGenerator.Right();
            left = meshGenerator.Left();
            // получение ширины ленты для алгебры
            int WidthMatrix = (int)mesh.GetWidthMatrix();
            // TO DO подумать о реклонировании algebra если размер матрицы не поменялся 
            algebra = new AlgebraGaussTape((uint)mesh.CountKnots, (uint)WidthMatrix);

            MEM.Alloc(mesh.CountKnots, ref eddyViscosity, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref eddyViscosity0, "eddyViscosity");
            MEM.Alloc(mesh.CountKnots, ref U, "U");
            // память под напряжения в области
            MEM.Alloc(mesh.CountKnots, ref TauY, "TauY");
            MEM.Alloc(mesh.CountKnots, ref TauZ, "TauZ");
            MEM.Alloc(Params.CountKnots, ref tau, "tau");
            unknowns.Clear();
            unknowns.Add(new Unknown("Скорость Ux", U, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Турб. вязкость", eddyViscosity, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xy", TauY, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений T_xz", TauZ, TypeFunForm.Form_2D_Rectangle_L1));
            unknowns.Add(new CalkPapams("Поле напряжений", TauY, TauZ, TypeFunForm.Form_2D_Rectangle_L1));

            if (taskU == null)
                taskU = new FEPoissonTaskTri(mesh, algebra);
            else
                taskU.SetTask(mesh, algebra);

            wMesh = new MeshWrapperCrossCFGTri(mesh);
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new TriSroSecRiverTask_1YD(Params);
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
