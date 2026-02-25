using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a BoolVariable to a SpriteRenderer's flipX or flipY property.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Sprite Renderer Flip Binder")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererFlipBinder : VariableBinder<BoolVariable>
    {
        public enum FlipAxis { X, Y }

        [Tooltip("The BoolVariable controlling the flip state.")]
        [SerializeField] private BoolVariable variable;

        [Tooltip("Which axis to flip.")]
        [SerializeField] private FlipAxis axis = FlipAxis.X;

        private SpriteRenderer _renderer;

        protected override BoolVariable Variable => variable;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        protected override void Bind()
        {
            ApplyFlip();
        }

        protected override void OnVariableChanged()
        {
            ApplyFlip();
        }

        private void ApplyFlip()
        {
            if (_renderer == null || Variable == null) return;
            if (axis == FlipAxis.X)
                _renderer.flipX = Variable.Value;
            else
                _renderer.flipY = Variable.Value;
        }
    }
}
