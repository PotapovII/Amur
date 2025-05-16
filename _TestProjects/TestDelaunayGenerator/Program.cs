using CommonLib.Geometry;
using DelaunayGenerator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDelaunayGenerator.Areas;
using TestDelaunayGenerator.Boundary;

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
            bool showForm = true; //отобразить форму
            bool usePointsFilter = true; //true - использовать предварительный фильтр
            int countTests = 1; //количество тестов
            Test test = new Test();
            M:
            Console.WriteLine("Выор тестовой области:");
            Console.WriteLine("1. Прямоугольник простой");
            Console.WriteLine("2. Прямоугольник большой");
            Console.WriteLine("3. Трапеция");
            Console.WriteLine("4. Круглое множество");
            Console.WriteLine("5. Круглое множество с границей");
            Console.WriteLine("6. Круглое множество с вогнутой границей");
            Console.WriteLine("7. Равномерное распределение");
            Console.WriteLine("8. Звезда (сетка) (с границей)");
            Console.WriteLine("Esc: выход");
            try
            {
                //ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();

                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.Escape:
                        return;
                        case ConsoleKey.D1:
                            test.CreateRestArea(0);
                            test.Run();
                        break;
                    case ConsoleKey.D2:
                        test.CreateRestArea(1);
                        test.Run();
                        break;
                    case ConsoleKey.D3:
                        test.CreateRestArea(2);
                        test.Run();
                        break;
                    case ConsoleKey.D4:
                        test.CreateRestArea(3);
                        test.Run();
                        break;
                    case ConsoleKey.D5:
                        test.CreateRestArea(4);
                        test.Run();
                        break;
                    case ConsoleKey.D6:
                        test.CreateRestArea(5);
                        test.Run();
                        break;
                    case ConsoleKey.D7:
                        AreaBase area = new UniformArea();
                        area.Initialize();
                        test.Run(area, usePointsFilter, countTests, showForm);
                        break;
                    case ConsoleKey.D8:
                        area = new GridArea();
                        area.BoundaryGenerator = new GeneratorFixed(50); //алгоритм генерации граничных то
                        IHPoint[] boundary = new IHPoint[]
                        {
                                new HPoint(0.5001,0.9001),
                                new HPoint(0.6001,0.4001),
                                new HPoint(0.9001,0.38001),
                                new HPoint(0.6001,0.2001),
                                new HPoint(0.9001,0.0101),
                                new HPoint(0.5001,0.1001),
                                new HPoint(0.1001,0.0101),
                                new HPoint(0.29, 0.135), //обычный узел заскочил на сетку
                                new HPoint(0.4011,0.2001),
                                new HPoint(0.0101,0.38001),
                                new HPoint(0.4001,0.4001),
                        };
                        area.AddBoundary(boundary);
                        test.Run(area, usePointsFilter, countTests, showForm);
                        break;
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
