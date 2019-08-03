using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SudokuSolver.Extensions
{
    public static class FuncExtensions
    {
        public static TimeSpan InvokeAndTime<T1, T2>(this Action<T1, T2> func, T1 arg1, T2 arg2)
            => InvokeAndTime(func as Delegate, arg1, arg2);
        public static TimeSpan InvokeAndTime<T1>(this Action<T1> func, T1 arg1)
            => InvokeAndTime(func as Delegate, arg1);
        public static TimeSpan InvokeAndTime(this Action func)
            => InvokeAndTime(func as Delegate);
        public static TimeSpan InvokeAndTime(Delegate func, params object[] args)
        {
            var watch = Stopwatch.StartNew();
            func.DynamicInvoke(args);
            watch.Stop();
            return watch.Elapsed;
        }

        public static (TimeSpan Elapsed, TResult Result) InvokeAndTime<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 arg1, T2 arg2)
            => InvokeAndTime<TResult>(func as Delegate, arg1, arg2);
        public static (TimeSpan Elapsed, TResult Result) InvokeAndTime<T1, TResult>(this Func<T1, TResult> func, T1 arg1)
            => InvokeAndTime<TResult>(func as Delegate, arg1);
        public static (TimeSpan Elapsed, TResult Result) InvokeAndTime<TResult>(this Func<TResult> func)
            => InvokeAndTime<TResult>(func as Delegate);
        public static (TimeSpan Elapsed, TResult Result) InvokeAndTime<TResult>(Delegate func, params object[] args)
        {
            var watch = Stopwatch.StartNew();
            var result = func.DynamicInvoke(args);
            watch.Stop();
            var convertedResult = (TResult)Convert.ChangeType(result, typeof(TResult));
            return (watch.Elapsed, convertedResult);
        }
    }
}
