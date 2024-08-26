//---------------------------------------------------------------------------
//                          ПРОЕКТ  "River"
//              создано  :   17.09.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverLib
{
    using System;
    using System.Collections.Generic;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;
    using CommonLib.ChannelProcess;

    /// <summary>
    ///  ОО: Определение класса ARiverEmpty - заглушки задачи для 
    ///  расчета полей скорости и напряжений в речном потоке
    /// </summary>
    [Serializable]
    public class RiverFunctionX1DTest01 : ARiverEmpty
    {
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public override string Name { get => "прокси ГД: функция напряжений - стационар"; }
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public override string VersionData() => "RiverFunctionX1D 19.04.2022"; 


        protected List<double> gammaList = new List<double>();
        protected List<double> timeList = new List<double>();

        // Field3D pole = null;
        double L = 30;
        double x1=0.25;
        double x0 = 15;
        double Azeta0 = 0.5;
        double a=1.2;

        int NN;
        double[] Velosity = null;
        double[] tau = null;
        double[] xx = null;
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverFunctionX1DTest01(new RiverStreamParams());
        }
        public RiverFunctionX1DTest01(RiverStreamParams p) : base(p)
        {
            NN = 500;
            _typeTask = TypeTask.streamX1D;
            double tf = 0.5;
            MEM.Alloc(NN, ref x);
            MEM.Alloc(NN, ref y);
            MEM.Alloc(NN, ref tau);
            MEM.Alloc(NN, ref zeta0);
            double dx = L / (NN - 1);
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i * dx;
                double arg = ((x[i] - (x0 + x1)) / a);
                tau[i] = 1.0 / (1.0 - 2 * Azeta0 / tf * (x[i] - x0) / (a*a) * Math.Exp(- arg*arg));
            }
            mesh = new TwoMesh(x, y);
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
                for (int i = 0; i < x.Length; i++)
                    tauX[i] = tau[i];
                tauY = null;
                P = null;
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public override void AddMeshPolesForGraphics(ISavePoint sp)
        {
            double rho_w = SPhysics.rho_w;
            //if (gammaList.Count > 2)
            //    sp.AddCurve("Циркуляция", timeList.ToArray(), gammaList.ToArray());
            //if(tau==null || Velosity==null)
            MEM.Alloc(NN, ref Velosity);
            for (int i = 0; i < x.Length; i++)
                Velosity[i] = Math.Sign(tau[i]) * Math.Sqrt(Math.Abs(tau[i]) / rho_w);
            sp.AddCurve("Касательное напряжение", x, tau);
            sp.AddCurve("Касательная скорость потока дне", x, Velosity);
            //sp.AddCurve("Цилиндр", fxСircle, fyСircle);
            //sp.AddCurve("Обтекаемая форма", fxB, fyB);
            //sp.AddCurve("Дно", fxW, fyW);
        }
        /// <summary>
        /// Установка адаптеров для КЭ сетки и алгебры
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="algebra"></param>
        public override void Set(IMesh mesh, IAlgebra algebra = null)
        {
            TwoMesh m = null;
            if (mesh != null)
                m = mesh as TwoMesh;
            if (m != null)
            {
                if (m != null)
                {
                    xx = m.GetCoords(0);
                    NN = xx.Length;
                }
            }
        }
        /// <summary>
        /// Установка новых отметок дна
        /// </summary>
        /// <param name="zeta"></param>
        public override void SetZeta(double[] zeta, EBedErosion bedErosion)
        {
            Erosion = bedErosion;
            this.y = zeta;
        }
    }
}
