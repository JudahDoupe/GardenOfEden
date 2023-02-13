using UnityEngine;

namespace Chroma {
public class Log : MonoBehaviour {
    public static void M(string s) {
        // Create a prefix with each letter in a different color.
        string name = "Chroma";
        var prefix = "";
        for (int i = 0; i < name.Length; i++) {
            var hsv = Color.HSVToRGB(Mathf.Lerp(0.5f, 0.3f, Mathf.InverseLerp(0, name.Length, i)), 0.85f, 1);
            var color = ColorUtility.ToHtmlStringRGB(hsv);
            prefix += $"<color=#{color}>{name[i]}</color>";
        }

        Debug.Log($"<b>[{prefix}]</b> {s}");
    }
}
}