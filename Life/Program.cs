using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Life.Models;
using Life.Services;
using Life.Graphics;

namespace Life
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = BoardLoader.LoadSettings("settings.json");
            TemplateManager.LoadTemplates("shapes");

            Console.WriteLine("Выберите режим:");
            Console.WriteLine("1 — обычная игра");
            Console.WriteLine("2 — исследование стабильности");
            Console.WriteLine();

            var choice = Console.ReadKey(true).Key;
            if (choice == ConsoleKey.D1)
            {
                RunInteractiveGame(settings);
            }
            else if (choice == ConsoleKey.D2)
            {
                RunStabilityResearch(settings);
            }
            else
            {
                Console.WriteLine("Неизвестный режим.");
            }
        }

        static void RunInteractiveGame(Settings settings)
        {
            var board = new Board(settings.width, settings.height, settings.cellSize, settings.liveDensity);
            var stableCount = 0;
            var lastAlive = -1;
            var gen = 0;
            var running = true;

            while (running)
            {
                gen++;
                Console.Clear();
                BoardLoader.Render(board);

                var (alive, groups) = BoardLoader.CountCellsAndGroups(board);
                Console.WriteLine($"\nЖивых клеток: {alive}");
                Console.WriteLine($"Групп клеток: {groups}");
                Console.WriteLine($"Поколение: {gen}");

                if (alive == lastAlive)
                    stableCount++;
                else
                    stableCount = 0;

                lastAlive = alive;

                if (stableCount >= settings.stableGenerations)
                {
                    Console.WriteLine("\nИгра остановлена.");
                    Classifier.ClassifyGroups(board);
                    break;
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.Q)
                    {
                        running = false;
                        Console.WriteLine("Выход из игры.");
                    }
                    else if (key == ConsoleKey.P)
                    {
                        Console.WriteLine("Пауза. Нажмите любую клавишу...");
                        Console.ReadKey(true);
                    }
                    else if (key == ConsoleKey.S)
                    {
                        BoardLoader.Save(board, "outBoard.txt");
                        Console.WriteLine("Сохранено в outBoard.txt");
                        Thread.Sleep(500);
                    }
                    else if (key == ConsoleKey.L)
                    {
                        BoardLoader.Load(board, "inBoard.txt");
                        Console.WriteLine("Загружено из inBoard.txt");
                        BoardLoader.Render(board);
                        gen = 1;
                    }
                }

                board.Advance();
                Thread.Sleep(settings.speed);
            }

            Console.ReadKey();
        }

        static void RunStabilityResearch(Settings settings)
        {
            var results = new List<(double, int)>();

            for (double d = 0.05; d <= 0.95; d += 0.05)
            {
                int g = RunSilentSimulation(settings, d);
                results.Add((d, g));
                Console.WriteLine($"Плотность {d:F2}: стабилизация за {g} поколений");
            }

            GraphDataSaver.SaveData(results, "stability_data.txt");
            Console.WriteLine("\nИсследование завершено. Данные записаны в stability_data.txt");
            PlotGenerator.GeneratePlot("stability_data.txt", "output.png");
            Console.ReadKey();
        }

        static int RunSilentSimulation(Settings settings, double density)
        {
            var board = new Board(settings.width, settings.height, settings.cellSize, density);
            int stable = 0;
            int last = -1;
            int gen = 0;

            while (true)
            {
                gen++;
                var alive = BoardLoader.CountCellsAndGroups(board).aliveCells;

                if (alive == last)
                    stable++;
                else
                    stable = 0;

                last = alive;

                if (stable >= settings.stableGenerations)
                    break;

                board.Advance();
            }

            return gen;
        }
    }
}
