//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD.River2DSW
{
    using System;

    using MeshLib;
    using MemLogLib;
    using CommonLib;
    using CommonLib.Points;

    [Serializable]
    public abstract class APRiverFEM_2XYD : APRiver2XYD<ParamsRiver_2XYD>, IRiver
    {

        #region Локальыне данные задачи
        /// <summary>
        /// квадратурные точки для численного интегрирования КЭ
        /// </summary>
        protected NumInegrationPoints pIntegration;
        /// <summary>
        /// квадратурные точки для численного интегрирования граничных КЭ
        /// </summary>
        protected NumInegrationPoints bpIntegration;
        /// <summary>
        /// вектор приращений поля h u v за одну итерацию
        /// </summary>
        protected double[] result;
        #endregion

        #region Константы
        /// <summary>
        /// количество узлов на КЭ
        /// </summary>
        protected int CountElementKnots = 3;
        /// <summary>
        /// количество узлов на КЭ
        /// </summary>
        protected int CountBoundEKnots = 2;
        /// <summary>
        /// количество неизвестных в узле
        /// </summary>
        protected int CountUnknow = 3;
        /// <summary>
        /// размерность пространства для трехузлового КЭ с тремя степенями свободы
        /// </summary>
        protected int CountSpace = 9;
        /// <summary>
        /// размерность пространства для двухузлового КЭ с тремя степенями свободы
        /// </summary>
        protected int CountBoundSpace = 6;
        #endregion

        #region Локальные переменные
        /// <summary>
        /// Узлы КЭ
        /// </summary>
        protected uint[] knots = { 0, 0, 0 };
        protected uint[] bknots = { 0, 0 };
        /// <summary>
        /// координаты узлов КЭ
        /// </summary>
        protected double[] x = { 0, 0, 0 };
        protected double[] y = { 0, 0, 0 };
        protected double[] bx = { 0, 0 };
        protected double[] by = { 0, 0 };
        /// <summary>
        /// локальная матрица жесткости
        /// </summary>
        protected double[][] KMatrix = null;
        /// <summary>
        /// локальная матрица массы
        /// </summary>
        protected double[][] SMatrix = null;
        /// <summary>
        /// локальная матрица Якоби задачи
        /// </summary>
        protected double[][] JMatrix = null;
        /// <summary>
        /// локальная правая часть СЛАУ
        /// </summary>
        protected double[] FRight = null;
        /// <summary>
        /// адреса ГУ
        /// </summary>
        protected double[] Uo = null;
        protected double[] Vo = null;
        protected double[][] Ax = null;
        protected double[][] Ay = null;
        protected double[][] A2 = null;
        protected double[][] UI = null;
        protected double[][] wx, wy, I;
        protected double[][] dxx, dyx, dxy, dyy;
        protected double[][] dnf;

        #region Массивы для метода по обновлению скоростей
        /// <summary>
        /// глобальная диагональная матрица массы
        /// </summary>
        protected double[] M = null;
        /// <summary>
        /// глобальные правые части
        /// </summary>
        protected double[] FH = null;
        protected double[] FU = null;
        protected double[] FV = null;
        protected double[][] ME = null;
        protected double[] FHE = null;
        protected double[] FUE = null;
        protected double[] FVE = null;
        #endregion
        #endregion

        public APRiverFEM_2XYD(ParamsRiver_2XYD p) : base(p) { }

        /// <summary>
        /// Установка КЭ сетки и решателя задачи
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="algebra">решатель задачи</param>
        public override void Set(IMesh mesh, IAlgebra a = null)
        {
            base.Set(mesh, algebra);

            pIntegration = new NumInegrationPoints();
            bpIntegration = new NumInegrationPoints();

            MEM.Alloc2DClear(CountElementKnots, ref A2);
            MEM.Alloc2DClear(CountElementKnots, ref Ax);
            MEM.Alloc2DClear(CountElementKnots, ref Ay);
            MEM.Alloc2DClear(CountElementKnots, ref UI);

            MEM.Alloc2DClear(CountElementKnots, ref wx);
            MEM.Alloc2DClear(CountElementKnots, ref wy);
            MEM.Alloc2DClear(CountElementKnots, ref dnf);

            MEM.Alloc2DClear(CountElementKnots, ref dxx);
            MEM.Alloc2DClear(CountElementKnots, ref dyx);
            MEM.Alloc2DClear(CountElementKnots, ref dxy);
            MEM.Alloc2DClear(CountElementKnots, ref dyy);

            MEM.Alloc2DClear(CountElementKnots, ref I);

            MEM.AllocClear(CountElementKnots, ref Uo);
            MEM.AllocClear(CountElementKnots, ref Vo);

            MEM.Alloc(CountElementKnots, ref FHE);
            MEM.Alloc(CountElementKnots, ref FUE);
            MEM.Alloc(CountElementKnots, ref FVE);
            MEM.Alloc(CountElementKnots, CountElementKnots, ref ME);
        }

        /// <summary>
        /// анализ полученного решения
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="result"></param>
        protected void AnalysisResult(IRiverNode[] nodes, double[] result)
        {
            double varsum = 0.0;
            // норма поправок
            double uchange = 0.0;
            //  поправока решения
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].h += result[i * CountUnknow];
                nodes[i].qx += result[i * CountUnknow + 1];
                nodes[i].qy += result[i * CountUnknow + 2];
                // Сумма квадратов нормы ошибки (поправки) по каждой переменной
                varsum += nodes[i].h * nodes[i].h +
                          nodes[i].qx * nodes[i].qx +
                          nodes[i].qy * nodes[i].qy;
            }
            // Квадрат нормы ошибки (поправки)
            for (int i = 0; i < nodes.Length; i++)
                uchange += result[i] * result[i];
            if (varsum > 0.0)
                uchange = Math.Sqrt(uchange / varsum);
            else
                uchange = 0.0;
            // если изменение слишком велико, отклонить итерацию и записать информацию в файл журнала
            if (uchange > 1.25 * Params.maxSolСorrection)
            {
                Logger.Instance.Info("отклонить итерацию  t = " + time.ToString());
                // откат по времени
                time -= dtime;
                // уменьшаем шаг по времени
                Params.dtTrend = 0.5 * Params.maxSolСorrection / uchange;
                // сбрасываем значения в узлах к значениям
                // на предыдущем временном шаге
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i].h = nodes[i].h0;
                    nodes[i].qx = nodes[i].qx0;
                    nodes[i].qy = nodes[i].qy0;
                }
            }
            else
            {
                // делаем сдвиг полученного решение на предыдущий слой
                ShiftingTemporaryLayers(nodes);
                // обновление скоростей
                UpdateVelocities();
                // коэффициент = (по умолчанию 0,05) / uchange
                Params.dtTrend = Params.maxSolСorrection / uchange;
                if (Params.dtTrend > 1.5)
                    Params.dtTrend = 1.5;
            }
            // обновление шага по времени
            dtime *= Params.dtTrend;
            if (dtime > Params.dtmax)
                dtime = Params.dtmax;
        }

        /// <summary>
        /// функция для установки старых значений(uo, uoo)
        /// </summary>
        public void ShiftingTemporaryLayers(IRiverNode[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                IRiverNode elemNodes = nodes[i];
                // порядок сохранения глубины и расходов различный
                elemNodes.h00 = elemNodes.h0;// h
                elemNodes.h0 = elemNodes.h;  // h
                elemNodes.qx00 = elemNodes.qx0;// qx
                elemNodes.qx0 = elemNodes.qx;  // qx
                elemNodes.qy00 = elemNodes.qy0;// qy
                elemNodes.qy0 = elemNodes.qy;  // qy
            }
        }

        ///// <summary>
        ///// Получение полей глубины и скоростей на текущем шаге по времени
        ///// </summary>
        ///// <param name="h">глубины</param>
        ///// <param name="u">скорость по х</param>
        ///// <param name="v">скорость по у</param>
        public void GetValues_Zeta_Eta_h_qx_qy(IRiverNode[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                _Zeta[i] = nodes[i].zeta;
                _h[i] = nodes[i].h;
                _Eta[i] = _Zeta[i] + _h[i];
                _qx[i] = nodes[i].qx;
                _qy[i] = nodes[i].qy;
            }
        }

        #region Вычисление ЛМЖ и ПЧ
        /// <summary>
        /// создание/очистка ЛМЖ и ЛПЧ ...
        /// </summary>
        /// <param name="cu">количество неизвестных</param>
        protected virtual void ClearLocal()
        {
            MEM.Alloc2DClear(CountSpace, ref KMatrix);
            MEM.Alloc2DClear(CountSpace, ref SMatrix);
            MEM.Alloc2DClear(CountSpace, ref JMatrix);
            MEM.AllocClear(CountSpace, ref FRight);
        }
        #endregion

        #region абстрактные методы 
        public abstract void UpdateVelocities();
        #endregion
    }

}
