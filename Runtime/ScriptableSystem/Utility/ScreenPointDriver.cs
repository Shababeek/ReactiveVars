using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Writes the screen-space position of a world object to a Vector2Variable or Vector3Variable.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Drivers/Screen Point Driver")]
    public class ScreenPointDriver : VariableDriver<ScriptableVariable>
    {
        [Tooltip("The camera to use for projection. Defaults to Camera.main if not set.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("The transform whose world position is projected to screen space.")]
        [SerializeField] private Transform targetTransform;

        [Tooltip("Vector3Variable to receive the screen position (x, y, depth).")]
        [SerializeField] private Vector3Variable screenPositionV3;

        [Tooltip("Vector2Variable to receive the screen position (x, y only).")]
        [SerializeField] private Vector2Variable screenPositionV2;

        [Tooltip("BoolVariable set to true when the object is in front of the camera.")]
        [SerializeField] private BoolVariable isOnScreen;

        [Tooltip("Use viewport coordinates (0-1) instead of pixel coordinates.")]
        [SerializeField] private bool useViewportSpace;

        protected override ScriptableVariable Variable => screenPositionV3 != null
            ? screenPositionV3
            : screenPositionV2;

        protected override void OnEnable()
        {
            if (targetTransform == null)
                targetTransform = transform;

            if (targetCamera == null)
                targetCamera = Camera.main;

            base.OnEnable();

            if (targetCamera == null)
                Debug.LogWarning($"ScreenPointDriver on {gameObject.name}: No camera found.", this);
        }

        private void LateUpdate()
        {
            if (targetCamera == null || targetTransform == null) return;

            Vector3 worldPos = targetTransform.position;
            Vector3 screenPos = useViewportSpace
                ? targetCamera.WorldToViewportPoint(worldPos)
                : targetCamera.WorldToScreenPoint(worldPos);

            bool inFront = screenPos.z > 0f;

            if (screenPositionV3 != null)
            {
                if (SilentUpdates)
                    screenPositionV3.SetValueWithoutNotify(screenPos);
                else
                    screenPositionV3.Value = screenPos;
            }

            if (screenPositionV2 != null)
            {
                var pos2D = new Vector2(screenPos.x, screenPos.y);
                if (SilentUpdates)
                    screenPositionV2.SetValueWithoutNotify(pos2D);
                else
                    screenPositionV2.Value = pos2D;
            }

            if (isOnScreen != null)
            {
                bool onScreen;
                if (useViewportSpace)
                    onScreen = inFront && screenPos.x is >= 0f and <= 1f && screenPos.y is >= 0f and <= 1f;
                else
                    onScreen = inFront && screenPos.x >= 0f && screenPos.x <= Screen.width
                               && screenPos.y >= 0f && screenPos.y <= Screen.height;

                if (SilentUpdates)
                    isOnScreen.SetValueWithoutNotify(onScreen);
                else
                    isOnScreen.Value = onScreen;
            }
        }
    }
}
