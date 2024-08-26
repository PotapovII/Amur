//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 24.07.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace NPRiverLib.APRiver1XD.SWE_River1D
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Контейнер для решения тестовых задач или задач по умолчанию
    /// </summary>
    [Serializable]
    public class TaskTestClass
    {
        protected Dictionary<string, Action> m_actions = new Dictionary<string, Action>();
        List<string> names = new List<string>();
        /// <summary>
        /// Добавить тестовыую задачу 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void Add(string name, Action action)
        {
            if (m_actions.ContainsKey(name) == false)
            {
                m_actions.Add(name, action);
                names.Add(name);
            }
        }
        /// <summary>
        /// Создает список тестовых задач для загрузчика по умолчанию
        /// </summary>
        /// <returns></returns>
        public List<string> GetTestsNames()
        {
            return names;
        }
        /// <summary>
        /// Получить делегат по имени
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public Action GetTestsNames(string Name)
        {
            return m_actions[Name];
        }
        /// <summary>
        /// Получить делегат по коду
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Action GetTestsNames(int ID)
        {
            return m_actions[names[ID]];
        }
        /// <summary>
        /// Выполнить делегат по имени
        /// </summary>
        /// <param name="Name"></param>
        public void Do(string Name)
        {
            m_actions[Name]();
        }
        /// <summary>
        /// Выполнить делегат по коду
        /// </summary>
        public void Do(int ID)
        {
            m_actions[names[ID]]();
        }
    }
}
