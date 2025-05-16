//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.12.23
//---------------------------------------------------------------------------
namespace PhysicsParamsLib
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    /// <summary>
    /// Контейнер скалярных физических параметров задач
    /// </summary>
    public class PhysicsParams : IPhysicsParams
    {
        /// <summary>
        /// Список параметров
        /// </summary>
        public  List<IPhyParamer> PhyParams { get; set; }
        /// <summary>
        /// Получить праметр по метке
        /// </summary>
        /// <param name="Alias"></param>
        /// <returns></returns>
        public IPhyParamer GetPhyParams(string Alias)
        {
            foreach (IPhyParamer p in PhyParams)
                if (p.Alias == Alias)
                    return p;
            throw new Exception("Параметр не райден");
        }
        /// <summary>
        /// Получить значение параметра по метке
        /// </summary>
        /// <param name="Alias">Метка</param>
        /// <param name="arg"></param>
        /// <returns></returns>
        //double GetValuParams(string Alias, double arg = 0);
        public double GetValuParams(string Alias)
        {
            foreach (IPhyParamer p in PhyParams)
                if (p.Alias == Alias)
                    return p.Value;
            throw new Exception("Параметр не райден");
        }
        /// <summary>
        /// Чтение параметров задачи из файла
        /// </summary>
        /// <param name="p"></param>
        public void LoadParams(string fileName="")
        {
            try
            {
                if (fileName == "")
                    Initialize();
                else
                {
                    PhyParams.Clear();
                    string[] lines = File.ReadAllLines(fileName);
                    foreach (string s in lines)
                        PhyParams.Add(PhyParamer.Parse(s));
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
                throw new Exception("Файл параметров не загружен");
            }
        }
        public void Initialize()
        {
            PhyParams.Clear();
            #region Физика воды
            PhyParams.Add(new PhyParamer(1000,"rho_w","Плотность воды"));
            PhyParams.Add(new PhyParamer(0.000001, "nu_w", "Кин. вязкость воды"));
            PhyParams.Add(new PhyParamer(0.001, "mu_w", "Мол. вязкость воды"));
            PhyParams.Add(new PhyParamer(0.4, "kappa_w", "Коэффициент Кармана для воды"));
            PhyParams.Add(new PhyParamer(0.07275, "sigma_w", "Коэффициент поверхностного натяжения воды"));
            #endregion
            #region Физика песка
            PhyParams.Add(new PhyParamer(2650, "rho_s", "Плотность песка"));
            PhyParams.Add(new PhyParamer(28.0, "phi_s", "Угол внутреннего трения"));
            PhyParams.Add(new PhyParamer(0.25, "kappa_ws", "Коэффициент Кармана для смеси"));
            PhyParams.Add(new PhyParamer(0.35, "eps_s", "Коэффициент пористости песка"));
            PhyParams.Add(new PhyParamer(0.001, "d_50", "Средний диаметр песка"));
            PhyParams.Add(new PhyParamer(0.001, "k_50", "Коэффициент шероховатости песка"));
            PhyParams.Add(new PhyParamer(0.00001, "tau_s", "Коэффициент связности песка"));
            PhyParams.Add(new PhyParamer(0.5, "cx_s", "Лобовое сопротивление частиц песка"));
            #endregion
            #region Физика 
            PhyParams.Add(new PhyParamer(9.81, "grav", "Ускорение свободного падения"));
            PhyParams.Add(new PhyParamer(335.0, "us_a", "Скорость сзвука в воздухе"));
            PhyParams.Add(new PhyParamer(1435.0, "us_w", "Скорость сзвука в воде"));
            
            #endregion
        }
    }
}
