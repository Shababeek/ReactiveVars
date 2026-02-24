using System;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Tweens a float value toward a target using linear interpolation.
    /// Supports rate-based or duration-based tweening with an optional AnimationCurve.
    /// </summary>
    public class TweenableFloat : ITweenable
    {
        /// <summary>Fired each frame with the current interpolated value.</summary>
        public event Action<float> OnChange;

        /// <summary>Fired once when the tween reaches its target.</summary>
        public event Action OnFinished;

        private float _start;
        private float _target;
        private float _value;
        private float _rate;
        private float _t;
        private AnimationCurve _curve;
        private readonly VariableTweener _tweener;

        /// <summary>Gets or sets the target value. Setting starts a new tween from the current value.</summary>
        public float Value
        {
            get => _value;
            set
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    _value = value;
                    OnChange?.Invoke(value);
                    return;
                }
#endif
                _t = 0;
                _start = _value;
                _target = value;
                _tweener.AddTweenable(this);
            }
        }

        /// <summary>Creates a rate-based TweenableFloat. Rate controls how fast t progresses per second (after tweenScale).</summary>
        public TweenableFloat(VariableTweener tweener, Action<float> onChange = null,
            float rate = 2f, float value = 0f, AnimationCurve curve = null)
        {
            _start = _target = _value = value;
            _rate = rate;
            _curve = curve;
            _t = 0;
            OnChange = onChange;
            _tweener = tweener;
        }

        /// <summary>Replaces the easing curve at runtime. Null reverts to linear.</summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
        }

        /// <summary>Advances the tween by one step. Returns true when complete.</summary>
        public bool Tween(float scaledDeltaTime)
        {
            _t += _rate * scaledDeltaTime;

            float eval = _curve != null ? _curve.Evaluate(Mathf.Clamp01(_t)) : _t;
            _value = Mathf.Lerp(_start, _target, eval);
            OnChange?.Invoke(_value);

            if (_t < 1f) return false;

            OnFinished?.Invoke();
            return true;
        }
    }
}
