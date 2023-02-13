using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chroma {
public class CurveDrawer : MaterialPropertyDrawer {
    private readonly int _resolution = 256;

    public CurveDrawer() { }

    public CurveDrawer(float res) {
        _resolution = (int)res;
    }

    private static string TextureName(MaterialProperty prop) {
        return $"z_{prop.name}Tex";
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
        var guiContent = new GUIContent(label);
        OnGUI(position, prop, guiContent, editor);
    }

    public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor, string tooltip) {
        var guiContent = new GUIContent(label, tooltip);
        OnGUI(position, prop, guiContent, editor);
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
        if (prop.type != MaterialProperty.PropType.Texture) {
            EditorGUI.HelpBox(position, $"[Curve] used on property {prop.name} of type {prop.type}.",
                              MessageType.Error);
            return;
        }

        if (!AssetDatabase.Contains(prop.targets.FirstOrDefault())) {
            EditorGUI.HelpBox(position, $"Material {prop.targets.FirstOrDefault()?.name} is not an Asset.",
                              MessageType.Error);
            return;
        }

        var textureName = TextureName(prop);

        AnimationCurve currentCurve = null;
        if (prop.targets.Length == 1) {
            var target = (Material)prop.targets[0];
            var path = AssetDatabase.GetAssetPath(target);
            var textureAsset = DrawerUtils.LoadSubAsset(path, textureName);
            if (textureAsset != null) {
                currentCurve = Deserialize(prop, textureAsset.name);
            }

            var materialReset = target.GetTexture(prop.name) == null;
            if (currentCurve == null || materialReset) {
                // Create the default curve.
                currentCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            }

            EditorGUI.showMixedValue = false;
        } else {
            EditorGUI.showMixedValue = true;
        }

        using (var changeScope = new EditorGUI.ChangeCheckScope()) {
            EditorGUILayout.Space(-18);
            EditorGUILayout.BeginHorizontal();

            // Using different offsets for SG and text shaders since `EditorGUILayout.GradientField` does not expand to
            // the full width for text shaders.
            var material = (Material)prop.targets[0];
            var guiContent = new GUIContent(label.text, label.tooltip);
            if (HasShaderGraphTag.Check(material)) {
                // Shader graph.
                currentCurve = EditorGUILayout.CurveField(guiContent, currentCurve);
            } else {
                // Text shader.
                EditorGUILayout.LabelField(guiContent, EditorStyles.label,
                                           GUILayout.Width(EditorGUIUtility.labelWidth));
                var options = new[] { GUILayout.MinWidth(0) };
                var rect = EditorGUILayout.GetControlRect(true, 18, EditorStyles.colorField, options);
                rect.xMin = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth + rect.width;
                rect.xMin = Mathf.Min(rect.xMin, EditorGUIUtility.labelWidth + 32);
                currentCurve = EditorGUI.CurveField(rect, currentCurve);
            }

            if (changeScope.changed) {
                HandleCurveUpdated(prop, textureName, currentCurve);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.showMixedValue = false;
    }

    private void HandleCurveUpdated(MaterialProperty prop, string textureName, AnimationCurve currentCurve) {
        string encodedCurve = Serialize(currentCurve);
        string fullAssetName = textureName + encodedCurve;
        foreach (Object target in prop.targets) {
            if (!AssetDatabase.Contains(target)) {
                continue;
            }

            var path = AssetDatabase.GetAssetPath(target);
            var textureAsset = GetTexture(path, textureName, FilterMode.Bilinear);
            Undo.RecordObject(textureAsset, "Change Material Curve");
            textureAsset.name = fullAssetName;
            Bake(currentCurve, textureAsset);
            EditorUtility.SetDirty(textureAsset);

            var material = (Material)target;
            material.SetTexture(prop.name, textureAsset);
        }
    }

    private Texture2D GetTexture(string path, string name, FilterMode filterMode) {
        var textureAsset = DrawerUtils.LoadSubAsset(path, name);

        if (textureAsset == null) textureAsset = CreateTexture(path, name, filterMode);

        // Force set filter mode for legacy materials.
        textureAsset.filterMode = filterMode;

        if (textureAsset.width != _resolution) {
#if UNITY_2021_2_OR_NEWER
            textureAsset.Reinitialize(_resolution, 1);
#else
            textureAsset.Resize(_resolution, 1);
#endif
        }

        return textureAsset;
    }

    private Texture2D CreateTexture(string path, string name, FilterMode filterMode) {
        var textureAsset = new Texture2D(_resolution, 1, TextureFormat.RHalf, false) {
            name = name, wrapMode = TextureWrapMode.Clamp, filterMode = filterMode
        };
        AssetDatabase.AddObjectToAsset(textureAsset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(path);
        return textureAsset;
    }

    private static string Serialize(AnimationCurve curve) {
        return curve == null ? null : JsonUtility.ToJson(new CurveData(curve));
    }

    private static AnimationCurve Deserialize(MaterialProperty prop, string name) {
        if (prop == null) return null;

        var json = name.Substring(TextureName(prop).Length);
        try {
            var curveRepresentation = JsonUtility.FromJson<CurveData>(json);
            return curveRepresentation?.ToCurve();
        }
        catch (Exception e) {
            Log.M($"Bypass decoding a curve. Debug info: {json} - {e}");
            return null;
        }
    }

    private void Bake(AnimationCurve curve, Texture2D texture) {
        if (curve == null) return;
        for (int x = 0; x < texture.width; x++) {
            var value = curve.Evaluate((float)x / (texture.width - 1));
            var color = new Color(value, 0, 0, 1);
            for (int y = 0; y < texture.height; y++) {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
    }

    [Serializable]
    private class CurveData {
        public Key[] keys;

        public CurveData() { }

        public CurveData(AnimationCurve source) {
            FromCurve(source);
        }

        public void FromCurve(AnimationCurve source) {
            keys = source.keys.Select(key => new Key(key)).ToArray();
        }

        public void ToCurve(AnimationCurve curve) {
            curve.keys = keys.Select(key => key.ToCurveKey()).ToArray();
        }

        public AnimationCurve ToCurve() {
            var curve = new AnimationCurve();
            ToCurve(curve);
            return curve;
        }

        [Serializable]
        public struct Key {
            public float time;
            public float value;
            public float inTangent;
            public float outTangent;
            public float inWeight;
            public float outWeight;
            public WeightedMode weightedMode;

            public Key(Keyframe source) {
                time = default;
                value = default;
                inTangent = default;
                outTangent = default;
                inWeight = default;
                outWeight = default;
                weightedMode = default;
                FromCurveKey(source);
            }

            public void FromCurveKey(Keyframe source) {
                time = source.time;
                value = source.value;
                inTangent = source.inTangent;
                outTangent = source.outTangent;
                inWeight = source.inWeight;
                outWeight = source.outWeight;
                weightedMode = source.weightedMode;
            }

            public Keyframe ToCurveKey() {
                Keyframe key = default;
                key.time = time;
                key.value = value;
                key.inTangent = inTangent;
                key.outTangent = outTangent;
                key.inWeight = inWeight;
                key.outWeight = outWeight;
                key.weightedMode = weightedMode;
                return key;
            }
        }
    }
}
}