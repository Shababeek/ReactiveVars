using System;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Tweens a Transform's position and rotation toward a target transform.
    /// </summary>
    public class TransformTweenable : ITweenable
    {
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Transform _target;
        private Transform _transform;
        private float _time;
        private AnimationCurve _curve;

        /// <summary>Fired when the tween reaches the target.</summary>
        public event Action OnTweenComplete;

        /// <summary>Advances the tween by one step. Returns true when complete.</summary>
        public bool Tween(float scaledDeltaTime)
        {
            if (_transform == null || _target == null)
            {
                Debug.LogWarning("TransformTweenable: Transform or target is null.");
                return true;
            }

            _time += scaledDeltaTime;
            float eval = _curve != null ? _curve.Evaluate(Mathf.Clamp01(_time)) : _time;

            _transform.position = Vector3.Lerp(_startPosition, _target.position, eval);
            _transform.rotation = Quaternion.Slerp(_startRotation, _target.rotation, eval);

            if (_time < 1f) return false;

            OnTweenComplete?.Invoke();
            return true;
        }

        /// <summary>Sets up the tween between current transform state and a target.</summary>
        public void Initialize(Transform transform, Transform target, AnimationCurve curve = null)
        {
            if (transform == null)
            {
                Debug.LogError("TransformTweenable: Source transform cannot be null.");
                return;
            }

            if (target == null)
            {
                Debug.LogError("TransformTweenable: Target transform cannot be null.");
                return;
            }

            _transform = transform;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _target = target;
            _curve = curve;
            _time = 0;
        }
    }
}
