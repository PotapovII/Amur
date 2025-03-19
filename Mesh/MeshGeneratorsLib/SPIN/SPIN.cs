
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
    using MemLogLib;
    public class SPIN
    {
        //* ---- идентификаторы используемые при генерации КЭ сетки ---- *//
        int nhh, nvv, in0;
        int m, ksi, ksj, ic, nc, il;
        int ijk, im, npi, npj, nmi, nmj, jn;
        int iu, ik, ko, nuh;
        //int[] nh0, nv0, mt0;
        //int[] no0,kw0, mw0, ms10, ms20, nx0, mg0, nw0, nb0;
        //int[] nu0 = new int[12];
        //int[] nn0 = new int[4];
        //int[] mnn0 = new int[4];
        float x, y, x1, y1, sx, sy;

        //* --- количество горизотральных макроэлементов  --------------- *//
        int noh;
        //* --- количество вертикальных макроэлементов  ----------------- *//
        int nov;
        //* --- количество подобластей с различной физико-механикой ------*//
        //int ktm;
        //* --- количество узлов на горизонтальных гранях --------------- *//
        int[] nh0; // noh
        //* --- количество узлов на вертикальных гранях ----------------- *//
        int[] nv0; // nov
        //* --- топологический maссив ----------------------------------- *//
        int[] mt0; // noh * nov);
        //* --- вычисление скалярных параметров ------------------------- *//
        int nh;
        int nv;
        int kh;
        int kv;
        int jp; int jh; //int ju;
        //int hak; //int huk; int hhk;
        int ne; //int ng; int iss;
        //int nmg;
        //* --- maссив обхода 8 узловых элементов ----------------------- *//
        int[] nw0;  // 8 * ne
        //* --- maссив обхода 4 узловых элементов ----------------------- *//
        int[] nb0; // 4 * ne
        //* --- maссив признаков областей ------------------------------- *//
        int[] nx0; // ne
        //* --- maссивы граничных узлов --------------------------------- *//
        int[] mnn0; // 4
        int[] nn0; // 4
        int[][] mgt0; // mnn0 // 4
        //* --- maссив координат узлов 8-ми узловых КЭ  ----------------- *//
        float[] xx0; // jp
        float[] yy0;// jp
        //* --- maссив координат узлов 4-ми узловых КЭ  ----------------- *//
        float[] xh0; // jh
        float[] yh0;  // jh

        int[] no0;
        int[] mg0;
        int[] kw0;
        int[] ms10;
        int[] ms20;
        int[] mw0;
        //*---------------------------------------------------------------*//
        //*              описание используемых массивов                   *//
        //*_______________________________________________________________*//
        //*  *noh0 - мас. гориз. узлового разбиения граней макроэлементов *//
        //*  *nov0 - мас. верт. узлового разбиения граней макроэлементов  *//
        //*  *mt0 - мас. топологии разбиваемой облости                    *//
        //*  *no0 - массив обхода узлов макроэлементов                    *//
        //*  *kw0 - массив - карта 4-х узловых конечных элементов         *//
        //*  *mw0 - массив - карта 8-и узловых конечных элементов         *//
        //*  *ms10 - массив гориз. координат КЭ в конечноэлементной карте *//
        //*  *ms20 - массив вертик.координат КЭ в конечноэлементной карте *//
        //*  *nx0 - массив меток принадлежности КЭ к типу подобласти карты*//
        //*  *mg0 - массив внешних граничных узлов области                *//
        //*  *nw0 - массив обхода генерируемой  8-и узловой  КЭ сетки     *//
        //*  *nb0 - массив обхода генерируемой  4-х узловой  КЭ сетки     *//
        //*---------------------------------------------------------------*//
        //*  *xs0 - массив Х координат узлов макроэлементов               *//
        //*  *ys0 - массив У координат узлов макроэлементов               *//
        //*  *xx0 - мас. Х координат узлов генер. КЭ 8-и узловой КЭ сетки *//
        //*  *yy0 - мас. У координат узлов генер. КЭ 8-и узловой КЭ сетки *//
        //*  *xh0 - мас. Х координат узлов генер. КЭ 4-х узловой КЭ сетки *//
        //*  *yh0 - мас. У координат узлов генер. КЭ 4-х узловой КЭ сетки *//
        //*---------------------------------------------------------------*//
        float[] xs0, ys0;
        float[] sx0 = new float[12];
        float[] sy0 = new float[12];
        float[] ff0 = new float[12];
        //* ----- массивы используемые при генерации КЭ среды ------------*//

        //* --- ф-я вывода сгенерированных данных о конечноэлементной --- *//
        //* ------------- сетке и конечноэлементной среде --------------- *//
        public SPIN(
            ////--- количество горизотральных макроэлементов  --------------- 
            //int noh
            //// --- количество вертикальных макроэлементов  -----------------
            //, int nov
            //// --- количество подобластей с различной физико-механикой -----
            //, int ktm
            //// --- количество узлов на горизонтальных гранях ---------------
            //, int[] nh0 // noh
            //// --- количество узлов на вертикальных гранях -----------------
            //, int[] nv0 // nov
            //// --- топологический maссив -----------------------------------
            //, int[] mt0 // noh * nov);
            //// --- вычисление скалярных параметров -------------------------
            //, int nh
            //, int nv
            //, int kh
            //, int kv
            //, int jp, int jh, int ju
            //, int hak, int huk, int hhk
            //, int ne, int ng, int iss
            //, int nmg
            //// --- maссив обхода 8 узловых элементов ----------------------- 
            //, int[] nw0  // 8 * ne
            //// --- maссив обхода 4 узловых элементов ----------------------- 
            //, int[] nb0 // 4 * ne
            //// --- maссив признаков областей -------------------------------
            //, int[] nx0 // ne
            //// --- maссивы граничных узлов ---------------------------------
            //, int[] mnn0 // 4
            //, int[] nn0  // 4
            //, int[][] mgt0 // mnn0 // 4
            //// --- maссив координат узлов 8-ми узловых КЭ  -----------------
            //, float[] xx0 // jp
            //, float[] yy0 // jp
            //// --- maссив координат узлов 4-ми узловых КЭ  -----------------
            //, float[] xh0 // jh
            //, float[] yh0  // jh
            //// --- массив ф-й формы 8 узловых КЭ ---------------------------
            //, float[] uf0 // 8 * nmg
            //// --- массив ф-й формы 8 узловых КЭ ---------------------------
            //, float[] hf0 //  4 * nmg
            //// --- массив производных от ф-й формы 8 узловых КЭ ------------
            //, float[] dfx0 // 8 * nmg
            //, float[] dfy0 // 8 * nmg

            //, float[] auj0 // 5 * ne * nmg
            //// --- maссивы контурной среды --------------------------------- 
            //, float[] dl0 // 3 * ng
            //, float[] fl0 // 3 * ng
            //, float[] ds0// 4 ng * (mnn0[i] - 1) // 2
        ){}

        public SPIN(
            //--- количество горизотральных макроэлементов  --------------- 
            int noh
            // --- количество вертикальных макроэлементов  -----------------
            , int nov
            // --- количество узлов на горизонтальных гранях ---------------
            , int[] nh0 // noh
            // --- количество узлов на вертикальных гранях -----------------
            , int[] nv0 // nov
            // --- топологический maссив -----------------------------------
            , int[] mt0   // noh * nov
            // массив Х координат узлов макроэлементов
            , float[] xs0 // ku
            // массив У координат узлов макроэлементов               
            , float[] ys0 // ku
            )
        {
            this.noh = noh;
            this.nov = nov;
            this.nh0 = nh0;
            this.nv0 = nv0;
            this.mt0 = mt0;
            this.xs0 = xs0;
            this.ys0 = ys0;
        }

        public void gset(int kk = 0)
        {
            int i, j, k, l, ii, jj;
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
            vomc(in_fl, nh0, noh, kk); //* кол. узлов на горизонт. гранях ---
            vomc(in_fl, nv0, nov, kk); //* кол. узлов на вертикал. гранях ---
            vomc(in_fl, mt0, noh * nov, kk); //* топологический maссив ------

            // ________ вычисление объема требуемой памяти ------------------
            nh = summ(nh0, noh) - noh + 1; 
            nv = summ(nv0, nov) - nov + 1;
            kh = (nh - 1) / 2; 
            kv = (nv - 1) / 2; 
            int mx = 3 * noh + 1; 
            int my = 3 * nov + 1;
            int ke = kh * kv;
            int ku = mx * my - 4 * noh * nov; 
            jp = nv * nh - ke; 
            jh = (kh + 1) * (kv + 1);
            // -------- вычисление ширины лент глобальных матриц жесткости 
            //hak = nh + kh + 4; 
            //huk = 2 * hak; 
            //hhk = kh + 3;
            if (kk > 1)
            {
                // printf("nh= %4d nv= %4d kh= %4d kv= %4d ke= %4d ku= %4d\n",nh, nv, kh, kv, ke, ku);
            }
            // -------- выделение требуемого объема памяти ------------------
            MEM.Alloc(12 * noh * nov,ref no0);
            MEM.Alloc(ke, ref nx0);
            MEM.Alloc(nh * nv, ref mg0);
            MEM.Alloc(nh * nv, ref mw0);
            MEM.Alloc(8 * ke, ref nw0);
            MEM.Alloc(4 * ke, ref nb0);
            MEM.Alloc(ke, ref ms10);
            MEM.Alloc(ke, ref ms20);
            MEM.Alloc(ku, ref xs0);
            MEM.Alloc(ku, ref ys0);
            MEM.Alloc(jp, ref xx0);
            MEM.Alloc(jp, ref yy0);
            MEM.Alloc(jh, ref xh0);
            MEM.Alloc(jh, ref yh0);
            
            //* -------- генерация массива обхода узлов макроэлементов -------*//
            gmuse(no0, mx, my, noh, nov);
            //* -------- контрольный вывод массива узлов макроэлементов ------*//
            if (kk > 1)
                LOG.Print(" массив обхода макроэлементов", no0, 12 * (noh * nov), 12);
            //* ------ построчный ввод координат узлов суперэлементов -------*//
            vomv_xy(in_fl, xs0, ys0, ku, kk);
            //* ------- графический контрооль введенной информации --------*//
            // graph_chk(xs0, ys0, no0, ku, noh, nov,is, 0, 12);
            //* ------- инициализация скалярных параметров ----------------*//
            iu = 0; 
            ik = 1; 
            ko = 0; 
            nuh = 0; 
            //* ------- инициализация рабочих массивов --------------------*//
            for (l = 0; l < nh * nv; l++) 
                mw0[l] = kw0[l] = -1;
            //* ------- начало генерации КЭ сетки -------------------------*//
            //* ------- цикл по вертикальным подоблостям ------------------*//
            for (ii = 1; ii <= nov; ii++)
            {
                nvv = nv0[ii - 1];
                sy = 2.0f / (nvv - 1);
                //* ---- условие связности узлов подобластей ---------------*//
                if (ii == 1)  
                    in0= 1 ; 
                else 
                    in0= 2;
                //* ---- определение поузлового сдвига ksi -----------------*//
                ksi = summ(nv0, ii - 1); 
                ksi = ksi - ii + 1;
                //* ---- цикл по вертикали в подобласти --------------------*//
                for (i =in0; i <= nvv; i++)
                {
                    y = 1 - (i - 1) * sy; 
                    my = i / 2; 
                    my = (2 * my) - i;
                    //* -- цикл по горизонтальным подобластям --------------*//
                    for (jj = 1; jj <= noh; jj++)
                    {
                        nhh = nh0[jj - 1]; 
                        sx = 2.0f/(nhh - 1);
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
                                    mw0[nc] = ko; kw0[nc] = ko;
                                    nx0[ko - 1] = mt0[adm(ii, jj, noh)];
                                }
                                else
                                {  
                                    // вычисления массива значений
                                    // функций форм  текущего макроэлемента в
                                    // точке с координатами  х, у
                                    fff(x, y,ref ff0);
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
            ne = ko; 
            //ju = 2 * jp;
            nc = kh + 1;
            //* ---- генерация массивов обхода узлов конечноэлементной сетки -- *//
            //* для четырех и восьми узловых четырехугольных конечных элементов *//
            if (kk > 0)
                LOG.Print("mw0", mw0, nv * nh, nh);
                //priomc("mw0", mw0, nv * nh, nh);

            for (ic = 0; ic < ne; ic++)
            {
                i = ms10[ic]; 
                j = ms20[ic]; 
                ik = ic * 8; 
                il = ic * 4;
                nw0[ik + 0] = vdm(mw0, i + 1, j - 1, nh);
                nw0[ik  +1] = vdm(mw0, i + 1, j, nh);
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
            // --- генерация массива граничных узлов для восьмиузловых КЭ  --- 
            if (kk > 1)
            {
                LOG.Print("mw0", mw0, nv * nh, nh);
                //priomc("mw0", mw0, nv * nh, nh);
                LOG.Print(" массив обхода K. элементов", nw0, 8 * ke, 8);
                //priomc(" массив обхода K. элементов", nw0, 8 * ke, 8);
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
            //printf(" топологический массив mt0[] задан некоректно \n");
            return;
        nnnn: mg0[k] = nc; k++;
            // цикл обхода, по часовой стрелке конечноэлементной карты -- 
            for (; ;)
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
                    goto cont; 
                }
                if (npj > 0 && mg0[k - 1] != npj && npi < 0)
                { 
                    mg0[k] = npj; 
                    k++; 
                    j++; 
                    goto cont; 
                }
                if (nmi > 0 && mg0[k - 1] != nmi && npj < 0)
                { 
                    mg0[k] = nmi; 
                    k++; 
                    i--; 
                    goto cont; 
                }
                if (nmj > 0 && mg0[k - 1] != nmj && nmi < 0)
                { 
                    mg0[k] = nmj; 
                    k++; 
                    j--; 
                }
            cont:
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
                    (k > 1000)) 
                { 
                    nn0[l] = k - 1; 
                    break; 
                }
            }
            // цикл по граням области 
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
                //* ----- выделение памяти по массив граничных узлов ------- *//
                membory(kk);
                MEM.Alloc(4, l, ref mgt0);
                //mgt0[i] = (int*)calloc(l, sizeof(int)); 
                l = 0;
                // ----- цикл по узлам грани --------------------------- 
                for (k = m; k <= nn0[i]; k++)
                {
                    mgt0[i][l] = mg0[k];
                    l++;
                }
            }
            // ----- освобождение памяти массивом mg0 ------------------- 
            mg0 = null;
            //free(mg0);

            if (kk > 2)
            {
                LOG.Print("mw0", mw0, nv * nh, nh);
                LOG.Print("kw0", kw0, nv * nh, nh);
                //priomc("mw0", mw0, nv * nh, nh);
                //priomc("kw0", kw0, nv * nh, nh);
                i = 4; //priomc("nb0", nb0, 4 * ke, i);
                LOG.Print("nb0", nb0, 4 * ke, i);

                i = 8; 
                LOG.Print("nw0", nw0, 8 * ke, i);
                // priomc("nw0", nw0, 8 * ke, i);
                LOG.Print("xx0", xx0, jp, i);
                // priomf("xx0", xx0, jp, i);
                LOG.Print("yy0", yy0, jp, i);
                // priomf("yy0", yy0, jp, i);
                LOG.Print("xh0", xh0, jh, i);
                //priomf("xh0", xh0, jh, i);
                LOG.Print("yh0", xh0, jh, i);
                // priomf("yh0", xh0, jh, i);
            }
            //* ------- конец процедуры генерации КЭ сетки ------------------*//
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
        void fff(float x, float y, ref float[] ff0)
        {   
            /* ф-я вычисления массива значений функций двенадцати форм     */
            /* текущего макроэлемента в точке с координатами  х, у         */
            int i;
            float e, b;
            float[] xz0 = { -1.0f , -0.333333f, 0.333333f, 1.0f, 1.0f, 1.0f, 
                             1.0f, 0.3333333f, -0.3333333f, -1.0f, -1.0f, -1.0f };
            float[] yz0 = { -1.0f, -1.0f, -1.0f, -1.0f, -0.3333333f, 0.3333333f,
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
        /// ф-я суммирования одномерного массива или его чати от 0 до i-1 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        int summ(int[] id, int i)
        { 
            int k, l; l = k = 0;
            while (l < i) { k += id[l]; l++; }
            return k;
        }

        /// <summary>
        /// ф-я генерации массива обхода узлов макроэлементов
        /// </summary>
        /// <param name="no0"></param>
        /// <param name="nh"></param>
        /// <param name="nv"></param>
        /// <param name="kh"></param>
        /// <param name="kv"></param>
        void gmuse(int[] no0, int nh, int nv, int kh, int kv)
        {
            int[] j0 = null;
            int i, j, k, m, m1, ih, iv, iss, id, ir, io;
            k = nh * nv - 4 * kh * kv;
            MEM.Alloc(k, ref j0);
            i = 0;
            while (i < k)
            {
                iv = i;
                j0[iv] = ++i;
            }
            iv = 0;
            iss = ih = nh + 2 * (kh + 1);
            id = nh;
            ir = k = nh + kh + 1;
            for (i = 0; i < kv; i++)
            {
                m = m1 = 0;

                for (j = 0; j < kh; j++)
                {
                    io = 12 * j + 12 * i * kh;

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
        }

        /// <summary>
        ///  ф-я вычисления адреса эмулируемого указателем двухмерного массива 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        int adm(int i, int j, int h)
        {
            return (i - 1) * h + j - 1;
        }

        /// <summary>
        /// ф-я ввода и печати эмулируемого указателем *id  массива      
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="id"></param>
        /// <param name="d"></param>
        /// <param name="k"></param>
        void vomc(StreamReader inn, int[] id, int d, int k)
        { 
            int l; l = 0;
            //fscanf(in, " %s ", ti0);
            //if (k > 0) printf("\n %s %d ", ti0, d);
            //while (l < d) { fscanf(in, "%d", (id + l++)); }
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
        void vomv_xy(StreamReader inn, float[] xd0, float[] yd0, int d, int k)
        { 
            int l; float a, b; 
            string version = inn.ReadLine();
            //fscanf(in, " %s", ti0);
            for (l = 0; l < d; l++)
            {
                string line = inn.ReadLine();
                string[] lines = line.Split(new char[] { ',',' ',';','\t', '\n'});
                //fscanf(in, " %f %f", &a, &b); 
                xd0[l] = float.Parse(lines[0].Trim(), MEM.formatter);
                yd0[l] = float.Parse(lines[0].Trim(), MEM.formatter);
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

        /// <summary>
        /// ф-я скалярного произведения двух векторов
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="b0"></param>
        /// <param name="cu"></param>
        /// <returns></returns>
        float fun_sum(float[] a0, float[] b0, int cu)
        { 
            int i; float s;
            s = 0;
            for (i = 0; i < cu; i++) s += a0[i] * b0[i];
            return (s);
        }

        void membory(int k)
        { /* --- ф-я контроля свободной динамической памяти ----------- */
            long kk; float a;
            if (k > 0)
            {
                //kk = coreleft(); 
                //a = kk / 1024.0;
                //printf(" количество свободной памяти %f ", a);
            }
        }

        //void graph_chk(float* xs0, float* ys0, int* no0, int ku,
        //   int noh, int nov, int is, int kl, int cu)
        //{ /* ---- ф-я графического контролья КЭ сетки -------------- */
        //    int xxa, xxb, yya, yyb, i, j, ik, la, lb, n, maxx, maxy;
        //    union inkey { char ch[2]; int i; }
        //    ct;
        //    char buf[92];
        //    int GRDRV = DETECT;
        //    float xa, xb, ya, yb, xmm, xmn, ymm, ymn, xl, yl;
        //    xmm = fun_max(xs0, ku); xmn = fun_min(xs0, ku);
        //    ymm = fun_max(ys0, ku); ymn = fun_min(ys0, ku);
        //    xl = xmm - xmn; yl = ymm - ymn;
        //    /* ---- инициализация графического режима ----------------------- */
        //    initgraph(&GRDRV, 2, "DRV");
        //    maxx = getmaxx(); maxy = getmaxy();
        //    maxx = getmaxx() - 69; maxy = getmaxy() - 69;
        //    if (kl == 0) { if (maxx > maxy) maxx = maxy; else maxy = maxx; }
        //    if (is > 0)
        //    {
        //        if (yl > xl) maxx = ceil(xl * maxx / yl); else maxx = ceil(yl * maxx / xl);
        //    }
        //    /* ---- смена цвета экрана -------------------------------------- */
        //    setpalette(0, 15);
        //    setcolor(8);   /* черный */
        //    line(10, 10, 10, 469); line(629, 10, 629, 469);
        //    line(10, 10, 629, 10); line(10, 469, 629, 469);
        //    line(10, 444, 629, 444);
        //    setcolor(9);   /* синий */
        //    outtextxy(70, 453, " ESC - переход к меню ");
        //    outtextxy(270, 453, " Enter - продолжение генерации");
        //    setcolor(8); n = 0;
        //    /* ---- построение горизонтальных линий ------------------------- */
        //    for (ik = 0; ik < nov * noh; ik++)
        //    {
        //        n = cu * ik;
        //        for (j = 0; j < cu; j++)
        //        {
        //            if (j < cu - 1) { la = no0[n + j + 1] - 1; lb = no0[n + j] - 1; }
        //            else { la = no0[n] - 1; lb = no0[n + cu - 1] - 1; }
        //            xa = xs0[la]; xb = xs0[lb]; ya = ys0[la]; yb = ys0[lb];
        //            xxa = 30 + ceil((xa - xmn) * maxx / xl); yya = 25 + maxy - ceil((ya - ymn) * maxy / yl);
        //            xxb = 30 + ceil((xb - xmn) * maxx / xl); yyb = 25 + maxy - ceil((yb - ymn) * maxy / yl);
        //            line(xxa, yya, xxb, yyb);
        //        }
        //    }
        //    /* ----- cocтояние ожидания нажатия кловишы ---------------------- */
        //    for (; ; )
        //    {
        //        while (!bioskey(1)) ;                     /* ждать нажатия клавиши */
        //        ct.i = bioskey(0);
        //        ct.i = ct.i;
        //        switch (ct.ch[0])
        //        {                     /* обычная клавиша       */
        //            case 13: goto mbbb;           /* продолжить генерацию  */
        //            case 27:
        //                if (spawnl(P_OVERLAY, "DRV\\spin.exe", "DRV\\spin.exe", path, NULL))
        //                    printf("\n ошибка в файле DRV\\spin.exe");
        //                exit(-1);                                /* выйти      */
        //        }
        //    }
        //mbbb:
        //    /* ----- востановление цвета экрана ------------------------------ */
        //    setpalette(0, 15);
        //    /* ----- закрытие графического режима ---------------------------- */
        //    closegraph();
        //}



    }
}
