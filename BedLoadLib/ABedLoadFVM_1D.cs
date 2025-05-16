//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 07.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          27.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;

    using MemLogLib;
    using CommonLib;
    using CommonLib.Physics;
  
    [Serializable]
    public abstract class ABedLoadFVM_1D<TParam> :
        ABedLoad<TParam> where TParam : class, ITProperty<TParam>
    {
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        public new string VersionData() => versionData;
        protected string versionData = "ABedLoadFVM_1D 27.03.2025";
        /// <summary>
        /// Расход наносов по механизмам движения донного материала
        /// </summary>
        protected const int idxTransit = 0, idxZeta = 1, idxAll = 2, idxPress = 3;

        #region  Свойства
        protected double us, AB;
        /// <summary>
        /// Расход наносов на КО
        /// </summary>
        protected double[][] Gs = null;
        /// <summary>
        /// Расход градиентов наносов на КО
        /// </summary>
        protected double[][] dGs = null;

        /// <summary>
        /// критическое напряжение на КО
        /// </summary>
        public double[] tau0Elem = null;
        /// <summary>
        /// отметки дна в центрах КО
        /// </summary>
        protected double[] zetaElem = null;
        #endregion
        #region Краевые условия
        /// <summary>
        /// транзитный расход на входе
        /// </summary>
        public double Gtran_in = 0;
        /// <summary>
        /// транзитный расход на выходе
        /// </summary>
        public double Gtran_out = 0;
        #endregion
        #region Служебные переменные
        /// <summary>
        /// Количество расчетных подобластей
        /// </summary>
        protected int N;
        /// <summary>
        ///  косинус гамма - косинус угола между 
        ///  нормалью к дну и вертикальной осью
        /// </summary>
        protected double[] CosGamma = null;

        protected double[] G0 = null;
        protected double[] A = null;
        protected double[] B = null;
        protected double[] C = null;
        /// <summary>
        /// Поток взвешенных наносов по Х в улах
        /// </summary>        
        protected double[] GCx = null;

        protected double[] CE = null;
        protected double[] CW = null;
        protected double[] CP = null;
        protected double[] S = null;

        protected double[] AE = null;
        protected double[] AW = null;
        protected double[] AP = null;
        protected double[] AP0 = null;

        protected double[] ps = null;
        /// <summary>
        /// Флаг для определения сухого-мокрого дна
        /// </summary>
        public int[] DryWetEelm = null;
        /// <summary>
        /// длина расчетной области
        /// </summary>
        protected double L;
        /// <summary>
        /// <summary>
        /// Флаг отладки
        /// </summary>
        public int debug = 0;
        ///// <summary>
        ///// Поле сообщений о состоянии задачи
        ///// </summary>
        //public string Message = "Ok";
        #endregion
        #region Рабочие массивы
        /// <summary>
        /// массив координаты узлов
        /// </summary>
        public double[] x = null;
        /// <summary>
        /// координаты центров КО
        /// </summary>
        protected double[] X = null;
        #endregion

        public ABedLoadFVM_1D(TParam p, TypeTask tt) : base(p, tt) { }
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public override void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions)
        {
            double tanphi = SPhysics.PHYS.tanphi;
            if (mesh.CountElements == 0) return;
            base.SetTask(mesh, Zeta0, Roughness, BConditions);
            this.x = mesh.GetCoords(0);
            if (mesh.GetCoords(1) != null)
                this.Zeta0 = mesh.GetCoords(1);
            this.N = Count - 1;
            this.L = x[N] - x[0];
            // узловые массивы
            MEM.Alloc(N, ref A);
            MEM.Alloc(N, ref B);
            MEM.Alloc(N, ref C);
            MEM.Alloc(N, ref G0);
            MEM.Alloc(N, ref dZeta);
            MEM.Alloc(N, ref CosGamma);
            MEM.Alloc<int>(N, ref DryWetEelm);
            MEM.Alloc(N, ref GCx);

            MEM.Alloc<int>(Count, ref DryWet);
            MEM.Alloc(Count, ref Zeta);
            MEM.Alloc(Count, ref ps);
            MEM.Alloc(Count, ref CE);
            MEM.Alloc(Count, ref CW);
            MEM.Alloc(Count, ref CP);
            MEM.Alloc(Count, ref S);
            MEM.Alloc(Count, ref AE);
            MEM.Alloc(Count, ref AW);
            MEM.Alloc(Count, ref AP);
            MEM.Alloc(Count, ref AP0);

            MEM.Alloc(mesh.CountElements, ref X);
            MEM.Alloc(mesh.CountElements, ref tau0Elem);
            MEM.Alloc(mesh.CountElements, ref zetaElem);

            avalanche = new Avalanche1DX_Old(mesh, tanphi, 0.6);

            eTaskReady = ETaskStatus.TaskReady;
        }

        /// <summary>
        /// Вычисление числа Курранта
        /// </summary>
        /// <returns></returns>
        public double Kurant()
        {
            double G1 = SPhysics.PHYS.G1;
            double tau0 = SPhysics.PHYS.tau0;
            double kurant;
            double minL = double.MaxValue;
            for (int i = 0; i < x.Length - 1; i++)
            {
                double dx = x[i + 1] - x[i];
                if (dx < minL)
                    minL = dx;
            }
            double minD = G1 * tau0 * Math.Sqrt(tau0);
            kurant = minL / (2.0 * minD);
            return kurant;
        }
        /// <summary>
        /// Масштаб расхода от уклона дна
        /// </summary>
        /// <param name="Gx">(d zeta/d x)/tan phi</param>
        /// <param name="Gy">(d zeta/d y)/tan phi</param>
        /// <returns></returns>
        public double DRate(double Gx, double Gy = 0)
        {
            double scaleMin = 0.05;
            double scale = 1 - Gx * Gx - Gy * Gy;
            if (scale <= scaleMin) // область осыпания склона
                return 20; // 1.0 / scaleMin;
            else
                return 1.0 / scale;
        }
        /// <summary>
        /// Конвертация элементных данных в узловые
        /// </summary>
        /// <param name="P_elem"></param>
        /// <param name="P_node"></param>
        /// <param name="scale"></param>
        protected void ConvertElemToNode(double[] P_elem, ref double[] P_node, double scale = 1)
        {
            if (P_elem == null)
            {
                for (int j = 1; j < N; j++)
                    P_node[j] = 0;
            }
            else
            if (P_elem.Length != P_node.Length)
            {
                for (int j = 1; j < N; j++)
                    P_node[j] = 0.5 * (P_elem[j] + P_elem[j - 1]) * scale;
                P_node[0] = (2 * P_elem[1] - P_elem[2]) * scale;
                P_node[N] = (2 * P_elem[N - 1] - P_elem[N - 2]) * scale;
            }
        }
    }
}
