using SudokuSolver.BoardData;
using SudokuSolver.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            runAll();
            //runSingle();
        }

        public static void runSingle()
        {
            Board startBoard = BoardReader.getBoard(15, 1);
            if (startBoard == null) return;
            Solver solver = new Solver(startBoard);
            Board endBoard = solver.Solve();

            solver.PrintCanGoArrays();

            solver.PrintSteps();

            Console.WriteLine("Before:");
            Console.WriteLine(startBoard.ToString());
            Console.WriteLine();
            Console.WriteLine("After:");
            Console.WriteLine(endBoard.ToString(false));
            Console.WriteLine();
            Console.WriteLine("Invlid Cells:");
            Console.WriteLine(String.Join("\n", solver.InvalidCells().Select(p => p.ToString())));
        }

        public static void runAll()
        {
            // setup
            int maxPuzzleNo = 16;
            int maxDifficulty = 3;
            int summaryCount = 0;
            string[,] summary = new string[maxPuzzleNo * maxDifficulty + 2, 4]; // plus two for headers
            Action<object, object, object, object> setSummaryValues = (object col1, object col2, object col3, object col4) =>
            {
                summary[summaryCount, 0] = col1.ToString();
                summary[summaryCount, 1] = col2.ToString();
                summary[summaryCount, 2] = col3.ToString();
                summary[summaryCount, 3] = col4.ToString();
                summaryCount += 1;
            };
            Action setHeader = () => setSummaryValues("Puzzle No.", "Dificulty", "Is Filled", "Is Valid");
            setHeader();
            // run it
            for (int puzzleNo = 1; puzzleNo <= maxPuzzleNo; puzzleNo += 1)
            {
                for (int difficulty = 1; difficulty <= maxDifficulty; difficulty += 1)
                {
                    string puzzleNoStr = puzzleNo.ToString().PadLeft(2, '0');
                    string difficultyStr = difficulty.ToString().PadLeft(2, '0');

                    Board startBoard = BoardReader.getBoard(puzzleNo, difficulty);
                    if (startBoard == null) {
                        setSummaryValues(puzzleNoStr, difficultyStr, "N/A", "N/A");
                        continue;
                    }
                    Solver solver = new Solver(startBoard, Solver.DebugOptionFlags.NONE);
                    Board endBoard = solver.Solve();
                    setSummaryValues(puzzleNoStr, difficultyStr, endBoard.IsFilled(), solver.IsValid());
                }
            }
            // print
            setHeader();
            Console.WriteLine(summary.ToArrayString(-1, 1));
        }

        public static void tests()
        {
            Board board = BoardReader.getBoard(16, 2);
            Console.WriteLine(board.GetCell(2, 3) == board.GetRow(2)[3]);
            Console.WriteLine(board.GetCell(3, 5) == board.GetColumn(5)[3]);
            Console.WriteLine(board.GetCell(4, 5) == board.GetSquare(1, 1)[1, 2]);

            Console.WriteLine(board.GetRow(1).GetHashCode() == board.GetRow(1).GetHashCode());
            Console.WriteLine(board.GetColumn(1).GetHashCode() == board.GetColumn(1).GetHashCode());
            Console.WriteLine(board.GetSquare(1, 1).GetHashCode() == board.GetSquare(1, 1).GetHashCode());

            Console.WriteLine(new Board().GetHashCode() != new Board().GetHashCode());

        }
    }
}
