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

        public static char[,] ExtractSingleGroup(char[,] matrix, int startX, int startY, bool[,] visited)
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
    }
}
