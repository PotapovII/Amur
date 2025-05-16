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
    using BedLoadLib;
    using CommonLib;
    using CommonLib.BedLoad;
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
            // 1DX FEM
            tasks.Add(new BedLoadFEM_1XD());
            // 1DX FVM
            tasks.Add(new BedLoadFVM_1XD());
            tasks.Add(new BedLoadFVM_1XD_Bagnold());
            tasks.Add(new BedLoadFVM_1XD_Engelund());
            tasks.Add(new BedLoadFVM_1XD_Pow());
            tasks.Add(new BedLoadFVM_1XD_UTTc());
            tasks.Add(new BedLoadFVM_1XD_Z());
            // 1DX MIX
            tasks.Add(new СBedLoadFVM_1XD_MIX());

            // 1DY FVM
            tasks.Add(new BedLoadFVM_1YD());
            tasks.Add(new BedLoadFVMStream_1YD());
            // 1DY FVM MIX
            tasks.Add(new СSBedLoadFVM_1YD_MIX());

            // 2DX FEM
            tasks.Add(new CBedLoadFEMTaskTri_2D());
            // класс для произвольной сетки не активен требует дополнительной настройки
            // tasks.Add(new CBedLoadFEMTask_2D());

            //tasks.Add(new CBedLoadTask_1XD(new BedLoadParams()));
            //tasks.Add(new CBedLoadTask1D_UTTc(new BedLoadParams()));
            //tasks.Add(new CBedLoadTask1DZ(new BedLoadParams()));
            //tasks.Add(new CBedLoadTask1D_Pow(new BedLoadParams()));
            //tasks.Add(new CBedLoadTask1D_Engelund(new BedLoadParams()));
            //tasks.Add(new CBedLoadTask1DBagnold(new BedLoadParams()));

            //tasks.Add(new BedLoadTask_1YD(new BedLoadParams()));
            //tasks.Add(new CBedLoadMixTask1D(new BedMixModelParams()));
            //tasks.Add(new СRiverSideBedLoadTaskMix1D(new BedMixModelParams()));
            //tasks.Add(new CBedLoadFEMTask1DTri(new BedLoadParams()));

            ////tasks.Add(new BedLoadTask2D_AFVM(new BedLoadParams()));

            ////tasks.Add(new BedLoadTaskZeta2DFVM(new BedLoadParams()));
            //tasks.Add(new BedLoadTask2DFVM(new BedLoadParams()));
            //tasks.Add(new Adapter1Dfor2D(new BedLoadParams()));
            //tasks.Add(new Adapter2DTfor2DQ(new BedLoadParams()));
            ////tasks.Add(new BedLoadTask2DFV(new BedLoadParams()));



            //tasks.Add(new CBedLoadFEMTask2DTri(new BedLoadParams()));
            //tasks.Add(new CBedLoadFEMTask2DTri_GMAXN(new BedLoadParams()));
            //tasks.Add(new CBedLoadFEMTask2DTri_GMAX(new BedLoadParams()));
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
