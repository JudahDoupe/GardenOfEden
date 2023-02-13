using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chroma {
public class Vector2Drawer : MaterialPropertyDrawer {
    private Vector2 _value;

    public Vector2Drawer() {
        _value = new Vector2(0, 0);
    }

    public Vector2Drawer(Vector2 value) {
        _value = value;
    }

    private static bool IsPropertyTypeSuitable(MaterialProperty prop) {
        return prop.type == MaterialProperty.PropType.Vector;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        OnGUI(position, prop, label, editor, string.Empty);
    }

    public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor, string tooltip) {
        var guiContent = new GUIContent(label, tooltip);
        OnGUI(position, prop, guiContent, editor);
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
        if (!IsPropertyTypeSuitable(prop)) {
            EditorGUI.HelpBox(position, $"[Vector2] used on non-vector property \"{prop.name}\"", MessageType.Error);
            return;
        }

        using var changeScope = new EditorGUI.ChangeCheckScope();
        EditorGUILayout.Space(-15);

        _value = EditorGUILayout.Vector2Field(label, prop.vectorValue);
        if (changeScope.changed) {
            foreach (Object target in prop.targets) {
                if (!AssetDatabase.Contains(target)) {
                    // Failsafe for non-asset materials - should never trigger.
                    continue;
                }

                Undo.RecordObject(target, "Change Material Vector2");
                var material = (Material)target;
                material.SetVector(prop.name, _value);
                EditorUtility.SetDirty(material);
            }
        }
    }
}
}