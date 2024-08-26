/////////////////////////////////////////////////////
// дата: 16 03 2021
// автор: Потапов Игорь Иванович 
// библиотека: SPH_LibraryKernels
// лицензия: свободное распространение
/////////////////////////////////////////////////////


namespace MeshLib
{
    using CommonLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Класс для управления коллекцией функций формы порожденных 
    /// от базового класса AbFunForm
    /// </summary>
    public static class FunFormsManager
    {
        public static string ErrorMessage = "Ok";
        /// <summary>
        /// список потомков базового класса
        /// </summary>
        static Type[] ListModels = null;
        /// <summary>
        /// Строка с имененм пространства имен в котором находится требуемая иерархия классов
        /// </summary>
        static string NameSpace = "FEMToolsLib";
        static Dictionary<string, uint> NameToIndex = new Dictionary<string, uint>();
        static Dictionary<TypeFunForm, uint> IDToIndex = new Dictionary<TypeFunForm, uint>();
        /// <summary>
        /// Получить список потомков
        /// </summary>
        /// <returns>список потомков</returns>
        public static List<string> GetNamesKernels(FFGeomType ffGeomType = FFGeomType.All)
        {
            if (ListModels == null)
            {
                try
                {
                    // получение имени пространства имен
                    NameSpace = Assembly.GetExecutingAssembly().GetName().Name;
                    Assembly assembly = System.Reflection.Assembly.Load(NameSpace);
                    Type baseType = typeof(AbFunForm);
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
                    MemLogLib.Logger.Instance.Exception(e);
                }
            }
            List<string> Names = new List<string>();
            NameToIndex.Clear();
            IDToIndex.Clear();
            for (uint i = 0; i < ListModels.Length; i++)
            {
                AbFunForm ff = CreateKernel(i);
                NameToIndex.Add(ff.Name, i);
                IDToIndex.Add(ff.ID, i);
                if (ff.GeomType == ffGeomType || ffGeomType == FFGeomType.All)
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
        static AbFunForm CreateKernel(uint Index)
        {
            Type TestType = Type.GetType(ListModels[Index % ListModels.Length].FullName, false, true);
            // ищем конструктор с параметрами
            ConstructorInfo constructor = TestType.GetConstructor(new Type[] { });
            // вызываем конструтор
            return (AbFunForm)constructor.Invoke(new object[] { });
        }
        /// <summary>
        /// получить экземпляр указанного класса по Имени класса
        /// </summary>
        /// <param name="Index">порядковый номер в списке потомков базового класса</param>
        /// <returns></returns>
        public static AbFunForm CreateKernel(string FFName)
        {
            if (NameToIndex.Count == 0)
                GetNamesKernels(FFGeomType.All);
            uint Index = NameToIndex[FFName];
            Type TestType = Type.GetType(ListModels[Index % ListModels.Length].FullName, false, true);
            // ищем конструктор с параметрами
            ConstructorInfo constructor = TestType.GetConstructor(new Type[] { });
            // вызываем конструтор
            return (AbFunForm)constructor.Invoke(new object[] { });
        }
        /// <summary>
        /// получить экземпляр указанного класса по ID класса
        /// </summary>
        /// <param name="Index">порядковый номер в списке потомков базового класса</param>
        /// <returns></returns>
        public static AbFunForm CreateKernel(TypeFunForm ID)
        {
            if (NameToIndex.Count == 0)
                GetNamesKernels(FFGeomType.All);
            uint Index = IDToIndex[ID];
            Type TestType = Type.GetType(ListModels[Index % ListModels.Length].FullName, false, true);
            // ищем конструктор с параметрами
            ConstructorInfo constructor = TestType.GetConstructor(new Type[] { });
            // вызываем конструтор
            return (AbFunForm)constructor.Invoke(new object[] { });
        }
    }
}
