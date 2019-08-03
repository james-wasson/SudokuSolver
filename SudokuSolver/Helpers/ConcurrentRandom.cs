using System;
using System.Collections.Generic;
using System.Text;

namespace SudokuSolver.Helpers
{
    public static class ConcurrentRandom
    {
        private static readonly Random _global = new Random();
        [ThreadStatic] private static Random _local;

        private static Random getLocal()
        {
            if (_local is null)
            {
                lock (_global)
                {
                    if (_local is null)
                    {
                        int seed = _global.Next();
                        _local = new Random(seed);
                    }
                }
            }
            return _local;
        }
        public static int Next() => getLocal().Next();
        public static int Next(int maxValue) => getLocal().Next(maxValue);
        public static int Next(int minValue, int maxValue) => getLocal().Next(minValue, maxValue);
        public static void NextBytes(byte[] buffer) => getLocal().NextBytes(buffer);
        public static void NextBytes(Span<byte> buffer) => getLocal().NextBytes(buffer);
        public static double NextDouble() => getLocal().NextDouble();
    }
}
