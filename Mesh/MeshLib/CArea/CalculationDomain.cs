////---------------------------------------------------------------------------
////              Реализация библиотеки для моделирования 
////               гидродинамических и русловых процессов
////---------------------------------------------------------------------------
////                    21.02.2024 Потапов И.И.
////---------------------------------------------------------------------------
////                  Эксперимент: прототип "Расчетная область"
////---------------------------------------------------------------------------
//namespace MeshLib.CArea
//{

//    using MemLogLib;
//    using CommonLib;
//    using CommonLib.Tasks;
    
//    using System;
//    using System.Collections.Generic;
//    /// <summary>
//    /// Расчетная область
//    /// </summary>
//    [Serializable]
//    public class CalculationDomain : ABaseCalculationDomain
//    {
//        /// <summary>
//        /// Создание расчетной области
//        /// </summary>
//        /// <param name="mesh"></param>
//        /// <param name="unknowns"></param>
//        public CalculationDomain(IMesh mesh, IUnknown[] unknowns) : base(mesh, unknowns)
//        {
//            MEM.Alloc((int)unknowns.Length, mesh.CountBoundElements, ref typeBoundCond);
//            MEM.Alloc((int)unknowns.Length, mesh.CountBoundElements, ref bcValues);
//        }
//        /// <summary>
//        /// Получить индексы КЭ с заданным типом TypeBoundCond
//        /// </summary>
//        /// <param name="unknow"></param>
//        /// <param name="bt"></param>
//        /// <returns></returns>
//        public void idxBoundElementsType(uint unknown, TypeBoundCond bt, ref int[] idxElem)
//        {
//            List<int> idx = new List<int>();
//            for (int i = 0; i < typeBoundCond[unknown].Length; i++)
//            {
//                if (typeBoundCond[unknown][i] == bt)
//                    idx.Add(i);
//            }
//            idxElem = idx.ToArray();
//        }
//        /// <summary>
//        /// Получить КЭ и параметры на грничных элементах
//        /// </summary>
//        /// <param name="unknown">Номер неизвестной</param>
//        /// <param name="bt">тип граничного условия</param>
//        /// <param name="idxElem">индексы граничных элементов</param>
//        /// <param name="bVElem">значения параметров на граничных элементах</param>
//        public void GetBoundElementsType(uint unknown, ref int[] idxElem, ref IBCValues[] bVElem)
//        {
//            List<int> idx = new List<int>();
//            List<BCValues> vals = new List<BCValues>();
//            for (int i = 0; i < typeBoundCond[unknown].Length; i++)
//            {
//                if (typeBoundCond[unknown][i] == bt)
//                {
//                    idx.Add(i);
//                    vals.Add(new BCValues(bcValues[unknown][i]));
//                }
//            }
//            idxElem = idx.ToArray();
//            bVElem = vals.ToArray();
//        }
//        /// <summary>
//        /// Недостаток !!!
//        /// 1. Работа с TwoElement для элементов старших порядков
//        /// </summary>
//        /// <param name="unknow"></param>
//        /// <param name="bt"></param>
//        /// <param name="idxBoundKnot"></param>
//        public void idxBoundKnotType(uint unknown, TypeBoundCond bt,
//            ref uint[] idxBoundKnot, ref BCValues[] valBoundKnot)
//        {
//            int[] idxElem = null;
//            IBCValues[] bVals = null;
//            GetBoundElementsType(unknown, bt, ref idxElem, ref bVals);
//            List<uint> idx = new List<uint>();
//            TwoElement[] bElems = mesh.GetBoundElems();
//            double[] x = mesh.GetCoords(0);
//            double[] y = mesh.GetCoords(1);

//            BCValues[] bCValues = new BCValues[x.Length];
//            double[] L = new double[x.Length];

//            for (int i = 0; i < idxElem.Length; i++)
//            {
//                int ie = idxElem[i];
//                TwoElement be = bElems[ie];
//                IBCValues bv = bVals[i];
//                if (idx.Contains(be.Vertex1) == false)
//                    idx.Add(be.Vertex1);
//                if (idx.Contains(be.Vertex2) == false)
//                    idx.Add(be.Vertex2);
//                double dx = x[be.Vertex2] - x[be.Vertex1];
//                double dy = y[be.Vertex2] - y[be.Vertex1];
//                double dL2 = Math.Sqrt(dx * dx + dy * dy) / 2;
//                L[be.Vertex1] += dL2;
//                L[be.Vertex2] += dL2;

//                if (bCValues[be.Vertex1] == null)
//                {
//                    bCValues[be.Vertex1] = new BCValues(bv);
//                    bCValues[be.Vertex1].Mult(dL2);
//                }
//                else
//                {
//                    BCValues tmp = new BCValues(bv);
//                    tmp.Mult(dL2);
//                    bCValues[be.Vertex1].Sum(tmp);
//                }
//                if (bCValues[be.Vertex2] == null)
//                {
//                    bCValues[be.Vertex2] = new BCValues(bv);
//                    bCValues[be.Vertex2].Mult(dL2);
//                }
//                else
//                {
//                    BCValues tmp = new BCValues(bv);
//                    tmp.Mult(dL2);
//                    bCValues[be.Vertex2].Sum(tmp);
//                }
//            }
//            for (int i = 0; i < bCValues.Length; i++)
//            {
//                if (bCValues[i] != null)
//                    bCValues[i].Mult(1 / L[i]);
//            }
//            List<BCValues> LbCValues = new List<BCValues>();
//            for (int i = 0; i < idx.Count; i++)
//            {
//                LbCValues.Add(bCValues[idx[i]]);
//            }
//            idxBoundKnot = idx.ToArray();
//            valBoundKnot = LbCValues.ToArray();
//        }

//        /// <summary>
//        /// Значения величин для выполнения граничных на ГКЭ расчетной области 
//        /// для каждой неизвестной
//        /// </summary>
//        /// <param name="unknow">номер неизвестной</param>
//        /// <returns></returns>
//        BCValues[][] bcValues = null;
//        public IBCValues[] GetBoundElementsValues(uint unknown) => bcValues[unknown];

//        /// <summary>
//        /// Установка Граничных условий для неизвестной unknown
//        /// </summary>
//        /// <param name="tbc"></param>
//        /// <param name="bcv"></param>
//        /// <param name="unknown"></param>
//        public void Set(TypeBoundCond[] tbc, BCValues[] bcv, uint unknown)
//        {
//            for (int i = 0; i < tbc.Length; i++)
//                typeBoundCond[unknown][i] = tbc[i];
//            for (int i = 0; i < tbc.Length; i++)
//                bcValues[unknown][i] = new BCValues(bcv[i]);
//        }

//        /// <summary>
//        /// Начальные условия
//        /// </summary>
//        public override void InitValues() { }
//        /// <summary>
//        /// Вычисление зависимых граничных условий
//        /// </summary>
//        /// <param name="unknown"></param>
//        public override void CalcBoundary(uint unknown) { }
//    }

//}
