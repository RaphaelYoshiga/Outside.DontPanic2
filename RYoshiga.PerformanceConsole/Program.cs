using System;

namespace RYoshiga.PerformanceConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var floors = new Floors();
            var random = new Random();

            var rows = 12;
            var columns = 24;

            for (int i = 0; i <= rows; i++)
            {
                floors.Add(i, new Floor(random.Next(0, columns)));
            }

            var game = new Game(floors, rows, columns, 10);
            floors.InitializeScanResults(columns);
            game.SetGeneralProperties(1000, 100, rows);

            game.TakeDecision(new Clone(0, 0, Direction.Right));
        }
    }
}
