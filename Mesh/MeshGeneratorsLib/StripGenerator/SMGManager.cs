//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.03.2025 Потапов И.И.
//---------------------------------------------------------------------------
namespace MeshGeneratorsLib.StripGenerator
{
    
    using System;
    using System.Reflection;
    using System.ComponentModel;
    using System.Collections.Generic;
    /// <summary>
    /// Тип ленточного герератора КЭ сетки для створовых задач
    /// TO DO все генераторы нестабильны в различных геометриях :(
    /// </summary>
    [Serializable]
    public enum StripGenMeshType
    {
        /// <summary>
        /// Ленточный генератор КЭ сетки
        /// </summary>
        [Description("Ленточный триангулятор, ф. масивы ")]
        StripMeshGenerator_0 = 0,
        [Description("Ленточный триангулятор, списки")]
        StripMeshGenerator_1 = 1,
        [Description("Ленточный триангулятор, карта")]
        StripMeshGenerator_2 = 2,
        [Description("Ленточный три/тетрангулятор, карта")]
        StripMeshGenerator_3 = 3,
        [Description("Ленточный три/тетрангулятор, карта V.1")]
        StripMeshGenerator_4 = 4
    }
    /// <summary>
    /// Простой менеджер ленточных герераторов КЭ сетки 
    /// </summary>
    public static class SMGManager
    {
        public static IStripMeshGenerator GetMeshGenerator(StripGenMeshType tp,
                CrossStripMeshOption Option)
        {
            switch (tp)
            {
                case StripGenMeshType.StripMeshGenerator_0:
                    return new HStripMeshGeneratorTri(Option);
                case StripGenMeshType.StripMeshGenerator_1:
                    return new HStripMeshGenerator(Option);
                case StripGenMeshType.StripMeshGenerator_2:
                    return new CrossStripMeshGeneratorTri(Option);
                case StripGenMeshType.StripMeshGenerator_3:
                    return new CrossStripMeshGenerator(Option);
                case StripGenMeshType.StripMeshGenerator_4:
                    return new StripMeshGenerator(Option);
                default:
                    return new CrossStripMeshGenerator(Option);
            }
        }
        /// <summary>
        /// Получение описания полей перечисления (без контроля целостности)
        /// </summary>
        /// <returns></returns>
        public static List<string> GetNames()
        {
            List<string> names = new List<string>();
            try
            {
                for (StripGenMeshType f = StripGenMeshType.StripMeshGenerator_0;
                    f <= StripGenMeshType.StripMeshGenerator_4; f++)
                {
                    FieldInfo fi = f.GetType().GetField(f.ToString());
                    DescriptionAttribute[] attributes =
                    (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute), false);
                    names.Add(attributes[0].Description);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            return names;
        }
    }
}
