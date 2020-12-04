using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MK94.SeeRaw
{
    public static class SeeRawTypes
    {
        public static Progress Progress(Action<bool> pause = null, Action<int?> setSpeed = null, CancellationTokenSource cancellationTokenSource = null) =>
            new Progress(pause, setSpeed, cancellationTokenSource);

        public static Navigation Navigation() => new Navigation();

        public static Form Form(string text, Action<Dictionary<string, object>> callback) => new Form(text, callback);

        #region Action
        // Yes this looks wrong, but Action has 17 different generic variations and there doesn't seem to be a better way of supporting all of them

        public static Actionable Action(string text, Delegate action) => new Actionable(text, action);

        public static Actionable Action(string text, Action action) => new Actionable(text, action);

        public static Actionable Action<T1>
            (string text, Action<T1> action, T1 arg1 = default) => new Actionable(text, action, arg1);
        public static Actionable Action<T1, T2>
                    (string text, Action<T1, T2> action, T1 arg1 = default, T2 arg2 = default) => new Actionable(text, action, arg1, arg2);
        public static Actionable Action<T1, T2, T3>
                    (string text, Action<T1, T2, T3> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default) => new Actionable(text, action, arg1, arg2, arg3);
        public static Actionable Action<T1, T2, T3, T4>
                    (string text, Action<T1, T2, T3, T4> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4);
        public static Actionable Action<T1, T2, T3, T4, T5>
                    (string text, Action<T1, T2, T3, T4, T5> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5);
        public static Actionable Action<T1, T2, T3, T4, T5, T6>
                    (string text, Action<T1, T2, T3, T4, T5, T6> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default, T11 arg11 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default, T11 arg11 = default, T12 arg12 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default, T11 arg11 = default, T12 arg12 = default, T13 arg13 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default, T11 arg11 = default, T12 arg12 = default, T13 arg13 = default, T14 arg14 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default, T11 arg11 = default, T12 arg12 = default, T13 arg13 = default, T14 arg14 = default, T15 arg15 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
                    (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default, T10 arg10 = default, T11 arg11 = default, T12 arg12 = default, T13 arg13 = default, T14 arg14 = default, T15 arg15 = default, T16 arg16 = default) => new Actionable(text, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        #endregion
    }
}
