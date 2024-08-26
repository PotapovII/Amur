//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          19.12.2021
//---------------------------------------------------------------------------
namespace BLLib
{
    using System;
    using MemLogLib;
    using CommonLib;
    /// <summary>
    /// ОО: Реализация класса SAvalanche вычисляющего осыпание склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// </summary>
    [Serializable]
    public class Avalanche2DFV : AvalancheX
    {
        /// <summary>
        /// Сетка для КО методов
        /// </summary>
        protected IFVComMesh fvMesh;

        public Avalanche2DFV(IFVComMesh fvMesh, double tf, double Relax = 0.5) :
            base(fvMesh, tf, Relax)
        {
            this.fvMesh = fvMesh;
        }
        /// <summary>
        /// Лавинное обрушение дна
        /// </summary>
        /// <param name="Z">дно</param>
        public override void Lavina(ref double[] Z)
        {
            int i = 0;
            for (; i < 2; i++)
            {
                if (LavinaTriangle(ref Z) == false)
                    break;
            }
        }
        /// <summary>
        /// Метод лавинного обрушения правого склона, 
        /// обрушение с правой стороны
        /// </summary>
        /// <param name="Z">массив донных отметок</param>
        /// <param name="ds">координаты узлов</param>
        /// <param name="tf">тангенс внутреннено трения **</param>
        /// <param name="Step">шаг остановки процесса, 
        /// при 0 лавина проходит полностью</param>
        public bool LavinaTriangle(ref double[] Z)
        {
            bool flag = true;
            maxDz = 0;
            for (int elem = 0; elem < fvMesh.CountElements; elem++)
            {
                IFVElement fve = fvMesh.AreaElems[elem];
                for (int nf =0; nf < fve.Facets.Length; nf++)
                {
                    IFVFacet facet = fve.Facets[nf];
                    if(facet.NBElem != null)
                    {
                        double Distance = fve.Distance[nf];
                        AvalancheFacet(ref Z[elem], ref Z[facet.NBElem.Id], fve.Distance[nf], fve.Volume, facet.NBElem.Volume);
                    }
                }
            }
            if (maxDz < MEM.Error4)
                flag = false;
            return flag;
        }
        /// <summary>
        /// Осыпание вдоль ребра 
        /// </summary>
        /// <param name="za"></param>
        /// <param name="zb"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        public void AvalancheFacet(ref double za,ref double zb, double dh, double Va, double Vb)
        {
            double mV = Math.Min(Va, Vb);
            double dz = zb - za;
            if(dh < Math.Abs(dz))
            {
                double delta = 0.25 * (Math.Abs(dz)- dh) * Relax * mV;
                maxDz = Math.Max(maxDz, Math.Abs(delta));
                if (zb < za)
                {
                    za -= delta / Va;
                    zb += delta / Vb;
                }
                else
                {
                    za += delta / Va;
                    zb -= delta / Vb; 
                }
            }
        }
    }
}
