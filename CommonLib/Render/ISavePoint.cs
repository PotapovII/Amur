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
//                 кодировка : 14.12.2020 Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 25.05.2024 Потапов И.И.  ... + Name
//---------------------------------------------------------------------------
namespace CommonLib
{
    using CommonLib.Function;
    using System;
    using System.ComponentModel;
    /// <summary>
    /// ОО: Отсылка данных для отрисовки
    /// </summary>
    /// <param name="a"></param>
    [Serializable]
    public delegate void SendParam(ISavePoint a);
    /// <summary>
    /// ОО: Отсылка строки для отображения на формах
    /// </summary>
    /// <param name="text"></param>
    [Serializable]
    public delegate void SendText(string text);
    /// <summary>
    /// Произвольная функция
    /// </summary>
    /// <typeparam name="Tp"></typeparam>
    /// <param name="value"></param>
    [Serializable]
    public delegate void SendValue<Tp>(Tp value);
    ///// <summary>
    ///// Адаптер точки сохранения
    ///// </summary>
    //public static class GetSavePoint
    //{
    //    /// <summary>
    //    /// Создание точки сохранения данных
    //    /// </summary>
    //    /// <returns></returns>
    //    public static ISavePoint GetSavePoint()
    //    {
    //        return new SavePoint();
    //    }
    //}
    /// <summary>
    /// Адаптер точки сохранения
    /// </summary>
    public interface ISavePoint 
    {
        /// <summary>
        /// Наименование точки сохранения
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// сетка задачи в точке сохранения
        /// </summary>
        [Browsable(false)]
        IRenderMesh mesh { get; }
        /// <summary>
        /// Контейнер кривых
        /// </summary>
        [Browsable(false)]
        IGraphicsData graphicsData { get; }
        /// <summary>
        /// Получить новый экземпляр кривой
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        IGraphicsCurve CloneCurves(string Name, 
            TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve,
            bool Check = true);
        /// <summary>
        /// Получить новый экземпляр кривой
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        IGraphicsCurve CloneCurves(string Name, double[] x, double[] y, 
            TypeGraphicsCurve tGraphicsCurve = TypeGraphicsCurve.AreaCurve, 
            bool Check = true);
        /// <summary>
        /// расчетное время в точке сохранения
        /// </summary>
        double time { get; }
        /// <summary>
        /// Установка времени и сетки кадра, и (или) графиков
        /// </summary>
        /// <param name="time"></param>
        /// <param name="mesh"></param>
        /// <param name="gd"></param>
        void SetSavePoint(double time, IMesh mesh, IGraphicsData gd);
        /// <summary>
        /// Добавление поля привязанного к узлам сетки
        /// </summary>
        /// <param name="Name">Название поля</param>
        /// <param name="Value">Значение поля в узлах сетки</param>
        void Add(string Name, double[] Value);
        /// <summary>
        /// Добавление векторного поля привязанного к узлам сетки
        /// </summary>
        /// <param name="Name">Название поля</param>
        /// <param name="Vx">Аргументы функции поля</param>
        /// <param name="Vy">Значение функции поля</param>
        void Add(string Name, double[] Vx, double[] Vy);
        /// <summary>
        /// Добавление поля не привязанного к узлам сетки
        /// </summary>
        /// <param name="Name">Название поля</param>
        /// <param name="arg">Аргументы функции поля</param>
        /// <param name="Value">Значение функции поля</param>
        void AddCurve(string Name, double[] X, double[] Y, 
            TypeGraphicsCurve TGraphicsCurve = TypeGraphicsCurve.AreaCurve);
        /// <summary>
        /// Удаление кривой по имени
        /// </summary>
        /// <param name="Name">Название поля</param>
        void RemoveCurve(string Name);
        /// <summary>
        /// Добавить кривую в контейнер кривых IDigFunction
        /// </summary>
        /// <param name="curve"></param>
        void AddCurve(IDigFunction curve);

        /// <summary>
        /// Добавить кривую в контейнер кривых IGraphicsData 
        /// </summary>
        /// <param name="curve"></param>
        void AddCurve(IGraphicsCurve curve);
        /// <summary>
        /// Добавить кривую в контейнер кривых IGraphicsData 
        /// </summary>
        /// <param name="curve"></param>
        void ClearСurve(TypeGraphicsCurve TGraphicsCurve = TypeGraphicsCurve.TimeCurve);
        /// <summary>
        /// Добавить контейнер кривых 
        /// </summary>
        /// <param name="Name">Название поля</param>
        /// <param name="arg">Аргументы функции поля</param>
        /// <param name="Value">Значение функции поля</param>
        void AddGraphicsData(IGraphicsData gd);
        /// <summary>
        /// Сериализация sp
        /// </summary>
        /// <param name="sp">точка сохранения</param>
        /// <param name="NameSave">путь,имя файла</param>
        void SerializableSavePoint(ISavePoint sp, string NameSave);
        /// <summary>
        /// Загрузка точки сохранения
        /// </summary>
        /// <param name="FileName">путь,имя файла</param>
        /// <returns></returns>
        ISavePoint LoadSavePoint(string FileName);
        /// <summary>
        /// Сохраняем облако глубин / КЭ сетку в файл 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="FileName"></param>
        /// <param name="shift"></param>
        void ImportSPMesh(string FileName, int shift = 1);
        /// <summary>
        /// Загрузка сетки из файла 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="FileName"></param>
        /// <param name="shift"></param>
        void ExportSPMesh(string FileName, int shift = 1);
    }
}