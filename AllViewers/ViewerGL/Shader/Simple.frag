#version 330

// получение значения из фрагментного шейдера
in vec4 inColorFraf;
out vec4 outColor;

void main()
{
    outColor = inColorFraf;
}