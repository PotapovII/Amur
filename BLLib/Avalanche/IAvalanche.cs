////              Реализация библиотеки для моделирования 
////               гидродинамических и русловых процессов
////                      - (C) Copyright 2021 -
////                       ALL RIGHT RESERVED
////                        ПРОЕКТ "BLLib"
////---------------------------------------------------------------------------
////                   разработка: Потапов И.И.
////                          25.12.2021
////---------------------------------------------------------------------------
//namespace BLLib
//{
//    using CommonLib;
//    using System.ComponentModel;
//    public interface IAvalanche
//    {
//        /// <summary>
//        /// Установка параметров модели
//        /// </summary>
//        /// <param name="mesh">сетка области</param>
//        /// <param name="tf">угол внутреннего трения</param>
//        /// <param name="Relax">релаксация обрушения</param>
//        /// <param name="Step">количество шагов по  обрушению</param>
//        void Set(IMesh mesh, double tf, double Relax=0.8);
//        /// <summary>
//        /// Лавинное обрушение дна
//        /// </summary>
//        /// <param name="Z">дно</param>
//        void Lavina(ref double[] Z);
//    }
//}
