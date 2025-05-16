//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//                  - (C) Copyright 2000-2003
//                      ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                 кодировка : 1.02.2003 Потапов И.И.
//               ПРОЕКТ  "MixTasker' на базе "DISER"
//---------------------------------------------------------------------------
//        Перенос на C#, вариант от : 05.02.2022  Потапов И.И.
//    реализация генерации базисной КЭ сетки без поддержки полей свойств
//                         убран 4 порядок сетки 
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib
{
    using System;
    using System.Collections.Generic;
    using CommonLib;
    using MeshLib;
    /// <summary>
    /// ОО: Генерация массивов связности в области и на границе, и граничных узлов. 
    /// Создает КЭ сетки 1-3 порядка.
    /// Поддерживает трехгранные, четырехгранные и смешанные КЭ сетки
    /// абстрагирует способ вычисления координат узлов сетки
    /// </summary>
    public abstract class AMeshBuilder : MeshBuilderArgs, IMeshBuilder
    {
        /// <summary>
        /// Название строителя КЭ сетки
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// создаваемая сетка
        /// </summary>
        protected IFEMesh SMesh = null;
        List<IFENods> B = new List<IFENods>();
        List<IFENods> R = new List<IFENods>();
        List<IFENods> T = new List<IFENods>();
        List<IFENods> L = new List<IFENods>();
        /// <summary>
        /// настройка билдера
        /// </summary>
        /// <param name="mp">парамеры сетки</param>
        /// <param name="mesh">сетка</param>
        public void Set(MeshBuilderArgs mp, IFEMesh SMesh)
        {
            this.Set(mp);
            this.SMesh = SMesh;
        }
        /// <summary>
        /// вычисление массива конечных элементов в подобласти
        /// независимый вызов
        /// </summary>
        public virtual void BuilderAElement()
        {
            /// <summary>
            /// генерация массивов соответствий для подобласти
            /// </summary>
            // формирование массива соответствий
            int ii = 0, jj = 0;
            TypeFunForm ElemType = 0, DElemType = 0;
            List<int> Elem = new List<int>();
            List<int> DElem = new List<int>();
            List<IFElement> Elements = new List<IFElement>();

            MemLogLib.LOG.Print("map", Map);

            // Определение массива граничных узлов
            switch (meshRange) // порядок КЭ базы
            {
                case TypeRangeMesh.mRange1: // 1 порядок базовой сетки
                    {
                        // формирование массива соответствий
                        for (int i = 0; i < MaxNodeY - 1; i++)
                        {
                            for (int j = 0; j < MaxNodeX - 1; j++)
                            {
                                // создаем текущий элемент (или два :))
                                Elem.Clear();
                                DElem.Clear();
                                // определяем тип элемента
                                int keyFE = TestFE(i, j, ref ii, ref jj);
                                switch (keyFE)
                                {
                                    case 0: // центр
                                        if (meshType == TypeMesh.Rectangle ||
                                            meshType == TypeMesh.MixMesh)
                                        {
                                            Elem.Add(Map[i + 1][j]);
                                            Elem.Add(Map[i + 1][j + 1]);
                                            Elem.Add(Map[i][j + 1]);
                                            Elem.Add(Map[i][j]);
                                            ElemType = TypeFunForm.Form_2D_Rectangle_L1;
                                        }
                                        else
                                        {
                                            if ((i % 2 == 1 && j % 2 == 0) || (i % 2 == 0 && j % 2 == 1))
                                            {
                                                // левый нижний трехугольный КЭ
                                                Elem.Add(Map[i + 1][j + 1]);
                                                Elem.Add(Map[i][j + 1]);
                                                Elem.Add(Map[i][j]);
                                                // правый верхний трехугольный КЭ
                                                DElem.Add(Map[i + 1][j]);
                                                DElem.Add(Map[i + 1][j + 1]);
                                                DElem.Add(Map[i][j]);
                                            }
                                            else
                                            {
                                                // правый нижний трехугольный КЭ
                                                Elem.Add(Map[i + 1][j]);
                                                Elem.Add(Map[i][j + 1]);
                                                Elem.Add(Map[i][j]);
                                                // левый верхний трехугольник
                                                DElem.Add(Map[i + 1][j]);
                                                DElem.Add(Map[i + 1][j + 1]);
                                                DElem.Add(Map[i][j + 1]);
                                            }
                                            ElemType = TypeFunForm.Form_2D_Triangle_L1;
                                            DElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        }
                                        break;
                                    case 1: // 3 узловой КЭ слева внизу
                                        Elem.Add(Map[i + 1][j + 1]);
                                        Elem.Add(Map[i][j + 1]);
                                        Elem.Add(Map[i][j]);
                                        ElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                    case 2: // 3 узловой КЭ справа внизу
                                        Elem.Add(Map[i + 1][j]);
                                        Elem.Add(Map[i][j + 1]);
                                        Elem.Add(Map[i][j]);
                                        ElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                    case 3: // 3 узловой КЭ справа вверху
                                        Elem.Add(Map[i + 1][j]);
                                        Elem.Add(Map[i + 1][j + 1]);
                                        Elem.Add(Map[i][j]);
                                        ElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                    case 4: // 3 узловой КЭ слева вверху
                                        Elem.Add(Map[i + 1][j]);
                                        Elem.Add(Map[i + 1][j + 1]);
                                        Elem.Add(Map[i][j + 1]);
                                        ElemType = TypeFunForm.Form_2D_Triangle_L1;
                                        break;
                                }
                                // добавляем элемент в сетку
                                if (Elem.Count > 0)
                                {
                                    if (Elem[0] != -1 && Elem[1] != -1 && Elem[2] != -1)
                                    {
                                        Elements.Add(new FElement(Elem.ToArray(), area,  flag,  ElemType));
                                    }
                                }
                                // если определен парный КЭ добавляем и его
                                if (DElem.Count > 0)
                                {
                                    if (DElem[0] != -1 && DElem[1] != -1 && DElem[2] != -1)
                                    {
                                        Elements.Add(new FElement(DElem.ToArray(), area, flag, DElemType));
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TypeRangeMesh.mRange2: // 2 порядок
                    {
                        for (int i = 0; i < (MaxNodeY - 1) / 2; i++)
                        {
                            for (int j = 0; j < (MaxNodeX - 1) / 2; j++)
                            {
                                // создаем текущий элемент (или два :))
                                Elem.Clear();
                                DElem.Clear();
                                int keyFE = TestFE(i, j, ref ii, ref jj);
                                switch (keyFE)
                                {
                                    case -1:
                                        // пустой КЭ
                                        break;
                                    case 0: // центр - четырехугольный КЭ
                                        {
                                            if (meshType == TypeMesh.Rectangle ||
                                                meshType == TypeMesh.MixMesh)
                                            //    if (TypeMesh == mtTetrangleMesh || TypeMesh == mtMixMesh)
                                            {
                                                Elem.Add(Map[ii + 1][jj - 1]);
                                                Elem.Add(Map[ii + 1][jj]);
                                                Elem.Add(Map[ii + 1][jj + 1]);
                                                Elem.Add(Map[ii][jj + 1]);
                                                Elem.Add(Map[ii - 1][jj + 1]);
                                                Elem.Add(Map[ii - 1][jj]);
                                                Elem.Add(Map[ii - 1][jj - 1]);
                                                Elem.Add(Map[ii][jj - 1]);
                                                Elem.Add(Map[ii][jj]);
                                                ElemType = TypeFunForm.Form_2D_Rectangle_L2;
                                            }
                                            else
                                            {
                                                if ((i % 2 == 1 && j % 2 == 0) || (i % 2 == 0 && j % 2 == 1))
                                                {
                                                    // левый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    Elem.Add(Map[ii + 1][jj + 1]);
                                                    Elem.Add(Map[ii][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj]);
                                                    // правый верхний трехугольный КЭ
                                                    DElem.Add(Map[ii + 1][jj - 1]);
                                                    DElem.Add(Map[ii + 1][jj]);
                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj]);
                                                    DElem.Add(Map[ii - 1][jj - 1]);
                                                    DElem.Add(Map[ii][jj - 1]);
                                                }
                                                else
                                                {
                                                    // правый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii + 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    Elem.Add(Map[ii - 1][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj]);
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj - 1]);
                                                    // левый верхний трехугольник
                                                    DElem.Add(Map[ii + 1][jj - 1]);
                                                    DElem.Add(Map[ii + 1][jj]);
                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj + 1]);
                                                    DElem.Add(Map[ii - 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj]);
                                                }
                                                ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                                DElemType = TypeFunForm.Form_2D_Triangle_L2;
                                            }
                                        }
                                        break;
                                    case 1: // левый нижний трехугольный КЭ
                                        {
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                        }
                                        break;
                                    case 2: // правый нижний трехугольный КЭ
                                        {
                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                        }
                                        break;
                                    case 3: // правый верхний трехугольный КЭ
                                        {
                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                            break;
                                        }
                                    case 4: // левый верхний трехугольник
                                        {
                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L2;
                                        }
                                        break;
                                }
                                // добавляем элемент в сетку
                                if (Elem.Count > 0)
                                    Elements.Add(new FElement(Elem.ToArray(), area, flag, ElemType));
                                // если определен парный КЭ добавляем и его
                                if (DElem.Count > 0)
                                    Elements.Add(new FElement(DElem.ToArray(), area, flag, DElemType));
                            }
                        }
                    }
                    break;
                case TypeRangeMesh.mRange3: // 3 порядок
                    {
                        // формирование массива соответствий
                        for (int i = 0; i < (MaxNodeY - 1) / 3; i++)
                        {
                            for (int j = 0; j < (MaxNodeX - 1) / 3; j++)
                            {
                                // создаем текущий элемент (или два :))
                                Elem.Clear();
                                DElem.Clear();
                                int keyFE = TestFE(i, j, ref ii, ref jj);
                                switch (keyFE)
                                {
                                    case -1:
                                        // пустой КЭ
                                        break;
                                    case 0: // центр - четырехугольный КЭ
                                        {
                                            if (meshType == TypeMesh.Rectangle ||
                                            meshType == TypeMesh.MixMesh)
                                            //if (TypeMesh == mtTetrangleMesh || TypeMesh == mtMixMesh)
                                            {
                                                Elem.Add(Map[ii + 2][jj - 1]);
                                                Elem.Add(Map[ii + 2][jj]);
                                                Elem.Add(Map[ii + 2][jj + 1]);
                                                Elem.Add(Map[ii + 2][jj + 2]);

                                                Elem.Add(Map[ii + 1][jj + 2]);
                                                Elem.Add(Map[ii][jj + 2]);
                                                Elem.Add(Map[ii - 1][jj + 2]);
                                                Elem.Add(Map[ii - 1][jj + 1]);

                                                Elem.Add(Map[ii - 1][jj]);
                                                Elem.Add(Map[ii - 1][jj - 1]);
                                                Elem.Add(Map[ii][jj - 1]);
                                                Elem.Add(Map[ii + 1][jj - 1]);

                                                Elem.Add(Map[ii + 1][jj]);
                                                Elem.Add(Map[ii + 1][jj + 1]);
                                                Elem.Add(Map[ii][jj + 1]);
                                                Elem.Add(Map[ii][jj]);
                                                ElemType = TypeFunForm.Form_2D_Rectangle_L3;
                                            }
                                            else
                                            {
                                                if ((i % 2 == 1 && j % 2 == 0) || (i % 2 == 0 && j % 2 == 1))
                                                {
                                                    // левый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    Elem.Add(Map[ii + 1][jj + 1]);
                                                    Elem.Add(Map[ii + 2][jj + 2]);

                                                    Elem.Add(Map[ii + 1][jj + 2]);
                                                    Elem.Add(Map[ii][jj + 2]);
                                                    Elem.Add(Map[ii - 1][jj + 2]);
                                                    Elem.Add(Map[ii - 1][jj + 1]);

                                                    Elem.Add(Map[ii - 1][jj]);
                                                    Elem.Add(Map[ii][jj + 1]);
                                                    // правый верхний трехугольный КЭ
                                                    DElem.Add(Map[ii + 2][jj - 1]);
                                                    DElem.Add(Map[ii + 2][jj]);
                                                    DElem.Add(Map[ii + 2][jj + 1]);
                                                    DElem.Add(Map[ii + 2][jj + 2]);

                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                    DElem.Add(Map[ii][jj]);
                                                    DElem.Add(Map[ii - 1][jj - 1]);
                                                    DElem.Add(Map[ii][jj - 1]);

                                                    DElem.Add(Map[ii + 1][jj - 1]);
                                                    DElem.Add(Map[ii + 1][jj]);
                                                }
                                                else
                                                {
                                                    // правый нижний трехугольный КЭ
                                                    Elem.Add(Map[ii + 2][jj - 1]);
                                                    Elem.Add(Map[ii + 1][jj]);
                                                    Elem.Add(Map[ii][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj + 2]);

                                                    Elem.Add(Map[ii - 1][jj + 1]);
                                                    Elem.Add(Map[ii - 1][jj]);
                                                    Elem.Add(Map[ii - 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj - 1]);

                                                    Elem.Add(Map[ii + 1][jj - 1]);
                                                    Elem.Add(Map[ii][jj]);
                                                    // левый верхний трехугольник
                                                    DElem.Add(Map[ii + 2][jj - 1]);
                                                    DElem.Add(Map[ii + 2][jj]);
                                                    DElem.Add(Map[ii + 2][jj + 1]);
                                                    DElem.Add(Map[ii + 2][jj + 2]);

                                                    DElem.Add(Map[ii + 1][jj + 2]);
                                                    DElem.Add(Map[ii][jj + 2]);
                                                    DElem.Add(Map[ii - 1][jj + 2]);
                                                    DElem.Add(Map[ii][jj + 1]);

                                                    DElem.Add(Map[ii + 1][jj]);
                                                    DElem.Add(Map[ii + 1][jj + 1]);
                                                }
                                                ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                                DElemType = TypeFunForm.Form_2D_Triangle_L3;
                                            }
                                        }
                                        break;
                                    case 1:
                                        {
                                            // левый нижний трехугольный КЭ
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii + 2][jj + 2]);

                                            Elem.Add(Map[ii + 1][jj + 2]);
                                            Elem.Add(Map[ii][jj + 2]);
                                            Elem.Add(Map[ii - 1][jj + 2]);
                                            Elem.Add(Map[ii - 1][jj + 1]);

                                            Elem.Add(Map[ii - 1][jj]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;

                                        }
                                        break;
                                    case 2:
                                        {
                                            // правый нижний трехугольный КЭ
                                            Elem.Add(Map[ii + 2][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj + 2]);

                                            Elem.Add(Map[ii - 1][jj + 1]);
                                            Elem.Add(Map[ii - 1][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);

                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                        }
                                        break;
                                    case 3:
                                        {
                                            // правый верхний трехугольный КЭ
                                            Elem.Add(Map[ii + 2][jj - 1]);
                                            Elem.Add(Map[ii + 2][jj]);
                                            Elem.Add(Map[ii + 2][jj + 1]);
                                            Elem.Add(Map[ii + 2][jj + 2]);

                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            Elem.Add(Map[ii][jj]);
                                            Elem.Add(Map[ii - 1][jj - 1]);
                                            Elem.Add(Map[ii][jj - 1]);

                                            Elem.Add(Map[ii + 1][jj - 1]);
                                            Elem.Add(Map[ii + 1][jj]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                            break;
                                        }
                                    case 4:
                                        {
                                            // левый верхний трехугольник
                                            Elem.Add(Map[ii + 2][jj - 1]);
                                            Elem.Add(Map[ii + 2][jj]);
                                            Elem.Add(Map[ii + 2][jj + 1]);
                                            Elem.Add(Map[ii + 2][jj + 2]);

                                            Elem.Add(Map[ii + 1][jj + 2]);
                                            Elem.Add(Map[ii][jj + 2]);
                                            Elem.Add(Map[ii - 1][jj + 2]);
                                            Elem.Add(Map[ii][jj + 1]);

                                            Elem.Add(Map[ii + 1][jj]);
                                            Elem.Add(Map[ii + 1][jj + 1]);
                                            ElemType = TypeFunForm.Form_2D_Triangle_L3;
                                        }
                                        break;
                                }
                                // добавляем элемент в сетку
                                if (Elem.Count > 0)
                                    Elements.Add(new FElement(Elem.ToArray(), area, flag, ElemType));
                                // если определен парный КЭ добавляем и его
                                if (DElem.Count > 0)
                                    Elements.Add(new FElement(DElem.ToArray(), area, flag, DElemType));
                            }
                        }
                    }
                    break;
            }
            SMesh.AreaElems = Elements.ToArray();
        }
        /// <summary>
        /// вычисление массива граничных конечных элементов в подобласти
        /// вызывать после метода BuilderBNods();
        /// </summary>
        public virtual void BuilderBElement()
        {
            int cs = (int)meshRange;
            // количество граничных КЭ - в
            int CountBFE = (B.Count + R.Count + T.Count + L.Count - 4) / (int)meshRange;
            SMesh.BoundElems = new IFElement[CountBFE];
            int elem = 0;
            CreateBE(B.ToArray(), cs, 0, ref elem);
            CreateBE(R.ToArray(), cs, 1, ref elem);
            CreateBE(T.ToArray(), cs, 2, ref elem);
            CreateBE(L.ToArray(), cs, 3, ref elem);
        }
        /// <summary>
        /// вычисление массива граничных  узлов
        /// независимый вызов
        /// </summary>
        public virtual void BuilderBNods()
        {
            B.Clear();
            R.Clear();
            T.Clear();
            L.Clear();
            uint nf;
            try
            {
                // количество граничных узлов
                int CountBN = Right[0] - Left[0] +
                          Right[MaxNodeY - 1] - Left[MaxNodeY - 1] +
                          Right.Count + Left.Count;
                SMesh.BNods = new IFENods[CountBN];
                uint n = 0;
                // узлы границы против часовой стрелки
                // низ
                int id;
                int i = MaxNodeY - 1;
                for (int j = Left[i]; j < Right[i]; j++)
                {
                    id = Map[i][j];
                    SMesh.BNods[n] = new FENods(id, pMap[i][j].marker);
                    B.Add(SMesh.BNods[n]);
                    n++;
                }
                // Console.WriteLine();
                // справа
                nf = 0;
                for (i = Right.Count - 1; i > -1; i--)
                {
                    id = Map[i][Right[i] - 1];
                    SMesh.BNods[n] = new FENods(id, pMap[i][Right[i] - 1].marker);
                    R.Add(SMesh.BNods[n]);
                    n++;
                }
                // Console.WriteLine();
                // верх
                i = 0;
                nf = 0;
                for (int j = Right[i] - 1; j > Left[i] - 1; j--)
                {
                    id = Map[i][j];
                    SMesh.BNods[n] = new FENods(id, pMap[i][j].marker);
                    T.Add(SMesh.BNods[n]);
                    n++;
                }
                // Console.WriteLine();
                // слева
                nf = 0;
                for (i = 0; i < Left.Count; i++)
                {
                    id = Map[i][Left[i]];
                    SMesh.BNods[n] = new FENods(id, pMap[i][Left[i]].marker);
                    L.Add(SMesh.BNods[n]);
                    n++;
                }
            }
            catch(Exception ex)
            {
                MemLogLib.Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// вычисление координат узлов
        /// независимый вызов
        /// </summary>
        public abstract void BuilderCoords();
        /// <summary>
        /// Определение массива параметров сетки
        /// вызывать после метода BuilderCoords()
        /// </summary>
        public abstract void BuilderParams();
        /// <summary>
        /// Возвращает ссылку на сетку подобласти
        /// </summary>
        public IFEMesh GetFEMesh() => SMesh;
        #region Utils
        /// <summary>
        /// Инкрементная запись граничных КЭ
        /// </summary>
        /// <param name="M"></param>
        /// <param name="cs"></param>
        /// <param name="ID"></param>
        /// <param name="elem"></param>
        /// <param name="flag"></param>
        protected void CreateBE(IFENods[] M, int cs, int ID, ref int elem)
        {
            try
            {
                TypeFunForm tff = HFForm1D.GetTypeFunForm1D(meshRange);
                int Count = (M.Length - 1) / (int)meshRange;
                int nods = cs + 1;
                for (int be = 0; be < Count; be++)
                {
                    int bes = be >= RealSegmRibs[ID].MarkBC.Length - 1 ? RealSegmRibs[ID].MarkBC.Length - 1 : be;
                    int Mark = RealSegmRibs[ID].MarkBC[bes];
                    SMesh.BoundElems[elem] = new FElement(nods, ID, Mark, tff);
                    for (int s = 0; s < nods; s++)
                        SMesh.BoundElems[elem][s].ID = M[be * cs + s].ID;
                    elem++;
                }
            }
            catch (Exception ex)
            {
                MemLogLib.Logger.Instance.Exception(ex);
            }
        }
        /// <summary>
        /// можно создать инкрементный массив зависящий от range и код будет "проще" !
        /// </summary>
        /// <param name="i">КЭ</param>
        /// <param name="j">КЭ</param>
        /// <param name="ii">центр КЭ в мапе</param>
        /// <param name="jj">центр КЭ в мапе</param>
        /// <returns></returns>
        int TestFE(int i, int j, ref int ii, ref int jj)
        {
            int Type = 0;
            // определение типа КЭ
            switch (meshRange)
            {
                case TypeRangeMesh.mRange1:
                    ii = i;
                    jj = j;
                    if (Map[ii][jj] == -1 && Map[ii][jj + 1] == -1 &&
                       Map[ii + 1][jj] == -1 && Map[ii + 1][jj + 1] == -1) Type = -1;
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] != -1 &&
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] != -1) Type = 0;
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] != -1 &&       //  -|
                       Map[ii + 1][jj] == -1 && Map[ii + 1][jj + 1] != -1) Type = 1; //   |
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] != -1 &&       //  |-
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] == -1) Type = 2; //  |
                    else
                    if (Map[ii][jj] != -1 && Map[ii][jj + 1] == -1 &&       //  |
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] != -1) Type = 3; //  |_
                    else
                    if (Map[ii][jj] == -1 && Map[ii][jj + 1] != -1 &&       //   |
                       Map[ii + 1][jj] != -1 && Map[ii + 1][jj + 1] != -1) Type = 4; //  _|
                    break;
                case TypeRangeMesh.mRange2:
                    ii = 2 * i + 1;
                    jj = 2 * j + 1;
                    if (Map[ii][jj] == -1) Type = -1;
                    else
                    if (Map[ii - 1][jj - 1] == -1 && Map[ii - 1][jj + 1] == -1 &&
                       Map[ii + 1][jj - 1] == -1 && Map[ii + 1][jj + 1] == -1) Type = -1;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] != -1 &&
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] != -1) Type = 0;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] != -1 &&       //  -|
                       Map[ii + 1][jj - 1] == -1 && Map[ii + 1][jj + 1] != -1) Type = 1; //   |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] != -1 &&       //  |-
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] == -1) Type = 2; //  |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 1] == -1 &&       //  |
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] != -1) Type = 3; //  |_
                    else
                    if (Map[ii - 1][jj - 1] == -1 && Map[ii - 1][jj + 1] != -1 &&       //   |
                       Map[ii + 1][jj - 1] != -1 && Map[ii + 1][jj + 1] != -1) Type = 4; //  _|
                    break;
                case TypeRangeMesh.mRange3:
                    ii = 3 * i + 1;
                    jj = 3 * j + 1;
                    if (Map[ii][jj] == -1 && Map[ii][jj + 1] == -1 &&
                       Map[ii + 1][jj] == -1 && Map[ii + 1][jj + 1] == -1) Type = -1;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] != -1 &&
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] != -1) Type = 0;
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] != -1 &&       //  -|
                       Map[ii + 2][jj - 1] == -1 && Map[ii + 2][jj + 2] != -1) Type = 1; //   |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] != -1 &&       //  |-
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] == -1) Type = 2; //  |
                    else
                    if (Map[ii - 1][jj - 1] != -1 && Map[ii - 1][jj + 2] == -1 &&       //  |
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] != -1) Type = 3; //  |_
                    else
                    if (Map[ii - 1][jj - 1] == -1 && Map[ii - 1][jj + 2] != -1 &&       //   |
                       Map[ii + 2][jj - 1] != -1 && Map[ii + 2][jj + 2] != -1) Type = 4; //  _|
                    break;
                    //case TypeRangeMesh.mRange4:
                    //    ii = 4 * i + 2;
                    //    jj = 4 * j + 2;
                    //    if (Map[ii][jj] == -1) Type = -1;
                    //    else
                    //    if (Map[ii - 2][jj - 2] == -1 && Map[ii - 2][jj + 2] == -1 &&
                    //       Map[ii + 2][jj - 2] == -1 && Map[ii + 2][jj + 2] == -1) Type = -1;
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] != -1 &&
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] != -1) Type = 0;
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] != -1 &&       //  -|
                    //       Map[ii + 2][jj - 2] == -1 && Map[ii + 2][jj + 2] != -1) Type = 1; //   |
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] != -1 &&       //  |-
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] == -1) Type = 2; //  |
                    //    else
                    //    if (Map[ii - 2][jj - 2] != -1 && Map[ii - 2][jj + 2] == -1 &&       //  |
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] != -1) Type = 3; //  |_
                    //    else
                    //    if (Map[ii - 2][jj - 2] == -1 && Map[ii - 2][jj + 2] != -1 &&       //   |
                    //       Map[ii + 2][jj - 2] != -1 && Map[ii + 2][jj + 2] != -1) Type = 4; //  _|
                    //    break;
            }
            return Type;
        }
        #endregion

        /// <summary>
        /// Клонируем билдер
        /// </summary>
        /// <returns></returns>
        public abstract IMeshBuilder Clone();
    }
}
