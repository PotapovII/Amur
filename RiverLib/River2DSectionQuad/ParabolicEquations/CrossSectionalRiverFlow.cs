//-------------------------------------------------------------------------------------
// Прототип решателя общего вида
// 09 09 2023
//-------------------------------------------------------------------------------------
namespace RiverLib.River2DSectionQuad.ParabolicEquations
{
    using MemLogLib;
    using MeshLib;
    using System;

    /// <summary>
    /// Расчет парамета диффузии и правой части
    /// </summary>
    /// <param name="F"></param>
    /// <param name="F0"></param>
    /// <param name="Dif"></param>
    /// <param name="Sc"></param>
    public delegate void calkMass(int IndexTask, ARectangleMesh mesh, double[][][] F, double[][][] F0, ref double[][] Dif, ref double[][] Sc);

    /// <summary>
    /// поток  реки в поперечном створе
    /// </summary>
    [Serializable]
    public class CrossSectionalRiverFlow
    {
        #region Внешнии переменные используемые в TaskLink
        /// <summary>
        /// Поле скоростей
        /// </summary>
        public double[][] mU0;
        /// <summary>
        /// Поле вязкостей текущее и с предыдущей итерации
        /// </summary>
        public double[][] mMu0;
        /// <summary>
        /// Диффузионные коэффициенты 
        /// </summary>
        double[][] Dif = null;
        /// <summary>
        /// Правая часть
        /// </summary>
        double[][] Sc = null;
        #endregion

        /// <summary>
        /// шаг по времени
        /// </summary>
        public double dtime;
        /// <summary>
        /// Количество узлов по длине области
        /// </summary>
        public int Nx;
        /// <summary>
        /// Количество узлов по высоте области
        /// </summary>
        public int Ny;

        protected double dx;
        protected double dy;

        /// <summary>
        /// названия полей
        /// </summary>
        public string[] Names = null;
        /// <summary>
        /// Стек неизвестных
        /// </summary>
        [NonSerialized]
        double[][][] mF = null;
        double[][] F = null;
        /// <summary>
        /// Стек неизвестных на предыдущей итерации
        /// </summary>
        [NonSerialized]
        double[][][] mF0 = null;
        //double[][] F0 = null;
        [NonSerialized]
        protected ParabolicEquationsTask_FVM[] tasks;
        /// <summary>
        /// Расчетная сетка области для задачи вычисления поля скорости в створе
        /// </summary>
        [NonSerialized] 
        protected ARectangleMesh[] meshs = null;
        
        /// <summary>
        /// Количество итераций по нелинейности
        /// </summary>
        public int CountNoLineIterations = 3;
        /// <summary>
        /// Релаксация полей
        /// </summary>
        protected double relax = 0.25;
        /// <summary>
        /// Релаксация полей
        /// </summary>
        protected double ErrorNL;


        public CrossSectionalRiverFlow(double[][] mU, double[][] mMu,
                                        ARectangleMesh mesh, ARectangleMesh meshMu,
                                        ParabolicEquationsTask_FVM taskU,
                                        ParabolicEquationsTask_FVM taskMu,
                                        double dtime, string NamesU, string NamesMu, double errorNL = 0.001)
        {
            ErrorNL = errorNL;
            dx = mesh.dx;
            dy = mesh.dy;
            Nx = mesh.Nx;
            Ny = mesh.Ny;

            Names = new string[2] { NamesU, NamesMu };
            mF = new double[2][][];
            mF0 = new double[2][][];
            meshs = new ARectangleMesh[2];
            // Стек неизвестных
            mF[0] = mU;
            mF[1] = mMu;
            // Стек неизвестных на предыдущей итерации
            MEM.MemCopy(ref mU0, mU);
            MEM.MemCopy(ref mMu0, mMu);
            mF0[0] = mU0;
            mF0[1] = mMu0;
            meshs[0] = mesh;
            meshs[1] = meshMu;
            tasks = new ParabolicEquationsTask_FVM[2];
            tasks[0] = taskMu;
            tasks[1] = taskU;
            ErrorNL = errorNL;
        }
        /// <summary>
        /// Относительная погрешность
        /// </summary>
        /// <param name="testIndes"></param>
        /// <returns></returns>
        public double ErrorAssessment(int testIndes = 0)
        {
            double error = 0;
            double eMax = MEM.Error10;
            double e,em;
            for (uint i = 0; i < Nx; i++)
                for (int j = meshs[testIndes].Y_init[i]; j < Ny; j++)
                {
                    e = Math.Abs(mF[testIndes][i][j] - mF0[testIndes][i][j]);
                    em =  Math.Abs(mF[testIndes][i][j]);
                    if (e> error)
                        error = e;
                    if (em > eMax)
                        eMax = em;
                }
            return error/ eMax;
        }
        /// <summary>
        /// Модель турбулентности Спаларта-Аллмараса
        /// </summary>
        public virtual void CalkTask(calkMass[] funU)
        {
            try
            {
                // Цикл итераций по нелинейности
                for (int iter = 0; iter < CountNoLineIterations; iter++)
                {
                    // Цикл по подзадачам u, mU, 
                    for (int IndexTask = 0; IndexTask < F.Length; IndexTask++)
                    {
                        funU[IndexTask](IndexTask,meshs[IndexTask], mF, mF0, ref Dif, ref Sc);
                        tasks[IndexTask].TaskSolve(Dif, Sc, ref mF[IndexTask], ref F[IndexTask]);
                        RelaxMu(ref mF[IndexTask], mF0[IndexTask], relax, meshs[IndexTask].Y_init);
                    }
                    // Коррекция вычисляяемых граничных условий
                    double cec = ErrorAssessment();
                    if (cec < MEM.Error3)
                        break;
                }
            }
            catch (Exception ep)
            {
                Console.WriteLine("Ошибка " + ep.Message);
            }
        }
        /// <summary>
        /// Наложение штрафа на поле
        /// </summary>
        /// <param name="mas"></param>
        /// <param name="weight"></param>
        protected void RelaxMu(ref double[][] mas, double[][] mas0, double relax, int[] Y_indexs)
        {
            for (uint i = 0; i < Nx; i++)
                for (int j = Y_indexs[i]; j < Ny; j++)
                {
                    mas[i][j] = mas[i][j] * relax + (1 - relax) * mas0[i][j];
                    mas0[i][j] = mas[i][j];
                }
        }
    }
}
