namespace TestEliz2D
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class ManagerOfChildsGenerators
    {

        /// <summary>
        /// список потомков базового класса
        /// </summary>
        private Type[] ListOfChilds;
        private Type[] ListOfParameters;
        Type baseType;
        /// <summary>
        /// конструктор и метод создания списка типов базового класса
        /// </summary>
        /// <param name="ChildType">Тип класса-потомка (использовать операцию typeof)</param>
        /// <param name="LibName">Название библиотеки(this.GetType().FullName взять первое до точки)</param>
        public string[] GetChildsNames()
        {
            //
            string[] Names = new string[ListOfChilds.Length];
            for (int i = 0; i < ListOfChilds.Length; i++)
            {
                PropertyInfo s = ListOfChilds[i].GetProperty("Name");
                object obj = ListOfChilds[i].GetConstructor(new Type[] { }).Invoke(new object[] { });
                Names[i] = Convert.ToString(s.GetValue(obj, null));
                //Names[i] = ListOfChilds[i].Name;
            }
            return Names;
        }
        //
        public string[] GetParametersNames()
        {
            string[] Names = new string[ListOfParameters.Length];
            //
            for (int i = 0; i < ListOfParameters.Length; i++)
                Names[i] = ListOfParameters[i].Name;

            return Names;
        }
        public ManagerOfChildsGenerators()
        {
            string NameSpace = Assembly.GetExecutingAssembly().GetName().Name;

            baseType = typeof(PointsGeneratorEliz);
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(NameSpace);

            ListOfChilds = Array.FindAll(assembly.GetTypes(),
                    delegate (Type type)
                    {
                        return (baseType != type) && baseType.IsAssignableFrom(type);
                    }
                );
            //
            ListOfChilds = ListOfChilds.OrderBy(s => s.Name).ToArray();
            //
            baseType = typeof(Parameter);
            //
            assembly = System.Reflection.Assembly.Load(NameSpace);

            ListOfParameters = Array.FindAll(assembly.GetTypes(),
                    delegate (Type type)
                    {
                        return (baseType != type) && baseType.IsAssignableFrom(type);
                    }
                );
            //
            ListOfParameters = ListOfParameters.OrderBy(s => s.Name).ToArray();
        }
        /// <summary>
        /// получить экземпляр указанного класса
        /// </summary>
        /// <param name="Index">порядковый номер в списке потомков базового класса</param>
        /// <returns></returns>
        public PointsGeneratorEliz CreateDetermChild(int Index)
        {
            try
            {
                return (PointsGeneratorEliz)ListOfChilds[Index].GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            catch
            {
                return null;
            }
        }
        //
        public PointsGeneratorEliz CreateDetermChild(int Index, Parameter Param)
        {
            try
            {
                return (PointsGeneratorEliz)ListOfChilds[Index].GetConstructor(new Type[] { typeof(Parameter) }).Invoke(new object[] { Param });
            }
            catch
            {
                return null;
            }
        }
        //
        /// <summary>
        /// получить экземпляр указанного класса
        /// </summary>
        /// <param name="Index">порядковый номер в списке потомков базового класса</param>
        /// <returns></returns>
        public Parameter CreateDetermParam(int Index)
        {
            try
            {
                return (Parameter)ListOfParameters[Index].GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            catch
            {
                return null;
            }
        }
        //
        /// <summary>
        /// Метод возвращает Свойства полей наследника класса Parameter
        /// </summary>
        /// <param name="childParameter"> Объект Parameter</param>
        /// <param name="Names">Возвращаемый массив имен параметров</param>
        /// <param name="Types">Возвращаемый массив типов параметров</param>
        public void Par(Parameter childParameter, out string[] Names, out Type[] Types)
        {
            PropertyInfo[] baseInfo = childParameter.GetType().BaseType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Public);
            PropertyInfo[] childInfo = childParameter.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Public);
            Names = new string[baseInfo.Length + childInfo.Length];
            Types = new Type[Names.Length];
            int k = 0;
            for (int i = 0; i < baseInfo.Length; i++)
            {
                Names[k] = baseInfo[i].Name;
                Types[k++] = baseInfo[i].PropertyType;
            }
            //
            for (int i = 0; i < childInfo.Length; i++)
            {
                Names[k] = childInfo[i].Name;
                Types[k++] = childInfo[i].PropertyType;
            }
        }
    }
}

