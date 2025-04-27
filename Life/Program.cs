using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace cli_life
{
    public class CellState
{
    public bool IsAlive { get; set; }
}

public class BoardState
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int CellSize { get; set; }
    public List<List<CellState>> Cells { get; set; }
}
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
        public static Board FromState(BoardState state)
        {
            var board = new Board(state.Width, state.Height, state.CellSize, 0);
            for (int y = 0; y < board.Rows; y++)
            {
                for (int x = 0; x < board.Columns; x++)
                {
                    board.Cells[x, y].IsAlive = state.Cells[y][x].IsAlive;
                }
            }
            return board;
        }
        public BoardState ToState()
        {
            var state = new BoardState
            {
                Width = Width,
                Height = Height,
                CellSize = CellSize,
                Cells = new List<List<CellState>>()
            };

            for (int y = 0; y < Rows; y++)
            {
                var row = new List<CellState>();
                for (int x = 0; x < Columns; x++)
                {
                    row.Add(new CellState { IsAlive = Cells[x, y].IsAlive });
                }
                state.Cells.Add(row);
            }

            return state;
        }
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
    public class BoardConfig
    {
        public int width { get; set; }
        public int height { get; set; }
        public int cellSize { get; set; }
        public double liveDensity { get; set; }
    }
    public class Program
    {
        static Board board;

        static private void Reset()
        {
            string configPath = Path.Combine(GetSourceDirectory(), "config.json");

            string jsonString = File.ReadAllText(configPath);
            Console.Write(jsonString);
            BoardConfig config = JsonSerializer.Deserialize<BoardConfig>(jsonString);
            board = new Board(config.width, config.height, config.cellSize, config.liveDensity);
            Console.Write(board);
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
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static string GetSourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string path = "")
        {
            return Path.GetDirectoryName(path);
        }
        // static void Save(string filePath)
        // {
        //     var state = board.ToState();
        //     string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        //     File.WriteAllText(filePath, json);
        //     Console.WriteLine("Состояние сохранено в " + filePath);
        // }
        // static void Load(string filePath)
        // {
        //     if (!File.Exists(filePath))
        //     {
        //         Console.WriteLine("Файл не найден: " + filePath);
        //         return;
        //     }

        //     string json = File.ReadAllText(filePath);
        //     BoardState state = JsonSerializer.Deserialize<BoardState>(json);
        //     board = Board.FromState(state);
        //     Console.WriteLine("Состояние загружено из " + filePath);
        // }
        public static void LoadFromTxt(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден: " + filePath);
                return;
            }

            string[] lines = File.ReadAllLines(filePath);

            int width = int.Parse(lines[0].Split('=')[1]);
            int height = int.Parse(lines[1].Split('=')[1]);
            int cellSize = int.Parse(lines[2].Split('=')[1]);

            board = new Board(width, height, cellSize);

            for (int y = 0; y < board.Rows; y++)
            {
                string line = lines[3 + y];
                for (int x = 0; x < board.Columns; x++)
                {
                    board.Cells[x, y].IsAlive = line[x] == '*';
                }
            }

            Console.WriteLine("Состояние загружено из " + filePath);
        }
        public static void SaveAsTxt(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Width={board.Width}");
                writer.WriteLine($"Height={board.Height}");
                writer.WriteLine($"CellSize={board.CellSize}");

                for (int y = 0; y < board.Rows; y++)
                {
                    StringBuilder line = new StringBuilder();
                    for (int x = 0; x < board.Columns; x++)
                    {
                        line.Append(board.Cells[x, y].IsAlive ? '*' : '.');
                    }
                    writer.WriteLine(line.ToString());
                }
            }

            Console.WriteLine("Состояние сохранено в " + filePath);
        }
        static void LoadPattern(string patternName, int offsetX = 0, int offsetY = 0)
        {
            string filePath = Path.Combine(GetSourceDirectory(), "patterns", patternName + ".txt");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл с паттерном не найден: " + filePath);
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    int boardX = x + offsetX;
                    int boardY = y + offsetY;

                    if (boardX >= 0 && boardX < board.Columns && boardY >= 0 && boardY < board.Rows)
                    {
                        board.Cells[boardX, boardY].IsAlive = lines[y][x] == '*';
                    }
                }
            }

            Console.WriteLine($"Паттерн '{patternName}' загружен.");
        }
        public static int CountAliveCells(Board board)
        {
            int count = 0;
            foreach (var cell in board.Cells)
                if (cell.IsAlive)
                    count++;
            return count;
        }

        static void ResearchStability(int maxGenerations = 500, int stablePeriod = 10)
        {
            string outputPath = Path.Combine(GetSourceDirectory(), "result.csv");
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("Density,StableGeneration");

                double[] densities = {0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };

                foreach (double density in densities)
                {
                    Console.WriteLine($"▶ Исследуем плотность {density}");

                    board = new Board(100, 100, 10, density);

                    Queue<int> lastCounts = new Queue<int>();
                    int stableGeneration = -1;

                    for (int generation = 0; generation < maxGenerations; generation++)
                    {
                        int alive = CountAliveCells(board);

                        lastCounts.Enqueue(alive);
                        if (lastCounts.Count > stablePeriod)
                            lastCounts.Dequeue();

                        if (lastCounts.Count == stablePeriod && lastCounts.All(x => x == lastCounts.Peek()))
                        {
                            stableGeneration = generation;
                            break;
                        }

                        board.Advance();
                    }

                    writer.WriteLine($"{density}   {(stableGeneration >= 0 ? stableGeneration.ToString() : "None")}");
                }
            }

            Console.WriteLine($"✅ Данные записаны в: {outputPath}");
        }


        static void Main(string[] args)
        {
            Console.WriteLine("1 - Начать заново, 2 - Загрузить состояние:");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.D1) Reset();
            else if (key == ConsoleKey.D2) LoadFromTxt(Path.Combine(GetSourceDirectory(), "GameBoard.txt"));
            while(true)
            {
                Console.Clear();
                Render();
                board.Advance();
                Thread.Sleep(500);
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.S)
                    {
                        SaveAsTxt(Path.Combine(GetSourceDirectory(), "GameBoard.txt"));

                    }
                    else if (k == ConsoleKey.Escape)
                    {
                        System.Console.WriteLine("ESC BUTTON");
                        break;
                    }
                }
            }
            // ResearchStability();
        }
    }
}