using System;
using UnityEditor;
using UnityEngine;

namespace Chroma {
public sealed class GradientSetWindow : EditorWindow {
    public event Action<Gradient> OnGradientSelected;

    private const int NumGradients = 10;
    private readonly Gradient[] _gradients = new Gradient[NumGradients];
    private GradientMode _mode = GradientMode.Blend;

    private void OnEnable() {
        titleContent = new GUIContent("Random Gradients");
        minSize = new Vector2(200, 240);

        for (int i = 0; i < NumGradients; i++) {
            _gradients[i] = Palettes.GetRandom();
        }
    }

    private void OnOnGradientSelected(Gradient obj) {
        OnGradientSelected?.Invoke(obj);
    }

    private void OnGUI() {
        _mode = (GradientMode)EditorGUILayout.EnumPopup("Gradient Mode", _mode);

        for (int i = 0; i < NumGradients; i++) {
            EditorGUILayout.BeginHorizontal();
            var g = _gradients[i];
            g.mode = _mode;
            GeneratorUtils.DistributeEvenly(ref g);
            EditorGUILayout.GradientField(g);
            if (GUILayout.Button("Select", GUILayout.Width(60))) {
                OnOnGradientSelected(g);
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Randomize")) {
            for (int i = 0; i < NumGradients; i++) {
                _gradients[i] = Palettes.GetRandom();
            }
        }
    }
}
}