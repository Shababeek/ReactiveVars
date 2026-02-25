using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Binds a SpriteVariable to a SpriteRenderer's sprite property.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Binders/Sprite Renderer Binder")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererBinder : VariableBinder<SpriteVariable>
    {
        [Tooltip("The SpriteVariable to bind.")]
        [SerializeField] private SpriteVariable variable;

        private SpriteRenderer _renderer;

        protected override SpriteVariable Variable => variable;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        protected override void Bind()
        {
            if (_renderer != null && Variable != null)
                _renderer.sprite = Variable.Value;
        }

        protected override void OnVariableChanged()
        {
            if (_renderer != null && Variable != null)
                _renderer.sprite = Variable.Value;
        }
    }
}
