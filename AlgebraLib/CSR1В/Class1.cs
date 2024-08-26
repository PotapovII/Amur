//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AlgebraLib
//{
//    using CommonLib;
//    class AlgebraCRS1D_BeCG : Algebra
//    {
//        int n = 5;
//        int nz = 13;
//        double[] A = { 31, 24, 31, 41, 31, 4, 4, 31, 1, 21, 1, 1, 1 };
//        int[] I = { 0, 1, 2, 3, 4, 0, 1, 2, 3, 1, 2, 3, 4 };
//        int[] J = { 0, 1, 2, 3, 4, 1, 2, 3, 4, 0, 1, 2, 3 };
//        int[] IA;
//        int[] JA;
//        double[] AN;
//        double[] b;
//        double[] x = { 0, 0, 0, 0, 0 };
//        double[] xx = { 1, 1, 1, 1, 1 };
//        double[] work;
//        int niter, rc;
//        public AlgebraCRS1D_BeCG(uint N)
//        {

//        }

//        protected override void solve(ref double[] X)
//        {
//            b = new double[n];
//            work = new double[6 * n];

//            sp_create(n, nz, IA, JA, AN);
//            sp_convert(nz, A, I, J, n, IA, JA, AN);

//            sp_mult_col(IA, JA, AN, xx, n, b);

//            rc = iter_bicg(IA, JA, AN, b, n, 1e-3, 20, x,
//                            null, null, null, null, null,
//                            null, null, null, ref niter, work);
//        }

//        /**
//  Решение разреженной системы линейных уравнений \f$Ax=b\f$ методом
//  бисопряженных градиентов с предобусловливанием. Метод останавливается,
//  как только относительная норма невязки \f$\|b-Ax\|_2/(\|b\|_2+1)\f$ не станет
//  меньше заданного допуска \f$tol\f$ или количество итераций не превысит
//  \f$maxiter\f$.

//  - Вход:
//        - \f$IA\f$, \f$JA\f$, \f$AN\f$ - разреженное представление матрицы \f$A\f$ в RR(C)U-формате
//        - \f$b\f$ - вектор правых частей системы
//        - \f$n\f$ - порядок системы
//        - \f$tol\f$ - допуск на относительную норму невязки \f$\|b-Ax\|_2/(\|b\|_2+1)\f$
//        - \f$maxiter\f$ - максимальное количество итераций  
//        - \f$x\f$ - начальное приближение решения
//        - \f$IM\f$, \f$JM\f$, \f$MN\f$, \f$DN\f$ - разреженное представление матрицы 
//          \f$M\f$ в RR(U)U-формате 
//          (см. ниже варианты использования предобусловливателя)
//        - \f$IK\f$, \f$JK\f$, \f$KN\f$, \f$DK\f$ - разреженное представление матрицы 
//          \f$K\f$ в RR(U)U-формате
//          (см. ниже варианты использования предобусловливателя)
//        - \f$work\f$ - рабочий массив длины не менее \f$6n\f$
//  - Выход:
//        - \f$x\f$ - найденное решение
//        - \f$niter\f$ - количество выполненных итераций

//  При успешном завершении функция возвращает \f$1\f$.
//  Если за \f$maxiter\f$ итераций заданная точность не достигнута, то
//  функция возвращает \f$0\f$.

