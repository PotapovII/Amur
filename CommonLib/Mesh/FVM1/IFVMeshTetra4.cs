#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                          ПРОЕКТ  "МКЭ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.06.2022 Потапов И.И.
//---------------------------------------------------------------------------
namespace CommonLib
{
    /// <summary>
    /// ОО: IRectFVMesh - базистная контрольно - объемная сетка в четырех гранной области 
    /// </summary>
    public interface IRectFVMesh: IMesh 
    {
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению X 
        /// (количество узлов по i)
        /// </summary>
        int Nx { get; set; }
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению Y
        /// (количество узлов по j)
        /// </summary>
        int Ny { get; set; }
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению X 
        /// (количество узлов по i)
        /// </summary>
        int imax { get; set; }
        /// <summary>
        /// Количество контрольных объемов + 1 по направлению Y
        /// (количество узлов по j)
        /// </summary>
        int jmax { get; set; }
        /// <summary>
        /// Координаты х для цетров контрольных объемов
        /// </summary>
        double[][] x { get; set; }
        /// <summary>
        /// Координаты y для цетров контрольных объемов
        /// </summary>
        double[][] y { get; set; }
        /// <summary>
        /// Координаты узловых точек для скорости u
        /// </summary>
        double[][] xu { get; set; }
        /// <summary>
        /// Координаты узловых точек для скорости v
        /// </summary>
        double[][] yv { get; set; }
        /// <summary>
        /// Расстояние между узловыми точками для скорости u
        /// или ширина контрольного объема
        /// </summary>
        double[][] Dx { get; set; }
        /// <summary>
        /// Расстояние между узловыми точками для скорости v
        /// или высота контрольного объема
        /// </summary>
        double[][] Dy { get; set; }
        /// <summary>
        /// Расстояние между центрами контрольных объемов по х
        /// </summary>
        double[][] hx { get; set; }
        /// <summary>
        /// Расстояние между центрами контрольных объемов по у
        /// </summary>
        double[][] hy { get; set; }
    }
}