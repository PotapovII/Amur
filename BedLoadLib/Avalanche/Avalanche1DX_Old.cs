//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2022 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          15.06.2022
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using CommonLib;
    /// <summary>
    /// ОО: Реализация класса SAvalanche вычисляющего осыпание склона из 
    /// несвязного материала. Осыпание склона происходит при превышении 
    /// уклона склона угла внутреннего трения для материала 
    /// формирующего склон.
    /// </summary>
    [Serializable]
    public class Avalanche1DX_Old : AvalancheX
    {
        public Avalanche1DX_Old(IMesh mesh, double tf, double Relax = 0.8) : base(mesh, tf, Relax)
        {
            Set(mesh, tf, Relax);
        }
        /// <summary>
        /// Лавинное обрушение дна
        /// </summary>
        /// <param name="Z">дно</param>
        public override void Lavina(ref double[] Z)
        {
            SAvalanche.Lavina(Z, ds, tf, Relax, 0);
        }
    }
}
