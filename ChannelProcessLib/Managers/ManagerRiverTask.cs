//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BLLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                     27.12.19 - 04.09.21
//---------------------------------------------------------------------------
//            Модуль BLLib для расчета донных деформаций 
//             (учет движения только влекомых наносов)
//    по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//---------------------------------------------------------------------------
namespace ChannelProcessLib
{
    using CommonLib;
       // using RiverLib;
        //using RiverLib.River2D;
        //using RiverLib.Patankar;
    using NPRiverLib.APRiver1YD;

    using System.Collections.Generic;
    using NPRiverLib.APRiver_2XYD.River2DSW;
    using NPRiverLib.APRiver_1XD.River2D_FVM_ke;
    using NPRiverLib.APRiver_1XD.River1DSW;
    using NPRiverLib.APRiver1XD.BEM_River2D;
    using NPRiverLib.APRiver2XYD.River2DFST;
    using NPRiverLib.APRiver1XD.KGD_River2D;
    using RiverLib;

    /// <summary>
    /// Менеджер решателей для задач расчета речного потока
    /// </summary>
    public class ManagerRiverTask
    {
        /// <summary>
        /// список задач расчета речного потока
        /// </summary>
        List<IRiver> tasks = new List<IRiver>();
        public ManagerRiverTask()
        {
            #region 1D течения 
            //tasks.Add(new RiverStatic1DSW(new River1DSWParams()));
            //tasks.Add(new RiverKGD1DSW(new River1DSWParams()));
            tasks.Add(new RiverSWEStatic1XD(new RiverSWEParams1XD()));
            tasks.Add(new RiverSWE_KGD1XD(new RiverSWEParams1XD()));
            #endregion

            #region 2XD течения 
            // Патанкар (КО) - канал - k -е модель (ортогональная криволинейная сетка)
            tasks.Add(new CPatankarStream1XD());
            // течение под цилиндром
            tasks.Add(new RiverBEMCircleMesh1XD(new RiverBEMParams1XD()));
            // КГД (КО/KE)- канал - k -е/w модели (симплексы)
            tasks.Add(new KGD_Eliz2024_1XD(new RGDParameters1XD()));

            //tasks.Add(new VPatankarStream1XD()); TODO добавить загрузчик для корректного старта
            //tasks.Add(new RiverBEMCircleMeshAdapt1XD(new RiverBEMParams1XD()));
            //tasks.Add(new River2DFV_rho_var());
            //tasks.Add(new River2DFV_rho_const());
            //tasks.Add(new River2D());
            //tasks.Add(new RiverSW_FCT(new RiverSW_FCTParams()));
            //tasks.Add(new GasDynamics_FCT(new RiverSW_FCTParams()));
            //tasks.Add(new WaterTaskEliz2020(new WElizParameter()));
            #endregion

            #region 1YD течение в створе реки
                //tasks.Add(new TriSroSecRiverTask(new RiverStreamParams()));
                //tasks.Add(new VarSroSecRiverTask(new RiverStreamParams()));
                //tasks.Add(new RiverStreamTask(new RiverStreamParams()));
                //tasks.Add(new SrossSectionalRiverTask(new RiverStreamParams()));
                //tasks.Add(new TriSroSecRiverTask_1Y(new RiverStreamParams()));

            tasks.Add(new TriSroSecRiverTask1YD());
            tasks.Add(new TriRoseRiverTask1YD());
            // 
                //tasks.Add(new RiverSectionalQuad(new RiverStreamQuadParams()));
                tasks.Add(new RiverSectionaChannel(new RiverStreamQuadParams()));
                //tasks.Add(new RiverSectionaChannelQuad(new RiverStreamQuadParams()));
                //tasks.Add(new RiverSectionaChannelTrapez(new RiverStreamQuadParams()));


                //tasks.Add(new RiverSectionalQuad_SV(new RiverStreamQuadParams()));
                //tasks.Add(new RiverSectionalQuad_Phi(new RiverStreamQuadParams()));

            #endregion

            #region  2XYD 

            tasks.Add(new TriRiverSWE_2XYD());
            tasks.Add(new RiverSWE_FCT_2XYD());
            
            #endregion

            #region  прокси
                //tasks.Add(new RiverEmptyX1D(new RiverStreamParams()));
                //tasks.Add(new RiverFunctionX1D(new RiverStreamParams()));
                //tasks.Add(new RiverEmptyY1D(new RiverStreamParams()));

                //tasks.Add(new RiverEmptyXY2DQuad(new RiverStreamParams()));
                //tasks.Add(new RiverFunctionX1DTest01(new RiverStreamParams()));
                //tasks.Add(new RiverEmptyXY2D(new RiverStreamParams()));
            #endregion

        }
        /// <summary>
        /// Список задач выбранного типа
        /// </summary>
        /// <param name="typeTask"></param>
        /// <returns></returns>
        public List<TaskMetka> GetStreamNameBLTask(TypeTask typeTask)
        {
            List<TaskMetka> Names = new List<TaskMetka>();
            for (int id = 0; id < tasks.Count; id++)
                if (tasks[id].typeTask == typeTask)
                    Names.Add(new TaskMetka(tasks[id].Name, id));
            return Names;
        }
        /// <summary>
        /// Получить экземпляр
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public IRiver Clone(int id)
        {
            return tasks[id].Clone();
        }
    }
}
