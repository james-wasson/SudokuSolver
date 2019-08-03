using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using SudokuSolver.Interfaces;
using SudokuSolver.Navigators;
using SudokuSolver.Helpers;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver.BoardData
{
    public class Cell : DeepEquals<Cell>, IHasGridSiblings<Cell>
    {
        public readonly Board Board;
        public readonly int RowNo;
        public readonly int ColumnNo;
        protected override object[] GetEquateableObjects() => new object[] { Board, RowNo, ColumnNo };
        public CellValue Value {
            get => Board.GetCellValue(RowNo, ColumnNo);
            set => Board.SetCellValue(value, RowNo, ColumnNo);
        }
        public Cell LeftSibling => Board.GetCell(RowNo, ColumnNo - 1);
        public Cell RightSibling => Board.GetCell(RowNo, ColumnNo + 1);
        public Cell UpperSibling => Board.GetCell(RowNo - 1, ColumnNo);
        public Cell LowerSibling => Board.GetCell(RowNo + 1, ColumnNo);
        public Row Row => Board.GetRow(RowNo);
        public Column Column => Board.GetColumn(ColumnNo);
        public Square Square => Board.GetSquareByRowColumn(RowNo, ColumnNo);

        public Cell(Board board, int row, int column)
        {
            Board = board;
            RowNo = row;
            ColumnNo = column;
        }

        public void Deconstruct(out CellValue value, out int row, out int column)
        {
            value = Value;
            row = RowNo;
            column = ColumnNo;
        }

        public void Deconstruct(out int row, out int column)
        {
            row = RowNo;
            column = ColumnNo;
        }

        public override string ToString() => $"Cell({RowNo},{ColumnNo}) = {Value}";
    }
}
