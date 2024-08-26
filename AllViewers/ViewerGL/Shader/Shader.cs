namespace ViewerGL
{
    using System;
    using System.IO;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;
    /// <summary>
    /// Класс поддержки вершинного и фрагментного шейдера
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// id вершинного шейдера
        /// </summary>
        readonly int vertexShader_ID = 0;
        /// <summary>
        /// id фрагментного шейдера
        /// </summary>
        readonly int fragmentShader_ID = 0;
        /// <summary>
        /// id шейдера
        /// </summary>
        readonly int shderProgram_ID = 0;
        /// <summary>
        /// Получение строк кода
        /// </summary>
        /// <param name="vertexfile"></param>
        /// <param name="fragmentfile"></param>
        /// <exception cref="Exception"></exception>
        public Shader(string vertexfile, string fragmentfile)
        {
            // создание ID для вершинного шейдера
            vertexShader_ID = CreateShader(ShaderType.VertexShader, vertexfile);
            // создание ID для фрагментного шейдера
            fragmentShader_ID = CreateShader(ShaderType.FragmentShader, fragmentfile);
            // создание программы
            shderProgram_ID = GL.CreateProgram();
            // загрузка програмы и ее компиляция
            GL.AttachShader(shderProgram_ID, vertexShader_ID);
            GL.AttachShader(shderProgram_ID, fragmentShader_ID);
            // линковка програмы
            GL.LinkProgram(shderProgram_ID);
            // получение кода
            GL.GetProgram(shderProgram_ID, 
                GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetProgramInfoLog(shderProgram_ID);
                throw new Exception($"Ошибка линковки шейдерной " +
                    $"программы № {shderProgram_ID} \n\n {infoLog}");
            }
            DeleteShader(vertexShader_ID);
            DeleteShader(fragmentShader_ID);
        }

        public void ActiveProgram() => GL.UseProgram(shderProgram_ID);

        public void DeactiveProgram() => GL.UseProgram(0);

        public void DeleteProgram() => GL.DeleteProgram(shderProgram_ID);

        public int GetAttribProgram(string name) =>
            GL.GetAttribLocation(shderProgram_ID, name);

        public void SetUniform4(string name, Vector4 vec)
        {
            int location = GL.GetUniformLocation(shderProgram_ID, name);
            GL.Uniform4(location, vec);
        }
        /// <summary>
        ///  создание шейдера
        /// </summary>
        /// <param name="shaderType"></param>
        /// <param name="shaderFile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private int CreateShader(ShaderType shaderType, string shaderFile)
        {
            string shaderStr = File.ReadAllText(shaderFile);
            int shaderID = GL.CreateShader(shaderType);
            GL.ShaderSource(shaderID, shaderStr);
            GL.CompileShader(shaderID);
        
            GL.GetShader(shaderID, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shaderID);
                throw new Exception($"Ошибка прикомпиляции шейдера № " +
                    $"{shaderID} \n\n {infoLog}");
            }

            return shaderID;
        }

        private void DeleteShader(int shader)
        {
            GL.DetachShader(shderProgram_ID, shader);
            GL.DeleteShader(shader);
        }
    }
}
