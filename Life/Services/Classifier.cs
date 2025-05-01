using System;
using System.Collections.Generic;
using Life.Models;

namespace Life.Services
{
    public static class Classifier
    {
        public static void ClassifyGroups(Board board)
        {
            char[,] matrix = new char[board.Columns, board.Rows];

            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    matrix[x, y] = board.Cells[x, y].IsAlive ? '0' : '.';
                }
            }

            var groups = GroupExtractor.ExtractAllGroupsFromMatrix(matrix);

            foreach (var group in groups)
            {
                bool matched = false;

                foreach (var template in TemplateManager.Templates)
                {
                    char[,] original = template.Value;
                    char[,] rotated90 = TemplateManager.RotateMatrix(original);
                    char[,] rotated180 = TemplateManager.RotateMatrix(rotated90);
                    char[,] rotated270 = TemplateManager.RotateMatrix(rotated180);

                    if (TemplateManager.AreMatricesEqual(group, original) ||
                        TemplateManager.AreMatricesEqual(group, rotated90) ||
                        TemplateManager.AreMatricesEqual(group, rotated180) ||
                        TemplateManager.AreMatricesEqual(group, rotated270))
                    {
                        Console.WriteLine($"Найдена схема: {template.Key}");
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
    }
}