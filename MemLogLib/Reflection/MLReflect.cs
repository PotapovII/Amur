namespace MemLogLib.Reflection
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.ComponentModel;
    using System.Collections.Generic;
    
    /// <summary>
    /// Работа с рефлексией
    /// </summary>
    public static class REFL
    {
        /// <summary>
        /// Получить значение атрибута Description для значения enum
        /// </summary>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = 
            (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
    /// <summary>
    /// Методы расширения позволяют "добавлять" методы в существующие типы без создания 
    /// нового производного типа, перекомпиляции и иного изменения первоначального типа. 
    /// Методы расширения представляют собой особую разновидность статического метода, 
    /// но вызываются так же, как методы экземпляра в расширенном типе. Для клиентского кода, 
    /// написанного на языках C#, F# и Visual Basic, нет видимого различия между вызовом 
    /// метода расширения и вызовом методов, фактически определенных в типе.
    /// ------------------------------------------------------------------------------------
    ///      Это позволяет сделать "текучий интерфейс" (fluent interface)
    /// </summary>
    public static class MLReflect
    {
        #region Селекторы для работа с константами классов
        /// <summary>
        /// Если вы хотите получить значения всех констант определенного типа из целевого типа
        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ///                                 this Type type
        /// Теперь в пространстве имён, в котором объявлен MemLogLib.Reflection и во всех модулях, 
        /// в которые будет добавлен using этого пространства имён, у объектов Type появится метод
        /// GetAllPublicConstantValues. При выполнении метода в качестве первого аргумента выступит 
        /// собственно сам объект Type, как это показано в следующем методе GetConstValues
        /// </summary>
        public static List<T> GetAllPublicConstantValues<T>(this Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }
        // Важно, что методы расширения это приём нарушающий классические ООП принципы.
        // Но в тоже время такая замечательная вещь как LINQ реализована на их основе.
        public static List<string> GetConstValues(string TypeName = "Foo")
        {
            Type type = Type.GetType(TypeName);
            List<string> constValues = type.GetAllPublicConstantValues<string>();
            return constValues;
        }
        /// <summary>
        /// Получить контейнер полей констант
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetConstants(this Type type)
        {
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly);
        }
        /// <summary>
        /// Получить контейнер полей констант заданного типа (НО только ссылочного типа)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetConstantsValues<T>(this Type type) where T : class
        {
            var fieldInfos = GetConstants(type);

            return fieldInfos.Select(fi => fi.GetRawConstantValue() as T);
        }
        public static object GetConstValues(string NameClass, string NameField)
        {
            Type type = GetType(NameClass);
            FieldInfo fld = type.GetField(NameField);
            return fld.GetValue(null);
        }
        public static object GetConstValues(Type type, string NameField)
        {
            FieldInfo fld = type.GetField(NameField);
            return fld.GetValue(null);
        }
        #endregion

        #region Получиь тип объекта
        public static Type GetType(string typeName = "MyNamespace.MyClass", 
                                   // Type.FullName
                                   string assemblyName = "MyAssemblyName") 
                            // MyAssembly.FullName or MyAssembly.GetName().Name
        {
            string assemblyQualifiedName = Assembly.CreateQualifiedName(assemblyName, typeName);
            Type myClassType = Type.GetType(assemblyQualifiedName);
            return myClassType;
        }
        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
        #endregion

        #region Создатели
        /// <summary>
        /// Создать объект по строковому имени с конструктором по умолчанию
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public static object Create(string TypeName)
        {
            try
            {
                Type type = Type.GetType(TypeName);
                return Activator.CreateInstance(type);
            }
            catch (Exception exp)
            {
                Logger.Instance.Info("Создать объект с типпом" + TypeName +" не удалось");
                Logger.Instance.Exception(exp);
                return null;
            }
        }
        /// <summary>
        /// Создать объект по строковому имени для конструктора с параметрами
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public static object Create(string TypeName, object[] Args)
        {
            try
            {
                Type type = Type.GetType(TypeName);
                return Activator.CreateInstance(type, Args);
            }
            catch (Exception exp)
            {
                Logger.Instance.Info("Создать объект с типпом" + TypeName + " не удалось");
                Logger.Instance.Exception(exp);
                return null;
            }
        }
        /// <summary>
        /// Создать объект по шаблонному имени для конструктора с параметрами
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        public static T Create<T>()
        {
            Type type = typeof(T);
            return (T)Activator.CreateInstance(type);
        }
        public static T Create<T>(string TypeName, object[] Args)
        {
            Type type = Type.GetType(TypeName);
            return (T)Activator.CreateInstance(type, Args);
        }
        /// <summary>
        /// Создать объект по шаблонному имени для конструктора с параметрами
        /// </summary>
        public static T Create<T>(object[] Args)
        {
            Type type = typeof(T);
            return (T)Activator.CreateInstance(type, Args);
        }
        #endregion 
    }
}
