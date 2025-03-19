/// ---------------------------------------------------------------
/// Реплика с моего кода 1993 года (язык С), из пакета 
///     по расчету полимеризации топлива в РДДТ 
/// ---------------------------------------------------------------    
///     только генерация сетки по макроэлементам
///                     Потапов И.И. 
///                     13.03.2025
/// ---------------------------------------------------------------

namespace MeshGeneratorsLib.SPIN
{
    using System;
    using System.IO;

    using MeshLib;
    using MemLogLib;
    using CommonLib;

    /// <summary>
    /// Клеточный генератор КЭ сетки
    /// </summary>
    public class SpinMeshGeneratot
    {
        /// <summary>
        /// количество горизотральных макроэлементов
        /// </summary>
        int noh;
        /// <summary>
        /// количество вертикальных макроэлементов
        /// </summary>
        int nov;
        /// <summary>
        /// количество узлов на горизонтальных гранях 
        /// </summary>
        int[] nh0 = null;
        /// <summary>
        /// количество узлов на вертикальных гранях
        /// </summary>
        int[] nv0 = null;
        /// <summary>
        /// топологический maссив    // noh * nov
        /// </summary>
        int[] mt0 = null;
        /// <summary>
        /// количество опроных вершин
        /// </summary>
        int ku;
        /// <summary>
        /// массив Х координат узлов макроэлементов
        /// </summary>
        double[] xs0 = null;  
        /// <summary>
        /// массив У координат узлов макроэлементов
        /// </summary>
        double[] ys0 = null;
        #region
        //* --- maссив обхода 8 узловых элементов ----------------------- *//
        int[] nw0 = null;  // 8 * ne
        //* --- maссив обхода 4 узловых элементов ----------------------- *//
        int[] nb0 = null; // 4 * ne
        //* --- maссив признаков областей ------------------------------- *//
        int[] nx0 = null; // ne
        //* --- maссивы граничных узлов --------------------------------- *//
        int[] mnn0 = null; // 4
        int[] nn0 = null; // 4
        /// <summary>
        /// массив граничных узлов
        /// </summary>
        int[][] mgt0 = null; // mnn0 // 4
        //* --- maссив координат узлов 8-ми узловых КЭ  ----------------- *//
        double[] xx0 = null; // jp
        double[] yy0 = null;// jp
        //* --- maссив координат узлов 4-ми узловых КЭ  ----------------- *//
        double[] xh0 = null; // jh
        double[] yh0 = null;  // jh

        int[] kw0 = null;
        int[] ms10 = null;
        int[] ms20 = null;
        /// <summary>
        /// узловую карту 8 узловых элементов
        /// </summary>
        int[] mw0 = null;
        int[] mg0 = null;

        double[] sx0 = new double[12];
        double[] sy0 = new double[12];
        double[] ff0 = new double[12];
        #endregion
        public SpinMeshGeneratot()
        {
        }
        public SpinMeshGeneratot(int[] nh0, int[] nv0, int[] mt0, double[] xs0, double[] ys0)
        {
            Set(nh0, nv0, mt0, xs0, ys0);
        }
        public void Set(int[] nh0, int[] nv0, int[] mt0, double[] xs0, double[] ys0)
        {
            this.nh0 = nh0;
            this.nv0 = nv0;
            this.mt0 = mt0;
            this.xs0 = xs0;
            this.ys0 = ys0;
            noh = nh0.Length;
            nov = nv0.Length;
            ku = xs0.Length;
        }

        public void LoadData(string filename)
        {
            // ------ ф-я генерации конечно-элементной сетки -------------- 
            // ------- ввод основных скалярных параметров ------------------ 
            // --- количество горизотральных макроэлементов  --------------- 
            //noh = vsc(in_fl, kk);
            // --- количество вертикальных макроэлементов  ----------------- 
            //nov = vsc(in_fl, kk);
            // --- количество подобластей с различной физико-механикой ------
            //ktm = vsc(in_fl, kk);
            // --- количество точек интегрирования --------------------------
            //ng = vsc(in_fl, kk);
            //---параметр типа задачи(плоская : 0 осесимметричная: 1) ---
            StreamReader in_fl = new StreamReader("data.fem");
            //is= vsc(in_fl, kk);
            // -------- отведение динамической памяти ----------------------
            nh0 = new int[noh];
            nv0 = new int[nov];
            mt0 = new int[nov * noh];
            // -------- введение количества узлов на гранях макроэлементов --
            vomc(in_fl, nh0, noh); //* кол. узлов на горизонт. гранях ---
            vomc(in_fl, nv0, nov); //* кол. узлов на вертикал. гранях ---
            vomc(in_fl, mt0, noh * nov); //* топологический maссив ------
            //* ------ построчный ввод координат узлов суперэлементов -------
            vomv_xy(in_fl,ref xs0,ref ys0, ku);
        }

