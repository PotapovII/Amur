namespace TestSUPG
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
            M:
            Console.WriteLine("Выор панели задач:");
            Console.WriteLine("1. Канал Розовского");
            Console.WriteLine("2. Канал Вим ван Бален");
            Console.WriteLine("3. Канал трапецивидный");
            Console.WriteLine("4. Река Десна");
            Console.WriteLine("5. Река ... FDitrich");
            Console.WriteLine("6. Старые тесты");
            Console.WriteLine("7. Новые тесты Quad");
            Console.WriteLine("8. Тест 2D");
            Console.WriteLine("9. Тест 1D");
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
                if (consoleKeyInfo.Key == ConsoleKey.D7)
                    Application.Run(new Tests());
                if (consoleKeyInfo.Key == ConsoleKey.D8)
                {
                    TestPhiVortex task = new TestPhiVortex(21, 21, 1, 1, 0.5);
                    task.ShowMesh();
                }
                if (consoleKeyInfo.Key == ConsoleKey.D9)
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
