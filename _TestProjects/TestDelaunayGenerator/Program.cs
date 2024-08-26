using DelaunayGenerator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDelaunayGenerator
{
    internal class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Test test = new Test();
            M:
            Console.WriteLine("Выор тестовой области:");
            Console.WriteLine("1. Прямоугольник простой");
            Console.WriteLine("2. Прямоугольник большой");
            Console.WriteLine("3. Трапеция");
            Console.WriteLine("4. Круглое множество");
            Console.WriteLine("Esc: выход");
            try
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                if (consoleKeyInfo.Key == ConsoleKey.Escape)
                    return;
                if (consoleKeyInfo.Key == ConsoleKey.D1)
                    test.CreateRestArea(0);
                if (consoleKeyInfo.Key == ConsoleKey.D2)
                    test.CreateRestArea(1);
                if (consoleKeyInfo.Key == ConsoleKey.D3)
                    test.CreateRestArea(2);
                if (consoleKeyInfo.Key == ConsoleKey.D4)
                    test.CreateRestArea(3);
                test.Run();
                Console.Clear();
                goto M;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                goto M;
            }
            
            
        }
    }
}
