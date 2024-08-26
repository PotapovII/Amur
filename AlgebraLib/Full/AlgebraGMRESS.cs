//---------------------------------------------------------------------------
//                     - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                            28 07 2021        
//                    реализация:  Потапов И.И.
//---------------------------------------------------------------------------
namespace AlgebraLib
{
    using MemLogLib;
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Метод Гаусса для решения САУ по столбцам, 
    /// прямоугольная матрица, выбор гл. элемента
    /// </summary>
    public class AlgebraGMRESS : AFullAlgebra
    {
        /// <summary>
        /// Подпространство M
        /// </summary>
        public int M;
        double[] Q = null;
        double[] D = null;
        double[] R = null;
        double[] V = null;
        double[] H = null;
        double[] PreCon = null;
        /// <summary>
        /// Название метода
        /// </summary>
        public new string Name { get { return ""; } }
        public AlgebraGMRESS(uint NN, int M = 10) : base(new AlgebraResultIterative(), NN)
        {
            this.M = M;
            name = "Метод GMRESS для полной матрицы";
            result.Name = name;
            SetAlgebra(result, NN);
        }

        public override void SetAlgebra(IAlgebraResult res, uint NN)
        {
            try
            {
                if (NN != FN)
                    base.SetAlgebra(res, NN);
                MEM.AllocClear(FN, ref Q);
                MEM.AllocClear(FN, ref D);
                MEM.AllocClear(FN, ref PreCon);
                MEM.AllocClear(M + 1, ref R);
                MEM.AllocClear((int)FN * M, ref V);
                MEM.AllocClear(M * (M + 1), ref H);
            }
            catch (Exception ex)
            {
                result.errorType = ErrorType.outOfMemory;
                result.Message = ex.Message;
            }
        }


        /// <summary>
        /// Решение САУ
        /// </summary>
        /// <param name="X">Вектор в который возвращается результат решения СЛАУ</param>
        protected override void solve(ref double[] X)
        {
            MEM.Alloc<double>((int)N, ref X);
            // Диагональное предобуславливание
            for (int i = 0; i < FN; i++)
                PreCon[i] = 1.0 / Matrix[i][i];



            double tem = 1, res = 0, res0, ccos, ssin, resCheck = 0;
            int ii, it, nk, i0, im;

            double Error = MEM.Error9;


            // Вычисление начальной ошибки
            matrixVectorMult(X, ref Q);

            for (ii = 0; ii < FN; ii++)
            {
                D[ii] = Right[ii] - Q[ii];
                Q[ii] = PreCon[ii] * D[ii];
                res += Q[ii] * Q[ii];
            }

            res = Math.Sqrt(res);
            res0 = res;
            // итерационное решение
            for (it = 0; it < 15 * FN; it++)
            {
                nk = M;
                // Метод Арнольди (который использует метод ортогонализации Грама-Шмидта)
                for (ii = 0; ii < FN; ii++)
                    Q[ii] = Q[ii] / res;
                for (ii = 0; ii <= nk; ii++)
                    R[ii] = 0;
                R[0] = res;
                for (int j = 0; j < M; j++)
                {
                    for (ii = 0; ii < FN; ii++)
                        V[ii * M + j] = Q[ii];

                    matrixVectorMult(Q, ref D);

                    for (ii = 0; ii < FN; ii++)
                        Q[ii] = PreCon[ii] * D[ii];
                    for (int i = 0; i <= j; i++)
                    {
                        H[i * M + j] = 0;
                        for (ii = 0; ii < FN; ii++)
                            H[i * M + j] += Q[ii] * V[ii * M + i];
                        for (ii = 0; ii < FN; ii++)
                            Q[ii] -= H[i * M + j] * V[ii * M + i];
                    }
                    tem = 0;
                    for (ii = 0; ii < FN; ii++) tem += Q[ii] * Q[ii];
                    tem = Math.Sqrt(tem);
                    H[(j + 1) * M + j] = tem;
                    if (tem < Error)
                    {
                        nk = j + 1;
                        goto l5;
                    }
                    for (ii = 0; ii < FN; ii++)
                        Q[ii] = Q[ii] / tem;
                }
            l5:
                // LOG.Print("H", H, nk, 5);
                // приведение к треугольной форме методом вращений
                for (int i = 0; i < nk; i++)
                {
                    im = i + 1;
                    tem = (1.0) / (Math.Sqrt(H[i * M + i] * H[i * M + i] + H[im * M + i] * H[im * M + i]));
                    ccos = H[i * M + i] * tem;
                    ssin = -H[im * M + i] * tem;
                    for (int j = i; j < nk; j++)
                    {
                        tem = H[i * M + j];
                        H[i * M + j] = ccos * tem - ssin * H[im * M + j];
                        H[im * M + j] = ssin * tem + ccos * H[im * M + j];
                    }
                    R[im] = ssin * R[i];
                    R[i] = R[i] * ccos;
                }
                //LOG.Print("H", H, nk, 5);
                //LOG.Print("H", R, 5);
                // решение линейной системы для поиска вессов
                for (int i = (nk - 1); i >= 0; i--)
                {
                    R[i] = R[i] / H[i * M + i];
                    for (i0 = (i - 1); i0 >= 0; i0--)
                        R[i0] = R[i0] - H[i0 * M + i] * R[i];
                }
                // поправка решения
                for (int i = 0; i < nk; i++)
                {
                    for (ii = 0; ii < FN; ii++)
                        X[ii] += R[i] * V[ii * M + i];
                }
                // новая невязка решения и стоп тесты
                matrixVectorMult(X, ref Q);
                res = 0;
                for (ii = 0; ii < FN; ii++)
                {
                    D[ii] = Right[ii] - Q[ii];
                    Q[ii] = PreCon[ii] * D[ii];
                    res += Q[ii] * Q[ii];
                }
                res = Math.Sqrt(res);
                resCheck = res / res0;
                if (resCheck < Error)
                    break;
            }
            ((AlgebraResultIterative)result).Iterations = it;
            ((AlgebraResultIterative)result).ratioError = resCheck;
            ((AlgebraResultIterative)result).Error_L2 = res;
            ((AlgebraResultIterative)result).Error0_L2 = res0;
            result.Message = " M = " + M.ToString();
        }
        public override IAlgebra Clone()
        {
            return new AlgebraGMRESS(N);
        }
    }
}