//  Возможны следующие варианты использования предобусловливателя:
//  - Обусловливатель не используется. В этом случае входной параметр
//    \f$MD\f$ должен быть равен \f$NULL\f$. Значения параметров \f$IM\f$,
//    \f$JM\f$, \f$MN\f$, \f$IK\f$, \f$JK\f$, \f$KN\f$, \f$KD\f$ произвольно
//    (например, они тоже равны \f$NULL\f$).
//  - В качестве предобусловливателя используется диагональная матрица
//    \f$D\f$. В этом случае \f$MD\f$ должен указывать на диагональные элементы
//    матрицы \f$D\f$. Входной параметр \f$MN\f$ должен быть равен \f$NULL\f$.
//    Значение параметров \f$IM\f$, \f$JM\f$, \f$IK\f$, \f$JK\f$, \f$KN\f$,
//    \f$KD\f$ произвольно (например, они равны \f$NULL\f$).
//  - В качестве предобусловливателя используется матрица \f$M\times K\f$,
//    где \f$M\f$ - нижнетреугольная разреженная матрица, \f$K\f$ -
//    верхнетреугольная разреженная матрица. Их RR(U)U-представления хранятся
//    соответственно в массивах \f$IM\f$, \f$JM\f$, \f$MN\f$, \f$MD\f$ и
//    \f$IK\f$, \f$JK\f$, \f$KN\f$, \f$KD\f$.

//  см. [BelovZolotykh]

//  Трудоемкость:

//  \todo Необходимо реализовать пп. 2, 3 вариантов использования предобусловливателя 
//        (т.е. действительно предоставить возможность с ним работать).
//        Для этого также необходимо написать 4 функции для решения треугольных
//        разреженных систем (нижняя треугольная, верхняя треугольная,
//        не транспонированная, транспонированная:
//        sp_solve_lower, sp_solve_lower_trans, sp_solve_upper, sp_solve_upper_trans).
//        Также нужно написать функцию, реализующую метод сопряженных градиентов
//        для симметричной положительно определенной системы.
//*/
//        int iter_bicg(int[] IA,int[] JA,double[] AN, double[] b, int n,
//                      double tol,int maxiter,double[] x,int[] IM,int[] JM,double[] MN,double[] MD,
//                      int[] IK,int[] JK,double[] KN,
//                      double[] KD,ref int niter,double[] work)
//        {
//            double[] r;
//            double[] r_tilde;
//            double[] p;
//            double[] p_tilde;
//            double[] z;
//            double[] z_tilde;
//            double r_tilde_z, r_tilde_z_new, norm_b, rel_tol, alpha, beta;
//            int j;
//            r = work;
//            //r_tilde = r + n;
//            //p = r_tilde + n;
//            //p_tilde = p + n;
//            //z = p_tilde + n;
//            //z_tilde = z + n;
//            r_tilde = r;
//            p = r_tilde;
//            p_tilde = p;
//            z = p_tilde;
//            z_tilde = z;

//            sp_mult_col(IA, JA, AN, x, n,ref r);
//            cblas_daxpy(n, -1, b, 1,ref r, 1);
//            cblas_dscal(n, -1,ref r, 1);

//            cblas_dcopy(n, r, 1,ref r_tilde, 1);

//            cblas_dcopy(n, r, 1,ref p, 1); /* нужно: p = M\r; */
//            cblas_dcopy(n, r_tilde, 1,ref p_tilde, 1); /* нужно: p_tilde = r_tilde/M; */

//            cblas_dcopy(n, p, 1,ref z, 1);
//            cblas_dcopy(n, p_tilde, 1,ref z_tilde, 1);
//            r_tilde_z = cblas_ddot(n, r_tilde, 1, z, 1);

//            norm_b = cblas_dnrm2(n, b, 1);
//            if (norm_b == 0)
//            {
//                for (j = 0; j < n; j++)
//                    x[j] = 0;
//                return 0;
//            }

//            rel_tol = (norm_b + 1) * tol;

//            niter = 0;

//            while (cblas_dnrm2(n, r, 1) > rel_tol)
//            {
//                niter++;
//                if (niter > maxiter)
//                    return 0;

//                sp_mult_col(IA, JA, AN, p, n, ref z_tilde); /* here z_tilde is used as temp value A*p */

//                alpha = r_tilde_z / cblas_ddot(n, p_tilde, 1, z_tilde, 1);
//                cblas_daxpy(n, -alpha, z_tilde, 1,ref r, 1);

