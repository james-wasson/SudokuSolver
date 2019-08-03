using SudokuSolver.BoardData;
using SudokuSolver.Extensions;
using SudokuSolver.Navigators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using SudokuSolver.Helpers;
using static SudokuSolver.BoardData.Board;

namespace SudokuSolver.BoardData
{
    public partial class Board : IReadOnlyList<Cell>, ICloneable
    {
        public static readonly int MAX_VALUE = EnumHelper.GetValues<CellValue>().Select(p => (int)p).Max();
        public static readonly int SIZE = MAX_VALUE;
        public static readonly int SQUARE_SIZE = SIZE / 3;

        private CellValue[,] Grid = new CellValue[SIZE, SIZE];
        public int Count => SIZE * SIZE;
        /**
         * If called in a loop sistuation this produces correct results
         */
        public Cell this[int index] => this[index / Count, index % Count];
        public Cell this[int row, int column] => this.GetCell(row, column);
        public IEnumerable<CellValue> CellValues => this.Grid.Flatten();

        public Board() : this((CellValue?[])null) { }
        public Board(IEnumerable<CellValue> values) : this(values.Cast<CellValue?>().ToArray()) { }
        public Board(IEnumerable<CellValue?> values) : this(values.ToArray()) { }
        public Board(params CellValue[] values) : this(values.Cast<CellValue?>().ToArray()) { }
        public Board(params CellValue?[] values)
        {
            foreach (var index in EnumerableHelper.GridRange(SIZE).Indexed())
            {
                if (values == null || index.Index >= values.Length || !values[index.Index].HasValue)
                    Grid[index.Value.Row, index.Value.Column] = CellValue.Empty;
                else
                    Grid[index.Value.Row, index.Value.Column] = values[index.Index].Value;
            }
        }

        public IEnumerable<Row> Rows => Enumerable.Range(0, SIZE).Select(row => this.GetRow(row));
        public IEnumerable<Column> Columns => Enumerable.Range(0, SIZE).Select(column => this.GetColumn(column));
        public IEnumerable<Square> Squares => EnumerableHelper.GridRange(SQUARE_SIZE).Select(index => this.GetSquare(index.Row, index.Column));

        public void SetCellValue(CellValue value, int row, int column)
        {
            if (row >= Grid.Length || row < 0 || column >= Grid.Length || column < 0)
                throw new IndexOutOfRangeException("Cannot set cell value.");
            Grid[row, column] = value;
        }

        public CellValue GetCellValue(int row, int column)
        {
            if (row >= Grid.Length || row < 0 || column >= Grid.Length || column < 0)
                throw new IndexOutOfRangeException("Cannot get cell value.");
            return Grid[row, column];
        }

        public Cell GetCell(int row, int column)
        {
            if (row >= Grid.Length || row < 0 || column >= Grid.Length || column < 0)
                throw new IndexOutOfRangeException("Cannot get cell.");
            return new Cell(this, row, column);
        }

        public Row GetRow(int row)
        {
            if (row >= SIZE || row < 0)
                throw new IndexOutOfRangeException("Cannot get row.");
            return new Row(this, row);
        }

        public Column GetColumn(int column)
        {
            if (column >= SIZE || column < 0)
                throw new IndexOutOfRangeException("Cannot get column.");
            return new Column(this, column);
        }

        public Square GetSquareByRowColumn(int row, int column) => this.GetSquare(row / SQUARE_SIZE, column / SQUARE_SIZE);
        public Square GetSquare(int squareRow, int squareColumn)
        {
            if (squareRow >= SQUARE_SIZE || squareRow < 0 || squareColumn >= SQUARE_SIZE || squareColumn < 0)
                throw new IndexOutOfRangeException("Cannot get square.");
            return new Square(this, squareRow, squareColumn);
        }

        public bool BoardsAreEqual(object obj) => BoardsAreEqual(obj as Board);
        public bool BoardsAreEqual(Board other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (ReferenceEquals(null, other))
                return false;
            return this.CellValues.SequenceEqual(other.CellValues);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<Cell> GetEnumerator()
        {
            foreach (var index in EnumerableHelper.GridRange(SIZE))
                yield return new Cell(this, index.Row, index.Column);
        }

        public override string ToString() => this.ToString(true);
        public string ToString(bool includeSplitters) => Grid.Select(p => (int)p).ToArrayString(includeSplitters ? SQUARE_SIZE : -1);

        public object Clone() => new Board(this.CellValues);
        public Board TypedClone() => this.Clone() as Board;
    }
}
