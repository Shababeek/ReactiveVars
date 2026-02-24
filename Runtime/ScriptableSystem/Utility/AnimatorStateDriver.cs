using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Reads an Animator parameter each frame and writes it to a ScriptableVariable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Animator State Driver")]
    public class AnimatorStateDriver : MonoBehaviour
    {
        public enum ParameterType
        {
            Float,
            Int,
            Bool
        }

        [Tooltip("When true, updates variables without raising change events.")]
        [SerializeField] private bool silentUpdates;

        [Tooltip("The name of the Animator parameter to read.")]
        [SerializeField] private string parameterName;

        [Tooltip("The type of the Animator parameter.")]
        [SerializeField] private ParameterType parameterType = ParameterType.Float;

        [Header("Output Variables (assign one matching the parameter type)")]
        [Tooltip("FloatVariable for float parameters.")]
        [SerializeField] private FloatVariable floatVariable;

        [Tooltip("IntVariable for int parameters.")]
        [SerializeField] private IntVariable intVariable;

        [Tooltip("BoolVariable for bool parameters.")]
        [SerializeField] private BoolVariable boolVariable;

        private Animator _animator;
        private int _parameterHash;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
                Debug.LogWarning($"AnimatorStateDriver requires an Animator on {gameObject.name}", this);
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                Debug.LogWarning($"Parameter name is not set on {gameObject.name}", this);
                return;
            }

            _parameterHash = Animator.StringToHash(parameterName);
        }

        private void Update()
        {
            if (_animator == null || !_animator.isActiveAndEnabled) return;

            switch (parameterType)
            {
                case ParameterType.Float:
                    WriteFloat();
                    break;
                case ParameterType.Int:
                    WriteInt();
                    break;
                case ParameterType.Bool:
                    WriteBool();
                    break;
            }
        }

        private void WriteFloat()
        {
            if (floatVariable == null) return;
            float value = _animator.GetFloat(_parameterHash);
            if (silentUpdates)
                floatVariable.SetValueWithoutNotify(value);
            else
                floatVariable.Value = value;
        }

        private void WriteInt()
        {
            if (intVariable == null) return;
            int value = _animator.GetInteger(_parameterHash);
            if (silentUpdates)
                intVariable.SetValueWithoutNotify(value);
            else
                intVariable.Value = value;
        }

        private void WriteBool()
        {
            if (boolVariable == null) return;
            bool value = _animator.GetBool(_parameterHash);
            if (silentUpdates)
                boolVariable.SetValueWithoutNotify(value);
            else
                boolVariable.Value = value;
        }
    }
}
