using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuSolver.Interfaces
{
    public interface IHasGridSiblings<TSelf> where TSelf : IHasGridSiblings<TSelf>
    {
        TSelf LeftSibling { get; }
        TSelf RightSibling { get; }
        TSelf UpperSibling { get; }
        TSelf LowerSibling { get; }
    }
}
