﻿#version 330

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 aColor;

// передача значения во фрагментный шейдер
out vec4 inColorFraf;

// Вершина состоит из одного или нескольких атрибутов 
// (позиции, нормали, координаты текстуры и т. д.).
// На стороне ЦП, когда вы создаете свой VAO, 
// вы описываете каждый атрибут, говоря: «Эти данные 
// в этом буфере будут атрибутом 0, данные рядом 
// с ним будут атрибутом 1 и т. д.». 
// Обратите внимание, что VAO хранит только эту 
// информацию о том, кто где расположен. 
// Фактические данные вершин хранятся в VBO.
// В вершинном шейдере строка с макетом и позицией 
// просто говорит: «получите атрибут 0 и поместите
// его в переменную с именем position» 
// (location - представляет номер атрибута в VAO).
// Если эти два шага выполнены правильно, вы должны
// получить позицию в переменной с именем position :)
void main()
{
    inColorFraf = aColor;
    gl_Position = vec4(aPosition, 1.0);
}