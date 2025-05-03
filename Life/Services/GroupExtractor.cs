using System;
using System.Collections.Generic;

namespace Life.Services
{
    public static class GroupExtractor
    {
        public static List<char[,]> ExtractAllGroupsFromMatrix(char[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            bool[,] visited = new bool[width, height];
            List<char[,]> groups = new();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!visited[x, y] && matrix[x, y] == '0')
                    {
                        var group = ExtractSingleGroup(matrix, x, y, visited);
                        groups.Add(group);
                    }
                }
            }

            return groups;
        }

        public static char[,] ExtractSingleGroup(char[,] matrix, int startX, int startY, bool[,] visited)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            var stack = new Stack<(int, int)>();
            var cells = new List<(int, int)>();

            int minX = startX, maxX = startX;
            int minY = startY, maxY = startY;

            stack.Push((startX, startY));

            while (stack.Count > 0)
            {
                var (x, y) = stack.Pop();
                x = (x + width) % width;
                y = (y + height) % height;

                if (visited[x, y] || matrix[x, y] != '0')
                    continue;

                visited[x, y] = true;
                cells.Add((x, y));

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;

                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        stack.Push((x + dx, y + dy));
                    }
            }

            int groupWidth = maxX - minX + 1;
            int groupHeight = maxY - minY + 1;
            var result = new char[groupWidth, groupHeight];

            for (int i = 0; i < groupWidth; i++)
                for (int j = 0; j < groupHeight; j++)
                    result[i, j] = '.';

            foreach (var (x, y) in cells)
                result[x - minX, y - minY] = '0';

            return result;
        }
    }
}
