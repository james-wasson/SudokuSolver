using SudokuSolver.BoardData;
using SudokuSolver.Extensions;
using SudokuSolver.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver.Navigators
{
    public abstract class CellNavigator<TSelf> : DeepEquals<TSelf>, IReadOnlyList<Cell> where TSelf : CellNavigator<TSelf>
    {
        public readonly Board Board;
        public int Count => Board.SIZE;
        public TSelf Key => (TSelf) this;
        public abstract Cell this[int index] { get; }
        public CellNavigator(Board board)
        {
            Board = board;
        }

        public abstract IEnumerator<Cell> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
