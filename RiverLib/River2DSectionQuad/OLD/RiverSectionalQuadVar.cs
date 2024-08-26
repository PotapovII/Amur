using CommonLib;
using GeometryLib;
using MemLogLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RiverLib
{
    [Serializable]
    public class RiverSectionalQuadVar : RiverSectionalQuad
    {
        /// <summary>
        /// Наименование задачи
        /// </summary>
        public override string Name { get => "Расчет потока в створе русла Var"; }
        public RiverSectionalQuadVar(RiverStreamQuadParams p) : base(p)
        {
        }
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        public override IRiver Clone()
        {
            return new RiverSectionalQuadVar(this.Params);
        }
        /// <summary>
        /// Поле плотности 
        /// </summary>
        //public double[][] Epsilon;
        public double[][] Water;
        public double[][] qP, qE, qW, qN, qS;
        protected double Qp, Qe, Qw, Qn, Qs;
        /// <summary>
        /// Установка параметров задачи
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="mesh"></param>
        public override void SetDataForRiverStream(ref HKnot right, ref HKnot left)
        {
            base.SetDataForRiverStream(ref right, ref left);
            int Nx = Params.Nx;
            int Ny = Params.Ny;

            //MEM.Alloc(Nx - 1, Ny - 1, ref Epsilon, "Epsilon");
            MEM.Alloc(Nx - 1, Ny - 1, ref Water, "Water");
            MEM.Alloc(Nx, Ny, ref qP, "qP");
            MEM.Alloc(Nx, Ny, ref qW, "aW");
            MEM.Alloc(Nx, Ny, ref qE, "aE");
            MEM.Alloc(Nx, Ny, ref qS, "aS");
            MEM.Alloc(Nx, Ny, ref qN, "aN");
        }
        /// <summary>
        /// Определяем значения вязкости и плотности в узлах расчетной области, 
        /// исходя из расположения донных отметок   
        /// </summary>
        public override void SetMuAndRho()
        {
            base.SetMuAndRho();
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            for (uint i = 0; i < Nx - 1; i++)
            {
                double xm = mesh.Xmin + i * dx;
                double midleZeta = (Zeta[i] + Zeta[i + 1]) * 0.5;
                for (int j = 0; j < Ny - 1; j++)
                {
                    double ym = mesh.Ymin + dy * j;
                    double ym1 = ym + dy;
                    if (ym <= midleZeta && midleZeta < ym1)
                    {
                        Water[i][j] = 1 - (midleZeta - ym) / dy;
                    }
                    else
                    {
                        if (midleZeta > ym1)
                            Water[i][j] = 0; // чистый песок
                        else
                            Water[i][j] = 1; // вода
                    }
                }
            }
            for (uint i = 1; i < Nx-1; i++)
            {
                for (int j = 1; j < Ny-1; j++)
                {
                    //qP[i][j] = 0.25 * (Epsilon[i - 1][j - 1] + Epsilon[i - 1][j] + Epsilon[i][j - 1] + Epsilon[i][j]);
                    //qE[i][j] = 0.5 * (Epsilon[i][j - 1] + Epsilon[i][j]);
                    //qW[i][j] = 0.5 * (Epsilon[i - 1][j - 1] + Epsilon[i - 1][j]);
                    //qN[i][j] = 0.5 * (Epsilon[i - 1][j] + Epsilon[i][j]);
                    //qS[i][j] = 0.5 * (Epsilon[i - 1][j - 1] + Epsilon[i][j - 1]);

                    qP[i][j] = 0.25 * (Water[i - 1][j - 1] + Water[i - 1][j] + Water[i][j - 1] + Water[i][j]);
                    qE[i][j] = 0.5 * (Water[i][j - 1] + Water[i][j]);
                    qW[i][j] = 0.5 * (Water[i - 1][j - 1] + Water[i - 1][j]);
                    qN[i][j] = 0.5 * (Water[i - 1][j] + Water[i][j]);
                    qS[i][j] = 0.5 * (Water[i - 1][j - 1] + Water[i][j - 1]);
                    mRho[i][j] = aP[i][j] * rho_w + (1 - aP[i][j]) * rho_s * (1-epsilon);
                    
                }
            }
            //LOG.Print("aP",aP);
            //LOG.Print("mRho", aP);
           // LOG.Print("mRho", Water);
        }
        protected virtual void CalkAk()
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = 1; j < Ny - 1; j++)
                {
                    Ap = mMu[i][j];
                    Ae = mMu[i + 1][j];
                    Aw = mMu[i - 1][j];
                    An = mMu[i][j + 1];
                    As = mMu[i][j - 1];

                    Qp = qP[i][j];
                    Qe = qE[i][j];
                    Qw = qW[i][j];
                    Qn = qN[i][j];
                    Qs = qS[i][j];

                    aE[i][j] = Qe * ((Ae + Ap) / 2.0) / (dx * dx);
                    aW[i][j] = Qw * ((Aw + Ap) / 2.0) / (dx * dx);
                    aS[i][j] = Qs * ((As + Ap) / 2.0) / (dy * dy);
                    aN[i][j] = Qn * ((An + Ap) / 2.0) / (dy * dy);
                    aP[i][j] = (Qe * aE[i][j] + Qw * aW[i][j] + Qn * aN[i][j] + Qs * aS[i][j]);
                    aQ[i][j] = Qp * mRho[i][j] * g * Params.J;

                }
           // LOG.Print("aP",aP);
        }
        /// <summary>
        /// Рассчитываем значения скоростей во внутренних узлах расчетной области методом Зейделя
        /// #5 Определяем граничные значения скоростей из граничных условий задачи
        /// #6 Находим максимальную невязку (разницу значений на текущей и предыдущей итерации в одном узле) во внутренних узлах расчетной сетки
        /// #7 Находим относительную невязку, частное модулей максимальной невязки и максимального значения в узлах
        /// #8 Если относительная невязка меньше значения погрешности - решение найдено, заканчиваем вычисление. Если больше – возврат к пункту 4
        /// </summary>
        public override void VelocityCalculation()   
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            CalkAk();
            double RelDis = 1;
            int steps = 0;
            for (int index = 0; index < 100000; index++)
            {
                double MaxDis = 0;
                MaxU = 0;
                for (int i = NxMin; i < Nx - 1; i++) // -> X
                    for (int j = 1; j < Ny - 1; j++) // -> Y
                    {
                        uBuff = mU[i][j];
                        bap = aP[i][j];
                        if (bap < MEM.Error9) 
                            continue;
                        fe = aE[i][j] * mU[i + 1][j];
                        fw = aW[i][j] * mU[i - 1][j];
                        fn = aN[i][j] * mU[i][j + 1];
                        fs = aS[i][j] * mU[i][j - 1];
                        mU[i][j] = (fe + fw + fn + fs + aQ[i][j]) / bap;
                        if (MaxDis < Math.Abs(uBuff - mU[i][j]))     // #6
                            MaxDis = Math.Abs(uBuff - mU[i][j]);
                        if (MaxU < Math.Abs(mU[i][j]))
                            MaxU = Math.Abs(mU[i][j]);
                    }
                // Определяем граничные значения скоростей 
                UBorders();                 // #5
                // Находим относительную невязку
                RelDis = MaxDis / MaxU;     // #7
                // Если относительная невязка
                // меньше заданного значения погрешности
                if (RelDis < MEM.Error8)         // #8
                {
                    //Console.Write("Условие достигнуто на {0} итерации", steps);
                    //Console.WriteLine();
                    break;
                }
                steps++;
            }
            LOG.Print("aE", aE);
            LOG.Print("aP", aP);
            LOG.Print("aN", aN);
            //LOG.Print("mU", mU);
        }
        /// <summary>
        /// #5 Определяем граничные значения скоростей из граничных условий задачи   
        /// </summary>
        public void UBorders() // #5
        {
            int Nx = Params.Nx;
            int Ny = Params.Ny;
            for (int ix = 0; ix < Nx; ix++)
            {
                // На  дне
                //if (mesh.BCType[0] == TypeBoundCond.Dirichlet)
                mU[ix][0] = 0; // Дирихле 
                               //else
                               //    mU[ix][0] = mU[ix][1]; // Нейман и ...
                               // На свободной поверхности потока
                               //if (mesh.BCType[2] == TypeBoundCond.Dirichlet)
                               //    mU[ix][Ny-1] = 0; // Дирихле 
                               //else
                mU[ix][Ny - 1] = mU[ix][Ny - 2];  // Нейман и ...
            }
            for (int iy = 0; iy < Ny; iy++) // Нейман по бокам        
            {
                mU[NxMin - 1][iy] = mU[NxMin][iy];
                mU[Nx - 1][iy] = mU[Nx - 2][iy];
            }
        }

    }
}
