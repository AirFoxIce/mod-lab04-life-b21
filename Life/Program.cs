using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        public Board(int width, int height, int cellSize)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }

    public class Settings
    {
        public int width { get; set; }
        public int height { get; set; }
        public int cellSize { get; set; }
        public double liveDensity { get; set; }
        public int stableGenerations { get; set; }
        public int speed { get; set; }
    }

    class Program
    {
        static Settings LoadSettings(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Settings>(json);
        }

        static void Load(string path)
        {
            string[] lines = File.ReadAllLines(path);
            int rows = lines.Length;
            int columns = lines[0].Length;
            board = new Board(columns, rows, 1);
            for (int y = 0; y < board.Rows && y < lines.Length; y++)
            {
                string line = lines[y];

                for (int x = 0; x < board.Columns && x < line.Length; x++)
                {
                    Cell currentCell = board.Cells[x, y];

                    if (line[x] == '0')
                    {
                        currentCell.IsAlive = true;
                    }
                    else
                    {
                        currentCell.IsAlive = false;
                    }
                }
            }
        }

        static void Save(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    for (int x = 0; x < board.Columns; x++)
                    {
                        Cell currentCell = board.Cells[x, y];
                        if (currentCell.IsAlive)
                        {
                            writer.Write('0');
                        }
                        else
                        {
                            writer.Write('.');
                        }
                    }
                    writer.WriteLine();
                }
            }
        }

        static private (int aliveCells, int groups) CountCellsAndGroups(Board board)
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

        static void LoadTemplates(string folderPath)
        {
            templates.Clear();

            foreach (string filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                string[] lines = File.ReadAllLines(filePath);

                int width = lines[0].Length;
                int height = lines.Length;

                char[,] matrix = new char[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        matrix[x, y] = lines[y][x];
                    }
                }

                string templateName = Path.GetFileNameWithoutExtension(filePath);
                templates[templateName] = matrix;
            }
        }

        static List<char[,]> ExtractAllGroupsFromMatrix(char[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            bool[,] visited = new bool[width, height];
            List<char[,]> groups = new List<char[,]>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!visited[x, y] && matrix[x, y] == '0')
                    {
                        var groupMatrix = ExtractSingleGroup(matrix, x, y, visited);
                        groups.Add(groupMatrix);
                    }
                }
            }

            return groups;
        }


        static void DebugPrintGroupsAndTemplates(List<char[,]> groups, Dictionary<string, char[,]> templates)
        {
            Console.WriteLine("\n=== Группы ===");
            for (int i = 0; i < groups.Count; i++)
            {
                Console.WriteLine($"Группа {i + 1}:");
                PrintMatrix(groups[i]);
                Console.WriteLine();
            }

            Console.WriteLine("\n=== Шаблоны ===");
            foreach (var template in templates)
            {
                Console.WriteLine($"Шаблон {template.Key}:");
                PrintMatrix(template.Value);
                Console.WriteLine();
            }
        }

        static void PrintMatrix(char[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write(matrix[x, y]);
                }
                Console.WriteLine();
            }
        }


        static void ClassifyGroups(Board board)
        {
            // Создаём рабочую копию текущего поля в виде матрицы
            char[,] matrix = new char[board.Columns, board.Rows];
            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    matrix[x, y] = board.Cells[x, y].IsAlive ? '0' : '.';
                }
            }

            LoadTemplates("shapes");

            var groups = ExtractAllGroupsFromMatrix(matrix);

            DebugPrintGroupsAndTemplates(groups, templates);

            foreach (var group in groups)
            {
                bool matched = false;

                foreach (var template in templates)
                {
                    if (AreMatricesEqual(group, template.Value))
                    {
                        Console.WriteLine(template.Key);
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    Console.WriteLine("Неизвестная схема");
                }
            }
        }



        static char[,] ExtractSingleGroup(char[,] matrix, int startX, int startY, bool[,] visited)
        {
            var stack = new Stack<(int, int)>();
            var cells = new List<(int, int)>();

            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            int[,] adjustedX = new int[width, height];
            int[,] adjustedY = new int[width, height];

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    adjustedX[i, j] = int.MaxValue;
                    adjustedY[i, j] = int.MaxValue;
                }

            stack.Push((startX, startY));

            int minX = 0, maxX = 0;
            int minY = 0, maxY = 0;
            bool first = true;

            adjustedX[(startX + width) % width, (startY + height) % height] = 0;
            adjustedY[(startX + width) % width, (startY + height) % height] = 0;

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();

                int realX = (x + width) % width;
                int realY = (y + height) % height;

                if (visited[realX, realY])
                    continue;

                if (matrix[realX, realY] != '0')
                    continue;

                visited[realX, realY] = true;

                int adjX = adjustedX[realX, realY];
                int adjY = adjustedY[realX, realY];

                cells.Add((adjX, adjY));

                if (first)
                {
                    minX = maxX = adjX;
                    minY = maxY = adjY;
                    first = false;
                }
                else
                {
                    minX = Math.Min(minX, adjX);
                    maxX = Math.Max(maxX, adjX);
                    minY = Math.Min(minY, adjY);
                    maxY = Math.Max(maxY, adjY);
                }

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        int nx = x + dx;
                        int ny = y + dy;

                        int newRealX = (nx + width) % width;
                        int newRealY = (ny + height) % height;

                        if (!visited[newRealX, newRealY] && matrix[newRealX, newRealY] == '0')
                        {
                            int currentAdjX = adjustedX[realX, realY];
                            int currentAdjY = adjustedY[realX, realY];

                            int diffX = dx;
                            int diffY = dy;

                            int adjNeighborX = currentAdjX + diffX;
                            int adjNeighborY = currentAdjY + diffY;

                            if (adjustedX[newRealX, newRealY] == int.MaxValue)
                                adjustedX[newRealX, newRealY] = adjNeighborX;
                            if (adjustedY[newRealX, newRealY] == int.MaxValue)
                                adjustedY[newRealX, newRealY] = adjNeighborY;

                            stack.Push((nx, ny));
                        }
                    }
                }
            }

            int groupWidth = maxX - minX + 1;
            int groupHeight = maxY - minY + 1;
            char[,] groupMatrix = new char[groupWidth, groupHeight];

            for (int i = 0; i < groupWidth; i++)
                for (int j = 0; j < groupHeight; j++)
                    groupMatrix[i, j] = '.';

            foreach (var (gx, gy) in cells)
            {
                groupMatrix[gx - minX, gy - minY] = '0';
            }

            return groupMatrix;
        }


        private static bool AreMatricesEqual(char[,] a, char[,] b)
        {
            if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                return false;

            for (int x = 0; x < a.GetLength(0); x++)
            {
                for (int y = 0; y < a.GetLength(1); y++)
                {
                    if (a[x, y] != b[x, y])
                        return false;
                }
            }

            return true;
        }

        static Board board;
        static int lastAliveCells = -1;
        static int stableCounter = 0;
        static int stableGenerations;
        static int countSteps = 0;
        static Dictionary<string, char[,]> templates = new Dictionary<string, char[,]>();
        static private void Reset(Settings settings)
        {
            if (settings.liveDensity < 0.0)
            {
                settings.liveDensity = 0.0;
            }
            else if (settings.liveDensity > 1.0)
            {
                settings.liveDensity = 1.0;
            }
            board = new Board(settings.width, settings.height, settings.cellSize, settings.liveDensity);
        }
        static void Render()
        {
            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('0');
                    }
                    else
                    {
                        Console.Write('.');
                    }
                }
                Console.Write('\n');
            }
        }
        static void Main(string[] args)
        {
            LoadTemplates("shapes");
            var settings = LoadSettings("settings.json");
            stableGenerations = settings.stableGenerations;
            Reset(settings);
            bool running = true;
            while (running)
            {
                countSteps++;
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            running = false;
                            Console.WriteLine("Игра окончена.");
                            break;

                        case ConsoleKey.P:
                            Console.WriteLine("Пауза. Нажмите любую клавишу для продолжения...");
                            Console.ReadKey(true);
                            break;

                        case ConsoleKey.S:
                            Save("outBoard.txt");
                            Console.WriteLine("Состояние игры сохранено.");
                            Thread.Sleep(500);
                            break;
                        case ConsoleKey.L:
                            Load("inBoard.txt");
                            countSteps = 1;
                            Console.WriteLine("Состояние игры загружено.");
                            Render();
                            break;
                    }
                }
                else
                {
                    Console.Clear();
                    Render();

                    var (aliveCells, groups) = CountCellsAndGroups(board);
                    Console.WriteLine($"\n===== Информация о поле =====");
                    Console.WriteLine($"Живых клеток: {aliveCells}");
                    Console.WriteLine($"Групп клеток : {groups}");
                    Console.WriteLine($"Количество поколений : {countSteps}");
                    Console.WriteLine($"==============================\n");

                    if (aliveCells == lastAliveCells)
                    {
                        stableCounter++;
                    }
                    else
                    {
                        stableCounter = 0;
                    }

                    lastAliveCells = aliveCells;

                    if (stableCounter >= stableGenerations)
                    {
                        running = false;
                    }
                    board.Advance();
                    Thread.Sleep(settings.speed);
                }
            }
            Console.WriteLine("\nИгра остановлена.");
            ClassifyGroups(board);
            Console.ReadKey();
        }
    }
}