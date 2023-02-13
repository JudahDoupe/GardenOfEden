using UnityEngine;

namespace UnityEditor {
public static class ContextMenuItems {
    [MenuItem("Assets/Remove All Child Assets")]
    public static void RemoveAllChildAssets() {
        foreach (var asset in Selection.GetFiltered<Object>(SelectionMode.Assets)) {
            var path = AssetDatabase.GetAssetPath(asset);
            foreach (var childAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path)) {
                Object.DestroyImmediate(childAsset, true);
            }

            AssetDatabase.ImportAsset(path);
        }
    }
}
}