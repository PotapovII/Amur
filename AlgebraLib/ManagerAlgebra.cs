/////////////////////////////////////////////////////
// дата: 10 11 2021
// автор: Потапов Игорь Иванович 
// библиотека: AlgebraLib
// лицензия: свободное распространение
/////////////////////////////////////////////////////
namespace AlgebraLib
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    /// <summary>
    /// ОО: Класс для управления коллекцией классов от интерфейсая IAlgebra
    /// </summary>
    public static class ManagerAlgebra
    {
        public static string ErrorMessage = "Ok";
        /// <summary>
        /// список потомков базового класса
        /// </summary>
        static Type[] ListModels = null;
        /// <summary>
        /// Строка с имененм пространства имен в котором находится требуемая иерархия классов
        /// </summary>
        static string NameSpace = "AlgebraLib";
        static Dictionary<string, uint> NameToIndex = new Dictionary<string, uint>();
        /// <summary>
        /// Получить список потомков
        /// </summary>
        /// <returns>список потомков</returns>
        public static List<string> GetNamesKernels(AlgebraType ffGeomType = AlgebraType.All)
        {
            if (ListModels == null)
            {
                try
                {
                    // получение имени пространства имен
                    NameSpace = Assembly.GetExecutingAssembly().GetName().Name;
                    Assembly assembly = System.Reflection.Assembly.Load(NameSpace);
                    Type baseType = typeof(Algebra);
                    ListModels = Array.FindAll
                        (
                            assembly.GetTypes(),
                            delegate (Type type)
                            {
                                return (baseType != type) && baseType.IsAssignableFrom(type);
                            }
                        );
                    ListModels = ListModels.OrderBy(s => s.Name).ToArray();
                }
                catch (Exception e)
                {
                    ErrorMessage = e.Message;
                }
            }
            List<string> Names = new List<string>();
            NameToIndex.Clear();
            for (uint i = 0; i < ListModels.Length; i++)
            {
                Algebra ff = CreateKernel(i);
                NameToIndex.Add(ff.Name, i);
                if (ff.algebraType == ffGeomType || ffGeomType == AlgebraType.All)
                {
                    Names.Add(ff.Name);
                }
            }
            return Names;
        }
        /// <summary>
        /// получить экземпляр указанного класса по Index полного списка
        /// </summary>
        /// <param name="Index">порядковый номер в списке потомков базового класса</param>
        /// <returns></returns>
        private static Algebra CreateKernel(uint Index)
        {
            Type TestType = Type.GetType(ListModels[Index % ListModels.Length].FullName, false, true);
            // ищем конструктор с параметрами
            ConstructorInfo constructor = TestType.GetConstructor(new Type[] { });
            // вызываем конструтор
            return (Algebra)constructor.Invoke(new object[] { });
        }
        /// <summary>
        /// получить экземпляр указанного класса по Имени класса
        /// </summary>
        /// <param name="Index">порядковый номер в списке потомков базового класса</param>
        /// <returns></returns>
        public static Algebra CreateAlgebra(string FFName)
        {
            if (NameToIndex.Count == 0)
                GetNamesKernels(AlgebraType.All);
            uint Index = NameToIndex[FFName];
            Type TestType = Type.GetType(ListModels[Index % ListModels.Length].FullName, false, true);
            // ищем конструктор с параметрами
            ConstructorInfo constructor = TestType.GetConstructor(new Type[] { });
            // вызываем конструтор
            return (Algebra)constructor.Invoke(new object[] { });
        }
    }
}