//                sp_mult_row(IA, JA, AN, p_tilde, n, n, z_tilde);
//                cblas_daxpy(n, -alpha, z_tilde, 1,ref r_tilde, 1);

//                cblas_daxpy(n, alpha, p, 1,ref x, 1);

//                cblas_dcopy(n, r, 1,ref z, 1); /* нужно: z = M\r; */
//                cblas_dcopy(n, r_tilde, 1,ref z_tilde, 1); /* нужно: z_tilde = r_tilde/M; */
//                r_tilde_z_new = cblas_ddot(n, r_tilde, 1, z, 1);

//                beta = r_tilde_z_new / r_tilde_z;
//                cblas_dscal(n, beta,ref p, 1);
//                cblas_daxpy(n, 1, z, 1,ref p, 1);

//                cblas_dscal(n, beta,ref p_tilde, 1);
//                cblas_daxpy(n, 1, z_tilde, 1,ref p_tilde, 1);

//                r_tilde_z = r_tilde_z_new;
//            }

//            return 1;
//        }
//        /// <summary>
//        /// Вычисляет скалярное произведение двух векторов (с двойной точностью).
//        /// </summary>
//        /// <param name="n">Количество элементов в векторах.</param>
//        /// <param name="a">Вектор a</param>
//        /// <param name="incX">Шагай внутри X. Например, если 7, используется каждый седьмой элемент.incX</param>
//        /// <param name="b">Вектор b</param>
//        /// <param name="incY">Шагай внутри Y. Например, если 7, используется каждый седьмой элемент.incY</param>
//        /// <returns></returns>
//        double cblas_ddot(int n, double[] a, int incX, double[] b, int incY)
//        {
//            double sum = 0;
//            int i = 0;
//            int j = 0;
//            for (int k=0; k<n; k++)
//            {
//                sum += a[i] * b[j];
//                i += incX;
//                j += incY;
//            }
//            return sum;
//        }
//        /// <summary>
//        /// Вычисляет L2-норму(евклидова длина) вектора(двойная точность).
//        /// </summary>
//        /// <param name="n"></param>
//        /// <param name="a"></param>
//        /// <param name="incX"></param>
//        /// <returns></returns>
//        double cblas_dnrm2(int n, double[] a, int incX)
//        {
//            double sum = 0;
//            int i = 0;
//            for (int k = 0; k < n; k++)
//            {
//                sum += a[i] * a[i];
//                i += incX;
//            }
//            return Math.Sqrt( sum );
//        }

//        /// <summary>
//        /// По возвращении содержимое вектора b заменяется результатом.Вычисленное значение равно(alpha* a[i]) + b[i].
//        /// </summary>
//        /// <param name="n">Количество элементов в векторах.</param>
//        /// <param name="alpha">Коэффициент масштабирования для значений в a.</param>
//        /// <param name="a"></param>
//        /// <param name="incX">Шагай внутри X. Например, если 7, используется каждый седьмой элемент.incX</param>
//        /// <param name="b"></param>
//        /// <param name="incY"></param>
//        void cblas_daxpy(int n, double alpha, double[] X, int incX, ref double[] Y, int incY)
//        {
//            int i = 0;
//            int j = 0;
//            for (int k = 0; k < n; k++)
//            {
//                Y[j] += alpha * X[i];
//                i += incX;
//                j += incY;
//            }
//        }

//        /// <summary>
//        /// Умножает каждый элемент вектора на константу(с двойной точностью).
//        /// </summary>
//        /// <param name="n">Количество элементов в векторах.</param>
//        /// <param name="b">Постоянный коэффициент масштабирования, на который нужно умножать</param>
//        /// <param name="a">Вектор a</param>
//        /// <param name="incX">Шагай внутри X. Например, если 7, используется каждый седьмой элемент.incX</param>

