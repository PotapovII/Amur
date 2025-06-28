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
//                      Проект "Home" dNdy++
//                  - (C) Copyright 2003
//                        Потапов И.И.
//                         14.11.03
//---------------------------------------------------------------------------
//         сильно, упрощенный перенос с dNdy++ на dNdy#
//                 нет адресных таблиц ...
//                      Потапов И.И.
//                      07.04.2021
//---------------------------------------------------------------------------
namespace FEMTasksLib
{
    using CommonLib;
    using FEMTasksLib.FEMTasks.Utils;
    using MemLogLib;
    using MeshLib;
    using System;

    /// <summary>
    ///  ОО: Решатель для задачи изгиба пластинки 
    ///  уравнение Софи - Жормен на Эрмитовых элементах
    ///  d^4 w         d^4 w      d^4 w
    ///  ----- + 2 ----------- + ------- + Q = 0  in Omega
    ///  d x^4     d x^2 d y^2    d y^4
    ///   
    ///            d w        d w
    ///   w = 0,   --- = 0,   ---- = 0   in d Omega  
    ///            d x        d y
    /// </summary>
    [Serializable]
    public class SophieGermainTask : AFETask
    {
        /// <summary>
        /// количество степеней свободы в узле
        /// </summary>
        private int cs = 3;
        /// <summary>
        /// распределенная нагрузка деленная на модуль жесткости
        /// </summary>
        private double Q;

        public SophieGermainTask(int Nx,int Ny, double Q)
        {
            this.Q = Q;
        }
        public override void SetTask(IMesh mesh, IAlgebra algebra)
        {
            this.mesh = mesh;
            this.algebra = algebra;
            IPointsA.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Triangle_Ermit3);
            IPointsB.SetInt((int)mesh.typeRangeMesh, TypeFunForm.Form_2D_Rectangle_Ermit4);
        }
        public override void SolveTask(ref double[] result)
        {
            MEM.Alloc<double>((int)algebra.N, ref result);
            algebra.Clear();
            for (uint elem = 0; elem < mesh.CountElements; elem++)
            {
                // получить узлы КЭ
                TypeFunForm typeff = TypeFunForm.Form_2D_Rectangle_Ermit4; 
                if (FunFormHelp.CheckFF(typeff) == 0)
                    pIntegration = IPointsA;
                else
                    pIntegration = IPointsB;
                AbFunForm ff = FunFormsManager.CreateKernel(typeff);
                int cu = knots.Length;
                InitLocal(cu, cs);
                //Координаты и площадь
                mesh.GetElemCoords(elem, ref x, ref y);
                // установка координат узлов
                ff.SetGeoCoords(x, y);
                int Length = cs * cu;
                // цикл по точкам интегрирования
                for (int pi = 0; pi < pIntegration.weight.Length; pi++)
                {
                    ff.CalkDiffForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    double DWJ = ff.DetJ * pIntegration.weight[pi];
                    int li, lj;
                    // Вычисление ЛЖМ для задачи Софи - Жормен
                    // используется сплошная нумерация функций формы для прогиба и поворотов
                    for (int ai = 0; ai < Length; ai++)
                        for (int aj = 0; aj < cs * cu; aj++)
                            LaplMatrix[ai][aj]     += (ff.DN2x[ai] + ff.DN2y[ai]) * (ff.DN2x[aj] + ff.DN2y[aj]) * DWJ;

                    ff.CalkForm(pIntegration.xi[pi], pIntegration.eta[pi]);
                    // Вычисление ЛПЧ от объемных сил
                    for (int ai = 0; ai < Length; ai++)
                        LocalRight[ai + 1] += Q * ff.N[ai] * DWJ;     // вертикальная объемня нагрузка
                }
                // получем массив адресов для BC
                FEMUtils.GetAdress(knots, ref adressBound, cs);
                // добавление вновь сформированной ЛЖМ в ГМЖ
                algebra.AddToMatrix(LaplMatrix, adressBound);
                // добавление вновь сформированной ЛПЧ в ГПЧ
                algebra.AddToRight(LocalRight, adressBound);
            }
            // получить граничные узлы
            uint[] bound = mesh.GetBoundKnotsByMarker(0);
            // получить адреса
            FEMUtils.GetAdress(bound, ref adressBound, cs);
            // установить ГУ
            algebra.BoundConditions(0.0, adressBound);
            //algebra.Print();
            // решение
            algebra.Solve(ref result);
            //algebra.Print();
        }
    }
}



