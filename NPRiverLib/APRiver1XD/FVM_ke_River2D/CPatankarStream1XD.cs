//---------------------------------------------------------------------------
//               Класс CFDTask предназначен для запуска на решение 
//                      задачи турбулентного теплообмена
//                              Потапов И.И.
//                        - (C) Copyright 2015 -
//                          ALL RIGHT RESERVED
//                               31.07.15
//---------------------------------------------------------------------------
//   Реализация библиотеки для решения задачи турбулентного теплообмена
//                  методом контрольного объема
//---------------------------------------------------------------------------
//                  добавлена иерархия параметров 12.03.2021              
//---------------------------------------------------------------------------
//      добавлена адаптация вывода результатов в форматах библиотек 
//                              MeshLib, RenderLib;
//                                  19.12.2021
//---------------------------------------------------------------------------
//               Реверс в NPRiverLib 16.02.2024 Потапов И.И.
//---------------------------------------------------------------------------

namespace NPRiverLib.APRiver_1XD.River2D_FVM_ke
{
    using MemLogLib;
    using CommonLib;
    using AlgebraLib;

    using System;

    /// <summary>
    /// ОО: Реализация решателя для задачи о расчете профильного турбулентного потока 
    /// в формулировке RANS k-e методом контрольных объемов
    /// </summary>
    [Serializable]
    public class CPatankarStream1XD : APatankarRiver1XD
    {
        #region методы IRiver2D
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="ps"></param>
        public CPatankarStream1XD() : this(new PatankarParams1XD()) { }
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="ps"></param>
        public CPatankarStream1XD(PatankarParams1XD ps) : base(ps)
        {
            name = "Турбулентнй поток 2D (k-e) rho_w = const";
            Version = "River2D FVM ke 16.02.2024";
        }
        /// <summary>
        /// Создает экземпляр класса River2DFV_rho_const
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new CPatankarStream1XD(Params);
        }

        #endregion
        #region Расчет коэффициентов
        /// <summary>
        /// Инициализация данных (праметров мат модели),
        /// коэффициентов релаксации схемы,
        /// начальных и граничных условий задачи,
        /// расчет массового расхода потока
        /// rho_w = const
        /// </summary>
        public override void OnInitialData()
        {
            if (Nx == 0 || Ny == 0) return;
            // Коэффициенты реласксации модели
            relax[0] = 0.5f; // u-velocity
            relax[1] = 0.5f; // v-velocity
            relax[2] = 0.8f; // p_correction
            relax[3] = 1.0f; // temperature
            relax[4] = 0.4f; // turb kin energy
            relax[5] = 0.4f; // dissipation of turb kin energy
            relax[6] = 0.5f; // gamma
            double bx;
            // Инициализация данных
            for (int jj = 0; jj < Ny; jj++)
                if (y[0][jj] > Params.LV)
                {
                    jdxTube = jj; break;
                }
            topTube = 0;
            bottomTube = 0;
            for (int i = 0; i < Nx; i++)
            {
                for (int j = 0; j < Ny; j++)
                {
                    if (i == 0)
                    {
                        // на стенке 
                        v[i][j] = 0;
                        t[i][j] = 0;
                        tke[i][j] = 0;
                    }
                    else
                    {
                        if (Params.streamInsBoundary == true || j < 2)
                        {
                            bx = x[i][j];
                            if (Params.Wen1 >= bx)
                            {
                                v[i][j] = Params.V1_inlet;   // основная скорость
                                if (Params.TemperOrConcentration == true)
                                    t[i][j] = 0;
                                else
                                    t[i][j] = Params.t1;   // основная температура потока
                            }
                            if (Params.Wen2 + Params.Wen1 > bx && Params.Wen1 <= bx)
                            {
                                if (bottomTube == 0)
                                    bottomTube = i - 1;

                                v[i][j] = Params.V2_inlet;    // скорость набегания
                                if (Params.TemperOrConcentration == true)
                                    t[i][j] = 0;
                                else
                                    t[i][j] = Params.t2;    // температура потока набегания
                            }
                            if (Params.Wen3 + Params.Wen2 + Params.Wen1 > bx && Params.Wen2 + Params.Wen1 <= bx)
                            {
                                if (topTube == 0)
                                    topTube = i;
                                v[i][j] = Params.V3_inlet;    // скорость набегания
                                if (Params.TemperOrConcentration == true)
                                    t[i][j] = 0;
                                else
                                    t[i][j] = Params.t3;    // температура потока набегания
                            }
                            //  v[i][j] = GetU(bx);
                        }
                    }
                    pc[i][j] = 0;
                    p[i][j] = 0;

                    //tke[i][j] = (double)(0.0005 * v[i][j] * v[i][j]);
                    //dis[i][j] = (double)(100.001 * tke[i][j] * tke[i][j]);
                    tke[i][j] = (double)(Coeff_k * v[i][j] * v[i][j]);
                    dis[i][j] = (double)(Coeff_e * tke[i][j] * tke[i][j]);
                    // кинематическая тв
                    double nu0 = (double)(Cmu * tke[i][j] * tke[i][j] / (dis[i][j] + 1e-30));
                    mut[i][j] = nu0 + nu;
                }
            }
            //LOG.Print("V", v);
            // ---------------------------------------------------------
            // граничные условия для функции тока
            // вся область и дно
            for (int i = 0; i < Nx; i++)
                for (int j = 0; j < Ny; j++)
                    phi[i][j] = 0;
            // втекание потока
            for (int i = 1; i < Nx; i++)
                phi[i][0] = 0.5 * (v[i][0] + v[i - 1][0]) * Hx[i - 1][0] + phi[i - 1][0];
            // верхняя крышка
            for (int j = 0; j < Ny; j++)
                phi[Nx - 1][j] = phi[Nx - 1][0];
            // ---------------------------------------------------------

            for (int i = 0; i < Nx; i++)
            {
                // вход: профиль
                bcV.BC[BCond.S_index][i] = 1;
                bcV.Value[BCond.S_index][i] = v[i][0];

                // p' = 0 на контуре
                bcP.BC[BCond.S_index][i] = 1;
                bcP.Value[BCond.S_index][i] = 0;
                bcP.BC[BCond.N_index][i] = 1;
                bcP.Value[BCond.N_index][i] = 0;

                // вход: T профиль
                bcT.BC[BCond.S_index][i] = 1;
                bcT.Value[BCond.S_index][i] = t[i][0];

                // вход: T профиль
                bcK.BC[BCond.S_index][i] = 1;
                bcK.Value[BCond.S_index][i] = tke[i][0];

                // вход: T профиль
                bcE.BC[BCond.S_index][i] = 1;
                bcE.Value[BCond.S_index][i] = dis[i][0];
            }

            // расчет массового расхода (жидкость течет вертикально)
            flowin = 0;
            for (int i = 1; i < imax; i++)
                flowin += rho_w * v[i][1] * Hx[i][0];
        }