//        void cblas_dscal(int n,double b, ref double[] a, int incX)
//        {
//            int i = 0;
//            for (int k = 0; k < n; k++)
//            {
//                a[i] *= b;
//                i += incX;
//            }
//        }
//        /// <summary>
//        /// Копирует вектор в другой вектор(с двойной точностью).
//        /// </summary>
//        /// <param name="n">Количество элементов в векторах.</param>
//        /// <param name="a">Исходный вектор  a</param>
//        /// <param name="incX">Шагай внутри X. Например, если 7, используется каждый седьмой элемент.incX</param>
//        /// <param name="b">Вектор назначения b</param>
//        /// <param name="incY">Шагай внутри Y. Например, если 7, используется каждый седьмой элемент.incY</param>
//        /// <returns></returns>
//        void cblas_dcopy(int n, double[] a, int incX,ref double[] b, int incY)
//        {
//            int i = 0;
//            int j = 0;
//            for (int k = 0; k < n; k++)
//            {
//                b[j] = a[i];
//                i += incX;
//                j += incY;
//            }
//        }
//        //               _ __X: UnsafePointer<Double>!,
//        //               _ __incX: Int32,
//        //               _ __Y: UnsafeMutablePointer<Double>!,
//        //               _ __incY: Int32)
//        //            N
//        //Количество элементов в векторах.

//        //X
//        //Исходный вектор X.

//        //incX
//        //Шагай внутри X.Например, если 7, используется каждый седьмой элемент.incX

//        //Y
//        //Вектор назначения Y.

//        //incY
//        //Шагай внутри Y. Например, если 7, используется каждый седьмой элемент.incY

//        //      Умножение разреженной матрицы на плотный столбец.
//        //- Вход: 
//        //       - \f$IA\f$, \f$JA\f$, \f$AN\f$ - разреженное представление матрицы \f$A\f$ в RR(C) U-формате
//        //       - \f$b\f$ - плотный вектор-столбец
//        //       - \f$m\f$ - число строк матрицы \f$A\f$                     
//        //- Выход: 
//        //       - \f$c\f$ - произведение \f$A\cdot b\f$
//        void sp_mult_col(int[] IA, int[] JA, double[] AN, double[] b, int m,ref double[] c)
//        {
//            int i, j, k = 0;
//            double sum;
//            for (i = 0; i < m; i++)
//            {
//                sum = 0;
//                //for (j = *IA++; j < *IA; j++)
//                //{
//                //    sum += (*AN++) * b[*JA++];
//                //}
//                //*c++ = sum;
//                for (j = IA[i]; j < IA[i+1]; j++)
//                {
//                    sum += AN[k] * b[JA[k]];
//                    k++;
//                }
//                c[i] = sum;
//            }
//        }

//        void sp_mult_row(int[] IA, int[] JA, double[] AN, double[] b, int m, int n, double[] c)
//        {
//            int i, j, z, k=0;
//            //memset(c, 0, sizeof(*c) * n);
//            MEM.AllocClear(n, ref c);
//            for (i = 0; i < m; i++)
//            {
//                for (j = IA[i]; j < IA[i+1]; j++)
//                {
//                    z = JA[k];
//                    c[z] += AN[k] * b[i];
//                    k++;
//                }
//            }
//        }

