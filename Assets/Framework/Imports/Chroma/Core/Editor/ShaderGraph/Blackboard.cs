#if CHROMA_SG || CHROMA_URP || CHROMA_HDRP
#if UNITY_2021_2_OR_NEWER
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chroma {
[InitializeOnLoad]
public class Blackboard : Editor {
    private const double UpdateInterval = 0.5f;
    private static double _lastUpdate;
    private static StyleSheet _styleSheet;

    static Blackboard() {
        EditorApplication.update += EditorUpdate;
    }

    private static void EditorUpdate() {
        if (EditorApplication.timeSinceStartup - _lastUpdate < UpdateInterval) return;
        _lastUpdate = EditorApplication.timeSinceStartup;

        if (!_styleSheet) {
            _styleSheet = Resources.Load<StyleSheet>("Styles/ChromaBlackboard");
        }

        var windows = Resources.FindObjectsOfTypeAll<MaterialGraphEditWindow>();
        foreach (var window in windows) {
            if (!window.hasFocus) continue;
            if (window.graphEditorView?.blackboardController == null) continue;

            var blackboardController = window.graphEditorView.blackboardController;
            var blackboardElements = blackboardController.blackboard.Query<SGBlackboardField>().Visible().ToList();

            int foldoutCount = 0;

            foreach (var element in blackboardElements) {
                var countInFoldouts = true;
                element.ClearClassList();

                if (ChromaPropertyDrawer.HasAnyAttributes(element.text)) {
                    element.AddToClassList("chroma");
                }

                if (ChromaPropertyDrawer.IsPureAttribute(element.text)) {
                    element.AddToClassList("chroma-pure-attribute");
                    countInFoldouts = false;
                }

                // Tabs.
                var indent = ChromaPropertyDrawer.NumTabs(element.text);

                // Headers.
                if (ChromaPropertyDrawer.HasHeaderAttribute(element.text)) {
                    element.typeText = "Header";
                    element.AddToClassList("chroma-header");
                    element.AddToClassList("chroma-pure-attribute");
                }

                // Foldouts.
                if (ChromaPropertyDrawer.HasFoldoutAttribute(element.text)) {
                    element.typeText = "Foldout";
                    element.AddToClassList("chroma-pure-attribute");
                    foldoutCount = ChromaPropertyDrawer.GetFoldoutCount(element.text);
                    countInFoldouts = false;
                    --indent;
                }

                // Spaces.
                if (ChromaPropertyDrawer.HasSpaceAttribute(element.text)) {
                    element.AddToClassList("chroma-space");
                }

                // Lines.
                if (ChromaPropertyDrawer.HasLineAttribute(element.text)) {
                    element.AddToClassList("chroma-space");
                }

                // Tooltips.
                if (ChromaPropertyDrawer.HasTooltipAttribute(element.text)) {
                    element.AddToClassList("chroma-pure-attribute");
                    countInFoldouts = false;
                    if (foldoutCount == 0) {
                        ++indent;
                    }
                }

                // Notes.
                if (ChromaPropertyDrawer.HasNoteAttribute(element.text)) {
                    element.AddToClassList("chroma-pure-attribute");
                }

                if (foldoutCount > 0) {
                    ++indent;
                }

                if (countInFoldouts) {
                    --foldoutCount;
                }

                if (indent > 0) {
                    element.AddToClassList($"chroma-tab-{indent}");
                }

                if (_styleSheet && !element.styleSheets.Contains(_styleSheet)) {
                    element.styleSheets.Add(_styleSheet);
                }

                // The following attributes are only applicable to properties of specific types.
                var property = window.graphObject.graph.properties.FirstOrDefault(p => p.displayName == element.text);
                if (property == null) {
                    continue;
                }

                // Gradients.
                if (HasGradientAttribute(property)) {
                    element.typeText = "Chroma Gradient";
                    element.AddToClassList("chroma-specific");
                }

                // Curves.
                if (HasCurveAttribute(property)) {
                    element.typeText = "Chroma Curve";
                    element.AddToClassList("chroma-specific");
                }

                // MinMax.
                if (HasMinMaxAttribute(property)) {
                    element.typeText = "Min/Max Range";
                    element.AddToClassList("chroma-specific");
                }
            }
        }
    }

    private static bool HasGradientAttribute(AbstractShaderProperty property) {
        var s = property.displayName.ToLower();
        return property.propertyType == PropertyType.Texture2D &&
               ChromaPropertyDrawer.HasGradientSubstring(property.displayName);
    }

    private static bool HasCurveAttribute(AbstractShaderProperty property) {
        var s = property.displayName.ToLower();
        return property.propertyType == PropertyType.Texture2D &&
               ChromaPropertyDrawer.HasCurveSubstring(property.displayName);
    }

    private static bool HasMinMaxAttribute(AbstractShaderProperty property) {
        var s = property.displayName.ToLower();
        return property.propertyType == PropertyType.Vector2 &&
               ChromaPropertyDrawer.HasMinMaxSubstring(property.displayName);
    }
}
}
#endif
#endif