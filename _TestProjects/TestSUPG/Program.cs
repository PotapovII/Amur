﻿namespace TestSUPG
{
    using System;
    using System.Windows.Forms;

    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Test0 t = new Test0();
            //t.Do();
            //t.DoR();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
        M:
            Console.WriteLine("Выор панели задач:");
            Console.WriteLine("1. Канал Розовского");
            Console.WriteLine("2. Канал Вим ван Бален");
            Console.WriteLine("3. Канал трапецивидный");
            Console.WriteLine("4. Река Десна");
<<<<<<< HEAD
            Console.WriteLine("5. Река Muddy Creek");
            Console.WriteLine("6. Старые тесты");
=======
            Console.WriteLine("5. Старые тесты");
            Console.WriteLine("6. Тест 2D");
            Console.WriteLine("7. Тест 1D");
>>>>>>> bbbbf678e590e725cc293ddb1e5cb43af4912d05
            Console.WriteLine("Esc: выход");
            try
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                if (consoleKeyInfo.Key == ConsoleKey.Escape)
                    return;
                if (consoleKeyInfo.Key == ConsoleKey.D1)
                    Application.Run(new FVortexStream());
                if (consoleKeyInfo.Key == ConsoleKey.D2)
                    Application.Run(new FQuadVortexStream());
                if (consoleKeyInfo.Key == ConsoleKey.D3)
                    Application.Run(new FTrapezoidStreem());
                if (consoleKeyInfo.Key == ConsoleKey.D4)
                    Application.Run(new FDesna());
                if (consoleKeyInfo.Key == ConsoleKey.D5)
                    Application.Run(new FDitrich());
                if (consoleKeyInfo.Key == ConsoleKey.D6)
                    Application.Run(new FMain());
                if (consoleKeyInfo.Key == ConsoleKey.D6)
                {
                    PhiVortex task = new PhiVortex();
                    task.Solver();
                }
                if (consoleKeyInfo.Key == ConsoleKey.D7)
                {
                    Test0 t = new Test0();
                    t.Do();
                    t.DoR();
                }
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