//        /// <summary>
//        /// Выделение памяти под разреженную матрицу.
//        /// - Выход: IA, JA, AN - массивы под представление матрицы в RR(C)U-формате
//        /// </summary>
//        /// <param name="m">число строк матрицы A</param>
//        /// <param name="nz">число ненулевых элементов матрицы A</param>
//        /// <param name="IA"></param>
//        /// <param name="JA"></param>
//        /// <param name="AN"></param>
//        void sp_create(int m, int nz, int[] IA, int[] JA, double[] AN)
//        {
//            // Выделяем память матрицу СЛАУ
//            MEM.Alloc<int>(m + 1, ref IA);
//            MEM.Alloc<int>(nz, ref JA);
//            MEM.Alloc<double>(nz, ref AN);
//        }
//        //      Построение разреженной матрицы по списку ненулевых элементов.
//        //- Вход: 
//        //       - nz - число ненулевых элементов
//        //       - A - массив ненулевых элементов матрицы
//        //       - I, J - индексы строк и столбцов ненулевых элементов матрицы
//        //       - m - число строк матрицы A                     
//        //- Выход: 
//        //       - IA, JA, AN - разреженное представление матрицы в RR(C)U-формате
//        //  Трудоемкость: 
//        void sp_convert(int nz, double[] A, int[] I, int[] J, int m, int[] IA, int[] JA, double[] AN)
//        {
//            /* I, J - индексы строк и столбцов ненулевых элементов матрицы */
//            int i, j, k;
//            int[] pI = I;
//            int[] pJ = J;
//            double[] pA = A;
//            int[] pi = IA;
//            //memset(IA, 0, sizeof(int) * (m + 1));
//            for (i = 0; i < m + 1; i++)
//                IA[i] = 0;
//            int II = 0;
//            for (i = 0; i < nz; i++)
//            {
//                //j = *pI++ + 2;
//                j = pI[II++] + 2;
//                if (j <= m)
//                    IA[j]++;
//            }
//            //pi += 2;
//            //for (i = 3; i <= m; i++, pi++)
//            //    *(pi + 1) += *pi;
//            II += 2;
//            for (i = 3; i <= m; i++)
//            {
//                pi[II + 1] += pi[II];
//                II++;
//            }

//            //pI = I;
//            //for (i = 0; i < nz; i++)
//            //{
//            //    j = *pI++ + 1;
//            //    k = IA[j]++;
//            //    AN[k] = *pA++;
//            //    JA[k] = *pJ++;
//            //}
//            II = 0;
//            for (i = 0; i < nz; i++)
//            {
//                j = pI[II++] + 1;
//                k = IA[j]++;
//                AN[k] = pA[i];
//                JA[k] = pJ[i];
//            }
//        }
//        /// <summary>
//        /// Клонирование объекта
//        /// </summary>
//        /// <returns></returns>
//        public override IAlgebra Clone()
//        {
//            return new AlgebraCRS1D_BeCG(N);
//        }
//        /// <summary>
//        /// Сборка САУ по строкам (не для всех решателей)
//        /// </summary>
//        /// <param name="ColElems">Коэффициенты строки системы</param>
//        /// <param name="ColAdress">Адреса коэффицентов</param>
//        /// <param name="IndexRow">Индекс формируемой строки системы</param>
//        /// <param name="Right">Значение правой части строки</param>
//        public override void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
//        {

//        }
//        /// <summary>
//        /// Формирование матрицы системы
//        /// </summary>
//        /// <param name="LMartix">Локальная матрица</param>
//        /// <param name="Adress">Глабальные адреса</param>
//        public override void AddToMatrix(double[][] LMartix, uint[] Adress)
//        {

//        }
//        /// <summary>
//        /// Удовлетворение ГУ (с накопительными вызовами)
//        /// </summary>
//        /// <param name="Condition">Значения неизвестных по адресам</param>
//        /// <param name="Adress">Глабальные адреса</param>
//        public override void BoundConditions(double[] Conditions, uint[] Adress)
//        {

//        }
//        /// <summary>
//        /// Удовлетворение ГУ (с накопительными вызовами)
//        /// </summary>
//        public override void BoundConditions(double Condition, uint[] Adress)
//        {

//        }
//        /// <summary>
//        /// Операция определения невязки R = Matrix X - Right
//        /// </summary>
//        /// <param name="R">результат</param>
//        /// <param name="X">умножаемый вектор</param>
//        /// <param name="IsRight">знак операции = +/- 1</param>
//        public override void getResidual(ref double[] R, double[] X, int IsRight = 1)
//        {

//        }
//        ///// <summary>
//        ///// Вывод САУ на КОНСОЛЬ
//        ///// </summary>
//        public override void Print()
//        {

//        }

//    }
//}
