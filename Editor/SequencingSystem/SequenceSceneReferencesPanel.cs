using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Two-way reference panel for SequenceNode ScriptableObjects.
    ///   "Used by"    — scene MonoBehaviours AND SequenceNode assets that reference this object.
    ///   "References" — objects (assets / scene) that this object's fields point to.
    /// </summary>
    internal static class SequenceSceneReferencesPanel
    {
        private const float RowHeight = 20f;
        private const float DotW      = 14f;
        private const float ArrowW    = 24f;
        private const float TypeW     = 150f;

        // ── colours ───────────────────────────────────────────────────────────
        private static readonly Color ColEnabled  = new(0.25f, 0.85f, 0.35f);
        private static readonly Color ColDisabled = new(0.50f, 0.50f, 0.50f);
        private static readonly Color ColAsset    = new(0.55f, 0.75f, 1.00f);
        private static readonly Color ColRowEven  = new(0f,    0f,    0f,    0.08f);
        private static readonly Color ColRowOdd   = new(1f,    1f,    1f,    0.03f);
        private static readonly Color ColSection  = new(0.28f, 0.28f, 0.28f, 0.60f);

        // ── styles ────────────────────────────────────────────────────────────
        private static GUIStyle _dotStyle;
        private static GUIStyle _pathStyle;
        private static GUIStyle _typeStyle;
        private static GUIStyle _sectionStyle;

        private static GUIStyle DotStyle(Color c) => (_dotStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize  = 10,
            padding   = new RectOffset(0, 0, 0, 0),
            margin    = new RectOffset(0, 0, 0, 0),
        }).WithColor(c);

        private static GUIStyle PathStyle => _pathStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            normal   = { textColor = new Color(0.6f, 0.6f, 0.6f) },
            clipping = TextClipping.Clip,
        };

        private static GUIStyle TypeStyle => _typeStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
        {
            clipping = TextClipping.Clip,
        };

        private static GUIStyle SectionStyle => _sectionStyle ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(4, 0, 0, 0),
        };

        // ──────────────────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────────────────

        /// <param name="usedBy">Scene components AND SequenceNode assets that reference this asset.</param>
        /// <param name="references">Objects (assets / scene) referenced BY this asset's fields.</param>
        public static void Draw(
            Object target,
            ref List<Object> usedBy,
            ref List<Object> references,
            ref bool    show,
            ref string  filter,
            ref Vector2 scroll)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawHeader(target, ref usedBy, ref references, ref show, ref filter);

            if (show)
            {
                DrawFilterBar(ref filter);
                DrawBody(usedBy, references, ref scroll, filter);
            }

            EditorGUILayout.EndVertical();
        }

        public static void FindAllReferences(Object target, List<Object> usedBy, List<Object> references)
        {
            FindIncoming(target, usedBy);
            FindOutgoing(target, references);
        }

        // ── Header ────────────────────────────────────────────────────────────
        private static void DrawHeader(
            Object target,
            ref List<Object> usedBy, ref List<Object> references,
            ref bool show, ref string filter)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(show ? "▼" : "▶", EditorStyles.toolbarButton, GUILayout.Width(20)))
                show = !show;

            var total = usedBy.Count + references.Count;
            GUILayout.Label(total > 0 ? $"References  ({total})" : "References", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("↺  Scan", EditorStyles.toolbarButton, GUILayout.Width(58)))
            {
                FindAllReferences(target, usedBy, references);
                filter = "";
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Filter bar ────────────────────────────────────────────────────────
        private static void DrawFilterBar(ref string filter)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);
            if (!string.IsNullOrEmpty(filter) &&
                GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)))
                filter = "";
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        // ── Body ──────────────────────────────────────────────────────────────
        private static void DrawBody(
            List<Object> usedBy,
            List<Object> references,
            ref Vector2 scroll,
            string filter)
        {
            var visUsedBy = FilterObjects(usedBy,     filter);
            var visRefs   = FilterObjects(references, filter);
            var total     = visUsedBy.Count + visRefs.Count;

            if (usedBy.Count == 0 && references.Count == 0)
            {
                EditorGUILayout.LabelField("No references found. Click Scan.", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(2);
                return;
            }

            if (total == 0)
            {
                EditorGUILayout.LabelField($"No matches for \"{filter}\"", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(2);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(260));

            int rowIndex = 0;

            if (visUsedBy.Count > 0)
            {
                DrawSectionHeader($"Used by  ({visUsedBy.Count})");
                foreach (var obj in visUsedBy)
                    if (obj != null) DrawRow(obj, rowIndex++, isUsedBy: true);
            }

            if (visRefs.Count > 0)
            {
                if (visUsedBy.Count > 0) GUILayout.Space(3);
                DrawSectionHeader($"References  ({visRefs.Count})");
                foreach (var obj in visRefs)
                    if (obj != null) DrawRow(obj, rowIndex++, isUsedBy: false);
            }

            EditorGUILayout.EndScrollView();

            // Select-all only for scene objects in "Used by"
            var sceneUsedBy = visUsedBy.OfType<Component>().ToList();
            if (sceneUsedBy.Count > 1)
            {
                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button($"Select All Scene ({sceneUsedBy.Count})",
                        EditorStyles.miniButton, GUILayout.Width(160)))
                {
                    Selection.objects = sceneUsedBy.Select(c => (Object)c.gameObject).ToArray();
                }
                GUILayout.Space(2);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        // ── Section header ────────────────────────────────────────────────────
        private static void DrawSectionHeader(string label)
        {
            var r = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(r, ColSection);
            GUI.Label(r, label, SectionStyle);
        }

        // ── Unified row (handles scene Component, ScriptableObject asset) ─────
        private static void DrawRow(Object obj, int index, bool isUsedBy)
        {
            var row = GUILayoutUtility.GetRect(0, RowHeight, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(row, index % 2 == 0 ? ColRowEven : ColRowOdd);

            var x = row.x + 2;

            // ── dot ──────────────────────────────────────────────────────────
            var comp    = obj as Component;
            var gameObj = comp?.gameObject ?? obj as GameObject;
            bool isScene = gameObj != null && gameObj.scene.IsValid() && gameObj.scene.isLoaded;

            Color dotColor;
            string dotChar;
            if (isScene)
            {
                var beh = comp as Behaviour;
                var active = beh == null || (beh.enabled && gameObj.activeInHierarchy);
                dotColor = active ? ColEnabled : ColDisabled;
                dotChar  = "●";
            }
            else
            {
                dotColor = ColAsset;
                dotChar  = "◆";
            }
            GUI.Label(new Rect(x, row.y, DotW, RowHeight), dotChar, DotStyle(dotColor));
            x += DotW;

            // ── → ping button ─────────────────────────────────────────────────
            if (GUI.Button(new Rect(x, row.y + 2, ArrowW - 2, RowHeight - 4), "→", EditorStyles.miniButton))
            {
                Selection.activeObject = isScene ? (Object)gameObj : obj;
                EditorGUIUtility.PingObject(obj);
            }
            x += ArrowW;

            // ── [icon] Type ───────────────────────────────────────────────────
            var tc = EditorGUIUtility.ObjectContent(obj, obj.GetType());
            tc.text = obj.GetType().Name;
            GUI.Label(new Rect(x, row.y, TypeW, RowHeight), tc, TypeStyle);
            x += TypeW;

            // ── label / path ──────────────────────────────────────────────────
            var remaining = row.xMax - x - 2;
            if (remaining > 20)
            {
                string full, display;
                if (isScene)
                {
                    full    = BuildPath(gameObj);
                    display = ShortenPath(full, 40);
                }
                else
                {
                    full = display = obj.name;
                }
                GUI.Label(new Rect(x, row.y, remaining, RowHeight),
                    new GUIContent(display, full), PathStyle);
            }
        }

        // ── Scanning ──────────────────────────────────────────────────────────

        private static void FindIncoming(Object target, List<Object> usedBy)
        {
            usedBy.Clear();

            // 1. Scene MonoBehaviours
            foreach (var mb in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (mb == null || !mb.gameObject.scene.isLoaded) continue;
                if (HasSerializedRef(mb, target)) usedBy.Add(mb);
            }

            // 2. SequenceNode assets loaded in memory (includes sub-assets like Steps)
            //    This catches e.g. a Step in Game.asset that calls Day1.Begin()
            foreach (var node in Resources.FindObjectsOfTypeAll<SequenceNode>())
            {
                if (node == null || node == target) continue;
                if (usedBy.Contains(node)) continue;
                if (HasSerializedRef(node, target)) usedBy.Add(node);
            }

            usedBy.Sort((a, b) => string.Compare(
                DisplayNameFor(a), DisplayNameFor(b),
                System.StringComparison.OrdinalIgnoreCase));
        }

        private static void FindOutgoing(Object target, List<Object> references)
        {
            references.Clear();
            var assetPath  = AssetDatabase.GetAssetPath(target);
            var targetType = target.GetType();

            var so   = new SerializedObject(target);
            var prop = so.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                var refVal = prop.objectReferenceValue;
                if (refVal == null || refVal == target) continue;
                if (refVal is MonoScript) continue;
                if (refVal.GetType() == targetType) continue;

                // Skip sub-assets living in the same file
                var refPath = AssetDatabase.GetAssetPath(refVal);
                if (!string.IsNullOrEmpty(assetPath) && refPath == assetPath) continue;

                if (!references.Contains(refVal))
                    references.Add(refVal);
            }

            references.Sort((a, b) =>
                string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasSerializedRef(Object owner, Object target)
        {
            var so   = new SerializedObject(owner);
            var prop = so.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                    prop.objectReferenceValue == target)
                    return true;
            }
            return false;
        }

        // ── Filters ───────────────────────────────────────────────────────────
        private static List<Object> FilterObjects(List<Object> src, string filter)
        {
            if (string.IsNullOrEmpty(filter)) return src;
            var q = filter.ToLowerInvariant();
            return src.Where(o => o != null &&
                (o.GetType().Name.ToLowerInvariant().Contains(q) ||
                 DisplayNameFor(o).ToLowerInvariant().Contains(q))).ToList();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string DisplayNameFor(Object obj)
        {
            var comp = obj as Component;
            if (comp != null) return BuildPath(comp.gameObject);
            return obj.name;
        }

        private static string BuildPath(GameObject go)
        {
            var s = go.name;
            var t = go.transform.parent;
            while (t != null) { s = t.name + "/" + s; t = t.parent; }
            return s;
        }

        private static string ShortenPath(string path, int max) =>
            path.Length <= max ? path : "…/" + path[(path.Length - max)..];

        private static GUIStyle WithColor(this GUIStyle s, Color c)
        {
            s.normal.textColor = c;
            return s;
        }
    }
}
