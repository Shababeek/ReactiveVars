using System.Collections;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Sets a BoolVariable to true for a duration then resets to false. Common for cooldowns and invincibility frames.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Cooldown Driver")]
    public class CooldownDriver : VariableDriver<BoolVariable>
    {
        [Tooltip("The BoolVariable representing the cooldown state (true = on cooldown).")]
        [SerializeField] private BoolVariable variable;

        [Tooltip("Duration of the cooldown in seconds.")]
        [SerializeField] private float duration = 1f;

        [Tooltip("Optional FloatVariable to write the remaining cooldown time.")]
        [SerializeField] private FloatVariable remainingTimeVariable;

        [Tooltip("Use unscaled time (ignores Time.timeScale).")]
        [SerializeField] private bool useUnscaledTime;

        protected override BoolVariable Variable => variable;

        private Coroutine _cooldownRoutine;

        /// <summary>Starts the cooldown. If already on cooldown, restarts it.</summary>
        public void Trigger()
        {
            if (_cooldownRoutine != null)
                StopCoroutine(_cooldownRoutine);

            _cooldownRoutine = StartCoroutine(CooldownRoutine());
        }

        /// <summary>Cancels the cooldown immediately, setting the variable to false.</summary>
        public void Cancel()
        {
            if (_cooldownRoutine != null)
            {
                StopCoroutine(_cooldownRoutine);
                _cooldownRoutine = null;
            }

            SetBool(false);
            SetRemaining(0f);
        }

        private IEnumerator CooldownRoutine()
        {
            SetBool(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += delta;
                SetRemaining(Mathf.Max(0f, duration - elapsed));
                yield return null;
            }

            SetBool(false);
            SetRemaining(0f);
            _cooldownRoutine = null;
        }

        private void SetBool(bool value)
        {
            if (Variable == null) return;
            if (SilentUpdates)
                Variable.SetValueWithoutNotify(value);
            else
                Variable.Value = value;
        }

        private void SetRemaining(float value)
        {
            if (remainingTimeVariable == null) return;
            if (SilentUpdates)
                remainingTimeVariable.SetValueWithoutNotify(value);
            else
                remainingTimeVariable.Value = value;
        }

        private void OnDisable()
        {
            Cancel();
        }
    }
}
