using System;
using System.IO;
using System.Text.Json;
using Life.Models;
using System.Collections.Generic;


namespace Life.Services
{
    public static class BoardLoader
    {
        public static Settings LoadSettings(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Settings>(json);
        }

        public static void Save(Board board, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    for (int x = 0; x < board.Columns; x++)
                    {
                        Cell currentCell = board.Cells[x, y];
                        writer.Write(currentCell.IsAlive ? '0' : '.');
                    }
                    writer.WriteLine();
                }
            }
        }

        public static void Load(Board board, string path)
        {
            string[] lines = File.ReadAllLines(path);
            for (int y = 0; y < board.Rows && y < lines.Length; y++)
            {
                string line = lines[y];
                for (int x = 0; x < board.Columns && x < line.Length; x++)
                {
                    Cell currentCell = board.Cells[x, y];
                    currentCell.IsAlive = (line[x] == '0');
                }
            }
        }

        public static void Render(Board board)
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    Console.Write(cell.IsAlive ? '0' : '.');
                }
                Console.Write('\n');
            }
        }

        public static (int aliveCells, int groups) CountCellsAndGroups(Board board)
        {
            int aliveCells = 0;
            int combinations = 0;
            bool[,] visited = new bool[board.Columns, board.Rows];

            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    if (board.Cells[x, y].IsAlive)
                    {
                        aliveCells++;
                        if (!visited[x, y])
                        {
                            CountCombinations(board, x, y, visited);
                            combinations++;
                        }
                    }
                }
            }

            return (aliveCells, combinations);
        }

        private static void CountCombinations(Board board, int x, int y, bool[,] visited)
        {
            var stack = new Stack<(int, int)>();
            stack.Push((x, y));

            while (stack.Count > 0)
            {
                var (currentX, currentY) = stack.Pop();

                if (visited[currentX, currentY])
                    continue;

                visited[currentX, currentY] = true;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int newX = (currentX + dx + board.Columns) % board.Columns;
                        int newY = (currentY + dy + board.Rows) % board.Rows;

                        if (!visited[newX, newY] && board.Cells[newX, newY].IsAlive)
                        {
                            stack.Push((newX, newY));
                        }
                    }
                }
            }
        }

    }
}
