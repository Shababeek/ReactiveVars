using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Custom editor for Step ScriptableObjects.
    /// Shows default inspector fields plus a scene references panel.
    /// </summary>
    [CustomEditor(typeof(Step))]
    public class StepEditor : Editor
    {
        private List<Object> _usedBy       = new();
        private List<Object> _outgoingRefs = new();
        private bool   _showRefs   = true;
        private string _refsFilter = "";
        private Vector2 _refsScroll;

        private void OnEnable()
        {
            SequenceSceneReferencesPanel.FindAllReferences(target, _usedBy, _outgoingRefs);
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private bool _refsScanQueued;

        private void OnHierarchyChanged()
        {
            if (Application.isPlaying) return;
            if (_refsScanQueued) return;
            _refsScanQueued = true;
            EditorApplication.delayCall += () =>
            {
                _refsScanQueued = false;
                if (target == null) return;
                SequenceSceneReferencesPanel.FindAllReferences(target, _usedBy, _outgoingRefs);
                Repaint();
            };
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            SequenceSceneReferencesPanel.Draw(target, ref _usedBy, ref _outgoingRefs, ref _showRefs, ref _refsFilter, ref _refsScroll);

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

                var step = (Step)target;
                var statusColor = step.StepStatus switch
                {
                    SequenceStatus.Started   => new Color(0.3f, 1f, 0.3f),
                    SequenceStatus.Completed => new Color(0.55f, 0.55f, 0.55f),
                    _                        => Color.white,
                };

                GUI.color = statusColor;
                EditorGUILayout.LabelField($"Status: {step.StepStatus}", EditorStyles.boldLabel);
                GUI.color = Color.white;

                GUI.enabled = step.StepStatus == SequenceStatus.Started;
                if (GUILayout.Button("Complete Step"))
                    step.CompleteStep();
                GUI.enabled = true;
            }
        }
    }
}
