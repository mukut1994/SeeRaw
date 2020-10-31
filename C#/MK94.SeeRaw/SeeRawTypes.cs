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

        #region Action
        // Yes this looks wrong, but Action has 17 different generic variations and there doesn't seem to be a better way of supporting all of them

        public static Actionable Action(string text, Delegate action) => new Actionable(text, action);

        public static Actionable Action(string text, Action action) => new Actionable(text, action); 
        public static Actionable Action<T1>
            (string text, Action<T1> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2>
            (string text, Action<T1, T2> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3>
            (string text, Action<T1, T2, T3> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4>
            (string text, Action<T1, T2, T3, T4> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5>
            (string text, Action<T1, T2, T3, T4, T5> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6>
            (string text, Action<T1, T2, T3, T4, T5, T6> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action) => new Actionable(text, action);
        public static Actionable Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
            (string text, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action) => new Actionable(text, action);
        #endregion
    }
}
