namespace FEMTasksLib.FEMTasks.Utils
{
    using MemLogLib;
    /// <summary>
    /// Утилиты метода конечных элементов
    /// </summary>
    public static class FEMUtils
    {
        /// <summary>
        /// Получить адреса неизвестных по узлам в векторных задачах
        /// </summary>
        /// <param name="knots">номера узлов конечного элемента</param>
        /// <param name="addressesOfUnknown">адреса неизвестных в матрице</param>
        /// <param name="cs">количество степеней свободы в узле</param>
        /// <param name="shift">сдвиг нумерации неизвестных</param>
        public static void GetAdress(uint[] knots, ref uint[] addressesOfUnknown, int cs = 2, uint shift = 0)
        {
            MEM.Alloc<uint>(knots.Length * cs, ref addressesOfUnknown, "adressBound");
            if (cs == 1)
                addressesOfUnknown = knots;
            for (int ai = 0; ai < knots.Length; ai++)
            {
                int li = cs * ai;
                for (uint i = 0; i < cs; i++)
                    addressesOfUnknown[li + i] = knots[ai] * (uint)cs + i + shift;
            }
        }
    }

}
