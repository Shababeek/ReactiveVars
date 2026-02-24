using System;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Tweens a Quaternion toward a target using spherical interpolation with optional AnimationCurve easing.
    /// </summary>
    public class TweenableQuaternion : ITweenable
    {
        /// <summary>Fired each frame with the current interpolated rotation.</summary>
        public event Action<Quaternion> OnChange;

        /// <summary>Fired once when the tween reaches its target.</summary>
        public event Action OnFinished;

        private Quaternion _start;
        private Quaternion _target;
        private Quaternion _value;
        private float _rate;
        private float _t;
        private AnimationCurve _curve;
        private readonly VariableTweener _tweener;

        /// <summary>Gets or sets the target rotation. Setting starts a new tween from the current rotation.</summary>
        public Quaternion Value
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

        /// <summary>Creates a TweenableQuaternion with the given rate and optional easing curve.</summary>
        public TweenableQuaternion(VariableTweener tweener, Action<Quaternion> onChange = null,
            float rate = 2f, Quaternion? value = null, AnimationCurve curve = null)
        {
            _start = _target = _value = value ?? Quaternion.identity;
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
            _value = Quaternion.Slerp(_start, _target, eval);
            OnChange?.Invoke(_value);

            if (_t < 1f) return false;

            OnFinished?.Invoke();
            return true;
        }
    }
}
