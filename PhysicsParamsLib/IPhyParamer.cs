//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.12.23
//---------------------------------------------------------------------------
namespace PhysicsParamsLib
{
    /// <summary>
    /// Физические скалярные параметры общие для всех задач
    /// </summary>
    public interface IPhyParamer
    {
        /// <summary>
        /// Значение
        /// </summary>
        //double Value(double arg = 0);
        double Value { get; }
        /// <summary>
        /// Название
        /// </summary>
        string Name { get;  }
        /// <summary>
        /// Метка параметра 
        /// </summary>
        string Alias { get; }
        ///// <summary>
        ///// Конвертор из строки
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //IPhyParamer Parse(string str);
        /// <summary>
        /// Конвертация в строку
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string ToString(string str = "");
    }
   
}
