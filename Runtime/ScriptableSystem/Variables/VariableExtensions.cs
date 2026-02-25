using System;
using UniRx;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Extension methods for reactive variable operations.
    /// </summary>
    public static class VariableExtensions
    {
        /// <summary>Returns an observable that fires only when the BoolVariable becomes true.</summary>
        public static IObservable<bool> WhenTrue(this BoolVariable variable)=> variable.OnValueChanged.Where(v => v);

        /// <summary>Returns an observable that fires only when the BoolVariable becomes false.</summary>
        public static IObservable<bool> WhenFalse(this BoolVariable variable) => variable.OnValueChanged.Where(v => !v);
        /// <summary>Only emits when the value actually changes (skips duplicate consecutive values).</summary>
        public static IObservable<T> Distinct<T>(this ScriptableVariable<T> variable) => variable.OnValueChanged.DistinctUntilChanged();

        /// <summary>Returns an observable that fires when a numerical variable exceeds the threshold.</summary>
        public static IObservable<Unit> WhenAbove(this INumericalVariable variable, float threshold)
        {
            if (variable is not ScriptableVariable sv) return Observable.Empty<Unit>();
            return sv.OnRaised.Where(_ => variable.AsFloat > threshold);
        }

        /// <summary>Returns an observable that fires when a numerical variable drops below the threshold.</summary>
        public static IObservable<Unit> WhenBelow(this INumericalVariable variable, float threshold)
        {
            if (variable is not ScriptableVariable sv) return Observable.Empty<Unit>();
            return sv.OnRaised.Where(_ => variable.AsFloat < threshold);
        }

        /// <summary>Returns an observable that fires when a numerical variable is within the given range.</summary>
        public static IObservable<Unit> WhenInRange(this INumericalVariable variable, float min, float max)
        {
            if (variable is not ScriptableVariable sv) return Observable.Empty<Unit>();
            return sv.OnRaised.Where(_ => variable.AsFloat >= min && variable.AsFloat <= max);
        }

        /// <summary>Returns an observable that emits the float value each time a numerical variable changes.</summary>
        public static IObservable<float> OnFloatChanged(this INumericalVariable variable)
        {
            if (variable is not ScriptableVariable sv) return Observable.Empty<float>();
            return sv.OnRaised.Select(_ => variable.AsFloat);
        }

        /// <summary>Throttles value change notifications to at most once per specified interval.</summary>
        public static IObservable<T> Throttled<T>(this ScriptableVariable<T> variable, TimeSpan interval) => variable.OnValueChanged.ThrottleFirst(interval);

     }
}
