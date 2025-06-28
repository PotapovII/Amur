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
//   добавлена адаптация вывода результатов в форматах библиотек 
//                              MeshLib, RenderLib;
//                                  16.03.2021
//---------------------------------------------------------------------------
//                   рефакторинг : Потапов И.И.
//                          25.04.25
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;

    using MemLogLib;

    using CommonLib;
    using CommonLib.Function;
    using CommonLib.Physics;

    using MeshLib;
    using CommonLib.BedLoad;

    /// <summary>
    /// ОО: Реализация решателя для задачи о расчете профильного турбулентного потока 
    /// в формулировке RANS k-e методом контрольных объемов
    /// </summary>
    [Serializable]
    public class BedLoadTask2DFV : ABedLoadTask_2D<BedLoadParams2D>, IBedLoadTask
    {
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IBedLoadTask Clone()
        {
            return new BedLoadTask2DFV(Params);
        }
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        public override string NameBLParams()
        {
            return "BedLoadParams2D.txt";
        }
        /// <summary>
        /// Загрузка полей задачи из форматного файла
        /// </summary>
        /// <param name="file">имя файла</param>
        public override void LoadData(IDigFunction[] crossFunctions = null)
        {
            throw new NotImplementedException();
        }
        

        public override void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Флаг совместимости сеток
        /// </summary>
        protected bool flagGenMesh = false;
        /// <summary>
        /// Количество узлов по направлению X 
        /// (количество узлов по i)
        /// </summary>
        public int Nx { get; set; }
        /// <summary>
        /// Количество узлов по направлению Y
        /// (количество узлов по j)
        /// </summary>
        public int Ny { get; set; }
        /// <summary>
        /// Количество контрольных объемов по направлению X 
        /// (количество узлов по i)
        /// </summary>
        public int imax { get; set; }
        /// <summary>
        /// Количество контрольных объемов по направлению Y
        /// (количество узлов по j)
        /// </summary>
        public int jmax { get; set; }
        /// <summary>
        /// отметки дна
        /// </summary>
        //public double[][] zeta;
        /// <summary>
        /// отметки дна
        /// </summary>
        public double[][] zeta0;

        protected double[][] tauX = null;
        protected double[][] tauY = null;
        protected double[][] P = null;


        protected double[][] App;
        protected double[][] App0;
        protected double[][] Aep;
        protected double[][] Awp;
        protected double[][] Apn;
        protected double[][] Aps;

        protected double[][] Aen;
        protected double[][] Aes;
        protected double[][] Awn;
        protected double[][] Aws;

        protected double[][] sc;
        protected double[][] sp;

        double[][] G0 = null;
        double[][] A = null;
        double[][] B = null;
        double[][] C = null;
        double[][] D = null;

        protected double[][] mtau = null;
        protected bool[][] DryWet = null;
        protected double[][] CosGamma = null;

        protected double[][] Sxx;
        protected double[][] Sxy;
        protected double[][] Syx;
        protected double[][] Syy;
        protected double[][] Hxx;
        protected double[][] Hxy;
        protected double[][] Hyx;
        protected double[][] Hyy;

        RectFVMesh qmesh = null;

        public BedLoadTask2DFV(BedLoadParams2D p) : base(p)
        {
            name = "деформация дна в прямоугольной области МКО.";
        }

        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        public virtual void SetTask(IMesh imesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions){ }

        public virtual void SetTask(RectFVMesh mesh, double[][] Zeta0, double[][] Roughness, IBoundaryConditions BConditions)
        {
            this.mesh = mesh;
            this.qmesh = mesh;
            if (mesh == null || Zeta0 == null)
            {
                Logger.Instance.Info("Сетка не установленна: BedLoadTask2DFV.SetTask");
                return;
            }
            this.mesh = mesh;

            zeta0 = Zeta0;
            Nx = mesh.Nx;
            Ny = mesh.Ny;
            imax = mesh.imax;
            jmax = mesh.jmax;

            MEM.Alloc2D(Nx, Ny, ref App);
            MEM.Alloc2D(Nx, Ny, ref App0);

            MEM.Alloc2D(Nx, Ny, ref Aep);
            MEM.Alloc2D(Nx, Ny, ref Awp);
            MEM.Alloc2D(Nx, Ny, ref Apn);
            MEM.Alloc2D(Nx, Ny, ref Aps);

            MEM.Alloc2D(Nx, Ny, ref Aen);
            MEM.Alloc2D(Nx, Ny, ref Aes);
            MEM.Alloc2D(Nx, Ny, ref Awn);
            MEM.Alloc2D(Nx, Ny, ref Aws);

            MEM.Alloc2D(Nx, Ny, ref sc);
            MEM.Alloc2D(Nx, Ny, ref sp);

            MEM.Alloc2D(Nx, Ny, ref G0);
            MEM.Alloc2D(Nx, Ny, ref A);
            MEM.Alloc2D(Nx, Ny, ref B);
            MEM.Alloc2D(Nx, Ny, ref C);
            MEM.Alloc2D(Nx, Ny, ref D);

            MEM.Alloc2D(Nx, Ny, ref CosGamma);

            MEM.Alloc2D(Nx, Ny, ref zeta0);
            MEM.Alloc2D(Nx, Ny, ref mtau);
            MEM.Alloc2D(Nx, Ny, ref tauX);
            MEM.Alloc2D(Nx, Ny, ref tauY);
            MEM.Alloc2D(Nx, Ny, ref P);
            MEM.Alloc2D(Nx, Ny, ref Sxx);
            MEM.Alloc2D(Nx, Ny, ref Sxy);
            MEM.Alloc2D(Nx, Ny, ref Syx);
            MEM.Alloc2D(Nx, Ny, ref Syy);
            MEM.Alloc2D(Nx, Ny, ref Hxx);
            MEM.Alloc2D(Nx, Ny, ref Hxy);
            MEM.Alloc2D(Nx, Ny, ref Hyx);
            MEM.Alloc2D(Nx, Ny, ref Hyy);

            MEM.Alloc2D(Nx, Ny, ref DryWet);

            //avalanche = new Avalanche2DQ(mesh, tanphi, DirectAvalanche.AvalancheXY, 0.3);

            eTaskReady = ETaskStatus.TaskReady;
        }


        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. (2010), 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta">>возвращаемая форма дна на n+1 итерации</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        public override void CalkZetaFDM(ref double[] Zeta, double[] TauX, double[] TauY, double[] Press = null, double[][] CS = null) 
        {
           
        }
        public void CalkZetaFDM(ref double[][] zeta, double[][] TauX, double[][] TauY, double[][] Press = null, double[][] CS = null) 
        {
            double s = SPhysics.PHYS.s;
            double tanphi = SPhysics.PHYS.tanphi;
            double G1 = SPhysics.PHYS.G1;
            double epsilon = SPhysics.PHYS.epsilon;
            double tau0 = SPhysics.PHYS.tau0;

            int i, j;
            double cosA, sinA, cos2, sin2, cs2;
            MEM.Alloc2D(Nx, Ny, ref zeta);
            #region Вычисление косинуса уклона нормали к дну
            double dZetadX, dZetadY;
            for (j = 1; j < jmax; j++)
            {
                for (i = 1; i < imax; i++)
                {
                    dZetadX = (zeta0[i + 1][j] - zeta0[i - 1][j]) / (qmesh.Dx[i][j] + qmesh.Dx[i - 1][j]) * qmesh.hy[i][j];
                    dZetadY = (zeta0[i][j + 1] - zeta0[i][j - 1]) / (qmesh.Dy[i][j] + qmesh.Dy[i][j - 1]) * qmesh.hx[i][j];
                    CosGamma[i][j] = 1.0 / Math.Sqrt(1 + dZetadX * dZetadX + dZetadY * dZetadY);
                }
            }
            for (i = 1; i < imax; i++)
            {
                j = 0;
                CosGamma[i][j] = CosGamma[i][j+1];
                j = jmax;
                CosGamma[i][j] = CosGamma[i][j-1];
            }
            for (j = 1; j < jmax; j++)
            {
                i = 0;
                CosGamma[i][j] = CosGamma[i + 1][j];
                i = imax;
                CosGamma[i][j] = CosGamma[i - 1][j];
            }
            i = 0; j = 0;
            CosGamma[i][j] = 0.5*(CosGamma[i + 1][j] + CosGamma[i][j+1]);
            i = imax; j = 0;
            CosGamma[i][j] = 0.5 * (CosGamma[i - 1][j] + CosGamma[i][j + 1]);
            i = imax; j = jmax;
            CosGamma[i][j] = 0.5 * (CosGamma[i - 1][j] + CosGamma[i][j - 1]);
            i = 0; j = jmax;
            CosGamma[i][j] = 0.5 * (CosGamma[i + 1][j] + CosGamma[i][j - 1]);
            #endregion

            #region Ищем G0, A, B, C, D в узлах
            for (i = 0; i < Nx; i++)
            {
                for (j = 0; j < Ny; j++)
                {
                    mtau[i][j] = Math.Sqrt(tauX[i][j] * tauX[i][j] + tauY[i][j] * tauY[i][j]);

                    if (mtau[i][j] > MEM.Error6)
                    {
                        cosA = tauX[i][j] / mtau[i][j];
                        sinA = tauY[i][j] / mtau[i][j];
                    }
                    else
                    {
                        cosA = 1;  sinA = 0;
                    }
                    cos2 = cosA * cosA;
                    sin2 = sinA * sinA;
                    cs2 = sinA * cosA;
                    double mtauS = tauX[i][j] * cosA + tauY[i][j] * sinA;
                    
                    if (tau0 > mtau[i][j])
                    {
                        chi = 1;
                        DryWet[i][j] = true; // сухая (не размываемая) часть дна
                        A[i][j] = 0;
                        B[i][j] = 0;
                        C[i][j] = 0;
                        G0[i][j] = 0;
                    }
                    else
                    {
                        G0[i][j] = rho_s * G1 * mtauS * Math.Sqrt(mtau[i][j]);
                        chi = Math.Sqrt(tau0 / mtau[i][j]);
                        A[i][j] = Math.Max(0, 1 - chi);
                        C[i][j] = A[i][j] / (s * tanphi);

                        if (Params.blm != TypeBLModel.BLModel_2010)
                            B[i][j] = (chi / 2 + A[i][j]) / tanphi;
                        else
                            B[i][j] = (chi / 2 + A[i][j] * (1 + s) / s) / tanphi;
                        D[i][j] = A[i][j] * 4.0 / 5.0 / tanphi;
                    }
                    double ss = (1 + s) / s;
                    Sxx[i][j] = ( ss * D[i][j] * sin2 + B[i][j] * cos2 ) * G0[i][j];
                    Sxy[i][j] = ( cs2 * (B[i][j] - ss * D[i][j]) ) * G0[i][j];
                    Syx[i][j] = ( cs2 * (B[i][j] - ss * D[i][j]) ) * G0[i][j];
                    Syy[i][j] = ( ss * D[i][j] * cos2 + B[i][j] * sin2 ) * G0[i][j];
                    // ?
                    Hxx[i][j] = ( -D[i][j] / s * sin2 - C[i][j] * cos2 ) * G0[i][j];
                    Hxy[i][j] = ( cs2 * (D[i][j] / s - C[i][j]) ) * G0[i][j];
                    Hyx[i][j] = ( cs2 * (D[i][j] / s - C[i][j]) ) * G0[i][j];
                    Hyy[i][j] = ( -D[i][j] / s * cos2 - C[i][j] * sin2 ) * G0[i][j];
                }
            }
            #endregion 

            // Сдвиг четки для текущей задачи - отсутсвует
            int ist = 1;
            int jst = 1;
            // Релаксация для текущей задачи
            //rel = 1 - relax[IndexTask];
            // Расчет вертикальных коэффициентов As[i][j], An[i][j]
            //             1  Aw    Ny  
            //i=0,j=0  *-------------*------------*--> j (y)
            //         | Aws         Awp          | Awn    
            //         |                          |
            //         |                          |     
            //         |                          |     
            //         |                          |
            //         *             *            *     
            //         | Aps         App          | Apn    
            //         |                          |
            //         |                          |     
            //         |                          |     
            //         |                          |
            //      Nx *------------*-------------*
            //         Aes         Aep           Aen  
            //         |          Aep           Aen  
            //         V i (x)          
            //      
            #region коэффициенты FVM

            for (i = ist; i < imax; i++)
            {
                // Вычисление коэффициентов
                for (j = jst; j < jmax; j++)
                {
                    double hy_hx = qmesh.hy[i][j] / qmesh.hx[i][j];
                    double hx_hy = qmesh.hx[i][j] / qmesh.hy[i][j];

                    if (DryWet[i][j] == true)
                    {
                        Aep[i][j] = 0;
                        Awp[i][j] = 0;
                        Apn[i][j] = 0;
                        Aps[i][j] = 0;

                        Aen[i][j] = 0;
                        Awn[i][j] = 0;
                        Aes[i][j] = 0;
                        Aes[i][j] = 0;

                        App0[i][j] = rho_s * (1 - epsilon) * qmesh.hx[i][j] * qmesh.hy[i][j] / dtime;
                        App[i][j] = App0[i][j];
                        sc[i][j] = App0[i][j];
                        sp[i][j] = 0;
                    }
                    else
                    {
                        //double epsilon = 0.3;
                        // App0 := -(1-epsilon)*rho/dt*Hx_pp*Hy_pp
                        App0[i][j] = rho_s * (1 - epsilon) * qmesh.hx[i][j] * qmesh.hy[i][j] / dtime;
                        // Aep:= 1 / 2 * (Sxx_pp + Sxx_ep) / Dx_pp * Hy_pp / Hx_pp
                        Aep[i][j] = 0.5 * (Sxx[i][j] + Sxx[i + 1][j]) / qmesh.Dx[i][j] * hy_hx;
                        // Awp := 1/2*(Sxx_pp+Sxx_wp)/Dx_wp*Hy_pp/Hx_pp
                        Awp[i][j] = 0.5 * (Sxx[i][j] + Sxx[i - 1][j]) / qmesh.Dx[i - 1][j] * hy_hx;
                        // Apn := 1/2*(Syy_pp+Syy_pn)/Dy_pp*Hx_pp/Hy_pp
                        Aps[i][j] = 0.5 * (Syy[i][j] + Syy[i][j + 1]) / qmesh.Dy[i][j] * hx_hy;
                        // Aps := 1/2*(Syy_pp+Syy_ps)/Dy_ps*Hx_pp/Hy_pp
                        Apn[i][j] = 0.5 * (Syy[i][j] + Syy[i][j - 1]) / qmesh.Dy[i][j - 1] * hx_hy;
                        // центр
                        App[i][j] = Aep[i][j] + Awp[i][j] + Apn[i][j] + Aps[i][j] + App0[i][j];
                        // Aen := Syx_pn/(Dx_pn+Dx_wn)*Hx_pp/Hy_pp+Sxy_ep/(Dy_ep+Dy_es)*Hy_pp/Hx_pp
                        Aen[i][j] = Syx[i][j + 1] / (qmesh.Dx[i][j + 1] + qmesh.Dx[i - 1][j + 1]) * hx_hy +
                                    Sxy[i + 1][j] / (qmesh.Dy[i + 1][j] + qmesh.Dy[i + 1][j - 1]) * hy_hx;
                        // Awn := -Syx_pn/(Dx_pn+Dx_wn)*Hx_pp/Hy_pp-Sxy_wp/(Dy_wp+Dy_ws)*Hy_pp/Hx_pp
                        Awn[i][j] = -Syx[i][j + 1] / (qmesh.Dx[i][j + 1] + qmesh.Dx[i - 1][j + 1]) * hx_hy +
                                   -Sxy[i - 1][j] / (qmesh.Dy[i - 1][j] + qmesh.Dy[i - 1][j - 1]) * hy_hx;
                        // Aes := -Syx_ps/(Dx_ps+Dx_ws)*Hx_pp/Hy_pp-Sxy_ep/(Dy_ep+Dy_es)*Hy_pp/Hx_pp
                        Aes[i][j] = -Syx[i][j - 1] / (qmesh.Dx[i][j - 1] + qmesh.Dx[i - 1][j - 1]) * hx_hy +
                                   -Sxy[i + 1][j] / (qmesh.Dy[i + 1][j] + qmesh.Dy[i + 1][j - 1]) * hy_hx;
                        // Aws := Syx_ps/(Dx_ps+Dx_ws)*Hx_pp/Hy_pp+Sxy_wp/(Dy_wp+Dy_ws)*Hy_pp/Hx_pp
                        Aes[i][j] = -Syx[i][j - 1] / (qmesh.Dx[i][j - 1] + qmesh.Dx[i - 1][j - 1]) * hx_hy +
                                   -Sxy[i - 1][j] / (qmesh.Dy[i - 1][j] + qmesh.Dy[i - 1][j - 1]) * hy_hx;
                        // 1/2*(a_ep*cosA_ep-a_wp*cosA_wp)*Hy_pp/Hx_pp+
                        // 1/2*(a_pn*cosA_pn-a_ps*cosA_ps)*Hx_pp/Hy_pp
                        sc[i][j] = 0.5 * (A[i + 1][j] * CosGamma[i + 1][j] - A[i - 1][j] * CosGamma[i - 1][j]) * qmesh.hy[i][j] +
                                   0.5 * (A[i][j + 1] * CosGamma[i][j + 1] - A[i][j - 1] * CosGamma[i][j - 1]) * qmesh.hx[i][j] +
                                   App0[i][j];
                        sp[i][j] = 0;
                    }
                }
            }
            
            // решение задачи

            double maxZeta = 0 , zetaOld, errorValue, maxError =0;
            int iter = 0;
            for (iter = 0; iter < 100000; iter++)
            {
                for (i = ist; i < imax; i++)
                {
                    // Вычисление коэффициентов
                    for (j = jst; j < jmax; j++)
                    {
                        zetaOld = zeta[i][j];
                        zeta[i][j] =  (   App0[i][j] * zeta0[i][j] 
                                        + Aep[i][j] * zeta[i + 1][j] 
                                        + Awp[i][j] * zeta[i - 1][j] 
                                        + Apn[i][j] * zeta[i][j + 1] 
                                        + Aps[i][j] * zeta[i][j - 1] 
                                        + Aen[i][j] * zeta[i + 1][j + 1] 
                                        + Awn[i][j] * zeta[i - 1][j + 1] 
                                        + Aes[i][j] * zeta[i + 1][j - 1] 
                                        + Aws[i][j] * zeta[i - 1][j - 1] 
                                        + sc[i][j]
                                        ) / App[i][j];
                        maxZeta = Math.Max(Math.Abs(zetaOld), maxZeta);
                        errorValue = Math.Abs(zetaOld - zeta[i][j]);
                        if (maxError < errorValue)
                            maxError = errorValue;
                    }
                }
                if (maxError/maxZeta < MEM.Error6)
                    break;

                // Сглаживание дна по лавинной моделе
                if (Params.isAvalanche == AvalancheType.AvalancheSimple)
                {
                    if(avalanche!=null)
                        ((Avalanche2DQ)avalanche).Lavina2D(ref zeta);
                }
            }
            #endregion
            
        }
        /// <summary>
        /// Расчет коэффиентов Aw, As, ... 
        /// по схеме Патанкара со степенным законом
        /// </summary>
        /// <param name="Diff">диффузионный поток</param>
        /// <param name="Flow">конвективный поток</param>
        /// <returns>расчетный коэффициент</returns>
        protected double DifFlow(double Diff, double Flow)
        {
            if (Flow == 0.0f)
                return Diff;
            double temp = Diff - 0.1 * Math.Abs(Flow);
            if (temp <= 0.0f)
                return 0.0f;
            temp = temp / Diff;
            return (Diff * temp * temp * temp * temp * temp);
        }
        /// <summary>
        /// Формирование данных для отрисовки связанных с сеткой IMesh со сороны задачи
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        public void AddMeshPolesForGraphics(ISavePoint sp)
        {

        }


    }
}

