namespace AlgebraLib.Sparse
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using MemLogLib;
    public class Sparse1DAlgebra : Algebra
    {
        //****************************************************************************************************************************
        // СЛАУ и вектора неизвестных (в том числе их копии с предыдущей итерации по нелинейности).
        //***************************************************************************************************************************
        /// <summary>
        /// Количество ненулевых элементов в строке матрицы.
        /// </summary>
        int[] iLCount = null;
        /// <summary>
        /// Номера столбцов ненулевых элементов матрицы.
        /// </summary>
        int[] jLCount = null;
        /// <summary>
        /// Количество ненулевых элементов в строке матрицы для верхней треугольник матрицы
        /// </summary>
        int[] iUCount = null;
        /// <summary>
        /// Номера столбцов ненулевых элементов в строке матрицы для верхней треугольник матрицы
        /// </summary>
        int[] jUCount = null;
        /// <summary>
        /// Диагональ матрицы.
        /// </summary>
        double[] diMatrix;
        /// <summary>
        ///  Нижний треугольник матрицы.
        /// </summary>
        double[] LMatrix;
        /// <summary>
        /// Верхний треугольник матрицы.
        /// </summary>
        double[] UMatrix;

        uint[][] AreaElems = null;
        public Sparse1DAlgebra(uint FN, uint[][] AreaElems) : base(null, FN)
        {
            if (AreaElems == null)
                throw new Exception("AreaElems == null в конструкторе Sparse1DAlgebra");
            
            this.AreaElems = AreaElems;
            //****************************************************************************************************************************
            //Формирование матрицы СЛАУ, выделение памяти под вектор правой части СЛАУ и вектора неизвестных.
            //Задание начальных приближений векторов неизвестных.
            //****************************************************************************************************************************
            CalkMatrixProfile(AreaElems);
            MEM.Alloc(FN, ref diMatrix);
            MEM.Alloc(FN, ref Right);
            MEM.Alloc(iLCount[FN], ref LMatrix);
            MEM.Alloc(iLCount[FN], ref UMatrix);
        }
        /// <summary>
        /// Расчет 
        /// </summary>
        /// <param name="CountElements"></param>
        /// <param name="FN"></param>
        /// <param name="nvtr"></param>
        /// <param name="iLCount"></param>
        /// <param name="jLCount"></param>
        /// <param name="iUCount"></param>
        /// <param name="jUCount"></param>
        public void CalkMatrixProfile(uint[][] AreaElems)
        {
            int CountElements = AreaElems.Length;

            List<int>[] lTable = new List<int>[FN];
            List<int>[] uTable = new List<int>[FN];
            for (int i = 0; i < FN; i++)
            {
                lTable[i] = new List<int>();
                uTable[i] = new List<int>();
            }
            // заносим в таблицу информацию о том какие узлы связаны с какими поскольку матрица имеет симметричный
            // профиль, то достаточно собрать информацию нужную для построения нижнего треугольника. Т.е. учитываем
            // связь двух узлов если глобальный номер первого узла(a) больше номера второго узла(b). В строке №a
            // хранится информация о всех узлах с которыми связан узел a.
            for (int i = 0; i < CountElements; i++)
            {
                int cu = AreaElems[i].Length;
                for (int j = 0; j < cu; j++)
                    for (int k = 0; k < cu; k++)
                        if (AreaElems[i][j] > AreaElems[i][k])
                            if (!lTable[AreaElems[i][j]].Contains((int)AreaElems[i][k]))
                                lTable[AreaElems[i][j]].Add((int)AreaElems[i][k]);
            }
            // выделяем память под массив iLCount
            iLCount = new int[FN + 1];

            // подсчитываем количество элементов в портрете матрицы
            int lTableCount = 0;
            for (int i = 0; i < FN; i++)
                lTableCount += lTable[i].Count;

            // выделяем память под массив jLCount
            jLCount = new int[lTableCount];

            // формируем iLCount и jLCount
            iLCount[0] = 0;
            iLCount[1] = 0;

            for (int i = 1, j = 0; i < FN; i++)
            {
                for (int k = 0; k < lTable[i].Count; k++)
                {
                    // заполняем jLCount
                    jLCount[j] = (int)lTable[i][k];
                    uTable[lTable[i][k]].Add(j);
                    j++;
                }
                // заполняем iLCount
                iLCount[i + 1] = iLCount[i] + lTable[i].Count;
            }

            iUCount = new int[FN + 1];

            int uTableCount = 0;
            for (int i = 0; i < FN; i++)
                uTableCount += uTable[i].Count;

            jUCount = new int[uTableCount];

            iUCount[0] = 0;
            for (int i = 0, j = 0; i < FN; i++)
            {
                for (int k = 0; k < uTable[i].Count; k++)
                {
                    jUCount[j] = (int)uTable[i][k];
                    j++;
                }
                iUCount[i + 1] = iUCount[i] + uTable[i].Count;
            }
        }
        
        public override void Clear()
        {
            for (int i = 0; i < FN; i++)
            {
                diMatrix[i] = 0;
                Right[i] = 0;
            }
            int Count = iLCount[FN];
            for (int i = 0; i < Count; i++)
            {
                LMatrix[i] = 0;
                UMatrix[i] = 0;
            }
        }

        //public virtual void AddToRight(double[] LRight, uint[] Adress)
        //public void AddRight(double[] right, int[] adress)
        //{
        //    for (int i = 0; i < adress.Length; i++)
        //        Right[adress[i]] += right[i];
        //}

        public void BCondition(int[] bcAdress, double[] bcValue)
        {
            for (int i = 0; i < bcAdress.Length; i++)
            {
                diMatrix[bcAdress[i]] = 1;
                Right[bcAdress[i]] = bcValue[i];
                for (int j = iLCount[bcAdress[i]]; j < iLCount[bcAdress[i] + 1]; j++)
                    LMatrix[j] = 0;
                for (int j = iUCount[bcAdress[i]]; j < iUCount[bcAdress[i] + 1]; j++)
                    UMatrix[jUCount[j]] = 0;
            }
        }
        public void BConditionZero(int[] bcAdress)
        {
            for (int i = 0; i < bcAdress.Length; i++)
            {
                diMatrix[bcAdress[i]] = 1;
                Right[bcAdress[i]] = 0;
                for (int j = iLCount[bcAdress[i]]; j < iLCount[bcAdress[i] + 1]; j++)
                    LMatrix[j] = 0;
                for (int j = iUCount[bcAdress[i]]; j < iUCount[bcAdress[i] + 1]; j++)
                    UMatrix[jUCount[j]] = 0;
            }
        }

        /// <summary>
        /// Решение СЛАУ с нижнетреугольной матрицей в разреженном строчном формате.        
        /// </summary>
        public void solveLowSLAE(uint dimension, double[] LMatrix, double[] diMatrix, double[] Right)
        {
            for (int i = 0; i < dimension; i++)
            {
                for (int j = iLCount[i]; j < iLCount[i + 1]; j++)
                    Right[i] -= LMatrix[j] * Right[jLCount[j]];
                Right[i] /= diMatrix[i];
            }
        }

        /// <summary>
        /// Решение СЛАУ с верхнетреугольной матрицей в разреженном строчном формате (на диагонали единицы).
        /// </summary>
        public void solveUpSLAE(uint dimension, double[] UMatrix, double[] Right)
        {
            for (uint i = dimension - 1; i > 0; i--)
                for (int j = iLCount[i + 1] - 1; j >= iLCount[i]; j--)
                    Right[jLCount[j]] -= UMatrix[j] * Right[i];
        }
        /// <summary>
        /// Неполная LU факторизация матрицы.        
        /// </summary>
        public void partFactorLU(uint dimension, double[] l, double[] di_f, double[] u)
        {
            for (int k = 0; k < dimension; k++)
            {
                for (int j = iLCount[k]; j < iLCount[k + 1]; j++)
                {
                    l[j] = LMatrix[j];
                    u[j] = UMatrix[j];

                    for (int i = iLCount[k]; i < j; i++)
                        for (int m = iLCount[jLCount[j]]; m < iLCount[jLCount[j] + 1]; m++)
                            if (jLCount[i] == jLCount[m])
                            {
                                l[j] -= l[i] * u[m];
                                u[j] -= u[i] * l[m];
                                m = iLCount[jLCount[j] + 1];
                            }
                            else
                                if (jLCount[i] < jLCount[m])
                                m = iLCount[jLCount[j] + 1];

                    u[j] = u[j] / di_f[jLCount[j]];
                }

                di_f[k] = diMatrix[k];
                for (int i = iLCount[k]; i < iLCount[k + 1]; i++)
                    di_f[k] -= l[i] * u[i];
            }
        }

        /// <summary>
        /// Умножение матрицы на вектор.
        /// </summary>
        public void matrixMultByVector(uint dimension, double[] vector, double[] result)
        {
            for (int i = 0; i < dimension; i++)
                result[i] = vector[i] * diMatrix[i];

            for (int i = 0; i < dimension; i++)
                for (int j = iLCount[i]; j < iLCount[i + 1]; j++)
                {
                    result[i] += vector[jLCount[j]] * LMatrix[j];
                    result[jLCount[j]] += vector[i] * UMatrix[j];
                }
        }

        //================================================================================================================================
        //Индекс элемента матрицы A[i,j] в массиве LMatrix.
        //================================================================================================================================
        public int position(uint i, uint j)
        {
            int index;
            for (index = iLCount[i]; jLCount[index] != j && index < iLCount[i + 1]; index++) ;
            return index != iLCount[i + 1] ? index : -1;
        }

        #region общие абстрактные методы
        /// <summary>
        /// решение СЛАУ
        /// </summary>
        /// <param name="X"></param>
        protected override void solve(ref double[] x)
        {
            uint dimension = FN;
            double[] r;                  //Вектор невязки.
            double[] p;                  //Вспомогательный вектор метода.
            double[] z;                  //Вспомогательный вектор метода.

            double[] l;                  //Нижня треугольная матрица факторизации без диагонали.
            double[] di_f;               //Диагональ нижней треугольной матрицы факторизации.
            double[] u;                  //Верхняя треугольная матрица факторизации.
            double alpha, beta;          //Вспомогательные константы метода.
            double norm_f, norm_r;       //Норма правой части, норма невязки.
            double scalar;               //Значение скалярного произведения двух векторов.

            double[] temp1, temp2;       // Вектора для хранения промежуточных результатов вычислений.

            temp1 = new double[dimension];
            temp2 = new double[dimension];

            l = new double[iLCount[dimension]];
            u = new double[iLCount[dimension]];
            di_f = new double[dimension];

            partFactorLU(dimension, l, di_f, u);

            r = new double[dimension];
            p = new double[dimension];
            z = new double[dimension];

            //вычисление нормы Right
            norm_f = 0;
            for (int i = 0; i < dimension; i++)
                norm_f += Right[i] * Right[i];
            norm_f = Math.Sqrt(norm_f);

            //нахождение r нуле-вого
            matrixMultByVector(dimension, x, r);
            for (int i = 0; i < dimension; i++)
                r[i] = Right[i] - r[i];
            solveLowSLAE(dimension, l, di_f, r);

            //вычисление нормы r
            norm_r = 0;
            for (int i = 0; i < dimension; i++)
                norm_r += r[i] * r[i];
            norm_r = Math.Sqrt(norm_r);

            //нахождение z нулевого
            //memcpy(z, r, dimension * sizeof(double));
            MEM.MemCopy(ref z, r);

            solveUpSLAE(dimension, u, z);

            //нахождение p нуле-вого
            matrixMultByVector(dimension,  z, p);
            solveLowSLAE(dimension,  l, di_f, p);

            // итерационный шаг
            for (int curIter = 0; curIter < 10000 && norm_r / norm_f > 1e-8; curIter++)
            {
                //нахождение alpha i-го
                alpha = p[0] * r[0];
                scalar = p[0] * p[0];
                for (int i = 1; i < dimension; i++)
                {
                    alpha += p[i] * r[i];
                    scalar += p[i] * p[i];
                }
                alpha /= scalar;

                //нахождение x i-го
                for (int i = 0; i < dimension; i++)
                    x[i] += alpha * z[i];

                //нахождение r i-го
                for (int i = 0; i < dimension; i++)
                    r[i] -= alpha * p[i];

                //вспомогательный вектор для нахождения beta
                MEM.MemCopy(ref temp1, r);

                solveUpSLAE(dimension, u, temp1);

                matrixMultByVector(dimension,  temp1, temp2);

                solveLowSLAE(dimension, l, di_f, temp2);

                //нахождение beta i-го
                beta = p[0] * temp2[0];
                for (int i = 1; i < dimension; i++)
                    beta += p[i] * temp2[i];
                beta /= -scalar;

                //нахождение p i-го
                for (int i = 0; i < dimension; i++)
                {
                    p[i] *= beta;
                    p[i] += temp2[i];
                }

                //вспомогательный вектор для нахождения z i-го
                MEM.MemCopy(ref temp1, r);

                solveUpSLAE(dimension, u, temp1);

                //нахождение z i-го
                for (int i = 0; i < dimension; i++)
                {
                    z[i] *= beta;
                    z[i] += temp1[i];
                }

                //вычисление нормы r
                norm_r = 0;
                for (int i = 0; i < dimension; i++)
                    norm_r += r[i] * r[i];
                norm_r = Math.Sqrt(norm_r);
            }
        }
        /// <summary>
        /// нормировка системы для малых значений матрицы и правой части
        /// </summary>
        protected override void SystemNormalization() { }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public override IAlgebra Clone()
        {
            return new Sparse1DAlgebra(this.FN, this.AreaElems);
        }
        /// <summary>
        /// Сборка САУ по строкам (не для всех решателей)
        /// </summary>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части строки</param>
        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
            result.errorType = ErrorType.methodCannotSolveSuchSLAEs;
            throw new Exception("метод GetStringSystem еще не реализован для данного типа");
        }
        /// <summary>
        /// Получить строку (не для всех решателей)
        /// </summary>
        /// <param name="IndexRow">Индекс получемой строки системы</param>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="R">Значение правой части</param>
        public override void GetStringSystem(uint IndexRow, ref double[] ColElems, ref double R)
        {
            result.errorType = ErrorType.methodCannotSolveSuchSLAEs;
            throw new Exception("метод GetStringSystem еще не реализован для данного типа");
        }

        /// <summary>
        /// Формирование матрицы системы
        /// </summary>
        /// <param name="LMartix">Локальная матрица</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void AddToMatrix(double[][] lMatrix, uint[] adress)
        {
            for (int i = 0; i < adress.Length; i++)
            {
                for (int j = 0; j < adress.Length; j++)
                {
                    if (adress[i] == adress[j])
                        diMatrix[adress[i]] += lMatrix[i][j];
                    else
                        if (adress[i] > adress[j])
                        LMatrix[position(adress[i], adress[j])] += lMatrix[i][j];
                    else
                        UMatrix[position(adress[j], adress[i])] += lMatrix[i][j];
                }
            }
        }
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        /// <param name="Condition">Значения неизвестных по адресам</param>
        /// <param name="Adress">Глабальные адреса</param>
        public override void BoundConditions(double[] Conditions, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                diMatrix[Adress[i]] = 1;
                Right[Adress[i]] = Conditions[i];
                for (int j = iLCount[Adress[i]]; j < iLCount[Adress[i] + 1]; j++)
                    LMatrix[j] = 0;
                for (int j = iUCount[Adress[i]]; j < iUCount[Adress[i] + 1]; j++)
                    UMatrix[jUCount[j]] = 0;
            }
        }
        /// <summary>
        /// Удовлетворение ГУ (с накопительными вызовами)
        /// </summary>
        public override void BoundConditions(double Condition, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                diMatrix[Adress[i]] = 1;
                Right[Adress[i]] = Condition;
                for (int j = iLCount[Adress[i]]; j < iLCount[Adress[i] + 1]; j++)
                    LMatrix[j] = 0;
                for (int j = iUCount[Adress[i]]; j < iUCount[Adress[i] + 1]; j++)
                    UMatrix[jUCount[j]] = 0;
            }
        }
        /// <summary>
        /// Операция определения невязки R = Matrix X - Right
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        public override void getResidual(ref double[] R, double[] X, int IsRight = 1)
        {

        }
        /// <summary>
        /// Вывод САУ на КОНСОЛЬ
        /// </summary>
        /// <param name="flag">количество знаков мантисы</param>
        /// <param name="color">длина цветового блока</param>
        public override void Print(int flag = 0, int color = 1)
        { 
        }
        #endregion
    }
}
