namespace MeshGeneratorsLib
{
    using CommonLib;
    using System.Collections.Generic;

    /// <summary>
    /// Менеджер решателей для задач расчета речного потока
    /// </summary>
    public class ManagerMeshBuilder
    {
        /// <summary>
        /// список задач расчета речного потока
        /// </summary>
        List<IMeshBuilder> tasks = new List<IMeshBuilder>();
        public ManagerMeshBuilder()
        {
            tasks.Add(new DiffMeshBuilder());
            tasks.Add(new AlgebraBuilder());
            tasks.Add(new DiffOrtoBuilder());
        }
        /// <summary>
        /// Список задач выбранного типа
        /// </summary>
        /// <param name="typeTask"></param>
        /// <returns></returns>
        public List<TaskMetka> GetStreamName()
        {
            List<TaskMetka> Names = new List<TaskMetka>();
            for (int id = 0; id < tasks.Count; id++)
                Names.Add(new TaskMetka(tasks[id].Name, id));
            return Names;
        }
        /// <summary>
        /// Получить экземпляр
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public IMeshBuilder Clone(int id)
        {
            return tasks[id].Clone();
        }
    }

}