        public IMesh CreateMesh()
        {
            IMesh mesh = new ComplecsMesh();
           
            // ________ вычисление объема требуемой памяти ------------------
            int nh = summ(nh0, noh) - noh + 1;
            int nv = summ(nv0, nov) - nov + 1;
            int kh = (nh - 1) / 2;
            int kv = (nv - 1) / 2;
            int mx = 3 * noh + 1;
            int my = 3 * nov + 1;
            int ke = kh * kv;
            int ku = mx * my - 4 * noh * nov;
            int jp = nv * nh - ke;
            int jh = (kh + 1) * (kv + 1);
            // -------- выделение требуемого объема памяти ------------------
            int[] no0 = null;
            MEM.Alloc(12 * noh * nov, ref no0);
            MEM.Alloc(ke, ref nx0);
            MEM.Alloc(nh * nv, ref mg0);
            MEM.Alloc(nh * nv, ref mw0);
            MEM.Alloc(8 * ke, ref nw0);
            MEM.Alloc(4 * ke, ref nb0);
            MEM.Alloc(ke, ref ms10);
            MEM.Alloc(ke, ref ms20);
            //MEM.Alloc(ku, ref xs0);
            //MEM.Alloc(ku, ref ys0);
            MEM.Alloc(jp, ref xx0);
            MEM.Alloc(jp, ref yy0);
            MEM.Alloc(jh, ref xh0);
            MEM.Alloc(jh, ref yh0);
            #region генерация массива обхода узлов макроэлементов 

            int[] j0 = null;
            int i, j, k, l, m, m1, ih, iv, iss, id, ir, io;
            k = mx * my - 4 * noh * nov;
            MEM.Alloc(k, ref j0);
            i = 0;
            while (i < k)
            {
                iv = i;
                j0[iv] = ++i;
            }
            iv = 0;
            iss = ih = mx + 2 * (noh + 1);
            id = mx;
            ir = k = mx + noh + 1;
            for (i = 0; i < nov; i++)
            {
                m = m1 = 0;
                for (j = 0; j < noh; j++)
                {
                    io = 12 * j + 12 * i * noh;
                    no0[io + 0] = j0[m + ih];
                    no0[io + 1] = j0[m + ih + 1];
                    no0[io + 2] = j0[m + ih + 2];
                    no0[io + 3] = j0[m + ih + 3];
                    no0[io + 4] = j0[m1 + ir + 1];
                    no0[io + 5] = j0[id + m1 + 1];
                    no0[io + 6] = j0[m + iv + 3];
                    no0[io + 7] = j0[m + iv + 2];
                    no0[io + 8] = j0[iv + m + 1];
                    no0[io + 9] = j0[iv + m];
                    no0[io + 10] = j0[id + m1];
                    no0[io + 11] = j0[m1 + ir];
                    m = m + 3;
                    m1 = m1 + 1;
                }
                ih = ih + iss;
                iv = iv + iss;
                id = id + iss;
                ir = ir + iss;
            }
            #endregion

            int nhh, nvv, in0;
            int ksi, ksj, ic, nc, il;
            int ijk, im, npi, npj, nmi, nmj, jn;
            double x, y, x1, y1, sx, sy;
            int iu = 0;
            int ik = 1;
            int ko = 0;
            int nuh = 0;
            //* ------- инициализация рабочих массивов --------------------*//
            for (l = 0; l < nh * nv; l++)
                mw0[l] = kw0[l] = -1;
            //* ------- начало генерации КЭ сетки -------------------------*//
            //* ------- цикл по вертикальным подоблостям ------------------*//
            for (int ii = 1; ii <= nov; ii++)
            {
                nvv = nv0[ii - 1];
                sy = 2.0f / (nvv - 1);
                //* ---- условие связности узлов подобластей ---------------*//
                if (ii == 1)
                    in0 = 1;
                else
                    in0 = 2;
                //* ---- определение поузлового сдвига ksi -----------------*//
                ksi = summ(nv0, ii - 1);
                ksi = ksi - ii + 1;
                //* ---- цикл по вертикали в подобласти --------------------*//
                for (i = in0; i <= nvv; i++)
                {
                    y = 1 - (i - 1) * sy;
                    my = i / 2;
                    my = (2 * my) - i;
                    //* -- цикл по горизонтальным подобластям --------------*//
                    for (int jj = 1; jj <= noh; jj++)
                    {
                        nhh = nh0[jj - 1];
                        sx = 2.0f / (nhh - 1);
                        // выборка характеристик макроэлемента  
                        ijk = 12 * adm(ii, jj, noh);
                        for (ic = 0; ic < 12; ic++)
                        {
                            nc = no0[ijk + ic] - 1;
                            sx0[ic] = xs0[nc];
                            sy0[ic] = ys0[nc];
                        }
                        // условие генерации в подобласти по горизонтали 
                        if (ii < nov)
                            im = ii + 1;
                        else
                            im = ii;
                        ic = adm(ii, jj, noh);
                        nc = adm(im, jj, noh);
                        if ((mt0[ic] > 0) || (i == nvv && mt0[nc] != 0))
                        {
                            if (jj == 1)
                                jn = 1;
                            else
                            {
                                if (mt0[ic - 1] == 0)
                                    jn = 1;
                                else
                                    jn = 2;
                            }
                            // определение поузлового сдвига ksj -------- 
                            ksj = summ(nh0, jj - 1); ksj = ksj - jj + 1;
                            // цикл по строкам в подоблостях --------- 
                            for (j = jn; j <= nhh; j++)
                            {
                                x = -1 + (j - 1) * sx;
                                mx = j / 2;
                                mx = (2 * mx) - j;
                                // условие вычеркивания центрального узла  
                                // в обрабатываемом конечном элементе
                                if (my == 0 && mx == 0)
                                {
                                    ko++;
                                    ms10[ko - 1] = ksi + i;
                                    ms20[ko - 1] = ksj + j;
                                    nc = adm(ksi + i, ksj + j, nh);
                                    mw0[nc] = ko; 
                                    kw0[nc] = ko;
                                    nx0[ko - 1] = mt0[adm(ii, jj, noh)];
                                }
                                else
                                {
                                    // вычисления массива значений
                                    // функций форм  текущего макроэлемента в
                                    // точке с координатами  х, у
                                    fff(x, y, ref ff0);
                                    //* вычислениее координат текущего --- *//
                                    //* узла обрабатываемого элемента ---- *//
                                    xx0[iu] = x1 = fun_sum(ff0, sx0, 12);
                                    yy0[iu] = y1 = fun_sum(ff0, sy0, 12);
                                    iu++;
                                    //* запись глобального номера узла в   *//
                                    //* узловую карту 8 узловых элементов  *//
                                    nc = adm(ksi + i, ksj + j, nh);
                                    mw0[nc] = iu;
                                    //* priomc("mw0",mw0,nv*nh,nh); *//
                                    //* запись глобального номера узла в   *//
                                    //* узловую карту 8 узловых элементов  *//
                                    if (my != 0 && mx != 0)
                                    {
                                        xh0[nuh] = x1;
                                        yh0[nuh] = y1;
                                        nuh++;
                                        kw0[nc] = nuh;
                                    }
                                } //* if my *//
                            } //* j *//
                        } //* if mt0 *//
                    } //* jj *//
                } //*  i  *//
            } //* ii *//
            ku = jp = iu;
            jh = nuh;
            int ne = ko;
            //ju = 2 * jp;
            nc = kh + 1;
            //* ---- генерация массивов обхода узлов конечноэлементной сетки -- *//
            //* для четырех и восьми узловых четырехугольных конечных элементов *//

            for (ic = 0; ic < ne; ic++)
            {
                i = ms10[ic];
                j = ms20[ic];
                ik = ic * 8;
                il = ic * 4;
                nw0[ik + 0] = vdm(mw0, i + 1, j - 1, nh);
                nw0[ik + 1] = vdm(mw0, i + 1, j, nh);
                nw0[ik + 2] = vdm(mw0, i + 1, j + 1, nh);
                nw0[ik + 3] = vdm(mw0, i, j + 1, nh);
                nw0[ik + 4] = vdm(mw0, i - 1, j + 1, nh);
                nw0[ik + 5] = vdm(mw0, i - 1, j, nh);
                nw0[ik + 6] = vdm(mw0, i - 1, j - 1, nh);
                nw0[ik + 7] = vdm(mw0, i, j - 1, nh);

                nb0[il + 0] = vdm(kw0, i + 1, j - 1, nh);
                nb0[il + 1] = vdm(kw0, i + 1, j + 1, nh);
                nb0[il + 2] = vdm(kw0, i - 1, j + 1, nh);
                nb0[il + 3] = vdm(kw0, i - 1, j - 1, nh);
            }
            // graph_chk(xx0, yy0, nw0, jp, ne, 1, iss, 0, 8);
            j = 1;
            l = 0;
            k = 0;
            // --- поиск начального ненулевого узла на верхней грани --------- 
            for (i = 1; i <= nh; i++)
            {
                nc = vdm(mw0, j, i, nh);
                if (nc > 0)
                    goto nnnn;
            }
            Console.WriteLine("топологический массив mt0[] задан некоректно ");
            return null;
        nnnn: 
            mg0[k] = nc; 
            k++;
            // цикл обхода, по часовой стрелке конечноэлементной карты -- 
            for (;;)
            {
                npi = npj = nmi = nmj = -1;
                // -- определение поля возможных приращений  npi, npj, nmi, nmj 
                if (i + 1 <= nh)
                    npi = vdm(mw0, j, i + 1, nh);
                if (j + 1 <= nv)
                    npj = vdm(mw0, j + 1, i, nh);
                if (i - 1 >= 1)
                    nmi = vdm(mw0, j, i - 1, nh);
                if (j - 1 >= 1)
                    nmj = vdm(mw0, j - 1, i, nh);
                // - выбор направления с учетом невозможности обратного хода 
                if (npi > 0 && mg0[k - 1] != npi && nmj < 0)
                {
                    mg0[k] = npi;
                    k++;
                    i++;
                }
                else
                if (npj > 0 && mg0[k - 1] != npj && npi < 0)
                {
                    mg0[k] = npj;
                    k++;
                    j++;
                }
                else
                if (nmi > 0 && mg0[k - 1] != nmi && npj < 0)
                {
                    mg0[k] = nmi;
                    k++;
                    i--;
                }
                else
                if (nmj > 0 && mg0[k - 1] != nmj && nmi < 0)
                {
                    mg0[k] = nmj;
                    k++;
                    j--;
                }
                // маркировка обходимых граней конечноэлементной карты 
                if ((i == nh && l == 0) ||
                    (j == nv && l == 1) ||
                    (i == 1 && l == 2))
                {
                    nn0[l] = k - 1;
                    l++;
                }
                // условия выхода из безконечного цикла обхода 
                if ((j == 1 && l == 3) ||
                    (k > 1000000))
                {
                    nn0[l] = k - 1;
                    break;
                }
            }
            // цикл по граням области 
            mgt0 = new int[4][];
            for (i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    l = nn0[i] + 1;
                    m = 0;
                }
                else
                {
                    l = nn0[i] - nn0[i - 1] + 1;
                    m = nn0[i - 1];
                }
                mnn0[i] = l;
                // ----- выделение памяти под массив граничных узлов ------- 
                mgt0[i] = new int[l];
                l = 0;
                // ----- цикл по узлам грани --------------------------- 
                for (k = m; k <= nn0[i]; k++)
                {
                    mgt0[i][l] = mg0[k];
                    l++;
                }
            }
            // ------- конец процедуры генерации КЭ сетки ------------------
            return mesh;
        }

