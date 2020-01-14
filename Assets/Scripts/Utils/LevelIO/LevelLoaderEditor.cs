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
            loader.SaveLevel(loader.MapName);
        }
        if (GUILayout.Button("Load Level From File"))
        {
            loader.LoadLevel(loader.MapName);
        }
        if (GUILayout.Button("Load Level From Mesh"))
        {
            loader.RenderMaps();
        }
        if (GUILayout.Button("Disable Mesh"))
        {
            loader.SetRenderersEnabled(false);
        }
    }
}