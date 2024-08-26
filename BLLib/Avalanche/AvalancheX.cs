//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          04.06.2022
//---------------------------------------------------------------------------
namespace BLLib
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
    public abstract class AvalancheX : IAvalanche
    {
    
        /// <summary>
        /// координаты узлов по оси Х
        /// </summary>
        protected double[] ds;
        /// <summary>
        /// угол внутреннего трения
        /// </summary>
        protected double tf;
        /// <summary>
        /// релаксация обрушения
        /// </summary>
        protected double Relax;
        /// <summary>
        /// Сетка 
        /// </summary>
        protected IMesh mesh;

        protected double maxDz = 0;
        public AvalancheX(IMesh mesh, double tf, double Relax = 0.8)
        {
            Set(mesh, tf, Relax);
        }
        /// <summary>
        /// Установка параметров модели
        /// </summary>
        /// <param name="mesh">сетка области</param>
        /// <param name="tf">угол внутреннего трения</param>
        /// <param name="Relax">релаксация обрушения</param>
        /// <param name="Step">количество шагов по  обрушению</param>
        public void Set(IMesh mesh, double tf, double Relax = 0.8)
        {
            this.mesh = mesh;
            ds = mesh.GetCoords(0);
            this.tf = tf;
            this.Relax = Relax;
        }
        /// <summary>
        /// Лавинное обрушение дна
        /// </summary>
        /// <param name="Z">дно</param>
        public abstract void Lavina(ref double[] Z);
    }
}
