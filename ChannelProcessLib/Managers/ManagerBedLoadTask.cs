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
    using BLLib;
    using CommonLib;
    using System.Collections.Generic;
    /// <summary>
    /// Менеджер решателей для задач донных деформаций
    /// </summary>
    public class ManagerBedLoadTask
    {
        /// <summary>
        ///  список задач расчета донных деформаций
        /// </summary>
        List<IBedLoadTask> tasks = new List<IBedLoadTask>();
        public ManagerBedLoadTask()
        {
            tasks.Add(new CBedLoadTask_1XD(new BedLoadParams()));
            tasks.Add(new CBedLoadTask1D_UTTc(new BedLoadParams()));
            tasks.Add(new CBedLoadTask1DZ(new BedLoadParams()));
            tasks.Add(new CBedLoadTask1D_Pow(new BedLoadParams()));
            tasks.Add(new CBedLoadTask1D_Engelund(new BedLoadParams()));
            tasks.Add(new CBedLoadTask1DBagnold(new BedLoadParams()));
            
            tasks.Add(new BedLoadTask_1YD(new BedLoadParams()));
            tasks.Add(new CBedLoadMixTask1D(new BedMixModelParams()));
            tasks.Add(new СRiverSideBedLoadTaskMix1D(new BedMixModelParams()));
            tasks.Add(new CBedLoadFEMTask1DTri(new BedLoadParams()));



            //tasks.Add(new BedLoadTask2D_AFVM(new BedLoadParams()));

            //tasks.Add(new BedLoadTaskZeta2DFVM(new BedLoadParams()));
            tasks.Add(new BedLoadTask2DFVM(new BedLoadParams()));
            tasks.Add(new Adapter1Dfor2D(new BedLoadParams()));
            tasks.Add(new Adapter2DTfor2DQ(new BedLoadParams()));
            //tasks.Add(new BedLoadTask2DFV(new BedLoadParams()));

            

            tasks.Add(new CBedLoadFEMTask2DTri(new BedLoadParams()));
            tasks.Add(new CBedLoadFEMTask2DTri_GMAXN(new BedLoadParams()));
            tasks.Add(new CBedLoadFEMTask2DTri_GMAX(new BedLoadParams()));
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
        public IBedLoadTask Clone(int id)
        {
            return tasks[id].Clone();
        }
    }
}
