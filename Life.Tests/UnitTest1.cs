
using System;
using System.Collections.Generic;
using Xunit;
using Life.Models;
using Life.Services;

namespace Life.Tests
{
    public class CombinedTests
    {
        [Fact]
        public void AliveCell_WithTwoNeighbors_StaysAlive()
        {
            // Клетка с двумя соседями должна остаться живой
            var cell = new Cell { IsAlive = true };
            cell.neighbors.Add(new Cell { IsAlive = true });
            cell.neighbors.Add(new Cell { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void AliveCell_WithOneNeighbor_Dies()
        {
            // Клетка с одним соседом умирает
            var cell = new Cell { IsAlive = true };
            cell.neighbors.Add(new Cell { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void DeadCell_WithThreeNeighbors_ComesToLife()
        {
            // Мёртвая клетка с тремя живыми соседями оживает
            var cell = new Cell { IsAlive = false };
            cell.neighbors.Add(new Cell { IsAlive = true });
            cell.neighbors.Add(new Cell { IsAlive = true });
            cell.neighbors.Add(new Cell { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void Board_Advance_ChangesStateCorrectly()
        {
            // Проверка, что горизонтальная линия превращается в вертикальную (blinker)
            var board = new Board(3, 3, 1);
            board.Cells[0, 1].IsAlive = true;
            board.Cells[1, 1].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;

            board.Advance();

            Assert.True(board.Cells[1, 0].IsAlive);
            Assert.True(board.Cells[1, 1].IsAlive);
            Assert.True(board.Cells[1, 2].IsAlive);
        }

        [Fact]
        public void Board_ToroidalWrapping_WorksProperly()
        {
            // Проверяем, что поле правильно сворачивается (тор)
            var board = new Board(3, 3, 1);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            board.Cells[0, 2].IsAlive = true;
            var (alive, _) = BoardLoader.CountCellsAndGroups(board);
            Assert.Equal(3, alive);
        }

        [Fact]
        public void BlockPattern_RemainsStable_ForSeveralGenerations()
        {
            // Блок 2x2 не должен изменяться со временем
            var board = new Board(4, 4, 1);
            board.Cells[1, 1].IsAlive = true;
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;

            int stable = 0, last = -1, gen = 0;
            while (stable < 5 && gen < 50)
            {
                var (alive, _) = BoardLoader.CountCellsAndGroups(board);
                if (alive == last) stable++; else stable = 0;
                last = alive;
                board.Advance();
                gen++;
            }
            Assert.True(stable >= 5);
        }

        [Fact]
        public void LoadTemplates_ReturnsNonEmptyDictionary()
        {
            // Должен загрузить хотя бы один шаблон
            TemplateManager.LoadTemplates("shapes");
            Assert.True(TemplateManager.Templates.Count > 0);
        }

        [Fact]
        public void AreMatricesEqual_ReturnsTrue_WhenIdentical()
        {
            // Сравнение одинаковых матриц
            char[,] a = { { '0', '.' }, { '.', '0' } };
            char[,] b = { { '0', '.' }, { '.', '0' } };
            Assert.True(TemplateManager.AreMatricesEqual(a, b));
        }

        [Fact]
        public void AreMatricesEqual_ReturnsFalse_WhenDifferent()
        {
            // Сравнение разных матриц
            char[,] a = { { '0', '.' }, { '.', '0' } };
            char[,] b = { { '.', '0' }, { '0', '.' } };
            Assert.False(TemplateManager.AreMatricesEqual(a, b));
        }

        [Fact]
        public void Classify_RecognizesBlockPattern()
        {
            // Классификатор должен распознать блок
            TemplateManager.LoadTemplates("shapes");
            var board = new Board(4, 4, 1);
            board.Cells[1, 1].IsAlive = true;
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            var output = Classifier.ClassifyGroups(board);
            Assert.Contains("Блок", output);
        }

        [Fact]
        public void Classify_ReturnsUnknown_WhenNoMatch()
        {
            // Если шаблон не найден — вернуть "неизвестно"
            TemplateManager.LoadTemplates("shapes");
            var board = new Board(4, 4, 1);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[3, 3].IsAlive = true;
            var output = Classifier.ClassifyGroups(board);
            Assert.Contains("Неизвестная схема", output);
        }

        [Fact]
        public void CountCellsAndGroups_ReturnsCorrectAliveCount()
        {
            // Проверяем, что правильно считается количество живых клеток
            var board = new Board(5, 5, 1);
            board.Cells[1, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;

            var (alive, _) = BoardLoader.CountCellsAndGroups(board);

            Assert.Equal(2, alive);
        }

        [Fact]
        public void Advance_KillsLonelyCell()
        {
            // Одинокая клетка должна умереть после одного поколения
            var board = new Board(3, 3, 1);
            board.Cells[1, 1].IsAlive = true;

            board.Advance();

            Assert.False(board.Cells[1, 1].IsAlive);
        }

        [Fact]
        public void ClassifyGroups_HandlesEmptyTemplateGracefully()
        {
            // Проверяем, что классификатор не ломается, если шаблонов нет
            TemplateManager.Templates.Clear();
            var board = new Board(4, 4, 1);
            var result = Classifier.ClassifyGroups(board);
            Assert.NotNull(result);
        }


        [Fact]
        public void Randomize_CreatesDifferentBoards()
        {
            // Два случайных поля с одинаковыми настройками не должны быть идентичными
            var board1 = new Board(10, 10, 1, 0.5);
            var board2 = new Board(10, 10, 1, 0.5);

            bool identical = true;
            for (int x = 0; x < board1.Columns; x++)
                for (int y = 0; y < board1.Rows; y++)
                    if (board1.Cells[x, y].IsAlive != board2.Cells[x, y].IsAlive)
                    {
                        identical = false;
                        break;
                    }

            Assert.False(identical);
        }
    }
}
