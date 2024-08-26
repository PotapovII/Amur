using OpenTK;
using System;

namespace ViewerGL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameWindow main = null;
            string[] strings = {
                    "11 :вершинная отрисовка простых фигур",
                    "1 :вершинная отрисовка простых фигур",
                    "2 :вершинные дисплейные списки",
                    "3 :дисплейные списки",
                    "4 :дисплейные элементные списки",
                    "5 :вершинные буферные объекты (VBO)",
                    "6 :объекты вершинных массивов (VAO)",
                    "7 :перспективная проекция",
                    "8 :работа с текстурами в 2D",
                    "9 :шейдеры, EBO и VAO",
                    "0 :работа с камерой и светом",
                    "Q | q :работа с камерой, светом и материалом",
                    "W | w :работа со светом - солнышко",
                    "E | e :работа со светом и нормалями - лампа",
                    "R | r :отрисовка КЭ сетки работа",
                    "T | t :работа мышью и камерой",
                    "Y | y :работа мышью и масштабом",
                    "U | u :работа с текстом",
                    "UU | uu :работа с текстом",
                    "I | i :работа с текстом",
                    "O | o :работа масштабом",
                    "P | p :работа со шрифтами",
                    "Z | z :ВЫХОД"
                };

            for (; ;)
            {
                Console.Clear();
                foreach (string s in strings)
                    Console.WriteLine(s);

                string skey = Console.ReadLine();
                
                if (skey == "Z" || skey == "z") break;
                
                switch (skey)
                {
                    case "11":
                        main = new GLMain_00();
                        break;
                    case "1":
                        main = new GLMain_01_Simple();
                        break;
                    case "2":
                        main = new GLMain_02_DisplayList();
                        break;
                    case "3":
                        main = new GLMain_03_VertexArray();
                        break;
                    case "4":
                        main = new GLMain_03_VertexArrayIndex();
                        break;
                    case "5":
                        main = new GLMain_04_VBO();
                        break;
                    case "6":
                        main = new GLMain_05_VAO();
                        break;
                    case "7":
                        main = new GLMain_06_Frustum();
                        break;
                    case "8":
                        main = new GLMain_07_Image();
                        break;
                    case "9":
                        main = new GLMain_09_Shader_EBO();
                        break;
                    case "0":
                        main = new GLMain_10_Light0();
                        break;
                    case "Q":
                    case "q":
                        main = new GLMain_10_Light01();
                        break;
                    case "W":
                    case "w":
                        main = new GLMain_11_Sun();
                        break;
                    case "E":
                    case "e":
                        main = new GLMain_13_Lamp();
                        break;
                    case "R":
                    case "r":
                        main = new GLMain_00_Render_Mesh();
                        break;
                    case "T":
                    case "t":
                        main = new GLMain_12_Frustum_and_Mouse();
                        break;
                    case "Y":
                    case "y":
                        main = new GLView_MouseScale();
                        break;
                    case "U":
                    case "u":
                        main = new GLMain_14_Text();
                        break;
                    case "UU":
                    case "uu":
                        main = new GLMain_14_Text1();
                        break;
                    case "I":
                    case "i":
                        main = new GLMain_14_Text2();
                        break;
                    case "O":
                    case "o":
                        main = new GLViewOne();
                        break;
                    case "P":
                    case "p":
                        main = new GLMain_14_TextFont();
                        break;
                        
                }
                if (main != null)
                {
                    double fps = 120;
                    main.VSync = VSyncMode.On;
                    main.Run(fps);
                }
                else
                    Console.WriteLine("Задача не выбрана");
            }
        }
    }
}
