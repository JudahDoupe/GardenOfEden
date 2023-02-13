using UnityEditor;
using UnityEngine;

namespace Chroma {
[InitializeOnLoad]
public class InitializeAsset : MonoBehaviour {
    private const string ReadmeUuid = "aa5c68210392dc8438f3032ad8867b9f";

    static InitializeAsset() {
        const string chromaReadmeSelected = "Chroma.Readme.Selected";
        if (EditorPrefs.GetBool(chromaReadmeSelected, false)) {
            return;
        }

        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(ReadmeUuid), typeof(Object));
        EditorPrefs.SetBool(chromaReadmeSelected, true);
    }
}
}