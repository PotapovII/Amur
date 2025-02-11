//---------------------------------------------------------------------------
//                          ПРОЕКТ  "River"
//              создано  :   17.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
//                          27.12.2024 Потапов И.И.
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.ABaseTask
{
    using System;
    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.ChannelProcess;
    using MeshGeneratorsLib.TapeGenerator;
    /// <summary>
    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class RiverEmpty1XDTest01 : ARiverAnyEmpty<RiverEmptyParamsCircle>, IRiver
    {
        double L = 30;
        double H = 5;
        double x1 = 0.25;
        double x0 = 15;
        double Azeta0 = 0.5;
        double a = 1.2;
        double[] Velosity = null;
        /// <summary>
        ///  Сетка для расчета донных деформаций
        /// </summary>
        public override IMesh BedMesh() { return new TwoMesh(x, zeta0); }
        /// <summary>
        /// координаты дна по оси х
        /// </summary>
        protected double[] x = null;
        /// <summary>
        /// фиктивная свободная поверхность
        /// </summary>
        protected double[] Eta = null;
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public RiverEmpty1XDTest01() : this(new RiverEmptyParamsCircle()) { }

        public RiverEmpty1XDTest01(RiverEmptyParamsCircle p) : base(p, TypeTask.streamX1D)
        {
            name = "Прокси для тестовой задачи размыва дна";
            Version = "River2D 27.12.2024"; // "RiverFunctionX1D 19.04.2022"; 
        }

        public override void DefaultCalculationDomain(uint testTaskID = 0)
        {
            double tf = 0.5;
            MEM.Alloc(Params.CountKnots, ref x);
            MEM.Alloc(Params.CountKnots, ref zeta0);
            MEM.Alloc(Params.CountKnots, ref tauX);
            MEM.Alloc(x.Length, ref Velosity);

            double dx = L / (Params.CountKnots - 1);
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i * dx;
                double arg = ((x[i] - (x0 + x1)) / a);
                tauX[i] = 1.0 / (1.0 - 2 * Azeta0 / tf * (x[i] - x0) / (a*a) * Math.Exp(- arg*arg));
            }
            for (int i = 0; i < x.Length; i++)
                Velosity[i] = Math.Sign(tauX[i]) * Math.Sqrt(Math.Abs(tauX[i]) / rho_w);

            MEM.Alloc(x.Length, ref Eta, H);
            mesh = TapeMeshGenerator.CreateMesh(x, zeta0, Eta);
            Set(mesh, algebra);
            eTaskReady = ETaskStatus.TaskReady;
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            base.SetZeta(zeta, bedErosion);
            mesh = TapeMeshGenerator.CreateMesh(x, zeta0, Eta);
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            double[] rezult = null;
            TapeMeshGenerator.Convert2DFrom1D(ref rezult, tauX);
            sp.Add("Касательное напряжение", rezult);
            sp.AddCurve("Касательное напряжение", x, tauX);
        }
        /// <summary>
        /// Получение полей придонных касательных напряжений и давления/свободной поверхности по контексту задачи
        /// усредненных на конечных элементах
        /// </summary>
        /// <param name="tauX">придонное касательное напряжение по х</param>
        /// <param name="tauY">придонное касательное напряжение по у</param>
        /// <param name="P">придонных давления/свободная поверхности потока, по контексту задачи</param>
        public override void GetTau(ref double[] tauX, ref double[] tauY, ref double[] P, ref double[][] CS, StressesFlag sf = StressesFlag.Nod)
        {
            try
            {
                MEM.Alloc(x.Length, ref tauX);
                MEM.Copy(ref tauX, this.tauX);
                CS = null;
                tauY = null;
                P = null;
            }
            catch (Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverEmpty1XDTest01(Params);
        }
    }
}
