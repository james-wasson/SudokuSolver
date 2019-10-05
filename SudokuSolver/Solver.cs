using SudokuSolver.BoardData;
using SudokuSolver.Extensions;
using SudokuSolver.Helpers;
using SudokuSolver.Navigators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver
{
    public class Solver
    {
        public static readonly IReadOnlyList<CellValue> ValidCellValues = EnumHelper.GetValues<CellValue>().WhereNot(CellValue.Empty).ToList().AsReadOnly();
        public readonly Board Board = null;
        private Dictionary<CellValue, bool[,]> CanGoStore = new Dictionary<CellValue, bool[,]>();
        public DebugOptionFlags DebugOptions { get; set; } = DebugOptionFlags.LOG_STEPS;
        private List<(string StepName, bool AnyChanged, TimeSpan Elapsed, int Iteration)> _steps = new List<(string, bool, TimeSpan, int)>();
        public IReadOnlyList<(string StepName, bool AnyChanged, TimeSpan Elapsed, int Iteration)> Steps => _steps.AsReadOnly();
        private int solveIteration = 0;

        public Solver(Board board, DebugOptionFlags debugOptions) : this(board)
        {
            this.DebugOptions = debugOptions;
        }

        public Solver(Board board)
        {
            Board = board.TypedClone();
            // fill CanGoDictionary with defaults
            foreach (var cellValue in ValidCellValues)
                CanGoStore.Add(cellValue, new bool[SIZE, SIZE].Map(true));
        }

        private bool PerformActionAndLog(Func<bool> action)
        {
            if (DebugOptions.HasFlag(DebugOptionFlags.LOG_STEPS))
            {
                var resultAndTime = action.InvokeAndTime();
                _steps.Add((action.Method.Name, resultAndTime.Result, resultAndTime.Elapsed, solveIteration));
                return resultAndTime.Result;
            }
            else
            {
                return action.Invoke();
            }
        }

        public Board Solve()
        {
            _steps.RemoveAll();
            solveIteration = 0;
            while (true)
            {
                if (Board.IsFilled())
                    break;
                solveIteration += 1;
                if (PerformActionAndLog(FillLastInGroup))
                    continue;
                if (PerformActionAndLog(FillSquareLineByCellValue))
                    continue;
                if (PerformActionAndLog(PointingPairs))
                    continue;
                if (PerformActionAndLog(FillBoardByCellValue))
                    continue;
                if (PerformActionAndLog(FillBoardByLastTicks))
                    continue;
                if (PerformActionAndLog(FillNaked))
                    continue;
                if (PerformActionAndLog(FillHidden))
                    continue;
                break;
            }
            return Board;
        }

        private IEnumerable<Cell> QueryCanGoStore(CellValue cellValue, bool emptyCheck = true)
            => Board.Where(cell => QueryCanGoStore(cellValue, cell, emptyCheck));
        private IEnumerable<Cell> QueryCanGoStore(CellValue cellValue, IEnumerable<Cell> cells, bool emptyCheck = true)
            => cells.Where(cell => QueryCanGoStore(cellValue, cell, emptyCheck));
        private IEnumerable<CellValue> QueryCanGoStore(Cell cell, bool emptyCheck = true)
            => ValidCellValues.Where(cellValue => QueryCanGoStore(cellValue, cell, emptyCheck));
        private bool QueryCanGoStore(CellValue cellValue, Cell cell, bool emptyCheck = true)
        {
            if (emptyCheck && cell.IsFilled()) return false; // slight speed improvement
            return CanGoStore[cellValue][cell.RowNo, cell.ColumnNo];
        }

        private bool SetCanGoStore(CellValue cellValue, Cell cell, bool canGo = false)
        {
            var previous = CanGoStore[cellValue][cell.RowNo, cell.ColumnNo];
            CanGoStore[cellValue][cell.RowNo, cell.ColumnNo] = canGo;
            return previous != canGo;
        }

        private bool FillBoardByLastTicks()
        {
            bool anyChanged = false;
            Board.CellsWithoutValue()
                .Select(cell => (Cell: cell, CellValues: ValidCellValues.Where(value => QueryCanGoStore(value, cell))))
                .Where(p => p.CellValues.CountEQ(1))
                .ForEach(p => {
                    anyChanged = ChangeCellValue(p.Cell, p.CellValues.First()) || anyChanged;
                });
            return anyChanged;
        }

        private bool FillBoardByCellValue()
        {
            bool anyChanged = false;
            foreach (var cellValue in ValidCellValues)
            {
                // information we need to learn this round
                var invalidRows = new HashSet<int>();
                var invalidColumns = new HashSet<int>();
                var invalidSquares = new HashSet<(int Row, int Column)>();
                foreach (var cell in QueryCanGoStore(cellValue))
                {
                    bool isGood = false;
                    // check row less than 2 constraint
                    if (!invalidRows.Contains(cell.RowNo)) // previous data check
                    {
                        if (QueryCanGoStore(cellValue, Board.GetRow(cell.RowNo)).CountGT(1))
                            invalidRows.Add(cell.RowNo);
                        else
                            isGood = true;
                    }
                    // check column less than 2 constraint
                    if (!invalidColumns.Contains(cell.ColumnNo)) // previous data check
                    {
                        if (QueryCanGoStore(cellValue, Board.GetColumn(cell.ColumnNo)).CountGT(1))
                            invalidColumns.Add(cell.ColumnNo);
                        else
                            isGood = true;
                    }

                    // check square less than 2 constraint
                    var square = cell.Square;
                    if (!invalidSquares.Contains((square.RowNo, square.ColumnNo))) // previous data check
                    {
                        if (QueryCanGoStore(cellValue, square).CountGT(1))
                            invalidSquares.Add((square.RowNo, square.ColumnNo));
                        else
                            isGood = true;
                    }

                    // now it is the only place this cellValue can go in these: row, column, square
                    if (isGood)
                        anyChanged = ChangeCellValue(cell, cellValue) || anyChanged;
                };
            }
            return anyChanged;
        }

        private bool ChangeCellValue(Cell cell, CellValue cellValue)
        {
            var anyChanged = cell.Value != cellValue;
            cell.Value = cellValue;
            // set so none can go in this square
            ValidCellValues.ForEach(p => SetCanGoStore(p, cell));
            // set so others (of same value) cannot go in this row
            cell.Row.ForEach(p => SetCanGoStore(cellValue, p));
            // set so others (of same value) cannot go in this column
            cell.Column.ForEach(p => SetCanGoStore(cellValue, p));
            // set so others (of same value) cannot go in this square
            cell.Square.ForEach(p => SetCanGoStore(cellValue, p));
            return anyChanged;
        }

        private bool FillHidden()
        {
            bool anyChanged = false;
            foreach (var grouping in Board.Groupings)
            {
                var cellGroupings = grouping.Select(cell => (CellValues: QueryCanGoStore(cell), Cell: cell))
                        .Where(p => p.CellValues.CountGT(1));
                foreach (var combo in ValidCellValues.GetCombinations(2, 3).Select(p => p.ToHashSet()))
                {
                    var posHidden = cellGroupings.Where(obj => combo.IsSubsetOf(obj.CellValues));
                    // Found a naked pair or triple
                    if (posHidden.CountEQ(combo.Count()))
                    {
                        ValidCellValues.WhereNot(combo)
                            .ForEach(value =>
                                posHidden.Select(p => p.Cell).ForEach(cell =>
                                    anyChanged = SetCanGoStore(value, cell) || anyChanged));
                    }
                    // immediately return because this is an expensive opperation
                    if (anyChanged)
                        return anyChanged;
                }
            }
            return anyChanged;
        }

        private bool FillNaked()
        {
            bool anyChanged = false;
            foreach (var grouping in Board.Groupings)
            {
                var cellGroupings = grouping.Select(cell => (CellValues: QueryCanGoStore(cell), Cell: cell))
                        .Where(p => p.CellValues.CountGT(1));
                foreach (var combo in ValidCellValues.GetCombinations(2, 3).Select(p => p.ToHashSet()))
                {
                    var posNaked = cellGroupings.Where(obj => combo.IsSupersetOf(obj.CellValues));
                    // Found a naked pair or triple
                    if (posNaked.CountEQ(combo.Count()))
                    {
                        grouping.WhereNot(posNaked.Select(p => p.Cell))
                            .ForEach(otherCell =>
                                combo.ForEach(value =>
                                    anyChanged = SetCanGoStore(value, otherCell) || anyChanged));
                    }
                    // immediately return because this is an expensive opperation
                    if (anyChanged)
                        return anyChanged;
                }
            }
            return anyChanged;
        }

        private bool FillSquareLineByCellValue()
        {
            bool anyChanged = false;
            foreach (var cellValue in ValidCellValues)
            {
                foreach (var square in Board.Squares)
                {
                    var query = QueryCanGoStore(cellValue, square);
                    var canGoCount = query.Count();
                    if (canGoCount == 2 || canGoCount == 3) // only cases this matters
                    {
                        var rowNos = query.Select(p => p.RowNo).ToHashSet();
                        if (rowNos.Count == 1)
                        {
                            Board.GetRow(rowNos.First())
                                .Where(cell => cell.Square != square)
                                .ForEach(cell => anyChanged = SetCanGoStore(cellValue, cell) || anyChanged);
                            continue;
                        }
                        var columnNos = query.Select(p => p.ColumnNo).ToHashSet();
                        if (columnNos.Count == 1)
                        {
                            Board.GetColumn(columnNos.First())
                                .Where(cell => cell.Square != square)
                                .ForEach(cell => anyChanged = SetCanGoStore(cellValue, cell) || anyChanged);
                            continue;
                        }
                    }
                }
            }
            return anyChanged;
        }

        private enum WhichDouble
        {
            GroupLeft = 2,
            GroupRight = 0,
            GroupSplit = 1,
            None = -1000
        }

        private WhichDouble FindWhichDouble(int rowA, int rowB)
        {
            rowA = rowA % SQUARE_SIZE;
            rowB = rowB % SQUARE_SIZE;
            rowA = Math.Min(rowA, rowB);
            rowB = Math.Max(rowA, rowB);
            if (rowA == 0 && rowB == 2) return WhichDouble.GroupSplit;
            if (rowA == 0 && rowB == 1) return WhichDouble.GroupLeft;
            if (rowA == 1 && rowB == 2) return WhichDouble.GroupRight;
            return WhichDouble.None;
        }

        private bool DoesntMatchWhichDouble(WhichDouble whichDouble, int rowNo)
        {
            rowNo = rowNo % SQUARE_SIZE;
            bool doesMatch = false;
            switch (whichDouble)
            {
                case WhichDouble.GroupLeft:
                    doesMatch = rowNo == 2;
                    break;
                case WhichDouble.GroupRight:
                    doesMatch = rowNo == 0;
                    break;
                case WhichDouble.GroupSplit:
                    doesMatch = rowNo == 1;
                    break;
            }
            return doesMatch;
        }

        private bool PointingPairs()
        {
            bool anyChanged = false;
            foreach (var cellValue in ValidCellValues)
            {
                var validSquares = QueryCanGoStore(cellValue)
                    .GroupBy(p => p.Square) // group by square
                    .ToDictionary(p => p.Key, p =>
                    {
                        var rowNos = p.Select(q => q.RowNo).ToHashSet();
                        var columnNos = p.Select(q => q.ColumnNo).ToHashSet();
                        var rowWhich = rowNos.Count == 2 ? FindWhichDouble(rowNos.First(), rowNos.Last()) : WhichDouble.None;
                        var colWhich = columnNos.Count == 2 ? FindWhichDouble(columnNos.First(), columnNos.Last()) : WhichDouble.None;
                        return (rowWhichDouble: rowWhich, colWhichDouble: colWhich);
                    });
                Action<bool> perform = (isRow) =>
                {
                    for (int sqrRowOrCol = 0; sqrRowOrCol < SQUARE_SIZE; sqrRowOrCol += 1)
                    {
                        var squares = validSquares.Where(p => (isRow ? p.Key.RowNo : p.Key.ColumnNo) == sqrRowOrCol);
                        var whichDoubles = squares.Select(p => isRow ? p.Value.rowWhichDouble : p.Value.colWhichDouble)
                            .Where(p => p != WhichDouble.None);
                        WhichDouble whichDouble = WhichDouble.None;
                        if (whichDoubles.Where(p => p == WhichDouble.GroupLeft).Count() == 2)
                            whichDouble = WhichDouble.GroupLeft;
                        else if (whichDoubles.Where(p => p == WhichDouble.GroupRight).Count() == 2)
                            whichDouble = WhichDouble.GroupRight;
                        else if (whichDoubles.Where(p => p == WhichDouble.GroupSplit).Count() == 2)
                            whichDouble = WhichDouble.GroupSplit;
                        if (whichDouble != WhichDouble.None)
                        {
                            squares.Where(p => (isRow ? p.Value.rowWhichDouble : p.Value.colWhichDouble) != whichDouble)
                                .Select(p => p.Key).SelectMany()
                                .Where(p => !DoesntMatchWhichDouble(whichDouble, isRow ? p.RowNo : p.ColumnNo))
                                .ForEach(cell => anyChanged = SetCanGoStore(cellValue, cell) || anyChanged);
                        }
                    }
                };
                perform.Invoke(true);
                perform.Invoke(false);
            }
            return anyChanged;
        }

        private bool FillLastInGroup()
        {
            bool anyChanged = false;
            foreach (var cellValue in ValidCellValues)
            {
                QueryCanGoStore(cellValue, false).ForEach(cell =>
                {
                    if (!CanGoSimple(cell, cellValue))
                    {
                        anyChanged = true;
                        SetCanGoStore(cellValue, cell);
                    }
                });
            }
            return anyChanged;
        }

        private static bool CanGoSimple(Cell cell, CellValue value)
        {
            if (cell.IsFilled())
                return false;
            if (cell.Row.CellsWithValue(value).Any())
                return false;
            if (cell.Column.CellsWithValue(value).Any())
                return false;
            if (cell.Square.CellsWithValue(value).Any())
                return false;
            return true;
        }

        public void PrintSteps()
        {
            var maxIteration = _steps.Select(p => p.Iteration).Max();
            object[,] array = new object[_steps.Count + maxIteration, 3];
            int previousIter = -1;
            int index = 0;
            _steps.OrderBy(p => p.Iteration)
                .ForEach((p) => {
                    if (previousIter < p.Iteration)
                    {
                        previousIter = p.Iteration;
                        array[index, 0] = "Iteration:";
                        array[index, 1] = p.Iteration;
                        index += 1;
                    }
                    array[index, 0] = p.StepName;
                    array[index, 1] = p.AnyChanged;
                    array[index, 2] = p.Elapsed.Milliseconds;
                    index += 1;
                });
            Console.WriteLine(array.ToArrayString());
        }

        public void PrintCanGoArrays()
        {
            var grid = new string[SIZE, SIZE];
            foreach (var value in ValidCellValues)
            {
                CanGoStore[value].ForEach((canGo, row, column) =>
                {
                    if (canGo)
                    {
                        if (grid[row, column] is null)
                            grid[row, column] = ((int)value).ToString();
                        else
                            grid[row, column] += "," + ((int)value).ToString();
                    }
                });
            }
            Console.WriteLine(grid.ToArrayString(SQUARE_SIZE));
        }

        public static bool IsValid(Cell cell, CellValue value)
        {
            if (cell.Row.Where(p => p != cell).CellsWithValue(value).Any())
                return false;
            if (cell.Column.Where(p => p != cell).CellsWithValue(value).Any())
                return false;
            if (cell.Square.Where(p => p != cell).CellsWithValue(value).Any())
                return false;
            return true;
        }

        public bool IsSolved()
        {
            return Board.IsFilled() && IsValid();
        }

        public bool IsValid()
        {
            return Board.FilledCells().All(p => IsValid(p, p.Value));
        }

        public IEnumerable<Cell> InvalidCells()
        {
            return Board.FilledCells().Where(p => !IsValid(p, p.Value));
        }

        [Flags]
        public enum DebugOptionFlags
        {
            NONE = 0x0,
            LOG_STEPS = 0x1,
        }
    }
}
