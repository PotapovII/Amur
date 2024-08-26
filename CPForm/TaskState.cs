namespace CPForm
{
    /// <summary>
    /// Состояния задачи
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// старт нового расчета
        /// </summary>
        startTask = 0, 
        /// <summary>
        /// одни шаг расчета
        /// </summary>
        runOneStep,
        /// <summary>
        /// презапуск расчета с контекста  
        /// </summary>
        restartTask,
        /// <summary>
        /// расчет
        /// </summary>
        runTask,
        /// <summary>
        /// пауза расчета 
        /// </summary>
        pauseTask,
        /// <summary>
        /// остановка расчета
        /// </summary>
        stopTask       
    }
}
