//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.12.23
//---------------------------------------------------------------------------
namespace PhysicsParamsLib
{
    using System.Collections.Generic;
    /// <summary>
    /// Контейнер скалярных физических параметров задач
    /// </summary>
    public interface IPhysicsParams
    {
        /// <summary>
        /// Список параметров
        /// </summary>
        List<IPhyParamer> PhyParams { get; set; }
        /// <summary>
        /// Получить праметр по метке
        /// </summary>
        /// <param name="Alias"></param>
        /// <returns></returns>
        IPhyParamer GetPhyParams(string Alias);
        /// <summary>
        /// Получить значение параметра по метке
        /// </summary>
        /// <param name="Alias">Метка</param>
        /// <param name="arg"></param>
        /// <returns></returns>
        //double GetValuParams(string Alias, double arg = 0);
        double GetValuParams(string Alias);
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="p"></param>
        void LoadParams(string fileName="");
        /// <summary>
        /// Установка параметров по умолчанию
        /// </summary>
        void Initialize();
    }
}
