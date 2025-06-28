//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2019 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                     27.12.19 - 04.09.21
//---------------------------------------------------------------------------
//            Модуль BedLoadLib для расчета донных деформаций 
//             (учет движения только влекомых наносов)
//    по русловой модели Петрова А.Г. и Потапова И.И. от 2014 г.
//---------------------------------------------------------------------------
namespace ChannelProcessLib
{
    using System.Collections.Generic;

    using CommonLib;
    using NPRiverLib.ABaseTask;
    using NPRiverLib.APRiver_1XD;
    using NPRiverLib.APRiver1YD;

    using NPRiverLib.APRiver_1XD.River2D_FVM_ke;
    using NPRiverLib.APRiver_1XD.River1DSW;
    
    using NPRiverLib.APRiver1XD.BEM_River2D;
    using NPRiverLib.APRiver2XYD.River2DFST;
    using NPRiverLib.APRiver1XD.KGD_River2D;
    
    using NPRiverLib.APRiver2XYD.River2DSW;
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
            #region 1XD течения 
            //tasks.Add(new RiverStatic1DSW(new River1DSWParams()));
            //tasks.Add(new RiverKGD1DSW(new River1DSWParams()));
            tasks.Add(new RiverSWEStatic1XD(new RiverSWEParams1XD()));
            tasks.Add(new RiverSWE_KGD1XD(new RiverSWEParams1XD()));
            // КГД (КО/KE)- канал - k -е/w модели (симплексы)
            tasks.Add(new KGD_Eliz2024_1XD());
            tasks.Add(new TriFEMRiver_1XD());
            #endregion

            #region 2XD течения 
            // Патанкар (КО) - канал - k -е модель (ортогональная криволинейная сетка)
            tasks.Add(new CPatankarStream1XD());
            // течение под цилиндром
            tasks.Add(new RiverBEMCircleMesh1XD(new RiverBEMParams1XD()));
            #endregion

            #region 1YD течение в створе реки
                tasks.Add(new TriSecRiver_1YD());
                tasks.Add(new TriSecRiverTask1YBase());
                tasks.Add(new TriSecRiverTask1YD());
                tasks.Add(new TriSroSecRiverTask1YD());
                //tasks.Add(new TriRoseRiverTask1YD());

                tasks.Add(new RiverSectionalQuad(new RiverStreamQuadParams()));
                tasks.Add(new RiverSectionaChannel(new RiverStreamQuadParams()));
                tasks.Add(new RiverSectionaChannelQuad(new RiverStreamQuadParams()));
       //         tasks.Add(new RiverSectionaChannelTrapez(new RiverStreamQuadParams()));
                tasks.Add(new RiverSectionalQuad_SV(new RiverStreamQuadParams()));
                tasks.Add(new RiverSectionalQuad_Phi(new RiverStreamQuadParams()));

            #endregion

            #region  2XYD 
                tasks.Add(new TriRiverSWE2XYD());
                tasks.Add(new RiverSWE_FCT_2XYD());
            #endregion

            #region  прокси
                tasks.Add(new RiverEmpty2XYD());
                tasks.Add(new RiverEmpty1XDCircle());
                tasks.Add(new RiverEmpty1XDTest01());

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
