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
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          08.04.21
//---------------------------------------------------------------------------
namespace CommonLib.BedLoad
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Модель движения донного матеиала
    /// </summary>
    [Serializable]
    public enum TypeBLModel
    {
        /// <summary>
        /// Модель влекомых наносов Петрова П.Г.
        /// </summary>
        [Description("Модель влекомых наносов Петрова П.Г.")]
        BLModel_1991 = 0,
        /// <summary>
        /// Модель влекомых наносов Петрова A.Г. Потапова И.И.
        /// </summary>
        [Description("Модель влекомых наносов Петрова A.Г. Потапова И.И.")]
        BLModel_2010,
        /// <summary>
        /// Модель влекомых наносов Петрова A.Г. Потапова И.И.
        /// </summary>
        [Description("Модель влекомых наносов Петрова A.Г. Потапова И.И.")]
        BLModel_2014,
        /// <summary>
        /// Модель влекомых наносов Потапова И.И.
        /// </summary>
        [Description("Модель влекомых наносов Потапова И.И.")]
        BLModel_2021
    }
    /// <summary>
    /// ОО: Шаблон 2D решателя по расчету донных деформаций
    /// </summary>
    public interface IBedLoadTask : ITask
    {
        /// <summary>
        /// Название файла параметров задачи
        /// </summary>
        string NameBLParams();
        /// <summary>
        /// Решатель для СЛАУ 
        /// </summary>
        IAlgebra Algebra();
        /// <summary>
        /// Лавинное обрушение
        /// </summary>
        IAvalanche GetAvalanche();
        /// <summary>
        /// Текщий уровень дна
        /// </summary>
        // double[] CZeta { get; }
        /// <summary>
        /// Вычисление текущих расходов и их градиентов для построения графиков
        /// </summary>
        /// <param name="Gs">возвращаемые расходы (по механизмам движения)</param>
        /// <param name="dGs">возвращаемые градиенты расходы (по механизмам движения)</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        void Calk_Gs(ref double[][] Gs, ref double[][] dGs, double[] tau, double[] P = null);
        /// <summary>
        /// Установка текущей геометрии расчетной области
        /// </summary>
        /// <param name="mesh">Сетка расчетной области</param>
        /// <param name="Zeta0">начальный уровень дна</param>
        /// <param name="Roughness">шероховатость дна</param>
        /// <param name="BConditions">граничные условия, 
        /// количество в обзем случае определяется через маркеры 
        /// границ сетеи</param>
        void SetTask(IMesh mesh, double[] Zeta0, double[] Roughness, IBoundaryConditions BConditions);
        /// <summary>
        /// Вычисление изменений формы донной поверхности 
        /// на одном шаге по времени по модели 
        /// Петрова А.Г. и Потапова И.И. (2010), 2014
        /// Реализация решателя - методом контрольных объемов,
        /// Патанкар (неявный дискретный аналог ст 40 ф.3.40 - 3.41)
        /// Коэффициенты донной подвижности, определяются 
        /// как среднее гармонические величины         
        /// </summary>
        /// <param name="Zeta">>возвращаемая форма дна на n+1 итерации</param>
        /// <param name="tau">придонное касательное напряжение</param>
        /// <param name="P">придонное давление</param>
        void CalkZetaFDM(ref double[] Zeta, double[] tauX, double[] tauY, 
                                double[] P = null, double[][] CS = null);
        /// <summary>
        /// Создает экземпляр класса
        /// </summary>
        /// <returns></returns>
        IBedLoadTask Clone();
    }
}
