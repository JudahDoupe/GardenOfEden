using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelLoader))]
public class LevelLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelLoader loader = (LevelLoader)target;

        if (GUILayout.Button("Save Level"))
        {
            loader.ExportLevel(loader.MapName);
        }
        if (GUILayout.Button("Reload Level"))
        {
            loader.LoadLevel(loader.MapName);
        }
    }
}