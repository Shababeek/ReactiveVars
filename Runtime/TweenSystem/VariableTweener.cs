using System.Collections.Generic;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Manages and updates a collection of ITweenable instances each frame.
    /// </summary>
    public class VariableTweener : MonoBehaviour
    {
        [Tooltip("Global speed multiplier applied to all managed tweenables.")]
        [SerializeField] private float _tweenScale = 12f;

        private readonly List<ITweenable> _tweenables = new();

        /// <summary>Gets or sets the global speed multiplier for all tweenables.</summary>
        public float TweenScale
        {
            get => _tweenScale;
            set => _tweenScale = value;
        }

        /// <summary>Number of tweenables currently being updated.</summary>
        public int ActiveCount => _tweenables.Count;

        private void OnEnable()
        {
            _tweenables.Clear();
        }

        /// <summary>Registers a tweenable for per-frame updates until it completes.</summary>
        public void AddTweenable(ITweenable value)
        {
            if (value == null)
            {
                Debug.LogWarning("VariableTweener: Attempted to add null tweenable.");
                return;
            }

            if (!_tweenables.Contains(value))
                _tweenables.Add(value);
        }

        /// <summary>Removes a tweenable, stopping its updates immediately.</summary>
        public void RemoveTweenable(ITweenable value)
        {
            if (value != null)
                _tweenables.Remove(value);
        }

        /// <summary>Removes all active tweenables.</summary>
        public void Clear()
        {
            _tweenables.Clear();
        }

        private void Update()
        {
            for (int i = _tweenables.Count - 1; i >= 0; i--)
            {
                if (_tweenables[i].Tween(Time.deltaTime * _tweenScale))
                {
                    _tweenables.RemoveAt(i);
                }
            }
        }
    }
}
