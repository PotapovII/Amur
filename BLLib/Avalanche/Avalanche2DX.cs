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
    public class Avalanche2DX : AvalancheX
    {
        public DirectAvalanche directAvalanche = DirectAvalanche.AvalancheXY;
        /// <summary>
        /// длины сторон треугольных КЭ
        /// </summary>
        protected double[][] FacetLength = null;
        protected bool[][] FacetFlag = null;
        public Avalanche2DX(IMesh mesh, double tf, DirectAvalanche directAvalanche = DirectAvalanche.AvalancheX, double Relax = 0.5) :
            base(mesh, tf, Relax)
        {
            TriElement[] Elems = mesh.GetAreaElems();
            this.directAvalanche = directAvalanche;
            MEM.Alloc2DClear(Elems.Length, 3, ref FacetLength);
            MEM.Alloc(Elems.Length, 3, ref FacetFlag);
            CalkFacet();
        }

        protected bool Check(int elem, double dx,double dy)
        {
            switch(directAvalanche)
            {
                case DirectAvalanche.AvalancheX:
                    if (MEM.Equals(dx, 0, MEM.Error6) == false && MEM.Equals(dy, 0, MEM.Error6) == true)
                        return true;
                    else
                        return false;
                case DirectAvalanche.AvalancheY:
                    if (MEM.Equals(dx, 0, MEM.Error6) == true && MEM.Equals(dy, 0, MEM.Error6) == false)
                        return true;
                    else
                        return false;
                case DirectAvalanche.AvalancheXY:
                    return true;
            }
            return true;
        }
        /// <summary>
        /// вычисление длин сторон треугольных КЭ
        /// </summary>
        public void CalkFacet()
        {
            try
            {
                double[] x = mesh.GetCoords(0);
                double[] y = mesh.GetCoords(1);
                uint knotA, knotB;
                TriElement[] Elems = mesh.GetAreaElems();
                
                for (int elem = 0; elem < Elems.Length; elem++)
                {
                    knotA = Elems[elem].Vertex1;
                    knotB = Elems[elem].Vertex2;
                    double dx = x[knotA] - x[knotB];
                    double dy = y[knotA] - y[knotB];
                    FacetFlag[elem][0] = Check(elem, dx, dy);
                    FacetLength[elem][0] = Math.Sqrt(dx * dx + dy * dy) * tf;
                    knotA = Elems[elem].Vertex2;
                    knotB = Elems[elem].Vertex3;
                    dx = x[knotA] - x[knotB];
                    dy = y[knotA] - y[knotB];
                    FacetFlag[elem][1] = Check(elem, dx, dy);
                    FacetLength[elem][1] = Math.Sqrt(dx * dx + dy * dy) * tf;
                    knotA = Elems[elem].Vertex3;
                    knotB = Elems[elem].Vertex1;
                    dx = x[knotA] - x[knotB];
                    dy = y[knotA] - y[knotB];
                    FacetFlag[elem][2] = Check(elem, dx, dy);
                    FacetLength[elem][2] = Math.Sqrt(dx * dx + dy * dy) * tf;
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.Exception(ex);
            }
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
            //Console.WriteLine( "лавинных итераций : " + i.ToString());
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
            TriElement[] Elems = mesh.GetAreaElems();
            for (int elem = 0; elem < Elems.Length; elem++)
            {
                if (FacetFlag[elem][0] == true)
                    AvalancheFacet(ref Z[Elems[elem].Vertex1], ref Z[Elems[elem].Vertex2], FacetLength[elem][0]);
                if (FacetFlag[elem][1] == true)
                    AvalancheFacet(ref Z[Elems[elem].Vertex2], ref Z[Elems[elem].Vertex3], FacetLength[elem][1]);
                if (FacetFlag[elem][2] == true)
                    AvalancheFacet(ref Z[Elems[elem].Vertex3], ref Z[Elems[elem].Vertex1], FacetLength[elem][2]);
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
        public void AvalancheFacet(ref double za,ref double zb, double dh)
        {
            double dz = zb - za;
            if(dh < Math.Abs(dz))
            {
                double delta = (Math.Abs(dz)- dh) * Relax / 3;
                maxDz = Math.Max(maxDz, Math.Abs(delta));
                if (zb < za)
                {
                    za -= delta;
                    zb += delta;
                }
                else
                {
                    za += delta;
                    zb -= delta;
                }
            }
        }
    }
}
