using System;
using System.IO;
using ScottPlot;

namespace Life.Graphics
{
    public static class PlotGenerator
    {
        public static void GeneratePlot(string dataFile, string outputFile)
        {
            string[] lines = File.ReadAllLines(dataFile);
            int count = lines.Length;

            double[] xs = new double[count];
            double[] ys = new double[count];

            for (int i = 0; i < count; i++)
            {
                string[] parts = lines[i].Split(';');
                xs[i] = double.Parse(parts[0].Trim());
                ys[i] = double.Parse(parts[1].Trim());
            }

            var plt = new ScottPlot.Plot(800, 600);
            plt.AddScatter(xs, ys);
            plt.XLabel("Плотность заполнения");
            plt.YLabel("Поколения до стабилизации");
            plt.Title("График зависимости стабилизации от плотности");

            // Сохраняем изображение
            plt.SaveFig(outputFile);
            Console.WriteLine($"График сохранён в файл: {outputFile}");
        }
    }
}