        /// <summary>
        /// ф-я суммирования одномерного массива или его чати от 0 до i-1 
        /// </summary>
        int summ(int[] id, int i)
        {
            int k, l; l = k = 0;
            while (l < i) 
            { 
                k += id[l]; 
                l++; 
            }
            return k;
        }

        /// <summary>
        ///  ф-я вычисления адреса эмулируемого указателем двухмерного массива 
        /// </summary>
        int adm(int i, int j, int h)
        {
            return (i - 1) * h + j - 1;
        }

        /// <summary>
        /// ф-я скалярного произведения двух векторов
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="b0"></param>
        /// <param name="cu"></param>
        /// <returns></returns>
        double fun_sum(double[] a0, double[] b0, int cu)
        {
            int i; double s;
            s = 0;
            for (i = 0; i < cu; i++) 
                s += a0[i] * b0[i];
            return (s);
        }


        /* выбор элемента из эмулируемого двухмерного массива с длиной    */
        /* строки mh; iv,ih - индекс строки и столбца выбераемого элемета */
        // судя по смещениям индексация узлов велась с 1 а нес 0
        int vdm(int[] id, int iv, int ih, int mh)
        {
            return id[mh * (iv - 1) + ih - 1];
        }

        /// <summary>
        /// Серендиповы функции формы 3 порядк (12 узлов)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ff0"></param>
        void fff(double x, double y, ref double[] ff0)
        {
            /* ф-я вычисления массива значений функций двенадцати форм     */
            /* текущего макроэлемента в точке с координатами  х, у         */
            int i;
            double e, b;
            double[] xz0 = { -1.0f , -0.333333f, 0.333333f, 1.0f, 1.0f, 1.0f,
                             1.0f, 0.3333333f, -0.3333333f, -1.0f, -1.0f, -1.0f };
            double[] yz0 = { -1.0f, -1.0f, -1.0f, -1.0f, -0.3333333f, 0.3333333f,
                             1.0f, 1.0f, 1.0f, 1.0f, 0.33333333f, -0.33333333f };
            for (i = 1; i <= 12; i++)
            {
                e = x * xz0[i - 1];
                b = y * yz0[i - 1];
                if (i == 1 || i == 4 || i == 7 || i == 10)
                {
                    ff0[i - 1] = (1.0f + e) * (1.0f + b) * (9.0f * (x * x + y * y) - 10.0f) / 32.0f;
                }
                else
                {
                    if (i == 5 || i == 6 || i == 11 || i == 12)
                    {
                        ff0[i - 1] = 9.0f * (1.0f + e) * (1.0f + 9.0f * b) * (1.0f - y * y) / 32.0f;
                    }
                    else
                    {
                        ff0[i - 1] = 9.0f * (1.0f + b) * (1.0f + 9.0f * e) * (1.0f - x * x) / 32.0f;
                    }
                }
            }
        }


