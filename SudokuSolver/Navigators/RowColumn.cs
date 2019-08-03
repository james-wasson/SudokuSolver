using SudokuSolver.BoardData;
using SudokuSolver.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuSolver.Navigators
{
    public abstract class RowColumn<TRowCol> : CellNavigator<TRowCol>, IHasSiblings<TRowCol> where TRowCol : RowColumn<TRowCol>
    {
        public RowColumn(Board board) : base(board) { }
        public abstract TRowCol NextSibling { get; }
        public abstract TRowCol PreviousSibling { get; }
    }
}
