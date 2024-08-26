//---------------------------------------------------------------------------
//                      По мотивам кода С++ River2DM  
//                                - 1998 -
//---------------------------------------------------------------------------
//             кодировка на C#: 05.07.2021 Потапов И.И. 
//---------------------------------------------------------------------------
//             осправления  27 - 29.12.2023 Потапов И.И. 
//---------------------------------------------------------------------------
// Сетка TriRiverMesh не имеет граничных узлов поэтому в задаче ГГУ
// выполняются через граничные элементы (модифициуя ЛМЖ и ЛПЧ) перед
// сборкой САУ
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver_2XYD.River2DSW
{
    using System;
    using CommonLib;
    using MemLogLib;
    using AlgebraLib;
    /// <summary>
    /// ОО: Общий интерфейс Алгебры
    /// </summary>
    [Serializable]
    public class AlgebraRiver : IAlgebra
    {
        AlgebraResult result = new AlgebraResult();
        /// <summary>
        /// Информация о полученном результате
        /// </summary>
        public IAlgebraResult Result { get => result; }
        /// <summary>
        /// Сетка
        /// </summary>
        IMesh mesh;
        /// <summary>
        /// Название метода
        /// </summary>
        public string Name { get { return "River2D"; } }
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        public uint N { get { return (uint)FN; } set { FN = (int)value; } }
        /// <summary>
        /// порядок СЛУ
        /// </summary>
        protected int FN;
        /// <summary>
        /// глобальная матрица жесткости (Lp)
        /// </summary>
        public double[] Lp = null;
        /// <summary>
        /// глобальная матрица жесткости (Kp)
        /// </summary>
        public double[] Kp = null;
        /// <summary>
        /// глобальная правая часть (ГПЧ)
        /// </summary>
        public double[] Fp = null;
        /// <summary>
        /// Адреса диагональных элементов плотно упакованной матрицы
        /// </summary>
        protected uint[] diagp;
        /// <summary>
        /// 
        /// </summary>
        uint[] iptrs;
        protected int code = 1;
        public int CountUnknow = 3;
        public int CountElementKnots = 3;

        public AlgebraRiver(IMesh mesh)
        {
            Set(mesh);
        }
        public AlgebraRiver(AlgebraRiver ra)
        {
            Set(ra.mesh);
        }
        public void Set(IMesh mesh)
        {
            this.mesh = mesh;
            if (mesh != null)
                if (mesh.CountKnots != 0)
                    Init();
            Result.N = (uint)FN;
            Result.Name = Name;
        }
        /// <summary>
        /// <summary>
        /// Очистка матрицы и правой части
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Kp.Length; i++)
            {
                Kp[i] = 0;
                Lp[i] = 0;
            }
            for (int i = 0; i < Fp.Length; i++)
                Fp[i] = 0;
        }
        // считаем необходимое кол-во памяти под матрицу и выделяем его
        protected void Init()
        {
            int d;
            uint i, j, nff, ind;
            uint[] mpdArray = new uint[mesh.CountKnots];
            FN = mesh.CountKnots * CountUnknow;
            diagp = new uint[FN];
            iptrs = new uint[mesh.CountKnots];
            ind = 0;
            diagp[ind] = 0;
            iptrs[ind] = ind;
            ind++;
            for (i = 1; i < CountUnknow; i++)
            {
                nff = i + 1;
                diagp[ind] = diagp[ind - 1] + nff;
                ind++;
            }
            TriElement[] ements = mesh.GetAreaElems();
            for (i = 0; i < mesh.CountElements; i++)
            {
                d = (int)(ements[i].Vertex3 - ements[i].Vertex2);
                if (d > 0)
                {
                    if (d > mpdArray[((int)ements[i].Vertex3)])
                        mpdArray[((int)ements[i].Vertex3)] = (uint)d;
                }
                else
                {
                    d *= -1;
                    if (d > mpdArray[((int)ements[i].Vertex2)])
                        mpdArray[((int)ements[i].Vertex2)] = (uint)d;
                }
                d = (int)ements[i].Vertex2 - (int)ements[i].Vertex1;
                if (d > 0)
                {
                    if (d > mpdArray[((int)ements[i].Vertex2)])
                        mpdArray[((int)ements[i].Vertex2)] = (uint)d;
                }
                else
                {
                    d *= -1;
                    if (d > mpdArray[((int)ements[i].Vertex1)])
                        mpdArray[((int)ements[i].Vertex1)] = (uint)d;
                }
                d = (int)ements[i].Vertex3 - (int)ements[i].Vertex1;
                if (d > 0)
                {
                    if (d > mpdArray[((int)ements[i].Vertex3)])
                        mpdArray[((int)ements[i].Vertex3)] = (uint)d;
                }
                else
                {
                    d *= -1;
                    if (d > mpdArray[((int)ements[i].Vertex1)])
                        mpdArray[((int)ements[i].Vertex1)] = (uint)d;
                }
            }
            for (i = 1; i < mesh.CountKnots; i++)
            {
                nff = (uint)((mpdArray[i] + 1) * CountUnknow - (CountUnknow - 1));
                for (j = 0; j < CountUnknow; j++)
                {
                    iptrs[i] = ind;
                    diagp[ind] = diagp[(uint)(ind - 1)] + nff;
                    ind++;
                    nff += 1;
                }
            }
            // количество элементов в собранной матрице
            uint nelem = diagp[(uint)(ind - 1)] + 1;
            MEM.Alloc0(nelem, ref Kp, "Kp");
            MEM.Alloc0(nelem, ref Lp, "Lp");
            MEM.Alloc0((uint)FN, ref Fp, "Fp");
        }
        /// <summary>
        /// Сборка ГМЖ
        /// </summary>
        /// <param name="LMartix">ЛМЖ</param>
        /// <param name="Adress">узлы КЭ</param>
        public void AddToMatrix(double[][] LMartix, uint[] Adress)
        {
            int i, j, ii, jj;
            int frow, fcol, row, col;
            for (i = 0; i < Adress.Length; i++)
            {
                frow = (int)Adress[i] * CountUnknow;
                for (ii = 0; ii < CountUnknow; ii++)
                {
                    row = frow + ii;
                    for (j = 0; j < Adress.Length; j++)
                    {
                        fcol = (int)Adress[j] * CountUnknow;
                        for (jj = 0; jj < CountUnknow; jj++)
                        {
                            col = fcol + jj;
                            if (col >= row)
                                Kp[diagp[col] - col + row] += LMartix[i * CountUnknow + ii][j * CountUnknow + jj];
                            if (col <= row)
                                Lp[diagp[row] - row + col] += LMartix[i * CountUnknow + ii][j * CountUnknow + jj];
                        }
                    }
                }
            }
        }
        /// <summary>
        /// // Сборка ГПЧ
        /// </summary>
        public void AddToRight(double[] LRight, uint[] Adress)
        {
            for (int i = 0; i < Adress.Length; i++)
            {
                int frow = (int)Adress[i] * CountUnknow;
                for (int ii = 0; ii < CountUnknow; ii++)
                {
                    int row = frow + ii;
                    Fp[row] += LRight[i * CountUnknow + ii];
                }
            }
        }
        /// <summary>
        /// Сборка САУ по строкам (не для всех решателей)
        /// </summary>
        /// <param name="ColElems">Коэффициенты строки системы</param>
        /// <param name="ColAdress">Адреса коэффицентов</param>
        /// <param name="IndexRow">Индекс формируемой строки системы</param>
        /// <param name="Right">Значение правой части строки</param>
        public void AddStringSystem(double[] ColElems, uint[] ColAdress, uint IndexRow, double R)
        {
        }
        /// <summary>
        /// Добавление в правую часть
        /// </summary>
        public void CopyRight(double[] CRight)
        {
            for (int a = 0; a < Fp.Length; a++)
                Fp[a] += CRight[a];
        }
        /// <summary>
        /// Получение правой части СЛАУ
        /// </summary>
        /// <param name="CRight"></param>
        public void GetRight(ref double[] CRight)
        {
            for (int a = 0; a < Fp.Length; a++)
                CRight[a] = Fp[a];
        }
        /// <summary>
        /// Удовлетворение ГУ
        /// </summary>
        public void BoundConditions(double[] Conditions, uint[] Adress) { }
        /// <summary>
        /// Выполнение ГУ
        /// </summary>
        public void BoundConditions(double Conditions, uint[] Adress) { }
        /// <summary>
        /// Операция умножения вектора на матрицу
        /// </summary>
        /// <param name="R">результат</param>
        /// <param name="X">умножаемый вектор</param>
        /// <param name="IsRight">знак операции = +/- 1</param>
        public void getResidual(ref double[] R, double[] X, int IsRight = 1) { }
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        public void Solve(ref double[] X)
        {
            Result.Start();
            solve(ref X);
            Result.X = X;
            Result.Stop();
        }
        /// <summary>
        /// Решение СЛУ
        /// </summary>
        public void solve(ref double[] X)
        {
            uint nf, id, n, CountIntPoint, nj;
            uint ip, iLp, idp;
            //double t;
            double csum;
            for (uint j = 1; j < FN; j++)
            {
                nf = (diagp[j] - diagp[j - 1]);
                if (code < 2)
                {
                    iLp = diagp[j - 1] + 1;
                    id = (j - nf + 1);
                    for (uint i = 0; i < nf - 1; i++)
                    {
                        if (id > 0)
                            CountIntPoint = (uint)(diagp[id] - diagp[id - 1] - 1);
                        else
                            CountIntPoint = 2000000000;
                        n = ((nj = i) < (CountIntPoint)) ? nj : CountIntPoint;
                        idp = diagp[id];
                        csum = Dot(Lp, (uint)(iLp - n), Kp, (uint)(idp - n), n);
                        Lp[iLp] = (Lp[iLp] - csum) / Kp[idp];
                        iLp++;
                        id++;
                    }
                    ip = diagp[j - 1] + 1;
                    id = j - nf + 1;
                    for (uint i = 0; i < nf; i++)
                    {
                        if (id > 0)
                            CountIntPoint = diagp[id] - diagp[id - 1] - 1;
                        else
                            CountIntPoint = 1000000000;
                        n = ((nj = i) < (CountIntPoint)) ? nj : CountIntPoint;
                        idp = diagp[id];
                        id++;
                        csum = Dot(Kp, ip - n, Lp, idp - n, n);
                        Kp[ip] -= csum;
                        ip++;
                    }
                }
                iLp = diagp[j - 1] + 1;
                id = j - nf + 1;
                if (code > 0)
                {
                    csum = Dot(Lp, iLp, Fp, id, nf - 1);
                    Fp[j] -= csum;
                }
            }
            if (code == 0)
            {
                MEM.MemCopy<double>(ref X, Fp);
                return;
            }
            for (int j = FN - 1; j > -1; j--)
            {
                Fp[j] /= Kp[diagp[j]];
                if (j == 0)
                    nf = 1;
                else
                    nf = diagp[j] - diagp[j - 1];
                for (int i = 1; i < nf; i++)
                {
                    Fp[j - nf + i] -= Fp[j] * Kp[diagp[j - 1] + i];
                }
            }
            MEM.MemCopy<double>(ref X, Fp);
        }
        double Dot(double[] L, uint iL, double[] K, uint iK, uint n)
        {
            double t = 0.0;
            for (int i = 0; i < n; i++)
                t += L[iL + i] * K[iK + i];
            return t;
        }
        /// <summary>
        /// Клонирование объекта
        /// </summary>
        /// <returns></returns>
        public IAlgebra Clone()
        {
            return new AlgebraRiver(this);
        }
        ///// <summary>
        ///// Вывод САУ на КОНСОЛЬ
        ///// </summary>
        public void Print(int flag = 0)
        {

        }
    }
}