        /// <summary>
        /// ф-я ввода и печати эмулируемого указателем *id  массива      
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="id"></param>
        /// <param name="d"></param>
        /// <param name="k"></param>
        void vomc(StreamReader inn, int[] id, int d)
        {
            int l; l = 0;
            inn.ReadLine();
            //fscanf(in, " %s ", ti0);
            //if (k > 0) 
            //    printf("\n %s %d ", ti0, d);
            //while (l < d) 
            //{ 
            //    fscanf(in, "%d", (id + l++)); 
            //}
            //if (k > 0)
            //{
            //    l = 0;
            //    while (l < d) { printf("\n элемент N %d : %d", l + 1, *(id + l)); l++; }
            //}
        }
        /// <summary>
        /// ф-я ввода одномерного вещественного массива
        /// </summary>
        /// <param name="inn"></param>
        /// <param name="xd0"></param>
        /// <param name="yd0"></param>
        /// <param name="d"></param>
        /// <param name="k"></param>
        void vomv_xy(StreamReader inn,ref double[] xd0, ref double[] yd0, int d, int k = 0)
        {
            int l; double a, b;
            string version = inn.ReadLine();
            //fscanf(in, " %s", ti0);
            for (l = 0; l < d; l++)
            {
                string line = inn.ReadLine();
                string[] lines = line.Split(new char[] { ',', ' ', ';', '\t', '\n' });
                //fscanf(in, " %f %f", &a, &b); 
                xd0[l] = double.Parse(lines[0].Trim(), MEM.formatter);
                yd0[l] = double.Parse(lines[0].Trim(), MEM.formatter);
            }
            if (k > 0)
            {
                l = 0;
                Console.WriteLine(" координаты узлов макроэлементов ");
                while (l < d)
                {
                    Console.WriteLine("\n узел N {0}  {1}  {2}", l + 1, xd0[l], yd0[l]);
                    l++;
                }
            }
        }

    }
}
