using SudokuSolver.BoardData;
using SudokuSolver.Helpers;
using SudokuSolver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver.Navigators
{
    public class Square : CellNavigator<Square>, IHasGridSiblings<Square>
    {
        public readonly int RowNo;
        public readonly int ColumnNo;
        protected override object[] GetEquateableObjects() => new object[] { Board, RowNo, ColumnNo };
        public Cell this[int rowOffset, int columnOffset] => Board.GetCell(RowNo * SQUARE_SIZE + rowOffset, ColumnNo * SQUARE_SIZE + columnOffset);
        /**
         * If called in a loop sistuation this produces correct results
         */
        public override Cell this[int index] => this[index / SQUARE_SIZE, index % SQUARE_SIZE];
        public Square LeftSibling => Board.GetSquare(RowNo, ColumnNo - 1);
        public Square RightSibling => Board.GetSquare(RowNo, ColumnNo + 1);
        public Square UpperSibling => Board.GetSquare(RowNo - 1, ColumnNo);
        public Square LowerSibling => Board.GetSquare(RowNo + 1, ColumnNo);

        public Square(Board board, int rowNo, int columnNo) : base(board)
        {
            RowNo = rowNo;
            ColumnNo = columnNo;
        }

        public override IEnumerator<Cell> GetEnumerator()
        {
            foreach (var index in EnumerableHelper.GridRange(Board.SQUARE_SIZE))
                yield return this[index.Row, index.Column];
        }

        public override string ToString() => $"Square({RowNo},{ColumnNo})";
    }
}
