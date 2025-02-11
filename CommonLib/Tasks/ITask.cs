#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          01.09.21
//---------------------------------------------------------------------------
//    добавлен массив неизвестных для алгоритмов автоматизации
//    по заданию краевых условий задач
//                          25.08.22
//---------------------------------------------------------------------------
namespace CommonLib
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// Метка задачи - для индитификации в менеджерах задач
    /// </summary>
    public struct TaskMetka
    {
        /// <summary>
        /// код задачи
        /// </summary>
        public int id;
        /// <summary>
        /// наименование задачи
        /// </summary>
        public string Name;
        public TaskMetka(string Name, int id)
        {
            this.id = id;
            this.Name = Name;
        }
    }
    /// <summary>
    /// Тип задачи используется для выбора 
    /// совместимых подзадач донных деформаций
    /// </summary>
    [Serializable]
    public enum TypeTask
    {
        /// <summary>
        /// продольные русловые потоки
        /// </summary>
        streamX1D = 0,
        /// <summary>
        /// поперечные русловые потоки
        /// </summary>
        streamY1D = 1,
        /// <summary>
        /// плановые русловые потоки
        /// </summary>
        streamXY2D = 2  
    }
    /// <summary>
    /// Тип задачи используется для выбора 
    /// совместимых турбулентных моделей
    /// </summary>
    [Serializable]
    public enum SpaceTypeTask
    {
        /// <summary>
        /// одномерные задачи
        /// </summary>
        stream1D = 0,
        /// <summary>
        /// двухмерные задачи
        /// </summary>
        stream2D = 1
    }
    /// <summary>
    /// ОО: Общая информация о задаче
    /// </summary>
    public interface ITask : IPropertyTask
    {
        /// <summary>
        /// Шаг по времени
        /// </summary>
        [DisplayName("Шаг по времени")]
        [Category("Задача")]
        double dtime { get; set; }
        /// <summary>
        /// Наименование задачи
        /// </summary>
        [DisplayName("Наименование задачи")]
        [Category("Задача")]
        string Name { get; }
        /// <summary>
        /// Тип задачи используется для выбора совместимых подзадач
        /// </summary>
        [DisplayName("Тип задачи")]
        [Category("Тип задачи")]
        TypeTask typeTask { get; }
        /// <summary>
        /// Текущее время выполнения задачи
        /// </summary>
        [Browsable(false)]
        [DisplayName("Текущее время")]
        [Category("Задача")]
        double time { get; set; }
        /// <summary>
        /// Готовность задачи к расчету
        /// </summary>
        bool TaskReady();
        /// <summary>
        /// Сетка для расчетной области задачи
        /// </summary>
        IMesh Mesh();
        /// <summary>
        /// версия дата последних изменений интерфейса задачи
        /// </summary>
        string VersionData();
        /// <summary>
        /// Искомые поля задачи которые позволяют сформировать на сетке расчетной области краевые условия задачи
        /// </summary>
        IUnknown[] Unknowns();
        /// <summary>
        /// Формирование задачей полей данных, привязанных к узлам расчетной сетки (IMesh)
        /// и добавление их в кадр задачи ISavePoint для их последующей визуализации
        /// </summary>
        /// <param name="sp">контейнер данных</param>
        void AddMeshPolesForGraphics(ISavePoint sp);
    }
}
