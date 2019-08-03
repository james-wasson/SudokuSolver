using SudokuSolver.BoardData;
using SudokuSolver.Navigators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver
{
    public static class BoardReader
    {
        public static Board getBoard(String name)
        {
            string text;
            try
            {
                // Read entire text file content in one string 
                text = File.ReadAllText("assets/problems/" + name + ".txt");
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            var values = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(str =>
            {
                try
                {
                    return (CellValue?)Enum.Parse(typeof(CellValue), str);
                }
                catch (Exception)
                {
                    return null;
                }
            }).ToArray();
            return new Board(values);
        }

        public static Board getBoard(int difficulty, int boardNo)
        {
            string paddedDifficulty = difficulty.ToString().PadLeft(2, '0');
            string paddedBoardNo = boardNo.ToString().PadLeft(2, '0');
            return getBoard(paddedDifficulty + '_' + paddedBoardNo);
        }
    }
}
