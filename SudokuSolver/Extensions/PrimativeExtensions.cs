using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuSolver.Extensions
{
    public static class PrimativeExtensions
    {
        public static bool IsEven(this int input) => input % 2 == 0;
        public static bool IsOdd(this int input) => input % 2 != 0;
    }
}
