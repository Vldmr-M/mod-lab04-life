using System.Collections.Generic;
using System.Linq;
using cli_life;
using Xunit;

namespace TestProject1
{
    public class BoardTests
    {
        [Fact]
        public void Board_InitializesCorrectly()
        {
            var board = new Board(100, 100, 10);
            Assert.Equal(10, board.CellSize);
            Assert.Equal(10, board.Columns);
            Assert.Equal(10, board.Rows);
        }

        [Fact]
        public void Board_ContainsCorrectNumberOfCells()
        {
            var board = new Board(100, 100, 10);
            Assert.Equal(10 * 10, board.Cells.Length);
        }

        [Fact]
        public void Board_CellsAreConnectedToNeighbors()
        {
            var board = new Board(100, 100, 10);
            foreach (var cell in board.Cells)
            {
                Assert.Equal(8, cell.neighbors.Count);
            }
        }

        [Fact]
        public void Cell_DeterminesNextState_CorrectlyForAliveCell()
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 3; i++) cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 5; i++) cell.neighbors.Add(new Cell { IsAlive = false });

            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void Cell_DiesWithFewNeighbors()
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 1; i++) cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 7; i++) cell.neighbors.Add(new Cell { IsAlive = false });

            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void Cell_ComesToLifeWithThreeNeighbors()
        {
            var cell = new Cell { IsAlive = false };
            for (int i = 0; i < 3; i++) cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 5; i++) cell.neighbors.Add(new Cell { IsAlive = false });

            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void Board_Advance_ChangesState()
        {
            var board = new Board(100, 100, 10, 1.0); // все живы
            int before = Program.CountAliveCells(board);
            board.Advance();
            int after = Program.CountAliveCells(board);

            Assert.NotEqual(before, after); // что-то точно изменилось
        }

        [Fact]
        public void ToStateAndFromState_PreservesData()
        {
            var board1 = new Board(100, 100, 10, 1.0);
            var state = board1.ToState();
            var board2 = Board.FromState(state);

            for (int x = 0; x < board1.Columns; x++)
            for (int y = 0; y < board1.Rows; y++)
                Assert.Equal(board1.Cells[x, y].IsAlive, board2.Cells[x, y].IsAlive);
        }

        [Fact]
        public void CountAliveCells_ReturnsCorrectCount()
        {
            var board = new Board(100, 100, 10, 0);
            int count = Program.CountAliveCells(board);
            Assert.Equal(0, count);
        }
        
        [Fact]
        public void ToState_ShouldReturnValidStructure()
        {
            var board = new Board(20, 20, 10, 0);
            var state = board.ToState();

            Assert.NotNull(state.Cells);
            Assert.Equal(board.Rows, state.Cells.Count);
            Assert.Equal(board.Columns, state.Cells[0].Count);
        }
        [Fact]
        public void Cell_DiesWithOverpopulation()
        {
            var center = new Cell { IsAlive = true };
            var neighbors = new List<Cell>();
            for (int i = 0; i < 8; i++) neighbors.Add(new Cell { IsAlive = true });
            center.neighbors.AddRange(neighbors);

            center.DetermineNextLiveState();
            center.Advance();

            Assert.False(center.IsAlive);
        }
        
        [Fact]
        public void Randomize_ShouldCreateSomeAliveCells()
        {
            var board = new Board(100, 100, 10);
            int aliveBefore = Program.CountAliveCells(board);
            board.Randomize(0.5);
            int aliveAfter = Program.CountAliveCells(board);

            Assert.True(aliveAfter > 0);
            Assert.NotEqual(aliveBefore, aliveAfter);
        }
        [Fact]
        public void CountAliveCells_ShouldReturnCorrectCount()
        {
            var board = new Board(30, 30, 10, 0);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[1, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;

            int aliveCount = Program.CountAliveCells(board);

            Assert.Equal(3, aliveCount);
        }
        [Fact]
        public void Advance_ShouldKillLonelyCell()
        {
            var board = new Board(30, 30, 10, 0);
            board.Cells[1, 1].IsAlive = true;

            board.Advance();

            Assert.False(board.Cells[1, 1].IsAlive); // одиночная клетка умирает
        }
        [Fact]
        public void Randomize_ShouldRespectDensity()
        {
            var board = new Board(50, 50, 10);
            board.Randomize(0.0);

            int alive = Program.CountAliveCells(board);
            Assert.Equal(0, alive);

            board.Randomize(1.0);
            alive = Program.CountAliveCells(board);
            int total = board.Columns * board.Rows;
            Assert.Equal(total, alive);
        }

    }
}
