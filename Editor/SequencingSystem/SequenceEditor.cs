using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Custom editor for Sequence that provides a reorderable list of steps.
    /// </summary>
    [CustomEditor(typeof(Sequence))]
    public class SequenceEditor : Editor
    {
        private ReorderableList _stepList;
        private Sequence sequence;

        // References panel state
        private List<Object> _usedBy      = new();
        private List<Object> _outgoingRefs = new();
        private bool   _showRefs   = true;
        private string _refsFilter = "";
        private Vector2 _refsScroll;

        private void OnEnable()
        {
            sequence = (Sequence)target;
            _stepList = new ReorderableList(serializedObject, serializedObject.FindProperty("steps"), true, true, true,
                true);
            _stepList.onAddCallback += OnAddCallback;
            _stepList.onRemoveCallback += OnRemoveCallback;
            _stepList.drawElementCallback += DrawElementCallback;
            _stepList.onReorderCallback += OnReorderCallback;

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

        private void OnReorderCallback(ReorderableList list)
        {
            serializedObject.ApplyModifiedProperties();
            for (var i = 0; i < sequence.Steps.Count; i++)
            {
                var obj = sequence.Steps[i];
                var semiIndex = obj.name.IndexOf('_');
                obj.name = $"{sequence.name}-{i+1}_{obj.name.Substring(semiIndex + 1)}";
            }

            var path = AssetDatabase.GetAssetPath(sequence);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (_stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue == null) return;
            var nameRect = rect;
            var objRect = nameRect;
            nameRect.width = rect.width / 3 - 2;
            objRect.width = rect.width * 2f / 3 - 2;
            objRect.x += nameRect.width + 4;
            var elementName = _stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;
            var semiIndex = elementName.IndexOf('_') + 1;
            elementName = elementName.Substring(semiIndex);
            var newName = EditorGUI.TextField(nameRect, elementName);
            if (newName != elementName)
            {
                _stepList.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name = $"{sequence.name}-{index}_{newName}";
                OnReorderCallback(_stepList);
            }

            EditorGUI.PropertyField(objRect, _stepList.serializedProperty.GetArrayElementAtIndex(index),
                new GUIContent());
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            var item = sequence.Steps[list.index];
            sequence.Steps.RemoveAt(list.index);

            if (item == null) return;
            AssetDatabase.RemoveObjectFromAsset(item);
            AssetDatabase.SaveAssets();
            OnReorderCallback(list);
        }

        private void OnAddCallback(ReorderableList list)
        {
            var path = AssetDatabase.GetAssetPath(sequence);
            var step = CreateInstance<Step>();
            var index = list.serializedProperty.arraySize;
            step.name = $"{sequence.name}-{index}_step";
            //list.serializedProperty.InsertArrayElementAtIndex(index);
            if (sequence.Steps == null)
            {
                sequence.Init();
            }
            sequence.Steps.Add(step);

            AssetDatabase.AddObjectToAsset(step, $"{path}");
            AssetDatabase.ImportAsset(path);
            serializedObject.ApplyModifiedProperties();
            //list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = step;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
            _stepList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Create Sequence in Scene"))
            {
                var obj = new GameObject(sequence.name);
                obj.AddComponent<SequenceBehaviour>().sequence = sequence;
                
                // Create a single StepEventListener with all steps
                var listenerObj = new GameObject($"{sequence.name}_StepListeners");
                var listener = listenerObj.AddComponent<StepEventListener>();
                
                // Initialize the step list
                listener.StepList = new List<StepEventListener.StepWithEvents>();
                
                // Add all steps to the single listener
                foreach (var step in sequence.Steps)
                {
                    listener.AddStep(step);
                }
                
                listenerObj.transform.parent = obj.transform;
            }

            EditorGUILayout.Space(8);
            SequenceSceneReferencesPanel.Draw(target, ref _usedBy, ref _outgoingRefs, ref _showRefs, ref _refsFilter, ref _refsScroll);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

            var resetColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Reset All Steps"))
            {
                sequence.Reset();
                EditorUtility.SetDirty(sequence);
            }
            GUI.backgroundColor = resetColor;

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(2);
                var text = sequence.Started ? "Restart Quest" : "Start Quest";
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(text)) sequence.Begin();

                if (sequence.Started)
                    if (GUILayout.Button("Next Step"))
                        sequence.CurrentStep.CompleteStep();
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Step Status", EditorStyles.miniBoldLabel);
                for (int i = 0; i < sequence.Steps.Count; i++)
                {
                    var step = sequence.Steps[i];
                    if (step == null) continue;

                    var isCurrent = sequence.Started && i == sequence.CurrentStepIndex;
                    var statusColor = step.StepStatus switch
                    {
                        SequenceStatus.Started   => new Color(0.4f, 1f, 0.4f),
                        SequenceStatus.Completed => new Color(0.5f, 0.5f, 0.5f),
                        _                        => Color.white,
                    };

                    var style = isCurrent ? EditorStyles.boldLabel : EditorStyles.label;
                    var semiIndex = step.name.IndexOf('_') + 1;
                    var displayName = semiIndex > 0 ? step.name.Substring(semiIndex) : step.name;

                    GUI.color = statusColor;
                    EditorGUILayout.LabelField($"  {i + 1}. {displayName}  [{step.StepStatus}]", style);
                    GUI.color = Color.white;
                }
            }
        }
    }
}