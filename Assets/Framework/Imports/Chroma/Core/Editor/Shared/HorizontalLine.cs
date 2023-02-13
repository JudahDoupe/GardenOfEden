using UnityEditor;
using UnityEngine;

namespace Chroma {
public static class HorizontalLine {
    public static void Draw(Color color, float thickness = 2, float padding = 10, float hOffset = 0) {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.x -= -2 - hOffset;
        r.y += padding / 2f;
        r.width -= hOffset + 2;
        r.height = thickness;
        EditorGUI.DrawRect(r, color);
    }
}
}