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
            Console.WriteLine("4. Старые тесты");
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
                    Application.Run(new FMain());
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
