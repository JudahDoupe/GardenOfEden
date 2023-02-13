using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace Chroma {
[CustomEditor(typeof(Readme))]
public class ReadmeEditor : Editor {
    private static readonly string AssetName = "Chroma";

    private static readonly string DemosFolderGuid = "7d1d8fe65961842038b7176ee43397ba";
    private static readonly string DemosAsmdefGuid = "ab1464285151b4556a0a92d71dee6445";
    private static readonly string DemosFileName = "Demos (URP)";

    private Readme _readme;
    private bool _showingVersionMessage;
    private string _versionLatest;

    private bool _showingClearCacheMessage;
    private bool _cacheClearedSuccessfully;

    private bool _urpInstalled;

    private Gradient _gradient;

    private void OnEnable() {
        _readme = serializedObject.targetObject as Readme;
        if (_readme == null) {
            Debug.LogError($"[{AssetName}] Readme error.");
            return;
        }

        _readme.Refresh();
        _showingVersionMessage = false;
        _showingClearCacheMessage = false;
        _versionLatest = null;

        AssetDatabase.importPackageStarted += OnImportPackageStarted;
        AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        AssetDatabase.importPackageFailed += OnImportPackageFailed;
        AssetDatabase.importPackageCancelled += OnImportPackageCancelled;

        _urpInstalled = false;
        var listRequest = Client.List(true);
        EditorApplication.delayCall += () => {
            while (!listRequest.IsCompleted) { }

            if (listRequest.Status == StatusCode.Success) {
                foreach (var package in listRequest.Result) {
                    if (package.name == "com.unity.render-pipelines.universal") {
                        _urpInstalled = true;
                        break;
                    }
                }
            }
        };

        _gradient = new Gradient();
        _gradient.SetKeys(_readme.lineGradient.colorKeys, _readme.lineGradient.alphaKeys);
        _gradient.mode = _readme.lineGradient.mode;

        EditorApplication.update += Update;
    }

    private void Update() {
        // Move gradient keys slowly.
        var keys = _gradient.colorKeys;
        for (var i = 0; i < keys.Length; i++) {
            var key = keys[i];
            var originalTime = _readme.lineGradient.colorKeys[i].time;
            key.time = originalTime + Mathf.Sin(Time.time * 0.25f + i * 0.8f) * 0.2f;
            keys[i] = key;
        }

        _gradient.SetKeys(keys, _gradient.alphaKeys);

        EditorUtility.SetDirty(_readme);
    }

    private void OnDisable() {
        AssetDatabase.importPackageStarted -= OnImportPackageStarted;
        AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
        AssetDatabase.importPackageFailed -= OnImportPackageFailed;
        AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI() {
        {
            EditorGUILayout.Space();
            DrawGradient(_gradient, 5);
            var style = new GUIStyle(GUI.skin.label) {
                fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, richText = true
            };
            EditorGUILayout.LabelField($"<color=white>{AssetName}</color>", style);
            EditorGUILayout.LabelField($"Version {_readme.AssetVersion}", EditorStyles.miniLabel);
            EditorGUILayout.Separator();
        }

        if (GUILayout.Button("Documentation")) {
            OpenDocumentation();
        }

        if (GUILayout.Button("Asset Store page")) {
            OpenAssetStore();
        }

        {
            if (_showingVersionMessage) {
                EditorGUILayout.Space(20);

                if (_versionLatest == null) {
                    EditorGUILayout.HelpBox($"Checking the latest version...", MessageType.None);
                } else {
                    var local = Version.Parse(_readme.AssetVersion);
                    var remote = Version.Parse(_versionLatest);
                    if (local >= remote) {
                        EditorGUILayout.HelpBox($"You have the latest version! {_readme.AssetVersion}.",
                                                MessageType.Info);
                    } else {
                        var message = $"Update needed. The latest version is {_versionLatest}, but you have " +
                                      $"{_readme.AssetVersion}.";
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    }
                }
            }

            if (GUILayout.Button("Check for updates")) {
                _showingVersionMessage = true;
                _versionLatest = null;
                CheckVersion();
            }

            if (_showingVersionMessage) {
                EditorGUILayout.Space(20);
            }
        }

        {
            if (!string.IsNullOrEmpty(_readme.PackageManagerError)) {
                EditorGUILayout.Separator();
                HorizontalLine.Draw(Color.yellow, 1, 0);
                EditorGUILayout.HelpBox($"Package Manager error: {_readme.PackageManagerError}", MessageType.Warning);
                HorizontalLine.Draw(Color.yellow, 1, 0);
            }
        }

        {
            HorizontalLine.Draw(Color.gray, 1, 20);
            EditorGUILayout.LabelField("Demos", EditorStyles.boldLabel);

            // Show a warning if URP is not installed.
            if (!_urpInstalled) {
                var message =
                    $"{AssetName} demos require Universal Render Pipeline. Please install it from the Package Manager.";
                EditorGUILayout.HelpBox(message, MessageType.Info);

                if (GUILayout.Button("Open Package Manager")) {
                    UnityEditor.PackageManager.UI.Window.Open("com.unity.render-pipelines.universal");
                }
            }

            // Only enable buttons if URP is installed.
            EditorGUI.BeginDisabledGroup(!_urpInstalled);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import demos")) {
                ExtractDemos();
            }

            if (GUILayout.Button("Remove demo files")) {
                DeleteDemos();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        {
            HorizontalLine.Draw(Color.gray, 1, 20);
            EditorGUILayout.LabelField("Support", EditorStyles.boldLabel);

            if (GUILayout.Button("Open support ticket on GitHub")) {
                OpenSupportTicketGitHub();
            }

            EditorGUILayout.LabelField("Please copy the debug info below and paste it in the ticket.",
                                       EditorStyles.miniLabel);
        }

        {
            HorizontalLine.Draw(Color.gray, 1, 20);
            EditorGUILayout.LabelField("Package Manager", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear cache")) {
                ClearPackageCache();
            }

            if (GUILayout.Button($"Select {AssetName}")) {
                OpenPackageManager();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button($"Reimport {AssetName} files")) {
                ReimportAsset();
            }

            if (_showingClearCacheMessage) {
                if (_cacheClearedSuccessfully) {
                    var message = $"Successfully removed cached packages. \nPlease re-download {AssetName} in the " +
                                  $"Package Manager.";
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                } else {
                    EditorGUILayout.HelpBox($"Could not find or clear package cache. It might be already cleared.",
                                            MessageType.Warning);
                }
            }
        }

        DrawColorSpaceCheck();

        {
            HorizontalLine.Draw(Color.gray, 1, 20);
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Debug info", EditorStyles.miniBoldLabel);

            GUILayout.BeginVertical();
            if (GUILayout.Button("Copy", EditorStyles.miniButtonLeft)) {
                CopyDebugInfoToClipboard();
            }

            if (EditorGUIUtility.systemCopyBuffer == GetDebugInfoString()) {
                EditorGUILayout.LabelField("Copied!", EditorStyles.miniLabel);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            var debugInfo = GetDebugInfo();
            foreach (var s in debugInfo) {
                EditorGUILayout.LabelField($"    " + s, EditorStyles.miniLabel);
            }

            EditorGUILayout.Separator();
        }
    }

    private void OnImportPackageStarted(string packageName) { }

    private void OnImportPackageCompleted(string packageName) {
        _readme.Refresh();
        Repaint();
        EditorUtility.SetDirty(this);
    }

    private void OnImportPackageFailed(string packageName, string errorMessage) {
        Debug.LogError($"<b>[{AssetName}]</b> Failed to unpack {packageName}: {errorMessage}.");
    }

    private void OnImportPackageCancelled(string packageName) {
        Debug.LogError($"<b>[{AssetName}]</b> Cancelled unpacking {packageName}.");
    }

    private string[] GetDebugInfo() {
        var info = new List<string> {
            $"{AssetName} version {_readme.AssetVersion}",
            $"Unity {_readme.UnityVersion}",
            $"Dev platform: {Application.platform}",
            $"Target platform: {EditorUserBuildSettings.activeBuildTarget}",
            $"Render pipeline: {Shader.globalRenderPipeline}",
            $"Color space: {PlayerSettings.colorSpace}"
        };

        var qualityConfig = QualitySettings.renderPipeline == null ? "N/A" : QualitySettings.renderPipeline.name;
        info.Add($"Quality config: {qualityConfig}");

        var graphicsConfig = GraphicsSettings.currentRenderPipeline == null
            ? "N/A"
            : GraphicsSettings.currentRenderPipeline.name;
        info.Add($"Graphics config: {graphicsConfig}");

        return info.ToArray();
    }

    private string GetDebugInfoString() {
        string[] info = GetDebugInfo();
        return String.Join("\n", info);
    }

    private void CopyDebugInfoToClipboard() {
        EditorGUIUtility.systemCopyBuffer = GetDebugInfoString();
    }

    private void OpenPackageManager() {
        const string packageName = "Chroma: Creative Shader Tools";
        UnityEditor.PackageManager.UI.Window.Open(packageName);
    }

    private void ReimportAsset() {
        const string rootGuid = "95c5978d7490c4e8f9e3f7439049c19b";
        var assetRoot = AssetDatabase.GUIDToAssetPath(rootGuid);
        if (string.IsNullOrEmpty(assetRoot)) {
            var message = "Could not find the root asset folder. Please re-import from the Package Manager.";
            EditorUtility.DisplayDialog(AssetName, message, "OK");
        } else {
            AssetDatabase.ImportAsset(assetRoot, ImportAssetOptions.ImportRecursive);
            EditorUtility.DisplayDialog(AssetName, "Successfully re-imported the root asset folder.", "OK");
        }
    }

    private void ClearPackageCache() {
        string path = string.Empty;
        // TODO: Use UPM_CACHE_ROOT.
        if (Application.platform == RuntimePlatform.OSXEditor) {
            path = "~/Library/Unity/Asset Store-5.x/Dustyroom/";
        }

        if (Application.platform == RuntimePlatform.LinuxEditor) {
            path = "~/.local/share/unity3d/Asset Store-5.x/Dustyroom/";
        }

        if (Application.platform == RuntimePlatform.WindowsEditor) {
            // This wouldn't understand %APPDATA%.
            path =
                Application.persistentDataPath
                    .Substring(0, Application.persistentDataPath.IndexOf("AppData", StringComparison.Ordinal)) +
                "/AppData/Roaming/Unity/Asset Store-5.x/Dustyroom";
        }

        if (path == string.Empty) return;

        _cacheClearedSuccessfully |= FileUtil.DeleteFileOrDirectory(path);
        _showingClearCacheMessage = true;

        OpenPackageManager();
    }

    private void CheckVersion() {
        NetworkManager.GetVersion(version => { _versionLatest = version; });
    }

    private void OpenSupportTicketGitHub() {
        Application.OpenURL("https://github.com/Dustyroom/chroma-doc/issues/new/choose");
    }

    private void OpenDocumentation() {
        Application.OpenURL("https://chroma.dustyroom.com/");
    }

    private void OpenAssetStore() {
        Application.OpenURL("https://u3d.as/");
    }

    private void DrawColorSpaceCheck() {
        if (PlayerSettings.colorSpace != ColorSpace.Linear) {
            HorizontalLine.Draw(Color.gray, 1, 20);
            var message = $"{AssetName} demo scenes were created for the Linear color space, but your project " +
                          $"is using {PlayerSettings.colorSpace}.\nThis may result in the demo scenes appearing " +
                          $"slightly different compared to the Asset Store screenshots.\nOptionally, you may switch " +
                          $"the color space using the button below.";
            EditorGUILayout.HelpBox(message, MessageType.Warning);

            if (GUILayout.Button("Switch player settings to Linear color space")) {
                PlayerSettings.colorSpace = ColorSpace.Linear;
            }
        }
    }

    public static void DeleteDemos() {
        var pathDemos = AssetDatabase.GUIDToAssetPath(DemosFolderGuid);
        var files = AssetDatabase.FindAssets("t:Folder", new[] { pathDemos });
        foreach (var file in files) {
            var filePath = AssetDatabase.GUIDToAssetPath(file);
            AssetDatabase.DeleteAsset(filePath);
        }

        var pathAsmdef = AssetDatabase.GUIDToAssetPath(DemosAsmdefGuid);
        AssetDatabase.DeleteAsset(pathAsmdef);
    }

    public static void ExtractDemos() {
        var path = AssetDatabase.GUIDToAssetPath(DemosFolderGuid);
        AssetDatabase.ImportPackage($"{path}/{DemosFileName}.unitypackage", false);
    }

    private static void DrawGradient(Gradient color, float padding) {
        EditorGUILayout.Space(-30);
        float thickness = 25;
        EditorGUILayout.BeginHorizontal();
        var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        float steps = r.width / 2f;
        r.y += 25;
        for (float i = 0; i < steps; i++) {
            var segment = new Rect(r.x + (r.width / steps) * i, r.y + padding / 2f, r.width / steps, thickness);
            EditorGUI.DrawRect(segment, color.Evaluate(i / steps));
        }

        EditorGUILayout.EndHorizontal();
    }
}
}