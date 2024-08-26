//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 21.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
namespace RenderLib
{
    using CommonLib;
    using CommonLib.DrvGraphics;
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// подписи PInvoke для методов GradientFill.
    /// </summary>
    /// <remarks>
    /// Минимальные требования: Windows 2000 Professional
    /// </remarks>    
    public static class FillMethods
    {
        /// <резюме>
        /// Функция GradientFill заполняет прямоугольные и треугольные структуры
        /// </summary>
        /// <param name = "hdc"> Дескриптор контекста целевого устройства </param>
        /// <param name = "pVertex"> Массив структур TriVertex, каждая из которых определяет вершину треугольника </param>
        /// <param name = "nVertex"> Количество вершин в pVertex </param>
        /// <param name = "pMesh"> Массив элементов </param>
        /// <param name = "nMesh"> Количество элементов в pMesh </param>
        /// <param name = "ulMode"> Определяет режим градиентной заливки </param>
        /// <returns> Если функция завершается успешно, возвращается значение true, false </returns>
        public static bool GradientFill([In] IntPtr hdc, TriVertex[] pVertex, uint nVertex, uint[] pMesh, uint nMesh,
                                        GradientFillMode ulMode)
        {
            return Native.GradientFill(hdc, pVertex, nVertex, pMesh, nMesh, ulMode);
        }

        /// <резюме>
        /// Функция GradientFill заполняет прямоугольные и треугольные структуры
        /// </summary>
        /// <param name = "hdc"> Дескриптор контекста целевого устройства </param>
        /// <param name = "pVertex"> Массив структур TriVertex, каждая из которых определяет вершину треугольника </param>
        /// <param name = "nVertex"> Количество вершин в pVertex </param>
        /// <param name = "pMesh"> Массив структур TriElement в режиме треугольника </param>
        /// <param name = "nMesh"> Количество элементов в pMesh </param>
        /// <param name = "ulMode"> Определяет режим градиентной заливки </param>
        /// <returns> Если функция завершается успешно, возвращается значение true, false </returns>
        public static bool GradientFill([In] IntPtr hdc, TriVertex[] pVertex, uint nVertex, TriElement[] pMesh,
                                        uint nMesh, GradientFillMode ulMode)
        {
            return Native.GradientFill(hdc, pVertex, nVertex, pMesh, nMesh, ulMode);
        }

        /// <резюме>
        /// Функция GradientFill заполняет прямоугольные и треугольные структуры
        /// </summary>
        /// <param name = "hdc"> Дескриптор контекста целевого устройства </param>
        /// <param name = "pVertex"> Массив структур TriVertex, каждая из которых определяет вершину треугольника </param>
        /// <param name = "nVertex"> Количество вершин в pVertex </param>
        /// <param name = "pMesh"> массив структур TwoElement в режиме прямоугольника </param>
        /// <param name = "nMesh"> Количество элементов в pMesh </param>
        /// <param name = "ulMode"> Определяет режим градиентной заливки </param>
        /// <returns> Если функция завершается успешно, возвращается значение true, false </returns>
        public static bool GradientFill([In] IntPtr hdc, TriVertex[] pVertex, uint nVertex, TwoElement[] pMesh,
                                        uint nMesh, GradientFillMode ulMode)
        {
            return Native.GradientFill(hdc, pVertex, nVertex, pMesh, nMesh, ulMode);
        }
        /// <summary>
        /// ОО: Маршалинг нативных методов из библиотеки msimg32.dll для заливки сетки
        /// </summary>
        internal class Native
        {
            [DllImport("msimg32.dll", EntryPoint = "GradientFill", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GradientFill([In] IntPtr hdc,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)] TriVertex[] pVertex,
                uint nVertex, uint[] pMesh, uint nMesh, GradientFillMode ulMode);

            [DllImport("msimg32.dll", EntryPoint = "GradientFill", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GradientFill([In] IntPtr hdc,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)] TriVertex[] pVertex,
                uint nVertex, TwoElement[] pMesh, uint nMesh, GradientFillMode ulMode);

            [DllImport("msimg32.dll", EntryPoint = "GradientFill", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GradientFill([In] IntPtr hdc,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)] TriVertex[] pVertex,
                uint nVertex, TriElement[] pMesh, uint nMesh, GradientFillMode ulMode);
        }
    }
}