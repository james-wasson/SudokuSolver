using SudokuSolver.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Helpers
{
    public abstract class DeepEquals<TSelf> : IEquatable<TSelf>, IEquatable<DeepEquals<TSelf>> where TSelf : DeepEquals<TSelf>
    {
        private readonly int _initial;
        private readonly int _shift;
        public DeepEquals()
        {
            int typeHashCode = GetType().GetHashCode();
            // mod by some prime to make it small enough
            _initial = typeHashCode % 29;
            _shift = typeHashCode % 31;
        }

        protected abstract object[] GetEquateableObjects();

        public override int GetHashCode() => GetEquateableObjects().GetRealHashCode(_initial, _shift);
        public override bool Equals(object obj) => this.Equals(obj as DeepEquals<TSelf>);
        public bool Equals(TSelf other) => this == other;
        public bool Equals(DeepEquals<TSelf> other) => this == other;
        public static bool operator !=(DeepEquals<TSelf> obj1, DeepEquals<TSelf> obj2) => !(obj1 == obj2);
        public static bool operator ==(DeepEquals<TSelf> obj1, DeepEquals<TSelf> obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;
            if (ReferenceEquals(null, obj1) || ReferenceEquals(null, obj2))
                return false;
            return obj1.GetEquateableObjects().SequenceEqual(obj2.GetEquateableObjects());
        }
    }
}
