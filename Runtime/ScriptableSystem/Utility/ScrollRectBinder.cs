using UnityEngine;
using UnityEngine.UI;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a FloatVariable to a ScrollRect's normalized scroll position with two-way synchronization.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/ScrollRect Binder")]
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectBinder : TwoWayVariableBinder<FloatVariable>
    {
        [SerializeField] private FloatVariable scrollPosition;
        [SerializeField] private bool useHorizontal = false;

        private ScrollRect _scrollRect;

        protected override FloatVariable Variable => scrollPosition;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        protected override void UpdateUIFromVariable()
        {
            Vector2 pos = _scrollRect.normalizedPosition;
            if (useHorizontal)
                pos.x = scrollPosition.Value;
            else
                pos.y = scrollPosition.Value;
            _scrollRect.normalizedPosition = pos;
        }

        protected override void SubscribeToUI()
        {
            SubscribeUIEvent(_scrollRect.onValueChanged, value =>
                GuardedUpdate(() =>
                {
                    scrollPosition.Value = useHorizontal ? value.x : value.y;
                }));
        }
    }
}