        protected void BCV(int i, int j, double V)
        {
            Ae[i][j] = 0;
            Aw[i][j] = 0;
            As[i][j] = 0;
            An[i][j] = 0;
            Ap[i][j] = 1;
            sc[i][j] = V;
        }


        /// <summary>
        /// Расчет дискретных аналогов и их коэффициентов
        /// </summary>
        /// <param name="pSolve"></param>
        /// <param name="time"></param>
        /// <param name="IndexTask"></param>
        public override void CalkCoef(ITPSolver pSolve, ITPSolver pSolveR, double time, int IndexTask)
        {
            int i, j, ist, jst;
            double flow, diff, agam, flow_p, flow_m, gam_p, gam_m;
            double fx, fy;
            double rel;
            double vol;
            if (Params.shiftV == true && Params.LV > 0 && jdxTube == 0)
            {
                for (int jj = 0; jj < y[0].Length; jj++)
                if (y[0][jj] > Params.LV)
                {
                    jdxTube = jj; break;
                }
                for (i = 0; i < Nx; i++)
                {
                    double bx = x[i][jdxTube];
                    if (Params.Wen2 + Params.Wen1 > bx && Params.Wen1 <= bx)
                    {
                        if (bottomTube == 0)
                            bottomTube = i - 1;
                    }
                    if ((Params.Wen3 + Params.Wen2 + Params.Wen1 > bx && Params.Wen2 + Params.Wen1 <= bx))
                    {
                        if (topTube == 0)
                            topTube = i;
                        break;
                    }
                }
            }
            // граничные условия
            // BCGetU0();
            // Вилка по решаемым задачам
            switch (IndexTask)
            {
                case 0: // Коэффициенты для расчета скорости u - поперек потока
                    #region 0 u - скорость поперек потока
                    {
                        // Расчет турбулентной вязкости потока
                        OnLookingGama(IndexTask);
                        
                        ERR.INF_NAN("gam для u / River2DFV_rho_const.CalkCoef()", gam);

                        // Смешение сетки для скорости u (на 1 по х)
                        ist = 2;
                        jst = 1;
                        // Релаксация для текущей задачи
                        rel = 1 - relax[IndexTask];
                        // Расчет горизонтальных коэффициентов Aw, Ae для скорости u
                        // цикл вдоль потока
                        for (j = jst; j < jmax; j++)
                        {
                            // Конвективный поток по Х
                            flow = rho_w * (u[ist - 1][j] + u[ist][j]) * Hy[ist - 1][j] / 2;
                            // Диффузионный поток по Х
                            diff = gam[1][j] * Hy[ist - 1][j] / Hx[ist - 1][j];
                            // Расчет коэффициента Aw по Потанкару на границе
                            Aw[ist][j] = DifFlow(diff, flow) + Math.Max(0, flow);
                            // цикл поперек потока
                            for (i = ist; i < imax; i++)
                            {
                                // Конвективный поток по Х
                                flow = rho_w * (u[i][j] + u[i + 1][j]) * Hy[i][j] / 2;
                                // Диффузионный поток X
                                diff = gam[i][j] * Hy[i][j] / Hx[i][j];
                                // Расчет коэффициента Aw по Потанкару
                                Aw[i + 1][j] = DifFlow(diff, flow) + Math.Max(0, flow);
                                // Расчет коэффициента Ae по Потанкару
                                Ae[i][j] = Aw[i + 1][j] - flow;
                            }
                        }
                        // Расчет вертикальных коэффициентов An, As для скорости u
                        for (i = ist; i < imax; i++)
                        {
                            // Расчет конвективного потока по Y справа и слева от центра грани
                            flow_p = rho_w * v[i][jst] * Hx[i][jst - 1];
                            flow_m = rho_w * v[i - 1][jst] * Hx[i - 1][jst - 1];
                            // Осреднение конвективного потока 
                            flow = (flow_p + flow_m) / 2;
                            // Диффузионный поток Y
                            diff = (gam[i][jst - 1] * Hx[i][jst] + gam[i - 1][jst - 1] * Hx[i - 1][jst]) / Dy[i][jst];
                            // Расчет коэффициента As по Потанкару на границе
                            As[i][jst] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                            for (j = jst; j < jmax; j++)
                            {
                                if (j < jmax - 1)
                                {
                                    // Балансировка
                                    fy = Hy[i][j + 1] / Dy[i][j + 1] / 2;
                                    // Средняя проводимость -
                                    agam = gam[i][j] * fy + gam[i][j + 1] * (1 - fy);
                                    // Конвективный поток + 
                                    flow_p = v[i][j + 1] * Hx[i][j + 1] * rho_w;
                                    // Средняя проводимость +
                                    gam_p = gam[i][j] * gam[i][j + 1] / agam * Hx[i][j + 1];
                                    // Средняя проводимость -
                                    agam = gam[i - 1][j] * fy + gam[i - 1][j + 1] * (1 - fy);
                                    // Конвективный поток -
                                    flow_m = v[i - 1][j + 1] * Hx[i - 1][j + 1] * rho_w;
                                    // Средняя проводимость -
                                    gam_m = gam[i - 1][j] * gam[i - 1][j + 1] / agam * Hx[i - 1][j + 1];
                                    // Осреднение конвективного потока
                                    flow = (flow_p + flow_m) / 2;
                                    // Вычисление диффузионного потока
                                    diff = (gam_p + gam_m) / Dy[i][j + 1];
                                }
                                else // Граница
                                {
                                    // Конвективный поток + 
                                    flow_p = rho_w * v[i][jmax] * Hx[i][jmax - 1];
                                    // Конвективный поток -
                                    flow_m = rho_w * v[i - 1][jmax] * Hx[i - 1][jmax - 1];
                                    // Осреднение конвективного потока
                                    flow = (flow_p + flow_m) / 2;
                                    // Диффузионный поток Y
                                    diff = (gam[i][jmax] * Hx[i][jmax - 1] + gam[i - 1][jmax] * Hx[i - 1][jmax - 1]) / Dy[i - 1][jmax];
                                }
                                // Расчет вертикальных коэффициентов
                                As[i][j + 1] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                                An[i][j] = As[i][j + 1] - flow;
                            }
                        }
                        // Расчет правых частей
                        OnSource(ist, jst, IndexTask);
                        // Вычисление центральных коэффициентов схемы Ap и Ap0, 
                        // используемых для расчеток невязок давления pc,
                        // очистка текущего значения невязки pc
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                // Вычисление площади КО
                                vol = Dx[i][j] * Hy[i][j];
                                // Вычисление центральных коэффициентов схемы Ap0 и Ap, 
                                Ap0[i][j] = rho_w * vol / dtime;
                                Ap[i][j] = (Ap0[i][j] + Aw[i][j] + Ae[i][j] + An[i][j] + As[i][j] - sp[i][j] * vol) / relax[IndexTask];
                                // Вычисление весовых множетелей для поправки скорости ue = ue^0*du(pc_P-pc_E)  
                                du[i][j] = Hy[i][j] / Ap[i][j];
                                // Вычисление источников (правой части и временного члена)
                                sc[i][j] = sc[i][j] * vol + (Ap[i][j] * rel + Ap0[i][j]) * u[i][j];
                                // Вычисление источников (градиента давления)
                                sc[i][j] += (p[i - 1][j] - p[i][j]) * Hy[i][j];
                            }
                        }
                        if (Params.shiftV == true && Params.LV > 0)
                        {
                            for (i = 0; i < Nx; i++)
                            {
                                double bx = x[i][jdxTube];
                                //if (Params.Wen1 >= bx && Params.V1_inlet > 0)
                                //    BCV(i, jdxTube, Params.V1_inlet); // основная скорость
                                if ((Params.Wen2 + Params.Wen1 > bx && Params.Wen1 <= bx) && Params.V2_inlet > 0)
                                {
                                    for (int jj = 0; jj <= jdxTube; jj++)
                                        BCV(i, jj, 0);   // скорость набегания
                                }
                                //if ((Params.Wen3 + Params.Wen2 + Params.Wen1 > bx && Params.Wen2 + Params.Wen1 <= bx))
                                //{
                                //    if(Params.V3_inlet > 0)
                                //        BCV(i, jdxTube, Params.V3_inlet); ;    // скорость набегания
                                //}
                            }
                            for (int jj = 0; jj <= jdxTube; jj++)
                                BCV(bottomTube, jj, 0);   // скорость набегания
                            for (int jj = 0; jj <= jdxTube; jj++)
                                BCV(topTube, jj, 0);   // скорость набегания
                        }
                        //Решение САУ для получения скорости u
                        bcU.SetBCondition(ist, jst, imax, jmax, ref u);
                        bool result = pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcU, ref u, 1);
                        if (result == false)
                            pSolveR.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcU, ref u, 1);

                        ERR.INF_NAN("u / River2DFV_rho_const.CalkCoef()", u);
                    }
                    #endregion 0
                    break;
                case 1: // Коэффициенты для расчета скорости v
                    #region 1 Коэффициенты для расчета скорости v
                    {
                        // Расчет вязкости
                        OnLookingGama(IndexTask);
                        
                        ERR.INF_NAN("gam для v / River2DFV_rho_const.CalkCoef()", gam);
                        // Смешение сетки для скорости v (на 1 по y)
                        ist = 1;
                        jst = 2;
                        // Релаксация для текущей задачи
                        rel = 1 - relax[IndexTask];
                        // Расчет вертикальных коэффициентов As, An для скорости v
                        for (i = ist; i < imax; i++)
                        {
                            // Граница сверху
                            // Конвективный поток по Y
                            flow = rho_w * (v[i][jst - 1] + v[i][jst]) * Hx[i][jst - 1] / 2;
                            // Диффузионный поток по Y
                            diff = gam[i][jst - 1] * Hx[i][jst - 1] / Hy[i][jst - 1];
                            // Расчет коэффициента As для скорости v
                            As[i][jst] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                            for (j = jst; j < jmax; j++)
                            {
                                // Диффузионный поток по Y
                                flow = rho_w * (v[i][j] + v[i][j + 1]) * Hx[i][j] / 2;
                                // Диффузионный поток по Y
                                diff = gam[i][j] * Hx[i][j] / Hy[i][j];
                                // Расчет вертикальных коэффициентов As, An для скорости v
                                As[i][j + 1] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                                An[i][j] = As[i][j + 1] - flow;
                            }
                        }
                        // Расчет горизонтальных коэффициентов Ae, Aw для скорости v
                        for (j = jst; j < jmax; j++)
                        {
                            // Правая граница
                            // Конвективный поток +
                            flow_p = rho_w * u[ist][j] * Hy[ist - 1][j];
                            // Конвективный поток -
                            flow_m = rho_w * u[ist][j - 1] * Hy[ist - 1][j - 1];
                            // Средний конвективный поток
                            flow = (flow_p + flow_m) / 2;
                            // Диффузионный поток
                            diff = (gam[ist - 1][j] * Hy[ist - 1][j] + gam[ist - 1][j - 1] * Hy[ist - 1][j - 1]) / Dx[ist][j];
                            // Расчет коэффициента Aw для скорости v
                            Aw[ist][j] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                            for (i = ist; i < imax; i++)
                            {
                                if (i < imax - 1)
                                {
                                    // Балансировка
                                    fx = Hx[i + 1][j] / Dx[i + 1][j] / 2;
                                    // Средняя вязость по горизонтали
                                    agam = gam[i][j] * fx + gam[i + 1][j] * (1 - fx);
                                    // Конвективный поток +
                                    flow_p = rho_w * u[i + 1][j] * Hy[i][j];
                                    // Средне квадратичная вязкость по горизонтали +
                                    gam_p = gam[i][j] * gam[i + 1][j] / agam * Hy[i][j];
                                    // Средняя вязость по вертикали
                                    agam = gam[i][j - 1] * fx + gam[i + 1][j - 1] * (1 - fx);
                                    // Конвективный поток -
                                    flow_m = rho_w * u[i + 1][j - 1] * Hy[i][j - 1];
                                    // Средне квадратичная вязкость по горизонтали -
                                    gam_m = gam[i][j - 1] * gam[i + 1][j - 1] / agam * Hy[i][j - 1];
                                    // Средний конвективный поток
                                    flow = (flow_p + flow_m) / 2;
                                    // Средний диффузионный поток
                                    diff = (gam_p + gam_m) / Dx[i + 1][j];
                                }
                                else
                                {
                                    // Левая граница (стенка)
                                    // Конвективный поток +
                                    flow_p = rho_w * u[imax][j] * Hy[imax - 1][j];
                                    // Конвективный поток -
                                    flow_m = rho_w * u[imax][j - 1] * Hy[imax - 1][j - 1];
                                    // Средний конвективный поток
                                    flow = (flow_p + flow_m) / 2;
                                    // Диффузионный поток
                                    diff = (gam[imax][j] * Hy[imax - 1][j] + gam[imax][j - 1] * Hy[imax - 1][j - 1]) / Dx[imax][j];
                                }
                                Aw[i + 1][j] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                                Ae[i][j] = Aw[i + 1][j] - flow;
                            }
                        }
                        // Расчет правой части для v
                        OnSource(ist, jst, IndexTask);
                        
                        ERR.INF_NAN("gam / River2DFV_rho_const.CalkCoef()", gam);
                        // Вычисление центральных коэффициентов схемы Ap и Ap0, 
                        // используемых для расчеток невязок давления pc,
                        // очистка текущего значения невязки pc
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                // Вычисление площади КО
                                vol = Hx[i][j] * Dy[i][j];
                                // Вычисление центральных коэффициентов схемы Ap0 и Ap, 
                                Ap0[i][j] = rho_w * vol / dtime;
                                Ap[i][j] = (Ap0[i][j] + Aw[i][j] + Ae[i][j] + An[i][j] + As[i][j] - sp[i][j] * vol) / relax[IndexTask];
                                // Вычисление весовых множетелей для поправки скорости vn = vn^0*dv(pc_P-pc_N)  
                                dv[i][j] = Hx[i][j] / Ap[i][j];
                                // Вычисление источников (правой части и временного члена)
                                sc[i][j] = sc[i][j] * vol + (Ap[i][j] * rel + Ap0[i][j]) * v[i][j];
                                // Вычисление источников (градиента давления)
                                sc[i][j] += (p[i][j - 1] - p[i][j]) * Hx[i][j];
                            }
                        }
                        // Решение САУ для получения скорости v
                        bcV.SetBCondition(ist, jst, imax, jmax, ref v);
                        // Console.WriteLine("V ist = {0:F4}, jst = {1:F4}, imax = {2:F4}, jmax = {3:F4},", ist, jst, imax, jmax);
                        
                        if(Params.shiftV == true && Params.LV > 0)
                        {
                            for (i = 0; i < Nx; i++)
                            {
                                double bx = x[i][jdxTube];
                                //if (Params.Wen1 >= bx && Params.V1_inlet > 0)
                                //    BCV(i, jdxTube, Params.V1_inlet); // основная скорость
                                if ((Params.Wen2 + Params.Wen1 > bx && Params.Wen1 <= bx) && Params.V2_inlet > 0)
                                {
                                    for (int jj = 0; jj <= jdxTube; jj++)
                                        BCV(i, jj, Params.V2_inlet);   // скорость набегания
                                }
                                //if ((Params.Wen3 + Params.Wen2 + Params.Wen1 > bx && Params.Wen2 + Params.Wen1 <= bx))
                                //{
                                //    if(Params.V3_inlet > 0)
                                //        BCV(i, jdxTube, Params.V3_inlet); ;    // скорость набегания
                                //}
                            }
                            for (int jj = 0; jj <= jdxTube; jj++)
                                BCV(bottomTube, jj, 0);   // скорость набегания
                            for (int jj = 0; jj <= jdxTube; jj++)
                                BCV(topTube, jj, 0);   // скорость набегания
                        }

                        pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcV, ref v, 1);
                        ERR.INF_NAN("v / River2DFV_rho_const.CalkCoef()", v);
                    }
                    #endregion
                    break;
                case 2:
                    #region 2 расчет коэффициентов для вычисления поправки давления pc
                    {
                        ist = 1;
                        jst = 1;
                        rel = 1 - relax[IndexTask];
                        // для несжимаемых потоков - очистим источниковый член
                        for (i = ist; i < imax; i++)
                            for (j = jst; j < jmax; j++)
                                sc[i][j] = 0;
                        // расчет вертикальных потоков и коэффициентов
                        // бежим по горизонтали
                        for (i = ist; i < imax; i++)
                        {
                            // массовые потоки на границах
                            sc[i][jst] += rho_w * v[i][jst] * Hx[i][jst];
                            sc[i][jmax - 1] -= rho_w * v[i][jmax] * Hx[i][jmax - 1];

                            As[i][jst] = 0;
                            An[i][jmax - 1] = 0;
                            // для всех узлов по вертикали
                            // бежим по вертикали
                            for (j = jst; j < jmax - 1; j++)
                            {
                                // пропорция 
                                fy = Hy[i][j] / Dy[i][j] / 2;
                                // вертикальный поток черз грань
                                flow = rho_w * v[i][j + 1] * Hx[i][j];
                                // расчет источников через грань
                                sc[i][j] -= flow;
                                sc[i][j + 1] += flow;
                                // расчет южного коэффициента
                                As[i][j + 1] = rho_w * dv[i][j + 1];
                                // он же северный но со смещением в 1
                                An[i][j] = As[i][j + 1];
                            }
                        }
                        // расчет горизонтальных потоков и коэффициентов
                        for (j = jst; j < jmax; j++)
                        {
                            // массовые потоки на границах
                            sc[ist][j] += rho_w * u[ist][j] * Hy[ist][j];
                            sc[imax - 1][j] -= rho_w * u[imax][j] * Hy[imax - 1][j];

                            Aw[ist][j] = 0;
                            Ae[imax - 1][j] = 0;

                            for (i = ist; i < imax - 1; i++)
                            {
                                // пропорция 
                                fx = Hx[i][j] / Dx[i][j] / 2;
                                // горизонтальный поток черз грань
                                flow = rho_w * u[i + 1][j] * Hy[i][j];
                                sc[i][j] -= flow;
                                sc[i + 1][j] += flow;
                                // расчет западного коэффициента
                                Aw[i + 1][j] = rho_w * du[i + 1][j];
                                // он же восточный но со смещением в 1
                                Ae[i][j] = Aw[i + 1][j];
                            }
                        }
                        // Расчет максимальной невязки потоков на КО
                        smax = 0f;
                        // Вычисление центральных коэффициентов схемы Ap, 
                        // используемых для расчеток невязок давления pc,
                        // очистка текущего значения невязки pc
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                Ap[i][j] = Aw[i][j] + Ae[i][j] + An[i][j] + As[i][j];
                                pc[i][j] = 0;
                                smax = Math.Max(smax, Math.Abs(sc[i][j]));
                            }
                        }
                        // решение САУ для получения невязки pc
                        // bcP.SetBCondition(ist, jst, imax, jmax, ref pc);
                        // Console.WriteLine("p' ist = {0:F4}, jst = {1:F4}, imax = {2:F4}, jmax = {3:F4},", ist, jst, imax, jmax);
                        // MEM.Alloc2DClear(Nx, Ny,ref pc);
                        pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcP, ref pc, 1);
                        
                        ERR.INF_NAN("pc / River2DFV_rho_const.CalkCoef()", pc);

                        // Поправка давления и скоростей потока, с заданной релаксацией
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                // Поправка давления 
                                p[i][j] += pc[i][j] * relax[IndexTask];
                                // Поправка скорости u
                                if (i != ist)
                                    u[i][j] += du[i][j] * (pc[i - 1][j] - pc[i][j]);
                                // Поправка скорости v
                                if (j != jst)
                                    v[i][j] += dv[i][j] * (pc[i][j - 1] - pc[i][j]);
                            }
                        }
                    }
                    #endregion
                    break;
                case 6:// Расчет коэффициентов для вычисления функции тока
                    #region 6 Расчет коэффициентов для вычисления функции тока
                    ist = 1;
                    jst = 1;
                    rel = 1 - relax[IndexTask];
                    for (i = ist; i < imax; i++)
                    {
                        // граница сверху
                        // Конвективный поток
                        flow = 0;
                        // Диффузионный поток
                        diff = Hx[i][jst] / Dy[i][jst];
                        // Вычисление коэффициента
                        As[i][jst] = diff;
                        for (j = jst; j < jmax; j++)
                        {
                            // Диффузионный поток
                            if (j < jmax - 1)
                                diff = Hx[i][j] / Hy[i][j];
                            else
                                // Диффузионный поток
                                diff = Hx[i][jmax - 1] / Dy[i][jmax];
                            // Вычисление коэффициентов
                            As[i][j + 1] = diff;
                            An[i][j] = As[i][j + 1];
                        }
                    }
                    // Расчет горизонтальных коэффициентов Ae[i][j], Aw[i][j]
                    for (j = jst; j < jmax - 1; j++)
                    {
                        // граница справа (ось симметрии)
                        // Диффузионный поток
                        diff = Hy[ist - 1][j] / Dx[ist][j];
                        // Вычисление коэффициента
                        Aw[ist][j] = diff;
                        // В области
                        for (i = 1; i < imax; i++)
                        {
                            // Диффузионный поток
                            if (i < imax - 1)

                                diff = Hy[i][j] / Hx[i][j];
                            else
                                diff = Hy[i][j] / Dx[imax][j];
                            // Вычисление коэффициентов
                            Aw[i + 1][j] = diff;
                            Ae[i][j] = Aw[i + 1][j];
                        }
                    }
                    // Расчет правых частей

                    OnSource(ist, jst, IndexTask);

                    
                    ERR.INF_NAN("sc / River2DFV_rho_const.CalkCoef()", sc);
                    for (i = ist; i < imax; i++)
                    {
                        for (j = jst; j < jmax; j++)
                        {
                            // Расчет площади КО,
                            vol = Hx[i][j] * Hy[i][j];
                            // Расчет Ap0, Ap, 
                            Ap0[i][j] = rho_w * vol / dtime;
                            Ap[i][j] = (Ap0[i][j] + Aw[i][j] + Ae[i][j] + An[i][j] + As[i][j] -
                                sp[i][j] * vol) / relax[IndexTask];
                            // коррекция правой части временными членами
                            sc[i][j] = sc[i][j] * vol + (Ap[i][j] * rel + Ap0[i][j]) * F[IndexTask][i][j];
                        }
                    }
                    bcF[IndexTask].SetBCondition(ist, jst, imax, jmax, ref F[IndexTask]);
                    pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcF[IndexTask], ref F[IndexTask], 1);
                    
                    ERR.INF_NAN("F[" + IndexTask.ToString() + "] / River2DFV_rho_const.CalkCoef()", F[IndexTask]);
                    #endregion 
                    break;
                default:  // Расчет коэффициентов скалярных уравнений 
                    #region def
                    {
                        // Расчет "проводимости" текущей задачи
                        OnLookingGama(IndexTask);
                        
                        ERR.INF_NAN(" def gam / River2DFV_rho_const.CalkCoef()", gam);

                        // Сдвиг четки для текущей задачи - отсутсвует
                        ist = 1;
                        jst = 1;
                        // Релаксация для текущей задачи
                        rel = 1 - relax[IndexTask];
                        // Расчет вертикальных коэффициентов As[i][j], An[i][j]
                        //             1       imax  
                        //i=0,j=0|-------------|--> i x
                        //       |             |
                        //       |             |
                        //       | 0         0 |
                        //       |             |     Ae[i][j] = Aw[i+1][j] - flow
                        //       |             |     An[i][j] = As[i][j+1] - flow
                        //       |             |
                        //       |-------------|
                        //       | y    1  
                        //       V j          
                        //             
                        for (i = ist; i < imax; i++)
                        {
                            // граница сверху
                            // Конвективный поток
                            flow = rho_w * v[i][jst] * Hx[i][jst - 1];
                            // Диффузионный поток
                            diff = gam[i][jst - 1] * Hx[i][jst] / Dy[i][jst];
                            // Вычисление коэффициента
                            As[i][jst] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                            for (j = jst; j < jmax; j++)
                            {
                                if (j < jmax - 1)
                                {
                                    // Балансровка схемы при сгущениях
                                    fy = Hy[i][j] / Dy[i][j] / 2;
                                    // Средняя "проводимость"
                                    agam = gam[i][j] * fy + gam[i][j + 1] * (1 - fy);
                                    // Конвективный поток
                                    flow = rho_w * v[i][j] * Hx[i][j];
                                    // Диффузионный поток
                                    diff = gam[i][j] * gam[i][j + 1] / agam * Hx[i][j] / Hy[i][j];
                                }
                                else
                                {
                                    // граница снизу
                                    // Конвективный поток
                                    flow = rho_w * v[i][jmax] * Hx[i][jmax - 1];
                                    // Диффузионный поток
                                    diff = gam[i][jmax] * Hx[i][jmax - 1] / Dy[i][jmax];
                                }
                                // Вычисление коэффициентов
                                As[i][j + 1] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                                An[i][j] = As[i][j + 1] - flow;
                            }
                        }
                        // Расчет горизонтальных коэффициентов Ae[i][j], Aw[i][j]
                        for (j = jst; j < jmax - 1; j++)
                        {
                            // граница справа (ось симметрии)
                            // Конвективный поток
                            flow = rho_w * u[ist][j] * Hy[ist - 1][j];
                            // Диффузионный поток
                            diff = gam[ist - 1][j] * Hy[ist - 1][j] / Dx[ist][j];
                            // Вычисление коэффициента
                            Aw[ist][j] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                            // В области
                            for (i = 1; i < imax; i++)
                            {
                                if (i < imax - 1)
                                {
                                    // Балансровка схемы при сгущениях
                                    fx = Hx[i][j] / Dx[i][j] / 2;
                                    // Средняя "проводимость"
                                    agam = gam[i][j] * fx + gam[i + 1][j] * (1 - fx);
                                    // Конвективный поток
                                    flow = rho_w * u[i][j] * Hy[i][j];
                                    // Диффузионный поток
                                    diff = gam[i][j] * gam[i + 1][j] / agam * Hy[i][j] / Hx[i][j];
                                }
                                else
                                {
                                    // Стенка с права (твердая стенка)
                                    // Конвективный поток
                                    flow = rho_w * u[imax][j] * Hy[i][j];
                                    // Диффузионный поток
                                    diff = gam[imax][j] * Hy[i][j] / Dx[imax][j];
                                }
                                // Вычисление коэффициентов
                                Aw[i + 1][j] = DifFlow(diff, flow) + Math.Max(0.0f, flow);
                                Ae[i][j] = Aw[i + 1][j] - flow;
                            }
                        }
                        // Расчет правых частей
                        OnSource(ist, jst, IndexTask);
                        
                        ERR.INF_NAN(" def sc0 / River2DFV_rho_const.CalkCoef()", sc);
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                // Расчет площади КО,
                                vol = Hx[i][j] * Hy[i][j];
                                // Расчет Ap0, Ap, 
                                Ap0[i][j] = rho_w * vol / dtime;
                                Ap[i][j] = (Ap0[i][j] + Aw[i][j] + Ae[i][j] + An[i][j] + As[i][j] -
                                    sp[i][j] * vol) / relax[IndexTask];
                                // коррекция правой части временными членами
                                sc[i][j] = sc[i][j] * vol + (Ap[i][j] * rel + Ap0[i][j]) * F[IndexTask][i][j];
                            }
                        }
                        ERR.INF_NAN(" def sc1 / River2DFV_rho_const.CalkCoef()", sc);
                        // решение САУ для получения расчетного скалярного поля 
                        //if (IndexTask == 3)
                        //{
                        //    //LOG.Print("Ap", Ap, 2);
                        //    //LOG.Print("Ae", Ae, 2);
                        //    //LOG.Print("Aw", Aw, 2);
                        //    //LOG.Print("An", An, 2);
                        //    //LOG.Print("As ", As, 2);

                        //    //int[] BCFlag = { 1, 1, 1, 1 };
                        //    //AlgebraAdapter algebraAdapter = new AlgebraAdapter(imax, jmax);
                        //    //algebraAdapter.SetBCondition(ist, jst, ref F[IndexTask]);
                        //    //algebraAdapter.OnTDMASolver(ist, jst, Ae, Aw, An, As, Ap, sc, ref F[IndexTask], BCFlag);

                        //    //pSolve.OnTDMASolver(ist, jst, Ae, Aw, An, As, Ap, sc, F[IndexTask]);
                        //    bcF[IndexTask].SetBCondition(ist, jst, imax, jmax, ref F[IndexTask]);
                        //    //Console.WriteLine("~ ist = {0:F4}, jst = {1:F4}, imax = {2:F4}, jmax = {3:F4},", ist, jst, imax, jmax);
                        //    pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcF[IndexTask], ref F[IndexTask], 1);
                        //    ERR.INF_NAN(F[IndexTask]);
                        //}
                        //if (IndexTask == 4)
                        //{
                        //    //pSolve.OnTDMASolver(ist, jst, Ae, Aw, An, As, Ap, sc, F[IndexTask]);
                        //    bcF[IndexTask].SetBCondition(ist, jst, imax, jmax, ref F[IndexTask]);
                        //    //Console.WriteLine("~ ist = {0:F4}, jst = {1:F4}, imax = {2:F4}, jmax = {3:F4},", ist, jst, imax, jmax);
                        //    pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcF[IndexTask], ref F[IndexTask], 1);
                        //    //ERR.MIN(F[IndexTask], 0);
                        //    ERR.INF_NAN(F[IndexTask]);
                        //}
                        //if (IndexTask == 5)
                        //{
                        //    //pSolve.OnTDMASolver(ist, jst, Ae, Aw, An, As, Ap, sc, F[IndexTask]);
                        //    bcF[IndexTask].SetBCondition(ist, jst, imax, jmax, ref F[IndexTask]);
                        //    //Console.WriteLine("~ ist = {0:F4}, jst = {1:F4}, imax = {2:F4}, jmax = {3:F4},", ist, jst, imax, jmax);
                        //    pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcF[IndexTask], ref F[IndexTask], 1);
                        //    ERR.INF_NAN(F[IndexTask]);
                        //}

                        if (IndexTask == 4 || IndexTask == 5)
                        {
                            //tke[i][j] = (double)(Coeff_k * v[i][j] * v[i][j]);
                            //dis[i][j] = (double)(Coeff_e * tke[i][j] * tke[i][j]);


                            if (Params.shiftV == true && Params.LV > 0)
                            {
                                for (i = 0; i < Nx; i++)
                                {
                                    double bx = x[i][jdxTube];
                                    //if (Params.Wen1 >= bx && Params.V1_inlet > 0)
                                    //    BCV(i, jdxTube, Params.V1_inlet); // основная скорость
                                    if ((Params.Wen2 + Params.Wen1 > bx && Params.Wen1 <= bx) && Params.V2_inlet > 0)
                                    {
                                        if (IndexTask == 4)
                                            for (int jj = 0; jj <= jdxTube; jj++)
                                                BCV(i, jj, Coeff_k * v[i][jj] * v[i][jj]);   
                                        else
                                            for (int jj = 0; jj <= jdxTube; jj++)
                                                BCV(i, jj, Coeff_e * tke[i][jj] * tke[i][jj]);   
                                    }
                                    //if ((Params.Wen3 + Params.Wen2 + Params.Wen1 > bx && Params.Wen2 + Params.Wen1 <= bx))
                                    //{
                                    //    if(Params.V3_inlet > 0)
                                    //        BCV(i, jdxTube, Params.V3_inlet); ;    // скорость набегания
                                    //}
                                }
                                //for (int jj = 0; jj <= jdxTube; jj++)
                                //    BCV(bottomTube, jj, 0);   // скорость набегания
                                //for (int jj = 0; jj <= jdxTube; jj++)
                                //    BCV(topTube, jj, 0);   // скорость набегания
                            }
                        }

                        bcF[IndexTask].SetBCondition(ist, jst, imax, jmax, ref F[IndexTask]);
                        pSolve.OnTDMASolver(ist, jst, imax, jmax, Ae, Aw, An, As, Ap, sc, bcF[IndexTask], ref F[IndexTask], 1);
                        
                        ERR.INF_NAN("def F[" + IndexTask.ToString() + "] / River2DFV_rho_const.CalkCoef()", F[IndexTask]);
                    }
                    #endregion;
                    break;
            }
        }
        /// <summary>
        /// Расчет коэффициентов диффузии для текущей задачи (турбулентной вязкости, диффузии ...) 
        /// </summary>
        /// <param name="IndexTask"></param>
        protected override void OnLookingGama(int IndexTask)
        {
            int i, j;
            double amt;
            double rel;
            double factor = 1;
            double mu = rho_w * nu;
            switch (IndexTask)
            {
                case 0: // При расчете скорости u
                    {
                        rel = 1 - relax[6];
                        for (i = 0; i < imax + 1; i++)
                        {
                            gam[i][jmax] = mu; 
                            for (j = 0; j < jmax + 1; j++)
                            {
                                // Новая турбулентная вязкость mu_t = cm * rho * ke^2/dis
                                amt = Cmu * rho_w * tke[i][j] * tke[i][j] / (dis[i][j] + MEM.Error10);
                                // Расчет турбулентной вязкости на новом шаге ... с учетом релаксации
                                mut[i][j] = relax[6] * amt + rel * mut[i][j];
                                // Tурбулентная вязкость (не кинематическая)
                                gam[i][j] = mut[i][j] * factor + mu;
                            }
                        }
                        break;
                    }
                case 1:  // При расчете скорости v
                    {
                        double b_kappa_w = 1.0 / kappa_w;
                        for (j = 1; j < jmax + 1; j++)
                        {
                            gam[0][j] = mu;
                            // расчет пристеночного масштаба, учитывая что стенка вертикальная и слева (i = 0)
                            //  xplus = cm4 * rho * sqrt (  ke  ) * dx /mu = cm4 * Re_x
                            //  xplus[j] = cmu4 * rho[1][j] * Math.Sqrt(tke[1][j]) * Dx[1][j] / mu;
                            xplus[j] = cmu4 * Math.Sqrt(tke[1][j]) * Dx[1][j] / nu;
                            // поправка вязкости на стенке
                            if (xplus[j] > 11.5)
                                gam[0][j] = nu * xplus[j] / (b_kappa_w * Math.Log(E_wall * xplus[j])) + mu;
                            gam[imax][j]  = mu;
                        }
                        // StatisticaYplus();
                        break;
                    }
                case 3: 
                    {
                        if (Params.TemperOrConcentration == true)
                        {
                            // При расчете концентрации ищем диффузию 
                            double b_kappa_w = 1.0 / kappa_w;
                            factor = 1.0 / PrandtlС;
                            for (j = 0; j < jmax + 1; j++)
                            {
                                gam[imax][j] = mu * factor ;
                                
                                if (xplus[j] > 11.5)
                                    gam[0][j] = nu / PrandtlС * xplus[j] / (b_kappa_w * Math.Log(E_wall * xplus[j]) + pfn);
                                else
                                    gam[0][j] = mu * factor;

                                for (i = 1; i < imax; i++)
                                    gam[i][j] = (mut[i][j] + mu) * factor;
                            }
                        }
                        else
                        {
                            // При расчете температуры ищем температуропроводность
                            double b_kappa_w = 1.0 / kappa_w;
                            factor = 1.0 / Prandtl_T;
                            for (j = 0; j < jmax + 1; j++)
                            {
                                gam[imax][j] = mu * factor / Prandtl;
                                gam[0][j] = mu * factor / Prandtl;
                                if (xplus[j] > 11.5)
                                    gam[0][j] = nu / Prandtl_T * xplus[j] / (b_kappa_w * Math.Log(E_wall * xplus[j]) + pfn);
                                for (i = 1; i < imax; i++)
                                {
                                    gam[i][j] = (mut[i][j] + mu) * factor;

                                    if (j == jmax)
                                        gam[i][j] = mu * factor;
                                }
                            }
                        }
                        break;
                    }
                case 4: // Турбулентная кинитическая энергия 
                    {
                        factor = 1.0 / Prandtl_Kin;
                        for (j = 0; j < jmax + 1; j++)
                        {
                            gam[imax][j] = nu * factor;
                            gam[0][j] = nu * factor;
                            for (i = 1; i < imax; i++)
                            {
                                gam[i][j] = (mut[i][j] + mu) * factor;
                                if (j == jmax)
                                    gam[i][j] = mu * factor;
                            }
                        }
                        break;
                    }
                case 5: // Дисспация турбулентной кинитической энергия
                    {
                        factor = 1.0 / Prandtl_dis;
                        for (j = 0; j < jmax + 1; j++)
                            for (i = 1; i < imax; i++)
                                gam[i][j] = (mut[i][j] + mu) * factor;
                    }
                    break;
            }
        }
        /// <summary>
        /// Расчет правой части уравнения
        /// </summary>
        /// <param name="ist">смещение по сетки по i</param>
        /// <param name="jst">смещение по сетки по j</param>
        /// <param name="IndexTask"></param>
        protected override void OnSource(int ist, int jst, int IndexTask)
        {
            int i, j;
            double gamp, gamm;
            double dudx, dudy, dvdx, dvdy;
            switch (IndexTask)
            {
                // При расчете скорости u необходимо учитывать все члены тензора Рейнольдса часть из них
                // связанная с асимптотикой уравнения неразрывности считается здесь
                case 0:
                    {
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                sc[i][j] = (gam[i][j] * (u[i + 1][j] - u[i][j]) / Hx[i][j] -
                                    gam[i - 1][j] * (u[i][j] - u[i - 1][j]) / Hx[i - 1][j]) / Dx[i][j];

                                gamp = gam[i][j + 1] * gam[i - 1][j + 1] / (gam[i][j + 1] + gam[i - 1][j + 1]);
                                gamp += gam[i][j] * gam[i - 1][j] / (gam[i][j] + gam[i - 1][j]);

                                gamm = gam[i][j - 1] * gam[i - 1][j - 1] / (gam[i][j - 1] + gam[i - 1][j - 1]);
                                gamm += gam[i][j] * gam[i - 1][j] / (gam[i][j] + gam[i - 1][j]);

                                sc[i][j] += (gamp * (v[i][j + 1] - v[i - 1][j + 1]) - gamm * (v[i][j] - v[i - 1][j])) / (Hy[i][j] * Dx[i][j]);

                                sp[i][j] = 0;
                            }
                        }
                        break;
                    }
                case 1:// При расчете скорости v ... аналогично u...
                    {
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                sc[i][j] = (gam[i][j] * (v[i][j + 1] - v[i][j]) / Hy[i][j] -
                                           gam[i][j - 1] * (v[i][j] - v[i][j - 1]) / Hy[i][j - 1]) / Dy[i][j];

                                gamp = gam[i + 1][j] * gam[i + 1][j - 1] / (gam[i + 1][j] + gam[i + 1][j - 1] + 1e-30);
                                gamp += gam[i][j] * gam[i][j - 1] / (gam[i][j] + gam[i][j - 1] + 1e-30);

                                gamm = gam[i - 1][j] * gam[i - 1][j - 1] / (gam[i - 1][j] + gam[i - 1][j - 1] + 1e-30);
                                gamm += gam[i][j] * gam[i][j - 1] / (gam[i][j] + gam[i][j - 1] + 1e-30);

                                sc[i][j] += (gamp * (u[i + 1][j] - u[i + 1][j - 1]) - gamm * (u[i][j] - u[i][j - 1])) / (Hx[i][j] * Dy[i][j]);

                                sp[i][j] = 0;
                            }
                        }
                        break;
                    }
                //
                // для функции давления источник расчитывается вместе с коэффициентами
                //
                case 3:  // При расчете температуры - источников нет (задача не замкнута на энергию)
                    {
                        if (Params.TemperOrConcentration == true)
                        {
                            for (i = ist; i < imax; i++)
                            {
                                for (j = jst; j < jmax; j++)
                                {
                                    dudx = (u[i + 1][j] - u[i][j]) / Hx[i][j];
                                    //dvdx = (v[i][j+1] - v[i][j]) / Hx[i][j];
                                    sc[i][j] = 0;
                                    sp[i][j] = - Ws * dudx;
                                    //sp[i][j] = - Ws * dvdx;
                                }
                            }
                        }
                        else
                        {
                            for (i = ist; i < imax; i++)
                            {
                                for (j = jst; j < jmax; j++)
                                {
                                    sc[i][j] = 0;
                                    sp[i][j] = 0;
                                }
                            }
                        }
                        break;
                    }
                case 4:  // При расчете турбулентной кинитической энергии: считаем генерацию и дисспацию
                    {
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                // расчет компонент тензора скоростей деформаций
                                dudx = (u[i + 1][j] - u[i][j]) / Hx[i][j];
                                dvdy = (v[i][j + 1] - v[i][j]) / Hy[i][j];
                                dudy = (u[i][j + 1] - u[i][j - 1] + u[i + 1][j + 1] - u[i + 1][j - 1]) /
                                    (Dy[i][j] + Dy[i][j + 1]) / 2;
                                dvdx = (v[i + 1][j] - v[i - 1][j] + v[i + 1][j + 1] - v[i - 1][j + 1]) /
                                    (Dx[i][j] + Dx[i + 1][j]) / 2;
                                // расчет генерации КИ
                                gen[i][j] = 2 * (dudx * dudx + dvdy * dvdy) + (dudy + dvdx) * (dudy + dvdx);
                                // расчет источников для b и Ap
                                sc[i][j] = gen[i][j] * mut[i][j];
                                sp[i][j] = - rho_w * dis[i][j] / (tke[i][j] + 1e-30);
                            }
                        }
                        break;
                    }
                case 5: // При расчете диссипации турбулентной кинитической энергии
                    {
                        //double WW = 1e+30;
                        double WW = 1e+10;

                        for (j = jst; j < jmax; j++)
                        {
                            // диссипация - граничные условия
                            //double diss = Cmu * Math.Sqrt(tke[ist][j] * tke[ist][j] * tke[ist][j]) / (0.4 * cmu4 * Dx[ist][j]);
                            double diss = Cmu * tke[ist][j] * Math.Sqrt(Math.Abs(tke[ist][j])) / (0.4 * cmu4 * Dx[ist][j]);
                            // расчет граничных условий на дне 
                            sc[ist][j] = (double)(WW * diss);
                            sp[ist][j] = (double)-WW;
                            // расчет источников для b и Ap
                            for (i = ist + 1; i < imax; i++)
                            {
                                sc[i][j] =   c1 * gen[i][j] * Cmu * rho_w * tke[i][j];
                                sp[i][j] = - c2 * rho_w * dis[i][j] / (tke[i][j] + MEM.Error10);
                            }
                        }
                        break;
                    }
                case 6: // При расчете функции тока вычисляем в правой части вихрь скоростивихрь
                    {
                        for (i = ist; i < imax; i++)
                        {
                            for (j = jst; j < jmax; j++)
                            {
                                // расчет компонент тензора скоростей деформаций
                                //dudx = (u[i + 1][j] - u[i][j]) / Hx[i][j];
                                dvdy = (v[i][j + 1] - v[i][j]) / Hy[i][j];
                                //dudy = (u[i][j + 1] - u[i][j - 1] + u[i + 1][j + 1] - u[i + 1][j - 1]) /
                                //    (Dy[i][j] + Dy[i][j + 1]) / 2;
                                dvdx = (v[i + 1][j] - v[i - 1][j] + v[i + 1][j + 1] - v[i - 1][j + 1]) /
                                    (Dx[i][j] + Dx[i + 1][j]) / 2;
                                // расчет генерации КИ
                                //gen[i][j] = 2 * (dudx * dudx + dvdy * dvdy) + (dudy + dvdx) * (dudy + dvdx);
                                // расчет источников для b и Ap
                                sc[i][j] = dvdy - dvdx;
                                sp[i][j] = 0;
                            }
                        }
                        break;
                    }
                default: { } break;
            }
        }
        #endregion
    }
}
