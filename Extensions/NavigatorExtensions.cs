using SudokuSolver.BoardData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static SudokuSolver.BoardData.Board;
using SudokuSolver.Navigators;
using SudokuSolver.Interfaces;

namespace SudokuSolver.Extensions
{
    public static class NavigatorExtensions
    {
        public static IEnumerable<Cell> CellsWithValue(this IEnumerable<Cell> cells, params CellValue[] values) => cells.Where(p => values.Contains(p.Value));
        public static IEnumerable<Cell> CellsWithoutValue(this IEnumerable<Cell> cells, params CellValue[] values) => cells.Where(p => !values.Contains(p.Value));
        public static IEnumerable<Cell> CellsWithValue(this IEnumerable<Cell> cells, CellValue value) => cells.Where(p => p.Value == value);
        public static IEnumerable<Cell> CellsWithoutValue(this IEnumerable<Cell> cells, CellValue value) => cells.Where(p => p.Value != value);
        public static IEnumerable<Cell> EmptyCells(this IEnumerable<Cell> cells) => cells.CellsWithValue(CellValue.Empty);
        public static IEnumerable<Cell> FilledCells(this IEnumerable<Cell> cells) => cells.CellsWithoutValue(CellValue.Empty);
        public static bool IsEmpty(this IEnumerable<Cell> cells) => cells.FilledCells().CountEQ(0);
        public static bool IsFilled(this IEnumerable<Cell> cells) => cells.EmptyCells().CountEQ(0);
        public static bool IsEmpty(this Cell cell) => cell.Value == CellValue.Empty;
        public static bool IsFilled(this Cell cell) => cell.Value != CellValue.Empty;

        public static IEnumerable<TSelf> GetVerticalSiblings<TSelf>(this TSelf self) where TSelf : IHasGridSiblings<TSelf>
        {
            for(var upper = self.UpperSibling; upper != null; upper = upper.UpperSibling)
                yield return upper;
            for (var lower = self.LowerSibling; lower != null; lower = lower.LowerSibling)
                yield return lower;
        }

        public static IEnumerable<TSelf> GetHorizontalSiblings<TSelf>(this TSelf self) where TSelf : IHasGridSiblings<TSelf>
        {
            for (var left = self.LeftSibling; left != null; left = left.LeftSibling)
                yield return left;
            for (var right = self.RightSibling; right != null; right = right.RightSibling)
                yield return right;
        }

        public static IEnumerable<TSelf> GetAffectingSiblings<TSelf>(this TSelf self) where TSelf : IHasGridSiblings<TSelf>
        {
            for (var upper = self.UpperSibling; upper != null; upper = upper.UpperSibling)
                yield return upper;
            for (var lower = self.LowerSibling; lower != null; lower = lower.LowerSibling)
                yield return lower;
            for (var left = self.LeftSibling; left != null; left = left.LeftSibling)
                yield return left;
            for (var right = self.RightSibling; right != null; right = right.RightSibling)
                yield return right;
        }
    }
}
