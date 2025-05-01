using System;
using System.Collections.Generic;
using System.IO;

namespace Life.Services
{
    public static class TemplateManager
    {
        public static Dictionary<string, char[,]> Templates { get; private set; } = new Dictionary<string, char[,]>();

        public static void LoadTemplates(string folderPath)
        {
            Templates.Clear();

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
                Templates[templateName] = matrix;
            }
        }

        public static bool AreMatricesEqual(char[,] a, char[,] b)
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

        public static char[,] RotateMatrix(char[,] matrix)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            char[,] rotated = new char[height, width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    rotated[y, width - x - 1] = matrix[x, y];
                }
            }

            return rotated;
        }

    }
}
