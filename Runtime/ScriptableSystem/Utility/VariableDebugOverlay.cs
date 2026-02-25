using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shababeek.ReactiveVars
{
    /// <summary>
    /// Runtime debug overlay that displays ScriptableVariable values on screen.
    /// Toggle visibility with a configurable key. Add variables manually or auto-discover from scene.
    /// </summary>
    [AddComponentMenu("ReactiveVars/Utility/Variable Debug Overlay")]
    public class VariableDebugOverlay : MonoBehaviour
    {
        [Tooltip("Key to toggle the debug overlay visibility.")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        [Tooltip("Whether the overlay is visible on start.")]
        [SerializeField] private bool visibleOnStart = true;

        [Tooltip("Variables to watch. If empty, auto-discovers from scene binders and drivers.")]
        [SerializeField] private List<ScriptableVariable> watchedVariables = new();

        [Tooltip("Auto-discover variables from scene binders and drivers on enable.")]
        [SerializeField] private bool autoDiscover = true;

        [Header("Display Settings")]
        [Tooltip("Screen anchor position.")]
        [SerializeField] private ScreenAnchor anchor = ScreenAnchor.TopLeft;

        [Tooltip("Font size for the overlay text.")]
        [SerializeField] private int fontSize = 14;

        [Tooltip("Maximum number of variables to display.")]
        [SerializeField] private int maxDisplayCount = 30;

        [Tooltip("Background opacity (0-1).")]
        [SerializeField, Range(0f, 1f)] private float backgroundOpacity = 0.85f;

        public enum ScreenAnchor { TopLeft, TopRight, BottomLeft, BottomRight }

        private bool _visible;
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;
        private HashSet<ScriptableVariable> _allVariables = new();
        private Vector2 _scrollPosition;
        private string _searchFilter = "";

        private void OnEnable()
        {
            _visible = visibleOnStart;
            RefreshVariables();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                _visible = !_visible;
        }

        /// <summary>Refreshes the list of watched variables.</summary>
        public void RefreshVariables()
        {
            _allVariables.Clear();

            foreach (var v in watchedVariables)
            {
                if (v != null) _allVariables.Add(v);
            }

            if (autoDiscover)
                DiscoverSceneVariables();
        }

        /// <summary>Adds a variable to the watch list at runtime.</summary>
        public void AddVariable(ScriptableVariable variable)
        {
            if (variable != null) _allVariables.Add(variable);
        }

        /// <summary>Removes a variable from the watch list.</summary>
        public void RemoveVariable(ScriptableVariable variable)
        {
            _allVariables.Remove(variable);
        }

        private void DiscoverSceneVariables()
        {
            // Find all binders
            var binders = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in binders)
            {
                if (mb == null) continue;

                var fields = mb.GetType().GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);

                foreach (var field in fields)
                {
                    if (typeof(ScriptableVariable).IsAssignableFrom(field.FieldType))
                    {
                        var val = field.GetValue(mb) as ScriptableVariable;
                        if (val != null) _allVariables.Add(val);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!_visible) return;

            InitStyles();

            float width = 320;
            float height = Mathf.Min(Screen.height * 0.8f, 40 + _allVariables.Count * 22 + 60);
            Rect windowRect = GetAnchoredRect(width, height);

            GUI.Box(windowRect, GUIContent.none, _boxStyle);
            GUILayout.BeginArea(windowRect);

            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("Reactive Vars Debug", _headerStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("R", GUILayout.Width(24)))
                RefreshVariables();
            if (GUILayout.Button("X", GUILayout.Width(24)))
                _visible = false;
            GUILayout.EndHorizontal();

            // Search
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            _searchFilter = GUILayout.TextField(_searchFilter);
            GUILayout.EndHorizontal();

            // Variables list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            int displayed = 0;
            var sorted = _allVariables
                .Where(v => v != null)
                .OrderBy(v => v.name);

            foreach (var variable in sorted)
            {
                if (displayed >= maxDisplayCount) break;

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !variable.name.Contains(_searchFilter, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                DrawVariable(variable);
                displayed++;
            }

            GUILayout.EndScrollView();

            GUILayout.Label($"{_allVariables.Count} variables | Press {toggleKey} to toggle",
                _labelStyle);

            GUILayout.EndArea();
        }

        private void DrawVariable(ScriptableVariable variable)
        {
            GUILayout.BeginHorizontal();

            string typeName = variable.GetType().Name.Replace("Variable", "");
            string valueStr = variable.GetValue()?.ToString() ?? "null";

            // Color code by type
            Color typeColor = typeName switch
            {
                "Float" or "Int" or "Double" => new Color(0.5f, 0.8f, 1f),
                "Bool" => new Color(1f, 0.7f, 0.3f),
                "Text" => new Color(0.7f, 1f, 0.7f),
                "Vector2" or "Vector3" or "Quaternion" => new Color(1f, 0.6f, 0.8f),
                "Color" => new Color(1f, 1f, 0.5f),
                _ => Color.white
            };

            var originalColor = GUI.color;
            GUI.color = typeColor;
            GUILayout.Label($"[{typeName}]", _labelStyle, GUILayout.Width(80));
            GUI.color = originalColor;

            GUILayout.Label(variable.name, _labelStyle, GUILayout.Width(120));
            GUILayout.Label(valueStr, _labelStyle);

            GUILayout.EndHorizontal();
        }

        private Rect GetAnchoredRect(float width, float height)
        {
            float margin = 10f;
            return anchor switch
            {
                ScreenAnchor.TopLeft => new Rect(margin, margin, width, height),
                ScreenAnchor.TopRight => new Rect(Screen.width - width - margin, margin, width, height),
                ScreenAnchor.BottomLeft => new Rect(margin, Screen.height - height - margin, width, height),
                ScreenAnchor.BottomRight => new Rect(Screen.width - width - margin, Screen.height - height - margin, width, height),
                _ => new Rect(margin, margin, width, height)
            };
        }

        private void InitStyles()
        {
            if (_labelStyle != null) return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                normal = { textColor = Color.white }
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize + 2,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.12f, backgroundOpacity));
            bgTex.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTex },
                padding = new RectOffset(8, 8, 8, 8)
            };
        }
    }
}
