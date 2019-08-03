using SudokuSolver.BoardData;
using System;
using System.Collections.Generic;
using System.Text;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver.Navigators
{
    public class Row : RowColumn<Row>
    {
        public readonly int RowNo;
        protected override object[] GetEquateableObjects() => new object[] { Board, RowNo };
        public override Row NextSibling => Board.GetRow(RowNo + 1);
        public override Row PreviousSibling => Board.GetRow(RowNo - 1);
        public override Cell this[int column] => Board.GetCell(RowNo, column);

        public Row(Board board, int rowNo) : base(board)
        {
            RowNo = rowNo;
        }

        public override IEnumerator<Cell> GetEnumerator()
        {
            for (int column = 0; column < Board.SIZE; column += 1)
                yield return this[column];
        }

        public override string ToString() => $"Row({RowNo})";
    }
}
