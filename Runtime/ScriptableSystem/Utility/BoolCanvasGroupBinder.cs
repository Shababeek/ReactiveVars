using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a BoolVariable to a CanvasGroup's visibility with optional smooth fade.
    /// </summary>
    [AddComponentMenu("Shababeek/Scriptable System/Bool CanvasGroup Binder")]
    [RequireComponent(typeof(CanvasGroup))]
    public class BoolCanvasGroupBinder : VariableBinder<BoolVariable>
    {
        [SerializeField] private BoolVariable variable;

        [Header("Visibility")]
        [Tooltip("Alpha when the variable is true.")]
        [SerializeField] private float visibleAlpha = 1f;

        [Tooltip("Alpha when the variable is false.")]
        [SerializeField] private float hiddenAlpha = 0f;

        [SerializeField] private bool controlInteractable = true;
        [SerializeField] private bool controlBlocksRaycasts = true;

        [Header("Fade")]
        [SerializeField] private bool smoothFade = false;

        [Tooltip("Fade speed (alpha per second).")]
        [SerializeField] private float fadeSpeed = 5f;

        [Header("Options")]
        [SerializeField] private bool invert = false;

        private CanvasGroup _canvasGroup;
        private float _targetAlpha;

        protected override BoolVariable Variable => variable;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Bind()
        {
            ApplyState(GetEffectiveValue(), true);
        }

        protected override void OnVariableChanged()
        {
            ApplyState(GetEffectiveValue(), false);
        }

        private void Update()
        {
            if (!smoothFade) return;

            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
        }

        private bool GetEffectiveValue()
        {
            return invert ? !variable.Value : variable.Value;
        }

        private void ApplyState(bool visible, bool immediate)
        {
            _targetAlpha = visible ? visibleAlpha : hiddenAlpha;

            if (!smoothFade || immediate)
            {
                _canvasGroup.alpha = _targetAlpha;
            }

            if (controlInteractable)
                _canvasGroup.interactable = visible;

            if (controlBlocksRaycasts)
                _canvasGroup.blocksRaycasts = visible;
        }

        /// <summary>Sets visibility immediately, ignoring the variable.</summary>
        public void SetVisible(bool visible)
        {
            ApplyState(visible, true);
        }

        /// <summary>Forces a refresh from the current variable value.</summary>
        public void Refresh()
        {
            if (variable != null)
                ApplyState(GetEffectiveValue(), false);
        }
    }
}
