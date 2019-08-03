using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuSolver.Interfaces
{
    public interface IHasSiblings<TSelf> where TSelf : IHasSiblings<TSelf>
    {
        TSelf NextSibling { get; }
        TSelf PreviousSibling { get; }
    }
}
