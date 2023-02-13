using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chroma {
public class MinMaxDrawer : MaterialPropertyDrawer {
    private Vector2 _value;
    private readonly Vector2 _range;

    public MinMaxDrawer() {
        _value = new Vector2(0, 1);
        _range = new Vector2(0, 1);
    }

    public MinMaxDrawer(Vector2 value, Vector2 range) {
        _value = value;
        _range = range;
    }

    private static bool IsPropertyTypeSuitable(MaterialProperty prop) {
        return prop.type == MaterialProperty.PropType.Vector;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        var guiContent = new GUIContent(label);
        OnGUI(position, prop, guiContent, editor);
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
        if (!IsPropertyTypeSuitable(prop)) {
            EditorGUI.HelpBox(position, $"[MinMax] used on non-vector property \"{prop.name}\"", MessageType.Error);
            return;
        }

        using var changeScope = new EditorGUI.ChangeCheckScope();
        EditorGUILayout.Space(-18);

        _value = prop.vectorValue;

        if (!string.IsNullOrEmpty(label.tooltip)) {
            label.tooltip += "\n";
        }

        label.tooltip += $"Range: {_range.x}, {_range.y}\nValue: {_value.x}, {_value.y}";

        EditorGUILayout.MinMaxSlider(label, ref _value.x, ref _value.y, _range.x, _range.y);

        if (changeScope.changed) {
            foreach (Object target in prop.targets) {
                if (!AssetDatabase.Contains(target)) {
                    // Failsafe for non-asset materials - should never trigger.
                    continue;
                }

                Undo.RecordObject(target, "Change Material MinMax");
                var material = (Material)target;
                material.SetVector(prop.name, _value);
                EditorUtility.SetDirty(material);
            }
        }
    }
}
}