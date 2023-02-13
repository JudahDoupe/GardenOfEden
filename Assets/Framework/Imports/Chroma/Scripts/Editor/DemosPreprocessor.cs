using Chroma;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

[InitializeOnLoad]
public class DemosPreprocessor : Editor {
    private const string DemosFileGuid = "5ee6cbd68276ebe45b833b94a0e9fb11";
    private const string ChromaDemosUrpImported = "Chroma.Demos.URPImported";

    static DemosPreprocessor() {
        if (EditorPrefs.GetBool(ChromaDemosUrpImported, false)) return;
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged() {
        var selections = Selection.assetGUIDs;
        if (selections.Length != 1) {
            return;
        }

        var guid = selections[0];
        if (guid != DemosFileGuid) return;

#if DUSTYROOM_DEV
        Log.M("Chroma demos package selected");
#endif

        if (EditorPrefs.GetBool(ChromaDemosUrpImported, false)) return;

        // Check if URP is imported.
        if (PackageInfo.FindForAssetPath("Packages/com.unity.render-pipelines.universal") == null) {
            // Show a dialog to the user.
            if (EditorUtility.DisplayDialog("Chroma Demos",
                                            "Chroma Demos require the Universal Render Pipeline. Would you like to import it now?",
                                            "Yes", "No")) {
                // Import URP.
                Client.Add("com.unity.render-pipelines.universal");
                // Do not show the dialog again.
            }

            EditorPrefs.SetBool(ChromaDemosUrpImported, true);
        }
    }
}