using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Helpers
{
    public static class EnumerableHelper
    {
        public static IEnumerable<(int Row, int Column)> GridRange(int count) => GridRange(0, 0, count, count);
        public static IEnumerable<(int Row, int Column)> GridRange(int start, int count) => GridRange(start, start, count, count);
        public static IEnumerable<(int Row, int Column)> GridRange(int start, int rowCount, int columnCount) => GridRange(start, start, rowCount, columnCount);
        public static IEnumerable<(int Row, int Column)> GridRange(int rowStart, int columnStart, int rowCount, int columnCount)
        {
            if (rowCount < 0 || columnCount < 0)
                throw new ArgumentException("Counts cannot be less than zero.");
            for (int row = rowStart; row < rowCount; row += 1)
                for (int column = columnStart; column < columnCount; column += 1)
                    yield return (Row: row, Column: column);
        }
    }
}
