using SudokuSolver.BoardData;
using System;
using System.Collections.Generic;
using System.Text;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver.Navigators
{
    public class Column : RowColumn<Column>
    {
        public readonly int ColumnNo;
        protected override object[] GetEquateableObjects() => new object[] { Board, ColumnNo };
        public override Column NextSibling => Board.GetColumn(ColumnNo + 1);
        public override Column PreviousSibling => Board.GetColumn(ColumnNo - 1);
        public override Cell this[int row] => Board.GetCell(row, ColumnNo);

        public Column(Board board, int columnNo) : base(board)
        {
            ColumnNo = columnNo;
        }

        public override IEnumerator<Cell> GetEnumerator()
        {
            for (int row = 0; row < Board.SIZE; row += 1)
                yield return this[row];
        }

        public override string ToString() => $"Column({ColumnNo})";
    }
}
