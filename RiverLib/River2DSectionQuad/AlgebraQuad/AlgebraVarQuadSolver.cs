namespace RiverLib
{
    using System;
    using CommonLib;
    using CommonLib.Delegate;
    using MemLogLib;
    using MeshLib;
    using MeshLib.Mesh.RecMesh;

    public class AlgebraVarQuadSolver
    {
        /// <summary>
        /// Тип граничных условий на границе четрырехугольника
        /// </summary>
        TypeBoundCond[] BCType;
        /// <summary>
        /// Значения граничных условий на границе четрырехугольника
        /// </summary>
        BoundaryConditionsQuad BConditions = null;
        /// <summary>
        /// Регулярная расчетная сетка
        /// </summary>
        RectangleMesh mesh;

        /// <summary>
        /// Начальный интекс донной поверхности
        /// </summary>
        protected int[] bottonIndex;
        int NxMin;
        double[] DBCValue;
        double[] NBCValue;
        protected double dx, dy;
        protected int Nx, Ny;
        protected double[][] aP, aE, aW, aN, aS, aQ;
        protected double uBuff, bap, fe, fw, fn, fs, Ap, An, Ae, As, Aw, MaxU, Dis;
        public AlgebraVarQuadSolver(RectangleMesh imesh, TypeBoundCond[] BCType, IBoundaryConditions BConditions)
        {
            this.mesh = imesh;
            this.BCType = BCType;
            this.BConditions = (BoundaryConditionsQuad)BConditions;
            DBCValue = BConditions.GetValueDir();
            NBCValue = BConditions.GetValueNeu();
            this.Nx = mesh.Nx;
            this.Ny = mesh.Ny;
            this.dx = mesh.dx;
            this.dy = mesh.dy;
            Init();
        }

        protected void Init()
        {
            MEM.Alloc(Nx, Ny, ref aP, "aP");
            MEM.Alloc(Nx, Ny, ref aW, "aW");
            MEM.Alloc(Nx, Ny, ref aE, "aE");
            MEM.Alloc(Nx, Ny, ref aS, "aS");
            MEM.Alloc(Nx, Ny, ref aN, "aN");
            MEM.Alloc(Nx, Ny, ref aQ, "aQ");
        }
        /// <summary>
        /// Расчет коэффициентов
        /// </summary>
        /// <param name="mas"></param>
        public virtual void CalkAk(double[][] mas, int NxMin, int NyMin=1)
        {
            MEM.VAlloc(Nx, NyMin - 1, ref bottonIndex, "bottonIndex");
            this.NxMin = NxMin;
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = bottonIndex[i] + 1; j < Ny - 1; j++)
                {
                    Ap = mas[i][j];
                    Ae = mas[i + 1][j];
                    Aw = mas[i - 1][j];
                    An = mas[i][j + 1];
                    As = mas[i][j - 1];
                    aE[i][j] = ((Ae + Ap) / 2.0) / (dx * dx);
                    aW[i][j] = ((Aw + Ap) / 2.0) / (dx * dx);
                    aS[i][j] = ((As + Ap) / 2.0) / (dy * dy);
                    aN[i][j] = ((An + Ap) / 2.0) / (dy * dy);
                    aP[i][j] = aE[i][j] + aW[i][j] + aN[i][j] + aS[i][j];
                }
        }

        /// <summary>
        /// Расчет коэффициентов
        /// </summary>
        /// <param name="mas"></param>
        public virtual void CalkAk(double[][] mas,int [] bottonIndex, int NxMin)
        {
            this.bottonIndex = bottonIndex;
            this.NxMin = NxMin;
            for (int i = NxMin; i < Nx - 1; i++)        
                for (int j = bottonIndex[i] + 1; j < Ny - 1; j++)
                {
                    Ap = mas[i][j];
                    Ae = mas[i + 1][j];
                    Aw = mas[i - 1][j];
                    An = mas[i][j + 1];
                    As = mas[i][j - 1];
                    aE[i][j] = ((Ae + Ap) / 2.0) / (dx * dx);
                    aW[i][j] = ((Aw + Ap) / 2.0) / (dx * dx);
                    aS[i][j] = ((As + Ap) / 2.0) / (dy * dy);
                    aN[i][j] = ((An + Ap) / 2.0) / (dy * dy);
                    aP[i][j] = aE[i][j] + aW[i][j] + aN[i][j] + aS[i][j];
                }
        }
        public virtual void CalkRight(double[][] F)
        {
            for (int i = NxMin - 1; i < Nx; i++)
                for (int j = bottonIndex[i]; j < Ny; j++)
                {
                    aQ[i][j] = F[i][j];
                }
        }
        public virtual void CalkRight(double[][] F, double value)
        {
            for (int i = NxMin - 1; i < Nx; i++)
                for (int j = bottonIndex[i]; j < Ny; j++)
                {
                    aQ[i][j] = F[i][j] * value;
                }
        }
        public virtual void CalkRight(double F)
        {
            for (int i = NxMin - 1; i < Nx; i++)
                for (int j = bottonIndex[i]; j < Ny; j++)
                {
                    aQ[i][j] = F;
                }
        }
        /// <summary>
        /// Решение Системы линейных уравнений
        /// </summary>
        /// <param name="X"></param>
        /// <param name="CoundaryCondition"></param>
        public virtual void SystemSolver(ref double[][] X, BProc BoundaryCondition = null)
        {
            if (BoundaryCondition == null)
                BoundaryCondition = StandartBoundCondition;
            double RelDis;
            int steps = 0;
            for (int index = 0; index < 100000; index++)
            {
                double MaxDis = 0;
                MaxU = 0;
                for (int i = NxMin; i < Nx - 1; i++) // -> X
                    for (int j = bottonIndex[i] + 1; j < Ny - 1; j++)
                    {
                        uBuff = X[i][j];
                        bap = aP[i][j];
                        fe = aE[i][j] * X[i + 1][j];
                        fw = aW[i][j] * X[i - 1][j];
                        fn = aN[i][j] * X[i][j + 1];
                        fs = aS[i][j] * X[i][j - 1];
                        X[i][j] = (fe + fw + fn + fs + aQ[i][j]) / bap;
                        if (MaxDis < Math.Abs(uBuff - X[i][j]))
                            MaxDis = Math.Abs(uBuff - X[i][j]);
                        if (MaxU < Math.Abs(X[i][j]))
                            MaxU = Math.Abs(X[i][j]);
                    }
                // Определяем граничные значения скоростей 
                BoundaryCondition(ref X);
                // Находим относительную невязку
                RelDis = MaxDis / (MaxU + MEM.Error14);
                // Если относительная невязка
                // меньше заданного значения погрешности
                if (RelDis < MEM.Error8)
                    break;
                steps++;
            }
        }
        /// <summary>
        /// Определяем граничные значения 
        /// </summary>
        public virtual void StandartBoundCondition(ref double[][] X) // #5
        {
            #region Дно
            int bottom = 0;
            if (BCType[bottom] == TypeBoundCond.Dirichlet) 
            {
                for (int i = 0; i < Nx; i++)
                    X[i][0] = DBCValue[0]; // Дирихле 
            }
            if (BCType[bottom] == TypeBoundCond.Neumann)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][0] = X[i][1] - dy * NBCValue[bottom]; // Нейман по нормали 
            }
            #endregion 
            #region Крышка
            int top = 2;
            if (BCType[top] == TypeBoundCond.Dirichlet)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][Ny - 1] = DBCValue[top]; // Дирихле 
            }
            if (BCType[top] == TypeBoundCond.Neumann)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][Ny - 1] = X[i][Ny - 2] + dy * NBCValue[top]; // Дирихле 
            }
            #endregion
            #region Справа
            int right = 1;
            if (BCType[right] == TypeBoundCond.Dirichlet)
            {
                for (int j = 0; j < Ny; j++)
                    X[Nx - 1][j] = DBCValue[right];// Дирихле 
            }
            if (BCType[right] == TypeBoundCond.Neumann)
            {
                for (int j = 0; j < Ny; j++)
                    X[Nx - 1][j] = X[Nx - 2][j] + dx * NBCValue[right]; // Дирихле 
            }
            #endregion
            #region Слева
            int left = 3;
            if (BCType[left] == TypeBoundCond.Dirichlet)
            {
                for (int j = 0; j < Ny; j++)
                    X[NxMin - 1][j] = DBCValue[left];// Дирихле 
            }
            if (BCType[left] == TypeBoundCond.Neumann)
            {
                for (int j = 0; j < Ny; j++)
                    X[NxMin - 1][j] = X[NxMin][j] - dx * NBCValue[left]; // Дирихле 
            }
            #endregion
        }

        /// <summary>
        /// Определяем граничные значения 
        /// </summary>
        public virtual void VarBoundCondition(ref double[][] X) // #5
        {
            #region Дно
            int bottom = 0;
            if (BCType[bottom] == TypeBoundCond.Dirichlet)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][0] = DBCValue[bottom]; // Дирихле 
            }
            if (BCType[bottom] == TypeBoundCond.Neumann)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][0] = X[i][1] - dy * NBCValue[bottom]; // Нейман по нормали 
            }
            #endregion 
            #region Крышка
            int top = 2;
            if (BCType[top] == TypeBoundCond.Dirichlet)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][Ny - 1] = DBCValue[top]; // Дирихле 
            }
            if (BCType[top] == TypeBoundCond.Neumann)
            {
                for (int i = 0; i < Nx; i++)
                    X[i][Ny - 1] = X[i][Ny - 2] + dy * NBCValue[top]; // Дирихле 
            }
            #endregion
            #region Справа
            int right = 1;
            if (BCType[right] == TypeBoundCond.Dirichlet)
            {
                for (int j = 0; j < Ny; j++)
                    X[Nx - 1][j] = DBCValue[right];// Дирихле 
            }
            if (BCType[right] == TypeBoundCond.Neumann)
            {
                for (int j = 0; j < Ny; j++)
                    X[Nx - 1][j] = X[Nx - 2][j] + dx * NBCValue[right]; // Дирихле 
            }
            #endregion
            #region Слева
            int left = 3;
            if (BCType[left] == TypeBoundCond.Dirichlet)
            {
                for (int j = 0; j < Ny; j++)
                    X[NxMin - 1][j] = DBCValue[left];// Дирихле 
            }
            if (BCType[left] == TypeBoundCond.Neumann)
            {
                for (int j = 0; j < Ny; j++)
                    X[NxMin - 1][j] = X[NxMin][j] - dx * NBCValue[left]; // Дирихле 
            }
            #endregion
        }
    }
}
