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
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 14.02.2024 Потапов И.И.
//---------------------------------------------------------------------------
//                      Сбока No Property River Lib
//              с убранными из наследников Property классами 
//---------------------------------------------------------------------------
namespace CommonLib.Points
{
    public interface IRiverNode : IHPoint
    {
        /// <summary>
        /// глубина
        /// </summary>
        double h { get; set; }
        /// <summary>
        /// расход по х
        /// </summary>
        double qx { get; set; }
        /// <summary>
        /// расход по у
        /// </summary>
        double qy { get; set; }
        /// <summary>
        /// поля на (time - 1) (предыдущей) итерации
        /// </summary>
        double h0 { get; set; }
        /// <summary>
        /// расход по х
        /// </summary>
        double qx0 { get; set; }
        /// <summary>
        /// расход по у
        /// </summary>
        double qy0 { get; set; }
        /// <summary>
        /// поля на (time - 2) итерации
        /// </summary>
        double h00 { get; set; }
        /// <summary>
        /// расход по х
        /// </summary>
        double qx00 { get; set; }
        /// <summary>
        /// расход по у
        /// </summary>
        double qy00 { get; set; }
        /// <summary>
        /// отметка дна
        /// </summary>
        double zeta { get; set; }
        /// <summary>
        /// шероховатость дна
        /// </summary>
        double ks { get; set; }
        /// <summary>
        /// ice thickness/ толщина льда  
        /// </summary>
        double Hice { get; set; }
        /// <summary>
        /// ice roughness/шероховатость льда 
        /// </summary>
        double KsIce { get; set; }
        /// <summary>
        /// ice[0] - ice thickness/ толщина льда  
        /// </summary>
        double h_ise { get; set; }
        /// <summary>
        /// /// ice[1] - ice roughness/шероховатость льда 
        /// </summary>
        double ks_ise { get; set; }
        /// <summary>
        /// глубина без льда
        /// </summary>
        double hd { get; set; }
        /// <summary>
        /// скорость по х
        /// </summary>
        double udx { get; set; }
        /// <summary>
        /// скорость по у
        /// </summary>
        double udy { get; set; }

        string ToString(string format = "F6");
        string ToStringCDG(string format = "F6");
        string ToStringBED(string format = "F6");
    }
}
