using System;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Tweens any INumericalVariable's value toward a target float.
    /// Pushes interpolated values directly into the variable via SetFromFloat each frame.
    /// </summary>
    public class TweenableNumerical : ITweenable
    {
        /// <summary>Fired once when the tween reaches its target.</summary>
        public event Action OnFinished;

        private readonly INumericalVariable _variable;
        private readonly VariableTweener _tweener;
        private float _start;
        private float _target;
        private float _rate;
        private float _t;
        private AnimationCurve _curve;

        /// <summary>Gets or sets the target value. Setting starts a new tween from the variable's current value.</summary>
        public float Target
        {
            get => _target;
            set
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    _variable.SetFromFloat(value);
                    return;
                }
#endif
                _t = 0;
                _start = _variable.AsFloat;
                _target = value;
                _tweener.AddTweenable(this);
            }
        }

        /// <summary>Creates a TweenableNumerical that drives the given variable.</summary>
        public TweenableNumerical(INumericalVariable variable, VariableTweener tweener,
            float rate = 2f, AnimationCurve curve = null)
        {
            _variable = variable;
            _tweener = tweener;
            _rate = rate;
            _curve = curve;
            _t = 0;
            _start = _target = variable.AsFloat;
        }

        /// <summary>Replaces the easing curve at runtime. Null reverts to linear.</summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
        }

        /// <summary>Sets the tween speed.</summary>
        public void SetRate(float rate)
        {
            _rate = rate;
        }

        /// <summary>Advances the tween by one step. Returns true when complete.</summary>
        public bool Tween(float scaledDeltaTime)
        {
            _t += _rate * scaledDeltaTime;

            float eval = _curve != null ? _curve.Evaluate(Mathf.Clamp01(_t)) : _t;
            float value = Mathf.Lerp(_start, _target, eval);
            _variable.SetFromFloat(value);

            if (_t < 1f) return false;

            OnFinished?.Invoke();
            return true;
        }
    }
}
