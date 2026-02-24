using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Writes a Transform's position, rotation, and/or scale to ScriptableVariables each frame.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Transform Driver")]
    public class TransformDriver : MonoBehaviour
    {
        [Tooltip("When true, updates variables without raising change events.")]
        [SerializeField] private bool silentUpdates;

        [Tooltip("Use local space instead of world space.")]
        [SerializeField] private bool useLocalSpace;

        [Header("Position")]
        [Tooltip("Optional Vector3Variable to receive the position.")]
        [SerializeField] private Vector3Variable positionVariable;

        [Header("Rotation")]
        [Tooltip("Optional QuaternionVariable to receive the rotation.")]
        [SerializeField] private QuaternionVariable rotationVariable;

        [Tooltip("Optional Vector3Variable to receive the euler angles.")]
        [SerializeField] private Vector3Variable eulerAnglesVariable;

        [Header("Scale")]
        [Tooltip("Optional Vector3Variable to receive the local scale.")]
        [SerializeField] private Vector3Variable scaleVariable;

        private void Update()
        {
            WritePosition();
            WriteRotation();
            WriteScale();
        }

        private void WritePosition()
        {
            if (positionVariable == null) return;
            Vector3 pos = useLocalSpace ? transform.localPosition : transform.position;
            if (silentUpdates)
                positionVariable.SetValueWithoutNotify(pos);
            else
                positionVariable.Value = pos;
        }

        private void WriteRotation()
        {
            if (rotationVariable != null)
            {
                Quaternion rot = useLocalSpace ? transform.localRotation : transform.rotation;
                if (silentUpdates)
                    rotationVariable.SetValueWithoutNotify(rot);
                else
                    rotationVariable.Value = rot;
            }

            if (eulerAnglesVariable != null)
            {
                Vector3 euler = useLocalSpace ? transform.localEulerAngles : transform.eulerAngles;
                if (silentUpdates)
                    eulerAnglesVariable.SetValueWithoutNotify(euler);
                else
                    eulerAnglesVariable.Value = euler;
            }
        }

        private void WriteScale()
        {
            if (scaleVariable == null) return;
            if (silentUpdates)
                scaleVariable.SetValueWithoutNotify(transform.localScale);
            else
                scaleVariable.Value = transform.localScale;
        }
    }
}
