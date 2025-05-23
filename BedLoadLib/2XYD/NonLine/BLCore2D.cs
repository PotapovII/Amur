﻿//---------------------------------------------------------------------------
//              Реализация библиотеки для моделирования 
//               гидродинамических и русловых процессов
//                      - (C) Copyright 2021 -
//                       ALL RIGHT RESERVED
//                        ПРОЕКТ "BedLoadLib"
//---------------------------------------------------------------------------
//                   разработка: Потапов И.И.
//                          31.05.22
//---------------------------------------------------------------------------
namespace BedLoadLib
{
    using System;
    using GeometryLib;
    //---------------------------------------------------------------------------
    // Аппроксимация Ядер в функции расхода влекомых наносов
    //---------------------------------------------------------------------------
    [Serializable]
    public class BLCore2D
    {
        FieldSquare DZX = null;
        FieldSquare DZY = null;
        public BLCore2D()
        {
            double[,] MasdGx = {
            // Gx  - 1      -0.75       -0.5       -0.25         0             0.25        0.5          0.75        1.0
            {50.0000000, 40.0000000, 30.0000000, 20.0000000, 10.00000000, 0.235611225, 0.341666021, 0.645911535, 15.22323781}, 
            {40.0000000, 20.0000000, 2.35697681, 1.17444214, 0.949050354, 0.945504745, 1.139260100, 1.911343348, 41.28270570}, 
            {30.0000000, 4.02605087, 1.51935761, 1.09767132, 0.985700070, 1.027945318, 1.267976368, 2.155301506, 46.90654700}, 
            {15.0000000, 2.50701707, 1.36837980, 1.07325389, 0.997133682, 1.058350634, 1.318918485, 2.256225936, 49.30647136}, 
            {5.00064840, 2.28512671, 1.33324445, 1.06665244, 0.999999999, 1.066652446, 1.333244454, 2.285126710, 50.00250008}, 
            {15.0000000, 2.50701707, 1.36837980, 1.07325389, 0.997133682, 1.058350634, 1.318918485, 2.256225936, 49.30647136}, 
            {30.0000000, 4.02605087, 1.51935761, 1.09767132, 0.985700070, 1.027945318, 1.267976368, 2.155301506, 46.90654700}, 
            {40.0000000, 20.0000000, 2.35697681, 1.17444214, 0.949050354, 0.945504745, 1.139260100, 1.911343348, 41.28270570}, 
            {50.0000000, 40.0000000, 30.0000000, 20.0000000, 10.00000000, 0.235611225, 0.341666021, 0.645911535, 15.22323781}};

            double[,] MasdGy_Gy = {
            // Gx  - 1      -0.75       -0.5       -0.25         0           0.25        0.5          0.75        1.0
            { 100.00000,  75.000000,  50.000000,  25.000000,  15.000000,  0.8099846,  0.4835741,  0.3513729,  0.2771153}, 
            { 75.000000,  50.000000,  3.6498905,  1.3722521,  0.8231438,  0.5786538,  0.4413468,  0.3538932,  0.2935740}, 
            { 50.000000,  3.8590114,  1.1539426,  0.6549779,  0.4483223,  0.3364196,  0.2667490,  0.2194508,  0.1853830}, 
            { 25.000000,  1.0479849,  0.4614846,  0.2875205,  0.2052480,  0.1577366,  0.1270044,  0.1056062,  0.0899140}, 
            { 0.0000000,  0.0000000,  0.0000000,  0.0000000,  0.0000000,  0.0000000,  0.0000000,  0.0000000,  0.0000000}, 
            {-25.000000, -1.0479849, -0.4614846, -0.2875205, -0.2052480, -0.1577366, -0.1270044, -0.1056062, -0.0899140}, 
            {-50.000000, -3.8590114, -1.1539426, -0.6549779, -0.4483223, -0.3364196, -0.2667490, -0.2194508, -0.1853830}, 
            {-75.000000, -50.000000, -3.6498905, -1.3722521, -0.8231438, -0.5786538, -0.4413468, -0.3538932, -0.2935740}, 
            {-100.00000, -75.000000, -50.000000, -25.000000, -15.000000, -0.8099846, -0.4835741, -0.3513729, -0.2771153}};
            double[,] MasdGy = {
            // Gx  - 1      -0.75       -0.5       -0.25         0          0.25        0.5          0.75        1.0
            {-100.00000, -75.000000, -50.000000, -25.000000, -15.000000, -0.8100656, -0.4836225, -0.3514080, -0.2771430}, 
            {-75.000000, -50.000000, -4.8670074, -1.8298525, -1.0976349, -0.7716156, -0.5885212, -0.4719048, -0.3914712}, 
            {-50.000000, -7.7187948, -2.3081160, -1.3100868, -0.8967343, -0.6729065, -0.5335514, -0.4389456, -0.3708032}, 
            {-25.000000, -4.1923591, -1.8461232, -1.1501972, -0.8210744, -0.6310097, -0.5080685, -0.4224673, -0.3596922}, 
            {-25.000000, -4.1923591, -1.8461232, -1.1501972, -0.8210744, -0.6310097, -0.5080685, -0.4224673, -0.3596922}, 
            {-25.000000, -4.1923591, -1.8461232, -1.1501972, -0.8210744, -0.6310097, -0.5080685, -0.4224673, -0.3596922}, 
            {-50.000000, -7.7187948, -2.3081160, -1.3100868, -0.8967343, -0.6729065, -0.5335514, -0.4389456, -0.3708032}, 
            {-75.000000, -50.000000, -4.8670074, -1.8298525, -1.0976349, -0.7716156, -0.5885212, -0.4719048, -0.3914712}, 
            {-100.00000, -75.000000, -50.000000, -25.000000, -15.000000, -0.8100656, -0.4836225, -0.3514080, -0.2771430} };

            DZX = new FieldSquare(1, 1, MasdGx);
            DZY = new FieldSquare(1, 1, MasdGy);
        }
        public double CoreX(double dZx, double dZy)
        {
            return DZX.Value(dZx, dZy);
        }
        public double CoreY(double dZx, double dZy)
        {
            return DZY.Value(dZx, dZy);
        }
    }
}
