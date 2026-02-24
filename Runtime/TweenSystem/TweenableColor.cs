using System;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Tweens a Color toward a target using linear interpolation with optional AnimationCurve easing.
    /// </summary>
    public class TweenableColor : ITweenable
    {
        /// <summary>Fired each frame with the current interpolated color.</summary>
        public event Action<Color> OnChange;

        /// <summary>Fired once when the tween reaches its target.</summary>
        public event Action OnFinished;

        private Color _start;
        private Color _target;
        private Color _value;
        private float _rate;
        private float _t;
        private AnimationCurve _curve;
        private readonly VariableTweener _tweener;

        /// <summary>Gets or sets the target color. Setting starts a new tween from the current color.</summary>
        public Color Value
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

        /// <summary>Creates a TweenableColor with the given rate and optional easing curve.</summary>
        public TweenableColor(VariableTweener tweener, Action<Color> onChange = null,
            float rate = 2f, Color? value = null, AnimationCurve curve = null)
        {
            _start = _target = _value = value ?? Color.white;
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
            _value = Color.Lerp(_start, _target, eval);
            OnChange?.Invoke(_value);

            if (_t < 1f) return false;

            OnFinished?.Invoke();
            return true;
        }
    }
}
