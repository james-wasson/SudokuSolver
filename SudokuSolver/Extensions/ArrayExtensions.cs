using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SudokuSolver.Helpers.EnumerableHelper;

namespace SudokuSolver.Extensions
{
    public static class ArrayExtensions
    {
        public static TOut[,] Select<TIn, TOut>(this TIn[,] matrix, Func<TIn, TOut> select) => matrix.Select((item, row, col) => select.Invoke(item));
        public static TOut[,] Select<TIn, TOut>(this TIn[,] matrix, Func<TIn, int, int, TOut> select)
        {
            int rowLen = matrix.GetLength(0);
            int colLen = matrix.GetLength(1);
            TOut[,] result = new TOut[rowLen, colLen];
            for (int row = 0; row < rowLen; row += 1)
            {
                for (int col = 0; col < colLen; col += 1)
                {
                    result[row, col] = select(matrix[row, col], row, col);
                }
            }
            return result;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this TSource[,] matrix, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => Aggregate(matrix, seed, (carry, source, row, col) => func.Invoke(carry, source));
        public static TAccumulate Aggregate<TSource, TAccumulate>(this TSource[,] matrix, TAccumulate seed, Func<TAccumulate, TSource, int, int, TAccumulate> func)
        {
            int rowLen = matrix.GetLength(0);
            int colLen = matrix.GetLength(1);
            for (int row = 0; row < rowLen; row += 1)
            {
                for (int col = 0; col < colLen; col += 1)
                {
                    seed = func(seed, matrix[row, col], row, col);
                }
            }
            return seed;
        }

        public static IEnumerable<T> Flatten<T>(this T[,] matrix) => (matrix as Array).Flatten<T>();
        public static IEnumerable<T> Flatten<T>(this Array matrix)
        {
            foreach (T item in matrix)
                yield return item;
        }

        public static void ForEach<T>(this T[,] matrix, Action<T> action)
        {
            foreach (var item in matrix)
                action.Invoke(item);
        }
        public static void ForEach<T>(this T[,] matrix, Action<T, int, int> action)
        {
            int rowLen = matrix.GetLength(0);
            int colLen = matrix.GetLength(1);
            for (int row = 0; row < rowLen; row += 1)
            {
                for (int col = 0; col < colLen; col += 1)
                {
                    action.Invoke(matrix[row, col], row, col);
                }
            }
        }

        public static T[,] Map<T>(this T[,] matrix, Func<int, int, T> valueSelector)
            => matrix.Map((previous, row, col) => valueSelector.Invoke(row, col));
        public static T[,] Map<T>(this T[,] matrix, Func<T, T> valueSelector)
            => matrix.Map((previous, row, col) => valueSelector.Invoke(previous));
        public static T[,] Map<T>(this T[,] matrix, Func<T> valueSelector)
            => matrix.Map((previous, row, col) => valueSelector.Invoke());
        public static T[,] Map<T>(this T[,] matrix, T value)
            => matrix.Map((previous, row, col) => value);
        public static T[,] Map<T>(this T[,] matrix, Func<T, int, int, T> valueSelector)
        {
            int rowLen = matrix.GetLength(0);
            int colLen = matrix.GetLength(1);
            for (int row = 0; row < rowLen; row += 1)
            {
                for (int col = 0; col < colLen; col += 1)
                {
                    matrix[row, col] = valueSelector.Invoke(matrix[row, col], row, col);
                }
            }
            return matrix;
        }

        public static string ToArrayString<T>(this T[,] matrix) => matrix.ToArrayString(-1);
        public static string ToArrayString<T>(this T[,] matrix, int splitRowColEvery, char rowColSplitter) => matrix.ToArrayString(splitRowColEvery, splitRowColEvery, rowColSplitter, rowColSplitter);
        public static string ToArrayString<T>(this T[,] matrix, int splitRowColEvery, char rowSplitter = '-', char colSplitter = '|') => matrix.ToArrayString(splitRowColEvery, splitRowColEvery, rowSplitter, colSplitter);
        public static string ToArrayString<T>(this T[,] matrix, int splitRowEvery, int splitColEvery, char rowSplitter = '-', char colSplitter = '|')
        {
            int rowLen = matrix.GetLength(0);
            int colLen = matrix.GetLength(1);

            bool hasRowSplitter = splitRowEvery > 0;
            bool hasColSplitter = splitColEvery > 0;

            string[,] stringMatrix = matrix.Select(p => p == null ? "" : p.ToString());
            var maxCharLengthByCol = stringMatrix.Aggregate(new Dictionary<int, int>(), (dict, val, row, col) =>
            {
                var max = dict.GetValueOrAdd(col, val.Length);
                if (max < val.Length) dict[col] = val.Length;
                return dict;
            });
            string colPadder = null;
            string rowPadder = null;
            if (hasColSplitter)
            {
                colPadder = ' ' + colSplitter.ToString() + ' ';
            }
            if (hasRowSplitter)
            {
                int rowCharLength = maxCharLengthByCol.Values.Select(p => p + 2).Sum(); // 2 padding chars
                if (hasColSplitter)
                {
                    // add total columns printed
                    rowCharLength += colLen / splitColEvery + 1; // + 1 for first column
                    rowPadder = ' ' + new string(rowSplitter, rowCharLength) + " \n";
                }
                else
                {
                    // minus 2 for each side
                    rowPadder = ' ' + new string(rowSplitter, rowCharLength - 2) + " \n";
                }
            }

            string result = "";
            for (int row = 0; row < rowLen; row += 1)
            {
                // first row splitter
                if (hasRowSplitter && row == 0) result += rowPadder;
                bool previousHasColSplitter = false;
                for (int col = 0; col < colLen; col += 1)
                {
                    if (hasColSplitter)
                    {
                        // first column splitter
                        if (col == 0)
                        {
                            previousHasColSplitter = true;
                            result += colPadder;
                        }
                        if (!previousHasColSplitter)
                            result += ' ';
                        result += stringMatrix[row, col].PadRight(maxCharLengthByCol[col], ' ');
                        // add column splitter
                        if ((col + 1) % splitColEvery == 0)
                        {
                            previousHasColSplitter = true;
                            result += colPadder;
                        }
                        else
                        {
                            result += ' ';
                            previousHasColSplitter = false;
                        }
                    }
                    else // no column splitter
                    {
                        result += ' ' + stringMatrix[row, col].PadRight(maxCharLengthByCol[col] + 1, ' ');
                        previousHasColSplitter = false;
                    }
                }
                result += '\n';
                // add row splitter
                if (hasRowSplitter && (row + 1) % splitRowEvery == 0) result += rowPadder;
            }
            return result;
        }
    }
}
