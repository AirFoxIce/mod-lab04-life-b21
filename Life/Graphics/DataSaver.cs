using System;
using System.Collections.Generic;
using System.IO;

namespace Life.Graphics
{
    public static class GraphDataSaver
    {
        public static void SaveData(List<(double x, int y)> data, string filePath, string delimiter = ";")
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var (x, y) in data)
                {
                    writer.WriteLine($"{x:F2};{y}");
                }
            }
        }
    }
}
