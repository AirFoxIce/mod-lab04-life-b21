using System;
using System.Threading;
using Life.Models;
using Life.Services;

namespace Life
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = BoardLoader.LoadSettings("settings.json");

            Board board = new Board(settings.width, settings.height, settings.cellSize, settings.liveDensity);

            TemplateManager.LoadTemplates("shapes");

            bool running = true;
            int stableCounter = 0;
            int lastAliveCells = -1;
            int countSteps = 0;

            while (running)
            {
                countSteps++;

                Console.Clear();
                BoardLoader.Render(board);

                var (aliveCells, groups) = BoardLoader.CountCellsAndGroups(board);

                Console.WriteLine($"\n===== Информация о поле =====");
                Console.WriteLine($"Живых клеток: {aliveCells}");
                Console.WriteLine($"Групп клеток: {groups}");
                Console.WriteLine($"Количество поколений: {countSteps}");
                Console.WriteLine($"==============================\n");

                if (aliveCells == lastAliveCells)
                    stableCounter++;
                else
                    stableCounter = 0;

                lastAliveCells = aliveCells;

                if (stableCounter >= settings.stableGenerations)
                {
                    Console.WriteLine("\nИгра остановлена.\n");
                    Classifier.ClassifyGroups(board);
                    
                    break;
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.Q:
                            running = false;
                            Console.WriteLine("Игра завершена.");
                            break;
                        case ConsoleKey.P:
                            Console.WriteLine("Пауза. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey(true);
                            break;
                        case ConsoleKey.S:
                            BoardLoader.Save(board, "outBoard.txt");
                            Console.WriteLine("Состояние игры сохранено.");
                            Thread.Sleep(500);
                            break;
                        case ConsoleKey.L:
                            BoardLoader.Load(board, "inBoard.txt");
                            Console.WriteLine("Состояние игры загружено.");
                            BoardLoader.Render(board);
                            countSteps = 1;
                            break;
                    }
                }

                board.Advance();
                Thread.Sleep(settings.speed);
            }

            Console.ReadKey();
        }
    }
}
